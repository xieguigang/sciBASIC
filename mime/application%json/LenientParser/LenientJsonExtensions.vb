#Region "Microsoft.VisualBasic::7107b8e8bdaf7ae3e6f860f6b7d923be, mime\application%json\LenientParser\LenientJsonExtensions.vb"

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

    '   Total Lines: 80
    '    Code Lines: 25 (31.25%)
    ' Comment Lines: 46 (57.50%)
    '    - Xml Docs: 97.83%
    ' 
    '   Blank Lines: 9 (11.25%)
    '     File Size: 3.13 KB


    '     Module LenientJsonExtensions
    ' 
    '         Function: LoadLenientJson, ParseJsonLenient, RepairJson
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Namespace LenientJson

    ''' <summary>
    ''' Extension methods providing convenient access to the <see cref="LenientJsonParser"/>.
    ''' These extensions allow you to parse potentially malformed JSON strings
    ''' (e.g., from LLM output) using a fluent API style.
    ''' </summary>
    Public Module LenientJsonExtensions

        ''' <summary>
        ''' Parse a JSON string leniently, tolerating common syntax errors
        ''' produced by Large Language Models (LLMs).
        ''' </summary>
        ''' <param name="json">The JSON text to parse (possibly malformed).</param>
        ''' <returns>
        ''' The parsed <see cref="JsonElement"/>, or <c>Nothing</c> if the input
        ''' is empty or contains no recognizable JSON value.
        ''' </returns>
        ''' <example>
        ''' <code>
        ''' Dim json As String = "{name: 'Alice', age: 30,}"  ' Unquoted key, single quote, trailing comma
        ''' Dim obj As JsonElement = json.ParseJsonLenient()
        ''' Console.WriteLine(obj("name").ToString())  ' Output: Alice
        ''' </code>
        ''' </example>
        <Extension>
        Public Function ParseJsonLenient(json As String) As JsonElement
            Return LenientJsonParser.ParseJSON(json)
        End Function

        ''' <summary>
        ''' Load and parse a JSON file leniently from the specified file path.
        ''' </summary>
        ''' <param name="file">The path to the JSON file.</param>
        ''' <returns>
        ''' The parsed <see cref="JsonElement"/>, or <c>Nothing</c> if the file
        ''' does not exist or is empty.
        ''' </returns>
        <Extension>
        Public Function LoadLenientJson(file As String) As JsonElement
            Return LenientJsonParser.Open(file)
        End Function

        ''' <summary>
        ''' Repair a malformed JSON string by parsing it leniently and then
        ''' re-serializing it to produce valid JSON.
        ''' </summary>
        ''' <param name="json">The malformed JSON text.</param>
        ''' <returns>
        ''' A valid JSON string if parsing succeeded; otherwise, the original
        ''' input string is returned unchanged.
        ''' </returns>
        ''' <example>
        ''' <code>
        ''' Dim malformed As String = "{name: 'Alice', age: 30,}"
        ''' Dim repaired As String = malformed.RepairJson()
        ''' ' repaired = {"name":"Alice","age":30}
        ''' </code>
        ''' </example>
        <Extension>
        Public Function RepairJson(json As String) As String
            If String.IsNullOrEmpty(json) Then
                Return json
            End If

            Dim element As JsonElement = LenientJsonParser.ParseJSON(json)

            If element Is Nothing Then
                Return json
            End If

            ' Serialize the parsed element back to valid JSON
            Return element.BuildJsonString
        End Function

    End Module
End Namespace
