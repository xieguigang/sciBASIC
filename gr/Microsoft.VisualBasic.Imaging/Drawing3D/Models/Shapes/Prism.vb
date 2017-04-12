Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.IsoMetric

Namespace Drawing3D.IsoMetric.Shapes


    ''' <summary>
    ''' Created by fabianterhorst on 01.04.17.
    ''' </summary>

    Public Class Prism
        Inherits Shape3D

        Public Sub New(ByVal origin As Point3D)
            Me.New(origin, 1, 1, 1)
        End Sub

        Public Sub New(ByVal origin As Point3D, ByVal dx As Double, ByVal dy As Double, ByVal dz As Double)
            MyBase.New()

            Dim ___paths As Path3D() = New Path3D(5) {}

            ' Squares parallel to the x-axis 
            Dim face1 As New Path3D(New Point3D() {origin, New Point3D(origin.X + dx, origin.Y, origin.Z), New Point3D(origin.X + dx, origin.Y, origin.Z + dz), New Point3D(origin.X, origin.Y, origin.Z + dz)
            })

            ' Push this face and its opposite 
            ___paths(0) = face1
            ___paths(1) = face1.Reverse().TranslatePoints(0, dy, 0)

            ' Square parallel to the y-axis 
            Dim face2 As New Path3D(New Point3D() {origin, New Point3D(origin.X, origin.Y, origin.Z + dz), New Point3D(origin.X, origin.Y + dy, origin.Z + dz), New Point3D(origin.X, origin.Y + dy, origin.Z)
            })
            ___paths(2) = face2
            ___paths(3) = face2.Reverse().TranslatePoints(dx, 0, 0)

            ' Square parallel to the xy-plane 
            Dim face3 As New Path3D(New Point3D() {origin, New Point3D(origin.X + dx, origin.Y, origin.Z), New Point3D(origin.X + dx, origin.Y + dy, origin.Z), New Point3D(origin.X, origin.Y + dy, origin.Z)
            })
            ' This surface is oriented backwards, so we need to reverse the points 
            ___paths(4) = face3.Reverse()
            ___paths(5) = face3.TranslatePoints(0, 0, dz)

            paths = ___paths.AsList
        End Sub
    End Class

End Namespace