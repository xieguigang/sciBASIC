Option Strict On
Option Explicit On

Imports System

Namespace TSK

    ''' <summary>
    ''' TSK 前向 / 反向的**可复用工作区**：把每个行块都要重新分配的中间量预先分配一次。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 训练时每个 epoch 会把数据切成若干行块（默认 512 行），原来的实现每个行块都重新分配
    ''' <c>X / Mu / LogMu / W / Wbar / F / Z / Prod</c> 等整套缓冲（n=4、R=16、512 行时约 345 KB，
    ''' 其中多个数组超过 85 KB 的大对象阈值直接进 LOH）。3 万样本 × 1.8 万基因的规模下
    ''' 这会变成 TB 级的大对象分配，GC 成为主要瓶颈。
    ''' </para>
    ''' <para>
    ''' 工作区按"容量 = 最大行块行数"一次性分配，之后每次前向只更新
    ''' <see cref="TskInferenceTrace.SampleCount"/> 与 <see cref="TskInferenceTrace.RowStart"/>。
    ''' 所有消费者都只遍历 <c>t &lt; SampleCount</c>，padding 区不会被读到；
    ''' 但**调用方不得跨调用持有返回的 trace 引用**——下一次前向会就地覆盖同一批数组。
    ''' </para>
    ''' <para>
    ''' 本类型**不是线程安全的**：每个训练器实例独占一份（逐基因训练时天然就是每线程一份）。
    ''' </para>
    ''' </remarks>
    Public Class TskForwardWorkspace

        ''' <summary>工作区容量（可容纳的最大行块行数）。</summary>
        Public Property Capacity As Integer

        ''' <summary>绑定的网络结构。</summary>
        Public Property Network As TskNetwork

        ''' <summary>复用的前向结果（只更新 <c>SampleCount</c> / <c>RowStart</c>）。</summary>
        Public Property Trace As TskInferenceTrace

        ''' <summary>复用的梯度对象（每次 <see cref="ClearGradient"/> 后重新累加）。</summary>
        Public Property Gradient As TskGradient

        ''' <summary>输入矩阵缓冲，形状 <c>[Capacity, n]</c>。</summary>
        Friend Property X As Double(,)
        ''' <summary>隶属度缓冲，形状 <c>[Capacity, sum(m_i)]</c>。</summary>
        Friend Property Mu As Double(,)
        ''' <summary>隶属度对数缓冲，形状 <c>[Capacity, sum(m_i)]</c>。</summary>
        Friend Property LogMu As Double(,)
        ''' <summary>规则触发强度缓冲，形状 <c>[Capacity, R]</c>。</summary>
        Friend Property W As Double(,)
        ''' <summary>归一化触发强度缓冲，形状 <c>[Capacity, R]</c>。</summary>
        Friend Property Wbar As Double(,)
        ''' <summary>规则结论值缓冲，形状 <c>[Capacity, R, o]</c>。</summary>
        Friend Property F As Double(,,)
        ''' <summary>网络输出缓冲，形状 <c>[Capacity, o]</c>。</summary>
        Friend Property Z As Double(,)
        ''' <summary>矩阵乘法目标缓冲，形状 <c>[Capacity, R]</c>。</summary>
        Friend Property Prod As Double(,)
        ''' <summary>结论系数转置缓冲，形状 <c>[n, R]</c>。</summary>
        Friend Property Pt As Double(,)
        ''' <summary>每行触发强度之和。</summary>
        Friend Property SumW As Double()
        ''' <summary>每行是否有效。</summary>
        Friend Property Valid As Boolean()
        ''' <summary>每个输入的模糊集列偏移。</summary>
        Friend Property MfOffsets As Integer()

        ''' <summary>输出误差缓冲，形状 <c>[Capacity, o]</c>。</summary>
        Friend Property Delta As Double(,)
        ''' <summary>反向传播系数缓冲，形状 <c>[Capacity, R]</c>。</summary>
        Friend Property Coefficient As Double(,)
        ''' <summary>单个输入的隶属度梯度累加缓冲，形状 <c>[Capacity, max(m_i)]</c>。</summary>
        Friend Property Column As Double(,)
        ''' <summary>隶属函数对数梯度缓冲，形状 <c>[Capacity, max(pc)]</c>。</summary>
        Friend Property LogGrad As Double(,)

        ''' <summary>按容量与网络结构一次性分配全部缓冲。</summary>
        ''' <param name="capacity">可容纳的最大行块行数（必须 &gt; 0）</param>
        ''' <param name="network">网络结构（决定各缓冲的列数）</param>
        ''' <param name="includeForwardBuffers">
        ''' 是否同时分配前向缓冲；只做反向累加（<see cref="TskLayers.Accumulate(TskInferenceTrace, Double(,), Boolean, TskForwardWorkspace)"/>）
        ''' 时可以传 <c>False</c>，省掉一整套用不到的前向数组
        ''' </param>
        Public Sub New(capacity As Integer, network As TskNetwork, Optional includeForwardBuffers As Boolean = True)
            If network Is Nothing Then
                Throw New ArgumentNullException(NameOf(network))
            End If
            If capacity <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(capacity), capacity, "the workspace capacity must be positive")
            End If

            Me.Capacity = capacity
            Me.Network = network

            Dim n As Integer = network.InputCount
            Dim rCount As Integer = network.Rules.RuleCount
            Dim oCount As Integer = network.OutputCount
            Dim totalMf As Integer = 0
            Dim maxMf As Integer = 1
            Dim maxParameter As Integer = 1

            For i As Integer = 0 To n - 1
                Dim mfs As IMembershipFunction() = network.Inputs(i).Functions

                totalMf += mfs.Length

                If mfs.Length > maxMf Then
                    maxMf = mfs.Length
                End If

                For k As Integer = 0 To mfs.Length - 1
                    If mfs(k).ParameterCount > maxParameter Then
                        maxParameter = mfs(k).ParameterCount
                    End If
                Next
            Next

            If totalMf < 1 Then totalMf = 1

            Dim offsets As Integer() = New Integer(n - 1) {}
            Dim cursor As Integer = 0

            For i As Integer = 0 To n - 1
                offsets(i) = cursor
                cursor += network.Inputs(i).FunctionCount
            Next

            MfOffsets = offsets

            If includeForwardBuffers Then
                X = New Double(capacity - 1, n - 1) {}
                Mu = New Double(capacity - 1, totalMf - 1) {}
                LogMu = New Double(capacity - 1, totalMf - 1) {}
                W = New Double(capacity - 1, rCount - 1) {}
                Wbar = New Double(capacity - 1, rCount - 1) {}
                F = New Double(capacity - 1, rCount - 1, oCount - 1) {}
                Z = New Double(capacity - 1, oCount - 1) {}
                Prod = New Double(capacity - 1, rCount - 1) {}
                Pt = New Double(n - 1, rCount - 1) {}
                SumW = New Double(capacity - 1) {}
                Valid = New Boolean(capacity - 1) {}

                Trace = New TskInferenceTrace With {
                    .InputCount = n,
                    .RuleCount = rCount,
                    .OutputCount = oCount,
                    .X = X,
                    .Mu = Mu,
                    .LogMu = LogMu,
                    .MfOffsets = MfOffsets,
                    .W = W,
                    .Wbar = Wbar,
                    .SumW = SumW,
                    .Valid = Valid,
                    .F = F,
                    .Z = Z
                }
            End If

            Delta = New Double(capacity - 1, oCount - 1) {}
            Coefficient = New Double(capacity - 1, rCount - 1) {}
            Column = New Double(capacity - 1, maxMf - 1) {}
            LogGrad = New Double(capacity - 1, maxParameter - 1) {}

            Gradient = New TskGradient(network)
        End Sub

        ''' <summary>把复用的梯度对象清零（每次反向累加之前调用）。</summary>
        Public Sub ClearGradient()
            Call Array.Clear(Gradient.Premise, 0, Gradient.Premise.Length)
            Call Array.Clear(Gradient.Theta, 0, Gradient.Theta.Length)
            Call Array.Clear(Gradient.RuleActivation, 0, Gradient.RuleActivation.Length)

            Gradient.SquaredError = 0.0
            Gradient.SampleCount = 0
        End Sub

        ''' <summary>
        ''' 清空前向状态（<c>W / Wbar / F / Z</c>）：复用缓冲时，
        ''' 失效行（触发强度下溢为 0）必须显式归零，不能残留上一块的数据。
        ''' </summary>
        Friend Sub ClearForwardState()
            If W Is Nothing Then
                Return
            End If

            Call Array.Clear(W, 0, W.Length)
            Call Array.Clear(Wbar, 0, Wbar.Length)
            Call Array.Clear(F, 0, F.Length)
            Call Array.Clear(Z, 0, Z.Length)
        End Sub

    End Class

End Namespace
