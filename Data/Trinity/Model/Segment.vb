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

        Public Overrides Function ToString() As String
            Return tokens.JoinBy(" ")
        End Function

    End Class
End Namespace