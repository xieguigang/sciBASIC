Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging.Drawing3D

Namespace Plot3D

    Public Class Canvas : Inherits GDIDevice

        Protected Overrides Sub __init()
            '  data = Evaluate(Function(x, y) x * y, New DoubleRange(-1, 10), New DoubleRange(-10, 10))
            models += New Surface With {.brush = Brushes.Red, .vertices = {New Point3D(10, 10, 10), New Point3D(0, 0, 0), New Point3D(2, 2, 2)}}
            Call DirectCast(models.First, Surface).Allocation()
            Call Run()
        End Sub

        Protected Overrides Sub ___animationLoop()

        End Sub
    End Class
End Namespace