Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json.BSON
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Text

Public Module JSONSerializer

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj"></param>
    ''' <param name="maskReadonly">
    ''' 如果这个参数为真，则不会序列化只读属性
    ''' </param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function GetJson(Of T)(obj As T,
                                  Optional maskReadonly As Boolean = False,
                                  Optional indent As Boolean = False,
                                  Optional enumToStr As Boolean = True,
                                  Optional unixTimestamp As Boolean = True) As String

        Return New JSONSerializerOptions With {
            .indent = indent,
            .maskReadonly = maskReadonly,
            .enumToString = enumToStr,
            .unixTimestamp = unixTimestamp
        }.DoCall(Function(opts)
                     Return obj.GetType.GetJsonElement(obj, opts).BuildJsonString(opts)
                 End Function)
    End Function

    <Extension>
    Public Function BuildJsonString(json As JsonElement, opts As JSONSerializerOptions) As String
        If json Is Nothing Then
            Return "null"
        End If

        Select Case json.GetType
            Case GetType(JsonValue) : Return DirectCast(json, JsonValue).jsonValueString(opts)
            Case GetType(JsonObject) : Return DirectCast(json, JsonObject).jsonObjectString(opts)
            Case GetType(JsonArray) : Return DirectCast(json, JsonArray).jsonArrayString(opts)
            Case Else
                Throw New NotImplementedException(json.GetType.FullName)
        End Select
    End Function

    <Extension>
    Private Function jsonValueString(obj As JsonValue, opt As JSONSerializerOptions) As String
        Dim value As Object = obj.value

        If value Is Nothing Then
            Return "null"
        ElseIf value.GetType Is obj.BSONValue Then
            Return DirectCast(value, BSONValue).ToString
        Else
            Return Scripting.ToString(value, "null")
        End If
    End Function

    <Extension>
    Private Function jsonObjectString(obj As JsonObject, opt As JSONSerializerOptions) As String
        Dim members As New List(Of String)

        For Each member In obj
            members.Add($"""{member.Name}"": {member.Value.BuildJsonString(opt)}")
        Next

        If opt.indent Then
            Return $"{{
            {members.JoinBy("," & ASCII.LF)}
        }}"
        Else
            Return $"{{{members.JoinBy(",")}}}"
        End If
    End Function

    <Extension>
    Private Function jsonArrayString(arr As JsonArray, opt As JSONSerializerOptions) As String
        Dim a As New StringBuilder
        Dim array$() = arr _
            .Select(Function(x) x.BuildJsonString(opt)) _
            .ToArray

        If opt.indent Then
            Call a.AppendLine("[").AppendLine(array.JoinBy(", ")).AppendLine("]")
        Else
            Call a.Append("[").Append(array.JoinBy(", ")).Append("]")
        End If

        Return a.ToString
    End Function
End Module
