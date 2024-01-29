Namespace SVG.XML.Enums
    Public Class SvgStrokeLineCap
        Inherits SvgEnum
        Private Sub New(value As String)
            MyBase.New(value)
        End Sub

        Public Shared ReadOnly Property Butt As SvgStrokeLineCap = New SvgStrokeLineCap("butt")

        Public Shared ReadOnly Property Round As SvgStrokeLineCap = New SvgStrokeLineCap("round")

        Public Shared ReadOnly Property Square As SvgStrokeLineCap = New SvgStrokeLineCap("square")
    End Class
End Namespace
