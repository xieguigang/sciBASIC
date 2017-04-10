Imports System.Runtime.CompilerServices

Namespace Drawing3D.Math3D

    Public Module Transformation

        Public ReadOnly Property ORIGIN As New Point3D(0, 0, 0)

        ''' <summary>
        ''' Translate a point from a given dx, dy, and dz
        ''' </summary>
        ''' 
        <Extension>
        Public Function Translate(base As Point3D, dx As Double, dy As Double, dz As Double) As Point3D
            With base
                Return New Point3D(.X + dx, .Y + dy, .Z + dz)
            End With
        End Function

        ''' <summary>
        ''' Scale a point about a given origin
        ''' </summary>
        ''' 
        <Extension>
        Public Function Scale(base As Point3D, origin As Point3D, dx As Double, dy As Double, dz As Double) As Point3D
            With base
                Return New Point3D(
                    (.X - origin.X) * dx + origin.X,
                    (.Y - origin.Y) * dy + origin.Y,
                    (.Z - origin.Z) * dz + origin.Z)
            End With
        End Function

        <Extension>
        Public Function Scale(base As Point3D, origin As Point3D, dx As Double) As Point3D
            Return base.Scale(origin, dx, dx, dx)
        End Function

        <Extension>
        Public Function Scale(base As Point3D, origin As Point3D, dx As Double, dy As Double) As Point3D
            Return base.Scale(origin, dx, dy, 1)
        End Function

        ''' <summary>
        ''' Distance between two points
        ''' </summary>
        <Extension> Public Function distance(p1 As Point3D, p2 As Point3D) As Double
            Dim dx As Double = p2.X - p1.X
            Dim dy As Double = p2.Y - p1.Y
            Dim dz As Double = p2.Z - p1.Z

            Return Math.Sqrt(dx * dx + dy * dy + dz * dz)
        End Function
    End Module
End Namespace