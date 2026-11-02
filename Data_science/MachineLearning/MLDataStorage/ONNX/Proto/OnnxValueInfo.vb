' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\ONNX\Proto\OnnxValueInfo.vb
' /********************************************************************************/

Imports System.Linq
Imports Microsoft.VisualBasic.Data.IO.Protobuf.Serialization

Namespace ONNX.Proto

    ''' <summary>
    ''' ONNX <c>OperatorSetIdProto</c>：模型依赖的算子集版本。
    ''' </summary>
    <ProtoContract>
    Public Class OperatorSetIdProto

        ''' <summary>算子集所属的域，空字符串表示 ONNX 默认域。</summary>
        <ProtoMember(1)>
        Public Property Domain As String

        ''' <summary>算子集版本号。</summary>
        <ProtoMember(2)>
        Public Property Version As Long

        ''' <summary>返回 <c>domain=version</c> 形式的文本。</summary>
        Public Overrides Function ToString() As String
            Return $"{If(String.IsNullOrEmpty(Domain), "<default>", Domain)}={Version}"
        End Function

    End Class

    ''' <summary>
    ''' ONNX <c>StringStringEntryProto</c>：键值对（用于 <c>metadata_props</c> 与 <c>external_data</c>）。
    ''' </summary>
    <ProtoContract>
    Public Class StringStringEntryProto

        ''' <summary>键。</summary>
        <ProtoMember(1)>
        Public Property Key As String

        ''' <summary>值。</summary>
        <ProtoMember(2)>
        Public Property Value As String

        ''' <summary>返回 <c>key=value</c> 形式的文本。</summary>
        Public Overrides Function ToString() As String
            Return $"{Key}={Value}"
        End Function

    End Class

    ''' <summary>
    ''' ONNX <c>TensorShapeProto.Dimension</c>：单个维度。
    ''' </summary>
    <ProtoContract>
    Public Class TensorShapeDimension

        ''' <summary>静态维度的长度；0 表示该维度不是静态已知的。</summary>
        ''' <remarks>
        ''' 这里用 0 而不是 -1 作为"未知"的哨兵值，是因为 protobuf 的 proto3 语义会把 0 视为缺省值从而不写出
        ''' 该字段，正好对应 ONNX 中"没有 <c>dim_value</c>"的含义；若用 -1 则会被写成 <c>dim_value = -1</c>。
        ''' </remarks>
        <ProtoMember(1)>
        Public Property DimValue As Long

        ''' <summary>动态维度的符号名（例如 <c>batch</c>）。</summary>
        <ProtoMember(2)>
        Public Property DimParam As String

        ''' <summary>是否为动态维度（既没有静态长度也没有符号名，或者带有符号名）。</summary>
        Public ReadOnly Property IsDynamic As Boolean
            Get
                Return Not String.IsNullOrEmpty(DimParam) OrElse DimValue <= 0
            End Get
        End Property

        ''' <summary>返回维度的可读文本（静态长度或符号名）。</summary>
        Public Overrides Function ToString() As String
            If Not String.IsNullOrEmpty(DimParam) Then
                Return DimParam
            ElseIf DimValue <= 0 Then
                Return "?"
            Else
                Return DimValue.ToString()
            End If
        End Function

    End Class

    ''' <summary>
    ''' ONNX <c>TensorShapeProto</c>：张量的形状。
    ''' </summary>
    <ProtoContract>
    Public Class TensorShapeProto

        ''' <summary>各维度。</summary>
        <ProtoMember(1)>
        Public Property Dims As New List(Of TensorShapeDimension)

        ''' <summary>
        ''' 取得静态维度的长度数组；存在动态维度时该位置为 -1。
        ''' </summary>
        ''' <returns>维度长度数组。</returns>
        Public Function GetDims() As Long()
            Dim result As New List(Of Long)

            For Each item As TensorShapeDimension In Dims
                If item Is Nothing OrElse item.IsDynamic Then
                    Call result.Add(-1)
                Else
                    Call result.Add(item.DimValue)
                End If
            Next

            Return result.ToArray()
        End Function

        ''' <summary>返回 <c>[1, 3, 28, 28]</c> 形式的形状文本。</summary>
        Public Overrides Function ToString() As String
            Return "[" & String.Join(", ", Dims.Select(Function(item) item.ToString())) & "]"
        End Function

    End Class

    ''' <summary>
    ''' ONNX <c>TypeProto.Tensor</c>：张量类型描述（元素类型 + 形状）。
    ''' </summary>
    <ProtoContract>
    Public Class TensorTypeInfo

        ''' <summary>元素数据类型。</summary>
        <ProtoMember(1)>
        Public Property ElemType As TensorDataType

        ''' <summary>形状；可能为空（未知形状）。</summary>
        <ProtoMember(2)>
        Public Property Shape As TensorShapeProto

        ''' <summary>返回 <c>FLOAT[1, 3, 28, 28]</c> 形式的文本。</summary>
        Public Overrides Function ToString() As String
            If Shape Is Nothing Then
                Return ElemType.ToString()
            Else
                Return $"{ElemType}{Shape}"
            End If
        End Function

    End Class

    ''' <summary>
    ''' ONNX <c>TypeProto</c>：值类型描述。
    ''' </summary>
    ''' <remarks>
    ''' 目前只描述张量类型（<c>tensor_type</c>）；序列/映射等其它 <c>oneof value</c> 分支在读取时被忽略，
    ''' 这不影响网络权重的解析。
    ''' </remarks>
    <ProtoContract>
    Public Class TypeProto

        ''' <summary>张量类型信息。</summary>
        <ProtoMember(1)>
        Public Property TensorType As TensorTypeInfo

        ''' <summary>类型标注（例如 <c>IMAGE</c>）。</summary>
        <ProtoMember(6)>
        Public Property Denotation As String

        ''' <summary>返回类型描述文本。</summary>
        Public Overrides Function ToString() As String
            If TensorType Is Nothing Then
                Return "unknown"
            Else
                Return TensorType.ToString()
            End If
        End Function

    End Class

    ''' <summary>
    ''' ONNX <c>ValueInfoProto</c>：图输入 / 输出 / 中间值的描述。
    ''' </summary>
    <ProtoContract>
    Public Class ValueInfoProto

        ''' <summary>值（张量）名称。</summary>
        <ProtoMember(1)>
        Public Property Name As String

        ''' <summary>类型描述。</summary>
        <ProtoMember(2)>
        Public Property Type As TypeProto

        ''' <summary>说明文本。</summary>
        <ProtoMember(3)>
        Public Property DocString As String

        ''' <summary>
        ''' 取得静态形状；无法确定时返回空数组。
        ''' </summary>
        ''' <returns>维度长度数组。</returns>
        Public Function GetDims() As Long()
            If Type Is Nothing OrElse Type.TensorType Is Nothing OrElse Type.TensorType.Shape Is Nothing Then
                Return New Long() {}
            End If

            Return Type.TensorType.Shape.GetDims()
        End Function

        ''' <summary>返回 <c>name: TYPE[shape]</c> 形式的文本。</summary>
        Public Overrides Function ToString() As String
            Return $"{Name}: {If(Type Is Nothing, "unknown", Type.ToString())}"
        End Function

    End Class

End Namespace
