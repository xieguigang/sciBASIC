Imports System.Runtime.CompilerServices
Imports Vector = Microsoft.VisualBasic.Imaging.Drawing3D.Point3D

Namespace Drawing3D.Math3D

    Public Module VectorMath

        <Extension>
        Public Function CrossProduct(v1 As Vector, v2 As Vector) As Vector
            Dim i As Double = v1.Y * v2.Z - v2.Y * v1.Z
            Dim j As Double = -1 * (v1.X * v2.Z - v2.X * v1.Z)
            Dim k As Double = v1.X * v2.Y - v2.X * v1.Y

            Return New Vector(i, j, k)
        End Function

        <Extension>
        Public Function DotProduct(v1 As Vector, v2 As Vector) As Double
            Return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z
        End Function

        <Extension>
        Public Function Magnitude(v As Vector) As Double
            With v
                Return Math.Sqrt(.X ^ 2 + .Y ^ 2 + .Z ^ 2)
            End With
        End Function

        <Extension>
        Public Function Normalize(v As Vector) As Vector
            Dim magnitude As Double = v.Magnitude()

            ' If the magnitude is 0 then return the zero vector instead of dividing by 0
            If magnitude = 0 Then
                Return New Vector(0, 0, 0)
            Else
                Dim i = v.X / magnitude
                Dim j = v.Y / magnitude
                Dim k = v.Z / magnitude

                Return New Point3D(i, j, k)
            End If
        End Function
    End Module
End Namespace