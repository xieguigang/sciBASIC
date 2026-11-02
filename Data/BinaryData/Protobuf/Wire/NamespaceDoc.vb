Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Wire\NamespaceDoc.vb
' /********************************************************************************/

Namespace Wire

    ''' <summary>
    ''' protobuf 线格式（wire format）的底层字节读写层。
    '''
    ''' 该命名空间提供 <see cref="ProtoWriter"/> 与 <see cref="ProtoReader"/> 两个包装类，
    ''' 它们建立在 <c>Microsoft.VisualBasic.Data.IO</c> 的
    ''' <c>BinaryDataWriter</c> / <c>BinaryDataReader</c> 之上，补齐了 protobuf 所需的
    ''' varint、zigzag、定长小端字段以及长度前缀编解码。
    ''' </summary>
    Friend Module NamespaceDoc
    End Module

End Namespace
