#Region "Microsoft.VisualBasic::gtop-process-monitor, cuda\GTop\ProcessMonitor.vb"

    ' Author:
    '       xieguigang (xie.guigang@live.com)
    '
    ' Copyright (c) sciBASIC.NET Foundation
    ' GNU GENERAL PUBLIC LICENSE (GPL3)

#End Region

Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic.Computing.ILCuda.Runtime

''' <summary>
''' 每进程 GPU 使用监控（有状态）：
'''   * 按 PID 记录最近的 CUDA 利用率历史（用于表格里的迷你曲线）；
'''   * 进程失去 GPU 活动时不立即移除，而是持续 <see cref="Ttl"/>（默认 1 分钟）后移除；
'''   * 输出按 (CUDA%+1)×(显存占用%+1)×(Copy%+1)×(编码%+解码%+1) 的乘积降序排序。
''' 其中"显存占用%" = 该进程专用显存 / 整卡显存总量 × 100（Windows 无每进程显存带宽利用率，故以此代理）。
''' </summary>
Public NotInheritable Class ProcessMonitor

    ''' <summary>一行进程显示数据（已按分数降序、含迷你曲线历史）。</summary>
    Public NotInheritable Class Row
        Public Pid As Integer
        Public Name As String
        Public Cuda As Integer
        Public Copy As Integer
        Public Enc As Integer
        Public Dec As Integer
        Public MemMB As Double
        Public MemPercent As Integer
        Public Score As Long
        Public History As Integer()
    End Class

    Private NotInheritable Class State
        Public Pid As Integer
        Public Name As String
        Public LastActive As DateTime
        Public Cuda As Integer
        Public Copy As Integer
        Public Enc As Integer
        Public Dec As Integer
        Public MemMB As Double
        Public ReadOnly History As New List(Of Integer)()
    End Class

    Private ReadOnly _counter As GpuProcessCounter
    Private ReadOnly _totalBytes As ULong
    Private ReadOnly _historyLen As Integer
    Private ReadOnly _ttl As TimeSpan
    Private ReadOnly _states As New Dictionary(Of Integer, State)()

    Public Sub New(counter As GpuProcessCounter, totalBytes As ULong,
                   Optional historyLen As Integer = 20,
                   Optional ttl As TimeSpan? = Nothing)
        _counter = counter
        _totalBytes = totalBytes
        _historyLen = System.Math.Max(4, historyLen)
        _ttl = If(ttl.HasValue, ttl.Value, TimeSpan.FromMinutes(1))
    End Sub

    Public ReadOnly Property Ttl As TimeSpan
        Get
            Return _ttl
        End Get
    End Property

    ''' <summary>采集一次并合并进按 PID 的状态表；随后按 TTL 过期清理。</summary>
    Public Sub Update()
        Dim now = DateTime.Now
        Dim samples = _counter.Poll()

        Dim present As New HashSet(Of Integer)()

        ' 合并本轮出现的进程
        For Each p In samples
            present.Add(p.Pid)

            Dim st As State = Nothing
            If Not _states.TryGetValue(p.Pid, st) Then
                st = New State With {.Pid = p.Pid, .LastActive = now}
                _states(p.Pid) = st
            End If

            If Not String.IsNullOrEmpty(p.Name) Then st.Name = p.Name
            st.Cuda = p.CudaCorePercent
            st.Copy = p.CopyEnginePercent
            st.Enc = p.EncoderPercent
            st.Dec = p.DecoderPercent
            st.MemMB = p.DedicatedMB
            PushHistory(st.History, p.CudaCorePercent)

            If p.ActivityPercent > 0 Then st.LastActive = now
        Next

        ' 本轮未出现的进程：利用率清零、历史补 0（保留最后一次显存），但不更新活跃时间
        For Each kv In _states
            If Not present.Contains(kv.Key) Then
                kv.Value.Cuda = 0
                kv.Value.Copy = 0
                kv.Value.Enc = 0
                kv.Value.Dec = 0
                PushHistory(kv.Value.History, 0)
            End If
        Next

        ' 过期清理：超过 TTL 期间一直没有 GPU 活动的进程移除
        Dim expired As New List(Of Integer)()
        For Each kv In _states
            If now - kv.Value.LastActive > _ttl Then expired.Add(kv.Key)
        Next
        For Each dropPid In expired
            _states.Remove(dropPid)
        Next
    End Sub

    ''' <summary>按活动度分数降序返回当前应显示的进程行。</summary>
    Public Function Rows() As IReadOnlyList(Of Row)
        Dim list As New List(Of Row)()

        For Each kv In _states
            Dim st = kv.Value

            Dim memPct As Integer = 0
            If _totalBytes > 0UL Then
                Dim bytes = st.MemMB * 1024.0 * 1024.0
                memPct = CInt(System.Math.Round(bytes / _totalBytes * 100.0))
            End If

            Dim score = CLng(st.Cuda + 1) * (memPct + 1) * (st.Copy + 1) * (st.Enc + st.Dec + 1)

            list.Add(New Row With {
                .Pid = st.Pid,
                .Name = st.Name,
                .Cuda = st.Cuda,
                .Copy = st.Copy,
                .Enc = st.Enc,
                .Dec = st.Dec,
                .MemMB = st.MemMB,
                .MemPercent = memPct,
                .Score = score,
                .History = st.History.ToArray()
            })
        Next

        Return list _
            .OrderByDescending(Function(r) r.Score) _
            .ThenByDescending(Function(r) r.MemMB) _
            .ToArray()
    End Function

    Private Sub PushHistory(h As List(Of Integer), value As Integer)
        h.Add(value)
        If h.Count > _historyLen Then h.RemoveAt(0)
    End Sub
End Class
