Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports NetworkEditor.Models

Namespace NetworkEditor.Controls

    ''' <summary>
    ''' 导航地图：绘制全图缩略 + 当前视口矩形，点击/拖拽可移动主视图中心
    ''' </summary>
    Public Class MinimapControl : Inherits UserControl

        Private _canvas As NetworkEditorCanvas = Nothing

        Public Sub New()
            Me.DoubleBuffered = True
            Me.BackColor = Color.FromArgb(&HFF262D3A)
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw, True)
        End Sub

        Public Property Canvas As NetworkEditorCanvas
            Get
                Return _canvas
            End Get
            Set(value As NetworkEditorCanvas)
                If _canvas IsNot Nothing Then
                    RemoveHandler _canvas.ViewChanged, AddressOf RefreshMap
                End If
                _canvas = value
                If _canvas IsNot Nothing Then
                    AddHandler _canvas.ViewChanged, AddressOf RefreshMap
                End If
                Me.Invalidate()
            End Set
        End Property

        Private Sub RefreshMap(sender As Object, e As EventArgs)
            Me.Invalidate()
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            Dim g = e.Graphics
            g.SmoothingMode = SmoothingMode.HighQuality
            g.Clear(Color.FromArgb(&HFF262D3A))

            If _canvas Is Nothing OrElse _canvas.State Is Nothing Then
                Return
            End If

            Dim world = _canvas.GetWorldBounds()
            If world.IsEmpty Then
                Return
            End If

            Dim pad As Single = 6
            Dim scale As Single, offX As Single, offY As Single
            ComputeTransform(world, scale, offX, offY, pad)

            For Each n As Node In _canvas.State.Graph.vertex
                If n.data.initialPostion Is Nothing Then
                    Continue For
                End If
                Dim sx = offX + (n.data.initialPostion.x - world.X) * scale
                Dim sy = offY + (n.data.initialPostion.y - world.Y) * scale
                Using b = New SolidBrush(_canvas.State.GetNodeColor(n))
                    g.FillRectangle(b, sx - 1, sy - 1, 2, 2)
                End Using
            Next

            Dim view = _canvas.GetViewRectGraph()
            Dim rx = offX + (view.X - world.X) * scale
            Dim ry = offY + (view.Y - world.Y) * scale
            Dim rw = view.Width * scale
            Dim rh = view.Height * scale
            Using pen = New Pen(Color.FromArgb(&HFFFFB454), 1.5F)
                g.DrawRectangle(pen, rx, ry, rw, rh)
            End Using

            Using pen = New Pen(Color.FromArgb(&HFF4A90D9), 1)
                g.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1)
            End Using
        End Sub

        Private Sub ComputeTransform(world As RectangleF, ByRef scale As Single, ByRef offX As Single, ByRef offY As Single, pad As Single)
            Dim aw = ClientSize.Width - pad * 2
            Dim ah = ClientSize.Height - pad * 2
            scale = Math.Min(aw / Math.Max(1.0F, world.Width), ah / Math.Max(1.0F, world.Height))
            If scale <= 0 Then
                scale = 1
            End If
            offX = pad + (aw - world.Width * scale) / 2.0F
            offY = pad + (ah - world.Height * scale) / 2.0F
        End Sub

        Private Function ToGraph(pt As PointF, ByRef gx As Double, ByRef gy As Double) As Boolean
            Dim world = _canvas.GetWorldBounds()
            If world.IsEmpty Then
                Return False
            End If
            Dim pad As Single = 6
            Dim scale As Single, offX As Single, offY As Single
            ComputeTransform(world, scale, offX, offY, pad)
            gx = (pt.X - offX) / scale + world.X
            gy = (pt.Y - offY) / scale + world.Y
            Return True
        End Function

        Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
            MyBase.OnMouseDown(e)
            If _canvas Is Nothing Then
                Return
            End If
            Dim gx, gy As Double
            If ToGraph(e.Location, gx, gy) Then
                _canvas.CenterOnGraph(gx, gy)
            End If
        End Sub

        Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
            MyBase.OnMouseMove(e)
            If _canvas Is Nothing Then
                Return
            End If
            If e.Button = MouseButtons.Left Then
                Dim gx, gy As Double
                If ToGraph(e.Location, gx, gy) Then
                    _canvas.CenterOnGraph(gx, gy)
                End If
            End If
        End Sub
    End Class

End Namespace
