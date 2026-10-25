Imports System.Globalization
Imports System.IO
Imports System.Text

Namespace Numpy

    ''' <summary>
    ''' 极简的 Python 字面量解析器：专门用于解析 npy 头部字典与 dtype 描述文本。
    ''' </summary>
    ''' <remarks>
    ''' 支持的语法子集：
    ''' 
    ''' <list type="bullet">
    ''' <item>字典 <c>{...}</c>，映射为 <c>Dictionary(Of Object, Object)</c>；</item>
    ''' <item>列表 <c>[...]</c>，映射为 <c>List(Of Object)</c>；</item>
    ''' <item>元组 <c>(...)</c>，映射为 <c>Object()</c>；</item>
    ''' <item>单双引号字符串（含 <c>\n</c>/<c>\t</c>/<c>\xNN</c>/<c>\uNNNN</c>/<c>\UNNNNNNNN</c> 转义）；</item>
    ''' <item><c>True</c> / <c>False</c> / <c>None</c> 常量；</item>
    ''' <item>十进制整数与浮点数字面量。</item>
    ''' </list>
    ''' 
    ''' 它不实现 Python 的运算符、注释、bytes 字面量与任意表达式；npy 头部并不需要这些特性。
    ''' </remarks>
    Friend Module PyLiteral

        ''' <summary>
        ''' 解析一段 Python 字面量文本。
        ''' </summary>
        ''' <param name="text">字面量文本</param>
        ''' <returns>字符串 / 布尔 / <c>Nothing</c> / <c>Long</c> / <c>Double</c> /
        ''' <c>Dictionary(Of Object, Object)</c> / <c>List(Of Object)</c> / <c>Object()</c></returns>
        Friend Function Parse(text As String) As Object
            If text Is Nothing Then
                Throw New ArgumentNullException(NameOf(text))
            End If

            Dim pos As Integer = 0

            Return ParseValue(text, pos)
        End Function

        Private Function ParseValue(text As String, ByRef pos As Integer) As Object
            Call SkipSpaces(text, pos)

            If pos >= text.Length Then
                Throw NotLiteral(text, pos, "unexpected end of text")
            End If

            Dim c As Char = text(pos)

            Select Case c
                Case "{"c
                    Return ParseDict(text, pos)
                Case "["c
                    Return ParseList(text, pos)
                Case "("c
                    Return ParseTuple(text, pos)
                Case "'"c, """"c
                    Return ParseString(text, pos)
                Case Else
                    If Char.IsDigit(c) OrElse c = "-"c OrElse c = "+"c OrElse c = "."c Then
                        Return ParseNumber(text, pos)
                    Else
                        Return ParseIdentifier(text, pos)
                    End If
            End Select
        End Function

        Private Function ParseDict(text As String, ByRef pos As Integer) As Dictionary(Of Object, Object)
            Dim dict As New Dictionary(Of Object, Object)()

            pos += 1 ' 跳过 '{'

            Do
                Call SkipSpaces(text, pos)

                If pos >= text.Length Then
                    Throw NotLiteral(text, pos, "unterminated dict literal")
                End If
                If text(pos) = "}"c Then
                    pos += 1
                    Exit Do
                End If

                Dim key As Object = ParseValue(text, pos)

                Call SkipSpaces(text, pos)

                If pos >= text.Length OrElse text(pos) <> ":"c Then
                    Throw NotLiteral(text, pos, "':' expected in dict literal")
                End If

                pos += 1

                Dim value As Object = ParseValue(text, pos)

                dict(key) = value

                Call SkipSpaces(text, pos)

                If pos >= text.Length Then
                    Throw NotLiteral(text, pos, "unterminated dict literal")
                End If

                If text(pos) = ","c Then
                    pos += 1
                ElseIf text(pos) = "}"c Then
                    pos += 1
                    Exit Do
                Else
                    Throw NotLiteral(text, pos, "',' or '}' expected in dict literal")
                End If
            Loop

            Return dict
        End Function

        Private Function ParseList(text As String, ByRef pos As Integer) As List(Of Object)
            Dim list As New List(Of Object)()

            pos += 1 ' 跳过 '['

            Do
                Call SkipSpaces(text, pos)

                If pos >= text.Length Then
                    Throw NotLiteral(text, pos, "unterminated list literal")
                End If
                If text(pos) = "]"c Then
                    pos += 1
                    Exit Do
                End If

                list.Add(ParseValue(text, pos))

                Call SkipSpaces(text, pos)

                If pos >= text.Length Then
                    Throw NotLiteral(text, pos, "unterminated list literal")
                End If

                If text(pos) = ","c Then
                    pos += 1
                ElseIf text(pos) = "]"c Then
                    pos += 1
                    Exit Do
                Else
                    Throw NotLiteral(text, pos, "',' or ']' expected in list literal")
                End If
            Loop

            Return list
        End Function

        Private Function ParseTuple(text As String, ByRef pos As Integer) As Object()
            Dim items As New List(Of Object)()

            pos += 1 ' 跳过 '('

            Do
                Call SkipSpaces(text, pos)

                If pos >= text.Length Then
                    Throw NotLiteral(text, pos, "unterminated tuple literal")
                End If
                If text(pos) = ")"c Then
                    pos += 1
                    Exit Do
                End If

                items.Add(ParseValue(text, pos))

                Call SkipSpaces(text, pos)

                If pos >= text.Length Then
                    Throw NotLiteral(text, pos, "unterminated tuple literal")
                End If

                If text(pos) = ","c Then
                    pos += 1
                ElseIf text(pos) = ")"c Then
                    pos += 1
                    Exit Do
                Else
                    Throw NotLiteral(text, pos, "',' or ')' expected in tuple literal")
                End If
            Loop

            Return items.ToArray
        End Function

        Private Function ParseString(text As String, ByRef pos As Integer) As String
            Dim quote As Char = text(pos)
            Dim sb As New StringBuilder()

            pos += 1

            Do While pos < text.Length
                Dim c As Char = text(pos)

                If c = quote Then
                    pos += 1
                    Return sb.ToString()
                ElseIf c = "\"c Then
                    pos += 1

                    If pos >= text.Length Then
                        Throw NotLiteral(text, pos, "unterminated escape sequence")
                    End If

                    Dim esc As Char = text(pos)

                    Select Case esc
                        Case "n"c
                            sb.Append(vbLf)
                            pos += 1
                        Case "r"c
                            sb.Append(vbCr)
                            pos += 1
                        Case "t"c
                            sb.Append(vbTab)
                            pos += 1
                        Case "0"c
                            sb.Append(vbNullChar)
                            pos += 1
                        Case "a"c
                            sb.Append(ChrW(7))
                            pos += 1
                        Case "b"c
                            sb.Append(ChrW(8))
                            pos += 1
                        Case "f"c
                            sb.Append(ChrW(12))
                            pos += 1
                        Case "v"c
                            sb.Append(ChrW(11))
                            pos += 1
                        Case "x"c
                            sb.Append(ChrW(ParseHex(text, pos + 1, 2)))
                            pos += 3
                        Case "u"c
                            sb.Append(ChrW(ParseHex(text, pos + 1, 4)))
                            pos += 5
                        Case "U"c
                            sb.Append(Char.ConvertFromUtf32(ParseHex(text, pos + 1, 8)))
                            pos += 9
                        Case Else
                            ' \\ \' \" 以及未知转义都退化为字符本身
                            sb.Append(esc)
                            pos += 1
                    End Select
                Else
                    sb.Append(c)
                    pos += 1
                End If
            Loop

            Throw NotLiteral(text, pos, "unterminated string literal")
        End Function

        Private Function ParseNumber(text As String, ByRef pos As Integer) As Object
            Dim start As Integer = pos
            Dim isFloat As Boolean = False

            Do While pos < text.Length
                Dim c As Char = text(pos)

                If Char.IsDigit(c) Then
                    pos += 1
                ElseIf c = "-"c OrElse c = "+"c OrElse c = "."c Then
                    If c = "."c Then
                        isFloat = True
                    End If
                    pos += 1
                ElseIf c = "e"c OrElse c = "E"c
                    isFloat = True
                    pos += 1
                Else
                    Exit Do
                End If
            Loop

            Dim val As String = text.Substring(start, pos - start)

            If isFloat Then
                Return Double.Parse(val, NumberStyles.Float, CultureInfo.InvariantCulture)
            Else
                Return Long.Parse(val, NumberStyles.Integer, CultureInfo.InvariantCulture)
            End If
        End Function

        Private Function ParseIdentifier(text As String, ByRef pos As Integer) As Object
            Dim start As Integer = pos

            Do While pos < text.Length
                Dim c As Char = text(pos)

                If Char.IsLetterOrDigit(c) OrElse c = "_"c Then
                    pos += 1
                Else
                    Exit Do
                End If
            Loop

            Dim [name] As String = text.Substring(start, pos - start)

            Select Case [name]
                Case "True", "TRUE", "true"
                    Return True
                Case "False", "FALSE", "false"
                    Return False
                Case "None", "NONE", "none"
                    Return Nothing
                Case "nan", "NaN"
                    Return Double.NaN
                Case "inf", "Inf", "Infinity"
                    Return Double.PositiveInfinity
                Case Else
                    Throw NotLiteral(text, start, $"unsupported python literal token '{[name]}'")
            End Select
        End Function

        Private Function ParseHex(text As String, start As Integer, digits As Integer) As Integer
            If start + digits > text.Length Then
                Throw NotLiteral(text, start, "incomplete hexadecimal escape sequence")
            End If

            Return Integer.Parse(text.Substring(start, digits), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
        End Function

        Private Sub SkipSpaces(text As String, ByRef pos As Integer)
            Do While pos < text.Length
                Dim c As Char = text(pos)

                If Char.IsWhiteSpace(c) Then
                    pos += 1
                Else
                    Exit Do
                End If
            Loop
        End Sub

        Private Function NotLiteral(text As String, pos As Integer, message As String) As InvalidDataException
            Dim context As String = text
            Dim at As Integer = pos

            If context.Length > 64 Then
                at = System.Math.Min(at, context.Length - 1)
                context = context.Substring(0, System.Math.Min(context.Length, 64)) & "..."
            End If

            Return New InvalidDataException($"invalid python literal at position {at}: {message} (text: {context})")
        End Function

    End Module

End Namespace
