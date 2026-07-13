#Region "Microsoft.VisualBasic::d2ce444eda903d488dae51493ae3bc6f, gr\Microsoft.VisualBasic.Imaging\PostScript\GraphicsPostScript.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.


    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 542
    '    Code Lines: 418 (77.12%)
    ' Comment Lines: 13 (2.40%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 111 (20.48%)
    '     File Size: 21.96 KB


    '     Class GraphicsPostScript
    ' 
    '         Properties: Driver, RenderingOrigin, Size, TextContrast
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetContextInfo, GetStringPath, (+4 Overloads) IsVisible, (+3 Overloads) MeasureString
    ' 
    '         Sub: AddMetafileComment, ClearCanvas, (+4 Overloads) DrawArc, (+3 Overloads) DrawBezier, (+2 Overloads) DrawBeziers
    '              DrawCircle, (+2 Overloads) DrawClosedCurve, (+7 Overloads) DrawCurve, (+4 Overloads) DrawEllipse, (+10 Overloads) DrawImage
    '              (+4 Overloads) DrawImageUnscaled, DrawImageUnscaledAndClipped, (+4 Overloads) DrawLine, (+2 Overloads) DrawLines, DrawPath
    '              (+4 Overloads) DrawPie, (+2 Overloads) DrawPolygon, (+4 Overloads) DrawRectangle, (+2 Overloads) DrawRectangles, (+4 Overloads) DrawString
    '              ExcludeClip, (+2 Overloads) FillClosedCurve, (+4 Overloads) FillEllipse, FillPath, (+3 Overloads) FillPie
    '              (+2 Overloads) FillPolygon, (+4 Overloads) FillRectangle, Flush, (+2 Overloads) IntersectClip, ReleaseHandle
    '              ResetClip, ResetTransform, RotateTransform, ScaleTransform, (+2 Overloads) SetClip
    '              (+2 Overloads) TranslateClip, TranslateTransform
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Net.Http
Imports std = System.Math

Namespace PostScript

    ''' <summary>
    ''' Graphics for create postscript 
    ''' </summary>
    Public Class GraphicsPostScript : Inherits IGraphics

        Public Overrides Property RenderingOrigin As Point
        Public Overrides Property TextContrast As Integer
        Public Overrides ReadOnly Property Size As Size
            Get
                Return painting.size
            End Get
        End Property

        Public Overrides ReadOnly Property Driver As Drivers
            Get
                Return Drivers.PostScript
            End Get
        End Property

        Dim painting As New PostScriptBuilder

        ''' <summary>
        ''' the current transform matrix (ctm) that is baked into the element
        ''' coordinates before they are stored. works in the gdi+ space, the
        ''' postscript y flip is applied later by the <see cref="Writer"/>.
        ''' </summary>
        Dim ctm As New Matrix2D

        Sub New(size As Size, dpi As Size)
            Call MyBase.New(dpi)
            painting.size = size
        End Sub

        Public Overrides Sub AddMetafileComment(data() As Byte)
            Call painting.Add(New PsComment(data))
        End Sub

        Protected Overrides Sub ClearCanvas(color As Color)
            Call painting.Clear()
            Call painting.Add(New Elements.Rectangle(New Rectangle(New Point, painting.size), color))
        End Sub

        ''' <summary>
        ''' do nothing at here
        ''' </summary>
        Protected Overrides Sub ReleaseHandle()
        End Sub

#Region "transform helpers (bake ctm into gdi+ coordinates)"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function tf(p As PointF) As PointF
            Return ctm.TransformPoint(p)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function tf(p As Point) As PointF
            Return ctm.TransformPoint(New PointF(p.X, p.Y))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function tf(rect As RectangleF) As RectangleF
            Dim a = tf(New PointF(rect.X, rect.Y))
            Dim b = tf(New PointF(rect.Right, rect.Y))
            Dim c = tf(New PointF(rect.Right, rect.Bottom))
            Dim d = tf(New PointF(rect.X, rect.Bottom))
            Dim minX = std.Min(std.Min(a.X, b.X), std.Min(c.X, d.X))
            Dim maxX = std.Max(std.Max(a.X, b.X), std.Max(c.X, d.X))
            Dim minY = std.Min(std.Min(a.Y, b.Y), std.Min(c.Y, d.Y))
            Dim maxY = std.Max(std.Max(a.Y, b.Y), std.Max(c.Y, d.Y))

            Return New RectangleF(minX, minY, maxX - minX, maxY - minY)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function tf(rect As Rectangle) As RectangleF
            Return tf(New RectangleF(rect.X, rect.Y, rect.Width, rect.Height))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function tf(points As PointF()) As PointF()
            If points Is Nothing Then
                Return Nothing
            End If

            Return points _
                .Select(Function(p) tf(p)) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function tfScale(size As SizeF) As SizeF
            Dim s = ctm.ScaleFactors()
            Return New SizeF(size.Width * s.Width, size.Height * s.Height)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function tfRadius(radius!) As Single
            Dim s = ctm.ScaleFactors()
            Return radius * (s.Width + s.Height) / 2
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function brushColor(brush As Brush) As String
            If TypeOf brush Is SolidBrush Then
                Return DirectCast(brush, SolidBrush).Color.ToHtmlColor
            Else
                ' gradient / texture brush: fall back to black for the postscript backend
                Return "black"
            End If
        End Function

#End Region

#Region "curve / bezier sampling helpers"

        Private Shared Function bezierPoint(p0 As PointF, p1 As PointF, p2 As PointF, p3 As PointF, t!) As PointF
            Dim u = 1 - t
            Dim uu = u * u
            Dim uuu = uu * u
            Dim tt = t * t
            Dim ttt = tt * t

            Dim x = uuu * p0.X + 3 * uu * t * p1.X + 3 * u * tt * p2.X + ttt * p3.X
            Dim y = uuu * p0.Y + 3 * uu * t * p1.Y + 3 * u * tt * p2.Y + ttt * p3.Y

            Return New PointF(CSng(x), CSng(y))
        End Function

        Private Shared Function sampleBezier(p0 As PointF, p1 As PointF, p2 As PointF, p3 As PointF, steps%) As PointF()
            Dim out(steps) As PointF

            For s As Integer = 0 To steps
                out(s) = bezierPoint(p0, p1, p2, p3, s / steps)
            Next

            Return out
        End Function

        ''' <summary>
        ''' sample a (optionally closed) cardinal spline through the given points.
        ''' the tension follows the gdi+ convention (default 0.5).
        ''' </summary>
        Private Shared Function sampleCurve(points As PointF(), tension!, closed As Boolean) As PointF()
            If points Is Nothing OrElse points.Length < 2 Then
                Return points
            End If

            Dim result As New List(Of PointF)
            Dim n = points.Length
            Dim steps = 16

            For i As Integer = 0 To n - 2
                Dim p0 = points(If(closed, (i - 1 + n) Mod n, If(i - 1 < 0, 0, i - 1)))
                Dim p1 = points(i)
                Dim p2 = points(i + 1)
                Dim p3 = points(If(closed, (i + 2) Mod n, If(i + 2 >= n, n - 1, i + 2)))

                Dim c1 = New PointF(p1.X + (p2.X - p0.X) * tension / 3, p1.Y + (p2.Y - p0.Y) * tension / 3)
                Dim c2 = New PointF(p2.X - (p3.X - p1.X) * tension / 3, p2.Y - (p3.Y - p1.Y) * tension / 3)

                For s As Integer = 0 To steps
                    result.Add(bezierPoint(p1, c1, c2, p2, s / steps))
                Next
            Next

            If closed Then
                result.Add(points(0))
            End If

            Return result.ToArray
        End Function

#End Region

        Public Overrides Sub DrawArc(pen As Pen, rect As RectangleF, startAngle As Single, sweepAngle As Single)
            Call DrawArc(pen, (rect.X), (rect.Y), (rect.Width), (rect.Height), startAngle, sweepAngle)
        End Sub

        Public Overrides Sub DrawArc(pen As Pen, rect As Rectangle, startAngle As Single, sweepAngle As Single)
            Call DrawArc(pen, CSng(rect.X), CSng(rect.Y), CSng(rect.Width), CSng(rect.Height), startAngle, sweepAngle)
        End Sub

        Public Overrides Sub DrawArc(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)
            Call DrawArc(pen, CSng(x), CSng(y), CSng(width), CSng(height), CSng(startAngle), CSng(sweepAngle))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub DrawArc(pen As Pen, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)
            Dim r = tf(New RectangleF(x, y, width, height))

            Call painting.Add(New Elements.Arc With {
                .height = r.Height,
                .startAngle = startAngle,
                .stroke = New Stroke(pen),
                .sweepAngle = sweepAngle,
                .width = r.Width,
                .x = r.X,
                .y = r.Y
            })
        End Sub

        Public Overrides Sub DrawBezier(pen As Pen, pt1 As Point, pt2 As Point, pt3 As Point, pt4 As Point)
            Call DrawBezier(pen, pt1.PointF, pt2.PointF, pt3.PointF, pt4.PointF)
        End Sub

        Public Overrides Sub DrawBezier(pen As Pen, pt1 As PointF, pt2 As PointF, pt3 As PointF, pt4 As PointF)
            Dim pts = sampleBezier(pt1, pt2, pt3, pt4, 32)

            Call painting.Add(New Elements.Poly With {
                .closedPath = False,
                .fill = Nothing,
                .points = tf(pts),
                .stroke = New Stroke(pen)
            })
        End Sub

        Public Overrides Sub DrawBezier(pen As Pen, x1 As Single, y1 As Single, x2 As Single, y2 As Single, x3 As Single, y3 As Single, x4 As Single, y4 As Single)
            Call DrawBezier(pen, New PointF(x1, y1), New PointF(x2, y2), New PointF(x3, y3), New PointF(x4, y4))
        End Sub

        Public Overrides Sub DrawBeziers(pen As Pen, points() As PointF)
            If points Is Nothing OrElse points.Length < 4 Then
                Return
            End If

            Dim all As New List(Of PointF)

            For i As Integer = 0 To points.Length - 4 Step 3
                Dim seg = sampleBezier(points(i), points(i + 1), points(i + 2), points(i + 3), 16)

                If all.Count > 0 Then
                    ' skip the duplicated join point
                    seg = seg.Skip(1).ToArray
                End If

                Call all.AddRange(seg)
            Next

            Call painting.Add(New Elements.Poly With {
                .closedPath = False,
                .fill = Nothing,
                .points = tf(all.ToArray),
                .stroke = New Stroke(pen)
            })
        End Sub

        Public Overrides Sub DrawBeziers(pen As Pen, points() As Point)
            If points Is Nothing OrElse points.Length < 4 Then
                Return
            End If

            Call DrawBeziers(pen, points _
                .Select(Function(p) p.PointF) _
                .ToArray)
        End Sub

        Public Overrides Sub DrawClosedCurve(pen As Pen, points() As Point)
            Call DrawClosedCurve(pen, points.PointF.ToArray)
        End Sub

        Public Overrides Sub DrawClosedCurve(pen As Pen, points() As PointF)
            Dim pts = sampleCurve(points, 0.5, closed:=True)

            Call painting.Add(New Elements.Poly With {
                .closedPath = True,
                .fill = Nothing,
                .points = tf(pts),
                .stroke = New Stroke(pen)
            })
        End Sub

        Public Overrides Sub DrawCurve(pen As Pen, points() As Point)
            Call DrawCurve(pen, points.PointF.ToArray)
        End Sub

        Public Overrides Sub DrawCurve(pen As Pen, points() As PointF)
            Call DrawCurve(pen, points, 0.5)
        End Sub

        Public Overrides Sub DrawCurve(pen As Pen, points() As PointF, tension As Single)
            Dim pts = sampleCurve(points, tension, closed:=False)

            Call painting.Add(New Elements.Poly With {
                .closedPath = False,
                .fill = Nothing,
                .points = tf(pts),
                .stroke = New Stroke(pen)
            })
        End Sub

        Public Overrides Sub DrawCurve(pen As Pen, points() As Point, tension As Single)
            Call DrawCurve(pen, points.PointF.ToArray, tension)
        End Sub

        Public Overrides Sub DrawCurve(pen As Pen, points() As PointF, offset As Integer, numberOfSegments As Integer)
            Call DrawCurve(pen, points, offset, numberOfSegments, 0.5)
        End Sub

        Public Overrides Sub DrawCurve(pen As Pen, points() As PointF, offset As Integer, numberOfSegments As Integer, tension As Single)
            If points Is Nothing OrElse offset < 0 OrElse numberOfSegments < 1 Then
                Return
            End If

            Dim subset = points _
                .Skip(offset) _
                .Take(numberOfSegments + 1) _
                .ToArray

            Call DrawCurve(pen, subset, tension)
        End Sub

        Public Overrides Sub DrawCurve(pen As Pen, points() As Point, offset As Integer, numberOfSegments As Integer, tension As Single)
            If points Is Nothing Then
                Return
            End If

            Call DrawCurve(pen, points.PointF.ToArray, offset, numberOfSegments, tension)
        End Sub

        Public Overrides Sub DrawEllipse(pen As Pen, rect As Rectangle)
            Call DrawEllipse(pen, New RectangleF(rect.X, rect.Y, rect.Width, rect.Height))
        End Sub

        Public Overrides Sub DrawEllipse(pen As Pen, rect As RectangleF)
            Call DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height)
        End Sub

        Public Overrides Sub DrawEllipse(pen As Pen, x As Single, y As Single, width As Single, height As Single)
            Dim r = tf(New RectangleF(x, y, width, height))

            Call painting.Add(New Elements.Arc With {
                .height = r.Height,
                .startAngle = 0,
                .stroke = New Stroke(pen),
                .sweepAngle = 360,
                .width = r.Width,
                .x = r.X,
                .y = r.Y
            })
        End Sub

        Public Overrides Sub DrawEllipse(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer)
            Call DrawEllipse(pen, CSng(x), CSng(y), CSng(width), CSng(height))
        End Sub

        Public Overrides Sub DrawImage(image As Image, point As Point)
            Call DrawImage(image, New PointF(point.X, point.Y))
        End Sub

        Public Overrides Sub DrawImage(image As Image, destPoints() As Point)
            Call DrawImage(image, destPoints _
                .Select(Function(p) p.PointF) _
                .ToArray)
        End Sub

        Public Overrides Sub DrawImage(image As Image, destPoints() As PointF)
            If image Is Nothing OrElse destPoints.IsNullOrEmpty Then
                Return
            End If

            Dim xs = destPoints.Select(Function(p) p.X)
            Dim ys = destPoints.Select(Function(p) p.Y)
            Dim minX = xs.Min
            Dim maxX = xs.Max
            Dim minY = ys.Min
            Dim maxY = ys.Max
            Dim rect = New RectangleF(minX, minY, maxX - minX, maxY - minY)

            Call DrawImage(image, rect)
        End Sub

        Public Overrides Sub DrawImage(image As Image, rect As Rectangle)
            Call DrawImage(image, New RectangleF(rect.X, rect.Y, rect.Width, rect.Height))
        End Sub

        Public Overrides Sub DrawImage(image As Image, point As PointF)
            Call AddImage(image, point, New Size(image.Width, image.Height), New SizeF(1, 1))
        End Sub

        Public Overrides Sub DrawImage(image As Image, rect As RectangleF)
            If image Is Nothing OrElse image.Width = 0 OrElse image.Height = 0 Then
                Return
            End If

            Dim sx = rect.Width / image.Width
            Dim sy = rect.Height / image.Height

            Call AddImage(image, rect.Location, New Size(image.Width, image.Height), New SizeF(sx, sy))
        End Sub

        Public Overrides Sub DrawImage(image As Image, x As Integer, y As Integer)
            Call DrawImage(image, CSng(x), CSng(y))
        End Sub

        Public Overrides Sub DrawImage(image As Image, x As Single, y As Single)
            Call AddImage(image, New PointF(x, y), New Size(image.Width, image.Height), New SizeF(1, 1))
        End Sub

        Public Overrides Sub DrawImage(image As Image, x As Single, y As Single, width As Single, height As Single)
            Call DrawImage(image, New RectangleF(x, y, width, height))
        End Sub

        Public Overrides Sub DrawImage(image As Image, x As Integer, y As Integer, width As Integer, height As Integer)
            Call DrawImage(image, CSng(x), CSng(y), CSng(width), CSng(height))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub AddImage(image As Image, location As PointF, size As Size, scale As SizeF)
            Call painting.Add(New Elements.ImageData With {
                .image = New DataURI(image),
                .location = tf(location),
                .size = size,
                .scale = tfScale(scale)
            })
        End Sub

        Public Overrides Sub DrawImageUnscaled(image As Image, rect As Rectangle)
            Call DrawImageUnscaled(image, rect.X, rect.Y)
        End Sub

        Public Overrides Sub DrawImageUnscaled(image As Image, point As Point)
            Call DrawImageUnscaled(image, point.X, point.Y)
        End Sub

        Public Overrides Sub DrawImageUnscaled(image As Image, x As Integer, y As Integer)
            Call AddImage(image, New PointF(x, y), New Size(image.Width, image.Height), New SizeF(1, 1))
        End Sub

        Public Overrides Sub DrawImageUnscaled(image As Image, x As Integer, y As Integer, width As Integer, height As Integer)
            ' the width/height parameters are not used by the unscaled draw
            Call AddImage(image, New PointF(x, y), New Size(image.Width, image.Height), New SizeF(1, 1))
        End Sub

        Public Overrides Sub DrawImageUnscaledAndClipped(image As Image, rect As Rectangle)
            ' the clipping region is not supported by the postscript backend yet,
            ' so the image is drawn unscaled at the top-left of the rectangle.
            Call AddImage(image, New PointF(rect.X, rect.Y), New Size(image.Width, image.Height), New SizeF(1, 1))
        End Sub

        Public Overrides Sub DrawLine(pen As Pen, pt1 As PointF, pt2 As PointF)
            Call painting.Add(New Elements.Line(pen, tf(pt1), tf(pt2)))
        End Sub

        Public Overrides Sub DrawLine(pen As Pen, pt1 As Point, pt2 As Point)
            Call painting.Add(New Elements.Line(pen, tf(pt1), tf(pt2)))
        End Sub

        Public Overrides Sub DrawLine(pen As Pen, x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer)
            Call painting.Add(New Elements.Line(pen, tf(New PointF(x1, y1)), tf(New PointF(x2, y2))))
        End Sub

        Public Overrides Sub DrawLine(pen As Pen, x1 As Single, y1 As Single, x2 As Single, y2 As Single)
            Call painting.Add(New Elements.Line(pen, tf(New PointF(x1, y1)), tf(New PointF(x2, y2))))
        End Sub

        Public Overrides Sub DrawLines(pen As Pen, points() As PointF)
            If points Is Nothing Then
                Return
            End If

            For Each ab As SlideWindow(Of PointF) In points.SlideWindows(2)
                Call painting.Add(New Elements.Line(pen, tf(ab(0)), tf(ab(1))))
            Next
        End Sub

        Public Overrides Sub DrawLines(pen As Pen, points() As Point)
            If points Is Nothing Then
                Return
            End If

            For Each ab As SlideWindow(Of Point) In points.SlideWindows(2)
                Call painting.Add(New Elements.Line(pen, tf(ab(0)), tf(ab(1))))
            Next
        End Sub

        Public Overrides Sub DrawPath(pen As Pen, path As GraphicsPath)
            If path Is Nothing Then
                Return
            End If

            Dim pts = path.PathPoints

            If pts Is Nothing OrElse pts.Length = 0 Then
                Return
            End If

            Call painting.Add(New Elements.Poly With {
                .closedPath = True,
                .fill = Nothing,
                .points = tf(pts),
                .stroke = New Stroke(pen)
            })
        End Sub

        Public Overrides Sub DrawPie(pen As Pen, rect As Rectangle, startAngle As Single, sweepAngle As Single)
            Call DrawPie(pen, New RectangleF(rect.X, rect.Y, rect.Width, rect.Height), startAngle, sweepAngle)
        End Sub

        Public Overrides Sub DrawPie(pen As Pen, rect As RectangleF, startAngle As Single, sweepAngle As Single)
            Dim r = tf(rect)

            Call painting.Add(New Elements.Pie With {
                .x = r.X,
                .y = r.Y,
                .width = r.Width,
                .height = r.Height,
                .startAngle = startAngle,
                .sweepAngle = sweepAngle,
                .fill = Nothing,
                .stroke = New Stroke(pen)
            })
        End Sub

        Public Overrides Sub DrawPie(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)
            Call DrawPie(pen, CSng(x), CSng(y), CSng(width), CSng(height), CSng(startAngle), CSng(sweepAngle))
        End Sub

        Public Overrides Sub DrawPie(pen As Pen, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)
            Call DrawPie(pen, New RectangleF(x, y, width, height), startAngle, sweepAngle)
        End Sub

        Public Overrides Sub DrawCircle(center As PointF, fill As Color, stroke As Pen, radius As Single)
            Dim c = tf(center)
            Dim r = tfRadius(radius)

            Call painting.Add(New Elements.Circle With {
                .stroke = If(stroke Is Nothing, Nothing, New Stroke(stroke)),
                .shape = New Shapes.Circle(c.OffSet2D(-r, -r), CInt(r * 2), fill)
            })
        End Sub

        Public Overrides Sub DrawPolygon(pen As Pen, points() As PointF)
            Call painting.Add(New Elements.Polygon With {
                .points = tf(points),
                .fill = Nothing,
                .stroke = New Stroke(pen)
            })
        End Sub

        Public Overrides Sub DrawPolygon(pen As Pen, points() As Point)
            Call painting.Add(New Elements.Polygon With {
                .points = tf(points.PointF.ToArray),
                .fill = Nothing,
                .stroke = New Stroke(pen)
            })
        End Sub

        Public Overrides Sub DrawRectangle(pen As Pen, rect As Rectangle)
            Dim box As New Elements.Rectangle(tf(rect), Nothing)

            box.shape.fill = ""
            box.shape.border = New Stroke(pen)
            painting.Add(box)
        End Sub

        Public Overrides Sub DrawRectangle(pen As Pen, rect As RectangleF)
            Dim box As New Elements.Rectangle(tf(rect), Nothing)

            box.shape.fill = ""
            box.shape.border = New Stroke(pen)
            painting.Add(box)
        End Sub

        Public Overrides Sub DrawRectangle(pen As Pen, x As Single, y As Single, width As Single, height As Single)
            Dim rect As New Elements.Rectangle(tf(New RectangleF(x, y, width, height)), Nothing)

            rect.shape.fill = ""
            rect.shape.border = New Stroke(pen)
            painting.Add(rect)
        End Sub

        Public Overrides Sub DrawRectangle(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer)
            Call DrawRectangle(pen, CSng(x), CSng(y), CSng(width), CSng(height))
        End Sub

        Public Overrides Sub DrawRectangles(pen As Pen, rects() As RectangleF)
            For Each rect As RectangleF In rects
                Call DrawRectangle(pen, rect)
            Next
        End Sub

        Public Overrides Sub DrawRectangles(pen As Pen, rects() As Rectangle)
            For Each rect As RectangleF In rects
                Call DrawRectangle(pen, rect)
            Next
        End Sub

        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, ByRef point As PointF)
            Call painting.Add(New Elements.Text With {
                .font = New CSSFont(font, brush),
                .location = tf(point),
                .rotation = 0,
                .text = s
            })
        End Sub

        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, layoutRectangle As RectangleF)
            Call DrawString(s, font, brush, layoutRectangle.Location)
        End Sub

        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, x As Single, y As Single)
            Call DrawString(s, font, brush, New PointF(x, y))
        End Sub

        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, ByRef x As Single, ByRef y As Single, angle As Single)
            Call painting.Add(New Elements.Text With {
                .font = New CSSFont(font, brush),
                .location = tf(New PointF(x, y)),
                .rotation = angle,
                .text = s
            })
        End Sub

        Public Overrides Sub ExcludeClip(rect As Rectangle)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillClosedCurve(brush As Brush, points() As PointF)
            Dim pts = sampleCurve(points, 0.5, closed:=True)

            Call painting.Add(New Elements.Polygon With {
                .fill = brushColor(brush),
                .points = tf(pts)
            })
        End Sub

        Public Overrides Sub FillClosedCurve(brush As Brush, points() As Point)
            Call FillClosedCurve(brush, points.PointF.ToArray)
        End Sub

        Public Overrides Sub FillEllipse(brush As Brush, rect As Rectangle)
            Call FillEllipse(brush, New RectangleF(rect.X, rect.Y, rect.Width, rect.Height))
        End Sub

        Public Overrides Sub FillEllipse(brush As Brush, rect As RectangleF)
            Call FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height)
        End Sub

        Public Overrides Sub FillEllipse(brush As Brush, x As Single, y As Single, width As Single, height As Single)
            Dim r = tf(New RectangleF(x, y, width, height))

            Call painting.Add(New Elements.Pie With {
                .x = r.X,
                .y = r.Y,
                .width = r.Width,
                .height = r.Height,
                .startAngle = 0,
                .sweepAngle = 360,
                .fill = brushColor(brush),
                .stroke = Nothing
            })
        End Sub

        Public Overrides Sub FillEllipse(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer)
            Call FillEllipse(brush, CSng(x), CSng(y), CSng(width), CSng(height))
        End Sub

        Public Overrides Sub FillPath(brush As Brush, path As GraphicsPath)
            If path Is Nothing Then
                Return
            End If

            Dim pts = path.PathPoints

            If pts Is Nothing OrElse pts.Length = 0 Then
                Return
            End If

            Call painting.Add(New Elements.Polygon With {
                .fill = brushColor(brush),
                .points = tf(pts)
            })
        End Sub

        Public Overrides Sub FillPie(brush As Brush, rect As Rectangle, startAngle As Single, sweepAngle As Single)
            Call FillPie(brush, CSng(rect.X), CSng(rect.Y), CSng(rect.Width), CSng(rect.Height), startAngle, sweepAngle)
        End Sub

        Public Overrides Sub FillPie(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)
            Call FillPie(brush, CSng(x), CSng(y), CSng(width), CSng(height), CSng(startAngle), CSng(sweepAngle))
        End Sub

        Public Overrides Sub FillPie(brush As Brush, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)
            Dim r = tf(New RectangleF(x, y, width, height))

            Call painting.Add(New Elements.Pie With {
                .height = r.Height,
                .startAngle = startAngle,
                .sweepAngle = sweepAngle,
                .width = r.Width,
                .y = r.Y,
                .x = r.X,
                .fill = brushColor(brush),
                .stroke = Nothing
            })
        End Sub

        Public Overrides Sub FillPolygon(brush As Brush, points() As Point)
            Call painting.Add(New Elements.Polygon With {
                .fill = brushColor(brush),
                .points = tf(points.PointF.ToArray)
            })
        End Sub

        Public Overrides Sub FillPolygon(brush As Brush, points() As PointF)
            Call painting.Add(New Elements.Polygon With {
                .fill = brushColor(brush),
                .points = tf(points)
            })
        End Sub

        Public Overrides Sub FillRectangle(brush As Brush, rect As Rectangle)
            Call painting.Add(New Elements.Rectangle(tf(rect), DirectCast(brush, SolidBrush).Color))
        End Sub

        Public Overrides Sub FillRectangle(brush As Brush, rect As RectangleF)
            Call painting.Add(New Elements.Rectangle(tf(rect), DirectCast(brush, SolidBrush).Color))
        End Sub

        Public Overrides Sub FillRectangle(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer)
            Call FillRectangle(brush, New Rectangle(x, y, width, height))
        End Sub

        Public Overrides Sub FillRectangle(brush As Brush, x As Single, y As Single, width As Single, height As Single)
            Call FillRectangle(brush, New RectangleF(x, y, width, height))
        End Sub

        Public Overrides Sub IntersectClip(rect As RectangleF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub IntersectClip(rect As Rectangle)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub ResetClip()
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub ResetTransform()
            Call ctm.Reset()
        End Sub

        Public Overrides Sub RotateTransform(angle As Single)
            Call ctm.Rotate(angle)
        End Sub

        Public Overrides Sub ScaleTransform(sx As Single, sy As Single)
            Call ctm.Scale(sx, sy)
        End Sub

        Public Overrides Sub SetClip(rect As RectangleF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub SetClip(rect As Rectangle)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub TranslateClip(dx As Single, dy As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub TranslateClip(dx As Integer, dy As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub TranslateTransform(dx As Single, dy As Single)
            Call ctm.Translate(dx, dy)
        End Sub

        ''' <summary>
        ''' get the current postscript canvas context information
        ''' </summary>
        ''' <returns>An object in clr type <see cref="PostScriptBuilder"/>.</returns>
        Public Overrides Function GetContextInfo() As Object
            Return painting
        End Function

        Public Overrides Function IsVisible(rect As Rectangle) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function IsVisible(rect As RectangleF) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function IsVisible(x As Integer, y As Integer, width As Integer, height As Integer) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function IsVisible(x As Single, y As Single, width As Single, height As Single) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MeasureString(text As String, font As Font) As SizeF
            Return DriverLoad.MeasureTextSize(text, font)
        End Function

        Public Overrides Function MeasureString(text As String, font As Font, width As Integer) As SizeF
            Return DriverLoad.MeasureTextSize(text, font)
        End Function

        Public Overrides Function MeasureString(text As String, font As Font, layoutArea As SizeF) As SizeF
            Return DriverLoad.MeasureTextSize(text, font)
        End Function

        Public Overrides Function GetStringPath(s As String, rect As RectangleF, font As Font) As GraphicsPath
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' do nothing on flush
        ''' </summary>
        Public Overrides Sub Flush()
        End Sub
    End Class
End Namespace
