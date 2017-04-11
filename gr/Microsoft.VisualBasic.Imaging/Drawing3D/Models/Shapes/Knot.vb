Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.IsoMetric

Namespace Drawing3D.IsoMetric.Shapes

    Public Class Knot
        Inherits Shape3D

        Public Sub New(ByVal origin As Point3D)
            push((New Prism(Math3D.ORIGIN, 5, 1, 1)).paths)
            push((New Prism(New Point3D(4, 1, 0), 1, 4, 1)).paths)
            push((New Prism(New Point3D(4, 4, -2), 1, 1, 3)).paths)
            push(New Path3D(New Point3D() {New Point3D(0, 0, 2), New Point3D(0, 0, 1), New Point3D(1, 0, 1), New Point3D(1, 0, 2)}))
            push(New Path3D(New Point3D() {New Point3D(0, 0, 2), New Point3D(0, 1, 2), New Point3D(0, 1, 1), New Point3D(0, 0, 1)}))
            scalePath3Ds(Math3D.ORIGIN, 1.0 / 5.0)
            translatePath3Ds(-0.1, 0.15, 0.4)
            translatePath3Ds(origin.X, origin.Y, origin.Z)
        End Sub
    End Class

End Namespace