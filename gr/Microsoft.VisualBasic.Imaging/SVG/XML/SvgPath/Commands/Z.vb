Namespace SVG.PathHelper

    Public Class Z : Inherits Command

        Public Sub New(Optional isRelative As Boolean = False)
            MyBase.isRelative = isRelative
        End Sub

        Public Overrides Sub Scale(factor As Double)

        End Sub

        Public Overrides Sub Translate(deltaX As Double, deltaY As Double)

        End Sub

        Public Overrides Function ToString() As String
            Return $"{If(isRelative, "z"c, "Z"c)}"
        End Function

    End Class
End Namespace