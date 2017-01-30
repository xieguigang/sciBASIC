Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Linq

Public Class FormLandscape

    Dim canvas As GDIDevice

    Private Sub FormLandscape_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim surfaces = New Cube(10)

        canvas = New GDIDevice With {
            .Painter = Sub(g, camera)
                           Call g.Clear(Color.LightBlue)
                           Call surfaces.Draw(g, camera)
                       End Sub,
            .Dock = DockStyle.Fill,
            .AutoRotation = False,
            .Animation = Sub(_camera)
                             ' Update the variable after each frame.
                             _camera.angleX += 1
                             _camera.angleY += 1
                             _camera.angleZ += 1
                         End Sub
        }
        Controls.Add(canvas)
        canvas.Run()
    End Sub

    Private Sub LoadModelToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LoadModelToolStripMenuItem.Click
        Using file As New OpenFileDialog With {
            .Filter = "3D model(*.3mf)|*.3mf"
        }
            If file.ShowDialog = DialogResult.OK Then
                Dim project = Landscape.IO.Open(file.FileName)
                Dim surfaces = project.GetSurfaces

                canvas.Painter = Sub(g, camera)
                                     Call g.Clear(Color.White)
                                     Call camera.Draw(g, surfaces, True)
                                 End Sub
            End If
        End Using
    End Sub
End Class