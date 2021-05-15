#Region "Microsoft.VisualBasic::b9faeb7c9edf9050ffa75af1c2571f39, mime\application%json\Serializer\JSONSerializer.vb"

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

    ' Module JSONSerializer
    ' 
    '     Function: BuildJsonString, GetJson, jsonArrayString, jsonObjectString, jsonValueString
    ' 
    ' /********************************************************************************/

#End Region

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

    <Extension>
    Private Function jsonValueString(obj As JsonValue, opt As JSONSerializerOptions) As String
        Dim value As Object = obj.value

        If value Is Nothing Then
            Return "null"
        ElseIf value.GetType Is obj.BSONValue Then
            Return DirectCast(value, BSONValue).ToString
        Else
            Return BSONValue.FromValue(value).ToString
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
