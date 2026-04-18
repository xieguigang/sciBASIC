Imports System.Text

Namespace Scripting

    Public Module CCodeStringLiteralEscape

        Public Function EscapeForCStringLiteral(literal As String) As String
            If String.IsNullOrEmpty(literal) Then
                Return ""
            Else
                Return EscapeForCStringLiteralInternal(literal)
            End If
        End Function

        Private Function EscapeForCStringLiteralInternal(literal As String) As String
            Dim sb As New StringBuilder(literal.Length * 2)
            Dim i As Integer = 0

            While i < literal.Length
                Dim c As Char = literal(i)

                If c = "\"c Then
                    ' 遇到反斜杠，需要判断后续字符是否是合法的C语言转义字符
                    If i + 1 < literal.Length Then
                        Dim nextChar As Char = literal(i + 1)

                        If IsLegalCEscapeChar(nextChar, literal, i + 1) Then
                            ' 合法转义：原样保留反斜杠和后续字符（如 \t 保持为 \t）
                            sb.Append("\"c)
                            sb.Append(nextChar)
                            i += 2
                        Else
                            ' 非法转义：将反斜杠本身转义为双反斜杠（如 \{ 变为 \\{）
                            sb.Append("\\")
                            i += 1 ' 注意这里只加1，下一个循环会处理 nextChar
                        End If
                    Else
                        ' 反斜杠是字符串的最后一个字符，属于非法情况，转义为双反斜杠
                        sb.Append("\\")
                        i += 1
                    End If

                ElseIf c = """"c Then
                    ' C语言字符串中的双引号必须转义
                    sb.Append("\""")
                    i += 1

                    ' === 可选逻辑：如果你的XML中包含真实的控制字符（如真实的TAB/回车），而非 \t / \n 文本，需要下面这段 ===
                ElseIf c = ControlChars.Tab Then
                    sb.Append("\t")
                    i += 1
                ElseIf c = ControlChars.Lf Then
                    sb.Append("\n")
                    i += 1
                ElseIf c = ControlChars.Cr Then
                    sb.Append("\r")
                    i += 1
                    ' ==========================================

                Else
                    ' 其他普通字符原样追加
                    sb.Append(c)
                    i += 1
                End If
            End While

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' 判断反斜杠后的字符是否构成了合法的C语言转义序列
        ''' </summary>
        Private Function IsLegalCEscapeChar(c As Char, input As String, currentIndex As Integer) As Boolean
            ' 1. 简单转义字符
            Select Case c
                Case "'"c, """"c, "?"c, "\"c, "a"c, "b"c, "f"c, "n"c, "r"c, "t"c, "v"c
                    Return True
                Case Else
                    ' 继续判断数字情况
            End Select

            ' 2. 八进制转义 (\0 到 \7)
            If c >= "0"c AndAlso c <= "7"c Then
                Return True
            End If

            ' 3. 十六进制转义 (\x 后必须跟至少一个十六进制数字)
            If c = "x"c OrElse c = "X"c Then
                If currentIndex + 1 < input.Length Then
                    Dim hexChar As Char = input(currentIndex + 1)
                    Return (hexChar >= "0"c AndAlso hexChar <= "9"c) OrElse
                       (hexChar >= "a"c AndAlso hexChar <= "f"c) OrElse
                       (hexChar >= "A"c AndAlso hexChar <= "F"c)
                End If
                ' 如果 \x 后面没有字符了，属于非法的C语言转义
                Return False
            End If

            ' 4. 其他所有字符（如 {, c, ! 等）跟在 \ 后面都属于非法转义
            Return False
        End Function
    End Module
End Namespace