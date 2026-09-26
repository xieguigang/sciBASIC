#Region "Microsoft.VisualBasic::40b8815b9390fa86abecbf5b3f30b0bf, Data_science\MachineLearning\TensorFlow\Compute\TensorComputeBase.vb"

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

    '   Total Lines: 1333
    '    Code Lines: 980 (73.52%)
    ' Comment Lines: 81 (6.08%)
    '    - Xml Docs: 59.26%
    ' 
    '   Blank Lines: 272 (20.41%)
    '     File Size: 56.04 KB


    '     Class TensorComputeBase
    ' 
    '         Properties: PinnedDeviceBytes, SupportsDeviceResidency
    ' 
    '         Function: Abs, Add, AddScalar, ArgGlobal, ArgMax
    '                   ArgMin, Clip, Concat, Conv2D, Conv2DBackwardBias
    '                   Conv2DBackwardFilter, Conv2DBackwardInput, ConvOutSize, Cos, Divide
    '                   DivideScalar, Elu, Exp, Gelu, Heaviside
    '                   HuberLoss, IsDevicePinned, L2Loss, L2Norm, LeakyRelu
    '                   Log, LogSoftmax, MapBinary, MapUnary, MaskedCrossEntropy
    '                   MatMul, Max, Maximum, MaxPool2D, MaxPool2DBackward
    '                   Mean, MeanAll, Min, Minimum, MseLoss
    '                   Multiply, MultiplyScalar, Negate, NormalizeAxis, PinDevice
    '                   Pow, Prod, Reciprocal, ReduceAlongAxis, ReduceArgAxis
    '                   ReduceGlobal, Relu, Sigmoid, SigmoidCrossEntropyWithLogits, Sin
    '                   Slice, Softmax, SpMM, Sqrt, Square
    '                   StdDev, Subtract, Sum, SumAll, Swish
    '                   SyncFromDevice, Tanh, TopK, Transpose, TryAdamWStep
    '                   UnpinDevice, Wrap
    ' 
    '         Sub: AxisLayout, RequireRank4, RequireSameShape
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' 标量参考实现：把原本散落在 Tensor.vb / Math.vb / nn.vb 里的循环算法集中到这里，
' 作为所有后端的兜底实现。
'
'   * SIMDTensor 只重写“有向量指令收益”的热点算子，其余自动继承这里；
'   * CudaTensor 只重写“有 CUDA 内核”的算子，其余自动继承这里。
'
' 这样即使某个后端没有覆盖某个算子，语义也始终正确（退化为 CPU 计算），
' 不会出现“切换后端之后某个算子不可用”的情况。
' ---------------------------------------------------------------------------

Imports std = System.Math

Namespace Compute

    ''' <summary>
    ''' <see cref="ITensorCompute"/> 的标量兜底实现。
    ''' </summary>
    Public MustInherit Class TensorComputeBase
        Implements ITensorCompute

        Public MustOverride ReadOnly Property Name As String Implements ITensorCompute.Name

#Region "helpers"

        ''' <summary>要求两个张量形状一致</summary>
        Protected Shared Sub RequireSameShape(a As Tensor, b As Tensor, opName As String)
            If Not a.Shape.SequenceEqual(b.Shape) Then
                Throw New ArgumentException(
                    $"张量形状必须相同才能{opName}: [{String.Join(",", a.Shape)}] vs [{String.Join(",", b.Shape)}]")
            End If
        End Sub

        ''' <summary>由计算结果的一维数组与形状包装出新的张量</summary>
        Protected Shared Function Wrap(data As Double(), shape As Integer()) As Tensor
            Return New Tensor(data, shape)
        End Function

        ''' <summary>逐元素一元运算的通用标量实现</summary>
        Protected Shared Function MapUnary(t As Tensor, op As Func(Of Double, Double)) As Tensor
            Dim src = t.Data
            Dim dst(src.Length - 1) As Double
            For i As Integer = 0 To src.Length - 1
                dst(i) = op(src(i))
            Next
            Return New Tensor(dst, t.Shape)
        End Function

        ''' <summary>逐元素二元运算的通用标量实现</summary>
        Protected Shared Function MapBinary(a As Tensor, b As Tensor, op As Func(Of Double, Double, Double)) As Tensor
            RequireSameShape(a, b, "进行逐元素运算")
            Dim srcA = a.Data
            Dim srcB = b.Data
            Dim dst(srcA.Length - 1) As Double
            For i As Integer = 0 To srcA.Length - 1
                dst(i) = op(srcA(i), srcB(i))
            Next
            Return New Tensor(dst, a.Shape)
        End Function

#End Region

#Region "逐元素 - 二元"

        Public Overridable Function Add(a As Tensor, b As Tensor) As Tensor Implements ITensorCompute.Add
            Return MapBinary(a, b, Function(x, y) x + y)
        End Function

        Public Overridable Function Subtract(a As Tensor, b As Tensor) As Tensor Implements ITensorCompute.Subtract
            Return MapBinary(a, b, Function(x, y) x - y)
        End Function

        Public Overridable Function Multiply(a As Tensor, b As Tensor) As Tensor Implements ITensorCompute.Multiply
            Return MapBinary(a, b, Function(x, y) x * y)
        End Function

        Public Overridable Function Divide(a As Tensor, b As Tensor) As Tensor Implements ITensorCompute.Divide
            Return MapBinary(a, b, Function(x, y) x / y)
        End Function

        Public Overridable Function Maximum(a As Tensor, b As Tensor) As Tensor Implements ITensorCompute.Maximum
            Return MapBinary(a, b, Function(x, y) std.Max(x, y))
        End Function

        Public Overridable Function Minimum(a As Tensor, b As Tensor) As Tensor Implements ITensorCompute.Minimum
            Return MapBinary(a, b, Function(x, y) std.Min(x, y))
        End Function

#End Region

#Region "逐元素 - 一元"

        Public Overridable Function Exp(t As Tensor) As Tensor Implements ITensorCompute.Exp
            Return MapUnary(t, Function(x) std.Exp(x))
        End Function

        Public Overridable Function Log(t As Tensor) As Tensor Implements ITensorCompute.Log
            Return MapUnary(t, Function(x) std.Log(x))
        End Function

        Public Overridable Function Sqrt(t As Tensor) As Tensor Implements ITensorCompute.Sqrt
            Return MapUnary(t, Function(x) std.Sqrt(x))
        End Function

        Public Overridable Function Square(t As Tensor) As Tensor Implements ITensorCompute.Square
            Return MapUnary(t, Function(x) x * x)
        End Function

        Public Overridable Function Abs(t As Tensor) As Tensor Implements ITensorCompute.Abs
            Return MapUnary(t, Function(x) std.Abs(x))
        End Function

        Public Overridable Function Sin(t As Tensor) As Tensor Implements ITensorCompute.Sin
            Return MapUnary(t, Function(x) std.Sin(x))
        End Function

        Public Overridable Function Cos(t As Tensor) As Tensor Implements ITensorCompute.Cos
            Return MapUnary(t, Function(x) std.Cos(x))
        End Function

        Public Overridable Function Tanh(t As Tensor) As Tensor Implements ITensorCompute.Tanh
            Return MapUnary(t, Function(x) std.Tanh(x))
        End Function

        Public Overridable Function Sigmoid(t As Tensor) As Tensor Implements ITensorCompute.Sigmoid
            Return MapUnary(t, Function(x) 1.0 / (1.0 + std.Exp(-x)))
        End Function

        Public Overridable Function Negate(t As Tensor) As Tensor Implements ITensorCompute.Negate
            Return MapUnary(t, Function(x) -x)
        End Function

        Public Overridable Function Reciprocal(t As Tensor) As Tensor Implements ITensorCompute.Reciprocal
            Return MapUnary(t, Function(x) 1.0 / x)
        End Function

        Public Overridable Function Relu(t As Tensor) As Tensor Implements ITensorCompute.Relu
            Return MapUnary(t, Function(x) std.Max(0.0, x))
        End Function

        Public Overridable Function LeakyRelu(t As Tensor, alpha As Double) As Tensor Implements ITensorCompute.LeakyRelu
            Return MapUnary(t, Function(x) If(x >= 0.0, x, alpha * x))
        End Function

        Public Overridable Function Elu(t As Tensor, alpha As Double) As Tensor Implements ITensorCompute.Elu
            Return MapUnary(t, Function(x) If(x >= 0.0, x, alpha * (std.Exp(x) - 1.0)))
        End Function

        Public Overridable Function Gelu(t As Tensor) As Tensor Implements ITensorCompute.Gelu
            Dim c = std.Sqrt(2.0 / std.PI)
            Return MapUnary(t, Function(x) 0.5 * x * (1.0 + std.Tanh(c * (x + 0.044715 * x * x * x))))
        End Function

        Public Overridable Function Swish(t As Tensor) As Tensor Implements ITensorCompute.Swish
            Return MapUnary(t, Function(x) x / (1.0 + std.Exp(-x)))
        End Function

        ''' <summary>
        ''' 阶跃函数(Heaviside): 大于 0 的元素取 1, 否则取 0
        ''' </summary>
        Public Overridable Function Heaviside(t As Tensor) As Tensor Implements ITensorCompute.Heaviside
            Return MapUnary(t, Function(x) If(x > 0.0, 1.0, 0.0))
        End Function

#End Region

#Region "标量运算"

        Public Overridable Function AddScalar(t As Tensor, scalar As Double) As Tensor Implements ITensorCompute.AddScalar
            Return MapUnary(t, Function(x) x + scalar)
        End Function

        Public Overridable Function MultiplyScalar(t As Tensor, scalar As Double) As Tensor Implements ITensorCompute.MultiplyScalar
            Return MapUnary(t, Function(x) x * scalar)
        End Function

        Public Overridable Function DivideScalar(t As Tensor, scalar As Double) As Tensor Implements ITensorCompute.DivideScalar
            Return MapUnary(t, Function(x) x / scalar)
        End Function

        Public Overridable Function Pow(t As Tensor, exponent As Double) As Tensor Implements ITensorCompute.Pow
            Return MapUnary(t, Function(x) std.Pow(x, exponent))
        End Function

        Public Overridable Function Clip(t As Tensor, low As Double, high As Double) As Tensor Implements ITensorCompute.Clip
            Return MapUnary(t, Function(x) std.Min(std.Max(x, low), high))
        End Function

#End Region

#Region "矩阵运算"

        Public Overridable Function MatMul(a As Tensor, b As Tensor) As Tensor Implements ITensorCompute.MatMul
            If a.Rank <> 2 OrElse b.Rank <> 2 Then
                Throw New ArgumentException("矩阵乘法需要二维张量")
            End If
            If a.Shape(1) <> b.Shape(0) Then
                Throw New ArgumentException($"矩阵维度不匹配: {a.Shape(1)} != {b.Shape(0)}")
            End If

            Dim m = a.Shape(0)
            Dim n = b.Shape(1)
            Dim k = a.Shape(1)

            Dim result = New Tensor(m, n)
            Dim srcA = a.Data
            Dim srcB = b.Data
            Dim dst = result.Data

            For i As Integer = 0 To m - 1
                Dim rowA = i * k
                Dim rowC = i * n
                For j As Integer = 0 To n - 1
                    Dim sum As Double = 0
                    For p As Integer = 0 To k - 1
                        sum += srcA(rowA + p) * srcB(p * n + j)
                    Next
                    dst(rowC + j) = sum
                Next
            Next

            Return result
        End Function

        ''' <summary>
        ''' 稀疏 × 稠密的主机（标量）实现：dense[batch, Rows] · W[Rows, Columns] → [batch, Columns]。
        ''' </summary>
        ''' <remarks>
        ''' 热路径直接操作底层数组，并对 0/1 脉冲输入跳过零源以利用稀疏性。
        ''' 有 CUDA 内核的后端（CudaTensor）会覆盖本方法。
        ''' </remarks>
        Public Overridable Function SpMM(csr As SparseCsr, dense As Tensor) As Tensor Implements ITensorCompute.SpMM
            If csr Is Nothing Then
                Throw New ArgumentNullException(NameOf(csr))
            End If
            If dense Is Nothing Then
                Throw New ArgumentNullException(NameOf(dense))
            End If
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

            Dim result = New Tensor(batch, cols)
            Dim xd = dense.Data
            Dim od = result.Data

            For b As Integer = 0 To batch - 1
                Dim bo = b * cols
                Dim ro = b * rows
                For r As Integer = 0 To rows - 1
                    Dim xv = xd(ro + r)
                    If xv = 0.0 Then Continue For    ' 脉冲输入高度稀疏：跳过零源
                    Dim k = rp(r)
                    Dim kEnd = rp(r + 1)
                    While k < kEnd
                        od(bo + ci(k)) += xv * vv(k)
                        k += 1
                    End While
                Next
            Next

            Return result
        End Function

        ''' <summary>
        ''' 融合的稀疏递归 LIF 单步（主机标量实现，就地更新状态张量）。
        ''' </summary>
        ''' <remarks>
        ''' 与「SpMM + Add + MultiplyScalar + Heaviside + ElementwiseMultiply」这条逐算子路径
        ''' <b>逐比特等价</b>（相同的循环次序、相同的运算结合次序、相同的 β/θ 单精度舍入），
        ''' 但把每步 6 个 <c>[batch, Units]</c> 中间张量的分配与 5 次全量遍历压缩为
        ''' 1 个累加缓冲 + 1 次融合遍历：
        ''' <list type="bullet">
        '''   <item>递归输入按 CSR 行并行累加到 <c>I</c>（跳过零源，与 <see cref="SpMM"/> 同序）；</item>
        '''   <item>泄漏积分 / 阈值触发 / 复位 / 计数累加在同一个循环内完成，全部就地写回。</item>
        ''' </list>
        ''' 这正是十万级神经元规模下主机侧的主要开销来源，GPU 后端覆写本方法后可进一步
        ''' 把状态留在显存中（见 <see cref="ITensorCompute.PinDevice64"/>）。
        ''' </remarks>
        Public Overridable Function LifStep(synapses As SparseCsr,
                                            sPrev As Tensor,
                                            externalCurrent As Tensor,
                                            h As Tensor,
                                            s As Tensor,
                                            counts As Tensor,
                                            beta As Double,
                                            threshold As Double,
                                            subtractThreshold As Boolean) As Boolean Implements ITensorCompute.LifStep

            If synapses Is Nothing Then Throw New ArgumentNullException(NameOf(synapses))
            If sPrev Is Nothing Then Throw New ArgumentNullException(NameOf(sPrev))
            If h Is Nothing Then Throw New ArgumentNullException(NameOf(h))
            If s Is Nothing Then Throw New ArgumentNullException(NameOf(s))
            If counts Is Nothing Then Throw New ArgumentNullException(NameOf(counts))

            If sPrev.Rank <> 2 OrElse sPrev.Shape(1) <> synapses.Rows Then
                Throw New ArgumentException(
                    $"LifStep 的 sPrev 形状应为 [batch, {synapses.Rows}]，实际 [{String.Join(",", sPrev.Shape)}]")
            End If
            If h.Shape(0) <> sPrev.Shape(0) OrElse h.Shape(1) <> synapses.Columns OrElse
               Not h.Shape.SequenceEqual(s.Shape) OrElse Not h.Shape.SequenceEqual(counts.Shape) Then
                Throw New ArgumentException(
                    $"LifStep 的 h/s/counts 形状应同为 [{sPrev.Shape(0)}, {synapses.Columns}]，" &
                    $"实际 h=[{String.Join(",", h.Shape)}] s=[{String.Join(",", s.Shape)}] counts=[{String.Join(",", counts.Shape)}]")
            End If
            If externalCurrent IsNot Nothing AndAlso Not externalCurrent.Shape.SequenceEqual(h.Shape) Then
                Throw New ArgumentException(
                    $"LifStep 的 externalCurrent 形状应为 [{String.Join(",", h.Shape)}]，" &
                    $"实际 [{String.Join(",", externalCurrent.Shape)}]")
            End If

            Dim batch = sPrev.Shape(0)
            Dim cols = synapses.Columns
            Dim ed As Double() = If(externalCurrent Is Nothing, Nothing, externalCurrent.Data)

            ' 递归输入累加缓冲：I[batch, cols]（唯一的一次分配）
            Dim I = New Double(batch * cols - 1) {}

            Call LifRecurrentInput(synapses, sPrev.Data, batch, I)

            ' 泄漏积分 → 阈值触发 → 复位 → 计数累加（全部就地）
            Call LifUpdateInPlace(I, ed, h.Data, s.Data, counts.Data,
                                  CDbl(CSng(beta)), threshold, CDbl(CSng(threshold)),
                                  subtractThreshold)

            ' 三个状态张量都是就地写入：声明设备端缓存副本失效
            Call h.MarkHostModified()
            Call s.MarkHostModified()
            Call counts.MarkHostModified()

            Return True
        End Function

        ''' <summary>
        ''' 融合单步的第一阶段：<c>I[b, c] = Σ_r W[r, c]·S_prev[b, r]</c>。
        ''' </summary>
        ''' <param name="synapses">CSR 连接矩阵（行 = 突触前，列 = 突触后）</param>
        ''' <param name="sPrev">上一时刻脉冲的主机数组 <c>[batch, Rows]</c></param>
        ''' <param name="batch">批大小</param>
        ''' <param name="synapticInput">输出缓冲 <c>[batch, columns]</c>，调用前必须为零</param>
        ''' <remarks>
        ''' 累加次序与 <see cref="SpMM"/> 完全一致（batch → 行 → 非零元），
        ''' 因此在 CPU 后端下本方法的输出与 <c>SpMM</c> 逐比特相同；
        ''' 外部电流由 <see cref="LifUpdateInPlace"/> 在同样的运算次序下叠加。
        ''' </remarks>
        Protected Overridable Sub LifRecurrentInput(synapses As SparseCsr,
                                                    sPrev As Double(),
                                                    batch As Integer,
                                                    synapticInput As Double())

            Dim rows = synapses.Rows
            Dim cols = synapses.Columns
            Dim rp = synapses.RowPointers
            Dim ci = synapses.ColumnIndices
            Dim vv = synapses.Values

            For b As Integer = 0 To batch - 1
                Dim bo = b * cols
                Dim ro = b * rows

                For r As Integer = 0 To rows - 1
                    Dim xv = sPrev(ro + r)
                    Dim k = rp(r)
                    Dim kEnd = rp(r + 1)

                    ' 脉冲输入高度稀疏：零源直接跳过（与 SpMM 的快速路径一致）
                    If xv <> 0.0 Then
                        While k < kEnd
                            synapticInput(bo + ci(k)) += xv * vv(k)
                            k += 1
                        End While
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' 融合单步的第二阶段（逐元素，可就地更新）：泄漏积分 → 阈值触发 → 复位 → 计数累加。
        ''' </summary>
        ''' <param name="synapticInput">突触输入 <c>[batch, columns]</c>（只读）</param>
        ''' <param name="externalCurrent">外部电流主机数组，可为 <c>Nothing</c></param>
        ''' <param name="h">膜电位，就地更新为 <c>H[t]</c></param>
        ''' <param name="s">输出脉冲，就地写入 0/1</param>
        ''' <param name="counts">计数累加器，就地累加</param>
        ''' <param name="beta32">已按单精度舍入的 β（与 <c>Tensor * CSng(β)</c> 一致）</param>
        ''' <param name="threshold">发放阈值（判定用，全精度）</param>
        ''' <param name="thr32">已按单精度舍入的阈值（复位减法用）</param>
        ''' <param name="subtractThreshold">复位模式，见 <see cref="ITensorCompute.LifStep"/></param>
        ''' <remarks>
        ''' 逐元素运算彼此独立，因此本方法是<b>可安全向量化</b>的扩展点
        ''' （<see cref="SIMDTensor"/> 用 <c>System.Numerics.Vector</c> 覆写了它）。
        ''' </remarks>
        Protected Overridable Sub LifUpdateInPlace(synapticInput As Double(),
                                                   externalCurrent As Double(),
                                                   h As Double(),
                                                   s As Double(),
                                                   counts As Double(),
                                                   beta32 As Double,
                                                   threshold As Double,
                                                   thr32 As Double,
                                                   subtractThreshold As Boolean)

            For i As Integer = 0 To synapticInput.Length - 1
                Dim pre = synapticInput(i)

                If externalCurrent IsNot Nothing Then
                    pre += externalCurrent(i)
                End If

                Dim u = pre + h(i) * beta32
                Dim sp = If(u - threshold > 0.0, 1.0, 0.0)

                s(i) = sp
                h(i) = If(subtractThreshold, u - sp * thr32, u * (1.0 - sp))
                counts(i) += sp
            Next
        End Sub

#Region "批量细胞管线融合算子（CPU 参考实现）"

        ''' <summary>
        ''' 液态时间常数网络批量 RK4 积分（CPU 参考实现，就地推进状态）。
        ''' </summary>
        ''' <remarks>
        ''' 运算次序与 <c>LiquidCell.ComputeDerivative</c> + <c>ODESolver.RK4Step</c> 的
        ''' 逐样本写法完全一致（自身递归项 → 输入项 → 门控项 → 激活 → 斜率合成），
        ''' 因此 CPU 后端下的结果与逐细胞路径逐比特相同；
        ''' GPU 后端的差异只来自浮点归约顺序（见 <c>cellaccel.cu</c>）。
        ''' </remarks>
        Public Overridable Function LtcRk4Batch(state As Tensor, input As Tensor,
                                                weightRecurrent As Tensor, weightInput As Tensor, bias As Tensor,
                                                weightGate As Tensor, weightGateInput As Tensor, biasGate As Tensor,
                                                tauEff As Tensor, dt As Double, subSteps As Integer,
                                                activation As Integer) As Boolean Implements ITensorCompute.LtcRk4Batch

            If state Is Nothing OrElse input Is Nothing Then Throw New ArgumentNullException(NameOf(state))
            If weightRecurrent Is Nothing OrElse weightInput Is Nothing Then
                Throw New ArgumentNullException(NameOf(weightRecurrent))
            End If
            If tauEff Is Nothing Then Throw New ArgumentNullException(NameOf(tauEff))
            If state.Rank <> 2 OrElse input.Rank <> 2 Then
                Throw New ArgumentException($"LtcRk4Batch 要求 state/input 为二维张量，" &
                                            $"实际 state=[{String.Join(",", state.Shape)}] input=[{String.Join(",", input.Shape)}]")
            End If

            Dim batch As Integer = state.Shape(0)
            Dim m As Integer = state.Shape(1)
            Dim nIn As Integer = input.Shape(1)

            If input.Shape(0) <> batch Then
                Throw New ArgumentException($"LtcRk4Batch 的 state/input 批大小不一致：{batch} vs {input.Shape(0)}")
            End If
            If subSteps < 1 Then subSteps = 1

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
            Dim sw As Double() = New Double(m - 1) {}
            Dim sh As Double() = New Double(m - 1) {}
            Dim sk As Double() = New Double(4 * m - 1) {}
            Dim invTau As Double() = New Double(m - 1) {}

            For i As Integer = 0 To m - 1
                invTau(i) = 1.0 / tau(i)
            Next

            For b As Integer = 0 To batch - 1
                Dim rowOff As Integer = b * m

                For i As Integer = 0 To m - 1
                    sh(i) = h(rowOff + i)
                Next

                For s As Integer = 1 To subSteps
                    For i As Integer = 0 To m - 1
                        sw(i) = sh(i)
                    Next

                    For stage As Integer = 0 To 3
                        For i As Integer = 0 To m - 1
                            Dim z As Double = If(bd Is Nothing, 0.0, bd(i))
                            Dim zg As Double = If(bg Is Nothing, 0.0, bg(i))

                            For k As Integer = 0 To m - 1
                                Dim hv As Double = sw(k)

                                z += hv * wrec(k * m + i)

                                If hasGate Then
                                    zg += hv * wg(k * m + i)
                                End If
                            Next

                            For j As Integer = 0 To nIn - 1
                                Dim uv As Double = u(b * nIn + j)

                                z += uv * win(j * m + i)

                                If hasGate Then
                                    zg += uv * wgi(j * m + i)
                                End If
                            Next

                            Dim decay As Double = invTau(i)

                            If hasGate Then
                                decay += 1.0 / (1.0 + std.Exp(-zg))
                            End If

                            sk(stage * m + i) = decay * (CellActivation.Apply(z, activation) - sw(i))
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

                For i As Integer = 0 To m - 1
                    h(rowOff + i) = sh(i)
                Next
            Next

            Call state.MarkHostModified()

            Return True
        End Function

        ''' <summary>
        ''' 图卷积批量层（CPU 参考实现）：自身分支 + CSR 邻居聚合 + 偏置 + 激活。
        ''' </summary>
        Public Overridable Function GraphLayerBatch(x As Tensor, wSelf As Tensor, wRel As Tensor,
                                                    selfW As Tensor, bias As Tensor, edges As SparseCsr,
                                                    output As Tensor, activation As Integer) As Boolean Implements ITensorCompute.GraphLayerBatch

            If x Is Nothing OrElse wSelf Is Nothing OrElse output Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If x.Rank <> 2 Then
                Throw New ArgumentException($"GraphLayerBatch 要求节点特征为二维张量，实际 [{String.Join(",", x.Shape)}]")
            End If

            Dim rows As Integer = x.Shape(0)
            Dim inF As Integer = x.Shape(1)
            Dim outF As Integer = output.Shape(1)

            If output.Shape(0) <> rows Then
                Throw New ArgumentException($"GraphLayerBatch 的输入/输出行数不一致：{rows} vs {output.Shape(0)}")
            End If

            Dim xd As Double() = x.Data
            Dim ws As Double() = wSelf.Data
            Dim wr As Double() = If(wRel Is Nothing, Nothing, wRel.Data)
            Dim swD As Double() = If(selfW Is Nothing, Nothing, selfW.Data)
            Dim bd As Double() = If(bias Is Nothing, Nothing, bias.Data)
            Dim od As Double() = output.Data
            Dim rp As Integer() = If(edges Is Nothing, Nothing, edges.RowPointers)
            Dim ci As Integer() = If(edges Is Nothing, Nothing, edges.ColumnIndices)
            Dim vv As Double() = If(edges Is Nothing, Nothing, edges.Values)

            ' 节点数：优先取邻接矩阵的行数（= 基因数），其次取逐节点缩放的规模；
            ' 两者都缺省时说明本层没有邻居项也没有逐节点缩放（解码器形态），此时节点数无关紧要。
            Dim nodes As Integer = rows

            If edges IsNot Nothing Then
                nodes = edges.Rows
            ElseIf swD IsNot Nothing Then
                nodes = swD.Length
            End If

            For row As Integer = 0 To rows - 1
                Dim nodeIdx As Integer = row Mod nodes
                Dim baseRow As Integer = row - nodeIdx
                Dim orow As Integer = row * outF

                For j As Integer = 0 To outF - 1
                    Dim acc As Double = If(bd Is Nothing, 0.0, bd(j))
                    Dim scale As Double = If(swD Is Nothing, 1.0, swD(nodeIdx))
                    Dim selfSum As Double = 0.0

                    For k As Integer = 0 To inF - 1
                        selfSum += xd(row * inF + k) * ws(k * outF + j)
                    Next

                    acc += scale * selfSum

                    If rp IsNot Nothing AndAlso wr IsNot Nothing Then
                        For e As Integer = rp(nodeIdx) To rp(nodeIdx + 1) - 1
                            Dim c As Double = vv(e)

                            If c = 0.0 Then
                                Continue For
                            End If

                            Dim srcRow As Integer = baseRow + ci(e)
                            Dim edgeSum As Double = 0.0

                            For k As Integer = 0 To inF - 1
                                edgeSum += xd(srcRow * inF + k) * wr(k * outF + j)
                            Next

                            acc += c * edgeSum
                        Next
                    End If

                    od(orow + j) = CellActivation.Apply(acc, activation)
                Next
            Next

            Call output.MarkHostModified()

            Return True
        End Function

        ''' <summary>
        ''' 节点特征拼装（CPU 参考实现）：<c>[x̄ ‖ p ‖ e_i ‖ z_pert]</c>。
        ''' </summary>
        Public Overridable Function GraphFeatureBatch(xNorm As Tensor, flag As Tensor, embed As Tensor, zPert As Tensor,
                                                      output As Tensor, n As Integer, d As Integer) As Boolean Implements ITensorCompute.GraphFeatureBatch

            If xNorm Is Nothing OrElse flag Is Nothing OrElse embed Is Nothing OrElse zPert Is Nothing OrElse output Is Nothing Then
                Throw New ArgumentNullException(NameOf(xNorm))
            End If

            Dim rows As Integer = xNorm.Length
            Dim dims As Integer = 2 + 2 * d
            Dim xd As Double() = xNorm.Data
            Dim fd As Double() = flag.Data
            Dim ed As Double() = embed.Data
            Dim zd As Double() = zPert.Data
            Dim od As Double() = output.Data

            For row As Integer = 0 To rows - 1
                Dim i As Integer = row Mod n
                Dim b As Integer = row \ n
                Dim off As Integer = row * dims

                od(off) = xd(row)
                od(off + 1) = fd(row)

                For k As Integer = 0 To d - 1
                    od(off + 2 + k) = ed(i * d + k)
                    od(off + 2 + d + k) = zd(b * d + k)
                Next
            Next

            Call output.MarkHostModified()

            Return True
        End Function

        ''' <summary>
        ''' 通量读取头批量计算（CPU 参考实现）：<c>v = e ⊙ gsat([h ‖ u]·Wv + bv)</c>。
        ''' </summary>
        ''' <remarks>
        ''' 逐项对应 <c>MetabolicLiquidNetwork.ComputeFlux</c>：同样的夹断 ±30、
        ''' 同样的可逆开关（可逆取 2σ−1，不可逆取 σ）、同样的酶水平乘子 <c>u(j)</c>。
        ''' </remarks>
        Public Overridable Function FluxHeadBatch(h As Tensor, u As Tensor, wFlux As Tensor, fluxBias As Tensor,
                                                  reversible As Tensor, output As Tensor) As Boolean Implements ITensorCompute.FluxHeadBatch

            If h Is Nothing OrElse u Is Nothing OrElse wFlux Is Nothing OrElse output Is Nothing Then
                Throw New ArgumentNullException(NameOf(h))
            End If

            Dim batch As Integer = h.Shape(0)
            Dim m As Integer = h.Shape(1)
            Dim nIn As Integer = u.Shape(1)
            Dim r As Integer = output.Shape(1)

            If wFlux.Shape(0) <> m + nIn OrElse wFlux.Shape(1) <> r Then
                Throw New ArgumentException($"FluxHeadBatch 的 Wv 形状应为 [{m + nIn}, {r}]，实际 [{String.Join(",", wFlux.Shape)}]")
            End If

            Dim hd As Double() = h.Data
            Dim ud As Double() = u.Data
            Dim wd As Double() = wFlux.Data
            Dim bd As Double() = If(fluxBias Is Nothing, Nothing, fluxBias.Data)
            Dim rd As Double() = If(reversible Is Nothing, Nothing, reversible.Data)
            Dim od As Double() = output.Data

            For b As Integer = 0 To batch - 1
                Dim hOff As Integer = b * m
                Dim uOff As Integer = b * nIn
                Dim oOff As Integer = b * r

                For j As Integer = 0 To r - 1
                    Dim z As Double = If(bd Is Nothing, 0.0, bd(j))

                    For i As Integer = 0 To m - 1
                        z += hd(hOff + i) * wd(i * r + j)
                    Next

                    For i As Integer = 0 To nIn - 1
                        z += ud(uOff + i) * wd((m + i) * r + j)
                    Next

                    If z > 30.0 Then
                        z = 30.0
                    ElseIf z < -30.0 Then
                        z = -30.0
                    End If

                    Dim sat As Double = 1.0 / (1.0 + std.Exp(-z))
                    Dim reversibleFlag As Boolean = rd IsNot Nothing AndAlso rd(j) <> 0.0

                    od(oOff + j) = ud(uOff + j) * If(reversibleFlag, 2.0 * sat - 1.0, sat)
                Next
            Next

            Call output.MarkHostModified()

            Return True
        End Function

        ''' <summary>
        ''' 系统时间常数批量计算（CPU 参考实现）：<c>τ^sys = 1/(1/τ_eff + f)</c>。
        ''' </summary>
        Public Overridable Function SystemTauBatch(h As Tensor, u As Tensor, weightGate As Tensor,
                                                   weightGateInput As Tensor, biasGate As Tensor,
                                                   tauEff As Tensor, output As Tensor) As Boolean Implements ITensorCompute.SystemTauBatch

            If h Is Nothing OrElse u Is Nothing OrElse tauEff Is Nothing OrElse output Is Nothing Then
                Throw New ArgumentNullException(NameOf(h))
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

            For b As Integer = 0 To batch - 1
                Dim hOff As Integer = b * m
                Dim uOff As Integer = b * nIn

                For i As Integer = 0 To m - 1
                    Dim decay As Double = 1.0 / td(i)

                    If hasGate Then
                        Dim zg As Double = If(bg Is Nothing, 0.0, bg(i))

                        For k As Integer = 0 To m - 1
                            zg += hd(hOff + k) * wg(k * m + i)
                        Next

                        For j As Integer = 0 To nIn - 1
                            zg += ud(uOff + j) * wgi(j * m + i)
                        Next

                        decay += 1.0 / (1.0 + std.Exp(-zg))
                    End If

                    od(hOff + i) = 1.0 / decay
                Next
            Next

            Call output.MarkHostModified()

            Return True
        End Function

#End Region

        Public Overridable Function Transpose(t As Tensor) As Tensor Implements ITensorCompute.Transpose
            If t.Rank <> 2 Then
                Throw New ArgumentException("只支持二维张量转置")
            End If

            Dim rows = t.Shape(0)
            Dim cols = t.Shape(1)
            Dim result = New Tensor(cols, rows)
            Dim src = t.Data
            Dim dst = result.Data

            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To cols - 1
                    dst(j * rows + i) = src(i * cols + j)
                Next
            Next

            Return result
        End Function

#End Region

#Region "形状变换与选择"

        ' ------------------------------------------------------------------
        ' Slice / Concat / TopK
        '
        ' 这三个算子都是「索引与搬移」语义而不是「算术」语义：
        '   * Slice / Concat 的实现完全由 Array.Copy 的整块内存搬移构成，
        '     已经是内存带宽受限的最优形态，因此不在 SIMDTensor 中重复覆写；
        '   * TopK 是选择问题，没有可利用的向量指令收益。
        ' 把实现统一放在这里，等价于同时为 SIMDTensor 与 CudaTensor 提供正确行为。
        ' ------------------------------------------------------------------

        ''' <summary>把可能为负的轴编号规范到 <c>[0, Rank)</c>，并做越界校验</summary>
        Protected Shared Function NormalizeAxis(t As Tensor, axis As Integer, opName As String) As Integer
            Dim a As Integer = axis
            If a < 0 Then a = t.Rank + a

            If a < 0 OrElse a >= t.Rank Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(axis), $"{opName} 的轴编号 {axis} 超出张量秩 {t.Rank} 的合法范围")
            End If

            Return a
        End Function

        ''' <summary>
        ''' 把某根轴上的索引分解成「外层个数 / 轴长度 / 内层连续长度」三元组。
        ''' </summary>
        ''' <remarks>
        ''' 行主序下第 <paramref name="axis"/> 维的步长是 <c>innerSize = Π shape(axis+1..)</c>，
        ''' 而 <c>outerSize = Π shape(0..axis-1)</c> 是前面各维的元素总数。
        ''' 因此 "沿该轴取第 k 段" 就是一次长度为 <c>innerSize</c> 的连续块搬移。
        ''' </remarks>
        Protected Shared Sub AxisLayout(shape As Integer(), axis As Integer,
                                        ByRef outerSize As Integer, ByRef axisSize As Integer, ByRef innerSize As Integer)
            outerSize = 1
            For i As Integer = 0 To axis - 1
                outerSize *= shape(i)
            Next

            axisSize = shape(axis)

            innerSize = 1
            For i As Integer = axis + 1 To shape.Length - 1
                innerSize *= shape(i)
            Next
        End Sub

        Public Overridable Function Slice(t As Tensor, axis As Integer, start As Integer, length As Integer) As Tensor Implements ITensorCompute.Slice
            If t Is Nothing Then Throw New ArgumentNullException(NameOf(t))

            Dim a = NormalizeAxis(t, axis, "Slice")

            If start < 0 OrElse length < 0 Then
                Throw New ArgumentException($"Slice 的 start({start}) / length({length}) 不能为负数")
            End If

            Dim shape = t.Shape

            If start + length > shape(a) Then
                Throw New ArgumentException(
                    $"Slice 区间 [{start}, {start + length}) 超出轴 {a} 的长度 {shape(a)}")
            End If

            Dim outerSize As Integer, axisSize As Integer, innerSize As Integer
            Call AxisLayout(shape, a, outerSize, axisSize, innerSize)

            Dim outShape = CType(shape.Clone(), Integer())
            outShape(a) = length

            Dim result = New Tensor(outShape)
            Dim src = t.Data
            Dim dst = result.Data
            Dim blockSize = length * innerSize

            If blockSize > 0 Then
                For o As Integer = 0 To outerSize - 1
                    Call Array.Copy(src, (o * axisSize + start) * innerSize, dst, o * blockSize, blockSize)
                Next
            End If

            Return result
        End Function

        Public Overridable Function Concat(parts As Tensor(), axis As Integer) As Tensor Implements ITensorCompute.Concat
            If parts Is Nothing OrElse parts.Length = 0 Then
                Throw New ArgumentException("Concat 至少需要一个输入张量")
            End If
            If parts.Any(Function(p) p Is Nothing) Then
                Throw New ArgumentException("Concat 的输入张量不能为 Nothing")
            End If

            Dim rank = parts(0).Rank
            Dim a = NormalizeAxis(parts(0), axis, "Concat")
            Dim refShape = parts(0).Shape
            Dim totalAxis As Integer = 0

            For i As Integer = 0 To parts.Length - 1
                Dim p = parts(i)

                If p.Rank <> rank Then
                    Throw New ArgumentException($"Concat 要求所有输入张量秩一致: {rank} vs {p.Rank}")
                End If

                For d As Integer = 0 To rank - 1
                    If d <> a AndAlso p.Shape(d) <> refShape(d) Then
                        Throw New ArgumentException(
                            $"Concat 要求除轴 {a} 外的所有维度一致，第 {d} 维出现 {refShape(d)} vs {p.Shape(d)}")
                    End If
                Next

                totalAxis += p.Shape(a)
            Next

            Dim outShape = CType(refShape.Clone(), Integer())
            outShape(a) = totalAxis

            Dim outerSize As Integer, axisSize As Integer, innerSize As Integer
            Call AxisLayout(refShape, a, outerSize, axisSize, innerSize)

            Dim result = New Tensor(outShape)
            Dim dst = result.Data

            For o As Integer = 0 To outerSize - 1
                Dim cursor As Integer = 0

                For i As Integer = 0 To parts.Length - 1
                    Dim partAxis = parts(i).Shape(a)
                    Dim blockSize = partAxis * innerSize

                    If blockSize > 0 Then
                        Call Array.Copy(parts(i).Data, o * blockSize, dst,
                                        (o * totalAxis + cursor) * innerSize, blockSize)
                    End If

                    cursor += partAxis
                Next
            Next

            Return result
        End Function

        Public Overridable Function TopK(t As Tensor, k As Integer, ByRef indices As Tensor) As Tensor Implements ITensorCompute.TopK
            If t Is Nothing Then Throw New ArgumentNullException(NameOf(t))
            If t.Rank < 1 Then Throw New ArgumentException("TopK 需要秩 >= 1 的张量")

            Dim n = t.Shape(t.Rank - 1)

            If k < 1 OrElse k > n Then
                Throw New ArgumentException($"TopK 的 k={k} 必须落在 [1, {n}] 范围内")
            End If

            Dim blocks = t.Length \ n
            Dim outShape = CType(t.Shape.Clone(), Integer())
            outShape(t.Rank - 1) = k

            Dim values = New Tensor(outShape)
            Dim indexTensor = New Tensor(outShape)
            Dim src = t.Data
            Dim dstValues = values.Data
            Dim dstIndex = indexTensor.Data

            ' 复用的临时缓冲区：避免每个 block 都重新分配
            Dim cursor(n - 1) As Integer
            Dim work(n - 1) As Double

            For blk As Integer = 0 To blocks - 1
                Dim offset = blk * n
                Call Array.Copy(src, offset, work, 0, n)

                For i As Integer = 0 To n - 1
                    cursor(i) = i
                Next

                ' 部分选择排序：每轮在剩余区间内挑出最大者交换到区间首部，
                ' 只做 k 轮即可，复杂度 O(n * k)，在 k << n 时远优于全排序。
                For s As Integer = 0 To k - 1
                    Dim best As Integer = s

                    For j As Integer = s + 1 To n - 1
                        If work(j) > work(best) Then best = j
                    Next

                    If best <> s Then
                        Dim swapValue = work(s)
                        work(s) = work(best)
                        work(best) = swapValue

                        Dim swapIndex = cursor(s)
                        cursor(s) = cursor(best)
                        cursor(best) = swapIndex
                    End If

                    dstValues(blk * k + s) = work(s)
                    dstIndex(blk * k + s) = cursor(s)
                Next
            Next

            indices = indexTensor

            Return values
        End Function

#End Region

#Region "训练算子"

        ''' <summary>
        ''' 带损失掩码的 softmax 交叉熵（CPU 参考实现）。
        ''' </summary>
        ''' <remarks>
        ''' CPU 后端保持"逐行三趟循环"的朴素写法：求行最大值 → 求 exp 和 → 归一化。
        ''' GPU 后端（<c>CudaTensor</c>）会用"每行一个 block + 共享内存树形归约"的
        ''' 融合内核覆盖它，把 3300 万次 <c>exp</c> 从主机搬到设备。
        ''' 两条路径的损失定义与梯度定义必须严格一致，否则 CPU/GPU 结果不可比。
        ''' </remarks>
        Public Overridable Function MaskedCrossEntropy(logits As Tensor,
                                                       targets As Integer(),
                                                       mask As Boolean(),
                                                       ByRef dLogits As Tensor) As Double Implements ITensorCompute.MaskedCrossEntropy
            If logits Is Nothing Then Throw New ArgumentNullException(NameOf(logits))
            If logits.Rank <> 2 Then Throw New ArgumentException("掩码交叉熵要求 [rows, vocab] 的二维 logits")

            Dim rows = logits.Shape(0)
            Dim vocab = logits.Shape(1)

            dLogits = New Tensor(logits.Shape)

            Dim src = logits.Data
            Dim grad = dLogits.Data
            Dim total As Double = 0.0
            Dim count As Integer = 0

            For r As Integer = 0 To rows - 1
                If mask IsNot Nothing AndAlso r < mask.Length AndAlso Not mask(r) Then Continue For
                If targets Is Nothing OrElse r >= targets.Length Then Continue For

                Dim t = targets(r)
                If t < 0 OrElse t >= vocab Then Continue For

                count += 1

                Dim offset = r * vocab
                Dim maxVal = Double.NegativeInfinity

                For j As Integer = 0 To vocab - 1
                    If src(offset + j) > maxVal Then maxVal = src(offset + j)
                Next

                Dim sumExp As Double = 0.0

                For j As Integer = 0 To vocab - 1
                    Dim e = std.Exp(src(offset + j) - maxVal)
                    grad(offset + j) = e
                    sumExp += e
                Next

                If sumExp <= 0 Then sumExp = 1.0

                For j As Integer = 0 To vocab - 1
                    grad(offset + j) /= sumExp
                Next

                total -= std.Log(std.Max(grad(offset + t), 1.0E-12))
                grad(offset + t) -= 1.0
            Next

            If count = 0 Then
                Call dLogits.MarkHostModified()
                Return 0.0
            End If

            ' 归一化：损失与梯度都除以有效位置数，使不同 batch 的损失可比
            Dim inv = 1.0 / count

            For i As Integer = 0 To grad.Length - 1
                grad(i) *= inv
            Next

            Call dLogits.MarkHostModified()

            Return total * inv
        End Function

        ''' <summary>
        ''' 默认后端不提供设备端 AdamW，返回 <c>False</c> 让调用方走主机循环。
        ''' </summary>
        Public Overridable Function TryAdamWStep(param As Tensor, gradient As Tensor,
                                                 momentum As Tensor, velocity As Tensor,
                                                 learningRate As Double, beta1 As Double, beta2 As Double,
                                                 eps As Double, biasCorrection1 As Double,
                                                 biasCorrection2 As Double,
                                                 weightDecay As Double) As Boolean Implements ITensorCompute.TryAdamWStep
            Return False
        End Function

        ''' <summary>默认后端没有"设备常驻"概念。</summary>
        Public Overridable ReadOnly Property SupportsDeviceResidency As Boolean Implements ITensorCompute.SupportsDeviceResidency
            Get
                Return False
            End Get
        End Property

        ''' <summary>默认后端不支持钉住，直接返回 <c>False</c>。</summary>
        Public Overridable Function PinDevice(t As Tensor, label As String, zeroFill As Boolean) As Boolean Implements ITensorCompute.PinDevice
            Return False
        End Function

        ''' <summary>默认后端没有双精度常驻缓冲，返回 <c>False</c>。</summary>
        Public Overridable Function PinDevice64(t As Tensor, label As String, zeroFill As Boolean) As Boolean Implements ITensorCompute.PinDevice64
            Return False
        End Function

        ''' <summary>默认后端没有常驻缓冲，返回 <c>False</c>。</summary>
        Public Overridable Function UnpinDevice(t As Tensor) As Boolean Implements ITensorCompute.UnpinDevice
            Return False
        End Function

        ''' <summary>默认后端没有任何张量被钉住。</summary>
        Public Overridable Function IsDevicePinned(t As Tensor) As Boolean Implements ITensorCompute.IsDevicePinned
            Return False
        End Function

        ''' <summary>默认后端不占用显存。</summary>
        Public Overridable ReadOnly Property PinnedDeviceBytes As Long Implements ITensorCompute.PinnedDeviceBytes
            Get
                Return 0L
            End Get
        End Property

        ''' <summary>默认后端没有设备副本，因此无需同步。</summary>
        Public Overridable Function SyncFromDevice(t As Tensor) As Boolean Implements ITensorCompute.SyncFromDevice
            Return False
        End Function

#End Region

#Region "卷积与池化"

        ' ------------------------------------------------------------------
        ' 标量参考实现（正确性优先）。
        '
        ' 布局约定与 <see cref="ITensorCompute"/> 的声明一致，全部 channel-last:
        '     输入 (N,H,W,C) / 卷积核 (KH,KW,C,OutC) / 输出 (N,OH,OW,OutC)
        ' 该实现同时充当 SIMDTensor 与 CudaTensor 的兜底（内核不可用时回退到这里）。
        ' ------------------------------------------------------------------

        ''' <summary>卷积/池化的输出边长: (input + 2*padding - kernel) / stride + 1</summary>
        Protected Shared Function ConvOutSize(inputSize As Integer, kernelSize As Integer,
                                              stride As Integer, padding As Integer) As Integer
            If stride <= 0 Then Throw New ArgumentException("stride 必须为正数")
            Return (inputSize + 2 * padding - kernelSize) \ stride + 1
        End Function

        ''' <summary>卷积/池化的入参形状校验</summary>
        Private Shared Sub RequireRank4(t As Tensor, name As String)
            If t Is Nothing OrElse t.Rank <> 4 Then
                Throw New ArgumentException($"{name} 需要四维张量 (N,H,W,C)")
            End If
        End Sub

        Public Overridable Function Conv2D(x As Tensor, filters As Tensor, bias As Tensor,
                                           stride As Integer, padding As Integer) As Tensor Implements ITensorCompute.Conv2D
            Call RequireRank4(x, "x")
            Call RequireRank4(filters, "filters")

            Dim batch = x.Shape(0), inH = x.Shape(1), inW = x.Shape(2), inC = x.Shape(3)
            Dim filtH = filters.Shape(0), filtW = filters.Shape(1)
            Dim filtC = filters.Shape(2), outC = filters.Shape(3)

            If filtC <> inC Then
                Throw New ArgumentException($"卷积核通道数 {filtC} 与输入通道数 {inC} 不一致")
            End If
            If bias IsNot Nothing AndAlso bias.Length <> outC Then
                Throw New ArgumentException($"偏置长度 {bias.Length} 与输出通道数 {outC} 不一致")
            End If

            Dim outH = ConvOutSize(inH, filtH, stride, padding)
            Dim outW = ConvOutSize(inW, filtW, stride, padding)

            If outH <= 0 OrElse outW <= 0 Then
                Throw New ArgumentException($"卷积输出尺寸非法: {outH}x{outW}")
            End If

            Dim result = New Tensor(batch, outH, outW, outC)
            Dim src = x.Data
            Dim kern = filters.Data
            Dim dst = result.Data
            Dim biasData = If(bias Is Nothing, Nothing, bias.Data)

            For n As Integer = 0 To batch - 1
                For oh As Integer = 0 To outH - 1
                    For ow As Integer = 0 To outW - 1
                        For oc As Integer = 0 To outC - 1
                            Dim sum As Double = If(biasData Is Nothing, 0.0, biasData(oc))

                            For kh As Integer = 0 To filtH - 1
                                Dim ih As Integer = oh * stride + kh - padding
                                If ih < 0 OrElse ih >= inH Then Continue For

                                For kw As Integer = 0 To filtW - 1
                                    Dim iw As Integer = ow * stride + kw - padding
                                    If iw < 0 OrElse iw >= inW Then Continue For

                                    Dim xBase = ((n * inH + ih) * inW + iw) * inC
                                    Dim kBase = ((kh * filtW + kw) * inC) * outC + oc

                                    For c As Integer = 0 To inC - 1
                                        sum += src(xBase + c) * kern(kBase + c * outC)
                                    Next
                                Next
                            Next

                            dst(((n * outH + oh) * outW + ow) * outC + oc) = sum
                        Next
                    Next
                Next
            Next

            Return result
        End Function

        Public Overridable Function Conv2DBackwardInput(gradOutput As Tensor, filters As Tensor,
                                                        inputShape As Integer(), stride As Integer,
                                                        padding As Integer) As Tensor Implements ITensorCompute.Conv2DBackwardInput
            Call RequireRank4(gradOutput, "gradOutput")
            Call RequireRank4(filters, "filters")

            Dim batch = inputShape(0), inH = inputShape(1), inW = inputShape(2), inC = inputShape(3)
            Dim filtH = filters.Shape(0), filtW = filters.Shape(1), outC = filters.Shape(3)
            Dim outH = gradOutput.Shape(1), outW = gradOutput.Shape(2)

            Dim result = New Tensor(batch, inH, inW, inC)
            Dim g = gradOutput.Data
            Dim kern = filters.Data
            Dim dst = result.Data

            For n As Integer = 0 To batch - 1
                For oh As Integer = 0 To outH - 1
                    For ow As Integer = 0 To outW - 1
                        For oc As Integer = 0 To outC - 1
                            Dim gv = g(((n * outH + oh) * outW + ow) * outC + oc)

                            For kh As Integer = 0 To filtH - 1
                                Dim ih As Integer = oh * stride + kh - padding
                                If ih < 0 OrElse ih >= inH Then Continue For

                                For kw As Integer = 0 To filtW - 1
                                    Dim iw As Integer = ow * stride + kw - padding
                                    If iw < 0 OrElse iw >= inW Then Continue For

                                    Dim xBase = ((n * inH + ih) * inW + iw) * inC
                                    Dim kBase = ((kh * filtW + kw) * inC) * outC + oc

                                    For c As Integer = 0 To inC - 1
                                        dst(xBase + c) += gv * kern(kBase + c * outC)
                                    Next
                                Next
                            Next
                        Next
                    Next
                Next
            Next

            Return result
        End Function

        Public Overridable Function Conv2DBackwardFilter(gradOutput As Tensor, x As Tensor,
                                                         filterShape As Integer(), stride As Integer,
                                                         padding As Integer) As Tensor Implements ITensorCompute.Conv2DBackwardFilter
            Call RequireRank4(gradOutput, "gradOutput")
            Call RequireRank4(x, "x")

            Dim batch = x.Shape(0), inH = x.Shape(1), inW = x.Shape(2), inC = x.Shape(3)
            Dim filtH = filterShape(0), filtW = filterShape(1), outC = filterShape(3)
            Dim outH = gradOutput.Shape(1), outW = gradOutput.Shape(2)

            Dim result = New Tensor(filterShape)
            Dim g = gradOutput.Data
            Dim src = x.Data
            Dim dst = result.Data

            For n As Integer = 0 To batch - 1
                For oh As Integer = 0 To outH - 1
                    For ow As Integer = 0 To outW - 1
                        For oc As Integer = 0 To outC - 1
                            Dim gv = g(((n * outH + oh) * outW + ow) * outC + oc)

                            For kh As Integer = 0 To filtH - 1
                                Dim ih As Integer = oh * stride + kh - padding
                                If ih < 0 OrElse ih >= inH Then Continue For

                                For kw As Integer = 0 To filtW - 1
                                    Dim iw As Integer = ow * stride + kw - padding
                                    If iw < 0 OrElse iw >= inW Then Continue For

                                    Dim xBase = ((n * inH + ih) * inW + iw) * inC
                                    Dim kBase = ((kh * filtW + kw) * inC) * outC + oc

                                    For c As Integer = 0 To inC - 1
                                        dst(kBase + c * outC) += gv * src(xBase + c)
                                    Next
                                Next
                            Next
                        Next
                    Next
                Next
            Next

            Return result
        End Function

        Public Overridable Function Conv2DBackwardBias(gradOutput As Tensor) As Tensor Implements ITensorCompute.Conv2DBackwardBias
            Call RequireRank4(gradOutput, "gradOutput")

            Dim batch = gradOutput.Shape(0), outH = gradOutput.Shape(1)
            Dim outW = gradOutput.Shape(2), outC = gradOutput.Shape(3)

            Dim result = New Tensor(New Integer() {outC})
            Dim g = gradOutput.Data
            Dim dst = result.Data

            For n As Integer = 0 To batch - 1
                For oh As Integer = 0 To outH - 1
                    For ow As Integer = 0 To outW - 1
                        Dim baseIndex = ((n * outH + oh) * outW + ow) * outC

                        For oc As Integer = 0 To outC - 1
                            dst(oc) += g(baseIndex + oc)
                        Next
                    Next
                Next
            Next

            Return result
        End Function

        Public Overridable Function MaxPool2D(x As Tensor, size As Integer, stride As Integer, padding As Integer,
                                              ByRef argMax As Tensor) As Tensor Implements ITensorCompute.MaxPool2D
            Call RequireRank4(x, "x")

            If size <= 0 Then Throw New ArgumentException("池化窗口必须为正数")

            Dim batch = x.Shape(0), inH = x.Shape(1), inW = x.Shape(2), inC = x.Shape(3)
            Dim outH = ConvOutSize(inH, size, stride, padding)
            Dim outW = ConvOutSize(inW, size, stride, padding)

            If outH <= 0 OrElse outW <= 0 Then
                Throw New ArgumentException($"池化输出尺寸非法: {outH}x{outW}")
            End If

            Dim result = New Tensor(batch, outH, outW, inC)
            Dim idx = New Tensor(batch, outH, outW, inC)
            Dim src = x.Data
            Dim dst = result.Data
            Dim dstIdx = idx.Data

            For n As Integer = 0 To batch - 1
                For oh As Integer = 0 To outH - 1
                    For ow As Integer = 0 To outW - 1
                        For c As Integer = 0 To inC - 1
                            ' 越界的窗口位置直接跳过(等价于 -inf), 避免零填充在负数输入时错误胜出
                            Dim best As Double = Double.NegativeInfinity
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
                                ' 整个窗口都落在填充区(极小输入 + 大 padding 才会出现)
                                dst(outIndex) = 0.0
                                dstIdx(outIndex) = 0
                            Else
                                dst(outIndex) = best
                                dstIdx(outIndex) = bestIdx
                            End If
                        Next
                    Next
                Next
            Next

            argMax = idx

            Return result
        End Function

        Public Overridable Function MaxPool2DBackward(gradOutput As Tensor, argMax As Tensor,
                                                      inputShape As Integer()) As Tensor Implements ITensorCompute.MaxPool2DBackward
            Call RequireRank4(gradOutput, "gradOutput")

            Dim result = New Tensor(inputShape)
            Dim g = gradOutput.Data
            Dim idx = argMax.Data
            Dim dst = result.Data

            ' 每个输出位置唯一对应一个输入位置, 因此可以直接累加而无需原子操作
            For i As Integer = 0 To g.Length - 1
                Dim target As Integer = CInt(idx(i))

                If target >= 0 AndAlso target < dst.Length Then
                    dst(target) += g(i)
                End If
            Next

            Return result
        End Function

#End Region

#Region "归约运算"

        Public Overridable Function SumAll(t As Tensor) As Double Implements ITensorCompute.SumAll
            Dim src = t.Data
            Dim sum As Double = 0
            For i As Integer = 0 To src.Length - 1
                sum += src(i)
            Next
            Return sum
        End Function

        Public Overridable Function MeanAll(t As Tensor) As Double Implements ITensorCompute.MeanAll
            Return SumAll(t) / t.Length
        End Function

        Public Overridable Function Sum(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor Implements ITensorCompute.Sum
            If Not axis.HasValue Then
                Return Tensor.Scalar(SumAll(t))
            End If
            Return ReduceAlongAxis(t, axis.Value, keepdims, 0.0, Function(acc, val) acc + val)
        End Function

        Public Overridable Function Mean(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor Implements ITensorCompute.Mean
            If Not axis.HasValue Then
                Return Tensor.Scalar(MeanAll(t))
            End If

            Dim sumResult = ReduceAlongAxis(t, axis.Value, keepdims, 0.0, Function(acc, val) acc + val)
            Dim count = t.Shape(axis.Value)
            Return MultiplyScalar(sumResult, 1.0 / count)
        End Function

        Public Overridable Function Max(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor Implements ITensorCompute.Max
            If Not axis.HasValue Then
                Return Tensor.Scalar(ReduceGlobal(t, Double.NegativeInfinity, Function(acc, val) std.Max(acc, val)))
            End If
            Return ReduceAlongAxis(t, axis.Value, keepdims, Double.NegativeInfinity, Function(acc, val) std.Max(acc, val))
        End Function

        Public Overridable Function Min(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor Implements ITensorCompute.Min
            If Not axis.HasValue Then
                Return Tensor.Scalar(ReduceGlobal(t, Double.PositiveInfinity, Function(acc, val) std.Min(acc, val)))
            End If
            Return ReduceAlongAxis(t, axis.Value, keepdims, Double.PositiveInfinity, Function(acc, val) std.Min(acc, val))
        End Function

        Public Overridable Function Prod(t As Tensor, axis As Integer?) As Tensor Implements ITensorCompute.Prod
            If Not axis.HasValue Then
                Return Tensor.Scalar(ReduceGlobal(t, 1.0, Function(acc, val) acc * val))
            End If
            Return ReduceAlongAxis(t, axis.Value, False, 1.0, Function(acc, val) acc * val)
        End Function

        Public Overridable Function StdDev(t As Tensor) As Double Implements ITensorCompute.StdDev
            Dim mean = MeanAll(t)
            Dim src = t.Data
            Dim sumSq As Double = 0
            For i As Integer = 0 To src.Length - 1
                Dim diff = src(i) - mean
                sumSq += diff * diff
            Next
            Return std.Sqrt(sumSq / src.Length)
        End Function

        Public Overridable Function L2Norm(t As Tensor) As Double Implements ITensorCompute.L2Norm
            Dim src = t.Data
            Dim sumSq As Double = 0
            For i As Integer = 0 To src.Length - 1
                sumSq += src(i) * src(i)
            Next
            Return std.Sqrt(sumSq)
        End Function

        Public Overridable Function ArgMax(t As Tensor, axis As Integer?) As Tensor Implements ITensorCompute.ArgMax
            If Not axis.HasValue Then
                Return Tensor.Scalar(ArgGlobal(t, findMin:=False))
            End If
            Return ReduceArgAxis(t, axis.Value, False)
        End Function

        Public Overridable Function ArgMin(t As Tensor, axis As Integer?) As Tensor Implements ITensorCompute.ArgMin
            If Not axis.HasValue Then
                Return Tensor.Scalar(ArgGlobal(t, findMin:=True))
            End If
            Return ReduceArgAxis(t, axis.Value, True)
        End Function

#End Region

#Region "神经网络"

        Public Overridable Function Softmax(t As Tensor, axis As Integer) As Tensor Implements ITensorCompute.Softmax
            Dim rank = t.Rank
            If axis < 0 Then axis = rank + axis

            If rank = 1 Then
                Dim srcData = t.Data
                Dim dstData(srcData.Length - 1) As Double

                Dim maxVal = srcData(0)
                For i As Integer = 1 To srcData.Length - 1
                    If srcData(i) > maxVal Then maxVal = srcData(i)
                Next

                Dim sumExp As Double = 0
                For i As Integer = 0 To srcData.Length - 1
                    Dim e = std.Exp(srcData(i) - maxVal)
                    dstData(i) = e
                    sumExp += e
                Next

                For i As Integer = 0 To dstData.Length - 1
                    dstData(i) /= sumExp
                Next

                Return New Tensor(dstData, t.Shape)
            End If

            Dim shape = t.Shape
            Dim axisSize = shape(axis)
            Dim outerSize = 1
            For i As Integer = 0 To axis - 1
                outerSize *= shape(i)
            Next
            Dim stride = 1
            For i As Integer = axis + 1 To rank - 1
                stride *= shape(i)
            Next

            Dim srcArr = t.Data
            Dim dstArr(srcArr.Length - 1) As Double

            For outer As Integer = 0 To outerSize - 1
                Dim baseIdx = outer * axisSize * stride
                For innerPtr As Integer = 0 To stride - 1
                    Dim maxVal = srcArr(baseIdx + innerPtr)
                    For k As Integer = 1 To axisSize - 1
                        Dim val = srcArr(baseIdx + k * stride + innerPtr)
                        If val > maxVal Then maxVal = val
                    Next

                    Dim sumExp As Double = 0
                    Dim exps(axisSize - 1) As Double
                    For k As Integer = 0 To axisSize - 1
                        Dim val = srcArr(baseIdx + k * stride + innerPtr)
                        exps(k) = std.Exp(val - maxVal)
                        sumExp += exps(k)
                    Next

                    For k As Integer = 0 To axisSize - 1
                        dstArr(baseIdx + k * stride + innerPtr) = exps(k) / sumExp
                    Next
                Next
            Next

            Return New Tensor(dstArr, shape)
        End Function

        Public Overridable Function LogSoftmax(t As Tensor, axis As Integer) As Tensor Implements ITensorCompute.LogSoftmax
            Dim rank = t.Rank
            If axis < 0 Then axis = rank + axis

            If rank = 1 Then
                Dim srcData = t.Data
                Dim maxVal = srcData(0)
                For i As Integer = 1 To srcData.Length - 1
                    If srcData(i) > maxVal Then maxVal = srcData(i)
                Next

                Dim sumExp As Double = 0
                For i As Integer = 0 To srcData.Length - 1
                    sumExp += std.Exp(srcData(i) - maxVal)
                Next
                Dim logSumExp = maxVal + std.Log(sumExp)

                Dim dstData(srcData.Length - 1) As Double
                For i As Integer = 0 To srcData.Length - 1
                    dstData(i) = srcData(i) - logSumExp
                Next

                Return New Tensor(dstData, t.Shape)
            End If

            Dim shape = t.Shape
            Dim axisSize = shape(axis)
            Dim outerSize = 1
            For i As Integer = 0 To axis - 1
                outerSize *= shape(i)
            Next
            Dim stride = 1
            For i As Integer = axis + 1 To rank - 1
                stride *= shape(i)
            Next

            Dim srcArr = t.Data
            Dim dstArr(srcArr.Length - 1) As Double

            For outer As Integer = 0 To outerSize - 1
                Dim baseIdx = outer * axisSize * stride
                For innerPtr As Integer = 0 To stride - 1
                    Dim maxVal = srcArr(baseIdx + innerPtr)
                    For k As Integer = 1 To axisSize - 1
                        Dim val = srcArr(baseIdx + k * stride + innerPtr)
                        If val > maxVal Then maxVal = val
                    Next

                    Dim sumExp As Double = 0
                    For k As Integer = 0 To axisSize - 1
                        sumExp += std.Exp(srcArr(baseIdx + k * stride + innerPtr) - maxVal)
                    Next
                    Dim logSumExp = maxVal + std.Log(sumExp)

                    For k As Integer = 0 To axisSize - 1
                        dstArr(baseIdx + k * stride + innerPtr) = srcArr(baseIdx + k * stride + innerPtr) - logSumExp
                    Next
                Next
            Next

            Return New Tensor(dstArr, shape)
        End Function

        Public Overridable Function SigmoidCrossEntropyWithLogits(labels As Tensor, logits As Tensor) As Tensor Implements ITensorCompute.SigmoidCrossEntropyWithLogits
            RequireSameShape(labels, logits, "计算 sigmoid 交叉熵")

            Dim srcLabels = labels.Data
            Dim srcLogits = logits.Data
            Dim dst(srcLogits.Length - 1) As Double

            For i As Integer = 0 To dst.Length - 1
                Dim x = srcLogits(i)
                Dim z = srcLabels(i)
                dst(i) = std.Max(x, 0.0) - x * z + std.Log(1.0 + std.Exp(-std.Abs(x)))
            Next

            Return New Tensor(dst, logits.Shape)
        End Function

        Public Overridable Function MseLoss(predictions As Tensor, targets As Tensor) As Tensor Implements ITensorCompute.MseLoss
            RequireSameShape(predictions, targets, "计算均方误差")

            Dim srcPred = predictions.Data
            Dim srcTarget = targets.Data
            Dim sumSq As Double = 0
            For i As Integer = 0 To srcPred.Length - 1
                Dim diff = srcPred(i) - srcTarget(i)
                sumSq += diff * diff
            Next

            Return Tensor.Scalar(sumSq / srcPred.Length)
        End Function

        Public Overridable Function L2Loss(t As Tensor) As Tensor Implements ITensorCompute.L2Loss
            Dim src = t.Data
            Dim sumSq As Double = 0
            For i As Integer = 0 To src.Length - 1
                sumSq += src(i) * src(i)
            Next
            Return Tensor.Scalar(sumSq / 2.0)
        End Function

        Public Overridable Function HuberLoss(predictions As Tensor, targets As Tensor, delta As Double) As Tensor Implements ITensorCompute.HuberLoss
            RequireSameShape(predictions, targets, "计算 Huber 损失")

            Dim srcPred = predictions.Data
            Dim srcTarget = targets.Data
            Dim totalLoss As Double = 0
            For i As Integer = 0 To srcPred.Length - 1
                Dim diff = std.Abs(srcPred(i) - srcTarget(i))
                If diff <= delta Then
                    totalLoss += 0.5 * diff * diff
                Else
                    totalLoss += delta * (diff - 0.5 * delta)
                End If
            Next

            Return Tensor.Scalar(totalLoss / srcPred.Length)
        End Function

#End Region

#Region "内部工具"

        Private Shared Function ReduceGlobal(t As Tensor, initValue As Double,
                                             reduceFunc As Func(Of Double, Double, Double)) As Double
            Dim src = t.Data
            Dim value = initValue
            For i As Integer = 0 To src.Length - 1
                value = reduceFunc(value, src(i))
            Next
            Return value
        End Function

        Private Shared Function ArgGlobal(t As Tensor, findMin As Boolean) As Double
            Dim src = t.Data
            Dim bestIdx = 0
            Dim bestVal = src(0)

            For i As Integer = 1 To src.Length - 1
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

        ''' <summary>沿指定轴执行规约操作（通用实现）</summary>
        Protected Shared Function ReduceAlongAxis(t As Tensor, axis As Integer, keepdims As Boolean,
                                                  initValue As Double,
                                                  reduceFunc As Func(Of Double, Double, Double)) As Tensor
            Dim rank = t.Rank
            Dim origShape = t.Shape

            Dim outerSize = 1
            For i As Integer = 0 To axis - 1
                outerSize *= origShape(i)
            Next

            Dim stride = 1
            For i As Integer = axis + 1 To rank - 1
                stride *= origShape(i)
            Next

            Dim axisSize = origShape(axis)

            Dim outShapeList = New List(Of Integer)
            For i As Integer = 0 To rank - 1
                If i = axis Then
                    If keepdims Then outShapeList.Add(1)
                Else
                    outShapeList.Add(origShape(i))
                End If
            Next

            If outShapeList.Count = 0 Then
                Return Tensor.Scalar(ReduceGlobal(t, initValue, reduceFunc))
            End If

            Dim result = New Tensor(outShapeList.ToArray())
            Dim srcData = t.Data
            Dim dstData = result.Data
            Dim dstIdx = 0

            For outer As Integer = 0 To outerSize - 1
                Dim baseIdx = outer * axisSize * stride
                For inner As Integer = 0 To stride - 1
                    Dim value = initValue
                    For k As Integer = 0 To axisSize - 1
                        value = reduceFunc(value, srcData(baseIdx + k * stride + inner))
                    Next
                    dstData(dstIdx) = value
                    dstIdx += 1
                Next
            Next

            Return result
        End Function

        ''' <summary>沿指定轴找到极值的索引（argmax / argmin）</summary>
        Protected Shared Function ReduceArgAxis(t As Tensor, axis As Integer, findMin As Boolean) As Tensor
            Dim rank = t.Rank
            Dim origShape = t.Shape

            Dim outerSize = 1
            For i As Integer = 0 To axis - 1
                outerSize *= origShape(i)
            Next

            Dim stride = 1
            For i As Integer = axis + 1 To rank - 1
                stride *= origShape(i)
            Next

            Dim axisSize = origShape(axis)

            Dim outShapeList = New List(Of Integer)
            For i As Integer = 0 To rank - 1
                If i <> axis Then outShapeList.Add(origShape(i))
            Next

            Dim result = New Tensor(outShapeList.ToArray())
            Dim srcData = t.Data
            Dim dstData = result.Data
            Dim dstIdx = 0

            For outer As Integer = 0 To outerSize - 1
                Dim baseIdx = outer * axisSize * stride
                For inner As Integer = 0 To stride - 1
                    Dim bestVal = srcData(baseIdx + inner)
                    Dim bestK = 0
                    For k As Integer = 1 To axisSize - 1
                        Dim val = srcData(baseIdx + k * stride + inner)
                        If (findMin AndAlso val < bestVal) OrElse (Not findMin AndAlso val > bestVal) Then
                            bestVal = val
                            bestK = k
                        End If
                    Next
                    dstData(dstIdx) = bestK
                    dstIdx += 1
                Next
            Next

            Return result
        End Function

#End Region

    End Class

End Namespace
