Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.IO.Compression
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.PT
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' PyTorch <c>.pt</c> 读写模块（<c>Microsoft.VisualBasic.MachineLearning.DataStorage.PT</c>）的往返测试。
''' </summary>
''' <remarks>
''' <para>
''' 测试数据来自 KcatNet 项目真实产出的两个 PyTorch 归档：
''' </para>
''' <list type="bullet">
''' <item><c>Dataset/degree.pt</c>：根是普通 dict，3 个 int64 张量且 <c>requires_grad = True</c>；</item>
''' <item><c>RESULT/model_KcatNet.pt</c>：根是 OrderedDict，约 200 个 float32 权重张量（23 MB）。</item>
''' </list>
''' <para>
''' 覆盖：归档结构探测、读取、写出、再读取的逐值往返、元素类型保持、异常输入。
''' </para>
''' </remarks>
Public Module PtTests

    Private Const KcatNetHome As String = "C:\Users\Administrator\Downloads\KcatNet-main\KcatNet-main"

    Private ReadOnly DegreeFile As String = IO.Path.Combine(KcatNetHome, "Dataset", "degree.pt")
    Private ReadOnly ModelFile As String = IO.Path.Combine(KcatNetHome, "RESULT", "model_KcatNet.pt")

    ''' <summary>运行全部 PT 测试。</summary>
    Public Sub Run()
        Call Console.WriteLine("== PyTorch .pt 读写测试")

        Call TestDataTypeCodec()
        Call TestSyntheticRoundtrip()
        Call TestDegreeFile()
        Call TestModelFile()
        Call TestInvalidInputs()
    End Sub

#Region "数据类型编解码"

    ''' <summary>
    ''' 元素类型编解码：所有可精确表示的值必须原样往返。
    ''' </summary>
    Private Sub TestDataTypeCodec()
        Call Console.WriteLine("-- 元素类型编解码")

        ' 只使用各类型可**精确表示**的数值，这样编解码往返应当逐位相等
        Call CheckCodec(PtElementType.Float32, {0.0, 1.0, -2.5, 0.5, 1024.0, 9.094947017729282E-13})
        Call CheckCodec(PtElementType.Float64, {0.0, 1.0, -2.5, std.PI, 1.0E-300})
        Call CheckCodec(PtElementType.Float16, {0.0, 1.0, -2.5, 0.5, 65504.0, 0.0009765625})
        Call CheckCodec(PtElementType.BFloat16, {0.0, 1.0, -2.5, 0.5, 1.152921504606847E+18, 3.0517578125E-05})
        Call CheckCodec(PtElementType.Int64, {0.0, 1.0, -1.0, 123456789.0, -987654321.0})
        Call CheckCodec(PtElementType.Int32, {0.0, 1.0, -1.0, 2147483647.0})
        Call CheckCodec(PtElementType.Int16, {0.0, 1.0, -1.0, 32767.0, -32768.0})
        Call CheckCodec(PtElementType.Int8, {0.0, 1.0, -1.0, 127.0, -128.0})
        Call CheckCodec(PtElementType.UInt8, {0.0, 1.0, 128.0, 255.0})
        Call CheckCodec(PtElementType.Bool, {0.0, 1.0, 1.0, 0.0})

        ' 类型记号解析：新式 dtype 与旧式 storage 类名都要能识别
        Call DebugAudit.AreEqual(PtElementType.Float32, PtDataType.Parse("torch.float32"), "torch.float32 应当解析为 Float32")
        Call DebugAudit.AreEqual(PtElementType.Float32, PtDataType.Parse("torch.FloatStorage"), "torch.FloatStorage 应当解析为 Float32")
        Call DebugAudit.AreEqual(PtElementType.Int64, PtDataType.Parse("LongStorage"), "LongStorage 应当解析为 Int64")
        Call DebugAudit.AreEqual(PtElementType.UInt8, PtDataType.Parse("torch.ByteStorage"), "torch.ByteStorage 应当解析为 UInt8")
        Call DebugAudit.AreEqual(PtElementType.Bool, PtDataType.Parse("torch.bool"), "torch.bool 应当解析为 Bool")

        Dim unsupported As Boolean = False

        Try
            Call PtDataType.Parse("torch.complex64")
        Catch ex As NotSupportedException
            unsupported = True
        End Try

        Call DebugAudit.Test(unsupported, "无法识别的 dtype 记号应当抛出 NotSupportedException")
    End Sub

    Private Sub CheckCodec(type As PtElementType, values As Double())
        Dim bytes As Byte() = PtDataType.Encode(values, type)

        Call DebugAudit.AreEqual(values.Length * PtDataType.ElementSize(type), bytes.Length,
                                 $"{PtDataType.DTypeName(type)}: 字节长度应当等于 元素数 × 元素大小")

        Dim restored As Double() = PtDataType.Decode(bytes, 0, values.Length, type)
        Dim diff As Double = MaxAbsDifference(values, restored)

        Call DebugAudit.Test(diff = 0.0,
                             $"{PtDataType.DTypeName(type)}: 编解码往返应当完全一致（max|Δ| = {diff:G3}）")
    End Sub

#End Region

#Region "合成张量往返"

    ''' <summary>
    ''' 用合成的张量覆盖 1D / 2D / 4D 形状、多种元素类型与 requires_grad。
    ''' </summary>
    Private Sub TestSyntheticRoundtrip()
        Call Console.WriteLine("-- 合成张量写出 / 读回")

        Dim state As New PtStateDict()

        Call state.Add("layer.weight", PatternTensor({4, 3}, 11), PtElementType.Float32)
        Call state.Add("layer.bias", PatternTensor({4}, 12), PtElementType.Float32, requiresGrad:=True)
        Call state.Add("attention.scale", New Tensor(New Double() {0.125}, 1), PtElementType.Float64)
        Call state.Add("counter", New Tensor(New Double() {1, 2, 3, 4, 5, 6}, 2, 3), PtElementType.Int64)
        Call state.Add("mask", New Tensor(New Double() {1, 0, 1, 1}, 2, 2), PtElementType.Bool)
        Call state.Add("blocks", PatternTensor({2, 2, 3, 2}, 13), PtElementType.Float32)

        Dim path As String = TempFile("scibasic_pt_synthetic")

        Try
            Call state.Save(path)
            Call CheckArchiveLayout(path, IO.Path.GetFileNameWithoutExtension(path), expectedStorages:=state.Count)

            Dim restored As PtStateDict = PtFile.LoadStateDict(path)

            Call DebugAudit.AreEqual(state.Count, restored.Count, "合成模型: 读回的张量个数应当一致")

            For Each entry As PtTensorEntry In state.Entries
                Dim other As PtTensorEntry

                If Not restored.TryGetEntry(entry.Name, other) Then
                    Call DebugAudit.Test(False, $"合成模型: 读回结果缺少 '{entry.Name}'")
                    Continue For
                End If

                Call DebugAudit.AreEqual(entry.ElementType, other.ElementType,
                                         $"合成模型: '{entry.Name}' 的元素类型应当保持为 {PtDataType.DTypeName(entry.ElementType)}")
                Call DebugAudit.Test(entry.Shape.SequenceEqual(other.Shape),
                                     $"合成模型: '{entry.Name}' 的形状应当保持为 [{String.Join(", ", entry.Shape)}]，实际 [{String.Join(", ", other.Shape)}]")
                Call DebugAudit.AreEqual(entry.RequiresGrad, other.RequiresGrad,
                                         $"合成模型: '{entry.Name}' 的 requires_grad 应当保持")
                Call DebugAudit.AreEqual(0.0, MaxAbsDifference(entry.Value.Data, other.Value.Data),
                                         $"合成模型: '{entry.Name}' 的数值应当完全一致")
            Next

            Call Console.WriteLine($"  [ok] 合成模型 {state.Count} 个张量往返")
        Catch ex As Exception
            Call ReportException("合成模型往返", ex)
        Finally
            Call Remove(path)
        End Try
    End Sub

    Private Function PatternTensor(shape As Integer(), seed As Integer) As Tensor
        Dim length As Integer = 1

        For Each size As Integer In shape
            length *= size
        Next

        Dim values As Double() = New Double(length - 1) {}
        Dim state As Long = (seed * 1103515245L + 12345L) And &H7FFFFFFFL

        For i As Integer = 0 To length - 1
            state = (state * 1103515245L + 12345L) And &H7FFFFFFFL
            values(i) = CSng((state Mod 20000L) / 10000.0 - 1.0)
        Next

        Return New Tensor(values, shape)
    End Function

#End Region

#Region "真实模型：degree.pt"

    Private Sub TestDegreeFile()
        Call Console.WriteLine($"-- KcatNet degree.pt: {DegreeFile}")

        If Not File.Exists(DegreeFile) Then
            Call DebugAudit.Test(False, $"测试数据缺失：{DegreeFile}")
            Call Console.WriteLine("  [fail] 文件不存在")
            Return
        End If

        Try
            Dim info As PtArchiveInfo = PtFile.ReadInfo(DegreeFile)

            Call DebugAudit.AreEqual("3", info.Version, "degree.pt: 归档版本应为 3（新式 zip 序列化）")
            Call DebugAudit.AreEqual("degree", info.ArchiveName, "degree.pt: 归档前缀应为 'degree'")
            Call DebugAudit.Test(info.DataPklSize > 0, "degree.pt: data.pkl 不应为空")

            Dim state As PtStateDict = PtFile.LoadStateDict(DegreeFile)

            Call DebugAudit.AreEqual(3, state.Count, "degree.pt: 应当读出 3 个张量")
            Call DebugAudit.Test(info.StorageCount >= state.Count,
                                 $"degree.pt: 存储块个数（{info.StorageCount}）不应少于张量个数（{state.Count}）")

            For Each expected As String In New String() {"ligand_deg", "clique_deg", "protein_deg"}
                Dim entry As PtTensorEntry

                If Not state.TryGetEntry(expected, entry) Then
                    Call DebugAudit.Test(False, $"degree.pt: 缺少权重 '{expected}'")
                    Continue For
                End If

                Call DebugAudit.AreEqual(PtElementType.Int64, entry.ElementType,
                                         $"degree.pt: '{expected}' 的元素类型应为 torch.int64")
                ' degree_dict 保存的是普通张量（不是 nn.Parameter），因此 requires_grad 为 False
                Call DebugAudit.Test(Not entry.RequiresGrad,
                                     $"degree.pt: '{expected}' 的 requires_grad 应为 False（保存的是普通张量）")
                Call DebugAudit.Test(entry.Value.Length > 0, $"degree.pt: '{expected}' 不应为空张量")
                Call DebugAudit.Test(entry.Value.Data.All(AddressOf IsFiniteValue),
                                     $"degree.pt: '{expected}' 的数值必须全部有限")

                Call Console.WriteLine($"  [ok] {expected}: [{String.Join(", ", entry.Shape)}] int64, " &
                                      $"sum={entry.Value.Data.Sum():G6}")
            Next

            Call TestRoundtrip(DegreeFile, "degree.pt")
        Catch ex As Exception
            Call ReportException("degree.pt 读取", ex)
        End Try
    End Sub

#End Region

#Region "真实模型：model_KcatNet.pt"

    Private Sub TestModelFile()
        Call Console.WriteLine($"-- KcatNet model_KcatNet.pt: {ModelFile}")

        If Not File.Exists(ModelFile) Then
            Call DebugAudit.Test(False, $"测试数据缺失：{ModelFile}")
            Call Console.WriteLine("  [fail] 文件不存在")
            Return
        End If

        Try
            Dim info As PtArchiveInfo = PtFile.ReadInfo(ModelFile)

            Call DebugAudit.AreEqual("3", info.Version, "model_KcatNet.pt: 归档版本应为 3")
            Call DebugAudit.Test(info.StorageCount > 100,
                                 $"model_KcatNet.pt: 存储块个数应当大于 100，实际 {info.StorageCount}")

            Dim state As PtStateDict = PtFile.LoadStateDict(ModelFile)
            Dim totalValues As Long = state.Entries.Sum(Function(e) CLng(e.Value.Length))
            Dim badShape As Integer = 0
            Dim notFinite As Integer = 0
            Dim notFloat As Integer = 0

            For Each entry As PtTensorEntry In state.Entries
                Dim expected As Integer = 1

                For Each size As Integer In entry.Shape
                    expected *= size
                Next

                If expected <> entry.Value.Length Then
                    badShape += 1
                End If

                If Not entry.Value.Data.All(AddressOf IsFiniteValue) Then
                    notFinite += 1
                End If

                If entry.ElementType <> PtElementType.Float32 Then
                    notFloat += 1
                End If
            Next

            Call DebugAudit.Test(state.Count > 100, $"model_KcatNet.pt: 张量个数应当大于 100，实际 {state.Count}")
            Call DebugAudit.AreEqual(0, badShape, "model_KcatNet.pt: 每个张量的元素数必须与形状一致")
            Call DebugAudit.AreEqual(0, notFinite, "model_KcatNet.pt: 权重必须全部是有限值")
            Call DebugAudit.AreEqual(0, notFloat, "model_KcatNet.pt: 权重元素类型应全部为 torch.float32")
            Call DebugAudit.Test(totalValues > 1000000,
                                 $"model_KcatNet.pt: 权重总数应当超过 1e6，实际 {totalValues}")

            Call DebugAudit.Test(state.ContainsKey("atom_type_encoder.weight"),
                                 "model_KcatNet.pt: 应当包含 'atom_type_encoder.weight'")

            Call DebugAudit.Mark("[pt] model_KcatNet.pt", $"{state.Count} tensors, {totalValues} values")

            Call Console.WriteLine($"  [ok] {state.Count} 个张量, {totalValues} 个权重, 存储块 {info.StorageCount}")

            Call TestRoundtrip(ModelFile, "model_KcatNet.pt")
        Catch ex As Exception
            Call ReportException("model_KcatNet.pt 读取", ex)
        End Try
    End Sub

#End Region

#Region "往返"

    ''' <summary>
    ''' 读原始 pt → 写出新 pt → 再读回，逐张量比对名称、形状、元素类型与数值。
    ''' </summary>
    Private Sub TestRoundtrip(source As String, label As String)
        Dim target As String = TempFile("scibasic_pt_roundtrip_" & IO.Path.GetFileNameWithoutExtension(source))
        Dim original As PtStateDict = PtFile.LoadStateDict(source)

        Try
            Call PtFile.Save(target, original)
            Call CheckArchiveLayout(target, IO.Path.GetFileNameWithoutExtension(target), expectedStorages:=original.Count)

            Dim restored As PtStateDict = PtFile.LoadStateDict(target)

            Call DebugAudit.AreEqual(original.Count, restored.Count, $"{label}: 往返后张量个数应当一致")
            Call DebugAudit.Test(original.Names.SequenceEqual(restored.Names),
                                 $"{label}: 往返后张量名称的顺序应当一致")

            Dim mismatch As Integer = 0
            Dim maxDiff As Double = 0.0
            Dim typeMismatch As Integer = 0

            For Each entry As PtTensorEntry In original.Entries
                Dim other As PtTensorEntry

                If Not restored.TryGetEntry(entry.Name, other) Then
                    mismatch += 1
                    Continue For
                End If

                If Not entry.Shape.SequenceEqual(other.Shape) Then
                    mismatch += 1
                End If

                If entry.ElementType <> other.ElementType Then
                    typeMismatch += 1
                End If

                maxDiff = std.Max(maxDiff, MaxAbsDifference(entry.Value.Data, other.Value.Data))
            Next

            Call DebugAudit.AreEqual(0, mismatch, $"{label}: 往返后形状必须完全一致（不一致 {mismatch} 项）")
            Call DebugAudit.AreEqual(0, typeMismatch, $"{label}: 往返后元素类型必须保持（不一致 {typeMismatch} 项）")
            Call DebugAudit.AreEqual(0.0, maxDiff, $"{label}: 往返后数值必须完全一致（max|Δ| = {maxDiff:G3}）")

            Call DebugAudit.Mark($"[pt] roundtrip {label}", $"{restored.Count} tensors, max|Δ|={maxDiff:G3}")

            Call Console.WriteLine($"  [ok] 往返 {label}: {restored.Count} 个张量, max|Δ|={maxDiff:G3}")

            ' 独立实现（CPython pickle）复核写出的归档是否符合 PyTorch 的持久化 ID 协议
            Call ValidateWithPython(target, label)
        Catch ex As Exception
            Call ReportException($"{label} 往返", ex)
        Finally
            Call Remove(target)
        End Try
    End Sub

    ''' <summary>
    ''' 检查写出的归档结构：zip 条目命名、version 条目、data.pkl 中的 PyTorch 记号。
    ''' </summary>
    Private Sub CheckArchiveLayout(path As String, expectedArchiveName As String, expectedStorages As Integer)
        Using stream As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
            Using zip As New ZipArchive(stream, ZipArchiveMode.Read)
                Dim names As New HashSet(Of String)(zip.Entries.Select(Function(e) e.FullName), StringComparer.Ordinal)

                Call DebugAudit.Test(names.Contains($"{expectedArchiveName}/data.pkl"),
                                     $"写出的归档应当包含 '{expectedArchiveName}/data.pkl'")
                Call DebugAudit.Test(names.Contains($"{expectedArchiveName}/version"),
                                     $"写出的归档应当包含 '{expectedArchiveName}/version'")

                Dim storages As Integer = zip.Entries.Where(
                    Function(e) e.FullName.StartsWith($"{expectedArchiveName}/data/", StringComparison.Ordinal)).Count()

                Call DebugAudit.AreEqual(expectedStorages, storages,
                                         "写出的存储块个数应当与张量个数一致（每个张量独占一块存储）")

                Dim versionEntry As ZipArchiveEntry = zip.GetEntry($"{expectedArchiveName}/version")

                Using reader As New StreamReader(versionEntry.Open(), Encoding.ASCII)
                    Call DebugAudit.AreEqual("3", reader.ReadToEnd().Trim(), "写出的 version 条目内容应当为 '3'")
                End Using

                Dim pickleEntry As ZipArchiveEntry = zip.GetEntry($"{expectedArchiveName}/data.pkl")

                Using source As Stream = pickleEntry.Open()
                    Using buffer As New MemoryStream()
                        Call source.CopyTo(buffer)

                        Dim text As String = Encoding.ASCII.GetString(buffer.ToArray())

                        Call DebugAudit.Test(text.Contains("torch._utils"), "data.pkl 应当引用 torch._utils 模块")
                        Call DebugAudit.Test(text.Contains("_rebuild_tensor_v2"), "data.pkl 应当使用 _rebuild_tensor_v2 重建张量")
                        Call DebugAudit.Test(text.Contains("storage"), "data.pkl 应当包含存储持久化 ID")
                        Call DebugAudit.Test(text.Contains("collections") AndAlso text.Contains("OrderedDict"),
                                             "data.pkl 的根对象应当是 collections.OrderedDict")
                        Dim typeTokens As String() = {
                            "float32", "float64", "float16", "bfloat16",
                            "int64", "int32", "int16", "int8", "uint8", "bool",
                            "FloatStorage", "DoubleStorage", "LongStorage", "ByteStorage"
                        }

                        Call DebugAudit.Test(typeTokens.Any(Function(t) text.Contains(t)),
                                             "data.pkl 应当包含 torch 元素类型记号")
                    End Using
                End Using
            End Using
        End Using
    End Sub

#End Region

#Region "异常输入"

    Private Sub TestInvalidInputs()
        Call Console.WriteLine("-- 异常输入")

        Dim missing As Boolean = False

        Try
            Call PtFile.LoadStateDict(IO.Path.Combine(IO.Path.GetTempPath(), Guid.NewGuid().ToString("N") & ".pt"))
        Catch ex As FileNotFoundException
            missing = True
        End Try

        Call DebugAudit.Test(missing, "不存在的文件应当抛出 FileNotFoundException")

        Dim notZip As String = TempFile("scibasic_pt_notzip")
        Dim notZipReported As Boolean = False

        Try
            Call File.WriteAllText(notZip, "this is not a pt archive")

            Try
                Call PtFile.LoadStateDict(notZip)
            Catch ex As InvalidDataException
                notZipReported = True
            End Try
        Finally
            Call Remove(notZip)
        End Try

        Call DebugAudit.Test(notZipReported, "非 zip 文件应当抛出 InvalidDataException")

        Dim noPickle As String = TempFile("scibasic_pt_nopickle")
        Dim noPickleReported As Boolean = False

        Try
            Using stream As New FileStream(noPickle, FileMode.Create, FileAccess.Write)
                Using zip As New ZipArchive(stream, ZipArchiveMode.Create)
                    Dim entry As ZipArchiveEntry = zip.CreateEntry("archive/data/0", CompressionLevel.NoCompression)

                    Using target As Stream = entry.Open()
                        Call target.Write(New Byte() {1, 2, 3, 4}, 0, 4)
                    End Using
                End Using
            End Using

            Try
                Call PtFile.LoadStateDict(noPickle)
            Catch ex As InvalidDataException
                noPickleReported = True
            End Try
        Finally
            Call Remove(noPickle)
        End Try

        Call DebugAudit.Test(noPickleReported, "缺少 data.pkl 的 zip 应当抛出 InvalidDataException")
    End Sub

#End Region

#Region "辅助"

    ''' <summary>
    ''' CPython 侧的独立校验脚本：模拟 <c>torch.load</c> 的 <c>persistent_load</c> 协议，
    ''' 逐项检查 <c>data.pkl</c> 的全局符号、持久化 ID 元组与存储块长度。
    ''' </summary>
    Private ReadOnly PythonValidator As String = String.Join(vbLf, New String() {
        "import sys, zipfile, pickle, io, collections",
        "",
        "ITEMS = {",
        "    'torch.float32': 4, 'torch.float64': 8, 'torch.float16': 2, 'torch.bfloat16': 2,",
        "    'torch.int64': 8, 'torch.int32': 4, 'torch.int16': 2, 'torch.int8': 1,",
        "    'torch.uint8': 1, 'torch.bool': 1,",
        "    'torch.FloatStorage': 4, 'torch.DoubleStorage': 8, 'torch.HalfStorage': 2,",
        "    'torch.BFloat16Storage': 2, 'torch.LongStorage': 8, 'torch.IntStorage': 4,",
        "    'torch.ShortStorage': 2, 'torch.CharStorage': 1, 'torch.ByteStorage': 1,",
        "    'torch.BoolStorage': 1,",
        "}",
        "",
        "def rebuild(*args):",
        "    return {'tensor': args}",
        "",
        "ALLOWED = {",
        "    'collections.OrderedDict': collections.OrderedDict,",
        "    'torch._utils._rebuild_tensor_v2': rebuild,",
        "    'torch._utils._rebuild_tensor': rebuild,",
        "}",
        "",
        "class Loader(pickle.Unpickler):",
        "    def find_class(self, module, name):",
        "        key = module + '.' + name",
        "        if key in ALLOWED:",
        "            return ALLOWED[key]",
        "        if module == 'torch':",
        "            return key",
        "        raise pickle.UnpicklingError('unexpected global: ' + key)",
        "",
        "    def persistent_load(self, pid):",
        "        return ('storage-ref',) + tuple(pid)",
        "",
        "path = sys.argv[1]",
        "zf = zipfile.ZipFile(path)",
        "names = zf.namelist()",
        "records = [n for n in names if n.endswith('/data.pkl') or n == 'data.pkl']",
        "if not records:",
        "    raise SystemExit('missing data.pkl')",
        "pkl = records[0]",
        "prefix = pkl[:-len('data.pkl')]",
        "root = Loader(io.BytesIO(zf.read(pkl))).load()",
        "",
        "count = 0",
        "for name, value in root.items():",
        "    if not isinstance(value, dict) or 'tensor' not in value:",
        "        raise SystemExit('not a rebuilt tensor: ' + str(name))",
        "    args = value['tensor']",
        "    if len(args) != 6:",
        "        raise SystemExit('bad tensor argument count: ' + str(len(args)))",
        "    ref, offset, size, stride, requires_grad, hooks = args",
        "    if not (isinstance(ref, tuple) and len(ref) == 6 and ref[0] == 'storage-ref' and ref[1] == 'storage'):",
        "        raise SystemExit('bad persistent id for ' + str(name) + ': ' + repr(ref))",
        "    dtype, key, location, numel = ref[2], ref[3], ref[4], ref[5]",
        "    if dtype not in ITEMS:",
        "        raise SystemExit('unknown dtype: ' + str(dtype))",
        "    record = prefix + 'data/' + str(key)",
        "    if record not in names:",
        "        raise SystemExit('missing storage record: ' + record)",
        "    length = len(zf.read(record))",
        "    if length != numel * ITEMS[dtype]:",
        "        raise SystemExit('storage size mismatch: %s (%d != %d)' % (record, length, numel * ITEMS[dtype]))",
        "    if not isinstance(size, tuple) or not isinstance(stride, tuple) or len(size) != len(stride):",
        "        raise SystemExit('bad size/stride for ' + str(name))",
        "    if not isinstance(requires_grad, bool):",
        "        raise SystemExit('bad requires_grad for ' + str(name))",
        "    count += 1",
        "",
        "print('OK %d' % count)"
    })

    ''' <summary>
    ''' 用 CPython 的 pickle 独立复核写出的归档（模拟 PyTorch 的持久化 ID 协议）。
    ''' </summary>
    ''' <remarks>
    ''' 本机没有 PyTorch，无法直接用 <c>torch.load</c> 验证；这个脚本用标准库复现了
    ''' <c>torch.serialization.Unpickler</c> 的 <c>find_class</c> / <c>persistent_load</c> 行为，
    ''' 从而证明写出的 <c>data.pkl</c> 是合法的 Python pickle、且存储引用符合
    ''' <c>('storage', dtype, key, location, numel)</c> 协议。找不到 python 时记为跳过而不是失败。
    ''' </remarks>
    Private Sub ValidateWithPython(path As String, label As String)
        Dim script As String = IO.Path.Combine(IO.Path.GetTempPath(), "scibasic_pt_validate.py")

        Try
            Call File.WriteAllText(script, PythonValidator, Encoding.ASCII)
        Catch ex As IOException
            Call DebugAudit.Mark($"[pt] python 校验 {label}", "跳过：无法写入校验脚本 " & ex.Message)
            Return
        End Try

        Dim start As New ProcessStartInfo("python") With {
            .RedirectStandardOutput = True,
            .RedirectStandardError = True,
            .UseShellExecute = False,
            .CreateNoWindow = True
        }

        Call start.ArgumentList.Add(script)
        Call start.ArgumentList.Add(path)

        Try
            Using process As Process = Process.Start(start)
                Dim stdout As String = process.StandardOutput.ReadToEnd()
                Dim stderr As String = process.StandardError.ReadToEnd()

                Call process.WaitForExit()

                If process.ExitCode = 0 AndAlso stdout.Contains("OK ") Then
                    Call Console.WriteLine($"  [ok] CPython 结构校验 {label}: {stdout.Trim()}")
                    Call DebugAudit.Test(True, $"{label}: CPython pickle 复现 PyTorch 归档结构成功（{stdout.Trim()}）")
                Else
                    Call Console.WriteLine($"  [fail] CPython 结构校验 {label}: {stderr.Trim()}")
                    Call DebugAudit.Test(False,
                        $"{label}: CPython 结构校验失败（exit={process.ExitCode}）：{stderr.Trim()} {stdout.Trim()}")
                End If
            End Using
        Catch ex As System.ComponentModel.Win32Exception
            Call Console.WriteLine($"  [skip] CPython 结构校验 {label}: 未找到 python 解释器")
            Call DebugAudit.Mark($"[pt] python 校验 {label}", "跳过：未找到 python 解释器")
        Finally
            Call Remove(script)
        End Try
    End Sub

    ''' <summary>记录并打印异常（含调用栈，便于定位归档解析问题）。</summary>
    Private Sub ReportException(label As String, ex As Exception)
        Call DebugAudit.Test(False, $"{label} 抛出异常 {ex.GetType().Name}: {ex.Message}")
        Call Console.WriteLine($"  [fail] {label}: {ex.Message}")
        Call Console.WriteLine(ex.ToString())
    End Sub

    Private Function TempFile(name As String) As String
        Return IO.Path.Combine(IO.Path.GetTempPath(), $"{name}_{Guid.NewGuid():N}.pt")
    End Function

    Private Sub Remove(path As String)
        Try
            If File.Exists(path) Then
                Call File.Delete(path)
            End If
        Catch ex As IOException
            ' 临时文件清理失败不影响测试结论
        End Try
    End Sub

    Private Function MaxAbsDifference(a As Double(), b As Double()) As Double
        If a Is Nothing OrElse b Is Nothing OrElse a.Length <> b.Length Then
            Return Double.PositiveInfinity
        End If

        Dim max As Double = 0.0

        For i As Integer = 0 To a.Length - 1
            Dim diff As Double = std.Abs(a(i) - b(i))

            If diff > max Then
                max = diff
            End If
        Next

        Return max
    End Function

    Private Function IsFiniteValue(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

#End Region

End Module
