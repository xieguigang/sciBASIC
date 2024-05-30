#Region "Microsoft.VisualBasic::29010f0a63140f389eebe42645c2ba02, Microsoft.VisualBasic.Core\src\Extensions\Image\GDI+\GDICanvas.vb"

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

    '   Total Lines: 4885
    '    Code Lines: 688 (14.08%)
    ' Comment Lines: 4022 (82.33%)
    '    - Xml Docs: 6.94%
    ' 
    '   Blank Lines: 175 (3.58%)
    '     File Size: 211.61 KB


    '     Class GDICanvas
    ' 
    '         Properties: CompositingMode, CompositingQuality, DpiX, DpiY, Font
    '                     Graphics, InterpolationMode, IsClipEmpty, IsVisibleClipEmpty, PageScale
    '                     PageUnit, PixelOffsetMode, RenderingOrigin, SmoothingMode, Stroke
    '                     TextContrast, TextRenderingHint
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+3 Overloads) BeginContainer, GetContextInfo, GetNearestColor, (+8 Overloads) IsVisible, MeasureCharacterRanges
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

        Public Overrides Property CompositingMode As CompositingMode
            Get
                Return g.CompositingMode
            End Get
            Set(value As CompositingMode)
                g.CompositingMode = value
            End Set
        End Property

        Public Overrides Property CompositingQuality As CompositingQuality
            Get
                Return g.CompositingQuality
            End Get
            Set(value As CompositingQuality)
                g.CompositingQuality = value
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
        ''' Gets the horizontal resolution of this System.Drawing.Graphics.
        ''' </summary>
        ''' <returns>
        ''' The value, in dots per inch, for the horizontal resolution supported by this System.Drawing.Graphics.
        ''' </returns>
        Public Overrides ReadOnly Property DpiX As Single
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Graphics.DpiX
            End Get
        End Property

        ''' <summary>
        ''' Gets the vertical resolution of this System.Drawing.Graphics.
        ''' </summary>
        ''' <returns>The value, in dots per inch, for the vertical resolution supported by this System.Drawing.Graphics.</returns>
        Public Overrides ReadOnly Property DpiY As Single
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Graphics.DpiY
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the interpolation mode associated with this System.Drawing.Graphics.
        ''' </summary>
        ''' <returns>One of the System.Drawing.Drawing2D.InterpolationMode values.</returns>
        Public Overrides Property InterpolationMode As InterpolationMode
            Get
                Return Graphics.InterpolationMode
            End Get
            Set(value As InterpolationMode)
                g.InterpolationMode = value
            End Set
        End Property
        '
        ' Summary:
        '     Gets a value indicating whether the clipping region of this System.Drawing.Graphics
        '     is empty.
        '
        ' Returns:
        '     true if the clipping region of this System.Drawing.Graphics is empty; otherwise,
        '     false.
        Public Overrides ReadOnly Property IsClipEmpty As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Graphics.IsClipEmpty
            End Get
        End Property
        '
        ' Summary:
        '     Gets a value indicating whether the visible clipping region of this System.Drawing.Graphics
        '     is empty.
        '
        ' Returns:
        '     true if the visible portion of the clipping region of this System.Drawing.Graphics
        '     is empty; otherwise, false.
        Public Overrides ReadOnly Property IsVisibleClipEmpty As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Graphics.IsVisibleClipEmpty
            End Get
        End Property

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
        '     Gets or sets the unit of measure used for page coordinates in this System.Drawing.Graphics.
        '
        ' Returns:
        '     One of the System.Drawing.GraphicsUnit values other than System.Drawing.GraphicsUnit.World.
        '
        ' Exceptions:
        '   T:System.ComponentModel.InvalidEnumArgumentException:
        '     System.Drawing.Graphics.PageUnit is set to System.Drawing.GraphicsUnit.World,
        '     which is not a physical unit.
        Public Overrides Property PageUnit As GraphicsUnit
            Get
                Return Graphics.PageUnit
            End Get
            Set(value As GraphicsUnit)
                Graphics.PageUnit = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or set a value specifying how pixels are offset during rendering of this
        ''' System.Drawing.Graphics.
        ''' </summary>
        ''' <returns>This property specifies a member of the System.Drawing.Drawing2D.PixelOffsetMode
        ''' enumeration</returns>
        Public Overrides Property PixelOffsetMode As PixelOffsetMode
            Get
                Return Graphics.PixelOffsetMode
            End Get
            Set(value As PixelOffsetMode)
                Graphics.PixelOffsetMode = value
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

        ''' <summary>
        ''' Gets or sets the rendering quality for this System.Drawing.Graphics.
        ''' </summary>
        ''' <returns>One of the System.Drawing.Drawing2D.SmoothingMode values.</returns>
        Public Overrides Property SmoothingMode As SmoothingMode
            Get
                Return Graphics.SmoothingMode
            End Get
            Set(value As SmoothingMode)
                g.SmoothingMode = value
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

        ''' <summary>
        ''' Gets or sets the rendering mode for text associated with this System.Drawing.Graphics.
        ''' </summary>
        ''' <returns>One of the System.Drawing.Text.TextRenderingHint values.</returns>
        Public Overrides Property TextRenderingHint As TextRenderingHint
            Get
                Return Graphics.TextRenderingHint
            End Get
            Set(value As TextRenderingHint)
                Graphics.TextRenderingHint = value
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

        ' Summary:
        '     Performs a bit-block transfer of color data, corresponding to a rectangle of
        '     pixels, from the screen to the drawing surface of the System.Drawing.Graphics.
        '
        ' Parameters:
        '   upperLeftSource:
        '     The point at the upper-left corner of the source rectangle.
        '
        '   upperLeftDestination:
        '     The point at the upper-left corner of the destination rectangle.
        '
        '   blockRegionSize:
        '     The size of the area to be transferred.
        '
        ' Exceptions:
        '   T:System.ComponentModel.Win32Exception:
        '     The operation failed.
        Public Overrides Sub CopyFromScreen(upperLeftSource As Point, upperLeftDestination As Point, blockRegionSize As Size)
            Call Graphics.CopyFromScreen(upperLeftSource, upperLeftDestination, blockRegionSize)
        End Sub
        '
        ' Summary:
        '     Performs a bit-block transfer of color data, corresponding to a rectangle of
        '     pixels, from the screen to the drawing surface of the System.Drawing.Graphics.
        '
        ' Parameters:
        '   upperLeftSource:
        '     The point at the upper-left corner of the source rectangle.
        '
        '   upperLeftDestination:
        '     The point at the upper-left corner of the destination rectangle.
        '
        '   blockRegionSize:
        '     The size of the area to be transferred.
        '
        '   copyPixelOperation:
        '     One of the System.Drawing.CopyPixelOperation values.
        '
        ' Exceptions:
        '   T:System.ComponentModel.InvalidEnumArgumentException:
        '     copyPixelOperation is not a member of System.Drawing.CopyPixelOperation.
        '
        '   T:System.ComponentModel.Win32Exception:
        '     The operation failed.
        Public Overrides Sub CopyFromScreen(upperLeftSource As Point, upperLeftDestination As Point, blockRegionSize As Size, copyPixelOperation As CopyPixelOperation)
            Call Graphics.CopyFromScreen(upperLeftSource, upperLeftDestination, blockRegionSize, copyPixelOperation)
        End Sub
        '
        ' Summary:
        '     Performs a bit-block transfer of the color data, corresponding to a rectangle
        '     of pixels, from the screen to the drawing surface of the System.Drawing.Graphics.
        '
        ' Parameters:
        '   sourceX:
        '     The x-coordinate of the point at the upper-left corner of the source rectangle.
        '
        '   sourceY:
        '     The y-coordinate of the point at the upper-left corner of the source rectangle.
        '
        '   destinationX:
        '     The x-coordinate of the point at the upper-left corner of the destination rectangle.
        '
        '   destinationY:
        '     The y-coordinate of the point at the upper-left corner of the destination rectangle.
        '
        '   blockRegionSize:
        '     The size of the area to be transferred.
        '
        ' Exceptions:
        '   T:System.ComponentModel.Win32Exception:
        '     The operation failed.
        Public Overrides Sub CopyFromScreen(sourceX As Integer, sourceY As Integer, destinationX As Integer, destinationY As Integer, blockRegionSize As Size)
            Call Graphics.CopyFromScreen(sourceX, sourceY, destinationX, destinationY, blockRegionSize)
        End Sub
        '
        ' Summary:
        '     Performs a bit-block transfer of the color data, corresponding to a rectangle
        '     of pixels, from the screen to the drawing surface of the System.Drawing.Graphics.
        '
        ' Parameters:
        '   sourceX:
        '     The x-coordinate of the point at the upper-left corner of the source rectangle.
        '
        '   sourceY:
        '     The y-coordinate of the point at the upper-left corner of the source rectangle
        '
        '   destinationX:
        '     The x-coordinate of the point at the upper-left corner of the destination rectangle.
        '
        '   destinationY:
        '     The y-coordinate of the point at the upper-left corner of the destination rectangle.
        '
        '   blockRegionSize:
        '     The size of the area to be transferred.
        '
        '   copyPixelOperation:
        '     One of the System.Drawing.CopyPixelOperation values.
        '
        ' Exceptions:
        '   T:System.ComponentModel.InvalidEnumArgumentException:
        '     copyPixelOperation is not a member of System.Drawing.CopyPixelOperation.
        '
        '   T:System.ComponentModel.Win32Exception:
        '     The operation failed.
        Public Overrides Sub CopyFromScreen(sourceX As Integer, sourceY As Integer, destinationX As Integer, destinationY As Integer, blockRegionSize As Size, copyPixelOperation As CopyPixelOperation)
            Call Graphics.CopyFromScreen(sourceX, sourceY, destinationX, destinationY, blockRegionSize, copyPixelOperation)
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
        '     Draws a closed cardinal spline defined by an array of System.Drawing.Point structures
        '     using a specified tension.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and height of the curve.
        '
        '   points:
        '     Array of System.Drawing.Point structures that define the spline.
        '
        '   tension:
        '     Value greater than or equal to 0.0F that specifies the tension of the curve.
        '
        '   fillmode:
        '     Member of the System.Drawing.Drawing2D.FillMode enumeration that determines how
        '     the curve is filled. This parameter is required but ignored.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-points is null.
        Public Overrides Sub DrawClosedCurve(pen As Pen, points() As Point, tension As Single, fillmode As FillMode)

        End Sub
        '
        ' Summary:
        '     Draws a closed cardinal spline defined by an array of System.Drawing.PointF structures
        '     using a specified tension.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and height of the curve.
        '
        '   points:
        '     Array of System.Drawing.PointF structures that define the spline.
        '
        '   tension:
        '     Value greater than or equal to 0.0F that specifies the tension of the curve.
        '
        '   fillmode:
        '     Member of the System.Drawing.Drawing2D.FillMode enumeration that determines how
        '     the curve is filled. This parameter is required but is ignored.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-points is null.
        Public Overrides Sub DrawClosedCurve(pen As Pen, points() As PointF, tension As Single, fillmode As FillMode)

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
        '     Draws the image represented by the specified System.Drawing.Icon within the area
        '     specified by a System.Drawing.Rectangle structure.
        '
        ' Parameters:
        '   icon:
        '     System.Drawing.Icon to draw.
        '
        '   targetRect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     resulting image on the display surface. The image contained in the icon parameter
        '     is scaled to the dimensions of this rectangular area.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     icon is null.
        Public Overrides Sub DrawIcon(icon As Icon, targetRect As Rectangle)
            Call Graphics.DrawIcon(icon, targetRect)
        End Sub
        '
        ' Summary:
        '     Draws the image represented by the specified System.Drawing.Icon at the specified
        '     coordinates.
        '
        ' Parameters:
        '   icon:
        '     System.Drawing.Icon to draw.
        '
        '   x:
        '     The x-coordinate of the upper-left corner of the drawn image.
        '
        '   y:
        '     The y-coordinate of the upper-left corner of the drawn image.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     icon is null.
        Public Overrides Sub DrawIcon(icon As Icon, x As Integer, y As Integer)
            Call Graphics.DrawIcon(icon, x, y)
        End Sub
        '
        ' Summary:
        '     Draws the image represented by the specified System.Drawing.Icon without scaling
        '     the image.
        '
        ' Parameters:
        '   icon:
        '     System.Drawing.Icon to draw.
        '
        '   targetRect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     resulting image. The image is not scaled to fit this rectangle, but retains its
        '     original size. If the image is larger than the rectangle, it is clipped to fit
        '     inside it.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     icon is null.
        Public Overrides Sub DrawIconUnstretched(icon As Icon, targetRect As Rectangle)
            Call Graphics.DrawIconUnstretched(icon, targetRect)
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
        '
        ' Summary:
        '     Draws the specified portion of the specified System.Drawing.Image at the specified
        '     location and with the specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destPoints:
        '     Array of three System.Drawing.Point structures that define a parallelogram.
        '
        '   srcRect:
        '     System.Drawing.Rectangle structure that specifies the portion of the image object
        '     to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used by the srcRect parameter.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit)
            Call Graphics.DrawImage(image, destPoints, srcRect, srcUnit)
        End Sub
        '
        ' Summary:
        '     Draws the specified portion of the specified System.Drawing.Image at the specified
        '     location and with the specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destRect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     drawn image. The image is scaled to fit the rectangle.
        '
        '   srcRect:
        '     System.Drawing.Rectangle structure that specifies the portion of the image object
        '     to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used by the srcRect parameter.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcRect As Rectangle, srcUnit As GraphicsUnit)
            Call Graphics.DrawImage(image, destRect, srcRect, srcUnit)
        End Sub

        ''' <summary>
        ''' Draws the specified portion of the specified System.Drawing.Image at the specified
        ''' location and with the specified size.
        ''' </summary>
        ''' <param name="image">System.Drawing.Image to draw.</param>
        ''' <param name="destPoints">Array of three System.Drawing.PointF structures that define a parallelogram.</param>
        ''' <param name="srcRect">System.Drawing.RectangleF structure that specifies the portion of the image object
        ''' to draw.</param>
        ''' <param name="srcUnit">Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        ''' of measure used by the srcRect parameter.</param>
        Public Overrides Sub DrawImage(image As Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit)
            Call Graphics.DrawImage(image, destPoints, srcRect, srcUnit)
        End Sub

        ''' <summary>
        ''' Draws the specified portion of the specified System.Drawing.Image at the specified
        ''' location and with the specified size.
        ''' </summary>
        ''' <param name="image">System.Drawing.Image to draw.</param>
        ''' <param name="destRect">System.Drawing.RectangleF structure that specifies the location and size of the
        ''' drawn image. The image is scaled to fit the rectangle.</param>
        ''' <param name="srcRect">System.Drawing.RectangleF structure that specifies the portion of the image object
        ''' to draw.</param>
        ''' <param name="srcUnit">Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        ''' of measure used by the srcRect parameter.</param>
        Public Overrides Sub DrawImage(image As Image, destRect As RectangleF, srcRect As RectangleF, srcUnit As GraphicsUnit)
            Call Graphics.DrawImage(image, destRect, srcRect, srcUnit)
        End Sub
        '
        ' Summary:
        '     Draws a portion of an image at a specified location.
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
        '   srcRect:
        '     System.Drawing.RectangleF structure that specifies the portion of the System.Drawing.Image
        '     to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used by the srcRect parameter.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, x As Single, y As Single, srcRect As RectangleF, srcUnit As GraphicsUnit)
            Graphics.DrawImage(image, x, y, srcRect, srcUnit)
        End Sub
        '
        ' Summary:
        '     Draws the specified portion of the specified System.Drawing.Image at the specified
        '     location.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destPoints:
        '     Array of three System.Drawing.Point structures that define a parallelogram.
        '
        '   srcRect:
        '     System.Drawing.Rectangle structure that specifies the portion of the image object
        '     to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used by the srcRect parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies recoloring and gamma information
        '     for the image object.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)
            Graphics.DrawImage(image, destPoints, srcRect, srcUnit, imageAttr)
        End Sub
        '
        ' Summary:
        '     Draws the specified portion of the specified System.Drawing.Image at the specified
        '     location and with the specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destPoints:
        '     Array of three System.Drawing.PointF structures that define a parallelogram.
        '
        '   srcRect:
        '     System.Drawing.RectangleF structure that specifies the portion of the image object
        '     to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used by the srcRect parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies recoloring and gamma information
        '     for the image object.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)

        End Sub
        '
        ' Summary:
        '     Draws a portion of an image at a specified location.
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
        '   srcRect:
        '     System.Drawing.Rectangle structure that specifies the portion of the image object
        '     to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used by the srcRect parameter.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, x As Integer, y As Integer, srcRect As Rectangle, srcUnit As GraphicsUnit)
            Call Graphics.DrawImage(image, x, y, srcRect, srcUnit)
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
        '     Draws the specified portion of the specified System.Drawing.Image at the specified
        '     location and with the specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destPoints:
        '     Array of three System.Drawing.PointF structures that define a parallelogram.
        '
        '   srcRect:
        '     System.Drawing.Rectangle structure that specifies the portion of the image object
        '     to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used by the srcRect parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies recoloring and gamma information
        '     for the image object.
        '
        '   callback:
        '     System.Drawing.Graphics.DrawImageAbort delegate that specifies a method to call
        '     during the drawing of the image. This method is called frequently to check whether
        '     to stop execution of the System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.Point[],System.Drawing.Rectangle,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort)
        '     method according to application-determined criteria.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort)

        End Sub
        '
        ' Summary:
        '     Draws the specified portion of the specified System.Drawing.Image at the specified
        '     location and with the specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destPoints:
        '     Array of three System.Drawing.PointF structures that define a parallelogram.
        '
        '   srcRect:
        '     System.Drawing.RectangleF structure that specifies the portion of the image object
        '     to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used by the srcRect parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies recoloring and gamma information
        '     for the image object.
        '
        '   callback:
        '     System.Drawing.Graphics.DrawImageAbort delegate that specifies a method to call
        '     during the drawing of the image. This method is called frequently to check whether
        '     to stop execution of the System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.PointF[],System.Drawing.RectangleF,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort)
        '     method according to application-determined criteria.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort)

        End Sub
        '
        ' Summary:
        '     Draws the specified portion of the specified System.Drawing.Image at the specified
        '     location and with the specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destRect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     drawn image. The image is scaled to fit the rectangle.
        '
        '   srcX:
        '     The x-coordinate of the upper-left corner of the portion of the source image
        '     to draw.
        '
        '   srcY:
        '     The y-coordinate of the upper-left corner of the portion of the source image
        '     to draw.
        '
        '   srcWidth:
        '     Width of the portion of the source image to draw.
        '
        '   srcHeight:
        '     Height of the portion of the source image to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used to determine the source rectangle.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit)

        End Sub
        '
        ' Summary:
        '     Draws the specified portion of the specified System.Drawing.Image at the specified
        '     location and with the specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destPoints:
        '     Array of three System.Drawing.PointF structures that define a parallelogram.
        '
        '   srcRect:
        '     System.Drawing.Rectangle structure that specifies the portion of the image object
        '     to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used by the srcRect parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies recoloring and gamma information
        '     for the image object.
        '
        '   callback:
        '     System.Drawing.Graphics.DrawImageAbort delegate that specifies a method to call
        '     during the drawing of the image. This method is called frequently to check whether
        '     to stop execution of the System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.Point[],System.Drawing.Rectangle,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort,System.Int32)
        '     method according to application-determined criteria.
        '
        '   callbackData:
        '     Value specifying additional data for the System.Drawing.Graphics.DrawImageAbort
        '     delegate to use when checking whether to stop execution of the System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.Point[],System.Drawing.Rectangle,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort,System.Int32)
        '     method.
        Public Overrides Sub DrawImage(image As Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort, callbackData As Integer)

        End Sub
        '
        ' Summary:
        '     Draws the specified portion of the specified System.Drawing.Image at the specified
        '     location and with the specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destRect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     drawn image. The image is scaled to fit the rectangle.
        '
        '   srcX:
        '     The x-coordinate of the upper-left corner of the portion of the source image
        '     to draw.
        '
        '   srcY:
        '     The y-coordinate of the upper-left corner of the portion of the source image
        '     to draw.
        '
        '   srcWidth:
        '     Width of the portion of the source image to draw.
        '
        '   srcHeight:
        '     Height of the portion of the source image to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used to determine the source rectangle.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit)

        End Sub
        '
        ' Summary:
        '     Draws the specified portion of the specified System.Drawing.Image at the specified
        '     location and with the specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destPoints:
        '     Array of three System.Drawing.PointF structures that define a parallelogram.
        '
        '   srcRect:
        '     System.Drawing.RectangleF structure that specifies the portion of the image object
        '     to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used by the srcRect parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies recoloring and gamma information
        '     for the image object.
        '
        '   callback:
        '     System.Drawing.Graphics.DrawImageAbort delegate that specifies a method to call
        '     during the drawing of the image. This method is called frequently to check whether
        '     to stop execution of the System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.PointF[],System.Drawing.RectangleF,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort,System.Int32)
        '     method according to application-determined criteria.
        '
        '   callbackData:
        '     Value specifying additional data for the System.Drawing.Graphics.DrawImageAbort
        '     delegate to use when checking whether to stop execution of the System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.PointF[],System.Drawing.RectangleF,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort,System.Int32)
        '     method.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort, callbackData As Integer)

        End Sub
        '
        ' Summary:
        '     Draws the specified portion of the specified System.Drawing.Image at the specified
        '     location and with the specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destRect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     drawn image. The image is scaled to fit the rectangle.
        '
        '   srcX:
        '     The x-coordinate of the upper-left corner of the portion of the source image
        '     to draw.
        '
        '   srcY:
        '     The y-coordinate of the upper-left corner of the portion of the source image
        '     to draw.
        '
        '   srcWidth:
        '     Width of the portion of the source image to draw.
        '
        '   srcHeight:
        '     Height of the portion of the source image to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used to determine the source rectangle.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies recoloring and gamma information
        '     for the image object.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)

        End Sub
        '
        ' Summary:
        '     Draws the specified portion of the specified System.Drawing.Image at the specified
        '     location and with the specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destRect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     drawn image. The image is scaled to fit the rectangle.
        '
        '   srcX:
        '     The x-coordinate of the upper-left corner of the portion of the source image
        '     to draw.
        '
        '   srcY:
        '     The y-coordinate of the upper-left corner of the portion of the source image
        '     to draw.
        '
        '   srcWidth:
        '     Width of the portion of the source image to draw.
        '
        '   srcHeight:
        '     Height of the portion of the source image to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used to determine the source rectangle.
        '
        '   imageAttrs:
        '     System.Drawing.Imaging.ImageAttributes that specifies recoloring and gamma information
        '     for the image object.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes)

        End Sub
        '
        ' Summary:
        '     Draws the specified portion of the specified System.Drawing.Image at the specified
        '     location and with the specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destRect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     drawn image. The image is scaled to fit the rectangle.
        '
        '   srcX:
        '     The x-coordinate of the upper-left corner of the portion of the source image
        '     to draw.
        '
        '   srcY:
        '     The y-coordinate of the upper-left corner of the portion of the source image
        '     to draw.
        '
        '   srcWidth:
        '     Width of the portion of the source image to draw.
        '
        '   srcHeight:
        '     Height of the portion of the source image to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used to determine the source rectangle.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies recoloring and gamma information
        '     for image.
        '
        '   callback:
        '     System.Drawing.Graphics.DrawImageAbort delegate that specifies a method to call
        '     during the drawing of the image. This method is called frequently to check whether
        '     to stop execution of the System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.Rectangle,System.Int32,System.Int32,System.Int32,System.Int32,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort)
        '     method according to application-determined criteria.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort)

        End Sub
        '
        ' Summary:
        '     Draws the specified portion of the specified System.Drawing.Image at the specified
        '     location and with the specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destRect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     drawn image. The image is scaled to fit the rectangle.
        '
        '   srcX:
        '     The x-coordinate of the upper-left corner of the portion of the source image
        '     to draw.
        '
        '   srcY:
        '     The y-coordinate of the upper-left corner of the portion of the source image
        '     to draw.
        '
        '   srcWidth:
        '     Width of the portion of the source image to draw.
        '
        '   srcHeight:
        '     Height of the portion of the source image to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used to determine the source rectangle.
        '
        '   imageAttrs:
        '     System.Drawing.Imaging.ImageAttributes that specifies recoloring and gamma information
        '     for the image object.
        '
        '   callback:
        '     System.Drawing.Graphics.DrawImageAbort delegate that specifies a method to call
        '     during the drawing of the image. This method is called frequently to check whether
        '     to stop execution of the System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.Rectangle,System.Single,System.Single,System.Single,System.Single,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort)
        '     method according to application-determined criteria.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As DrawImageAbort)

        End Sub
        '
        ' Summary:
        '     Draws the specified portion of the specified System.Drawing.Image at the specified
        '     location and with the specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destRect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     drawn image. The image is scaled to fit the rectangle.
        '
        '   srcX:
        '     The x-coordinate of the upper-left corner of the portion of the source image
        '     to draw.
        '
        '   srcY:
        '     The y-coordinate of the upper-left corner of the portion of the source image
        '     to draw.
        '
        '   srcWidth:
        '     Width of the portion of the source image to draw.
        '
        '   srcHeight:
        '     Height of the portion of the source image to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used to determine the source rectangle.
        '
        '   imageAttrs:
        '     System.Drawing.Imaging.ImageAttributes that specifies recoloring and gamma information
        '     for the image object.
        '
        '   callback:
        '     System.Drawing.Graphics.DrawImageAbort delegate that specifies a method to call
        '     during the drawing of the image. This method is called frequently to check whether
        '     to stop execution of the System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.Rectangle,System.Int32,System.Int32,System.Int32,System.Int32,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort,System.IntPtr)
        '     method according to application-determined criteria.
        '
        '   callbackData:
        '     Value specifying additional data for the System.Drawing.Graphics.DrawImageAbort
        '     delegate to use when checking whether to stop execution of the DrawImage method.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As DrawImageAbort, callbackData As IntPtr)

        End Sub
        '
        ' Summary:
        '     Draws the specified portion of the specified System.Drawing.Image at the specified
        '     location and with the specified size.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   destRect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     drawn image. The image is scaled to fit the rectangle.
        '
        '   srcX:
        '     The x-coordinate of the upper-left corner of the portion of the source image
        '     to draw.
        '
        '   srcY:
        '     The y-coordinate of the upper-left corner of the portion of the source image
        '     to draw.
        '
        '   srcWidth:
        '     Width of the portion of the source image to draw.
        '
        '   srcHeight:
        '     Height of the portion of the source image to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used to determine the source rectangle.
        '
        '   imageAttrs:
        '     System.Drawing.Imaging.ImageAttributes that specifies recoloring and gamma information
        '     for the image object.
        '
        '   callback:
        '     System.Drawing.Graphics.DrawImageAbort delegate that specifies a method to call
        '     during the drawing of the image. This method is called frequently to check whether
        '     to stop execution of the System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.Rectangle,System.Single,System.Single,System.Single,System.Single,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort,System.IntPtr)
        '     method according to application-determined criteria.
        '
        '   callbackData:
        '     Value specifying additional data for the System.Drawing.Graphics.DrawImageAbort
        '     delegate to use when checking whether to stop execution of the DrawImage method.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public Overrides Sub DrawImage(image As Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As DrawImageAbort, callbackData As IntPtr)

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
        '     System.Drawing.Brush and System.Drawing.Font objects using the formatting attributes
        '     of the specified System.Drawing.StringFormat.
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
        '   point:
        '     System.Drawing.PointF structure that specifies the upper-left corner of the drawn
        '     text.
        '
        '   format:
        '     System.Drawing.StringFormat that specifies formatting attributes, such as line
        '     spacing and alignment, that are applied to the drawn text.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.-or-s is null.
        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, point As PointF, format As StringFormat)
            Call Graphics.DrawString(s, font, brush, point, format)
        End Sub
        '
        ' Summary:
        '     Draws the specified text string in the specified rectangle with the specified
        '     System.Drawing.Brush and System.Drawing.Font objects using the formatting attributes
        '     of the specified System.Drawing.StringFormat.
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
        '   layoutRectangle:
        '     System.Drawing.RectangleF structure that specifies the location of the drawn
        '     text.
        '
        '   format:
        '     System.Drawing.StringFormat that specifies formatting attributes, such as line
        '     spacing and alignment, that are applied to the drawn text.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.-or-s is null.
        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, layoutRectangle As RectangleF, format As StringFormat)
            Call Graphics.DrawString(s, font, brush, layoutRectangle, format)
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
        '     Draws the specified text string at the specified location with the specified
        '     System.Drawing.Brush and System.Drawing.Font objects using the formatting attributes
        '     of the specified System.Drawing.StringFormat.
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
        '   format:
        '     System.Drawing.StringFormat that specifies formatting attributes, such as line
        '     spacing and alignment, that are applied to the drawn text.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.-or-s is null.
        Public Overrides Sub DrawString(s As String, font As Font, brush As Brush, x As Single, y As Single, format As StringFormat)
            Call Graphics.DrawString(s, font, brush, x, y, format)
        End Sub
        '
        ' Summary:
        '     Closes the current graphics container and restores the state of this System.Drawing.Graphics
        '     to the state saved by a call to the System.Drawing.Graphics.BeginContainer method.
        '
        ' Parameters:
        '   container:
        '     System.Drawing.Drawing2D.GraphicsContainer that represents the container this
        '     method restores.
        Public Overrides Sub EndContainer(container As GraphicsContainer)

        End Sub
        '
        ' Summary:
        '     Sends the records in the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display in a specified parallelogram.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoints:
        '     Array of three System.Drawing.Point structures that define a parallelogram that
        '     determines the size and location of the drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, callback As EnumerateMetafileProc)

        End Sub
        '
        ' Summary:
        '     Sends the records in the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display at a specified point.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoint:
        '     System.Drawing.Point structure that specifies the location of the upper-left
        '     corner of the drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, callback As EnumerateMetafileProc)

        End Sub
        '
        ' Summary:
        '     Sends the records of the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display in a specified rectangle.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destRect:
        '     System.Drawing.RectangleF structure that specifies the location and size of the
        '     drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, callback As EnumerateMetafileProc)

        End Sub
        '
        ' Summary:
        '     Sends the records of the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display in a specified rectangle.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destRect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, callback As EnumerateMetafileProc)

        End Sub
        '
        ' Summary:
        '     Sends the records in the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display in a specified parallelogram.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoints:
        '     Array of three System.Drawing.PointF structures that define a parallelogram that
        '     determines the size and location of the drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, callback As EnumerateMetafileProc)

        End Sub
        '
        ' Summary:
        '     Sends the records in the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display at a specified point.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoint:
        '     System.Drawing.PointF structure that specifies the location of the upper-left
        '     corner of the drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, callback As EnumerateMetafileProc)

        End Sub
        '
        ' Summary:
        '     Sends the records of the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display in a specified rectangle.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destRect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, callback As EnumerateMetafileProc, callbackData As IntPtr)

        End Sub
        '
        ' Summary:
        '     Sends the records in the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display in a specified parallelogram.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoints:
        '     Array of three System.Drawing.PointF structures that define a parallelogram that
        '     determines the size and location of the drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, callback As EnumerateMetafileProc, callbackData As IntPtr)

        End Sub
        '
        ' Summary:
        '     Sends the records in the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display in a specified parallelogram.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoints:
        '     Array of three System.Drawing.Point structures that define a parallelogram that
        '     determines the size and location of the drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, callback As EnumerateMetafileProc, callbackData As IntPtr)

        End Sub
        '
        ' Summary:
        '     Sends the records of the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display in a specified rectangle.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destRect:
        '     System.Drawing.RectangleF structure that specifies the location and size of the
        '     drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, callback As EnumerateMetafileProc, callbackData As IntPtr)

        End Sub
        '
        ' Summary:
        '     Sends the records in the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display at a specified point.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoint:
        '     System.Drawing.Point structure that specifies the location of the upper-left
        '     corner of the drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, callback As EnumerateMetafileProc, callbackData As IntPtr)

        End Sub
        '
        ' Summary:
        '     Sends the records in the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display at a specified point.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoint:
        '     System.Drawing.PointF structure that specifies the location of the upper-left
        '     corner of the drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, callback As EnumerateMetafileProc, callbackData As IntPtr)

        End Sub
        '
        ' Summary:
        '     Sends the records in a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display at a specified point.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoint:
        '     System.Drawing.PointF structure that specifies the location of the upper-left
        '     corner of the drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.RectangleF structure that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)

        End Sub
        '
        ' Summary:
        '     Sends the records in the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display in a specified parallelogram using specified
        '     image attributes.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoints:
        '     Array of three System.Drawing.Point structures that define a parallelogram that
        '     determines the size and location of the drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies image attribute information
        '     for the drawn image.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

        End Sub
        '
        ' Summary:
        '     Sends the records in a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display at a specified point.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoint:
        '     System.Drawing.Point structure that specifies the location of the upper-left
        '     corner of the drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.Rectangle structure that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)

        End Sub
        '
        ' Summary:
        '     Sends the records of the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display in a specified rectangle using specified
        '     image attributes.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destRect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies image attribute information
        '     for the drawn image.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

        End Sub
        '
        ' Summary:
        '     Sends the records of a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display in a specified rectangle.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destRect:
        '     System.Drawing.RectangleF structure that specifies the location and size of the
        '     drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.RectangleF structure that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)

        End Sub
        '
        ' Summary:
        '     Sends the records in the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display in a specified parallelogram using specified
        '     image attributes.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoints:
        '     Array of three System.Drawing.PointF structures that define a parallelogram that
        '     determines the size and location of the drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies image attribute information
        '     for the drawn image.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

        End Sub
        '
        ' Summary:
        '     Sends the records of a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display in a specified rectangle.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destRect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.Rectangle structure that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)

        End Sub
        '
        ' Summary:
        '     Sends the records in a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display in a specified parallelogram.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoints:
        '     Array of three System.Drawing.PointF structures that define a parallelogram that
        '     determines the size and location of the drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.RectangleF structures that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)

        End Sub
        '
        ' Summary:
        '     Sends the records of the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display in a specified rectangle using specified
        '     image attributes.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destRect:
        '     System.Drawing.RectangleF structure that specifies the location and size of the
        '     drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies image attribute information
        '     for the drawn image.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

        End Sub
        '
        ' Summary:
        '     Sends the records in the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display at a specified point using specified image
        '     attributes.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoint:
        '     System.Drawing.Point structure that specifies the location of the upper-left
        '     corner of the drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies image attribute information
        '     for the drawn image.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

        End Sub
        '
        ' Summary:
        '     Sends the records in a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display in a specified parallelogram.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoints:
        '     Array of three System.Drawing.Point structures that define a parallelogram that
        '     determines the size and location of the drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.Rectangle structure that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)

        End Sub
        '
        ' Summary:
        '     Sends the records in the specified System.Drawing.Imaging.Metafile, one at a
        '     time, to a callback method for display at a specified point using specified image
        '     attributes.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoint:
        '     System.Drawing.PointF structure that specifies the location of the upper-left
        '     corner of the drawn metafile.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies image attribute information
        '     for the drawn image.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

        End Sub
        '
        ' Summary:
        '     Sends the records in a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display at a specified point.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoint:
        '     System.Drawing.Point structure that specifies the location of the upper-left
        '     corner of the drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.Rectangle structure that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)

        End Sub
        '
        ' Summary:
        '     Sends the records of a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display in a specified rectangle.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destRect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.Rectangle structure that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)

        End Sub
        '
        ' Summary:
        '     Sends the records in a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display in a specified parallelogram.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoints:
        '     Array of three System.Drawing.PointF structures that define a parallelogram that
        '     determines the size and location of the drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.RectangleF structure that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)

        End Sub
        '
        ' Summary:
        '     Sends the records in a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display in a specified parallelogram.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoints:
        '     Array of three System.Drawing.Point structures that define a parallelogram that
        '     determines the size and location of the drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.Rectangle structure that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)

        End Sub
        '
        ' Summary:
        '     Sends the records of a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display in a specified rectangle.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destRect:
        '     System.Drawing.RectangleF structure that specifies the location and size of the
        '     drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.RectangleF structure that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)

        End Sub
        '
        ' Summary:
        '     Sends the records in a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display at a specified point.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoint:
        '     System.Drawing.PointF structure that specifies the location of the upper-left
        '     corner of the drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.RectangleF structure that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)

        End Sub
        '
        ' Summary:
        '     Sends the records in a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display at a specified point using specified
        '     image attributes.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoint:
        '     System.Drawing.Point structure that specifies the location of the upper-left
        '     corner of the drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.Rectangle structure that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   unit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies image attribute information
        '     for the drawn image.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, srcRect As Rectangle, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

        End Sub
        '
        ' Summary:
        '     Sends the records in a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display in a specified parallelogram
        '     using specified image attributes.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoints:
        '     Array of three System.Drawing.PointF structures that define a parallelogram that
        '     determines the size and location of the drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.RectangleF structure that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   unit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies image attribute information
        '     for the drawn image.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, srcRect As RectangleF, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

        End Sub
        '
        ' Summary:
        '     Sends the records of a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display in a specified rectangle using
        '     specified image attributes.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destRect:
        '     System.Drawing.Rectangle structure that specifies the location and size of the
        '     drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.Rectangle structure that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   unit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies image attribute information
        '     for the drawn image.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, srcRect As Rectangle, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

        End Sub
        '
        ' Summary:
        '     Sends the records of a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display in a specified rectangle using
        '     specified image attributes.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destRect:
        '     System.Drawing.RectangleF structure that specifies the location and size of the
        '     drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.RectangleF structure that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   unit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies image attribute information
        '     for the drawn image.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, srcRect As RectangleF, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

        End Sub
        '
        ' Summary:
        '     Sends the records in a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display in a specified parallelogram
        '     using specified image attributes.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoints:
        '     Array of three System.Drawing.Point structures that define a parallelogram that
        '     determines the size and location of the drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.Rectangle structure that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   unit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies image attribute information
        '     for the drawn image.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, srcRect As Rectangle, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

        End Sub
        '
        ' Summary:
        '     Sends the records in a selected rectangle from a System.Drawing.Imaging.Metafile,
        '     one at a time, to a callback method for display at a specified point using specified
        '     image attributes.
        '
        ' Parameters:
        '   metafile:
        '     System.Drawing.Imaging.Metafile to enumerate.
        '
        '   destPoint:
        '     System.Drawing.PointF structure that specifies the location of the upper-left
        '     corner of the drawn metafile.
        '
        '   srcRect:
        '     System.Drawing.RectangleF structure that specifies the portion of the metafile,
        '     relative to its upper-left corner, to draw.
        '
        '   unit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure used to determine the portion of the metafile that the rectangle specified
        '     by the srcRect parameter contains.
        '
        '   callback:
        '     System.Drawing.Graphics.EnumerateMetafileProc delegate that specifies the method
        '     to which the metafile records are sent.
        '
        '   callbackData:
        '     Internal pointer that is required, but ignored. You can pass System.IntPtr.Zero
        '     for this parameter.
        '
        '   imageAttr:
        '     System.Drawing.Imaging.ImageAttributes that specifies image attribute information
        '     for the drawn image.
        Public Overrides Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, srcRect As RectangleF, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

        End Sub
        '
        ' Summary:
        '     Updates the clip region of this System.Drawing.Graphics to exclude the area specified
        '     by a System.Drawing.Region.
        '
        ' Parameters:
        '   region:
        '     System.Drawing.Region that specifies the region to exclude from the clip region.
        Public Overrides Sub ExcludeClip(region As Region)

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
        '     Fills the interior of a closed cardinal spline curve defined by an array of System.Drawing.Point
        '     structures using the specified fill mode.
        '
        ' Parameters:
        '   brush:
        '     System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   points:
        '     Array of System.Drawing.Point structures that define the spline.
        '
        '   fillmode:
        '     Member of the System.Drawing.Drawing2D.FillMode enumeration that determines how
        '     the curve is filled.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.-or-points is null.
        Public Overrides Sub FillClosedCurve(brush As Brush, points() As Point, fillmode As FillMode)

        End Sub
        '
        ' Summary:
        '     Fills the interior of a closed cardinal spline curve defined by an array of System.Drawing.PointF
        '     structures using the specified fill mode.
        '
        ' Parameters:
        '   brush:
        '     System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   points:
        '     Array of System.Drawing.PointF structures that define the spline.
        '
        '   fillmode:
        '     Member of the System.Drawing.Drawing2D.FillMode enumeration that determines how
        '     the curve is filled.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.-or-points is null.
        Public Overrides Sub FillClosedCurve(brush As Brush, points() As PointF, fillmode As FillMode)

        End Sub
        '
        ' Summary:
        '     Fills the interior of a closed cardinal spline curve defined by an array of System.Drawing.Point
        '     structures using the specified fill mode and tension.
        '
        ' Parameters:
        '   brush:
        '     System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   points:
        '     Array of System.Drawing.Point structures that define the spline.
        '
        '   fillmode:
        '     Member of the System.Drawing.Drawing2D.FillMode enumeration that determines how
        '     the curve is filled.
        '
        '   tension:
        '     Value greater than or equal to 0.0F that specifies the tension of the curve.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.-or-points is null.
        Public Overrides Sub FillClosedCurve(brush As Brush, points() As Point, fillmode As FillMode, tension As Single)

        End Sub
        '
        ' Summary:
        '     Fills the interior of a closed cardinal spline curve defined by an array of System.Drawing.PointF
        '     structures using the specified fill mode and tension.
        '
        ' Parameters:
        '   brush:
        '     A System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   points:
        '     Array of System.Drawing.PointF structures that define the spline.
        '
        '   fillmode:
        '     Member of the System.Drawing.Drawing2D.FillMode enumeration that determines how
        '     the curve is filled.
        '
        '   tension:
        '     Value greater than or equal to 0.0F that specifies the tension of the curve.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.-or-points is null.
        Public Overrides Sub FillClosedCurve(brush As Brush, points() As PointF, fillmode As FillMode, tension As Single)

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
        '
        ' Summary:
        '     Fills the interior of a polygon defined by an array of points specified by System.Drawing.Point
        '     structures using the specified fill mode.
        '
        ' Parameters:
        '   brush:
        '     System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   points:
        '     Array of System.Drawing.Point structures that represent the vertices of the polygon
        '     to fill.
        '
        '   fillMode:
        '     Member of the System.Drawing.Drawing2D.FillMode enumeration that determines the
        '     style of the fill.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.-or-points is null.
        Public Overrides Sub FillPolygon(brush As Brush, points() As Point, fillMode As FillMode)
            Call Graphics.FillPolygon(brush, points, fillMode)
        End Sub
        '
        ' Summary:
        '     Fills the interior of a polygon defined by an array of points specified by System.Drawing.PointF
        '     structures using the specified fill mode.
        '
        ' Parameters:
        '   brush:
        '     System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   points:
        '     Array of System.Drawing.PointF structures that represent the vertices of the
        '     polygon to fill.
        '
        '   fillMode:
        '     Member of the System.Drawing.Drawing2D.FillMode enumeration that determines the
        '     style of the fill.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.-or-points is null.
        Public Overrides Sub FillPolygon(brush As Brush, points() As PointF, fillMode As FillMode)
            Call Graphics.FillPolygon(brush, points, fillMode)
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

        '
        ' Summary:
        '     Fills the interior of a System.Drawing.Region.
        '
        ' Parameters:
        '   brush:
        '     System.Drawing.Brush that determines the characteristics of the fill.
        '
        '   region:
        '     System.Drawing.Region that represents the area to fill.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.-or-region is null.
        Public Overrides Sub FillRegion(brush As Brush, region As Region)

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
        '     Forces execution of all pending graphics operations with the method waiting or
        '     not waiting, as specified, to return before the operations finish.
        '
        ' Parameters:
        '   intention:
        '     Member of the System.Drawing.Drawing2D.FlushIntention enumeration that specifies
        '     whether the method returns immediately or waits for any existing operations to
        '     finish.
        Public Overrides Sub Flush(intention As FlushIntention)

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
        '     the current clip region and the specified System.Drawing.Region.
        '
        ' Parameters:
        '   region:
        '     System.Drawing.Region to intersect with the current region.
        Public Overrides Sub IntersectClip(region As Region)

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
        '
        ' Summary:
        '     Multiplies the world transformation of this System.Drawing.Graphics and specified
        '     the System.Drawing.Drawing2D.Matrix.
        '
        ' Parameters:
        '   matrix:
        '     4x4 System.Drawing.Drawing2D.Matrix that multiplies the world transformation.
        Public Overrides Sub MultiplyTransform(matrix As Drawing2D.Matrix)

        End Sub
        '
        ' Summary:
        '     Multiplies the world transformation of this System.Drawing.Graphics and specified
        '     the System.Drawing.Drawing2D.Matrix in the specified order.
        '
        ' Parameters:
        '   matrix:
        '     4x4 System.Drawing.Drawing2D.Matrix that multiplies the world transformation.
        '
        '   order:
        '     Member of the System.Drawing.Drawing2D.MatrixOrder enumeration that determines
        '     the order of the multiplication.
        Public Overrides Sub MultiplyTransform(matrix As Drawing2D.Matrix, order As MatrixOrder)

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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetTransformMatrix(m As Matrix)
            Graphics.Transform = m
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
        ''' Applies the specified rotation to the transformation matrix of this System.Drawing.Graphics
        ''' in the specified order.
        ''' </summary>
        ''' <param name="angle">Angle of rotation in degrees.</param>
        ''' <param name="order">Member of the System.Drawing.Drawing2D.MatrixOrder enumeration that specifies
        ''' whether the rotation is appended or prepended to the matrix transformation.</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub RotateTransform(angle As Single, order As MatrixOrder)
            Call Graphics.RotateTransform(angle, order)
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

        ''' <summary>
        ''' Applies the specified scaling operation to the transformation matrix of this
        ''' System.Drawing.Graphics in the specified order.
        ''' </summary>
        ''' <param name="sx">Scale factor in the x direction.</param>
        ''' <param name="sy">Scale factor in the y direction.</param>
        ''' <param name="order">Member of the System.Drawing.Drawing2D.MatrixOrder enumeration that specifies
        ''' whether the scaling operation is prepended or appended to the transformation
        ''' matrix.</param>
        Public Overrides Sub ScaleTransform(sx As Single, sy As Single, order As MatrixOrder)
            Call Graphics.ScaleTransform(sx, sy, order)
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
        '     Sets the clipping region of this System.Drawing.Graphics to the specified System.Drawing.Drawing2D.GraphicsPath.
        '
        ' Parameters:
        '   path:
        '     System.Drawing.Drawing2D.GraphicsPath that represents the new clip region.
        Public Overrides Sub SetClip(path As GraphicsPath)

        End Sub
        '
        ' Summary:
        '     Sets the clipping region of this System.Drawing.Graphics to the Clip property
        '     of the specified System.Drawing.Graphics.
        '
        ' Parameters:
        '   g:
        '     System.Drawing.Graphics from which to take the new clip region.
        Public Overrides Sub SetClip(g As Graphics)

        End Sub
        '
        ' Summary:
        '     Sets the clipping region of this System.Drawing.Graphics to the result of the
        '     specified operation combining the current clip region and the specified System.Drawing.Region.
        '
        ' Parameters:
        '   region:
        '     System.Drawing.Region to combine.
        '
        '   combineMode:
        '     Member from the System.Drawing.Drawing2D.CombineMode enumeration that specifies
        '     the combining operation to use.
        Public Overrides Sub SetClip(region As Region, combineMode As CombineMode)

        End Sub
        '
        ' Summary:
        '     Sets the clipping region of this System.Drawing.Graphics to the result of the
        '     specified operation combining the current clip region and the rectangle specified
        '     by a System.Drawing.RectangleF structure.
        '
        ' Parameters:
        '   rect:
        '     System.Drawing.RectangleF structure to combine.
        '
        '   combineMode:
        '     Member of the System.Drawing.Drawing2D.CombineMode enumeration that specifies
        '     the combining operation to use.
        Public Overrides Sub SetClip(rect As RectangleF, combineMode As CombineMode)

        End Sub
        '
        ' Summary:
        '     Sets the clipping region of this System.Drawing.Graphics to the result of the
        '     specified operation combining the current clip region and the rectangle specified
        '     by a System.Drawing.Rectangle structure.
        '
        ' Parameters:
        '   rect:
        '     System.Drawing.Rectangle structure to combine.
        '
        '   combineMode:
        '     Member of the System.Drawing.Drawing2D.CombineMode enumeration that specifies
        '     the combining operation to use.
        Public Overrides Sub SetClip(rect As Rectangle, combineMode As CombineMode)

        End Sub
        '
        ' Summary:
        '     Sets the clipping region of this System.Drawing.Graphics to the result of the
        '     specified operation combining the current clip region and the specified System.Drawing.Drawing2D.GraphicsPath.
        '
        ' Parameters:
        '   path:
        '     System.Drawing.Drawing2D.GraphicsPath to combine.
        '
        '   combineMode:
        '     Member of the System.Drawing.Drawing2D.CombineMode enumeration that specifies
        '     the combining operation to use.
        Public Overrides Sub SetClip(path As GraphicsPath, combineMode As CombineMode)

        End Sub
        '
        ' Summary:
        '     Sets the clipping region of this System.Drawing.Graphics to the result of the
        '     specified combining operation of the current clip region and the System.Drawing.Graphics.Clip
        '     property of the specified System.Drawing.Graphics.
        '
        ' Parameters:
        '   g:
        '     System.Drawing.Graphics that specifies the clip region to combine.
        '
        '   combineMode:
        '     Member of the System.Drawing.Drawing2D.CombineMode enumeration that specifies
        '     the combining operation to use.
        Public Overrides Sub SetClip(g As Graphics, combineMode As CombineMode)

        End Sub
        '
        ' Summary:
        '     Transforms an array of points from one coordinate space to another using the
        '     current world and page transformations of this System.Drawing.Graphics.
        '
        ' Parameters:
        '   destSpace:
        '     Member of the System.Drawing.Drawing2D.CoordinateSpace enumeration that specifies
        '     the destination coordinate space.
        '
        '   srcSpace:
        '     Member of the System.Drawing.Drawing2D.CoordinateSpace enumeration that specifies
        '     the source coordinate space.
        '
        '   pts:
        '     Array of System.Drawing.Point structures that represents the points to transformation.
        Public Overrides Sub TransformPoints(destSpace As CoordinateSpace, srcSpace As CoordinateSpace, pts() As Point)

        End Sub
        '
        ' Summary:
        '     Transforms an array of points from one coordinate space to another using the
        '     current world and page transformations of this System.Drawing.Graphics.
        '
        ' Parameters:
        '   destSpace:
        '     Member of the System.Drawing.Drawing2D.CoordinateSpace enumeration that specifies
        '     the destination coordinate space.
        '
        '   srcSpace:
        '     Member of the System.Drawing.Drawing2D.CoordinateSpace enumeration that specifies
        '     the source coordinate space.
        '
        '   pts:
        '     Array of System.Drawing.PointF structures that represent the points to transform.
        Public Overrides Sub TransformPoints(destSpace As CoordinateSpace, srcSpace As CoordinateSpace, pts() As PointF)

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
        '
        ' Summary:
        '     Changes the origin of the coordinate system by applying the specified translation
        '     to the transformation matrix of this System.Drawing.Graphics in the specified
        '     order.
        '
        ' Parameters:
        '   dx:
        '     The x-coordinate of the translation.
        '
        '   dy:
        '     The y-coordinate of the translation.
        '
        '   order:
        '     Member of the System.Drawing.Drawing2D.MatrixOrder enumeration that specifies
        '     whether the translation is prepended or appended to the transformation matrix.
        Public Overrides Sub TranslateTransform(dx As Single, dy As Single, order As MatrixOrder)
            Call Graphics.TranslateTransform(dx, dy, order)
        End Sub

        Protected Overrides Sub Finalize()
            Call Graphics.Dispose()
        End Sub

        '
        ' Summary:
        '     Saves a graphics container with the current state of this System.Drawing.Graphics
        '     and opens and uses a new graphics container.
        '
        ' Returns:
        '     This method returns a System.Drawing.Drawing2D.GraphicsContainer that represents
        '     the state of this System.Drawing.Graphics at the time of the method call.
        Public Overrides Function BeginContainer() As GraphicsContainer
            Return Graphics.BeginContainer
        End Function
        '
        ' Summary:
        '     Saves a graphics container with the current state of this System.Drawing.Graphics
        '     and opens and uses a new graphics container with the specified scale transformation.
        '
        ' Parameters:
        '   dstrect:
        '     System.Drawing.Rectangle structure that, together with the srcrect parameter,
        '     specifies a scale transformation for the container.
        '
        '   srcrect:
        '     System.Drawing.Rectangle structure that, together with the dstrect parameter,
        '     specifies a scale transformation for the container.
        '
        '   unit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure for the container.
        '
        ' Returns:
        '     This method returns a System.Drawing.Drawing2D.GraphicsContainer that represents
        '     the state of this System.Drawing.Graphics at the time of the method call.
        Public Overrides Function BeginContainer(dstrect As Rectangle, srcrect As Rectangle, unit As GraphicsUnit) As GraphicsContainer
            Return Graphics.BeginContainer(dstrect, srcrect, unit)
        End Function
        '
        ' Summary:
        '     Saves a graphics container with the current state of this System.Drawing.Graphics
        '     and opens and uses a new graphics container with the specified scale transformation.
        '
        ' Parameters:
        '   dstrect:
        '     System.Drawing.RectangleF structure that, together with the srcrect parameter,
        '     specifies a scale transformation for the new graphics container.
        '
        '   srcrect:
        '     System.Drawing.RectangleF structure that, together with the dstrect parameter,
        '     specifies a scale transformation for the new graphics container.
        '
        '   unit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the unit
        '     of measure for the container.
        '
        ' Returns:
        '     This method returns a System.Drawing.Drawing2D.GraphicsContainer that represents
        '     the state of this System.Drawing.Graphics at the time of the method call.
        Public Overrides Function BeginContainer(dstrect As RectangleF, srcrect As RectangleF, unit As GraphicsUnit) As GraphicsContainer
            Return Graphics.BeginContainer(dstrect, srcrect, unit)
        End Function
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
        Public Overrides Function IsVisible(point As PointF) As Boolean
            Return Graphics.IsVisible(point)
        End Function
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
        Public Overrides Function IsVisible(point As Point) As Boolean
            Return Graphics.IsVisible(point)
        End Function
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
        Public Overrides Function IsVisible(x As Single, y As Single) As Boolean
            Return Graphics.IsVisible(x, y)
        End Function
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
        Public Overrides Function IsVisible(x As Integer, y As Integer) As Boolean
            Return Graphics.IsVisible(x, y)
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
        '
        ' Summary:
        '     Gets an array of System.Drawing.Region objects, each of which bounds a range
        '     of character positions within the specified string.
        '
        ' Parameters:
        '   text:
        '     String to measure.
        '
        '   font:
        '     System.Drawing.Font that defines the text format of the string.
        '
        '   layoutRect:
        '     System.Drawing.RectangleF structure that specifies the layout rectangle for the
        '     string.
        '
        '   stringFormat:
        '     System.Drawing.StringFormat that represents formatting information, such as line
        '     spacing, for the string.
        '
        ' Returns:
        '     This method returns an array of System.Drawing.Region objects, each of which
        '     bounds a range of character positions within the specified string.
        Public Overrides Function MeasureCharacterRanges(text As String, font As Font, layoutRect As RectangleF, stringFormat As StringFormat) As Region()
            Return Graphics.MeasureCharacterRanges(text, font, layoutRect, stringFormat)
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
        '
        ' Summary:
        '     Measures the specified string when drawn with the specified System.Drawing.Font
        '     and formatted with the specified System.Drawing.StringFormat.
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
        '   stringFormat:
        '     System.Drawing.StringFormat that represents formatting information, such as line
        '     spacing, for the string.
        '
        ' Returns:
        '     This method returns a System.Drawing.SizeF structure that represents the size,
        '     in the units specified by the System.Drawing.Graphics.PageUnit property, of the
        '     string specified in the text parameter as drawn with the font parameter and the
        '     stringFormat parameter.
        '
        ' Exceptions:
        '   T:System.ArgumentException:
        '     font is null.
        Public Overrides Function MeasureString(text As String, font As Font, layoutArea As SizeF, stringFormat As StringFormat) As SizeF
            Return Graphics.MeasureString(text, font, layoutArea, stringFormat)
        End Function
        '
        ' Summary:
        '     Measures the specified string when drawn with the specified System.Drawing.Font
        '     and formatted with the specified System.Drawing.StringFormat.
        '
        ' Parameters:
        '   text:
        '     String to measure.
        '
        '   font:
        '     System.Drawing.Font that defines the text format of the string.
        '
        '   width:
        '     Maximum width of the string.
        '
        '   format:
        '     System.Drawing.StringFormat that represents formatting information, such as line
        '     spacing, for the string.
        '
        ' Returns:
        '     This method returns a System.Drawing.SizeF structure that represents the size,
        '     in the units specified by the System.Drawing.Graphics.PageUnit property, of the
        '     string specified in the text parameter as drawn with the font parameter and the
        '     stringFormat parameter.
        '
        ' Exceptions:
        '   T:System.ArgumentException:
        '     font is null.
        Public Overrides Function MeasureString(text As String, font As Font, width As Integer, format As StringFormat) As SizeF
            Return Graphics.MeasureString(text, font, width, format)
        End Function
        '
        ' Summary:
        '     Measures the specified string when drawn with the specified System.Drawing.Font
        '     and formatted with the specified System.Drawing.StringFormat.
        '
        ' Parameters:
        '   text:
        '     String to measure.
        '
        '   font:
        '     System.Drawing.Font defines the text format of the string.
        '
        '   origin:
        '     System.Drawing.PointF structure that represents the upper-left corner of the
        '     string.
        '
        '   stringFormat:
        '     System.Drawing.StringFormat that represents formatting information, such as line
        '     spacing, for the string.
        '
        ' Returns:
        '     This method returns a System.Drawing.SizeF structure that represents the size,
        '     in the units specified by the System.Drawing.Graphics.PageUnit property, of the
        '     string specified by the text parameter as drawn with the font parameter and the
        '     stringFormat parameter.
        '
        ' Exceptions:
        '   T:System.ArgumentException:
        '     font is null.
        Public Overrides Function MeasureString(text As String, font As Font, origin As PointF, stringFormat As StringFormat) As SizeF
            Return Graphics.MeasureString(text, font, origin, stringFormat)
        End Function
        '
        ' Summary:
        '     Measures the specified string when drawn with the specified System.Drawing.Font
        '     and formatted with the specified System.Drawing.StringFormat.
        '
        ' Parameters:
        '   text:
        '     String to measure.
        '
        '   font:
        '     System.Drawing.Font that defines the text format of the string.
        '
        '   layoutArea:
        '     System.Drawing.SizeF structure that specifies the maximum layout area for the
        '     text.
        '
        '   stringFormat:
        '     System.Drawing.StringFormat that represents formatting information, such as line
        '     spacing, for the string.
        '
        '   charactersFitted:
        '     Number of characters in the string.
        '
        '   linesFilled:
        '     Number of text lines in the string.
        '
        ' Returns:
        '     This method returns a System.Drawing.SizeF structure that represents the size
        '     of the string, in the units specified by the System.Drawing.Graphics.PageUnit
        '     property, of the text parameter as drawn with the font parameter and the stringFormat
        '     parameter.
        '
        ' Exceptions:
        '   T:System.ArgumentException:
        '     font is null.
        Public Overrides Function MeasureString(text As String, font As Font, layoutArea As SizeF, stringFormat As StringFormat, ByRef charactersFitted As Integer, ByRef linesFilled As Integer) As SizeF
            Return Graphics.MeasureString(text, font, layoutArea, stringFormat, charactersFitted, linesFilled)
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
