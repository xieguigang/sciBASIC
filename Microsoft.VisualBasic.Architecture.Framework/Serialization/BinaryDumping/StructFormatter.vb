Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Formatters.Binary

Namespace Serialization.BinaryDumping

    Public Module StructFormatter

        ''' <summary>
        ''' Save a structure type object into a binary file.(使用二进制序列化保存一个对象)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="obj"></param>
        ''' <param name="path"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function Serialize(Of T)(obj As T, path As String) As Boolean
            Dim buffer As Byte() = obj.GetSerializeBuffer
            Return buffer.FlushStream(path)
        End Function

        <Extension> Public Function GetSerializeBuffer(Of T)(obj As T) As Byte()
            Dim IFormatter As IFormatter = New BinaryFormatter()
            Dim Stream As New IO.MemoryStream()
            Call IFormatter.Serialize(Stream, obj)
            Dim buffer As Byte() = Stream.ToArray
            Return buffer
        End Function

        <Extension> Public Function DeSerialize(Of T)(bytes As Byte()) As T
            Dim obj As Object = (New BinaryFormatter).[Deserialize](New MemoryStream(bytes))
            Return DirectCast(obj, T)
        End Function

        ''' <summary>
        ''' Load a strucutre object from the file system by using binary serializer deserialize.
        ''' (使用反二进制序列化从指定的文件之中加载一个对象)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="path"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function Load(Of T)(path As String) As T
            If Not FileIO.FileSystem.FileExists(path) Then
                Return Activator.CreateInstance(Of T)()
            End If
            Using Stream As Stream = New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
                Dim IFormatter As IFormatter = New BinaryFormatter()
                Dim obj As T = DirectCast(IFormatter.Deserialize(Stream), T)
                Return obj
            End Using
        End Function
    End Module
End Namespace