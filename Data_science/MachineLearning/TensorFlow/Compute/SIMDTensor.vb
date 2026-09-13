#Region "Microsoft.VisualBasic::6fba187cd5a3336de9139d7de4e7a95b, Data_science\MachineLearning\TensorFlow\Compute\SIMDTensor.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 227
    '    Code Lines: 149 (65.64%)
    ' Comment Lines: 24 (10.57%)
    '    - Xml Docs: 45.83%
    ' 
    '   Blank Lines: 54 (23.79%)
    '     File Size: 8.78 KB


    '     Class SIMDTensor
    ' 
    '         Properties: [Default], Name
    ' 
    '         Function: Abs, Add, AddScalar, Clip, Divide
    '                   DivideScalar, Exp, L2Norm, Log, MatMul
    '                   Max, Maximum, Mean, MeanAll, Min
    '                   Minimum, Multiply, MultiplyScalar, Negate, Pow
    '                   Reciprocal, Sqrt, Square, StdDev, Subtract
    '                   Sum, SumAll, ToJagged
    ' 
    '         Sub: Register
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' 默认计算后端：基于 System.Numerics.Vector 的 SIMD 加速 CPU 实现。
'
' 这里只重写“确实有向量指令收益”的热点算子（逐元素算术/数学函数、整体归约、
' 矩阵乘），其余算子在 <see cref="TensorComputeBase"/> 中已经有正确的标量实现，
' 直接继承即可。
'
' 所有实现都复用 Microsoft.VisualBasic.Core 中的 Math.SIMD 基础库：
'   SimdEngine  —— 逐元素算术
'   SimdMath    —— 逐元素数学函数
'   SimdReduce  —— 单线程归约
'   SimdParallel—— 分块并行 + 块内向量化
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Math.SIMD
Imports nv = System.Numerics
Imports std = System.Math
Imports tpl = System.Threading.Tasks

Namespace Compute

    ''' <summary>
    ''' 默认的 SIMD 加速 CPU 计算后端。
    ''' </summary>
    Public Class SIMDTensor
        Inherits TensorComputeBase

        ''' <summary>
        ''' 矩阵乘走到 SIMD 行列点积路径的最小规模（m*k*n）。
        ''' 低于该规模时构建交错数组的开销会超过收益，直接退回标量实现。
        ''' </summary>
        Public Const MatrixDotThreshold As Integer = 10000

        ''' <summary>全局默认实例（<see cref="Tensor.computeKernel"/> 的初始值）</summary>
        Public Shared ReadOnly Property [Default] As SIMDTensor = New SIMDTensor()

        Public Overrides ReadOnly Property Name As String = "SIMD"

        ''' <summary>
        ''' 把 <see cref="Tensor.computeKernel"/> 切换为 SIMD 加速的 CPU 实现。
        ''' </summary>
        Public Shared Sub Register()
            SyncLock Tensor.SyncRoot
                Tensor.computeKernel = [Default]
            End SyncLock
        End Sub

#Region "逐元素 - 二元"

        Public Overrides Function Add(a As Tensor, b As Tensor) As Tensor
            RequireSameShape(a, b, "相加")
            Return Wrap(SimdParallel.Add(a.Data, b.Data), a.Shape)
        End Function

        Public Overrides Function Subtract(a As Tensor, b As Tensor) As Tensor
            RequireSameShape(a, b, "相减")
            Return Wrap(SimdParallel.Subtract(a.Data, b.Data), a.Shape)
        End Function

        Public Overrides Function Multiply(a As Tensor, b As Tensor) As Tensor
            RequireSameShape(a, b, "相乘")
            Return Wrap(SimdParallel.Multiply(a.Data, b.Data), a.Shape)
        End Function

        Public Overrides Function Divide(a As Tensor, b As Tensor) As Tensor
            RequireSameShape(a, b, "相除")
            Return Wrap(SimdEngine.Divide(a.Data, b.Data), a.Shape)
        End Function

        Public Overrides Function Maximum(a As Tensor, b As Tensor) As Tensor
            RequireSameShape(a, b, "取最大值")
            Return Wrap(SimdEngine.Max(Of Double)(a.Data, b.Data), a.Shape)
        End Function

        Public Overrides Function Minimum(a As Tensor, b As Tensor) As Tensor
            RequireSameShape(a, b, "取最小值")
            Return Wrap(SimdEngine.Min(Of Double)(a.Data, b.Data), a.Shape)
        End Function

#End Region

#Region "逐元素 - 一元"

        Public Overrides Function Exp(t As Tensor) As Tensor
            Return Wrap(SimdMath.Exp(t.Data), t.Shape)
        End Function

        Public Overrides Function Log(t As Tensor) As Tensor
            Return Wrap(SimdMath.Log(t.Data), t.Shape)
        End Function

        Public Overrides Function Sqrt(t As Tensor) As Tensor
            Return Wrap(SimdMath.Sqrt(t.Data), t.Shape)
        End Function

        Public Overrides Function Square(t As Tensor) As Tensor
            Return Wrap(SimdMath.Square(Of Double)(t.Data), t.Shape)
        End Function

        Public Overrides Function Abs(t As Tensor) As Tensor
            Return Wrap(SimdMath.Abs(Of Double)(t.Data), t.Shape)
        End Function

        Public Overrides Function Negate(t As Tensor) As Tensor
            Return Wrap(SimdMath.Negate(Of Double)(t.Data), t.Shape)
        End Function

        Public Overrides Function Reciprocal(t As Tensor) As Tensor
            Return Wrap(SimdMath.Reciprocal(t.Data), t.Shape)
        End Function

#End Region

#Region "标量运算"

        Public Overrides Function AddScalar(t As Tensor, scalar As Double) As Tensor
            Return Wrap(SimdEngine.AddScalar(Of Double)(t.Data, scalar), t.Shape)
        End Function

        Public Overrides Function MultiplyScalar(t As Tensor, scalar As Double) As Tensor
            Return Wrap(SimdParallel.MultiplyScalar(scalar, t.Data), t.Shape)
        End Function

        Public Overrides Function DivideScalar(t As Tensor, scalar As Double) As Tensor
            Return Wrap(SimdEngine.DivideScalar(t.Data, scalar), t.Shape)
        End Function

        Public Overrides Function Pow(t As Tensor, exponent As Double) As Tensor
            Return Wrap(SimdMath.PowScalar(t.Data, exponent), t.Shape)
        End Function

        Public Overrides Function Clip(t As Tensor, low As Double, high As Double) As Tensor
            Return Wrap(SimdMath.Clamp(Of Double)(t.Data, low, high), t.Shape)
        End Function

#End Region

#Region "矩阵运算"

        Public Overrides Function MatMul(a As Tensor, b As Tensor) As Tensor
            If a.Rank <> 2 OrElse b.Rank <> 2 Then
                Throw New ArgumentException("矩阵乘法需要二维张量")
            End If
            If a.Shape(1) <> b.Shape(0) Then
                Throw New ArgumentException($"矩阵维度不匹配: {a.Shape(1)} != {b.Shape(0)}")
            End If

            Dim m = a.Shape(0)
            Dim k = a.Shape(1)
            Dim n = b.Shape(1)

            If m * k * n < MatrixDotThreshold Then
                Return MyBase.MatMul(a, b)
            End If

            Dim rowsA = ToJagged(a.Data, m, k)
            Dim rowsB = ToJagged(b.Data, k, n)
            Dim product = SimdParallel.MatrixDot(rowsA, rowsB)

            Dim flat(m * n - 1) As Double
            For i As Integer = 0 To m - 1
                Call Array.Copy(product(i), 0, flat, i * n, n)
            Next

            Return Wrap(flat, New Integer() {m, n})
        End Function

        Private Shared Function ToJagged(flat As Double(), rows As Integer, cols As Integer) As Double()()
            Dim jagged(rows - 1)() As Double
            For i As Integer = 0 To rows - 1
                Dim row(cols - 1) As Double
                Call Array.Copy(flat, i * cols, row, 0, cols)
                jagged(i) = row
            Next
            Return jagged
        End Function

#End Region

#Region "卷积与池化"

        ''' <summary>
        ''' 卷积参与并行/向量化计算所需的最少乘加次数；低于该规模时调度开销会超过收益，
        ''' 直接退回 <see cref="TensorComputeBase"/> 的标量实现。
        ''' </summary>
        Public Const ConvParallelThreshold As Long = 200000

        ''' <summary>
        ''' 卷积前向：按输出行并行，行内沿输出通道向量化。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 并行粒度取“输出行”（batch × outH）；每一行完全由某一个线程独立计算并写入，
        ''' 线程之间没有任何共享读写，因此结果与线程数无关，可复现。
        ''' </para>
        ''' <para>
        ''' 行内的循环次序改为“先按 (kh,kw,c) 取一个输入标量，再沿输出通道 oc 累加”。
        ''' 由于后端张量采用 channel-last 布局，卷积核与累加器沿 oc 都是连续的，
        ''' 这一步正好是一次 axpy 风格的向量运算。对任一输出元素而言，它仍然只沿着
        ''' (kh,kw,c) 递增的方向累加，求和顺序与标量实现逐项一致，所以结果逐位相同。
        ''' </para>
        ''' </remarks>
        Public Overrides Function Conv2D(x As Tensor, filters As Tensor, bias As Tensor,
                                        stride As Integer, padding As Integer) As Tensor
            If x Is Nothing OrElse x.Rank <> 4 OrElse filters Is Nothing OrElse filters.Rank <> 4 OrElse
               x.Shape(3) <> filters.Shape(2) OrElse
               (bias IsNot Nothing AndAlso bias.Length <> filters.Shape(3)) Then
                ' 形状非法时交给基类抛出规范的异常
                Return MyBase.Conv2D(x, filters, bias, stride, padding)
            End If

            Dim batch = x.Shape(0), inH = x.Shape(1), inW = x.Shape(2), inC = x.Shape(3)
            Dim filtH = filters.Shape(0), filtW = filters.Shape(1), outC = filters.Shape(3)
            Dim outH = ConvOutSize(inH, filtH, stride, padding)
            Dim outW = ConvOutSize(inW, filtW, stride, padding)

            If outH <= 0 OrElse outW <= 0 Then
                Return MyBase.Conv2D(x, filters, bias, stride, padding)
            End If

            Dim totalOps = CType(batch, Long) * outH * outW * outC * filtH * filtW * inC

            If totalOps < ConvParallelThreshold Then
                Return MyBase.Conv2D(x, filters, bias, stride, padding)
            End If

            Dim result = New Tensor(batch, outH, outW, outC)
            Dim src = x.Data
            Dim kern = filters.Data
            Dim dst = result.Data
            Dim biasData = If(bias Is Nothing, Nothing, bias.Data)
            Dim filterPlane = inC * outC

            Call tpl.Parallel.For(0, batch * outH,
                Sub(row)
                    Dim n = row \ outH
                    Dim oh = row Mod outH
                    Dim acc(outC - 1) As Double

                    For ow As Integer = 0 To outW - 1
                        If biasData Is Nothing Then
                            Call Array.Clear(acc, 0, outC)
                        Else
                            Call Array.Copy(biasData, 0, acc, 0, outC)
                        End If

                        For kh As Integer = 0 To filtH - 1
                            Dim ih = oh * stride + kh - padding
                            If ih < 0 OrElse ih >= inH Then Continue For

                            For kw As Integer = 0 To filtW - 1
                                Dim iw = ow * stride + kw - padding
                                If iw < 0 OrElse iw >= inW Then Continue For

                                Dim xBase = ((n * inH + ih) * inW + iw) * inC
                                Dim kBase = (kh * filtW + kw) * filterPlane

                                For c As Integer = 0 To inC - 1
                                    Call Axpy(src(xBase + c), kern, kBase + c * outC, acc, 0, outC)
                                Next
                            Next
                        Next

                        Call Array.Copy(acc, 0, dst, ((n * outH + oh) * outW + ow) * outC, outC)
                    Next
                End Sub)

            Return result
        End Function

        ''' <summary>
        ''' 卷积反向 - 对卷积核的梯度：按 (kh, kw, c) 平面并行，平面内沿输出通道向量化。
        ''' </summary>
        ''' <remarks>
        ''' 一个 (kh, kw, c) 三元组恰好对应 dst 之中长度为 outC 的一段连续区间，因此各线程写入
        ''' 的区域互不相交。每个 dst 元素由唯一的三元组负责，线程把该元素在 (n, oh, ow) 上的
        ''' 全部贡献按同样的顺序累加完毕后一次写入，与标量实现逐位相同。
        ''' </remarks>
        Public Overrides Function Conv2DBackwardFilter(gradOutput As Tensor, x As Tensor,
                                                      filterShape As Integer(), stride As Integer,
                                                      padding As Integer) As Tensor
            If gradOutput Is Nothing OrElse gradOutput.Rank <> 4 OrElse x Is Nothing OrElse x.Rank <> 4 Then
                Return MyBase.Conv2DBackwardFilter(gradOutput, x, filterShape, stride, padding)
            End If

            Dim batch = x.Shape(0), inH = x.Shape(1), inW = x.Shape(2), inC = x.Shape(3)
            Dim filtH = filterShape(0), filtW = filterShape(1), outC = filterShape(3)
            Dim outH = gradOutput.Shape(1), outW = gradOutput.Shape(2)

            Dim totalOps = CType(batch, Long) * outH * outW * outC * filtH * filtW * inC

            If totalOps < ConvParallelThreshold Then
                Return MyBase.Conv2DBackwardFilter(gradOutput, x, filterShape, stride, padding)
            End If

            Dim result = New Tensor(filterShape)
            Dim g = gradOutput.Data
            Dim src = x.Data
            Dim dst = result.Data

            Call tpl.Parallel.For(0, filtH * filtW * inC,
                Function() New Double(outC - 1) {},
                Function(plane, state, acc)
                    Dim kh = plane \ (filtW * inC)
                    Dim kw = (plane \ inC) Mod filtW
                    Dim c = plane Mod inC

                    Call Array.Clear(acc, 0, outC)

                    For n As Integer = 0 To batch - 1
                        For oh As Integer = 0 To outH - 1
                            Dim ih = oh * stride + kh - padding
                            If ih < 0 OrElse ih >= inH Then Continue For

                            For ow As Integer = 0 To outW - 1
                                Dim iw = ow * stride + kw - padding
                                If iw < 0 OrElse iw >= inW Then Continue For

                                Dim sv = src(((n * inH + ih) * inW + iw) * inC + c)
                                Dim gBase = ((n * outH + oh) * outW + ow) * outC

                                Call Axpy(sv, g, gBase, acc, 0, outC)
                            Next
                        Next
                    Next

                    Dim dstBase = ((kh * filtW + kw) * inC + c) * outC

                    ' dst 的初值为 0 且只被本线程写入, 所以这里的赋值与累加等价
                    Call Array.Copy(acc, 0, dst, dstBase, outC)

                    Return acc
                End Function,
                Sub(acc)
                End Sub)

            Return result
        End Function

        ''' <summary>
        ''' 卷积反向 - 对输入的梯度：按输入行并行。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 每个线程独占一整行输入 (ih)，行与行之间的梯度元素互不相交，因此不存在数据竞争，
        ''' 结果也与线程数无关。
        ''' </para>
        ''' <para>
        ''' 对某个输入行有贡献的输出行 oh 满足 ``ih = oh * stride + kh - padding`` 且
        ''' ``kh ∈ [0, filtH)``，也就是 ``oh ∈ [(ih + padding - filtH + 1) / stride, (ih + padding) / stride]``；
        ''' 只遍历这个小窗口即可，总计算量与标量实现的 (oh, kh) 遍历相当。
        ''' </para>
        ''' <para>
        ''' 对任一输入梯度元素，贡献按 (oh, ow, oc, kw) 递增的顺序累加，其中 kh 由 (ih, oh) 唯一确定，
        ''' 因此与标量实现之中 (oh, ow, oc, kh, kw) 的求和顺序逐项一致，结果逐位相同。
        ''' </para>
        ''' </remarks>
        Public Overrides Function Conv2DBackwardInput(gradOutput As Tensor, filters As Tensor,
                                                      inputShape As Integer(), stride As Integer,
                                                      padding As Integer) As Tensor
            If gradOutput Is Nothing OrElse gradOutput.Rank <> 4 OrElse filters Is Nothing OrElse filters.Rank <> 4 Then
                Return MyBase.Conv2DBackwardInput(gradOutput, filters, inputShape, stride, padding)
            End If

            Dim batch = inputShape(0), inH = inputShape(1), inW = inputShape(2), inC = inputShape(3)
            Dim filtH = filters.Shape(0), filtW = filters.Shape(1), outC = filters.Shape(3)
            Dim outH = gradOutput.Shape(1), outW = gradOutput.Shape(2)

            Dim totalOps = CType(batch, Long) * outH * outW * outC * filtH * filtW * inC

            If totalOps < ConvParallelThreshold Then
                Return MyBase.Conv2DBackwardInput(gradOutput, filters, inputShape, stride, padding)
            End If

            Dim result = New Tensor(batch, inH, inW, inC)
            Dim g = gradOutput.Data
            Dim kern = filters.Data
            Dim dst = result.Data
            Dim filterPlane = inC * outC

            Call tpl.Parallel.For(0, batch * inH,
                Sub(inputRow)
                    Dim n = inputRow \ inH
                    Dim ih = inputRow Mod inH

                    Dim ohFirst = CeilDiv(ih + padding - filtH + 1, stride)
                    Dim ohLast = (ih + padding) \ stride

                    If ohFirst < 0 Then ohFirst = 0
                    If ohLast > outH - 1 Then ohLast = outH - 1

                    For oh As Integer = ohFirst To ohLast
                        Dim kh = ih - (oh * stride - padding)
                        If kh < 0 OrElse kh >= filtH Then Continue For

                        Dim rowBase = (kh * filtW) * filterPlane

                        For ow As Integer = 0 To outW - 1
                            For oc As Integer = 0 To outC - 1
                                Dim gv = g(((n * outH + oh) * outW + ow) * outC + oc)
                                Dim kBase = rowBase + oc

                                For kw As Integer = 0 To filtW - 1
                                    Dim iw = ow * stride + kw - padding
                                    If iw < 0 OrElse iw >= inW Then Continue For

                                    Dim dstBase = ((n * inH + ih) * inW + iw) * inC
                                    Dim kB = kBase + kw * filterPlane

                                    For c As Integer = 0 To inC - 1
                                        dst(dstBase + c) += gv * kern(kB + c * outC)
                                    Next
                                Next
                            Next
                        Next
                    Next
                End Sub)

            Return result
        End Function

        ''' <summary>
        ''' ``dst(dstOffset .. +len) += scalar * src(srcOffset .. +len)`` 的向量化实现。
        ''' </summary>
        ''' <remarks>
        ''' 每个元素只做一次“乘 + 加”，与标量循环的运算顺序完全一致，因此不引入任何数值差异。
        ''' </remarks>
        Private Shared Sub Axpy(scalar As Double, src As Double(), srcOffset As Integer,
                               dst As Double(), dstOffset As Integer, len As Integer)
            Dim lanes = nv.Vector(Of Double).Count
            Dim i As Integer = 0

            If len >= lanes Then
                Dim vs = New nv.Vector(Of Double)(scalar)

                While i <= len - lanes
                    Dim acc = New nv.Vector(Of Double)(dst, dstOffset + i)
                    Dim v = New nv.Vector(Of Double)(src, srcOffset + i)

                    Call nv.Vector.Add(acc, nv.Vector.Multiply(vs, v)).CopyTo(dst, dstOffset + i)

                    i += lanes
                End While
            End If

            While i < len
                dst(dstOffset + i) += scalar * src(srcOffset + i)
                i += 1
            End While
        End Sub

        ''' <summary>``ceil(a / b)``，其中 ``b`` 为正数</summary>
        ''' <remarks>
        ''' VB 的 ``\`` 是“向零截断”而不是向下取整，因此这里按
        ''' ``ceil(a / b) = -floor(-a / b)`` 计算，并把负数情形下的截断偏差补回来。
        ''' </remarks>
        Private Shared Function CeilDiv(a As Integer, b As Integer) As Integer
            Dim negA = -a
            Dim q = negA \ b

            If negA Mod b <> 0 AndAlso negA < 0 Then
                q -= 1
            End If

            Return -q
        End Function

#End Region

#Region "归约运算"

        Public Overrides Function SumAll(t As Tensor) As Double
            Return SimdParallel.Sum(t.Data)
        End Function

        Public Overrides Function MeanAll(t As Tensor) As Double
            If t.Length = 0 Then Return 0
            Return SimdParallel.Sum(t.Data) / t.Length
        End Function

        Public Overrides Function Sum(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor
            If Not axis.HasValue Then Return Tensor.Scalar(SumAll(t))
            Return MyBase.Sum(t, axis, keepdims)
        End Function

        Public Overrides Function Mean(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor
            If Not axis.HasValue Then Return Tensor.Scalar(MeanAll(t))
            Return MyBase.Mean(t, axis, keepdims)
        End Function

        Public Overrides Function Max(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor
            If axis.HasValue OrElse t.Length = 0 Then Return MyBase.Max(t, axis, keepdims)
            Return Tensor.Scalar(SimdParallel.Max(t.Data))
        End Function

        Public Overrides Function Min(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor
            If axis.HasValue OrElse t.Length = 0 Then Return MyBase.Min(t, axis, keepdims)
            Return Tensor.Scalar(SimdParallel.Min(t.Data))
        End Function

        Public Overrides Function L2Norm(t As Tensor) As Double
            Return SimdParallel.L2Norm(t.Data)
        End Function

        Public Overrides Function StdDev(t As Tensor) As Double
            If t.Length = 0 Then Return 0

            Dim src = t.Data
            Dim mean = SimdParallel.Sum(src) / src.Length
            Dim centered = SimdEngine.SubtractScalar(Of Double)(src, mean)

            Return std.Sqrt(SimdParallel.SumSquares(centered) / src.Length)
        End Function

#End Region

    End Class

End Namespace
