Namespace SVG.Path

    Public Class V : Inherits Command
        Public Property Y As Double

        Public Sub New(ByVal y As Double)
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
            Y = Double.Parse(tokens(0))
        End Sub

        Public Overrides Sub Scale(ByVal factor As Double)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub Translate(ByVal deltaX As Double, ByVal deltaY As Double)
            Throw New NotImplementedException()
        End Sub
    End Class
End Namespace