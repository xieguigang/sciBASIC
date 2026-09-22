' ---------------------------------------------------------------------------
' 设备常驻缓冲注册表
'
' 用途：把"权重 / 梯度 / 优化器状态"这类长期存在的张量钉在显存里，
' 避免每个训练步都从主机重新上传整个模型。
'
' 为什么需要它 —— 与 DeviceCache 的分工：
'
'   * DeviceCache 是"算子输入的暂存区"，以 (主机数组引用, 数据版本号) 为键做 LRU 淘汰。
'     这个设计对推理非常合适：权重不变，缓冲一直命中。
'     但训练时不行 —— AdamW 每步都会改写权重并递增版本号，于是每个权重
'     每步都会失效重传。200M 参数在单精度下就是 764 MB/步的纯浪费。
'
'   * DeviceResidentStore 是"主副本所在"，缓冲一旦钉住就不再随版本号失效，
'     直到显式 Unpin / Dispose 才释放。因此训练时权重只需要上传一次。
'
' 释放语义（重要）：
'   ILCuda 的 DeviceMemory 没有终结器，依赖 GC 回收会让 cuMemAlloc 出来的显存
'   永不归还。所以这里采用与 DeviceCache 相同的显式所有权：
'   Unpin / Clear / Dispose 立即释放显存，Entry 的 Finalize 只作异常路径兜底。
' ---------------------------------------------------------------------------

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports ILCudaRuntime = Microsoft.VisualBasic.Computing.ILCuda.Runtime

Namespace GPUTensor

    ''' <summary>
    ''' 以主机 <c>Double()</c> 数组引用为键的设备常驻缓冲注册表。
    ''' </summary>
    ''' <remarks>
    ''' 缓冲一律是<b>单精度</b>（<c>DeviceBuffer(Of Single)</c>）：消费级显卡的 FP64
    ''' 吞吐只有 FP32 的 1/64，而训练的全部浮点运算里矩阵乘占 95% 以上，
    ''' 因此设备侧统一用 FP32；主机侧保留 <c>Double()</c> 主副本以便检查点落盘
    ''' 与 CPU 路径对照。
    ''' </remarks>
    Public NotInheritable Class DeviceResidentStore
        Implements IDisposable

        ''' <summary>
        ''' 单个常驻条目。
        ''' </summary>
        ''' <remarks>
        ''' <see cref="Finalize"/> 只是异常路径的兜底：正常流程一律走显式 Dispose。
        ''' </remarks>
        Private NotInheritable Class Entry

            Public ReadOnly Buffer As ILCudaRuntime.DeviceBuffer(Of Single)
            ''' <summary>钉住时刻的数据版本号，仅用于诊断（不参与失效判定）</summary>
            Public ReadOnly Version As Long
            Public ReadOnly Label As String

            Public Sub New(buffer As ILCudaRuntime.DeviceBuffer(Of Single), version As Long, label As String)
                Me.Buffer = buffer
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
            Implements IEqualityComparer(Of Double())

            Public Function Equals(a As Double(), b As Double()) As Boolean Implements IEqualityComparer(Of Double()).Equals
                Return ReferenceEquals(a, b)
            End Function

            Public Function GetHashCode(a As Double()) As Integer Implements IEqualityComparer(Of Double()).GetHashCode
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
        Public Function TryGet(host As Double(), ByRef buffer As ILCudaRuntime.DeviceBuffer(Of Single)) As Boolean
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
        ''' 把主机数组钉成常驻显存缓冲（已钉住则直接返回现有缓冲）。
        ''' </summary>
        ''' <param name="engine">用于诊断显存余量的引擎</param>
        ''' <param name="host">主机主副本</param>
        ''' <param name="label">诊断用标签，例如 <c>layer3.moe.expert5.Wg</c></param>
        ''' <param name="zeroFill">
        ''' 是否用 0 初始化。梯度累加器与优化器状态（m / v）用 <c>True</c>，
        ''' 权重用 <c>False</c>（按主机内容上传）。
        ''' </param>
        ''' <remarks>
        ''' 分配前会查询设备可用显存；不足以容纳时抛出带具体字节数的
        ''' <see cref="OutOfMemoryException"/>，而不是让 CUDA 底层抛出难以定位的错误码。
        ''' </remarks>
        Public Function Pin(engine As ILCudaRuntime.CudaEngine, host As Double(), label As String,
                            Optional zeroFill As Boolean = False) As ILCudaRuntime.DeviceBuffer(Of Single)
            If host Is Nothing Then Throw New ArgumentNullException(NameOf(host))
            If host.Length = 0 Then Throw New ArgumentException("不能钉住空数组", NameOf(host))

            Dim existing As ILCudaRuntime.DeviceBuffer(Of Single) = Nothing
            If TryGet(host, existing) Then Return existing

            If _disposed Then Throw New ObjectDisposedException(NameOf(DeviceResidentStore))

            Dim needBytes As Long = CLng(host.Length) * 4L

            Call EnsureCapacity(engine, needBytes, label)

            Dim buffer As New ILCudaRuntime.DeviceBuffer(Of Single)(host.Length)

            If zeroFill Then
                buffer.Fill(0.0F)
            Else
                buffer.Write(ToSingle(host))
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
        ''' 把常驻缓冲的内容回读到主机（设备为主副本时用于同步检查点 / 统计）。
        ''' </summary>
        ''' <remarks>
        ''' 未钉住时直接返回 <c>Nothing</c>，由调用方决定是否视为错误。
        ''' </remarks>
        Public Function Download(host As Double()) As Double()
            Dim buffer As ILCudaRuntime.DeviceBuffer(Of Single) = Nothing

            If Not TryGet(host, buffer) Then Return Nothing

            Return ToDouble(buffer.Read())
        End Function

        ''' <summary>把主机内容重新上传到常驻缓冲。</summary>
        Public Function Upload(host As Double()) As Boolean
            Dim buffer As ILCudaRuntime.DeviceBuffer(Of Single) = Nothing

            If Not TryGet(host, buffer) Then Return False
            If buffer.Count <> host.Length Then Return False

            buffer.Write(ToSingle(host))

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

            ' 留 5% 余量：训练过程中还会有算子输出缓冲的临时分配
            Dim margin As Long = CLng(info.FreeBytes / 20UL)

            If needBytes + margin > CLng(info.FreeBytes) Then
                Throw New OutOfMemoryException(
                    $"显存不足以常驻 {label}：需要 {FormatMb(needBytes)}，" &
                    $"当前可用 {FormatMb(CLng(info.FreeBytes))}（总量 {FormatMb(CLng(info.TotalBytes))}）。" &
                    "请调小模型规模、batch 或序列长度。")
            End If
        End Sub

        Private Shared Function FormatMb(bytes As Long) As String
            Return $"{bytes / 1024.0 / 1024.0:N1} MB"
        End Function

        ''' <summary>
        ''' 低于该元素数就串行转换：小数组上并行调度本身的开销更大。
        ''' </summary>
        Private Const ParallelConvertThreshold As Integer = 16384

        ''' <summary>主机 <c>Double()</c> → 显存 <c>Single()</c>（降精度上传）</summary>
        ''' <remarks>
        ''' 这一步是 O(元素数) 的纯类型转换，必须在主机上完成（<c>Double</c> 与 <c>Single</c>
        ''' 的位模式不同，不能用 <c>Buffer.BlockCopy</c> 直接搬）。语言模型的 LM head
        ''' 在 12.8 万词表下是 3300 万元素，串行 VB 循环会达到数百毫秒 ——
        ''' 足以盖过内核本身的全部计算时间。因此大数组一律分块并行。
        ''' </remarks>
        Friend Shared Function ToSingle(host As Double()) As Single()
            Dim buffer(host.Length - 1) As Single
            Dim n = host.Length

            If n < ParallelConvertThreshold Then
                For i As Integer = 0 To n - 1
                    buffer(i) = CSng(host(i))
                Next

                Return buffer
            End If

            Call RunChunked(n,
                Sub(start As Integer, [end] As Integer)
                    For i As Integer = start To [end] - 1
                        buffer(i) = CSng(host(i))
                    Next
                End Sub)

            Return buffer
        End Function

        ''' <summary>显存 <c>Single()</c> → 主机 <c>Double()</c>（升精度回读）</summary>
        Friend Shared Function ToDouble(deviceData As Single()) As Double()
            Dim buffer(deviceData.Length - 1) As Double
            Dim n = deviceData.Length

            If n < ParallelConvertThreshold Then
                For i As Integer = 0 To n - 1
                    buffer(i) = CDbl(deviceData(i))
                Next

                Return buffer
            End If

            Call RunChunked(n,
                Sub(start As Integer, [end] As Integer)
                    For i As Integer = start To [end] - 1
                        buffer(i) = CDbl(deviceData(i))
                    Next
                End Sub)

            Return buffer
        End Function

        ''' <summary>
        ''' 把 <c>[0, n)</c> 切成约 <c>4 × 处理器数</c> 块并行执行。
        ''' </summary>
        ''' <remarks>
        ''' 块数取"处理器数的 4 倍"是为了让负载均衡足以吸收个别块偏慢的情况，
        ''' 又不会因为块太小而让调度开销占主导。
        ''' </remarks>
        Private Shared Sub RunChunked(n As Integer, body As Action(Of Integer, Integer))
            Dim workers = System.Math.Max(1, Environment.ProcessorCount)
            Dim chunk As Integer = System.Math.Max(ParallelConvertThreshold, n \ (workers * 4))

            Dim blocks = (n + chunk - 1) \ chunk

            Call System.Threading.Tasks.Parallel.For(0, blocks,
                Sub(block As Integer)
                    Dim start = block * chunk
                    Dim [end] = System.Math.Min(start + chunk, n)

                    Call body(start, [end])
                End Sub)
        End Sub

    End Class

End Namespace
