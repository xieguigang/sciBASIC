' ---------------------------------------------------------------------------
' 早停 + 最优权重快照
'
' 为什么 GCN 特别需要它：
'   半监督节点分类的标签集通常只有几十个节点（Karate 只有 2~20 个），
'   几百个 epoch 之后训练损失还能继续降，但验证损失早就回升 —— 也就是
'   在记忆那几十个标签。早停在这个场景不是“可选优化”，而是主要的正则手段之一
'   （另一个是 weight decay）。
'
' 快照语义：保存**参数的深拷贝**，而不是张量引用。回滚时把数值拷贝回去，
' 参数张量的对象身份保持不变 —— 优化器在训练前拿的是这些引用，换对象会让
' 它继续更新一份再也不会被读取的旧数据（静默失效）。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>早停的指标方向</summary>
Public Enum EarlyStoppingMode

    ''' <summary>越小越好（损失）</summary>
    Minimize = 0
    ''' <summary>越大越好（准确率）</summary>
    Maximize = 1

End Enum

''' <summary>
''' 早停：连续 <see cref="Patience"/> 个 epoch 没有达到 <see cref="MinDelta"/> 的改善就要求停止，
''' 并能把参数回滚到最优那一轮。
''' </summary>
Public Class EarlyStopping

    Private _bestValue As Double
    Private _bestEpoch As Integer
    Private _waited As Integer
    Private _hasBest As Boolean
    Private _stopped As Boolean
    Private _improvement As Boolean

    Private _snapshot As List(Of Double())

    ''' <summary>创建早停器。</summary>
    ''' <param name="patience">容忍多少个 epoch 无改善</param>
    ''' <param name="minDelta">被视为“有改善”的最小变化量</param>
    ''' <param name="mode">指标方向</param>
    Public Sub New(Optional patience As Integer = 50,
                   Optional minDelta As Double = 1.0E-4,
                   Optional mode As EarlyStoppingMode = EarlyStoppingMode.Minimize)

        If patience <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(patience))

        Me.Patience = patience
        Me.MinDelta = minDelta
        Me.Mode = mode
    End Sub

    ''' <summary>容忍多少个 epoch 无改善</summary>
    Public Property Patience As Integer
    ''' <summary>被视为“有改善”的最小变化量</summary>
    Public Property MinDelta As Double
    ''' <summary>指标方向</summary>
    Public Property Mode As EarlyStoppingMode

    ''' <summary>历史最优指标值（未记录时为 NaN）</summary>
    Public ReadOnly Property BestValue As Double
        Get
            Return _bestValue
        End Get
    End Property

    ''' <summary>历史最优指标出现在第几个 epoch（从 1 开始；0 表示尚未记录）</summary>
    Public ReadOnly Property BestEpoch As Integer
        Get
            Return _bestEpoch
        End Get
    End Property

    ''' <summary>本次 <see cref="ShouldStop"/> 调用是否刷新了最优</summary>
    Public ReadOnly Property IsImprovement As Boolean
        Get
            Return _improvement
        End Get
    End Property

    ''' <summary>是否已判定应该停止</summary>
    Public ReadOnly Property Stopped As Boolean
        Get
            Return _stopped
        End Get
    End Property

    ''' <summary>自上次改善以来已等待的 epoch 数</summary>
    Public ReadOnly Property Waited As Integer
        Get
            Return _waited
        End Get
    End Property

    ''' <summary>
    ''' 用当前指标值判断是否该停；顺便在改善时保存参数快照。
    ''' </summary>
    ''' <param name="value">当前 epoch 的监控指标</param>
    ''' <param name="epoch">当前 epoch 序号（从 1 开始，用于记录 <see cref="BestEpoch"/>）</param>
    ''' <param name="parameters">需要快照的参数张量；为 Nothing 时只判停不保存</param>
    Public Function ShouldStop(value As Double, Optional epoch As Integer = 1,
                               Optional parameters As List(Of Tensor) = Nothing) As Boolean

        _improvement = False

        If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
            ' 发散本身就是“不该继续”的信号，但不要把它记成最优
            _stopped = True
            Return True
        End If

        If Not _hasBest Then
            _hasBest = True
            _bestValue = value
            _bestEpoch = epoch
            _improvement = True
            _waited = 0

            Call SaveBest(parameters)

            Return False
        End If

        Dim improved As Boolean

        If Mode = EarlyStoppingMode.Maximize Then
            improved = value > _bestValue + MinDelta
        Else
            improved = value < _bestValue - MinDelta
        End If

        If improved Then
            _bestValue = value
            _bestEpoch = epoch
            _improvement = True
            _waited = 0

            Call SaveBest(parameters)

            Return False
        End If

        _waited += 1
        _stopped = _waited >= Patience

        Return _stopped
    End Function

    ''' <summary>保存参数快照（深拷贝数值）。</summary>
    Public Sub SaveBest(parameters As List(Of Tensor))
        If parameters Is Nothing Then Return

        Dim snapshot As New List(Of Double())(parameters.Count)

        For Each parameter In parameters
            Dim copy = CType(parameter.Data.Clone(), Double())
            Call snapshot.Add(copy)
        Next

        _snapshot = snapshot
    End Sub

    ''' <summary>
    ''' 把参数回滚到最优那一轮。
    ''' </summary>
    ''' <returns>是否真的回滚了（没有快照时返回 False）</returns>
    Public Function RestoreBest(parameters As List(Of Tensor)) As Boolean
        If _snapshot Is Nothing OrElse parameters Is Nothing Then Return False
        If _snapshot.Count <> parameters.Count Then Return False

        For i = 0 To parameters.Count - 1
            Dim values = _snapshot(i)

            If values.Length <> parameters(i).Length Then Return False

            ' 写回同一块缓冲区：参数张量的对象身份必须保持不变
            Call Array.Copy(values, parameters(i).Data, values.Length)
            Call parameters(i).MarkHostModified()
        Next

        Return True
    End Function

    ''' <summary>清空状态（复用同一个早停器跑下一组实验时调用）。</summary>
    Public Sub Reset()
        _bestValue = Double.NaN
        _bestEpoch = 0
        _waited = 0
        _hasBest = False
        _stopped = False
        _improvement = False
        _snapshot = Nothing
    End Sub

End Class
