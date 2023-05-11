Namespace SVG.Path

    Public Class L : Inherits Command
        Public Property X As Double
        Public Property Y As Double

        Public Sub New(ByVal x As Double, ByVal y As Double)
            Me.X = x
            Me.Y = y
        End Sub

        Public Sub New(ByVal text As String, ByVal Optional isRelative As Boolean = False)
            MyBase.isRelative = isRelative
            Dim tokens = Parse(text)
            Me.MapTokens(tokens)
        End Sub

        Public Sub New(ByVal tokens As List(Of String), ByVal Optional isRelative As Boolean = False)
            MyBase.isRelative = isRelative
            Me.MapTokens(tokens)
        End Sub

        Private Sub MapTokens(tokens As System.Collections.Generic.List(Of String))
            X = Double.Parse(tokens(0))
            Y = Double.Parse(tokens(1))
        End Sub

        Public Overrides Sub Scale(ByVal factor As Double)
            X *= factor
            Y *= factor
        End Sub

        Public Overrides Sub Translate(ByVal deltaX As Double, ByVal deltaY As Double)
            X += deltaX
            Y += deltaY
        End Sub

        Public Overrides Function ToString() As String
            Return $"{If(isRelative, "l"c, "L"c)}{X} {Y}"
        End Function

    End Class
End Namespace