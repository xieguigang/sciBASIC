' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\ONNX\OnnxModel.vb
' /********************************************************************************/

Imports System.IO
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.Protobuf
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX.Proto

Namespace ONNX

    ''' <summary>
    ''' 一个 ONNX 模型文件的对象视图：负责文件读写、初始化器解析、形状查询与结构摘要。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 本类型只关心"模型里有什么"，不负责推理（见 <see cref="OnnxGraphEvaluator"/>）也不负责与
    ''' 卷积网络之间转换（见 <see cref="OnnxExport"/> / <see cref="OnnxImport"/>）。
    ''' </para>
    ''' <para>
    ''' 序列化依赖 <c>Microsoft.VisualBasic.Data.IO.Protobuf.ProtoSerializer</c>，因此整个实现没有任何
    ''' 第三方依赖。
    ''' </para>
    ''' </remarks>
    Public Class OnnxModel

        Private _initializers As Dictionary(Of String, OnnxTensor)

        ''' <summary>底层的 protobuf 消息。</summary>
        Public ReadOnly Property Proto As ModelProto

        ''' <summary>模型文件的路径；由 <see cref="FromStream"/> 构造时为 Nothing。</summary>
        Public ReadOnly Property FilePath As String

        ''' <summary>计算图。</summary>
        Public ReadOnly Property Graph As GraphProto
            Get
                Return Proto.Graph
            End Get
        End Property

        ''' <summary>IR 版本号。</summary>
        Public ReadOnly Property IrVersion As Long
            Get
                Return Proto.IrVersion
            End Get
        End Property

        ''' <summary>生成该模型的工具名称。</summary>
        Public ReadOnly Property ProducerName As String
            Get
                Return Proto.ProducerName
            End Get
        End Property

        ''' <summary>生成该模型的工具版本。</summary>
        Public ReadOnly Property ProducerVersion As String
            Get
                Return Proto.ProducerVersion
            End Get
        End Property

        ''' <summary>默认域（ONNX 标准域）的算子集版本。</summary>
        Public ReadOnly Property OpsetVersion As Long
            Get
                Return Proto.GetOpsetVersion()
            End Get
        End Property

        ''' <summary>模型级元数据。</summary>
        Public ReadOnly Property Metadata As Dictionary(Of String, String)
            Get
                If Proto.MetadataProps Is Nothing Then
                    Proto.MetadataProps = New Dictionary(Of String, String)
                End If

                Return Proto.MetadataProps
            End Get
        End Property

        ''' <summary>
        ''' 初始化器（常量权重张量）的字典视图；键为张量名称。
        ''' </summary>
        Public ReadOnly Property Initializers As Dictionary(Of String, OnnxTensor)
            Get
                If _initializers Is Nothing Then
                    _initializers = LoadInitializers()
                End If

                Return _initializers
            End Get
        End Property

        ''' <summary>图输入中排除初始化器之后的"真实输入"。</summary>
        Public ReadOnly Property Inputs As ValueInfoProto()
            Get
                If Graph Is Nothing OrElse Graph.Input Is Nothing Then
                    Return New ValueInfoProto() {}
                End If

                Return Graph.Input _
                    .Where(Function(v) Graph.GetInitializer(v.Name) Is Nothing) _
                    .ToArray()
            End Get
        End Property

        ''' <summary>图输出。</summary>
        Public ReadOnly Property Outputs As ValueInfoProto()
            Get
                If Graph Is Nothing OrElse Graph.Output Is Nothing Then
                    Return New ValueInfoProto() {}
                End If

                Return Graph.Output.ToArray()
            End Get
        End Property

        ''' <summary>用已有的 protobuf 消息创建模型对象。</summary>
        ''' <param name="proto">模型消息。</param>
        ''' <param name="path">可选的模型文件路径（仅用于诊断信息）。</param>
        Public Sub New(proto As ModelProto, Optional path As String = Nothing)
            If proto Is Nothing Then
                Throw New ArgumentNullException(NameOf(proto))
            End If

            Me.Proto = proto
            Me.FilePath = path
        End Sub

        Private Function LoadInitializers() As Dictionary(Of String, OnnxTensor)
            Dim result As New Dictionary(Of String, OnnxTensor)(StringComparer.Ordinal)

            If Graph Is Nothing OrElse Graph.Initializer Is Nothing Then
                Return result
            End If

            For Each tensor As TensorProto In Graph.Initializer
                If tensor Is Nothing OrElse String.IsNullOrEmpty(tensor.Name) Then
                    Continue For
                End If

                result(tensor.Name) = OnnxTensor.FromTensorProto(tensor)
            Next

            Return result
        End Function

        ''' <summary>
        ''' 取得指定名称的初始化器。
        ''' </summary>
        ''' <param name="name">张量名称。</param>
        ''' <returns>运行时张量；不存在时抛出 <see cref="KeyNotFoundException"/>。</returns>
        Public Function GetInitializer(name As String) As OnnxTensor
            Dim tensor As OnnxTensor = Nothing

            If Initializers.TryGetValue(name, tensor) Then
                Return tensor
            Else
                Throw New KeyNotFoundException($"模型中没有名为 ""{name}"" 的初始化器（常量张量）")
            End If
        End Function

        ''' <summary>
        ''' 判断模型中是否存在指定名称的初始化器。
        ''' </summary>
        ''' <param name="name">张量名称。</param>
        ''' <returns>存在时返回 True。</returns>
        Public Function HasInitializer(name As String) As Boolean
            If String.IsNullOrEmpty(name) Then
                Return False
            End If

            Return Initializers.ContainsKey(name)
        End Function

        ''' <summary>图中出现过的算子类型（去重并保持出现顺序）。</summary>
        ''' <returns>算子类型名称。</returns>
        Public Function GetOpTypes() As String()
            If Graph Is Nothing Then
                Return New String() {}
            End If

            Return Graph.GetOpTypes()
        End Function

        ''' <summary>
        ''' 从文件加载 ONNX 模型。
        ''' </summary>
        ''' <param name="path">ONNX 文件路径。</param>
        ''' <returns>模型对象。</returns>
        Public Shared Function Load(path As String) As OnnxModel
            If String.IsNullOrEmpty(path) Then
                Throw New ArgumentNullException(NameOf(path))
            End If

            If Not File.Exists(path) Then
                Throw New FileNotFoundException($"找不到 ONNX 模型文件：{path}", path)
            End If

            Using stream As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
                Return FromStream(stream, leaveOpen:=True, path:=path)
            End Using
        End Function

        ''' <summary>
        ''' 从数据流加载 ONNX 模型。
        ''' </summary>
        ''' <param name="stream">包含模型字节的数据流（必须可定位或完整保留 protobuf 消息）。</param>
        ''' <param name="leaveOpen">读取完成后是否保持数据流打开。</param>
        ''' <param name="path">可选的来源路径，仅用于诊断信息。</param>
        ''' <returns>模型对象。</returns>
        Public Shared Function FromStream(stream As Stream, Optional leaveOpen As Boolean = True, Optional path As String = Nothing) As OnnxModel
            If stream Is Nothing Then
                Throw New ArgumentNullException(NameOf(stream))
            End If

            Dim proto As ModelProto = ProtoSerializer.Deserialize(Of ModelProto)(stream, leaveOpen)

            If proto.Graph Is Nothing Then
                Throw New InvalidDataException($"ONNX 模型 {If(path, "<stream>")} 中没有计算图（graph 字段缺失）")
            End If

            Return New OnnxModel(proto, path)
        End Function

        ''' <summary>
        ''' 从 protobuf 字节数组加载 ONNX 模型。
        ''' </summary>
        ''' <param name="data">模型字节。</param>
        ''' <param name="path">可选的来源路径，仅用于诊断信息。</param>
        ''' <returns>模型对象。</returns>
        Public Shared Function FromProtobuf(data As Byte(), Optional path As String = Nothing) As OnnxModel
            If data Is Nothing Then
                Throw New ArgumentNullException(NameOf(data))
            End If

            Using stream As New MemoryStream(data, writable:=False)
                Return FromStream(stream, leaveOpen:=False, path:=path)
            End Using
        End Function

        ''' <summary>
        ''' 把模型序列化为 protobuf 字节数组。
        ''' </summary>
        ''' <returns>序列化后的字节。</returns>
        Public Function ToProtobuf() As Byte()
            Return ProtoSerializer.Serialize(Proto)
        End Function

        ''' <summary>
        ''' 把模型写入数据流。
        ''' </summary>
        ''' <param name="stream">目标数据流。</param>
        ''' <param name="leaveOpen">写完后是否保持数据流打开。</param>
        Public Sub Save(stream As Stream, Optional leaveOpen As Boolean = True)
            If stream Is Nothing Then
                Throw New ArgumentNullException(NameOf(stream))
            End If

            ProtoSerializer.Serialize(Proto, stream, leaveOpen)
        End Sub

        ''' <summary>
        ''' 把模型写入文件（覆盖写入）。
        ''' </summary>
        ''' <param name="path">目标文件路径，通常以 <c>.onnx</c> 结尾。</param>
        Public Sub Save(path As String)
            If String.IsNullOrEmpty(path) Then
                Throw New ArgumentNullException(NameOf(path))
            End If

            Dim dir As String = IO.Path.GetDirectoryName(path)

            If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
                Call Directory.CreateDirectory(dir)
            End If

            Using stream As New FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)
                Call Save(stream, leaveOpen:=True)
            End Using
        End Sub

        ''' <summary>
        ''' 生成模型的结构摘要（元信息、算子序列、输入输出形状、初始化器清单）。
        ''' </summary>
        ''' <returns>多行文本。</returns>
        Public Function Summary() As String
            Dim sb As New StringBuilder()

            Call sb.AppendLine($"ONNX model: {If(FilePath, "<memory>")}")
            Call sb.AppendLine($"  ir_version       : {IrVersion}")
            Call sb.AppendLine($"  opset            : {String.Join(", ", Proto.OpsetImport.Select(Function(o) o.ToString()))}")
            Call sb.AppendLine($"  producer         : {ProducerName} {ProducerVersion}".TrimEnd())
            Call sb.AppendLine($"  graph            : {Graph.Name} ({Graph.Node.Count} nodes)")
            Call sb.AppendLine($"  initializer      : {Graph.Initializer.Count} tensors")

            If Metadata IsNot Nothing AndAlso Metadata.Count > 0 Then
                Call sb.AppendLine($"  metadata         : {String.Join("; ", Metadata.Keys)}")
            End If

            Call sb.AppendLine("  inputs:")

            For Each v As ValueInfoProto In Inputs
                Call sb.AppendLine($"    - {v}")
            Next

            Call sb.AppendLine("  outputs:")

            For Each v As ValueInfoProto In Outputs
                Call sb.AppendLine($"    - {v}")
            Next

            Call sb.AppendLine("  nodes:")

            Dim index As Integer = 0

            For Each n As NodeProto In Graph.Node
                Dim attrs As String = ""

                If n.Attribute IsNot Nothing AndAlso n.Attribute.Count > 0 Then
                    attrs = " " & String.Join(" ", n.Attribute.Select(Function(a) a.ToString()))
                End If

                Call sb.AppendLine($"    [{index}] {n.OpType} {String.Join(",", n.Input)} -> {String.Join(",", n.Output)}{attrs}")
                index += 1
            Next

            Call sb.AppendLine("  initializers:")

            For Each name As String In Initializers.Keys
                Call sb.AppendLine($"    - {Initializers(name)}  ({name})")
            Next

            Return sb.ToString()
        End Function

        ''' <summary>返回模型的单行描述。</summary>
        Public Overrides Function ToString() As String
            Return $"{Path.GetFileName(If(FilePath, "onnx"))}: {Graph.Node.Count} nodes, {Initializers.Count} initializers"
        End Function

    End Class

End Namespace
