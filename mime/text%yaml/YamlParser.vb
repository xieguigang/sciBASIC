#Region "Microsoft.VisualBasic::8e16ba821926c90cd2b27b2ce686fe88, mime\text%yaml\YamlParser.vb"

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

    '   Total Lines: 1278
    '    Code Lines: 826 (64.63%)
    ' Comment Lines: 280 (21.91%)
    '    - Xml Docs: 37.14%
    ' 
    '   Blank Lines: 172 (13.46%)
    '     File Size: 51.15 KB


    ' Class YamlParser
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: CollectQuotedString, CreateScalarValue, FinalizeBlockScalar, FindMappingColon, IsQuotedStringClosed
    '               NormalizeMultiLineQuotedString, Parse, ParseBlockScalarValue, ParseFile, ParseFlowMapping
    '               ParseFlowSequence, ParseMapping, ParseSequence, ParseValue, PreProcess
    '               SplitFlowItems, StripComment, TryParseInteger, UnescapeDoubleQuotedString, UnquoteString
    ' 
    '     Sub: Reset
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports std = System.Math

#Region "YAML Document Parser - Converts YAML to JsonElement Model"

' YamlParser.vb
'
' A YAML document parser that converts YAML text into the JsonElement object model
' (JsonObject, JsonArray, JsonValue), enabling YAML-to-JSON conversion via the
' existing JSON element model as an intermediary.
'
' Supported YAML features:
'   - Block mappings (key: value)
'   - Block sequences (- item)
'   - Multi-line strings (folded '>', literal '|', quoted single/double, continuation)
'   - Flow collections ([a, b], {k: v})
'   - Scalar type inference (string, boolean, integer, double, null)
'   - Comments (# ...)
'   - Anchors (&anchor) and aliases (*alias)
'   - Explicit document markers (---, ...)
'   - Special keys like $ref preserved as strings
'   - Merge keys (<<: *alias)
'
' Design:
'   The parser works in two phases:
'   1. Pre-processing: raw YAML text is split into logical "YamlLine" records,
'      each with an indentation level and content. Multi-line strings (block scalars
'      with | or >, and quoted multi-line strings) are joined into single logical lines.
'   2. Recursive descent parsing: the pre-processed lines are parsed into the
'      JsonElement model (JsonObject, JsonArray, JsonValue) based on indentation
'      and content structure.

#End Region


''' <summary>
''' A YAML document parser that converts YAML text into JsonElement model objects.
''' The parser supports block mappings, block sequences, multi-line strings,
''' flow collections, anchors/aliases, and automatic scalar type inference.
''' </summary>
Public Class YamlParser

#Region "Public API"

    ''' <summary>
    ''' Parse a YAML document string into a JsonElement model tree.
    ''' If the YAML contains multiple documents separated by '---', only the
    ''' first document is parsed.
    ''' </summary>
    ''' <param name="yamlText">The YAML document text.</param>
    ''' <returns>The root JsonElement (typically a JsonObject).</returns>
    Public Function Parse(yamlText As String) As JsonElement
        If String.IsNullOrEmpty(yamlText) Then
            Return New JsonObject()
        End If

        ' Phase 1: Pre-process the raw YAML text into logical lines
        Dim lines As List(Of YamlLine) = PreProcess(yamlText)

        ' Phase 2: Parse the logical lines into the JsonElement model
        Dim index As Integer = 0
        Return ParseValue(lines, index, 0)
    End Function

    ''' <summary>
    ''' Parse a YAML document from a file path into a JsonElement model tree.
    ''' </summary>
    ''' <param name="filePath">The path to the YAML file.</param>
    ''' <returns>The root JsonElement (typically a JsonObject).</returns>
    Public Function ParseFile(filePath As String) As JsonElement
        Dim yamlText As String = System.IO.File.ReadAllText(filePath, Encoding.UTF8)
        Return Parse(yamlText)
    End Function

#End Region

#Region "Phase 1: Pre-processing"

    ''' <summary>
    ''' Pre-process the raw YAML text into a list of logical YamlLine records.
    ''' This handles:
    '''   - Stripping comments (outside of quoted strings)
    '''   - Joining multi-line block scalars (| and >)
    '''   - Joining multi-line quoted strings
    '''   - Tracking indentation levels
    '''   - Skipping document markers (---, ...) and empty lines
    ''' </summary>
    Private Function PreProcess(yamlText As String) As List(Of YamlLine)
        Dim rawLines() As String = yamlText.Split({ControlChars.Lf}, StringSplitOptions.None)
        Dim result As New List(Of YamlLine)
        Dim mlState As MultiLineState = Nothing

        Dim i As Integer = 0
        While i < rawLines.Length
            Dim raw As String = rawLines(i)
            Dim lineNum As Integer = i + 1

            ' Calculate indentation
            Dim indent As Integer = 0
            While indent < raw.Length AndAlso raw(indent) = " "c
                indent += 1
            End While

            ' Also count tabs as indentation (each tab = 2 spaces equivalent)
            Dim tempIndent As Integer = indent
            While tempIndent < raw.Length AndAlso raw(tempIndent) = ControlChars.Tab
                indent += 2
                tempIndent += 1
            End While

            ' Get the content after indentation
            Dim content As String = If(tempIndent < raw.Length, raw.Substring(tempIndent).TrimEnd(), "")

            ' -------------------------------------------------------
            ' Handle multi-line block scalar collection (| or >)
            ' -------------------------------------------------------
            If mlState IsNot Nothing Then
                ' A block scalar continues as long as the line is either:
                ' - Empty/whitespace-only
                ' - Indented more than the key line's indentation
                ' We determine the base indentation from the first content line.
                If content.Length = 0 OrElse indent > mlState.BaseIndent Then
                    If mlState.BaseIndent < 0 AndAlso content.Length > 0 Then
                        ' First content line determines base indentation
                        mlState.BaseIndent = indent
                    End If

                    ' Add the line content (relative to base indentation)
                    If content.Length > 0 Then
                        Dim relativeIndent As Integer = std.Max(0, indent - mlState.BaseIndent)
                        Dim lineContent As String = New String(" "c, relativeIndent) & content
                        mlState.Lines.Add(lineContent)
                    Else
                        ' Empty line within block scalar
                        mlState.Lines.Add("")
                    End If

                    i += 1
                    Continue While
                Else
                    ' Block scalar has ended; finalize it
                    Dim blockValue As String = FinalizeBlockScalar(mlState)
                    result.Add(New YamlLine With {
                        .Indent = indent,
                        .Content = mlState.Key & " " & blockValue,
                        .LineNumber = mlState.StartLineNumber
                    })
                    mlState = Nothing
                    ' Don't increment i; re-process the current line
                    Continue While
                End If
            End If

            ' -------------------------------------------------------
            ' Skip empty lines and comment-only lines
            ' -------------------------------------------------------
            If content.Length = 0 Then
                i += 1
                Continue While
            End If

            ' Skip document markers
            If content = "---" OrElse content = "..." Then
                i += 1
                Continue While
            End If

            ' -------------------------------------------------------
            ' Check for block scalar indicators (| or >)
            ' -------------------------------------------------------
            Dim colonPos As Integer = FindMappingColon(content)
            If colonPos >= 0 Then
                Dim keyPart As String = content.Substring(0, colonPos).Trim()
                Dim valuePart As String = content.Substring(colonPos + 1).Trim()

                ' Strip inline comment from value part
                valuePart = StripComment(valuePart)

                If valuePart = "|" OrElse valuePart = ">" Then
                    ' Start collecting a block scalar
                    mlState = New MultiLineState()
                    mlState.Style = valuePart(0)
                    mlState.Key = keyPart
                    mlState.StartLineNumber = lineNum
                    i += 1
                    Continue While
                End If

                ' Handle block scalar with chomping/indent indicators
                ' e.g., key: |+, key: >-, key: |2
                If valuePart.Length > 0 AndAlso (valuePart(0) = "|"c OrElse valuePart(0) = ">"c) Then
                    Dim rest As String = valuePart.Substring(1)
                    Dim isBlockScalar As Boolean = True

                    ' Check that the rest is only chomping/indent indicators
                    For Each ch As Char In rest
                        If ch <> "+"c AndAlso ch <> "-"c AndAlso Not Char.IsDigit(ch) Then
                            isBlockScalar = False
                            Exit For
                        End If
                    Next

                    If isBlockScalar Then
                        mlState = New MultiLineState()
                        mlState.Style = valuePart(0)
                        mlState.Key = keyPart
                        mlState.StartLineNumber = lineNum

                        ' Parse indicators
                        For Each ch As Char In rest
                            If ch = "+"c Then mlState.ChompPlus = True
                            If ch = "-"c Then mlState.ChompMinus = True
                            If Char.IsDigit(ch) Then mlState.IndentIndicator = Val(ch.ToString())
                        Next

                        i += 1
                        Continue While
                    End If
                End If
            End If

            ' -------------------------------------------------------
            ' Handle multi-line quoted strings
            ' e.g., description: 'line1
            '   line2
            '   line3'
            ' -------------------------------------------------------
            If colonPos >= 0 Then
                Dim keyPart As String = content.Substring(0, colonPos).Trim()
                Dim valuePart As String = content.Substring(colonPos + 1).TrimStart()

                If valuePart.Length > 0 AndAlso (valuePart(0) = "'"c OrElse valuePart(0) = """"c) Then
                    Dim quoteChar As Char = valuePart(0)
                    Dim fullValue As String = CollectQuotedString(valuePart, rawLines, i, indent)

                    If fullValue IsNot Nothing Then
                        content = keyPart & ": " & fullValue
                    End If
                End If
            End If

            ' -------------------------------------------------------
            ' Strip comments from the content (outside quoted strings)
            ' -------------------------------------------------------
            content = StripComment(content)

            If content.Length > 0 Then
                result.Add(New YamlLine With {
                    .Indent = indent,
                    .Content = content,
                    .LineNumber = lineNum
                })
            End If

            i += 1
        End While

        ' Finalize any remaining block scalar
        If mlState IsNot Nothing Then
            Dim blockValue As String = FinalizeBlockScalar(mlState)
            result.Add(New YamlLine With {
                .Indent = 0,
                .Content = mlState.Key & " " & blockValue,
                .LineNumber = mlState.StartLineNumber
            })
        End If

        Return result
    End Function

    ''' <summary>
    ''' Collect a multi-line quoted string that spans multiple lines.
    ''' Returns the complete quoted string (including quotes), or Nothing if
    ''' the string is complete on a single line.
    ''' </summary>
    Private Function CollectQuotedString(firstPart As String, rawLines() As String,
                                         ByRef lineIndex As Integer, baseIndent As Integer) As String
        Dim quoteChar As Char = firstPart(0)

        ' Check if the string is already closed on this line
        If IsQuotedStringClosed(firstPart, quoteChar) Then
            Return Nothing ' Single-line quoted string, no special handling needed
        End If

        ' Multi-line quoted string: collect until we find the closing quote
        Dim sb As New StringBuilder()
        sb.Append(firstPart)

        Dim j As Integer = lineIndex + 1
        While j < rawLines.Length
            Dim nextLine As String = rawLines(j).TrimEnd()
            Dim nextContent As String = nextLine.TrimStart()

            If nextContent.Length = 0 Then
                ' Empty line within quoted string - preserve a newline
                sb.Append(ControlChars.Lf)
                j += 1
                Continue While
            End If

            ' Append the content (with a space separator for readability)
            sb.Append(" ")
            sb.Append(nextContent)

            ' Check if the accumulated string now has a matching closing quote
            ' We check the full accumulated string from the opening quote
            If IsQuotedStringClosed(sb.ToString(), quoteChar) Then
                lineIndex = j ' Update the line index to skip consumed lines
                ' Return the complete multi-line quoted string
                Dim fullStr As String = sb.ToString()
                ' Normalize the multi-line string content
                Return NormalizeMultiLineQuotedString(fullStr, quoteChar)
            End If

            j += 1
        End While

        ' Unclosed string - return what we have
        Return NormalizeMultiLineQuotedString(sb.ToString(), quoteChar)
    End Function

    ''' <summary>
    ''' Check if a string that starts with a quote character has a matching closing quote.
    ''' Handles escaped quotes within the string.
    ''' </summary>
    Private Function IsQuotedStringClosed(s As String, quoteChar As Char) As Boolean
        If s.Length < 2 Then Return False

        Dim i As Integer = 1 ' Start after the opening quote
        While i < s.Length
            If quoteChar = "'"c Then
                ' In single-quoted strings, '' is an escaped single quote
                If s(i) = "'"c Then
                    If i + 1 < s.Length AndAlso s(i + 1) = "'"c Then
                        i += 2 ' Skip escaped quote
                        Continue While
                    ElseIf i = s.Length - 1 Then
                        Return True ' Closing quote found
                    Else
                        Return False ' Quote in the middle (not closing)
                    End If
                End If
            ElseIf quoteChar = """"c Then
                ' In double-quoted strings, \" is an escaped double quote
                If s(i) = "\"c Then
                    i += 2 ' Skip escaped character
                    Continue While
                ElseIf s(i) = """"c Then
                    Return True ' Closing quote found
                End If
            End If
            i += 1
        End While

        Return False
    End Function

    ''' <summary>
    ''' Normalize a multi-line quoted string by extracting the content between quotes
    ''' and re-joining it as a single-line string with proper escaping.
    ''' </summary>
    Private Function NormalizeMultiLineQuotedString(s As String, quoteChar As Char) As String
        ' Extract content between the opening and closing quotes
        Dim content As String

        If quoteChar = "'"c Then
            ' Find the first and last single quote
            Dim firstQuote As Integer = s.IndexOf("'"c)
            Dim lastQuote As Integer = s.LastIndexOf("'"c)

            If firstQuote >= 0 AndAlso lastQuote > firstQuote Then
                content = s.Substring(firstQuote + 1, lastQuote - firstQuote - 1)
                ' In single-quoted strings, '' is an escaped single quote
                content = content.Replace("''", "'")
            Else
                content = s
            End If
        Else
            ' Double-quoted string
            Dim firstQuote As Integer = s.IndexOf(""""c)
            Dim lastQuote As Integer = s.LastIndexOf(""""c)

            If firstQuote >= 0 AndAlso lastQuote > firstQuote Then
                content = s.Substring(firstQuote + 1, lastQuote - firstQuote - 1)
            Else
                content = s
            End If
        End If

        ' Normalize whitespace: replace newlines and surrounding spaces with a single space
        content = Regex.Replace(content, "\s*\n\s*", " ")
        content = content.Trim()

        ' Re-wrap in quotes
        If quoteChar = "'"c Then
            ' Escape single quotes for the output
            content = content.Replace("'", "''")
            Return "'" & content & "'"
        Else
            ' Escape double quotes and backslashes
            content = content.Replace("\", "\\")
            content = content.Replace("""", "\""")
            Return """" & content & """"
        End If
    End Function

    ''' <summary>
    ''' Finalize a block scalar (| or >) by joining the collected lines
    ''' according to the block scalar style.
    ''' </summary>
    Private Function FinalizeBlockScalar(state As MultiLineState) As String
        If state.Lines.Count = 0 Then
            If state.ChompPlus Then
                Return "|\n\n"
            End If
            Return "|\n"
        End If

        ' Remove trailing empty lines (for clip and strip chomping)
        Dim trailingEmpty As Integer = 0
        For j As Integer = state.Lines.Count - 1 To 0 Step -1
            If state.Lines(j).Length = 0 Then
                trailingEmpty += 1
            Else
                Exit For
            End If
        Next

        Dim effectiveLines As New List(Of String)(state.Lines)
        If state.ChompMinus Then
            ' Strip: remove all trailing empty lines
            effectiveLines.RemoveRange(state.Lines.Count - trailingEmpty, trailingEmpty)
        ElseIf Not state.ChompPlus Then
            ' Clip (default): keep one trailing newline
            effectiveLines.RemoveRange(state.Lines.Count - trailingEmpty, trailingEmpty)
        End If
        ' Keep (+: keep all trailing empty lines) - no removal

        Dim result As String
        If state.Style = "|"c Then
            ' Literal: preserve newlines
            result = "|" & ControlChars.Lf & String.Join(ControlChars.Lf, effectiveLines)
        Else
            ' Folded: replace single newlines with spaces, double newlines with single newline
            Dim sb As New StringBuilder()
            Dim k As Integer = 0
            While k < effectiveLines.Count
                If effectiveLines(k).Length = 0 Then
                    ' Empty line becomes a newline
                    sb.Append(ControlChars.Lf)
                    k += 1
                Else
                    ' Collect consecutive non-empty lines and join with spaces
                    Dim paragraph As New StringBuilder(effectiveLines(k))
                    k += 1
                    While k < effectiveLines.Count AndAlso effectiveLines(k).Length > 0
                        paragraph.Append(" "c)
                        paragraph.Append(effectiveLines(k))
                        k += 1
                    End While
                    sb.Append(paragraph.ToString())
                    If k < effectiveLines.Count Then
                        sb.Append(ControlChars.Lf)
                    End If
                End If
            End While
            result = ">" & ControlChars.Lf & sb.ToString()
        End If

        ' Add trailing newline based on chomping
        If state.ChompPlus Then
            result &= ControlChars.Lf
        ElseIf Not state.ChompMinus Then
            result &= ControlChars.Lf
        End If

        Return result
    End Function

#End Region

#Region "Phase 2: Recursive descent parsing"

    ''' <summary>
    ''' Parse a value starting at the given line index and indentation level.
    ''' Determines the type of value (mapping, sequence, or scalar) and delegates
    ''' to the appropriate parsing method.
    ''' </summary>
    Private Function ParseValue(lines As List(Of YamlLine), ByRef index As Integer, baseIndent As Integer) As JsonElement
        If index >= lines.Count Then
            Return JsonValue.NULL
        End If

        Dim line As YamlLine = lines(index)

        ' Determine what type of value this is
        If line.Content.StartsWith("- ") OrElse line.Content = "-" Then
            ' Block sequence
            Return ParseSequence(lines, index, baseIndent)
        ElseIf FindMappingColon(line.Content) >= 0 Then
            ' Block mapping
            Return ParseMapping(lines, index, baseIndent)
        ElseIf line.Content.StartsWith("[") Then
            ' Flow sequence
            Dim result As JsonArray = ParseFlowSequence(line.Content)
            index += 1
            Return result
        ElseIf line.Content.StartsWith("{") Then
            ' Flow mapping
            Dim result As JsonObject = ParseFlowMapping(line.Content)
            index += 1
            Return result
        Else
            ' Scalar value
            Dim result As JsonElement = CreateScalarValue(line.Content)
            index += 1
            Return result
        End If
    End Function

    ''' <summary>
    ''' Parse a block mapping (key: value pairs) starting at the given line.
    ''' </summary>
    Private Function ParseMapping(lines As List(Of YamlLine), ByRef index As Integer, baseIndent As Integer) As JsonObject
        Dim obj As New JsonObject()

        While index < lines.Count
            Dim line As YamlLine = lines(index)

            ' Stop if we've dedented past the base level
            If line.Indent < baseIndent Then Exit While

            ' Stop if this is a sequence item at the same or lesser indentation
            If line.Content.StartsWith("- ") AndAlso line.Indent <= baseIndent Then Exit While

            ' Find the colon separator
            Dim colonPos As Integer = FindMappingColon(line.Content)
            If colonPos < 0 Then Exit While

            ' Extract the key
            Dim key As String = line.Content.Substring(0, colonPos).Trim()

            ' Handle anchors on the key
            Dim anchorName As String = Nothing
            Dim anchorMatch As Match = Regex.Match(key, "&(\w+)")
            If anchorMatch.Success Then
                anchorName = anchorMatch.Groups(1).Value
                key = key.Replace(anchorMatch.Value, "").Trim()
            End If

            ' Handle alias on the key (merge key)
            If key = "<<" Then
                ' Merge key - handle alias
                Dim valuePart As String = line.Content.Substring(colonPos + 1).Trim()
                Dim aliasMatch As Match = Regex.Match(valuePart, "\*(\w+)")
                If aliasMatch.Success Then
                    Dim aliasName As String = aliasMatch.Groups(1).Value
                    If _anchors.ContainsKey(aliasName) Then
                        Dim mergeSource As JsonElement = _anchors(aliasName)
                        If TypeOf mergeSource Is JsonObject Then
                            Dim mergeObj As JsonObject = DirectCast(mergeSource, JsonObject)
                            For Each mergeKey As String In mergeObj.ObjectKeys
                                If Not obj.HasObjectKey(mergeKey) Then
                                    obj.Add(mergeKey, mergeObj(mergeKey))
                                End If
                            Next
                        End If
                    End If
                End If
                index += 1
                Continue While
            End If

            ' Extract the value part (after the colon)
            Dim valueStr As String = line.Content.Substring(colonPos + 1).Trim()

            ' Remove inline comment from value
            valueStr = StripComment(valueStr)

            ' Parse the value
            Dim value As JsonElement

            If valueStr.Length = 0 Then
                ' Value is on subsequent lines (nested structure)
                index += 1
                If index >= lines.Count Then
                    ' No more lines - null value
                    value = JsonValue.NULL
                Else
                    Dim nextLine As YamlLine = lines(index)
                    If nextLine.Indent <= line.Indent AndAlso Not nextLine.IsListItem Then
                        ' Next line is at same or lesser indentation - null value
                        value = JsonValue.NULL
                    Else
                        ' Parse the nested value
                        value = ParseValue(lines, index, nextLine.Indent)
                    End If
                End If
            ElseIf valueStr.StartsWith("[") Then
                ' Flow sequence on the same line
                value = ParseFlowSequence(valueStr)
                index += 1
            ElseIf valueStr.StartsWith("{") Then
                ' Flow mapping on the same line
                value = ParseFlowMapping(valueStr)
                index += 1
            ElseIf valueStr.StartsWith("|") OrElse valueStr.StartsWith(">") Then
                ' Block scalar (already pre-processed into a single-line representation)
                value = ParseBlockScalarValue(valueStr)
                index += 1
            Else
                ' Inline scalar value
                ' Handle alias reference
                Dim aliasMatch2 As Match = Regex.Match(valueStr, "\*(\w+)")
                If aliasMatch2.Success AndAlso valueStr.Trim() = "*" & aliasMatch2.Groups(1).Value Then
                    Dim aliasName As String = aliasMatch2.Groups(1).Value
                    If _anchors.ContainsKey(aliasName) Then
                        value = _anchors(aliasName)
                    Else
                        value = CreateScalarValue(valueStr)
                    End If
                Else
                    value = CreateScalarValue(valueStr)
                End If
                index += 1
            End If

            ' Store anchor if present
            If anchorName IsNot Nothing Then
                _anchors(anchorName) = value
            End If

            ' Add the key-value pair to the object
            obj.Add(key, value)
        End While

        Return obj
    End Function

    ''' <summary>
    ''' Parse a block sequence (list items starting with '-') starting at the given line.
    ''' </summary>
    Private Function ParseSequence(lines As List(Of YamlLine), ByRef index As Integer, baseIndent As Integer) As JsonArray
        Dim arr As New JsonArray()

        While index < lines.Count
            Dim line As YamlLine = lines(index)

            ' Stop if we've dedented past the base level
            If line.Indent < baseIndent Then Exit While

            ' Stop if this is a mapping at the same or lesser indentation
            If Not line.Content.StartsWith("- ") AndAlso line.Content <> "-" Then Exit While

            ' Calculate the dash position and content indent
            Dim dashIndent As Integer = line.Indent
            Dim contentAfterDash As String = line.Content.Substring(2).Trim()

            ' Remove inline comment
            contentAfterDash = StripComment(contentAfterDash)

            Dim itemValue As JsonElement

            If contentAfterDash.Length = 0 Then
                ' Empty sequence item - could be a nested structure on next lines
                index += 1
                If index >= lines.Count Then
                    itemValue = JsonValue.NULL
                Else
                    Dim nextLine As YamlLine = lines(index)
                    If nextLine.Indent <= dashIndent Then
                        itemValue = JsonValue.NULL
                    Else
                        itemValue = ParseValue(lines, index, nextLine.Indent)
                    End If
                End If
            ElseIf contentAfterDash.StartsWith("[") Then
                ' Flow sequence as a sequence item
                itemValue = ParseFlowSequence(contentAfterDash)
                index += 1
            ElseIf contentAfterDash.StartsWith("{") Then
                ' Flow mapping as a sequence item
                itemValue = ParseFlowMapping(contentAfterDash)
                index += 1
            ElseIf FindMappingColon(contentAfterDash) >= 0 Then
                ' Mapping key: value on the same line as the dash
                ' This is a compact mapping notation: - key: value
                ' We need to parse this as a mapping
                '
                ' The tricky part: the mapping might continue on subsequent lines
                ' with the same indentation as the content after the dash.
                '
                ' Create a synthetic line for the mapping content
                Dim mappingLines As New List(Of YamlLine)
                Dim contentIndent As Integer = dashIndent + 2 ' Content is indented 2 more than the dash

                ' Add the current line's content as a mapping line
                mappingLines.Add(New YamlLine With {
                    .Indent = contentIndent,
                    .Content = contentAfterDash,
                    .LineNumber = line.LineNumber
                })

                ' Collect subsequent lines that belong to this mapping
                index += 1
                While index < lines.Count
                    Dim nextLine As YamlLine = lines(index)
                    If nextLine.Indent < contentIndent Then Exit While
                    If nextLine.Content.StartsWith("- ") AndAlso nextLine.Indent <= dashIndent Then Exit While
                    mappingLines.Add(nextLine)
                    index += 1
                End While

                ' Parse the mapping
                Dim mappingIndex As Integer = 0
                itemValue = ParseMapping(mappingLines, mappingIndex, contentIndent)
            Else
                ' Scalar value
                itemValue = CreateScalarValue(contentAfterDash)
                index += 1
            End If

            arr.Add(itemValue)
        End While

        Return arr
    End Function

#End Region

#Region "Flow collection parsing"

    ''' <summary>
    ''' Parse a flow sequence (inline array like [a, b, c]).
    ''' </summary>
    Private Function ParseFlowSequence(s As String) As JsonArray
        Dim arr As New JsonArray()

        ' Remove surrounding brackets
        s = s.Trim()
        If s.StartsWith("[") Then s = s.Substring(1)
        If s.EndsWith("]") Then s = s.Substring(0, s.Length - 1)

        s = s.Trim()
        If s.Length = 0 Then Return arr

        ' Split by commas (respecting nested structures)
        Dim items As List(Of String) = SplitFlowItems(s, ","c)

        For Each item As String In items
            Dim trimmed As String = item.Trim()
            If trimmed.Length = 0 Then Continue For

            If trimmed.StartsWith("{") Then
                arr.Add(ParseFlowMapping(trimmed))
            ElseIf trimmed.StartsWith("[") Then
                arr.Add(ParseFlowSequence(trimmed))
            Else
                arr.Add(CreateScalarValue(trimmed))
            End If
        Next

        Return arr
    End Function

    ''' <summary>
    ''' Parse a flow mapping (inline object like {key: value, key2: value2}).
    ''' </summary>
    Private Function ParseFlowMapping(s As String) As JsonObject
        Dim obj As New JsonObject()

        ' Remove surrounding braces
        s = s.Trim()
        If s.StartsWith("{") Then s = s.Substring(1)
        If s.EndsWith("}") Then s = s.Substring(0, s.Length - 1)

        s = s.Trim()
        If s.Length = 0 Then Return obj

        ' Split by commas (respecting nested structures)
        Dim items As List(Of String) = SplitFlowItems(s, ","c)

        For Each item As String In items
            Dim trimmed As String = item.Trim()
            If trimmed.Length = 0 Then Continue For

            ' Find the colon separator
            Dim colonPos As Integer = FindMappingColon(trimmed)
            If colonPos >= 0 Then
                Dim key As String = trimmed.Substring(0, colonPos).Trim()
                Dim valueStr As String = trimmed.Substring(colonPos + 1).Trim()

                ' Unquote the key if necessary
                key = UnquoteString(key)

                Dim value As JsonElement
                If valueStr.StartsWith("{") Then
                    value = ParseFlowMapping(valueStr)
                ElseIf valueStr.StartsWith("[") Then
                    value = ParseFlowSequence(valueStr)
                Else
                    value = CreateScalarValue(valueStr)
                End If

                obj.Add(key, value)
            End If
        Next

        Return obj
    End Function

    ''' <summary>
    ''' Split a flow collection string by a separator character, respecting
    ''' nested brackets, braces, and quoted strings.
    ''' </summary>
    Private Function SplitFlowItems(s As String, separator As Char) As List(Of String)
        Dim result As New List(Of String)
        Dim current As New StringBuilder()
        Dim depth As Integer = 0
        Dim inSingleQuote As Boolean = False
        Dim inDoubleQuote As Boolean = False
        Dim i As Integer = 0

        While i < s.Length
            Dim ch As Char = s(i)

            If inSingleQuote Then
                current.Append(ch)
                If ch = "'"c Then
                    ' Check for escaped single quote
                    If i + 1 < s.Length AndAlso s(i + 1) = "'"c Then
                        current.Append(s(i + 1))
                        i += 2
                        Continue While
                    End If
                    inSingleQuote = False
                End If
            ElseIf inDoubleQuote Then
                current.Append(ch)
                If ch = "\"c AndAlso i + 1 < s.Length Then
                    current.Append(s(i + 1))
                    i += 2
                    Continue While
                End If
                If ch = """"c Then inDoubleQuote = False
            Else
                If ch = "'"c Then
                    inSingleQuote = True
                    current.Append(ch)
                ElseIf ch = """"c Then
                    inDoubleQuote = True
                    current.Append(ch)
                ElseIf ch = "{"c OrElse ch = "["c Then
                    depth += 1
                    current.Append(ch)
                ElseIf ch = "}"c OrElse ch = "]"c Then
                    depth -= 1
                    current.Append(ch)
                ElseIf ch = separator AndAlso depth = 0 Then
                    result.Add(current.ToString())
                    current.Clear()
                Else
                    current.Append(ch)
                End If
            End If

            i += 1
        End While

        If current.Length > 0 Then
            result.Add(current.ToString())
        End If

        Return result
    End Function

#End Region

#Region "Block scalar value parsing"

    ''' <summary>
    ''' Parse a block scalar value that has been pre-processed into a single-line
    ''' representation. The format is: |content or >content
    ''' where content contains embedded newlines.
    ''' </summary>
    Private Function ParseBlockScalarValue(s As String) As JsonValue
        If s.Length <= 1 Then
            Return New JsonValue("")
        End If

        ' The block scalar content starts after the | or > character
        ' and the first newline
        Dim contentStart As Integer = s.IndexOf(ControlChars.Lf)
        If contentStart < 0 Then
            Return New JsonValue("")
        End If

        Dim content As String = s.Substring(contentStart + 1)

        ' For literal style (|), the content is preserved as-is
        ' For folded style (>), the content has already been folded during pre-processing
        ' In both cases, we strip the trailing newline that was added by the chomping logic
        If content.EndsWith(ControlChars.Lf) Then
            content = content.Substring(0, content.Length - 1)
        End If

        Return New JsonValue(content)
    End Function

#End Region

#Region "Scalar value creation"

    ''' <summary>
    ''' Create a JsonValue from a YAML scalar string, performing type inference.
    ''' The following types are recognized:
    '''   - null (null, Null, NULL, ~)
    '''   - boolean (true, false, True, False, TRUE, FALSE)
    '''   - integer (decimal, hex 0x..., octal 0o..., binary 0b...)
    '''   - double (decimal with point, scientific notation, .inf, -.inf, .nan)
    '''   - string (everything else, including quoted strings)
    ''' </summary>
    Private Function CreateScalarValue(s As String) As JsonValue
        If s Is Nothing Then Return JsonValue.NULL

        s = s.Trim()
        If s.Length = 0 Then Return New JsonValue("")

        ' Handle quoted strings
        If s.Length >= 2 Then
            If s(0) = "'"c AndAlso s(s.Length - 1) = "'"c Then
                ' Single-quoted string: remove quotes and unescape ''
                Dim content As String = s.Substring(1, s.Length - 2)
                content = content.Replace("''", "'")
                Return New JsonValue(content)
            End If

            If s(0) = """"c AndAlso s(s.Length - 1) = """"c Then
                ' Double-quoted string: remove quotes and unescape
                Dim content As String = s.Substring(1, s.Length - 2)
                content = UnescapeDoubleQuotedString(content)
                Return New JsonValue(content)
            End If
        End If

        ' Handle null
        If s = "null" OrElse s = "Null" OrElse s = "NULL" OrElse s = "~" Then
            Return JsonValue.NULL
        End If

        ' Handle boolean
        If s = "true" OrElse s = "True" OrElse s = "TRUE" Then
            Return New JsonValue(True)
        End If
        If s = "false" OrElse s = "False" OrElse s = "FALSE" Then
            Return New JsonValue(False)
        End If

        ' Handle integer
        Dim intVal As Long
        If TryParseInteger(s, intVal) Then
            If intVal >= Integer.MinValue AndAlso intVal <= Integer.MaxValue Then
                Return New JsonValue(CInt(intVal))
            Else
                Return New JsonValue(intVal)
            End If
        End If

        ' Handle double
        Dim dblVal As Double
        If s = ".inf" OrElse s = ".Inf" OrElse s = ".INF" Then
            Return New JsonValue(Double.PositiveInfinity)
        End If
        If s = "-.inf" OrElse s = "-.Inf" OrElse s = "-.INF" Then
            Return New JsonValue(Double.NegativeInfinity)
        End If
        If s = ".nan" OrElse s = ".NaN" OrElse s = ".NAN" Then
            Return New JsonValue(Double.NaN)
        End If
        If Double.TryParse(s, Globalization.NumberStyles.Float,
                           Globalization.CultureInfo.InvariantCulture, dblVal) Then
            Return New JsonValue(dblVal)
        End If

        ' Default: string
        Return New JsonValue(s)
    End Function

    ''' <summary>
    ''' Try to parse a string as an integer (decimal, hex, octal, or binary).
    ''' </summary>
    Private Function TryParseInteger(s As String, ByRef result As Long) As Boolean
        ' Decimal integer (with optional sign)
        If Regex.IsMatch(s, "^[+-]?\d+$") Then
            Return Long.TryParse(s, Globalization.NumberStyles.Integer,
                                 Globalization.CultureInfo.InvariantCulture, result)
        End If

        ' Hexadecimal (0x prefix)
        If s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) Then
            Dim hexStr As String = s.Substring(2)
            If Regex.IsMatch(hexStr, "^[0-9a-fA-F]+$") Then
                Return Long.TryParse(hexStr, Globalization.NumberStyles.HexNumber,
                                     Globalization.CultureInfo.InvariantCulture, result)
            End If
        End If

        ' Octal (0o prefix)
        If s.StartsWith("0o", StringComparison.OrdinalIgnoreCase) Then
            Dim octStr As String = s.Substring(2)
            If Regex.IsMatch(octStr, "^[0-7]+$") Then
                Try
                    result = Convert.ToInt64(octStr, 8)
                    Return True
                Catch
                    Return False
                End Try
            End If
        End If

        ' Binary (0b prefix)
        If s.StartsWith("0b", StringComparison.OrdinalIgnoreCase) Then
            Dim binStr As String = s.Substring(2)
            If Regex.IsMatch(binStr, "^[01]+$") Then
                Try
                    result = Convert.ToInt64(binStr, 2)
                    Return True
                Catch
                    Return False
                End Try
            End If
        End If

        ' Octal with leading 0 (YAML 1.1 style: 0123 = 83)
        ' Note: YAML 1.2 dropped this, but some files still use it
        ' We skip this to avoid ambiguity with decimal numbers like "07"

        Return False
    End Function

    ''' <summary>
    ''' Unescape a double-quoted YAML string content (without surrounding quotes).
    ''' Supports: \\, \", \/, \a, \b, \e, \f, \n, \r, \t, \v, \0, \xNN, \uNNNN, \UNNNNNNNN
    ''' </summary>
    Private Function UnescapeDoubleQuotedString(s As String) As String
        If s Is Nothing Then Return Nothing
        If Not s.Contains("\"c) Then Return s

        Dim sb As New StringBuilder(s.Length)
        Dim i As Integer = 0

        Const Bell As Char = ChrW(7)

        While i < s.Length
            If s(i) = "\"c AndAlso i + 1 < s.Length Then
                Dim nextCh As Char = s(i + 1)
                Select Case nextCh
                    Case "\"c : sb.Append("\"c) : i += 2
                    Case """"c : sb.Append(""""c) : i += 2
                    Case "/"c : sb.Append("/"c) : i += 2
                    Case "a"c : sb.Append(Bell) : i += 2
                    Case "b"c : sb.Append(ControlChars.Back) : i += 2
                    Case "e"c : sb.Append(ChrW(&H1B)) : i += 2  ' Escape
                    Case "f"c : sb.Append(ControlChars.FormFeed) : i += 2
                    Case "n"c : sb.Append(ControlChars.Lf) : i += 2
                    Case "r"c : sb.Append(ControlChars.Cr) : i += 2
                    Case "t"c : sb.Append(ControlChars.Tab) : i += 2
                    Case "v"c : sb.Append(ControlChars.VerticalTab) : i += 2
                    Case "0"c : sb.Append(ChrW(0)) : i += 2
                    Case " "c : sb.Append(" "c) : i += 2
                    Case "x"c, "X"c
                        ' Hex escape: \xNN
                        If i + 3 < s.Length Then
                            Dim hexStr As String = s.Substring(i + 2, 2)
                            Dim code As Integer
                            If Integer.TryParse(hexStr, Globalization.NumberStyles.HexNumber,
                                                Globalization.CultureInfo.InvariantCulture, code) Then
                                sb.Append(ChrW(code))
                                i += 4
                            Else
                                sb.Append(s(i))
                                i += 1
                            End If
                        Else
                            sb.Append(s(i))
                            i += 1
                        End If
                    Case "u"c
                        ' Unicode escape: \uNNNN
                        If i + 5 < s.Length Then
                            Dim hexStr As String = s.Substring(i + 2, 4)
                            Dim code As Integer
                            If Integer.TryParse(hexStr, Globalization.NumberStyles.HexNumber,
                                                Globalization.CultureInfo.InvariantCulture, code) Then
                                sb.Append(ChrW(code))
                                i += 6
                            Else
                                sb.Append(s(i))
                                i += 1
                            End If
                        Else
                            sb.Append(s(i))
                            i += 1
                        End If
                    Case "U"c
                        ' Unicode escape: \UNNNNNNNN
                        If i + 9 < s.Length Then
                            Dim hexStr As String = s.Substring(i + 2, 8)
                            Dim code As Long
                            If Long.TryParse(hexStr, Globalization.NumberStyles.HexNumber,
                                             Globalization.CultureInfo.InvariantCulture, code) Then
                                sb.Append(Char.ConvertFromUtf32(CInt(code)))
                                i += 10
                            Else
                                sb.Append(s(i))
                                i += 1
                            End If
                        Else
                            sb.Append(s(i))
                            i += 1
                        End If
                    Case Else
                        ' Unknown escape - keep as-is
                        sb.Append(s(i))
                        i += 1
                End Select
            Else
                sb.Append(s(i))
                i += 1
            End If
        End While

        Return sb.ToString()
    End Function

#End Region

#Region "Utility methods"

    ''' <summary>
    ''' Find the position of the mapping colon (':') in a line, respecting
    ''' quoted strings and flow collections. Returns -1 if no mapping colon found.
    ''' A colon is only a mapping colon if it is followed by a space or end-of-line.
    ''' </summary>
    Private Function FindMappingColon(s As String) As Integer
        Dim inSingleQuote As Boolean = False
        Dim inDoubleQuote As Boolean = False
        Dim depth As Integer = 0

        For i As Integer = 0 To s.Length - 1
            Dim ch As Char = s(i)

            If inSingleQuote Then
                If ch = "'"c Then
                    ' Check for escaped single quote
                    If i + 1 < s.Length AndAlso s(i + 1) = "'"c Then
                        i += 1 ' Skip the next quote
                    Else
                        inSingleQuote = False
                    End If
                End If
            ElseIf inDoubleQuote Then
                If ch = "\"c AndAlso i + 1 < s.Length Then
                    i += 1 ' Skip escaped character
                ElseIf ch = """"c Then
                    inDoubleQuote = False
                End If
            Else
                If ch = "'"c Then
                    inSingleQuote = True
                ElseIf ch = """"c Then
                    inDoubleQuote = True
                ElseIf ch = "{"c OrElse ch = "["c Then
                    depth += 1
                ElseIf ch = "}"c OrElse ch = "]"c Then
                    depth -= 1
                ElseIf ch = ":"c AndAlso depth = 0 Then
                    ' A colon is a mapping separator if:
                    ' - It is followed by a space
                    ' - Or it is at the end of the string
                    If i + 1 >= s.Length OrElse s(i + 1) = " "c Then
                        Return i
                    End If
                End If
            End If
        Next

        Return -1
    End Function

    ''' <summary>
    ''' Strip a trailing inline comment from a string, respecting quoted strings.
    ''' Comments start with '#' and continue to the end of the line.
    ''' </summary>
    Private Function StripComment(s As String) As String
        If s Is Nothing Then Return Nothing

        Dim inSingleQuote As Boolean = False
        Dim inDoubleQuote As Boolean = False

        For i As Integer = 0 To s.Length - 1
            Dim ch As Char = s(i)

            If inSingleQuote Then
                If ch = "'"c Then
                    If i + 1 < s.Length AndAlso s(i + 1) = "'"c Then
                        i += 1
                    Else
                        inSingleQuote = False
                    End If
                End If
            ElseIf inDoubleQuote Then
                If ch = "\"c AndAlso i + 1 < s.Length Then
                    i += 1
                ElseIf ch = """"c Then
                    inDoubleQuote = False
                End If
            Else
                If ch = "'"c Then
                    inSingleQuote = True
                ElseIf ch = """"c Then
                    inDoubleQuote = True
                ElseIf ch = "#"c Then
                    ' Found a comment - strip it and any trailing whitespace
                    Dim before As String = s.Substring(0, i).TrimEnd()
                    Return before
                End If
            End If
        Next

        Return s.TrimEnd()
    End Function

    ''' <summary>
    ''' Remove surrounding quotes from a string key (single or double quotes).
    ''' </summary>
    Private Function UnquoteString(s As String) As String
        If s Is Nothing OrElse s.Length < 2 Then Return s

        If s(0) = "'"c AndAlso s(s.Length - 1) = "'"c Then
            Dim content As String = s.Substring(1, s.Length - 2)
            Return content.Replace("''", "'")
        End If

        If s(0) = """"c AndAlso s(s.Length - 1) = """"c Then
            Dim content As String = s.Substring(1, s.Length - 2)
            Return UnescapeDoubleQuotedString(content)
        End If

        Return s
    End Function

#End Region

#Region "Anchor/alias support"

    ''' <summary>
    ''' Dictionary of named anchors for alias resolution.
    ''' Key: anchor name, Value: the JsonElement associated with the anchor.
    ''' </summary>
    Private ReadOnly _anchors As New Dictionary(Of String, JsonElement)

    Public Sub New()

    End Sub

    ''' <summary>
    ''' Clear all stored anchors. Call this before parsing a new document.
    ''' </summary>
    Public Sub Reset()
        _anchors.Clear()
    End Sub

#End Region

End Class
