Namespace SVG.Path

    Public Class T : Inherits Command
        Public Property X As Double
        Public Property Y As Double

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
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub Translate(ByVal deltaX As Double, ByVal deltaY As Double)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Function ToString() As String
            Return $"{If(isRelative, "t"c, "T"c)}{X},{Y}"
        End Function
    End Class
End Namespace