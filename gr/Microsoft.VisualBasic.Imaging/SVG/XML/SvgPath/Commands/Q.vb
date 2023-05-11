Namespace SVG.PathHelper

    Public Class Q : Inherits Command
        Public Property X1 As Double
        Public Property Y1 As Double
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
            X = Double.Parse(tokens(2))
            Y = Double.Parse(tokens(3))
        End Sub

        Public Overrides Sub Scale(factor As Double)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub Translate(deltaX As Double, deltaY As Double)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Function ToString() As String
            Return $"{If(isRelative, "q"c, "Q"c)}{X1},{Y1} {X},{Y}"
        End Function
    End Class
End Namespace