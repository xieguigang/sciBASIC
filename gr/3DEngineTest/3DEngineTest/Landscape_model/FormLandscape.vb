Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Linq

Public Class FormLandscape

    Private Sub FormLandscape_Load(sender As Object, e As EventArgs) Handles Me.Load
        Call SaveDemo()

        Dim path$ = App.HOME & "/demo.xml"
        Dim model As Landscape.Graphics = path.LoadXml(Of Landscape.Graphics)
        Dim surfaces = model.Surfaces.ToArray(Function(s) s.createobject)
        Dim canvas As New GDIDevice With {
            .Painter = Sub(g, camera)
                           Call g.FillBg(model.bg, New Rectangle(New Point, camera.screen))
                           Call camera.Draw(g, surfaces)
                       End Sub,
            .Dock = DockStyle.Fill,
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
End Class