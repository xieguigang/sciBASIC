#Region "Microsoft.VisualBasic::6599d138e0b9142a25d5ce7a5336f349, vs_solutions\JavaScript\Language\Lexer.vb"

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

    '   Total Lines: 577
    '    Code Lines: 457 (79.20%)
    ' Comment Lines: 57 (9.88%)
    '    - Xml Docs: 63.16%
    ' 
    '   Blank Lines: 63 (10.92%)
    '     File Size: 22.54 KB


    ' Class Lexer
    ' 
    '     Properties: Cur, Peek
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: IsIdPart, IsIdStart, Lex, Make, NextToken
    '               RegexAllowed, ScanNumber, ScanRegex, ScanString, ScanTemplate
    '               ScanTemplateExpressionSource
    ' 
    '     Sub: Advance, SkipStringLiteral, SkipTemplateExpression, SkipTemplateLiteral, ValidateNumericSeparators
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.Text

''' <summary>
''' JavaScript lexer (ES5-core subset): numbers, strings with escapes,
''' identifiers/keywords, comments and every operator/punctuator the parser
''' supports. Tracks line breaks for automatic-semicolon-insertion.
''' </summary>
''' <remarks>
''' Reserved words are keyword tokens; CONTEXTUAL keywords (of/get/set/async/
''' await/yield/static/from/as) stay identifier tokens so that legal code such
''' as <c>var of = 1</c> or <c>obj.get</c> keeps working — the parser matches
''' them by text.
''' </remarks>
Public NotInheritable Class Lexer

    Public Shared ReadOnly Keywords As New HashSet(Of String) From {
        "var", "let", "const", "function", "if", "else", "for", "while", "do",
        "break", "continue", "return", "throw", "try", "catch", "finally",
        "true", "false", "null", "undefined", "typeof", "new", "in", "this",
        "switch", "case", "default", "class", "extends", "super",
        "instanceof", "delete", "void"
    }

    Private ReadOnly _src As String
    Private _pos As Integer
    Private _line As Integer = 1
    Private _col As Integer = 1
    ''' <summary>Last significant token — decides '/' as division vs regex start.</summary>
    Private _lastToken As Token

    Private Sub New(source As String)
        _src = source
    End Sub

    ''' <summary>Tokenise <paramref name="source"/>; throws <see cref="ParseException"/> on bad input.</summary>
    Public Shared Function Lex(source As String) As List(Of Token)
        Dim lx As New Lexer(source)
        Dim tokens As New List(Of Token)

        Try
            While True
                Dim t = lx.NextToken(False)      ' line-break detection restarts per token
                tokens.Add(t)
                lx._lastToken = t
                If t.name = TokenType.Eof Then Exit While
            End While
        Catch ex As ParseException
            ' every lexer failure happens at the current scanner position: attach it so the
            ' host can render a position frame (the message text itself stays unchanged)
            Throw ex.WithPosition(lx._line, lx._col)
        End Try

        Return tokens
    End Function

    ' ---- helpers ----

    Private ReadOnly Property Cur As Char
        Get
            Return If(_pos < _src.Length, _src(_pos), ChrW(0))
        End Get
    End Property

    Private ReadOnly Property Peek As Char
        Get
            Return If(_pos + 1 < _src.Length, _src(_pos + 1), ChrW(0))
        End Get
    End Property

    Private Sub Advance()
        If _pos >= _src.Length Then Return
        If _src(_pos) = vbLf Then
            _line += 1
            _col = 1
        Else
            _col += 1
        End If
        _pos += 1
    End Sub

    Private Function IsIdStart(c As Char) As Boolean
        Return Char.IsLetter(c) OrElse c = "_"c OrElse c = "$"c
    End Function

    Private Function IsIdPart(c As Char) As Boolean
        Return Char.IsLetterOrDigit(c) OrElse c = "_"c OrElse c = "$"c
    End Function

    Private Function Make(type As TokenType, text As String, value As Object, startLine As Integer, startCol As Integer, brk As Boolean) As Token
        Return New Token(type, text, value, startLine, startCol, brk)
    End Function

    ' ---- main scan ----

    Private Function NextToken(lineBreakBefore As Boolean) As Token
        ' whitespace / comments (set brk on newline)
        Dim brk = lineBreakBefore
        While _pos < _src.Length
            Dim c = _src(_pos)
            If c = vbLf OrElse c = vbCr Then
                brk = True
                Advance()
            ElseIf c = " "c OrElse c = vbTab Then
                Advance()
            ElseIf c = "/"c AndAlso Peek = "/"c Then
                While _pos < _src.Length AndAlso _src(_pos) <> vbLf
                    Advance()
                End While
            ElseIf c = "/"c AndAlso Peek = "*"c Then
                Advance() : Advance()
                While _pos < _src.Length
                    If _src(_pos) = "*"c AndAlso Peek = "/"c Then
                        Advance() : Advance()
                        Exit While
                    End If
                    If _src(_pos) = vbLf Then brk = True
                    Advance()
                End While
            Else
                Exit While
            End If
        End While

        Dim sl = _line, sc = _col

        If _pos >= _src.Length Then Return Make(TokenType.Eof, "<eof>", Nothing, sl, sc, brk)

        Dim cur = _src(_pos)

        ' numbers: 12, 12.5, .5, 1e3, 0x1F
        If Char.IsDigit(cur) OrElse (cur = "."c AndAlso Char.IsDigit(Peek)) Then
            Return ScanNumber(sl, sc, brk)
        End If

        ' identifiers / keywords
        If IsIdStart(cur) Then
            Dim start = _pos
            While _pos < _src.Length AndAlso IsIdPart(_src(_pos))
                Advance()
            End While
            Dim word = _src.Substring(start, _pos - start)
            Dim tt = If(Keywords.Contains(word), TokenType.Keyword, TokenType.Identifier)
            Return Make(tt, word, Nothing, sl, sc, brk)
        End If

        ' strings
        If cur = """"c OrElse cur = "'"c Then
            Return ScanString(sl, sc, brk)
        End If

        ' template literals: `cooked ${expr} text`
        If cur = "`"c Then
            Return ScanTemplate(sl, sc, brk)
        End If

        ' regular-expression literal (context dependent: '/' after a value is division)
        If cur = "/"c AndAlso RegexAllowed() Then
            Return ScanRegex(sl, sc, brk)
        End If

        ' multi-char operators first (3 → 2 → 1)
        If _pos + 2 < _src.Length Then
            Dim three = _src.Substring(_pos, 3)
            If three = "===" OrElse three = "!==" OrElse three = "..." OrElse three = ">>>" Then
                Advance() : Advance() : Advance()
                Return Make(TokenType.Punct, three, Nothing, sl, sc, brk)
            End If
        End If
        Dim two As String = Nothing
        If _pos + 1 <= _src.Length - 1 Then two = _src.Substring(_pos, 2)
        ' '**' first: it must win over '*'/'*=' (a ** b is exponentiation)
        If two = "**" Then
            Advance() : Advance()
            Return Make(TokenType.Punct, "**", Nothing, sl, sc, brk)
        End If
        If two = "??" Then
            Advance() : Advance()
            Return Make(TokenType.Punct, "??", Nothing, sl, sc, brk)
        End If
        ' '?.' is optional chaining, but '? .5' (conditional with a decimal) is not:
        ' a digit right after '?.' means the '.' starts a number literal
        If two = "?." AndAlso (_pos + 2 >= _src.Length OrElse Not Char.IsDigit(_src(_pos + 2))) Then
            Advance() : Advance()
            Return Make(TokenType.Punct, "?.", Nothing, sl, sc, brk)
        End If
        If two = "==" OrElse two = "!=" OrElse two = "<=" OrElse two = ">=" OrElse
           two = "&&" OrElse two = "||" OrElse two = "++" OrElse two = "--" OrElse
           two = "+=" OrElse two = "-=" OrElse two = "*=" OrElse two = "/=" OrElse
           two = "%=" OrElse two = "=>" OrElse two = "<<" OrElse two = ">>" Then
            Dim op = two
            Advance() : Advance()
            Return Make(TokenType.Punct, op, Nothing, sl, sc, brk)
        End If

        ' single-char operators / punctuation
        If "+-*/%=<>!?:.,;(){}[]&|^~".IndexOf(cur) >= 0 Then
            Advance()
            Return Make(TokenType.Punct, cur.ToString(), Nothing, sl, sc, brk)
        End If

        Throw New ParseException($"unexpected character '{cur}' at {sl}:{sc}")
    End Function

    Private Function ScanNumber(sl As Integer, sc As Integer, brk As Boolean) As Token
        Dim start = _pos
        ' hex
        If Cur = "0"c AndAlso (Peek = "x"c OrElse Peek = "X"c) Then
            Advance() : Advance()
            While _pos < _src.Length AndAlso (Uri.IsHexDigit(_src(_pos)) OrElse _src(_pos) = "_"c)
                Advance()
            End While
            Dim hx = _src.Substring(start + 2, _pos - start - 2)
            ValidateNumericSeparators(hx, sl, sc)
            hx = hx.Replace("_", "")
            Dim v As Long
            If Not Long.TryParse(hx, NumberStyles.HexNumber, CultureInfo.InvariantCulture, v) Then
                Throw New ParseException($"invalid hex literal at {sl}:{sc}")
            End If
            Return Make(TokenType.Number, _src.Substring(start, _pos - start), CDbl(v), sl, sc, brk)
        End If

        While Char.IsDigit(Cur) OrElse Cur = "_"c
            Advance()
        End While
        If Cur = "."c AndAlso Char.IsDigit(Peek) Then
            Advance()
            While Char.IsDigit(Cur) OrElse Cur = "_"c
                Advance()
            End While
        ElseIf Cur = "."c AndAlso Not Char.IsLetter(Peek) AndAlso Peek <> "_"c AndAlso Peek <> "$"c Then
            ' trailing dot: "1." or "1.toFixed()" (member access on int) — keep it simple: consume the dot
            Advance()
        End If
        If Cur = "e"c OrElse Cur = "E"c Then
            Dim save = _pos
            Advance()
            If Cur = "+"c OrElse Cur = "-"c Then Advance()
            If Char.IsDigit(Cur) Then
                While Char.IsDigit(Cur) OrElse Cur = "_"c
                    Advance()
                End While
            Else
                ' not an exponent (e.g. identifier 'e'?) — rewind
                _pos = save
            End If
        End If

        Dim text = _src.Substring(start, _pos - start)
        ValidateNumericSeparators(text, sl, sc)
        Dim value As Double
        If Not Double.TryParse(text.Replace("_", ""), NumberStyles.Float, CultureInfo.InvariantCulture, value) Then
            Throw New ParseException($"invalid number literal '{text}' at {sl}:{sc}")
        End If
        Return Make(TokenType.Number, text, value, sl, sc, brk)
    End Function

    ''' <summary>
    ''' Numeric separators (ES2021 <c>1_000_000</c>) must sit between two digits —
    ''' reject <c>1__0</c>, <c>_1</c>, <c>1_</c> and friends.
    ''' </summary>
    Private Shared Sub ValidateNumericSeparators(text As String, sl As Integer, sc As Integer)
        For i = 0 To text.Length - 1
            If text(i) = "_"c Then
                If i = 0 OrElse i = text.Length - 1 OrElse
                   Not Char.IsLetterOrDigit(text(i - 1)) OrElse Not Char.IsLetterOrDigit(text(i + 1)) Then
                    Throw New ParseException($"invalid numeric separator in '{text}' at {sl}:{sc}")
                End If
            End If
        Next
    End Sub

    Private Function ScanString(sl As Integer, sc As Integer, brk As Boolean) As Token
        Dim quote = Cur
        Dim sb As New StringBuilder()
        Advance()
        While True
            If _pos >= _src.Length Then Throw New ParseException($"unterminated string starting at {sl}:{sc}")
            Dim c = Cur
            If c = quote Then
                Advance()
                Exit While
            End If
            If c = vbLf Then Throw New ParseException($"unterminated string (newline) at {sl}:{sc}")
            If c = "\"c Then
                Advance()
                Dim esc = Cur
                Select Case esc
                    Case "n"c : sb.Append(vbLf)
                    Case "t"c : sb.Append(vbTab)
                    Case "r"c : sb.Append(vbCr)
                    Case "b"c : sb.Append(ChrW(8))
                    Case "f"c : sb.Append(ChrW(12))
                    Case "v"c : sb.Append(ChrW(11))
                    Case "0"c : sb.Append(ChrW(0))
                    Case """"c, "'"c, "\"c, "/"c : sb.Append(esc)
                    Case "u"c
                        Advance()
                        Dim hex As New StringBuilder()
                        For i = 1 To 4
                            If _pos >= _src.Length OrElse Not Uri.IsHexDigit(Cur) Then
                                Throw New ParseException($"bad \\u escape at {sl}:{sc}")
                            End If
                            hex.Append(Cur)
                            Advance()
                        Next
                        sb.Append(ChrW(Convert.ToInt32(hex.ToString(), 16)))
                        Continue While
                    Case "x"c
                        Advance()
                        Dim hex As New StringBuilder()
                        For i = 1 To 2
                            If _pos >= _src.Length OrElse Not Uri.IsHexDigit(Cur) Then
                                Throw New ParseException($"bad \\x escape at {sl}:{sc}")
                            End If
                            hex.Append(Cur)
                            Advance()
                        Next
                        sb.Append(ChrW(Convert.ToInt32(hex.ToString(), 16)))
                        Continue While
                    Case Else
                        Throw New ParseException($"unknown escape '\{esc}' at {_line}:{_col}")
                End Select
                Advance()
            Else
                sb.Append(c)
                Advance()
            End If
        End While
        Return Make(TokenType.String, sb.ToString(), sb.ToString(), sl, sc, brk)
    End Function

    ' ---- regular expressions ----

    ''' <summary>
    ''' '/' starts a regex unless the previous token can end an expression
    ''' (identifier, literal, ')', ']', '}', '++'/'--' or a value keyword) —
    ''' the standard heuristic used by acorn/esprima.
    ''' </summary>
    Private Function RegexAllowed() As Boolean
        Dim t = _lastToken
        If t Is Nothing Then Return True            ' start of input

        If t.name = TokenType.Number OrElse t.name = TokenType.String OrElse
           t.name = TokenType.Template OrElse t.name = TokenType.Regex OrElse
           t.name = TokenType.Identifier Then
            Return False
        End If

        If t.name = TokenType.Keyword Then
            ' value keywords end an expression → the '/' divides
            Select Case t.text
                Case "this", "super", "true", "false", "null", "undefined"
                    Return False
            End Select
            Return True
        End If

        If t.name = TokenType.Punct Then
            Select Case t.text
                Case ")", "]", "}", "++", "--"
                    Return False
            End Select
            Return True
        End If

        Return True
    End Function

    ''' <summary>Scan <c>/pattern/flags</c> (character classes keep '/' literal).</summary>
    Private Function ScanRegex(sl As Integer, sc As Integer, brk As Boolean) As Token
        Advance()   ' consume the opening '/'
        Dim sb As New StringBuilder()
        Dim inClass = False

        While True
            If _pos >= _src.Length Then
                Throw New ParseException($"unterminated regular expression at {sl}:{sc}")
            End If
            Dim c = Cur
            If c = vbLf OrElse c = vbCr Then
                Throw New ParseException($"unterminated regular expression at {sl}:{sc}")
            End If

            If c = "\"c Then
                sb.Append(c)
                Advance()
                If _pos >= _src.Length Then
                    Throw New ParseException($"unterminated regular expression escape at {sl}:{sc}")
                End If
                sb.Append(Cur)
                Advance()
                Continue While
            End If

            If c = "["c Then inClass = True
            If c = "]"c Then inClass = False

            If c = "/"c AndAlso Not inClass Then
                Advance()
                Exit While
            End If

            sb.Append(c)
            Advance()
        End While

        Dim flags As New StringBuilder()
        While _pos < _src.Length AndAlso Char.IsLetter(_src(_pos))
            flags.Append(_src(_pos))
            Advance()
        End While

        Dim pattern = sb.ToString(), flagText = flags.ToString()
        Return Make(TokenType.Regex, "/" & pattern & "/" & flagText,
                    New RegexLiteral(pattern, flagText), sl, sc, brk)
    End Function

    ' ---- template literals ----

    ''' <summary>
    ''' Scan a backtick template literal into a <see cref="TemplateChunk"/> list:
    ''' cooked text chunks plus the raw source of each <c>${...}</c> placeholder.
    ''' Newlines are legal inside templates; nested templates/strings inside
    ''' placeholders are skipped verbatim so brace matching stays correct.
    ''' </summary>
    Private Function ScanTemplate(sl As Integer, sc As Integer, brk As Boolean) As Token
        Advance()   ' consume the opening backtick
        Dim parts As New List(Of TemplateChunk)
        Dim sb As New StringBuilder()

        While True
            If _pos >= _src.Length Then
                Throw New ParseException($"unterminated template literal at {sl}:{sc}")
            End If
            Dim c = Cur

            If c = "`"c Then
                Advance()
                Exit While
            End If

            If c = "$"c AndAlso Peek = "{"c Then
                parts.Add(New TemplateChunk(False, sb.ToString()))
                sb.Clear()
                Advance() : Advance()   ' consume '${'
                Dim src = ScanTemplateExpressionSource()
                parts.Add(New TemplateChunk(True, src))
                Continue While
            End If

            If c = "\"c Then
                Advance()
                Dim esc = Cur
                Select Case esc
                    Case "n"c : sb.Append(vbLf) : Advance()
                    Case "t"c : sb.Append(vbTab) : Advance()
                    Case "r"c : sb.Append(vbCr) : Advance()
                    Case "b"c : sb.Append(ChrW(8)) : Advance()
                    Case "f"c : sb.Append(ChrW(12)) : Advance()
                    Case "v"c : sb.Append(ChrW(11)) : Advance()
                    Case "0"c : sb.Append(ChrW(0)) : Advance()
                    Case """"c, "'"c, "\"c, "/"c, "`"c, "$"c : sb.Append(esc) : Advance()
                    Case ChrW(10)
                        ' line continuation: the backslash-newline pair vanishes
                        Advance()
                    Case "u"c
                        Advance()
                        Dim hex As New StringBuilder()
                        For i = 1 To 4
                            If _pos >= _src.Length OrElse Not Uri.IsHexDigit(Cur) Then
                                Throw New ParseException($"bad \u escape at {sl}:{sc}")
                            End If
                            hex.Append(Cur)
                            Advance()
                        Next
                        sb.Append(ChrW(Convert.ToInt32(hex.ToString(), 16)))
                    Case "x"c
                        Advance()
                        Dim hex As New StringBuilder()
                        For i = 1 To 2
                            If _pos >= _src.Length OrElse Not Uri.IsHexDigit(Cur) Then
                                Throw New ParseException($"bad \x escape at {sl}:{sc}")
                            End If
                            hex.Append(Cur)
                            Advance()
                        Next
                        sb.Append(ChrW(Convert.ToInt32(hex.ToString(), 16)))
                    Case Else
                        Throw New ParseException($"unknown escape '\{esc}' at {_line}:{_col}")
                End Select
                Continue While
            End If

            sb.Append(c)
            Advance()
        End While

        parts.Add(New TemplateChunk(False, sb.ToString()))
        Return Make(TokenType.Template, "<template>", parts, sl, sc, brk)
    End Function

    ''' <summary>
    ''' Raw source of a <c>${...}</c> placeholder (the '${' is already consumed).
    ''' Skips nested strings/templates and tracks brace depth so that
    ''' <c>${ {a: 1} }</c> and <c>${ `x${y}z` }</c> scan correctly.
    ''' </summary>
    Private Function ScanTemplateExpressionSource() As String
        Dim start = _pos
        SkipTemplateExpression()
        ' SkipTemplateExpression leaves _pos just past the matching '}'
        Return _src.Substring(start, _pos - start - 1)
    End Function

    Private Sub SkipTemplateExpression()
        Dim depth = 1

        While _pos < _src.Length
            Dim c = Cur

            If c = "{"c Then
                depth += 1
                Advance()
            ElseIf c = "}"c Then
                depth -= 1
                Advance()
                If depth = 0 Then Exit Sub
            ElseIf c = """"c OrElse c = "'"c Then
                SkipStringLiteral()
            ElseIf c = "`"c Then
                SkipTemplateLiteral()
            Else
                Advance()
            End If
        End While

        Throw New ParseException("unterminated template expression")
    End Sub

    ''' <summary>Skip a quoted string verbatim (opening quote is the current char).</summary>
    Private Sub SkipStringLiteral()
        Dim quote = Cur
        Advance()

        While _pos < _src.Length
            Dim c = Cur
            If c = "\"c Then
                Advance() : Advance()
                Continue While
            End If
            If c = quote Then
                Advance()
                Exit While
            End If
            If c = vbLf Then Throw New ParseException("unterminated string inside template expression")
            Advance()
        End While
    End Sub

    ''' <summary>Skip a nested template literal verbatim (opening backtick is current).</summary>
    Private Sub SkipTemplateLiteral()
        Advance()   ' consume '`'

        While _pos < _src.Length
            Dim c = Cur
            If c = "`"c Then
                Advance()
                Exit Sub
            End If
            If c = "\"c Then
                Advance() : Advance()
                Continue While
            End If
            If c = "$"c AndAlso Peek = "{"c Then
                Advance() : Advance()
                SkipTemplateExpression()
                Continue While
            End If
            Advance()
        End While

        Throw New ParseException("unterminated nested template literal")
    End Sub

End Class
