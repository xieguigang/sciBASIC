' ---------------------------------------------------------------------------
' 混合精度算子外观：把"该不该降到单精度"与"怎么转"收在一处
'
' 设计取向（为什么做成外观而不是改造各层的数据结构）：
'   四个深度学习项目的层内部仍然以 tensor(Double) 为存储主体 ——
'   这是有意的：权重、优化器状态、LayerNorm 统计量、ODE 状态都需要双精度。
'   真正适合降精度的只是**几个算力密集的算子**（GEMM / Conv / SpMM）。
'   于是最省侵入性的做法是：保持层的数据结构不变，只在调用这几个算子时
'   临时降到 tensorF 计算、再升回 tensor。转换成本是 O(输入+输出)，
'   而被加速的算子是 O(n³)，规模一大就完全摊得平。
'
' 好处：
'   * 调用点只改一行，不必重写整层的类型体系；
'   * 判据、转换、统计、回退全在一个地方，调参不会漏改；
'   * `MixedPrecisionPolicy.Enabled = False` 即可整体回退，便于定位回归。
'
' 所有函数遵循同一约定：**不适用时返回 Nothing**，由调用方走原有双精度路径。
' 这样调用侧的写法统一为：
'       Dim fast = MixedPrecisionOps.MatMul(a, b)
'       If fast IsNot Nothing Then Return fast
' ---------------------------------------------------------------------------

Imports std = System.Math

Namespace Compute

    ''' <summary>
    ''' 把算力密集算子临时降到单精度执行的集中入口。
    ''' </summary>
    Public Module MixedPrecisionOps

        ''' <summary>
        ''' 大矩阵乘：<c>a(m,k) × b(k,n)</c>。
        ''' </summary>
        ''' <returns>降精度后的结果；不适用时返回 <c>Nothing</c>。</returns>
        Public Function MatMul(a As Tensor, b As Tensor) As Tensor
            If a Is Nothing OrElse b Is Nothing Then Return Nothing
            If a.Rank <> 2 OrElse b.Rank <> 2 Then Return Nothing

            Dim m = a.Shape(0), k = a.Shape(1), n = b.Shape(1)
            If b.Shape(0) <> k Then Return Nothing

            Dim ops = MixedPrecisionPolicy.GemmOps(m, k, n)
            Dim decision = MixedPrecisionPolicy.Decide(PrecisionClass.ComputeBound, ops, "MatMul")

            If Not decision.UseSingle Then Return Nothing
            If Not IsConvertible(a) OrElse Not IsConvertible(b) Then Return Nothing

            Return TensorF.computeKernelF.MatMul(a.ToTensorF(), b.ToTensorF()).ToTensor()
        End Function

        ''' <summary>
        ''' 批量矩阵乘：<c>a(..., imax, kmax) × b(..., kmax, jmax)</c>。
        ''' </summary>
        ''' <remarks>
        ''' 单精度张量栈没有批量 GEMM 原语，因此这里**逐 block** 调用 2D 矩阵乘。
        ''' block 之间的数据搬运是 O(imax·kmax + kmax·jmax)，
        ''' 相对 GEMM 的 O(imax·kmax·jmax) 在常规维度下可以忽略。
        ''' <para>
        ''' b 为二维时表示"所有 block 共享同一个右乘矩阵"，此时把 a 压平成一个
        ''' 大矩阵一次算完，比逐 block 更快。
        ''' </para>
        ''' </remarks>
        Public Function BatchedMatMul(a As Tensor, b As Tensor) As Tensor
            If a Is Nothing OrElse b Is Nothing Then Return Nothing
            If a.Rank < 2 OrElse b.Rank < 2 Then Return Nothing

            Dim rank = a.Rank
            Dim imax = a.Shape(rank - 2), kmax = a.Shape(rank - 1)
            Dim jmax = b.Shape(b.Rank - 1)

            If b.Shape(b.Rank - 2) <> kmax Then Return Nothing

            Dim blocks = 1
            For ld = 0 To rank - 3
                blocks *= a.Shape(ld)
            Next

            Dim ops = MixedPrecisionPolicy.GemmOps(imax, kmax, jmax) * blocks
            Dim decision = MixedPrecisionPolicy.Decide(PrecisionClass.ComputeBound, ops, "BatchedMatMul")

            If Not decision.UseSingle Then Return Nothing
            If Not IsConvertible(a) OrElse Not IsConvertible(b) Then Return Nothing

            ' ---- 右乘矩阵被所有 block 共享：压平成一次 2D GEMM ----
            If b.Rank = 2 Then
                Dim rows = a.Length \ kmax
                Dim flat = Tensor.Wrap(a.Data, rows, kmax)

                Dim outShape = CType(a.Shape.Clone(), Integer())
                outShape(rank - 2) = imax
                outShape(rank - 1) = jmax

                Dim produced = TensorF.computeKernelF.MatMul(flat.ToTensorF(), b.ToTensorF()).ToTensor()
                Return Tensor.Wrap(produced.Data, outShape)
            End If

            If b.Length \ (kmax * jmax) <> blocks Then Return Nothing

            ' ---- 逐 block ----
            Dim result = New Tensor(CType(a.Shape.Clone(), Integer()))

            Dim bsA = imax * kmax
            Dim bsB = kmax * jmax
            Dim bsC = imax * jmax
            Dim cData = result.Data

            For blk = 0 To blocks - 1
                Dim aBlk = BlockCopy(a.Data, blk * bsA, imax, kmax)
                Dim bBlk = BlockCopy(b.Data, blk * bsB, kmax, jmax)
                Dim cBlk = TensorF.computeKernelF.MatMul(aBlk.ToTensorF(), bBlk.ToTensorF()).Data

                Array.Copy(cBlk, 0, cData, blk * bsC, bsC)
            Next

            Call result.MarkHostModified()
            Return result
        End Function

        ''' <summary>二维卷积：把 <c>x</c> / <c>filters</c> 降到单精度算完再升回。</summary>
        Public Function Conv2D(x As Tensor, filters As Tensor, bias As Tensor,
                               stride As Integer, padding As Integer) As Tensor
            If x Is Nothing OrElse filters Is Nothing Then Return Nothing
            If x.Rank <> 4 OrElse filters.Rank <> 4 Then Return Nothing
            If filters.Shape(2) <> x.Shape(3) Then Return Nothing

            Dim batch = x.Shape(0)
            Dim outH = ConvOutSize(x.Shape(1), filters.Shape(0), stride, padding)
            Dim outW = ConvOutSize(x.Shape(2), filters.Shape(1), stride, padding)

            If outH <= 0 OrElse outW <= 0 Then Return Nothing

            Dim ops = MixedPrecisionPolicy.ConvOps(batch, outH, outW, filters.Shape(3),
                                                   filters.Shape(0), filters.Shape(1), filters.Shape(2))
            Dim decision = MixedPrecisionPolicy.Decide(PrecisionClass.ComputeBound, ops, "Conv2D")

            If Not decision.UseSingle Then Return Nothing
            If Not IsConvertible(x) OrElse Not IsConvertible(filters) Then Return Nothing

            Dim fb = If(bias Is Nothing, Nothing, bias.ToTensorF())
            Dim xf = x.ToTensorF()
            Dim ff = filters.ToTensorF()

            Return TensorF.computeKernelF.Conv2D(xf, ff, fb, stride, padding).ToTensor()
        End Function

        ''' <summary>
        ''' 二维矩阵乘的"总是有结果"版本：不适用降精度时自动走双精度。
        ''' </summary>
        ''' <remarks>
        ''' 对绝大多数调用点来说，这是最省事的入口 —— 一行替换即可：
        ''' <c>a.MatMul(b)</c> → <c>MixedPrecisionOps.MatMulOr(a, b)</c>。
        ''' 只是调用方拿不到"是否真的降了精度"的信息，需要诊断时请用 <see cref="MatMul"/>。
        ''' </remarks>
        Public Function MatMulOr(a As Tensor, b As Tensor) As Tensor
            Dim fast = MatMul(a, b)
            If fast IsNot Nothing Then Return fast

            Return a.MatMul(b)
        End Function

        ''' <summary>二维卷积的"总是有结果"版本。</summary>
        Public Function Conv2DOr(x As Tensor, filters As Tensor, bias As Tensor,
                                 stride As Integer, padding As Integer) As Tensor
            Dim fast = Conv2D(x, filters, bias, stride, padding)
            If fast IsNot Nothing Then Return fast

            Return Tensor.computeKernel.Conv2D(x, filters, bias, stride, padding)
        End Function

        ''' <summary>CSR 稀疏 × 稠密。判据用非零元个数（决定实际工作量）。</summary>
        Public Function SpMM(csr As SparseCsr, dense As Tensor) As Tensor
            If csr Is Nothing OrElse dense Is Nothing Then Return Nothing
            If dense.Rank <> 2 OrElse dense.Shape(1) <> csr.Rows Then Return Nothing

            Dim decision = MixedPrecisionPolicy.Decide(
                PrecisionClass.ComputeBound, csr.NonZeros, "SpMM")

            If Not decision.UseSingle Then Return Nothing
            If Not IsConvertible(dense) Then Return Nothing

            Return TensorF.computeKernelF.SpMM(csr, dense.ToTensorF()).ToTensor()
        End Function

        ' ---------------- 内部 ----------------

        ''' <summary>把一个连续块从扁平数组里拷出来，并包装成 <c>(rows, cols)</c> 张量</summary>
        Private Function BlockCopy(source As Double(), offset As Integer,
                                   rows As Integer, cols As Integer) As Tensor
            Dim block(rows * cols - 1) As Double
            Array.Copy(source, offset, block, 0, block.Length)
            Return Tensor.Wrap(block, rows, cols)
        End Function

        ''' <summary>输出尺寸（与 <c>TensorComputeBase.ConvOutSize</c> 同式）</summary>
        Private Function ConvOutSize(inputSize As Integer, kernel As Integer,
                                     stride As Integer, padding As Integer) As Integer
            Return (inputSize + 2 * padding - kernel) \ stride + 1
        End Function

        ''' <summary>
        ''' 数据是否可以安全降到单精度。
        ''' </summary>
        ''' <remarks>
        ''' 溢出到 ±Single.MaxValue 之外的值会在单精度里变成 Infinity，
        ''' 而 Inf 一旦进入动量或二阶矩就会永久粘住。宁可退到双精度也不要传染。
        ''' 扫描成本是 O(元素数)，相对被加速算子的 O(n³) 可以忽略。
        ''' </remarks>
        Private Function IsConvertible(t As Tensor) As Boolean
            Return MixedPrecisionPolicy.IsSafeForSingle(t.Data)
        End Function

    End Module

End Namespace
