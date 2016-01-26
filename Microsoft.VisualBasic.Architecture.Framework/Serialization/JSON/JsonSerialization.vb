Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Runtime.Serialization.Json
Imports System.IO
Imports System.Text
Imports System.Runtime.CompilerServices

Namespace Serialization

    ''' <summary>
    ''' 使用.NET系统环境之中自带的框架进行JSON序列化和反序列化
    ''' </summary>
    Public Module JsonContract

        <Extension> Public Function GetJson(Of T)(obj As T) As String
            Using ms As New MemoryStream()
                Dim jsonSer As New DataContractJsonSerializer(GetType(T))
                Call jsonSer.WriteObject(ms, obj)
                Dim json As String = Encoding.UTF8.GetString(ms.ToArray())
                Return json
            End Using
        End Function

        ''' <summary>
        ''' JSON反序列化
        ''' </summary>
        <Extension> Public Function LoadObject(Of T)(json As String) As T
            Using MS As New MemoryStream(Encoding.UTF8.GetBytes(json))
                Dim ser As New DataContractJsonSerializer(GetType(T))
                Dim obj As T = DirectCast(ser.ReadObject(MS), T)
                Return obj
            End Using
        End Function
    End Module
End Namespace
