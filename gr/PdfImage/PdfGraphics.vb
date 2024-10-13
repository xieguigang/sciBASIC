#Region "Microsoft.VisualBasic::a99a1285927ab7aed534d6db57f3d343, gr\PdfImage\PdfGraphics.vb"

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

    '   Total Lines: 537
    '    Code Lines: 408 (75.98%)
    ' Comment Lines: 6 (1.12%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 123 (22.91%)
    '     File Size: 20.09 KB


    ' Class PdfGraphics
    ' 
    '     Properties: PageScale, RenderingOrigin, TextContrast
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: GetContextInfo, GetNearestColor, (+4 Overloads) IsVisible, Save
    ' 
    '     Sub: AddMetafileComment, ClearCanvas, Dispose, (+4 Overloads) DrawArc, (+3 Overloads) DrawBezier
    '          (+2 Overloads) DrawBeziers, DrawCircle, (+2 Overloads) DrawClosedCurve, (+7 Overloads) DrawCurve, (+4 Overloads) DrawEllipse
    '          (+10 Overloads) DrawImage, (+4 Overloads) DrawImageUnscaled, DrawImageUnscaledAndClipped, (+4 Overloads) DrawLine, (+2 Overloads) DrawLines
    '          DrawPath, (+4 Overloads) DrawPie, (+2 Overloads) DrawPolygon, (+4 Overloads) DrawRectangle, (+2 Overloads) DrawRectangles
    '          (+4 Overloads) DrawString, ExcludeClip, (+2 Overloads) FillClosedCurve, (+4 Overloads) FillEllipse, FillPath
    '          (+3 Overloads) FillPie, (+2 Overloads) FillPolygon, (+4 Overloads) FillRectangle, Flush, (+2 Overloads) IntersectClip
    '          ResetClip, ResetTransform, RotateTransform, ScaleTransform, (+2 Overloads) SetClip
    '          (+2 Overloads) TranslateClip, TranslateTransform
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Drawing.Text
Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.application.pdf
Imports Microsoft.VisualBasic.MIME.Html.CSS

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
Imports FontStyle = System.Drawing.FontStyle
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
#End If

Public Class PdfGraphics : Inherits IGraphics
    Implements SaveGdiBitmap

    Friend ReadOnly g As PdfContents
    Friend ReadOnly page As PdfPage
    Friend ReadOnly buffer As Stream

    ''' <summary>
    ''' 在PDF文件之中的坐标系是与gdi的绘图坐标系是颠倒过来的
    ''' 所以会需要通过这个height值来进行颠倒修正
    ''' </summary>
    ReadOnly height As Integer

    Sub New(size As Size, page As PdfPage, buffer As Stream)
        Call MyBase.New(300)

        Me.buffer = buffer
        Me.page = page
        Me.g = New PdfContents(page)
        Me.height = size.Height
    End Sub

    Public Overrides Property PageScale As Single
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As Single)
            Throw New NotImplementedException()
        End Set
    End Property

    Public Overrides Property RenderingOrigin As Point
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As Point)
            Throw New NotImplementedException()
        End Set
    End Property

    Public Overrides Property TextContrast As Integer
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As Integer)
            Throw New NotImplementedException()
        End Set
    End Property

    Public Overrides Sub AddMetafileComment(data() As Byte)
        Throw New NotImplementedException()
    End Sub

    Protected Overrides Sub ClearCanvas(color As Color)
        Call g.SetColorNonStroking(color)
        Call g.DrawRectangle(New PointD(0, 0), New SizeD(Size.Width, Size.Height), PaintOp.Fill)
    End Sub

    Public Overrides Sub Dispose()
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawArc(pen As Pen, rect As RectangleF, startAngle As Single, sweepAngle As Single)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawArc(pen As Pen, rect As Rectangle, startAngle As Single, sweepAngle As Single)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawArc(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawArc(pen As Pen, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)
        Throw New NotImplementedException()
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
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawClosedCurve(pen As Pen, points() As PointF)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawCurve(pen As Pen, points() As Point)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawCurve(pen As Pen, points() As PointF)
        Throw New NotImplementedException()
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
        DrawImage(image, New Rectangle(point, image.Size))
    End Sub

    Public Overrides Sub DrawImage(image As Image, destPoints() As Point)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawImage(image As Image, destPoints() As PointF)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawImage(image As Image, rect As Rectangle)
        Dim element As New MIME.application.pdf.PdfImage(page.Document)

        Call g.SaveGraphicsState()
        Call g.MoveTo(New PointD(0, 0))
        Call element.LoadImage(image)
        Call g.DrawImage(element, rect.X, rect.Y, rect.Width)
    End Sub

    Public Overrides Sub DrawImage(image As Image, point As PointF)
        DrawImage(image, New Rectangle(point.ToPoint, image.Size))
    End Sub

    Public Overrides Sub DrawImage(image As Image, rect As RectangleF)
        DrawImage(image, New Rectangle(rect.X, rect.Y, rect.Width, rect.Height))
    End Sub

    Public Overrides Sub DrawImage(image As Image, x As Integer, y As Integer)
        DrawImage(image, New Rectangle(x, y, image.Width, image.Height))
    End Sub

    Public Overrides Sub DrawImage(image As Image, x As Single, y As Single)
        DrawImage(image, New Rectangle(x, y, image.Width, image.Height))
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
        Call DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y)
    End Sub

    Public Overrides Sub DrawLine(pen As Pen, pt1 As Point, pt2 As Point)
        Call DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y)
    End Sub

    Public Overrides Sub DrawLine(pen As Pen, x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer)
        Call DrawLine(pen, CSng(x1), CSng(y1), CSng(x2), CSng(y2))
    End Sub

    Public Overrides Sub DrawLine(pen As Pen, x1 As Single, y1 As Single, x2 As Single, y2 As Single)
        g.DrawLine(x1, height - y1, x2, height - y2, New PdfTableBorderStyle(pen))
    End Sub

    Public Overrides Sub DrawLines(pen As Pen, points() As PointF)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawLines(pen As Pen, points() As Point)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawPath(pen As Pen, path As GraphicsPath)
        Dim allPoints = path.PathPoints

        g.SetColorNonStroking(pen.Color)

        For Each line In allPoints.CreateSlideWindows(winSize:=2)
            Call DrawLine(pen, line.First, line.Last)
        Next

        g.SetPaintOp(PaintOp.Stroke)
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
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawPolygon(pen As Pen, points() As PointF)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawPolygon(pen As Pen, points() As Point)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawRectangle(pen As Pen, rect As Rectangle)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawRectangle(pen As Pen, rect As RectangleF)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawRectangle(pen As Pen, x As Single, y As Single, width As Single, height As Single)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawRectangle(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawRectangles(pen As Pen, rects() As RectangleF)
        For Each rect In rects
            Call DrawRectangle(pen, rect)
        Next
    End Sub

    Public Overrides Sub DrawRectangles(pen As Pen, rects() As Rectangle)
        For Each rect In rects
            Call DrawRectangle(pen, rect)
        Next
    End Sub

    Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, ByRef point As PointF)
        Call DrawString(s, font, brush, point.X, point.Y)
    End Sub

    Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, layoutRectangle As RectangleF)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, x As Single, y As Single)
        Dim pdfFont As PdfFont = PdfFont.CreatePdfFont(page.Document, font.Name, font.Style)
        Dim color As Color = DirectCast(brush, SolidBrush).Color

        Call g.SetColorNonStroking(color)
        Call g.DrawText(pdfFont, font.Size, x, height - y, s)
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
        Call FillEllipse(brush, New RectangleF(rect.X, rect.Y, rect.Width, rect.Height))
    End Sub

    Public Overrides Sub FillEllipse(brush As Brush, rect As RectangleF)
        Dim x As Double = rect.X + rect.Width / 2
        Dim y As Double = Me.height - (rect.Y + rect.Height / 2)

        g.SetColorNonStroking(DirectCast(brush, SolidBrush).Color)
        g.DrawOval(x, y, rect.Width, rect.Height, PaintOp.Fill)
    End Sub

    Public Overrides Sub FillEllipse(brush As Brush, x As Single, y As Single, width As Single, height As Single)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub FillEllipse(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub FillPath(brush As Brush, path As GraphicsPath)
        Dim allPoints = path.PathPoints
        Dim pen As New Pen(brush, 1)

        g.SetColorNonStroking(DirectCast(brush, SolidBrush).Color)

        For Each line In allPoints.CreateSlideWindows(winSize:=2)
            Call DrawLine(pen, line.First, line.Last)
        Next

        g.SetPaintOp(PaintOp.Fill)
    End Sub

    Public Overrides Sub FillPie(brush As Brush, rect As Rectangle, startAngle As Single, sweepAngle As Single)
        Dim x As Double = rect.X + rect.Width / 2
        Dim y As Double = Me.height - (rect.Y + rect.Height / 2)

        g.SetColorNonStroking(DirectCast(brush, SolidBrush).Color)
        g.DrawOval(x, y, rect.Width, rect.Height, PaintOp.Fill)
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
        Call FillRectangle(brush, New RectangleF(rect.X, rect.Y, rect.Width, rect.Height))
    End Sub

    Public Overrides Sub FillRectangle(brush As Brush, rect As RectangleF)
        Dim y As Double = Me.height - rect.Y

        Call g.MoveTo(rect.X, y)

        ' change nonstroking (fill) color to yellow
        Call g.SetColorNonStroking(DirectCast(brush, SolidBrush).Color)
        Call g.DrawRectangle(rect.X, y, rect.Width, rect.Height, PaintOp.Fill)
    End Sub

    Public Overrides Sub FillRectangle(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub FillRectangle(brush As Brush, x As Single, y As Single, width As Single, height As Single)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub Flush()
        ' save graphics state
        g.SaveGraphicsState()
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

    Public Overrides Function GetContextInfo() As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function GetNearestColor(color As Color) As Color
        Throw New NotImplementedException()
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

    Public Function Save(stream As Stream, format As ImageFormats) As Boolean Implements SaveGdiBitmap.Save
        Dim pdf As New PdfImage(Me, Size, New Padding)

        Call pdf.Save(stream)
        Call stream.Flush()

        Return True
    End Function

    Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, ByRef x As Single, ByRef y As Single, angle As Single)
        Throw New NotImplementedException
    End Sub
End Class
