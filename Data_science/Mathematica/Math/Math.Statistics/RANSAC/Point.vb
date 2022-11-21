Namespace RANSAC

    Friend Class Point

        Public x, y, z As Double

        Public Sub New()

        End Sub

        Public Sub New(ByVal x As Double, ByVal y As Double, ByVal z As Double)
            Me.x = x
            Me.y = y
            Me.z = z
        End Sub

        Public Sub New(ByVal point As Double())
            x = point(0)
            y = point(1)
            z = point(2)
        End Sub
    End Class

End Namespace