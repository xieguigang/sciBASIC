' ---------------------------------------------------------------------------
' 单层 GCN：H' = σ( Â · H · Wᵀ + b )，Â = D̃^(-1/2) Ã D̃^(-1/2)
'
' 为什么内部用“特征主序” [features, nodes] 而不是常见的 [nodes, features]：
'   TensorFlow 的 SpMM 契约是 dense[batch, Rows] × CSR(Rows, Columns)，
'   也就是“稠密 × 稀疏”。而 GCN 要的是 Â·H（稀疏 × 稠密）。
'   设 X 为 [nodes, features]，则 (Â·H)ᵀ = Hᵀ·Â —— 因为 Â 是对称的。
'   于是把激活值都存成 Hᵀ [features, nodes] 之后：
'     * 稀疏路径 = SpMM(Â, Hᵀ)     ✔ 正好落在既有契约上（含 CUDA 内核）
'     * 稠密路径 = MatMul(Hᵀ, Â)   ✔ 普通二维 GEMM
'     * 反向 dHᵀ = SpMM(Â, dYᵀ)    ✔ 同样因为 Â 对称，不需要转置
'   代价是模型边界各转置一次 O(n·d)，换来层内零转置 —— 这是划算的。
'
' 计算次序：先传播再变换（Â·H 再乘 W），而不是先变换再传播。
'   两者数学等价，但先传播的成本是 O(nnz·inF)，后者的成本是 O(nnz·outF)。
'   GCN 的典型形状是 inF 远大于 outF（例如 1433 → 16），先传播能省一个数量级。
'
' 梯度与精度：
'   * W / b / dW / db 全部 fp64 并**持久分配**：训练器在开训前一次性
'     GetGradients() 拿到张量引用，之后由优化器原地读写。反向时若把新张量
'     赋给字段，优化器会永远更新那个再也不会被写入的旧对象 —— 表现为
'     “损失卡住不动且不报错”（GNN/LinearLayer.vb 里踩过这个坑）。
'   * 四个算力热点（传播、变换、dW、dZ）走 GCNPrecision，规模达标即降 fp32。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.DeepLearning.GNN
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 一层 Kipf &amp; Welling 图卷积：<c>H' = σ(Â · H · Wᵀ + b)</c>。
''' </summary>
''' <remarks>
''' <para>
''' 输入输出都是<b>特征主序</b>布局 <c>[features, nodes]</c>（即 Hᵀ），
''' 由 <see cref="GCNNetwork"/> 在模型边界完成与 <c>[nodes, features]</c> 的互换。
''' </para>
''' <para>
''' 激活函数支持 <see cref="ActivationType.None"/> / ReLU / Sigmoid / Tanh / LeakyReLU；
''' Softmax 不在此列 —— 分类任务的 softmax 由损失函数（交叉熵）内部完成，
''' 提前做 softmax 会让梯度多乘一层雅可比，既不必要也更易数值不稳。
''' </para>
''' </remarks>
Public Class GraphConvLayer

    Private ReadOnly _inFeatures As Integer
    Private ReadOnly _outFeatures As Integer
    Private ReadOnly _activation As ActivationType
    Private ReadOnly _useBias As Boolean

    ''' <summary>权重 [outFeatures, inFeatures]（与 dW 同形，便于优化器按下标一一对应）</summary>
    Private ReadOnly _weights As Tensor
    ''' <summary>偏置 [1, outFeatures]</summary>
    Private ReadOnly _bias As Tensor
    Private ReadOnly _weightGradient As Tensor
    Private ReadOnly _biasGradient As Tensor

    Private _inputT As Tensor
    Private _adjacency As NormalizedAdjacency
    ''' <summary>Â·X（变换前的聚合结果），反向要它算 dW</summary>
    Private _aggregatedT As Tensor
    ''' <summary>激活前的输出，反向要它算激活导数</summary>
    Private _outputT As Tensor

    ''' <summary>创建一层图卷积。</summary>
    ''' <param name="inFeatures">输入特征维度</param>
    ''' <param name="outFeatures">输出特征维度</param>
    ''' <param name="activation">激活函数</param>
    ''' <param name="useBias">是否使用偏置（Kipf 原文每层都带 bias）</param>
    ''' <param name="name">层名称</param>
    ''' <param name="seed">Xavier 初始化的随机种子；给定后同一份代码每次跑出完全相同的权重</param>
    Public Sub New(inFeatures As Integer,
                   outFeatures As Integer,
                   Optional activation As ActivationType = ActivationType.ReLU,
                   Optional useBias As Boolean = True,
                   Optional name As String = Nothing,
                   Optional seed As Integer? = Nothing)

        If inFeatures <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(inFeatures))
        If outFeatures <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(outFeatures))

        _inFeatures = inFeatures
        _outFeatures = outFeatures
        _activation = activation
        _useBias = useBias
        Me.Name = If(name, $"GraphConv_{inFeatures}_{outFeatures}")

        ' Xavier 初始化：fanIn/fanOut 的次序必须与 [outF, inF] 的布局一致。
        ' （GNN/LinearLayer 里曾经把两者写反，方阵时静默学错，非方阵时直接报维度不匹配。
        '   对 Xavier 而言交换两个实参不改变初始化分布，所以问题只在形状上暴露。）
        _weights = Tensor.XavierInit(outFeatures, inFeatures, seed)
        _bias = If(useBias, New Tensor(1, outFeatures), New Tensor(0))

        _weightGradient = New Tensor(outFeatures, inFeatures)
        _biasGradient = If(useBias, New Tensor(1, outFeatures), New Tensor(0))
    End Sub

    ''' <summary>层名称</summary>
    Public Property Name As String

    ''' <summary>层是否处于训练模式（本层无随机性，保留该标志是为了与其它层统一）</summary>
    Public Property IsTraining As Boolean = True

    ''' <summary>输入特征维度</summary>
    Public ReadOnly Property InFeatures As Integer
        Get
            Return _inFeatures
        End Get
    End Property

    ''' <summary>输出特征维度</summary>
    Public ReadOnly Property OutFeatures As Integer
        Get
            Return _outFeatures
        End Get
    End Property

    ''' <summary>激活函数</summary>
    Public ReadOnly Property Activation As ActivationType
        Get
            Return _activation
        End Get
    End Property

    ''' <summary>权重矩阵 [outFeatures, inFeatures]</summary>
    Public ReadOnly Property Weights As Tensor
        Get
            Return _weights
        End Get
    End Property

    ''' <summary>偏置 [1, outFeatures]</summary>
    Public ReadOnly Property Bias As Tensor
        Get
            Return _bias
        End Get
    End Property

    ''' <summary>
    ''' 前向（特征主序）。
    ''' </summary>
    ''' <param name="inputT">输入特征 <c>[inFeatures, nodes]</c></param>
    ''' <param name="adjacency">对称归一化邻接</param>
    ''' <returns>输出特征 <c>[outFeatures, nodes]</c></returns>
    Public Function ForwardT(inputT As Tensor, adjacency As NormalizedAdjacency) As Tensor
        If inputT Is Nothing Then Throw New ArgumentNullException(NameOf(inputT))
        If adjacency Is Nothing Then Throw New ArgumentNullException(NameOf(adjacency))
        If inputT.Rank <> 2 Then Throw New ArgumentException("输入特征必须是二维张量", NameOf(inputT))

        If inputT.Shape(0) <> _inFeatures Then
            Throw New ArgumentException(
                $"输入特征维 {inputT.Shape(0)} 与本层 inFeatures={_inFeatures} 不一致", NameOf(inputT))
        End If
        If inputT.Shape(1) <> adjacency.Nodes Then
            Throw New ArgumentException(
                $"输入节点数 {inputT.Shape(1)} 与邻接矩阵阶数 {adjacency.Nodes} 不一致", NameOf(inputT))
        End If

        _inputT = inputT
        _adjacency = adjacency

        ' ① 邻居聚合：Â·X → [inFeatures, nodes]（稠密 GEMM 或 CSR SpMM，热点 → fp32）
        _aggregatedT = GCNPrecision.Propagate(adjacency, inputT)

        ' ② 特征变换：W·Z → [outFeatures, nodes]（热点 → fp32）
        Dim transformed = GCNPrecision.MatMulOr(_weights, _aggregatedT)

        ' ③ 偏置
        If _useBias Then
            Dim rows = transformed.Shape(0)
            Dim cols = transformed.Shape(1)
            Dim data = transformed.Data
            Dim bias = _bias.Data

            For i = 0 To rows - 1
                Dim biasValue = bias(i)
                Dim offset = i * cols

                For j = 0 To cols - 1
                    data(offset + j) += biasValue
                Next
            Next

            Call transformed.MarkHostModified()
        End If

        _outputT = transformed

        ' ④ 激活（fp64 逐元素；这里的元素数是 n·outF，不属于“算力热点”）
        Return ActivationFunctions.Apply(transformed, _activation)
    End Function

    ''' <summary>
    ''' 反向（特征主序）。
    ''' </summary>
    ''' <param name="gradT">对输出的梯度 <c>[outFeatures, nodes]</c></param>
    ''' <returns>对输入的梯度 <c>[inFeatures, nodes]</c></returns>
    Public Function BackwardT(gradT As Tensor) As Tensor
        If _outputT Is Nothing Then
            Throw New InvalidOperationException("反向传播之前必须先调用 ForwardT")
        End If
        If gradT Is Nothing Then Throw New ArgumentNullException(NameOf(gradT))
        If gradT.Rank <> 2 OrElse gradT.Shape(0) <> _outFeatures OrElse gradT.Shape(1) <> _outputT.Shape(1) Then
            Throw New ArgumentException(
                $"输出梯度形状应为 [{_outFeatures}, {_outputT.Shape(1)}]，实际 [{String.Join(",", gradT.Shape)}]", NameOf(gradT))
        End If

        ' ① 激活导数：dY = dOut ⊙ σ'(pre-activation)
        Dim dY = gradT.ElementwiseMultiply(ActivationFunctions.Derivative(_outputT, _activation))

        ' ② dW = dY · Zᵀ → [outFeatures, inFeatures]（热点 → fp32，写回持久张量）
        Dim dW = GCNPrecision.MatMulOr(dY, _aggregatedT.Transpose())

        Call Array.Copy(dW.Data, _weightGradient.Data, _weightGradient.Length)
        Call _weightGradient.MarkHostModified()

        ' ③ db = Σ_nodes dY → [outFeatures]，扁平布局与 [1, outFeatures] 一致
        If _useBias Then
            Dim db = dY.Sum(1)

            Call Array.Copy(db.Data, _biasGradient.Data, _biasGradient.Length)
            Call _biasGradient.MarkHostModified()
        End If

        ' ④ dZ = Wᵀ·dY → [inFeatures, nodes]（热点 → fp32）
        Dim dZ = GCNPrecision.MatMulOr(_weights.Transpose(), dY)

        ' ⑤ dX = Âᵀ·dZ。Â 对称，因此传播算子与其转置是同一个（热点 → fp32）
        Return GCNPrecision.Propagate(_adjacency, dZ)
    End Function

    ''' <summary>本层可训练参数（顺序固定：权重、偏置）</summary>
    Public Function GetParameters() As List(Of Tensor)
        Dim parameters As New List(Of Tensor) From {_weights}

        If _useBias Then Call parameters.Add(_bias)

        Return parameters
    End Function

    ''' <summary>本层参数梯度（与 <see cref="GetParameters"/> 一一对应）</summary>
    Public Function GetGradients() As List(Of Tensor)
        Dim gradients As New List(Of Tensor) From {_weightGradient}

        If _useBias Then Call gradients.Add(_biasGradient)

        Return gradients
    End Function

    ''' <summary>把本层梯度清零（每个训练步反向之前必须调用，否则梯度跨步无界累积）</summary>
    Public Sub ResetGradients()
        For Each grad In GetGradients()
            Dim data = grad.Data

            For i = 0 To data.Length - 1
                data(i) = 0.0
            Next

            Call grad.MarkHostModified()
        Next
    End Sub

    Public Overrides Function ToString() As String
        Return $"{Name}({_inFeatures}→{_outFeatures}, {_activation}, bias={_useBias})"
    End Function

End Class
