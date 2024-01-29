Namespace SVG.XML.Enums
    Public Class SvgDominantBaseline
        Inherits SvgEnum
        Private Sub New(value As String)
            MyBase.New(value)
        End Sub

        Public Shared ReadOnly Property Auto As SvgDominantBaseline = New SvgDominantBaseline("auto")

        Public Shared ReadOnly Property UseScript As SvgDominantBaseline = New SvgDominantBaseline("use-script")

        Public Shared ReadOnly Property NoChange As SvgDominantBaseline = New SvgDominantBaseline("no-change")

        Public Shared ReadOnly Property ResetSize As SvgDominantBaseline = New SvgDominantBaseline("reset-size")

        Public Shared ReadOnly Property Ideographic As SvgDominantBaseline = New SvgDominantBaseline("ideagraphic")

        Public Shared ReadOnly Property Alphabetic As SvgDominantBaseline = New SvgDominantBaseline("alphabetic")

        Public Shared ReadOnly Property Hanging As SvgDominantBaseline = New SvgDominantBaseline("hanging")

        Public Shared ReadOnly Property Mathematical As SvgDominantBaseline = New SvgDominantBaseline("mathematical")

        Public Shared ReadOnly Property Central As SvgDominantBaseline = New SvgDominantBaseline("central")

        Public Shared ReadOnly Property Middle As SvgDominantBaseline = New SvgDominantBaseline("middle")

        Public Shared ReadOnly Property TextAfterEdge As SvgDominantBaseline = New SvgDominantBaseline("text-after-edge")

        Public Shared ReadOnly Property TextBeforeEdge As SvgDominantBaseline = New SvgDominantBaseline("text-before-edge")
    End Class
End Namespace
