#Region "Microsoft.VisualBasic::97fc2e7834d4529fc24256353875d3ca, Data_science\MachineLearning\SNN\SparseLIFLayer.vb"

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

    '   Total Lines: 176
    '    Code Lines: 89 (50.57%)
    ' Comment Lines: 50 (28.41%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 37 (21.02%)
    '     File Size: 6.77 KB


    ' Class SparseLIFLayer
    ' 
    '     Properties: Beta, Counts, FallbackSteps, InputSize, KeepHistory
    '                 LastStepPath, Membrane, Name, ResetMode
    '                 ResidentPrecision, SHistory, Synapses, Threshold
    '                 UHistory, Units, UseFusedStep
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: ForwardStep, SyncFromDevice, ToString
    ' 
    '     Sub: ReleaseDeviceBuffers, ResetState
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' SparseLIFLayer.vb — 稀疏递归 LIF 层（单层大稀疏连接）
'
' 与稠密 LIFLayer 的区别：
'   LIFLayer 为多输入 → 多输出的前馈全连接层（Weight [in, units] 稠密）。
'   本层为"同一神经元群"内部的稀疏突触连接：Synapses 是方阵 W[N, N]，
'   W[pre, post] 表示突触前神经元 pre 连到突触后神经元 post（含自反馈 pre=post）。
'
' 每个时间步的前向动态（readme 模块二）：
'   I_rec[t] = W · S[t−1]                递归输入：上一时刻脉冲经稀疏矩阵回灌（含循环/自反馈）
'   I[t]     = I_ext[t] + I_rec[t]        叠加外部注入电流（如感觉神经元的刺激）
'   U[t]     = β·H[t−1] + I[t]           泄漏积分
'   S[t]     = Θ(U[t] − U_thr)           阈值触发（二值脉冲）
'   H[t]     = 复位(U[t], S[t])          发放后复位
'
' 由于 S[t−1] 作为跨时间步状态参与计算，本层天然是一个脉冲递归网络（SRNN），
' 对连接矩阵中存在的循环连接与自反馈均正确建模——这正是真实连接组仿真的核心。
'
' 注意：本层仅用于前向仿真（权重固定），不保存 BPTT 反向缓存，也不提供反向传播。
' ============================================================================

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports std = System.Math

''' <summary>
''' 融合 LIF 状态在设备上的精度档位。
''' </summary>
Public Enum LifResidentPrecision

    ''' <summary>
    ''' 双精度常驻（默认）：膜电位与计数累加器都以 Double 留在显存。
    ''' </summary>
    ''' <remarks>
    ''' 递归状态是逐时间步累积的，单精度舍入会让个别神经元恰好跨过阈值，
    ''' 从而在脉冲计数上产生整数差异 —— 需要「CPU / GPU 对拍逐位一致」时必须用本档位。
    ''' </remarks>
    Double64 = 0

    ''' <summary>
    ''' 单精度常驻（最快档）：膜电位与计数用 Single，脉冲仍用 Double。
    ''' </summary>
    ''' <remarks>
    ''' 显存与带宽占用减半。脉冲保持双精度是有意的：下一步的稀疏乘法要复用它，
    ''' 而 0/1 在两种精度下都是精确值，所以只有膜电位会引入舍入。
    ''' </remarks>
    Single32 = 1

End Enum

''' <summary>
''' 某个时间步实际走的那条前向路径（诊断 / 报告用）。
''' </summary>
Public Enum LifStepPath

    ''' <summary>尚未执行任何时间步</summary>
    None = 0

    ''' <summary>逐算子路径（SpMM + Add + MultiplyScalar + Heaviside + 复位）</summary>
    OpByOp = 1

    ''' <summary>融合算子路径（后端用一次调用完成整步，可配合设备常驻状态）</summary>
    Fused = 2

End Enum

Public Class SparseLIFLayer

#Region "超参数与连接"

    Public ReadOnly Property Name As String

    ''' <summary>输入特征维度（外部注入电流的原始特征数，经 inputMap 散射到神经元）</summary>
    Public ReadOnly Property InputSize As Integer

    ''' <summary>神经元数量 N（= Synapses 的行列数）</summary>
    Public ReadOnly Property Units As Integer

    ''' <summary>稀疏突触连接矩阵 W[N, N]：行 = 突触前，列 = 突触后</summary>
    Public ReadOnly Property Synapses As SparseMatrix

    ''' <summary>膜电位衰减系数 β ∈ (0,1)，越小遗忘越快</summary>
    Public Property Beta As Double

    ''' <summary>发放阈值 U_thr</summary>
    Public Property Threshold As Double

    ''' <summary>复位模式</summary>
    Public Property ResetMode As LIFResetMode

#End Region

#Region "状态与轨迹缓存"

    ''' <summary>膜电位 H（跨时间步持续）[batch, Units]</summary>
    Private _H As Tensor

    ''' <summary>上一时间步的输出脉冲 S[t−1]，作为本步递归输入来源 [batch, Units]</summary>
    Private _Sprev As Tensor

    ''' <summary>
    ''' 本步脉冲的写入目标 <c>S[t]</c> [batch, Units]。
    ''' </summary>
    ''' <remarks>
    ''' 与 <see cref="_Sprev"/> 构成双缓冲：每步结束后两者交换引用，
    ''' 于是「本步的输出」天然成为「下一步的输入」，全程零拷贝。
    ''' 这也是融合算子能跨步复用同一片显存缓冲的前提。
    ''' </remarks>
    Private _Sout As Tensor

    ''' <summary>
    ''' 脉冲计数累加器 <c>Σ_t S[t]</c> [batch, Units]。
    ''' </summary>
    ''' <remarks>
    ''' 由融合算子在<b>设备端</b>累加，整个仿真窗只需最终回读一次 ——
    ''' 这正是它取代 <c>Decoder.SpikeCounts</c> 的 O(T·N) 主机循环的关键。
    ''' </remarks>
    Private _counts As Tensor

    Private _U As New List(Of Tensor)()
    Private _S As New List(Of Tensor)()

    ''' <summary>本层是否曾成功走过融合算子路径（诊断用）</summary>
    Private _fusedReady As Boolean = False

    ''' <summary>回退到逐算子路径的次数（一个仿真窗内最多发生一次状态切换）</summary>
    Private _fallbackSteps As Integer = 0

    Private _lastPath As LifStepPath = LifStepPath.None

    ''' <summary>
    ''' 是否优先使用融合单步算子（默认 <c>True</c>）。
    ''' </summary>
    ''' <remarks>
    ''' 融合路径与逐算子路径<b>数值等价</b>（相同的循环次序与结合次序），
    ''' 但把每步 6 个 <c>[batch, Units]</c> 中间张量压成 1 个，并在 GPU 后端下消除
    ''' 每步 6~12 次显存往返。置为 <c>False</c> 可强制走逐算子路径
    ''' （唯一区别是能拿到 <see cref="UHistory"/>，见 <see cref="KeepHistory"/>）。
    ''' </remarks>
    Public Property UseFusedStep As Boolean = True

    ''' <summary>
    ''' 是否记录逐步轨迹（默认 <c>True</c>，保持既有行为）。
    ''' </summary>
    ''' <remarks>
    ''' <c>True</c>：每一步的脉冲都会同步回主机并<b>拷贝</b>一份存入 <see cref="SHistory"/>，
    ''' 供 <c>Decoder</c> 统计与栅格图使用；代价是每步一次 <c>[batch, Units]</c> 回读。
    ''' <para>
    ''' <c>False</c>：只维护设备端的计数累加器（见 <see cref="Counts"/>），
    ''' 整个仿真窗零逐步回读 —— 这是"全脑规模、只关心发放计数"场景的推荐配置。
    ''' </para>
    ''' 注意：<see cref="UHistory"/> 只在逐算子路径下产生（融合算子内部不落 <c>U</c>），
    ''' 需要膜电位轨迹时请把 <see cref="UseFusedStep"/> 置为 <c>False</c>。
    ''' </remarks>
    Public Property KeepHistory As Boolean = True

    ''' <summary>
    ''' 设备常驻状态的精度档位（默认 <see cref="LifResidentPrecision.Double64"/>）。
    ''' </summary>
    ''' <remarks>仅在 <see cref="ResetState"/> 时生效（档位变化需要重新分配常驻缓冲）。</remarks>
    Public Property ResidentPrecision As LifResidentPrecision = LifResidentPrecision.Double64

    ''' <summary>最近一个时间步实际使用的路径。</summary>
    Public ReadOnly Property LastStepPath As LifStepPath
        Get
            Return _lastPath
        End Get
    End Property

    ''' <summary>回退到逐算子路径的时间步数（<c>0</c> 表示整段仿真都走在融合路径上）。</summary>
    Public ReadOnly Property FallbackSteps As Integer
        Get
            Return _fallbackSteps
        End Get
    End Property

    ''' <summary>
    ''' 脉冲计数累加器 <c>Σ_t S[t]</c> [batch, Units]。
    ''' </summary>
    ''' <remarks>
    ''' 该张量在 GPU 后端下<b>以设备为主副本</b>：读主机内容之前必须先调用
    ''' <see cref="SyncFromDevice"/>。
    ''' </remarks>
    Public ReadOnly Property Counts As Tensor
        Get
            Return _counts
        End Get
    End Property

    ''' <summary>当前膜电位 <c>H[t]</c>（设备为主副本，读主机内容前需同步）。</summary>
    Public ReadOnly Property Membrane As Tensor
        Get
            Return _H
        End Get
    End Property

    ''' <summary>最近一次仿真中各时间步的触发前膜电位轨迹（可视化用）</summary>
    Public ReadOnly Property UHistory As List(Of Tensor)
        Get
            Return _U
        End Get
    End Property

    ''' <summary>最近一次仿真中各时间步的输出脉冲轨迹（可视化用）</summary>
    Public ReadOnly Property SHistory As List(Of Tensor)
        Get
            Return _S
        End Get
    End Property

#End Region

#Region "构造与状态重置"

    ''' <summary>
    ''' 构建稀疏递归 LIF 层。
    ''' </summary>
    ''' <param name="synapses">方阵稀疏突触连接 W[N, N]（行=pre，列=post）</param>
    Public Sub New(name As String, inputSize As Integer, synapses As SparseMatrix,
                   Optional beta As Double = 0.9,
                   Optional threshold As Double = 1.0,
                   Optional resetMode As LIFResetMode = LIFResetMode.ZeroOnSpike)

        If synapses Is Nothing Then
            Throw New ArgumentNullException(NameOf(synapses))
        End If
        If synapses.Rows <> synapses.Columns Then
            Throw New ArgumentException(
                $"稀疏连接矩阵必须为方阵（pre/post 为同一神经元群），实际 {synapses.Rows}x{synapses.Columns}")
        End If

        Me.Name = name
        Me.InputSize = inputSize
        Me.Units = synapses.Columns
        Me.Synapses = synapses
        Me.Beta = beta
        Me.Threshold = threshold
        Me.ResetMode = resetMode

        _H = New Tensor(1, Units)
        _Sprev = New Tensor(1, Units)
        _Sout = New Tensor(1, Units)
        _counts = New Tensor(1, Units)
    End Sub

    ''' <summary>
    ''' 重置膜电位、递归脉冲状态与轨迹缓存。每个样本批次开始前必须调用！
    ''' </summary>
    ''' <remarks>
    ''' 本方法同时负责<b>设备常驻缓冲的分配与释放</b>：上一轮的张量会被解除钉住
    ''' （否则它们占着的显存会一直留到后端 Dispose），新一轮的四个状态张量会按
    ''' <see cref="ResidentPrecision"/> 重新钉住。钉住失败（显存不足或后端不支持）时
    ''' 自动退回逐算子路径，功能不受影响。
    ''' </remarks>
    Public Sub ResetState(batchSize As Integer)
        If batchSize <= 0 Then
            Throw New ArgumentException($"batchSize 必须为正整数，实际 {batchSize}", NameOf(batchSize))
        End If

        Call ReleaseDeviceState()

        _H = New Tensor(batchSize, Units)
        _Sprev = New Tensor(batchSize, Units)
        _Sout = New Tensor(batchSize, Units)
        _counts = New Tensor(batchSize, Units)
        _U = New List(Of Tensor)()
        _S = New List(Of Tensor)()
        _fallbackSteps = 0
        _lastPath = LifStepPath.None
        _fusedReady = PrepareDeviceState()
    End Sub

    ''' <summary>
    ''' 按当前档位把四个状态张量钉成设备常驻缓冲。
    ''' </summary>
    ''' <returns>融合路径是否可用</returns>
    ''' <remarks>
    ''' 分档策略：
    ''' <list type="bullet">
    '''   <item>脉冲 <c>S_prev</c> / <c>S</c> 一律走双精度通道 —— 它是下一步稀疏乘法的
    '''         dense 输入，而 0/1 在两种精度下都是精确值，因此复用双精度内核最省事；</item>
    '''   <item>膜电位与计数累加器按 <see cref="ResidentPrecision"/> 选择通道。</item>
    ''' </list>
    ''' </remarks>
    Private Function PrepareDeviceState() As Boolean
        If Not UseFusedStep Then Return False

        Dim backend = Tensor.computeKernel

        ' CPU 后端：融合实现直接就地写主机数组，不需要常驻缓冲
        If Not backend.SupportsDeviceResidency Then Return True

        Dim ok As Boolean = backend.PinDevice64(_Sprev, "lif.S_prev", zeroFill:=False)
        ok = ok AndAlso backend.PinDevice64(_Sout, "lif.S", zeroFill:=False)

        If ResidentPrecision = LifResidentPrecision.Double64 Then
            ok = ok AndAlso backend.PinDevice64(_H, "lif.H", zeroFill:=False)
            ok = ok AndAlso backend.PinDevice64(_counts, "lif.counts", zeroFill:=True)
        Else
            ok = ok AndAlso backend.PinDevice(_H, "lif.H", zeroFill:=False)
            ok = ok AndAlso backend.PinDevice(_counts, "lif.counts", zeroFill:=True)
        End If

        If Not ok Then
            ' 常驻失败（显存不足 / 后端不支持）：解除已钉住的部分，整体退回逐算子路径
            Call ReleaseDeviceState()
        End If

        Return ok
    End Function

    ''' <summary>解除四个状态张量的设备常驻缓冲（幂等；未钉住时是空操作）。</summary>
    Private Sub ReleaseDeviceState()
        Dim backend = Tensor.computeKernel

        If backend.SupportsDeviceResidency Then
            If _H IsNot Nothing Then Call backend.UnpinDevice(_H)
            If _Sprev IsNot Nothing Then Call backend.UnpinDevice(_Sprev)
            If _Sout IsNot Nothing Then Call backend.UnpinDevice(_Sout)
            If _counts IsNot Nothing Then Call backend.UnpinDevice(_counts)
        End If
    End Sub

    ''' <summary>
    ''' 把设备常驻状态（膜电位 / 上一时刻脉冲 / 计数累加器）回读到主机数组。
    ''' </summary>
    ''' <returns>实际完成同步的张量个数（<c>0</c> 表示没有常驻状态需要同步）</returns>
    ''' <remarks>
    ''' GPU 后端下状态以设备为主副本，凡是"读主机内容"的场合（解码统计、栅格图、
    ''' 检查点落盘、切换到逐算子路径之前）都必须先调用本方法。
    ''' 未使用常驻缓冲时是空操作，不构成性能负担。
    ''' </remarks>
    Public Function SyncFromDevice() As Integer
        Dim backend = Tensor.computeKernel
        Dim synced As Integer = 0

        If backend.SyncFromDevice(_H) Then synced += 1
        If backend.SyncFromDevice(_Sprev) Then synced += 1
        If backend.SyncFromDevice(_counts) Then synced += 1

        Return synced
    End Function

    ''' <summary>
    ''' 解除本层的设备常驻缓冲，立即归还显存。
    ''' </summary>
    ''' <remarks>
    ''' 仿真结束后应当调用（或者在下一轮 <see cref="ResetState"/> 时自动发生）：
    ''' 常驻缓冲不参与后端的 LRU 淘汰，一直挂着会白占显存。
    ''' </remarks>
    Public Sub ReleaseDeviceBuffers()
        Call ReleaseDeviceState()
        _fusedReady = False
    End Sub

#End Region

#Region "前向传播"

    ''' <summary>
    ''' 单时间步前向：外部注入电流 → 递归稀疏输入 → LIF 触发 → 复位 → 更新递归状态。
    ''' </summary>
    ''' <param name="externalCurrent">
    ''' 外部注入电流 [batch, Units]（可为 Nothing 表示本步无外部刺激）。
    ''' 通常由编码后的脉冲按 inputMap 散射得到。
    ''' </param>
    ''' <returns>本时间步输出脉冲 S[t] [batch, Units]</returns>
    Public Function ForwardStep(externalCurrent As Tensor) As Tensor
        Dim batch = _Sprev.Shape(0)

        ' 形状校验（两条路径共用同一套判据，避免"换路径就换契约"）
        If externalCurrent IsNot Nothing Then
            If externalCurrent.Rank <> 2 OrElse
               externalCurrent.Shape(0) <> batch OrElse
               externalCurrent.Shape(1) <> Units Then
                Throw New ArgumentException(
                    $"externalCurrent 形状应为 [batch, {Units}]，实际 [{String.Join(",", externalCurrent.Shape)}]")
            End If
        End If

        ' ---- 融合路径：一次调用完成整步，并就地更新四个状态张量 ----
        If _fusedReady AndAlso
           Tensor.computeKernel.LifStep(Synapses.Csr, _Sprev, externalCurrent,
                                        _H, _Sout, _counts, Beta, Threshold,
                                        ResetMode = LIFResetMode.SubtractThreshold) Then

            Dim S = _Sout

            ' 双缓冲交换：本步的输出脉冲天然成为下一步的递归输入（零拷贝）
            _Sout = _Sprev
            _Sprev = S
            _lastPath = LifStepPath.Fused

            Call RecordStep(S, fused:=True)

            Return S
        End If

        ' ---- 兼容回退：逐算子路径（既有实现，数值语义不变）----
        If _fusedReady Then
            _fusedReady = False

            ' 解除常驻：此后以主机为准。若继续留着常驻缓冲，主机写入会让设备副本陈旧，
            ' 而后续任何一次 SyncFromDevice 都会用陈旧设备数据覆盖正确的主机结果。
            Call ReleaseDeviceState()
        End If

        _fallbackSteps += 1
        _lastPath = LifStepPath.OpByOp

        Return forwardStepByOperators(externalCurrent)
    End Function

    ''' <summary>
    ''' 逐算子路径：<c>SpMM + Add + MultiplyScalar + Heaviside + 复位</c>。
    ''' </summary>
    ''' <remarks>
    ''' 这是 <see cref="ForwardStep"/> 的兼容实现，也是 <see cref="UseFusedStep"/> 之前的唯一行为：
    ''' 每步分配 6 个 <c>[batch, Units]</c> 中间张量。它保留下来有两个用途：
    ''' 后端不支持融合算子时保证功能不缺失，以及提供含 <c>U</c> 的完整轨迹。
    ''' </remarks>
    Private Function forwardStepByOperators(externalCurrent As Tensor) As Tensor
        ' 1. 递归输入：上一时刻脉冲经稀疏突触矩阵回灌（含循环连接与自反馈）
        Dim I = Synapses.SpMM(_Sprev)

        ' 2. 叠加外部注入电流
        If externalCurrent IsNot Nothing Then
            I = I + externalCurrent
        End If

        ' 3. 泄漏积分：U = β·H + I
        Dim U = I + (_H * CSng(Beta))

        ' 4. 阈值触发：S = Θ(U − U_thr)
        Dim S = U.Apply(Function(v As Double) Surrogate.Spike(v - Threshold))

        ' 5. 复位
        Dim H As Tensor
        If ResetMode = LIFResetMode.ZeroOnSpike Then
            H = U.ElementwiseMultiply(S.Apply(Function(v As Double) 1.0 - v))
        Else
            H = U - (S * CSng(Threshold))
        End If

        If KeepHistory Then
            _U.Add(U)
            _S.Add(S)
        End If

        ' 计数累加器：融合路径由设备内核累加，这里补上（保证两条路径的 Counts 语义一致）
        Call AccumulateCounts(S)

        _H = H
        _Sprev = S

        Return S
    End Function

    ''' <summary>
    ''' 记录一个时间步的脉冲轨迹（<see cref="KeepHistory"/> 关闭时不做任何事）。
    ''' </summary>
    ''' <param name="S">本步的输出脉冲张量</param>
    ''' <param name="fused">
    ''' 该张量是否来自融合路径。融合路径下 <c>S</c> 的缓冲会<b>跨步复用</b>（双缓冲），
    ''' 且 GPU 后端下以设备为主副本；逐算子路径每步都产生一个全新的主机张量。
    ''' </param>
    ''' <remarks>
    ''' 正因为融合路径复用了缓冲，这里必须同步之后<b>拷贝</b>一份快照 ——
    ''' 否则 <see cref="SHistory"/> 里会挂着同一片被反复改写的缓冲
    ''' （<c>Decoder</c> 的统计与栅格图会把最后一步的脉冲重复计入）。
    ''' </remarks>
    Private Sub RecordStep(S As Tensor, fused As Boolean)
        If Not KeepHistory Then Return

        If Not fused Then
            ' 逐算子路径：S 是每步新建的主机张量，直接引用即可
            _S.Add(S)

            Return
        End If

        Call Tensor.computeKernel.SyncFromDevice(S)
        _S.Add(CType(S.Clone(), Tensor))
    End Sub

    ''' <summary>就地累加计数累加器：<c>counts += S</c>（主机循环，仅逐算子路径使用）。</summary>
    Private Sub AccumulateCounts(S As Tensor)
        If _counts Is Nothing OrElse _counts.Length <> S.Length Then Return

        Dim cd = _counts.Data
        Dim sd = S.Data

        For i As Integer = 0 To cd.Length - 1
            cd(i) += sd(i)
        Next

        Call _counts.MarkHostModified()
    End Sub

#End Region

    Public Overrides Function ToString() As String
        Return $"{Name}(SparseLIF, N={Units}, nnz={Synapses.NonZeros}, β={Beta})"
    End Function

End Class
