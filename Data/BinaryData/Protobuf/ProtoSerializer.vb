Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\ProtoSerializer.vb
' /********************************************************************************/

Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.Protobuf.Serialization
Imports Microsoft.VisualBasic.Data.IO.Protobuf.Serialization.Reflection
Imports Microsoft.VisualBasic.Data.IO.Protobuf.Wire

''' <summary>
''' protobuf 序列化入口。
''' </summary>
''' <remarks>
''' <para>
''' 典型用法：
''' <code>
''' Dim data As Byte() = ProtoSerializer.Serialize(person)
''' Dim copy As Person = ProtoSerializer.Deserialize(Of Person)(data)
''' </code>
''' </para>
''' <para>
''' 被序列化的类型需要至少一个 <see cref="ProtoMemberAttribute"/> 标注的公开属性或字段。
''' 类型模型按 <see cref="Context"/> 缓存，反射只在首次使用时发生。
''' </para>
''' </remarks>
Public NotInheritable Class ProtoSerializer

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 当前使用的序列化上下文，默认指向 <see cref="SerializationContext.DefaultContext"/>。
    ''' </summary>
    Public Shared Property Context As SerializationContext = SerializationContext.DefaultContext

    ''' <summary>
    ''' 把对象序列化为 protobuf 字节数组。
    ''' </summary>
    Public Shared Function Serialize(Of T)(obj As T) As Byte()
        Dim info As TypeInfo = EnsureMessage(GetType(T))

        Using writer As New ProtoWriter()
            Call info.WriteTo(obj, writer, Context)

            Return writer.ToArray()
        End Using
    End Function

    ''' <summary>
    ''' 把对象序列化到指定的输出流。
    ''' </summary>
    ''' <param name="obj">要序列化的对象。</param>
    ''' <param name="output">目标输出流。</param>
    ''' <param name="leaveOpen">完成后是否保持 <paramref name="output"/> 打开（默认 True）。</param>
    Public Shared Sub Serialize(Of T)(obj As T, output As Stream, Optional leaveOpen As Boolean = True)
        If output Is Nothing Then
            Throw New ArgumentNullException(NameOf(output))
        End If

        Dim info As TypeInfo = EnsureMessage(GetType(T))

        Using writer As New ProtoWriter(output, leaveOpen)
            Call info.WriteTo(obj, writer, Context)
            Call writer.Flush()
        End Using
    End Sub

    ''' <summary>
    ''' 把对象序列化到文件（覆盖写入）。
    ''' </summary>
    Public Shared Sub SerializeToFile(obj As Object, path As String)
        If obj Is Nothing Then
            Throw New ArgumentNullException(NameOf(obj))
        End If

        If String.IsNullOrEmpty(path) Then
            Throw New ArgumentNullException(NameOf(path))
        End If

        Dim info As TypeInfo = EnsureMessage(obj.GetType())

        Using stream As New FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read)
            Using writer As New ProtoWriter(stream, leaveOpen:=True)
                Call info.WriteTo(obj, writer, Context)
                Call writer.Flush()
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' 从 protobuf 字节数组反序列化为指定类型。
    ''' </summary>
    Public Shared Function Deserialize(Of T)(data As Byte()) As T
        Return CType(Deserialize(GetType(T), data), T)
    End Function

    ''' <summary>
    ''' 从输入流反序列化为指定类型。
    ''' </summary>
    Public Shared Function Deserialize(Of T)(input As Stream, Optional leaveOpen As Boolean = True) As T
        Return CType(Deserialize(GetType(T), input, leaveOpen), T)
    End Function

    ''' <summary>
    ''' 从 protobuf 字节数组反序列化为指定类型（非泛型入口）。
    ''' </summary>
    Public Shared Function Deserialize(type As Type, data As Byte()) As Object
        If data Is Nothing Then
            Throw New ArgumentNullException(NameOf(data))
        End If

        Using stream As New MemoryStream(data, writable:=False)
            Return Deserialize(type, stream, leaveOpen:=True)
        End Using
    End Function

    ''' <summary>
    ''' 从输入流反序列化为指定类型（非泛型入口）。
    ''' </summary>
    Public Shared Function Deserialize(type As Type, input As Stream, Optional leaveOpen As Boolean = True) As Object
        If type Is Nothing Then
            Throw New ArgumentNullException(NameOf(type))
        End If

        If input Is Nothing Then
            Throw New ArgumentNullException(NameOf(input))
        End If

        Dim info As TypeInfo = EnsureMessage(type)

        Using reader As New ProtoReader(input, leaveOpen)
            Return info.ReadFrom(reader, Nothing, Context)
        End Using
    End Function

    ''' <summary>
    ''' 从文件反序列化为指定类型。
    ''' </summary>
    Public Shared Function DeserializeFromFile(Of T)(path As String) As T
        If String.IsNullOrEmpty(path) Then
            Throw New ArgumentNullException(NameOf(path))
        End If

        Using stream As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
            Return Deserialize(Of T)(stream, leaveOpen:=True)
        End Using
    End Function

    ''' <summary>
    ''' 把字节数据合并进一个已存在的对象实例（protobuf 的 merge 语义）。
    ''' </summary>
    ''' <remarks>
    ''' 标量字段会被覆盖；repeated 字段与 map 字段会追加/覆盖元素；嵌套消息会递归合并。
    ''' </remarks>
    Public Shared Sub Populate(obj As Object, data As Byte())
        If obj Is Nothing Then
            Throw New ArgumentNullException(NameOf(obj))
        End If

        If data Is Nothing Then
            Throw New ArgumentNullException(NameOf(data))
        End If

        Dim info As TypeInfo = EnsureMessage(obj.GetType())

        Using stream As New MemoryStream(data, writable:=False)
            Using reader As New ProtoReader(stream, leaveOpen:=True)
                Call info.ReadFrom(reader, obj, Context)
            End Using
        End Using
    End Sub

    Private Shared Function EnsureMessage(type As Type) As TypeInfo
        Dim info As TypeInfo = Context.GetTypeInfo(type)

        If Not info.IsMessage Then
            Throw New InvalidOperationException(
                $"类型 {type.FullName} 上没有任何 <ProtoMember> 标注的公开成员，无法执行 protobuf 序列化")
        End If

        Return info
    End Function

End Class
