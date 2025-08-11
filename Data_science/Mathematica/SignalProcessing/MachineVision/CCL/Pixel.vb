Imports System.Drawing

Namespace CCL
    Public Class Pixel

        Public Property Position As Point
        Public Property color As Color

        Public Sub New(Position As Point, color As Color)
            Me.Position = Position
            Me.color = color
        End Sub
    End Class
End Namespace
