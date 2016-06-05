Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts

Public Class InputDevice

    Dim WithEvents Canvas As Canvas

    Sub New(canvas As Canvas)
        Me.Canvas = canvas
    End Sub

    Private Sub Canvas_MouseMove(sender As Object, e As MouseEventArgs) Handles Canvas.MouseMove
        If Not drag Then
            Return
        End If

        If dragNode Is Nothing Then
            dragNode = __getNode(e.Location)
        End If
        If dragNode Is Nothing Then
            Return
        Else
            dragNode.Pinned = True
        End If

        Dim npt As AbstractVector =
            Canvas.fdgPhysics.GetPoint(dragNode).position
        Dim pt As Point =
            Canvas.fdgRenderer.GraphToScreen(npt)

        pt.X = pt.X - userCursor.X + e.X
        pt.Y = pt.Y - userCursor.Y + e.Y

        Dim w = Canvas.fdgRenderer.ScreenToGraph(pt)

        npt.x = w.x
        npt.y = w.y
    End Sub

    Dim dragNode As Node

    Private Function __getNode(p As Point) As Node
        For Each node As Node In Canvas.Graph.nodes
            Dim r As Single = node.Data.radius
            Dim npt As Point =
                Canvas.fdgRenderer.GraphToScreen(
                Canvas.fdgPhysics.GetPoint(node).position)
            Dim pt As New Point(npt.X - r / 2, npt.Y - r / 2)
            Dim rect As New Rectangle(pt, New Size(r, r))

            If rect.Contains(p) Then
                Return node
            End If
        Next

        Return Nothing
    End Function

    Dim drag As Boolean
    Dim userCursor As Point

    Private Sub Canvas_MouseDown(sender As Object, e As MouseEventArgs) Handles Canvas.MouseDown
        drag = True
        userCursor = e.Location
    End Sub

    Private Sub Canvas_MouseUp(sender As Object, e As MouseEventArgs) Handles Canvas.MouseUp
        drag = False
        If Not dragNode Is Nothing Then
            dragNode.Pinned = False
            dragNode = Nothing
        End If
    End Sub
End Class
