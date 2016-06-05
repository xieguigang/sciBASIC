Imports Microsoft.VisualBasic.DataVisualization.Network.Graph

Public Class InputDevice

    Dim WithEvents Canvas As Canvas

    Sub New(canvas As Canvas)
        Me.Canvas = canvas
    End Sub

    Private Sub Canvas_MouseMove(sender As Object, e As MouseEventArgs) Handles Canvas.MouseMove
        Dim node As Node = __getNode(e.Location)

        If node Is Nothing Then
            Return
        End If

        If drag Then
            node.Data.initialPostion.Point2D = New Point With {
                .X = node.Data.initialPostion.x - userCursor.X + e.X,
                .Y = node.Data.initialPostion.y - userCursor.Y + e.Y
            }
        End If
    End Sub

    Private Function __getNode(p As Point) As Node
        For Each node As Node In Canvas.Graph.nodes
            If node.Data.Intersect(p) Then
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
    End Sub
End Class
