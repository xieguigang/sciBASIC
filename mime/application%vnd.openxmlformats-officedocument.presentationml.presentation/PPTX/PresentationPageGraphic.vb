Imports System.Linq
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports std = System.Math

''' <summary>
''' 类似 GDI+ Graphics 的 PPT 页面生成对象。每个实例对应一张幻灯片。
''' 绘图调用不会立即光栅化，而是累积为 <see cref="PptElement"/> 列表，
''' 在文档保存时由 PptxPackager 统一序列化为 DrawingML。
''' 
''' 变换（平移/缩放/旋转）以状态方式累积，并在元素创建时烘焙进坐标。
''' Arc/Bezier/Curve/Pie/Path 等曲线以采样折线（自由形状）近似。
''' Clip 相关 API 不影响输出，仅保留调用兼容性。
''' </summary>
Public Class PresentationPageGraphic : Inherits IGraphics

    Private ReadOnly _size As Drawing.Size
    Private ReadOnly _elements As New List(Of PptElement)
    Private _scaleX As Single = 1
    Private _scaleY As Single = 1
    Private _translateX As Single = 0
    Private _translateY As Single = 0
    Private _rotation As Single = 0
    Private _background As Drawing.Color? = Nothing
    Private _renderingOrigin As Drawing.Point = Drawing.Point.Empty
    Private _textContrast As Integer = 4

    Public Overrides ReadOnly Property Size As Drawing.Size
        Get
            Return _size
        End Get
    End Property

    ''' <summary>使用该驱动值不会向 DriverLoad 注册，仅作为类型标识。</summary>
    Public Overrides ReadOnly Property Driver As Driver.Drivers
        Get
            Return Microsoft.VisualBasic.Imaging.Driver.Drivers.Default
        End Get
    End Property

    Public Overrides Property RenderingOrigin As Drawing.Point
        Get
            Return _renderingOrigin
        End Get
        Set(value As Drawing.Point)
            _renderingOrigin = value
        End Set
    End Property

    Public Overrides Property TextContrast As Integer
        Get
            Return _textContrast
        End Get
        Set(value As Integer)
            _textContrast = value
        End Set
    End Property

    ''' <summary>默认 16:9 页面（10 × 5.625 inch，100 dpi → 1000 × 563 px）。</summary>
    Public Sub New()
        Me.New(New Drawing.Size(1000, 563))
    End Sub

    ''' <summary>自定义页面尺寸（单位：100 dpi 像素）。</summary>
    Public Sub New(size As Drawing.Size)
        MyBase.New(dpi:=100)
        _size = size
    End Sub

    ''' <summary>以英寸为单位创建页面。</summary>
    Public Sub New(widthInches As Single, heightInches As Single)
        Me.New(New Drawing.Size(CInt(widthInches * 100), CInt(heightInches * 100)))
    End Sub

    ''' <summary>当前页面累积的元素（供打包器读取）。</summary>
    Friend ReadOnly Property Elements As IReadOnlyList(Of PptElement)
        Get
            Return _elements
        End Get
    End Property

    ''' <summary>页面背景色（由 Clear 设置）。</summary>
    Friend ReadOnly Property PageBackground As Drawing.Color?
        Get
            Return _background
        End Get
    End Property

    ''' <summary>新增 DrawTable：生成 PPT 原生表格，而非矩形+文本模拟。</summary>
    Public Sub DrawTable(rect As Drawing.RectangleF, data As String(,), Optional style As PptTableStyle = Nothing)
        If data Is Nothing Then
            Return
        End If

        _elements.Add(New PptTableElement With {
            .Position = TransformPoint(New Drawing.PointF(rect.X, rect.Y)),
            .Extent = New Drawing.SizeF(rect.Width * std.Abs(_scaleX), rect.Height * std.Abs(_scaleY)),
            .Rotation = _rotation,
            .Data = data,
            .Style = style
        })
    End Sub

    Public Sub DrawTable(rect As Drawing.Rectangle, data As String(,), Optional style As PptTableStyle = Nothing)
        Call DrawTable(New Drawing.RectangleF(rect.X, rect.Y, rect.Width, rect.Height), data, style)
    End Sub

    ' ------------------------------------------------------------------------
    ' 变换与坐标
    ' ------------------------------------------------------------------------

    Private Function TransformPoint(point As Drawing.PointF) As Drawing.PointF
        Return New Drawing.PointF(point.X * _scaleX + _translateX, point.Y * _scaleY + _translateY)
    End Function

    Private Function TransformPoint(x As Single, y As Single) As Drawing.PointF
        Return TransformPoint(New Drawing.PointF(x, y))
    End Function

    Private Function TransformRect(x As Single, y As Single, width As Single, height As Single) As Drawing.RectangleF
        Dim location As Drawing.PointF = TransformPoint(x, y)
        Dim w As Single = width * std.Abs(_scaleX)
        Dim h As Single = height * std.Abs(_scaleY)

        If w < 0 OrElse h < 0 Then
            location = New Drawing.PointF(location.X + std.Min(w, 0), location.Y + std.Min(h, 0))
            w = std.Abs(w)
            h = std.Abs(h)
        End If

        Return New Drawing.RectangleF(location, New Drawing.SizeF(w, h))
    End Function

    Private Function Transform(rect As Drawing.RectangleF) As Drawing.RectangleF
        Return TransformRect(rect.X, rect.Y, rect.Width, rect.Height)
    End Function

    Private Function Transform(points As Drawing.PointF()) As Drawing.PointF()
        If points Is Nothing Then
            Return Array.Empty(Of Drawing.PointF)()
        End If

        Return points.Select(Function(p) TransformPoint(p)).ToArray()
    End Function

    Private Sub AddShape(geometry As PptShapeGeometry, rect As Drawing.RectangleF, fill As Drawing.Color?, pen As Pen,
                         Optional points As Drawing.PointF() = Nothing, Optional closed As Boolean = False)
        Dim r As Drawing.RectangleF = Transform(rect)

        _elements.Add(New PptShapeElement With {
            .Geometry = geometry,
            .Position = r.Location,
            .Extent = r.Size,
            .Rotation = _rotation,
            .Fill = fill,
            .Stroke = pen,
            .Points = If(points Is Nothing, Nothing, Transform(points).ToList()),
            .IsClosed = closed
        })
    End Sub

    ' ------------------------------------------------------------------------
    ' 基础属性与生命周期
    ' ------------------------------------------------------------------------

    Public Overrides Sub AddMetafileComment(data() As Byte)
        ' PresentationML 无对应物，保留兼容性。
    End Sub

    Public Overrides Sub Flush()
        ' 元素在内存中累积，无需刷新。
    End Sub

    Protected Overrides Sub ClearCanvas(color As Drawing.Color)
        _background = color
        _elements.Clear()
    End Sub

    Protected Overrides Sub ReleaseHandle()
        _elements.Clear()
    End Sub

    Public Overrides Function GetContextInfo() As GraphicsContextInfo
        Return New GraphicsContextInfo With {.Context = _elements}
    End Function

    Public Overrides Function IsVisible(rect As Drawing.Rectangle) As Boolean
        Return IsVisible(New Drawing.RectangleF(rect.X, rect.Y, rect.Width, rect.Height))
    End Function

    Public Overrides Function IsVisible(rect As Drawing.RectangleF) As Boolean
        Return rect.Width > 0 AndAlso rect.Height > 0 AndAlso Drawing.RectangleF.Intersect(rect, New Drawing.RectangleF(Drawing.PointF.Empty, _size.SizeF)).Width > 0
    End Function

    Public Overrides Function IsVisible(x As Integer, y As Integer, width As Integer, height As Integer) As Boolean
        Return IsVisible(New Drawing.RectangleF(x, y, width, height))
    End Function

    Public Overrides Function IsVisible(x As Single, y As Single, width As Single, height As Single) As Boolean
        Return IsVisible(New Drawing.RectangleF(x, y, width, height))
    End Function

    ' ------------------------------------------------------------------------
    ' 线条与基础形状
    ' ------------------------------------------------------------------------

    Public Overrides Sub DrawLine(pen As Pen, pt1 As Drawing.PointF, pt2 As Drawing.PointF)
        Dim rect As New Drawing.RectangleF(std.Min(pt1.X, pt2.X), std.Min(pt1.Y, pt2.Y), std.Abs(pt2.X - pt1.X), std.Abs(pt2.Y - pt1.Y))
        Call AddShape(PptShapeGeometry.Line, rect, Nothing, pen, New Drawing.PointF() {pt1, pt2})
    End Sub

    Public Overrides Sub DrawLine(pen As Pen, pt1 As Drawing.Point, pt2 As Drawing.Point)
        Call DrawLine(pen, New Drawing.PointF(pt1.X, pt1.Y), New Drawing.PointF(pt2.X, pt2.Y))
    End Sub

    Public Overrides Sub DrawLine(pen As Pen, x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer)
        Call DrawLine(pen, New Drawing.PointF(x1, y1), New Drawing.PointF(x2, y2))
    End Sub

    Public Overrides Sub DrawLine(pen As Pen, x1 As Single, y1 As Single, x2 As Single, y2 As Single)
        Call DrawLine(pen, New Drawing.PointF(x1, y1), New Drawing.PointF(x2, y2))
    End Sub

    Public Overrides Sub DrawLines(pen As Pen, points() As Drawing.PointF)
        If points Is Nothing OrElse points.Length < 2 Then
            Return
        End If

        Call AddShape(PptShapeGeometry.Line, Bounds(points), Nothing, pen, points)
    End Sub

    Public Overrides Sub DrawLines(pen As Pen, points() As Drawing.Point)
        Call DrawLines(pen, points.Select(Function(p) New Drawing.PointF(p.X, p.Y)).ToArray())
    End Sub

    Public Overrides Sub DrawRectangle(pen As Pen, rect As Drawing.Rectangle)
        Call DrawRectangle(pen, New Drawing.RectangleF(rect.X, rect.Y, rect.Width, rect.Height))
    End Sub

    Public Overrides Sub DrawRectangle(pen As Pen, rect As Drawing.RectangleF)
        Call AddShape(PptShapeGeometry.Rectangle, rect, Nothing, pen)
    End Sub

    Public Overrides Sub DrawRectangle(pen As Pen, x As Single, y As Single, width As Single, height As Single)
        Call DrawRectangle(pen, New Drawing.RectangleF(x, y, width, height))
    End Sub

    Public Overrides Sub DrawRectangle(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer)
        Call DrawRectangle(pen, New Drawing.RectangleF(x, y, width, height))
    End Sub

    Public Overrides Sub DrawRectangles(pen As Pen, rects() As Drawing.RectangleF)
        If rects Is Nothing Then
            Return
        End If

        For Each rect As Drawing.RectangleF In rects
            Call DrawRectangle(pen, rect)
        Next
    End Sub

    Public Overrides Sub DrawRectangles(pen As Pen, rects() As Drawing.Rectangle)
        If rects Is Nothing Then
            Return
        End If

        For Each rect As Drawing.Rectangle In rects
            Call DrawRectangle(pen, rect)
        Next
    End Sub

    Public Overrides Sub FillRectangle(brush As Brush, rect As Drawing.Rectangle)
        Call FillRectangle(brush, New Drawing.RectangleF(rect.X, rect.Y, rect.Width, rect.Height))
    End Sub

    Public Overrides Sub FillRectangle(brush As Brush, rect As Drawing.RectangleF)
        Call AddShape(PptShapeGeometry.Rectangle, rect, PptxDrawing.SolidColor(brush), Nothing)
    End Sub

    Public Overrides Sub FillRectangle(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer)
        Call FillRectangle(brush, New Drawing.RectangleF(x, y, width, height))
    End Sub

    Public Overrides Sub FillRectangle(brush As Brush, x As Single, y As Single, width As Single, height As Single)
        Call FillRectangle(brush, New Drawing.RectangleF(x, y, width, height))
    End Sub

    Public Overrides Sub DrawEllipse(pen As Pen, rect As Drawing.Rectangle)
        Call DrawEllipse(pen, New Drawing.RectangleF(rect.X, rect.Y, rect.Width, rect.Height))
    End Sub

    Public Overrides Sub DrawEllipse(pen As Pen, rect As Drawing.RectangleF)
        Call AddShape(PptShapeGeometry.Ellipse, rect, Nothing, pen)
    End Sub

    Public Overrides Sub DrawEllipse(pen As Pen, x As Single, y As Single, width As Single, height As Single)
        Call DrawEllipse(pen, New Drawing.RectangleF(x, y, width, height))
    End Sub

    Public Overrides Sub DrawEllipse(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer)
        Call DrawEllipse(pen, New Drawing.RectangleF(x, y, width, height))
    End Sub

    Public Overrides Sub FillEllipse(brush As Brush, rect As Drawing.Rectangle)
        Call FillEllipse(brush, New Drawing.RectangleF(rect.X, rect.Y, rect.Width, rect.Height))
    End Sub

    Public Overrides Sub FillEllipse(brush As Brush, rect As Drawing.RectangleF)
        Call AddShape(PptShapeGeometry.Ellipse, rect, PptxDrawing.SolidColor(brush), Nothing)
    End Sub

    Public Overrides Sub FillEllipse(brush As Brush, x As Single, y As Single, width As Single, height As Single)
        Call FillEllipse(brush, New Drawing.RectangleF(x, y, width, height))
    End Sub

    Public Overrides Sub FillEllipse(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer)
        Call FillEllipse(brush, New Drawing.RectangleF(x, y, width, height))
    End Sub

    Public Overrides Sub DrawCircle(center As Drawing.PointF, fill As Drawing.Color, stroke As Pen, radius As Single)
        Dim rect As New Drawing.RectangleF(center.X - radius, center.Y - radius, radius * 2, radius * 2)
        Call AddShape(PptShapeGeometry.Ellipse, rect, fill, stroke)
    End Sub

    Public Overrides Sub DrawPolygon(pen As Pen, points() As Drawing.PointF)
        If points Is Nothing OrElse points.Length < 2 Then
            Return
        End If

        Call AddShape(PptShapeGeometry.Freeform, Bounds(points), Nothing, pen, points, True)
    End Sub

    Public Overrides Sub DrawPolygon(pen As Pen, points() As Drawing.Point)
        Call DrawPolygon(pen, points.Select(Function(p) New Drawing.PointF(p.X, p.Y)).ToArray())
    End Sub

    Public Overrides Sub FillPolygon(brush As Brush, points() As Drawing.PointF)
        If points Is Nothing OrElse points.Length < 2 Then
            Return
        End If

        Call AddShape(PptShapeGeometry.Freeform, Bounds(points), PptxDrawing.SolidColor(brush), Nothing, points, True)
    End Sub

    Public Overrides Sub FillPolygon(brush As Brush, points() As Drawing.Point)
        Call FillPolygon(brush, points.Select(Function(p) New Drawing.PointF(p.X, p.Y)).ToArray())
    End Sub

    Private Shared Function Bounds(points As Drawing.PointF()) As Drawing.RectangleF
        Dim minX As Single = points.Min(Function(p) p.X)
        Dim minY As Single = points.Min(Function(p) p.Y)
        Dim maxX As Single = points.Max(Function(p) p.X)
        Dim maxY As Single = points.Max(Function(p) p.Y)
        Return New Drawing.RectangleF(minX, minY, maxX - minX, maxY - minY)
    End Function

    ' ------------------------------------------------------------------------
    ' 文本
    ' ------------------------------------------------------------------------

    Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, ByRef point As Drawing.PointF)
        Dim textSize As Drawing.SizeF = MeasureString(s, font)
        Call AddText(s, font, brush, New Drawing.RectangleF(point, textSize))
    End Sub

    Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, x As Single, y As Single)
        Dim point As New Drawing.PointF(x, y)
        Call DrawString(s, font, brush, point)
    End Sub

    Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, layoutRectangle As Drawing.RectangleF)
        Call AddText(s, font, brush, layoutRectangle)
    End Sub

    Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, ByRef x As Single, ByRef y As Single, angle As Single)
        Dim point As New Drawing.PointF(x, y)
        Call DrawString(s, font, brush, point)
    End Sub

    Private Sub AddText(s As String, font As Font, brush As Brush, layoutRect As Drawing.RectangleF)
        If String.IsNullOrEmpty(s) Then
            Return
        End If

        Dim r As Drawing.RectangleF = Transform(layoutRect)
        _elements.Add(New PptTextElement With {
            .Position = r.Location,
            .Extent = r.Size,
            .Rotation = _rotation,
            .Text = s,
            .Font = font,
            .Color = PptxDrawing.SolidColor(brush, Drawing.Color.Black),
            .Alignment = PptTextAlign.Left
        })
    End Sub

    Public Overrides Function MeasureString(text As String, font As Font) As Drawing.SizeF
        Try
            Return DriverLoad.MeasureTextSize(text, font)
        Catch ex As Exception
            Dim size As Single = If(font Is Nothing, 12, font.Size)
            Dim lines As Integer = If(String.IsNullOrEmpty(text), 1, text.Length)
            Return New Drawing.SizeF(lines * size * 0.5F, size * 1.2F)
        End Try
    End Function

    Public Overrides Function MeasureString(text As String, font As Font, width As Integer) As Drawing.SizeF
        Return MeasureString(text, font)
    End Function

    Public Overrides Function MeasureString(text As String, font As Font, layoutArea As Drawing.SizeF) As Drawing.SizeF
        Return MeasureString(text, font)
    End Function

    Public Overrides Function GetStringPath(s As String, rect As Drawing.RectangleF, font As Font) As GraphicsPath
        Dim path As New GraphicsPath()
        Call path.AddPolygon(New Drawing.PointF() {
            New Drawing.PointF(rect.X, rect.Y),
            New Drawing.PointF(rect.Right, rect.Y),
            New Drawing.PointF(rect.Right, rect.Bottom),
            New Drawing.PointF(rect.X, rect.Bottom)
        })
        Return path
    End Function

    ' ------------------------------------------------------------------------
    ' 图片
    ' ------------------------------------------------------------------------

    Public Overrides Sub DrawImage(image As Image, point As Drawing.Point)
        Call DrawImage(image, New Drawing.PointF(point.X, point.Y))
    End Sub

    Public Overrides Sub DrawImage(image As Image, point As Drawing.PointF)
        If image Is Nothing Then
            Return
        End If

        Call DrawImage(image, New Drawing.RectangleF(point, New Drawing.SizeF(image.Width, image.Height)))
    End Sub

    Public Overrides Sub DrawImage(image As Image, x As Integer, y As Integer)
        Call DrawImage(image, New Drawing.PointF(x, y))
    End Sub

    Public Overrides Sub DrawImage(image As Image, x As Single, y As Single)
        Call DrawImage(image, New Drawing.PointF(x, y))
    End Sub

    Public Overrides Sub DrawImage(image As Image, rect As Drawing.Rectangle)
        Call DrawImage(image, New Drawing.RectangleF(rect.X, rect.Y, rect.Width, rect.Height))
    End Sub

    Public Overrides Sub DrawImage(image As Image, rect As Drawing.RectangleF)
        If image Is Nothing Then
            Return
        End If

        Dim r As Drawing.RectangleF = Transform(rect)
        _elements.Add(New PptImageElement With {
            .Position = r.Location,
            .Extent = r.Size,
            .Rotation = _rotation,
            .Image = image
        })
    End Sub

    Public Overrides Sub DrawImage(image As Image, x As Single, y As Single, width As Single, height As Single)
        Call DrawImage(image, New Drawing.RectangleF(x, y, width, height))
    End Sub

    Public Overrides Sub DrawImage(image As Image, x As Integer, y As Integer, width As Integer, height As Integer)
        Call DrawImage(image, New Drawing.RectangleF(x, y, width, height))
    End Sub

    Public Overrides Sub DrawImage(image As Image, destPoints() As Drawing.PointF)
        If destPoints Is Nothing OrElse destPoints.Length = 0 Then
            Return
        End If

        Call DrawImage(image, Bounds(destPoints))
    End Sub

    Public Overrides Sub DrawImage(image As Image, destPoints() As Drawing.Point)
        Call DrawImage(image, destPoints.Select(Function(p) New Drawing.PointF(p.X, p.Y)).ToArray())
    End Sub

    Public Overrides Sub DrawImageUnscaled(image As Image, rect As Drawing.Rectangle)
        Call DrawImageUnscaled(image, rect.Location)
    End Sub

    Public Overrides Sub DrawImageUnscaled(image As Image, point As Drawing.Point)
        If image Is Nothing Then
            Return
        End If

        Call DrawImage(image, New Drawing.RectangleF(point.X, point.Y, image.Width, image.Height))
    End Sub

    Public Overrides Sub DrawImageUnscaled(image As Image, x As Integer, y As Integer)
        Call DrawImageUnscaled(image, New Drawing.Point(x, y))
    End Sub

    Public Overrides Sub DrawImageUnscaled(image As Image, x As Integer, y As Integer, width As Integer, height As Integer)
        Call DrawImageUnscaled(image, New Drawing.Point(x, y))
    End Sub

    Public Overrides Sub DrawImageUnscaledAndClipped(image As Image, rect As Drawing.Rectangle)
        If image Is Nothing Then
            Return
        End If

        Dim clipped As Drawing.Rectangle = Drawing.Rectangle.Intersect(rect, New Drawing.Rectangle(rect.Location, New Drawing.Size(image.Width, image.Height)))
        Call DrawImage(image, New Drawing.RectangleF(clipped.X, clipped.Y, clipped.Width, clipped.Height))
    End Sub

    ' ------------------------------------------------------------------------
    ' 曲线：以采样折线近似
    ' ------------------------------------------------------------------------

    Public Overrides Sub DrawArc(pen As Pen, rect As Drawing.RectangleF, startAngle As Single, sweepAngle As Single)
        Call AddShape(PptShapeGeometry.Freeform, rect, Nothing, pen, SampleArc(rect, startAngle, sweepAngle))
    End Sub

    Public Overrides Sub DrawArc(pen As Pen, rect As Drawing.Rectangle, startAngle As Single, sweepAngle As Single)
        Call DrawArc(pen, New Drawing.RectangleF(rect.X, rect.Y, rect.Width, rect.Height), startAngle, sweepAngle)
    End Sub

    Public Overrides Sub DrawArc(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)
        Call DrawArc(pen, New Drawing.RectangleF(x, y, width, height), startAngle, sweepAngle)
    End Sub

    Public Overrides Sub DrawArc(pen As Pen, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)
        Call DrawArc(pen, New Drawing.RectangleF(x, y, width, height), startAngle, sweepAngle)
    End Sub

    Private Shared Function SampleArc(rect As Drawing.RectangleF, startAngle As Single, sweepAngle As Single) As Drawing.PointF()
        Dim steps As Integer = std.Max(8, CInt(std.Abs(sweepAngle) / 5))
        Dim points(steps) As Drawing.PointF
        Dim cx As Single = rect.X + rect.Width / 2
        Dim cy As Single = rect.Y + rect.Height / 2

        For i As Integer = 0 To steps
            Dim angle As Double = (startAngle + sweepAngle * i / steps) * std.PI / 180.0
            points(i) = New Drawing.PointF(CSng(cx + std.Cos(angle) * rect.Width / 2), CSng(cy + std.Sin(angle) * rect.Height / 2))
        Next

        Return points
    End Function

    Public Overrides Sub DrawBezier(pen As Pen, pt1 As Drawing.Point, pt2 As Drawing.Point, pt3 As Drawing.Point, pt4 As Drawing.Point)
        Call DrawBezier(pen, New Drawing.PointF(pt1.X, pt1.Y), New Drawing.PointF(pt2.X, pt2.Y), New Drawing.PointF(pt3.X, pt3.Y), New Drawing.PointF(pt4.X, pt4.Y))
    End Sub

    Public Overrides Sub DrawBezier(pen As Pen, pt1 As Drawing.PointF, pt2 As Drawing.PointF, pt3 As Drawing.PointF, pt4 As Drawing.PointF)
        Call AddBezier(pen, New Drawing.PointF() {pt1, pt2, pt3, pt4})
    End Sub

    Public Overrides Sub DrawBezier(pen As Pen, x1 As Single, y1 As Single, x2 As Single, y2 As Single, x3 As Single, y3 As Single, x4 As Single, y4 As Single)
        Call DrawBezier(pen, New Drawing.PointF(x1, y1), New Drawing.PointF(x2, y2), New Drawing.PointF(x3, y3), New Drawing.PointF(x4, y4))
    End Sub

    Public Overrides Sub DrawBeziers(pen As Pen, points() As Drawing.Point)
        Call DrawBeziers(pen, points.Select(Function(p) New Drawing.PointF(p.X, p.Y)).ToArray())
    End Sub

    Public Overrides Sub DrawBeziers(pen As Pen, points() As Drawing.PointF)
        If points Is Nothing OrElse points.Length < 4 Then
            Return
        End If

        For i As Integer = 0 To points.Length - 4 Step 3
            Call AddBezier(pen, New Drawing.PointF() {points(i), points(i + 1), points(i + 2), points(i + 3)})
        Next
    End Sub

    Private Sub AddBezier(pen As Pen, control As Drawing.PointF())
        Dim points As Drawing.PointF() = SampleBezier(control)
        Call AddShape(PptShapeGeometry.Freeform, Bounds(points), Nothing, pen, points)
    End Sub

    Private Shared Function SampleBezier(control As Drawing.PointF()) As Drawing.PointF()
        Const steps As Integer = 20
        Dim points(steps) As Drawing.PointF

        For i As Integer = 0 To steps
            Dim t As Single = i / steps
            Dim u As Single = 1 - t
            Dim b0 As Single = u * u * u
            Dim b1 As Single = 3 * u * u * t
            Dim b2 As Single = 3 * u * t * t
            Dim b3 As Single = t * t * t
            points(i) = New Drawing.PointF(control(0).X * b0 + control(1).X * b1 + control(2).X * b2 + control(3).X * b3,
                                            control(0).Y * b0 + control(1).Y * b1 + control(2).Y * b2 + control(3).Y * b3)
        Next

        Return points
    End Function

    Public Overrides Sub DrawCurve(pen As Pen, points() As Drawing.PointF)
        Call DrawCurve(pen, points, 0.5F)
    End Sub

    Public Overrides Sub DrawCurve(pen As Pen, points() As Drawing.Point)
        Call DrawCurve(pen, points.Select(Function(p) New Drawing.PointF(p.X, p.Y)).ToArray())
    End Sub

    Public Overrides Sub DrawCurve(pen As Pen, points() As Drawing.PointF, tension As Single)
        If points Is Nothing OrElse points.Length < 2 Then
            Return
        End If

        Call AddShape(PptShapeGeometry.Freeform, Bounds(points), Nothing, pen, SampleCurve(points, tension))
    End Sub

    Public Overrides Sub DrawCurve(pen As Pen, points() As Drawing.Point, tension As Single)
        Call DrawCurve(pen, points.Select(Function(p) New Drawing.PointF(p.X, p.Y)).ToArray(), tension)
    End Sub

    Public Overrides Sub DrawCurve(pen As Pen, points() As Drawing.PointF, offset As Integer, numberOfSegments As Integer)
        Call DrawCurve(pen, points.Skip(offset).Take(numberOfSegments + 1).ToArray())
    End Sub

    Public Overrides Sub DrawCurve(pen As Pen, points() As Drawing.PointF, offset As Integer, numberOfSegments As Integer, tension As Single)
        Call DrawCurve(pen, points.Skip(offset).Take(numberOfSegments + 1).ToArray(), tension)
    End Sub

    Public Overrides Sub DrawCurve(pen As Pen, points() As Drawing.Point, offset As Integer, numberOfSegments As Integer, tension As Single)
        Call DrawCurve(pen, points.Skip(offset).Take(numberOfSegments + 1).Select(Function(p) New Drawing.PointF(p.X, p.Y)).ToArray(), tension)
    End Sub

    Private Shared Function SampleCurve(points As Drawing.PointF(), tension As Single) As Drawing.PointF()
        If points.Length = 2 Then
            Return points
        End If

        Dim result As New List(Of Drawing.PointF) From {points(0)}
        For i As Integer = 0 To points.Length - 2
            Dim p0 As Drawing.PointF = If(i > 0, points(i - 1), points(0))
            Dim p1 As Drawing.PointF = points(i)
            Dim p2 As Drawing.PointF = points(i + 1)
            Dim p3 As Drawing.PointF = If(i + 2 < points.Length, points(i + 2), p2)

            For s As Integer = 1 To 8
                Dim t As Single = s / 8
                Dim t2 As Single = t * t
                Dim t3 As Single = t2 * t
                Dim m1X As Single = tension * (p2.X - p0.X)
                Dim m1Y As Single = tension * (p2.Y - p0.Y)
                Dim m2X As Single = tension * (p3.X - p1.X)
                Dim m2Y As Single = tension * (p3.Y - p1.Y)
                Dim h1 As Single = 2 * t3 - 3 * t2 + 1
                Dim h2 As Single = t3 - 2 * t2 + t
                Dim h3 As Single = -2 * t3 + 3 * t2
                Dim h4 As Single = t3 - t2
                result.Add(New Drawing.PointF(h1 * p1.X + h2 * m1X + h3 * p2.X + h4 * m2X,
                                              h1 * p1.Y + h2 * m1Y + h3 * p2.Y + h4 * m2Y))
            Next
        Next

        Return result.ToArray()
    End Function

    Public Overrides Sub DrawClosedCurve(pen As Pen, points() As Drawing.PointF)
        If points Is Nothing OrElse points.Length < 2 Then
            Return
        End If

        Call AddShape(PptShapeGeometry.Freeform, Bounds(points), Nothing, pen, SampleClosedCurve(points), True)
    End Sub

    Public Overrides Sub DrawClosedCurve(pen As Pen, points() As Drawing.Point)
        Call DrawClosedCurve(pen, points.Select(Function(p) New Drawing.PointF(p.X, p.Y)).ToArray())
    End Sub

    Public Overrides Sub FillClosedCurve(brush As Brush, points() As Drawing.PointF)
        Call FillPolygon(brush, SampleClosedCurve(points))
    End Sub

    Public Overrides Sub FillClosedCurve(brush As Brush, points() As Drawing.Point)
        Call FillClosedCurve(brush, points.Select(Function(p) New Drawing.PointF(p.X, p.Y)).ToArray())
    End Sub

    Private Shared Function SampleClosedCurve(points As Drawing.PointF()) As Drawing.PointF()
        If points Is Nothing OrElse points.Length < 3 Then
            Return points
        End If

        Dim closed As New List(Of Drawing.PointF)(points) From {points(0), points(1)}
        Return SampleCurve(closed.ToArray(), 0.5F)
    End Function

    Public Overrides Sub DrawPie(pen As Pen, rect As Drawing.Rectangle, startAngle As Single, sweepAngle As Single)
        Call DrawPie(pen, New Drawing.RectangleF(rect.X, rect.Y, rect.Width, rect.Height), startAngle, sweepAngle)
    End Sub

    Public Overrides Sub DrawPie(pen As Pen, rect As Drawing.RectangleF, startAngle As Single, sweepAngle As Single)
        Call AddShape(PptShapeGeometry.Freeform, rect, Nothing, pen, SamplePie(rect, startAngle, sweepAngle), True)
    End Sub

    Public Overrides Sub DrawPie(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)
        Call DrawPie(pen, New Drawing.RectangleF(x, y, width, height), startAngle, sweepAngle)
    End Sub

    Public Overrides Sub DrawPie(pen As Pen, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)
        Call DrawPie(pen, New Drawing.RectangleF(x, y, width, height), startAngle, sweepAngle)
    End Sub

    Public Overrides Sub FillPie(brush As Brush, rect As Drawing.Rectangle, startAngle As Single, sweepAngle As Single)
        Call AddPie(brush, New Drawing.RectangleF(rect.X, rect.Y, rect.Width, rect.Height), startAngle, sweepAngle)
    End Sub

    Public Overrides Sub FillPie(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)
        Call AddPie(brush, New Drawing.RectangleF(x, y, width, height), startAngle, sweepAngle)
    End Sub

    Public Overrides Sub FillPie(brush As Brush, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)
        Call AddPie(brush, New Drawing.RectangleF(x, y, width, height), startAngle, sweepAngle)
    End Sub

    Private Sub AddPie(brush As Brush, rect As Drawing.RectangleF, startAngle As Single, sweepAngle As Single)
        Call AddShape(PptShapeGeometry.Freeform, rect, PptxDrawing.SolidColor(brush), Nothing, SamplePie(rect, startAngle, sweepAngle), True)
    End Sub

    Private Shared Function SamplePie(rect As Drawing.RectangleF, startAngle As Single, sweepAngle As Single) As Drawing.PointF()
        Dim cx As Single = rect.X + rect.Width / 2
        Dim cy As Single = rect.Y + rect.Height / 2
        Dim arc As Drawing.PointF() = SampleArc(rect, startAngle, sweepAngle)
        Dim result As New List(Of Drawing.PointF) From {New Drawing.PointF(cx, cy)}
        result.AddRange(arc)
        Return result.ToArray()
    End Function

    Public Overrides Sub DrawPath(pen As Pen, path As GraphicsPath)
        If path Is Nothing OrElse path.PathPoints Is Nothing OrElse path.PathPoints.Length = 0 Then
            Return
        End If

        Call AddShape(PptShapeGeometry.Freeform, Bounds(path.PathPoints), Nothing, pen, path.PathPoints)
    End Sub

    Public Overrides Sub FillPath(brush As Brush, path As GraphicsPath)
        If path Is Nothing OrElse path.PathPoints Is Nothing OrElse path.PathPoints.Length = 0 Then
            Return
        End If

        Call AddShape(PptShapeGeometry.Freeform, Bounds(path.PathPoints), PptxDrawing.SolidColor(brush), Nothing, path.PathPoints, True)
    End Sub

    ' ------------------------------------------------------------------------
    ' Clip（近似：不影响输出，仅保留调用兼容性）
    ' ------------------------------------------------------------------------

    Public Overrides Sub ExcludeClip(rect As Drawing.Rectangle)
    End Sub

    Public Overrides Sub IntersectClip(rect As Drawing.RectangleF)
    End Sub

    Public Overrides Sub IntersectClip(rect As Drawing.Rectangle)
    End Sub

    Public Overrides Sub ResetClip()
    End Sub

    Public Overrides Sub SetClip(rect As Drawing.RectangleF)
    End Sub

    Public Overrides Sub SetClip(rect As Drawing.Rectangle)
    End Sub

    Public Overrides Sub TranslateClip(dx As Single, dy As Single)
    End Sub

    Public Overrides Sub TranslateClip(dx As Integer, dy As Integer)
    End Sub

    ' ------------------------------------------------------------------------
    ' Transform
    ' ------------------------------------------------------------------------

    Public Overrides Sub ResetTransform()
        _scaleX = 1
        _scaleY = 1
        _translateX = 0
        _translateY = 0
        _rotation = 0
    End Sub

    Public Overrides Sub RotateTransform(angle As Single)
        _rotation = (_rotation + angle) Mod 360
    End Sub

    Public Overrides Sub ScaleTransform(sx As Single, sy As Single)
        _scaleX *= sx
        _scaleY *= sy
    End Sub

    Public Overrides Sub TranslateTransform(dx As Single, dy As Single)
        _translateX += dx
        _translateY += dy
    End Sub
End Class
