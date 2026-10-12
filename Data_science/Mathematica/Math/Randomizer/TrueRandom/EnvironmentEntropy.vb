#Region "Microsoft.VisualBasic::EnvironmentEntropy, Data_science\Mathematica\Math\Randomizer\TrueRandom\EnvironmentEntropy.vb"

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

    '   Class EnvironmentEntropy
    ' 
    '     Constructor: Sub New
    ' 
    '     Function: (+2 Overloads) GetBytes, GetSeed, Sample
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports std = System.Math

''' <summary>
''' 纯环境噪声熵收集器（慢、高熵）。
''' 
''' 通过采样操作系统运行时的各类不可预测量——高精度计时抖动、系统时钟、
''' 进程 CPU/内存/线程/句柄统计、垃圾回收计数与堆大小、线程池积压、GUID、
''' 以及 Windows 上的 PDH 性能计数器——将它们拼接后由 <see cref="SHA256"/>
''' 压缩为 32 字节熵块。熵块以"上一次输出 + 新采样"的方式演化，可累计产出
''' 任意长度字节。
''' 
''' 本收集器<b>不调用任何加密随机数发生器</b>（如
''' <see cref="RandomNumberGenerator"/>），全部熵取自实时环境噪声；因此适合
''' 少量高熵种子、密钥盐等场景，不适合大规模批量产出。
''' </summary>
Public Class EnvironmentEntropy

    ''' <summary>
    ''' 用于演化熵块的内部状态；初始为全零，首个块由首次采样决定。
    ''' </summary>
    Private _state As Byte()

    Private ReadOnly _sync As New Object

    ''' <summary>
    ''' 构造一个独立的熵收集器实例（各自维持独立演化状态）。
    ''' </summary>
    Public Sub New()
        _state = New Byte(31) {}
    End Sub

    ''' <summary>
    ''' 采集一帧原始环境噪声，拼接为字节流。
    ''' </summary>
    Private Function SampleRaw() As Byte()
        Dim buf As New List(Of Byte)(512)
        Dim proc As Process = Process.GetCurrentProcess()

        ' —— 高精度计时抖动：连续读取 Stopwatch 时间戳，差值即真实噪声 ——
        Dim t0 As Long = Stopwatch.GetTimestamp()
        Dim t1 As Long = Stopwatch.GetTimestamp()
        Dim t2 As Long = Stopwatch.GetTimestamp()
        buf.AddRange(BitConverter.GetBytes(t0))
        buf.AddRange(BitConverter.GetBytes(t1 Xor t0))
        buf.AddRange(BitConverter.GetBytes(t2 - t1))

        ' —— 系统时钟（亚滴答精度由 Stopwatch 提供）——
        Dim now As DateTime = DateTime.UtcNow
        buf.AddRange(BitConverter.GetBytes(now.Ticks))
        buf.AddRange(BitConverter.GetBytes(now.Ticks Xor t2))

        ' —— 低精度滴答计数 ——
        buf.AddRange(BitConverter.GetBytes(Environment.TickCount64))
        buf.AddRange(BitConverter.GetBytes(Environment.TickCount))

        ' —— 进程级运行噪声 ——
        Try
            proc.Refresh()
            buf.AddRange(BitConverter.GetBytes(proc.TotalProcessorTime.Ticks))
            buf.AddRange(BitConverter.GetBytes(proc.UserProcessorTime.Ticks))
            buf.AddRange(BitConverter.GetBytes(proc.PrivilegedProcessorTime.Ticks))
            buf.AddRange(BitConverter.GetBytes(proc.WorkingSet64))
            buf.AddRange(BitConverter.GetBytes(proc.PrivateMemorySize64))
            buf.AddRange(BitConverter.GetBytes(proc.VirtualMemorySize64))
            buf.AddRange(BitConverter.GetBytes(proc.Threads.Count))
            buf.AddRange(BitConverter.GetBytes(proc.HandleCount))
            buf.AddRange(BitConverter.GetBytes(proc.Id))
        Catch
            ' 某些平台无法读取进程统计，使用时钟噪声替代
            buf.AddRange(BitConverter.GetBytes(Stopwatch.GetTimestamp()))
        End Try

        ' —— 垃圾回收噪声 ——
        For gen As Integer = 0 To GC.MaxGeneration
            buf.AddRange(BitConverter.GetBytes(GC.CollectionCount(gen)))
        Next
        buf.AddRange(BitConverter.GetBytes(GC.GetTotalMemory(False)))

        ' —— 线程池积压 ——
        Try
            buf.AddRange(BitConverter.GetBytes(ThreadPool.PendingWorkItemCount))
            buf.AddRange(BitConverter.GetBytes(ThreadPool.CompletedWorkItemCount))
        Catch
            buf.AddRange(BitConverter.GetBytes(Stopwatch.GetTimestamp()))
        End Try

        ' —— 线程与任务调度噪声 ——
        buf.AddRange(BitConverter.GetBytes(Threading.Thread.CurrentThread.ManagedThreadId))
        buf.AddRange(BitConverter.GetBytes(TaskScheduler.Current.GetHashCode()))

        ' —— GUID 创建时戳（底层含随机/时序分量）——
        buf.AddRange(Guid.NewGuid().ToByteArray())

        ' —— Windows 上的 PDH 真实性能计数器 ——
        If RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then
            Dim pdh As PdhPerformanceCounters = PdhPerformanceCounters.GetShared()

            If pdh IsNot Nothing Then
                Dim pdhBytes As Byte() = pdh.SampleBytes()

                If pdhBytes IsNot Nothing AndAlso pdhBytes.Length > 0 Then
                    buf.AddRange(pdhBytes)
                End If
            End If
        End If

        ' —— 递增本地计数器，保证即使其它源瞬时稳定也有演化 ——
        Static localCounter As Long
        localCounter = localCounter + 1
        buf.AddRange(BitConverter.GetBytes(localCounter))
        buf.AddRange(Encoding.UTF8.GetBytes(Environment.MachineName & "|" & Environment.UserName & "|" & Environment.ProcessPath))

        Return buf.ToArray
    End Function

    ''' <summary>
    ''' 产出一块 32 字节熵：输入 = 上一块状态 ⊕ 新采样的原始噪声，SHA256 压缩。
    ''' 输出同时写回内部状态用于下一次演化。
    ''' </summary>
    Public Function Sample() As Byte()
        SyncLock _sync
            Dim raw As Byte() = SampleRaw()
            Dim input(raw.Length + _state.Length - 1) As Byte
            Array.Copy(_state, 0, input, 0, _state.Length)
            Array.Copy(raw, 0, input, _state.Length, raw.Length)

            Using sha As SHA256 = SHA256.Create()
                Dim block As Byte() = sha.ComputeHash(input)
                _state = block
                Return block
            End Using
        End SyncLock
    End Function

    ''' <summary>
    ''' 累计产出 <paramref name="count"/> 字节的环境熵。通过反复采样演化实现任意长度。
    ''' </summary>
    ''' <param name="count">所需熵字节数（应较小，纯噪声采集较慢）。</param>
    Public Function GetBytes(count As Integer) As Byte()
        If count <= 0 Then
            Return New Byte(-1) {}
        End If

        Dim out As New List(Of Byte)(count)
        Dim collected As Integer = 0

        Do
            Dim block As Byte() = Sample()
            Dim take As Integer = std.Min(block.Length, count - collected)
            out.AddRange(block.AsSpan(0, take).ToArray)
            collected += take
        Loop While collected < count

        Return out.ToArray
    End Function

    ''' <summary>
    ''' 产出一个 32 字节（256-bit）高熵种子，适合作为密钥盐或 PRNG 种子。
    ''' </summary>
    Public Function GetSeed() As Byte()
        Return Sample()
    End Function
End Class
