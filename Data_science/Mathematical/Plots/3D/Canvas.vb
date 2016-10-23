#Region "Microsoft.VisualBasic::92aa5bde6a412a4afe9d2d1ab35fc840, ..\visualbasic_App\Data_science\Mathematical\Plots\3D\Canvas.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

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

            ' Dim x As New DoubleRange(-5, 5)
            '  Dim y As New DoubleRange(-5, 5)

            ' models += Grid(Function(xx, yy) xx * yy, x, y, 0.5, 0.5, Pens.Yellow).Select(Function(l) DirectCast(l, I3DModel))
            '  models += Grid(Function(xx, yy) xx * yy, x, y, 0.05, 0.05, Pens.Green).Select(Function(l) DirectCast(l, I3DModel))

            models += New Line3D With {.a = New Point3D(30, 0, 0), .b = New Point3D(-30, 0, 0), .pen = Pens.Red}
            models += New Line3D With {.a = New Point3D(0, 30, 0), .b = New Point3D(0, -30, 0), .pen = Pens.Blue}
            models += New Line3D With {.a = New Point3D(0, 0, 30), .b = New Point3D(0, 0, -30), .pen = Pens.DarkViolet}
            models += New Cube(25)

            Call Run()
        End Sub

        Protected Overrides Sub ___animationLoop()

        End Sub
    End Class
End Namespace
