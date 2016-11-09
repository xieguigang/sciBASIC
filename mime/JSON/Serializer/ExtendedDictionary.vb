Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MIME.JSON.Parser
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module ExtendedDictionary

    ''' <summary>
    ''' 对继承自字典对象的Class类型进行序列化处理
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="V"></typeparam>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 1. 首先序列化字典本身
    ''' 2. 然后添加属性
    ''' </remarks>
    Public Function GetExtendedJson(Of V, T As Dictionary(Of String, V))(obj As T) As String
        Dim br As New JsonObject

        For Each key$ In obj.Keys
            Call br.Add(key, obj(key$).GetJson)
        Next

        Dim defines = DataFramework.Schema(obj.GetType, PropertyAccess.Readable,, True)

        For Each key$ In defines.Keys
            If key = NameOf(obj.Keys) OrElse
                key = NameOf(obj.Values) OrElse
                key = NameOf(obj.Comparer) OrElse
                key = NameOf(obj.Count) Then
                ' 忽略掉系统字典对象的自有属性
                Continue For
            End If

            Dim o = defines(key$).GetValue(obj)
            Dim value$ = If(
                o Is Nothing,
                "null",
                JsonContract.GetObjectJson(o, o.GetType, False))

            Call br.Add(key, value)
        Next

        Dim json$ = br.BuildJsonString
        Return json
    End Function
End Module
