Namespace SVG.XML
    Public Class SvgFillRule
        Inherits SvgEnum
        Private Sub New(value As String)
            MyBase.New(value)
        End Sub

        Public Shared ReadOnly Property NonZero As SvgFillRule = New SvgFillRule("nonzero")

        Public Shared ReadOnly Property EvenOdd As SvgFillRule = New SvgFillRule("evenodd")
    End Class
End Namespace
