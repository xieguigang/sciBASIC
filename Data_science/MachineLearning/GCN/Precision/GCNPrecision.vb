' ---------------------------------------------------------------------------
' GCN 的精度路由：把“这次能不能降到单精度”收在一个地方
'
' 为什么要有这个模块（而不是在层里直接写 MixedPrecisionOps.MatMulOr）：
'   1. 层里需要的是**语义**入口 —— “把 Â 作用到特征上”，它可能是稠密 GEMM，
'      也可能是 CSR SpMM；这两种算子的选择与精度选择是两件独立的事，
'      混在层代码里会变成一堆 if 嵌套；
'   2. 路由要可观测：每次“本来能降精度但没降”都应该被计数，
'      否则性能回归只能靠猜（见 MixedPrecisionDiagnostics）；
'   3. 和 DeepLearning/CNN 的 PrecisionRouter 保持同一套约定：
'      Try* 不适用时返回 Nothing，*Or 保证一定有结果，Enabled 一键回退。
'
' 精度边界（本模块只负责“算力热点”，其余一律 fp64）：
'   * 降：MatMul（Â·H / W·Z / dW / dZ）、SpMM（稀疏 Â·H）；
'   * 不降：归一化系数、logits 累加后的损失、交叉熵梯度、优化器状态 —— 那些在
'     MixedPrecisionPolicy 里属于 MustDouble，本模块不会碰它们。
'
' 布局约定：所有入口都按“特征主序” [features, nodes] 工作。
'   设 X 为 [nodes, features]，则 Â·X 的转置是 Xᵀ·Â。由于 Â 对称，
'   Xᵀ·Â = (Â·X)ᵀ —— 于是稠密路径是普通的二维 GEMM，稀疏路径正好落在
'   SpMM 的“稠密 × 稀疏”契约上（[batch, Rows] × CSR(Rows, Columns)）。
'   这一点让两条路径共享同一个布局，层与层之间不需要反复转置。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute

''' <summary>
''' GCN 算子的 fp64 / fp32 路由（GEMM 与 SpMM）。
''' </summary>
Public Module GCNPrecision

    ''' <summary>
    ''' GCN 侧总开关；关闭后所有路由一律走 fp64（用于定位数值回归）。
    ''' </summary>
    ''' <remarks>
    ''' 与 <see cref="MixedPrecisionPolicy.Enabled"/> 是“与”关系：任一为 False 即全程 fp64。
    ''' </remarks>
    Public Property Enabled As Boolean = True

    ' ---------------- GEMM ----------------

    ''' <summary>
    ''' 二维矩阵乘的 fp32 尝试。
    ''' </summary>
    ''' <returns>降精度后的结果；规模不足、数值不安全或开关关闭时返回 <c>Nothing</c>。</returns>
    Public Function TryMatMul(a As Tensor, b As Tensor) As Tensor
        If Not Enabled Then Return Nothing
        If a Is Nothing OrElse b Is Nothing Then Return Nothing
        If a.Rank <> 2 OrElse b.Rank <> 2 Then Return Nothing

        ' 形状不匹配交给调用方的 fp64 路径去抛（那里能给出更贴近业务的错误信息）
        If a.Shape(1) <> b.Shape(0) Then Return Nothing

        Dim fast = MixedPrecisionOps.MatMul(a, b)

        If fast Is Nothing Then
            MixedPrecisionDiagnostics.Fallback("GCN.MatMul", "规模不足或数值不安全")
            Return Nothing
        End If

        Call MixedPrecisionDiagnostics.Accelerated("GCN.MatMul")

        Return fast
    End Function

    ''' <summary>二维矩阵乘：fp32 优先，不适用时回退 fp64。</summary>
    Public Function MatMulOr(a As Tensor, b As Tensor) As Tensor
        Dim fast = TryMatMul(a, b)

        If fast IsNot Nothing Then Return fast

        Return a.MatMul(b)
    End Function

    ' ---------------- 消息传递 ----------------

    ''' <summary>
    ''' 把归一化邻接作用到特征上：输入输出都是特征主序 <c>[features, nodes]</c>。
    ''' </summary>
    ''' <param name="adjacency">对称归一化邻接</param>
    ''' <param name="featuresT">特征主序的特征矩阵 <c>[features, nodes]</c></param>
    ''' <returns><c>(Â·X)ᵀ</c>，形状同为 <c>[features, nodes]</c></returns>
    Public Function Propagate(adjacency As NormalizedAdjacency, featuresT As Tensor) As Tensor
        If adjacency Is Nothing Then Throw New ArgumentNullException(NameOf(adjacency))
        If featuresT Is Nothing Then Throw New ArgumentNullException(NameOf(featuresT))
        If featuresT.Rank <> 2 Then Throw New ArgumentException("特征矩阵必须是二维张量", NameOf(featuresT))
        If featuresT.Shape(1) <> adjacency.Nodes Then
            Throw New ArgumentException(
                $"特征矩阵的节点维 {featuresT.Shape(1)} 与邻接矩阵阶数 {adjacency.Nodes} 不一致", NameOf(featuresT))
        End If

        Select Case adjacency.EffectiveMode
            Case PropagationMode.Sparse
                Return PropagateSparse(adjacency.Sparse, featuresT)
            Case Else
                Return PropagateDense(adjacency.Dense, featuresT)
        End Select
    End Function

    ''' <summary>稠密传播：<c>Xᵀ·Â</c>（Â 对称，等价于 <c>(Â·X)ᵀ</c>）。</summary>
    Public Function PropagateDense(dense As Tensor, featuresT As Tensor) As Tensor
        Return MatMulOr(featuresT, dense)
    End Function

    ''' <summary>稀疏传播：<c>SpMM(Â, Xᵀ)</c>，fp32 优先。</summary>
    Public Function PropagateSparse(csr As SparseCsr, featuresT As Tensor) As Tensor
        Dim fast = TrySpMM(csr, featuresT)

        If fast IsNot Nothing Then Return fast

        Return Tensor.computeKernel.SpMM(csr, featuresT)
    End Function

    ''' <summary>
    ''' 稀疏 × 稠密的 fp32 尝试。
    ''' </summary>
    ''' <returns>降精度后的结果；不适用时返回 <c>Nothing</c>。</returns>
    Public Function TrySpMM(csr As SparseCsr, featuresT As Tensor) As Tensor
        If Not Enabled Then Return Nothing
        If csr Is Nothing OrElse featuresT Is Nothing Then Return Nothing
        If featuresT.Rank <> 2 OrElse featuresT.Shape(1) <> csr.Rows Then Return Nothing

        Dim fast = MixedPrecisionOps.SpMM(csr, featuresT)

        If fast Is Nothing Then
            ' 非零元太少时降精度不划算（主机端本来就没几个乘法可做），这不是错误
            MixedPrecisionDiagnostics.Fallback("GCN.SpMM", "非零元不足或数值不安全")
            Return Nothing
        End If

        Call MixedPrecisionDiagnostics.Accelerated("GCN.SpMM")

        Return fast
    End Function

    ''' <summary>把输入特征转成特征主序（<c>[nodes, features]</c> → <c>[features, nodes]</c>）。</summary>
    ''' <remarks>
    ''' 只在模型边界调用一次。O(n·d) 的拷贝相对每层 O(n²·d) / O(nnz·d) 可以忽略，
    ''' 而它换来的是层内“零转置”的整条链路。
    ''' </remarks>
    Public Function ToFeatureMajor(features As Tensor) As Tensor
        If features.Rank <> 2 Then Throw New ArgumentException("节点特征必须是二维张量 [nodes, features]", NameOf(features))

        Return features.Transpose()
    End Function

End Module
