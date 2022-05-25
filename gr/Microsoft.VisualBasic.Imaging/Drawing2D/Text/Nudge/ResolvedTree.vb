Namespace Drawing2D.Text.Nudge

    Public Class ResolvedTree

        Public Property parent As CloudOfTextRectangle
        Public Property childrens As ResolvedTree()

        Public Overrides Function ToString() As String
            If childrens.IsNullOrEmpty Then
                Return parent.ToString
            Else
                Return $"{parent.ToString}: {{{childrens.JoinBy("; ")}}}"
            End If
        End Function

    End Class
End Namespace