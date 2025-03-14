#Region "Microsoft.VisualBasic::2d129aa4ec27c6f433ef2c8466ed0e4e, gr\Microsoft.VisualBasic.Imaging\PostScript\GraphicsPostScript.vb"

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

    '   Total Lines: 520
    '    Code Lines: 396 (76.15%)
    ' Comment Lines: 13 (2.50%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 111 (21.35%)
    '     File Size: 21.00 KB


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

        Public Overrides Sub DrawArc(pen As Pen, rect As RectangleF, startAngle As Single, sweepAngle As Single)
            Call DrawArc(pen, (rect.X), (rect.Y), (rect.Width), (rect.Height), startAngle, sweepAngle)
        End Sub

        Public Overrides Sub DrawArc(pen As Pen, rect As Rectangle, startAngle As Single, sweepAngle As Single)
            Call DrawArc(pen, CSng(rect.X), CSng(rect.Y), CSng(rect.Width), CSng(rect.Height), startAngle, sweepAngle)
        End Sub

        Public Overrides Sub DrawArc(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)
            Call painting.Add(New Elements.Arc With {
                .height = height,
                .startAngle = startAngle,
                .stroke = New Stroke(pen),
                .sweepAngle = sweepAngle,
                .width = width,
                .x = x,
                .y = y
            })
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub DrawArc(pen As Pen, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)
            Call painting.Add(New Elements.Arc With {
                .height = height,
                .startAngle = startAngle,
                .stroke = New Stroke(pen),
                .sweepAngle = sweepAngle,
                .width = width,
                .x = x,
                .y = y
            })
        End Sub

        Public Overrides Sub DrawBezier(pen As Pen, pt1 As Point, pt2 As Point, pt3 As Point, pt4 As Point)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawBezier(pen As Pen, pt1 As PointF, pt2 As PointF, pt3 As PointF, pt4 As PointF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawBezier(pen As Pen, x1 As Single, y1 As Single, x2 As Single, y2 As Single, x3 As Single, y3 As Single, x4 As Single, y4 As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawBeziers(pen As Pen, points() As PointF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawBeziers(pen As Pen, points() As Point)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawClosedCurve(pen As Pen, points() As Point)
            Call DrawClosedCurve(pen, points.PointF.ToArray)
        End Sub

        Public Overrides Sub DrawClosedCurve(pen As Pen, points() As PointF)
            Call painting.Add(New Elements.Poly With {
                .closedPath = True,
                .fill = Nothing,
                .points = points,
                .stroke = New Stroke(pen)
            })
        End Sub

        Public Overrides Sub DrawCurve(pen As Pen, points() As Point)
            Call DrawCurve(pen, points.PointF.ToArray)
        End Sub

        Public Overrides Sub DrawCurve(pen As Pen, points() As PointF)
            Call painting.Add(New Elements.Poly With {
                .closedPath = False,
                .fill = Nothing,
                .points = points,
                .stroke = New Stroke(pen)
            })
        End Sub

        Public Overrides Sub DrawCurve(pen As Pen, points() As PointF, tension As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawCurve(pen As Pen, points() As Point, tension As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawCurve(pen As Pen, points() As PointF, offset As Integer, numberOfSegments As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawCurve(pen As Pen, points() As PointF, offset As Integer, numberOfSegments As Integer, tension As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawCurve(pen As Pen, points() As Point, offset As Integer, numberOfSegments As Integer, tension As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawEllipse(pen As Pen, rect As Rectangle)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawEllipse(pen As Pen, rect As RectangleF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawEllipse(pen As Pen, x As Single, y As Single, width As Single, height As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawEllipse(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, point As Point)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destPoints() As Point)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destPoints() As PointF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, rect As Rectangle)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, point As PointF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, rect As RectangleF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, x As Integer, y As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, x As Single, y As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, x As Single, y As Single, width As Single, height As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, x As Integer, y As Integer, width As Integer, height As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImageUnscaled(image As Image, rect As Rectangle)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImageUnscaled(image As Image, point As Point)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImageUnscaled(image As Image, x As Integer, y As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImageUnscaled(image As Image, x As Integer, y As Integer, width As Integer, height As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImageUnscaledAndClipped(image As Image, rect As Rectangle)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawLine(pen As Pen, pt1 As PointF, pt2 As PointF)
            Call painting.Add(New Elements.Line(pen, pt1, pt2))
        End Sub

        Public Overrides Sub DrawLine(pen As Pen, pt1 As Point, pt2 As Point)
            Call painting.Add(New Elements.Line(pen, pt1.PointF, pt2.PointF))
        End Sub

        Public Overrides Sub DrawLine(pen As Pen, x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer)
            Call painting.Add(New Elements.Line(pen, New PointF(x1, y1), New PointF(x2, y2)))
        End Sub

        Public Overrides Sub DrawLine(pen As Pen, x1 As Single, y1 As Single, x2 As Single, y2 As Single)
            Call painting.Add(New Elements.Line(pen, New PointF(x1, y1), New PointF(x2, y2)))
        End Sub

        Public Overrides Sub DrawLines(pen As Pen, points() As PointF)
            For Each ab As SlideWindow(Of PointF) In points.SlideWindows(2)
                Call painting.Add(New Elements.Line(pen, ab(0), ab(1)))
            Next
        End Sub

        Public Overrides Sub DrawLines(pen As Pen, points() As Point)
            For Each ab As SlideWindow(Of Point) In points.SlideWindows(2)
                Call painting.Add(New Elements.Line(pen, ab(0), ab(1)))
            Next
        End Sub

        Public Overrides Sub DrawPath(pen As Pen, path As GraphicsPath)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawPie(pen As Pen, rect As Rectangle, startAngle As Single, sweepAngle As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawPie(pen As Pen, rect As RectangleF, startAngle As Single, sweepAngle As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawPie(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawPie(pen As Pen, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawCircle(center As PointF, fill As Color, stroke As Pen, radius As Single)
            Call painting.Add(New Elements.Circle With {
                .stroke = New Stroke(stroke),
                .shape = New Shapes.Circle(center.OffSet2D(-radius, -radius), radius * 2, fill)
            })
        End Sub

        Public Overrides Sub DrawPolygon(pen As Pen, points() As PointF)
            Call painting.Add(New Elements.Polygon With {
                .points = points,
                .fill = Nothing,
                .stroke = New Stroke(pen)
            })
        End Sub

        Public Overrides Sub DrawPolygon(pen As Pen, points() As Point)
            Call painting.Add(New Elements.Polygon With {
                .points = points.PointF.ToArray,
                .fill = Nothing,
                .stroke = New Stroke(pen)
            })
        End Sub

        Public Overrides Sub DrawRectangle(pen As Pen, rect As Rectangle)
            Dim box As New Elements.Rectangle(rect, Nothing)

            box.shape.border = New Stroke(pen)
            painting.Add(box)
        End Sub

        Public Overrides Sub DrawRectangle(pen As Pen, rect As RectangleF)
            Dim box As New Elements.Rectangle(rect, Nothing)

            box.shape.border = New Stroke(pen)
            painting.Add(box)
        End Sub

        Public Overrides Sub DrawRectangle(pen As Pen, x As Single, y As Single, width As Single, height As Single)
            Dim rect As New Elements.Rectangle(New RectangleF(x, y, width, height), Nothing)

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
                .location = point,
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

        Public Overrides Sub ExcludeClip(rect As Rectangle)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillClosedCurve(brush As Brush, points() As PointF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillClosedCurve(brush As Brush, points() As Point)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillEllipse(brush As Brush, rect As Rectangle)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillEllipse(brush As Brush, rect As RectangleF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillEllipse(brush As Brush, x As Single, y As Single, width As Single, height As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillEllipse(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillPath(brush As Brush, path As GraphicsPath)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillPie(brush As Brush, rect As Rectangle, startAngle As Single, sweepAngle As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillPie(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillPie(brush As Brush, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillPolygon(brush As Brush, points() As Point)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillPolygon(brush As Brush, points() As PointF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillRectangle(brush As Brush, rect As Rectangle)
            Call painting.Add(New Elements.Rectangle(rect, DirectCast(brush, SolidBrush).Color))
        End Sub

        Public Overrides Sub FillRectangle(brush As Brush, rect As RectangleF)
            Call painting.Add(New Elements.Rectangle(rect, DirectCast(brush, SolidBrush).Color))
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
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub RotateTransform(angle As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub ScaleTransform(sx As Single, sy As Single)
            Throw New NotImplementedException()
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
            Throw New NotImplementedException()
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

        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, ByRef x As Single, ByRef y As Single, angle As Single)
            Call painting.Add(New Elements.Text With {
                .font = New CSSFont(font, brush),
                .location = New PointF(x, y),
                .rotation = angle,
                .text = s
            })
        End Sub

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
