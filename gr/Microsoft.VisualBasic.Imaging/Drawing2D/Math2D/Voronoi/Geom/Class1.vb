Imports std = System.Math

Namespace Drawing2D.Math2D.DelaunayVoronoi

    Public Class Vector2
        Public X, Y As Single

        Public Shared ReadOnly Property Zero As Vector2
            Get
                Return New Vector2(0, 0)
            End Get
        End Property

        Public ReadOnly Property Length As Single
            Get
                Return std.Sqrt(X * X + Y * Y)
            End Get
        End Property

        Public Sub New()
        End Sub
        Public Sub New(x As Single, y As Single)
            Me.X = x
            Me.Y = y
        End Sub

        Public Shared Operator -(v1 As Vector2, v2 As Vector2) As Vector2
            Return New Vector2(v1.X - v2.X, v1.Y - v2.Y)
        End Operator
    End Class
End Namespace