#Region "Microsoft.VisualBasic::7ebdf2c84720d471cae056725070e5f9, mime\text%yaml\YamlFrontMatterParser.vb"

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

    '   Total Lines: 131
    '    Code Lines: 66 (50.38%)
    ' Comment Lines: 45 (34.35%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 20 (15.27%)
    '     File Size: 5.31 KB


    ' Module YamlFrontMatterParser
    ' 
    '     Function: Parse, StripFrontMatter
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>
''' A lightweight YAML front-matter parser specialized for SKILL.md files.
''' 
''' This parser is intentionally minimal: it only understands the subset
''' of YAML used in skill metadata blocks (simple key-value pairs and
''' inline/bulleted lists). It avoids pulling in a full YAML dependency
''' so the module remains self-contained and easy to deploy.
''' 
''' A valid front-matter block is delimited by "---" lines at the very
''' top of the markdown file:
''' 
''' ---
''' name: pdf_extract_text
''' description: Extract text content from PDF documents
''' version: 1.0.0
''' tags:
'''   - pdf
'''   - text-extraction
''' ---
''' 
''' </summary>
Public Module YamlFrontMatterParser

    Private ReadOnly FrontMatterRegex As New Regex("^\s*---\s*\r?\n(.*?)\r?\n---\s*(\r?\n|$)", RegexOptions.Singleline Or RegexOptions.Compiled)

    ''' <summary>
    ''' Parse the YAML front-matter block from the raw markdown content
    ''' of a SKILL.md file and return a dictionary of key/value pairs.
    ''' List values are returned as comma-joined strings for simplicity.
    ''' </summary>
    ''' <param name="markdownContent">The full raw text of SKILL.md. When the
    ''' text contains no "---" delimited block it is parsed as a flat YAML
    ''' document so the same routine can be reused on standalone .yaml files.</param>
    ''' <returns>A dictionary of metadata keys to string values. Returns an empty dictionary if no content is supplied.</returns>
    Public Function Parse(markdownContent As String) As Dictionary(Of String, String)
        Dim result As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        If String.IsNullOrWhiteSpace(markdownContent) Then
            Return result
        End If

        ' A valid front-matter block is delimited by "---" lines. When no such
        ' block is present we still attempt to parse the whole text as a flat
        ' YAML document so the parser can be reused on standalone .yaml files.
        Dim match As Match = FrontMatterRegex.Match(markdownContent)
        Dim yamlBlock As String
        If match.Success Then
            yamlBlock = match.Groups(1).Value
        Else
            yamlBlock = markdownContent
        End If

        ParseYamlBlock(yamlBlock, result)
        Return result
    End Function

    ''' <summary>
    ''' Parse a flat YAML block (simple key/value pairs, inline/bulleted lists
    ''' and block scalars such as <c>description: &gt;-</c>) into a dictionary.
    ''' Nested mappings are not represented (the parser stays intentionally
    ''' flat) but their scalar leaves and lists are consumed without corrupting
    ''' sibling keys.
    ''' </summary>
    Private Sub ParseYamlBlock(yaml As String, result As Dictionary(Of String, String))
        Dim lines() As String = yaml.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)
        Dim currentKey As String = Nothing
        Dim listBuffer As New List(Of String)
        Dim i As Integer = 0

        While i < lines.Length
            Dim rawLine As String = lines(i)
            Dim line As String = rawLine.TrimEnd()

            ' Skip blank lines and full-line comments
            If String.IsNullOrWhiteSpace(line) OrElse line.TrimStart().StartsWith("#"c) Then
                i += 1
                Continue While
            End If

            Dim trimmed As String = line.TrimStart()

            ' Bulleted list item attached to the current key
            If trimmed.StartsWith("- ") OrElse trimmed = "-"c OrElse trimmed.StartsWith("-"c & vbTab) Then
                If currentKey IsNot Nothing Then
                    listBuffer.Add(StripQuotes(trimmed.Substring(1).Trim()))
                End If
                i += 1
                Continue While
            End If

            ' Flush any pending list into the previous key
            If currentKey IsNot Nothing AndAlso listBuffer.Count > 0 Then
                result(currentKey) = String.Join(", ", listBuffer)
                listBuffer.Clear()
                currentKey = Nothing
            End If

            ' Mapping colon (first ':' followed by whitespace or end of line,
            ' outside of quoted strings)
            Dim colonIdx As Integer = IndexOfMappingColon(line)
            If colonIdx <= 0 Then
                ' Indented line without a mapping colon (e.g. a nested mapping
                ' body) — cannot be represented in a flat dictionary, skip it.
                i += 1
                Continue While
            End If

            Dim key As String = line.Substring(0, colonIdx).Trim()
            Dim valuePart As String = line.Substring(colonIdx + 1).Trim()

            ' Block scalar indicator: key: | / key: > (with optional chomping/indent)
            If IsBlockScalarIndicator(valuePart) Then
                Dim bi As Integer = i + 1
                Dim blockValue As String = CollectBlockScalar(lines, bi, CountIndent(rawLine), valuePart)
                ' Store without the single trailing newline added by chomping.
                result(key) = blockValue.TrimEnd(ControlChars.Lf)
                i = bi
                currentKey = Nothing
                Continue While
            End If

            currentKey = key

            ' Inline flow list like: tags: [a, b, c]
            If valuePart.StartsWith("["c) AndAlso valuePart.EndsWith("]"c) Then
                Dim inner As String = valuePart.Substring(1, valuePart.Length - 2)
                Dim items() As String = inner.Split(New String() {","}, StringSplitOptions.RemoveEmptyEntries)
                Dim cleaned = items.Select(Function(s) StripQuotes(s.Trim())).Where(Function(s) s.Length > 0).ToList()
                result(key) = String.Join(", ", cleaned)
                currentKey = Nothing
            ElseIf valuePart.Length > 0 Then
                ' Scalar value, strip surrounding quotes
                result(key) = StripQuotes(valuePart)
            Else
                ' Value is empty - a bulleted list may follow on the next lines.
            End If

            i += 1
        End While

        ' Flush trailing list
        If currentKey IsNot Nothing AndAlso listBuffer.Count > 0 Then
            result(currentKey) = String.Join(", ", listBuffer)
        End If
    End Sub

    ''' <summary>
    ''' Find the position of the first mapping colon (':') in a line. A colon
    ''' only separates a key from its value when it is followed by whitespace
    ''' (space or tab) or the end of the line, and it is not quoted.
    ''' </summary>
    Private Function IndexOfMappingColon(line As String) As Integer
        Dim inSingle As Boolean = False
        Dim inDouble As Boolean = False
        For j As Integer = 0 To line.Length - 1
            Dim ch As Char = line(j)
            If inSingle Then
                If ch = "'"c Then inSingle = False
            ElseIf inDouble Then
                If ch = """"c Then inDouble = False
            Else
                If ch = "'"c Then inSingle = True
                If ch = """"c Then inDouble = True
                If ch = ":"c AndAlso (j + 1 >= line.Length OrElse line(j + 1) = " "c OrElse line(j + 1) = vbTab) Then
                    Return j
                End If
            End If
        Next
        Return -1
    End Function

    ''' <summary>
    ''' Count the leading indentation (spaces and tabs) of a raw line.
    ''' </summary>
    Private Function CountIndent(rawLine As String) As Integer
        Dim n As Integer = 0
        While n < rawLine.Length AndAlso (rawLine(n) = " "c OrElse rawLine(n) = ControlChars.Tab)
            n += 1
        End While
        Return n
    End Function

    ''' <summary>
    ''' Determine whether a value part is a bare block-scalar indicator such as
    ''' "|", ">", ">-", ">+", "|-", "|+", "|2" or ">-4" (i.e. it starts with
    ''' '|' or '>' and the remainder consists only of chomping/indent markers).
    ''' </summary>
    Private Function IsBlockScalarIndicator(valuePart As String) As Boolean
        If String.IsNullOrEmpty(valuePart) Then Return False
        If valuePart(0) <> "|"c AndAlso valuePart(0) <> ">"c Then Return False
        For k As Integer = 1 To valuePart.Length - 1
            Dim c As Char = valuePart(k)
            If c <> "-"c AndAlso c <> "+"c AndAlso Not Char.IsDigit(c) Then
                Return False
            End If
        Next
        Return True
    End Function

    ''' <summary>
    ''' Strip a single layer of surrounding single/double quotes and unescape
    ''' the contents of a double-quoted string.
    ''' </summary>
    Private Function StripQuotes(s As String) As String
        If String.IsNullOrEmpty(s) Then Return s
        s = s.Trim()
        If s.Length >= 2 Then
            If s(0) = """"c AndAlso s(s.Length - 1) = """"c Then
                Return UnescapeDouble(s.Substring(1, s.Length - 2))
            End If
            If s(0) = "'"c AndAlso s(s.Length - 1) = "'"c Then
                Return s.Substring(1, s.Length - 2).Replace("''", "'")
            End If
        End If
        Return s
    End Function

    ''' <summary>
    ''' Minimal unescape for double-quoted YAML scalars (backslash escapes).
    ''' </summary>
    Private Function UnescapeDouble(s As String) As String
        If Not s.Contains("\"c) Then Return s
        Dim sb As New StringBuilder(s.Length)
        Dim j As Integer = 0
        While j < s.Length
            If s(j) = "\"c AndAlso j + 1 < s.Length Then
                Select Case s(j + 1)
                    Case """"c : sb.Append(""""c)
                    Case "\"c : sb.Append("\"c)
                    Case "n"c : sb.Append(ControlChars.Lf)
                    Case "t"c : sb.Append(ControlChars.Tab)
                    Case "r"c : sb.Append(ControlChars.Cr)
                    Case Else : sb.Append(s(j + 1))
                End Select
                j += 2
            Else
                sb.Append(s(j))
                j += 1
            End If
        End While
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Collect a block scalar (literal '|' or folded '&gt;') starting at the
    ''' supplied line index. Lines are consumed while they are indented more
    ''' than the key line; the first non-empty content line fixes the base
    ''' indentation. The function advances <paramref name="index"/> past the
    ''' last consumed line.
    ''' </summary>
    Private Function CollectBlockScalar(lines() As String, ByRef index As Integer, keyIndent As Integer, indicator As String) As String
        Dim style As Char = If(indicator(0) = "|"c, "|"c, ">"c)
        Dim chomp As Char = ControlChars.NullChar
        For k As Integer = 1 To indicator.Length - 1
            If indicator(k) = "-"c Then chomp = "-"c
            If indicator(k) = "+"c Then chomp = "+"c
        Next

        Dim collected As New List(Of String)
        Dim baseIndent As Integer = -1

        While index < lines.Length
            Dim raw As String = lines(index)
            Dim indent As Integer = CountIndent(raw)
            ' TrimStart removes the leading indentation; the relative indent is
            ' reconstructed from (indent - baseIndent) below so that more-deeply
            ' indented lines keep their extra indentation.
            Dim content As String = raw.TrimStart()

            If content.Length = 0 Then
                collected.Add("")
                index += 1
                Continue While
            End If

            ' A line dedented to (or below) the key level ends the block scalar.
            If indent <= keyIndent Then
                Exit While
            End If

            If baseIndent < 0 Then baseIndent = indent
            Dim rel As Integer = System.Math.Max(0, indent - baseIndent)
            collected.Add(New String(" "c, rel) & content)
            index += 1
        End While

        ' Trailing empty line handling (chomping)
        Dim trailingEmpty As Integer = 0
        For j As Integer = collected.Count - 1 To 0 Step -1
            If collected(j).Length = 0 Then trailingEmpty += 1 Else Exit For
        Next

        Dim effective As New List(Of String)(collected)
        If chomp <> "+"c Then
            ' Strip ('-') and clip (default) both drop trailing empty lines.
            If trailingEmpty > 0 Then
                effective.RemoveRange(effective.Count - trailingEmpty, trailingEmpty)
            End If
        End If

        Dim result As String
        If style = "|"c Then
            ' Literal: preserve newlines verbatim.
            result = String.Join(ControlChars.Lf, effective)
        Else
            ' Folded: join consecutive non-empty lines with a space; an empty
            ' line breaks the paragraph and becomes a single newline.
            Dim sb As New StringBuilder()
            Dim k As Integer = 0
            While k < effective.Count
                If effective(k).Length = 0 Then
                    sb.Append(ControlChars.Lf)
                    k += 1
                Else
                    Dim para As New StringBuilder(effective(k))
                    k += 1
                    While k < effective.Count AndAlso effective(k).Length > 0
                        para.Append(" "c).Append(effective(k))
                        k += 1
                    End While
                    sb.Append(para.ToString())
                    If k < effective.Count Then sb.Append(ControlChars.Lf)
                End If
            End While
            result = sb.ToString()
        End If

        ' Trailing newline according to the chomping indicator.
        If chomp = "+"c Then
            If Not result.EndsWith(ControlChars.Lf) Then result &= ControlChars.Lf
        ElseIf chomp = "-"c Then
            ' Strip: no trailing newline.
        Else
            ' Clip (default): exactly one trailing newline.
            result &= ControlChars.Lf
        End If

        Return result
    End Function

    ''' <summary>
    ''' Strip the YAML front-matter block from markdown content and return
    ''' only the body (the actual markdown instructions). This is used by
    ''' Layer 2 to obtain the full instruction text without re-parsing
    ''' the metadata block.
    ''' </summary>
    Public Function StripFrontMatter(markdownContent As String) As String
        If String.IsNullOrWhiteSpace(markdownContent) Then
            Return ""
        End If

        Dim match As Match = FrontMatterRegex.Match(markdownContent)
        If Not match.Success Then
            Return markdownContent
        End If

        Return markdownContent.Substring(match.Length).TrimStart()
    End Function

End Module
