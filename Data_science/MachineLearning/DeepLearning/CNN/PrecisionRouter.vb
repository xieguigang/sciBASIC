' ---------------------------------------------------------------------------
' 卷积族算子的"精度路由"
'
' 与 TensorFlow 的 MixedPrecisionOps 是同一套约定的延续：
'   * 层内部仍然以 tensor(Double) 作为主存储（权重 / 优化器状态 / 统计量必须是双精度）；
'   * 真正值得降精度的只有**算力密集**的卷积族算子；
'   * 判据（规模 + 数值安全）集中在一处，调用点只问"这次能不能走单精度"。
'
' 之所以还要再补这一个模块：MixedPrecisionOps 只封装了 Conv2D 前向与 GEMM，
' 而 U-Net 解码器的转置卷积需要
'   * Conv2D（转置卷积前向走的就是它）
'   * Conv2DBackwardInput / Conv2DBackwardFilter / Conv2DBackwardBias
' 三个反向算子同样按"规模达标就降 fp32、算完升回 fp64"执行，
' 否则解码器一半的热点（反向）仍然留在双精度上，混合精度就名不副实。
'
' 全部函数遵循同一约定：**不适用时返回 Nothing**，由调用方走原有双精度路径；
' 带 Or 后缀的版本则保证一定有结果（不适用时自动回退双精度）。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute

Namespace CNN

    ''' <summary>
    ''' 卷积族算子在双精度(fp64)与单精度(fp32)之间的路由。
    ''' </summary>
    ''' <remarks>
    ''' 判据完全复用 <see cref="MixedPrecisionPolicy"/>：规模用
    ''' <see cref="MixedPrecisionPolicy.Decide"/> 判定，数值安全性用
    ''' <see cref="MixedPrecisionPolicy.IsSafeForSingle"/> 判定（存在非有限值或超出单精度范围的值时
    ''' 一律留在双精度，避免 Inf 顺着动量/二阶矩永久粘住）。
    ''' </remarks>
    Public Module PrecisionRouter

        ''' <summary>
        ''' 总开关；关闭后所有路由一律返回双精度结果（用于定位数值回归时整体回退）。
        ''' </summary>
        Public Property Enabled As Boolean = True

        ' ---------------- 保证有结果的入口 ----------------

        ''' <summary>二维卷积：规模达标时降到单精度计算，否则走双精度。</summary>
        Public Function Conv2DOr(x As Tensor, filters As Tensor, bias As Tensor,
                                 stride As Integer, padding As Integer) As Tensor
            Return MixedPrecisionOps.Conv2DOr(x, filters, bias, stride, padding)
        End Function

        ''' <summary>卷积反向（对输入）：规模达标时降到单精度计算，否则走双精度。</summary>
        Public Function Conv2DBackwardInputOr(gradOutput As Tensor, filters As Tensor,
                                              inputShape As Integer(),
                                              stride As Integer, padding As Integer) As Tensor
            Dim fast = TryConv2DBackwardInput(gradOutput, filters, inputShape, stride, padding)
            If fast IsNot Nothing Then Return fast

            Return Tensor.computeKernel.Conv2DBackwardInput(gradOutput, filters, inputShape, stride, padding)
        End Function

        ''' <summary>卷积反向（对卷积核）：规模达标时降到单精度计算，否则走双精度。</summary>
        Public Function Conv2DBackwardFilterOr(gradOutput As Tensor, x As Tensor,
                                               filterShape As Integer(),
                                               stride As Integer, padding As Integer) As Tensor
            Dim fast = TryConv2DBackwardFilter(gradOutput, x, filterShape, stride, padding)
            If fast IsNot Nothing Then Return fast

            Return Tensor.computeKernel.Conv2DBackwardFilter(gradOutput, x, filterShape, stride, padding)
        End Function

        ''' <summary>卷积反向（对偏置）：规模达标时降到单精度计算，否则走双精度。</summary>
        Public Function Conv2DBackwardBiasOr(gradOutput As Tensor) As Tensor
            Dim fast = TryConv2DBackwardBias(gradOutput)
            If fast IsNot Nothing Then Return fast

            Return Tensor.computeKernel.Conv2DBackwardBias(gradOutput)
        End Function

        ' ---------------- "可能没有结果"的入口 ----------------

        ''' <summary>
        ''' 卷积反向（对输入）的单精度尝试。
        ''' </summary>
        ''' <returns>降精度后的结果；不适用时返回 <c>Nothing</c>。</returns>
        Public Function TryConv2DBackwardInput(gradOutput As Tensor, filters As Tensor,
                                               inputShape As Integer(),
                                               stride As Integer, padding As Integer) As Tensor
            If Not Enabled Then Return Nothing
            If gradOutput Is Nothing OrElse filters Is Nothing OrElse inputShape Is Nothing Then Return Nothing
            If gradOutput.Rank <> 4 OrElse filters.Rank <> 4 OrElse inputShape.Length <> 4 Then Return Nothing

            Dim batch = inputShape(0), inH = inputShape(1), inW = inputShape(2), inC = inputShape(3)
            Dim filtH = filters.Shape(0), filtW = filters.Shape(1)
            Dim outC = gradOutput.Shape(3)

            ' 形状约束与后端一致：卷积核的输入通道数必须等于目标的通道数
            If filters.Shape(2) <> inC Then Return Nothing
            If filters.Shape(3) <> outC Then Return Nothing
            If gradOutput.Shape(0) <> batch Then Return Nothing

            Dim ops = Ops(batch, inH, inW, inC, filtH, filtW, outC)
            Dim decision = MixedPrecisionPolicy.Decide(PrecisionClass.ComputeBound, ops, "Conv2DBackwardInput")

            If Not decision.UseSingle Then Return Nothing
            If Not Safe(gradOutput) OrElse Not Safe(filters) Then Return Nothing

            Return TensorF.computeKernelF.Conv2DBackwardInput(
                gradOutput.ToTensorF(), filters.ToTensorF(), inputShape, stride, padding).ToTensor()
        End Function

        ''' <summary>
        ''' 卷积反向（对卷积核）的单精度尝试。
        ''' </summary>
        ''' <returns>降精度后的结果；不适用时返回 <c>Nothing</c>。</returns>
        Public Function TryConv2DBackwardFilter(gradOutput As Tensor, x As Tensor,
                                                filterShape As Integer(),
                                                stride As Integer, padding As Integer) As Tensor
            If Not Enabled Then Return Nothing
            If gradOutput Is Nothing OrElse x Is Nothing OrElse filterShape Is Nothing Then Return Nothing
            If gradOutput.Rank <> 4 OrElse x.Rank <> 4 OrElse filterShape.Length <> 4 Then Return Nothing

            Dim filtH = filterShape(0), filtW = filterShape(1), inC = filterShape(2), outC = filterShape(3)
            Dim batch = x.Shape(0), inH = x.Shape(1), inW = x.Shape(2)

            If x.Shape(3) <> inC Then Return Nothing
            If gradOutput.Shape(3) <> outC Then Return Nothing
            If gradOutput.Shape(0) <> batch Then Return Nothing

            Dim ops = Ops(batch, inH, inW, inC, filtH, filtW, outC)
            Dim decision = MixedPrecisionPolicy.Decide(PrecisionClass.ComputeBound, ops, "Conv2DBackwardFilter")

            If Not decision.UseSingle Then Return Nothing
            If Not Safe(gradOutput) OrElse Not Safe(x) Then Return Nothing

            Return TensorF.computeKernelF.Conv2DBackwardFilter(
                gradOutput.ToTensorF(), x.ToTensorF(), filterShape, stride, padding).ToTensor()
        End Function

        ''' <summary>
        ''' 卷积反向（对偏置）的单精度尝试。
        ''' </summary>
        ''' <remarks>
        ''' 这是一个纯归约（沿 N / OH / OW 求和），算术强度接近 1，
        ''' 因此按"带宽受限"档的阈值判断，规模要求比卷积本体高一档。
        ''' </remarks>
        ''' <returns>降精度后的结果；不适用时返回 <c>Nothing</c>。</returns>
        Public Function TryConv2DBackwardBias(gradOutput As Tensor) As Tensor
            If Not Enabled Then Return Nothing
            If gradOutput Is Nothing OrElse gradOutput.Rank <> 4 Then Return Nothing

            Dim decision = MixedPrecisionPolicy.Decide(
                PrecisionClass.MemoryBound, CLng(gradOutput.Length), "Conv2DBackwardBias")

            If Not decision.UseSingle Then Return Nothing
            If Not Safe(gradOutput) Then Return Nothing

            Return TensorF.computeKernelF.Conv2DBackwardBias(gradOutput.ToTensorF()).ToTensor()
        End Function

        ' ---------------- 内部 ----------------

        ''' <summary>卷积族算子的浮点运算量估算：输出元素数 × 感受野</summary>
        Private Function Ops(batch As Integer, h As Integer, w As Integer, c As Integer,
                             filtH As Integer, filtW As Integer, outC As Integer) As Long
            Return CLng(batch) * h * w * c * filtH * filtW * outC
        End Function

        ''' <summary>数据是否可以安全降到单精度</summary>
        Private Function Safe(t As Tensor) As Boolean
            Return MixedPrecisionPolicy.IsSafeForSingle(t.Data)
        End Function

    End Module

End Namespace
