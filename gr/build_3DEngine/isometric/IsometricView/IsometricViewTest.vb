#Region "Microsoft.VisualBasic::da3009a2466666d1a7e637d306b8b9ec, sciBASIC#\gr\build_3DEngine\isometric\IsometricView\IsometricViewTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 223
    '    Code Lines: 188
    ' Comment Lines: 1
    '   Blank Lines: 34
    '     File Size: 10.18 KB


    ' Class IsometricViewTest
    ' 
    '     Sub: cylinder, doScreenshotCylinder, doScreenshotExtrude, doScreenshotGrid, doScreenshotKnot
    '          doScreenshotOctahedron, doScreenshotOne, doScreenshotPath3D, doScreenshotPrism, doScreenshotPyramid
    '          doScreenshotRotateZ, doScreenshotScale, doScreenshotStairs, doScreenshotThree, doScreenshotTranslate
    '          doScreenshotTwo, extrude, grid, knot, measureAndScreenshotView
    '          octahedron, path, prism, pyramid, rotateZ
    '          sampleOne, sampleThree, sampleTwo, scale, stairs
    '          translate
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric
Imports IsometricView = Microsoft.VisualBasic.Imaging.Drawing3D.IsometricEngine

Public Class IsometricViewTest

    Private Shared ReadOnly BLUE As Color = Color.FromArgb(50, 60, 160)
    Private Shared ReadOnly GREEN As Color = Color.FromArgb(50, 160, 60)
    Private Shared ReadOnly RED As Color = Color.FromArgb(160, 60, 50)
    Private Shared ReadOnly TEAL As Color = Color.FromArgb(0, 180, 180)
    Private Shared ReadOnly YELLOW As Color = Color.FromArgb(180, 180, 0)
    Private Shared ReadOnly LIGHT_GREEN As Color = Color.FromArgb(40, 180, 40)
    Private Shared ReadOnly PURPLE As Color = Color.FromArgb(180, 0, 180)

    Private Sub measureAndScreenshotView(view As IsometricView,
                                         width%,
                                         height%,
                                         <CallerMemberName> Optional name$ = Nothing)

        Using g As Graphics2D = New Size(width, height).CreateGDIDevice
            Call view.Draw(g)
            Call g.ImageResource.SaveAs($"./{name}.png")
        End Using
    End Sub

    Public Overridable Sub doScreenshotOne()
        Dim view As New IsometricView
        sampleOne(view)
        measureAndScreenshotView(view, 680, 220)
    End Sub

    Public Overridable Sub doScreenshotTwo()
        Dim view As New IsometricView
        sampleTwo(view)
        measureAndScreenshotView(view, 680, 540)
    End Sub

    Public Overridable Sub doScreenshotThree()
        Dim view As New IsometricView
        sampleThree(0, view)
        measureAndScreenshotView(view, 820, 680)
    End Sub

    Public Overridable Sub doScreenshotGrid()
        Dim view As New IsometricView
        grid(view)
        measureAndScreenshotView(view, 680, 540)
    End Sub

    Public Overridable Sub doScreenshotPath3D()
        Dim view As New IsometricView
        path(view)
        measureAndScreenshotView(view, 680, 440)
    End Sub

    Public Overridable Sub doScreenshotTranslate()
        Dim view As New IsometricView
        translate(view)
        measureAndScreenshotView(view, 680, 440)
    End Sub

    Public Overridable Sub doScreenshotScale()
        Dim view As New IsometricView
        scale(view)
        measureAndScreenshotView(view, 680, 440)
    End Sub

    Public Overridable Sub doScreenshotRotateZ()
        Dim view As New IsometricView
        rotateZ(view)
        measureAndScreenshotView(view, 680, 440)
    End Sub

    Public Overridable Sub doScreenshotExtrude()
        Dim view As New IsometricView
        extrude(view)
        measureAndScreenshotView(view, 680, 440)
    End Sub

    Public Overridable Sub doScreenshotCylinder()
        Dim view As New IsometricView
        cylinder(view)
        measureAndScreenshotView(view, 680, 440)
    End Sub

    Public Overridable Sub doScreenshotKnot()
        Dim view As New IsometricView
        knot(view)
        measureAndScreenshotView(view, 680, 440)
    End Sub

    Public Overridable Sub doScreenshotOctahedron()
        Dim view As New IsometricView
        octahedron(view)
        measureAndScreenshotView(view, 680, 440)
    End Sub

    Public Overridable Sub doScreenshotPrism()
        Dim view As New IsometricView
        prism(view)
        measureAndScreenshotView(view, 680, 440)
    End Sub

    Public Overridable Sub doScreenshotPyramid()
        Dim view As New IsometricView
        pyramid(view)
        measureAndScreenshotView(view, 680, 440)
    End Sub

    Public Overridable Sub doScreenshotStairs()
        Dim view As New IsometricView
        stairs(view)
        measureAndScreenshotView(view, 680, 440)
    End Sub

    Public Overridable Sub grid(isometricView As IsometricEngine)
        For x As Integer = 0 To 9
            isometricView.Add(
                New Path3D({New Point3D(x, 0, 0), New Point3D(x, 10, 0), New Point3D(x, 0, 0)}),
                Color.FromArgb(50, 160, 60))
        Next
        For y As Integer = 0 To 9
            isometricView.Add(
                New Path3D({New Point3D(0, y, 0), New Point3D(10, y, 0), New Point3D(0, y, 0)}),
                Color.FromArgb(50, 160, 60))
        Next
        isometricView.Add(New Shapes.Prism(Math3D.ORIGIN), Color.FromArgb(33, 150, 243))
        isometricView.Add(New Path3D({Math3D.ORIGIN, New Point3D(0, 0, 10), Math3D.ORIGIN}), Color.FromArgb(160, 50, 60))
    End Sub

    Public Overridable Sub path(isometricView As IsometricEngine)
        isometricView.Add(New Shapes.Prism(Math3D.ORIGIN, 3, 3, 1), Color.FromArgb(50, 60, 160))
        isometricView.Add(New Path3D({New Point3D(1, 1, 1), New Point3D(2, 1, 1), New Point3D(2, 2, 1), New Point3D(1, 2, 1)}), Color.FromArgb(50, 160, 60))
    End Sub

    Public Overridable Sub sampleOne(isometricView As IsometricEngine)
        isometricView.Add(New Shapes.Prism(New Point3D(0, 0, 0)), Color.FromArgb(33, 150, 243))
    End Sub

    Public Overridable Sub sampleTwo(isometricView As IsometricEngine)
        isometricView.Add(New Shapes.Prism(New Point3D(0, 0, 0), 4, 4, 2), GREEN)
        isometricView.Add(New Shapes.Prism(New Point3D(-1, 1, 0), 1, 2, 1), PURPLE)
        isometricView.Add(New Shapes.Prism(New Point3D(1, -1, 0), 2, 1, 1), Color.FromArgb(33, 150, 243))
    End Sub

    Public Overridable Sub sampleThree(angle As Double, isometricView As IsometricEngine)
        isometricView.Clear()
        isometricView.Add(New Shapes.Prism(New Point3D(1, -1, 0), 4, 5, 2), Color.FromArgb(33, 150, 243))
        isometricView.Add(New Shapes.Prism(New Point3D(0, 0, 0), 1, 4, 1), Color.FromArgb(33, 150, 243))
        isometricView.Add(New Shapes.Prism(New Point3D(-1, 1, 0), 1, 3, 1), Color.FromArgb(33, 150, 243))
        isometricView.Add(New Shapes.Stairs(New Point3D(-1, 0, 0), 10), Color.FromArgb(33, 150, 243))
        isometricView.Add((New Shapes.Stairs(New Point3D(0, 3, 1), 10)).RotateZ(New Point3D(0.5, 3.5, 1), -Math.PI / 2), Color.FromArgb(33, 150, 243))
        isometricView.Add(New Shapes.Prism(New Point3D(3, 0, 2), 2, 4, 1), Color.FromArgb(33, 150, 243))
        isometricView.Add(New Shapes.Prism(New Point3D(2, 1, 2), 1, 3, 1), Color.FromArgb(33, 150, 243))
        isometricView.Add((New Shapes.Stairs(New Point3D(2, 0, 2), 10)).RotateZ(New Point3D(2.5, 0.5, 0), -Math.PI / 2), Color.FromArgb(33, 150, 243))
        isometricView.Add((New Shapes.Pyramid(New Point3D(2, 3, 3))).Scale(New Point3D(2, 4, 3), 0.5), Color.FromArgb(180, 180, 0))
        isometricView.Add((New Shapes.Pyramid(New Point3D(4, 3, 3))).Scale(New Point3D(5, 4, 3), 0.5), Color.FromArgb(180, 0, 180))
        isometricView.Add((New Shapes.Pyramid(New Point3D(4, 1, 3))).Scale(New Point3D(5, 1, 3), 0.5), Color.FromArgb(0, 180, 180))
        isometricView.Add((New Shapes.Pyramid(New Point3D(2, 1, 3))).Scale(New Point3D(2, 1, 3), 0.5), Color.FromArgb(40, 180, 40))
        isometricView.Add(New Shapes.Prism(New Point3D(3, 2, 3), 1, 1, 0.2), Color.FromArgb(50, 50, 50))
        isometricView.Add((New Shapes.Octahedron(New Point3D(3, 2, 3.2))).RotateZ(New Point3D(3.5, 2.5, 0), angle), Color.FromArgb(0, 180, 180))
    End Sub

    Public Overridable Sub translate(isometricView As IsometricEngine)
        Dim ___blue As Color = Color.FromArgb(50, 60, 160)
        Dim ___red As Color = Color.FromArgb(160, 60, 50)
        Dim cube As New Shapes.Prism(New Point3D(0, 0, 0))
        isometricView.Add(cube, ___red)
        isometricView.Add(cube.Translate(0, 0, 1.1), ___blue)
        isometricView.Add(cube.Translate(0, 0, 2.2), ___red)
    End Sub

    Public Overridable Sub scale(isometricView As IsometricEngine)
        Dim ___blue As Color = Color.FromArgb(50, 60, 160)
        Dim ___red As Color = Color.FromArgb(160, 60, 50)
        Dim cube As New Shapes.Prism(Math3D.ORIGIN)
        isometricView.Add(cube.Scale(Math3D.ORIGIN, 3.0, 3.0, 0.5), ___red)
        isometricView.Add(cube.Scale(Math3D.ORIGIN, 3.0, 3.0, 0.5).Translate(0, 0, 0.6), ___blue)
    End Sub

    Public Overridable Sub rotateZ(isometricView As IsometricEngine)
        Dim ___blue As Color = Color.FromArgb(50, 60, 160)
        Dim ___red As Color = Color.FromArgb(160, 60, 50)
        Dim cube As New Shapes.Prism(Math3D.ORIGIN, 3, 3, 1)
        isometricView.Add(cube, ___red)
        isometricView.Add(cube.RotateZ(New Point3D(1.5, 1.5, 0), Math.PI / 12).Translate(0, 0, 1.1), ___blue)
        ' (1.5, 1.5) is the center of the prism 
    End Sub

    Public Overridable Sub extrude(isometricView As IsometricEngine)
        Dim ___blue As Color = Color.FromArgb(50, 60, 160)
        Dim ___red As Color = Color.FromArgb(160, 60, 50)
        isometricView.Add(New Shapes.Prism(Math3D.ORIGIN, 3, 3, 1), ___blue)
        isometricView.Add(Shape3D.Extrude(New Path3D(New Point3D() {New Point3D(1, 1, 1), New Point3D(2, 1, 1), New Point3D(2, 3, 1)}), 0.3), ___red)
    End Sub

    Public Overridable Sub cylinder(isometricView As IsometricEngine)
        isometricView.Add(New Shapes.Cylinder(New Point3D(1, 1, 1), 30, 1), BLUE)
    End Sub

    Public Overridable Sub knot(isometricView As IsometricView)
        isometricView.Add(New Shapes.Knot(New Point3D(1, 1, 1)), GREEN)
    End Sub

    Public Overridable Sub octahedron(isometricView As IsometricView)
        isometricView.Add(New Shapes.Octahedron(New Point3D(1, 1, 1)), RED)
    End Sub

    Public Overridable Sub prism(isometricView As IsometricView)
        isometricView.Add(New Shapes.Prism(New Point3D(1, 1, 1)), YELLOW)
    End Sub

    Public Overridable Sub pyramid(isometricView As IsometricView)
        isometricView.Add(New Shapes.Pyramid(New Point3D(1, 1, 1)), TEAL)
    End Sub

    Public Overridable Sub stairs(isometricView As IsometricView)
        isometricView.Add(New Shapes.Stairs(New Point3D(1, 1, 1), 10), LIGHT_GREEN)
    End Sub
End Class
