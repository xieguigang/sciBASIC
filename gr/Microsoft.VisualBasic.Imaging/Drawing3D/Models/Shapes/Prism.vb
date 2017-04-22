Namespace Drawing3D.Models.Isometric.Shapes

    ''' <summary>
    ''' Created by fabianterhorst on 01.04.17.
    ''' </summary>
    Public Class Prism : Inherits Shape3D

        Public Sub New(origin As Point3D)
            Me.New(origin, 1, 1, 1)
        End Sub

        Public Sub New(origin As Point3D, dx As Double, dy As Double, dz As Double)
            MyBase.New()

            Dim paths As Path3D() = New Path3D(5) {}

            ' Squares parallel to the x-axis 
            Dim face1 As Path3D = {
                origin,
                New Point3D(origin.X + dx, origin.Y, origin.Z),
                New Point3D(origin.X + dx, origin.Y, origin.Z + dz),
                New Point3D(origin.X, origin.Y, origin.Z + dz)
            }

            ' Push this face and its opposite 
            paths(0) = face1
            paths(1) = face1.Reverse().TranslatePoints(0, dy, 0)

            ' Square parallel to the y-axis 
            Dim face2 As Path3D = {
                origin,
                New Point3D(origin.X, origin.Y, origin.Z + dz),
                New Point3D(origin.X, origin.Y + dy, origin.Z + dz),
                New Point3D(origin.X, origin.Y + dy, origin.Z)
            }
            paths(2) = face2
            paths(3) = face2.Reverse().TranslatePoints(dx, 0, 0)

            ' Square parallel to the xy-plane 
            Dim face3 As Path3D = {
                origin,
                New Point3D(origin.X + dx, origin.Y, origin.Z),
                New Point3D(origin.X + dx, origin.Y + dy, origin.Z),
                New Point3D(origin.X, origin.Y + dy, origin.Z)
            }
            ' This surface is oriented backwards, so we need to reverse the points 
            paths(4) = face3.Reverse()
            paths(5) = face3.TranslatePoints(0, 0, dz)

            MyBase.paths = paths.AsList
        End Sub
    End Class
End Namespace