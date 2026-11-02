' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\ONNX\Proto\OnnxEnums.vb
' /********************************************************************************/

Namespace ONNX.Proto

    ''' <summary>
    ''' ONNX <c>TensorProto.DataType</c>。
    ''' </summary>
    ''' <remarks>
    ''' 枚举的**数值**是 ONNX 规范的一部分（与 <c>onnx.proto</c> 完全一致），
    ''' 成员名只是为了在 VB 中避免与保留字冲突而做了轻微调整（例如 ONNX 的 <c>STRING</c> 在此处命名为
    ''' <see cref="StringValue"/>，<c>DOUBLE</c> 命名为 <see cref="Float64"/>）。
    ''' </remarks>
    Public Enum TensorDataType As Integer

        ''' <summary>未定义的数据类型。</summary>
        Undefined = 0

        ''' <summary>32 位单精度浮点数（ONNX 名称：FLOAT）。</summary>
        Float = 1

        ''' <summary>8 位无符号整数（ONNX 名称：UINT8）。</summary>
        UInt8 = 2

        ''' <summary>8 位有符号整数（ONNX 名称：INT8）。</summary>
        Int8 = 3

        ''' <summary>16 位无符号整数（ONNX 名称：UINT16）。</summary>
        UInt16 = 4

        ''' <summary>16 位有符号整数（ONNX 名称：INT16）。</summary>
        Int16 = 5

        ''' <summary>32 位有符号整数（ONNX 名称：INT32）。</summary>
        Int32 = 6

        ''' <summary>64 位有符号整数（ONNX 名称：INT64）。</summary>
        Int64 = 7

        ''' <summary>字符串（ONNX 名称：STRING）。</summary>
        StringValue = 8

        ''' <summary>布尔值（ONNX 名称：BOOL）。</summary>
        Bool = 9

        ''' <summary>16 位半精度浮点数（ONNX 名称：FLOAT16）。</summary>
        Float16 = 10

        ''' <summary>64 位双精度浮点数（ONNX 名称：DOUBLE）。</summary>
        Float64 = 11

        ''' <summary>32 位无符号整数（ONNX 名称：UINT32）。</summary>
        UInt32 = 12

        ''' <summary>64 位无符号整数（ONNX 名称：UINT64）。</summary>
        UInt64 = 13

        ''' <summary>64 位复数（ONNX 名称：COMPLEX64）。</summary>
        Complex64 = 14

        ''' <summary>128 位复数（ONNX 名称：COMPLEX128）。</summary>
        Complex128 = 15

        ''' <summary>16 位 brain floating point（ONNX 名称：BFLOAT16）。</summary>
        BFloat16 = 16

    End Enum

    ''' <summary>
    ''' ONNX <c>AttributeProto.AttributeType</c>：算子属性的取值类型。
    ''' </summary>
    Public Enum AttributeType As Integer

        ''' <summary>未定义。</summary>
        Undefined = 0

        ''' <summary>单个浮点数（ONNX 名称：FLOAT）。</summary>
        Float = 1

        ''' <summary>单个整数（ONNX 名称：INT）。</summary>
        Int = 2

        ''' <summary>字符串（ONNX 名称：STRING）。</summary>
        Text = 3

        ''' <summary>单个张量（ONNX 名称：TENSOR）。</summary>
        Tensor = 4

        ''' <summary>子图（ONNX 名称：GRAPH）。</summary>
        Graph = 5

        ''' <summary>浮点数数组（ONNX 名称：FLOATS）。</summary>
        Floats = 6

        ''' <summary>整数数组（ONNX 名称：INTS）。</summary>
        Ints = 7

        ''' <summary>字符串数组（ONNX 名称：STRINGS）。</summary>
        Texts = 8

        ''' <summary>张量数组（ONNX 名称：TENSORS）。</summary>
        Tensors = 9

        ''' <summary>子图数组（ONNX 名称：GRAPHS）。</summary>
        Graphs = 10

        ''' <summary>稀疏张量（ONNX 名称：SPARSE_TENSOR）。</summary>
        SparseTensor = 11

        ''' <summary>稀疏张量数组（ONNX 名称：SPARSE_TENSORS）。</summary>
        SparseTensors = 12

        ''' <summary>类型描述（ONNX 名称：TYPE_PROTO）。</summary>
        TypeProto = 13

        ''' <summary>类型描述数组（ONNX 名称：TYPE_PROTOS）。</summary>
        TypeProtos = 14

    End Enum

    ''' <summary>
    ''' ONNX <c>TensorProto.DataLocation</c>：张量数据的存放位置。
    ''' </summary>
    Public Enum DataLocation As Integer

        ''' <summary>数据内嵌在模型文件里（ONNX 名称：DEFAULT）。</summary>
        Embedded = 0

        ''' <summary>数据存放在外部文件中（<c>external_data</c>）。</summary>
        External = 1

    End Enum

    ''' <summary>
    ''' <see cref="TensorDataType"/> 的辅助函数。
    ''' </summary>
    Public Module TensorDataTypeHelper

        ''' <summary>
        ''' 取得单个元素的字节长度；字符串与复数类型返回 0（表示长度可变或暂不支持）。
        ''' </summary>
        ''' <param name="type">张量数据类型。</param>
        ''' <returns>元素占用的字节数。</returns>
        Public Function GetElementSize(type As TensorDataType) As Integer
            Select Case type
                Case TensorDataType.UInt8, TensorDataType.Int8, TensorDataType.Bool
                    Return 1
                Case TensorDataType.UInt16, TensorDataType.Int16, TensorDataType.Float16, TensorDataType.BFloat16
                    Return 2
                Case TensorDataType.Float, TensorDataType.Int32, TensorDataType.UInt32
                    Return 4
                Case TensorDataType.Float64, TensorDataType.Int64, TensorDataType.UInt64,
                     TensorDataType.Complex64
                    Return 8
                Case TensorDataType.Complex128
                    Return 16
                Case Else
                    Return 0
            End Select
        End Function

        ''' <summary>
        ''' 判断某个数据类型是否可以用 <see cref="OnnxTensor"/> 的 double 数值视图表示。
        ''' </summary>
        ''' <param name="type">张量数据类型。</param>
        ''' <returns>数值类型返回 True。</returns>
        Public Function IsNumericType(type As TensorDataType) As Boolean
            Select Case type
                Case TensorDataType.Float, TensorDataType.Float64,
                     TensorDataType.Int8, TensorDataType.Int16, TensorDataType.Int32, TensorDataType.Int64,
                     TensorDataType.UInt8, TensorDataType.UInt16, TensorDataType.UInt32, TensorDataType.UInt64,
                     TensorDataType.Bool
                    Return True
                Case Else
                    Return False
            End Select
        End Function

    End Module

End Namespace
