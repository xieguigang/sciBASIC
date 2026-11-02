Imports Microsoft.VisualBasic.DeepLearning.GNN
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' Zachary 空手道俱乐部（Karate Club）经典小图：34 个节点、78 条无向边、2 个派系。
''' </summary>
''' <remarks>
''' <para>
''' 这张图是半监督节点分类的“hello world”：GCN 论文用的就是它（以及 Citeseer/PubMed）。
''' 它适合作为回归用例的原因有三：
''' </para>
''' <list type="number">
'''   <item>拓扑固定、无外部依赖 —— 边表直接内嵌，不需要下载数据集；</item>
'''   <item>两个派系结构明显（边同质性约 0.85），训练几十个 epoch 就能把准确率拉起来；</item>
'''   <item>节点数只有 34，走稠密路径（34³ 远低于 fp32 GEMM 阈值），
'''         顺带验证“小图不降精度”的判断是对的。</item>
''' </list>
''' <para>
''' 特征用单位阵（每个节点 one-hot），与 GCN 原文设置一致：这份特征本身不含任何
''' 可泛化的信息（每个节点都是独一无二的），所以准确率完全来自**结构**的传播 ——
''' 这正是我们想验证的东西。
''' </para>
''' <para>
''' 标签用 Zachary 记录的实际派系（Mr. Hi 18 人 / Officer 16 人）。注意它**不是**
''' “前 17 / 后 17” 的简单切分：编号 9、15、16、19、21 属于 Officer 一方，
''' 而 17、18、20、22 属于 Mr. Hi 一方。用错误划分会让这个用例的上限只剩 80% 左右，
''' 从而把“模型没学好”和“标签不对”混为一谈。
''' </para>
''' </remarks>
Public Module KarateClub

    ''' <summary>节点数</summary>
    Public Const Nodes As Integer = 34

    ''' <summary>类别数（Mr. Hi / Officer）</summary>
    Public Const Classes As Integer = 2

    ''' <summary>
    ''' 78 条无向边，按 1-based 节点编号线性排列（每两个元素一条边）。
    ''' </summary>
    Private ReadOnly _Edges As Integer() = {
        2, 1, 3, 1, 3, 2, 4, 1, 4, 2, 4, 3, 5, 1, 6, 1, 7, 1, 7, 5, 7, 6,
        8, 1, 8, 2, 8, 3, 8, 4, 9, 1, 9, 3, 10, 3, 11, 1, 11, 5, 11, 6,
        12, 1, 13, 1, 13, 4, 14, 1, 14, 2, 14, 3, 14, 4, 17, 6, 17, 7,
        18, 1, 18, 2, 20, 1, 20, 2, 22, 1, 22, 2, 26, 24, 26, 25, 28, 3,
        28, 24, 28, 25, 29, 3, 30, 24, 30, 27, 31, 2, 31, 9, 32, 1, 32, 25,
        32, 26, 32, 29, 33, 3, 33, 9, 33, 15, 33, 16, 33, 19, 33, 21, 33, 23,
        33, 24, 33, 30, 33, 31, 33, 32, 34, 9, 34, 10, 34, 14, 34, 15, 34, 16,
        34, 19, 34, 20, 34, 21, 34, 23, 34, 24, 34, 27, 34, 28, 34, 29, 34, 30,
        34, 31, 34, 32, 34, 33}

    ''' <summary>
    ''' Zachary 记录的实际派系（0 = Mr. Hi，1 = Officer），按 0-based 节点序号排列。
    ''' </summary>
    Private ReadOnly _Faction As Integer() = {
        0, 0, 0, 0, 0, 0, 0, 0,
        1, 1,
        0, 0, 0, 0,
        1, 1,
        0, 0,
        1,
        0,
        1,
        0,
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}

    ''' <summary>
    ''' 构建样本。
    ''' </summary>
    ''' <param name="labelsPerClass">
    ''' 每个派系用于训练的标签节点数。默认 2 —— 34 个节点里只有 4 个有标签（约 12%），
    ''' 正是“半监督”的含义：剩下 88% 的节点靠结构传播被顺带分类。
    ''' </param>
    ''' <param name="validationPerClass">每个派系用于验证的节点数</param>
    Public Function Create(Optional labelsPerClass As Integer = 2,
                           Optional validationPerClass As Integer = 3) As GraphSample

        Dim features = CreateIdentity(Nodes)
        Dim graph As New Graph(Nodes, Nodes)

        For i = 0 To _Edges.Length - 1 Step 2
            Call graph.AddUndirectedEdge(_Edges(i) - 1, _Edges(i + 1) - 1)
        Next

        Dim labels = CType(_Faction.Clone(), Integer())

        Dim trainMask(Nodes - 1) As Boolean
        Dim valMask(Nodes - 1) As Boolean
        Dim testMask(Nodes - 1) As Boolean

        For c = 0 To Classes - 1
            Dim members As New List(Of Integer)()

            For i = 0 To Nodes - 1
                If labels(i) = c Then Call members.Add(i)
            Next

            For k = 0 To labelsPerClass - 1
                trainMask(members(k)) = True
            Next

            For k = labelsPerClass To labelsPerClass + validationPerClass - 1
                valMask(members(k)) = True
            Next
        Next

        For i = 0 To Nodes - 1
            testMask(i) = Not trainMask(i) AndAlso Not valMask(i)
        Next

        Return New GraphSample With {
            .Name = "Karate Club",
            .Graph = graph,
            .Features = features,
            .Labels = labels,
            .NumClasses = Classes,
            .TrainMask = trainMask,
            .ValidationMask = valMask,
            .TestMask = testMask
        }
    End Function

    ''' <summary>
    ''' 边同质性：同标签边占全部边的比例。用于确认标签划分与拓扑是否一致。
    ''' </summary>
    ''' <remarks>
    ''' 一张“标签对、拓扑也对”的图，同质性应该显著高于随机猜测（2 类随机约 0.5）。
    ''' 这个数字比准确率更适合做数据侧的断言：它不依赖模型，也不依赖训练轮数。
    ''' </remarks>
    Public Function Homophily(labels As Integer()) As Double
        Dim internal = 0
        Dim total = 0

        For i = 0 To _Edges.Length - 1 Step 2
            total += 1

            If labels(_Edges(i) - 1) = labels(_Edges(i + 1) - 1) Then internal += 1
        Next

        Return If(total > 0, internal / CDbl(total), 0.0)
    End Function

    ''' <summary>单位阵（每行一个 one-hot 节点特征）</summary>
    Private Function CreateIdentity(n As Integer) As Tensor
        Dim features = New Tensor(n, n)
        Dim data = features.Data

        For i = 0 To n - 1
            data(i * n + i) = 1.0
        Next

        Call features.MarkHostModified()

        Return features
    End Function

End Module
