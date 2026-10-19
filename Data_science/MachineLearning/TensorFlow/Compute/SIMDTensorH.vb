#Region "Microsoft.VisualBasic::e5f7a9b1c3d5678901e2f3a4b5c6d7e8, Data_science\MachineLearning\TensorFlow\Compute\SIMDTensorH.vb"

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

    '     Class SIMDTensorH
    '
    '         Properties: [Default], MatMulParallelThreshold, Name, ReductionParallelThreshold
    '                     VectorWidth
    '         Function: Abs, Add, AddScalar, Clip, Divide, DivideScalar, Exp, Log
    '                   MatMul, Max, MaxAll, Maximum, Mean, MeanAll, Min, MinAll
    '                   Minimum, Multiply, MultiplyScalar, Negate, Reciprocal
    '                   Relu, Sigmoid, Softmax, Sqrt, Square, Subtract
    '                   SubtractScalar, Sum, SumAll, Swish, Tanh, Transpose
    '         Sub: Register

    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' 半精度张量的默认计算后端：基于 HalfSimd 8 路 Vector128 内核的 SIMD CPU 实现
'
' 与双精度栈的 SIMDTensor、单精度栈的 SIMDTensorF 定位一致：只重写"确实有
' 向量指令收益"的算子，其余（含 CFD 三个原语、卷积池化、SpMM、LifStep、
' 损失族）自动继承 TensorComputeHBase 的标量实现。
'
' 为什么不能像 SIMDTensorF 那样直接用 System.Numerics.Vector：
'   Vector(Of Half) 是不受支持的类型（读它的 Count 会抛 NotSupportedException，
'   因为 Half 不是向量基元类型）。HalfSimd 用 MemoryMarshal.Cast(Of Half, Short)
'   把 Half 数组按位重解释成 Short 通道，从而复用 Vector128 的整型 lane 运算，
'   再经其 lane 核（LanesToSingle / LanesToHalf）做转换 —— 与 BCL 标量语义逐位一致。
'
' 相对单精度栈的收益：
'   存储带宽需求减半（逐元素算子全部是内存带宽受限），lane 数相同（8 路），
'   理论吞吐约为单精度版的 2 倍。
' ---------------------------------------------------------------------------

Imports halfSimd = Microsoft.VisualBasic.Math.Numerics
Imports tpl = System.Threading.Tasks
Imports std = System.Math

Namespace Compute

    ''' <summary>
    ''' 半精度张量的默认 SIMD 加速 CPU 计算后端。
    ''' </summary>
    Public Class SIMDTensorH
        Inherits TensorComputeHBase

        ''' <summary>向量宽度（HalfSimd 的 128 位向量一次处理 8 个 Half）</summary>
        Public Shared ReadOnly Property VectorWidth As Integer
            Get
                Return halfSimd.HalfSimd.Lanes
            End Get
        End Property

        ''' <summary>全局默认实例（<see cref="TensorH.computeKernelH"/> 的初始值）</summary>
        Public Shared ReadOnly Property [Default] As SIMDTensorH = New SIMDTensorH()

        ''' <summary>后端名称</summary>
        Public Overrides ReadOnly Property Name As String = "SIMD-F16"

        ''' <summary>
        ''' 把 <see cref="TensorH.computeKernelH"/> 切换为 SIMD 加速的 CPU 实现。
        ''' </summary>
        Public Shared Sub Register()
            SyncLock TensorH.SyncRoot
                TensorH.computeKernelH = [Default]
            End SyncLock
        End Sub

#Region "逐元素 - 二元"

        ''' <summary>逐元素相加</summary>
        Public Overrides Function Add(a As TensorH, b As TensorH) As TensorH
            RequireSameShape(a, b, "相加")
            Return Wrap(halfSimd.HalfSimd.Add(a.Data, b.Data), a.Shape)
        End Function

        ''' <summary>逐元素相减</summary>
        Public Overrides Function Subtract(a As TensorH, b As TensorH) As TensorH
            RequireSameShape(a, b, "相减")
            Return Wrap(halfSimd.HalfSimd.Subtract(a.Data, b.Data), a.Shape)
        End Function

        ''' <summary>逐元素相乘</summary>
        Public Overrides Function Multiply(a As TensorH, b As TensorH) As TensorH
            RequireSameShape(a, b, "相乘")
            Return Wrap(halfSimd.HalfSimd.Multiply(a.Data, b.Data), a.Shape)
        End Function

        ''' <summary>逐元素相除</summary>
        Public Overrides Function Divide(a As TensorH, b As TensorH) As TensorH
            RequireSameShape(a, b, "相除")
            Return Wrap(halfSimd.HalfSimd.Divide(a.Data, b.Data), a.Shape)
        End Function

        ''' <summary>逐元素取较大值（NaN 传播）</summary>
        Public Overrides Function Maximum(a As TensorH, b As TensorH) As TensorH
            RequireSameShape(a, b, "取最大值")
            Return Wrap(halfSimd.HalfSimd.Maximum(a.Data, b.Data), a.Shape)
        End Function

        ''' <summary>逐元素取较小值（NaN 传播）</summary>
        Public Overrides Function Minimum(a As TensorH, b As TensorH) As TensorH
            RequireSameShape(a, b, "取最小值")
            Return Wrap(halfSimd.HalfSimd.Minimum(a.Data, b.Data), a.Shape)
        End Function

#End Region

#Region "逐元素 - 一元"

        ''' <summary>逐元素平方根</summary>
        Public Overrides Function Sqrt(t As TensorH) As TensorH
            Return Wrap(halfSimd.HalfSimd.Sqrt(t.Data), t.Shape)
        End Function

        ''' <summary>逐元素平方</summary>
        Public Overrides Function Square(t As TensorH) As TensorH
            Return Wrap(halfSimd.HalfSimd.Square(t.Data), t.Shape)
        End Function

        ''' <summary>逐元素绝对值</summary>
        Public Overrides Function Abs(t As TensorH) As TensorH
            Return Wrap(halfSimd.HalfSimd.Abs(t.Data), t.Shape)
        End Function

        ''' <summary>逐元素取负</summary>
        Public Overrides Function Negate(t As TensorH) As TensorH
            Return Wrap(halfSimd.HalfSimd.Negate(t.Data), t.Shape)
        End Function

        ''' <summary>逐元素倒数</summary>
        Public Overrides Function Reciprocal(t As TensorH) As TensorH
            Return Wrap(halfSimd.HalfSimd.Reciprocal(t.Data), t.Shape)
        End Function

        ''' <summary>逐元素指数</summary>
        Public Overrides Function Exp(t As TensorH) As TensorH
            Return Wrap(halfSimd.HalfSimd.Exp(t.Data), t.Shape)
        End Function

        ''' <summary>逐元素自然对数</summary>
        Public Overrides Function Log(t As TensorH) As TensorH
            Return Wrap(halfSimd.HalfSimd.Log(t.Data), t.Shape)
        End Function

        ''' <summary>逐元素双曲正切</summary>
        Public Overrides Function Tanh(t As TensorH) As TensorH
            Return Wrap(halfSimd.HalfSimd.Tanh(t.Data), t.Shape)
        End Function

        ''' <summary>逐元素 Sigmoid</summary>
        Public Overrides Function Sigmoid(t As TensorH) As TensorH
            Return Wrap(halfSimd.HalfSimd.Sigmoid(t.Data), t.Shape)
        End Function

        ''' <summary>逐元素 ReLU</summary>
        Public Overrides Function Relu(t As TensorH) As TensorH
            Return Wrap(halfSimd.HalfSimd.Relu(t.Data), t.Shape)
        End Function

#End Region

#Region "标量运算"

        ''' <summary>逐元素加标量（Half 标量）</summary>
        Public Overrides Function AddScalar(t As TensorH, scalar As Half) As TensorH
            Return Wrap(halfSimd.HalfSimd.AddScalar(t.Data, scalar), t.Shape)
        End Function

        ''' <summary>逐元素加标量（Double 标量，先经 Single 广播）</summary>
        Public Overrides Function AddScalar(t As TensorH, scalar As Double) As TensorH
            Return Wrap(halfSimd.HalfSimd.AddScalar(t.Data, scalar), t.Shape)
        End Function

        ''' <summary>逐元素乘标量（Half 标量）</summary>
        Public Overrides Function MultiplyScalar(t As TensorH, scalar As Half) As TensorH
            Return Wrap(halfSimd.HalfSimd.MultiplyScalar(t.Data, scalar), t.Shape)
        End Function

        ''' <summary>逐元素乘标量（Double 标量，先经 Single 广播）</summary>
        Public Overrides Function MultiplyScalar(t As TensorH, scalar As Double) As TensorH
            Return Wrap(halfSimd.HalfSimd.MultiplyScalar(t.Data, scalar), t.Shape)
        End Function

        ''' <summary>逐元素除标量（Half 标量）</summary>
        Public Overrides Function DivideScalar(t As TensorH, scalar As Half) As TensorH
            Return Wrap(halfSimd.HalfSimd.DivideScalar(t.Data, scalar), t.Shape)
        End Function

        ''' <summary>逐元素除标量（Double 标量，先经 Single 广播）</summary>
        Public Overrides Function DivideScalar(t As TensorH, scalar As Double) As TensorH
            Return Wrap(halfSimd.HalfSimd.DivideScalar(t.Data, scalar), t.Shape)
        End Function

        ''' <summary>逐元素减标量（Half 标量）</summary>
        Public Overrides Function SubtractScalar(t As TensorH, scalar As Half) As TensorH
            Return Wrap(halfSimd.HalfSimd.SubtractScalar(t.Data, scalar), t.Shape)
        End Function

        ''' <summary>逐元素减标量（Double 标量，先经 Single 广播）</summary>
        Public Overrides Function SubtractScalar(t As TensorH, scalar As Double) As TensorH
            Return Wrap(halfSimd.HalfSimd.SubtractScalar(t.Data, scalar), t.Shape)
        End Function

        ''' <summary>把元素钳制到 [low, high] 区间</summary>
        Public Overrides Function Clip(t As TensorH, low As Half, high As Half) As TensorH
            Return Wrap(halfSimd.HalfSimd.Clip(t.Data, CSng(low), CSng(high)), t.Shape)
        End Function

#End Region

#Region "归约运算"

        ''' <summary>
        ''' 整体求和（HalfSimd 8 路内核 + Double 收尾）。
        ''' </summary>
        ''' <remarks>
        ''' 与标量兜底的差异：向量路径用 4 路 Single lane 部分和改变了归约顺序，
        ''' 最后几个二进制位可能与顺序累加不同（误差量级相同）—— 这与单精度栈
        ''' SIMDTensorF.SumAll 的取舍完全一致。
        ''' </remarks>
        Public Overrides Function SumAll(t As TensorH) As Double
            Dim x = t.Data
            If x.Length = 0 Then Return 0.0
            Return halfSimd.HalfSimd.Sum(x)
        End Function

        ''' <summary>整体求均值</summary>
        Public Overrides Function MeanAll(t As TensorH) As Double
            If t.Length = 0 Then Return 0.0
            Return SumAll(t) / t.Length
        End Function

        ''' <summary>最小值（NaN 传播，与标量兜底一致）</summary>
        Public Overrides Function MinAll(t As TensorH) As Half
            Dim x = t.Data
            If x.Length = 0 Then Return CType(0.0F, Half)
            Return halfSimd.HalfSimd.Min(x)
        End Function

        ''' <summary>最大值（NaN 传播，与标量兜底一致）</summary>
        Public Overrides Function MaxAll(t As TensorH) As Half
            Dim x = t.Data
            If x.Length = 0 Then Return CType(0.0F, Half)
            Return halfSimd.HalfSimd.Max(x)
        End Function

        ' ------------------------------------------------------------------
        ' 逐轴归约：这是「内存带宽受限」类的典型算子，没有向量指令可用
        ' （沿非最后一维的步长不是 1），但外层 outer×inner 之间没有数据依赖，
        ' 因此把并行放在这里。每个归约任务内部按 k 的顺序在 Double 域累加，
        ' 与标量兜底逐位一致（并行不改变单个输出元素的结果）。
        ' ------------------------------------------------------------------
        Public Shared Property ReductionParallelThreshold As Integer = 4096

        Public Overrides Function Sum(t As TensorH, axis As Integer?, keepdims As Boolean) As TensorH
            If Not axis.HasValue Then Return TensorH.Scalar(CType(SumAll(t), Half))
            Return ReduceAxisParallel(t, axis.Value, keepdims, 0.0#, Function(acc, val) acc + val)
        End Function

        Public Overrides Function Mean(t As TensorH, axis As Integer?, keepdims As Boolean) As TensorH
            If Not axis.HasValue Then Return TensorH.Scalar(CType(MeanAll(t), Half))

            Dim summed = ReduceAxisParallel(t, axis.Value, keepdims, 0.0#, Function(acc, val) acc + val)
            Dim count As Double = t.Shape(axis.Value)
            Return MultiplyScalar(summed, CType(CSng(1.0 / count), Half))
        End Function

        Public Overrides Function Max(t As TensorH, axis As Integer?, keepdims As Boolean) As TensorH
            If Not axis.HasValue Then Return TensorH.Scalar(MaxAll(t))
            Return ReduceAxisParallel(t, axis.Value, keepdims, Double.NegativeInfinity,
                                      Function(acc, val) If(val > acc, val, acc))
        End Function

        Public Overrides Function Min(t As TensorH, axis As Integer?, keepdims As Boolean) As TensorH
            If Not axis.HasValue Then Return TensorH.Scalar(MinAll(t))
            Return ReduceAxisParallel(t, axis.Value, keepdims, Double.PositiveInfinity,
                                      Function(acc, val) If(val < acc, val, acc))
        End Function

        ' ------------------------------------------------------------------
        ' softmax / log-softmax：三次遍历（求 max -> 求 exp 和 -> 归一化）。
        ' 与单精度栈同阈值同结构：归约在外层 outer×inner 之间没有数据依赖，
        ' 因此并行放在这里；max / exp / 求和全部在 Double 域完成。
        '
        ' 与标量兜底的已知差异（≤ 1-2 个 fp16 ULP，与 SIMDTensorF 同名取舍一致）：
        ' 标量兜底在 Double 域做 exps(k)/sumExp 一次舍入；本实现先把 e 舍入到
        ' Half 写回目标缓冲（exp 只算一遍），再用舍入后的 inv 相乘。
        ' ------------------------------------------------------------------

        Public Overrides Function Softmax(t As TensorH, axis As Integer) As TensorH
            Return AxisSoftmaxCore(t, axis, logSoftmax:=False)
        End Function

        Public Overrides Function LogSoftmax(t As TensorH, axis As Integer) As TensorH
            Return AxisSoftmaxCore(t, axis, logSoftmax:=True)
        End Function

        Private Shared Function AxisSoftmaxCore(t As TensorH, axis As Integer, logSoftmax As Boolean) As TensorH
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
            Dim dst(src.Length - 1) As Half

            ' 判据用**总元素数**而不是归约任务数 work：
            ' work 小（例如 256 行）但每行很长（512）时，并行同样划算。
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
        ''' 出口才舍入到 Half。
        ''' </summary>
        Private Shared Sub SoftmaxOneAxis(src As Half(), dst As Half(),
                                         wi As Integer, innerSize As Integer,
                                         axisSize As Integer, logSoftmax As Boolean)
            Dim outer = wi \ innerSize
            Dim inner = wi - outer * innerSize
            Dim baseIdx = outer * axisSize * innerSize + inner

            ' 减最大值：既防 exp 溢出，也保证最大元素的 e^0 = 1 使 sumExp >= 1
            Dim maxVal As Double = CDbl(src(baseIdx))
            For k = 1 To axisSize - 1
                Dim v As Double = CDbl(src(baseIdx + k * innerSize))
                If v > maxVal Then maxVal = v
            Next

            ' exp 只算一遍：先写进目标缓冲，累加和顺带完成，第二遍只做乘法
            Dim sumExp As Double = 0

            For k = 0 To axisSize - 1
                Dim e = CType(CSng(std.Exp(CDbl(src(baseIdx + k * innerSize)) - maxVal)), Half)
                dst(baseIdx + k * innerSize) = e
                sumExp += CDbl(e)
            Next

            If sumExp <= 0 Then sumExp = 1.0

            If logSoftmax Then
                Dim logSumExp = maxVal + std.Log(sumExp)

                For k = 0 To axisSize - 1
                    dst(baseIdx + k * innerSize) = CType(CDbl(src(baseIdx + k * innerSize)) - logSumExp, Half)
                Next
            Else
                Dim inv = CType(CSng(1.0 / sumExp), Half)

                For k = 0 To axisSize - 1
                    dst(baseIdx + k * innerSize) = CType(CSng(dst(baseIdx + k * innerSize)) * CSng(inv), Half)
                Next
            End If
        End Sub

        ''' <summary>
        ''' 沿指定轴的并行归约。累加统一在 <c>Double</c> 域进行，出口才降回 Half。
        ''' </summary>
        Private Shared Function ReduceAxisParallel(t As TensorH, axis As Integer, keepdims As Boolean,
                                                   initValue As Double,
                                                   reduce As Func(Of Double, Double, Double)) As TensorH
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
                For Each sample As Half In src
                    g = reduce(g, CDbl(sample))
                Next
                Return TensorH.Scalar(CType(g, Half))
            End If

            Dim result = New TensorH(outShape.ToArray())
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
                                                  value = reduce(value, CDbl(src(baseIdx + k * innerSize + inner)))
                                              Next

                                              dst(wi) = CType(value, Half)
                                          End Sub)
            Else
                For wi = 0 To work - 1
                    Dim outer = wi \ innerSize
                    Dim inner = wi - outer * innerSize
                    Dim baseIdx = outer * axisSize * innerSize
                    Dim value = initValue

                    For k = 0 To axisSize - 1
                        value = reduce(value, CDbl(src(baseIdx + k * innerSize + inner)))
                    Next

                    dst(wi) = CType(value, Half)
                Next
            End If

            Return result
        End Function

#End Region

#Region "矩阵运算"

        ' ------------------------------------------------------------------
        ' MatMul 走 HalfSimd.MatMulRowH：每 8 个输出列一块，4 个 2-lane Double
        ' 累加器按 p 的顺序求和 —— 与 TensorComputeHBase 的 Double 累加标量
        ' 三重循环**逐位一致**（可做差分回归）。行与行之间没有数据依赖，
        ' 因此大规模时把外层行并行化。
        ' ------------------------------------------------------------------
        Public Shared Property MatMulParallelThreshold As Long = 10000L

        Public Overrides Function MatMul(a As TensorH, b As TensorH) As TensorH
            If a.Rank <> 2 OrElse b.Rank <> 2 Then
                Throw New ArgumentException("矩阵乘法需要二维张量")
            End If
            If a.Shape(1) <> b.Shape(0) Then
                Throw New ArgumentException($"矩阵维度不匹配: {a.Shape(1)} != {b.Shape(0)}")
            End If

            Dim m = a.Shape(0), k = a.Shape(1), n = b.Shape(1)
            Dim x = a.Data, y = b.Data

            ' 输出元素数必须用 64 位校验（见 Tensor.MatrixSize 的说明）
            Dim r(Tensor.MatrixSize(m, n) - 1) As Half

            If CLng(m) * CLng(k) * CLng(n) >= MatMulParallelThreshold Then
                tpl.Parallel.For(0, m, Sub(i)
                                           Call halfSimd.HalfSimd.MatMulRowH(x, y, i, k, n, r, i * n)
                                       End Sub)
            Else
                For i As Integer = 0 To m - 1
                    Call halfSimd.HalfSimd.MatMulRowH(x, y, i, k, n, r, i * n)
                Next
            End If

            Return Wrap(r, New Integer() {m, n})
        End Function

        ''' <summary>
        ''' 二维转置。纯数据搬移，没有浮点累加因此不会引入任何数值差异，
        ''' 适合直接并行。
        ''' </summary>
        Public Overrides Function Transpose(t As TensorH) As TensorH
            If t.Rank <> 2 Then
                Throw New ArgumentException("只支持二维张量转置")
            End If

            Dim rows = t.Shape(0)
            Dim cols = t.Shape(1)
            Dim result = New TensorH(cols, rows)
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
                For i As Integer = 0 To rows - 1
                    Dim iBase = i * cols
                    For j As Integer = 0 To cols - 1
                        dst(j * rows + i) = src(iBase + j)
                    Next
                Next
            End If

            Return result
        End Function

#End Region

    End Class

End Namespace
