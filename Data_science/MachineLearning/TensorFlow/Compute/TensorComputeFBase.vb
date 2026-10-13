#Region "Microsoft.VisualBasic::3f6a4e334d69d4070a65ed642abdc0ea, Data_science\MachineLearning\TensorFlow\Compute\TensorComputeFBase.vb"

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

    '   Total Lines: 437
    '    Code Lines: 296 (67.73%)
    ' Comment Lines: 62 (14.19%)
    '    - Xml Docs: 74.19%
    ' 
    '   Blank Lines: 79 (18.08%)
    '     File Size: 19.02 KB


    '     Class TensorComputeFBase
    ' 
    '         Function: Abs, Add, AddScalar, AdvectTrilinear, Clip
    '                   Divide, DivideScalar, Exp, JacobiStencil7, Laplacian7
    '                   Log, MapBinary, MapUnary, MatMul, MaxAll
    '                   Maximum, MeanAll, MinAll, Minimum, Multiply
    '                   MultiplyScalar, Negate, Reciprocal, Relu, Sigmoid
    '                   Sqrt, Square, Subtract, SumAll, Tanh
    '                   TrilinearAt, Wrap
    ' 
    '         Sub: ApplyMask, RequireSameShape
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' ITensorComputeF 的标量兜底实现
'
' 与双精度栈的 TensorComputeBase 定位完全相同：给出**语义正确**的参考实现，
' 具体后端（SIMDTensorF / 未来的 CudaTensorF）只需要重写"确实有收益"的算子。
'
' 本文件同时给出 CFD 三个原语的 CPU 参考实现。它们不是死代码：
'   * 是 GPU 后端移植时的语义基准（GPU 结果必须与这里逐点比对）
'   * 是未来把 StableFluidsSolver 整个下放到后端时的 CPU 回退路径
'
' 网格布局约定（与 CFDEngine / VoxelShape 严格一致）：
'       idx = (i * ny + j) * nz + k
'   * (i±1, j, k) -> idx ± ny*nz
'   * (i, j±1, k) -> idx ± nz
'   * (i, j, k±1) -> idx ± 1
' ---------------------------------------------------------------------------

Imports std = System.Math

Namespace Compute

    ''' <summary>
    ''' <see cref="ITensorComputeF"/> 的标量兜底实现（CPU 参考实现）。
    ''' </summary>
    Public MustInherit Class TensorComputeFBase
        Implements ITensorComputeF

        ''' <summary>后端名称</summary>
        Public MustOverride ReadOnly Property Name As String Implements ITensorComputeF.Name

#Region "helpers"

        ''' <summary>校验两个张量形状一致，不一致则抛出。</summary>
        Protected Shared Sub RequireSameShape(a As TensorF, b As TensorF, op As String)
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
        Protected Shared Function Wrap(data As Single(), shape As Integer()) As TensorF
            Return TensorF.Wrap(data, shape)
        End Function

        ''' <summary>逐元素二元映射。</summary>
        Protected Shared Function MapBinary(a As TensorF, b As TensorF,
                                            f As Func(Of Single, Single, Single)) As TensorF
            RequireSameShape(a, b, "逐元素二元运算")
            Dim x = a.Data, y = b.Data
            Dim r(x.Length - 1) As Single
            For i = 0 To x.Length - 1
                r(i) = f(x(i), y(i))
            Next
            Return Wrap(r, a.Shape)
        End Function

        ''' <summary>逐元素一元映射。</summary>
        Protected Shared Function MapUnary(t As TensorF, f As Func(Of Single, Single)) As TensorF
            Dim x = t.Data
            Dim r(x.Length - 1) As Single
            For i = 0 To x.Length - 1
                r(i) = f(x(i))
            Next
            Return Wrap(r, t.Shape)
        End Function

#End Region

#Region "逐元素 - 二元"

        ''' <summary>逐元素相加</summary>
        Public Overridable Function Add(a As TensorF, b As TensorF) As TensorF Implements ITensorComputeF.Add
            Return MapBinary(a, b, Function(x, y) x + y)
        End Function

        ''' <summary>逐元素相减</summary>
        Public Overridable Function Subtract(a As TensorF, b As TensorF) As TensorF Implements ITensorComputeF.Subtract
            Return MapBinary(a, b, Function(x, y) x - y)
        End Function

        ''' <summary>逐元素相乘</summary>
        Public Overridable Function Multiply(a As TensorF, b As TensorF) As TensorF Implements ITensorComputeF.Multiply
            Return MapBinary(a, b, Function(x, y) x * y)
        End Function

        ''' <summary>逐元素相除</summary>
        Public Overridable Function Divide(a As TensorF, b As TensorF) As TensorF Implements ITensorComputeF.Divide
            Return MapBinary(a, b, Function(x, y) x / y)
        End Function

        ''' <summary>逐元素取较大值</summary>
        Public Overridable Function Maximum(a As TensorF, b As TensorF) As TensorF Implements ITensorComputeF.Maximum
            Return MapBinary(a, b, Function(x, y) std.Max(x, y))
        End Function

        ''' <summary>逐元素取较小值</summary>
        Public Overridable Function Minimum(a As TensorF, b As TensorF) As TensorF Implements ITensorComputeF.Minimum
            Return MapBinary(a, b, Function(x, y) std.Min(x, y))
        End Function

#End Region

#Region "逐元素 - 一元"

        ''' <summary>逐元素指数</summary>
        Public Overridable Function Exp(t As TensorF) As TensorF Implements ITensorComputeF.Exp
            Return MapUnary(t, Function(x) CSng(std.Exp(x)))
        End Function

        ''' <summary>逐元素自然对数</summary>
        Public Overridable Function Log(t As TensorF) As TensorF Implements ITensorComputeF.Log
            Return MapUnary(t, Function(x) CSng(std.Log(x)))
        End Function

        ''' <summary>逐元素平方根</summary>
        Public Overridable Function Sqrt(t As TensorF) As TensorF Implements ITensorComputeF.Sqrt
            Return MapUnary(t, Function(x) CSng(std.Sqrt(x)))
        End Function

        ''' <summary>逐元素平方</summary>
        Public Overridable Function Square(t As TensorF) As TensorF Implements ITensorComputeF.Square
            Return MapUnary(t, Function(x) x * x)
        End Function

        ''' <summary>逐元素绝对值</summary>
        Public Overridable Function Abs(t As TensorF) As TensorF Implements ITensorComputeF.Abs
            Return MapUnary(t, Function(x) std.Abs(x))
        End Function

        ''' <summary>逐元素取负</summary>
        Public Overridable Function Negate(t As TensorF) As TensorF Implements ITensorComputeF.Negate
            Return MapUnary(t, Function(x) -x)
        End Function

        ''' <summary>逐元素倒数</summary>
        Public Overridable Function Reciprocal(t As TensorF) As TensorF Implements ITensorComputeF.Reciprocal
            Return MapUnary(t, Function(x) 1.0F / x)
        End Function

        ''' <summary>逐元素双曲正切</summary>
        Public Overridable Function Tanh(t As TensorF) As TensorF Implements ITensorComputeF.Tanh
            Return MapUnary(t, Function(x) CSng(std.Tanh(x)))
        End Function

        ''' <summary>逐元素 Sigmoid</summary>
        Public Overridable Function Sigmoid(t As TensorF) As TensorF Implements ITensorComputeF.Sigmoid
            Return MapUnary(t, Function(x) 1.0F / (1.0F + CSng(std.Exp(-x))))
        End Function

        ''' <summary>逐元素 ReLU</summary>
        Public Overridable Function Relu(t As TensorF) As TensorF Implements ITensorComputeF.Relu
            Return MapUnary(t, Function(x) If(x > 0.0F, x, 0.0F))
        End Function

#End Region

#Region "标量运算"

        ''' <summary>逐元素加标量</summary>
        Public Overridable Function AddScalar(t As TensorF, scalar As Single) As TensorF Implements ITensorComputeF.AddScalar
            Return MapUnary(t, Function(x) x + scalar)
        End Function

        ''' <summary>逐元素乘标量</summary>
        Public Overridable Function MultiplyScalar(t As TensorF, scalar As Single) As TensorF Implements ITensorComputeF.MultiplyScalar
            Return MapUnary(t, Function(x) x * scalar)
        End Function

        ''' <summary>逐元素除标量</summary>
        Public Overridable Function DivideScalar(t As TensorF, scalar As Single) As TensorF Implements ITensorComputeF.DivideScalar
            Return MapUnary(t, Function(x) x / scalar)
        End Function

        ''' <summary>把元素钳制到 [low, high] 区间</summary>
        Public Overridable Function Clip(t As TensorF, low As Single, high As Single) As TensorF Implements ITensorComputeF.Clip
            Return MapUnary(t, Function(x) std.Min(std.Max(x, low), high))
        End Function

#End Region

#Region "矩阵运算"

        ''' <summary>二维矩阵乘法</summary>
        Public Overridable Function MatMul(a As TensorF, b As TensorF) As TensorF Implements ITensorComputeF.MatMul
            If a.Rank <> 2 OrElse b.Rank <> 2 Then
                Throw New ArgumentException("矩阵乘法需要二维张量")
            End If
            If a.Shape(1) <> b.Shape(0) Then
                Throw New ArgumentException($"矩阵维度不匹配: {a.Shape(1)} != {b.Shape(0)}")
            End If

            Dim m = a.Shape(0), k = a.Shape(1), n = b.Shape(1)
            Dim x = a.Data, y = b.Data
            Dim r(m * n - 1) As Single

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

            Return Wrap(r, New Integer() {m, n})
        End Function

        ''' <summary>二维张量转置</summary>
        Public Overridable Function Transpose(t As TensorF) As TensorF Implements ITensorComputeF.Transpose
            If t.Rank <> 2 Then
                Throw New ArgumentException("只支持二维张量转置")
            End If

            Dim rows = t.Shape(0)
            Dim cols = t.Shape(1)
            Dim result = New TensorF(cols, rows)
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
        ''' 稀疏 × 稠密矩阵乘的标量参考实现。逐点运算次序与双精度栈
        ''' <see cref="TensorComputeBase.SpMM"/> 完全一致，因此 CPU 与 GPU 后端可以对拍。
        ''' 因此在<span> <｜hy_place▁holder▁no▁813｜>可以逐比特对拍。
        ''' </summary>
        Public Overridable Function SpMM(csr As SparseCsr, dense As TensorF) As TensorF Implements ITensorComputeF.SpMM
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
                    Dim xv As Double = xd(ro + r)
                    If xv = 0.0 Then Continue For    ' 脉冲输入高度稀疏：跳过零源
                    Dim kEnd = rp(r + 1)
                    For k As Integer = rp(r) To kEnd - 1
                        acc(bo + ci(k)) += xv * vv(k)
                    Next
                Next
            Next

            Dim od(batch * cols - 1) As Single
            For i As Integer = 0 To od.Length - 1
                od(i) = CSng(acc(i))
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
        ''' 落到 <c>Single</c> 存储——这是刻意的设计：膜电位是逐时间步累积量，
        ''' 中间项的单精度舍入会改变阈值判定，历史上 SNN 的 CSng(Beta) 截断正是漂移根因。
        ''' </para>
        ''' </remarks>
        Public Overridable Function LifStep(synapses As SparseCsr,
                                            sPrev As TensorF,
                                            externalCurrent As TensorF,
                                            h As TensorF,
                                            s As TensorF,
                                            counts As TensorF,
                                            beta As Double,
                                            threshold As Double,
                                            subtractThreshold As Boolean) As Boolean Implements ITensorComputeF.LifStep
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
                    Dim xv As Double = sp(ro + i)
                    If xv = 0.0 Then Continue For
                    Dim kEnd = rp(i + 1)
                    For k As Integer = rp(i) To kEnd - 1
                        current(ci(k)) += xv * vv(k)
                    Next
                Next

                For i As Integer = 0 To units - 1
                    Dim idx = ro + i
                    Dim u As Double = beta * hp(idx) + current(i) + If(ext Is Nothing, 0.0, CDbl(ext(idx)))

                    If u >= threshold Then
                        sOut(idx) = 1.0F
                        hp(idx) = CSng(If(subtractThreshold, u - threshold, 0.0))
                    Else
                        sOut(idx) = 0.0F
                        hp(idx) = CSng(u)
                    End If

                    If cnt IsNot Nothing Then cnt(idx) += sOut(idx)
                Next
            Next

            Call h.MarkHostModified()
            Call s.MarkHostModified()
            If counts IsNot Nothing Then Call counts.MarkHostModified()

            Return True
        End Function

#End Region

#Region "归约运算"

        ''' <summary>整体求和（双精度累加，避免单精度大数组累加的舍入误差）</summary>
        Public Overridable Function SumAll(t As TensorF) As Double Implements ITensorComputeF.SumAll
            Dim x = t.Data
            Dim acc As Double = 0
            For i = 0 To x.Length - 1
                acc += x(i)
            Next
            Return acc
        End Function

        ''' <summary>整体求均值</summary>
        Public Overridable Function MeanAll(t As TensorF) As Double Implements ITensorComputeF.MeanAll
            If t.Length = 0 Then Return 0.0
            Return SumAll(t) / t.Length
        End Function

        ''' <summary>最小值</summary>
        Public Overridable Function MinAll(t As TensorF) As Single Implements ITensorComputeF.MinAll
            Dim x = t.Data
            If x.Length = 0 Then Return 0.0F
            Dim m = x(0)
            For i = 1 To x.Length - 1
                If x(i) < m Then m = x(i)
            Next
            Return m
        End Function

        ''' <summary>最大值</summary>
        Public Overridable Function MaxAll(t As TensorF) As Single Implements ITensorComputeF.MaxAll
            Dim x = t.Data
            If x.Length = 0 Then Return 0.0F
            Dim m = x(0)
            For i = 1 To x.Length - 1
                If x(i) > m Then m = x(i)
            Next
            Return m
        End Function

#End Region

#Region "CFD 扩展原语（CPU 参考实现）"

        ''' <summary>
        ''' 七点拉普拉斯：out = Σ(流体邻居) - nFluid·中心。固体单元输出 0。
        ''' </summary>
        Public Overridable Function Laplacian7(src As TensorF, mask As Byte(),
                                               nFluid As Byte()) As TensorF Implements ITensorComputeF.Laplacian7
            Dim shp = src.Shape
            Dim nx = shp(0), ny = shp(1), nz = shp(2)
            Dim x = src.Data
            Dim r = New Single(x.Length - 1) {}
            Dim plane = ny * nz

            For i = 1 To nx - 2
                For j = 1 To ny - 2
                    Dim baseIdx = i * plane + j * nz
                    For k = 1 To nz - 2
                        Dim idx = baseIdx + k
                        If mask IsNot Nothing AndAlso mask(idx) = 0 Then Continue For

                        Dim s As Double = 0
                        If mask Is Nothing OrElse mask(idx - plane) <> 0 Then s += x(idx - plane)
                        If mask Is Nothing OrElse mask(idx + plane) <> 0 Then s += x(idx + plane)
                        If mask Is Nothing OrElse mask(idx - nz) <> 0 Then s += x(idx - nz)
                        If mask Is Nothing OrElse mask(idx + nz) <> 0 Then s += x(idx + nz)
                        If mask Is Nothing OrElse mask(idx - 1) <> 0 Then s += x(idx - 1)
                        If mask Is Nothing OrElse mask(idx + 1) <> 0 Then s += x(idx + 1)

                        Dim div = If(nFluid Is Nothing, 6.0, CDbl(nFluid(idx)))
                        r(idx) = CSng(s - div * x(idx))
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
        Public Overridable Function JacobiStencil7(rhs As TensorF, mask As Byte(), nFluid As Byte(),
                                                   alpha As Single, beta As Single,
                                                   iterations As Integer,
                                                   Optional initial As TensorF = Nothing) As TensorF Implements ITensorComputeF.JacobiStencil7
            Dim shp = rhs.Shape
            Dim nx = shp(0), ny = shp(1), nz = shp(2)
            Dim n = rhs.Length
            Dim plane = ny * nz
            Dim b = rhs.Data

            Dim cur = New Single(n - 1) {}
            If initial IsNot Nothing Then Array.Copy(initial.Data, cur, n)
            Dim prev = New Single(n - 1) {}

            For iter = 0 To iterations - 1
                Array.Copy(cur, prev, n)

                For i = 1 To nx - 2
                    For j = 1 To ny - 2
                        Dim baseIdx = i * plane + j * nz
                        For k = 1 To nz - 2
                            Dim idx = baseIdx + k
                            If mask IsNot Nothing AndAlso mask(idx) = 0 Then
                                cur(idx) = 0.0F
                                Continue For
                            End If

                            Dim s As Double = 0
                            If mask Is Nothing OrElse mask(idx - plane) <> 0 Then s += prev(idx - plane)
                            If mask Is Nothing OrElse mask(idx + plane) <> 0 Then s += prev(idx + plane)
                            If mask Is Nothing OrElse mask(idx - nz) <> 0 Then s += prev(idx - nz)
                            If mask Is Nothing OrElse mask(idx + nz) <> 0 Then s += prev(idx + nz)
                            If mask Is Nothing OrElse mask(idx - 1) <> 0 Then s += prev(idx - 1)
                            If mask Is Nothing OrElse mask(idx + 1) <> 0 Then s += prev(idx + 1)

                            Dim denom As Double = If(beta > 0.0F,
                                                     CDbl(beta),
                                                     If(nFluid Is Nothing, 6.0, CDbl(nFluid(idx))))
                            If denom = 0.0 Then denom = 1.0

                            cur(idx) = CSng((alpha * s + b(idx)) / denom)
                        Next
                    Next
                Next
            Next

            Return Wrap(cur, shp)
        End Function

        ''' <summary>
        ''' 半拉格朗日平流：逐格反向追踪 dt 时间后在源场做三线性采样。
        ''' </summary>
        Public Overridable Function AdvectTrilinear(src As TensorF,
                                                    u As TensorF, v As TensorF, w As TensorF,
                                                    dt As Single, mask As Byte()) As TensorF Implements ITensorComputeF.AdvectTrilinear
            Dim shp = src.Shape
            Dim nx = shp(0), ny = shp(1), nz = shp(2)
            Dim plane = ny * nz
            Dim x = src.Data, uu = u.Data, vv = v.Data, ww = w.Data
            Dim r = New Single(x.Length - 1) {}

            For i = 0 To nx - 1
                For j = 0 To ny - 1
                    Dim baseIdx = i * plane + j * nz
                    For k = 0 To nz - 1
                        Dim idx = baseIdx + k
                        If mask IsNot Nothing AndAlso mask(idx) = 0 Then
                            r(idx) = 0.0F
                            Continue For
                        End If

                        Dim px = std.Min(std.Max(i - dt * uu(idx), 0.5), nx - 1.5)
                        Dim py = std.Min(std.Max(j - dt * vv(idx), 0.5), ny - 1.5)
                        Dim pz = std.Min(std.Max(k - dt * ww(idx), 0.5), nz - 1.5)

                        r(idx) = TrilinearAt(x, px, py, pz, nx, ny, nz, plane)
                    Next
                Next
            Next

            Return Wrap(r, shp)
        End Function

        ''' <summary>在连续坐标处对场做三线性插值采样。</summary>
        Protected Shared Function TrilinearAt(f As Single(), x As Double, y As Double, z As Double,
                                              nx As Integer, ny As Integer, nz As Integer,
                                              plane As Integer) As Single
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

            Dim c000 = f(a + k0), c001 = f(a + k1)
            Dim c010 = f(b + k0), c011 = f(b + k1)
            Dim c100 = f(c + k0), c101 = f(c + k1)
            Dim c110 = f(d + k0), c111 = f(d + k1)

            Dim c00 = c000 * (1 - fx) + c100 * fx
            Dim c01 = c001 * (1 - fx) + c101 * fx
            Dim c10 = c010 * (1 - fx) + c110 * fx
            Dim c11 = c011 * (1 - fx) + c111 * fx

            Dim c0 = c00 * (1 - fy) + c10 * fy
            Dim c1 = c01 * (1 - fy) + c11 * fy

            Return CSng(c0 * (1 - fz) + c1 * fz)
        End Function

        ''' <summary>按掩膜把所有给定场的固体单元置零。</summary>
        Public Overridable Sub ApplyMask(mask As Byte(), ParamArray fields As TensorF()) Implements ITensorComputeF.ApplyMask
            If mask Is Nothing OrElse fields Is Nothing Then Return

            For Each f In fields
                If f Is Nothing Then Continue For
                Dim d = f.Data
                For idx = 0 To d.Length - 1
                    If mask(idx) = 0 Then d(idx) = 0.0F
                Next
            Next
        End Sub

#End Region

    End Class

End Namespace
