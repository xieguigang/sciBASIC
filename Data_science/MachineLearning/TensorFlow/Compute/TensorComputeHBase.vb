#Region "Microsoft.VisualBasic::d4e6f8a0b2c4567890d1e2f3a4b5c6d7, Data_science\MachineLearning\TensorFlow\Compute\TensorComputeHBase.vb"

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

    '     Class TensorComputeHBase
    '
    '         Function: Abs, Add, AddScalar, AdvectTrilinear, ArgMax, ArgMin
    '                   ApplyMask, Clip, Conv2D, Conv2DBackwardBias, Conv2DBackwardFilter
    '                   Conv2DBackwardInput, Divide, DivideScalar, Elu, Exp, Gelu
    '                   HuberLoss, IsDevicePinned, JacobiStencil7, Laplacian7, L2Loss
    '                   LeakyRelu, LifStep, Log, LogSoftmax, MapBinary, MapUnary
    '                   MatMul, Max, MaxAll, MaxPool2D, MaxPool2DBackward, Maximum
    '                   Mean, MeanAll, Min, MinAll, Minimum, MseLoss, Multiply
    '                   MultiplyScalar, Negate, Reciprocal, Relu, Sigmoid
    '                   SigmoidCrossEntropyWithLogits, SpMM, Softmax, Sqrt, Square
    '                   Subtract, SubtractScalar, Sum, SumAll, Swish, SyncFromDevice
    '                   Tanh, Transpose, TryAdamWStep, TrilinearAt, Wrap
    '         Sub: RequireSameShape
    '
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' ITensorComputeH 的标量兜底实现
'
' 与双精度栈的 TensorComputeBase、单精度栈的 TensorComputeFBase 定位完全相同：
' 给出**语义正确**的参考实现，具体后端（SIMDTensorH / 未来的 CudaTensorH）只需要
' 重写"确实有收益"的算子。本文件同时是：
'   * GPU 后端移植时的语义基准（GPU 结果必须与这里对拍）；
'   * SIMD 路径不可用（长度不足 8 / SIMD 关闭）时的回退路径。
'
' 数值约定（fp16 硬约束，所有算子遵守）：
'   * 中间量一律升到 Single / Double，出口 CType(..., Half) 一次舍入；
'   * MatMul / SpMM / LifStep 的累加用 Double；
'   * MinAll / MaxAll 传播 NaN（与 HalfSimd.Min / Max 的既有语义一致，
'     保证 SIMD 路径与本标量路径逐位相同；单精度栈的对应算子忽略 NaN，
'     这是两条栈之间已知并有意的差异）。
'
' 网格布局约定（与 CFDEngine / VoxelShape 严格一致，CFD 原语使用）：
'       idx = (i * ny + j) * nz + k
'   * (i±1, j, k) -> idx ± ny*nz
'   * (i, j±1, k) -> idx ± nz
'   * (i, j, k±1) -> idx ± 1
' ---------------------------------------------------------------------------

Imports std = System.Math

Namespace Compute

    ''' <summary>
    ''' <see cref="ITensorComputeH"/> 的标量兜底实现（CPU 参考实现）。
    ''' </summary>
    Public MustInherit Class TensorComputeHBase
        Implements ITensorComputeH

        ''' <summary>后端名称</summary>
        Public MustOverride ReadOnly Property Name As String Implements ITensorComputeH.Name

#Region "helpers"

        ''' <summary>校验两个张量形状一致，不一致则抛出。</summary>
        Protected Shared Sub RequireSameShape(a As TensorH, b As TensorH, op As String)
            If a.Shape.Length <> b.Shape.Length Then
                Throw New ArgumentException($"{op}：两个张量维度数不一致")
            End If
            For i = 0 To a.Shape.Length - 1
                If a.Shape(i) <> b.Shape(i) Then
                    Throw New ArgumentException($"{op}：两个张量形状不一致")
                End If
            Next
        End Sub

        ''' <summary>把裸数组包装成同形张量。</summary>
        Protected Shared Function Wrap(data As Half(), shape As Integer()) As TensorH
            Return TensorH.Wrap(data, shape)
        End Function

        ''' <summary>逐元素二元映射。</summary>
        Protected Shared Function MapBinary(a As TensorH, b As TensorH,
                                            f As Func(Of Half, Half, Half)) As TensorH
            RequireSameShape(a, b, "逐元素二元运算")
            Dim x = a.Data, y = b.Data
            Dim r(x.Length - 1) As Half
            For i = 0 To x.Length - 1
                r(i) = f(x(i), y(i))
            Next
            Return Wrap(r, a.Shape)
        End Function

        ''' <summary>逐元素一元映射。</summary>
        Protected Shared Function MapUnary(t As TensorH, f As Func(Of Half, Half)) As TensorH
            Dim x = t.Data
            Dim r(x.Length - 1) As Half
            For i = 0 To x.Length - 1
                r(i) = f(x(i))
            Next
            Return Wrap(r, t.Shape)
        End Function

#End Region

#Region "逐元素 - 二元"

        ''' <summary>逐元素相加</summary>
        Public Overridable Function Add(a As TensorH, b As TensorH) As TensorH Implements ITensorComputeH.Add
            Return MapBinary(a, b, Function(x, y) CType(CSng(x) + CSng(y), Half))
        End Function

        ''' <summary>逐元素相减</summary>
        Public Overridable Function Subtract(a As TensorH, b As TensorH) As TensorH Implements ITensorComputeH.Subtract
            Return MapBinary(a, b, Function(x, y) CType(CSng(x) - CSng(y), Half))
        End Function

        ''' <summary>逐元素相乘</summary>
        Public Overridable Function Multiply(a As TensorH, b As TensorH) As TensorH Implements ITensorComputeH.Multiply
            Return MapBinary(a, b, Function(x, y) CType(CSng(x) * CSng(y), Half))
        End Function

        ''' <summary>逐元素相除</summary>
        Public Overridable Function Divide(a As TensorH, b As TensorH) As TensorH Implements ITensorComputeH.Divide
            Return MapBinary(a, b, Function(x, y) CType(CSng(x) / CSng(y), Half))
        End Function

        ''' <summary>逐元素取较大值（NaN 传播，与 System.Math.Max 一致）</summary>
        Public Overridable Function Maximum(a As TensorH, b As TensorH) As TensorH Implements ITensorComputeH.Maximum
            Return MapBinary(a, b, Function(x, y) CType(std.Max(CSng(x), CSng(y)), Half))
        End Function

        ''' <summary>逐元素取较小值（NaN 传播，与 System.Math.Min 一致）</summary>
        Public Overridable Function Minimum(a As TensorH, b As TensorH) As TensorH Implements ITensorComputeH.Minimum
            Return MapBinary(a, b, Function(x, y) CType(std.Min(CSng(x), CSng(y)), Half))
        End Function

#End Region

#Region "逐元素 - 一元"

        ' ------------------------------------------------------------------
        ' 全部一元算子遵循同一约定：在 Single 域求值（Half→Single 是精确
        ' widening），出口 CType(..., Half) 一次舍入。这样标量路径与
        ' HalfSimd 的向量路径（widen → Single lane 运算 → Narrow）逐位一致。
        ' ------------------------------------------------------------------

        ''' <summary>逐元素指数</summary>
        Public Overridable Function Exp(t As TensorH) As TensorH Implements ITensorComputeH.Exp
            Return MapUnary(t, Function(x) CType(std.Exp(CSng(x)), Half))
        End Function

        ''' <summary>逐元素自然对数</summary>
        Public Overridable Function Log(t As TensorH) As TensorH Implements ITensorComputeH.Log
            Return MapUnary(t, Function(x) CType(std.Log(CSng(x)), Half))
        End Function

        ''' <summary>逐元素平方根</summary>
        Public Overridable Function Sqrt(t As TensorH) As TensorH Implements ITensorComputeH.Sqrt
            Return MapUnary(t, Function(x) CType(std.Sqrt(CSng(x)), Half))
        End Function

        ''' <summary>逐元素平方</summary>
        Public Overridable Function Square(t As TensorH) As TensorH Implements ITensorComputeH.Square
            Return MapUnary(t, Function(x) CType(CSng(x) * CSng(x), Half))
        End Function

        ''' <summary>逐元素绝对值</summary>
        Public Overridable Function Abs(t As TensorH) As TensorH Implements ITensorComputeH.Abs
            Return MapUnary(t, Function(x) CType(std.Abs(CSng(x)), Half))
        End Function

        ''' <summary>逐元素双曲正切</summary>
        Public Overridable Function Tanh(t As TensorH) As TensorH Implements ITensorComputeH.Tanh
            Return MapUnary(t, Function(x) CType(std.Tanh(CSng(x)), Half))
        End Function

        ''' <summary>逐元素 Sigmoid</summary>
        Public Overridable Function Sigmoid(t As TensorH) As TensorH Implements ITensorComputeH.Sigmoid
            Return MapUnary(t, Function(x) CType(1.0F / (1.0F + CSng(std.Exp(-CSng(x)))), Half))
        End Function

        ''' <summary>逐元素取负</summary>
        Public Overridable Function Negate(t As TensorH) As TensorH Implements ITensorComputeH.Negate
            Return MapUnary(t, Function(x) CType(-CSng(x), Half))
        End Function

        ''' <summary>逐元素倒数</summary>
        Public Overridable Function Reciprocal(t As TensorH) As TensorH Implements ITensorComputeH.Reciprocal
            Return MapUnary(t, Function(x) CType(1.0F / CSng(x), Half))
        End Function

        ''' <summary>
        ''' 逐元素 ReLU：<c>x &gt; 0 ? x : 0</c>（NaN 输入取 0，与 HalfSimd.Relu 一致）。
        ''' </summary>
        Public Overridable Function Relu(t As TensorH) As TensorH Implements ITensorComputeH.Relu
            Return MapUnary(t, Function(x) If(CSng(x) > 0.0F, x, CType(0.0F, Half)))
        End Function

        ''' <summary>Leaky ReLU: max(alpha * x, x)。</summary>
        Public Overridable Function LeakyRelu(t As TensorH, alpha As Double) As TensorH Implements ITensorComputeH.LeakyRelu
            Return MapUnary(t, Function(x) If(CSng(x) >= 0.0F, x, CType(CSng(x) * CSng(alpha), Half)))
        End Function

        ''' <summary>ELU: x if x &gt;= 0 else alpha * (exp(x) - 1)。</summary>
        Public Overridable Function Elu(t As TensorH, alpha As Double) As TensorH Implements ITensorComputeH.Elu
            Return MapUnary(t, Function(x) If(CSng(x) >= 0.0F, x,
                                               CType(CSng(alpha) * (CSng(std.Exp(CSng(x))) - 1.0F), Half)))
        End Function

        ''' <summary>GELU（tanh 近似）。</summary>
        Public Overridable Function Gelu(t As TensorH) As TensorH Implements ITensorComputeH.Gelu
            Dim c = std.Sqrt(2.0 / std.PI)
            Return MapUnary(t, Function(x)
                                    Dim xs = CSng(x)
                                    Return CType(0.5 * xs * (1.0 + std.Tanh(c * (xs + 0.044715 * xs * xs * xs))), Half)
                                End Function)
        End Function

        ''' <summary>Swish / SiLU: x * sigmoid(x)。</summary>
        Public Overridable Function Swish(t As TensorH) As TensorH Implements ITensorComputeH.Swish
            Return MapUnary(t, Function(x) CType(CSng(x) / (1.0 + std.Exp(-CSng(x))), Half))
        End Function

#End Region

#Region "标量运算"

        ' HalfSimd 的标量重载只接 Single；这里的 Double 重载同样先经 CSng
        ' 广播后再舍入到 Half（标量精度损失对 fp16 输出无实际影响）。

        ''' <summary>逐元素加标量（Half 标量）</summary>
        Public Overridable Function AddScalar(t As TensorH, scalar As Half) As TensorH Implements ITensorComputeH.AddScalar
            Return MapUnary(t, Function(x) CType(CSng(x) + CSng(scalar), Half))
        End Function

        ''' <summary>逐元素加标量（Double 标量）</summary>
        Public Overridable Function AddScalar(t As TensorH, scalar As Double) As TensorH Implements ITensorComputeH.AddScalar
            Return MapUnary(t, Function(x) CType(CSng(x) + CSng(scalar), Half))
        End Function

        ''' <summary>逐元素乘标量（Half 标量）</summary>
        Public Overridable Function MultiplyScalar(t As TensorH, scalar As Half) As TensorH Implements ITensorComputeH.MultiplyScalar
            Return MapUnary(t, Function(x) CType(CSng(x) * CSng(scalar), Half))
        End Function

        ''' <summary>逐元素乘标量（Double 标量）</summary>
        Public Overridable Function MultiplyScalar(t As TensorH, scalar As Double) As TensorH Implements ITensorComputeH.MultiplyScalar
            Return MapUnary(t, Function(x) CType(CSng(x) * CSng(scalar), Half))
        End Function

        ''' <summary>逐元素除标量（Half 标量）</summary>
        Public Overridable Function DivideScalar(t As TensorH, scalar As Half) As TensorH Implements ITensorComputeH.DivideScalar
            Return MapUnary(t, Function(x) CType(CSng(x) / CSng(scalar), Half))
        End Function

        ''' <summary>逐元素除标量（Double 标量）</summary>
        Public Overridable Function DivideScalar(t As TensorH, scalar As Double) As TensorH Implements ITensorComputeH.DivideScalar
            Return MapUnary(t, Function(x) CType(CSng(x) / CSng(scalar), Half))
        End Function

        ''' <summary>逐元素减标量（Half 标量）</summary>
        Public Overridable Function SubtractScalar(t As TensorH, scalar As Half) As TensorH Implements ITensorComputeH.SubtractScalar
            Return MapUnary(t, Function(x) CType(CSng(x) - CSng(scalar), Half))
        End Function

        ''' <summary>逐元素减标量（Double 标量）</summary>
        Public Overridable Function SubtractScalar(t As TensorH, scalar As Double) As TensorH Implements ITensorComputeH.SubtractScalar
            Return MapUnary(t, Function(x) CType(CSng(x) - CSng(scalar), Half))
        End Function

        ''' <summary>把元素钳制到 [low, high] 区间（NaN 传播，与 Math.Min/Max 一致）</summary>
        Public Overridable Function Clip(t As TensorH, low As Half, high As Half) As TensorH Implements ITensorComputeH.Clip
            Return MapUnary(t, Function(x) CType(std.Min(std.Max(CSng(x), CSng(low)), CSng(high)), Half))
        End Function

#End Region

#Region "矩阵运算"

        ''' <summary>二维矩阵乘法（Double 累加，出口一次舍入）</summary>
        Public Overridable Function MatMul(a As TensorH, b As TensorH) As TensorH Implements ITensorComputeH.MatMul
            If a.Rank <> 2 OrElse b.Rank <> 2 Then
                Throw New ArgumentException("矩阵乘法需要二维张量")
            End If
            If a.Shape(1) <> b.Shape(0) Then
                Throw New ArgumentException($"矩阵维度不匹配: {a.Shape(1)} != {b.Shape(0)}")
            End If

            Dim m = a.Shape(0), k = a.Shape(1), n = b.Shape(1)
            Dim x = a.Data, y = b.Data
            Dim r(m * n - 1) As Half

            For i = 0 To m - 1
                Dim iBase = i * k
                Dim oBase = i * n
                For j = 0 To n - 1
                    Dim acc As Double = 0
                    For p = 0 To k - 1
                        acc += CDbl(x(iBase + p)) * CDbl(y(p * n + j))
                    Next
                    r(oBase + j) = CType(acc, Half)
                Next
            Next

            Return Wrap(r, New Integer() {m, n})
        End Function

        ''' <summary>二维张量转置</summary>
        Public Overridable Function Transpose(t As TensorH) As TensorH Implements ITensorComputeH.Transpose
            If t.Rank <> 2 Then
                Throw New ArgumentException("只支持二维张量转置")
            End If

            Dim rows = t.Shape(0)
            Dim cols = t.Shape(1)
            Dim result = New TensorH(cols, rows)
            Dim src = t.Data
            Dim dst = result.Data

            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To cols - 1
                    dst(j * rows + i) = src(i * cols + j)
                Next
            Next

            Return result
        End Function

        ''' <summary>
        ''' 稀疏 × 稠密矩阵乘的标量参考实现。逐点运算次序与两条既有栈完全一致，
        ''' 因此 CPU 与 GPU 后端可以对拍。累加刻意用 Double：行长度很大时
        ''' 累加会先出现可见误差，而这个误差会在下游的阈值判定里被放大。
        ''' </summary>
        Public Overridable Function SpMM(csr As SparseCsr, dense As TensorH) As TensorH Implements ITensorComputeH.SpMM
            If csr Is Nothing Then Throw New ArgumentNullException(NameOf(csr))
            If dense Is Nothing Then Throw New ArgumentNullException(NameOf(dense))
            If dense.Rank <> 2 OrElse dense.Shape(1) <> csr.Rows Then
                Throw New ArgumentException(
                    $"SpMM 输入形状应为 [batch, {csr.Rows}]，实际 [{String.Join(",", dense.Shape)}]")
            End If

            Dim batch = dense.Shape(0)
            Dim rows = csr.Rows
            Dim cols = csr.Columns
            Dim rp = csr.RowPointers
            Dim ci = csr.ColumnIndices
            Dim vv = csr.Values

            Dim acc(batch * cols - 1) As Double
            Dim xd = dense.Data

            For b As Integer = 0 To batch - 1
                Dim bo = b * cols
                Dim ro = b * rows
                For r As Integer = 0 To rows - 1
                    Dim xv As Double = CDbl(xd(ro + r))
                    If xv = 0.0 Then Continue For    ' 脉冲输入高度稀疏：跳过零源
                    Dim kEnd = rp(r + 1)
                    For k As Integer = rp(r) To kEnd - 1
                        acc(bo + ci(k)) += xv * vv(k)
                    Next
                Next
            Next

            Dim od(batch * cols - 1) As Half
            For i As Integer = 0 To od.Length - 1
                od(i) = CType(acc(i), Half)
            Next

            Return Wrap(od, New Integer() {batch, cols})
        End Function

        ''' <summary>
        ''' 融合的稀疏递归 LIF 单步（主机标量实现，就地更新状态张量）。
        ''' </summary>
        ''' <remarks>
        ''' 语义与「SpMM + Add + MultiplyScalar + Heaviside + ElementwiseMultiply」逐算子路径
        ''' 逐比特等价，但把每步 6 个 <c>[batch, Units]</c> 中间张量的分配与 5 次全量遍历
        ''' 压缩为 1 个双精度累加缓冲 + 1 次融合遍历。
        ''' <para>
        ''' β 与 θ 由调用方以 <c>Double</c> 传入，本实现全程用 <c>Double</c> 计算后再
        ''' 落到 <c>Half</c> 存储——这是刻意的设计：膜电位是逐时间步累积量，
        ''' 中间项的半精度舍入会改变阈值判定。
        ''' </para>
        ''' </remarks>
        Public Overridable Function LifStep(synapses As SparseCsr,
                                            sPrev As TensorH,
                                            externalCurrent As TensorH,
                                            h As TensorH,
                                            s As TensorH,
                                            counts As TensorH,
                                            beta As Double,
                                            threshold As Double,
                                            subtractThreshold As Boolean) As Boolean Implements ITensorComputeH.LifStep
            If synapses Is Nothing Then Throw New ArgumentNullException(NameOf(synapses))
            If sPrev Is Nothing Then Throw New ArgumentNullException(NameOf(sPrev))
            If h Is Nothing Then Throw New ArgumentNullException(NameOf(h))
            If s Is Nothing Then Throw New ArgumentNullException(NameOf(s))

            Dim batch = sPrev.Shape(0)
            Dim units = sPrev.Shape(1)
            If h.Shape(0) <> batch OrElse h.Shape(1) <> units Then
                Throw New ArgumentException("膜电位与脉冲张量形状不一致")
            End If
            If externalCurrent IsNot Nothing AndAlso externalCurrent.Length <> sPrev.Length Then
                Throw New ArgumentException("外部注入电流形状与脉冲张量不一致")
            End If

            Dim rp = synapses.RowPointers
            Dim ci = synapses.ColumnIndices
            Dim vv = synapses.Values
            Dim sp = sPrev.Data
            Dim hp = h.Data
            Dim sOut = s.Data
            Dim ext = If(externalCurrent Is Nothing, Nothing, externalCurrent.Data)
            Dim cnt = If(counts Is Nothing, Nothing, counts.Data)

            Dim current(units - 1) As Double

            For b As Integer = 0 To batch - 1
                Array.Clear(current, 0, units)
                Dim ro = b * units

                For i As Integer = 0 To units - 1
                    Dim xv As Double = CDbl(sp(ro + i))
                    If xv = 0.0 Then Continue For
                    Dim kEnd = rp(i + 1)
                    For k As Integer = rp(i) To kEnd - 1
                        current(ci(k)) += xv * vv(k)
                    Next
                Next

                For i As Integer = 0 To units - 1
                    Dim idx = ro + i
                    Dim u As Double = beta * CDbl(hp(idx)) + current(i) + If(ext Is Nothing, 0.0, CDbl(ext(idx)))

                    If u >= threshold Then
                        sOut(idx) = CType(1.0F, Half)
                        hp(idx) = CType(If(subtractThreshold, u - threshold, 0.0), Half)
                    Else
                        sOut(idx) = CType(0.0F, Half)
                        hp(idx) = CType(u, Half)
                    End If

                    If cnt IsNot Nothing Then cnt(idx) = CType(CDbl(cnt(idx)) + CDbl(sOut(idx)), Half)
                Next
            Next

            Call h.MarkHostModified()
            Call s.MarkHostModified()
            If counts IsNot Nothing Then counts.MarkHostModified()

            Return True
        End Function

#End Region

#Region "归约运算"

        ''' <summary>整体求和（双精度累加，避免半精度大数组累加的舍入误差）</summary>
        Public Overridable Function SumAll(t As TensorH) As Double Implements ITensorComputeH.SumAll
            Dim x = t.Data
            Dim acc As Double = 0
            For i = 0 To x.Length - 1
                acc += CDbl(x(i))
            Next
            Return acc
        End Function

        ''' <summary>整体求均值</summary>
        Public Overridable Function MeanAll(t As TensorH) As Double Implements ITensorComputeH.MeanAll
            If t.Length = 0 Then Return 0.0
            Return SumAll(t) / t.Length
        End Function

        ''' <summary>
        ''' 最小值。NaN 传播（与 <see cref="Microsoft.VisualBasic.Math.Numerics.HalfSimd.Min"/> 及 Enumerable.Min
        ''' 对浮点序列的语义一致），保证 SIMD 路径与本标量路径逐位相同。
        ''' </summary>
        Public Overridable Function MinAll(t As TensorH) As Half Implements ITensorComputeH.MinAll
            Dim x = t.Data
            If x.Length = 0 Then Return CType(0.0F, Half)
            Dim m = x(0)
            For i = 1 To x.Length - 1
                If Half.IsNaN(x(i)) Then Return CType(Single.NaN, Half)
                If x(i) < m Then m = x(i)
            Next
            Return m
        End Function

        ''' <summary>
        ''' 最大值。NaN 传播（与 <see cref="Microsoft.VisualBasic.Math.Numerics.HalfSimd.Max"/> 一致）。
        ''' </summary>
        Public Overridable Function MaxAll(t As TensorH) As Half Implements ITensorComputeH.MaxAll
            Dim x = t.Data
            If x.Length = 0 Then Return CType(0.0F, Half)
            Dim m = x(0)
            For i = 1 To x.Length - 1
                If Half.IsNaN(x(i)) Then Return CType(Single.NaN, Half)
                If x(i) > m Then m = x(i)
            Next
            Return m
        End Function

        Public Overridable Function Sum(t As TensorH, axis As Integer?, keepdims As Boolean) As TensorH Implements ITensorComputeH.Sum
            If Not axis.HasValue Then Return TensorH.Scalar(CType(SumAll(t), Half))
            Return ReduceAlongAxisH(t, axis.Value, keepdims, 0.0#, Function(acc, val) acc + val)
        End Function

        Public Overridable Function Mean(t As TensorH, axis As Integer?, keepdims As Boolean) As TensorH Implements ITensorComputeH.Mean
            If Not axis.HasValue Then Return TensorH.Scalar(CType(MeanAll(t), Half))

            Dim sumResult = ReduceAlongAxisH(t, axis.Value, keepdims, 0.0#, Function(acc, val) acc + val)
            Dim count As Double = t.Shape(axis.Value)
            Return MultiplyScalar(sumResult, CType(CSng(1.0 / count), Half))
        End Function

        Public Overridable Function Max(t As TensorH, axis As Integer?, keepdims As Boolean) As TensorH Implements ITensorComputeH.Max
            If Not axis.HasValue Then Return TensorH.Scalar(MaxAll(t))
            Return ReduceAlongAxisH(t, axis.Value, keepdims, Double.NegativeInfinity, Function(acc, val) std.Max(acc, val))
        End Function

        Public Overridable Function Min(t As TensorH, axis As Integer?, keepdims As Boolean) As TensorH Implements ITensorComputeH.Min
            If Not axis.HasValue Then Return TensorH.Scalar(MinAll(t))
            Return ReduceAlongAxisH(t, axis.Value, keepdims, Double.PositiveInfinity, Function(acc, val) std.Min(acc, val))
        End Function

        Public Overridable Function ArgMax(t As TensorH, axis As Integer?) As TensorH Implements ITensorComputeH.ArgMax
            If Not axis.HasValue Then Return TensorH.Scalar(CType(CSng(ArgGlobalH(t, findMin:=False)), Half))
            Return ReduceArgAxisH(t, axis.Value, False)
        End Function

        Public Overridable Function ArgMin(t As TensorH, axis As Integer?) As TensorH Implements ITensorComputeH.ArgMin
            If Not axis.HasValue Then Return TensorH.Scalar(CType(CSng(ArgGlobalH(t, findMin:=True)), Half))
            Return ReduceArgAxisH(t, axis.Value, True)
        End Function

#End Region

#Region "神经网络"

        ' ------------------------------------------------------------------
        ' 这一组算子的共同取舍：数据以 Half 存储，但**内部的中间量一律用 Double**。
        '
        '   存储精度 ≠ 计算精度。softmax 的最大值/exp/求和、log(1+e^-|x|)
        '   这些中间步骤在 Half 下连前向都会溢出（exp 在 x > 11.1 即 Infinity），
        '   必须重参数化 + 升精度。出口才舍入到 Half。
        ' ------------------------------------------------------------------

        Public Overridable Function Softmax(t As TensorH, axis As Integer) As TensorH Implements ITensorComputeH.Softmax
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
            Dim dst(src.Length - 1) As Half
            Dim exps(axisSize - 1) As Double

            For outer = 0 To outerSize - 1
                Dim baseIdx = outer * axisSize * innerSize

                For inner = 0 To innerSize - 1
                    ' 减最大值：既防 exp 溢出，也让最大元素的 e^0 = 1 保证 sumExp >= 1
                    Dim maxVal As Double = CDbl(src(baseIdx + inner))
                    For k = 1 To axisSize - 1
                        Dim v As Double = CDbl(src(baseIdx + k * innerSize + inner))
                        If v > maxVal Then maxVal = v
                    Next

                    Dim sumExp As Double = 0
                    For k = 0 To axisSize - 1
                        Dim e = std.Exp(CDbl(src(baseIdx + k * innerSize + inner)) - maxVal)
                        exps(k) = e
                        sumExp += e
                    Next

                    For k = 0 To axisSize - 1
                        dst(baseIdx + k * innerSize + inner) = CType(exps(k) / sumExp, Half)
                    Next
                Next
            Next

            Return Wrap(dst, shape)
        End Function

        Public Overridable Function LogSoftmax(t As TensorH, axis As Integer) As TensorH Implements ITensorComputeH.LogSoftmax
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
            Dim dst(src.Length - 1) As Half

            For outer = 0 To outerSize - 1
                Dim baseIdx = outer * axisSize * innerSize

                For inner = 0 To innerSize - 1
                    Dim maxVal As Double = CDbl(src(baseIdx + inner))
                    For k = 1 To axisSize - 1
                        Dim v As Double = CDbl(src(baseIdx + k * innerSize + inner))
                        If v > maxVal Then maxVal = v
                    Next

                    Dim sumExp As Double = 0
                    For k = 0 To axisSize - 1
                        sumExp += std.Exp(CDbl(src(baseIdx + k * innerSize + inner)) - maxVal)
                    Next

                    Dim logSumExp = maxVal + std.Log(sumExp)

                    For k = 0 To axisSize - 1
                        dst(baseIdx + k * innerSize + inner) = CType(CDbl(src(baseIdx + k * innerSize + inner)) - logSumExp, Half)
                    Next
                Next
            Next

            Return Wrap(dst, shape)
        End Function

        Public Overridable Function SigmoidCrossEntropyWithLogits(labels As TensorH, logits As TensorH) As TensorH Implements ITensorComputeH.SigmoidCrossEntropyWithLogits
            RequireSameShape(labels, logits, "计算 sigmoid 交叉熵")

            Dim z = labels.Data
            Dim x = logits.Data
            Dim r(x.Length - 1) As Half

            For i = 0 To x.Length - 1
                ' 稳定写法: max(x,0) - x*z + log(1 + exp(-|x|))
                Dim xi As Double = CDbl(x(i))
                Dim zi As Double = CDbl(z(i))
                r(i) = CType(std.Max(xi, 0.0) - xi * zi + std.Log(1.0 + std.Exp(-std.Abs(xi))), Half)
            Next

            Return Wrap(r, labels.Shape)
        End Function

        Public Overridable Function MseLoss(predictions As TensorH, targets As TensorH) As TensorH Implements ITensorComputeH.MseLoss
            RequireSameShape(predictions, targets, "计算均方误差")

            Dim p = predictions.Data
            Dim t = targets.Data
            Dim sumSq As Double = 0

            For i = 0 To p.Length - 1
                Dim diff As Double = CDbl(p(i)) - CDbl(t(i))
                sumSq += diff * diff
            Next

            ' 与两条既有栈严格一致：返回的是**均值标量**而不是逐元素误差
            Return TensorH.Scalar(CType(sumSq / p.Length, Half))
        End Function

        Public Overridable Function L2Loss(t As TensorH) As TensorH Implements ITensorComputeH.L2Loss
            Dim src = t.Data
            Dim sumSq As Double = 0
            For i = 0 To src.Length - 1
                sumSq += CDbl(src(i)) * CDbl(src(i))
            Next
            Return TensorH.Scalar(CType(sumSq * 0.5, Half))
        End Function

        Public Overridable Function HuberLoss(predictions As TensorH, targets As TensorH, delta As Half) As TensorH Implements ITensorComputeH.HuberLoss
            RequireSameShape(predictions, targets, "计算 Huber 损失")

            Dim p = predictions.Data
            Dim t = targets.Data
            Dim d As Double = CDbl(delta)
            Dim totalLoss As Double = 0

            For i = 0 To p.Length - 1
                Dim diff As Double = std.Abs(CDbl(p(i)) - CDbl(t(i)))

                If diff <= d Then
                    totalLoss += 0.5 * diff * diff
                Else
                    totalLoss += d * (diff - 0.5 * d)
                End If
            Next

            ' 与两条既有栈一致：先把每段损失求和再平均，结果是一个标量
            Return TensorH.Scalar(CType(totalLoss / p.Length, Half))
        End Function

#End Region

#Region "训练算子"

        Public Overridable Function MaskedCrossEntropy(logits As TensorH,
                                                       targets As Integer(),
                                                       mask As Boolean(),
                                                       ByRef dLogits As TensorH) As Double Implements ITensorComputeH.MaskedCrossEntropy
            If logits Is Nothing Then Throw New ArgumentNullException(NameOf(logits))
            If logits.Rank <> 2 Then Throw New ArgumentException("掩码交叉熵要求 [rows, vocab] 的二维 logits")

            Dim rows = logits.Shape(0)
            Dim vocab = logits.Shape(1)

            dLogits = New TensorH(New Integer() {rows, vocab})

            Dim src = logits.Data
            Dim grad = dLogits.Data
            Dim total As Double = 0.0
            Dim count As Integer = 0

            For r As Integer = 0 To rows - 1
                If mask IsNot Nothing AndAlso r < mask.Length AndAlso Not mask(r) Then Continue For
                If targets Is Nothing OrElse r >= targets.Length Then Continue For

                Dim target = targets(r)
                If target < 0 OrElse target >= vocab Then Continue For

                count += 1

                Dim offset = r * vocab
                Dim maxVal As Double = Double.NegativeInfinity

                For j As Integer = 0 To vocab - 1
                    Dim v As Double = CDbl(src(offset + j))
                    If v > maxVal Then maxVal = v
                Next

                Dim sumExp As Double = 0.0
                For j As Integer = 0 To vocab - 1
                    Dim e = std.Exp(CDbl(src(offset + j)) - maxVal)
                    grad(offset + j) = CType(e, Half)
                    sumExp += e
                Next

                If sumExp <= 0 Then sumExp = 1.0

                For j As Integer = 0 To vocab - 1
                    grad(offset + j) = CType(CDbl(grad(offset + j)) / sumExp, Half)
                Next

                Dim pt As Double = CDbl(grad(offset + target))
                total -= std.Log(std.Max(pt, 1.0E-12))
                grad(offset + target) = CType(pt - 1.0, Half)
            Next

            If count = 0 Then
                Call dLogits.MarkHostModified()
                Return 0.0
            End If

            ' 归一化：损失与梯度都除以有效位置数，使不同 batch 的损失可比
            Dim inv As Double = 1.0 / count

            For i = 0 To grad.Length - 1
                grad(i) = CType(CDbl(grad(i)) * inv, Half)
            Next

            Call dLogits.MarkHostModified()

            Return total * inv
        End Function

        ''' <summary>
        ''' 标量兜底不做设备端 AdamW，返回 <c>False</c> 让调用方走主机循环
        ''' （与两条既有栈的约定一致）。
        ''' </summary>
        Public Overridable Function TryAdamWStep(param As TensorH, gradient As TensorH,
                                                 momentum As TensorH, velocity As TensorH,
                                                 learningRate As Double, beta1 As Double, beta2 As Double,
                                                 eps As Double, biasCorrection1 As Double,
                                                 biasCorrection2 As Double,
                                                 weightDecay As Double) As Boolean Implements ITensorComputeH.TryAdamWStep
            Return False
        End Function

        ''' <summary>默认后端没有"设备常驻"概念。</summary>
        Public Overridable ReadOnly Property SupportsDeviceResidency As Boolean Implements ITensorComputeH.SupportsDeviceResidency
            Get
                Return False
            End Get
        End Property

        ''' <summary>默认后端不支持钉住。</summary>
        Public Overridable Function PinDevice(t As TensorH, label As String, zeroFill As Boolean) As Boolean Implements ITensorComputeH.PinDevice
            Return False
        End Function

        ''' <summary>默认后端没有常驻缓冲。</summary>
        Public Overridable Function UnpinDevice(t As TensorH) As Boolean Implements ITensorComputeH.UnpinDevice
            Return False
        End Function

        ''' <summary>默认后端没有任何张量被钉住。</summary>
        Public Overridable Function IsDevicePinned(t As TensorH) As Boolean Implements ITensorComputeH.IsDevicePinned
            Return False
        End Function

        ''' <summary>默认后端不占用显存。</summary>
        Public Overridable ReadOnly Property PinnedDeviceBytes As Long Implements ITensorComputeH.PinnedDeviceBytes
            Get
                Return 0L
            End Get
        End Property

        ''' <summary>默认后端没有设备副本，因此无需同步。</summary>
        Public Overridable Function SyncFromDevice(t As TensorH) As Boolean Implements ITensorComputeH.SyncFromDevice
            Return False
        End Function

#End Region

#Region "卷积与池化"

        ' ------------------------------------------------------------------
        ' 标量参考实现（正确性优先），与两条既有栈的 Conv2D / MaxPool2D 逐行列对应，
        ' 唯一差异是累加器用 Double、存储落到 Half。
        ' ------------------------------------------------------------------

        ''' <summary>卷积/池化的输出边长: (input + 2*padding - kernel) / stride + 1</summary>
        Protected Shared Function ConvOutSizeH(inputSize As Integer, kernelSize As Integer,
                                               stride As Integer, padding As Integer) As Integer
            If stride <= 0 Then Throw New ArgumentException("stride 必须为正数")
            Return (inputSize + 2 * padding - kernelSize) \ stride + 1
        End Function

        ''' <summary>卷积/池化的入参形状校验</summary>
        Private Shared Sub RequireRank4H(t As TensorH, name As String)
            If t Is Nothing OrElse t.Rank <> 4 Then
                Throw New ArgumentException($"{name} 需要四维张量 (N,H,W,C)")
            End If
        End Sub

        Public Overridable Function Conv2D(x As TensorH, filters As TensorH, bias As TensorH,
                                           stride As Integer, padding As Integer) As TensorH Implements ITensorComputeH.Conv2D
            RequireRank4H(x, "x")
            RequireRank4H(filters, "filters")

            Dim batch = x.Shape(0), inH = x.Shape(1), inW = x.Shape(2), inC = x.Shape(3)
            Dim filtH = filters.Shape(0), filtW = filters.Shape(1)
            Dim filtC = filters.Shape(2), outC = filters.Shape(3)

            If filtC <> inC Then
                Throw New ArgumentException($"卷积核通道数 {filtC} 与输入通道数 {inC} 不一致")
            End If
            If bias IsNot Nothing AndAlso bias.Length <> outC Then
                Throw New ArgumentException($"偏置长度 {bias.Length} 与输出通道数 {outC} 不一致")
            End If

            Dim outH = ConvOutSizeH(inH, filtH, stride, padding)
            Dim outW = ConvOutSizeH(inW, filtW, stride, padding)

            If outH <= 0 OrElse outW <= 0 Then
                Throw New ArgumentException($"卷积输出尺寸非法: {outH}x{outW}")
            End If

            Dim result = New TensorH(batch, outH, outW, outC)
            Dim src = x.Data
            Dim kern = filters.Data
            Dim dst = result.Data
            Dim biasData = If(bias Is Nothing, Nothing, bias.Data)

            For n As Integer = 0 To batch - 1
                For oh As Integer = 0 To outH - 1
                    For ow As Integer = 0 To outW - 1
                        For oc As Integer = 0 To outC - 1
                            Dim sum As Double = If(biasData Is Nothing, 0.0, CDbl(biasData(oc)))

                            For kh As Integer = 0 To filtH - 1
                                Dim ih As Integer = oh * stride + kh - padding
                                If ih < 0 OrElse ih >= inH Then Continue For

                                For kw As Integer = 0 To filtW - 1
                                    Dim iw As Integer = ow * stride + kw - padding
                                    If iw < 0 OrElse iw >= inW Then Continue For

                                    Dim xBase = ((n * inH + ih) * inW + iw) * inC
                                    Dim kBase = ((kh * filtW + kw) * inC) * outC + oc

                                    For c As Integer = 0 To inC - 1
                                        sum += CDbl(src(xBase + c)) * CDbl(kern(kBase + c * outC))
                                    Next
                                Next
                            Next

                            dst(((n * outH + oh) * outW + ow) * outC + oc) = CType(sum, Half)
                        Next
                    Next
                Next
            Next

            Return result
        End Function

        Public Overridable Function Conv2DBackwardInput(gradOutput As TensorH, filters As TensorH,
                                                        inputShape As Integer(), stride As Integer,
                                                        padding As Integer) As TensorH Implements ITensorComputeH.Conv2DBackwardInput
            RequireRank4H(gradOutput, "gradOutput")
            RequireRank4H(filters, "filters")

            Dim batch = gradOutput.Shape(0), outH = gradOutput.Shape(1)
            Dim outW = gradOutput.Shape(2), outC = gradOutput.Shape(3)
            Dim filtH = filters.Shape(0), filtW = filters.Shape(1)
            Dim filtC = filters.Shape(2)

            Dim inH = inputShape(1), inW = inputShape(2), inC = inputShape(3)

            If filtC <> inC Then
                Throw New ArgumentException($"卷积核通道数 {filtC} 与输入通道数 {inC} 不一致")
            End If

            Dim result = New TensorH(batch, inH, inW, inC)
            Dim g = gradOutput.Data
            Dim kern = filters.Data
            Dim dst = result.Data

            For n As Integer = 0 To batch - 1
                For ih As Integer = 0 To inH - 1
                    For iw As Integer = 0 To inW - 1
                        For ic As Integer = 0 To inC - 1
                            Dim sum As Double = 0.0

                            For oh As Integer = 0 To outH - 1
                                Dim kh As Integer = ih - oh * stride + padding
                                If kh < 0 OrElse kh >= filtH Then Continue For

                                For ow As Integer = 0 To outW - 1
                                    Dim kw As Integer = iw - ow * stride + padding
                                    If kw < 0 OrElse kw >= filtW Then Continue For

                                    Dim gBase = ((n * outH + oh) * outW + ow) * outC

                                    For oc As Integer = 0 To outC - 1
                                        sum += CDbl(g(gBase + oc)) * CDbl(kern(((kh * filtW + kw) * inC + ic) * outC + oc))
                                    Next
                                Next
                            Next

                            dst(((n * inH + ih) * inW + iw) * inC + ic) = CType(sum, Half)
                        Next
                    Next
                Next
            Next

            Return result
        End Function

        Public Overridable Function Conv2DBackwardFilter(gradOutput As TensorH, x As TensorH,
                                                         filterShape As Integer(), stride As Integer,
                                                         padding As Integer) As TensorH Implements ITensorComputeH.Conv2DBackwardFilter
            RequireRank4H(gradOutput, "gradOutput")
            RequireRank4H(x, "x")

            Dim batch = gradOutput.Shape(0), outH = gradOutput.Shape(1)
            Dim outW = gradOutput.Shape(2), outC = gradOutput.Shape(3)
            Dim filtH = filterShape(0), filtW = filterShape(1)
            Dim inC = filterShape(2)
            Dim inH = x.Shape(1), inW = x.Shape(2)

            If x.Shape(3) <> inC Then
                Throw New ArgumentException($"输入通道数 {x.Shape(3)} 与卷积核通道数 {inC} 不一致")
            End If

            Dim result = New TensorH(filtH, filtW, inC, outC)
            Dim g = gradOutput.Data
            Dim src = x.Data
            Dim dst = result.Data

            For kh As Integer = 0 To filtH - 1
                For kw As Integer = 0 To filtW - 1
                    For ic As Integer = 0 To inC - 1
                        For oc As Integer = 0 To outC - 1
                            Dim sum As Double = 0.0

                            For n As Integer = 0 To batch - 1
                                For oh As Integer = 0 To outH - 1
                                    Dim ih As Integer = oh * stride + kh - padding
                                    If ih < 0 OrElse ih >= inH Then Continue For

                                    For ow As Integer = 0 To outW - 1
                                        Dim iw As Integer = ow * stride + kw - padding
                                        If iw < 0 OrElse iw >= inW Then Continue For

                                        sum += CDbl(g(((n * outH + oh) * outW + ow) * outC + oc)) *
                                               CDbl(src(((n * inH + ih) * inW + iw) * inC + ic))
                                    Next
                                Next
                            Next

                            dst(((kh * filtW + kw) * inC + ic) * outC + oc) = CType(sum, Half)
                        Next
                    Next
                Next
            Next

            Return result
        End Function

        Public Overridable Function Conv2DBackwardBias(gradOutput As TensorH) As TensorH Implements ITensorComputeH.Conv2DBackwardBias
            RequireRank4H(gradOutput, "gradOutput")

            Dim batch = gradOutput.Shape(0), outH = gradOutput.Shape(1)
            Dim outW = gradOutput.Shape(2), outC = gradOutput.Shape(3)

            Dim result = New TensorH(New Integer() {outC})
            Dim g = gradOutput.Data
            Dim dst = result.Data

            For n As Integer = 0 To batch - 1
                For oh As Integer = 0 To outH - 1
                    For ow As Integer = 0 To outW - 1
                        Dim baseIndex = ((n * outH + oh) * outW + ow) * outC

                        For oc As Integer = 0 To outC - 1
                            dst(oc) = CType(CDbl(dst(oc)) + CDbl(g(baseIndex + oc)), Half)
                        Next
                    Next
                Next
            Next

            Return result
        End Function

        Public Overridable Function MaxPool2D(x As TensorH, size As Integer, stride As Integer, padding As Integer,
                                              ByRef argMax As TensorH) As TensorH Implements ITensorComputeH.MaxPool2D
            RequireRank4H(x, "x")

            If size <= 0 Then Throw New ArgumentException("池化窗口必须为正数")

            Dim batch = x.Shape(0), inH = x.Shape(1), inW = x.Shape(2), inC = x.Shape(3)
            Dim outH = ConvOutSizeH(inH, size, stride, padding)
            Dim outW = ConvOutSizeH(inW, size, stride, padding)

            If outH <= 0 OrElse outW <= 0 Then
                Throw New ArgumentException($"池化输出尺寸非法: {outH}x{outW}")
            End If

            Dim result = New TensorH(batch, outH, outW, inC)
            Dim idx = New TensorH(batch, outH, outW, inC)
            Dim src = x.Data
            Dim dst = result.Data
            Dim dstIdx = idx.Data

            For n As Integer = 0 To batch - 1
                For oh As Integer = 0 To outH - 1
                    For ow As Integer = 0 To outW - 1
                        For c As Integer = 0 To inC - 1
                            ' 越界的窗口位置直接跳过（等价于 -inf），避免零填充在负数输入时错误胜出
                            Dim best As Half = Half.NegativeInfinity
                            Dim bestIdx As Integer = -1

                            For kh As Integer = 0 To size - 1
                                Dim ih As Integer = oh * stride + kh - padding
                                If ih < 0 OrElse ih >= inH Then Continue For

                                For kw As Integer = 0 To size - 1
                                    Dim iw As Integer = ow * stride + kw - padding
                                    If iw < 0 OrElse iw >= inW Then Continue For

                                    Dim flat = ((n * inH + ih) * inW + iw) * inC + c
                                    Dim v = src(flat)

                                    If v > best Then
                                        best = v
                                        bestIdx = flat
                                    End If
                                Next
                            Next

                            Dim outIndex = ((n * outH + oh) * outW + ow) * inC + c

                            If bestIdx < 0 Then
                                ' 整个窗口都落在填充区（极小输入 + 大 padding 才会出现）
                                dst(outIndex) = CType(0.0F, Half)
                                dstIdx(outIndex) = CType(0.0F, Half)
                            Else
                                dst(outIndex) = best
                                dstIdx(outIndex) = CType(CSng(bestIdx), Half)
                            End If
                        Next
                    Next
                Next
            Next

            argMax = idx

            Return result
        End Function

        Public Overridable Function MaxPool2DBackward(gradOutput As TensorH, argMax As TensorH,
                                                      inputShape As Integer()) As TensorH Implements ITensorComputeH.MaxPool2DBackward
            RequireRank4H(gradOutput, "gradOutput")
            If argMax Is Nothing Then Throw New ArgumentNullException(NameOf(argMax))

            Dim result = New TensorH(inputShape)
            Dim g = gradOutput.Data
            Dim pos = argMax.Data
            Dim dst = result.Data

            ' 每个输出位置唯一对应一个输入位置，因此可以直接累加而无需原子操作
            For i As Integer = 0 To g.Length - 1
                Dim target As Integer = CInt(CSng(pos(i)))

                If target >= 0 AndAlso target < dst.Length Then
                    dst(target) = CType(CSng(dst(target)) + CSng(g(i)), Half)
                End If
            Next

            Return result
        End Function

#End Region

#Region "内部工具（归约 / 极值下标）"

        ''' <summary>整体极值下标。</summary>
        Private Shared Function ArgGlobalH(t As TensorH, findMin As Boolean) As Integer
            Dim src = t.Data
            If src.Length = 0 Then Throw New ArgumentException("空张量没有极值下标")

            Dim bestIdx = 0
            Dim bestVal = src(0)

            For i = 1 To src.Length - 1
                If findMin Then
                    If src(i) < bestVal Then
                        bestVal = src(i)
                        bestIdx = i
                    End If
                Else
                    If src(i) > bestVal Then
                        bestVal = src(i)
                        bestIdx = i
                    End If
                End If
            Next

            Return bestIdx
        End Function

        ''' <summary>沿指定轴归约。累加 / 极值比较统一提升到 Double，出口再降回 Half。</summary>
        Protected Shared Function ReduceAlongAxisH(t As TensorH, axis As Integer, keepdims As Boolean,
                                                   initValue As Double,
                                                   reduceFunc As Func(Of Double, Double, Double)) As TensorH
            Dim rank = t.Rank
            Dim origShape = t.Shape

            Dim outerSize = 1
            For i = 0 To axis - 1
                outerSize *= origShape(i)
            Next

            Dim innerSize = 1
            For i = axis + 1 To rank - 1
                innerSize *= origShape(i)
            Next

            Dim axisSize = origShape(axis)

            Dim outShapeList = New List(Of Integer)
            For i = 0 To rank - 1
                If i = axis Then
                    If keepdims Then outShapeList.Add(1)
                Else
                    outShapeList.Add(origShape(i))
                End If
            Next

            If outShapeList.Count = 0 Then
                Return TensorH.Scalar(CType(initValue, Half))
            End If

            Dim result = New TensorH(outShapeList.ToArray())
            Dim srcData = t.Data
            Dim dstData = result.Data
            Dim dstIdx = 0

            For outer = 0 To outerSize - 1
                Dim baseIdx = outer * axisSize * innerSize
                For inner = 0 To innerSize - 1
                    Dim value = initValue
                    For k = 0 To axisSize - 1
                        value = reduceFunc(value, CDbl(srcData(baseIdx + k * innerSize + inner)))
                    Next
                    dstData(dstIdx) = CType(value, Half)
                    dstIdx += 1
                Next
            Next

            Return result
        End Function

        ''' <summary>沿指定轴找极值下标（argmax / argmin）。</summary>
        Protected Shared Function ReduceArgAxisH(t As TensorH, axis As Integer, findMin As Boolean) As TensorH
            Dim rank = t.Rank
            Dim origShape = t.Shape

            Dim outerSize = 1
            For i = 0 To axis - 1
                outerSize *= origShape(i)
            Next

            Dim innerSize = 1
            For i = axis + 1 To rank - 1
                innerSize *= origShape(i)
            Next

            Dim axisSize = origShape(axis)

            Dim outShapeList = New List(Of Integer)
            For i = 0 To rank - 1
                If i <> axis Then outShapeList.Add(origShape(i))
            Next

            Dim result = New TensorH(outShapeList.ToArray())
            Dim srcData = t.Data
            Dim dstData = result.Data
            Dim dstIdx = 0

            For outer = 0 To outerSize - 1
                Dim baseIdx = outer * axisSize * innerSize
                For inner = 0 To innerSize - 1
                    Dim bestVal As Half = srcData(baseIdx + inner)
                    Dim bestK = 0
                    For k = 1 To axisSize - 1
                        Dim val = srcData(baseIdx + k * innerSize + inner)
                        If (findMin AndAlso val < bestVal) OrElse (Not findMin AndAlso val > bestVal) Then
                            bestVal = val
                            bestK = k
                        End If
                    Next
                    dstData(dstIdx) = CType(CSng(bestK), Half)
                    dstIdx += 1
                Next
            Next

            Return result
        End Function

#End Region

#Region "CFD 扩展原语（CPU 参考实现）"

        ''' <summary>
        ''' 七点拉普拉斯：out = Σ(流体邻居) - nFluid·中心。固体单元输出 0。
        ''' </summary>
        Public Overridable Function Laplacian7(src As TensorH, mask As Byte(),
                                               nFluid As Byte()) As TensorH Implements ITensorComputeH.Laplacian7
            Dim shp = src.Shape
            Dim nx = shp(0), ny = shp(1), nz = shp(2)
            Dim x = src.Data
            Dim r = New Half(x.Length - 1) {}
            Dim plane = ny * nz

            For i = 1 To nx - 2
                For j = 1 To ny - 2
                    Dim baseIdx = i * plane + j * nz
                    For k = 1 To nz - 2
                        Dim idx = baseIdx + k
                        If mask IsNot Nothing AndAlso mask(idx) = 0 Then Continue For

                        Dim s As Double = 0
                        If mask Is Nothing OrElse mask(idx - plane) <> 0 Then s += CDbl(x(idx - plane))
                        If mask Is Nothing OrElse mask(idx + plane) <> 0 Then s += CDbl(x(idx + plane))
                        If mask Is Nothing OrElse mask(idx - nz) <> 0 Then s += CDbl(x(idx - nz))
                        If mask Is Nothing OrElse mask(idx + nz) <> 0 Then s += CDbl(x(idx + nz))
                        If mask Is Nothing OrElse mask(idx - 1) <> 0 Then s += CDbl(x(idx - 1))
                        If mask Is Nothing OrElse mask(idx + 1) <> 0 Then s += CDbl(x(idx + 1))

                        Dim div = If(nFluid Is Nothing, 6.0, CDbl(nFluid(idx)))
                        r(idx) = CType(s - div * CDbl(x(idx)), Half)
                    Next
                Next
            Next

            Return Wrap(r, shp)
        End Function

        ''' <summary>
        ''' 带掩膜的七点 Jacobi 迭代：x_new = (alpha·Σ_fluid x_old + rhs) / denom，
        ''' 其中 denom = beta（beta &gt; 0 时）或该格流体邻居数 nFluid（beta &lt;= 0 时）。
        ''' 固体单元恒为 0。
        ''' </summary>
        Public Overridable Function JacobiStencil7(rhs As TensorH, mask As Byte(), nFluid As Byte(),
                                                   alpha As Half, beta As Half,
                                                   iterations As Integer,
                                                   Optional initial As TensorH = Nothing) As TensorH Implements ITensorComputeH.JacobiStencil7
            Dim shp = rhs.Shape
            Dim nx = shp(0), ny = shp(1), nz = shp(2)
            Dim n = rhs.Length
            Dim plane = ny * nz
            Dim b = rhs.Data

            Dim cur = New Half(n - 1) {}
            If initial IsNot Nothing Then Array.Copy(initial.Data, cur, n)
            Dim prev = New Half(n - 1) {}

            For iter = 0 To iterations - 1
                Array.Copy(cur, prev, n)

                For i = 1 To nx - 2
                    For j = 1 To ny - 2
                        Dim baseIdx = i * plane + j * nz
                        For k = 1 To nz - 2
                            Dim idx = baseIdx + k
                            If mask IsNot Nothing AndAlso mask(idx) = 0 Then
                                cur(idx) = CType(0.0F, Half)
                                Continue For
                            End If

                            Dim s As Double = 0
                            If mask Is Nothing OrElse mask(idx - plane) <> 0 Then s += CDbl(prev(idx - plane))
                            If mask Is Nothing OrElse mask(idx + plane) <> 0 Then s += CDbl(prev(idx + plane))
                            If mask Is Nothing OrElse mask(idx - nz) <> 0 Then s += CDbl(prev(idx - nz))
                            If mask Is Nothing OrElse mask(idx + nz) <> 0 Then s += CDbl(prev(idx + nz))
                            If mask Is Nothing OrElse mask(idx - 1) <> 0 Then s += CDbl(prev(idx - 1))
                            If mask Is Nothing OrElse mask(idx + 1) <> 0 Then s += CDbl(prev(idx + 1))

                            Dim denom As Double = If(CDbl(beta) > 0.0,
                                                     CDbl(beta),
                                                     If(nFluid Is Nothing, 6.0, CDbl(nFluid(idx))))
                            If denom = 0.0 Then denom = 1.0

                            cur(idx) = CType((CDbl(alpha) * s + CDbl(b(idx))) / denom, Half)
                        Next
                    Next
                Next
            Next

            Return Wrap(cur, shp)
        End Function

        ''' <summary>
        ''' 半拉格朗日平流：逐格反向追踪 dt 时间后在源场做三线性采样。
        ''' </summary>
        Public Overridable Function AdvectTrilinear(src As TensorH,
                                                    u As TensorH, v As TensorH, w As TensorH,
                                                    dt As Half, mask As Byte()) As TensorH Implements ITensorComputeH.AdvectTrilinear
            Dim shp = src.Shape
            Dim nx = shp(0), ny = shp(1), nz = shp(2)
            Dim plane = ny * nz
            Dim x = src.Data, uu = u.Data, vv = v.Data, ww = w.Data
            Dim r = New Half(x.Length - 1) {}
            Dim dtv As Double = CDbl(dt)

            For i = 0 To nx - 1
                For j = 0 To ny - 1
                    Dim baseIdx = i * plane + j * nz
                    For k = 0 To nz - 1
                        Dim idx = baseIdx + k
                        If mask IsNot Nothing AndAlso mask(idx) = 0 Then
                            r(idx) = CType(0.0F, Half)
                            Continue For
                        End If

                        Dim px = std.Min(std.Max(i - dtv * CDbl(uu(idx)), 0.5), nx - 1.5)
                        Dim py = std.Min(std.Max(j - dtv * CDbl(vv(idx)), 0.5), ny - 1.5)
                        Dim pz = std.Min(std.Max(k - dtv * CDbl(ww(idx)), 0.5), nz - 1.5)

                        r(idx) = TrilinearAt(x, px, py, pz, nx, ny, nz, plane)
                    Next
                Next
            Next

            Return Wrap(r, shp)
        End Function

        ''' <summary>在连续坐标处对场做三线性插值采样（权重在 Double 域，场值 Half）。</summary>
        Protected Shared Function TrilinearAt(f As Half(), x As Double, y As Double, z As Double,
                                              nx As Integer, ny As Integer, nz As Integer,
                                              plane As Integer) As Half
            Dim i0 = CInt(std.Floor(x)), j0 = CInt(std.Floor(y)), k0 = CInt(std.Floor(z))
            Dim fx = x - i0, fy = y - j0, fz = z - k0

            Dim i1 = std.Min(i0 + 1, nx - 1)
            Dim j1 = std.Min(j0 + 1, ny - 1)
            Dim k1 = std.Min(k0 + 1, nz - 1)
            i0 = std.Min(std.Max(i0, 0), nx - 1)
            j0 = std.Min(std.Max(j0, 0), ny - 1)
            k0 = std.Min(std.Max(k0, 0), nz - 1)

            Dim a = i0 * plane + j0 * nz
            Dim b = i0 * plane + j1 * nz
            Dim c = i1 * plane + j0 * nz
            Dim d = i1 * plane + j1 * nz

            Dim c000 = CDbl(f(a + k0)), c001 = CDbl(f(a + k1))
            Dim c010 = CDbl(f(b + k0)), c011 = CDbl(f(b + k1))
            Dim c100 = CDbl(f(c + k0)), c101 = CDbl(f(c + k1))
            Dim c110 = CDbl(f(d + k0)), c111 = CDbl(f(d + k1))

            Dim c00 = c000 * (1 - fx) + c100 * fx
            Dim c01 = c001 * (1 - fx) + c101 * fx
            Dim c10 = c010 * (1 - fx) + c110 * fx
            Dim c11 = c011 * (1 - fx) + c111 * fx

            Dim c0 = c00 * (1 - fy) + c10 * fy
            Dim c1 = c01 * (1 - fy) + c11 * fy

            Return CType(c0 * (1 - fz) + c1 * fz, Half)
        End Function

        ''' <summary>按掩膜把所有给定场的固体单元置零。</summary>
        Public Overridable Sub ApplyMask(mask As Byte(), ParamArray fields As TensorH()) Implements ITensorComputeH.ApplyMask
            If mask Is Nothing OrElse fields Is Nothing Then Return

            For Each f In fields
                If f Is Nothing Then Continue For
                Dim d = f.Data
                For idx = 0 To d.Length - 1
                    If mask(idx) = 0 Then d(idx) = CType(0.0F, Half)
                Next
            Next
        End Sub

#End Region

    End Class

End Namespace
