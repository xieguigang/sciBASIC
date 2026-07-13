''' <summary>
''' 双缓冲画布面板，重写 OnMouseWheel 以消费滚轮事件（用于缩放），并对外抛出 Zoom 事件。
''' </summary>
Public Class RenderPanel : Inherits Panel

    Public Event Zoom(delta As Integer)

    Public Sub New()
        Me.DoubleBuffered = True
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        RaiseEvent Zoom(e.Delta)
    End Sub
End Class
