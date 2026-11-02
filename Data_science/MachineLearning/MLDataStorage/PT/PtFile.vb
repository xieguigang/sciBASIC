' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\PT\PtFile.vb
' /********************************************************************************/

Imports System.Globalization
Imports System.IO
Imports System.IO.Compression
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports PyPickle = Global.Pickle

Namespace PT

    ''' <summary>
    ''' PyTorch 归档（<c>.pt</c>，新式 zip 序列化格式）的结构信息，用于诊断与测试断言。
    ''' </summary>
    Public Class PtArchiveInfo

        ''' <summary>文件路径（从流读取时为 <c>Nothing</c>）。</summary>
        Public Property FilePath As String

        ''' <summary>归档前缀（例如 <c>archive</c>、<c>degree</c>）。</summary>
        Public Property ArchiveName As String

        ''' <summary><c>version</c> 条目的内容（新式 zip 序列化格式为 <c>3</c>）。</summary>
        Public Property Version As String

        ''' <summary><c>data.pkl</c> 条目的大小（字节）。</summary>
        Public Property DataPklSize As Long

        ''' <summary>归档内全部存储键（按数值升序）。</summary>
        Public Property StorageKeys As String()

        ''' <summary>zip 条目总数。</summary>
        Public Property EntryCount As Integer

        ''' <summary>存储块个数。</summary>
        Public ReadOnly Property StorageCount As Integer
            Get
                Return If(StorageKeys Is Nothing, 0, StorageKeys.Length)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{ArchiveName}/ : version={Version}, {StorageCount} storages, data.pkl={DataPklSize} bytes"
        End Function

    End Class

    ''' <summary>
    ''' PyTorch <c>.pt</c> 模型权重文件（新式 zip 序列化格式）的读写门面。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <c>.pt</c> 文件本质是一个 zip 容器：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><c>&lt;prefix&gt;/data.pkl</c>：pickle 流，描述对象结构（名称 → 张量）；</item>
    ''' <item><c>&lt;prefix&gt;/data/&lt;key&gt;</c>：存储块的原始字节；</item>
    ''' <item><c>&lt;prefix&gt;/version</c>：新式 zip 序列化的版本号，内容为 <c>3</c>。</item>
    ''' </list>
    ''' <para>
    ''' 前缀不固定（由 <c>torch.save</c> 的文件名决定），因此读取时由以 <c>data.pkl</c> 结尾的条目反推。
    ''' </para>
    ''' <para>
    ''' 读写都建立在"名称 → <see cref="Tensor"/>"之上：读取把归档里的张量还原为框架张量，
    ''' 写出把框架张量重新组织为 PyTorch 可加载的权重字典。已知限制见 <see cref="NamespaceDoc"/>。
    ''' </para>
    ''' </remarks>
    Public Class PtFile

        ''' <summary>新式 zip 序列化格式的版本号。</summary>
        Public Const SerializationVersion As String = "3"

        ''' <summary>从流写出时使用的默认归档前缀。</summary>
        Public Const DefaultArchiveName As String = "archive"

        Private Const DATA_PICKLE As String = "data.pkl"

#Region "读取"

        ''' <summary>
        ''' 读取 <c>.pt</c> 文件，返回"名称 → 张量"的权重字典。
        ''' </summary>
        ''' <param name="path">文件路径</param>
        Public Shared Function Load(path As String) As Dictionary(Of String, Tensor)
            Return LoadStateDict(path).AsTensors()
        End Function

        ''' <summary>
        ''' 读取 <c>.pt</c> 文件，返回带元素类型与 <c>requires_grad</c> 元信息的权重字典。
        ''' </summary>
        ''' <param name="path">文件路径</param>
        ''' <exception cref="FileNotFoundException">文件不存在</exception>
        ''' <exception cref="InvalidDataException">文件不是合法的 PyTorch 归档</exception>
        Public Shared Function LoadStateDict(path As String) As PtStateDict
            If String.IsNullOrEmpty(path) Then
                Throw New ArgumentException("pt 文件路径不能为空", NameOf(path))
            End If

            If Not File.Exists(path) Then
                Throw New FileNotFoundException($"pt 模型文件不存在：'{path}'", path)
            End If

            Try
                Using stream As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
                    Dim state As PtStateDict = LoadStateDict(stream)

                    Return state
                End Using
            Catch ex As InvalidDataException
                Throw New InvalidDataException($"无法读取 PyTorch 归档 '{path}'：{ex.Message}", ex)
            End Try
        End Function

        ''' <summary>
        ''' 从流读取 PyTorch 归档。
        ''' </summary>
        ''' <param name="stream">zip 流（必须可读、可定位）</param>
        Public Shared Function LoadStateDict(stream As Stream) As PtStateDict
            If stream Is Nothing Then
                Throw New ArgumentNullException(NameOf(stream))
            End If

            Using zip As New ZipArchive(stream, ZipArchiveMode.Read, leaveOpen:=True)
                Dim dataPicker As ZipArchiveEntry = FindDataPickle(zip)

                If dataPicker Is Nothing Then
                    Throw New InvalidDataException(
                        "不是合法的 PyTorch 归档：zip 容器中找不到 '<prefix>/data.pkl' 条目")
                End If

                Dim prefix As String = GetPrefix(dataPicker.FullName)
                Dim pickleBytes As Byte() = ReadEntry(dataPicker)
                Dim reader As Func(Of String, Byte()) = CreateStorageReader(zip, prefix)
                Dim factory As New PtPickleFactory(reader)
                Dim raw As Object = PyPickle.MinimalPickleUnpickler.UnpickleFromBytes(pickleBytes, factory)

                If raw Is Nothing Then
                    Throw New InvalidDataException("PyTorch 归档的 data.pkl 中没有可用的对象")
                End If

                Dim state As New PtStateDict()

                Call Flatten(raw, "", state, factory)

                Return state
            End Using
        End Function

        ''' <summary>
        ''' 读取归档的结构信息（版本、前缀、存储键列表），不解析 pickle 内容。
        ''' </summary>
        ''' <param name="path">文件路径</param>
        Public Shared Function ReadInfo(path As String) As PtArchiveInfo
            If Not File.Exists(path) Then
                Throw New FileNotFoundException($"pt 模型文件不存在：'{path}'", path)
            End If

            Using stream As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
                Using zip As New ZipArchive(stream, ZipArchiveMode.Read, leaveOpen:=True)
                    Dim dataPicker As ZipArchiveEntry = FindDataPickle(zip)

                    If dataPicker Is Nothing Then
                        Throw New InvalidDataException(
                            $"不是合法的 PyTorch 归档 '{path}'：缺少 '<prefix>/data.pkl' 条目")
                    End If

                    Dim prefix As String = GetPrefix(dataPicker.FullName)
                    Dim storageDir As String = prefix & "data/"
                    Dim keys As New List(Of String)()

                    For Each entry As ZipArchiveEntry In zip.Entries
                        If entry.FullName.StartsWith(storageDir, StringComparison.Ordinal) Then
                            Call keys.Add(entry.FullName.Substring(storageDir.Length))
                        End If
                    Next

                    Return New PtArchiveInfo With {
                        .FilePath = path,
                        .ArchiveName = prefix.TrimEnd("/"c),
                        .Version = ReadVersion(zip, prefix),
                        .DataPklSize = dataPicker.Length,
                        .StorageKeys = SortKeys(keys).ToArray(),
                        .EntryCount = zip.Entries.Count
                    }
                End Using
            End Using
        End Function

        ''' <summary>查找 <c>data.pkl</c> 条目（允许任意前缀）。</summary>
        Private Shared Function FindDataPickle(zip As ZipArchive) As ZipArchiveEntry
            Dim exact As ZipArchiveEntry = zip.GetEntry(DATA_PICKLE)

            If exact IsNot Nothing Then
                Return exact
            End If

            For Each entry As ZipArchiveEntry In zip.Entries
                If entry.FullName.EndsWith("/" & DATA_PICKLE, StringComparison.Ordinal) Then
                    Return entry
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>取归档前缀（含结尾的 <c>/</c>；无前缀时返回空串）。</summary>
        Private Shared Function GetPrefix(entryName As String) As String
            Dim index As Integer = entryName.LastIndexOf("/"c)

            If index < 0 Then
                Return ""
            End If

            Return entryName.Substring(0, index + 1)
        End Function

        ''' <summary>读取 <c>version</c> 条目；缺失时返回 <c>Nothing</c>。</summary>
        Private Shared Function ReadVersion(zip As ZipArchive, prefix As String) As String
            Dim entry As ZipArchiveEntry = zip.GetEntry(prefix & "version")

            If entry Is Nothing Then
                Return Nothing
            End If

            Return Encoding.ASCII.GetString(ReadEntry(entry)).Trim()
        End Function

        ''' <summary>创建"存储键 → 原始字节"的读取委托。</summary>
        Private Shared Function CreateStorageReader(zip As ZipArchive, prefix As String) As Func(Of String, Byte())
            Dim storageDir As String = prefix & "data/"

            Return Function(key As String)
                       Dim entry As ZipArchiveEntry = zip.GetEntry(storageDir & key)

                       If entry Is Nothing Then
                           Return Nothing
                       End If

                       Return ReadEntry(entry)
                   End Function
        End Function

        Private Shared Function ReadEntry(entry As ZipArchiveEntry) As Byte()
            Using source As Stream = entry.Open()
                Using buffer As New MemoryStream(CInt(entry.Length))
                    Call source.CopyTo(buffer)

                    Return buffer.ToArray()
                End Using
            End Using
        End Function

        ''' <summary>
        ''' 把反序列化得到的对象树展开成"名称 → 张量"，嵌套字典按点号路径拼接名称。
        ''' </summary>
        Private Shared Sub Flatten(raw As Object, prefix As String, state As PtStateDict, factory As PtPickleFactory)
            If raw Is Nothing Then
                Return
            End If

            Dim tensor As Tensor = TryCast(raw, Tensor)

            If tensor IsNot Nothing Then
                Dim elementType As PtElementType = PtElementType.Float32
                Dim requiresGrad As Boolean = tensor.IsVariable

                Call factory.TryGetMetadata(tensor, elementType, requiresGrad)

                Call state.Add(If(String.IsNullOrEmpty(prefix), "tensor", prefix), tensor, elementType, requiresGrad)
                Return
            End If

            Dim map As IDictionary = TryCast(raw, IDictionary)

            If map IsNot Nothing Then
                For Each item As DictionaryEntry In map
                    Dim key As String = If(item.Key Is Nothing, "", CStr(item.Key))
                    Dim name As String = If(prefix.Length = 0, key, prefix & "." & key)

                    Call Flatten(item.Value, name, state, factory)
                Next

                Return
            End If

            If TypeOf raw Is String OrElse TypeOf raw Is Byte() Then
                Return
            End If

            Dim list As IList = TryCast(raw, IList)

            If list IsNot Nothing Then
                For i As Integer = 0 To list.Count - 1
                    Call Flatten(list(i), $"{prefix}[{i}]", state, factory)
                Next
            End If
        End Sub

#End Region

#Region "写出"

        ''' <summary>
        ''' 把"名称 → 张量"的权重字典保存为 <c>.pt</c> 文件（全部按同一元素类型写出）。
        ''' </summary>
        ''' <param name="path">目标文件路径</param>
        ''' <param name="tensors">名称 → 张量</param>
        ''' <param name="elementType">落盘元素类型，默认单精度（PyTorch 的默认权重精度）</param>
        Public Shared Sub Save(path As String, tensors As IDictionary(Of String, Tensor),
                               Optional elementType As PtElementType = PtElementType.Float32)
            Call Save(path, PtStateDict.FromTensors(tensors, elementType))
        End Sub

        ''' <summary>
        ''' 保存权重字典（每项按自身的元素类型写出）。
        ''' </summary>
        ''' <param name="path">目标文件路径</param>
        ''' <param name="state">权重字典</param>
        Public Shared Sub Save(path As String, state As PtStateDict)
            If String.IsNullOrEmpty(path) Then
                Throw New ArgumentException("pt 文件路径不能为空", NameOf(path))
            End If

            If state Is Nothing Then
                Throw New ArgumentNullException(NameOf(state))
            End If

            Dim archiveName As String = GetArchiveName(path)
            Dim folder As String = Path.GetDirectoryName(Path.GetFullPath(path))

            If Not String.IsNullOrEmpty(folder) AndAlso Not Directory.Exists(folder) Then
                Call Directory.CreateDirectory(folder)
            End If

            Using stream As New FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)
                Call SaveToStream(stream, state, archiveName)
            End Using
        End Sub

        ''' <summary>
        ''' 把权重字典写入 zip 流。
        ''' </summary>
        ''' <param name="stream">目标流</param>
        ''' <param name="state">权重字典</param>
        ''' <param name="archiveName">归档前缀（不含结尾 <c>/</c>）</param>
        ''' <param name="legacyStorageNames">
        ''' 是否使用旧式 storage 类名（<c>torch.FloatStorage</c>）而不是新式 dtype 记号（<c>torch.float32</c>）
        ''' </param>
        Public Shared Sub SaveToStream(stream As Stream, state As PtStateDict,
                                       Optional archiveName As String = DefaultArchiveName,
                                       Optional legacyStorageNames As Boolean = False)
            If stream Is Nothing Then
                Throw New ArgumentNullException(NameOf(stream))
            End If

            If state Is Nothing Then
                Throw New ArgumentNullException(NameOf(state))
            End If

            Dim prefix As String = SanitizeArchiveName(archiveName) & "/"

            Using zip As New ZipArchive(stream, ZipArchiveMode.Create, leaveOpen:=True)
                ' 1) 存储块：每个张量独占一块，键为顺序编号（与 torch.save 的命名一致）
                Dim keys As New List(Of String)(state.Count)

                For i As Integer = 0 To state.Count - 1
                    Dim entry As PtTensorEntry = state.Entries(i)

                    If entry.Value Is Nothing Then
                        Throw New InvalidDataException($"权重项 '{entry.Name}' 的张量为空，无法写出")
                    End If

                    Dim key As String = i.ToString(CultureInfo.InvariantCulture)
                    Dim bytes As Byte() = PtDataType.Encode(entry.Value.Data, entry.ElementType)

                    Call keys.Add(key)
                    Call WriteEntry(zip, prefix & "data/" & key, bytes)
                Next

                ' 2) pickle 流
                Dim pickleBytes As Byte() = PtPickler.Pickle(state, keys.ToArray(), legacyStorageNames)

                Call WriteEntry(zip, prefix & DATA_PICKLE, pickleBytes)

                ' 3) 版本标记
                Call WriteEntry(zip, prefix & "version", Encoding.ASCII.GetBytes(SerializationVersion))
            End Using
        End Sub

        ''' <summary>写入一个不压缩的 zip 条目（与 <c>torch.save</c> 的 ZIP_STORED 一致）。</summary>
        Private Shared Sub WriteEntry(zip As ZipArchive, name As String, bytes As Byte())
            Dim entry As ZipArchiveEntry = zip.CreateEntry(name, CompressionLevel.NoCompression)

            Using target As Stream = entry.Open()
                Call target.Write(bytes, 0, bytes.Length)
            End Using
        End Sub

        ''' <summary>由文件路径推导归档前缀（与 <c>torch.save</c> 一致，使用文件名主干）。</summary>
        Private Shared Function GetArchiveName(path As String) As String
            Return SanitizeArchiveName(Path.GetFileNameWithoutExtension(path))
        End Function

        ''' <summary>清理归档前缀中的非法字符（zip 条目名不能含路径分隔符）。</summary>
        Private Shared Function SanitizeArchiveName(name As String) As String
            If String.IsNullOrEmpty(name) Then
                Return DefaultArchiveName
            End If

            Dim builder As New StringBuilder(name.Length)

            For Each c As Char In name
                Select Case c
                    Case "/"c, "\"c, ":"c
                        Call builder.Append("_"c)
                    Case Else
                        Call builder.Append(c)
                End Select
            Next

            Return builder.ToString()
        End Function

#End Region

#Region "辅助"

        ''' <summary>把存储键按数值升序排序（非数值键排在后面，按字典序）。</summary>
        Private Shared Function SortKeys(keys As IEnumerable(Of String)) As IEnumerable(Of String)
            Return keys.OrderBy(Function(k) KeyOrder(k)).ThenBy(Function(k) k, StringComparer.Ordinal)
        End Function

        Private Shared Function KeyOrder(key As String) As Long
            Dim value As Long

            If Long.TryParse(key, NumberStyles.Integer, CultureInfo.InvariantCulture, value) Then
                Return value
            Else
                Return Long.MaxValue
            End If
        End Function

#End Region

    End Class

End Namespace
