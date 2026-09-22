' ---------------------------------------------------------------------------
' ParameterSet —— 参数注册表
'
' 把"参数张量 → 优化器状态"的配对集中管理，解决三件在 LLM 训练里必须的事：
'
'   1. 一键清零全部梯度：反传前调用 <see cref="ZeroGradients"/>；
'   2. 全局梯度范数裁剪：<see cref="ClipGradients"/> 一次性处理全部参数的梯度，
'      这样才能保持各参数之间的相对梯度尺度（逐参数裁剪会破坏这个关系）；
'   3. 统一的 AdamW 训练步 + 参数/显存占用统计。
'
' 同时它也是"权重衰减只作用于权重矩阵、不作用于 γ"这一惯例的落点：
' <see cref="Add"/> 的 weightDecay 参数由各层按自身语义给出。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 与一组参数张量绑定的优化器状态集合。
''' </summary>
Public Class ParameterSet

    ''' <summary>
    ''' 设备常驻训练的全局总开关。
    ''' </summary>
    ''' <remarks>
    ''' 关掉之后所有参数都走主机 AdamW 循环，与改造前的行为完全一致。
    ''' 保留这个开关有两个用处：
    '''   * 做"设备端 vs 主机端"的 A/B 耗时对比；
    '''   * 当设备路径出现数值问题时，可以一键回退以快速缩小排查范围。
    ''' </remarks>
    Public Shared Property EnableDeviceResidency As Boolean = True

    ''' <summary>单个参数的登记项：名称 + 参数张量 + 它的 AdamW 状态。</summary>
    Public Class Entry

        Friend Sub New(name As String, value As Tensor, optimizer As AdamW)
            _name = name
            _value = value
            _optimizer = optimizer
        End Sub

        Private ReadOnly _name As String
        Private ReadOnly _value As Tensor
        Private ReadOnly _optimizer As AdamW

        ''' <summary>参数名称（用于统计输出与调试定位）。</summary>
        Public ReadOnly Property Name As String
            Get
                Return _name
            End Get
        End Property

        ''' <summary>参数张量本体。</summary>
        Public ReadOnly Property Value As Tensor
            Get
                Return _value
            End Get
        End Property

        ''' <summary>该参数的 AdamW 优化器状态。</summary>
        Public ReadOnly Property Optimizer As AdamW
            Get
                Return _optimizer
            End Get
        End Property

        ''' <summary>该参数的梯度累加器。</summary>
        Public ReadOnly Property Gradient As Tensor
            Get
                Return _optimizer.Gradient
            End Get
        End Property

    End Class

    Private ReadOnly _entries As New List(Of Entry)
    Private ReadOnly _index As New Dictionary(Of String, Entry)

    ''' <summary>登记的参数项（按登记顺序）。</summary>
    Public ReadOnly Property Entries As IList(Of Entry)
        Get
            Return _entries
        End Get
    End Property

    ''' <summary>全部参数的梯度累加器，供全局范数裁剪使用。</summary>
    Public ReadOnly Property Gradients As IEnumerable(Of Tensor)
        Get
            Return _entries.Select(Function(e) e.Gradient)
        End Get
    End Property

    ''' <summary>参数元素总数（即通常所说的"模型参数量"）。</summary>
    Public ReadOnly Property TotalParameters As Long
        Get
            Dim total As Long = 0
            For Each e In _entries
                total += e.Value.Length
            Next
            Return total
        End Get
    End Property

    ''' <summary>
    ''' 参数本体占用的字节数。
    ''' </summary>
    ''' <remarks>
    ''' 注意这只是"权重"的部分：AdamW 还为每个参数额外持有 m、v、g 三份同形缓冲，
    ''' 因此训练时的实际内存需求约为本值的 4 倍。
    ''' </remarks>
    Public ReadOnly Property TotalBytes As Long
        Get
            Return TotalParameters * 8L
        End Get
    End Property

    ''' <summary>
    ''' 登记一个参数张量，并为它新建一份 AdamW 状态。
    ''' </summary>
    ''' <param name="name">参数名称（同一名称重复登记会抛异常，避免静默覆盖）</param>
    ''' <param name="value">参数张量</param>
    ''' <param name="weightDecay">解耦权重衰减系数；γ / 偏置一类参数应传 0</param>
    ''' <param name="deviceResident">是否允许把该参数钉到显存、走设备端 AdamW 更新</param>
    Public Function Add(name As String, value As Tensor, Optional weightDecay As Double = 0.0,
                        Optional deviceResident As Boolean = False) As Entry
        Return Attach(name, value, New AdamW(value), weightDecay, deviceResident)
    End Function

    ''' <summary>
    ''' 登记一个参数张量，并<b>复用调用方已有的 AdamW 状态</b>。
    ''' </summary>
    ''' <remarks>
    ''' 这一点至关重要：各层在构造时就为自己的权重建好了 AdamW（反向传播要往它的
    ''' 梯度累加器里原地累加），如果这里再新建一份，就会出现"梯度写进了 A、更新却读 B"
    ''' 的情况 —— 表现为 loss 完全不下降、全局梯度范数恒为 0，而且不报任何错。
    ''' </remarks>
    ''' <param name="name">参数名称</param>
    ''' <param name="value">参数张量</param>
    ''' <param name="optimizer">调用方持有的 AdamW 状态</param>
    ''' <param name="weightDecay">解耦权重衰减系数</param>
    ''' <param name="deviceResident">
    ''' 是否允许把该参数钉到显存、走设备端 AdamW 更新。
    ''' <b>只有"主机侧不再直接读取该参数"的权重矩阵才应传 True</b> ——
    ''' 详见 <see cref="AdamW.UseDeviceResidency"/> 的说明。
    ''' </param>
    Public Function Attach(name As String, value As Tensor, optimizer As AdamW,
                           Optional weightDecay As Double = 0.0,
                           Optional deviceResident As Boolean = False) As Entry

        If value Is Nothing Then Throw New ArgumentNullException(NameOf(value))
        If optimizer Is Nothing Then Throw New ArgumentNullException(NameOf(optimizer))

        If _index.ContainsKey(name) Then
            Throw New ArgumentException($"参数 '{name}' 已经登记过，不能重复登记")
        End If

        optimizer.WeightDecay = weightDecay
        optimizer.UseDeviceResidency = deviceResident AndAlso EnableDeviceResidency

        Dim entry As New Entry(name, value, optimizer)

        _entries.Add(entry)
        _index.Add(name, entry)

        Return entry
    End Function

    ''' <summary>按名称查找登记项；不存在时返回 <see langword="Nothing"/>。</summary>
    Public Function Find(name As String) As Entry
        Dim entry As Entry = Nothing
        _index.TryGetValue(name, entry)
        Return entry
    End Function

    ''' <summary>清零全部参数的梯度累加器。</summary>
    Public Sub ZeroGradients()
        For Each e In _entries
            e.Optimizer.ZeroGrad()
        Next
    End Sub

    ''' <summary>
    ''' 对全部参数的梯度做全局 L2 范数裁剪。
    ''' </summary>
    ''' <param name="maxNorm">允许的最大全局范数；&lt;= 0 表示不裁剪</param>
    ''' <returns>裁剪前的全局 L2 范数（用于观测训练稳定性）</returns>
    Public Function ClipGradients(Optional maxNorm As Double = 0.0) As Double
        Return LLMTensorOps.ClipGlobalNorm(Gradients, maxNorm)
    End Function

    ''' <summary>
    ''' 对全部参数执行一次 AdamW 更新。
    ''' </summary>
    ''' <param name="learningRate">学习率</param>
    ''' <param name="step">训练步序号（从 1 开始，用于偏差校正）</param>
    ''' <remarks>
    ''' 逐参数"先试设备路径、失败再走主机"：这样一个参数上设备不可用
    ''' （未开启常驻、后端不支持、内核缺失）不会影响其余参数，
    ''' 也不会因为局部失败而让整步训练中断。
    ''' </remarks>
    Public Sub ApplyUpdate(learningRate As Double, [step] As Integer)
        Dim deviceSteps As Integer = 0

        For Each e In _entries
            If e.Optimizer.TryDeviceStep(learningRate, [step], e.Value) Then
                deviceSteps += 1
            Else
                e.Optimizer.MakeTrainingStep(learningRate, [step], e.Value)
            End If
        Next

        _deviceUpdatedCount = deviceSteps
    End Sub

    Private _deviceUpdatedCount As Integer

    ''' <summary>最近一次 <see cref="ApplyUpdate"/> 中真正走了设备路径的参数个数。</summary>
    ''' <remarks>
    ''' 这是"设备常驻训练到底生效了没有"的最直接证据：
    ''' 如果恒为 0，说明所有参数都在走主机循环（未开启常驻、后端不支持、或内核缺失）。
    ''' </remarks>
    Public ReadOnly Property DeviceUpdatedCount As Integer
        Get
            Return _deviceUpdatedCount
        End Get
    End Property

    ''' <summary>
    ''' 把全部被钉住的参数的设备内容回写到主机。
    ''' </summary>
    ''' <returns>实际被回写的参数个数</returns>
    ''' <remarks>
    ''' 设备端 AdamW 会让主机副本变陈旧，因此在"读主机内容"的场合之前必须调用：
    ''' 检查点落盘、CPU 推理、以及任何在主机循环里直接读权重的模块。
    ''' </remarks>
    Public Function SyncFromDevice() As Integer
        Dim kernel = Tensor.computeKernel
        Dim synced As Integer = 0

        For Each e In _entries
            If kernel.IsDevicePinned(e.Value) AndAlso kernel.SyncFromDevice(e.Value) Then
                synced += 1
            End If
        Next

        Return synced
    End Function

    ''' <summary>当前被钉在显存里的参数字节数。</summary>
    Public ReadOnly Property PinnedBytes As Long
        Get
            Return Tensor.computeKernel.PinnedDeviceBytes
        End Get
    End Property

    ''' <summary>
    ''' 输出一份按参数名聚合的参数量统计，便于核对模型规模。
    ''' </summary>
    Public Function Describe() As String
        Const pattern As String = "{0,-48}{1,-22}{2,14:N0}"

        Dim sb As New System.Text.StringBuilder()
        Dim line As New String("-"c, 84)

        Call sb.AppendLine(String.Format(pattern, "parameter", "shape", "count"))
        Call sb.AppendLine(line)

        For Each e In _entries
            Call sb.AppendLine(String.Format(
                pattern, e.Name, "[" & String.Join(",", e.Value.Shape) & "]", e.Value.Length))
        Next

        Call sb.AppendLine(line)
        Call sb.AppendLine(String.Format(pattern, "TOTAL", "", TotalParameters))

        Return sb.ToString()
    End Function

End Class


