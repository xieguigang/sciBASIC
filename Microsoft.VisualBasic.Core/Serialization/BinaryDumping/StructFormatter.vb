#Region "Microsoft.VisualBasic::643441f00248ad1e5d3cf531e26528f1, Microsoft.VisualBasic.Core\Serialization\BinaryDumping\StructFormatter.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module StructFormatter
    ' 
    '         Function: DeSerialize, GetSerializeBuffer, Load, Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function Serialize(Of T)(obj As T, path As String) As Boolean
            Return obj.GetSerializeBuffer.FlushStream(path)
        End Function

        <Extension> Public Function GetSerializeBuffer(Of T)(obj As T) As Byte()
            Dim IFormatter As IFormatter = New BinaryFormatter()

            Using stream As New MemoryStream()
                Call IFormatter.Serialize(stream, obj)
                Dim buffer As Byte() = stream.ToArray
                Return buffer
            End Using
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
            If Not path.FileExists Then
                Return Activator.CreateInstance(Of T)()
            End If
            Using stream As Stream = New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
                Dim IFormatter As IFormatter = New BinaryFormatter()
                Dim obj As T = DirectCast(IFormatter.Deserialize(stream), T)
                Return obj
            End Using
        End Function
    End Module
End Namespace
