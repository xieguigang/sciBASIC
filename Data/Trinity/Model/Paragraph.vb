Namespace Model

    Public Class Paragraph

        Public Property sentences As Sentence()

        Public Overrides Function ToString() As String
            Return sentences.JoinBy(". ")
        End Function

    End Class
End Namespace