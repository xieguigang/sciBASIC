' ---------------------------------------------------------------------------
' 显存驻留缓存（LRU）
'
' 目的：避免每个算子都把主机 Double 数组重新降精度并上传一次显存。
' 缓存以“主机数组的引用标识 + 数据版本号”为键：
'   * 同一个数组、版本未变  -> 命中，直接复用显存缓冲；
'   * 版本改变（原地写入）  -> 淘汰旧缓冲并重新上传。
'
' 重要：ILCuda 的 DeviceMemory 没有终结器，如果依赖 GC 自动回收，
' cuMemAlloc 出来的显存将永远不会被释放。因此这里采用显式 LRU：
'   * 超出容量时立即 Dispose 被淘汰的条目；
'   * Clear() 在 CudaTensor.Dispose / 引擎关闭时逐条释放；
'   * Entry 保留 Finalize() 作为异常路径的兜底。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.ILCuda.Runtime

Namespace GPUTensor

    Friend NotInheritable Class DeviceCache
        Implements IDisposable

        ''' <summary>单个缓存条目：显存缓冲 + 上传时的数据版本 + LRU 时间戳</summary>
        Private NotInheritable Class Entry

            Public ReadOnly Buffer As DeviceBuffer(Of Single)
            Public ReadOnly Version As Long
            Public Property Ticks As Long

            Public Sub New(buffer As DeviceBuffer(Of Single), version As Long, ticks As Long)
                Me.Buffer = buffer
                Me.Version = version
                Me.Ticks = ticks
            End Sub

            ''' <summary>兜底释放（正常情况下都会走显式 Dispose）</summary>
            Protected Overrides Sub Finalize()
                Try
                    Buffer.Dispose()
                Catch
                End Try
            End Sub

        End Class

        ''' <summary>按数组引用身份比较的相等性比较器</summary>
        Private NotInheritable Class HostComparer
            Implements IEqualityComparer(Of Double())

            Public Function ReferenceEqualsTo(a As Double(), b As Double()) As Boolean Implements IEqualityComparer(Of Double()).Equals
                Return ReferenceEquals(a, b)
            End Function

            Public Function HashOf(a As Double()) As Integer Implements IEqualityComparer(Of Double()).GetHashCode
                If a Is Nothing Then Return 0
                Return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(a)
            End Function

        End Class

        Private ReadOnly _map As New Dictionary(Of Double(), Entry)(New HostComparer())
        Private ReadOnly _sync As New Object()
        Private ReadOnly _capacity As Long
        Private _bytes As Long
        Private _ticks As Long

        ''' <param name="capacityBytes">显存容量上限（字节）；&lt;= 0 表示不限制</param>
        Public Sub New(capacityBytes As Long)
            _capacity = capacityBytes
        End Sub

        ''' <summary>已驻留的字节数</summary>
        Public ReadOnly Property Bytes As Long
            Get
                Return _bytes
            End Get
        End Property

        ''' <summary>当前驻留的缓冲个数</summary>
        Public ReadOnly Property Count As Integer
            Get
                SyncLock _sync
                    Return _map.Count
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' 取得主机数组对应的显存缓冲；版本不一致时重新上传。
        ''' </summary>
        Public Function GetBuffer(engine As CudaEngine, host As Double(), version As Long) As DeviceBuffer(Of Single)
            If host Is Nothing OrElse host.Length = 0 Then
                Throw New ArgumentException("主机数组不能为空", NameOf(host))
            End If

            SyncLock _sync
                Dim entry As Entry = Nothing

                If _map.TryGetValue(host, entry) AndAlso entry.Version = version Then
                    _ticks += 1
                    entry.Ticks = _ticks
                    Return entry.Buffer
                End If

                ' 版本失效：淘汰旧条目并释放显存
                If entry IsNot Nothing Then
                    _map.Remove(host)
                    _bytes -= CLng(entry.Buffer.Count) * 4L
                    entry.Buffer.Dispose()
                End If

                Evict(CLng(host.Length) * 4L)

                Dim buffer As New DeviceBuffer(Of Single)(host.Length)
                buffer.Write(FloatBridge.ToSingle(host))

                _ticks += 1
                _map(host) = New Entry(buffer, version, _ticks)
                _bytes += CLng(host.Length) * 4L

                Return buffer
            End SyncLock
        End Function

        ''' <summary>按 LRU 淘汰直到腾出需要的空间</summary>
        Private Sub Evict(needBytes As Long)
            If _capacity <= 0 Then Return
            If _bytes + needBytes <= _capacity Then Return

            Dim snapshot = _map.ToArray()
            Array.Sort(snapshot, Function(x, y) x.Value.Ticks.CompareTo(y.Value.Ticks))

            For Each pair In snapshot
                If _bytes + needBytes <= _capacity Then Exit For

                _map.Remove(pair.Key)
                _bytes -= CLng(pair.Value.Buffer.Count) * 4L
                pair.Value.Buffer.Dispose()
            Next
        End Sub

        ''' <summary>释放全部驻留的显存缓冲</summary>
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
            Clear()
        End Sub

    End Class

End Namespace
