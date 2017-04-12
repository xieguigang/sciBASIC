Imports System
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.IsoMetric
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D

Namespace Drawing3D.IsoMetric.Shapes


    ''' <summary>
    ''' Created by fabianterhorst on 02.04.17.
    ''' </summary>

    Public Class Pyramid
        Inherits Shape3D

        Public Sub New(ByVal origin As Point3D)
            Me.New(origin, 1, 1, 1)
        End Sub

        Public Sub New(ByVal origin As Point3D, ByVal dx As Double, ByVal dy As Double, ByVal dz As Double)
            MyBase.New()

            Dim ___paths As Path3D() = New Path3D(3) {}

            ' Path parallel to the x-axis 
            Dim face1 As New Path3D({origin, New Point3D(origin.X + dx, origin.Y, origin.Z), New Point3D(origin.X + dx / 2.0, origin.Y + dy / 2.0, origin.Z + dz)
            })
            ' Push the face, and its opposite face, by rotating around the Z-axis 
            ___paths(0) = face1
            ___paths(1) = face1.RotateZ(Math3D.ORIGIN.Translate(dx / 2.0, dy / 2.0, 0), Math.PI)

            ' Path parallel to the y-axis 
            Dim face2 As New Path3D(New Point3D() {origin, New Point3D(origin.X + dx / 2, origin.Y + dy / 2, origin.Z + dz), New Point3D(origin.X, origin.Y + dy, origin.Z)
            })
            ___paths(2) = face2
            ___paths(3) = face2.RotateZ(origin.Translate(dx / 2.0, dy / 2.0, 0), Math.PI)
            paths = ___paths.AsList
        End Sub
    End Class

End Namespace