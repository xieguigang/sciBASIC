#Region "Microsoft.VisualBasic::b373ab817fa0ae1e9ac6ac4051709a23, ..\sciBASIC#\gr\3DEngineTest\3DEngineTest\Landscape_model\FormLandscape.vb"

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

Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Device
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Landscape
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
    End Sub

    Private Sub LoadModelToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LoadModelToolStripMenuItem.Click
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

    Private Sub AutoRotateToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AutoRotateToolStripMenuItem.Click
        canvas.AutoRotation = AutoRotateToolStripMenuItem.Checked
    End Sub

    Private Sub ResetToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ResetToolStripMenuItem.Click
        Dim surfaces = New Cube(10)
        canvas.Model = Function() surfaces.faces
        canvas.LightIllumination = False
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

        model = model.ToArray(Function(s) New Surface With {.brush = color, .vertices = s.vertices})
        canvas.Model = Function() model
    End Sub

    Private Sub trbFOV_Scroll(sender As Object, e As EventArgs) Handles trbFOV.Scroll
        canvas.FOV = TrackBar1.Value
    End Sub
End Class
