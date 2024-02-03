Namespace SVG.XML.Enums

    Public Class SvgClipPathUnits
        Inherits SvgEnum
        Private Sub New(value As String)
            MyBase.New(value)
        End Sub

        Public Shared ReadOnly Property UserSpaceOnUse As SvgClipPathUnits = New SvgClipPathUnits("userSpaceOnUse")

        Public Shared ReadOnly Property ObjectBoundingBox As SvgClipPathUnits = New SvgClipPathUnits("objectBoundingBox")
    End Class
End Namespace
