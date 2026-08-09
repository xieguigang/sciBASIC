#Region "Microsoft.VisualBasic::5c82bda0bb990af0990b08d170b115d4, vs_solutions\dev\VisualStudio\sln\Parser.vb"

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

    '   Total Lines: 434
    '    Code Lines: 292 (67.28%)
    ' Comment Lines: 48 (11.06%)
    '    - Xml Docs: 52.08%
    ' 
    '   Blank Lines: 94 (21.66%)
    '     File Size: 16.82 KB


    '     Module Parser
    ' 
    '         Function: ExtractSectionName, GetAttr, NormalizeGuid, Parse, ParseKeyValueSection
    '                   ParseProjectDeclaration, ParseSln, ParseSlnx, ParseSolutionConfigurations, ResolveType
    '                   SkipSection, SplitKeyValue, SplitQuoted, StripQuotes
    ' 
    '         Sub: ParseSlnxProjects
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports System.Text
Imports System.Xml

Namespace sln.File

    ''' <summary>
    ''' Parses Visual Studio solution files, both the classic ``.sln`` (text) format
    ''' and the new ``.slnx`` (XML) format, into a unified <see cref="Solution"/> model.
    ''' </summary>
    Module Parser

        ''' <summary>
        ''' Parse a solution file, automatically dispatching on its extension.
        ''' </summary>
        Public Function Parse(path As String) As Solution
            If String.IsNullOrEmpty(path) Then
                Return Nothing
            End If

            Select Case System.IO.Path.GetExtension(path).ToLowerInvariant()
                Case ".slnx"
                    Return ParseSlnx(path)
                Case Else
                    Return ParseSln(path)
            End Select
        End Function

        ' ------------------------------------------------------------------
        ' Shared helpers
        ' ------------------------------------------------------------------

        Friend Function SplitKeyValue(line As String) As (Key As String, Value As String)
            Dim eq As Integer = line.IndexOf("="c)

            If eq < 0 Then
                Return (line.Trim(), "")
            End If

            Return (line.Substring(0, eq).Trim(), line.Substring(eq + 1).Trim())
        End Function

        ''' <summary>
        ''' Split a comma separated list, honoring double quotes so that
        ''' paths containing commas are not broken.
        ''' </summary>
        Friend Function SplitQuoted(text As String) As String()
            Dim result As New List(Of String)
            Dim current As New StringBuilder()
            Dim inQuotes As Boolean = False
            Dim i As Integer = 0

            Do While i < text.Length
                Dim c As Char = text(i)

                If c = """"c Then
                    inQuotes = Not inQuotes
                ElseIf c = ","c AndAlso Not inQuotes Then
                    result.Add(current.ToString())
                    current.Clear()
                Else
                    current.Append(c)
                End If

                i += 1
            Loop

            result.Add(current.ToString())
            Return result.ToArray()
        End Function

        Friend Function StripQuotes(text As String) As String
            If String.IsNullOrEmpty(text) Then
                Return text
            End If

            text = text.Trim()

            If text.Length >= 2 AndAlso text(0) = """"c AndAlso text(text.Length - 1) = """"c Then
                Return text.Substring(1, text.Length - 2)
            End If

            Return text
        End Function

        Friend Function ExtractSectionName(line As String) As String
            ' GlobalSection(SolutionConfigurationPlatforms) = preSolution
            Dim open As Integer = line.IndexOf("("c)
            Dim close As Integer = line.IndexOf(")"c)

            If open >= 0 AndAlso close > open Then
                Return line.Substring(open + 1, close - open - 1).Trim()
            End If

            Return line
        End Function

        Friend Function NormalizeGuid(guid As String) As String
            If String.IsNullOrEmpty(guid) Then
                Return guid
            End If

            guid = guid.Trim()

            If Not guid.StartsWith("{"c) Then
                guid = "{" & guid
            End If

            If Not guid.EndsWith("}"c) Then
                guid = guid & "}"
            End If

            Return guid.ToUpperInvariant()
        End Function

        Friend Function ResolveType(typeGuid As String) As TypeId
            If String.IsNullOrEmpty(typeGuid) Then
                Return TypeId.Unknown
            End If

            Dim guid As String = NormalizeGuid(typeGuid)
            Dim descriptions = CType([Enum].GetValues(GetType(TypeId)), TypeId())

            For Each value As TypeId In descriptions
                Dim attr = CType(Attribute.GetCustomAttribute(GetType(TypeId).GetField(value.ToString()), GetType(DescriptionAttribute)), DescriptionAttribute)

                If attr IsNot Nothing AndAlso String.Equals(attr.Description, guid, StringComparison.OrdinalIgnoreCase) Then
                    Return value
                End If
            Next

            Return TypeId.Unknown
        End Function
    End Module
End Namespace
