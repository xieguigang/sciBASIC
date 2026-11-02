' ---------------------------------------------------------------------------
' PtLegacyFile —— 读取 torch 1.5 及更早版本的 legacy 归档（非 zip 序列化）
'
' 2019 年的预训练权重（例如 smiles-transformer 的 trfm_*.pkl）通常是
'     torch.save(state_dict, path)      # torch < 1.6，默认 legacy 格式
' 的产物；而 PtFile 只认 1.6+ 的 zip 归档。两者的二进制布局完全不同：
'
'   [legacy]  pickle(magic) + pickle(protocol_version) + pickle(sys_info)
'             + pickle(object)            ← 张量用 _rebuild_tensor_v2 描述
'             + 依次追加的 storage 数据块
'   [zip]     data.pkl + version + data/<key> ...
'
' 本类按 legacy 布局逐段解析：
'   1. 三次 UnpickleFromStream 读取文件头（该实现遇到 STOP 立即返回，位置正好停在下一段）；
'   2. 用自定义 IPickleObjectFactory 把 _rebuild_tensor_v2 变成"延迟引用"占位对象；
'   3. pickle 段结束后，按"首次出现顺序"读取 storage 数据块（键顺序与 pickle 的
'      persistent_id 顺序一致），再把占位对象物化为 Tensor；
'   4. 每一步都做长度/元素个数校验，任何不一致都抛出带上下文的异常，
'      绝不"猜"出一份可能错误的权重。
' ---------------------------------------------------------------------------

Imports System.Globalization
Imports System.IO
Imports System.Numerics
Imports System.Text
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports PyPickle = Microsoft.VisualBasic.Data.IO.Pickle

Namespace PT

    ''' <summary>
    ''' 归档格式。
    ''' </summary>
    Public Enum PtContainerFormat
        ''' <summary>无法识别。</summary>
        Unknown = 0
        ''' <summary>1.6+ 的新式 zip 归档（<c>data.pkl</c> + <c>data/&lt;key&gt;</c>）。</summary>
        Zip = 1
        ''' <summary>1.5 及更早版本的 legacy 归档（pickle 之后顺序追加 storage 数据块）。</summary>
        Legacy = 2
    End Enum

    ''' <summary>
    ''' PyTorch legacy 归档（torch &lt; 1.6 的 <c>torch.save</c> 默认格式）读取器。
    ''' </summary>
    Public Class PtLegacyFile

        ''' <summary>legacy 归档的文件头魔数长度（大整数以 LONG1 编码，长度不固定，仅用于诊断）。</summary>
        Private Const ZipMagicFirst As Byte = &H50     ' 'P'
        Private Const ZipMagicSecond As Byte = &H4B    ' 'K'

        Private Shared _lastStorageLayout As String

        ''' <summary>
        ''' 最近一次 legacy 解析所采用的 storage 布局（<c>A</c>/<c>B</c>/<c>C</c>，诊断用）。
        ''' </summary>
        Public Shared ReadOnly Property LastStorageLayout As String
            Get
                Return _lastStorageLayout
            End Get
        End Property

        ''' <summary>storage 占位符：键、元素类型、元素个数，以及延迟填充的数值。</summary>
        Private Class StorageRef
            Public Key As String
            Public ElementType As PtElementType
            Public Numel As Long
            Public Values As Double()
        End Class

        ''' <summary>张量占位符：指向 storage 的一段视图。</summary>
        Private Class TensorRef
            Public Storage As StorageRef
            Public Offset As Long
            Public Shape As Integer()
            Public Strides As Integer()
            Public RequiresGrad As Boolean
        End Class

        ''' <summary>
        ''' 把 <c>_rebuild_tensor_v2</c> 变成占位对象，并记录 storage 的首次出现顺序。
        ''' </summary>
        Private Class LegacyFactory
            Implements PyPickle.IPickleObjectFactory

            Private ReadOnly _storages As New Dictionary(Of String, StorageRef)(StringComparer.Ordinal)
            Private ReadOnly _order As New List(Of StorageRef)

            ''' <summary>按首次出现顺序排列的 storage 引用。</summary>
            Public ReadOnly Property Storages As IReadOnlyList(Of StorageRef)
                Get
                    Return _order
                End Get
            End Property

            Public Function Construct(moduleName As String, className As String, args As Object()) As Object _
                Implements PyPickle.IPickleObjectFactory.Construct
                If args Is Nothing Then Return Nothing

                Select Case className
                    Case "Size", "torch.Size"
                        ' torch.Size 由 GLOBAL(torch, Size) + REDUCE(tuple) 构造，className 只有 "Size"
                        Return toIntegerArray(If(args.Length > 0, args(0), Nothing))

                    Case "_rebuild_tensor_v2", "_rebuild_tensor"
                        If args.Length < 4 Then Return Nothing

                        Dim storage As StorageRef = resolveStorage(args(0))
                        Dim offset As Long = toInt64(args(1))
                        Dim shape As Integer() = toIntegerArray(args(2))
                        Dim strides As Integer() = toIntegerArray(args(3))
                        Dim requiresGrad As Boolean = args.Length > 4 AndAlso toBoolean(args(4))

                        Return New TensorRef With {
                            .Storage = storage,
                            .Offset = offset,
                            .Shape = shape,
                            .Strides = strides,
                            .RequiresGrad = requiresGrad
                        }

                    Case "_rebuild_parameter", "_rebuild_parameter_with_state"
                        ' 参数包装：第 0 个参数已经是张量占位符
                        Return If(args.Length > 0, args(0), Nothing)

                    Case Else
                        Return Nothing
                End Select
            End Function

            Public Function ApplyState(ByRef obj As Object, state As Object) As Boolean _
                Implements PyPickle.IPickleObjectFactory.ApplyState
                Return False
            End Function

            ''' <summary>解析 persistent_id 元组 <c>('storage', type, key, location, numel, ...)</c>。</summary>
            Private Function resolveStorage(value As Object) As StorageRef
                Dim items As Object() = toItems(value)

                If items Is Nothing OrElse items.Length < 5 Then
                    Throw New InvalidDataException(
                        $"legacy 归档中的 storage 引用格式不正确（期望至少 5 个元素，实际 {If(items Is Nothing, 0, items.Length)}，" &
                        $"类型 {If(value Is Nothing, "Nothing", value.GetType().FullName)}）")
                End If

                Dim kind As String = toToken(items(0))

                If Not String.Equals(kind, "storage", StringComparison.Ordinal) Then
                    Throw New InvalidDataException($"不支持的持久化 ID 类型：'{kind}'（仅支持 'storage'）")
                End If

                Dim typeToken As String = toToken(items(1))
                Dim key As String = toToken(items(2))
                Dim numel As Long = toInt64(items(4))
                Dim elementType As PtElementType

                If Not PtDataType.TryParse(typeToken, elementType) Then
                    Throw New InvalidDataException($"无法识别的 storage 元素类型：'{typeToken}'")
                End If

                Dim storage As StorageRef = Nothing

                If _storages.TryGetValue(key, storage) Then Return storage

                storage = New StorageRef With {
                    .Key = key,
                    .ElementType = elementType,
                    .Numel = numel
                }

                _storages(key) = storage
                _order.Add(storage)

                Return storage
            End Function

        End Class

        ''' <summary>探测归档格式。</summary>
        ''' <param name="path">归档文件路径。</param>
        Public Shared Function DetectFormat(path As String) As PtContainerFormat
            If Not File.Exists(path) Then Throw New FileNotFoundException($"archive not found: {path}", path)

            Using stream As Stream = File.OpenRead(path)
                Return DetectFormat(stream)
            End Using
        End Function

        ''' <summary>探测归档格式。</summary>
        ''' <param name="stream">可定位的输入流（方法结束时不关闭流）。</param>
        Public Shared Function DetectFormat(stream As Stream) As PtContainerFormat
            If stream Is Nothing Then Throw New ArgumentNullException(NameOf(stream))
            If Not stream.CanSeek Then Throw New ArgumentException("需要可定位的流", NameOf(stream))

            Dim origin As Long = stream.Position
            Dim header(1) As Byte

            Try
                If stream.Read(header, 0, 2) < 2 Then Return PtContainerFormat.Unknown

                If header(0) = ZipMagicFirst AndAlso header(1) = ZipMagicSecond Then Return PtContainerFormat.Zip

                ' legacy 归档以 pickle 协议头（0x80）或协议 0 的操作码开头
                If header(0) = &H80 OrElse header(0) = &H28 OrElse header(0) = &H5D Then
                    Return PtContainerFormat.Legacy
                End If

                Return PtContainerFormat.Unknown
            Finally
                stream.Seek(origin, SeekOrigin.Begin)
            End Try
        End Function

        ''' <summary>
        ''' 读取归档中的 state_dict（自动识别 zip / legacy）。
        ''' </summary>
        ''' <param name="path">归档文件路径。</param>
        Public Shared Function LoadStateDict(path As String) As PtStateDict
            Select Case DetectFormat(path)
                Case PtContainerFormat.Zip
                    Return PtFile.LoadStateDict(path)
                Case PtContainerFormat.Legacy
                    Return LoadLegacyStateDict(path)
                Case Else
                    Throw New InvalidDataException(
                        $"'{path}' 既不是 PyTorch zip 归档也不是 legacy 归档")
            End Select
        End Function

        ''' <summary>
        ''' 强制按 legacy 布局读取 state_dict。
        ''' </summary>
        ''' <param name="path">归档文件路径。</param>
        Public Shared Function LoadLegacyStateDict(path As String) As PtStateDict
            If Not File.Exists(path) Then Throw New FileNotFoundException($"archive not found: {path}", path)

            Using stream As Stream = File.OpenRead(path)
                Return LoadLegacyStateDict(stream, path)
            End Using
        End Function

        ''' <summary>
        ''' 强制按 legacy 布局从流读取 state_dict。
        ''' </summary>
        ''' <param name="stream">可定位的输入流（方法结束时不关闭流）。</param>
        ''' <param name="source">来源描述，仅用于异常信息。</param>
        Public Shared Function LoadLegacyStateDict(stream As Stream, Optional source As String = Nothing) As PtStateDict
            If stream Is Nothing Then Throw New ArgumentNullException(NameOf(stream))
            If Not stream.CanSeek Then Throw New ArgumentException("需要可定位的流", NameOf(stream))

            ' 1) 文件头：魔数 / 协议版本 / sys_info
            Dim magic As Object = PyPickle.MinimalPickleUnpickler.UnpickleFromStream(stream)
            Dim protocolVersion As Object = PyPickle.MinimalPickleUnpickler.UnpickleFromStream(stream)
            Dim sysInfo As Object = PyPickle.MinimalPickleUnpickler.UnpickleFromStream(stream)

            If magic Is Nothing OrElse protocolVersion Is Nothing Then
                Throw New InvalidDataException($"'{source}' 的 legacy 文件头无法解析")
            End If

            Dim version As Integer = Convert.ToInt32(protocolVersion, CultureInfo.InvariantCulture)

            ' 1001 是 torch 的 legacy 序列化协议版本；其它值说明布局不符，不要继续"猜"
            If version <> 1001 Then
                Throw New InvalidDataException(
                    $"'{source}' 的 legacy 协议版本为 {version}（期望 1001），可能是未知的归档格式")
            End If

            ' 2) 对象本体（张量用占位符表示）
            Dim factory As New LegacyFactory()
            Dim body As Object = PyPickle.MinimalPickleUnpickler.UnpickleFromStream(stream, factory)

            ' 2.5) torch 会在对象之后额外 pickl 一个"存储键列表"，数据块按该顺序追加。
            '      若该段不存在（部分版本），回退到 sorted / 首次出现顺序。
            Dim keyOrder As List(Of String) = tryReadStorageKeys(stream, factory.Storages.Count)

            ' 3) 依次读取 storage 数据块
            Call readStorages(stream, factory.Storages, keyOrder, source)

            ' 4) 物化为 PtStateDict
            Return materialize(body, factory, source)
        End Function

        ''' <summary>
        ''' 尝试读取 torch 在对象之后追加的"存储键列表"（一个 pickle 的字符串列表）。
        ''' </summary>
        ''' <param name="stream">当前位于对象 pickle 结束处的流。</param>
        ''' <param name="expectedCount">期望的键数量（= storage 数量）。</param>
        ''' <returns>键列表；该段不存在或格式不符时返回 <c>Nothing</c>（并把流位置复位）。</returns>
        ''' <remarks>
        ''' 位置复位很重要：如果这里误吞了数据块的开头，后续布局探测就会全部失败。
        ''' </remarks>
        Private Shared Function tryReadStorageKeys(stream As Stream, expectedCount As Integer) As List(Of String)
            Dim origin As Long = stream.Position

            Try
                Dim raw As Object = PyPickle.MinimalPickleUnpickler.UnpickleFromStream(stream)
                Dim items As Object() = toItems(raw)

                If items Is Nothing OrElse items.Length <> expectedCount Then
                    stream.Seek(origin, SeekOrigin.Begin)

                    Return Nothing
                End If

                Dim keys As New List(Of String)(items.Length)

                For Each item As Object In items
                    Dim key As String = toToken(item)

                    If String.IsNullOrEmpty(key) Then
                        stream.Seek(origin, SeekOrigin.Begin)

                        Return Nothing
                    End If

                    keys.Add(key)
                Next

                Return keys
            Catch ex As Exception
                stream.Seek(origin, SeekOrigin.Begin)

                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' 读取 pickle 段之后的 storage 数据块。
        ''' </summary>
        ''' <remarks>
        ''' 不同 torch 版本的 legacy 布局不完全相同，这里对三种候选做"全量自洽"校验：
        ''' <list type="bullet">
        ''' <item>A：<c>[numel][num_bytes][data]</c></item>
        ''' <item>B：<c>[numel][data]</c></item>
        ''' <item>C：<c>[data]</c>（长度完全由 pickle 中的 numel 决定）</item>
        ''' </list>
        ''' 只有"每个 storage 都与 pickle 声明的 numel 一致、且恰好耗尽文件"的布局才会被采用，
        ''' 因此不会静默解出一份可能错误的权重。
        ''' </remarks>
        Private Shared Sub readStorages(stream As Stream, storages As IReadOnlyList(Of StorageRef),
                                        keyOrder As IReadOnlyList(Of String), source As String)
            Dim remaining As Integer = CInt(stream.Length - stream.Position)

            If remaining < 0 Then Throw New InvalidDataException($"'{source}' 的 storage 段长度为负")

            Dim buffer(remaining - 1) As Byte
            Dim read As Integer = 0

            While read < remaining
                Dim n As Integer = stream.Read(buffer, read, remaining - read)

                If n <= 0 Then Exit While

                read += n
            End While

            If read <> remaining Then
                Throw New InvalidDataException($"'{source}' 的 storage 段读取不完整（{read}/{remaining} 字节）")
            End If

            ' 数据块的追加顺序：优先用 torch 写入的键列表，其次按键排序，最后按首次出现顺序；
            ' 每个数据块是否带 numel / num_bytes 头同样需要自适应。
            Dim sequences As New List(Of KeyValuePair(Of String, IReadOnlyList(Of StorageRef)))()

            If keyOrder IsNot Nothing AndAlso keyOrder.Count = storages.Count Then
                Dim byKey As New Dictionary(Of String, StorageRef)(StringComparer.Ordinal)

                For Each storage As StorageRef In storages
                    byKey(storage.Key) = storage
                Next

                If keyOrder.All(Function(k) byKey.ContainsKey(k)) Then
                    sequences.Add(New KeyValuePair(Of String, IReadOnlyList(Of StorageRef))(
                        "keylist", keyOrder.Select(Function(k) byKey(k)).ToList()))
                End If
            End If

            sequences.Add(New KeyValuePair(Of String, IReadOnlyList(Of StorageRef))(
                "sorted", storages.OrderBy(Function(s) s.Key, StringComparer.Ordinal).ToList()))
            sequences.Add(New KeyValuePair(Of String, IReadOnlyList(Of StorageRef))("appearance", storages))

            For Each sequenceEntry As KeyValuePair(Of String, IReadOnlyList(Of StorageRef)) In sequences
                Dim orderName As String = sequenceEntry.Key
                Dim sequence As IReadOnlyList(Of StorageRef) = sequenceEntry.Value

                For Each layout As String In New String() {"A", "B", "C"}
                    Dim all As New List(Of Double)()
                    Dim offset As Integer = 0
                    Dim ok As Boolean = True

                    For Each storage As StorageRef In sequence
                        Dim bytesNeeded As Integer =
                            CInt(storage.Numel) * PtDataType.ElementSize(storage.ElementType)

                        If layout <> "C" Then
                            If offset + 8 > buffer.Length Then ok = False : Exit For

                            Dim numel As Long = BitConverter.ToInt64(buffer, offset)

                            If numel <> storage.Numel Then ok = False : Exit For

                            offset += 8
                        End If

                        If layout = "A" Then
                            If offset + 8 > buffer.Length Then ok = False : Exit For

                            Dim numBytes As Long = BitConverter.ToInt64(buffer, offset)

                            If numBytes <> bytesNeeded Then ok = False : Exit For

                            offset += 8
                        End If

                        If offset + bytesNeeded > buffer.Length Then ok = False : Exit For

                        Call all.AddRange(PtDataType.Decode(buffer, offset, CInt(storage.Numel), storage.ElementType))
                        offset += bytesNeeded
                    Next

                    If Not ok OrElse offset <> buffer.Length Then Continue For

                    Dim cursor As Integer = 0

                    For Each storage As StorageRef In sequence
                        Dim count As Integer = CInt(storage.Numel)
                        Dim values(count - 1) As Double

                        For i As Integer = 0 To count - 1
                            values(i) = all(cursor)
                            cursor += 1
                        Next

                        storage.Values = values
                    Next

                    _lastStorageLayout = $"{layout}/{orderName}"

                    Return
                Next
            Next

            Dim totalRaw As Long = 0

            For Each storage As StorageRef In storages
                totalRaw += storage.Numel * PtDataType.ElementSize(storage.ElementType)
            Next

            Throw New InvalidDataException(
                $"'{source}' 的 storage 段布局无法识别（已尝试 布局 A/B/C × 顺序 sorted/appearance；" &
                $"storage={storages.Count}，剩余字节={remaining}，按 numel 计算的原始数据={totalRaw}，" &
                $"首个 storage={storages(0).Key}/{storages(0).Numel}，" &
                $"开头 32 字节={toHex(buffer, 0, 32)}，结尾 32 字节={toHex(buffer, buffer.Length - 32, 32)}）")
        End Sub

        ''' <summary>把占位对象物化为 <see cref="PtStateDict"/>。</summary>
        Private Shared Function materialize(body As Object, factory As LegacyFactory,
                                           source As String) As PtStateDict
            Dim state As New PtStateDict()

            If TypeOf body Is TensorRef Then
                ' 单个张量：用一个固定名称包装，便于诊断
                state.Add("tensor", materializeTensor(DirectCast(body, TensorRef), source), PtElementType.Float32)

                Return state
            End If

            Dim entries As IDictionary = TryCast(body, IDictionary)

            If entries Is Nothing Then
                Throw New InvalidDataException(
                    $"'{source}' 的对象本体不是 state_dict（实际类型：{If(body Is Nothing, "Nothing", body.GetType().FullName)}）")
            End If

            For Each entry As DictionaryEntry In entries
                Dim name As String = toToken(entry.Key)
                Dim tensorRef As TensorRef = TryCast(entry.Value, TensorRef)

                ' state_dict 是 OrderedDict 的子类，pickle 时会把 `_metadata` 一并写进字典；
                ' 这类非张量条目直接跳过（它们不是权重）。
                If tensorRef Is Nothing Then Continue For

                Dim elementType As PtElementType = tensorRef.Storage.ElementType

                state.Add(name, materializeTensor(tensorRef, source), elementType, tensorRef.RequiresGrad)
            Next

            If state.Count = 0 Then
                Throw New InvalidDataException($"'{source}' 中没有任何张量（state_dict 为空）")
            End If

            Return state
        End Function

        ''' <summary>按 offset / shape / strides 把 storage 的一段视图物化为张量。</summary>
        Private Shared Function materializeTensor(tensorRef As TensorRef, source As String) As Tensor
            Dim storage As StorageRef = tensorRef.Storage
            Dim shape As Integer() = If(tensorRef.Shape, New Integer() {})
            Dim values As Double() = storage.Values

            If values Is Nothing Then
                Throw New InvalidDataException($"'{source}' 中 storage '{storage.Key}' 尚未读取")
            End If

            If shape.Length = 0 Then
                If tensorRef.Offset < 0 OrElse tensorRef.Offset >= values.Length Then
                    Throw New InvalidDataException(
                        $"'{source}' 中标量张量的偏移 {tensorRef.Offset} 超出 storage '{storage.Key}'（{values.Length}）")
                End If

                Return New Tensor(New Double() {values(tensorRef.Offset)}, 1)
            End If

            Dim total As Long = 1

            For Each size As Integer In shape
                total *= size
            Next

            Dim strides As Integer() = If(tensorRef.Strides, rowMajorStrides(shape))

            If strides.Length <> shape.Length Then
                Throw New InvalidDataException(
                    $"'{source}' 中张量的形状 [{String.Join(", ", shape)}] 与步长 [{String.Join(", ", strides)}] 维数不一致")
            End If

            Dim result(total - 1) As Double

            If isContiguous(shape, strides) Then
                If tensorRef.Offset < 0 OrElse tensorRef.Offset + total > values.Length Then
                    Throw New InvalidDataException(
                        $"'{source}' 中张量的视图越界：offset={tensorRef.Offset}, total={total}, storage={values.Length}")
                End If

                Array.Copy(values, CInt(tensorRef.Offset), result, 0, CInt(total))
            Else
                For i As Integer = 0 To CInt(total) - 1
                    Dim coords As Integer() = flatToCoords(i, shape)
                    Dim index As Long = tensorRef.Offset

                    For d As Integer = 0 To shape.Length - 1
                        index += coords(d) * strides(d)
                    Next

                    result(i) = values(index)
                Next
            End If

            Return New Tensor(result, shape)
        End Function

        Private Shared Function rowMajorStrides(shape As Integer()) As Integer()
            Dim strides(shape.Length - 1) As Integer
            Dim acc As Integer = 1

            For i As Integer = shape.Length - 1 To 0 Step -1
                strides(i) = acc
                acc *= shape(i)
            Next

            Return strides
        End Function

        Private Shared Function isContiguous(shape As Integer(), strides As Integer()) As Boolean
            Dim expected As Integer() = rowMajorStrides(shape)

            For i As Integer = 0 To shape.Length - 1
                If strides(i) <> expected(i) Then Return False
            Next

            Return True
        End Function

        Private Shared Function flatToCoords(flat As Integer, shape As Integer()) As Integer()
            Dim coords(shape.Length - 1) As Integer
            Dim rest As Integer = flat

            For i As Integer = shape.Length - 1 To 0 Step -1
                coords(i) = rest Mod shape(i)
                rest \= shape(i)
            Next

            Return coords
        End Function

        ''' <summary>
        ''' 把 pickle 中的"序列"统一成对象数组。
        ''' </summary>
        ''' <remarks>
        ''' <c>PythonTuple</c> 只实现了 <c>IEnumerable(Of Object)</c>，不能直接 <c>TryCast</c> 成 <c>IList</c>。
        ''' </remarks>
        Private Shared Function toItems(value As Object) As Object()
            If value Is Nothing Then Return Nothing

            Dim tuple As PyPickle.PythonTuple = TryCast(value, PyPickle.PythonTuple)

            If tuple IsNot Nothing Then Return tuple.Items

            Dim array As Object() = TryCast(value, Object())

            If array IsNot Nothing Then Return array

            Dim list As IList = TryCast(value, IList)

            If list IsNot Nothing Then
                Dim items(list.Count - 1) As Object

                For i As Integer = 0 To list.Count - 1
                    items(i) = list(i)
                Next

                Return items
            End If

            Return Nothing
        End Function

        Private Shared Function toIntegerArray(value As Object) As Integer()
            If value Is Nothing Then Return New Integer() {}

            Dim list As New List(Of Integer)()

            If TypeOf value Is PyPickle.PythonTuple Then
                Dim tuple As PyPickle.PythonTuple = DirectCast(value, PyPickle.PythonTuple)

                For i As Integer = 0 To tuple.Length - 1
                    list.Add(Convert.ToInt32(tuple(i), CultureInfo.InvariantCulture))
                Next

                Return list.ToArray()
            End If

            Dim items As IEnumerable = TryCast(value, IEnumerable)

            If items IsNot Nothing AndAlso Not TypeOf value Is String Then
                For Each item As Object In items
                    list.Add(Convert.ToInt32(item, CultureInfo.InvariantCulture))
                Next

                Return list.ToArray()
            End If

            Return New Integer() {Convert.ToInt32(value, CultureInfo.InvariantCulture)}
        End Function

        ''' <summary>
        ''' 把 pickle 中的对象渲染为记号文本。
        ''' </summary>
        ''' <remarks>
        ''' 不能使用 <c>Convert.ToString(value, provider)</c>：<c>PythonGlobalRef</c>（例如
        ''' <c>torch.FloatStorage</c> 这类类引用）没有实现 <see cref="IConvertible"/>，会抛
        ''' <c>InvalidCastException</c>。这里统一退回 <see cref="Object.ToString"/>。
        ''' </remarks>
        ''' <summary>把字节片段渲染成十六进制文本（仅用于异常诊断）。</summary>
        Private Shared Function toHex(buffer As Byte(), offset As Integer, count As Integer) As String
            If buffer Is Nothing OrElse buffer.Length = 0 Then Return "<empty>"

            Dim start As Integer = System.Math.Max(0, offset)
            Dim [end] As Integer = System.Math.Min(buffer.Length, start + count)
            Dim sb As New System.Text.StringBuilder

            For i As Integer = start To [end] - 1
                sb.Append(buffer(i).ToString("X2")).Append(" "c)
            Next

            Return sb.ToString().Trim()
        End Function

        Private Shared Function toToken(value As Object) As String
            If value Is Nothing Then Return "None"

            Return value.ToString()
        End Function

        Private Shared Function toInt64(value As Object) As Long
            If value Is Nothing Then Return 0

            Return Convert.ToInt64(value, CultureInfo.InvariantCulture)
        End Function

        Private Shared Function toBoolean(value As Object) As Boolean
            If value Is Nothing Then Return False

            If TypeOf value Is Boolean Then Return CBool(value)

            Return Convert.ToInt32(value, CultureInfo.InvariantCulture) <> 0
        End Function

    End Class

End Namespace
