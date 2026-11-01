Namespace train.kernels

    ''' <summary>
    ''' 建树期的轻量节点。
    '''
    ''' 与 <see cref="TreeNode"/> 的区别：不持有 <c>G_left/H_left/Grad_missing/Hess_missing</c>
    ''' 这些 <c>Double(feature_dim)</c> 数组（旧实现每节点 6 条 + 1 个类别字典），
    ''' 节点统计量由分裂查找器的线程本地累加器承担。建树结束后再一次性物化为
    ''' <see cref="TreeNode"/>，从而保持 <c>Tree.root</c> / <c>ModelSerializer</c> 的既有契约。
    ''' </summary>
    Friend Class BuildNode

        ''' <summary>
        ''' 节点编号（在本层/本棵树内的扁平下标）
        ''' </summary>
        Public Id As Integer
        Public Depth As Integer
        ''' <summary>
        ''' 三叉树编号：根为 1，左子为 <c>3i-1</c>，缺失子为 <c>3i</c>，右子为 <c>3i+1</c>
        ''' </summary>
        Public Index As Integer

        Public Left As BuildNode
        Public Right As BuildNode
        Public NanChild As BuildNode

        Public Feature As Integer = -1
        Public Threshold As Single
        Public LeftCategories As Integer()
        Public NanDirection As Integer = SplitCandidate.NanDirectionNone

        ''' <summary>
        ''' 参与梯度累加（被行采样命中）的样本梯度/海森之和
        ''' </summary>
        Public Grad As Double
        Public Hess As Double
        ''' <summary>
        ''' 未采样掩码的全量海森之和，用于 <c>min_child_weight</c> 判据
        ''' </summary>
        Public HessAll As Double
        ''' <summary>
        ''' 采样命中的样本量
        ''' </summary>
        Public NSamples As Integer

        Public IsLeaf As Boolean
        Public LeafScore As Double

        ''' <summary>
        ''' 物化后的 <see cref="TreeNode"/>
        ''' </summary>
        Public Materialized As TreeNode

        Public Sub New(id As Integer, depth As Integer, index As Integer)
            Me.Id = id
            Me.Depth = depth
            Me.Index = index
        End Sub

        Public Overrides Function ToString() As String
            If IsLeaf Then
                Return $"#{Index} leaf={LeafScore:F6} n={NSamples}"
            End If

            Return $"#{Index} feature={Feature} gain-ok n={NSamples}"
        End Function
    End Class
End Namespace
