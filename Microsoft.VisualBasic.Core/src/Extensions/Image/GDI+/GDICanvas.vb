﻿#Region "Microsoft.VisualBasic::db3e0ff4343fc85738b1457dfce199f5, Microsoft.VisualBasic.Core\src\Extensions\Image\GDI+\GDICanvas.vb"

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

'   Total Lines: 4873
'    Code Lines: 672 (13.79%)
' Comment Lines: 4027 (82.64%)
'    - Xml Docs: 6.75%
' 
'   Blank Lines: 174 (3.57%)
'     File Size: 211.31 KB


'     Class GDICanvas
' 
'         Properties: CompositingMode, CompositingQuality, DpiX, DpiY, Graphics
'                     InterpolationMode, IsClipEmpty, IsVisibleClipEmpty, PageScale, PageUnit
'                     PixelOffsetMode, RenderingOrigin, SmoothingMode, TextContrast, TextRenderingHint
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: (+3 Overloads) BeginContainer, GetContextInfo, GetNearestColor, (+4 Overloads) IsVisible, MeasureCharacterRanges
'                   (+8 Overloads) MeasureString
' 
'         Sub: AddMetafileComment, Clear, ClearCanvas, (+4 Overloads) CopyFromScreen, Dispose
'              (+4 Overloads) DrawArc, DrawB, (+3 Overloads) DrawBezier, (+2 Overloads) DrawBeziers, (+4 Overloads) DrawClosedCurve
'              (+7 Overloads) DrawCurve, (+4 Overloads) DrawEllipse, (+2 Overloads) DrawIcon, DrawIconUnstretched, (+30 Overloads) DrawImage
'              (+4 Overloads) DrawImageUnscaled, DrawImageUnscaledAndClipped, (+5 Overloads) DrawLine, (+2 Overloads) DrawLines, DrawPath
'              (+4 Overloads) DrawPie, (+2 Overloads) DrawPolygon, (+4 Overloads) DrawRectangle, (+2 Overloads) DrawRectangles, (+7 Overloads) DrawString
'              EndContainer, (+36 Overloads) EnumerateMetafile, (+2 Overloads) ExcludeClip, (+6 Overloads) FillClosedCurve, (+4 Overloads) FillEllipse
'              FillPath, (+3 Overloads) FillPie, (+4 Overloads) FillPolygon, (+4 Overloads) FillRectangle, FillRegion
'              Finalize, (+2 Overloads) Flush, (+3 Overloads) IntersectClip, (+2 Overloads) MultiplyTransform, ResetClip
'              ResetTransform, (+2 Overloads) RotateTransform, (+2 Overloads) ScaleTransform, (+9 Overloads) SetClip, SetTransformMatrix
'              (+2 Overloads) TransformPoints, (+2 Overloads) TranslateClip, (+2 Overloads) TranslateTransform
' 
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Graphics
Imports System.Drawing.Imaging
Imports System.Drawing.Text
Imports System.Runtime.CompilerServices

Namespace Imaging

    ''' <summary>
    ''' 这个对象是<see cref="Graphics2D"/>以及<see cref="Wmf"/>公用的基础类型
    ''' </summary>
    ''' <remarks>the gdi+ graphics canvas base model</remarks>
    Public MustInherit Class GDICanvas : Inherits IGraphics
        Implements IDisposable

        Protected Friend g As Graphics

        ''' <summary>
        ''' GDI+ device handle.(GDI+绘图设备句柄)
        ''' </summary>
        ''' <remarks></remarks>
        Public Property Graphics As Graphics
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return g
            End Get
            Set(value As Graphics)
                g = value
            End Set
        End Property

        Sub New()
        End Sub

#Region "Implements Class Graphics"

        Public Overloads Sub DrawLine(x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer)
            Call Graphics.DrawLine(Stroke, x1, y1, x2, y2)
        End Sub

        Public Overloads Sub DrawString(str As String, x As Integer, y As Integer)
            Call Graphics.DrawString(str, Font, Brushes.Black, New Point(x, y))
        End Sub

        ''' <summary>
        ''' Gets or sets the scaling between world units and page units for this System.Drawing.Graphics.
        ''' </summary>
        ''' <returns>This property specifies a value for the scaling between world units and page
        ''' units for this System.Drawing.Graphics.</returns>
        Public Overrides Property PageScale As Single
            Get
                Return Graphics.PageScale
            End Get
            Set(value As Single)
                Graphics.PageScale = value
            End Set
        End Property

        '
        ' Summary:
        '     Gets or sets the rendering origin of this System.Drawing.Graphics for dithering
        '     and for hatch brushes.
        '
        ' Returns:
        '     A System.Drawing.Point structure that represents the dither origin for 8-bits-per-pixel
        '     and 16-bits-per-pixel dithering and is also used to set the origin for hatch
        '     brushes.
        Public Overrides Property RenderingOrigin As Point
            Get
                Return Graphics.RenderingOrigin
            End Get
            Set(value As Point)
                Graphics.RenderingOrigin = value
            End Set
        End Property
        '
        ' Summary:
        '     Gets or sets the gamma correction value for rendering text.
        '
        ' Returns:
        '     The gamma correction value used for rendering antialiased and ClearType text.
        Public Overrides Property TextContrast As Integer
            Get
                Return Graphics.TextContrast
            End Get
            Set(value As Integer)
                Graphics.TextContrast = value
            End Set
        End Property

        '
        ' Summary:
        '     Adds a comment to the current System.Drawing.Imaging.Metafile.
        '
        ' Parameters:
        '   data:
        '     Array of bytes that contains the comment.
        Public Overrides Sub AddMetafileComment(data() As Byte)
            Call Graphics.AddMetafileComment(data)
        End Sub

        Protected Overrides Sub ClearCanvas(color As Color)
            Call Graphics.Clear(color)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Sub Clear(bg As Brush)
            Call Graphics.FillRectangle(bg, New RectangleF(New PointF, Size.SizeF))
        End Sub

        '
        ' Summary:
        '     Draws an arc representing a portion of an ellipse specified by a System.Drawing.Rectangle
        '     structure.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the arc.
        '
        '   rect:
        '     System.Drawing.RectangleF structure that defines the boundaries of the ellipse.
        '
        '   startAngle:
        '     Angle in degrees measured clockwise from the x-axis to the starting point of
        '     the arc.
        '
        '   sweepAngle:
        '     Angle in degrees measured clockwise from the startAngle parameter to ending point
        '     of the arc.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.
        Public Overrides Sub DrawArc(pen As Pen, rect As Rectangle, startAngle As Single, sweepAngle As Single)
            Call Graphics.DrawArc(pen, rect, startAngle, sweepAngle)
        End Sub
        '
        ' Summary:
        '     Draws an arc representing a portion of an ellipse specified by a System.Drawing.RectangleF
        '     structure.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the arc.
        '
        '   rect:
        '     System.Drawing.RectangleF structure that defines the boundaries of the ellipse.
        '
        '   startAngle:
        '     Angle in degrees measured clockwise from the x-axis to the starting point of
        '     the arc.
        '
        '   sweepAngle:
        '     Angle in degrees measured clockwise from the startAngle parameter to ending point
        '     of the arc.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null
        Public Overrides Sub DrawArc(pen As Pen, rect As RectangleF, startAngle As Single, sweepAngle As Single)
            Call Graphics.DrawArc(pen, rect, startAngle, sweepAngle)
        End Sub
        '
        ' Summary:
        '     Draws an arc representing a portion of an ellipse specified by a pair of coordinates,
        '     a width, and a height.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the arc.
        '
        '   x:
        '     The x-coordinate of the upper-left corner of the rectangle that defines the ellipse.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the rectangle that defines the ellipse.
        '
        '   width:
        '     Width of the rectangle that defines the ellipse.
        '
        '   height:
        '     Height of the rectangle that defines the ellipse.
        '
        '   startAngle:
        '     Angle in degrees measured clockwise from the x-axis to the starting point of
        '     the arc.
        '
        '   sweepAngle:
        '     Angle in degrees measured clockwise from the startAngle parameter to ending point
        '     of the arc.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-rects is null.
        '
        '   T:System.ArgumentNullException:
        '     rects is a zero-length array.
        Public Overrides Sub DrawArc(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)

        End Sub
        '
        ' Summary:
        '     Draws an arc representing a portion of an ellipse specified by a pair of coordinates,
        '     a width, and a height.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the arc.
        '
        '   x:
        '     The x-coordinate of the upper-left corner of the rectangle that defines the ellipse.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the rectangle that defines the ellipse.
        '
        '   width:
        '     Width of the rectangle that defines the ellipse.
        '
        '   height:
        '     Height of the rectangle that defines the ellipse.
        '
        '   startAngle:
        '     Angle in degrees measured clockwise from the x-axis to the starting point of
        '     the arc.
        '
        '   sweepAngle:
        '     Angle in degrees measured clockwise from the startAngle parameter to ending point
        '     of the arc.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.
        Public Overrides Sub DrawArc(pen As Pen, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)

        End Sub

        ''' <summary>
        ''' Draws a Bézier spline defined by four System.Drawing.Point structures.
        ''' </summary>
        ''' <param name="pen">System.Drawing.Pen structure that determines the color, width, and style of the
        ''' curve.</param>
        ''' <param name="pt1">System.Drawing.Point structure that represents the starting point of the curve.</param>
        ''' <param name="pt2">System.Drawing.Point structure that represents the first control point for the
        ''' curve.</param>
        ''' <param name="pt3">System.Drawing.Point structure that represents the second control point for the
        ''' curve.</param>
        ''' <param name="pt4">System.Drawing.Point structure that represents the ending point of the curve.</param>
        Public Sub DrawBézier(pen As Pen, pt1 As Point, pt2 As Point, pt3 As Point, pt4 As Point)
            Call Graphics.DrawBezier(pen, pt1, pt2, pt3, pt4)
        End Sub

        ' Summary:
        '     Draws a Bézier spline defined by four System.Drawing.PointF structures.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the curve.
        '
        '   pt1:
        '     System.Drawing.PointF structure that represents the starting point of the curve.
        '
        '   pt2:
        '     System.Drawing.PointF structure that represents the first control point for the
        '     curve.
        '
        '   pt3:
        '     System.Drawing.PointF structure that represents the second control point for
        '     the curve.
        '
        '   pt4:
        '     System.Drawing.PointF structure that represents the ending point of the curve.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.
        Public Overrides Sub DrawBezier(pen As Pen, pt1 As PointF, pt2 As PointF, pt3 As PointF, pt4 As PointF)

        End Sub
        '
        ' Summary:
        '     Draws a Bézier spline defined by four ordered pairs of coordinates that represent
        '     points.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the curve.
        '
        '   x1:
        '     The x-coordinate of the starting point of the curve.
        '
        '   y1:
        '     The y-coordinate of the starting point of the curve.
        '
        '   x2:
        '     The x-coordinate of the first control point of the curve.
        '
        '   y2:
        '     The y-coordinate of the first control point of the curve.
        '
        '   x3:
        '     The x-coordinate of the second control point of the curve.
        '
        '   y3:
        '     The y-coordinate of the second control point of the curve.
        '
        '   x4:
        '     The x-coordinate of the ending point of the curve.
        '
        '   y4:
        '     The y-coordinate of the ending point of the curve.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.
        Public Overrides Sub DrawBezier(pen As Pen, x1 As Single, y1 As Single, x2 As Single, y2 As Single, x3 As Single, y3 As Single, x4 As Single, y4 As Single)

        End Sub
        '
        ' Summary:
        '     Draws a series of Bézier splines from an array of System.Drawing.Point structures.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the curve.
        '
        '   points:
        '     Array of System.Drawing.Point structures that represent the points that determine
        '     the curve. The number of points in the array should be a multiple of 3 plus 1,
        '     such as 4, 7, or 10.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-points is null.
        Public Overrides Sub DrawBeziers(pen As Pen, points() As Point)

        End Sub
        '
        ' Summary:
        '     Draws a series of Bézier splines from an array of System.Drawing.PointF structures.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the curve.
        '
        '   points:
        '     Array of System.Drawing.PointF structures that represent the points that determine
        '     the curve. The number of points in the array should be a multiple of 3 plus 1,
        '     such as 4, 7, or 10.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-points is null.
        Public Overrides Sub DrawBeziers(pen As Pen, points() As PointF)

        End Sub
        '
        ' Summary:
        '     Draws a closed cardinal spline defined by an array of System.Drawing.Point structures.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and height of the curve.
        '
        '   points:
        '     Array of System.Drawing.Point structures that define the spline.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-points is null.
        Public Overrides Sub DrawClosedCurve(pen As Pen, points() As Point)

        End Sub
        '
        ' Summary:
        '     Draws a closed cardinal spline defined by an array of System.Drawing.PointF structures.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and height of the curve.
        '
        '   points:
        '     Array of System.Drawing.PointF structures that define the spline.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-points is null.
        Public Overrides Sub DrawClosedCurve(pen As Pen, points() As PointF)

        End Sub


        '
        ' Summary:
        '     Draws a cardinal spline through a specified array of System.Drawing.Point structures.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and height of the curve.
        '
        '   points:
        '     Array of System.Drawing.Point structures that define the spline.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-points is null.
        Public Overrides Sub DrawCurve(pen As Pen, points() As Point)

        End Sub
        '
        ' Summary:
        '     Draws a cardinal spline through a specified array of System.Drawing.PointF structures.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the curve.
        '
        '   points:
        '     Array of System.Drawing.PointF structures that define the spline.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-points is null.
        Public Overrides Sub DrawCurve(pen As Pen, points() As PointF)

        End Sub

        '
        ' Summary:
        '     Draws a cardinal spline through a specified array of System.Drawing.Point structures
        '     using a specified tension.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the curve.
        '
        '   points:
        '     Array of System.Drawing.Point structures that define the spline.
        '
        '   tension:
        '     Value greater than or equal to 0.0F that specifies the tension of the curve.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-points is null.
        Public Overrides Sub DrawCurve(pen As Pen, points() As Point, tension As Single)

        End Sub

        '
        ' Summary:
        '     Draws a cardinal spline through a specified array of System.Drawing.PointF structures
        '     using a specified tension.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the curve.
        '
        '   points:
        '     Array of System.Drawing.PointF structures that represent the points that define
        '     the curve.
        '
        '   tension:
        '     Value greater than or equal to 0.0F that specifies the tension of the curve.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-points is null.
        Public Overrides Sub DrawCurve(pen As Pen, points() As PointF, tension As Single)

        End Sub

        '
        ' Summary:
        '     Draws a cardinal spline through a specified array of System.Drawing.PointF structures.
        '     The drawing begins offset from the beginning of the array.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the curve.
        '
        '   points:
        '     Array of System.Drawing.PointF structures that define the spline.
        '
        '   offset:
        '     Offset from the first element in the array of the points parameter to the starting
        '     point in the curve.
        '
        '   numberOfSegments:
        '     Number of segments after the starting point to include in the curve.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-points is null.
        Public Overrides Sub DrawCurve(pen As Pen, points() As PointF, offset As Integer, numberOfSegments As Integer)

        End Sub

        '
        ' Summary:
        '     Draws a cardinal spline through a specified array of System.Drawing.Point structures
        '     using a specified tension.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the curve.
        '
        '   points:
        '     Array of System.Drawing.Point structures that define the spline.
        '
        '   offset:
        '     Offset from the first element in the array of the points parameter to the starting
        '     point in the curve.
        '
        '   numberOfSegments:
        '     Number of segments after the starting point to include in the curve.
        '
        '   tension:
        '     Value greater than or equal to 0.0F that specifies the tension of the curve.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-points is null.
        Public Overrides Sub DrawCurve(pen As Pen, points() As Point, offset As Integer, numberOfSegments As Integer, tension As Single)

        End Sub


        '
        ' Summary:
        '     Draws a cardinal spline through a specified array of System.Drawing.PointF structures
        '     using a specified tension. The drawing begins offset from the beginning of the
        '     array.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the curve.
        '
        '   points:
        '     Array of System.Drawing.PointF structures that define the spline.
        '
        '   offset:
        '     Offset from the first element in the array of the points parameter to the starting
        '     point in the curve.
        '
        '   numberOfSegments:
        '     Number of segments after the starting point to include in the curve.
        '
        '   tension:
        '     Value greater than or equal to 0.0F that specifies the tension of the curve.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-points is null.
        Public Overrides Sub DrawCurve(pen As Pen, points() As PointF, offset As Integer, numberOfSegments As Integer, tension As Single)

        End Sub
        '
        ' Summary:
        '     Draws an ellipse specified by a bounding System.Drawing.Rectangle structure.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the ellipse.
        '
        '   rect:
        '     System.Drawing.Rectangle structure that defines the boundaries of the ellipse.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.
        Public Overrides Sub DrawEllipse(pen As Pen, rect As Rectangle)
            Call Graphics.DrawEllipse(pen, rect)
        End Sub
        '
        ' Summary:
        '     Draws an ellipse defined by a bounding System.Drawing.RectangleF.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the ellipse.
        '
        '   rect:
        '     System.Drawing.RectangleF structure that defines the boundaries of the ellipse.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.
        Public Overrides Sub DrawEllipse(pen As Pen, rect As RectangleF)
            Call Graphics.DrawEllipse(pen, rect)
        End Sub
        '
        ' Summary:
        '     Draws an ellipse defined by a bounding rectangle specified by coordinates for
        '     the upper-left corner of the rectangle, a height, and a width.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the ellipse.
        '
        '   x:
        '     The x-coordinate of the upper-left corner of the bounding rectangle that defines
        '     the ellipse.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the bounding rectangle that defines
        '     the ellipse.
        '
        '   width:
        '     Width of the bounding rectangle that defines the ellipse.
        '
        '   height:
        '     Height of the bounding rectangle that defines the ellipse.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.
        Public Overrides Sub DrawEllipse(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer)
            Call Graphics.DrawEllipse(pen, x, y, width, height)
        End Sub

        ''' <summary>
        ''' Draws an ellipse defined by a bounding rectangle specified by a pair of coordinates,
        ''' a height, and a width.
        ''' </summary>
        ''' <param name="pen">System.Drawing.Pen that determines the color, width, and style of the ellipse.</param>
        ''' <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines
        ''' the ellipse.</param>
        ''' <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines
        ''' the ellipse.</param>
        ''' <param name="width">Width of the bounding rectangle that defines the ellipse.</param>
        ''' <param name="height">Height of the bounding rectangle that defines the ellipse.</param>
        Public Overrides Sub DrawEllipse(pen As Pen, x As Single, y As Single, width As Single, height As Single)
            If x < 0 OrElse y < 0 Then
                Return
            End If
            Call Graphics.DrawEllipse(pen, x, y, width, height)
        End Sub

        '
        ' Summary:
        '     Draws the specified System.Drawing.Image at the specified location and with the
        '     specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   rect:
        '     System.Drawing.RectangleF structure that specifies the location and size of the
        '     drawn image.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, rect As RectangleF)
            Call Graphics.DrawImage(image, rect)
        End Sub
        '
        ' Summary:
        '     Draws the specified System.Drawing.Image at the specified location and with the
        '     specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   rect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     drawn image.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, rect As Rectangle)
            Call Graphics.DrawImage(image, rect)
        End Sub
        '
        ' Summary:
        '     Draws the specified System.Drawing.Image at the specified location and with the
        '     specified shape and size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destPoints:
        '     Array of three System.Drawing.Point structures that define a parallelogram.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, destPoints() As Point)
            Call Graphics.DrawImage(image, destPoints)
        End Sub
        '
        ' Summary:
        '     Draws the specified System.Drawing.Image at the specified location and with the
        '     specified shape and size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destPoints:
        '     Array of three System.Drawing.PointF structures that define a parallelogram.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, destPoints() As PointF)
            Call Graphics.DrawImage(image, destPoints)
        End Sub

        ''' <summary>
        ''' Draws the specified System.Drawing.Image, using its original physical size, at
        ''' the specified location.
        ''' </summary>
        ''' <param name="image">System.Drawing.Image to draw.</param>
        ''' <param name="point">System.Drawing.Point structure that represents the location of the upper-left
        ''' corner of the drawn image.</param>
        Public Overrides Sub DrawImage(image As Image, point As Point)
            Call Graphics.DrawImage(image, point)
        End Sub
        '
        ' Summary:
        '     Draws the specified System.Drawing.Image, using its original physical size, at
        '     the specified location.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   point:
        '     System.Drawing.PointF structure that represents the upper-left corner of the
        '     drawn image.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, point As PointF)
            Call Graphics.DrawImage(image, point)
        End Sub
        '
        ' Summary:
        '     Draws the specified image, using its original physical size, at the location
        '     specified by a coordinate pair.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   x:
        '     The x-coordinate of the upper-left corner of the drawn image.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the drawn image.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, x As Integer, y As Integer)
            Call Graphics.DrawImage(image, x, y)
        End Sub
        '
        ' Summary:
        '     Draws the specified System.Drawing.Image, using its original physical size, at
        '     the specified location.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   x:
        '     The x-coordinate of the upper-left corner of the drawn image.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the drawn image.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, x As Single, y As Single)
            Call Graphics.DrawImage(image, x, y)
        End Sub

        ''' <summary>
        ''' Draws the specified <see cref="Image"/> at the specified location and with the
        ''' specified size.
        ''' </summary>
        ''' <param name="image">System.Drawing.Image to draw.</param>
        ''' <param name="x">The x-coordinate of the upper-left corner of the drawn image.</param>
        ''' <param name="y">The y-coordinate of the upper-left corner of the drawn image.</param>
        ''' <param name="width">Width of the drawn image.</param>
        ''' <param name="height">Height of the drawn image.</param>
        Public Overrides Sub DrawImage(image As Image, x As Integer, y As Integer, width As Integer, height As Integer)
            Call Graphics.DrawImage(image, x, y, width, height)
        End Sub
        '
        ' Summary:
        '     Draws the specified System.Drawing.Image at the specified location and with the
        '     specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   x:
        '     The x-coordinate of the upper-left corner of the drawn image.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the drawn image.
        '
        '   width:
        '     Width of the drawn image.
        '
        '   height:
        '     Height of the drawn image.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, x As Single, y As Single, width As Single, height As Single)
            Call Graphics.DrawImage(image, x, y, width, height)
        End Sub

        '
        ' Summary:
        '     Draws a specified image using its original physical size at a specified location.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   rect:
        '     System.Drawing.Rectangle that specifies the upper-left corner of the drawn image.
        '     The X and Y properties of the rectangle specify the upper-left corner. The Width
        '     and Height properties are ignored.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImageUnscaled(image As Image, rect As Rectangle)
            Call Graphics.DrawImageUnscaled(image, rect)
        End Sub

        ''' <summary>
        ''' Draws a specified image using its original physical size at a specified location.
        ''' </summary>
        ''' <param name="image"><see cref="Image"/> to draw.</param>
        ''' <param name="point"><see cref="Point"/> structure that specifies the upper-left corner of the drawn
        ''' image.</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub DrawImageUnscaled(image As Image, point As Point)
            Call Graphics.DrawImageUnscaled(image, point)
        End Sub

        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        ''' <summary>
        ''' Draws the specified image using its original physical size at the location specified
        ''' by a coordinate pair.
        ''' </summary>
        ''' <param name="image">System.Drawing.Image to draw.</param>
        ''' <param name="x">The x-coordinate of the upper-left corner of the drawn image.</param>
        ''' <param name="y">The y-coordinate of the upper-left corner of the drawn image.</param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub DrawImageUnscaled(image As Image, x As Integer, y As Integer)
            Call Graphics.DrawImageUnscaled(image, x, y)
        End Sub

        ''' <summary>
        ''' Draws a specified image using its original physical size at a specified location.
        ''' </summary>
        ''' <param name="image"><see cref="Image"/> to draw.</param>
        ''' <param name="x">The x-coordinate of the upper-left corner of the drawn image.</param>
        ''' <param name="y">The y-coordinate of the upper-left corner of the drawn image.</param>
        ''' <param name="width"></param>
        ''' <param name="height"></param>
        Public Overrides Sub DrawImageUnscaled(image As Image, x As Integer, y As Integer, width As Integer, height As Integer)
            Call Graphics.DrawImageUnscaled(image, x, y, width, height)
        End Sub

        ''' <summary>
        ''' Draws the specified image without scaling and clips it, if necessary, to fit
        ''' in the specified rectangle.
        ''' </summary>
        ''' <param name="image">The <see cref="Image"/> to draw.</param>
        ''' <param name="rect">The <see cref="Rectangle"/> in which to draw the image.</param>
        Public Overrides Sub DrawImageUnscaledAndClipped(image As Image, rect As Rectangle)
            Call Graphics.DrawImageUnscaledAndClipped(image, rect)
        End Sub

        ''' <summary>
        ''' Draws a line connecting two <see cref="Point"/> structures.
        ''' </summary>
        ''' <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the line.</param>
        ''' <param name="pt1"><see cref="Point"/> structure that represents the first point to connect.</param>
        ''' <param name="pt2"><see cref="Point"/> structure that represents the second point to connect.</param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub DrawLine(pen As Pen, pt1 As Point, pt2 As Point)
            Call Graphics.DrawLine(pen, pt1, pt2)
        End Sub

        ''' <summary>
        ''' Draws a line connecting two System.Drawing.PointF structures.
        ''' </summary>
        ''' <param name="pen">System.Drawing.Pen that determines the color, width, and style of the line.</param>
        ''' <param name="pt1">System.Drawing.PointF structure that represents the first point to connect.</param>
        ''' <param name="pt2">System.Drawing.PointF structure that represents the second point to connect.</param>
        Public Overrides Sub DrawLine(pen As Pen, pt1 As PointF, pt2 As PointF)
            'If pt1.X < 0 OrElse pt1.Y < 0 OrElse pt2.X < 0 OrElse pt2.Y < 0 Then
            '    Return
            'End If

            Call Graphics.DrawLine(pen, pt1, pt2)
        End Sub
        '
        ' Summary:
        '     Draws a line connecting the two points specified by the coordinate pairs.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the line.
        '
        '   x1:
        '     The x-coordinate of the first point.
        '
        '   y1:
        '     The y-coordinate of the first point.
        '
        '   x2:
        '     The x-coordinate of the second point.
        '
        '   y2:
        '     The y-coordinate of the second point.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.
        Public Overrides Sub DrawLine(pen As Pen, x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer)
            Call Graphics.DrawLine(pen, x1, y1, x2, y2)
        End Sub
        '
        ' Summary:
        '     Draws a line connecting the two points specified by the coordinate pairs.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the line.
        '
        '   x1:
        '     The x-coordinate of the first point.
        '
        '   y1:
        '     The y-coordinate of the first point.
        '
        '   x2:
        '     The x-coordinate of the second point.
        '
        '   y2:
        '     The y-coordinate of the second point.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.
        Public Overrides Sub DrawLine(pen As Pen, x1 As Single, y1 As Single, x2 As Single, y2 As Single)
            Call Graphics.DrawLine(pen, x1, y1, x2, y2)
        End Sub
        '
        ' Summary:
        '     Draws a series of line segments that connect an array of System.Drawing.Point
        '     structures.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the line segments.
        '
        '   points:
        '     Array of System.Drawing.Point structures that represent the points to connect.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-points is null.
        Public Overrides Sub DrawLines(pen As Pen, points() As Point)
            Call Graphics.DrawLines(pen, points)
        End Sub
        '
        ' Summary:
        '     Draws a series of line segments that connect an array of System.Drawing.PointF
        '     structures.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the line segments.
        '
        '   points:
        '     Array of System.Drawing.PointF structures that represent the points to connect.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-points is null.
        Public Overrides Sub DrawLines(pen As Pen, points() As PointF)
            Call Graphics.DrawLines(pen, points)
        End Sub
        '
        ' Summary:
        '     Draws a System.Drawing.Drawing2D.GraphicsPath.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the path.
        '
        '   path:
        '     System.Drawing.Drawing2D.GraphicsPath to draw.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-path is null.
        Public Overrides Sub DrawPath(pen As Pen, path As GraphicsPath)
            Call Graphics.DrawPath(pen, path)
        End Sub
        '
        ' Summary:
        '     Draws a pie shape defined by an ellipse specified by a System.Drawing.Rectangle
        '     structure and two radial lines.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the pie shape.
        '
        '   rect:
        '     System.Drawing.Rectangle structure that represents the bounding rectangle that
        '     defines the ellipse from which the pie shape comes.
        '
        '   startAngle:
        '     Angle measured in degrees clockwise from the x-axis to the first side of the
        '     pie shape.
        '
        '   sweepAngle:
        '     Angle measured in degrees clockwise from the startAngle parameter to the second
        '     side of the pie shape.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.
        Public Overrides Sub DrawPie(pen As Pen, rect As Rectangle, startAngle As Single, sweepAngle As Single)
            Call Graphics.DrawPie(pen, rect, startAngle, sweepAngle)
        End Sub
        '
        ' Summary:
        '     Draws a pie shape defined by an ellipse specified by a System.Drawing.RectangleF
        '     structure and two radial lines.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the pie shape.
        '
        '   rect:
        '     System.Drawing.RectangleF structure that represents the bounding rectangle that
        '     defines the ellipse from which the pie shape comes.
        '
        '   startAngle:
        '     Angle measured in degrees clockwise from the x-axis to the first side of the
        '     pie shape.
        '
        '   sweepAngle:
        '     Angle measured in degrees clockwise from the startAngle parameter to the second
        '     side of the pie shape.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.
        Public Overrides Sub DrawPie(pen As Pen, rect As RectangleF, startAngle As Single, sweepAngle As Single)
            Call Graphics.DrawPie(pen, rect, startAngle, sweepAngle)
        End Sub
        '
        ' Summary:
        '     Draws a pie shape defined by an ellipse specified by a coordinate pair, a width,
        '     a height, and two radial lines.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the pie shape.
        '
        '   x:
        '     The x-coordinate of the upper-left corner of the bounding rectangle that defines
        '     the ellipse from which the pie shape comes.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the bounding rectangle that defines
        '     the ellipse from which the pie shape comes.
        '
        '   width:
        '     Width of the bounding rectangle that defines the ellipse from which the pie shape
        '     comes.
        '
        '   height:
        '     Height of the bounding rectangle that defines the ellipse from which the pie
        '     shape comes.
        '
        '   startAngle:
        '     Angle measured in degrees clockwise from the x-axis to the first side of the
        '     pie shape.
        '
        '   sweepAngle:
        '     Angle measured in degrees clockwise from the startAngle parameter to the second
        '     side of the pie shape.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.
        Public Overrides Sub DrawPie(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)

        End Sub
        '
        ' Summary:
        '     Draws a pie shape defined by an ellipse specified by a coordinate pair, a width,
        '     a height, and two radial lines.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the pie shape.
        '
        '   x:
        '     The x-coordinate of the upper-left corner of the bounding rectangle that defines
        '     the ellipse from which the pie shape comes.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the bounding rectangle that defines
        '     the ellipse from which the pie shape comes.
        '
        '   width:
        '     Width of the bounding rectangle that defines the ellipse from which the pie shape
        '     comes.
        '
        '   height:
        '     Height of the bounding rectangle that defines the ellipse from which the pie
        '     shape comes.
        '
        '   startAngle:
        '     Angle measured in degrees clockwise from the x-axis to the first side of the
        '     pie shape.
        '
        '   sweepAngle:
        '     Angle measured in degrees clockwise from the startAngle parameter to the second
        '     side of the pie shape.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.

        Public Overrides Sub DrawPie(pen As Pen, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)

        End Sub

        '
        ' Summary:
        '     Draws a polygon defined by an array of System.Drawing.Point structures.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the polygon.
        '
        '   points:
        '     Array of System.Drawing.Point structures that represent the vertices of the polygon.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.
        Public Overrides Sub DrawPolygon(pen As Pen, points() As Point)
            Call Graphics.DrawPolygon(pen, points)
        End Sub

        ''' <summary>
        ''' Draws a polygon defined by an array of System.Drawing.PointF structures.
        ''' </summary>
        ''' <param name="pen">System.Drawing.Pen that determines the color, width, and style of the polygon.</param>
        ''' <param name="points">Array of System.Drawing.PointF structures that represent the vertices of the
        ''' polygon.</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub DrawPolygon(pen As Pen, points() As PointF)
            Call Graphics.DrawPolygon(pen, points)
        End Sub

        ''' <summary>
        ''' Draws a rectangle specified by a System.Drawing.Rectangle structure.
        ''' </summary>
        ''' <param name="pen">A System.Drawing.Pen that determines the color, width, and style of the rectangle.</param>
        ''' <param name="rect">A System.Drawing.Rectangle structure that represents the rectangle to draw.</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub DrawRectangle(pen As Pen, rect As Rectangle)
            Call Graphics.DrawRectangle(pen, rect)
        End Sub
        '
        ' Summary:
        '     Draws a rectangle specified by a coordinate pair, a width, and a height.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the rectangle.
        '
        '   x:
        '     The x-coordinate of the upper-left corner of the rectangle to draw.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the rectangle to draw.
        '
        '   width:
        '     Width of the rectangle to draw.
        '
        '   height:
        '     Height of the rectangle to draw.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.
        Public Overrides Sub DrawRectangle(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer)
            Call Graphics.DrawRectangle(pen, x, y, width, height)
        End Sub
        '
        ' Summary:
        '     Draws a rectangle specified by a coordinate pair, a width, and a height.
        '
        ' Parameters:
        '   pen:
        '     A System.Drawing.Pen that determines the color, width, and style of the rectangle.
        '
        '   x:
        '     The x-coordinate of the upper-left corner of the rectangle to draw.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the rectangle to draw.
        '
        '   width:
        '     The width of the rectangle to draw.
        '
        '   height:
        '     The height of the rectangle to draw.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.
        Public Overrides Sub DrawRectangle(pen As Pen, x As Single, y As Single, width As Single, height As Single)
            Call Graphics.DrawRectangle(pen, x, y, width, height)
        End Sub
        '
        ' Summary:
        '     Draws a series of rectangles specified by System.Drawing.Rectangle structures.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the outlines
        '     of the rectangles.
        '
        '   rects:
        '     Array of System.Drawing.Rectangle structures that represent the rectangles to
        '     draw.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-rects is null.
        '
        '   T:System.ArgumentException:
        '     rects is a zero-length array.
        Public Overrides Sub DrawRectangles(pen As Pen, rects() As Rectangle)

        End Sub
        '
        ' Summary:
        '     Draws a series of rectangles specified by System.Drawing.RectangleF structures.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the outlines
        '     of the rectangles.
        '
        '   rects:
        '     Array of System.Drawing.RectangleF structures that represent the rectangles to
        '     draw.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-rects is null.
        '
        '   T:System.ArgumentException:
        '     rects is a zero-length array.
        Public Overrides Sub DrawRectangles(pen As Pen, rects() As RectangleF)

        End Sub

        ''' <summary>
        ''' Draws the specified text string in the specified rectangle with the specified
        ''' System.Drawing.Brush and System.Drawing.Font objects.
        ''' </summary>
        ''' <param name="s">String to draw.</param>
        ''' <param name="font">System.Drawing.Font that defines the text format of the string.</param>
        ''' <param name="brush">System.Drawing.Brush that determines the color and texture of the drawn text.</param>
        ''' <param name="layoutRectangle">System.Drawing.RectangleF structure that specifies the location of the drawn
        ''' text.</param>
        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, layoutRectangle As RectangleF)
            Call Graphics.DrawString(s, font, brush, layoutRectangle)
        End Sub

        ''' <summary>
        ''' Draws the specified text string at the specified location with the specified
        ''' <see cref="System.Drawing.Brush"/> and <see cref="System.Drawing.Font"/> objects.
        ''' </summary>
        ''' <param name="s">String to draw.</param>
        ''' <param name="font">System.Drawing.Font that defines the text format of the string.</param>
        ''' <param name="brush">System.Drawing.Brush that determines the color and texture of the drawn text.</param>
        ''' <param name="point">System.Drawing.PointF structure that specifies the upper-left corner of the drawn
        ''' text.</param>
        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, ByRef point As PointF)
            Call Graphics.DrawString(s, font, brush, point)
        End Sub

        '
        ' Summary:
        '     Draws the specified text string at the specified location with the specified
        '     System.Drawing.Brush and System.Drawing.Font objects.
        '
        ' Parameters:
        '   s:
        '     String to draw.
        '
        '   font:
        '     System.Drawing.Font that defines the text format of the string.
        '
        '   brush:
        '     System.Drawing.Brush that determines the color and texture of the drawn text.
        '
        '   x:
        '     The x-coordinate of the upper-left corner of the drawn text.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the drawn text.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.-or-s is null.
        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, x As Single, y As Single)
            Call Graphics.DrawString(s, font, brush, x, y)
        End Sub

        '
        ' Summary:
        '     Updates the clip region of this System.Drawing.Graphics to exclude the area specified
        '     by a System.Drawing.Rectangle structure.
        '
        ' Parameters:
        '   rect:
        '     System.Drawing.Rectangle structure that specifies the rectangle to exclude from
        '     the clip region.
        Public Overrides Sub ExcludeClip(rect As Rectangle)

        End Sub
        '
        ' Summary:
        '     Fills the interior of a closed cardinal spline curve defined by an array of System.Drawing.Point
        '     structures.
        '
        ' Parameters:
        '   brush:
        '     System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   points:
        '     Array of System.Drawing.Point structures that define the spline.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.-or-points is null.
        Public Overrides Sub FillClosedCurve(brush As Brush, points() As Point)

        End Sub
        '
        ' Summary:
        '     Fills the interior of a closed cardinal spline curve defined by an array of System.Drawing.PointF
        '     structures.
        '
        ' Parameters:
        '   brush:
        '     System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   points:
        '     Array of System.Drawing.PointF structures that define the spline.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.-or-points is null.
        Public Overrides Sub FillClosedCurve(brush As Brush, points() As PointF)

        End Sub

        '
        ' Summary:
        '     Fills the interior of an ellipse defined by a bounding rectangle specified by
        '     a System.Drawing.Rectangle structure.
        '
        ' Parameters:
        '   brush:
        '     System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   rect:
        '     System.Drawing.Rectangle structure that represents the bounding rectangle that
        '     defines the ellipse.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.
        Public Overrides Sub FillEllipse(brush As Brush, rect As Rectangle)
            Call Graphics.FillEllipse(brush, rect)
        End Sub

        ''' <summary>
        ''' Fills the interior of an ellipse defined by a bounding rectangle specified by
        ''' a System.Drawing.RectangleF structure.
        ''' </summary>
        ''' <param name="brush">System.Drawing.Brush that determines the characteristics of the fill.</param>
        ''' <param name="rect">System.Drawing.RectangleF structure that represents the bounding rectangle that
        ''' defines the ellipse.</param>
        Public Overrides Sub FillEllipse(brush As Brush, rect As RectangleF)
            Call Graphics.FillEllipse(brush, rect)
        End Sub
        '
        ' Summary:
        '     Fills the interior of an ellipse defined by a bounding rectangle specified by
        '     a pair of coordinates, a width, and a height.
        '
        ' Parameters:
        '   brush:
        '     System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   x:
        '     The x-coordinate of the upper-left corner of the bounding rectangle that defines
        '     the ellipse.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the bounding rectangle that defines
        '     the ellipse.
        '
        '   width:
        '     Width of the bounding rectangle that defines the ellipse.
        '
        '   height:
        '     Height of the bounding rectangle that defines the ellipse.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.
        Public Overrides Sub FillEllipse(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer)
            Call Graphics.FillEllipse(brush, x, y, width, height)
        End Sub

        ''' <summary>
        ''' Fills the interior of an ellipse defined by a bounding rectangle specified by
        ''' a pair of coordinates, a width, and a height.
        ''' </summary>
        ''' <param name="brush">System.Drawing.Brush that determines the characteristics of the fill.</param>
        ''' <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines
        ''' the ellipse.</param>
        ''' <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines
        ''' the ellipse.</param>
        ''' <param name="width">Width of the bounding rectangle that defines the ellipse.</param>
        ''' <param name="height">Height of the bounding rectangle that defines the ellipse.</param>
        Public Overrides Sub FillEllipse(brush As Brush, x As Single, y As Single, width As Single, height As Single)
            Call Graphics.FillEllipse(brush, x, y, width, height)
        End Sub

        ''' <summary>
        ''' Fills the interior of a System.Drawing.Drawing2D.GraphicsPath.
        ''' </summary>
        ''' <param name="brush">System.Drawing.Brush that determines the characteristics of the fill.</param>
        ''' <param name="path">System.Drawing.Drawing2D.GraphicsPath that represents the path to fill.</param>
        Public Overrides Sub FillPath(brush As Brush, path As GraphicsPath)
            Call Graphics.FillPath(brush, path)
        End Sub
        '
        ' Summary:
        '     Fills the interior of a pie section defined by an ellipse specified by a System.Drawing.RectangleF
        '     structure and two radial lines.
        '
        ' Parameters:
        '   brush:
        '     System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   rect:
        '     System.Drawing.Rectangle structure that represents the bounding rectangle that
        '     defines the ellipse from which the pie section comes.
        '
        '   startAngle:
        '     Angle in degrees measured clockwise from the x-axis to the first side of the
        '     pie section.
        '
        '   sweepAngle:
        '     Angle in degrees measured clockwise from the startAngle parameter to the second
        '     side of the pie section.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.
        Public Overrides Sub FillPie(brush As Brush, rect As Rectangle, startAngle As Single, sweepAngle As Single)
            Call Graphics.FillPie(brush, rect, startAngle, sweepAngle)
        End Sub
        '
        ' Summary:
        '     Fills the interior of a pie section defined by an ellipse specified by a pair
        '     of coordinates, a width, a height, and two radial lines.
        '
        ' Parameters:
        '   brush:
        '     System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   x:
        '     The x-coordinate of the upper-left corner of the bounding rectangle that defines
        '     the ellipse from which the pie section comes.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the bounding rectangle that defines
        '     the ellipse from which the pie section comes.
        '
        '   width:
        '     Width of the bounding rectangle that defines the ellipse from which the pie section
        '     comes.
        '
        '   height:
        '     Height of the bounding rectangle that defines the ellipse from which the pie
        '     section comes.
        '
        '   startAngle:
        '     Angle in degrees measured clockwise from the x-axis to the first side of the
        '     pie section.
        '
        '   sweepAngle:
        '     Angle in degrees measured clockwise from the startAngle parameter to the second
        '     side of the pie section.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.
        Public Overrides Sub FillPie(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)
            Call Graphics.FillPie(brush, x, y, width, height, startAngle, sweepAngle)
        End Sub

        ''' <summary>
        ''' Fills the interior of a pie section defined by an ellipse specified by a pair
        ''' of coordinates, a width, a height, and two radial lines.
        ''' </summary>
        ''' <param name="brush">System.Drawing.Brush that determines the characteristics of the fill.</param>
        ''' <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines
        ''' the ellipse from which the pie section comes.</param>
        ''' <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines
        ''' the ellipse from which the pie section comes.</param>
        ''' <param name="width">Width of the bounding rectangle that defines the ellipse from which the pie section
        ''' comes.</param>
        ''' <param name="height">Height of the bounding rectangle that defines the ellipse from which the pie
        ''' section comes.</param>
        ''' <param name="startAngle">Angle in degrees measured clockwise from the x-axis to the first side of the
        ''' pie section.</param>
        ''' <param name="sweepAngle">Angle in degrees measured clockwise from the startAngle parameter to the second
        ''' side of the pie section.</param>
        Public Overrides Sub FillPie(brush As Brush, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)
            If x < 0 OrElse y < 0 Then
                Return
            End If
            Call Graphics.FillPie(brush, x, y, width, height, startAngle, sweepAngle)
        End Sub
        '
        ' Summary:
        '     Fills the interior of a polygon defined by an array of points specified by System.Drawing.Point
        '     structures.
        '
        ' Parameters:
        '   brush:
        '     System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   points:
        '     Array of System.Drawing.Point structures that represent the vertices of the polygon
        '     to fill.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.-or-points is null.
        Public Overrides Sub FillPolygon(brush As Brush, points() As Point)
            Call Graphics.FillPolygon(brush, points)
        End Sub
        '
        ' Summary:
        '     Fills the interior of a polygon defined by an array of points specified by System.Drawing.PointF
        '     structures.
        '
        ' Parameters:
        '   brush:
        '     System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   points:
        '     Array of System.Drawing.PointF structures that represent the vertices of the
        '     polygon to fill.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.-or-points is null.
        Public Overrides Sub FillPolygon(brush As Brush, points() As PointF)
            Call Graphics.FillPolygon(brush, points)
        End Sub

        ''' <summary>
        ''' Fills the interior of a rectangle specified by a System.Drawing.Rectangle structure.
        ''' </summary>
        ''' <param name="brush">System.Drawing.Brush that determines the characteristics of the fill.</param>
        ''' <param name="rect">System.Drawing.Rectangle structure that represents the rectangle to fill.</param>
        Public Overrides Sub FillRectangle(brush As Brush, rect As Rectangle)
            Call Graphics.FillRectangle(brush, rect)
        End Sub
        '
        ' Summary:
        '     Fills the interior of a rectangle specified by a System.Drawing.RectangleF structure.
        '
        ' Parameters:
        '   brush:
        '     System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   rect:
        '     System.Drawing.RectangleF structure that represents the rectangle to fill.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.
        Public Overrides Sub FillRectangle(brush As Brush, rect As RectangleF)
            Call Graphics.FillRectangle(brush, rect)
        End Sub
        '
        ' Summary:
        '     Fills the interior of a rectangle specified by a pair of coordinates, a width,
        '     and a height.
        '
        ' Parameters:
        '   brush:
        '     System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   x:
        '     The x-coordinate of the upper-left corner of the rectangle to fill.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the rectangle to fill.
        '
        '   width:
        '     Width of the rectangle to fill.
        '
        '   height:
        '     Height of the rectangle to fill.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.
        Public Overrides Sub FillRectangle(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer)

        End Sub
        '
        ' Summary:
        '     Fills the interior of a rectangle specified by a pair of coordinates, a width,
        '     and a height.
        '
        ' Parameters:
        '   brush:
        '     System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   x:
        '     The x-coordinate of the upper-left corner of the rectangle to fill.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the rectangle to fill.
        '
        '   width:
        '     Width of the rectangle to fill.
        '
        '   height:
        '     Height of the rectangle to fill.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.
        Public Overrides Sub FillRectangle(brush As Brush, x As Single, y As Single, width As Single, height As Single)

        End Sub



        ''' <summary>
        ''' Forces execution of all pending graphics operations and returns immediately without
        ''' waiting for the operations to finish.
        ''' </summary>
        Public Overrides Sub Flush()
            Call Graphics.Flush()
        End Sub

        '
        ' Summary:
        '     Updates the clip region of this System.Drawing.Graphics to the intersection of
        '     the current clip region and the specified System.Drawing.RectangleF structure.
        '
        ' Parameters:
        '   rect:
        '     System.Drawing.RectangleF structure to intersect with the current clip region.
        Public Overrides Sub IntersectClip(rect As RectangleF)

        End Sub

        '
        ' Summary:
        '     Updates the clip region of this System.Drawing.Graphics to the intersection of
        '     the current clip region and the specified System.Drawing.Rectangle structure.
        '
        ' Parameters:
        '   rect:
        '     System.Drawing.Rectangle structure to intersect with the current clip region.
        Public Overrides Sub IntersectClip(rect As Rectangle)

        End Sub

        ''' <summary>
        ''' Resets the clip region of this System.Drawing.Graphics to an infinite region.
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub ResetClip()
            Call Graphics.ResetClip()
        End Sub

        ''' <summary>
        ''' Resets the world transformation matrix of this System.Drawing.Graphics to the
        ''' identity matrix.
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub ResetTransform()
            Call Graphics.ResetTransform()
        End Sub

        ''' <summary>
        ''' Applies the specified rotation to the transformation matrix of this System.Drawing.Graphics.
        ''' </summary>
        ''' <param name="angle">Angle of rotation in degrees.</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub RotateTransform(angle As Single)
            Call Graphics.RotateTransform(angle)
        End Sub

        ''' <summary>
        ''' Applies the specified scaling operation to the transformation matrix of this
        ''' System.Drawing.Graphics by prepending it to the object's transformation matrix.
        ''' </summary>
        ''' <param name="sx">Scale factor in the x direction.</param>
        ''' <param name="sy">Scale factor in the y direction.</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub ScaleTransform(sx As Single, sy As Single)
            Call Graphics.ScaleTransform(sx, sy)
        End Sub

        '
        ' Summary:
        '     Sets the clipping region of this System.Drawing.Graphics to the rectangle specified
        '     by a System.Drawing.Rectangle structure.
        '
        ' Parameters:
        '   rect:
        '     System.Drawing.Rectangle structure that represents the new clip region.
        Public Overrides Sub SetClip(rect As Rectangle)

        End Sub
        '
        ' Summary:
        '     Sets the clipping region of this System.Drawing.Graphics to the rectangle specified
        '     by a System.Drawing.RectangleF structure.
        '
        ' Parameters:
        '   rect:
        '     System.Drawing.RectangleF structure that represents the new clip region.
        Public Overrides Sub SetClip(rect As RectangleF)

        End Sub

        '
        ' Summary:
        '     Translates the clipping region of this System.Drawing.Graphics by specified amounts
        '     in the horizontal and vertical directions.
        '
        ' Parameters:
        '   dx:
        '     The x-coordinate of the translation.
        '
        '   dy:
        '     The y-coordinate of the translation.
        Public Overrides Sub TranslateClip(dx As Integer, dy As Integer)

        End Sub
        '
        ' Summary:
        '     Translates the clipping region of this System.Drawing.Graphics by specified amounts
        '     in the horizontal and vertical directions.
        '
        ' Parameters:
        '   dx:
        '     The x-coordinate of the translation.
        '
        '   dy:
        '     The y-coordinate of the translation.
        Public Overrides Sub TranslateClip(dx As Single, dy As Single)

        End Sub

        ''' <summary>
        ''' Changes the origin of the coordinate system by prepending the specified translation
        ''' to the transformation matrix of this System.Drawing.Graphics.
        ''' </summary>
        ''' <param name="dx">The x-coordinate of the translation.</param>
        ''' <param name="dy">The y-coordinate of the translation.</param>
        Public Overrides Sub TranslateTransform(dx As Single, dy As Single)
            Call Graphics.TranslateTransform(dx, dy)
        End Sub

        Protected Overrides Sub Finalize()
            Call Graphics.Dispose()
        End Sub

        '
        ' Summary:
        '     Gets the cumulative graphics context.
        '
        ' Returns:
        '     An System.Object representing the cumulative graphics context.
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Overrides Function GetContextInfo() As Object
            Return Graphics.GetContextInfo
        End Function

        '
        ' Summary:
        '     Gets the nearest color to the specified System.Drawing.Color structure.
        '
        ' Parameters:
        '   color:
        '     System.Drawing.Color structure for which to find a match.
        '
        ' Returns:
        '     A System.Drawing.Color structure that represents the nearest color to the one
        '     specified with the color parameter.
        Public Overrides Function GetNearestColor(color As Color) As Color
            Return Graphics.GetNearestColor(color)
        End Function
        '
        ' Summary:
        '     Indicates whether the rectangle specified by a System.Drawing.Rectangle structure
        '     is contained within the visible clip region of this System.Drawing.Graphics.
        '
        ' Parameters:
        '   rect:
        '     System.Drawing.Rectangle structure to test for visibility.
        '
        ' Returns:
        '     true if the rectangle specified by the rect parameter is contained within the
        '     visible clip region of this System.Drawing.Graphics; otherwise, false.
        Public Overrides Function IsVisible(rect As Rectangle) As Boolean
            Return Graphics.IsVisible(rect)
        End Function
        '
        ' Summary:
        '     Indicates whether the rectangle specified by a System.Drawing.RectangleF structure
        '     is contained within the visible clip region of this System.Drawing.Graphics.
        '
        ' Parameters:
        '   rect:
        '     System.Drawing.RectangleF structure to test for visibility.
        '
        ' Returns:
        '     true if the rectangle specified by the rect parameter is contained within the
        '     visible clip region of this System.Drawing.Graphics; otherwise, false.
        Public Overrides Function IsVisible(rect As RectangleF) As Boolean
            Return Graphics.IsVisible(rect)
        End Function
        '
        ' Summary:
        '     Indicates whether the specified System.Drawing.PointF structure is contained
        '     within the visible clip region of this System.Drawing.Graphics.
        '
        ' Parameters:
        '   point:
        '     System.Drawing.PointF structure to test for visibility.
        '
        ' Returns:
        '     true if the point specified by the point parameter is contained within the visible
        '     clip region of this System.Drawing.Graphics; otherwise, false.
        'Public Overrides Function IsVisible(point As PointF) As Boolean
        '    Return Graphics.IsVisible(point)
        'End Function
        '
        ' Summary:
        '     Indicates whether the specified System.Drawing.Point structure is contained within
        '     the visible clip region of this System.Drawing.Graphics.
        '
        ' Parameters:
        '   point:
        '     System.Drawing.Point structure to test for visibility.
        '
        ' Returns:
        '     true if the point specified by the point parameter is contained within the visible
        '     clip region of this System.Drawing.Graphics; otherwise, false.
        'Public Overrides Function IsVisible(point As Point) As Boolean
        '    Return Graphics.IsVisible(point)
        'End Function
        '
        ' Summary:
        '     Indicates whether the point specified by a pair of coordinates is contained within
        '     the visible clip region of this System.Drawing.Graphics.
        '
        ' Parameters:
        '   x:
        '     The x-coordinate of the point to test for visibility.
        '
        '   y:
        '     The y-coordinate of the point to test for visibility.
        '
        ' Returns:
        '     true if the point defined by the x and y parameters is contained within the visible
        '     clip region of this System.Drawing.Graphics; otherwise, false.
        'Public Overrides Function IsVisible(x As Single, y As Single) As Boolean
        '    Return Graphics.IsVisible(x, y)
        'End Function
        '
        ' Summary:
        '     Indicates whether the point specified by a pair of coordinates is contained within
        '     the visible clip region of this System.Drawing.Graphics.
        '
        ' Parameters:
        '   x:
        '     The x-coordinate of the point to test for visibility.
        '
        '   y:
        '     The y-coordinate of the point to test for visibility.
        '
        ' Returns:
        '     true if the point defined by the x and y parameters is contained within the visible
        '     clip region of this System.Drawing.Graphics; otherwise, false.
        'Public Overrides Function IsVisible(x As Integer, y As Integer) As Boolean
        '    Return Graphics.IsVisible(x, y)
        'End Function
        '
        ' Summary:
        '     Indicates whether the rectangle specified by a pair of coordinates, a width,
        '     and a height is contained within the visible clip region of this System.Drawing.Graphics.
        '
        ' Parameters:
        '   x:
        '     The x-coordinate of the upper-left corner of the rectangle to test for visibility.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the rectangle to test for visibility.
        '
        '   width:
        '     Width of the rectangle to test for visibility.
        '
        '   height:
        '     Height of the rectangle to test for visibility.
        '
        ' Returns:
        '     true if the rectangle defined by the x, y, width, and height parameters is contained
        '     within the visible clip region of this System.Drawing.Graphics; otherwise, false.
        Public Overrides Function IsVisible(x As Single, y As Single, width As Single, height As Single) As Boolean
            Return Graphics.IsVisible(x, y, width, height)
        End Function
        '
        ' Summary:
        '     Indicates whether the rectangle specified by a pair of coordinates, a width,
        '     and a height is contained within the visible clip region of this System.Drawing.Graphics.
        '
        ' Parameters:
        '   x:
        '     The x-coordinate of the upper-left corner of the rectangle to test for visibility.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the rectangle to test for visibility.
        '
        '   width:
        '     Width of the rectangle to test for visibility.
        '
        '   height:
        '     Height of the rectangle to test for visibility.
        '
        ' Returns:
        '     true if the rectangle defined by the x, y, width, and height parameters is contained
        '     within the visible clip region of this System.Drawing.Graphics; otherwise, false.
        Public Overrides Function IsVisible(x As Integer, y As Integer, width As Integer, height As Integer) As Boolean
            Return Graphics.IsVisible(x, y, width, height)
        End Function

        ''' <summary>
        ''' Measures the specified string when drawn with the specified <see cref="Font"/>.
        ''' </summary>
        ''' <param name="text">String to measure.</param>
        ''' <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
        ''' <returns>This method returns a System.Drawing.SizeF structure that represents the size,
        ''' in the units specified by the <see cref="PageUnit"/> property, of the
        ''' string specified by the text parameter as drawn with the font parameter.</returns>
        Public Overrides Function MeasureString(text$, font As Font) As SizeF
            Return Graphics.MeasureString(text, font)
        End Function

        ''' <summary>
        ''' Using <see cref="Font"/>
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Public Overloads Function MeasureString(text As String) As SizeF
            Return Graphics.MeasureString(text, Font)
        End Function

        ' Summary:
        '     Measures the specified string when drawn with the specified System.Drawing.Font.
        '
        ' Parameters:
        '   text:
        '     String to measure.
        '
        '   font:
        '     System.Drawing.Font that defines the format of the string.
        '
        '   width:
        '     Maximum width of the string in pixels.
        '
        ' Returns:
        '     This method returns a System.Drawing.SizeF structure that represents the size,
        '     in the units specified by the System.Drawing.Graphics.PageUnit property, of the
        '     string specified in the text parameter as drawn with the font parameter.
        '
        ' Exceptions:
        '   T:System.ArgumentException:
        '     font is null.
        Public Overrides Function MeasureString(text As String, font As Font, width As Integer) As SizeF
            Return Graphics.MeasureString(text, font, width)
        End Function
        '
        ' Summary:
        '     Measures the specified string when drawn with the specified System.Drawing.Font
        '     within the specified layout area.
        '
        ' Parameters:
        '   text:
        '     String to measure.
        '
        '   font:
        '     System.Drawing.Font defines the text format of the string.
        '
        '   layoutArea:
        '     System.Drawing.SizeF structure that specifies the maximum layout area for the
        '     text.
        '
        ' Returns:
        '     This method returns a System.Drawing.SizeF structure that represents the size,
        '     in the units specified by the System.Drawing.Graphics.PageUnit property, of the
        '     string specified by the text parameter as drawn with the font parameter.
        '
        ' Exceptions:
        '   T:System.ArgumentException:
        '     font is null.
        Public Overrides Function MeasureString(text As String, font As Font, layoutArea As SizeF) As SizeF
            Return Graphics.MeasureString(text, font, layoutArea)
        End Function

        Public Overrides Sub DrawBezier(pen As Pen, pt1 As Point, pt2 As Point, pt3 As Point, pt4 As Point)
            Call Graphics.DrawBezier(pen, pt1, pt2, pt3, pt4)
        End Sub

        Public Overrides Sub DrawRectangle(pen As Pen, rect As RectangleF)
            Call Graphics.DrawRectangles(pen, {rect})
        End Sub
#End Region

        ''' <summary>
        ''' Releases all resources used by this <see cref="Graphics"/>.
        ''' </summary>
        Public Overrides Sub Dispose() Implements IDisposable.Dispose
            ' 在这里不应该将图片资源给消灭掉，只需要释放掉gdi+资源就行了
            Call Graphics.Dispose()
        End Sub
    End Class
End Namespace
