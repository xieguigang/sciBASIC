#Region "Microsoft.VisualBasic::ed385e3d1132a9e90df98043b936aa37, sciBASIC#\mime\application%json\Extensions.vb"

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

'   Total Lines: 57
'    Code Lines: 37
' Comment Lines: 14
'   Blank Lines: 6
'     File Size: 1.88 KB


' Module Extensions
' 
'     Function: AsString, AsStringVector, ParseJson, ParseJsonFile
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.My.JavaScript

<HideModuleName> Public Module Extensions

    ''' <summary>
    ''' Parse json string
    ''' </summary>
    ''' <param name="JsonStr"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ParseJson(JsonStr As String) As JsonElement
        Return New JsonParser().OpenJSON(JsonStr)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ParseJsonFile(JsonFile As String) As JsonElement
        Return New JsonParser().Open(JsonFile)
    End Function

    ''' <summary>
    ''' try cast of the json element object as json literal value
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <returns>
    ''' nothing will be returns if the target <paramref name="obj"/>
    ''' is not a <see cref="JsonValue"/> type.
    ''' </returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsString(obj As JsonElement, decodeMetachar As Boolean) As String
        If TypeOf obj Is JsonValue Then
            Return DirectCast(obj, JsonValue).GetStripString(decodeMetachar)
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' this function auto encode the scalar value to array data
    ''' </summary>
    ''' <param name="array">should be json array or json literal value</param>
    ''' <param name="decodeMetachar"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsStringVector(array As JsonElement, decodeMetachar As Boolean) As String()
        If array Is Nothing Then
            Return Nothing
        End If
        If TypeOf array Is JsonValue Then
            Return New String() {array.AsString(decodeMetachar)}
        End If

        Return DirectCast(array, JsonArray) _
            .SafeQuery _
            .Select(AddressOf AsString) _
            .ToArray
    End Function

    <Extension>
    Public Function CreateJsObject(obj As JsonObject) As JavaScriptObject
        Dim js As New JavaScriptObject

        For Each member As NamedValue(Of JsonElement) In obj
            If TypeOf member.Value Is JsonObject Then
                js(member.Name) = DirectCast(member.Value, JsonObject).CreateJsObject
            ElseIf TypeOf member.Value Is JsonArray Then
                js(member.Name) = DirectCast(member.Value, JsonArray).DecodeArray
            Else
                js(member.Name) = DirectCast(member.Value, JsonValue).GetStripString(decodeMetachar:=True)
            End If
        Next

        Return js
    End Function

    <Extension>
    Private Function DecodeArray(raw As JsonArray) As Array
        Dim elementType As Type = raw.UnderlyingType
        Dim values As Array = Array.CreateInstance(elementType, raw.Length)

        If elementType Is GetType(Object) Then
            For i As Integer = 0 To raw.Length - 1
                Dim json As JsonElement = raw(i)

                If TypeOf json Is JsonArray Then
                    values(i) = DirectCast(json, JsonArray).DecodeArray
                ElseIf TypeOf json Is JsonObject Then
                    values(i) = DirectCast(json, JsonObject).CreateJsObject
                Else
                    values(i) = DirectCast(json, JsonValue).value
                End If
            Next
        Else
            For i As Integer = 0 To raw.Length - 1
                values(i) = Conversion.CTypeDynamic(DirectCast(raw(i), JsonValue).value, elementType)
            Next
        End If

        Return values
    End Function
End Module
