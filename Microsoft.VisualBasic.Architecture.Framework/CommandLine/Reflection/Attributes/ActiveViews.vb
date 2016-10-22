Namespace CommandLine.Reflection

    Public Class ActiveViews

        Public ReadOnly Property Views As String

        Sub New(view As String)
            Views = view
        End Sub

        Public Overrides Function ToString() As String
            Return Views
        End Function
    End Class
End Namespace