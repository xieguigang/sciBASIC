Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Runtime.Serialization.Json
Imports System.IO
Imports System.Text
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace Serialization

    ''' <summary>
    ''' 使用.NET系统环境之中自带的框架进行JSON序列化和反序列化
    ''' </summary>
    <PackageNamespace("Json.Contract")>
    Public Module JsonContract

        <ExportAPI("Get.Json")>
        Public Function GetJson(obj As Object, type As Type) As String
            Using ms As New MemoryStream()
                Dim jsonSer As New DataContractJsonSerializer(type)
                Call jsonSer.WriteObject(ms, obj)
                Dim json As String = Encoding.UTF8.GetString(ms.ToArray())
                Return json
            End Using
        End Function

        <Extension> Public Function GetJson(Of T)(obj As T) As String
            Return GetJson(obj, GetType(T))
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="json">null -> Nothing</param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <ExportAPI("LoadObject")>
        Public Function LoadObject(json As String, type As Type) As Object
            If String.Equals(json, "null", StringComparison.OrdinalIgnoreCase) Then
                Return Nothing
            End If

            Using MS As New MemoryStream(Encoding.UTF8.GetBytes(json))
                Dim ser As New DataContractJsonSerializer(type)
                Dim obj As Object = ser.ReadObject(MS)
                Return obj
            End Using
        End Function

        ''' <summary>
        ''' JSON反序列化
        ''' </summary>
        <Extension> Public Function LoadObject(Of T)(json As String) As T
            Dim value As Object = LoadObject(json, GetType(T))
            Dim obj As T = DirectCast(value, T)
            Return obj
        End Function

        Public Function LoadJsonFile(Of T)(file As String, Optional encoding As Encoding = Nothing) As T
            Dim json As String = IO.File.ReadAllText(file, If(encoding Is Nothing, Encoding.Default, encoding))
            Return json.LoadObject(Of T)
        End Function
    End Module
End Namespace
