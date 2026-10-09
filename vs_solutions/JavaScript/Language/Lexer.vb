Imports System.Globalization
Imports System.Text

''' <summary>
''' JavaScript lexer (ES5-core subset): numbers, strings with escapes,
''' identifiers/keywords, comments and every operator/punctuator the parser
''' supports. Tracks line breaks for automatic-semicolon-insertion.
''' </summary>
Public NotInheritable Class Lexer

    Public Shared ReadOnly Keywords As New HashSet(Of String) From {
        "var", "let", "const", "function", "if", "else", "for", "while", "do",
        "break", "continue", "return", "throw", "try", "catch", "finally",
        "true", "false", "null", "undefined", "typeof", "new", "in", "this"
    }

    Private ReadOnly _src As String
    Private _pos As Integer
    Private _line As Integer = 1
    Private _col As Integer = 1

    Private Sub New(source As String)
        _src = source
    End Sub

    ''' <summary>Tokenise <paramref name="source"/>; throws <see cref="ParseException"/> on bad input.</summary>
    Public Shared Function Lex(source As String) As List(Of Token)
        Dim lx As New Lexer(source)
        Dim tokens As New List(Of Token)
        While True
            Dim t = lx.NextToken(False)      ' line-break detection restarts per token
            tokens.Add(t)
            If t.name = TokenType.Eof Then Exit While
        End While
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

        ' multi-char operators first (3 → 2 → 1)
        If _pos + 2 < _src.Length Then
            Dim three = _src.Substring(_pos, 3)
            If three = "===" OrElse three = "!==" Then
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
        If two = "==" OrElse two = "!=" OrElse two = "<=" OrElse two = ">=" OrElse
           two = "&&" OrElse two = "||" OrElse two = "++" OrElse two = "--" OrElse
           two = "+=" OrElse two = "-=" OrElse two = "*=" OrElse two = "/=" OrElse
           two = "%=" OrElse two = "=>" Then
            Dim op = two
            Advance() : Advance()
            Return Make(TokenType.Punct, op, Nothing, sl, sc, brk)
        End If

        ' single-char operators / punctuation
        If "+-*/%=<>!?:.,;(){}[]".IndexOf(cur) >= 0 Then
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
            While _pos < _src.Length AndAlso Uri.IsHexDigit(_src(_pos))
                Advance()
            End While
            Dim hx = _src.Substring(start + 2, _pos - start - 2)
            Dim v As Long
            If Not Long.TryParse(hx, NumberStyles.HexNumber, CultureInfo.InvariantCulture, v) Then
                Throw New ParseException($"invalid hex literal at {sl}:{sc}")
            End If
            Return Make(TokenType.Number, _src.Substring(start, _pos - start), CDbl(v), sl, sc, brk)
        End If

        While Char.IsDigit(Cur)
            Advance()
        End While
        If Cur = "."c AndAlso Char.IsDigit(Peek) Then
            Advance()
            While Char.IsDigit(Cur)
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
                While Char.IsDigit(Cur)
                    Advance()
                End While
            Else
                ' not an exponent (e.g. identifier 'e'?) — rewind
                _pos = save
            End If
        End If

        Dim text = _src.Substring(start, _pos - start)
        Dim value As Double
        If Not Double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, value) Then
            Throw New ParseException($"invalid number literal '{text}' at {sl}:{sc}")
        End If
        Return Make(TokenType.Number, text, value, sl, sc, brk)
    End Function

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

End Class

