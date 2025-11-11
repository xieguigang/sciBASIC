Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Scripting.Expressions

Public Class PrincipalCurveVisualizer
    Private _dataPoints As List(Of DataPoint)
    Private _curvePoints As List(Of DataPoint)
    Private _bounds As RectangleF

    Public Sub New(dataPoints As List(Of DataPoint), curvePoints As List(Of DataPoint))
        Me._dataPoints = dataPoints
        Me._curvePoints = curvePoints
        CalculateBounds()
    End Sub

    Private Sub CalculateBounds()
        If _dataPoints.Count = 0 Then
            _bounds = New RectangleF(0, 0, 100, 100)
            Return
        End If

        Dim minX = _dataPoints.Min(Function(p) p.X)
        Dim maxX = _dataPoints.Max(Function(p) p.X)
        Dim minY = _dataPoints.Min(Function(p) p.Y)
        Dim maxY = _dataPoints.Max(Function(p) p.Y)

        ' 添加一些边距
        Dim marginX = (maxX - minX) * 0.1
        Dim marginY = (maxY - minY) * 0.1

        _bounds = New RectangleF(CSng(minX - marginX), CSng(minY - marginY),
                                CSng(maxX - minX + 2 * marginX), CSng(maxY - minY + 2 * marginY))
    End Sub

    ' 在Graphics对象上绘制[3,5](@ref)
    Public Sub Draw(g As Graphics, width As Integer, height As Integer)
        If _dataPoints.Count = 0 Then Return

        ' 计算缩放和偏移
        Dim scaleX = width / _bounds.Width
        Dim scaleY = height / _bounds.Height
        Dim scale = Math.Min(scaleX, scaleY) * 0.9 ' 保留边距

        Dim offsetX = -_bounds.Left + (width / scale - _bounds.Width) / 2
        Dim offsetY = -_bounds.Top + (height / scale - _bounds.Height) / 2

        ' 设置高质量渲染
        g.SmoothingMode = SmoothingMode.AntiAlias

        ' 绘制数据点[5](@ref)
        Dim pointBrush As New SolidBrush(Color.Blue)
        Dim pointRadius As Single = 3

        For Each point In _dataPoints
            Dim screenX = CSng((point.X + offsetX) * scale)
            Dim screenY = CSng((point.Y + offsetY) * scale)
            g.FillEllipse(pointBrush, screenX - pointRadius, screenY - pointRadius,
                         pointRadius * 2, pointRadius * 2)
        Next

        ' 绘制主曲线[3](@ref)
        If _curvePoints.Count > 1 Then
            Dim curvePen As New Pen(Color.Red, 2)
            Dim curvePoints = _curvePoints.Select(Function(p)
                                                      Return New PointF(CSng((p.X + offsetX) * scale), CSng((p.Y + offsetY) * scale))
                                                  End Function).ToArray()

            g.DrawLines(curvePen, curvePoints)
            curvePen.Dispose()
        End If

        pointBrush.Dispose()
    End Sub

    ' 在PictureBox中显示结果
    Public Sub DrawToPictureBox(pictureBox As PictureBox)
        If pictureBox.Width = 0 OrElse pictureBox.Height = 0 Then Return

        Using bmp As New Bitmap(pictureBox.Width, pictureBox.Height)
            Using g As Graphics = Graphics.FromImage(bmp)
                g.Clear(Color.White)
                Draw(g, pictureBox.Width, pictureBox.Height)
            End Using
            pictureBox.Image = bmp.Clone()
        End Using
    End Sub
End Class