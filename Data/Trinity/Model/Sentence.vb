Namespace Model

    Public Class Sentence

        Public Property segments As Segment()

        Public Overrides Function ToString() As String
            Return segments.JoinBy("; ")
        End Function

    End Class
End Namespace