Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts

Public Class InputDevice : Implements IDisposable

    Protected WithEvents Canvas As Canvas

    Sub New(canvas As Canvas)
        Me.Canvas = canvas
    End Sub

    Protected Overridable Sub Canvas_MouseMove(sender As Object, e As MouseEventArgs) Handles Canvas.MouseMove
        If Not drag Then   ' 设置tooltip
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

    Protected dragNode As Node

    Protected Overridable Function __getNode(p As Point) As Node
        For Each node As Node In Canvas.Graph.nodes
            Dim r As Single = node.Data.radius
            Dim v As FDGVector2 = TryCast(Canvas.fdgPhysics.GetPoint(node).position, FDGVector2)
            Dim npt As Point =
                Renderer.GraphToScreen(v, Canvas.fdgRenderer.ClientRegion)
            Dim pt As New Point(CInt(npt.X - r / 2), CInt(npt.Y - r / 2))
            Dim rect As New Rectangle(pt, New Size(CInt(r), CInt(r)))

            If rect.Contains(p) Then
                Return node
            End If
        Next

        Return Nothing
    End Function

    Protected drag As Boolean

    Protected Overridable Sub Canvas_MouseDown(sender As Object, e As MouseEventArgs) Handles Canvas.MouseDown
        drag = True
        dragNode = __getNode(e.Location)
    End Sub

    Protected Overridable Sub Canvas_MouseUp(sender As Object, e As MouseEventArgs) Handles Canvas.MouseUp
        drag = False
        If dragNode IsNot Nothing Then
            dragNode.Pinned = False
            dragNode = Nothing
        End If
    End Sub

    Protected Overridable Sub Canvas_MouseWheel(sender As Object, e As MouseEventArgs) Handles Canvas.MouseWheel

    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                Canvas = Nothing
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
