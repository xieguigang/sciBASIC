Imports stdNum = System.Math

Namespace RANSAC

    Friend Class Vector : Inherits Point

        Public ReadOnly Property magnitude As Double
            Get
                Return stdNum.Sqrt(stdNum.Pow(x, 2) + stdNum.Pow(y, 2) + stdNum.Pow(z, 2))
            End Get
        End Property

        Public ReadOnly Property normalized As Vector
            Get
                Return New Vector(x / magnitude, y / magnitude, z / magnitude)
            End Get
        End Property

        Public Sub New(x As Double, y As Double, z As Double)
            MyBase.New(x, y, z)

        End Sub

        Public Sub New(point As Double())
            MyBase.New(point)

        End Sub

        Public Shared Operator +(v1 As Vector, v2 As Vector) As Vector
            Return New Vector(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z)
        End Operator

        Public Shared Operator *(v As Vector, num As Double) As Vector
            Return New Vector(v.x * num, v.y * num, v.z * num)
        End Operator
    End Class
End Namespace