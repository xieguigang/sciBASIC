' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\ONNX\Proto\OnnxGraphProto.vb
' /********************************************************************************/

Imports System.Linq
Imports Microsoft.VisualBasic.Data.IO.Protobuf.Serialization

Namespace ONNX.Proto

    ''' <summary>
    ''' ONNX <c>NodeProto</c>：计算图中的一个算子节点。
    ''' </summary>
    <ProtoContract>
    Public Class NodeProto

        ''' <summary>输入张量名称列表。</summary>
        <ProtoMember(1)>
        Public Property Input As New List(Of String)

        ''' <summary>输出张量名称列表。</summary>
        <ProtoMember(2)>
        Public Property Output As New List(Of String)

        ''' <summary>节点名称（调试用）。</summary>
        <ProtoMember(3)>
        Public Property Name As String

        ''' <summary>算子类型（例如 <c>Gemm</c>、<c>Conv</c>）。</summary>
        <ProtoMember(4)>
        Public Property OpType As String

        ''' <summary>算子属性。</summary>
        <ProtoMember(5)>
        Public Property Attribute As New List(Of AttributeProto)

        ''' <summary>说明文本。</summary>
        <ProtoMember(6)>
        Public Property DocString As String

        ''' <summary>算子所属的域，空字符串表示 ONNX 默认域。</summary>
        <ProtoMember(7)>
        Public Property Domain As String

        ''' <summary>
        ''' 按名称查找属性。
        ''' </summary>
        ''' <param name="name">属性名称（区分大小写，与 ONNX 规范一致）。</param>
        ''' <returns>找到的属性；不存在时返回 Nothing。</returns>
        Public Function GetAttribute(name As String) As AttributeProto
            If Attribute Is Nothing Then
                Return Nothing
            End If

            Return Attribute.FirstOrDefault(Function(a) String.Equals(a.Name, name, StringComparison.Ordinal))
        End Function

        ''' <summary>
        ''' 读取整数属性，缺省时返回 <paramref name="[default]"/>。
        ''' </summary>
        ''' <param name="name">属性名称。</param>
        ''' <param name="[default]">缺省值。</param>
        ''' <returns>属性值。</returns>
        Public Function GetInt(name As String, Optional [default] As Long = 0) As Long
            Dim attr As AttributeProto = GetAttribute(name)

            If attr Is Nothing Then
                Return [default]
            End If

            Return attr.I
        End Function

        ''' <summary>
        ''' 读取浮点属性，缺省时返回 <paramref name="[default]"/>。
        ''' </summary>
        ''' <param name="name">属性名称。</param>
        ''' <param name="[default]">缺省值。</param>
        ''' <returns>属性值。</returns>
        Public Function GetFloat(name As String, Optional [default] As Single = 0) As Single
            Dim attr As AttributeProto = GetAttribute(name)

            If attr Is Nothing Then
                Return [default]
            End If

            Return attr.F
        End Function

        ''' <summary>
        ''' 读取整数数组属性，缺省时返回空数组。
        ''' </summary>
        ''' <param name="name">属性名称。</param>
        ''' <returns>属性值数组。</returns>
        Public Function GetInts(name As String) As Long()
            Dim attr As AttributeProto = GetAttribute(name)

            If attr Is Nothing OrElse attr.Ints Is Nothing Then
                Return New Long() {}
            End If

            Return attr.Ints.ToArray()
        End Function

        ''' <summary>返回 <c>op_type(name)</c> 形式的文本。</summary>
        Public Overrides Function ToString() As String
            Return $"{OpType}({If(String.IsNullOrEmpty(Name), String.Join(",", Input), Name)})"
        End Function

    End Class

    ''' <summary>
    ''' ONNX <c>GraphProto</c>：一个完整的计算图。
    ''' </summary>
    <ProtoContract>
    Public Class GraphProto

        ''' <summary>按拓扑顺序排列的节点。</summary>
        <ProtoMember(1)>
        Public Property Node As New List(Of NodeProto)

        ''' <summary>图名称。</summary>
        <ProtoMember(2)>
        Public Property Name As String

        ''' <summary>初始化器（常量权重张量）。</summary>
        <ProtoMember(5)>
        Public Property Initializer As New List(Of TensorProto)

        ''' <summary>说明文本。</summary>
        <ProtoMember(10)>
        Public Property DocString As String

        ''' <summary>图输入。</summary>
        <ProtoMember(11)>
        Public Property Input As New List(Of ValueInfoProto)

        ''' <summary>图输出。</summary>
        <ProtoMember(12)>
        Public Property Output As New List(Of ValueInfoProto)

        ''' <summary>中间值的类型信息。</summary>
        <ProtoMember(13)>
        Public Property ValueInfo As New List(Of ValueInfoProto)

        ''' <summary>图级元数据。</summary>
        <ProtoMember(16)>
        Public Property MetadataProps As New Dictionary(Of String, String)

        ''' <summary>按名称查找初始化器。</summary>
        ''' <param name="name">初始化器名称。</param>
        ''' <returns>找到的张量；不存在时返回 Nothing。</returns>
        Public Function GetInitializer(name As String) As TensorProto
            If Initializer Is Nothing Then
                Return Nothing
            End If

            Return Initializer.FirstOrDefault(Function(t) String.Equals(t.Name, name, StringComparison.Ordinal))
        End Function

        ''' <summary>返回图中出现过的算子类型（去重并保持出现顺序）。</summary>
        ''' <returns>算子类型名称。</returns>
        Public Function GetOpTypes() As String()
            Dim types As New List(Of String)

            For Each n As NodeProto In Node
                If Not types.Contains(n.OpType) Then
                    Call types.Add(n.OpType)
                End If
            Next

            Return types.ToArray()
        End Function

        ''' <summary>
        ''' 计算每个张量的产出节点，并生成"张量名称 → 产出图节点"的索引。
        ''' </summary>
        ''' <returns>输出张量名称到节点的映射。</returns>
        Public Function BuildProducerIndex() As Dictionary(Of String, NodeProto)
            Dim index As New Dictionary(Of String, NodeProto)(StringComparer.Ordinal)

            For Each n As NodeProto In Node
                For Each name As String In n.Output
                    If Not String.IsNullOrEmpty(name) Then
                        index(name) = n
                    End If
                Next
            Next

            Return index
        End Function

        ''' <summary>返回 <c>graph(name): N nodes</c> 形式的文本。</summary>
        Public Overrides Function ToString() As String
            Return $"graph({Name}): {Node.Count} nodes"
        End Function

    End Class

    ''' <summary>
    ''' ONNX <c>ModelProto</c>：ONNX 文件的根消息。
    ''' </summary>
    <ProtoContract>
    Public Class ModelProto

        ''' <summary>IR 版本号。</summary>
        <ProtoMember(1)>
        Public Property IrVersion As Long

        ''' <summary>生成该模型的工具名称（例如 <c>pytorch</c>）。</summary>
        <ProtoMember(2)>
        Public Property ProducerName As String

        ''' <summary>生成该模型的工具版本。</summary>
        <ProtoMember(3)>
        Public Property ProducerVersion As String

        ''' <summary>模型的域。</summary>
        <ProtoMember(4)>
        Public Property Domain As String

        ''' <summary>模型版本号。</summary>
        <ProtoMember(5)>
        Public Property ModelVersion As Long

        ''' <summary>说明文本。</summary>
        <ProtoMember(6)>
        Public Property DocString As String

        ''' <summary>计算图。</summary>
        <ProtoMember(7)>
        Public Property Graph As GraphProto

        ''' <summary>依赖的算子集版本。</summary>
        <ProtoMember(8)>
        Public Property OpsetImport As New List(Of OperatorSetIdProto)

        ''' <summary>模型级元数据（例如训练时的关节名、观测项名称）。</summary>
        <ProtoMember(14)>
        Public Property MetadataProps As New Dictionary(Of String, String)

        ''' <summary>取得默认域（ONNX 标准域）的算子集版本。</summary>
        ''' <returns>算子集版本；未声明时返回 0。</returns>
        Public Function GetOpsetVersion() As Long
            If OpsetImport Is Nothing Then
                Return 0
            End If

            Dim defaultOPSet = OpsetImport.FirstOrDefault(
                Function(o) String.IsNullOrEmpty(o.Domain))

            If defaultOPSet Is Nothing Then
                Return 0
            Else
                Return defaultOPSet.Version
            End If
        End Function

        ''' <summary>返回模型的单行描述。</summary>
        Public Overrides Function ToString() As String
            Return $"ONNX ir_version={IrVersion} opset={GetOpsetVersion()} producer={ProducerName} {Graph}"
        End Function

    End Class

End Namespace
