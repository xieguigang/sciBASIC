Imports Microsoft.VisualBasic.Imaging.LayoutModel

Namespace Layouts.Cola

    Public Class Node3D : Inherits GraphNode
        ' if fixed, layout will not move the node from its specified starting position
        Public fixed As Boolean
        Public width As Double
        Public height As Double
        Public px As Double
        Public py As Double
        Public bounds As Rectangle2D
        Public variable As Variable

        Public x As Double, y As Double, z As Double

        Public Sub New(Optional x As Double = 0, Optional y As Double = 0, Optional z As Double = 0)
            Me.x = x
            Me.y = y
            Me.z = z
        End Sub
    End Class
End Namespace