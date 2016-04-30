Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Imaging.Drawing3D

Public Class CubeTest : Inherits GDIDevice

    Protected m_angle As Integer

    Dim cubeModel As New Cube

    Protected Overrides Sub ___animationLoop()
        ' Update the variable after each frame.
        m_angle += 1
    End Sub

    Protected Overrides Sub __init()

    End Sub

    Protected Overrides Sub __updateGraphics(sender As Object, Gr As PaintEventArgs)
        ' Clear the window
        Gr.Graphics.Clear(Color.LightBlue)

        Dim view As ModelView = cubeModel.Rotate(m_angle, ClientSize, Aixs.All)
        Call view.UpdateGraphics(Gr.Graphics)
    End Sub
End Class
