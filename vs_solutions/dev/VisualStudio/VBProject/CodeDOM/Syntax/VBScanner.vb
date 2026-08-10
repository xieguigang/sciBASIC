#Region "Microsoft.VisualBasic::45860d33eeacf38cf8731462baade043, vs_solutions\dev\VisualStudio\VBProject\CodeDOM\Syntax\VBScanner.vb"

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

    '   Total Lines: 346
    '    Code Lines: 272 (78.61%)
    ' Comment Lines: 25 (7.23%)
    '    - Xml Docs: 40.00%
    ' 
    '   Blank Lines: 49 (14.16%)
    '     File Size: 15.05 KB


    '     Class VBScanner
    ' 
    '         Function: IsContinuation, Scan, StripContinuation, Tokenize, TryParseAttributes
    ' 
    '         Sub: BuildLogicalLine, FlushWord
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace VBProj.CodeDOM.Syntax

    ''' <summary>
    ''' lexical scanner for VB.NET source code.
    '''
    ''' It handles line continuations (the trailing underscore, outside of
    ''' strings/comments), strips single quote comments and collects triple
    ''' quote xml documentation lines, and finally tokenizes every logical
    ''' line into a flat list of <see cref="Token"/>.
    ''' </summary>
    Public Class VBScanner

        Private Shared ReadOnly Keywords As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "class", "module", "structure", "struct", "enum", "interface", "namespace", "end",
            "function", "sub", "property", "operator", "delegate", "dim", "static", "const",
            "public", "private", "friend", "protected", "shared", "overloads", "overrides",
            "overridable", "mustoverride", "notoverridable", "readonly", "writeonly", "default",
            "partial", "custom", "inherits", "implements", "of", "as", "byval", "byref",
            "optional", "paramarray", "new", "get", "set", "return", "if", "then", "else",
            "elseif", "endif", "for", "each", "while", "do", "loop", "until", "to", "step",
            "next", "select", "case", "try", "catch", "finally", "throw", "using", "with",
            "synclock", "continue", "exit", "stop", "me", "mybase", "myclass", "is", "isnot",
            "nothing", "true", "false", "and", "or", "xor", "not", "mod", "like", "typeof",
            "cbool", "cbyte", "cchar", "cdate", "cdec", "cdbl", "cint", "clng", "cobj",
            "csbyte", "cshort", "csng", "cstr", "cuint", "culng", "cushort", "directcast",
            "ctype", "handles", "addressof", "withevents", "in", "imports", "option", "explicit",
            "strict", "infer", "compare", "binary", "text", "off", "on", "rem", "goto", "let",
            "mid", "redim", "erase", "raiseevent", "addhandler", "removehandler", "alias",
            "declare", "lib", "ansi", "auto", "unicode", "narrow", "wide"
        }

        ''' <summary>
        ''' scan the given VB.NET source text into a list of statements.
        ''' </summary>
        Public Function Scan(source As String) As List(Of VBStatement)
            Dim physical As String() = source _
                .Replace(vbCrLf, vbLf) _
                .Replace(vbCr, vbLf) _
                .Split(vbLf)

            Dim stmts As New List(Of VBStatement)
            Dim xmlBuf As String = ""
            Dim attrBuf As New List(Of String)
            ' the earliest physical line of the leading xml doc / attribute
            ' block; 0 means "no leading block yet" for the current statement.
            Dim leadingLine As Integer = 0
            Dim i As Integer = 0

            While i < physical.Length
                Dim raw As String = physical(i)
                Dim trim As String = raw.Trim()

                If trim.Length = 0 Then
                    ' a blank line breaks the association with a pending xml doc
                    xmlBuf = ""
                    leadingLine = 0
                    i += 1
                    Continue While
                End If

                If trim.StartsWith("'''") Then
                    If leadingLine = 0 Then
                        leadingLine = i + 1
                    End If
                    xmlBuf &= If(xmlBuf.Length = 0, "", vbCrLf) & trim.Substring(3).Trim()
                    i += 1
                    Continue While
                End If

                If trim(0) = "#"c OrElse trim.Equals("rem", StringComparison.OrdinalIgnoreCase) OrElse trim.StartsWith("rem ", StringComparison.OrdinalIgnoreCase) Then
                    ' compiler directive or rem comment, skip entirely
                    i += 1
                    Continue While
                End If

                Dim logical As String = Nothing
                Dim startLine As Integer = i + 1
                Dim endLine As Integer = startLine
                BuildLogicalLine(physical, i, logical, endLine)

                Dim tokens As List(Of Token) = Tokenize(logical, startLine)

                ' a statement that consists solely of attribute blocks
                ' (e.g. a <ExportAPI> line on its own) is buffered and attached
                ' to the next real declaration
                Dim ownLineAttrs As List(Of String) = TryParseAttributes(tokens)
                If ownLineAttrs IsNot Nothing Then
                    If leadingLine = 0 Then
                        leadingLine = startLine
                    End If
                    attrBuf.AddRange(ownLineAttrs)
                    Continue While
                End If

                If tokens.Count > 0 OrElse xmlBuf.Length > 0 OrElse attrBuf.Count > 0 Then
                    stmts.Add(New VBStatement With {
                        .Line = startLine,
                        .EndLine = endLine,
                        .LeadingLine = If(leadingLine = 0, startLine, leadingLine),
                        .tokens = tokens,
                        .XmlDoc = xmlBuf,
                        .Attributes = New List(Of String)(attrBuf)
                    })
                End If

                xmlBuf = ""
                attrBuf.Clear()
                leadingLine = 0
            End While

            Return stmts
        End Function

        ' if the whole token list is composed only of attribute blocks
        ' (<...>), return their inner texts; otherwise return nothing.
        Private Shared Function TryParseAttributes(tokens As List(Of Token)) As List(Of String)
            Dim result As New List(Of String)
            Dim pos As Integer = 0

            While pos < tokens.Count
                If tokens(pos).Text <> "<"c Then
                    Return Nothing
                End If

                pos += 1
                Dim depth As Integer = 0
                Dim sb As New StringBuilder()

                While pos < tokens.Count
                    Dim tk As Token = tokens(pos)
                    If tk.Text = "("c Then
                        depth += 1
                        sb.Append(tk.Text)
                        pos += 1
                    ElseIf tk.Text = ")"c Then
                        depth -= 1
                        sb.Append(tk.Text)
                        pos += 1
                    ElseIf tk.Text = ">"c AndAlso depth = 0 Then
                        pos += 1
                        Exit While
                    Else
                        sb.Append(tk.Text)
                        pos += 1
                    End If
                End While

                result.Add(sb.ToString().Trim())
            End While

            If result.Count = 0 Then
                Return Nothing
            End If
            Return result
        End Function

        ' merge a run of physical lines that are joined by the line continuation
        ' character (a trailing underscore, outside strings/comments)
        ' i is advanced past the logical line; endLine is the 1-based physical
        ' line of the last physical line consumed (equal to startLine for a
        ' single-line statement).
        Private Sub BuildLogicalLine(physical As String(), ByRef i As Integer, ByRef logical As String, ByRef endLine As Integer)
            Dim sb As New StringBuilder()
            Dim startLine As Integer = i + 1

            While i < physical.Length
                Dim line As String = physical(i)
                sb.Append(StripContinuation(line))

                If IsContinuation(line) Then
                    sb.Append(" "c)
                    i += 1
                Else
                    i += 1
                    Exit While
                End If
            End While

            logical = sb.ToString().Trim()
            endLine = i
        End Sub

        Private Shared Function IsContinuation(line As String) As Boolean
            Dim inStr As Boolean = False
            Dim inCom As Boolean = False

            For j As Integer = 0 To line.Length - 1
                Dim c As Char = line(j)

                If inCom Then
                    Return False
                End If

                If inStr Then
                    If c = """"c Then
                        If j + 1 < line.Length AndAlso line(j + 1) = """"c Then
                            j += 1
                        Else
                            inStr = False
                        End If
                    End If
                ElseIf c = """"c Then
                    inStr = True
                ElseIf c = "'"c Then
                    inCom = True
                ElseIf c = "_"c Then
                    Dim rest As String = line.Substring(j + 1).Trim()
                    Return rest.Length = 0
                End If
            Next

            Return False
        End Function

        Private Shared Function StripContinuation(line As String) As String
            Dim k As Integer = line.Length - 1

            While k >= 0 AndAlso Char.IsWhiteSpace(line(k))
                k -= 1
            End While

            If k >= 0 AndAlso line(k) = "_"c Then
                Return line.Substring(0, k).TrimEnd()
            End If

            Return line.TrimEnd()
        End Function

        Private Shared Sub FlushWord(sb As StringBuilder, toks As List(Of Token), lineNum As Integer)
            If sb.Length > 0 Then
                Dim w As String = sb.ToString()
                sb.Clear()
                Dim kind As TokenKind = If(Keywords.Contains(w), TokenKind.Keyword, TokenKind.Identifier)
                toks.Add(New Token With {.kind = kind, .Text = w, .Line = lineNum})
            End If
        End Sub

        Private Shared Function Tokenize(line As String, lineNum As Integer) As List(Of Token)
            Dim toks As New List(Of Token)
            Dim n As Integer = line.Length
            Dim j As Integer = 0
            Dim sb As New StringBuilder()
            Dim inStr As Boolean = False
            Dim inCom As Boolean = False

            While j < n
                Dim c As Char = line(j)

                If inCom Then
                    Exit While
                End If

                If inStr Then
                    sb.Append(c)

                    If c = """"c Then
                        If j + 1 < n AndAlso line(j + 1) = """"c Then
                            j += 1
                            sb.Append(""""c)
                        Else
                            inStr = False

                            Dim k As Integer = j + 1
                            While k < n AndAlso Char.IsWhiteSpace(line(k))
                                k += 1
                            End While

                            If k < n AndAlso (line(k) = "c"c OrElse line(k) = "C"c) Then
                                toks.Add(New Token With {.Kind = TokenKind.CharLiteral, .Text = sb.ToString(), .Line = lineNum})
                                sb.Clear()
                                j = k
                            Else
                                toks.Add(New Token With {.Kind = TokenKind.[String], .Text = sb.ToString(), .Line = lineNum})
                                sb.Clear()
                            End If
                        End If
                    End If
                Else
                    Select Case c
                        Case """"c
                            FlushWord(sb, toks, lineNum)
                            inStr = True
                            sb.Append(c)

                        Case "'"c
                            FlushWord(sb, toks, lineNum)
                            inCom = True

                        Case "#"c
                            FlushWord(sb, toks, lineNum)
                            Dim k As Integer = j + 1
                            While k < n AndAlso line(k) <> "#"c
                                k += 1
                            End While
                            Dim dt As String = line.Substring(j, If(k < n, k - j + 1, n - j))
                            toks.Add(New Token With {.Kind = TokenKind.Number, .Text = dt})
                            j = If(k < n, k, n - 1)

                        Case Else
                            If Char.IsWhiteSpace(c) Then
                                FlushWord(sb, toks, lineNum)
                            ElseIf Char.IsLetter(c) OrElse c = "_"c OrElse c = "@"c Then
                                If sb.Length = 0 Then
                                    FlushWord(sb, toks, lineNum)
                                End If
                                sb.Append(c)
                            ElseIf Char.IsDigit(c) OrElse (c = "&"c AndAlso j + 1 < n AndAlso (line(j + 1) = "H"c OrElse line(j + 1) = "h"c OrElse line(j + 1) = "O"c OrElse line(j + 1) = "o"c)) Then
                                FlushWord(sb, toks, lineNum)
                                Dim k As Integer = j
                                If c = "&"c Then
                                    k += 1
                                End If
                                While k < n AndAlso (Char.IsLetterOrDigit(line(k)) OrElse line(k) = "_"c OrElse line(k) = "."c OrElse line(k) = "+"c OrElse line(k) = "-"c)
                                    k += 1
                                End While
                                While k < n AndAlso Char.IsLetter(line(k))
                                    k += 1
                                End While
                                toks.Add(New Token With {.Kind = TokenKind.Number, .Text = line.Substring(j, k - j), .Line = lineNum})
                                j = k - 1
                            Else
                                FlushWord(sb, toks, lineNum)
                                Dim two As String = If(j + 1 < n, c & line(j + 1), c.ToString())

                                Select Case two
                                    Case "<>", "<=", ">=", "<<", ">>", "+=", "-=", "*=", "/=", "\=", "^=", "&=", ":=", "->"
                                        toks.Add(New Token With {.Kind = TokenKind.Punctuation, .Text = two, .Line = lineNum})
                                        j += 1
                                    Case Else
                                        toks.Add(New Token With {.Kind = TokenKind.Punctuation, .Text = c.ToString(), .Line = lineNum})
                                End Select
                            End If
                    End Select
                End If

                j += 1
            End While

            FlushWord(sb, toks, lineNum)
            Return toks
        End Function
    End Class

End Namespace
