Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Text.Parser

    Public Module DelimiterParser

        ''' <summary>
        ''' 非正则表达式命令行解析引擎
        ''' </summary>
        ''' <param name="cli">the commandline string</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' + 双引号表示一个完整的token
        ''' + 空格为分隔符
        ''' </remarks>
        <Extension>
        Public Function GetTokens(cli As String) As String()
            Dim buffer As New CharPtr(cli)
            Dim tokens As New List(Of String)
            Dim tmp As New List(Of Char)
            Dim c As Char
            Dim quotOpen As Boolean = False

            Do While Not buffer.EndRead
                c = (+buffer)

                If quotOpen Then

                    ' 双引号是结束符，但是可以使用\"进行转义
                    If c <> ASCII.Quot Then
                        tmp += c
                    Else
                        If tmp.StartEscaping Then
                            tmp.RemoveLast
                            tmp += c
                        Else
                            ' 结束
                            tokens += tmp.CharString
                            tmp *= 0
                            quotOpen = False

                        End If
                    End If

                Else
                    If c = ASCII.Quot AndAlso tmp = 0 Then
                        quotOpen = True
                    ElseIf c = " "c Then
                        ' 分隔符
                        If tmp <> 0 Then
                            tokens += tmp.CharString
                            tmp *= 0
                        End If
                    Else
                        tmp += c
                    End If
                End If
            Loop

            If tmp <> 0 Then
                tokens += New String(tmp)
            End If

            Return tokens
        End Function
    End Module
End Namespace