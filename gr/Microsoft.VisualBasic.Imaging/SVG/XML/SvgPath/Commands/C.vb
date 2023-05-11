Namespace SVG.PathHelper

    Public Class C : Inherits Command
        Public Property X1 As Double
        Public Property Y1 As Double
        Public Property X2 As Double
        Public Property Y2 As Double
        Public Property X As Double
        Public Property Y As Double

        Public Sub New(text As String, Optional isRelative As Boolean = False)
            MyBase.isRelative = isRelative
            Dim tokens = Parse(text)
            Me.MapTokens(tokens)
        End Sub

        Public Sub New(tokens As List(Of String), Optional isRelative As Boolean = False)
            MyBase.isRelative = isRelative
            Me.MapTokens(tokens)
        End Sub

        Private Sub MapTokens(tokens As List(Of String))
            X1 = Double.Parse(tokens(0))
            Y1 = Double.Parse(tokens(1))
            X2 = Double.Parse(tokens(2))
            Y2 = Double.Parse(tokens(3))
            X = Double.Parse(tokens(4))
            Y = Double.Parse(tokens(5))
        End Sub

        Public Overrides Sub Scale(factor As Double)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub Translate(deltaX As Double, deltaY As Double)
            X1 += deltaX
            Y1 += deltaY
            X2 += deltaX
            Y2 += deltaY
            X += deltaX
            Y += deltaY
        End Sub

        Public Overrides Function ToString() As String
            Return $"{If(isRelative, "c"c, "C"c)}{X1},{Y1} {X2},{Y2} {X},{Y}"
        End Function
    End Class
End Namespace