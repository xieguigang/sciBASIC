Imports System.Text

Namespace Syntax

    ''' <summary>
    ''' a logical (already line-continued) source line together with the
    ''' xml documentation comment that immediately precedes it.
    ''' </summary>
    Public Class VBStatement
        Public Line As Integer
        Public Tokens As List(Of Token)
        Public XmlDoc As String
        Public Attributes As New List(Of String)
    End Class

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
            Dim i As Integer = 0

            While i < physical.Length
                Dim raw As String = physical(i)
                Dim trim As String = raw.Trim()

                If trim.Length = 0 Then
                    ' a blank line breaks the association with a pending xml doc
                    xmlBuf = ""
                    i += 1
                    Continue While
                End If

                If trim.StartsWith("'''") Then
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
                BuildLogicalLine(physical, i, logical)

                Dim tokens As List(Of Token) = Tokenize(logical)

                ' a statement that consists solely of attribute blocks
                ' (e.g. a <ExportAPI> line on its own) is buffered and attached
                ' to the next real declaration
                Dim ownLineAttrs As List(Of String) = TryParseAttributes(tokens)
                If ownLineAttrs IsNot Nothing Then
                    attrBuf.AddRange(ownLineAttrs)
                    Continue While
                End If

                If tokens.Count > 0 OrElse xmlBuf.Length > 0 OrElse attrBuf.Count > 0 Then
                    stmts.Add(New VBStatement With {
                        .Line = startLine,
                        .Tokens = tokens,
                        .XmlDoc = xmlBuf,
                        .Attributes = New List(Of String)(attrBuf)
                    })
                End If

                xmlBuf = ""
                attrBuf.Clear()
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
        Private Sub BuildLogicalLine(physical As String(), ByRef i As Integer, ByRef logical As String)
            Dim sb As New StringBuilder()

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

        Private Shared Sub FlushWord(sb As StringBuilder, toks As List(Of Token))
            If sb.Length > 0 Then
                Dim w As String = sb.ToString()
                sb.Clear()
                Dim kind As TokenKind = If(Keywords.Contains(w), TokenKind.Keyword, TokenKind.Identifier)
                toks.Add(New Token With {.Kind = kind, .Text = w})
            End If
        End Sub

        Private Shared Function Tokenize(line As String) As List(Of Token)
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
                                toks.Add(New Token With {.Kind = TokenKind.CharLiteral, .Text = sb.ToString()})
                                sb.Clear()
                                j = k
                            Else
                                toks.Add(New Token With {.Kind = TokenKind.[String], .Text = sb.ToString()})
                                sb.Clear()
                            End If
                        End If
                    End If
                Else
                    Select Case c
                        Case """"c
                            FlushWord(sb, toks)
                            inStr = True
                            sb.Append(c)

                        Case "'"c
                            FlushWord(sb, toks)
                            inCom = True

                        Case "#"c
                            FlushWord(sb, toks)
                            Dim k As Integer = j + 1
                            While k < n AndAlso line(k) <> "#"c
                                k += 1
                            End While
                            Dim dt As String = line.Substring(j, If(k < n, k - j + 1, n - j))
                            toks.Add(New Token With {.Kind = TokenKind.Number, .Text = dt})
                            j = If(k < n, k, n - 1)

                        Case Else
                            If Char.IsWhiteSpace(c) Then
                                FlushWord(sb, toks)
                            ElseIf Char.IsLetter(c) OrElse c = "_"c OrElse c = "@"c Then
                                If sb.Length = 0 Then
                                    FlushWord(sb, toks)
                                End If
                                sb.Append(c)
                            ElseIf Char.IsDigit(c) OrElse (c = "&"c AndAlso j + 1 < n AndAlso (line(j + 1) = "H"c OrElse line(j + 1) = "h"c OrElse line(j + 1) = "O"c OrElse line(j + 1) = "o"c)) Then
                                FlushWord(sb, toks)
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
                                toks.Add(New Token With {.Kind = TokenKind.Number, .Text = line.Substring(j, k - j)})
                                j = k - 1
                            Else
                                FlushWord(sb, toks)
                                Dim two As String = If(j + 1 < n, c & line(j + 1), c.ToString())

                                Select Case two
                                    Case "<>", "<=", ">=", "<<", ">>", "+=", "-=", "*=", "/=", "\=", "^=", "&=", ":=", "->"
                                        toks.Add(New Token With {.Kind = TokenKind.Punctuation, .Text = two})
                                        j += 1
                                    Case Else
                                        toks.Add(New Token With {.Kind = TokenKind.Punctuation, .Text = c.ToString()})
                                End Select
                            End If
                    End Select
                End If

                j += 1
            End While

            FlushWord(sb, toks)
            Return toks
        End Function
    End Class

End Namespace
