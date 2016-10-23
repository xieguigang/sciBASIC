Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.Imaging.Drawing3D

Namespace Plot3D

    Public Class Canvas : Inherits GDIDevice

        Protected Overrides Sub __init()

            Dim a As New Point3D(0, 0, 0)
            Dim b As New Point3D(1, 0, 0)
            Dim c As New Point3D(1, 1, 0)
            Dim d As New Point3D(0, 1, 0)

            '  data = Evaluate(Function(x, y) x * y, New DoubleRange(-1, 10), New DoubleRange(-10, 10))
            models += New Surface With {.brush = Brushes.Red, .vertices = {a, b, c, d}}
            models += New Shape2D With {.focus = a, .shape = New Circle(10, Color.Red)}
            models += New Shape2D With {.focus = b, .shape = New Circle(10, Color.Red)}
            models += New Shape2D With {.focus = c, .shape = New Circle(10, Color.Red)}

            Call DirectCast(models.First, Surface).Allocation()
            Call Run()
        End Sub

        Protected Overrides Sub ___animationLoop()

        End Sub
    End Class
End Namespace