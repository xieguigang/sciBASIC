Namespace Model

    Public Class Segment

        ''' <summary>
        ''' 带有前后顺序的单词列表
        ''' </summary>
        ''' <returns></returns>
        Public Property tokens As String()

        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return tokens.IsNullOrEmpty OrElse tokens.All(AddressOf TextRank.IsEmpty)
            End Get
        End Property

        Public Function has(token As String) As Boolean
            Return Array.IndexOf(tokens, token) > -1
        End Function

        ''' <summary>
        ''' exactly token matched
        ''' </summary>
        ''' <param name="token"></param>
        ''' <returns></returns>
        Public Function matchIndex(token As String) As Integer
            Return Array.IndexOf(tokens, token)
        End Function

        ''' <summary>
        ''' search for starts with [prefix]
        ''' </summary>
        ''' <param name="token"></param>
        ''' <returns></returns>
        Public Function searchIndex(token As String) As Integer
            For i As Integer = 0 To tokens.Length - 1
                If _tokens(i).StartsWith(token) Then
                    Return i
                End If
            Next

            Return -1
        End Function

        Public Overrides Function ToString() As String
            Return tokens.JoinBy(" ")
        End Function

    End Class
End Namespace