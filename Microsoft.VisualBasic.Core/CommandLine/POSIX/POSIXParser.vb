Imports Microsoft.VisualBasic.CommandLine.Parsers

Namespace CommandLine.POSIX

    Module POSIXParser

        Public Iterator Function JoinTokens(tokens As IEnumerable(Of String)) As IEnumerable(Of String)
            Dim continuteToken As String = Nothing

            For Each item As String In tokens
                If CliArgumentParsers.IsPossibleLogicFlag(item) Then
                    ' 在这里使用nothing来和""产生的空字符串进行区分
                    If Not continuteToken Is Nothing Then
                        Yield continuteToken
                        continuteToken = Nothing
                    End If

                    Yield item
                ElseIf continuteToken Is Nothing Then
                    continuteToken = item
                Else
                    continuteToken = $"{continuteToken} {item}"
                End If
            Next

            If Not continuteToken Is Nothing Then
                Yield continuteToken
            End If
        End Function
    End Module
End Namespace