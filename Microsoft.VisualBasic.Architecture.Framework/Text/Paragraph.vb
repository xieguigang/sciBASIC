Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace Text

    Public Module Paragraph

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="text$"></param>
        ''' <param name="len%"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 假若长度分割落在单词内，则添加一个连接符，假如是空格或者标点符号，则不处理
        ''' </remarks>
        Public Iterator Function Split(text$, len%) As IEnumerable(Of String)
            Dim lines$() = text.lTokens

            For Each line$ In lines
                Dim s As New Value(Of String)
                Dim left% = Scan0 + 1

                Do While (s = Mid$(line$, left, len)).Length = len
                    If s.value.Length = 0 Then
                        Exit Do ' 已经结束了
                    Else
                        left += len
                    End If

                    Dim nextLine$ = Mid(line, left, len) _
                        .Replace(ASCII.TAB, " "c)
                    Dim part As NamedValue(Of String) =
                        nextLine.GetTagValue

                    If Not String.IsNullOrEmpty(part.Name) Then ' 有空格
                        s.value = Trim((+s) & part.Name)
                        left += part.Name.Length + 1
                    ElseIf nextLine.First = " "c Then ' 第一个字符是空格，则忽略掉
                        left += 1
                    Else
                        s.value &= nextLine  ' 是剩余的结束部分
                        Yield +s
                        s.value = Nothing ' 必须要value值删除字符串，否则会重复出现最后一行
                        Exit Do
                    End If

                    Yield +s
                Loop

                If Not String.IsNullOrEmpty(+s) Then
                    Yield Trim(+s)
                Else
                    Yield vbCrLf
                End If
            Next
        End Function
    End Module
End Namespace