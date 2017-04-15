Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Isometric

Namespace Drawing3D.Isometric.Shapes

    Public Class Knot
        Inherits Shape3D

        Public Sub New(origin As Point3D)
            Push((New Prism(Math3D.ORIGIN, 5, 1, 1)).paths)
            Push((New Prism(New Point3D(4, 1, 0), 1, 4, 1)).paths)
            Push((New Prism(New Point3D(4, 4, -2), 1, 1, 3)).paths)
            Push(New Path3D(New Point3D() {New Point3D(0, 0, 2), New Point3D(0, 0, 1), New Point3D(1, 0, 1), New Point3D(1, 0, 2)}))
            Push(New Path3D(New Point3D() {New Point3D(0, 0, 2), New Point3D(0, 1, 2), New Point3D(0, 1, 1), New Point3D(0, 0, 1)}))
            ScalePath3Ds(Math3D.ORIGIN, 1.0 / 5.0)
            TranslatePath3Ds(-0.1, 0.15, 0.4)
            TranslatePath3Ds(origin.X, origin.Y, origin.Z)
        End Sub
    End Class

End Namespace