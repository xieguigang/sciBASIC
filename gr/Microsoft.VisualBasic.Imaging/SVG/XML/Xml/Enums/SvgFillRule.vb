Namespace SVG.XML.Enums

    Public Class SvgFillRule : Inherits SvgEnum

        Public Shared ReadOnly Property NonZero As New SvgFillRule("nonzero")
        Public Shared ReadOnly Property EvenOdd As New SvgFillRule("evenodd")

        Private Sub New(value As String)
            MyBase.New(value)
        End Sub

    End Class
End Namespace
