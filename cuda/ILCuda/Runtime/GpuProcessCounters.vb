#Region "Microsoft.VisualBasic::gpu-process-counters, cuda\ILCuda\Runtime\GpuProcessCounters.vb"

    ' Author:
    '       xieguigang (xie.guigang@live.com)
    '
    ' Copyright (c) sciBASIC.NET Foundation
    '
    ' GNU GENERAL PUBLIC LICENSE (GPL3)

#End Region

' ------------------------------------------------------------------------
' 每进程 GPU 计数器托管封装（Windows PDH）
'
' 数据来源（Windows 性能计数器）：
'   \GPU Engine(*)\Utilization Percentage   —— 每进程各引擎利用率（%）
'       实例名：pid_<PID>_luid_<hi>_<lo>_phys_<N>_eng_<M>_engtype_<type>
'       type ∈ compute / 3d / copy / videoencode / videodecode / ...
'   \GPU Process Memory(*)\Dedicated Usage  —— 每进程专用显存（字节）
'       实例名：pid_<PID>_luid_<hi>_<lo>_phys_<N>
'
' 说明：
'   * CUDA 计算核心利用率 ≈ engtype_compute（部分负载可能记入 3d，二者分列展示）；
'   * 数据拷贝"带宽"Windows 无法按进程给出，以 engtype_copy 的利用率 % 作代理；
'   * 只产出纯数据（不依赖 Console），排版交由上层（GTop）；
'   * 缺计数器集 / 非 Windows / 无权限 / 进程已退出 均优雅降级，不抛异常到上层。
' ------------------------------------------------------------------------

Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.InteropServices

Namespace Runtime

    ''' <summary>
    ''' 单个进程的 GPU 使用快照（各字段均为"采样窗口内"的值）。
    ''' </summary>
    Public Structure GpuProcessSample
        ''' <summary>进程 ID</summary>
        Public Pid As Integer
        ''' <summary>进程名（无法解析时为空串）</summary>
        Public Name As String
        ''' <summary>计算引擎利用率（engtype_compute），即 CUDA 核心利用率代理（%）</summary>
        Public CudaComputePercent As Integer
        ''' <summary>3D 引擎利用率（engtype_3d）；部分计算负载可能记在此列（%）</summary>
        Public Render3DPercent As Integer
        ''' <summary>拷贝引擎利用率（engtype_copy，同类型多引擎取最大）（%）—— 数据拷贝带宽代理</summary>
        Public CopyEnginePercent As Integer
        ''' <summary>视频编码引擎利用率（%）</summary>
        Public EncoderPercent As Integer
        ''' <summary>视频解码引擎利用率（%）</summary>
        Public DecoderPercent As Integer
        ''' <summary>专用显存占用（字节）</summary>
        Public DedicatedMemoryBytes As ULong

        Public ReadOnly Property DedicatedMB As Double
            Get
                Return DedicatedMemoryBytes / 1024.0 / 1024.0
            End Get
        End Property

        ''' <summary>
        ''' CUDA 核心利用率代理 = 3D 引擎与 Compute 引擎利用率的较大者。
        ''' 说明：Windows（WDDM）下 CUDA 计算通常记入 <c>3d</c> 引擎，而部分场景记入
        ''' <c>compute</c> 引擎，故取两者最大以覆盖两种映射。
        ''' </summary>
        Public ReadOnly Property CudaCorePercent As Integer
            Get
                Return System.Math.Max(CudaComputePercent, Render3DPercent)
            End Get
        End Property

        ''' <summary>活动度 = 各引擎利用率的最大值，用于排序与过滤</summary>
        Public ReadOnly Property ActivityPercent As Integer
            Get
                Dim m = System.Math.Max(CudaComputePercent, Render3DPercent)
                m = System.Math.Max(m, CopyEnginePercent)
                m = System.Math.Max(m, EncoderPercent)
                m = System.Math.Max(m, DecoderPercent)
                Return m
            End Get
        End Property
    End Structure

    ''' <summary>
    ''' 每进程 GPU 计数器采集器：持有两个 PDH 查询（引擎利用率、进程显存）。
    ''' 线程不安全，请单线程使用；实现 IDisposable 以释放 PDH 句柄。
    ''' </summary>
    Public NotInheritable Class GpuProcessCounter
        Implements IDisposable

        Private Const CounterEngines As String = "\GPU Engine(*)\Utilization Percentage"
        Private Const CounterMemory As String = "\GPU Process Memory(*)\Dedicated Usage"

        Private _queryEngines As IntPtr = IntPtr.Zero
        Private _counterEngines As IntPtr = IntPtr.Zero
        Private _queryMemory As IntPtr = IntPtr.Zero
        Private _counterMemory As IntPtr = IntPtr.Zero
        Private _primed As Boolean = False
        Private _disposed As Boolean = False

        ''' <summary>
        ''' 探测本机是否支持进程级 GPU 计数器（存在 GPU Engine 计数器集且可添加）。
        ''' 非 Windows / 无计数器集 / 无权限时返回 False，绝不抛异常。
        ''' </summary>
        Public Shared Function IsAvailable() As Boolean
            Dim q As IntPtr = IntPtr.Zero
            Try
                If PdhApi.PdhOpenQueryW(Nothing, IntPtr.Zero, q) <> PdhApi.PDH_SUCCESS Then
                    Return False
                End If

                Dim c As IntPtr = IntPtr.Zero
                Return PdhApi.PdhAddEnglishCounterW(q, CounterEngines, IntPtr.Zero, c) = PdhApi.PDH_SUCCESS
            Catch
                Return False
            Finally
                If q <> IntPtr.Zero Then
                    Try : PdhApi.PdhCloseQuery(q) : Catch : End Try
                End If
            End Try
        End Function

        Public Sub New()
            PdhApi.Check(PdhApi.PdhOpenQueryW(Nothing, IntPtr.Zero, _queryEngines), "PdhOpenQueryW(GPU Engine)")
            PdhApi.Check(PdhApi.PdhOpenQueryW(Nothing, IntPtr.Zero, _queryMemory), "PdhOpenQueryW(GPU Process Memory)")

            PdhApi.Check(PdhApi.PdhAddEnglishCounterW(_queryEngines, CounterEngines, IntPtr.Zero, _counterEngines), "PdhAddEnglishCounterW(GPU Engine)")
            PdhApi.Check(PdhApi.PdhAddEnglishCounterW(_queryMemory, CounterMemory, IntPtr.Zero, _counterMemory), "PdhAddEnglishCounterW(GPU Process Memory)")

            ' 预热：速率型计数器需要至少两次采集才有值
            PdhApi.PdhCollectQueryData(_queryEngines)
            PdhApi.PdhCollectQueryData(_queryMemory)
            _primed = True
        End Sub

        ''' <summary>
        ''' 采集一次全部进程的 GPU 计数，按 PID 聚合并返回（按活动度、显存降序）。
        ''' 仅保留"存在计算引擎上下文 或 存在任何引擎活动"的进程。
        ''' </summary>
        Public Function Poll() As IReadOnlyList(Of GpuProcessSample)
            If _disposed Then Throw New ObjectDisposedException(NameOf(GpuProcessCounter))

            If Not _primed Then
                PdhApi.PdhCollectQueryData(_queryEngines)
                PdhApi.PdhCollectQueryData(_queryMemory)
                _primed = True
            End If

            PdhApi.PdhCollectQueryData(_queryEngines)
            PdhApi.PdhCollectQueryData(_queryMemory)

            Dim acc As New Dictionary(Of Integer, Accumulator)()

            ' 引擎利用率（同类型引擎取最大）
            For Each item In ReadFormattedArray(_counterEngines)
                Dim pid = ParsePid(item.Name)
                If pid < 0 Then Continue For

                Dim a = GetAcc(acc, pid)
                Dim pct = CInt(System.Math.Round(item.Value))

                Select Case ParseEngType(item.Name)
                    Case "compute"
                        a.HasCompute = True
                        a.Compute = System.Math.Max(a.Compute, pct)
                    Case "3d"
                        a.Render3D = System.Math.Max(a.Render3D, pct)
                    Case "copy"
                        a.Copy = System.Math.Max(a.Copy, pct)
                    Case "videoencode"
                        a.Encoder = System.Math.Max(a.Encoder, pct)
                    Case "videodecode"
                        a.Decoder = System.Math.Max(a.Decoder, pct)
                End Select
            Next

            ' 每进程专用显存（跨适配器求和）
            For Each item In ReadFormattedArray(_counterMemory)
                Dim pid = ParsePid(item.Name)
                If pid < 0 Then Continue For

                Dim a = GetAcc(acc, pid)
                If item.Value > 0 Then
                    a.MemoryBytes += CULng(System.Math.Round(item.Value))
                End If
            Next

            Dim list As New List(Of GpuProcessSample)()
            For Each kv In acc
                Dim a = kv.Value
                Dim activity = System.Math.Max(System.Math.Max(a.Compute, a.Render3D), System.Math.Max(a.Copy, System.Math.Max(a.Encoder, a.Decoder)))

                If Not a.HasCompute AndAlso activity <= 0 Then Continue For

                list.Add(New GpuProcessSample With {
                    .Pid = kv.Key,
                    .Name = ResolveName(kv.Key),
                    .CudaComputePercent = a.Compute,
                    .Render3DPercent = a.Render3D,
                    .CopyEnginePercent = a.Copy,
                    .EncoderPercent = a.Encoder,
                    .DecoderPercent = a.Decoder,
                    .DedicatedMemoryBytes = a.MemoryBytes
                })
            Next

            Return list _
                .OrderByDescending(Function(x) x.ActivityPercent) _
                .ThenByDescending(Function(x) x.DedicatedMemoryBytes) _
                .ToArray()
        End Function

        ' ------------------------------------------------------------------
        ' 内部实现
        ' ------------------------------------------------------------------
        Private Class Accumulator
            Public HasCompute As Boolean
            Public Compute As Integer
            Public Render3D As Integer
            Public Copy As Integer
            Public Encoder As Integer
            Public Decoder As Integer
            Public MemoryBytes As ULong
        End Class

        Private Class PdhItem
            Public Name As String
            Public Value As Double
        End Class

        Private Shared Function GetAcc(acc As Dictionary(Of Integer, Accumulator), pid As Integer) As Accumulator
            Dim a As Accumulator = Nothing
            If Not acc.TryGetValue(pid, a) Then
                a = New Accumulator()
                acc(pid) = a
            End If
            Return a
        End Function

        ''' <summary>
        ''' 读取一个通配符计数器的全部实例（先取所需缓冲区大小，再分配并二次读取）。
        ''' 缓冲区不足（PDH_MORE_DATA）或无数据时返回空表，绝不抛异常。
        ''' </summary>
        Private Shared Function ReadFormattedArray(counter As IntPtr) As List(Of PdhItem)
            Dim result As New List(Of PdhItem)()
            If counter = IntPtr.Zero Then Return result

            Dim bufSize As UInteger = 0
            Dim itemCount As UInteger = 0
            Dim status = PdhApi.PdhGetFormattedCounterArrayW(counter, PdhApi.PDH_FMT_DOUBLE, bufSize, itemCount, IntPtr.Zero)

            If bufSize = 0UI OrElse itemCount = 0UI Then
                Return result
            End If

            Dim buffer = Marshal.AllocHGlobal(CInt(bufSize))
            Try
                status = PdhApi.PdhGetFormattedCounterArrayW(counter, PdhApi.PDH_FMT_DOUBLE, bufSize, itemCount, buffer)
                If status <> PdhApi.PDH_SUCCESS Then
                    Return result
                End If

                Dim itemSize = Marshal.SizeOf(Of PdhApi.PDH_FMT_COUNTERVALUE_ITEM)()
                For i As Integer = 0 To CInt(itemCount) - 1
                    Dim itemPtr = IntPtr.Add(buffer, i * itemSize)
                    Dim item = Marshal.PtrToStructure(Of PdhApi.PDH_FMT_COUNTERVALUE_ITEM)(itemPtr)

                    Dim value As Double = 0
                    If PdhApi.IsValidValue(item.FmtValue.CStatus) Then
                        value = item.FmtValue.DoubleValue
                    End If

                    result.Add(New PdhItem With {
                        .Name = Marshal.PtrToStringUni(item.SzName),
                        .Value = value
                    })
                Next
            Finally
                Marshal.FreeHGlobal(buffer)
            End Try

            Return result
        End Function

        ''' <summary>从实例名中解析 PID（pid_&lt;PID&gt;_...）；失败返回 -1</summary>
        Private Shared Function ParsePid(instanceName As String) As Integer
            If String.IsNullOrEmpty(instanceName) Then Return -1
            If Not instanceName.StartsWith("pid_", StringComparison.OrdinalIgnoreCase) Then Return -1

            Dim rest = instanceName.Substring(4)
            Dim idx = rest.IndexOf("_"c)
            Dim pidStr = If(idx >= 0, rest.Substring(0, idx), rest)

            Dim pid As Integer
            If Integer.TryParse(pidStr, pid) Then Return pid
            Return -1
        End Function

        ''' <summary>从实例名中解析引擎类型（engtype_ 之后的部分，保留其内部下划线，如 ofa_0）</summary>
        Private Shared Function ParseEngType(instanceName As String) As String
            Const key As String = "engtype_"

            If String.IsNullOrEmpty(instanceName) Then Return String.Empty
            Dim idx = instanceName.IndexOf(key, StringComparison.OrdinalIgnoreCase)
            If idx < 0 Then Return String.Empty

            Return instanceName.Substring(idx + key.Length).ToLowerInvariant()
        End Function

        ''' <summary>用 System.Diagnostics 解析进程名；进程已退出/无权限时返回空串</summary>
        Private Shared Function ResolveName(pid As Integer) As String
            Try
                Using p = System.Diagnostics.Process.GetProcessById(pid)
                    Return p.ProcessName
                End Using
            Catch
                Return String.Empty
            End Try
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True

            If _queryEngines <> IntPtr.Zero Then
                Try : PdhApi.PdhCloseQuery(_queryEngines) : Catch : End Try
                _queryEngines = IntPtr.Zero
                _counterEngines = IntPtr.Zero
            End If

            If _queryMemory <> IntPtr.Zero Then
                Try : PdhApi.PdhCloseQuery(_queryMemory) : Catch : End Try
                _queryMemory = IntPtr.Zero
                _counterMemory = IntPtr.Zero
            End If
        End Sub
    End Class
End Namespace
