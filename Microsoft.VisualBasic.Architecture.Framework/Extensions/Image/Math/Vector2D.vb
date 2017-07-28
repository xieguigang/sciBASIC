
Namespace Imaging.Math2D

    Public Class Vector2D
        Public x As Double
        Public y As Double

        Public Sub New()
            Me.New(0.0, 0.0)
        End Sub

        Public Sub New(paramDouble1 As Double, paramDouble2 As Double)
            Me.x = paramDouble1
            Me.y = paramDouble2
        End Sub

        Public Sub New(paramInt1 As Integer, paramInt2 As Integer)
            Me.x = paramInt1
            Me.y = paramInt2
        End Sub

        Public Overridable Function length() As Double
            Return Math.Sqrt(Me.x * Me.x + Me.y * Me.y)
        End Function

        Public Overridable Function reverse() As Vector2D
            Return New Vector2D(-Me.x, -Me.y)
        End Function

        Public Overridable Function multiple(paramDouble As Double) As Vector2D
            Return New Vector2D(paramDouble * Me.x, paramDouble * Me.y)
        End Function
    End Class
End Namespace
