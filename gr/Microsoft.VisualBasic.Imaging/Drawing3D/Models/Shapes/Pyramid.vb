Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D

Namespace Drawing3D.Models.Isometric.Shapes

    ''' <summary>
    ''' Created by fabianterhorst on 02.04.17.
    ''' </summary>
    Public Class Pyramid : Inherits Shape3D

        Public Sub New(origin As Point3D)
            Me.New(origin, 1, 1, 1)
        End Sub

        Public Sub New(origin As Point3D, dx As Double, dy As Double, dz As Double)
            MyBase.New()

            ' Path parallel to the x-axis 
            Dim face1 As Path3D = {
                origin,
                New Point3D(origin.X + dx, origin.Y, origin.Z),
                New Point3D(origin.X + dx / 2.0, origin.Y + dy / 2.0, origin.Z + dz)
            }
            Dim paths As Path3D() = New Path3D(3) {}
            ' Push the face, and its opposite face, by rotating around the Z-axis 
            paths(0) = face1
            paths(1) = face1.RotateZ(origin.Translate(dx / 2.0, dy / 2.0, 0), Math.PI)

            ' Path parallel to the y-axis 
            Dim face2 As Path3D = {
                origin,
                New Point3D(origin.X + dx / 2, origin.Y + dy / 2, origin.Z + dz),
                New Point3D(origin.X, origin.Y + dy, origin.Z)
            }
            paths(2) = face2
            paths(3) = face2.RotateZ(origin.Translate(dx / 2.0, dy / 2.0, 0), Math.PI)

            MyBase.paths = paths.AsList
        End Sub
    End Class
End Namespace