Imports System
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.IsoMetric
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D

Namespace Drawing3D.IsoMetric.Shapes


    ''' <summary>
    ''' Created by fabianterhorst on 02.04.17.
    ''' </summary>

    Public Class Octahedron
        Inherits Shape3D

        Public Sub New(ByVal origin As Point3D)
            MyBase.New()
            Dim center As Point3D = origin.Translate(0.5, 0.5, 0.5)
            Dim upperTriangle As New Path3D(New Point3D() {origin.translate(0, 0, 0.5), origin.translate(0.5, 0.5, 1), origin.translate(0, 1, 0.5)})
            Dim lowerTriangle As New Path3D(New Point3D() {origin.Translate(0, 0, 0.5), origin.Translate(0, 1, 0.5), origin.Translate(0.5, 0.5, 0)})
            Dim ___paths As Path3D() = New Path3D(7) {}
            Dim count As Integer = 0
            For i As Integer = 0 To 3
                ___paths(count) = upperTriangle.RotateZ(center, i * Math.PI / 2.0)
                count += 1
                ___paths(count) = lowerTriangle.RotateZ(center, i * Math.PI / 2.0)
                count += 1
            Next i
            paths = ___paths.AsList
            scalePath3Ds(center, Math.Sqrt(2) / 2.0, Math.Sqrt(2) / 2.0, 1)
        End Sub
    End Class

End Namespace