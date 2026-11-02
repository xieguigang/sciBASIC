' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\ONNX\Proto\OnnxTensorProto.vb
' /********************************************************************************/

Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.Protobuf.Serialization

Namespace ONNX.Proto

    ''' <summary>
    ''' ONNX <c>TensorProto</c>：模型中的多维数组常量（初始化器 / 权重）。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 数据可以有两种落盘方式：<c>raw_data</c>（一段连续的、小端字节序的原始字节）或者按类型分开的
    ''' typed data（<c>float_data</c> / <c>double_data</c> / <c>int32_data</c> / <c>int64_data</c> 等）。
    ''' 这两种方式在本类型上都保留原样，由 <see cref="OnnxTensor"/> 统一转换到 double 数值视图。
    ''' </para>
    ''' <para>
    ''' <c>segment</c>（已废弃）与 <c>sparse</c> 相关字段没有定义，读取时会被当作未知字段跳过。
    ''' </para>
    ''' </remarks>
    <ProtoContract>
    Public Class TensorProto

        ''' <summary>各维度的长度。</summary>
        <ProtoMember(1)>
        Public Property Dims As New List(Of Long)

        ''' <summary>元素数据类型。</summary>
        <ProtoMember(2)>
        Public Property DataType As TensorDataType

        ''' <summary>单精度浮点数据（当 <see cref="DataType"/> 为 <see cref="TensorDataType.Float"/> 时可用）。</summary>
        <ProtoMember(4)>
        Public Property FloatData As New List(Of Single)

        ''' <summary>32 位整数数据（int32 / uint8 / int8 / uint16 / int16 / bool 使用）。</summary>
        <ProtoMember(5)>
        Public Property Int32Data As New List(Of Integer)

        ''' <summary>字符串数据（每个元素是一段 UTF-8 字节）。</summary>
        <ProtoMember(6)>
        Public Property StringData As New List(Of Byte())

        ''' <summary>64 位整数数据（int64 使用）。</summary>
        <ProtoMember(7)>
        Public Property Int64Data As New List(Of Long)

        ''' <summary>张量名称（初始化器名称）。</summary>
        <ProtoMember(8)>
        Public Property Name As String

        ''' <summary>原始字节数据；小端字节序，长度应等于元素个数乘以元素字节长度。</summary>
        <ProtoMember(9)>
        Public Property RawData As Byte()

        ''' <summary>双精度浮点数据（当 <see cref="DataType"/> 为 <see cref="TensorDataType.Float64"/> 时可用）。</summary>
        <ProtoMember(10)>
        Public Property DoubleData As New List(Of Double)

        ''' <summary>64 位无符号整数数据（uint64 使用）。</summary>
        <ProtoMember(11)>
        Public Property UInt64Data As New List(Of Long)

        ''' <summary>说明文本。</summary>
        <ProtoMember(12)>
        Public Property DocString As String

        ''' <summary>外部数据描述（当 <see cref="DataLocation"/> 为 <see cref="Proto.DataLocation.External"/> 时使用）。</summary>
        <ProtoMember(13)>
        Public Property ExternalData As New List(Of StringStringEntryProto)

        ''' <summary>数据存放位置。</summary>
        <ProtoMember(14)>
        Public Property DataLocation As DataLocation

        ''' <summary>
        ''' 取得静态的形状数组（不会包含动态维度）。
        ''' </summary>
        ''' <returns>维度长度数组。</returns>
        Public Function GetShape() As Integer()
            Return Dims.Select(Function(d) CInt(d)).ToArray()
        End Function

        ''' <summary>
        ''' 取得元素总个数；维度未知时返回 -1。
        ''' </summary>
        Public ReadOnly Property ElementCount As Long
            Get
                If Dims Is Nothing OrElse Dims.Count = 0 Then
                    Return 1
                End If

                Dim n As Long = 1

                For Each d As Long In Dims
                    If d < 0 Then
                        Return -1
                    End If

                    n *= d
                Next

                Return n
            End Get
        End Property

        ''' <summary>返回 <c>name: FLOAT[2, 3]</c> 形式的文本。</summary>
        Public Overrides Function ToString() As String
            Return $"{Name}: {DataType}[{String.Join(", ", Dims)}]"
        End Function

    End Class

    ''' <summary>
    ''' ONNX <c>AttributeProto</c>：算子的属性。
    ''' </summary>
    ''' <remarks>
    ''' 同一时刻只有与 <see cref="Type"/> 对应的那一个（或一组）值字段有意义，这与 ONNX 的 <c>oneof value</c> 一致。
    ''' </remarks>
    <ProtoContract>
    Public Class AttributeProto

        ''' <summary>属性名称（例如 <c>kernel_shape</c>、<c>transB</c>）。</summary>
        <ProtoMember(1)>
        Public Property Name As String

        ''' <summary>单精度浮点值。</summary>
        <ProtoMember(2)>
        Public Property F As Single

        ''' <summary>整数值。</summary>
        <ProtoMember(3)>
        Public Property I As Long

        ''' <summary>字符串/字节串值（ONNX 中的 <c>s</c>）。</summary>
        <ProtoMember(4)>
        Public Property S As Byte()

        ''' <summary>张量值。</summary>
        <ProtoMember(5)>
        Public Property T As TensorProto

        ''' <summary>子图值。</summary>
        <ProtoMember(6)>
        Public Property G As GraphProto

        ''' <summary>浮点数数组。</summary>
        <ProtoMember(7)>
        Public Property Floats As New List(Of Single)

        ''' <summary>整数数组。</summary>
        <ProtoMember(8)>
        Public Property Ints As New List(Of Long)

        ''' <summary>字符串数组。</summary>
        <ProtoMember(9)>
        Public Property Strings As New List(Of Byte())

        ''' <summary>张量数组。</summary>
        <ProtoMember(10)>
        Public Property Tensors As New List(Of TensorProto)

        ''' <summary>子图数组。</summary>
        <ProtoMember(11)>
        Public Property Graphs As New List(Of GraphProto)

        ''' <summary>说明文本。</summary>
        <ProtoMember(13)>
        Public Property DocString As String

        ''' <summary>属性值的类型。</summary>
        <ProtoMember(20)>
        Public Property Type As AttributeType

        ''' <summary>取得字符串值（UTF-8 解码 <see cref="S"/>）。</summary>
        ''' <returns>字符串值；未设置时返回空字符串。</returns>
        Public Function GetString() As String
            If S Is Nothing OrElse S.Length = 0 Then
                Return ""
            End If

            Return Encoding.UTF8.GetString(S)
        End Function

        ''' <summary>返回属性的可读文本。</summary>
        Public Overrides Function ToString() As String
            Select Case Type
                Case AttributeType.Float
                    Return $"{Name}={F}"
                Case AttributeType.Int
                    Return $"{Name}={I}"
                Case AttributeType.Text
                    Return $"{Name}=""{GetString()}"""
                Case AttributeType.Ints
                    Return $"{Name}=[{String.Join(", ", Ints)}]"
                Case AttributeType.Floats
                    Return $"{Name}=[{String.Join(", ", Floats)}]"
                Case AttributeType.Tensor
                    Return $"{Name}={T}"
                Case Else
                    Return $"{Name}<{Type}>"
            End Select
        End Function

    End Class

End Namespace
