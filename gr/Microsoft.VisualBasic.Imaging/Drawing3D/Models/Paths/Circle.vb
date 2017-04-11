Imports System
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.IsoMetric

Namespace Drawing3D.IsoMetric.Paths


    ''' <summary>
    ''' Created by fabianterhorst on 01.04.17.
    ''' </summary>

    Public Class Circle
        Inherits Path3D

        <Obsolete>
        Public Sub New(ByVal origin As Point3D, ByVal radius As Double)
            Me.New(origin, radius, 20)
        End Sub

        Public Sub New(ByVal origin As Point3D, ByVal radius As Double, ByVal vertices As Double)
            MyBase.New()
            For i As Integer = 0 To vertices - 1
                Call Push(New Point3D((radius * Math.Cos(i * 2 * Math.PI / vertices)) + origin.X, (radius * Math.Sin(i * 2 * Math.PI / vertices)) + origin.Y, origin.Z))
            Next
        End Sub
    End Class

End Namespace