#Region "Microsoft.VisualBasic::ccb6c77addc6b58ec103fc71989229af, G:/GCModeller/src/runtime/sciBASIC#/mime/application%json//Serializer/JSONSerializer.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:


' Code Statistics:

'   Total Lines: 158
'    Code Lines: 111
' Comment Lines: 28
'   Blank Lines: 19
'     File Size: 5.65 KB


' Module JSONSerializer
' 
'     Function: (+2 Overloads) BuildJsonString, encodeString, GetJson, jsonArrayString, jsonObjectString
'               jsonValueString
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json.BSON
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.ValueTypes
Imports any = Microsoft.VisualBasic.Scripting

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
    Public Function BuildJsonString(json As JsonElement, Optional indent As Boolean = False) As String
        Return json.BuildJsonString(New JSONSerializerOptions With {.indent = indent})
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

    ''' <summary>
    ''' find two char
    ''' </summary>
    ReadOnly unescape As New Regex("[^\\]""", RegexOptions.Multiline)

    Private Function encodeString(value As String, opt As JSONSerializerOptions) As String
        If InStr(value, """") > 0 Then
            ' escape the quote symbol inside string,
            ' or json string will syntax error
            Dim unescape_quotes As String() = unescape.Matches(value).ToArray

            For Each unescape_char As String In unescape_quotes
                value = value.Replace(
                    unescape_char,
                    unescape_char.First & "\" & unescape_char.Last
                )
            Next

            If value.First = """"c Then
                value = "\" & value
            End If
        End If

        If opt.unicodeEscape Then
            Dim sb As New StringBuilder
            Dim code As Integer
            Dim bytes As Byte()

            For Each c As Char In DirectCast(value, String)
                code = AscW(c)

                If code < 0 OrElse code > Byte.MaxValue Then
                    sb.Append("\u")
                    bytes = Encoding.Unicode.GetBytes(c)
                    sb.Append(bytes(1).ToString("x"))
                    sb.Append(bytes(0).ToString("x"))
                Else
                    sb.Append(c)
                End If
            Next

            Return $"""{sb.ToString}"""
        Else
            Return JsonContract.GetObjectJson(GetType(String), value)
        End If
    End Function

    ''' <summary>
    ''' "..."
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <param name="opt"></param>
    ''' <returns></returns>
    <Extension>
    Private Function jsonValueString(obj As JsonValue, opt As JSONSerializerOptions) As String
        Dim value As Object = obj.value

        If value Is Nothing Then
            Return "null"
        ElseIf value.GetType Is obj.BSONValue Then
            value = DirectCast(value, BSONValue).GetObjectValue
        End If

        If TypeOf value Is Date AndAlso opt.unixTimestamp Then
            Return DirectCast(value, Date).UnixTimeStamp
        ElseIf TypeOf value Is String Then
            Return encodeString(value, opt)
        ElseIf TypeOf value Is Boolean Then
            Return value.ToString.ToLower
        Else
            ' number,integer,etc
            Return any.ToString(value)
        End If
    End Function

    ''' <summary>
    ''' {...}
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <param name="opt"></param>
    ''' <returns></returns>
    <Extension>
    Private Function jsonObjectString(obj As JsonObject, opt As JSONSerializerOptions) As String
        Dim members As New List(Of String)

        For Each member As NamedValue(Of JsonElement) In obj
            Call members.Add($"{encodeString(member.Name, opt)}: {member.Value.BuildJsonString(opt)}")
        Next

        If opt.indent Then
            Return $"{{
            {members.JoinBy("," & ASCII.LF)}
        }}"
        Else
            Return $"{{{members.JoinBy(",")}}}"
        End If
    End Function

    ''' <summary>
    ''' [...]
    ''' </summary>
    ''' <param name="arr"></param>
    ''' <param name="opt"></param>
    ''' <returns></returns>
    <Extension>
    Private Function jsonArrayString(arr As JsonArray, opt As JSONSerializerOptions) As String
        Dim a As New StringBuilder
        Dim array$() = arr _
            .Select(Function(item) item.BuildJsonString(opt)) _
            .ToArray

        If opt.indent Then
            Call a.AppendLine("[").AppendLine(array.JoinBy(", ")).AppendLine("]")
        Else
            Call a.Append("[").Append(array.JoinBy(", ")).Append("]")
        End If

        Return a.ToString
    End Function
End Module
