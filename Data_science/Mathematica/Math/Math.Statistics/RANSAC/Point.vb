Namespace RANSAC

    Friend Class Point

        Public x, y, z As Double

        Public Sub New()

        End Sub

        Public Sub New(x As Double, y As Double, z As Double)
            Me.x = x
            Me.y = y
            Me.z = z
        End Sub

        Public Sub New(point As Double())
            x = point(0)
            y = point(1)
            z = point(2)
        End Sub
    End Class

End Namespace