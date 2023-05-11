Namespace SVG.Path

    Public Class Z : Inherits Command

        Public Sub New(ByVal Optional isRelative As Boolean = False)
            MyBase.isRelative = isRelative
        End Sub

        Public Overrides Sub Scale(ByVal factor As Double)

        End Sub

        Public Overrides Sub Translate(ByVal deltaX As Double, ByVal deltaY As Double)

        End Sub

        Public Overrides Function ToString() As String
            Return $"{If(isRelative, "z"c, "Z"c)}"
        End Function

    End Class
End Namespace