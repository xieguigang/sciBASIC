#Region "Microsoft.VisualBasic::dcc13026580c32749df4e90217711313, Data_science\MachineLearning\TensorFlow\Compute\SIMDTensor.vb"

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

    '   Total Lines: 617
    '    Code Lines: 384 (62.24%)
    ' Comment Lines: 102 (16.53%)
    '    - Xml Docs: 78.43%
    ' 
    '   Blank Lines: 131 (21.23%)
    '     File Size: 27.26 KB


    '     Class SIMDTensor
    ' 
    '         Properties: [Default], Name
    ' 
    '         Function: Abs, Add, AddScalar, CeilDiv, Clip
    '                   Conv2D, Conv2DBackwardFilter, Conv2DBackwardInput, Divide, DivideScalar
    '                   Exp, L2Norm, LifStep, Log, MatMul
    '                   Max, Maximum, Mean, MeanAll, Min
    '                   Minimum, Multiply, MultiplyScalar, Negate, Pow
    '                   Reciprocal, Sqrt, Square, StdDev, Subtract
    '                   Sum, SumAll, ToJagged
    ' 
    '         Sub: Axpy, LifUpdateInPlace, Register
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

#Region "矩阵乘的 jagged 视图缓存"

    ''' <summary>
    ''' 一个稠密矩阵的 jagged 视图缓存条目。
    ''' </summary>
    Private NotInheritable Class JaggedRows

        Public ReadOnly Rows As Integer
        Public ReadOnly Cols As Integer
        Public ReadOnly Version As Long
        Public ReadOnly Jagged As Double()()

        Public Sub New(rows As Integer, cols As Integer, version As Long, jagged As Double()())
            Me.Rows = rows
            Me.Cols = cols
            Me.Version = version
            Me.Jagged = jagged
        End Sub

    End Class

    ' 键是底层 Double() 数组的引用：推理期权重数组是长命对象，
    ' 因此即使上层每次用新的 Tensor 实例包装同一个数组，缓存依然命中。
    Private Shared ReadOnly _jaggedCache As New Runtime.CompilerServices.ConditionalWeakTable(Of Double(), JaggedRows)

    Private Shared _cacheHits As Long = 0L
    Private Shared _cacheMisses As Long = 0L

    ''' <summary>jagged 视图缓存的命中次数（用于性能报告自证）</summary>
    Public Shared ReadOnly Property JaggedCacheHits As Long
        Get
            Return System.Threading.Interlocked.Read(_cacheHits)
        End Get
    End Property

    ''' <summary>jagged 视图缓存的重建次数</summary>
    Public Shared ReadOnly Property JaggedCacheMisses As Long
        Get
            Return System.Threading.Interlocked.Read(_cacheMisses)
        End Get
    End Property

    ''' <summary>
    ''' 取矩阵的 jagged 视图：命中缓存时零分配，未命中时按行复制一次。
    ''' 
    ''' 版本号来自 <see cref="Tensor"/> 的写入计数，原地写入后自动失效，
    ''' 因此与"每次重新展开"的结果逐位一致。
    ''' </summary>
    Private Shared Function GetJagged(t As Tensor, rows As Integer, cols As Integer) As Double()()
        Dim key As Double() = t.Data
        Dim version As Long = t.Version
        Dim entry As JaggedRows = Nothing

        If _jaggedCache.TryGetValue(key, entry) Then
            If entry.Rows = rows AndAlso entry.Cols = cols AndAlso entry.Version = version Then
                Call System.Threading.Interlocked.Increment(_cacheHits)

                Return entry.Jagged
            End If
        End If

        Call System.Threading.Interlocked.Increment(_cacheMisses)

        Dim jagged(rows - 1)() As Double

        For i As Integer = 0 To rows - 1
            Dim row(cols - 1) As Double

            Call Array.Copy(key, i * cols, row, 0, cols)
            jagged(i) = row
        Next

        Call _jaggedCache.Remove(key)
        Call _jaggedCache.Add(key, New JaggedRows(rows, cols, version, jagged))

        Return jagged
    End Function

#End Region

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

            ' 注意必须用 Long 计算规模：语言模型的输出层是 [N, d_model] × [d_model, vocab]，
            ' 在 10 万级词表下 m*k*n 轻松超过 Int32 上限（约 21.5 亿），
            ' 用 Int32 会直接抛 OverflowException。
            Dim totalOps As Long = CLng(m) * k * n

            If totalOps < MatrixDotThreshold Then
                Return MyBase.MatMul(a, b)
            End If

            ' 权重矩阵在推理期是常量：jagged 视图按底层数组缓存，
            ' 避免每次乘法都把 H×H 权重整体复制一遍（这是分配爆炸的主因）
            Dim rowsA = GetJagged(a, m, k)
            Dim rowsB = GetJagged(b, k, n)
            Dim product = SimdParallel.MatrixDot(rowsA, rowsB)

            ' 输出元素数必须用 64 位校验（见 Tensor.MatrixSize 的说明）
            Dim flat(Tensor.MatrixSize(m, n) - 1) As Double
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

        ''' <summary>
        ''' 融合的稀疏递归 LIF 单步：递归输入用标量散射（与 <see cref="TensorComputeBase.SpMM"/> 同序，
        ''' 保证与逐算子路径逐比特一致），逐元素部分用 <c>System.Numerics.Vector</c> 向量化。
        ''' </summary>
        ''' <remarks>
        ''' 向量化只加速「泄漏积分 → 阈值触发 → 复位 → 计数累加」这一段：它对每个元素
        ''' 独立运算，因此与标量版本结果逐比特相同（逐元素 IEEE 运算的结合次序未变）。
        ''' 散射段（<c>I(bo + colIdx) += …</c>）存在跨元素的累加依赖，无法向量化，
        ''' 但它已经带稀疏零源跳过，在脉冲输入下远比逐元素段轻。
        ''' </remarks>
        Public Overrides Function LifStep(synapses As SparseCsr,
                                         sPrev As Tensor,
                                         externalCurrent As Tensor,
                                         h As Tensor,
                                         s As Tensor,
                                         counts As Tensor,
                                         beta As Double,
                                         threshold As Double,
                                         subtractThreshold As Boolean) As Boolean

            ' 形状校验 / 状态失效声明 / 循环次序全部沿用基类实现，
            ' 只把它拆出的逐元素阶段换成向量化版本（见 LifUpdateInPlace）
            Return MyBase.LifStep(synapses, sPrev, externalCurrent, h, s, counts,
                                  beta, threshold, subtractThreshold)
        End Function

        ''' <summary>
        ''' 向量化的逐元素阶段：泄漏积分 → 阈值触发 → 复位 → 计数累加。
        ''' </summary>
        Protected Overrides Sub LifUpdateInPlace(synapticInput As Double(),
                                                 externalCurrent As Double(),
                                                 h As Double(),
                                                 s As Double(),
                                                 counts As Double(),
                                                 beta32 As Double,
                                                 threshold As Double,
                                                 thr32 As Double,
                                                 subtractThreshold As Boolean)

            Dim width = nv.Vector(Of Double).Count
            Dim vBeta = New nv.Vector(Of Double)(beta32)
            Dim vThreshold = New nv.Vector(Of Double)(threshold)
            Dim vThr32 = New nv.Vector(Of Double)(thr32)
            Dim vOne = New nv.Vector(Of Double)(1.0)
            Dim vZero = nv.Vector(Of Double).Zero

            Dim i As Integer = 0
            Dim vectorLimit = synapticInput.Length - width

            While i <= vectorLimit
                Dim vU = New nv.Vector(Of Double)(synapticInput, i)

                If externalCurrent IsNot Nothing Then
                    vU += New nv.Vector(Of Double)(externalCurrent, i)
                End If

                vU += New nv.Vector(Of Double)(h, i) * vBeta

                ' S = Θ(U − U_thr)：GreaterThan 与标量的 (u - thr > 0) 在 NaN 上语义一致（均为 False）
                Dim vS = nv.Vector.ConditionalSelect(nv.Vector.GreaterThan(vU - vThreshold, vZero), vOne, vZero)

                vS.CopyTo(s, i)

                Dim vNewH = If(subtractThreshold, vU - vS * vThr32, vU * (vOne - vS))

                vNewH.CopyTo(h, i)

                Dim vCounts = New nv.Vector(Of Double)(counts, i) + vS

                vCounts.CopyTo(counts, i)

                i += width
            End While

            ' 尾部不足一个向量的部分走标量（与基类实现完全相同）
            While i < synapticInput.Length
                Dim pre = synapticInput(i)

                If externalCurrent IsNot Nothing Then
                    pre += externalCurrent(i)
                End If

                Dim u = pre + h(i) * beta32
                Dim sp = If(u - threshold > 0.0, 1.0, 0.0)

                s(i) = sp
                h(i) = If(subtractThreshold, u - sp * thr32, u * (1.0 - sp))
                counts(i) += sp

                i += 1
            End While
        End Sub

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

#Region "批量图卷积（向量化）"

        ''' <summary>
        ''' 图卷积批量层（SIMD + 行并行实现）。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 标量实现是「对每个输出维度 j，再遍历输入维度 k」的三重循环，权重按列跨步读取，
        ''' 完全无法向量化。这里把求和方向反过来：固定输入维度 k，沿输出维度 outF 做一条
        ''' AXPY（权重行 <c>W[k, :]</c> 在内存中连续），由 SIMD 一次处理 4/8 个通道。
        ''' </para>
        ''' <para>
        ''' 行与行之间互不依赖（每条边的源行由 CSR 的列索引唯一确定），因此可以按行并行，
        ''' 线程私有累加缓冲由 <see cref="tpl.Parallel"/> 的 localInit 分配。
        ''' </para>
        ''' </remarks>
        Public Overrides Function GraphLayerBatch(x As Tensor, wSelf As Tensor, wRel As Tensor,
                                                  selfW As Tensor, bias As Tensor, edges As SparseCsr,
                                                  output As Tensor, activation As Integer) As Boolean

            If x Is Nothing OrElse wSelf Is Nothing OrElse output Is Nothing OrElse
               x.Rank <> 2 OrElse output.Shape(0) <> x.Shape(0) Then

                Return MyBase.GraphLayerBatch(x, wSelf, wRel, selfW, bias, edges, output, activation)
            End If

            Dim rows As Integer = x.Shape(0)
            Dim inF As Integer = x.Shape(1)
            Dim outF As Integer = output.Shape(1)
            Dim xd As Double() = x.Data
            Dim ws As Double() = wSelf.Data
            Dim wr As Double() = If(wRel Is Nothing, Nothing, wRel.Data)
            Dim sw As Double() = If(selfW Is Nothing, Nothing, selfW.Data)
            Dim bd As Double() = If(bias Is Nothing, Nothing, bias.Data)
            Dim od As Double() = output.Data
            Dim rp As Integer() = If(edges Is Nothing, Nothing, edges.RowPointers)
            Dim ci As Integer() = If(edges Is Nothing, Nothing, edges.ColumnIndices)
            Dim vv As Double() = If(edges Is Nothing, Nothing, edges.Values)

            Dim nodes As Integer = rows

            If edges IsNot Nothing Then
                nodes = edges.Rows
            ElseIf sw IsNot Nothing Then
                nodes = sw.Length
            End If

            If nodes <= 0 Then
                nodes = rows
            End If

            ' 纯矩阵乘形态（浓度读出头的调用方式：无图边、无逐节点缩放）：
            ' batch 很小时按输出维分块并行，避免整机只有 1 个核在跑
            If rp Is Nothing AndAlso sw Is Nothing AndAlso rows < 4 AndAlso outF >= 512 AndAlso ThreadBudget > 1 Then
                Call outputHeadParallel(xd, ws, bd, od, rows, inF, outF, activation)
                Call output.MarkHostModified()

                Return True
            End If

            Dim body As Func(Of Integer, System.Threading.Tasks.ParallelLoopState, Double(), Double()) =
                Function(row As Integer, loopState As System.Threading.Tasks.ParallelLoopState, acc As Double()) As Double()
                    Dim nodeIdx As Integer = row Mod nodes
                    Dim baseRow As Integer = row - nodeIdx

                    If bd Is Nothing Then
                        Call Array.Clear(acc, 0, outF)
                    Else
                        Call Array.Copy(bd, 0, acc, 0, outF)
                    End If

                    Dim scale As Double = If(sw Is Nothing, 1.0, sw(nodeIdx))
                    Dim rowOff As Integer = row * inF

                    For k As Integer = 0 To inF - 1
                        Dim v As Double = xd(rowOff + k) * scale

                        If v <> 0.0 Then
                            Call Axpy(v, ws, k * outF, acc, 0, outF)
                        End If
                    Next

                    If rp IsNot Nothing AndAlso wr IsNot Nothing Then
                        For e As Integer = rp(nodeIdx) To rp(nodeIdx + 1) - 1
                            Dim c As Double = vv(e)

                            If c = 0.0 Then
                                Continue For
                            End If

                            Dim srcOff As Integer = (baseRow + ci(e)) * inF

                            For k As Integer = 0 To inF - 1
                                Dim v As Double = xd(srcOff + k) * c

                                If v <> 0.0 Then
                                    Call Axpy(v, wr, k * outF, acc, 0, outF)
                                End If
                            Next
                        Next
                    End If

                    Dim outOff As Integer = row * outF

                    For j As Integer = 0 To outF - 1
                        od(outOff + j) = CellActivation.Apply(acc(j), activation)
                    Next

                    Return acc
                End Function

            If rows >= 4 Then
                Call tpl.Parallel.For(0, rows,
                    Function() New Double(outF - 1) {},
                    body,
                    Sub(acc)
                    End Sub)
            Else
                Dim acc As Double() = New Double(outF - 1) {}

                For row As Integer = 0 To rows - 1
                    Call body(row, Nothing, acc)
                Next
            End If

            Call output.MarkHostModified()

            Return True
        End Function

#End Region

#Region "批量液态单元内核（向量化）"

        ''' <summary>
        ''' LTC 的 RK4 批量前向（SIMD 实现）。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 与 <see cref="TensorComputeBase.LtcRk4Batch"/> 的数学定义完全一致，区别只在于
        ''' 内层求和的方向：权重按 <c>[输入维度][隐藏维度]</c> 行主序存放，固定输入索引时
        ''' 隐藏维度连续，因此可以把“逐元素乘加”变成一条长度为 m 的 AXPY，
        ''' 由 SIMD 向量指令一次处理 4/8 个隐藏单元。
        ''' </para>
        ''' <para>
        ''' 批量图卷积的标量实现同样是这一形态，此处优先覆盖代谢网络中占比最大的 RK4 内核。
        ''' </para>
        ''' </remarks>
        Private Function LtcRk4BatchRowwise(state As Tensor, input As Tensor,
                                            weightRecurrent As Tensor, weightInput As Tensor, bias As Tensor,
                                            weightGate As Tensor, weightGateInput As Tensor, biasGate As Tensor,
                                            tauEff As Tensor, dt As Double, subSteps As Integer,
                                            activation As Integer) As Boolean

            If state Is Nothing OrElse input Is Nothing OrElse weightRecurrent Is Nothing OrElse
               weightInput Is Nothing OrElse tauEff Is Nothing OrElse
               state.Rank <> 2 OrElse input.Rank <> 2 Then

                Return MyBase.LtcRk4Batch(state, input, weightRecurrent, weightInput, bias,
                                          weightGate, weightGateInput, biasGate, tauEff, dt, subSteps, activation)
            End If

            Dim batch As Integer = state.Shape(0)
            Dim m As Integer = state.Shape(1)
            Dim nIn As Integer = input.Shape(1)

            If input.Shape(0) <> batch OrElse subSteps < 1 Then
                Return MyBase.LtcRk4Batch(state, input, weightRecurrent, weightInput, bias,
                                          weightGate, weightGateInput, biasGate, tauEff, dt, subSteps, activation)
            End If

            Dim h As Double() = state.Data
            Dim u As Double() = input.Data
            Dim wrec As Double() = weightRecurrent.Data
            Dim win As Double() = weightInput.Data
            Dim bd As Double() = If(bias Is Nothing, Nothing, bias.Data)
            Dim wg As Double() = If(weightGate Is Nothing, Nothing, weightGate.Data)
            Dim wgi As Double() = If(weightGateInput Is Nothing, Nothing, weightGateInput.Data)
            Dim bg As Double() = If(biasGate Is Nothing, Nothing, biasGate.Data)
            Dim tau As Double() = tauEff.Data
            Dim hasGate As Boolean = wg IsNot Nothing AndAlso wgi IsNot Nothing

            Dim dtStep As Double = dt / subSteps
            Dim half As Double = 0.5 * dtStep
            Dim invTau As Double() = New Double(m - 1) {}

            For i As Integer = 0 To m - 1
                invTau(i) = 1.0 / tau(i)
            Next

            ' 批内每一行的状态互相独立：按行并行，线程私有工作区由 localInit 分配
            Dim body As Func(Of Integer, System.Threading.Tasks.ParallelLoopState, RowWorkspace, RowWorkspace) =
                Function(b As Integer, loopState As System.Threading.Tasks.ParallelLoopState, ws As RowWorkspace) As RowWorkspace
                    Dim rowOff As Integer = b * m
                    Dim uOff As Integer = b * nIn
                    Dim sw As Double() = ws.sw
                    Dim sh As Double() = ws.sh
                    Dim sk As Double() = ws.sk
                    Dim z As Double() = ws.z
                    Dim zg As Double() = ws.zg

                    Call Array.Copy(h, rowOff, sh, 0, m)

                    For s As Integer = 1 To subSteps
                        Call Array.Copy(sh, 0, sw, 0, m)

                        For stage As Integer = 0 To 3
                            ' z = bias + Wrec·sw + Win·u  （三者都按隐藏维度向量化）
                            If bd Is Nothing Then
                                Call Array.Clear(z, 0, m)
                            Else
                                Call Array.Copy(bd, 0, z, 0, m)
                            End If

                            If hasGate Then
                                If bg Is Nothing Then
                                    Call Array.Clear(zg, 0, m)
                                Else
                                    Call Array.Copy(bg, 0, zg, 0, m)
                                End If
                            End If

                            For k As Integer = 0 To m - 1
                                Dim hk As Double = sw(k)

                                If hk <> 0.0 Then
                                    Call Axpy(hk, wrec, k * m, z, 0, m)

                                    If hasGate Then
                                        Call Axpy(hk, wg, k * m, zg, 0, m)
                                    End If
                                End If
                            Next

                            For j As Integer = 0 To nIn - 1
                                Dim uj As Double = u(uOff + j)

                                If uj <> 0.0 Then
                                    Call Axpy(uj, win, j * m, z, 0, m)

                                    If hasGate Then
                                        Call Axpy(uj, wgi, j * m, zg, 0, m)
                                    End If
                                End If
                            Next

                            For i As Integer = 0 To m - 1
                                Dim decay As Double = invTau(i)

                                If hasGate Then
                                    decay += 1.0 / (1.0 + std.Exp(-zg(i)))
                                End If

                                sk(stage * m + i) = decay * (CellActivation.Apply(z(i), activation) - sw(i))
                            Next

                            If stage < 3 Then
                                Dim scale As Double = If(stage = 2, dtStep, half)

                                For i As Integer = 0 To m - 1
                                    sw(i) = sh(i) + scale * sk(stage * m + i)
                                Next
                            End If
                        Next

                        For i As Integer = 0 To m - 1
                            sh(i) += (dtStep / 6.0) * (sk(i) + 2.0 * sk(m + i) + 2.0 * sk(2 * m + i) + sk(3 * m + i))
                        Next
                    Next

                    Call Array.Copy(sh, 0, h, rowOff, m)

                    Return ws
                End Function

            If batch >= 4 Then
                Call tpl.Parallel.For(0, batch,
                    Function() New RowWorkspace(m),
                    body,
                    Sub(ws)
                    End Sub)
            Else
                Dim ws As New RowWorkspace(m)

                For b As Integer = 0 To batch - 1
                    Call body(b, Nothing, ws)
                Next
            End If

            Call state.MarkHostModified()

            Return True
        End Function

        ''' <summary>
        ''' 线程预算：与 Cella 的 <c>CELLA_THREADS</c> 约定一致，默认取物理核数。
        ''' </summary>
        Private Shared ReadOnly Property ThreadBudget As Integer
            Get
                Dim text As String = If(System.Environment.GetEnvironmentVariable("CELLA_THREADS"), "").Trim()
                Dim value As Integer

                If Integer.TryParse(text, value) AndAlso value >= 1 Then
                    Return value
                End If

                Return System.Math.Max(1, System.Environment.ProcessorCount \ 2)
            End Get
        End Property

        ''' <summary>
        ''' 输出维分块的 RK4 实现：即使 batch = 1 也能用满多个核心。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 逐行实现里，单个细胞的 m 个隐藏单元都在同一条线程上更新；当一次只推进
        ''' 1~3 个细胞（多物种菌群中每个物种各自成组，batch 往往就是 1）时，
        ''' 整台机器只有 1 个核在跑。
        ''' </para>
        ''' <para>
        ''' 这里把隐藏维 m 切成若干块：每块只读取权重的对应列区间
        ''' （<c>Wrec[k*m + j0 .. j1)</c>，连续访存），块间互不依赖；
        ''' 每个 RK4 stage 之间由 <c>Parallel.For</c> 的隐式屏障同步。
        ''' 求和次序与逐行实现一致（k 升序），因此结果可复现。
        ''' </para>
        ''' </remarks>
        Private Function LtcRk4BatchOutputParallel(state As Tensor, input As Tensor,
                                                   weightRecurrent As Tensor, weightInput As Tensor, bias As Tensor,
                                                   weightGate As Tensor, weightGateInput As Tensor, biasGate As Tensor,
                                                   tauEff As Tensor, dt As Double, subSteps As Integer,
                                                   activation As Integer) As Boolean

            Dim batch As Integer = state.Shape(0)
            Dim m As Integer = state.Shape(1)
            Dim nIn As Integer = input.Shape(1)

            Dim h As Double() = state.Data
            Dim u As Double() = input.Data
            Dim wrec As Double() = weightRecurrent.Data
            Dim win As Double() = weightInput.Data
            Dim bd As Double() = If(bias Is Nothing, Nothing, bias.Data)
            Dim wg As Double() = If(weightGate Is Nothing, Nothing, weightGate.Data)
            Dim wgi As Double() = If(weightGateInput Is Nothing, Nothing, weightGateInput.Data)
            Dim bg As Double() = If(biasGate Is Nothing, Nothing, biasGate.Data)
            Dim tau As Double() = tauEff.Data
            Dim hasGate As Boolean = wg IsNot Nothing AndAlso wgi IsNot Nothing

            Dim dtStep As Double = dt / subSteps
            Dim half As Double = 0.5 * dtStep
            Dim invTau As Double() = New Double(m - 1) {}

            For i As Integer = 0 To m - 1
                invTau(i) = 1.0 / tau(i)
            Next

            ' 分块数：按隐藏维大小给出上限，避免块太小导致 AXPY 过短
            Dim parts As Integer = System.Math.Min(ThreadBudget, System.Math.Max(1, m \ 128))

            If parts <= 1 Then
                Return LtcRk4BatchRowwise(state, input, weightRecurrent, weightInput, bias,
                                          weightGate, weightGateInput, biasGate, tauEff, dt, subSteps, activation)
            End If

            Dim block As Integer = (m + parts - 1) \ parts
            Dim sh As Double() = New Double(m - 1) {}
            Dim sw As Double() = New Double(m - 1) {}
            Dim sk As Double() = New Double(4 * m - 1) {}

            For b As Integer = 0 To batch - 1
                Dim rowOff As Integer = b * m
                Dim uOff As Integer = b * nIn

                Call Array.Copy(h, rowOff, sh, 0, m)

                For s As Integer = 1 To subSteps
                    Call Array.Copy(sh, 0, sw, 0, m)

                    For stage As Integer = 0 To 3
                        Dim scale As Double = If(stage = 2, dtStep, half)
                        Dim stageOffset As Integer = stage * m

                        ' 每个分块独立计算自己的输出区间；k 外层保证权重行连续访存
                        Call System.Threading.Tasks.Parallel.For(
                            0, parts,
                            Sub(part As Integer)
                                Dim j0 As Integer = part * block

                                If j0 >= m Then
                                    Return
                                End If

                                Dim len As Integer = System.Math.Min(block, m - j0)
                                Dim zs As Double() = New Double(len - 1) {}
                                Dim zgs As Double() = If(hasGate, New Double(len - 1) {}, Nothing)

                                If bd IsNot Nothing Then
                                    Call Array.Copy(bd, j0, zs, 0, len)
                                End If
                                If hasGate AndAlso bg IsNot Nothing Then
                                    Call Array.Copy(bg, j0, zgs, 0, len)
                                End If

                                For k As Integer = 0 To m - 1
                                    Dim hk As Double = sw(k)

                                    If hk <> 0.0 Then
                                        Call Axpy(hk, wrec, k * m + j0, zs, 0, len)

                                        If hasGate Then
                                            Call Axpy(hk, wg, k * m + j0, zgs, 0, len)
                                        End If
                                    End If
                                Next

                                For k As Integer = 0 To nIn - 1
                                    Dim uk As Double = u(uOff + k)

                                    If uk <> 0.0 Then
                                        Call Axpy(uk, win, k * m + j0, zs, 0, len)

                                        If hasGate Then
                                            Call Axpy(uk, wgi, k * m + j0, zgs, 0, len)
                                        End If
                                    End If
                                Next

                                For j As Integer = 0 To len - 1
                                    Dim index As Integer = j0 + j
                                    Dim decay As Double = invTau(index)

                                    If hasGate Then
                                        decay += 1.0 / (1.0 + std.Exp(-zgs(j)))
                                    End If

                                    sk(stageOffset + index) = decay * (CellActivation.Apply(zs(j), activation) - sw(index))
                                Next
                            End Sub)

                        ' 元素级更新在分块之外完成（O(m)，相对于 stage 的 O(m²) 可忽略）
                        If stage < 3 Then
                            For j As Integer = 0 To m - 1
                                sw(j) = sh(j) + scale * sk(stageOffset + j)
                            Next
                        End If
                    Next

                    For j As Integer = 0 To m - 1
                        sh(j) += (dtStep / 6.0) * (sk(j) + 2.0 * sk(m + j) + 2.0 * sk(2 * m + j) + sk(3 * m + j))
                    Next
                Next

                Call Array.Copy(sh, 0, h, rowOff, m)
            Next

            Call state.MarkHostModified()

            Return True
        End Function

#End Region

        ''' <summary>
        ''' LTC 的 RK4 批量前向（分块 GEMM 形态）。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 按行的实现会在每一行都完整读一遍 <c>Wrec(m×m)</c> 与 <c>Win(n×m)</c>：
        ''' m≈8130、n≈3401 时每个 stage 的权重流量是 <c>B·(m²+n·m)·8B</c>，在 8 细胞时已约 4 GB，
        ''' 完全是内存带宽瓶颈。
        ''' </para>
        ''' <para>
        ''' 这里把批内 8 行作为一组：固定输入维度 k 之后，对组内所有行复用同一行权重，
        ''' 使权重流量降为原来的 1/8；组内的累加缓冲（8×m 个 double，约 520 KB）驻留在 L2，
        ''' 不会被反复换出。组与组之间互不相关，因此可以并行。
        ''' </para>
        ''' <para>
        ''' 数学定义与 <see cref="TensorComputeBase.LtcRk4Batch"/> 完全一致，只有求和次序不同。
        ''' </para>
        ''' </remarks>
        Public Overrides Function LtcRk4Batch(state As Tensor, input As Tensor,
                                              weightRecurrent As Tensor, weightInput As Tensor, bias As Tensor,
                                              weightGate As Tensor, weightGateInput As Tensor, biasGate As Tensor,
                                              tauEff As Tensor, dt As Double, subSteps As Integer,
                                              activation As Integer) As Boolean

            If state Is Nothing OrElse input Is Nothing OrElse weightRecurrent Is Nothing OrElse
               weightInput Is Nothing OrElse tauEff Is Nothing OrElse
               state.Rank <> 2 OrElse input.Rank <> 2 Then

                Return MyBase.LtcRk4Batch(state, input, weightRecurrent, weightInput, bias,
                                          weightGate, weightGateInput, biasGate, tauEff, dt, subSteps, activation)
            End If

            Dim batch As Integer = state.Shape(0)
            Dim m As Integer = state.Shape(1)
            Dim nIn As Integer = input.Shape(1)

            If input.Shape(0) <> batch OrElse subSteps < 1 Then
                Return MyBase.LtcRk4Batch(state, input, weightRecurrent, weightInput, bias,
                                          weightGate, weightGateInput, biasGate, tauEff, dt, subSteps, activation)
            End If

            ' 小批量直接走按行实现：分块与并行的调度开销大于收益
            ' （实测 8/32 行时两者持平或按行略快；≥64 行时权重复用才开始摊薄开销）
            If batch < 64 Then
                ' 多物种菌群中每个物种各自成组，batch 往往只有 1~3：
                ' 此时按输出维分块，让单个细胞也能用满多个核心
                If batch < 4 AndAlso state.Shape(1) >= 1024 AndAlso ThreadBudget > 1 Then
                    Return LtcRk4BatchOutputParallel(state, input, weightRecurrent, weightInput, bias,
                                                     weightGate, weightGateInput, biasGate, tauEff, dt, subSteps, activation)
                End If

                Return LtcRk4BatchRowwise(state, input, weightRecurrent, weightInput, bias,
                                          weightGate, weightGateInput, biasGate, tauEff, dt, subSteps, activation)
            End If

            Dim h As Double() = state.Data
            Dim u As Double() = input.Data
            Dim wrec As Double() = weightRecurrent.Data
            Dim win As Double() = weightInput.Data
            Dim bdv As Double() = If(bias Is Nothing, Nothing, bias.Data)
            Dim wg As Double() = If(weightGate Is Nothing, Nothing, weightGate.Data)
            Dim wgi As Double() = If(weightGateInput Is Nothing, Nothing, weightGateInput.Data)
            Dim bgv As Double() = If(biasGate Is Nothing, Nothing, biasGate.Data)
            Dim tau As Double() = tauEff.Data
            Dim hasGate As Boolean = wg IsNot Nothing AndAlso wgi IsNot Nothing

            Dim dtStep As Double = dt / subSteps
            Dim half As Double = 0.5 * dtStep
            Dim invTau As Double() = New Double(m - 1) {}

            For i As Integer = 0 To m - 1
                invTau(i) = 1.0 / tau(i)
            Next

            Dim total As Integer = batch * m
            Dim shArr As Double() = New Double(total - 1) {}
            Dim skArr As Double() = New Double(4 * total - 1) {}

            Call Array.Copy(h, shArr, total)

            Const blockSize As Integer = 8
            Dim blocks As Integer = (batch + blockSize - 1) \ blockSize

            Dim body As Func(Of Integer, System.Threading.Tasks.ParallelLoopState, BlockWorkspace, BlockWorkspace) =
                Function(blk As Integer, loopState As System.Threading.Tasks.ParallelLoopState, ws As BlockWorkspace) As BlockWorkspace
                    Dim b0 As Integer = blk * blockSize
                    Dim cnt As Integer = System.Math.Min(blockSize, batch - b0)
                    Dim z As Double() = ws.z
                    Dim zg As Double() = ws.zg
                    Dim sw As Double() = ws.sw

                    For s As Integer = 1 To subSteps
                        For lb As Integer = 0 To cnt - 1
                            Call Array.Copy(shArr, (b0 + lb) * m, sw, lb * m, m)
                        Next

                        For stage As Integer = 0 To 3
                            For lb As Integer = 0 To cnt - 1
                                If bdv Is Nothing Then
                                    Call Array.Clear(z, lb * m, m)
                                Else
                                    Call Array.Copy(bdv, 0, z, lb * m, m)
                                End If

                                If hasGate Then
                                    If bgv Is Nothing Then
                                        Call Array.Clear(zg, lb * m, m)
                                    Else
                                        Call Array.Copy(bgv, 0, zg, lb * m, m)
                                    End If
                                End If
                            Next

                            ' 循环权重：固定 k，组内所有行复用 Wrec 的第 k 行
                            For k As Integer = 0 To m - 1
                                Dim wOff As Integer = k * m

                                For lb As Integer = 0 To cnt - 1
                                    Dim coef As Double = sw(lb * m + k)

                                    If coef <> 0.0 Then
                                        Call Axpy(coef, wrec, wOff, z, lb * m, m)

                                        If hasGate Then
                                            Call Axpy(coef, wg, wOff, zg, lb * m, m)
                                        End If
                                    End If
                                Next
                            Next

                            ' 输入权重：固定 j，组内所有行复用 Win 的第 j 行
                            For j As Integer = 0 To nIn - 1
                                Dim wOff As Integer = j * m

                                For lb As Integer = 0 To cnt - 1
                                    Dim coef As Double = u((b0 + lb) * nIn + j)

                                    If coef <> 0.0 Then
                                        Call Axpy(coef, win, wOff, z, lb * m, m)

                                        If hasGate Then
                                            Call Axpy(coef, wgi, wOff, zg, lb * m, m)
                                        End If
                                    End If
                                Next
                            Next

                            For lb As Integer = 0 To cnt - 1
                                Dim rowOff As Integer = (b0 + lb) * m
                                Dim zOff As Integer = lb * m
                                Dim sOff As Integer = stage * total + rowOff

                                For i As Integer = 0 To m - 1
                                    Dim decay As Double = invTau(i)

                                    If hasGate Then
                                        decay += 1.0 / (1.0 + std.Exp(-zg(zOff + i)))
                                    End If

                                    skArr(sOff + i) = decay * (CellActivation.Apply(z(zOff + i), activation) - sw(zOff + i))
                                Next
                            Next

                            If stage < 3 Then
                                Dim scale As Double = If(stage = 2, dtStep, half)

                                For lb As Integer = 0 To cnt - 1
                                    Dim rowOff As Integer = (b0 + lb) * m
                                    Dim sOff As Integer = stage * total + rowOff

                                    For i As Integer = 0 To m - 1
                                        sw(lb * m + i) = shArr(rowOff + i) + scale * skArr(sOff + i)
                                    Next
                                Next
                            End If
                        Next

                        For lb As Integer = 0 To cnt - 1
                            Dim rowOff As Integer = (b0 + lb) * m

                            For i As Integer = 0 To m - 1
                                shArr(rowOff + i) += (dtStep / 6.0) * (
                                    skArr(rowOff + i) + 2.0 * skArr(total + rowOff + i) +
                                    2.0 * skArr(2 * total + rowOff + i) + skArr(3 * total + rowOff + i))
                            Next
                        Next
                    Next

                    Return ws
                End Function

            If blocks > 1 Then
                Call tpl.Parallel.For(0, blocks,
                    Function() New BlockWorkspace(blockSize, m),
                    body,
                    Sub(ws)
                    End Sub)
            Else
                Dim ws As New BlockWorkspace(blockSize, m)

                For blk As Integer = 0 To blocks - 1
                    Call body(blk, Nothing, ws)
                Next
            End If

            Call Array.Copy(shArr, h, total)

            Call state.MarkHostModified()

            Return True
        End Function

        ''' <summary>
        ''' 分块 RK4 的工作区：组内累加缓冲 + 组内求值状态
        ''' </summary>
        Private Class BlockWorkspace

            Public ReadOnly z As Double()
            Public ReadOnly zg As Double()
            Public ReadOnly sw As Double()

            Sub New(blockSize As Integer, m As Integer)
                z = New Double(blockSize * m - 1) {}
                zg = New Double(blockSize * m - 1) {}
                sw = New Double(blockSize * m - 1) {}
            End Sub

        End Class

        ''' <summary>
        ''' RK4 单行积分的工作区（按线程复用，避免批内每行都分配）
        ''' </summary>
        Private Class RowWorkspace

            Public ReadOnly sw As Double()
            Public ReadOnly sh As Double()
            Public ReadOnly sk As Double()
            Public ReadOnly z As Double()
            Public ReadOnly zg As Double()

            Sub New(m As Integer)
                sw = New Double(m - 1) {}
                sh = New Double(m - 1) {}
                sk = New Double(4 * m - 1) {}
                z = New Double(m - 1) {}
                zg = New Double(m - 1) {}
            End Sub

        End Class

        ' ==================== 批量读出头（输出维并行） ====================

        ''' <summary>
        ''' 纯矩阵乘形态的批量输出头：<c>y = x·W + b</c>，按输出维分块并行。
        ''' </summary>
        ''' <remarks>
        ''' 当 batch = 1（多物种菌群里每个物种各自成组）时，按行并行完全失效；
        ''' 这里把输出维切成 <c>ThreadBudget</c> 块，块内沿输入维做 AXPY
        ''' （权重 <c>W[k, j0..j1)</c> 连续访存），块间互不依赖。
        ''' </remarks>
        Private Shared Sub outputHeadParallel(xd As Double(), w As Double(), bd As Double(), od As Double(),
                                              rows As Integer, inF As Integer, outF As Integer, activation As Integer)

            Dim parts As Integer = System.Math.Min(ThreadBudget, System.Math.Max(1, outF \ 128))

            If parts <= 1 Then
                Dim acc As Double() = New Double(outF - 1) {}

                For row As Integer = 0 To rows - 1
                    If bd Is Nothing Then
                        Call Array.Clear(acc, 0, outF)
                    Else
                        Call Array.Copy(bd, 0, acc, 0, outF)
                    End If

                    For k As Integer = 0 To inF - 1
                        Dim v As Double = xd(row * inF + k)

                        If v <> 0.0 Then
                            Call Axpy(v, w, k * outF, acc, 0, outF)
                        End If
                    Next

                    For j As Integer = 0 To outF - 1
                        od(row * outF + j) = CellActivation.Apply(acc(j), activation)
                    Next
                Next

                Return
            End If

            Dim block As Integer = (outF + parts - 1) \ parts

            Call System.Threading.Tasks.Parallel.For(0, if(rows > parts, rows, parts * rows),
                Sub(work As Integer)
                    Dim row As Integer
                    Dim j0 As Integer

                    If rows > parts Then
                        row = work
                        j0 = 0
                    Else
                        row = work \ parts
                        j0 = (work Mod parts) * block
                    End If

                    If j0 >= outF Then
                        Return
                    End If

                    Dim len As Integer = If(rows > parts, outF, System.Math.Min(block, outF - j0))
                    Dim acc As Double() = New Double(len - 1) {}

                    If bd IsNot Nothing Then
                        Call Array.Copy(bd, j0, acc, 0, len)
                    End If

                    For k As Integer = 0 To inF - 1
                        Dim v As Double = xd(row * inF + k)

                        If v <> 0.0 Then
                            Call Axpy(v, w, k * outF + j0, acc, 0, len)
                        End If
                    Next

                    Dim oOff As Integer = row * outF

                    For j As Integer = 0 To len - 1
                        od(oOff + j0 + j) = CellActivation.Apply(acc(j), activation)
                    Next
                End Sub)
        End Sub

        ''' <summary>
        ''' 通量读出头的批量内核（输出维并行 + 连续访存）。
        ''' </summary>
        ''' <remarks>
        ''' 基类实现按 <c>W[i, j]</c> 的列跨步读取（步长 = 反应数 r），
        ''' 在 r≈3.3e3 时几乎每次都 cache miss；这里改成固定输入维 i、沿输出维做 AXPY。
        ''' </remarks>
        Public Overrides Function FluxHeadBatch(h As Tensor, u As Tensor, wFlux As Tensor, fluxBias As Tensor,
                                                reversible As Tensor, output As Tensor) As Boolean

            If h Is Nothing OrElse u Is Nothing OrElse wFlux Is Nothing OrElse output Is Nothing OrElse
               h.Rank <> 2 OrElse u.Rank <> 2 OrElse output.Rank <> 2 Then

                Return MyBase.FluxHeadBatch(h, u, wFlux, fluxBias, reversible, output)
            End If

            Dim batch As Integer = h.Shape(0)
            Dim m As Integer = h.Shape(1)
            Dim nIn As Integer = u.Shape(1)
            Dim r As Integer = output.Shape(1)

            If wFlux.Shape(0) <> m + nIn OrElse wFlux.Shape(1) <> r OrElse u.Shape(0) <> batch Then
                Return MyBase.FluxHeadBatch(h, u, wFlux, fluxBias, reversible, output)
            End If

            Dim hd As Double() = h.Data
            Dim ud As Double() = u.Data
            Dim wd As Double() = wFlux.Data
            Dim bd As Double() = If(fluxBias Is Nothing, Nothing, fluxBias.Data)
            Dim rd As Double() = If(reversible Is Nothing, Nothing, reversible.Data)
            Dim od As Double() = output.Data

            Dim parts As Integer = If(batch >= ThreadBudget,
                                      1,
                                      System.Math.Min(ThreadBudget, System.Math.Max(1, r \ 128)))
            Dim block As Integer = (r + parts - 1) \ parts

            Call System.Threading.Tasks.Parallel.For(0, batch * parts,
                Sub(work As Integer)
                    Dim b As Integer = work \ parts
                    Dim j0 As Integer = (work Mod parts) * block

                    If j0 >= r Then
                        Return
                    End If

                    Dim len As Integer = System.Math.Min(block, r - j0)
                    Dim zs As Double() = New Double(len - 1) {}

                    If bd IsNot Nothing Then
                        Call Array.Copy(bd, j0, zs, 0, len)
                    End If

                    Dim hOff As Integer = b * m
                    Dim uOff As Integer = b * nIn

                    For i As Integer = 0 To m - 1
                        Dim hi As Double = hd(hOff + i)

                        If hi <> 0.0 Then
                            Call Axpy(hi, wd, i * r + j0, zs, 0, len)
                        End If
                    Next

                    For i As Integer = 0 To nIn - 1
                        Dim ui As Double = ud(uOff + i)

                        If ui <> 0.0 Then
                            Call Axpy(ui, wd, (m + i) * r + j0, zs, 0, len)
                        End If
                    Next

                    Dim oOff As Integer = b * r

                    For j As Integer = 0 To len - 1
                        Dim index As Integer = j0 + j
                        Dim z As Double = zs(j)

                        If z > 30.0 Then
                            z = 30.0
                        ElseIf z < -30.0 Then
                            z = -30.0
                        End If

                        Dim sat As Double = 1.0 / (1.0 + std.Exp(-z))
                        Dim reversibleFlag As Boolean = rd IsNot Nothing AndAlso rd(index) <> 0.0

                        od(oOff + index) = ud(uOff + index) * If(reversibleFlag, 2.0 * sat - 1.0, sat)
                    Next
                End Sub)

            Call output.MarkHostModified()

            Return True
        End Function

        ''' <summary>
        ''' 系统时间常数的批量内核（输出维并行 + 连续访存）。
        ''' </summary>
        Public Overrides Function SystemTauBatch(h As Tensor, u As Tensor, weightGate As Tensor,
                                                 weightGateInput As Tensor, biasGate As Tensor,
                                                 tauEff As Tensor, output As Tensor) As Boolean

            If h Is Nothing OrElse u Is Nothing OrElse tauEff Is Nothing OrElse output Is Nothing OrElse
               h.Rank <> 2 OrElse u.Rank <> 2 OrElse output.Rank <> 2 Then

                Return MyBase.SystemTauBatch(h, u, weightGate, weightGateInput, biasGate, tauEff, output)
            End If

            Dim batch As Integer = h.Shape(0)
            Dim m As Integer = h.Shape(1)
            Dim nIn As Integer = u.Shape(1)
            Dim hasGate As Boolean = weightGate IsNot Nothing AndAlso weightGateInput IsNot Nothing

            Dim hd As Double() = h.Data
            Dim ud As Double() = u.Data
            Dim wg As Double() = If(hasGate, weightGate.Data, Nothing)
            Dim wgi As Double() = If(hasGate, weightGateInput.Data, Nothing)
            Dim bg As Double() = If(hasGate, If(biasGate Is Nothing, Nothing, biasGate.Data), Nothing)
            Dim td As Double() = tauEff.Data
            Dim od As Double() = output.Data

            Dim parts As Integer = If(batch >= ThreadBudget,
                                      1,
                                      System.Math.Min(ThreadBudget, System.Math.Max(1, m \ 128)))
            Dim block As Integer = (m + parts - 1) \ parts

            Call System.Threading.Tasks.Parallel.For(0, batch * parts,
                Sub(work As Integer)
                    Dim b As Integer = work \ parts
                    Dim j0 As Integer = (work Mod parts) * block

                    If j0 >= m Then
                        Return
                    End If

                    Dim len As Integer = System.Math.Min(block, m - j0)
                    Dim hOff As Integer = b * m
                    Dim uOff As Integer = b * nIn

                    If Not hasGate Then
                        For i As Integer = 0 To len - 1
                            od(hOff + j0 + i) = td(j0 + i)
                        Next

                        Return
                    End If

                    Dim zgs As Double() = New Double(len - 1) {}

                    If bg IsNot Nothing Then
                        Call Array.Copy(bg, j0, zgs, 0, len)
                    End If

                    For k As Integer = 0 To m - 1
                        Dim hk As Double = hd(hOff + k)

                        If hk <> 0.0 Then
                            Call Axpy(hk, wg, k * m + j0, zgs, 0, len)
                        End If
                    Next

                    For k As Integer = 0 To nIn - 1
                        Dim uk As Double = ud(uOff + k)

                        If uk <> 0.0 Then
                            Call Axpy(uk, wgi, k * m + j0, zgs, 0, len)
                        End If
                    Next

                    For i As Integer = 0 To len - 1
                        Dim index As Integer = j0 + i
                        Dim decay As Double = 1.0 / td(index) + 1.0 / (1.0 + std.Exp(-zgs(i)))

                        od(hOff + index) = 1.0 / decay
                    Next
                End Sub)

            Call output.MarkHostModified()

            Return True
        End Function

    End Class

End Namespace
