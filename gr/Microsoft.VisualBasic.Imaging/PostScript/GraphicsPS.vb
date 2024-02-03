﻿#Region "Microsoft.VisualBasic::5b1d1506c295d3102d3c919ee453df89, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\PostScript\GraphicsPS.vb"

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

    '   Total Lines: 942
    '    Code Lines: 712
    ' Comment Lines: 7
    '   Blank Lines: 223
    '     File Size: 44.04 KB


    '     Class GraphicsPS
    ' 
    '         Properties: CompositingMode, CompositingQuality, InterpolationMode, IsClipEmpty, IsVisibleClipEmpty
    '                     PageScale, PageUnit, PixelOffsetMode, RenderingOrigin, SmoothingMode
    '                     TextContrast, TextRenderingHint
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+3 Overloads) BeginContainer, color, font, GetContextInfo, GetNearestColor
    '                   (+8 Overloads) IsVisible, linewidth, MeasureCharacterRanges, (+7 Overloads) MeasureString, note
    ' 
    '         Sub: AddMetafileComment, ClearCanvas, (+4 Overloads) CopyFromScreen, Dispose, (+4 Overloads) DrawArc
    '              (+3 Overloads) DrawBezier, (+2 Overloads) DrawBeziers, DrawCircle, (+4 Overloads) DrawClosedCurve, (+7 Overloads) DrawCurve
    '              (+4 Overloads) DrawEllipse, (+2 Overloads) DrawIcon, DrawIconUnstretched, (+30 Overloads) DrawImage, (+4 Overloads) DrawImageUnscaled
    '              DrawImageUnscaledAndClipped, (+4 Overloads) DrawLine, (+2 Overloads) DrawLines, DrawPath, (+4 Overloads) DrawPie
    '              (+2 Overloads) DrawPolygon, (+4 Overloads) DrawRectangle, (+2 Overloads) DrawRectangles, (+6 Overloads) DrawString, EndContainer
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
Imports System.Drawing.Text
Imports System.IO
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language.C

Namespace PostScript

    Public Class GraphicsPS : Inherits MockGDIPlusGraphics

        Public Overrides Property InterpolationMode As InterpolationMode

        Public Overrides Property CompositingMode As CompositingMode
            Get
                Throw New NotImplementedException()
            End Get
            Set(value As CompositingMode)
                Throw New NotImplementedException()
            End Set
        End Property

        Public Overrides Property CompositingQuality As CompositingQuality
            Get
                Throw New NotImplementedException()
            End Get
            Set(value As CompositingQuality)
                Throw New NotImplementedException()
            End Set
        End Property

        Public Overrides ReadOnly Property IsClipEmpty As Boolean
        Public Overrides ReadOnly Property IsVisibleClipEmpty As Boolean
        Public Overrides Property PageScale As Single
        Public Overrides Property PageUnit As GraphicsUnit
        Public Overrides Property PixelOffsetMode As PixelOffsetMode
        Public Overrides Property RenderingOrigin As Point
        Public Overrides Property SmoothingMode As SmoothingMode
        Public Overrides Property TextContrast As Integer
        Public Overrides Property TextRenderingHint As TextRenderingHint

        Dim ps_fontsize% = 15
        Dim buffer As New MemoryStream
        Dim fp As StreamWriter
        Dim originx, originy As Single

        Sub New(size As Size, dpi As Size)
            Call MyBase.New(size, dpi)

            Me.fp = New StreamWriter(buffer)

            fprintf(fp, "%%!PS-Adobe-3.0 EPSF-3.0\n")
            fprintf(fp, "%%%%DocumentData: Clean7Bit\n")
            fprintf(fp, "%%%\%Origin: %10.2f %10.2f\n", originx, originy)
            fprintf(fp, "%%%%BoundingBox: %10.2f %10.2f %10.2f %10.2f\n", originx, originy, size.Width, size.Height)
            fprintf(fp, "%%%%LanguageLevel: 2\n")
            fprintf(fp, "%%%%Pages: 1\n")
            fprintf(fp, "%%%%Page: 1 1                           \n")
            fprintf(fp, "%% Convert to PDF with something like this:\n")
            fprintf(fp, "%% gs -o OutputFileName.pdf -sDEVICE=pdfwrite -dEPSCrop InputFileName.ps\n")
            fprintf(fp, "%% PostScript generated using the PStools library\n")
            fprintf(fp, "%% from the Binghamton Optimality Research Group\n")
            fprintf(fp, "%% Get the library at https://github.com/profmadden/pstools\n")
            fprintf(fp, "%% This library is free to use, however you see fit.  It would be\n")
            fprintf(fp, "%% nice if you let us know that you're using it, though!\n")
            fprintf(fp, "%% Drop us an email at pmadden@binghamton.edu, or pop by our\n")
            fprintf(fp, "%% web page, https://optimal.cs.binghamton.edu\n")
            fprintf(fp, "%% Standard use-at-your-own-risk stuff applies....\n")
            fprintf(fp, "/Courier findfont 15 scalefont setfont\n")
        End Sub

        Public Function linewidth(width As Single) As GraphicsPS
            fprintf(fp, "%f setlinewidth\n", width)
            Return Me
        End Function

        Public Function color(r!, g!, b!) As GraphicsPS
            fprintf(fp, "%3.2f %3.2f %3.2f setrgbcolor\n", r, g, b)
            Return Me
        End Function

        Public Function font(name As String, fontsize!) As GraphicsPS
            fprintf(fp, "/%s findfont %f scalefont setfont\n", name, fontsize)
            ps_fontsize = fontsize
            Return Me
        End Function

        Public Function note(noteText As String) As GraphicsPS
            fprintf(fp, "%% %s\n", noteText)
            Return Me
        End Function

        Public Overrides Sub AddMetafileComment(data() As Byte)
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Sub ClearCanvas(color As Color)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub CopyFromScreen(upperLeftSource As Point, upperLeftDestination As Point, blockRegionSize As Size)
            Throw New NotImplementedException()
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

        Public Overrides Sub DrawIcon(icon As Icon, targetRect As Rectangle)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawIcon(icon As Icon, x As Integer, y As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawIconUnstretched(icon As Icon, targetRect As Rectangle)
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

        Public Overrides Sub DrawImage(image As Image, destRect As RectangleF, srcRect As RectangleF, srcUnit As GraphicsUnit)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcRect As Rectangle, srcUnit As GraphicsUnit)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, x As Single, y As Single, width As Single, height As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, x As Integer, y As Integer, width As Integer, height As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, x As Single, y As Single, srcRect As RectangleF, srcUnit As GraphicsUnit)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, x As Integer, y As Integer, srcRect As Rectangle, srcUnit As GraphicsUnit)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As Graphics.DrawImageAbort)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As Graphics.DrawImageAbort)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As Graphics.DrawImageAbort, callbackData As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As Graphics.DrawImageAbort, callbackData As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As Graphics.DrawImageAbort)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As Graphics.DrawImageAbort)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As Graphics.DrawImageAbort, callbackData As IntPtr)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As Graphics.DrawImageAbort, callbackData As IntPtr)
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
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawLine(pen As Pen, pt1 As Point, pt2 As Point)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawLine(pen As Pen, x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawLine(pen As Pen, x1 As Single, y1 As Single, x2 As Single, y2 As Single)
            fprintf(fp, "newpath %f %f moveto %f %f lineto stroke\n", x1, y1, x2, y2)
        End Sub

        Public Overrides Sub DrawLines(pen As Pen, points() As PointF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawLines(pen As Pen, points() As Point)
            Throw New NotImplementedException()
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
            fprintf(fp, "%f %f %f 0 360 arc closepath\n", center.X, center.Y, radius)

            If Not fill.IsTransparent Then
                fprintf(fp, "gsave fill grestore stroke\n")
            Else
                fprintf(fp, "stroke\n")
            End If
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
            Dim x1 = x, y1 = y
            Dim x2 = x1 + width
            Dim y2 = y1 + height

            fprintf(fp, "newpath %f %f moveto ", x1, y1)
            fprintf(fp, "%f %f lineto ", x2, y1)
            fprintf(fp, "%f %f lineto ", x2, y2)
            fprintf(fp, "%f %f lineto ", x1, y2)
            fprintf(fp, "%f %f lineto ", x1, y1)
            '           If (Fill())Then
            '{
            '	If (stroke) Then
            '                   fprintf(context -> fp, "closepath gsave fill grestore stroke\n");
            '	Else
            '                   fprintf(context -> fp, "closepath fill\n");
            '}

            fprintf(fp, "closepath stroke\n")
        End Sub

        Public Overrides Sub DrawRectangle(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawRectangles(pen As Pen, rects() As RectangleF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawRectangles(pen As Pen, rects() As Rectangle)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, ByRef point As PointF)
            Throw New NotImplementedException()
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

        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, x As Single, y As Single)
            fprintf(fp, "%f %f moveto (%s) show\n", x, y, s)
        End Sub

        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, x As Single, y As Single, format As StringFormat)
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

        Public Overrides Sub FillClosedCurve(brush As Brush, points() As PointF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillClosedCurve(brush As Brush, points() As Point)
            Throw New NotImplementedException()
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

        Public Overrides Sub FillPolygon(brush As Brush, points() As Point, fillMode As FillMode)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillPolygon(brush As Brush, points() As PointF, fillMode As FillMode)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillRectangle(brush As Brush, rect As Rectangle)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillRectangle(brush As Brush, rect As RectangleF)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillRectangle(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillRectangle(brush As Brush, x As Single, y As Single, width As Single, height As Single)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub FillRegion(brush As Brush, region As Region)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub Flush()
            fprintf(fp, "%%%%EOF\n")
            fp.Flush()
        End Sub

        Public Overrides Sub Flush(intention As FlushIntention)
            Throw New NotImplementedException()
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

        Public Overrides Function IsVisible(point As PointF) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function IsVisible(rect As RectangleF) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function IsVisible(point As Point) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function IsVisible(x As Single, y As Single) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function IsVisible(x As Integer, y As Integer) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function IsVisible(x As Integer, y As Integer, width As Integer, height As Integer) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function IsVisible(x As Single, y As Single, width As Single, height As Single) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MeasureCharacterRanges(text As String, font As Font, layoutRect As RectangleF, stringFormat As StringFormat) As Region()
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MeasureString(text As String, font As Font) As SizeF
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MeasureString(text As String, font As Font, width As Integer) As SizeF
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MeasureString(text As String, font As Font, layoutArea As SizeF) As SizeF
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MeasureString(text As String, font As Font, width As Integer, format As StringFormat) As SizeF
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MeasureString(text As String, font As Font, origin As PointF, stringFormat As StringFormat) As SizeF
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MeasureString(text As String, font As Font, layoutArea As SizeF, stringFormat As StringFormat) As SizeF
            Throw New NotImplementedException()
        End Function

        Public Overrides Function MeasureString(text As String, font As Font, layoutArea As SizeF, stringFormat As StringFormat, ByRef charactersFitted As Integer, ByRef linesFilled As Integer) As SizeF
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
