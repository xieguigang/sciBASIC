#Region "Microsoft.VisualBasic::f6a0a30eeaf8d5d78faa573656fbf437, mime\application%json\Serializer\ExtendedDictionary.vb"

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

    ' Module ExtendedDictionary
    ' 
    '     Function: GetExtendedJson, getSpecificProperties, LoadExtendedJson
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MIME.application.json.Parser
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module ExtendedDictionary

    Public Function LoadExtendedJson(Of V, T As Dictionary(Of String, V))(json$) As T
        ' 因为所需要反序列化的对象是一个字典的继承对象，所以这里得到的一定是字典对象
        Dim model As JsonObject = DirectCast(ParseJson(json$), JsonObject)
        Dim type As Type = GetType(T)
        Dim obj As Object = Activator.CreateInstance(type)
        Dim defines = type.getSpecificProperties(PropertyAccess.Writeable)

        For Each key$ In defines.Keys
            If model.ContainsKey(key$) Then
                Dim o As JsonElement = model(key)
                Dim j$ = o.BuildJsonString
                Dim entry As PropertyInfo = defines(key$)
                Dim value = LoadObject(j$, entry.PropertyType)

                Call entry.SetValue(obj, value)
                Call model.Remove(key)
            End If
        Next

        ' 剩下的元素都是字典的
        Dim out As T = DirectCast(obj, T)

        type = GetType(V)

        For Each key In model
            Dim j As String = key.Value.BuildJsonString
            Dim value As V = DirectCast(LoadObject(j$, type,), V)

            Call out.Add(key.Name, value)
        Next

        Return out
    End Function

    ''' <summary>
    ''' 得到除去字典以外的所有继承类的可写、可读属性
    ''' </summary>
    ''' <param name="type"></param>
    ''' <returns></returns>
    <Extension>
    Private Function getSpecificProperties(type As Type, acc As PropertyAccess) As Dictionary(Of String, PropertyInfo)
        Dim defines = DataFramework.Schema(type, acc,, True)

        ' 忽略掉系统字典对象的自有属性
        Call defines.Remove("Keys")
        Call defines.Remove("Values")
        Call defines.Remove("Comparer")
        Call defines.Remove("Count")

        Return defines
    End Function

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
    Public Function GetExtendedJson(Of V, T As Dictionary(Of String, V))(obj As T, Optional indent As Boolean = False) As String
        Dim br As New JsonObject

        For Each key$ In obj.Keys
            Call br.Add(key, obj(key$).GetJson)
        Next

        Dim o As Object
        Dim valueJSON As String
        Dim defines As Dictionary(Of String, PropertyInfo) =
            obj _
            .GetType _
            .getSpecificProperties(PropertyAccess.Readable)

        For Each key As String In defines.Keys
            o = defines(key$).GetValue(obj)

            If o Is Nothing Then
                valueJSON = "null"
            Else
                valueJSON = JsonContract.GetObjectJson(o, o.GetType, False)
            End If

            Call br.Add(key, valueJSON)
        Next

        Dim json$ = br.BuildJsonString

        If indent Then
            json = Formatter.Format(json:=json$)
        End If

        Return json
    End Function
End Module
