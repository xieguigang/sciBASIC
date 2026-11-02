Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Schema\ProtoLexer.vb
' /********************************************************************************/

Imports System.Text

Namespace Schema

    ''' <summary>
    ''' .proto 源文本的词法分析器。
    ''' </summary>
    ''' <remarks>
    ''' 支持 <c>//</c> 行注释、<c>/* */</c> 块注释、单引号/双引号字符串（含常用转义）、
    ''' 十进制 / 十六进制 / 八进制 / 浮点数值，以及带点号的限定标识符。
    ''' </remarks>
    Friend Class ProtoLexer

        Private ReadOnly _text As String
        Private ReadOnly _fileName As String
        Private _pos As Integer
        Private _line As Integer = 1
        Private _column As Integer = 1

        Public Sub New(text As String, Optional fileName As String = Nothing)
            _text = If(text, "")
            _fileName = fileName
        End Sub

        ''' <summary>
        ''' 扫描全部词法单元，末尾总是带有一个 <see cref="ProtoTokenType.EndOfFile"/>。
        ''' </summary>
        Public Function Tokenize() As List(Of ProtoToken)
            Dim tokens As New List(Of ProtoToken)

            Do
                Call SkipTrivia()

                If _pos >= _text.Length Then
                    Call tokens.Add(New ProtoToken(ProtoTokenType.EndOfFile, "", _line, _column))
                    Exit Do
                End If

                Dim ch As Char = _text(_pos)
                Dim startLine As Integer = _line
                Dim startColumn As Integer = _column

                If ch = """"c OrElse ch = "'"c Then
                    Call tokens.Add(ReadStringLiteral(startLine, startColumn))
                ElseIf Char.IsDigit(ch) OrElse (ch = "."c AndAlso IsDigitAt(_pos + 1)) Then
                    Call tokens.Add(ReadNumber(startLine, startColumn))
                ElseIf IsIdentifierStart(ch) Then
                    Call tokens.Add(ReadIdentifier(startLine, startColumn))
                ElseIf IsSymbol(ch) Then
                    Call Advance()
                    Call tokens.Add(New ProtoToken(ProtoTokenType.Symbol, CStr(ch), startLine, startColumn))
                Else
                    Throw New ProtoParseException($"无法识别的字符 '{ch}'", _fileName, startLine, startColumn)
                End If
            Loop

            Return tokens
        End Function

#Region "METHODS (PRIVATE)"

        Private Sub Advance()
            Dim ch As Char = _text(_pos)

            If ch = ControlChars.Cr Then
                _pos += 1

                If _pos < _text.Length AndAlso _text(_pos) = ControlChars.Lf Then
                    _pos += 1
                End If

                _line += 1
                _column = 1
            ElseIf ch = ControlChars.Lf Then
                _pos += 1
                _line += 1
                _column = 1
            Else
                _pos += 1
                _column += 1
            End If
        End Sub

        Private Function PeekChar(offset As Integer) As Char
            Dim index As Integer = _pos + offset

            If index < 0 OrElse index >= _text.Length Then
                Return ControlChars.NullChar
            Else
                Return _text(index)
            End If
        End Function

        Private Function IsDigitAt(index As Integer) As Boolean
            Return index >= 0 AndAlso index < _text.Length AndAlso Char.IsDigit(_text(index))
        End Function

        Private Shared Function IsIdentifierStart(ch As Char) As Boolean
            Return Char.IsLetter(ch) OrElse ch = "_"c
        End Function

        Private Shared Function IsIdentifierPart(ch As Char) As Boolean
            Return Char.IsLetterOrDigit(ch) OrElse ch = "_"c
        End Function

        Private Shared Function IsSymbol(ch As Char) As Boolean
            Return "{}[]()<>=;,:+-/.".IndexOf(ch) >= 0
        End Function

        Private Shared Function IsHexDigit(ch As Char) As Boolean
            Return Char.IsDigit(ch) OrElse (ch >= "a"c AndAlso ch <= "f"c) OrElse (ch >= "A"c AndAlso ch <= "F"c)
        End Function

        Private Sub SkipTrivia()
            Do
                If _pos >= _text.Length Then
                    Exit Do
                End If

                Dim ch As Char = _text(_pos)

                If ch = " "c OrElse ch = ControlChars.Tab OrElse ch = ControlChars.Cr OrElse ch = ControlChars.Lf Then
                    Call Advance()
                ElseIf ch = "/"c AndAlso PeekChar(1) = "/"c Then
                    Do While _pos < _text.Length AndAlso _text(_pos) <> ControlChars.Cr AndAlso _text(_pos) <> ControlChars.Lf
                        Call Advance()
                    Loop
                ElseIf ch = "/"c AndAlso PeekChar(1) = "*"c Then
                    Dim startLine As Integer = _line
                    Dim startColumn As Integer = _column
                    Dim closed As Boolean = False

                    Call Advance()
                    Call Advance()

                    Do While _pos < _text.Length
                        If _text(_pos) = "*"c AndAlso PeekChar(1) = "/"c Then
                            Call Advance()
                            Call Advance()
                            closed = True
                            Exit Do
                        End If

                        Call Advance()
                    Loop

                    If Not closed Then
                        Throw New ProtoParseException("块注释 /* 没有闭合", _fileName, startLine, startColumn)
                    End If
                Else
                    Exit Do
                End If
            Loop
        End Sub

        Private Function ReadStringLiteral(startLine As Integer, startColumn As Integer) As ProtoToken
            Dim quote As Char = _text(_pos)
            Dim builder As New StringBuilder()

            Call Advance()

            Do
                If _pos >= _text.Length Then
                    Throw New ProtoParseException("字符串常量没有闭合", _fileName, startLine, startColumn)
                End If

                Dim ch As Char = _text(_pos)

                If ch = quote Then
                    Call Advance()
                    Exit Do
                End If

                If ch = "\"c Then
                    Call Advance()

                    If _pos >= _text.Length Then
                        Throw New ProtoParseException("字符串常量在转义符后被截断", _fileName, startLine, startColumn)
                    End If

                    Dim escape As Char = _text(_pos)

                    Select Case escape
                        Case "n"c
                            Call builder.Append(ControlChars.Lf)
                        Case "r"c
                            Call builder.Append(ControlChars.Cr)
                        Case "t"c
                            Call builder.Append(ControlChars.Tab)
                        Case "0"c
                            Call builder.Append(ChrW(0))
                        Case "a"c
                            Call builder.Append(ChrW(7))
                        Case "b"c
                            Call builder.Append(ChrW(8))
                        Case "f"c
                            Call builder.Append(ChrW(12))
                        Case "v"c
                            Call builder.Append(ChrW(11))
                        Case "x"c
                            Call Advance()

                            Dim hex As New StringBuilder()

                            While _pos < _text.Length AndAlso hex.Length < 2 AndAlso IsHexDigit(_text(_pos))
                                Call hex.Append(_text(_pos))
                                Call Advance()
                            End While

                            If hex.Length > 0 Then
                                Call builder.Append(ChrW(Convert.ToInt32(hex.ToString(), 16)))
                            End If

                            Continue Do
                        Case Else
                            Call builder.Append(escape)
                    End Select

                    Call Advance()
                Else
                    Call builder.Append(ch)
                    Call Advance()
                End If
            Loop

            Return New ProtoToken(ProtoTokenType.StringLiteral, builder.ToString(), startLine, startColumn)
        End Function

        Private Function ReadNumber(startLine As Integer, startColumn As Integer) As ProtoToken
            Dim builder As New StringBuilder()

            If _text(_pos) = "0"c AndAlso (PeekChar(1) = "x"c OrElse PeekChar(1) = "X"c) Then
                Call builder.Append(_text(_pos))
                Call Advance()
                Call builder.Append(_text(_pos))
                Call Advance()

                Do While _pos < _text.Length AndAlso IsHexDigit(_text(_pos))
                    Call builder.Append(_text(_pos))
                    Call Advance()
                Loop

                Return New ProtoToken(ProtoTokenType.Number, builder.ToString(), startLine, startColumn)
            End If

            Do While _pos < _text.Length AndAlso Char.IsDigit(_text(_pos))
                Call builder.Append(_text(_pos))
                Call Advance()
            Loop

            If _pos < _text.Length AndAlso _text(_pos) = "."c AndAlso IsDigitAt(_pos + 1) Then
                Call builder.Append(_text(_pos))
                Call Advance()

                Do While _pos < _text.Length AndAlso Char.IsDigit(_text(_pos))
                    Call builder.Append(_text(_pos))
                    Call Advance()
                Loop
            End If

            If _pos < _text.Length AndAlso (_text(_pos) = "e"c OrElse _text(_pos) = "E"c) Then
                Call builder.Append(_text(_pos))
                Call Advance()

                If _pos < _text.Length AndAlso (_text(_pos) = "+"c OrElse _text(_pos) = "-"c) Then
                    Call builder.Append(_text(_pos))
                    Call Advance()
                End If

                Do While _pos < _text.Length AndAlso Char.IsDigit(_text(_pos))
                    Call builder.Append(_text(_pos))
                    Call Advance()
                Loop
            End If

            Return New ProtoToken(ProtoTokenType.Number, builder.ToString(), startLine, startColumn)
        End Function

        Private Function ReadIdentifier(startLine As Integer, startColumn As Integer) As ProtoToken
            Dim builder As New StringBuilder()

            Do While _pos < _text.Length AndAlso IsIdentifierPart(_text(_pos))
                Call builder.Append(_text(_pos))
                Call Advance()
            Loop

            ' 限定名：a.b.c（点号后必须紧跟标识符起始字符，避免吞掉语句结尾的点号）
            Do While _pos < _text.Length AndAlso _text(_pos) = "."c AndAlso IsIdentifierStart(PeekChar(1))
                Call builder.Append("."c)
                Call Advance()

                Do While _pos < _text.Length AndAlso IsIdentifierPart(_text(_pos))
                    Call builder.Append(_text(_pos))
                    Call Advance()
                Loop
            Loop

            Return New ProtoToken(ProtoTokenType.Identifier, builder.ToString(), startLine, startColumn)
        End Function

#End Region

    End Class

End Namespace
