Imports System
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.IsoMetric

Namespace Drawing3D.IsoMetric.Paths


    ''' <summary>
    ''' Created by fabianterhorst on 01.04.17.
    ''' </summary>

    Public Class Star
        Inherits Path3D

        Public Sub New(ByVal origin As Point3D, ByVal outerRadius As Double, ByVal innerRadius As Double, ByVal points As Integer)
            MyBase.New()
            Dim r As Double
            For i As Integer = 0 To points * 2 - 1
                r = If(i Mod 2 = 0, outerRadius, innerRadius)
                Call Push(New Point3D((r * Math.Cos(i * Math.PI / points)) + origin.X, (r * Math.Sin(i * Math.PI / points)) + origin.Y, origin.Z))
            Next
        End Sub
    End Class

End Namespace