#Region "Microsoft.VisualBasic::PdhPerformanceCounters, Data_science\Mathematica\Math\Randomizer\TrueRandom\PdhPerformanceCounters.vb"

    ' Author:
    ' 
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

    '   Class PdhPerformanceCounters
    ' 
    '     Properties: IsAvailable
    ' 
    '     Constructor: Sub New
    ' 
    '     Function: GetShared, SampleBytes, SampleDoubles, ToString
    ' 
    '     Sub: Dispose
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices
Imports System.Runtime.CompilerServices
Imports std = System.Math

''' <summary>
''' Windows 性能计数器封装（P/Invoke ``pdh.dll``）。
''' 
''' 通过 PDH 采集处理器时间、上下文切换、磁盘队列、内存等真实计数器，
''' 作为真随机数生成的物理熵源之一。该封装在 Windows 上可用，非 Windows
''' 平台 <see cref="IsAvailable"/> 返回 ``False`` 且所有采样返回空/降级值，
''' 因此调用方可以安全地回退到跨平台的环境噪声源。
''' 
''' PDH 调用仅在 Windows 运行时发生；声明本身无条件存在，但不会被非
''' Windows 平台加载（DllImport 延迟解析），从而保证跨平台编译与运行通过。
''' </summary>
Public NotInheritable Class PdhPerformanceCounters : Implements IDisposable

    ' PDH 状态码
    Private Const ERROR_SUCCESS As Integer = 0
    Private Const PDH_FMT_DOUBLE As UInteger = &H200UI

    <StructLayout(LayoutKind.Explicit)>
    Private Structure PDH_FMT_COUNTERVALUE
        <FieldOffset(0)> Public CStatus As UInteger
        <FieldOffset(8)> Public longValue As Integer
        <FieldOffset(8)> Public doubleValue As Double
        <FieldOffset(8)> Public largeValue As Long
    End Structure

    <DllImport("pdh.dll", CharSet:=CharSet.Unicode, ExactSpelling:=True, SetLastError:=True)>
    Private Shared Function PdhOpenQueryW(szDataSource As String, dwUserData As IntPtr, ByRef phQuery As IntPtr) As Integer
    End Function

    <DllImport("pdh.dll", CharSet:=CharSet.Unicode, ExactSpelling:=True, SetLastError:=True)>
    Private Shared Function PdhAddEnglishCounterW(hQuery As IntPtr, szFullCounterPath As String, dwUserData As IntPtr, ByRef phCounter As IntPtr) As Integer
    End Function

    <DllImport("pdh.dll", ExactSpelling:=True, SetLastError:=True)>
    Private Shared Function PdhCollectQueryData(hQuery As IntPtr) As Integer
    End Function

    <DllImport("pdh.dll", ExactSpelling:=True, SetLastError:=True)>
    Private Shared Function PdhGetFormattedCounterValue(hCounter As IntPtr, dwFormat As UInteger, ByRef lpdwType As UInteger, ByRef pValue As PDH_FMT_COUNTERVALUE) As Integer
    End Function

    <DllImport("pdh.dll", ExactSpelling:=True, SetLastError:=True)>
    Private Shared Function PdhCloseQuery(hQuery As IntPtr) As Integer
    End Function

    ''' <summary>
    ''' 在 Windows 上采集的英文计数器路径。速率型计数器（含 /sec）需要两次
    ''' CollectQueryData 才能得到有效差值，已在 <see cref="SampleDoubles"/> 中处理。
    ''' </summary>
    Private Shared ReadOnly CounterPaths As String() = {
        "\Processor(_Total)\% Processor Time",
        "\Processor(_Total)\% User Time",
        "\Processor(_Total)\% Privileged Time",
        "\Processor(_Total)\Interrupts/sec",
        "\System\Context Switches/sec",
        "\System\System Calls/sec",
        "\System\Processes",
        "\System\Threads",
        "\Memory\Available Bytes",
        "\Memory\Committed Bytes",
        "\PhysicalDisk(_Total)\Current Disk Queue Length",
        "\Processor(_Total)\DPCs/sec"
    }

    ''' <summary>
    ''' 当前运行时是否为 Windows 且 PDH 可用。
    ''' </summary>
    Public Shared ReadOnly Property IsAvailable As Boolean =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)

    Private ReadOnly _query As IntPtr
    Private ReadOnly _counters As New List(Of IntPtr)
    Private ReadOnly _sync As New Object
    Private _disposed As Boolean

    Private Sub New()
        ' 仅在 Windows 上构造；打开一个查询并添加全部计数器，
        ' 添加失败的计数器会被安全跳过（不同 Windows 版本可能缺少数路径）。
        Dim hQuery As IntPtr
        Dim openStatus As Integer = PdhOpenQueryW(Nothing, IntPtr.Zero, hQuery)

        If openStatus <> ERROR_SUCCESS Then
            Throw New System.InvalidOperationException("PdhOpenQueryW failed: " & openStatus)
        End If

        _query = hQuery

        For Each path As String In CounterPaths
            Dim hCounter As IntPtr
            Dim addStatus As Integer = PdhAddEnglishCounterW(_query, path, IntPtr.Zero, hCounter)

            If addStatus = ERROR_SUCCESS Then
                _counters.Add(hCounter)
            End If
        Next
    End Sub

    Private Shared _shared As PdhPerformanceCounters

    ''' <summary>
    ''' 获取（Windows 上的）共享 PDH 查询实例。非 Windows 返回 ``Nothing``。
    ''' </summary>
    Public Shared Function GetShared() As PdhPerformanceCounters
        If Not PdhPerformanceCounters.IsAvailable Then
            Return Nothing
        End If

        If _shared Is Nothing Then
            SyncLock GetType(PdhPerformanceCounters)
                If _shared Is Nothing Then
                    Try
                        _shared = New PdhPerformanceCounters
                    Catch ex As Exception
                        ' 任何 PDH 初始化失败都降级为不可用
                        _shared = Nothing
                    End Try
                End If
            End SyncLock
        End If

        Return _shared
    End Function

    ''' <summary>
    ''' 采集当前所有计数器的数值。失败的计数器以 ``Double.NaN`` 占位。
    ''' 速率型计数器已通过两次 CollectQueryData（间隔约 10ms）获取有效差值。
    ''' </summary>
    Public Function SampleDoubles() As Double()
        Dim values As Double()

        SyncLock _sync
            values = New Double(_counters.Count - 1) {}

            If _counters.Count = 0 Then
                Return values
            End If

            ' 第一次采集：为速率型计数器建立基线
            PdhCollectQueryData(_query)

            ' 短暂等待，使速率型计数器产生可测量的间隔
            Threading.Thread.Sleep(10)

            ' 第二次采集：得到有效速率值
            PdhCollectQueryData(_query)

            Dim type As UInteger = 0
            Dim cv As PDH_FMT_COUNTERVALUE

            For i As Integer = 0 To _counters.Count - 1
                Dim status As Integer = PdhGetFormattedCounterValue(_counters(i), PDH_FMT_DOUBLE, type, cv)

                If status = ERROR_SUCCESS AndAlso cv.CStatus = ERROR_SUCCESS Then
                    values(i) = cv.doubleValue
                Else
                    values(i) = Double.NaN
                End If
            Next
        End SyncLock

        Return values
    End Function

    ''' <summary>
    ''' 将所有计数器数值以 little-endian 双精度字节拼接为熵字节。
    ''' 非 Windows 或初始化失败时返回空数组（调用方应使用跨平台回退源）。
    ''' </summary>
    Public Function SampleBytes() As Byte()
        Dim doubles As Double() = SampleDoubles()
        Dim bytes As New List(Of Byte)(doubles.Length * 8)

        For Each d As Double In doubles
            ' NaN 也照常写入字节，其位模式本身具有噪声
            bytes.AddRange(BitConverter.GetBytes(d))
        Next

        Return bytes.ToArray
    End Function

    Public Overrides Function ToString() As String
        If _counters Is Nothing OrElse _counters.Count = 0 Then
            Return "PdhPerformanceCounters (no counters)"
        End If

        Return "PdhPerformanceCounters (" & _counters.Count & "/" & CounterPaths.Length & " counters)"
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then
            Return
        End If

        _disposed = True

        SyncLock _sync
            If _query <> IntPtr.Zero Then
                PdhCloseQuery(_query)
            End If
        End SyncLock
    End Sub
End Class
