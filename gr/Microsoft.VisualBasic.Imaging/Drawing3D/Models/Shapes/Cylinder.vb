Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.IsoMetric

Namespace Drawing3D.IsoMetric.Shapes


    ''' <summary>
    ''' Created by fabianterhorst on 01.04.17.
    ''' </summary>

    Public Class Cylinder
        Inherits Shape3D

        Public Sub New(origin As Point3D, vertices As Double, height As Double)
            Me.New(origin, 1, vertices, height)
        End Sub

        Public Sub New(origin As Point3D, radius As Double, vertices As Double, height As Double)
            MyBase.New()
            Dim circle As New Paths.Circle(origin, radius, vertices)
            extrude(Me, circle, height)
        End Sub
    End Class

End Namespace