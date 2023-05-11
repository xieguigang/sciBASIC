Namespace SVG.PathHelper

    Public Class T : Inherits Command
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
            X = Double.Parse(tokens(0))
            Y = Double.Parse(tokens(1))
        End Sub

        Public Overrides Sub Scale(factor As Double)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub Translate(deltaX As Double, deltaY As Double)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Function ToString() As String
            Return $"{If(isRelative, "t"c, "T"c)}{X},{Y}"
        End Function
    End Class
End Namespace