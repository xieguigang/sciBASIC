Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace Data.Trinity.NLP

    Public Module TextSplit

        ''' <summary>
        ''' 自定义分词器：兼容英文/数字词与中文（CJK）按单字切分。
        ''' 中英文统一小写、去首尾空白后：
        ''' - 连续的 [a-z0-9] 视为一个英文/数字词；
        ''' - 连续的汉字逐个作为 token（与 <see cref="Search"/> 对查询关键词采用同一分词逻辑，保证粒度一致）。
        ''' </summary>
        Public Function MakeWords(text As String) As IEnumerable(Of String)
            If String.IsNullOrWhiteSpace(text) Then
                Return Enumerable.Empty(Of String)()
            End If

            text = Strings.Trim(text).ToLower
            Dim tokens As New List(Of String)

            For Each m As Match In Regex.Matches(text, "[a-z0-9]+")
                tokens.Add(m.Value)
            Next

            For Each m As Match In Regex.Matches(text, "[一-鿿]+")
                Dim s As String = m.Value
                For i As Integer = 0 To s.Length - 1
                    tokens.Add(Char.ToString(s(i)))
                Next
            Next

            Return tokens.Distinct()
        End Function
    End Module
End Namespace