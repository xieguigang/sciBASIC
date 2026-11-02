Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\ObjectExtensions.vb
' /********************************************************************************/

Imports System.Runtime.CompilerServices

''' <summary>
''' 便于链式调用的 protobuf 扩展方法。
''' </summary>
Public Module ObjectExtensions

    ''' <summary>
    ''' 把对象序列化为 protobuf 字节数组。
    ''' </summary>
    <Extension>
    Public Function ToProtobuf(Of T)(obj As T) As Byte()
        Return ProtoSerializer.Serialize(obj)
    End Function

    ''' <summary>
    ''' 把对象序列化为 protobuf 文件，并返回文件路径。
    ''' </summary>
    <Extension>
    Public Function ToProtobufFile(Of T)(obj As T, path As String) As String
        Call ProtoSerializer.SerializeToFile(obj, path)

        Return path
    End Function

    ''' <summary>
    ''' 从 protobuf 字节数组反序列化为指定类型。
    ''' </summary>
    <Extension>
    Public Function FromProtobuf(Of T)(data As Byte()) As T
        Return ProtoSerializer.Deserialize(Of T)(data)
    End Function

End Module
