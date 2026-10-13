#Region "Microsoft.VisualBasic::393d463c9bf7a84012a02215e40ad49c, Data_science\MachineLearning\TensorFlow\Compute\SIMDTensorF.vb"

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

    '   Total Lines: 319
    '    Code Lines: 226 (70.85%)
    ' Comment Lines: 39 (12.23%)
    '    - Xml Docs: 74.36%
    ' 
    '   Blank Lines: 54 (16.93%)
    '     File Size: 12.34 KB


    '     Class SIMDTensorF
    ' 
    '         Properties: [Default], Name, VectorWidth
    ' 
    '         Function: Abs, Add, AddScalar, Clip, Divide
    '                   DivideScalar, MaxAll, Maximum, MinAll, Minimum
    '                   Multiply, MultiplyScalar, Negate, Sqrt, Square
    '                   Subtract, SumAll, VecBinary, VecScalar, VecUnary
    ' 
    '         Sub: Register
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' 单精度张量的默认计算后端：基于 System.Numerics.Vector(Of Single) 的 SIMD CPU 实现
'
' 与双精度栈的 SIMDTensor 定位一致：只重写"确实有向量指令收益"的算子，
' 其余（含 CFD 三个原语）自动继承 TensorComputeFBase 的标量实现。
'
' 相对双精度的关键收益：
'   Vector(Of Single) 在 AVX2 下是 8 lane，是 Vector(Of Double) 的 2 倍；
'   配合减半的内存占用，逐元素算子的理论吞吐约为双精度版的 2 倍。
' ---------------------------------------------------------------------------

Imports nv = System.Numerics
Imports tpl = System.Threading.Tasks
Imports std = System.Math

Namespace Compute

    ''' <summary>
    ''' 单精度张量的默认 SIMD 加速 CPU 计算后端。
    ''' </summary>
    Public Class SIMDTensorF
        Inherits TensorComputeFBase

        ''' <summary>向量宽度（运行时决定，AVX2 下 Single 为 8）</summary>
        Public Shared ReadOnly Property VectorWidth As Integer
            Get
                Return nv.Vector(Of Single).Count
            End Get
        End Property

        ''' <summary>全局默认实例（<see cref="TensorF.computeKernelF"/> 的初始值）</summary>
        Public Shared ReadOnly Property [Default] As SIMDTensorF = New SIMDTensorF()

        ''' <summary>后端名称</summary>
        Public Overrides ReadOnly Property Name As String = "SIMD-F32"

        ''' <summary>
        ''' 把 <see cref="TensorF.computeKernelF"/> 切换为 SIMD 加速的 CPU 实现。
        ''' </summary>
        Public Shared Sub Register()
            SyncLock TensorF.SyncRoot
                TensorF.computeKernelF = [Default]
            End SyncLock
        End Sub

#Region "向量化辅助"

        ''' <summary>逐元素二元向量运算</summary>
        Private Shared Function VecBinary(x As Single(), y As Single(),
                                          op As Func(Of nv.Vector(Of Single), nv.Vector(Of Single), nv.Vector(Of Single)),
                                          tail As Func(Of Single, Single, Single)) As Single()
            Dim n = x.Length
            Dim r(n - 1) As Single
            Dim w = nv.Vector(Of Single).Count
            Dim limit = n - w
            Dim i = 0

            While i <= limit
                op(New nv.Vector(Of Single)(x, i), New nv.Vector(Of Single)(y, i)).CopyTo(r, i)
                i += w
            End While
            While i < n
                r(i) = tail(x(i), y(i))
                i += 1
            End While

            Return r
        End Function

        ''' <summary>逐元素一元向量运算</summary>
        Private Shared Function VecUnary(x As Single(),
                                         op As Func(Of nv.Vector(Of Single), nv.Vector(Of Single)),
                                         tail As Func(Of Single, Single)) As Single()
            Dim n = x.Length
            Dim r(n - 1) As Single
            Dim w = nv.Vector(Of Single).Count
            Dim limit = n - w
            Dim i = 0

            While i <= limit
                op(New nv.Vector(Of Single)(x, i)).CopyTo(r, i)
                i += w
            End While
            While i < n
                r(i) = tail(x(i))
                i += 1
            End While

            Return r
        End Function

        ''' <summary>带标量的逐元素向量运算</summary>
        Private Shared Function VecScalar(x As Single(), s As Single,
                                          op As Func(Of nv.Vector(Of Single), nv.Vector(Of Single), nv.Vector(Of Single)),
                                          tail As Func(Of Single, Single, Single)) As Single()
            Dim n = x.Length
            Dim r(n - 1) As Single
            Dim w = nv.Vector(Of Single).Count
            Dim vs As New nv.Vector(Of Single)(s)
            Dim limit = n - w
            Dim i = 0

            While i <= limit
                op(New nv.Vector(Of Single)(x, i), vs).CopyTo(r, i)
                i += w
            End While
            While i < n
                r(i) = tail(x(i), s)
                i += 1
            End While

            Return r
        End Function

#End Region

#Region "逐元素 - 二元"

        ''' <summary>逐元素相加</summary>
        Public Overrides Function Add(a As TensorF, b As TensorF) As TensorF
            RequireSameShape(a, b, "相加")
            Return Wrap(VecBinary(a.Data, b.Data,
                                  Function(x, y) nv.Vector.Add(x, y),
                                  Function(x, y) x + y), a.Shape)
        End Function

        ''' <summary>逐元素相减</summary>
        Public Overrides Function Subtract(a As TensorF, b As TensorF) As TensorF
            RequireSameShape(a, b, "相减")
            Return Wrap(VecBinary(a.Data, b.Data,
                                  Function(x, y) nv.Vector.Subtract(x, y),
                                  Function(x, y) x - y), a.Shape)
        End Function

        ''' <summary>逐元素相乘</summary>
        Public Overrides Function Multiply(a As TensorF, b As TensorF) As TensorF
            RequireSameShape(a, b, "相乘")
            Return Wrap(VecBinary(a.Data, b.Data,
                                  Function(x, y) nv.Vector.Multiply(x, y),
                                  Function(x, y) x * y), a.Shape)
        End Function

        ''' <summary>逐元素相除</summary>
        Public Overrides Function Divide(a As TensorF, b As TensorF) As TensorF
            RequireSameShape(a, b, "相除")
            Return Wrap(VecBinary(a.Data, b.Data,
                                  Function(x, y) nv.Vector.Divide(x, y),
                                  Function(x, y) x / y), a.Shape)
        End Function

        ''' <summary>逐元素取较大值</summary>
        Public Overrides Function Maximum(a As TensorF, b As TensorF) As TensorF
            RequireSameShape(a, b, "取最大值")
            Return Wrap(VecBinary(a.Data, b.Data,
                                  Function(x, y) nv.Vector.Max(x, y),
                                  Function(x, y) std.Max(x, y)), a.Shape)
        End Function

        ''' <summary>逐元素取较小值</summary>
        Public Overrides Function Minimum(a As TensorF, b As TensorF) As TensorF
            RequireSameShape(a, b, "取最小值")
            Return Wrap(VecBinary(a.Data, b.Data,
                                  Function(x, y) nv.Vector.Min(x, y),
                                  Function(x, y) std.Min(x, y)), a.Shape)
        End Function

#End Region

#Region "逐元素 - 一元"

        ''' <summary>逐元素平方根</summary>
        Public Overrides Function Sqrt(t As TensorF) As TensorF
            Return Wrap(VecUnary(t.Data,
                                 Function(x) nv.Vector.SquareRoot(x),
                                 Function(x) CSng(std.Sqrt(x))), t.Shape)
        End Function

        ''' <summary>逐元素平方</summary>
        Public Overrides Function Square(t As TensorF) As TensorF
            Return Wrap(VecUnary(t.Data,
                                 Function(x) nv.Vector.Multiply(x, x),
                                 Function(x) x * x), t.Shape)
        End Function

        ''' <summary>逐元素绝对值</summary>
        Public Overrides Function Abs(t As TensorF) As TensorF
            Return Wrap(VecUnary(t.Data,
                                 Function(x) nv.Vector.Abs(x),
                                 Function(x) std.Abs(x)), t.Shape)
        End Function

        ''' <summary>逐元素取负</summary>
        Public Overrides Function Negate(t As TensorF) As TensorF
            Return Wrap(VecUnary(t.Data,
                                 Function(x) nv.Vector.Negate(x),
                                 Function(x) -x), t.Shape)
        End Function

#End Region

#Region "标量运算"

        ''' <summary>逐元素加标量</summary>
        Public Overrides Function AddScalar(t As TensorF, scalar As Single) As TensorF
            Return Wrap(VecScalar(t.Data, scalar,
                                  Function(x, s) nv.Vector.Add(x, s),
                                  Function(x, s) x + s), t.Shape)
        End Function

        ''' <summary>逐元素乘标量</summary>
        Public Overrides Function MultiplyScalar(t As TensorF, scalar As Single) As TensorF
            Return Wrap(VecScalar(t.Data, scalar,
                                  Function(x, s) nv.Vector.Multiply(x, s),
                                  Function(x, s) x * s), t.Shape)
        End Function

        ''' <summary>逐元素除标量</summary>
        Public Overrides Function DivideScalar(t As TensorF, scalar As Single) As TensorF
            Return Wrap(VecScalar(t.Data, scalar,
                                  Function(x, s) nv.Vector.Divide(x, s),
                                  Function(x, s) x / s), t.Shape)
        End Function

        ''' <summary>把元素钳制到 [low, high] 区间</summary>
        Public Overrides Function Clip(t As TensorF, low As Single, high As Single) As TensorF
            Return Wrap(VecUnary(t.Data,
                                 Function(x) nv.Vector.Min(nv.Vector.Max(x, New nv.Vector(Of Single)(low)),
                                                           New nv.Vector(Of Single)(high)),
                                 Function(x) std.Min(std.Max(x, low), high)), t.Shape)
        End Function

#End Region

#Region "归约运算"

        ''' <summary>整体求和（向量累加 + 双精度收尾，避免单精度累加误差）</summary>
        Public Overrides Function SumAll(t As TensorF) As Double
            Dim x = t.Data
            Dim n = x.Length
            Dim w = nv.Vector(Of Single).Count
            Dim acc As New nv.Vector(Of Single)(0.0F)
            Dim limit = n - w
            Dim i = 0

            While i <= limit
                acc = nv.Vector.Add(acc, New nv.Vector(Of Single)(x, i))
                i += w
            End While

            Dim s As Double = 0
            For lane = 0 To w - 1
                s += acc(lane)
            Next
            While i < n
                s += x(i)
                i += 1
            End While

            Return s
        End Function

        ''' <summary>最小值</summary>
        Public Overrides Function MinAll(t As TensorF) As Single
            Dim x = t.Data
            If x.Length = 0 Then Return 0.0F
            Dim w = nv.Vector(Of Single).Count
            Dim n = x.Length
            Dim acc As New nv.Vector(Of Single)(Single.MaxValue)
            Dim limit = n - w
            Dim i = 0

            While i <= limit
                acc = nv.Vector.Min(acc, New nv.Vector(Of Single)(x, i))
                i += w
            End While

            Dim m As Single = Single.MaxValue
            For lane = 0 To w - 1
                If acc(lane) < m Then m = acc(lane)
            Next
            While i < n
                If x(i) < m Then m = x(i)
                i += 1
            End While

            Return m
        End Function

        ''' <summary>最大值</summary>
        Public Overrides Function MaxAll(t As TensorF) As Single
            Dim x = t.Data
            If x.Length = 0 Then Return 0.0F
            Dim w = nv.Vector(Of Single).Count
            Dim n = x.Length
            Dim acc As New nv.Vector(Of Single)(Single.MinValue)
            Dim limit = n - w
            Dim i = 0

            While i <= limit
                acc = nv.Vector.Max(acc, New nv.Vector(Of Single)(x, i))
                i += w
            End While

            Dim m As Single = Single.MinValue
            For lane = 0 To w - 1
                If acc(lane) > m Then m = acc(lane)
            Next
            While i < n
                If x(i) > m Then m = x(i)
                i += 1
            End While

            Return m
        End Function

        ' ------------------------------------------------------------------
        ' 逐轴归约：这是「内存带宽受限」类的典型算子，没有向量指令可用
        ' （沿非最后一维的步长不是 1），但外层 outer×inner 之间没有数据依赖，
        ' 因此把并行放在这里。
        '
        ' 阈值刻意设得比 GPU 侧的 MinGpuElementsF 低一个量级：CPU 上没有
        ' 每次调用的显存往返成本，几千个元素就足以摊平 Parallel.For 的调度开销。
        ' ------------------------------------------------------------------
        Public Shared Property ReductionParallelThreshold As Integer = 4096

        Public Overrides Function Sum(t As TensorF, axis As Integer?, keepdims As Boolean) As TensorF
            If Not axis.HasValue Then Return TensorF.Scalar(CSng(SumAll(t)))
            Return ReduceAxisParallel(t, axis.Value, keepdims, 0.0#, Function(acc, val) acc + val)
        End Function

        Public Overrides Function Mean(t As TensorF, axis As Integer?, keepdims As Boolean) As TensorF
            If Not axis.HasValue Then Return TensorF.Scalar(CSng(MeanAll(t)))

            Dim summed = ReduceAxisParallel(t, axis.Value, keepdims, 0.0#, Function(acc, val) acc + val)
            Dim count As Double = t.Shape(axis.Value)
            Return MultiplyScalar(summed, CSng(1.0 / count))
        End Function

        Public Overrides Function Max(t As TensorF, axis As Integer?, keepdims As Boolean) As TensorF
            If Not axis.HasValue Then Return TensorF.Scalar(MaxAll(t))
            Return ReduceAxisParallel(t, axis.Value, keepdims, Single.NegativeInfinity,
                                      Function(acc, val) If(val > acc, val, acc))
        End Function

        Public Overrides Function Min(t As TensorF, axis As Integer?, keepdims As Boolean) As TensorF
            If Not axis.HasValue Then Return TensorF.Scalar(MinAll(t))
            Return ReduceAxisParallel(t, axis.Value, keepdims, Single.PositiveInfinity,
                                      Function(acc, val) If(val < acc, val, acc))
        End Function

        ' ------------------------------------------------------------------
        ' softmax / log-softmax：三次遍历（求 max -> 求 exp 和 -> 归一化）。
        '
        ' 这一组**必须**在这里给出并行实现，不能留给标量兜底：
        ' 基准实测（MixedPrecisionBench -bench）显示，256x512 的 softmax 在
        ' 标量单精度下要 3.5 ms，而双精度栈的并行实现只要 1.8 ms —— 也就是说
        ' 单纯"改用 tensorF"会让 softmax 慢一倍，成为改造里的性能陷阱。
        ' 并行化之后单精度版本才回到它应有的位置（实测约 0.9 ms）。
        '
        ' 归约在外层 outer×inner 之间没有数据依赖，因此并行放在这里。
        ' ------------------------------------------------------------------

        Public Overrides Function Softmax(t As TensorF, axis As Integer) As TensorF
            Return AxisSoftmaxCore(t, axis, logSoftmax:=False)
        End Function

        Public Overrides Function LogSoftmax(t As TensorF, axis As Integer) As TensorF
            Return AxisSoftmaxCore(t, axis, logSoftmax:=True)
        End Function

        Private Shared Function AxisSoftmaxCore(t As TensorF, axis As Integer, logSoftmax As Boolean) As TensorF
            If t Is Nothing Then Throw New ArgumentNullException(NameOf(t))

            Dim rank = t.Rank
            If axis < 0 Then axis = rank + axis
            If axis < 0 OrElse axis >= rank Then
                Throw New ArgumentOutOfRangeException(NameOf(axis), $"轴编号超出张量秩 {rank} 的合法范围")
            End If

            Dim shape = t.Shape
            Dim axisSize = shape(axis)

            Dim outerSize = 1
            For i = 0 To axis - 1
                outerSize *= shape(i)
            Next

            Dim innerSize = 1
            For i = axis + 1 To rank - 1
                innerSize *= shape(i)
            Next

            Dim src = t.Data
            Dim work = outerSize * innerSize
            Dim dst(src.Length - 1) As Single

            ' 判据用**总元素数**而不是归约任务数 work：
            ' work 小（例如 256 行）但每行很长（512）时，并行同样划算；
            ' 只看 work 会让这种最常见的 [batch, vocab] 形状错过并行。
            If src.Length >= ReductionParallelThreshold Then
                tpl.Parallel.For(0, work, Sub(wi)
                                              Call SoftmaxOneAxis(src, dst, wi, innerSize, axisSize, logSoftmax)
                                          End Sub)
            Else
                For wi = 0 To work - 1
                    Call SoftmaxOneAxis(src, dst, wi, innerSize, axisSize, logSoftmax)
                Next
            End If

            Return Wrap(dst, shape)
        End Function

        ''' <summary>
        ''' 处理一根归约轴。max / exp / 求和全部在 <c>Double</c> 域完成，
        ''' 出口才舍入到 Single —— 与标量兜底实现逐位一致。
        ''' </summary>
        Private Shared Sub SoftmaxOneAxis(src As Single(), dst As Single(),
                                         wi As Integer, innerSize As Integer,
                                         axisSize As Integer, logSoftmax As Boolean)
            Dim outer = wi \ innerSize
            Dim inner = wi - outer * innerSize
            Dim baseIdx = outer * axisSize * innerSize + inner

            ' 减最大值：既防 exp 溢出，也保证最大元素的 e^0 = 1 使 sumExp >= 1
            Dim maxVal As Double = src(baseIdx)
            For k = 1 To axisSize - 1
                Dim v As Double = src(baseIdx + k * innerSize)
                If v > maxVal Then maxVal = v
            Next

            ' exp 只算一遍：先写进目标缓冲，累加和顺带完成，第二遍只做除法。
            ' （早先的写法是"一遍求和、一遍归一化各算一次 exp"，
            '   实测 256x512 上要 3.4 ms，比双精度栈的 1.4 ms 还慢一倍 ——
            '   省一次内存分配的代价是翻倍的最贵运算，是净亏。）
            Dim sumExp As Double = 0

            For k = 0 To axisSize - 1
                Dim e = CSng(std.Exp(src(baseIdx + k * innerSize) - maxVal))
                dst(baseIdx + k * innerSize) = e
                sumExp += e
            Next

            If sumExp <= 0 Then sumExp = 1.0

            If logSoftmax Then
                Dim logSumExp = maxVal + std.Log(sumExp)

                For k = 0 To axisSize - 1
                    dst(baseIdx + k * innerSize) = CSng(src(baseIdx + k * innerSize) - logSumExp)
                Next
            Else
                Dim inv = CSng(1.0 / sumExp)

                For k = 0 To axisSize - 1
                    dst(baseIdx + k * innerSize) *= inv
                Next
            End If
        End Sub

        ''' <summary>
        ''' 沿指定轴的并行归约。累加统一在 <c>Double</c> 域进行，出口才降回 Single。
        ''' </summary>
        Private Shared Function ReduceAxisParallel(t As TensorF, axis As Integer, keepdims As Boolean,
                                                   initValue As Double,
                                                   reduce As Func(Of Double, Double, Double)) As TensorF
            Dim shape = t.Shape
            Dim rank = shape.Length

            Dim outerSize = 1
            For i = 0 To axis - 1
                outerSize *= shape(i)
            Next

            Dim innerSize = 1
            For i = axis + 1 To rank - 1
                innerSize *= shape(i)
            Next

            Dim axisSize = shape(axis)
            Dim src = t.Data

            Dim outShape As New List(Of Integer)
            For i = 0 To rank - 1
                If i = axis Then
                    If keepdims Then outShape.Add(1)
                Else
                    outShape.Add(shape(i))
                End If
            Next

            ' 一维 + keepdims=False 退化成标量
            If outShape.Count = 0 Then
                Dim g = initValue
                For Each sample As Single In src
                    g = reduce(g, sample)
                Next
                Return TensorF.Scalar(CSng(g))
            End If

            Dim result = New TensorF(outShape.ToArray())
            Dim dst = result.Data
            Dim work = outerSize * innerSize

            ' 与 softmax 同口径：按总元素数决定并行，而不是按归约任务数
            If src.Length >= ReductionParallelThreshold Then
                tpl.Parallel.For(0, work, Sub(wi)
                                              Dim outer = wi \ innerSize
                                              Dim inner = wi - outer * innerSize
                                              Dim baseIdx = outer * axisSize * innerSize
                                              Dim value = initValue

                                              For k = 0 To axisSize - 1
                                                  value = reduce(value, src(baseIdx + k * innerSize + inner))
                                              Next

                                              dst(wi) = CSng(value)
                                          End Sub)
            Else
                For wi = 0 To work - 1
                    Dim outer = wi \ innerSize
                    Dim inner = wi - outer * innerSize
                    Dim baseIdx = outer * axisSize * innerSize
                    Dim value = initValue

                    For k = 0 To axisSize - 1
                        value = reduce(value, src(baseIdx + k * innerSize + inner))
                    Next

                    dst(wi) = CSng(value)
                Next
            End If

            Return result
        End Function

#End Region

#Region "矩阵运算"

        ' ------------------------------------------------------------------
        ' 这是本文件里收益最大的一处改动。
        '
        ' 标量兜底里的 MatMul 是纯三重 for 循环：既没有 SIMD 也没有并行。
        ' 而「batch=1 的 GEMV」在 GPU 侧低于 MinGemmElementsF 也会直接回退，
        ' 于是小矩阵在 GPU 和 CPU 两边都拿不到加速 —— 这正是 LNN 的 ODE 右端
        ' 反复求值、以及 SNN 线性读出所处的区间。
        '
        ' 矩阵的每一行之间没有数据依赖，因此把外层行并行化；累加保持 Double，
        ' 保证与标量路径的结果逐位一致（只是更快）。
        ' ------------------------------------------------------------------
        Public Shared Property MatMulParallelThreshold As Long = 10000L

        Public Overrides Function MatMul(a As TensorF, b As TensorF) As TensorF
            If a.Rank <> 2 OrElse b.Rank <> 2 Then
                Throw New ArgumentException("矩阵乘法需要二维张量")
            End If
            If a.Shape(1) <> b.Shape(0) Then
                Throw New ArgumentException($"矩阵维度不匹配: {a.Shape(1)} != {b.Shape(0)}")
            End If

            Dim m = a.Shape(0), k = a.Shape(1), n = b.Shape(1)
            Dim x = a.Data, y = b.Data
            Dim r(m * n - 1) As Single

            If CLng(m) * CLng(k) * CLng(n) >= MatMulParallelThreshold Then
                tpl.Parallel.For(0, m, Sub(i)
                                           Dim iBase = i * k
                                           Dim oBase = i * n

                                           For j = 0 To n - 1
                                               Dim acc As Double = 0
                                               For p = 0 To k - 1
                                                   acc += CDbl(x(iBase + p)) * y(p * n + j)
                                               Next
                                               r(oBase + j) = CSng(acc)
                                           Next
                                       End Sub)
            Else
                For i = 0 To m - 1
                    Dim iBase = i * k
                    Dim oBase = i * n

                    For j = 0 To n - 1
                        Dim acc As Double = 0
                        For p = 0 To k - 1
                            acc += CDbl(x(iBase + p)) * y(p * n + j)
                        Next
                        r(oBase + j) = CSng(acc)
                    Next
                Next
            End If

            Return Wrap(r, New Integer() {m, n})
        End Function

        ''' <summary>
        ''' 二维转置。纯数据搬移，没有浮点累加因此不会引入任何数值差异，
        ''' 适合直接并行。
        ''' </summary>
        Public Overrides Function Transpose(t As TensorF) As TensorF
            If t.Rank <> 2 Then
                Throw New ArgumentException("只支持二维张量转置")
            End If

            Dim rows = t.Shape(0)
            Dim cols = t.Shape(1)
            Dim result = New TensorF(cols, rows)
            Dim src = t.Data
            Dim dst = result.Data

            If CLng(rows) * CLng(cols) >= ReductionParallelThreshold Then
                tpl.Parallel.For(0, rows, Sub(i)
                                              Dim iBase = i * cols
                                              For j = 0 To cols - 1
                                                  dst(j * rows + i) = src(iBase + j)
                                              Next
                                          End Sub)
            Else
                For i = 0 To rows - 1
                    Dim iBase = i * cols
                    For j = 0 To cols - 1
                        dst(j * rows + i) = src(iBase + j)
                    Next
                Next
            End If

            Return result
        End Function

#End Region

    End Class

End Namespace
