#Region "Microsoft.VisualBasic::4117d0e838a740403ec49cb3e8fd230d, sciBASIC#\mime\application%json\Extensions.vb"

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

    '   Total Lines: 50
    '    Code Lines: 33
    ' Comment Lines: 11
    '   Blank Lines: 6
    '     File Size: 1.70 KB


    ' Module Extensions
    ' 
    '     Function: AsString, AsStringVector, ParseJson, ParseJsonFile
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

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
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsString(obj As JsonElement, decodeMetachar As Boolean) As String
        Return DirectCast(obj, JsonValue).GetStripString(decodeMetachar)
    End Function

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
End Module
