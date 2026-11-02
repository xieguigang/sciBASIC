' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\ONNX\OnnxOperators.vb
' /********************************************************************************/

Imports System.Linq
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX.Proto
Imports std = System.Math

Namespace ONNX

    ''' <summary>
    ''' 当模型里出现本实现尚未覆盖的算子时抛出。
    ''' </summary>
    ''' <remarks>
    ''' 异常文本里带上算子名、节点名与输入/输出名称，方便定位是模型中哪一段计算不受支持。
    ''' </remarks>
    Public Class OnnxUnsupportedOperatorException : Inherits NotSupportedException

        ''' <summary>算子类型。</summary>
        Public ReadOnly Property OpType As String

        ''' <summary>节点名称。</summary>
        Public ReadOnly Property NodeName As String

        ''' <summary>创建一个"不支持的算子"异常。</summary>
        ''' <param name="node">出错的节点。</param>
        ''' <param name="reason">补充说明。</param>
        Public Sub New(node As NodeProto, Optional reason As String = Nothing)
            MyBase.New(BuildMessage(node, reason))

            Me.OpType = If(node Is Nothing, "", node.OpType)
            Me.NodeName = If(node Is Nothing, "", node.Name)
        End Sub

        Private Shared Function BuildMessage(node As NodeProto, reason As String) As String
            If node Is Nothing Then
                Return $"不支持的 ONNX 算子：{reason}"
            End If

            Dim text As String =
                $"不支持的 ONNX 算子 ""{node.OpType}""（节点：""{node.Name}""，" &
                $"输入 [{String.Join(", ", node.Input)}] -> 输出 [{String.Join(", ", node.Output)}]）"

            If Not String.IsNullOrEmpty(reason) Then
                text &= "：" & reason
            End If

            Return text
        End Function

    End Class

    ''' <summary>
    ''' ONNX 标准算子的纯函数实现，全部以 <see cref="OnnxTensor"/>（double 数值视图）为操作数。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 这里刻意**只**实现前向计算：模型推理不需要梯度，实现面越小，出错的概率越低。
    ''' </para>
    ''' <para>
    ''' 逐元素的二元算子按照 ONNX/NumPy 的广播规则对齐尾部维度；
    ''' 卷积族算子按 ONNX 的标准 NCHW 布局计算。
    ''' </para>
    ''' </remarks>
    Public Module OnnxOperators

#Region "ELEMENT WISE"

        ''' <summary>
        ''' 逐元素相加（支持 NumPy 广播）。
        ''' </summary>
        Public Function Add(a As OnnxTensor, b As OnnxTensor) As OnnxTensor
            Return Binary(a, b, Function(x, y) x + y)
        End Function

        ''' <summary>逐元素相减（支持 NumPy 广播）。</summary>
        Public Function Subtract(a As OnnxTensor, b As OnnxTensor) As OnnxTensor
            Return Binary(a, b, Function(x, y) x - y)
        End Function

        ''' <summary>逐元素相乘（支持 NumPy 广播）。</summary>
        Public Function Multiply(a As OnnxTensor, b As OnnxTensor) As OnnxTensor
            Return Binary(a, b, Function(x, y) x * y)
        End Function

        ''' <summary>逐元素相除（支持 NumPy 广播）。</summary>
        Public Function Divide(a As OnnxTensor, b As OnnxTensor) As OnnxTensor
            Return Binary(a, b, Function(x, y) x / y)
        End Function

        ''' <summary>逐元素取幂（支持 NumPy 广播）。</summary>
        Public Function Power(a As OnnxTensor, b As OnnxTensor) As OnnxTensor
            Return Binary(a, b, Function(x, y) std.Pow(x, y))
        End Function

        ''' <summary>
        ''' 通用的逐元素二元运算。
        ''' </summary>
        ''' <param name="a">左操作数。</param>
        ''' <param name="b">右操作数。</param>
        ''' <param name="op">运算函数。</param>
        ''' <returns>广播后的结果张量。</returns>
        Public Function Binary(a As OnnxTensor, b As OnnxTensor, op As Func(Of Double, Double, Double)) As OnnxTensor
            Dim dims As Integer() = BroadcastDims(a.Dims, b.Dims)
            Dim n As Integer = GetLength(dims)
            Dim result As Double() = New Double(n - 1) {}

            Dim outStrides As Integer() = OnnxTensor.GetStrides(dims)
            Dim aStrides As Integer() = OnnxTensor.GetStrides(a.Dims)
            Dim bStrides As Integer() = OnnxTensor.GetStrides(b.Dims)
            Dim coord As Integer() = New Integer(std.Max(0, dims.Length - 1)) {}

            For i As Integer = 0 To n - 1
                Dim remain As Integer = i

                For k As Integer = 0 To dims.Length - 1
                    coord(k) = remain \ outStrides(k)
                    remain = remain Mod outStrides(k)
                Next

                Dim x As Double = a.Data(BroadcastOffset(coord, dims.Length, a.Dims, aStrides))
                Dim y As Double = b.Data(BroadcastOffset(coord, dims.Length, b.Dims, bStrides))

                result(i) = op(x, y)
            Next

            Return New OnnxTensor(dims, result)
        End Function

        ''' <summary>
        ''' 逐元素一元运算。
        ''' </summary>
        ''' <param name="a">操作数。</param>
        ''' <param name="op">运算函数。</param>
        ''' <returns>结果张量（形状不变）。</returns>
        Public Function Unary(a As OnnxTensor, op As Func(Of Double, Double)) As OnnxTensor
            Dim result As Double() = New Double(a.Length - 1) {}

            For i As Integer = 0 To result.Length - 1
                result(i) = op(a.Data(i))
            Next

            Return New OnnxTensor(a.Dims, result)
        End Function

        ''' <summary>ReLU 激活。</summary>
        Public Function Relu(a As OnnxTensor) As OnnxTensor
            Return Unary(a, Function(x) If(x > 0, x, 0.0))
        End Function

        ''' <summary>LeakyReLU 激活。</summary>
        ''' <param name="a">操作数。</param>
        ''' <param name="alpha">负半轴斜率。</param>
        Public Function LeakyRelu(a As OnnxTensor, Optional alpha As Double = 0.01) As OnnxTensor
            Return Unary(a, Function(x) If(x >= 0, x, alpha * x))
        End Function

        ''' <summary>ELU 激活。</summary>
        ''' <param name="a">操作数。</param>
        ''' <param name="alpha">负半轴缩放系数。</param>
        Public Function Elu(a As OnnxTensor, Optional alpha As Double = 1.0) As OnnxTensor
            Return Unary(a, Function(x) If(x >= 0, x, alpha * (std.Exp(x) - 1.0)))
        End Function

        ''' <summary>SELU 激活。</summary>
        ''' <param name="a">操作数。</param>
        ''' <param name="alpha">负半轴缩放系数。</param>
        ''' <param name="gamma">输出缩放系数。</param>
        Public Function Selu(a As OnnxTensor, Optional alpha As Double = 1.6732631921768188, Optional gamma As Double = 1.0507010221481323) As OnnxTensor
            Return Unary(a, Function(x) gamma * If(x > 0, x, alpha * (std.Exp(x) - 1.0)))
        End Function

        ''' <summary>Sigmoid 激活。</summary>
        Public Function Sigmoid(a As OnnxTensor) As OnnxTensor
            Return Unary(a, Function(x) 1.0 / (1.0 + std.Exp(-x)))
        End Function

        ''' <summary>HardSigmoid 激活。</summary>
        ''' <param name="a">操作数。</param>
        ''' <param name="alpha">斜率。</param>
        ''' <param name="beta">偏移。</param>
        Public Function HardSigmoid(a As OnnxTensor, Optional alpha As Double = 0.2, Optional beta As Double = 0.5) As OnnxTensor
            Return Unary(a, Function(x) std.Max(0.0, std.Min(1.0, alpha * x + beta)))
        End Function

        ''' <summary>Tanh 激活。</summary>
        Public Function Tanh(a As OnnxTensor) As OnnxTensor
            Return Unary(a, Function(x) std.Tanh(x))
        End Function

        ''' <summary>Softplus 激活。</summary>
        Public Function Softplus(a As OnnxTensor) As OnnxTensor
            Return Unary(a, Function(x) std.Log(1.0 + std.Exp(x)))
        End Function

        ''' <summary>Softsign 激活。</summary>
        Public Function Softsign(a As OnnxTensor) As OnnxTensor
            Return Unary(a, Function(x) x / (1.0 + std.Abs(x)))
        End Function

        ''' <summary>指数函数。</summary>
        Public Function Exp(a As OnnxTensor) As OnnxTensor
            Return Unary(a, Function(x) std.Exp(x))
        End Function

        ''' <summary>自然对数。</summary>
        Public Function Log(a As OnnxTensor) As OnnxTensor
            Return Unary(a, Function(x) std.Log(x))
        End Function

        ''' <summary>平方根。</summary>
        Public Function Sqrt(a As OnnxTensor) As OnnxTensor
            Return Unary(a, Function(x) std.Sqrt(x))
        End Function

        ''' <summary>取负。</summary>
        Public Function Negate(a As OnnxTensor) As OnnxTensor
            Return Unary(a, Function(x) -x)
        End Function

        ''' <summary>取绝对值。</summary>
        Public Function Abs(a As OnnxTensor) As OnnxTensor
            Return Unary(a, Function(x) std.Abs(x))
        End Function

        ''' <summary>向下取整。</summary>
        Public Function Floor(a As OnnxTensor) As OnnxTensor
            Return Unary(a, Function(x) std.Floor(x))
        End Function

        ''' <summary>向上取整。</summary>
        Public Function Ceiling(a As OnnxTensor) As OnnxTensor
            Return Unary(a, Function(x) std.Ceiling(x))
        End Function

        ''' <summary>
        ''' Softmax：沿指定轴把数值归一化为概率分布。
        ''' </summary>
        ''' <param name="a">操作数。</param>
        ''' <param name="axis">归一化轴，默认 -1（最后一维）。</param>
        ''' <returns>概率分布张量。</returns>
        Public Function Softmax(a As OnnxTensor, Optional axis As Integer = -1) As OnnxTensor
            If a.Rank = 0 Then
                Return OnnxTensor.Scalar(1.0)
            End If

            Dim ax As Integer = If(axis < 0, axis + a.Rank, axis)

            If ax < 0 OrElse ax >= a.Rank Then
                Throw New ArgumentOutOfRangeException(NameOf(axis), axis, $"Softmax 的轴超出了张量的秩 {a.Rank}")
            End If

            ' 把张量视为 [outer, width, inner] 三段：这样沿 axis 的一组元素在内存里恰好是
            ' "width 段、每段 inner 个连续值"，逐段做 max/exp 归一化即可
            Dim outer As Integer = 1
            Dim inner As Integer = 1

            For i As Integer = 0 To ax - 1
                outer *= a.Dims(i)
            Next

            For i As Integer = ax + 1 To a.Rank - 1
                inner *= a.Dims(i)
            Next

            Dim width As Integer = a.Dims(ax)
            Dim result As Double() = New Double(a.Length - 1) {}
            Dim buffer As Double() = New Double(width - 1) {}

            For o As Integer = 0 To outer - 1
                For j As Integer = 0 To inner - 1
                    Dim offset As Integer = o * width * inner + j

                    For k As Integer = 0 To width - 1
                        buffer(k) = a.Data(offset + k * inner)
                    Next

                    Dim max As Double = buffer.Max()
                    Dim sum As Double = 0.0

                    For k As Integer = 0 To width - 1
                        buffer(k) = std.Exp(buffer(k) - max)
                        sum += buffer(k)
                    Next

                    For k As Integer = 0 To width - 1
                        result(offset + k * inner) = buffer(k) / sum
                    Next
                Next
            Next

            Return New OnnxTensor(a.Dims, result)
        End Function

#End Region

#Region "MATRIX"

        ''' <summary>
        ''' 矩阵乘法；支持 2 维与 1 维操作数（1 维按行/列向量处理）。
        ''' </summary>
        ''' <param name="a">左矩阵。</param>
        ''' <param name="b">右矩阵。</param>
        ''' <returns>乘积张量。</returns>
        Public Function MatMul(a As OnnxTensor, b As OnnxTensor) As OnnxTensor
            ' 1 维操作数按 ONNX 规则提升为 (1, n) 或 (n, 1)
            Dim aDims As Integer() = If(a.Rank = 1, New Integer() {1, a.Length}, a.Dims)
            Dim bDims As Integer() = If(b.Rank = 1, New Integer() {b.Length, 1}, b.Dims)

            If aDims.Length <> 2 OrElse bDims.Length <> 2 Then
                Throw New InvalidOperationException(
                    $"只支持 1~2 维的矩阵乘法，实际形状为 [{String.Join(", ", a.Dims)}] x [{String.Join(", ", b.Dims)}]")
            End If

            Dim m As Integer = aDims(0)
            Dim k As Integer = aDims(1)
            Dim n As Integer = bDims(1)

            If k <> bDims(0) Then
                Throw New InvalidOperationException(
                    $"MatMul 的形状不匹配：[{String.Join(", ", a.Dims)}] x [{String.Join(", ", b.Dims)}]")
            End If

            Dim result As Double() = New Double(m * n - 1) {}

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    Dim acc As Double = 0.0

                    For t As Integer = 0 To k - 1
                        acc += a.Data(i * k + t) * b.Data(t * n + j)
                    Next

                    result(i * n + j) = acc
                Next
            Next

            Dim dims As Integer()

            If a.Rank = 1 AndAlso b.Rank = 1 Then
                dims = New Integer() {1, 1}
            ElseIf a.Rank = 1 Then
                dims = New Integer() {n}
            ElseIf b.Rank = 1 Then
                dims = New Integer() {m}
            Else
                dims = New Integer() {m, n}
            End If

            Return New OnnxTensor(dims, result)
        End Function

        ''' <summary>
        ''' Gemm：<c>Y = alpha * A' * B' + beta * C</c>。
        ''' </summary>
        ''' <param name="a">左矩阵。</param>
        ''' <param name="b">右矩阵。</param>
        ''' <param name="c">可选的偏置矩阵。</param>
        ''' <param name="alpha">乘积系数。</param>
        ''' <param name="beta">偏置系数。</param>
        ''' <param name="transA">是否转置 A。</param>
        ''' <param name="transB">是否转置 B。</param>
        ''' <returns>结果张量。</returns>
        Public Function Gemm(a As OnnxTensor, b As OnnxTensor, Optional c As OnnxTensor = Nothing,
                             Optional alpha As Double = 1.0, Optional beta As Double = 1.0,
                             Optional transA As Boolean = False, Optional transB As Boolean = False) As OnnxTensor

            Dim left As OnnxTensor = If(transA, Transpose2D(a), a)
            Dim right As OnnxTensor = If(transB, Transpose2D(b), b)
            Dim product As OnnxTensor = MatMul(left, right)

            If alpha <> 1.0 Then
                For i As Integer = 0 To product.Data.Length - 1
                    product.Data(i) *= alpha
                Next
            End If

            If c IsNot Nothing AndAlso beta <> 0.0 Then
                Dim bias As OnnxTensor = c

                If beta <> 1.0 Then
                    bias = Unary(c, Function(x) x * beta)
                End If

                Return Add(product, bias)
            End If

            Return product
        End Function

        ''' <summary>对 2 维张量做转置。</summary>
        Private Function Transpose2D(t As OnnxTensor) As OnnxTensor
            If t.Rank <> 2 Then
                Throw New InvalidOperationException($"Gemm 的 transA/transB 只支持 2 维张量，实际为 {t.Rank} 维")
            End If

            Return t.SwapAxis(0, 1)
        End Function

#End Region

#Region "CONVOLUTION"

        ''' <summary>
        ''' 2 维卷积（NCHW 布局，支持 stride / padding / dilation / group）。
        ''' </summary>
        ''' <param name="input">输入，形状 <c>[N, C, H, W]</c>。</param>
        ''' <param name="filters">卷积核，形状 <c>[M, C/group, kH, kW]</c>。</param>
        ''' <param name="bias">偏置，形状 <c>[M]</c>；可为 Nothing。</param>
        ''' <param name="strides">步长 <c>[sH, sW]</c>。</param>
        ''' <param name="pads">填充 <c>[top, left, bottom, right]</c>；可为 Nothing。</param>
        ''' <param name="dilations">膨胀 <c>[dH, dW]</c>；可为 Nothing。</param>
        ''' <param name="group">分组数。</param>
        ''' <returns>输出，形状 <c>[N, M, outH, outW]</c>。</returns>
        Public Function Conv(input As OnnxTensor, filters As OnnxTensor, Optional bias As OnnxTensor = Nothing,
                             Optional strides As Integer() = Nothing, Optional pads As Integer() = Nothing,
                             Optional dilations As Integer() = Nothing, Optional group As Integer = 1) As OnnxTensor

            If input.Rank <> 4 OrElse filters.Rank <> 4 Then
                Throw New InvalidOperationException(
                    $"Conv 只支持 4 维张量（NCHW），实际输入为 [{String.Join(", ", input.Dims)}]，卷积核为 [{String.Join(", ", filters.Dims)}]")
            End If

            Dim sH As Integer = If(strides Is Nothing OrElse strides.Length < 2, 1, strides(0))
            Dim sW As Integer = If(strides Is Nothing OrElse strides.Length < 2, 1, strides(1))
            Dim pH As Integer = If(pads Is Nothing OrElse pads.Length < 4, 0, pads(0))
            Dim pW As Integer = If(pads Is Nothing OrElse pads.Length < 4, 0, pads(1))
            Dim pbH As Integer = If(pads Is Nothing OrElse pads.Length < 4, 0, pads(2))
            Dim pbW As Integer = If(pads Is Nothing OrElse pads.Length < 4, 0, pads(3))
            Dim dH As Integer = If(dilations Is Nothing OrElse dilations.Length < 2, 1, dilations(0))
            Dim dW As Integer = If(dilations Is Nothing OrElse dilations.Length < 2, 1, dilations(1))

            Dim batchSize As Integer = input.Dims(0)
            Dim channels As Integer = input.Dims(1)
            Dim inH As Integer = input.Dims(2)
            Dim inW As Integer = input.Dims(3)

            Dim outChannels As Integer = filters.Dims(0)
            Dim groupChannels As Integer = filters.Dims(1)
            Dim kH As Integer = filters.Dims(2)
            Dim kW As Integer = filters.Dims(3)

            If group < 1 Then group = 1
            If channels <> groupChannels * group Then
                Throw New InvalidOperationException(
                    $"Conv 的输入通道数 {channels} 与卷积核 [{String.Join(", ", filters.Dims)}]（group={group}）不匹配")
            End If

            Dim outH As Integer = (inH + pH + pbH - ((kH - 1) * dH + 1)) \ sH + 1
            Dim outW As Integer = (inW + pW + pbW - ((kW - 1) * dW + 1)) \ sW + 1

            If outH <= 0 OrElse outW <= 0 Then
                Throw New InvalidOperationException(
                    $"Conv 的输出尺寸非法：outH={outH}, outW={outW}（输入 {inH}x{inW}，卷积核 {kH}x{kW}，步长 {sH}x{sW}）")
            End If

            Dim outChannelsPerGroup As Integer = outChannels \ group
            Dim result As Double() = New Double(batchSize * outChannels * outH * outW - 1) {}
            Dim inputData As Double() = input.Data
            Dim filterData As Double() = filters.Data

            For n As Integer = 0 To batchSize - 1
                For m As Integer = 0 To outChannels - 1
                    Dim g As Integer = m \ outChannelsPerGroup
                    Dim cStart As Integer = g * groupChannels

                    For oy As Integer = 0 To outH - 1
                        For ox As Integer = 0 To outW - 1
                            Dim acc As Double = If(bias Is Nothing, 0.0, bias.Data(m))

                            For c As Integer = 0 To groupChannels - 1
                                For ky As Integer = 0 To kH - 1
                                    Dim iy As Integer = oy * sH - pH + ky * dH

                                    If iy < 0 OrElse iy >= inH Then
                                        Continue For
                                    End If

                                    For kx As Integer = 0 To kW - 1
                                        Dim ix As Integer = ox * sW - pW + kx * dW

                                        If ix < 0 OrElse ix >= inW Then
                                            Continue For
                                        End If

                                        Dim xv As Double = inputData(((n * channels + cStart + c) * inH + iy) * inW + ix)
                                        Dim wv As Double = filterData(((m * groupChannels + c) * kH + ky) * kW + kx)

                                        acc += xv * wv
                                    Next
                                Next
                            Next

                            result(((n * outChannels + m) * outH + oy) * outW + ox) = acc
                        Next
                    Next
                Next
            Next

            Return New OnnxTensor(New Integer() {batchSize, outChannels, outH, outW}, result)
        End Function

        ''' <summary>
        ''' 2 维转置卷积（NCHW 布局）：把较小的特征图"散射"回较大的空间尺寸。
        ''' </summary>
        ''' <param name="x">输入，形状 <c>[N, C, H, W]</c>。</param>
        ''' <param name="w">卷积核，形状 <c>[C, M, kH, kW]</c>（注意与 Conv 的通道顺序相反）。</param>
        ''' <param name="b">偏置，形状 <c>[M]</c>；可为 Nothing。</param>
        ''' <param name="strides">步长 <c>[sH, sW]</c>。</param>
        ''' <param name="pads">填充 <c>[top, left, bottom, right]</c>。</param>
        ''' <returns>输出，形状 <c>[N, M, outH, outW]</c>，其中 <c>out = stride*(in-1) + kernel - 2*pad</c>。</returns>
        Public Function ConvTranspose(x As OnnxTensor, w As OnnxTensor, Optional b As OnnxTensor = Nothing,
                                      Optional strides As Integer() = Nothing,
                                      Optional pads As Integer() = Nothing) As OnnxTensor

            If x.Rank <> 4 OrElse w.Rank <> 4 Then
                Throw New InvalidOperationException(
                    $"ConvTranspose 只支持 4 维张量（NCHW），实际输入为 [{String.Join(", ", x.Dims)}]，卷积核为 [{String.Join(", ", w.Dims)}]")
            End If

            Dim sH As Integer = If(strides Is Nothing OrElse strides.Length < 2, 1, strides(0))
            Dim sW As Integer = If(strides Is Nothing OrElse strides.Length < 2, 1, strides(1))
            Dim pH As Integer = If(pads Is Nothing OrElse pads.Length < 4, 0, pads(0))
            Dim pW As Integer = If(pads Is Nothing OrElse pads.Length < 4, 0, pads(1))

            Dim batchSize As Integer = x.Dims(0)
            Dim channels As Integer = x.Dims(1)
            Dim inH As Integer = x.Dims(2)
            Dim inW As Integer = x.Dims(3)

            Dim outChannels As Integer = w.Dims(1)
            Dim kH As Integer = w.Dims(2)
            Dim kW As Integer = w.Dims(3)

            If w.Dims(0) <> channels Then
                Throw New InvalidOperationException(
                    $"ConvTranspose 的输入通道数 {channels} 与卷积核 [{String.Join(", ", w.Dims)}] 不匹配")
            End If

            Dim outH As Integer = sH * (inH - 1) + kH - 2 * pH
            Dim outW As Integer = sW * (inW - 1) + kW - 2 * pW
            Dim result As Double() = New Double(batchSize * outChannels * outH * outW - 1) {}

            For n As Integer = 0 To batchSize - 1
                For m As Integer = 0 To outChannels - 1
                    For oy As Integer = 0 To outH - 1
                        For ox As Integer = 0 To outW - 1
                            Dim acc As Double = If(b Is Nothing, 0.0, b.Data(m))

                            For ky As Integer = 0 To kH - 1
                                Dim numerator As Integer = oy + pH - ky

                                If numerator < 0 OrElse numerator Mod sH <> 0 Then
                                    Continue For
                                End If

                                Dim iy As Integer = numerator \ sH

                                If iy >= inH Then
                                    Continue For
                                End If

                                For kx As Integer = 0 To kW - 1
                                    Dim numeratorX As Integer = ox + pW - kx

                                    If numeratorX < 0 OrElse numeratorX Mod sW <> 0 Then
                                        Continue For
                                    End If

                                    Dim ix As Integer = numeratorX \ sW

                                    If ix >= inW Then
                                        Continue For
                                    End If

                                    For c As Integer = 0 To channels - 1
                                        acc += x.Data(((n * channels + c) * inH + iy) * inW + ix) *
                                               w.Data(((c * outChannels + m) * kH + ky) * kW + kx)
                                    Next
                                Next
                            Next

                            result(((n * outChannels + m) * outH + oy) * outW + ox) = acc
                        Next
                    Next
                Next
            Next

            Return New OnnxTensor(New Integer() {batchSize, outChannels, outH, outW}, result)
        End Function

        ''' <summary>
        ''' 2 维最大池化（NCHW 布局）。
        ''' </summary>
        ''' <param name="x">输入，形状 <c>[N, C, H, W]</c>。</param>
        ''' <param name="kernel">池化窗 <c>[kH, kW]</c>。</param>
        ''' <param name="strides">步长 <c>[sH, sW]</c>。</param>
        ''' <param name="pads">填充 <c>[top, left, bottom, right]</c>。</param>
        ''' <returns>输出，形状 <c>[N, C, outH, outW]</c>。</returns>
        Public Function MaxPool(x As OnnxTensor, kernel As Integer(), Optional strides As Integer() = Nothing,
                                Optional pads As Integer() = Nothing) As OnnxTensor

            Return Pool(x, kernel, strides, pads, Function(current, candidate) If(candidate > current, candidate, current), Double.MinValue)
        End Function

        ''' <summary>
        ''' 2 维平均池化（NCHW 布局，padding 位置按 0 计入分母，与 ONNX 默认的 <c>count_include_pad=0</c> 一致）。
        ''' </summary>
        ''' <param name="x">输入，形状 <c>[N, C, H, W]</c>。</param>
        ''' <param name="kernel">池化窗 <c>[kH, kW]</c>。</param>
        ''' <param name="strides">步长 <c>[sH, sW]</c>。</param>
        ''' <param name="pads">填充 <c>[top, left, bottom, right]</c>。</param>
        ''' <returns>输出，形状 <c>[N, C, outH, outW]</c>。</returns>
        Public Function AveragePool(x As OnnxTensor, kernel As Integer(), Optional strides As Integer() = Nothing,
                                    Optional pads As Integer() = Nothing) As OnnxTensor

            Return Pool(x, kernel, strides, pads, Nothing, 0.0)
        End Function

        Private Function Pool(x As OnnxTensor, kernel As Integer(), strides As Integer(), pads As Integer(),
                              pick As Func(Of Double, Double, Double), seed As Double) As OnnxTensor

            If x.Rank <> 4 Then
                Throw New InvalidOperationException(
                    $"池化算子只支持 4 维张量（NCHW），实际为 [{String.Join(", ", x.Dims)}]")
            End If

            Dim kH As Integer = kernel(0)
            Dim kW As Integer = If(kernel.Length > 1, kernel(1), kernel(0))
            Dim sH As Integer = If(strides Is Nothing OrElse strides.Length < 2, kH, strides(0))
            Dim sW As Integer = If(strides Is Nothing OrElse strides.Length < 2, kW, strides(1))
            Dim pH As Integer = If(pads Is Nothing OrElse pads.Length < 4, 0, pads(0))
            Dim pW As Integer = If(pads Is Nothing OrElse pads.Length < 4, 0, pads(1))
            Dim pbH As Integer = If(pads Is Nothing OrElse pads.Length < 4, 0, pads(2))
            Dim pbW As Integer = If(pads Is Nothing OrElse pads.Length < 4, 0, pads(3))

            Dim batchSize As Integer = x.Dims(0)
            Dim channels As Integer = x.Dims(1)
            Dim inH As Integer = x.Dims(2)
            Dim inW As Integer = x.Dims(3)
            Dim outH As Integer = (inH + pH + pbH - kH) \ sH + 1
            Dim outW As Integer = (inW + pW + pbW - kW) \ sW + 1

            Dim result As Double() = New Double(batchSize * channels * outH * outW - 1) {}
            Dim data As Double() = x.Data

            For n As Integer = 0 To batchSize - 1
                For c As Integer = 0 To channels - 1
                    For oy As Integer = 0 To outH - 1
                        For ox As Integer = 0 To outW - 1
                            Dim acc As Double = seed
                            Dim count As Integer = 0

                            For ky As Integer = 0 To kH - 1
                                Dim iy As Integer = oy * sH - pH + ky

                                For kx As Integer = 0 To kW - 1
                                    Dim ix As Integer = ox * sW - pW + kx

                                    If iy < 0 OrElse iy >= inH OrElse ix < 0 OrElse ix >= inW Then
                                        Continue For
                                    End If

                                    Dim v As Double = data(((n * channels + c) * inH + iy) * inW + ix)

                                    If pick Is Nothing Then
                                        acc += v
                                    Else
                                        acc = pick(acc, v)
                                    End If

                                    count += 1
                                Next
                            Next

                            If pick Is Nothing Then
                                acc = If(count = 0, 0.0, acc / count)
                            End If

                            result(((n * channels + c) * outH + oy) * outW + ox) = acc
                        Next
                    Next
                Next
            Next

            Return New OnnxTensor(New Integer() {batchSize, channels, outH, outW}, result)
        End Function

        ''' <summary>
        ''' 全局平均池化：把 <c>[N, C, H, W]</c> 变成 <c>[N, C, 1, 1]</c>。
        ''' </summary>
        Public Function GlobalAveragePool(x As OnnxTensor) As OnnxTensor
            If x.Rank <> 4 Then
                Throw New InvalidOperationException($"GlobalAveragePool 只支持 4 维张量，实际为 [{String.Join(", ", x.Dims)}]")
            End If

            Dim batchSize As Integer = x.Dims(0)
            Dim channels As Integer = x.Dims(1)
            Dim inH As Integer = x.Dims(2)
            Dim inW As Integer = x.Dims(3)
            Dim result As Double() = New Double(batchSize * channels - 1) {}

            For n As Integer = 0 To batchSize - 1
                For c As Integer = 0 To channels - 1
                    Dim acc As Double = 0.0

                    For iy As Integer = 0 To inH - 1
                        For ix As Integer = 0 To inW - 1
                            acc += x.Data(((n * channels + c) * inH + iy) * inW + ix)
                        Next
                    Next

                    result(n * channels + c) = acc / (inH * inW)
                Next
            Next

            Return New OnnxTensor(New Integer() {batchSize, channels, 1, 1}, result)
        End Function

        ''' <summary>
        ''' 局部响应归一化（ONNX <c>LRN</c>）：沿通道方向做横向抑制。
        ''' </summary>
        ''' <param name="x">输入，形状 <c>[N, C, ...]</c>。</param>
        ''' <param name="size">参与归一化的邻域大小。</param>
        ''' <param name="alpha">缩放系数。</param>
        ''' <param name="beta">指数。</param>
        ''' <param name="bias">常数项。</param>
        Public Function LRN(x As OnnxTensor, Optional size As Integer = 5,
                            Optional alpha As Double = 0.0001, Optional beta As Double = 0.75,
                            Optional bias As Double = 1.0) As OnnxTensor

            If x.Rank < 2 Then
                Throw New InvalidOperationException("LRN 至少需要 2 维张量（N, C, ...）")
            End If

            Dim batchSize As Integer = x.Dims(0)
            Dim channels As Integer = x.Dims(1)
            Dim spatial As Integer = x.Length \ (batchSize * channels)
            Dim result As Double() = New Double(x.Length - 1) {}
            Dim halfWindow As Integer = (size - 1) \ 2

            For n As Integer = 0 To batchSize - 1
                For c As Integer = 0 To channels - 1
                    Dim start As Integer = std.Max(0, c - halfWindow)
                    Dim [end] As Integer = std.Min(channels - 1, c + halfWindow)

                    For s As Integer = 0 To spatial - 1
                        Dim square As Double = 0.0

                        For k As Integer = start To [end]
                            Dim v As Double = x.Data((n * channels + k) * spatial + s)

                            square += v * v
                        Next

                        Dim index As Integer = (n * channels + c) * spatial + s
                        Dim scale As Double = std.Pow(bias + alpha / size * square, -beta)

                        result(index) = x.Data(index) * scale
                    Next
                Next
            Next

            Return New OnnxTensor(x.Dims, result)
        End Function

#End Region

#Region "SHAPE"

        ''' <summary>
        ''' Flatten：把 <paramref name="axis"/> 之前与之后的维度分别合并成两个维度。
        ''' </summary>
        ''' <param name="x">输入张量。</param>
        ''' <param name="axis">分割轴，默认 1。</param>
        Public Function Flatten(x As OnnxTensor, Optional axis As Integer = 1) As OnnxTensor
            Dim ax As Integer = If(axis < 0, axis + x.Rank, axis)

            If x.Rank = 0 Then
                Return New OnnxTensor(New Integer() {1, 1}, x.Data.Clone())
            End If

            Dim outer As Integer = 1
            Dim inner As Integer = 1

            For i As Integer = 0 To ax - 1
                outer *= x.Dims(i)
            Next

            For i As Integer = ax To x.Rank - 1
                inner *= x.Dims(i)
            Next

            Return New OnnxTensor(New Integer() {outer, inner}, x.Data)
        End Function

        ''' <summary>
        ''' Concat：沿指定轴拼接多个张量。
        ''' </summary>
        ''' <param name="axis">拼接轴（支持负值）。</param>
        ''' <param name="items">待拼接的张量。</param>
        Public Function Concat(axis As Integer, ParamArray items As OnnxTensor()) As OnnxTensor
            If items Is Nothing OrElse items.Length = 0 Then
                Throw New ArgumentException("Concat 至少需要一个输入张量", NameOf(items))
            End If

            Dim rank As Integer = items(0).Rank
            Dim ax As Integer = If(axis < 0, axis + rank, axis)

            If rank = 0 Then
                Throw New InvalidOperationException("Concat 不支持 0 维张量")
            End If

            Dim dims As Integer() = items(0).Dims.Clone()
            Dim totalAxis As Integer = 0

            For Each t As OnnxTensor In items
                If t.Rank <> rank Then
                    Throw New InvalidOperationException("Concat 的所有输入必须具有相同的秩")
                End If

                For k As Integer = 0 To rank - 1
                    If k <> ax AndAlso t.Dims(k) <> dims(k) Then
                        Throw New InvalidOperationException("Concat 的输入只在拼接轴上允许维度不同")
                    End If
                Next

                totalAxis += t.Dims(ax)
            Next

            dims(ax) = totalAxis

            Dim inner As Integer = 1

            For k As Integer = ax + 1 To rank - 1
                inner *= dims(k)
            Next

            Dim outer As Integer = 1

            For k As Integer = 0 To ax - 1
                outer *= dims(k)
            Next

            Dim result As Double() = New Double(GetLength(dims) - 1) {}
            Dim axisSize As Integer = totalAxis

            For o As Integer = 0 To outer - 1
                Dim cursor As Integer = 0

                For Each t As OnnxTensor In items
                    Dim block As Integer = t.Dims(ax) * inner
                    Dim srcStart As Integer = o * block
                    Dim dstStart As Integer = o * axisSize * inner + cursor

                    Call Array.Copy(t.Data, srcStart, result, dstStart, block)

                    cursor += block
                Next
            Next

            Return New OnnxTensor(dims, result)
        End Function

        ''' <summary>
        ''' Reshape：按 ONNX 的规则（0 沿用输入维度、-1 自动推断）重排形状。
        ''' </summary>
        ''' <param name="x">输入张量。</param>
        ''' <param name="shape">目标形状。</param>
        Public Function Reshape(x As OnnxTensor, shape As Integer()) As OnnxTensor
            Return x.Reshape(shape)
        End Function

        ''' <summary>
        ''' Squeeze：删除长度为 1 的维度。
        ''' </summary>
        ''' <param name="x">输入张量。</param>
        ''' <param name="axes">待删除的维度；为空时删除所有长度为 1 的维度。</param>
        Public Function Squeeze(x As OnnxTensor, Optional axes As Integer() = Nothing) As OnnxTensor
            Dim keep As New List(Of Integer)

            If axes Is Nothing OrElse axes.Length = 0 Then
                For i As Integer = 0 To x.Rank - 1
                    If x.Dims(i) <> 1 Then
                        Call keep.Add(x.Dims(i))
                    End If
                Next
            Else
                Dim removed As New HashSet(Of Integer)(axes.Select(Function(a) If(a < 0, a + x.Rank, a)))

                For i As Integer = 0 To x.Rank - 1
                    If Not removed.Contains(i) Then
                        Call keep.Add(x.Dims(i))
                    End If
                Next
            End If

            Return New OnnxTensor(keep.ToArray(), x.Data)
        End Function

        ''' <summary>
        ''' Unsqueeze：在指定位置插入长度为 1 的维度。
        ''' </summary>
        ''' <param name="x">输入张量。</param>
        ''' <param name="axes">插入位置。</param>
        Public Function Unsqueeze(x As OnnxTensor, axes As Integer()) As OnnxTensor
            Dim outRank As Integer = x.Rank + axes.Length
            Dim dims As New List(Of Integer)(x.Dims)
            Dim normalized As Integer() = axes _
                .Select(Function(a) If(a < 0, a + outRank, a)) _
                .OrderBy(Function(a) a) _
                .ToArray()

            For Each ax As Integer In normalized
                Call dims.Insert(ax, 1)
            Next

            Return New OnnxTensor(dims.ToArray(), x.Data)
        End Function

        ''' <summary>
        ''' Shape：返回输入张量的形状（数值以 double 表示）。
        ''' </summary>
        Public Function Shape(x As OnnxTensor) As OnnxTensor
            Dim result As Double() = New Double(x.Rank - 1) {}

            For i As Integer = 0 To x.Rank - 1
                result(i) = x.Dims(i)
            Next

            Return New OnnxTensor(New Integer() {x.Rank}, result)
        End Function

        ''' <summary>
        ''' ReduceMean / ReduceSum 之类的规约运算。
        ''' </summary>
        ''' <param name="x">输入张量。</param>
        ''' <param name="axes">规约轴；Nothing 表示全部轴。</param>
        ''' <param name="keepDims">是否保留被规约的维度（长度置 1）。</param>
        ''' <param name="mean">True 求均值，False 求和。</param>
        Public Function Reduce(x As OnnxTensor, axes As Integer(), Optional keepDims As Boolean = True, Optional mean As Boolean = True) As OnnxTensor
            Dim normalized As Integer()

            If axes Is Nothing OrElse axes.Length = 0 Then
                normalized = Enumerable.Range(0, x.Rank).ToArray()
            Else
                normalized = axes.Select(Function(a) If(a < 0, a + x.Rank, a)).ToArray()
            End If

            Dim targets As New HashSet(Of Integer)(normalized)
            Dim outDims As New List(Of Integer)

            For i As Integer = 0 To x.Rank - 1
                If targets.Contains(i) Then
                    If keepDims Then
                        Call outDims.Add(1)
                    End If
                Else
                    Call outDims.Add(x.Dims(i))
                End If
            Next

            Dim n As Integer = GetLength(outDims.ToArray())
            Dim result As Double() = New Double(n - 1) {}

            Dim strides As Integer() = OnnxTensor.GetStrides(x.Dims)
            Dim outStrides As Integer() = OnnxTensor.GetStrides(outDims.ToArray())
            Dim coord As Integer() = New Integer(std.Max(0, x.Rank - 1)) {}
            Dim count As Integer() = New Integer(n - 1) {}

            For i As Integer = 0 To x.Length - 1
                Dim remain As Integer = i

                For k As Integer = 0 To x.Rank - 1
                    coord(k) = remain \ strides(k)
                    remain = remain Mod strides(k)
                Next

                Dim outIndex As Integer = 0
                Dim cursor As Integer = 0

                For k As Integer = 0 To x.Rank - 1
                    If Not targets.Contains(k) Then
                        outIndex += coord(k) * outStrides(cursor)
                        cursor += 1
                    End If
                Next

                result(outIndex) += x.Data(i)
                count(outIndex) += 1
            Next

            If mean Then
                For i As Integer = 0 To n - 1
                    If count(i) > 0 Then
                        result(i) /= count(i)
                    End If
                Next
            End If

            Return New OnnxTensor(outDims.ToArray(), result)
        End Function

        ''' <summary>
        ''' Gather：沿指定轴按索引取值。
        ''' </summary>
        ''' <param name="data">源张量。</param>
        ''' <param name="indices">索引（double 视图，内部取整）。</param>
        ''' <param name="axis">取值轴。</param>
        Public Function Gather(data As OnnxTensor, indices As OnnxTensor, Optional axis As Integer = 0) As OnnxTensor
            Dim ax As Integer = If(axis < 0, axis + data.Rank, axis)

            If ax < 0 OrElse ax >= data.Rank Then
                Throw New ArgumentOutOfRangeException(NameOf(axis), axis, "Gather 的轴超出张量秩")
            End If

            Dim outer As Integer = 1

            For i As Integer = 0 To ax - 1
                outer *= data.Dims(i)
            Next

            Dim width As Integer = data.Dims(ax)
            Dim inner As Integer = 1

            For i As Integer = ax + 1 To data.Rank - 1
                inner *= data.Dims(i)
            Next

            Dim nIndices As Integer = indices.Length
            Dim dims As New List(Of Integer)

            For i As Integer = 0 To ax - 1
                Call dims.Add(data.Dims(i))
            Next

            For Each d As Integer In indices.Dims
                Call dims.Add(d)
            Next

            For i As Integer = ax + 1 To data.Rank - 1
                Call dims.Add(data.Dims(i))
            Next

            Dim result As Double() = New Double(outer * nIndices * inner - 1) {}

            For o As Integer = 0 To outer - 1
                For i As Integer = 0 To nIndices - 1
                    Dim idx As Integer = CInt(indices.Data(i))

                    If idx < 0 Then
                        idx += width
                    End If

                    If idx < 0 OrElse idx >= width Then
                        Throw New IndexOutOfRangeException($"Gather 的索引 {idx} 超出轴 {ax} 的范围（0..{width - 1}）")
                    End If

                    For j As Integer = 0 To inner - 1
                        result((o * nIndices + i) * inner + j) = data.Data((o * width + idx) * inner + j)
                    Next
                Next
            Next

            Return New OnnxTensor(dims.ToArray(), result)
        End Function

#End Region

#Region "BROADCAST HELPERS"

        ''' <summary>
        ''' 计算两个形状按 NumPy 规则广播后的结果形状。
        ''' </summary>
        ''' <param name="a">形状 A。</param>
        ''' <param name="b">形状 B。</param>
        ''' <returns>广播后的形状。</returns>
        Public Function BroadcastDims(a As Integer(), b As Integer()) As Integer()
            Dim rank As Integer = std.Max(a.Length, b.Length)
            Dim result As Integer() = New Integer(rank - 1) {}

            For i As Integer = 1 To rank
                Dim da As Integer = If(a.Length - i >= 0, a(a.Length - i), 1)
                Dim db As Integer = If(b.Length - i >= 0, b(b.Length - i), 1)

                If da <> db AndAlso da <> 1 AndAlso db <> 1 Then
                    Throw New InvalidOperationException(
                        $"无法广播形状 [{String.Join(", ", a)}] 与 [{String.Join(", ", b)}]")
                End If

                result(rank - i) = std.Max(da, db)
            Next

            Return result
        End Function

        Private Function BroadcastOffset(coord As Integer(), coordRank As Integer,
                                         dims As Integer(), strides As Integer()) As Integer
            Dim offset As Integer = 0
            Dim shift As Integer = coordRank - dims.Length

            For i As Integer = 0 To dims.Length - 1
                If dims(i) > 1 Then
                    offset += coord(shift + i) * strides(i)
                End If
            Next

            Return offset
        End Function

        ''' <summary>
        ''' 计算形状的元素个数。
        ''' </summary>
        ''' <param name="dims">形状。</param>
        ''' <returns>元素个数（0 维时为 1）。</returns>
        Public Function GetLength(dims As Integer()) As Integer
            If dims Is Nothing OrElse dims.Length = 0 Then
                Return 1
            End If

            Dim n As Integer = 1

            For Each d As Integer In dims
                n *= d
            Next

            Return n
        End Function

#End Region

    End Module

End Namespace
