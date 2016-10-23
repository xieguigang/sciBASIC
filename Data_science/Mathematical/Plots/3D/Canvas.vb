Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.Imaging.Drawing3D

Namespace Plot3D

    Public Class Canvas : Inherits GDIDevice

        Protected Overrides Sub __init()
            '  data = Evaluate(Function(x, y) x * y, New DoubleRange(-1, 10), New DoubleRange(-10, 10))
            models += New Surface With {.brush = Brushes.Red, .vertices = {New Point3D(50, 50, 50), New Point3D(10, 10, 0), New Point3D(100, 100, 100)}}
            models += New Shape2D With {.focus = New Point3D(50, 50, 50), .shape = New Circle(10, Color.Red)}
            models += New Shape2D With {.focus = New Point3D(10, 10, 0), .shape = New Circle(10, Color.Red)}
            models += New Shape2D With {.focus = New Point3D(100, 100, 100), .shape = New Circle(10, Color.Red)}

            Call DirectCast(models.First, Surface).Allocation()
            Call Run()
        End Sub

        Protected Overrides Sub ___animationLoop()

        End Sub
    End Class
End Namespace