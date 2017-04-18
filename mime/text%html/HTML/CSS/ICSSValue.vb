Namespace HTML.CSS

    Public MustInherit Class ICSSValue

        Public MustOverride ReadOnly Property CSSValue As String

        Public Overrides Function ToString() As String
            Return CSSValue
        End Function
    End Class
End Namespace