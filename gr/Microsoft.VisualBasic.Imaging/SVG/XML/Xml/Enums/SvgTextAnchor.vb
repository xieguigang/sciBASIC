Namespace SVG.XML.Enums

    Public Class SvgTextAnchor
        Inherits SvgEnum

        Private Sub New(value As String)
            MyBase.New(value)
        End Sub

        Public Shared ReadOnly Property Start As SvgTextAnchor = New SvgTextAnchor("start")

        Public Shared ReadOnly Property Middle As SvgTextAnchor = New SvgTextAnchor("middle")

        Public Shared ReadOnly Property [End] As SvgTextAnchor = New SvgTextAnchor("end")
    End Class
End Namespace
