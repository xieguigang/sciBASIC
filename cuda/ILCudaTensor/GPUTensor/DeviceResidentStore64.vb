#Region "Microsoft.VisualBasic::f7a2c9b41d3e5c8a0f6b2d91c4e7a3b2, cuda\ILCudaTensor\GPUTensor\DeviceResidentStore64.vb"

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

    '   Total Lines: 0
    '    Code Lines: 0 (0.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 0 (0.00%)
    '     File Size: 0.00 KB


    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' DeviceResidentStore64 —— 以主机 Double() 数组引用为键的“双精度”设备常驻缓冲注册表
'
' 为什么需要它（而不是复用 DeviceResidentStore）：
'   DeviceResidentStore 的缓冲一律是单精度（DeviceBuffer(Of Single)），这是为稠密训练
'   量身定的取舍 —— 消费级显卡的 FP64 吞吐只有 FP32 的 1/64，而训练里矩阵乘占 95% 以上
'   的浮点运算。
'
'   但脉冲神经网络的状态是**逐时间步累积的递归量**：膜电位 H 每步都要参与泄漏积分，
'   阈值判定又是硬阶跃函数 Θ(u − U_thr)。单精度舍入会让个别神经元的膜电位恰好跨过
'   阈值，于是整个仿真窗内多发或少发一次脉冲 —— 这类误差不是"数值噪声"，而是
'   计数上的整数差异，会让「CPU / GPU 双精度对拍」这类验收无法通过。
'
'   因此这里提供第二条通道：状态张量以 Double 留在显存，既保留"零往返"，
'   又不引入任何精度损失。两条通道互不干扰（FP32 通道保持既有行为，供 AdamW /
'   FP32 GEMM 使用）。
'
' 与 FP32 通道的差异：
'   * 缓冲类型为 DeviceBuffer(Of Double)，上传 / 回读都是位模式直搬，无类型转换；
'   * 其余语义（引用身份为键、Pin 幂等、无 LRU、显式 Unpin 释放）完全一致。
' ---------------------------------------------------------------------------

Imports System.Runtime.CompilerServices
Imports ILCudaRuntime = Microsoft.VisualBasic.Computing.ILCuda.Runtime

Namespace GPUTensor

    ''' <summary>
    ''' 以主机 <c>Double()</c> 数组引用为键的<b>双精度</b>设备常驻缓冲注册表。
    ''' </summary>
    ''' <remarks>
    ''' 用于脉冲网络的递归状态（膜电位 / 脉冲 / 计数累加器）等高精度长期张量：
    ''' 它们既不能承受单精度舍入（会改变阈值判定），又必须避免每步上传下载
    ''' （<c>[1, N]</c> 张量在十万神经元规模下即 1.1 MB/次）。
    ''' </remarks>
    Public NotInheritable Class DeviceResidentStore64
        Implements IDisposable

        ''' <summary>单个常驻条目。</summary>
        Private NotInheritable Class Entry

            Public ReadOnly Buffer As ILCudaRuntime.DeviceBuffer(Of Double)
            ''' <summary>钉住时刻的数据版本号，仅用于诊断（不参与失效判定）</summary>
            Public ReadOnly Version As Long
            Public ReadOnly Label As String

            Public Sub New(buffer As ILCudaRuntime.DeviceBuffer(Of Double), version As Long, label As String)
                Me.Buffer = buffer
                Me.Version = version
                Me.Label = label
            End Sub

            ''' <summary>异常路径兜底（正常情况下都会走显式 Dispose）</summary>
            Protected Overrides Sub Finalize()
                Try
                    Buffer.Dispose()
                Catch
                End Try
            End Sub

        End Class

        ''' <summary>按数组引用身份比较的相等性比较器（与 DeviceCache 保持同一约定）</summary>
        Private NotInheritable Class HostComparer
            Implements IEqualityComparer(Of Double())

            Public Overloads Function Equals(a As Double(), b As Double()) As Boolean Implements IEqualityComparer(Of Double()).Equals
                Return ReferenceEquals(a, b)
            End Function

            Public Overloads Function GetHashCode(a As Double()) As Integer Implements IEqualityComparer(Of Double()).GetHashCode
                If a Is Nothing Then Return 0
                Return RuntimeHelpers.GetHashCode(a)
            End Function

        End Class

        Private ReadOnly _map As New Dictionary(Of Double(), Entry)(New HostComparer())
        Private ReadOnly _sync As New Object()
        Private _bytes As Long
        Private _disposed As Boolean

        ''' <summary>常驻缓冲占用的总字节数。</summary>
        Public ReadOnly Property TotalBytes As Long
            Get
                SyncLock _sync
                    Return _bytes
                End SyncLock
            End Get
        End Property

        ''' <summary>常驻条目的个数。</summary>
        Public ReadOnly Property Count As Integer
            Get
                SyncLock _sync
                    Return _map.Count
                End SyncLock
            End Get
        End Property

        ''' <summary>主机数组是否已被钉住。</summary>
        Public Function IsPinned(host As Double()) As Boolean
            If host Is Nothing Then Return False

            SyncLock _sync
                Return _map.ContainsKey(host)
            End SyncLock
        End Function

        ''' <summary>
        ''' 取已钉住的缓冲；未钉住时返回 <c>Nothing</c>（而不是抛异常）。
        ''' </summary>
        Public Function TryGet(host As Double(), ByRef buffer As ILCudaRuntime.DeviceBuffer(Of Double)) As Boolean
            buffer = Nothing
            If host Is Nothing Then Return False

            SyncLock _sync
                Dim entry As Entry = Nothing

                If _map.TryGetValue(host, entry) Then
                    buffer = entry.Buffer
                    Return True
                End If

                Return False
            End SyncLock
        End Function

        ''' <summary>
        ''' 把主机数组钉成双精度常驻显存缓冲（已钉住则直接返回现有缓冲）。
        ''' </summary>
        ''' <param name="engine">用于诊断显存余量的引擎</param>
        ''' <param name="host">主机主副本</param>
        ''' <param name="label">诊断用标签，例如 <c>lif.H</c></param>
        ''' <param name="zeroFill">是否用 0 初始化（计数累加器用 <c>True</c>）</param>
        Public Function Pin(engine As ILCudaRuntime.CudaEngine, host As Double(), label As String,
                            Optional zeroFill As Boolean = False) As ILCudaRuntime.DeviceBuffer(Of Double)
            If host Is Nothing Then Throw New ArgumentNullException(NameOf(host))
            If host.Length = 0 Then Throw New ArgumentException("不能钉住空数组", NameOf(host))

            Dim existing As ILCudaRuntime.DeviceBuffer(Of Double) = Nothing
            If TryGet(host, existing) Then Return existing

            If _disposed Then Throw New ObjectDisposedException(NameOf(DeviceResidentStore64))

            Dim needBytes As Long = CLng(host.Length) * 8L

            Call EnsureCapacity(engine, needBytes, label)

            Dim buffer As New ILCudaRuntime.DeviceBuffer(Of Double)(host.Length)

            If zeroFill Then
                buffer.Fill(0.0)
            Else
                buffer.Write(host)
            End If

            SyncLock _sync
                ' 并发重复钉住时以先注册者为准，后到的缓冲立即释放
                Dim winner As Entry = Nothing

                If _map.TryGetValue(host, winner) Then
                    buffer.Dispose()
                    Return winner.Buffer
                End If

                _map(host) = New Entry(buffer, 0L, label)
                _bytes += needBytes
            End SyncLock

            Return buffer
        End Function

        ''' <summary>解除钉住并立即释放显存。</summary>
        Public Function Unpin(host As Double()) As Boolean
            If host Is Nothing Then Return False

            SyncLock _sync
                Dim entry As Entry = Nothing

                If Not _map.TryGetValue(host, entry) Then Return False

                _map.Remove(host)
                _bytes -= CLng(entry.Buffer.Count) * CLng(entry.Buffer.ElementSize)
                entry.Buffer.Dispose()
            End SyncLock

            Return True
        End Function

        ''' <summary>
        ''' 把常驻缓冲的内容回读到主机（设备为主副本时用于检查点 / 统计同步）。
        ''' </summary>
        ''' <remarks>未钉住时直接返回 <c>Nothing</c>，由调用方决定是否视为错误。</remarks>
        Public Function Download(host As Double()) As Double()
            Dim buffer As ILCudaRuntime.DeviceBuffer(Of Double) = Nothing

            If Not TryGet(host, buffer) Then Return Nothing

            Return buffer.Read()
        End Function

        ''' <summary>把主机内容重新上传到常驻缓冲。</summary>
        Public Function Upload(host As Double()) As Boolean
            Dim buffer As ILCudaRuntime.DeviceBuffer(Of Double) = Nothing

            If Not TryGet(host, buffer) Then Return False
            If buffer.Count <> host.Length Then Return False

            buffer.Write(host)

            Return True
        End Function

        ''' <summary>各常驻缓冲的 <c>(标签, 元素数, 字节数)</c> 快照，用于诊断输出。</summary>
        Public Function Describe() As (Label As String, Elements As Integer, Bytes As Long)()
            SyncLock _sync
                Return _map.Values _
                    .Select(Function(e) (e.Label, e.Buffer.Count,
                                         CLng(e.Buffer.Count) * CLng(e.Buffer.ElementSize))) _
                    .OrderByDescending(Function(t) t.Item3) _
                    .ToArray()
            End SyncLock
        End Function

        ''' <summary>释放全部常驻缓冲。</summary>
        Public Sub Clear()
            SyncLock _sync
                For Each entry In _map.Values
                    entry.Buffer.Dispose()
                Next

                _map.Clear()
                _bytes = 0
            End SyncLock
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return

            _disposed = True
            Call Clear()
        End Sub

        ''' <summary>
        ''' 分配前预检显存余量，不足时抛出带具体数字的可读异常。
        ''' </summary>
        Private Shared Sub EnsureCapacity(engine As ILCudaRuntime.CudaEngine, needBytes As Long, label As String)
            If engine Is Nothing Then Return

            Dim info As ILCudaRuntime.MemoryInfo = Nothing

            ' 查询失败（例如上下文尚未就绪）时不阻断分配，交给 CUDA 自己报错。
            ' 注意 MemoryInfo 是 Structure，不能用 Is Nothing 判断，只看 TryQuery 的返回值。
            If Not ILCudaRuntime.CudaMemory.TryQuery(info) Then Return

            ' 留 5% 余量：仿真过程中还会有算子输出缓冲的临时分配
            Dim margin As Long = CLng(info.FreeBytes / 20UL)

            If needBytes + margin > CLng(info.FreeBytes) Then
                Throw New OutOfMemoryException(
                    $"显存不足以双精度常驻 {label}：需要 {FormatMb(needBytes)}，" &
                    $"当前可用 {FormatMb(CLng(info.FreeBytes))}（总量 {FormatMb(CLng(info.TotalBytes))}）。")
            End If
        End Sub

        Private Shared Function FormatMb(bytes As Long) As String
            Return $"{bytes / 1024.0 / 1024.0:N1} MB"
        End Function

    End Class

End Namespace
