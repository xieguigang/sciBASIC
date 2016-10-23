Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.Imaging.Drawing3D

Namespace Plot3D

    Public Class Canvas : Inherits GDIDevice

        Protected Overrides Sub __init()

            'Dim a As New Point3D(0, 0, 0)
            'Dim b As New Point3D(10, 0, 0)
            'Dim c As New Point3D(10, 10, 0)
            'Dim d As New Point3D(0, 10, 0)

            ''  data = Evaluate(Function(x, y) x * y, New DoubleRange(-1, 10), New DoubleRange(-10, 10))
            'models += New Surface With {.brush = Brushes.Red, .vertices = {a, b, c, d}}
            'models += New Shape2D With {.focus = a, .shape = New Circle(10, Color.Red)}
            'models += New Shape2D With {.focus = b, .shape = New Circle(10, Color.Red)}
            'models += New Shape2D With {.focus = c, .shape = New Circle(10, Color.Red)}

            'models += New Drawing3D.Line3D With {.a = a, .b = c, .pen = Pens.Yellow}


            'a = New Point3D(0, 20, 10)
            'b = New Point3D(50, 50, 0)
            'c = New Point3D(0, 0, -10)

            'models += New Surface With {.brush = Brushes.Blue, .vertices = {a, b, c}}

            'Call DirectCast(models.Last, Surface).Allocation()
            'Call DirectCast(models.First, Surface).Allocation()

            Dim x As New DoubleRange(-5, 5)
            Dim y As New DoubleRange(-5, 5)

            models += Grid(Function(xx, yy) xx * yy, x, y, 0.5, 0.5, Pens.Yellow).Select(Function(l) DirectCast(l, I3DModel))
            '  models += Grid(Function(xx, yy) xx * yy, x, y, 0.05, 0.05, Pens.Green).Select(Function(l) DirectCast(l, I3DModel))

            models += New Line3D With {.a = New Point3D(30, 0, 0), .b = New Point3D(-30, 0, 0), .pen = Pens.Red}
            models += New Line3D With {.a = New Point3D(0, 30, 0), .b = New Point3D(0, -30, 0), .pen = Pens.Blue}
            models += New Line3D With {.a = New Point3D(0, 0, 30), .b = New Point3D(0, 0, -30), .pen = Pens.DarkViolet}

            Call Run()
        End Sub

        Protected Overrides Sub ___animationLoop()

        End Sub
    End Class
End Namespace