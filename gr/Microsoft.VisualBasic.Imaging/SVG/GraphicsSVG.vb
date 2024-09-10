#Region "Microsoft.VisualBasic::d66d90077fbbdbf518144147b01d48f6, gr\Microsoft.VisualBasic.Imaging\SVG\GraphicsSVG.vb"

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

    '   Total Lines: 1029
    '    Code Lines: 753 (73.18%)
    ' Comment Lines: 32 (3.11%)
    '    - Xml Docs: 78.12%
    ' 
    '   Blank Lines: 244 (23.71%)
    '     File Size: 47.72 KB


    '     Class GraphicsSVG
    ' 
    '         Properties: Size
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: (+3 Overloads) BeginContainer, GetContextInfo, GetNearestColor, (+4 Overloads) IsVisible, MeasureString
    '                   Save
    ' 
    '         Sub: AddMetafileComment, ClearCanvas, (+4 Overloads) CopyFromScreen, Dispose, (+4 Overloads) DrawArc
    '              (+3 Overloads) DrawBezier, (+2 Overloads) DrawBeziers, DrawCircle, (+4 Overloads) DrawClosedCurve, (+7 Overloads) DrawCurve
    '              (+4 Overloads) DrawEllipse, (+2 Overloads) DrawIcon, DrawIconUnstretched, (+30 Overloads) DrawImage, (+4 Overloads) DrawImageUnscaled
    '              DrawImageUnscaledAndClipped, (+4 Overloads) DrawLine, (+2 Overloads) DrawLines, DrawPath, (+4 Overloads) DrawPie
    '              (+2 Overloads) DrawPolygon, (+6 Overloads) DrawRectangle, (+2 Overloads) DrawRectangles, (+7 Overloads) DrawString, EndContainer
    '              (+36 Overloads) EnumerateMetafile, (+2 Overloads) ExcludeClip, (+6 Overloads) FillClosedCurve, (+4 Overloads) FillEllipse, FillPath
    '              (+3 Overloads) FillPie, (+4 Overloads) FillPolygon, (+4 Overloads) FillRectangle, FillRegion, (+2 Overloads) Flush
    '              (+3 Overloads) IntersectClip, (+2 Overloads) MultiplyTransform, ResetClip, ResetTransform, (+2 Overloads) RotateTransform
    '              (+2 Overloads) ScaleTransform, (+9 Overloads) SetClip, (+2 Overloads) TransformPoints, (+2 Overloads) TranslateClip, (+2 Overloads) TranslateTransform
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.SVG.PathHelper
Imports Microsoft.VisualBasic.Imaging.SVG.XML
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Net.Http

Namespace SVG

    ''' <summary>
    ''' SVG graphics generator
    ''' </summary>
    Public Class GraphicsSVG : Inherits MockGDIPlusGraphics
        Implements SaveGdiBitmap

        ''' <summary>
        ''' SVG图型的数据结构以及渲染是树形的，但是利用程序代码进行SVG数据的生成却是线性的
        ''' 这样子会导致产生的SVG图形错位
        ''' 所以在这里需要使用多层结构来将线性的绘图操作模拟为SVG的树形结构
        ''' </summary>
        Friend ReadOnly __svgData As SVGDataLayers

        Public Overrides ReadOnly Property Size As Size
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return __svgData.size
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(size As SizeF, dpiX As Integer, dpiY As Integer)
            Call Me.New(size.Width, size.Height, dpiX, dpiY)
        End Sub

        Public Sub New(size As Size, dpiX As Integer, dpiY As Integer)
            Call MyBase.New(size, dpiX, dpiY)
            __svgData = New SVGDataLayers(size)
        End Sub

        Friend Sub New(svg As SVGDataLayers, dpiX As Integer, dpiY As Integer)
            Call Me.New(svg.size, dpiX, dpiY)
            __svgData = svg
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(width%, height%, dpiX As Integer, dpiY As Integer)
            Call Me.New(New Size(width, height), dpiX, dpiY)
        End Sub

        ''' <summary>
        ''' add comment to svg xml document
        ''' </summary>
        ''' <param name="data"></param>
        Public Overrides Sub AddMetafileComment(data() As Byte)
            Dim meta As String = data.ToBase64String

            Throw New NotImplementedException(meta)
        End Sub

        ''' <summary>
        ''' This function will clear entire svg document contents.
        ''' </summary>
        ''' <param name="color"></param>
        Protected Overrides Sub ClearCanvas(color As Color)
            __svgData.bg$ = color.ToHtmlColor
            __svgData.Clear()

            Call FillRectangle(New SolidBrush(color), New Rectangle(New Point, Size))
        End Sub

#Region "NotSupportedException"

        Public Overrides Sub CopyFromScreen(upperLeftSource As Point, upperLeftDestination As Point, blockRegionSize As Size)
            Throw New NotSupportedException
        End Sub

        Public Overrides Sub CopyFromScreen(upperLeftSource As Point, upperLeftDestination As Point, blockRegionSize As Size, copyPixelOperation As CopyPixelOperation)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub CopyFromScreen(sourceX As Integer, sourceY As Integer, destinationX As Integer, destinationY As Integer, blockRegionSize As Size)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub CopyFromScreen(sourceX As Integer, sourceY As Integer, destinationX As Integer, destinationY As Integer, blockRegionSize As Size, copyPixelOperation As CopyPixelOperation)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EndContainer(container As GraphicsContainer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, callback As Graphics.EnumerateMetafileProc)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, callback As Graphics.EnumerateMetafileProc)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, callback As Graphics.EnumerateMetafileProc)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, callback As Graphics.EnumerateMetafileProc)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, callback As Graphics.EnumerateMetafileProc)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, callback As Graphics.EnumerateMetafileProc)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, srcRect As Rectangle, unit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, srcRect As RectangleF, unit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, srcRect As RectangleF, unit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, srcRect As Rectangle, unit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, srcRect As RectangleF, unit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, srcRect As Rectangle, unit As GraphicsUnit, callback As Graphics.EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub ExcludeClip(rect As Rectangle)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub ExcludeClip(region As Region)
            Throw New NotImplementedException()
        End Sub

#End Region

#Region "向SVG之中嵌入图片图像数据"

        Public Overrides Sub DrawIcon(icon As Icon, targetRect As Rectangle)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawIcon(icon As Icon, x As Integer, y As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawIconUnstretched(icon As Icon, targetRect As Rectangle)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, point As Point)
            DrawImage(image, New RectangleF(point.PointF, image.Size))
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destPoints() As Point)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destPoints() As PointF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, rect As Rectangle)
            DrawImage(image, CType(rect, RectangleF))
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, point As PointF)
            DrawImage(image, New RectangleF(point, image.Size))
        End Sub

        Public Overrides Sub DrawImage(image As Image, rect As RectangleF)
            Dim point As PointF = rect.Location
            Dim img As SvgImage = __svgData.svg.AddImage

            Call img.SetImage(image)
            Call img.SetRectangle(rect)
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, x As Integer, y As Integer)
            DrawImage(image, CSng(x), CSng(y))
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, x As Single, y As Single)
            Dim size = image.Size
            DrawImage(image, New RectangleF(x, y, size.Width, size.Height))
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destRect As RectangleF, srcRect As RectangleF, srcUnit As GraphicsUnit)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destRect As Rectangle, srcRect As Rectangle, srcUnit As GraphicsUnit)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, x As Single, y As Single, width As Single, height As Single)
            DrawImage(image, New RectangleF(x, y, width, height))
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, x As Integer, y As Integer, width As Integer, height As Integer)
            DrawImage(image, New RectangleF(x, y, width, height))
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, x As Single, y As Single, srcRect As RectangleF, srcUnit As GraphicsUnit)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, x As Integer, y As Integer, srcRect As Rectangle, srcUnit As GraphicsUnit)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As Graphics.DrawImageAbort)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As Graphics.DrawImageAbort)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As Graphics.DrawImageAbort, callbackData As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As Graphics.DrawImageAbort, callbackData As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As Graphics.DrawImageAbort)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As Graphics.DrawImageAbort)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As Graphics.DrawImageAbort, callbackData As IntPtr)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Drawing.Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As Graphics.DrawImageAbort, callbackData As IntPtr)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImageUnscaled(image As Drawing.Image, rect As Rectangle)
            Dim img As New ImageData(image, image.Size, New Padding)
            Call Me.DrawImageUnscaled(img, rect)
        End Sub

        Public Overrides Sub DrawImageUnscaled(image As Drawing.Image, point As Point)
            Dim img As New ImageData(image, image.Size, New Padding)
            Call Me.DrawImageUnscaled(img, point)
        End Sub

        Public Overrides Sub DrawImageUnscaled(image As Drawing.Image, x As Integer, y As Integer)
            Dim img As New ImageData(image, image.Size, New Padding)
            Call Me.DrawImageUnscaled(img, x, y)
        End Sub

        Public Overrides Sub DrawImageUnscaled(image As Drawing.Image, x As Integer, y As Integer, width As Integer, height As Integer)
            Dim img As New ImageData(image, image.Size, New Padding)
            Call Me.DrawImageUnscaled(img, x, y, width, height)
        End Sub

        Public Overrides Sub DrawImageUnscaledAndClipped(image As Drawing.Image, rect As Rectangle)
            Dim img As New ImageData(image, image.Size, New Padding)
            Call Me.DrawImageUnscaledAndClipped(img, rect)
        End Sub
#End Region

#Region "矢量图绘制方法"

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

        Public Overrides Sub DrawClosedCurve(pen As Pen, points() As Point, tension As Single, fillmode As FillMode)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawClosedCurve(pen As Pen, points() As PointF, tension As Single, fillmode As FillMode)
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

        End Sub

        Public Overrides Sub DrawEllipse(pen As Pen, rect As RectangleF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawEllipse(pen As Pen, x As Single, y As Single, width As Single, height As Single)

        End Sub

        Public Overrides Sub DrawEllipse(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer)
            Throw New NotImplementedException()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub DrawLine(pen As Pen, pt1 As PointF, pt2 As PointF)
            DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub DrawLine(pen As Pen, pt1 As Point, pt2 As Point)
            DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub DrawLine(pen As Pen, x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer)
            DrawLine(pen, x1:=CSng(x1), x2:=CSng(x2), y1:=CSng(y1), y2:=CSng(y2))
        End Sub

        Public Overrides Sub DrawLine(pen As Pen, x1 As Single, y1 As Single, x2 As Single, y2 As Single)
            Dim line As SvgLine = __svgData.svg.AddLine

            line.SetPoint(x1, y1, x2, y2)
            line.Style = New Stroke(pen).CSSValue

            If pen.DashStyle <> DashStyle.Solid Then
                line.StrokeDashArray = New Double() {8, 4}
            End If

            Try
                If TypeOf pen.CustomEndCap Is AdjustableArrowCap Then
                    ' draw arrow on line end
                    Dim defs As SvgDefs = __svgData.svg.defs
                    Dim refId As String = $"M{line.GetHashCode}"
                    Dim gdiArrow As AdjustableArrowCap = pen.CustomEndCap
                    Dim marker As SvgMarker = defs.CreateMarker(refId, gdiArrow.Width, gdiArrow.Height)

                    line.MarkerEnd = $"url(#{refId})"
                End If
            Catch ex As Exception
                ' error maybe happends when the custom end cap has not been setup
                ' just ignores of this error
            End Try
        End Sub

        Public Overrides Sub DrawLines(pen As Pen, points() As PointF)
            For Each pt In points.SlideWindows(2)
                DrawLine(pen, pt(0), pt(1))
            Next
        End Sub

        Public Overrides Sub DrawLines(pen As Pen, points() As Point)
            For Each pt In points.SlideWindows(2)
                DrawLine(pen, pt(0), pt(1))
            Next
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="pen"></param>
        ''' <param name="path"></param>
        ''' <remarks>
        ''' 20220421  因为在svg没有设置默认底色的情况下，fill默认是黑色的，这个就会导致
        ''' path绘制的结果是黑色的多边形，所以在这个函数没有办法传递底色的情况下，自动添加
        ''' transparent填充底色来解决这个填充色为黑色默认值的问题
        ''' </remarks>
        Public Overrides Sub DrawPath(pen As Pen, path As GraphicsPath)
            Dim pathData As SvgPath = __svgData.svg.AddPath

            pathData.D = path.SVGPathData
            pathData.Style = $"fill: transparent; {New Stroke(pen).CSSValue}"
        End Sub

        Public Overrides Sub DrawPie(pen As Pen, rect As Rectangle, startAngle As Single, sweepAngle As Single)
            With rect
                Call DrawPie(pen, .X, .Y, .Width, .Height, startAngle, sweepAngle)
            End With
        End Sub

        Public Overrides Sub DrawPie(pen As Pen, rect As RectangleF, startAngle As Single, sweepAngle As Single)
            With rect
                Call DrawPie(pen, .X, .Y, .Width, .Height, startAngle, sweepAngle)
            End With
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub DrawPie(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)
            Call DrawPie(pen, x, y, width, height, CSng(startAngle), CSng(sweepAngle))
        End Sub

        Public Overrides Sub DrawPie(pen As Pen, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)
            Dim path As String = ModelBuilder.PiePath(x, y, width, height, startAngle, sweepAngle)
            Dim data As SvgPath = __svgData.svg.AddPath

            data.D = path
            data.Style = New Stroke(pen).CSSValue
        End Sub

        Public Overrides Sub DrawPolygon(pen As Pen, points() As PointF)
            Dim polygon As SvgPolygon = __svgData.svg.AddPolygon

            polygon.SetPolygonPath(points)
            polygon.Style = New Stroke(pen).CSSValue
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub DrawPolygon(pen As Pen, points() As Point)
            DrawPolygon(pen, points.Select(Function(pt) pt.PointF).ToArray)
        End Sub

        Public Overrides Sub DrawRectangle(pen As Pen, rect As Rectangle)
            Dim rectangle As SvgRect = __svgData.svg.AddRect
            Dim fill As String = __svgData.bg$

            rectangle.Style = New Stroke(pen).CSSValue & $" fill:{fill};"
            rectangle.Fill = fill
            rectangle.SetRectangle(rect)
        End Sub

        Public Overloads Sub DrawRectangle(pen As Pen, rect As Rectangle, fill As Color)
            Dim rectangle As SvgRect = __svgData.svg.AddRect

            rectangle.SetRectangle(rect)
            rectangle.Style = {New Stroke(pen).CSSValue, $"fill: {fill.ToHtmlColor}"}.JoinBy("; ")
        End Sub

        Public Overloads Sub DrawRectangle(pen As Pen, rect As RectangleF, fill As Color)
            Dim rectangle As SvgRect = __svgData.svg.AddRect

            rectangle.SetRectangle(rect)
            rectangle.Style = {New Stroke(pen).CSSValue, $"fill: {fill.ToHtmlColor}"}.JoinBy("; ")
        End Sub

        Public Overrides Sub DrawRectangle(pen As Pen, x As Single, y As Single, width As Single, height As Single)
            Dim rectangle As SvgRect = __svgData.svg.AddRect

            rectangle.SetRectangle(New PointF(x, y), New SizeF(width, height))
            rectangle.Style = New Stroke(pen).CSSValue & "; fill: transparent;"
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub DrawRectangle(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer)
            DrawRectangle(pen, CSng(x), CSng(y), CSng(width), CSng(height))
        End Sub

        Public Overrides Sub DrawRectangles(pen As Pen, rects() As RectangleF)
            For Each rect In rects
                With rect
                    Call DrawRectangle(pen, .X, .Y, .Width, .Height)
                End With
            Next
        End Sub

        Public Overrides Sub DrawRectangles(pen As Pen, rects() As Rectangle)
            For Each rect In rects
                Call DrawRectangle(pen, rect)
            Next
        End Sub

        Public Overrides Function MeasureString(text As String, font As Font) As SizeF
            Dim css As New CSSFont(font, FontFace.SVGPointSize(font.SizeInPoints, Dpi))
            Dim size As SizeF = gdi.MeasureString(text, font)

            Return size
        End Function

        Public Overloads Overrides Sub DrawString(s As String, font As Font, brush As Brush, ByRef x!, ByRef y!, angle!)
            ' 2019-04-18 似乎SVG的scale和gdi的scale有一些不一样
            ' 在这里存在一个位置偏移的bug
            ' 在这里尝试使用font size来修正
            Dim css As New CSSFont(font, FontFace.SVGPointSize(font.SizeInPoints, Dpi))
            Dim size As SizeF = gdi.MeasureString(s, font)
            Dim text As SvgText = __svgData.svg.AddText

            x = x + FontFace.SVGPointSize(size.Width, Dpi) / 6
            y = y + FontFace.SVGPointSize(size.Height, Dpi) / 1.5

            text.Text = s
            text.X = x
            text.Y = y
            text.Style = css.CSSValue

            If TypeOf brush Is SolidBrush Then
                text.Style &= $"fill: {DirectCast(brush, SolidBrush).Color.ToHtmlColor};"
            End If

            If angle <> 0.0 Then
                text.Style &= $"transform-origin: {x}px {y}px;"
                text.Transform = $"rotate({angle})"
            End If
        End Sub

        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, ByRef point As PointF)
            Dim x = point.X
            Dim y = point.Y

            Call DrawString(s, font, brush, x, y, angle:=0)

            point = New PointF(x, y)
        End Sub

        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, layoutRectangle As RectangleF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, layoutRectangle As RectangleF, format As StringFormat)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, point As PointF, format As StringFormat)
            Throw New NotImplementedException()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, x As Single, y As Single)
            Call DrawString(s, font, brush, New PointF(x, y))
        End Sub

        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, x As Single, y As Single, format As StringFormat)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillClosedCurve(brush As Brush, points() As PointF)
            Dim path As SvgPath = __svgData.svg.AddPath

            path.D = points.GraphicsPath.SVGPathData
            path.Style = "fill: " & DirectCast(brush, SolidBrush).Color.ToHtmlColor
        End Sub

        Public Overrides Sub FillClosedCurve(brush As Brush, points() As Point)
            Dim path As SvgPath = __svgData.svg.AddPath

            path.D = points.GraphicsPath.SVGPathData
            path.Style = "fill: " & DirectCast(brush, SolidBrush).Color.ToHtmlColor
        End Sub

        Public Overrides Sub FillClosedCurve(brush As Brush, points() As Point, fillmode As FillMode)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillClosedCurve(brush As Brush, points() As PointF, fillmode As FillMode)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillClosedCurve(brush As Brush, points() As PointF, fillmode As FillMode, tension As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillClosedCurve(brush As Brush, points() As Point, fillmode As FillMode, tension As Single)
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
            Dim pathData As SvgPath = __svgData.svg.AddPath

            pathData.D = path.SVGPathData
            pathData.Fill = DirectCast(brush, SolidBrush).Color.ToHtmlColor
        End Sub

        Public Overrides Sub FillPie(brush As Brush, rect As Rectangle, startAngle As Single, sweepAngle As Single)
            With rect
                Call FillPie(brush, .X, .Y, .Width, .Height, startAngle, sweepAngle)
            End With
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub FillPie(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)
            Call FillPie(brush, CSng(x), CSng(y), CSng(width), CSng(height), CSng(startAngle), CSng(sweepAngle))
        End Sub

        Public Overrides Sub FillPie(brush As Brush, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)
            Dim path As SvgPath = __svgData.svg.AddPath

            path.D = ModelBuilder.PiePath(x, y, width, height, startAngle, sweepAngle)
            path.Style = "fill: " & DirectCast(brush, SolidBrush).Color.ToHtmlColor
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub FillPolygon(brush As Brush, points() As Point)
            Call FillPolygon(brush, points.PointF.ToArray)
        End Sub

        Public Overrides Sub FillPolygon(brush As Brush, points() As PointF)
            Dim polygon As SvgPolygon = __svgData.svg.AddPolygon

            polygon.SetPolygonPath(points)
            polygon.Fill = brush.SVGColorHelper
        End Sub

        Public Overrides Sub FillPolygon(brush As Brush, points() As Point, fillMode As FillMode)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillPolygon(brush As Brush, points() As PointF, fillMode As FillMode)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillRectangle(brush As Brush, rect As Rectangle)
            With rect
                Call FillRectangle(brush, .X, .Y, .Width, .Height)
            End With
        End Sub

        Public Overrides Sub FillRectangle(brush As Brush, rect As RectangleF)
            With rect
                Call FillRectangle(brush, .X, .Y, .Width, .Height)
            End With
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub FillRectangle(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer)
            FillRectangle(brush, CSng(x), CSng(y), CSng(width), CSng(height))
        End Sub

        Public Overrides Sub FillRectangle(brush As Brush, x As Single, y As Single, width As Single, height As Single)
            Dim rect As SvgRect = __svgData.svg.AddRect
            Dim fill As String

            If TypeOf brush Is SolidBrush Then
                fill = DirectCast(brush, SolidBrush).Color.ToHtmlColor
            Else
                fill = New DataURI(DirectCast(brush, TextureBrush).Image).ToString
            End If

            rect.SetRectangle(New PointF(x, y), New SizeF(width, height))
            rect.Style = "fill: " & fill
        End Sub

        Public Overrides Sub FillRegion(brush As Brush, region As Region)
            Throw New NotImplementedException()
        End Sub
#End Region

        Public Overrides Sub Flush()

        End Sub

        Public Overrides Sub Flush(intention As FlushIntention)

        End Sub

        Public Overrides Sub IntersectClip(region As Region)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub IntersectClip(rect As RectangleF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub IntersectClip(rect As Rectangle)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub MultiplyTransform(matrix As Matrix)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub MultiplyTransform(matrix As Matrix, order As MatrixOrder)
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

        Public Overrides Sub RotateTransform(angle As Single, order As MatrixOrder)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub ScaleTransform(sx As Single, sy As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub ScaleTransform(sx As Single, sy As Single, order As MatrixOrder)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub SetClip(rect As RectangleF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub SetClip(path As GraphicsPath)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub SetClip(rect As Rectangle)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub SetClip(g As Graphics)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub SetClip(rect As Rectangle, combineMode As CombineMode)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub SetClip(region As Region, combineMode As CombineMode)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub SetClip(path As GraphicsPath, combineMode As CombineMode)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub SetClip(rect As RectangleF, combineMode As CombineMode)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub SetClip(g As Graphics, combineMode As CombineMode)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub TransformPoints(destSpace As CoordinateSpace, srcSpace As CoordinateSpace, pts() As Point)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub TransformPoints(destSpace As CoordinateSpace, srcSpace As CoordinateSpace, pts() As PointF)
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

        Public Overrides Sub TranslateTransform(dx As Single, dy As Single, order As MatrixOrder)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Function BeginContainer() As GraphicsContainer
            Throw New NotImplementedException()
        End Function

        Public Overrides Function BeginContainer(dstrect As RectangleF, srcrect As RectangleF, unit As GraphicsUnit) As GraphicsContainer
            Throw New NotImplementedException()
        End Function

        Public Overrides Function BeginContainer(dstrect As Rectangle, srcrect As Rectangle, unit As GraphicsUnit) As GraphicsContainer
            Throw New NotImplementedException()
        End Function

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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub DrawRectangle(pen As Pen, rect As RectangleF)
            Call Me.DrawRectangles(pen, {rect})
        End Sub

        Public Overrides Sub DrawCircle(center As PointF, fill As Color, stroke As Pen, radius As Single)
            Dim circle As SvgCircle = __svgData.svg.AddCircle

            circle.R = radius
            circle.CX = center.X
            circle.CY = center.Y
            circle.Fill = fill.ToHtmlColor
            circle.Stroke = New Stroke(stroke).CSSValue
        End Sub

        Public Overrides Sub Dispose()
        End Sub

        Public Function Save(stream As IO.Stream, format As ImageFormat) As Boolean Implements SaveGdiBitmap.Save
            Return New SVGData(Me, Size, New Padding).Save(stream)
        End Function
    End Class
End Namespace
