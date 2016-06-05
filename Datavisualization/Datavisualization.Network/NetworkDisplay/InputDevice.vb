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

        If dragNode IsNot Nothing Then
            Dim vec As FDGVector2 =
                Canvas.fdgRenderer.ScreenToGraph(
                New Point(e.Location.X, e.Location.Y))

            dragNode.Pinned = True
            Canvas.fdgPhysics.GetPoint(dragNode).position = vec
        Else
            dragNode = __getNode(e.Location)
        End If
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

    Private Sub Canvas_MouseDown(sender As Object, e As MouseEventArgs) Handles Canvas.MouseDown
        drag = True
        dragNode = __getNode(e.Location)
    End Sub

    Private Sub Canvas_MouseUp(sender As Object, e As MouseEventArgs) Handles Canvas.MouseUp
        drag = False
        If dragNode IsNot Nothing Then
            dragNode.Pinned = False
            dragNode = Nothing
        End If
    End Sub
End Class
