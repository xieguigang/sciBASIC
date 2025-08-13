
Imports Microsoft.VisualBasic.Imaging.Math2D

Namespace Drawing2D.Math2D.DelaunayVoronoi
    Public Class Circle

        Public center As Vector2D
        Public radius As Single

        Public Sub New(centerX As Single, centerY As Single, radius As Single)
            center = New Vector2D(centerX, centerY)
            Me.radius = radius
        End Sub

        Public Overrides Function ToString() As String
            Return "Circle (center: " & center.ToString() & "; radius: " & radius.ToString() & ")"
        End Function
    End Class
End Namespace
