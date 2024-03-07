Namespace Language

    Public Class Styles

        Public Property code_span As Boolean
        Public Property quote As Boolean
        Public Property bold As Boolean
        Public Property italic As Boolean
        Public Property header_title As Integer

        Public Function makeBold() As Styles
            Dim copy As Styles = makeCopy()
            copy.bold = bold
            Return copy
        End Function

        Private Function makeCopy() As Styles
            Return New Styles With {
                .bold = bold,
                .code_span = code_span,
                .header_title = header_title,
                .italic = italic,
                .quote = quote
            }
        End Function

    End Class
End Namespace