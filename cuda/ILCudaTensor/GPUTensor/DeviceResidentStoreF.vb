' ---------------------------------------------------------------------------
' 单精度栈的设备常驻缓冲注册表
'
' 为什么单精度栈必须自己有一份：
'   DeviceResidentStore 的键是 Double()，而 TensorF.Data 本身就是 Single()；
'   把 Single 主机的托管引用强塞进以 Double() 为键的表里是不成立的（类型不同、判等
'   又是引用身份），所以这里是一份以 Single() 为键的同构实现。
'
' 它解决的是混合精度改造里最要命的一件事：
'   **没有常驻，单层 tensorF 端到端反而会更慢。**
'   未钉住的张量每个算子都要"上传输入 + 回读输出"（DeviceBuffer.Read 是一次
'   完整的 D2H），一条 N 个算子的链就是 N 次往返，搬数据的代价远大于计算本身。
'   钉住之后设备成为主副本，中间结果不再往返。
'
' 使用约定（与 PinDevice 的文档一致，务必遵守）：
'   * 钉住后**设备是主副本**，主机 Data 会逐渐陈旧；
'   * 任何要读主机内容的场合（检查点落盘、主机侧统计）必须先 SyncFromDevice；
'   * 凡是要在主机循环里读写该数组的算子，必须先 Unpin，否则会把陈旧数据写回设备。
'
' 释放语义与 DeviceCache / DeviceResidentStore 一致：
'   ILCuda 的 DeviceMemory 没有终结器，依赖 GC 会让 cuMemAlloc 出来的显存永不归还，
'   因此 Unpin / Clear / Dispose 立即释放，Finalize 只作异常路径兜底。
' ---------------------------------------------------------------------------

Imports System.Runtime.CompilerServices
Imports ILCudaRuntime = Microsoft.VisualBasic.Computing.ILCuda.Runtime
Imports std = System.Math

Namespace GPUTensor

    ''' <summary>
    ''' 以主机 <c>Single()</c> 数组引用为键的设备常驻缓冲注册表（FP32 通道）。
    ''' </summary>
    Public NotInheritable Class DeviceResidentStoreF
        Implements IDisposable

        Private NotInheritable Class Entry

            Public ReadOnly Buffer As ILCudaRuntime.DeviceBuffer(Of Single)
            ''' <summary>钉住时刻的数据版本号，仅用于诊断（不参与失效判定）</summary>
            Public ReadOnly Version As Long
            Public ReadOnly Label As String
            ''' <summary>缓冲的元素个数（DeviceBuffer 本身不保留长度语义，这里显式记下以便统计字节数）</summary>
            Public ReadOnly Count As Integer

            Public Sub New(buffer As ILCudaRuntime.DeviceBuffer(Of Single), count As Integer,
                           version As Long, label As String)
                Me.Buffer = buffer
                Me.Count = count
                Me.Version = version
                Me.Label = label
            End Sub

            Protected Overrides Sub Finalize()
                Try
                    Buffer.Dispose()
                Catch
                End Try
            End Sub

        End Class

        ''' <summary>按数组引用身份比较的相等性比较器（与 DeviceCache 保持同一约定）</summary>
        Private NotInheritable Class HostComparer
            Implements IEqualityComparer(Of Single())

            Public Overloads Function Equals(a As Single(), b As Single()) As Boolean Implements IEqualityComparer(Of Single()).Equals
                Return ReferenceEquals(a, b)
            End Function

            Public Overloads Function GetHashCode(a As Single()) As Integer Implements IEqualityComparer(Of Single()).GetHashCode
                If a Is Nothing Then Return 0
                Return RuntimeHelpers.GetHashCode(a)
            End Function

        End Class

        Private ReadOnly _map As New Dictionary(Of Single(), Entry)(New HostComparer())
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
        Public Function IsPinned(host As Single()) As Boolean
            If host Is Nothing Then Return False

            SyncLock _sync
                Return _map.ContainsKey(host)
            End SyncLock
        End Function

        ''' <summary>取已钉住的缓冲；未钉住时返回 <c>Nothing</c> 而不是抛异常。</summary>
        Public Function TryGet(host As Single(), ByRef buffer As ILCudaRuntime.DeviceBuffer(Of Single)) As Boolean
            buffer = Nothing
            If host Is Nothing Then Return False

            SyncLock _sync
                Dim entry As Entry = Nothing

                If _map.TryGetValue(host, entry) Then
                    buffer = entry.Buffer
                    Return True
                End If
            End SyncLock

            Return False
        End Function

        ''' <summary>取已钉住缓冲的诊断标签；未钉住时返回 <c>Nothing</c>。</summary>
        Public Function LabelOf(host As Single()) As String
            If host Is Nothing Then Return Nothing

            SyncLock _sync
                Dim entry As Entry = Nothing

                If _map.TryGetValue(host, entry) Then
                    Return entry.Label
                End If
            End SyncLock

            Return Nothing
        End Function

        ''' <summary>
        ''' 把主机数组钉成设备常驻缓冲。
        ''' </summary>
        ''' <param name="host">以它为键；同时也作为首次上传的数据源</param>
        ''' <param name="label">诊断用标签</param>
        ''' <param name="zeroFill">
        ''' 是否用 0 初始化。梯度累加器与优化器状态必须用 <c>True</c> ——
        ''' 否则会把上一次的残留值当成初始动量。
        ''' </param>
        ''' <returns>已钉住（重复钉住同一份数组）时返回 <c>False</c>，此时不改动既有条目</returns>
        ''' <remarks>
        ''' 与 PinDevice64 的差别：这里的缓冲类型就是 <c>Single</c>，不做任何精度转换，
        ''' 也不存在"主机双精度主副本"——<c>TensorF.Data</c> 本来就是单精度的。
        ''' </remarks>
        Public Function Pin(host As Single(), version As Long, label As String, zeroFill As Boolean) As Boolean
            If host Is Nothing Then Throw New ArgumentNullException(NameOf(host))
            If host.Length = 0 Then Throw New ArgumentException("主机数组不能为空", NameOf(host))

            SyncLock _sync
                If _disposed Then Throw New ObjectDisposedException(NameOf(DeviceResidentStoreF))

                If _map.ContainsKey(host) Then Return False

                Dim buf As New ILCudaRuntime.DeviceBuffer(Of Single)(host.Length)

                If zeroFill Then
                    Dim zeros(host.Length - 1) As Single
                    buf.Write(zeros)
                Else
                    buf.Write(host)
                End If

                _map(host) = New Entry(buf, host.Length, version, label)
                _bytes += CLng(host.Length) * 4L

                Return True
            End SyncLock
        End Function

        ''' <summary>解除钉住并立即释放显存；原本未被钉住时返回 <c>False</c>。</summary>
        Public Function Unpin(host As Single()) As Boolean
            If host Is Nothing Then Return False

            SyncLock _sync
                Dim entry As Entry = Nothing

                If Not _map.TryGetValue(host, entry) Then Return False

                _map.Remove(host)
                _bytes -= CLng(entry.Count) * 4L
                entry.Buffer.Dispose()

                Return True
            End SyncLock
        End Function

        ''' <summary>
        ''' 把设备常驻缓冲的内容回写到主机数组。
        ''' </summary>
        ''' <returns>该数组未被钉住时返回 <c>False</c></returns>
        ''' <remarks>
        ''' 回写之后**不要**调用主机的 MarkHostModified：设备侧内容没有变化，
        ''' 版本号保持不变，下一次算子仍可以直接命中这份常驻缓冲。
        ''' </remarks>
        Public Function ReadBack(host As Single()) As Boolean
            If host Is Nothing Then Return False

            Dim buf As ILCudaRuntime.DeviceBuffer(Of Single) = Nothing

            If Not TryGet(host, buf) Then Return False

            Dim device = buf.Read()
            Array.Copy(device, host, std.Min(device.Length, host.Length))

            Return True
        End Function

        ''' <summary>释放全部常驻缓冲。</summary>
        Public Sub Clear()
            SyncLock _sync
                For Each kv In _map
                    kv.Value.Buffer.Dispose()
                Next

                _map.Clear()
                _bytes = 0L
            End SyncLock
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            SyncLock _sync
                If _disposed Then Return
                _disposed = True
            End SyncLock

            Call Clear()
            GC.SuppressFinalize(Me)
        End Sub

    End Class

End Namespace
