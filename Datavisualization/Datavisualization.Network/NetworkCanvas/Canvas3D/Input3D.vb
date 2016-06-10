Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts
Imports Microsoft.VisualBasic.Imaging.Drawing3D

''' <summary>
''' Mouse input device in 3D space
''' </summary>
Public Class Input3D : Inherits InputDevice

    Sub New(canvas As Canvas)
        Call MyBase.New(canvas)
        renderer = DirectCast(canvas.fdgRenderer, Renderer3D)
    End Sub

    Dim renderer As Renderer3D
    Dim usrCursor, location As Point

    Protected Overrides Sub Canvas_MouseDown(sender As Object, e As MouseEventArgs)
        drag = True
        usrCursor = e.Location
    End Sub

    Protected Overrides Sub Canvas_MouseMove(sender As Object, e As MouseEventArgs)
        If Not drag Then
            Return
        End If

        renderer.rotate = renderer.rotate + (-usrCursor.X + e.X) / 1000
    End Sub

    Protected Overrides Sub Canvas_MouseUp(sender As Object, e As MouseEventArgs)
        drag = False
    End Sub

    Dim vd As Integer

    '    Protected Overrides Sub Canvas_MouseWheel(sender As Object, e As MouseEventArgs)
    '        vd += e.Delta / 200

    '        Dim rect = renderer.ClientRegion

    '        For Each node In Canvas.Graph.nodes
    '            Dim pos As AbstractVector = Canvas.fdgPhysics.GetPoint(node).position
    '            Call Point3D.Project(pos.x, pos.y, pos.z, rect.Width, rect.Height, 1, vd)
    '#If DEBUG Then
    '            Call pos.__DEBUG_ECHO
    '#End If
    '        Next
    '    End Sub
End Class
