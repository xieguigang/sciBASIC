#Region "Microsoft.VisualBasic::0cea49925c31a060e3065bf17e855ff0, ..\sciBASIC#\mime\application%json\Extensions.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json.Parser

Public Module Extensions

    <Extension>
    Public Function ParseJsonStr(JsonStr As String) As JsonElement
        Dim value As JsonElement = New JsonParser().OpenJSON(JsonStr)
        Return value
    End Function

    Public Function ParseJsonFile(JsonFile As String) As JsonElement
        Dim value As JsonElement = New JsonParser().Open(JsonFile)
        Return value
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsString(obj As JsonElement) As String
        Return DirectCast(obj, JsonValue).GetStripString
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsStringVector(array As JsonElement) As String()
        If array Is Nothing Then
            Return Nothing
        End If
        If TypeOf array Is JsonValue Then
            Return {array.AsString}
        End If

        Return DirectCast(array, JsonArray) _
            .SafeQuery _
            .Select(AddressOf AsString) _
            .ToArray
    End Function
End Module
