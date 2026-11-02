' ---------------------------------------------------------------------------
' 多层 GCN 网络：把 L 个 GraphConvLayer 串起来，输出节点级 logits
'
' 经典配置是 2 层（Kipf 原文）：
'     Z = softmax( Â · ReLU( Â · X · W⁰ ) · W¹ )
' softmax 不在模型里做 —— 它属于损失（MaskedCrossEntropy 内部带 log-sum-exp），
' 提前做会让梯度多乘一层雅可比。Forward 返回的是 logits，概率另有入口。
'
' 布局边界：外部 API 收/发 [nodes, features]，内部一律特征主序 [features, nodes]。
'   两个边界各转置一次，层内零转置（理由见 GraphConvLayer 的注释）。
'
' Dropout 放置：作用在**每层的输入**上（含第一层的原始特征），这是 Kipf 原文做法，
' 等价于在消息传递之前扰动特征而不是扰动聚合结果 —— 后者会破坏归一化系数的语义。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.DeepLearning.GNN
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 图卷积神经网络（节点分类用）：若干 <see cref="GraphConvLayer"/> + 输入侧 Dropout。
''' </summary>
Public Class GCNNetwork

    Private ReadOnly _layers As New List(Of GraphConvLayer)()
    Private ReadOnly _dropouts As New List(Of DropoutLayer)()
    Private ReadOnly _inFeatures As Integer
    Private ReadOnly _numClasses As Integer

    Private ReadOnly _dropoutRate As Double
    Private _lastLogits As Tensor

    ''' <summary>
    ''' 构造两层 GCN（隐藏层 + 输出层），即 Kipf &amp; Welling 的经典结构。
    ''' </summary>
    ''' <param name="inFeatures">输入特征维度</param>
    ''' <param name="hiddenFeatures">隐藏层维度</param>
    ''' <param name="numClasses">类别数（输出维度）</param>
    ''' <param name="dropoutRate">每层输入侧的 Dropout 比例（0 表示关闭）</param>
    ''' <param name="seed">随机种子；给定后权重初始化与 Dropout 都确定，便于回归测试</param>
    ''' <param name="hiddenActivation">隐藏层激活函数</param>
    Public Sub New(inFeatures As Integer,
                   hiddenFeatures As Integer,
                   numClasses As Integer,
                   Optional dropoutRate As Double = 0.5,
                   Optional seed As Integer? = Nothing,
                   Optional hiddenActivation As ActivationType = ActivationType.ReLU)

        Call Me.New({inFeatures, hiddenFeatures, numClasses}, dropoutRate, seed, hiddenActivation)
    End Sub

    ''' <summary>
    ''' 构造任意层数的 GCN。
    ''' </summary>
    ''' <param name="layerSizes">
    ''' 各层的特征维度，长度即层数 + 1：<c>{inFeatures, hidden1, hidden2, …, numClasses}</c>。
    ''' 中间层用 <paramref name="hiddenActivation"/>，最后一层不带激活（输出 logits）。
    ''' </param>
    ''' <param name="dropoutRate">每层输入侧的 Dropout 比例</param>
    ''' <param name="seed">随机种子</param>
    ''' <param name="hiddenActivation">隐藏层激活函数</param>
    Public Sub New(layerSizes As Integer(),
                   Optional dropoutRate As Double = 0.5,
                   Optional seed As Integer? = Nothing,
                   Optional hiddenActivation As ActivationType = ActivationType.ReLU)

        If layerSizes Is Nothing OrElse layerSizes.Length < 2 Then
            Throw New ArgumentException("至少需要两层（输入层 + 输出层）", NameOf(layerSizes))
        End If
        If dropoutRate < 0.0 OrElse dropoutRate >= 1.0 Then
            Throw New ArgumentOutOfRangeException(NameOf(dropoutRate), "Dropout 比例必须在 [0, 1) 内")
        End If

        _inFeatures = layerSizes(0)
        _numClasses = layerSizes(layerSizes.Length - 1)

        For i = 0 To layerSizes.Length - 2
            Dim isLast = i = layerSizes.Length - 2
            Dim activation = If(isLast, ActivationType.None, hiddenActivation)
            Dim layerSeed = If(seed.HasValue, CType(seed.Value + i, Integer?), Nothing)

            Call _layers.Add(New GraphConvLayer(layerSizes(i), layerSizes(i + 1), activation,
                                                useBias:=True,
                                                name:=$"GCN_Layer{i + 1}",
                                                seed:=layerSeed))

            Dim dropoutSeed = If(seed.HasValue, CType(seed.Value + 1000 + i, Integer?), Nothing)

            Call _dropouts.Add(New DropoutLayer(CSng(dropoutRate), $"GCN_Dropout{i + 1}", dropoutSeed))
        Next

        _dropoutRate = dropoutRate
    End Sub

    ''' <summary>输入特征维度</summary>
    Public ReadOnly Property InFeatures As Integer
        Get
            Return _inFeatures
        End Get
    End Property

    ''' <summary>类别数（logits 的列数）</summary>
    Public ReadOnly Property NumClasses As Integer
        Get
            Return _numClasses
        End Get
    End Property

    ''' <summary>层数</summary>
    Public ReadOnly Property Depth As Integer
        Get
            Return _layers.Count
        End Get
    End Property

    ''' <summary>Dropout 比例</summary>
    Public ReadOnly Property DropoutRate As Double
        Get
            Return _dropoutRate
        End Get
    End Property

    ''' <summary>各图卷积层（只读视图，便于诊断与测试）</summary>
    Public ReadOnly Property Layers As IReadOnlyList(Of GraphConvLayer)
        Get
            Return _layers
        End Get
    End Property

    ''' <summary>最近一次前向得到的 logits（[nodes, classes]），无前向时为 Nothing</summary>
    Public ReadOnly Property LastLogits As Tensor
        Get
            Return _lastLogits
        End Get
    End Property

    ''' <summary>
    ''' 前向：节点特征 + 归一化邻接 → logits。
    ''' </summary>
    ''' <param name="features">节点特征 <c>[nodes, inFeatures]</c></param>
    ''' <param name="adjacency">对称归一化邻接</param>
    ''' <returns>logits <c>[nodes, numClasses]</c>（未做 softmax）</returns>
    Public Function Forward(features As Tensor, adjacency As NormalizedAdjacency) As Tensor
        If features Is Nothing Then Throw New ArgumentNullException(NameOf(features))
        If adjacency Is Nothing Then Throw New ArgumentNullException(NameOf(adjacency))
        If features.Rank <> 2 Then Throw New ArgumentException("节点特征必须是二维张量 [nodes, features]", NameOf(features))
        If features.Shape(0) <> adjacency.Nodes Then
            Throw New ArgumentException(
                $"节点特征的行数 {features.Shape(0)} 与邻接矩阵阶数 {adjacency.Nodes} 不一致", NameOf(features))
        End If
        If features.Shape(1) <> _inFeatures Then
            Throw New ArgumentException(
                $"节点特征维 {features.Shape(1)} 与网络输入维 {_inFeatures} 不一致", NameOf(features))
        End If

        Dim hiddenT = GCNPrecision.ToFeatureMajor(features)

        For i = 0 To _layers.Count - 1
            hiddenT = _dropouts(i).Forward(hiddenT)
            hiddenT = _layers(i).ForwardT(hiddenT, adjacency)
        Next

        _lastLogits = hiddenT.Transpose()

        Return _lastLogits
    End Function

    ''' <summary>前向并做 softmax，返回类别概率 <c>[nodes, numClasses]</c>。</summary>
    Public Function ForwardProbabilities(features As Tensor, adjacency As NormalizedAdjacency) As Tensor
        Return Tensor.computeKernel.Softmax(Forward(features, adjacency), 1)
    End Function

    ''' <summary>
    ''' 反向：logits 的梯度 → 输入特征的梯度。
    ''' </summary>
    ''' <param name="dLogits">对 logits 的梯度 <c>[nodes, numClasses]</c></param>
    ''' <returns>对输入特征的梯度 <c>[nodes, inFeatures]</c></returns>
    Public Function Backward(dLogits As Tensor) As Tensor
        If dLogits Is Nothing Then Throw New ArgumentNullException(NameOf(dLogits))
        If dLogits.Rank <> 2 OrElse dLogits.Shape(1) <> _numClasses Then
            Throw New ArgumentException(
                $"logits 梯度应为 [nodes, {_numClasses}]，实际 [{String.Join(",", dLogits.Shape)}]", NameOf(dLogits))
        End If

        Dim gradient = dLogits.Transpose()

        For i = _layers.Count - 1 To 0 Step -1
            gradient = _layers(i).BackwardT(gradient)
            gradient = _dropouts(i).Backward(gradient)
        Next

        Return gradient
    End Function

    ''' <summary>全节点的预测类别下标</summary>
    Public Function Predict(logits As Tensor) As Integer()
        If logits Is Nothing Then Throw New ArgumentNullException(NameOf(logits))

        Dim rows = logits.Shape(0)
        Dim cols = logits.Shape(1)
        Dim predicted(rows - 1) As Integer
        Dim data = logits.Data

        For i = 0 To rows - 1
            Dim offset = i * cols
            Dim best = 0
            Dim bestValue = data(offset)

            For j = 1 To cols - 1
                If data(offset + j) > bestValue Then
                    bestValue = data(offset + j)
                    best = j
                End If
            Next

            predicted(i) = best
        Next

        Return predicted
    End Function

    ''' <summary>便捷入口：前向 + 取 argmax。</summary>
    Public Function Predict(features As Tensor, adjacency As NormalizedAdjacency) As Integer()
        Return Predict(Forward(features, adjacency))
    End Function

    ''' <summary>全部可训练参数（按层顺序，训练前一次性取引用交给优化器）</summary>
    Public Function GetParameters() As List(Of Tensor)
        Dim parameters As New List(Of Tensor)()

        For Each layer In _layers
            Call parameters.AddRange(layer.GetParameters())
        Next

        Return parameters
    End Function

    ''' <summary>全部参数梯度（与 <see cref="GetParameters"/> 顺序一一对应）</summary>
    Public Function GetGradients() As List(Of Tensor)
        Dim gradients As New List(Of Tensor)()

        For Each layer In _layers
            Call gradients.AddRange(layer.GetGradients())
        Next

        Return gradients
    End Function

    ''' <summary>把所有层的梯度清零（每个训练步反向之前调用）</summary>
    Public Sub ResetGradients()
        For Each layer In _layers
            Call layer.ResetGradients()
        Next
    End Sub

    ''' <summary>训练 / 评估模式切换（只影响 Dropout）</summary>
    Public Sub SetTraining(isTraining As Boolean)
        For Each layer In _layers
            layer.IsTraining = isTraining
        Next

        ' 注意变量名不要用 dropout：TensorFlow 的 nn 模块导出了一个同名函数，会被解析成函数调用
        For Each dropoutLayer In _dropouts
            dropoutLayer.IsTraining = isTraining
        Next
    End Sub

    ''' <summary>参数总数（含偏置）</summary>
    Public ReadOnly Property ParameterCount As Integer
        Get
            Dim total = 0

            For Each parameter In GetParameters()
                total += parameter.Length
            Next

            Return total
        End Get
    End Property

    ''' <summary>打印结构（参数规模用来看“模型是否配得太大”）</summary>
    Public Sub PrintModelInfo()
        Console.WriteLine($"GCN 网络：{Depth} 层，输入 {_inFeatures} 维，输出 {_numClasses} 类，dropout={DropoutRate:P0}")
        Console.WriteLine(New String("-"c, 56))

        For i = 0 To _layers.Count - 1
            Dim layer = _layers(i)
            Console.WriteLine($"  {layer.Name,-14} {layer.InFeatures,5} → {layer.OutFeatures,-5} " &
                              $"激活={layer.Activation,-10} 参数={layer.Weights.Length + layer.Bias.Length}")
        Next

        Console.WriteLine($"  总参数量：{ParameterCount}")
    End Sub

End Class
