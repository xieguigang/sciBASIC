#Region "Microsoft.VisualBasic::d2e3264fc1af8895b07095bd7160d524, sciBASIC#\gr\3DEngineTest\3DEngineTest\Landscape_model\FormLandscape.vb"

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

    '   Total Lines: 180
    '    Code Lines: 146
    ' Comment Lines: 0
    '   Blank Lines: 34
    '     File Size: 8.90 KB


    ' Class FormLandscape
    ' 
    '     Sub: __isometricLoad, AutoRotateToolStripMenuItem_Click, FormLandscape_Load, IsometricComplexExampleToolStripMenuItem_Click, IsometricGridToolStripMenuItem_Click
    '          IsometricKnotToolStripMenuItem_Click, IsometricPieToolStripMenuItem_Click, LightToolStripMenuItem_Click, Load3mfToolStripMenuItem_Click, RemoveTexturesToolStripMenuItem_Click
    '          ResetToolStripMenuItem_Click, ResetToolStripMenuItem1_Click, ResetToolStripMenuItem2_Click, RotateXToolStripMenuItem_Click, RotateYToolStripMenuItem_Click
    '          RotateZToolStripMenuItem_Click, SetBackgroundColorToolStripMenuItem_Click, SetLightColorToolStripMenuItem_Click, TrackBar1_Scroll, trbFOV_Scroll
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.ChartPlots.Drawing3D.Device
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Device
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Landscape
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Landscape.Vendor_3mf
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Public Class FormLandscape

    Dim canvas As GDIDevice

    Private Sub FormLandscape_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim surfaces = New Cube(10)

        canvas = New GDIDevice With {
            .bg = Color.LightBlue,
            .Model = Function() surfaces.faces,
            .Dock = DockStyle.Fill,
            .AutoRotation = True,
            .ShowDebugger = True
        }
        Controls.Add(canvas)
        canvas.Run()
        canvas.RefreshInterval = 1
    End Sub

    Private Sub AutoRotateToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AutoRotateToolStripMenuItem.Click
        canvas.AutoRotation = AutoRotateToolStripMenuItem.Checked
    End Sub

    Private Sub ResetToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ResetToolStripMenuItem.Click
        Dim surfaces = New Cube(10)
        canvas.Model = Function() surfaces.faces
        canvas.LightIllumination = False
        canvas.FOV = 256
        canvas.ViewDistance = 0
        canvas.LightColor = Color.White
        canvas.DrawPath = False
        canvas.bg = Color.LightBlue
    End Sub

    Private Sub TrackBar1_Scroll(sender As Object, e As EventArgs) Handles TrackBar1.Scroll
        canvas.ViewDistance = TrackBar1.Value
    End Sub

    Private Sub RotateXToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RotateXToolStripMenuItem.Click
        canvas.RotationThread.X = If(RotateXToolStripMenuItem.Checked, 1, 0)
    End Sub

    Private Sub RotateYToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RotateYToolStripMenuItem.Click
        canvas.RotationThread.Y = If(RotateYToolStripMenuItem.Checked, 1, 0)
    End Sub

    Private Sub RotateZToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RotateZToolStripMenuItem.Click
        canvas.RotationThread.Z = If(RotateZToolStripMenuItem.Checked, 1, 0)
    End Sub

    Private Sub ResetToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles ResetToolStripMenuItem1.Click
        Call canvas.RotationThread.Reset
    End Sub

    Private Sub LightToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LightToolStripMenuItem.Click
        canvas.LightIllumination = LightToolStripMenuItem.Checked
    End Sub

    Private Sub RemoveTexturesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RemoveTexturesToolStripMenuItem.Click
        Dim model As Surface() = canvas.Model()()
        Dim color As SolidBrush = Brushes.Blue

        model = model _
            .Select(Function(s)
                        Return New Surface With {.brush = color, .vertices = s.vertices}
                    End Function) _
            .ToArray
        canvas.Model = Function() model
    End Sub

    Private Sub trbFOV_Scroll(sender As Object, e As EventArgs) Handles trbFOV.Scroll
        canvas.FOV = TrackBar1.Value
    End Sub

    Private Sub IsometricComplexExampleToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles IsometricComplexExampleToolStripMenuItem.Click
        Dim isometricView As New List(Of Surface)

        isometricView += New Shapes.Prism(New Point3D(1, -1, 0), 4, 5, 2).Model3D(Color.FromArgb(33, 150, 243))
        isometricView += New Shapes.Prism(New Point3D(0, 0, 0), 1, 4, 1).Model3D(Color.FromArgb(33, 150, 243))
        isometricView += New Shapes.Prism(New Point3D(-1, 1, 0), 1, 3, 1).Model3D(Color.FromArgb(33, 150, 243))
        isometricView += New Shapes.Stairs(New Point3D(-1, 0, 0), 10).Model3D(Color.FromArgb(33, 150, 243))
        isometricView += (New Shapes.Stairs(New Point3D(0, 3, 1), 10)).RotateZ(New Point3D(0.5, 3.5, 1), -Math.PI / 2).Model3D(Color.FromArgb(33, 150, 243))
        isometricView += New Shapes.Prism(New Point3D(3, 0, 2), 2, 4, 1).Model3D(Color.FromArgb(33, 150, 243))
        isometricView += New Shapes.Prism(New Point3D(2, 1, 2), 1, 3, 1).Model3D(Color.FromArgb(33, 150, 243))
        isometricView += (New Shapes.Stairs(New Point3D(2, 0, 2), 10)).RotateZ(New Point3D(2.5, 0.5, 0), -Math.PI / 2).Model3D(Color.FromArgb(33, 150, 243))
        isometricView += (New Shapes.Pyramid(New Point3D(2, 3, 3))).Scale(New Point3D(2, 4, 3), 0.5).Model3D(Color.FromArgb(180, 180, 0))
        isometricView += (New Shapes.Pyramid(New Point3D(4, 3, 3))).Scale(New Point3D(5, 4, 3), 0.5).Model3D(Color.FromArgb(180, 0, 180))
        isometricView += (New Shapes.Pyramid(New Point3D(4, 1, 3))).Scale(New Point3D(5, 1, 3), 0.5).Model3D(Color.FromArgb(0, 180, 180))
        isometricView += (New Shapes.Pyramid(New Point3D(2, 1, 3))).Scale(New Point3D(2, 1, 3), 0.5).Model3D(Color.FromArgb(40, 180, 40))
        isometricView += New Shapes.Prism(New Point3D(3, 2, 3), 1, 1, 0.2).Model3D(Color.FromArgb(50, 50, 50))
        isometricView += (New Shapes.Octahedron(New Point3D(3, 2, 3.2))).RotateZ(New Point3D(3.5, 2.5, 0), angle:=0).Model3D(Color.FromArgb(0, 180, 180))

        Call __isometricLoad(isometricView)
    End Sub

    Private Sub Load3mfToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles Load3mfToolStripMenuItem.Click
        Using file As New OpenFileDialog With {
            .Filter = "3D model(*.3mf)|*.3mf"
        }
            If file.ShowDialog = DialogResult.OK Then
                Dim project As Vendor_3mf.Project = Vendor_3mf.IO.Open(file.FileName)
                Dim surfaces As Surface() = project.GetSurfaces(True)

                canvas.LightIllumination = True
                canvas.Model = Function() surfaces
            End If
        End Using
    End Sub

    Private Sub __isometricLoad(modelData As IEnumerable(Of Surface))
        Dim models = modelData.Centra.Offsets(modelData).ToArray

        canvas.bg = Color.White

        canvas.LightIllumination = True
        canvas.DrawPath = False
        canvas.Model = Function() models
    End Sub

    Private Sub IsometricKnotToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles IsometricKnotToolStripMenuItem.Click
        Call __isometricLoad(New Shapes.Knot(New Point3D(1, 1, 1)).Model3D(Color.Green))
    End Sub

    Private Sub IsometricGridToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles IsometricGridToolStripMenuItem.Click
        Dim isometricView As New List(Of Surface)

        For x As Integer = 0 To 9
            isometricView += New Path3D({New Point3D(x, 0, 0), New Point3D(x, 10, 0), New Point3D(x, 0, 0)}).Model3D(Color.FromArgb(50, 160, 60))
        Next
        For y As Integer = 0 To 9
            isometricView += New Path3D({New Point3D(0, y, 0), New Point3D(10, y, 0), New Point3D(0, y, 0)}).Model3D(Color.FromArgb(50, 160, 60))
        Next
        isometricView += New Shapes.Prism(Math3D.ORIGIN).Model3D(Color.FromArgb(33, 150, 243))
        isometricView += New Path3D({Math3D.ORIGIN, New Point3D(0, 0, 10), Math3D.ORIGIN}).Model3D(Color.FromArgb(160, 50, 60))

        Call __isometricLoad(isometricView)

        canvas.DrawPath = True
    End Sub

    Private Sub SetLightColorToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SetLightColorToolStripMenuItem.Click
        Call New Form1 With {
            .setColor = Sub(c)
                            canvas.LightColor = c
                        End Sub
        }.ShowDialog()
    End Sub

    Private Sub ResetToolStripMenuItem2_Click(sender As Object, e As EventArgs) Handles ResetToolStripMenuItem2.Click
        canvas.LightColor = Color.White
    End Sub

    Private Sub SetBackgroundColorToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SetBackgroundColorToolStripMenuItem.Click
        Call New Form1 With {
            .setColor = Sub(c)
                            canvas.bg = c
                        End Sub
        }.ShowDialog()
    End Sub

    Private Sub IsometricPieToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles IsometricPieToolStripMenuItem.Click
        Dim view As New List(Of Surface)

        view += New Shapes.Pie(Math3D.ORIGIN, 10, 0, 280, 80, 4).Model3D(Color.Yellow)
        view += New Shapes.Cylinder(Math3D.ORIGIN - New Point3D(-4, 4, 0), 20, 4).Model3D(Color.Green)

        Call __isometricLoad(view)
    End Sub
End Class
