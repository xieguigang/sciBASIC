﻿#Region "Microsoft.VisualBasic::3aab7e4b8f80677522ec27c8b3e66d41, sciBASIC#\Microsoft.VisualBasic.Core\src\Extensions\Image\GDI+\Interface.vb"

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

    '   Total Lines: 4496
    '    Code Lines: 270
    ' Comment Lines: 4176
    '   Blank Lines: 50
    '     File Size: 204.30 KB


    '     Class IGraphics
    ' 
    '         Properties: Background, Dpi
    ' 
    '         Sub: Clear, FillPie, FillRectangle, (+2 Overloads) FillRectangles, Finalize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#Region "Assembly System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
' C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6\System.Drawing.dll
#End Region

Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Graphics
Imports System.Drawing.Imaging
Imports System.Drawing.Text
Imports System.Runtime.CompilerServices
Imports std = System.Math
Imports Interpolation2D = System.Drawing.Drawing2D.InterpolationMode

Namespace Imaging

    ''' <summary>
    ''' Encapsulates a GDI+(bitmap, wmf)/SVG etc drawing surface. This class must be inherited.
    ''' </summary>
    ''' <remarks>
    ''' <see cref="Graphics"/>
    ''' </remarks>
    Public MustInherit Class IGraphics
        Implements IDisposable

        ''' <summary>
        ''' the current canvas size in pixels: [width, height]
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property Size As Size

        ''' <summary>
        ''' set background via <see cref="Clear"/> method.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Background As Color

        ''
        '' Summary:
        ''     Gets or sets a System.Drawing.Region that limits the drawing region of this System.Drawing.Graphics.
        ''
        '' Returns:
        ''     A System.Drawing.Region that limits the portion of this System.Drawing.Graphics
        ''     that is currently available for drawing.
        'Public MustOverride Property Clip As Region
        ''
        '' Summary:
        ''     Gets a System.Drawing.RectangleF structure that bounds the clipping region of
        ''     this System.Drawing.Graphics.
        ''
        '' Returns:
        ''     A System.Drawing.RectangleF structure that represents a bounding rectangle for
        ''     the clipping region of this System.Drawing.Graphics.
        'Public MustOverride ReadOnly Property ClipBounds As RectangleF

        ''' <summary>
        ''' Gets a value that specifies how composited images are drawn to this System.Drawing.Graphics.
        ''' </summary>
        ''' <returns>This property specifies a member of the System.Drawing.Drawing2D.CompositingMode
        ''' enumeration. The default is System.Drawing.Drawing2D.CompositingMode.SourceOver.
        ''' </returns>
        Public MustOverride Property CompositingMode As CompositingMode

        ''' <summary>
        ''' Gets or sets the rendering quality of composited images drawn to this System.Drawing.Graphics.
        ''' </summary>
        ''' <returns>This property specifies a member of the System.Drawing.Drawing2D.CompositingQuality
        ''' enumeration. The default is System.Drawing.Drawing2D.CompositingQuality.Default.</returns>
        Public MustOverride Property CompositingQuality As CompositingQuality

        ''' <summary>
        ''' Gets the horizontal resolution of this System.Drawing.Graphics.
        ''' </summary>
        ''' <returns>The value, in dots per inch, for the horizontal resolution supported by this
        ''' System.Drawing.Graphics.</returns>
        Public MustOverride ReadOnly Property DpiX As Single

        ''' <summary>
        ''' Gets the vertical resolution of this System.Drawing.Graphics.
        ''' </summary>
        ''' <returns>The value, in dots per inch, for the vertical resolution supported by this System.Drawing.Graphics.</returns>
        Public MustOverride ReadOnly Property DpiY As Single

        ''' <summary>
        ''' max value of the [DpiX, DpiY]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Dpi As Single
            Get
                Return std.Max(DpiX, DpiY)
            End Get
        End Property


        '
        ' Summary:
        '     Gets or sets the interpolation mode associated with this System.Drawing.Graphics.
        '
        ' Returns:
        '     One of the System.Drawing.Drawing2D.InterpolationMode values.
        Public MustOverride Property InterpolationMode As InterpolationMode
        '
        ' Summary:
        '     Gets a value indicating whether the clipping region of this System.Drawing.Graphics
        '     is empty.
        '
        ' Returns:
        '     true if the clipping region of this System.Drawing.Graphics is empty; otherwise,
        '     false.
        Public MustOverride ReadOnly Property IsClipEmpty As Boolean
        '
        ' Summary:
        '     Gets a value indicating whether the visible clipping region of this System.Drawing.Graphics
        '     is empty.
        '
        ' Returns:
        '     true if the visible portion of the clipping region of this System.Drawing.Graphics
        '     is empty; otherwise, false.
        Public MustOverride ReadOnly Property IsVisibleClipEmpty As Boolean
        '
        ' Summary:
        '     Gets or sets the scaling between world units and page units for this System.Drawing.Graphics.
        '
        ' Returns:
        '     This property specifies a value for the scaling between world units and page
        '     units for this System.Drawing.Graphics.
        Public MustOverride Property PageScale As Single
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
        Public MustOverride Property PageUnit As GraphicsUnit
        '
        ' Summary:
        '     Gets or set a value specifying how pixels are offset during rendering of this
        '     System.Drawing.Graphics.
        '
        ' Returns:
        '     This property specifies a member of the System.Drawing.Drawing2D.PixelOffsetMode
        '     enumeration
        Public MustOverride Property PixelOffsetMode As PixelOffsetMode
        '
        ' Summary:
        '     Gets or sets the rendering origin of this System.Drawing.Graphics for dithering
        '     and for hatch brushes.
        '
        ' Returns:
        '     A System.Drawing.Point structure that represents the dither origin for 8-bits-per-pixel
        '     and 16-bits-per-pixel dithering and is also used to set the origin for hatch
        '     brushes.
        Public MustOverride Property RenderingOrigin As Point
        '
        ' Summary:
        '     Gets or sets the rendering quality for this System.Drawing.Graphics.
        '
        ' Returns:
        '     One of the System.Drawing.Drawing2D.SmoothingMode values.
        Public MustOverride Property SmoothingMode As SmoothingMode
        '
        ' Summary:
        '     Gets or sets the gamma correction value for rendering text.
        '
        ' Returns:
        '     The gamma correction value used for rendering antialiased and ClearType text.
        Public MustOverride Property TextContrast As Integer
        '
        ' Summary:
        '     Gets or sets the rendering mode for text associated with this System.Drawing.Graphics.
        '
        ' Returns:
        '     One of the System.Drawing.Text.TextRenderingHint values.
        Public MustOverride Property TextRenderingHint As TextRenderingHint
        ''
        '' Summary:
        ''     Gets or sets a copy of the geometric world transformation for this System.Drawing.Graphics.
        ''
        '' Returns:
        ''     A copy of the System.Drawing.Drawing2D.Matrix that represents the geometric world
        ''     transformation for this System.Drawing.Graphics.
        'Public MustOverride Property Transform As Matrix
        ''
        '' Summary:
        ''     Gets the bounding rectangle of the visible clipping region of this System.Drawing.Graphics.
        ''
        '' Returns:
        ''     A System.Drawing.RectangleF structure that represents a bounding rectangle for
        ''     the visible clipping region of this System.Drawing.Graphics.
        'Public MustOverride ReadOnly Property VisibleClipBounds As RectangleF

        '
        ' Summary:
        '     Adds a comment to the current System.Drawing.Imaging.Metafile.
        '
        ' Parameters:
        '   data:
        '     Array of bytes that contains the comment.
        Public MustOverride Sub AddMetafileComment(data() As Byte)

        ''' <summary>
        ''' Clears the entire drawing surface and fills it with the specified background
        ''' color.
        ''' </summary>
        ''' <param name="color">
        ''' <see cref="Color"/> structure that represents the background color of the drawing
        ''' surface.
        ''' </param>
        Public Sub Clear(color As Color)
            Me.ClearCanvas(color)
            Me._Background = color
        End Sub

        ''' <summary>
        ''' Clears the entire drawing surface and fills it with the specified background
        ''' color.
        ''' </summary>
        ''' <param name="color"></param>
        Protected MustOverride Sub ClearCanvas(color As Color)

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
        ' Exceptions:
        '   T:System.ComponentModel.Win32Exception:
        '     The operation failed.
        Public MustOverride Sub CopyFromScreen(upperLeftSource As Point, upperLeftDestination As Point, blockRegionSize As Size)
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
        Public MustOverride Sub CopyFromScreen(upperLeftSource As Point, upperLeftDestination As Point, blockRegionSize As Size, copyPixelOperation As CopyPixelOperation)
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
        Public MustOverride Sub CopyFromScreen(sourceX As Integer, sourceY As Integer, destinationX As Integer, destinationY As Integer, blockRegionSize As Size)
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
        Public MustOverride Sub CopyFromScreen(sourceX As Integer, sourceY As Integer, destinationX As Integer, destinationY As Integer, blockRegionSize As Size, copyPixelOperation As CopyPixelOperation)
        '
        ' Summary:
        '     Releases all resources used by this System.Drawing.Graphics.
        Public MustOverride Sub Dispose() Implements IDisposable.Dispose
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
        Public MustOverride Sub DrawArc(pen As Pen, rect As RectangleF, startAngle As Single, sweepAngle As Single)

        ''' <summary>
        ''' Draws an arc representing a portion of an ellipse specified by a System.Drawing.Rectangle
        ''' structure.
        ''' </summary>
        ''' <param name="pen">System.Drawing.Pen that determines the color, width, and style of the arc.</param>
        ''' <param name="rect">System.Drawing.RectangleF structure that defines the boundaries of the ellipse.</param>
        ''' <param name="startAngle">Angle in degrees measured clockwise from the x-axis to the starting point of
        ''' the arc.</param>
        ''' <param name="sweepAngle">Angle in degrees measured clockwise from the startAngle parameter to ending point
        ''' of the arc.</param>
        Public MustOverride Sub DrawArc(pen As Pen, rect As Rectangle, startAngle As Single, sweepAngle As Single)
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
        Public MustOverride Sub DrawArc(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)
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
        Public MustOverride Sub DrawArc(pen As Pen, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)
        '
        ' Summary:
        '     Draws a Bézier spline defined by four System.Drawing.Point structures.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen structure that determines the color, width, and style of the
        '     curve.
        '
        '   pt1:
        '     System.Drawing.Point structure that represents the starting point of the curve.
        '
        '   pt2:
        '     System.Drawing.Point structure that represents the first control point for the
        '     curve.
        '
        '   pt3:
        '     System.Drawing.Point structure that represents the second control point for the
        '     curve.
        '
        '   pt4:
        '     System.Drawing.Point structure that represents the ending point of the curve.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.
        Public MustOverride Sub DrawBezier(pen As Pen, pt1 As Point, pt2 As Point, pt3 As Point, pt4 As Point)
        '
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
        Public MustOverride Sub DrawBezier(pen As Pen, pt1 As PointF, pt2 As PointF, pt3 As PointF, pt4 As PointF)
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
        Public MustOverride Sub DrawBezier(pen As Pen, x1 As Single, y1 As Single, x2 As Single, y2 As Single, x3 As Single, y3 As Single, x4 As Single, y4 As Single)
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
        Public MustOverride Sub DrawBeziers(pen As Pen, points() As PointF)
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
        Public MustOverride Sub DrawBeziers(pen As Pen, points() As Point)
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
        Public MustOverride Sub DrawClosedCurve(pen As Pen, points() As Point)
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
        Public MustOverride Sub DrawClosedCurve(pen As Pen, points() As PointF)
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
        Public MustOverride Sub DrawClosedCurve(pen As Pen, points() As Point, tension As Single, fillmode As FillMode)
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
        Public MustOverride Sub DrawClosedCurve(pen As Pen, points() As PointF, tension As Single, fillmode As FillMode)
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
        Public MustOverride Sub DrawCurve(pen As Pen, points() As Point)
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
        Public MustOverride Sub DrawCurve(pen As Pen, points() As PointF)
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
        Public MustOverride Sub DrawCurve(pen As Pen, points() As PointF, tension As Single)
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
        Public MustOverride Sub DrawCurve(pen As Pen, points() As Point, tension As Single)
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
        Public MustOverride Sub DrawCurve(pen As Pen, points() As PointF, offset As Integer, numberOfSegments As Integer)
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
        Public MustOverride Sub DrawCurve(pen As Pen, points() As PointF, offset As Integer, numberOfSegments As Integer, tension As Single)
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
        Public MustOverride Sub DrawCurve(pen As Pen, points() As Point, offset As Integer, numberOfSegments As Integer, tension As Single)
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
        Public MustOverride Sub DrawEllipse(pen As Pen, rect As Rectangle)
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
        Public MustOverride Sub DrawEllipse(pen As Pen, rect As RectangleF)
        '
        ' Summary:
        '     Draws an ellipse defined by a bounding rectangle specified by a pair of coordinates,
        '     a height, and a width.
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
        Public MustOverride Sub DrawEllipse(pen As Pen, x As Single, y As Single, width As Single, height As Single)
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
        Public MustOverride Sub DrawEllipse(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer)
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
        Public MustOverride Sub DrawIcon(icon As Icon, targetRect As Rectangle)
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
        Public MustOverride Sub DrawIcon(icon As Icon, x As Integer, y As Integer)
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
        Public MustOverride Sub DrawIconUnstretched(icon As Icon, targetRect As Rectangle)
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
        '     System.Drawing.Point structure that represents the location of the upper-left
        '     corner of the drawn image.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public MustOverride Sub DrawImage(image As Image, point As Point)
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
        Public MustOverride Sub DrawImage(image As Image, destPoints() As Point)
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
        Public MustOverride Sub DrawImage(image As Image, destPoints() As PointF)

        ''' <summary>
        ''' Draws the specified <see cref="Image"/> at the specified location and with the
        ''' specified size.
        ''' </summary>
        ''' <param name="image">Image to draw.</param>
        ''' <param name="rect">Rectangle structure that specifies the location and size of the
        ''' drawn image.</param>
        Public MustOverride Sub DrawImage(image As Image, rect As Rectangle)

        ''' <summary>
        ''' Draws the specified <see cref="Image"/>, using its original physical size, at
        ''' the specified location.
        ''' </summary>
        ''' <param name="image"><see cref="Image"/> to draw.</param>
        ''' <param name="point"><see cref="Drawing.PointF"/> structure that represents the upper-left corner of the
        ''' drawn image.</param>
        Public MustOverride Sub DrawImage(image As Image, point As PointF)

        ''' <summary>
        ''' Draws the specified System.Drawing.Image at the specified location and with the
        ''' specified size.
        ''' </summary>
        ''' <param name="image">Image to draw.</param>
        ''' <param name="rect">RectangleF structure that specifies the location and size of the
        ''' drawn image.</param>
        Public MustOverride Sub DrawImage(image As Image, rect As RectangleF)
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
        Public MustOverride Sub DrawImage(image As Image, x As Integer, y As Integer)
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
        Public MustOverride Sub DrawImage(image As Image, x As Single, y As Single)
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
        '     System.Drawing.RectangleF structure that specifies the location and size of the
        '     drawn image. The image is scaled to fit the rectangle.
        '
        '   srcRect:
        '     System.Drawing.RectangleF structure that specifies the portion of the image object
        '     to draw.
        '
        '   srcUnit:
        '     Member of the System.Drawing.GraphicsUnit enumeration that specifies the units
        '     of measure used by the srcRect parameter.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public MustOverride Sub DrawImage(image As Image, destRect As RectangleF, srcRect As RectangleF, srcUnit As GraphicsUnit)
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
        Public MustOverride Sub DrawImage(image As Image, destRect As Rectangle, srcRect As Rectangle, srcUnit As GraphicsUnit)
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
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public MustOverride Sub DrawImage(image As Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit)
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
        Public MustOverride Sub DrawImage(image As Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit)
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
        Public MustOverride Sub DrawImage(image As Image, x As Single, y As Single, width As Single, height As Single)
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
        Public MustOverride Sub DrawImage(image As Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)
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
        Public MustOverride Sub DrawImage(image As Image, x As Integer, y As Integer, width As Integer, height As Integer)
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
        Public MustOverride Sub DrawImage(image As Image, x As Single, y As Single, srcRect As RectangleF, srcUnit As GraphicsUnit)
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
        Public MustOverride Sub DrawImage(image As Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)
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
        Public MustOverride Sub DrawImage(image As Image, x As Integer, y As Integer, srcRect As Rectangle, srcUnit As GraphicsUnit)
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
        Public MustOverride Sub DrawImage(image As Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort)
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
        Public MustOverride Sub DrawImage(image As Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort)
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
        Public MustOverride Sub DrawImage(image As Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort, callbackData As Integer)
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
        Public MustOverride Sub DrawImage(image As Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit)
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
        Public MustOverride Sub DrawImage(image As Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit)
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
        Public MustOverride Sub DrawImage(image As Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort, callbackData As Integer)
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
        Public MustOverride Sub DrawImage(image As Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes)
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
        Public MustOverride Sub DrawImage(image As Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)
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
        Public MustOverride Sub DrawImage(image As Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort)
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
        Public MustOverride Sub DrawImage(image As Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As DrawImageAbort)
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
        Public MustOverride Sub DrawImage(image As Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As DrawImageAbort, callbackData As IntPtr)
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
        Public MustOverride Sub DrawImage(image As Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As DrawImageAbort, callbackData As IntPtr)
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
        Public MustOverride Sub DrawImageUnscaled(image As Image, rect As Rectangle)
        '
        ' Summary:
        '     Draws a specified image using its original physical size at a specified location.
        '
        ' Parameters:
        '   image:
        '     System.Drawing.Image to draw.
        '
        '   point:
        '     System.Drawing.Point structure that specifies the upper-left corner of the drawn
        '     image.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public MustOverride Sub DrawImageUnscaled(image As Image, point As Point)
        '
        ' Summary:
        '     Draws the specified image using its original physical size at the location specified
        '     by a coordinate pair.
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
        Public MustOverride Sub DrawImageUnscaled(image As Image, x As Integer, y As Integer)
        '
        ' Summary:
        '     Draws a specified image using its original physical size at a specified location.
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
        '     Not used.
        '
        '   height:
        '     Not used.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public MustOverride Sub DrawImageUnscaled(image As Image, x As Integer, y As Integer, width As Integer, height As Integer)
        '
        ' Summary:
        '     Draws the specified image without scaling and clips it, if necessary, to fit
        '     in the specified rectangle.
        '
        ' Parameters:
        '   image:
        '     The System.Drawing.Image to draw.
        '
        '   rect:
        '     The System.Drawing.Rectangle in which to draw the image.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     image is null.
        Public MustOverride Sub DrawImageUnscaledAndClipped(image As Image, rect As Rectangle)

        ''' <summary>
        ''' Draws a line connecting two System.Drawing.PointF structures.
        ''' </summary>
        ''' <param name="pen"><see cref="Pen"/> that determines the color, width, and style of the line.</param>
        ''' <param name="pt1"><see cref="Drawing.PointF"/> structure that represents the first point to connect.</param>
        ''' <param name="pt2"><see cref="Drawing.PointF"/> structure that represents the second point to connect.</param>
        Public MustOverride Sub DrawLine(pen As Pen, pt1 As PointF, pt2 As PointF)

        ''' <summary>
        ''' Draws a line connecting two System.Drawing.Point structures.
        ''' </summary>
        ''' <param name="pen">System.Drawing.Pen that determines the color, width, and style of the line.</param>
        ''' <param name="pt1">System.Drawing.Point structure that represents the first point to connect.</param>
        ''' <param name="pt2">System.Drawing.Point structure that represents the second point to connect.</param>
        Public MustOverride Sub DrawLine(pen As Pen, pt1 As Point, pt2 As Point)

        ''' <summary>
        ''' Draws a line connecting the two points specified by the coordinate pairs.
        ''' </summary>
        ''' <param name="pen">System.Drawing.Pen that determines the color, width, and style of the line.</param>
        ''' <param name="x1">The x-coordinate of the first point.</param>
        ''' <param name="y1">The y-coordinate of the first point.</param>
        ''' <param name="x2">The x-coordinate of the second point.</param>
        ''' <param name="y2">The y-coordinate of the second point.</param>
        Public MustOverride Sub DrawLine(pen As Pen, x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer)

        ''' <summary>
        ''' Draws a line connecting the two points specified by the coordinate pairs.
        ''' </summary>
        ''' <param name="pen">System.Drawing.Pen that determines the color, width, and style of the line.</param>
        ''' <param name="x1">The x-coordinate of the first point.</param>
        ''' <param name="y1">The y-coordinate of the first point.</param>
        ''' <param name="x2">The x-coordinate of the second point.</param>
        ''' <param name="y2">The y-coordinate of the second point.</param>
        Public MustOverride Sub DrawLine(pen As Pen, x1 As Single, y1 As Single, x2 As Single, y2 As Single)
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
        Public MustOverride Sub DrawLines(pen As Pen, points() As PointF)
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
        Public MustOverride Sub DrawLines(pen As Pen, points() As Point)
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
        Public MustOverride Sub DrawPath(pen As Pen, path As GraphicsPath)
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
        Public MustOverride Sub DrawPie(pen As Pen, rect As Rectangle, startAngle As Single, sweepAngle As Single)
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
        Public MustOverride Sub DrawPie(pen As Pen, rect As RectangleF, startAngle As Single, sweepAngle As Single)
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
        Public MustOverride Sub DrawPie(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)
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
        Public MustOverride Sub DrawPie(pen As Pen, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)

        Public MustOverride Sub DrawCircle(center As PointF, fill As Color, stroke As Pen, radius!)

        '
        ' Summary:
        '     Draws a polygon defined by an array of System.Drawing.PointF structures.
        '
        ' Parameters:
        '   pen:
        '     System.Drawing.Pen that determines the color, width, and style of the polygon.
        '
        '   points:
        '     Array of System.Drawing.PointF structures that represent the vertices of the
        '     polygon.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     pen is null.-or-points is null.
        Public MustOverride Sub DrawPolygon(pen As Pen, points() As PointF)
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
        Public MustOverride Sub DrawPolygon(pen As Pen, points() As Point)

        ''' <summary>
        ''' Draws a rectangle specified by a System.Drawing.Rectangle structure.
        ''' </summary>
        ''' <param name="pen">A System.Drawing.Pen that determines the color, width, and style of the rectangle.</param>
        ''' <param name="rect">A System.Drawing.Rectangle structure that represents the rectangle to draw.</param>
        Public MustOverride Sub DrawRectangle(pen As Pen, rect As Rectangle)
        Public MustOverride Sub DrawRectangle(pen As Pen, rect As RectangleF)

        ''' <summary>
        ''' Draws a rectangle specified by a coordinate pair, a width, and a height.
        ''' </summary>
        ''' <param name="pen">A System.Drawing.Pen that determines the color, width, and style of the rectangle.</param>
        ''' <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
        ''' <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
        ''' <param name="width">The width of the rectangle to draw.</param>
        ''' <param name="height">The height of the rectangle to draw.</param>
        Public MustOverride Sub DrawRectangle(pen As Pen, x As Single, y As Single, width As Single, height As Single)

        ''' <summary>
        ''' Draws a rectangle specified by a coordinate pair, a width, and a height.
        ''' </summary>
        ''' <param name="pen">System.Drawing.Pen that determines the color, width, and style of the rectangle.</param>
        ''' <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
        ''' <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
        ''' <param name="width">Width of the rectangle to draw.</param>
        ''' <param name="height">Height of the rectangle to draw.</param>
        Public MustOverride Sub DrawRectangle(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer)

        ''' <summary>
        ''' Draws a series of rectangles specified by System.Drawing.RectangleF structures.
        ''' </summary>
        ''' <param name="pen">System.Drawing.Pen that determines the color, width, and style of the outlines
        ''' of the rectangles.</param>
        ''' <param name="rects">Array of System.Drawing.RectangleF structures that represent the rectangles to
        ''' draw.</param>
        Public MustOverride Sub DrawRectangles(pen As Pen, rects() As RectangleF)

        ''' <summary>
        ''' Draws a series of rectangles specified by System.Drawing.Rectangle structures.
        ''' </summary>
        ''' <param name="pen">System.Drawing.Pen that determines the color, width, and style of the outlines
        ''' of the rectangles.</param>
        ''' <param name="rects">Array of System.Drawing.Rectangle structures that represent the rectangles to
        ''' draw.</param>
        Public MustOverride Sub DrawRectangles(pen As Pen, rects() As Rectangle)

        ''' <summary>
        ''' Draws the specified text string at the specified location with the specified
        ''' <see cref="Brush"/> and <see cref="Font"/> objects.
        ''' </summary>
        ''' <param name="s">String to draw.</param>
        ''' <param name="font">System.Drawing.Font that defines the text format of the string.</param>
        ''' <param name="brush">System.Drawing.Brush that determines the color and texture of the drawn text.</param>
        ''' <param name="point">System.Drawing.PointF structure that specifies the upper-left corner of the drawn
        ''' text.</param>
        Public MustOverride Sub DrawString(s As String, font As Font, brush As Brush, ByRef point As PointF)
        '
        ' Summary:
        '     Draws the specified text string in the specified rectangle with the specified
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
        '   layoutRectangle:
        '     System.Drawing.RectangleF structure that specifies the location of the drawn
        '     text.
        '
        ' Exceptions:
        '   T:System.ArgumentNullException:
        '     brush is null.-or-s is null.
        Public MustOverride Sub DrawString(s As String, font As Font, brush As Brush, layoutRectangle As RectangleF)
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
        Public MustOverride Sub DrawString(s As String, font As Font, brush As Brush, layoutRectangle As RectangleF, format As StringFormat)
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
        Public MustOverride Sub DrawString(s As String, font As Font, brush As Brush, point As PointF, format As StringFormat)

        ''' <summary>
        ''' Draws the specified text string at the specified location with the specified
        ''' System.Drawing.Brush and System.Drawing.Font objects.
        ''' </summary>
        ''' <param name="s">String to draw.</param>
        ''' <param name="font">System.Drawing.Font that defines the text format of the string.</param>
        ''' <param name="brush">System.Drawing.Brush that determines the color and texture of the drawn text.</param>
        ''' <param name="x">The x-coordinate of the upper-left corner of the drawn text.</param>
        ''' <param name="y">The y-coordinate of the upper-left corner of the drawn text.</param>
        Public MustOverride Sub DrawString(s As String, font As Font, brush As Brush, x As Single, y As Single)
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
        Public MustOverride Sub DrawString(s As String, font As Font, brush As Brush, x As Single, y As Single, format As StringFormat)
        '
        ' Summary:
        '     Closes the current graphics container and restores the state of this System.Drawing.Graphics
        '     to the state saved by a call to the System.Drawing.Graphics.BeginContainer method.
        '
        ' Parameters:
        '   container:
        '     System.Drawing.Drawing2D.GraphicsContainer that represents the container this
        '     method restores.
        Public MustOverride Sub EndContainer(container As GraphicsContainer)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, callback As EnumerateMetafileProc)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, callback As EnumerateMetafileProc)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, callback As EnumerateMetafileProc)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, callback As EnumerateMetafileProc)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, callback As EnumerateMetafileProc)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, callback As EnumerateMetafileProc)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, callback As EnumerateMetafileProc, callbackData As IntPtr)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, callback As EnumerateMetafileProc, callbackData As IntPtr)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, callback As EnumerateMetafileProc, callbackData As IntPtr)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, callback As EnumerateMetafileProc, callbackData As IntPtr)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, callback As EnumerateMetafileProc, callbackData As IntPtr)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, callback As EnumerateMetafileProc, callbackData As IntPtr)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, srcRect As Rectangle, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, srcRect As RectangleF, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, srcRect As RectangleF, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, srcRect As Rectangle, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, srcRect As RectangleF, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
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
        Public MustOverride Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, srcRect As Rectangle, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)
        '
        ' Summary:
        '     Updates the clip region of this System.Drawing.Graphics to exclude the area specified
        '     by a System.Drawing.Rectangle structure.
        '
        ' Parameters:
        '   rect:
        '     System.Drawing.Rectangle structure that specifies the rectangle to exclude from
        '     the clip region.
        Public MustOverride Sub ExcludeClip(rect As Rectangle)
        '
        ' Summary:
        '     Updates the clip region of this System.Drawing.Graphics to exclude the area specified
        '     by a System.Drawing.Region.
        '
        ' Parameters:
        '   region:
        '     System.Drawing.Region that specifies the region to exclude from the clip region.
        Public MustOverride Sub ExcludeClip(region As Region)
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
        Public MustOverride Sub FillClosedCurve(brush As Brush, points() As PointF)
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
        Public MustOverride Sub FillClosedCurve(brush As Brush, points() As Point)
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
        Public MustOverride Sub FillClosedCurve(brush As Brush, points() As Point, fillmode As FillMode)
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
        Public MustOverride Sub FillClosedCurve(brush As Brush, points() As PointF, fillmode As FillMode)
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
        Public MustOverride Sub FillClosedCurve(brush As Brush, points() As PointF, fillmode As FillMode, tension As Single)
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
        Public MustOverride Sub FillClosedCurve(brush As Brush, points() As Point, fillmode As FillMode, tension As Single)
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
        Public MustOverride Sub FillEllipse(brush As Brush, rect As Rectangle)

        ''' <summary>
        ''' Fills the interior of an ellipse defined by a bounding rectangle specified by
        ''' a System.Drawing.RectangleF structure.
        ''' </summary>
        ''' <param name="brush">System.Drawing.Brush that determines the characteristics of the fill.</param>
        ''' <param name="rect">System.Drawing.RectangleF structure that represents the bounding rectangle that
        ''' defines the ellipse.</param>
        Public MustOverride Sub FillEllipse(brush As Brush, rect As RectangleF)
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
        Public MustOverride Sub FillEllipse(brush As Brush, x As Single, y As Single, width As Single, height As Single)
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
        Public MustOverride Sub FillEllipse(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer)

        ''' <summary>
        ''' Fills the interior of a System.Drawing.Drawing2D.GraphicsPath.
        ''' </summary>
        ''' <param name="brush">System.Drawing.Brush that determines the characteristics of the fill.</param>
        ''' <param name="path">System.Drawing.Drawing2D.GraphicsPath that represents the path to fill.</param>
        Public MustOverride Sub FillPath(brush As Brush, path As GraphicsPath)
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
        Public MustOverride Sub FillPie(brush As Brush, rect As Rectangle, startAngle As Single, sweepAngle As Single)

        ''' <summary>
        ''' Fills the interior of a pie section defined by an ellipse specified by a System.Drawing.RectangleF
        ''' structure and two radial lines.
        ''' </summary>
        ''' <param name="brush">System.Drawing.Brush that determines the characteristics of the fill.</param>
        ''' <param name="rect"><see cref="RectangleF"/> structure that represents the bounding rectangle that
        ''' defines the ellipse from which the pie section comes.</param>
        ''' <param name="startAngle">Angle in degrees measured clockwise from the x-axis to the first side of the
        ''' pie section.</param>
        ''' <param name="sweepAngle">Angle in degrees measured clockwise from the startAngle parameter to the second
        ''' side of the pie section.</param>
        Public Sub FillPie(brush As Brush, rect As RectangleF, startAngle As Single, sweepAngle As Single)
            With rect
                Call FillPie(brush, New Rectangle(.Location.ToPoint, .Size.ToSize), startAngle, sweepAngle)
            End With
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
        Public MustOverride Sub FillPie(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)

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
        Public MustOverride Sub FillPie(brush As Brush, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)
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
        Public MustOverride Sub FillPolygon(brush As Brush, points() As Point)
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
        Public MustOverride Sub FillPolygon(brush As Brush, points() As PointF)
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
        Public MustOverride Sub FillPolygon(brush As Brush, points() As Point, fillMode As FillMode)
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
        Public MustOverride Sub FillPolygon(brush As Brush, points() As PointF, fillMode As FillMode)

        ''' <summary>
        ''' Fills the interior of a rectangle specified by a <see cref="Rectangle"/> structure.
        ''' </summary>
        ''' <param name="brush">System.Drawing.Brush that determines the characteristics of the fill.</param>
        ''' <param name="rect">System.Drawing.Rectangle structure that represents the rectangle to fill.</param>
        Public MustOverride Sub FillRectangle(brush As Brush, rect As Rectangle)

        ''' <summary>
        ''' <see cref="FillRectangle(Brush, RectangleF)"/> extensions
        ''' </summary>
        ''' <param name="brush"></param>
        ''' <param name="x!"></param>
        ''' <param name="y!"></param>
        ''' <param name="size"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub FillRectangle(brush As Brush, x!, y!, size As SizeF)
            Call FillRectangle(brush, New RectangleF() With {.X = x, .Y = y, .Size = size})
        End Sub

        ''' <summary>
        ''' Fills the interior of a rectangle specified by a <see cref="RectangleF"/> structure.
        ''' </summary>
        ''' <param name="brush">
        ''' <see cref="Brush"/> that determines the characteristics of the fill.
        ''' </param>
        ''' <param name="rect">
        ''' <see cref="RectangleF"/> structure that represents the rectangle to fill.
        ''' </param>
        Public MustOverride Sub FillRectangle(brush As Brush, rect As RectangleF)
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
        Public MustOverride Sub FillRectangle(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer)
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
        Public MustOverride Sub FillRectangle(brush As Brush, x As Single, y As Single, width As Single, height As Single)

        ''' <summary>
        ''' Fills the interiors of a series of rectangles specified by System.Drawing.Rectangle
        ''' structures.
        ''' </summary>
        ''' <param name="brush">System.Drawing.Brush that determines the characteristics of the fill.</param>
        ''' <param name="rects">Array of System.Drawing.Rectangle structures that represent the rectangles to
        ''' fill.</param>
        Public Sub FillRectangles(brush As Brush, rects() As RectangleF)
            For Each rect In rects
                Call FillRectangle(brush, rect)
            Next
        End Sub

        ''' <summary>
        ''' Fills the interiors of a series of rectangles specified by System.Drawing.Rectangle
        ''' structures.
        ''' </summary>
        ''' <param name="brush">System.Drawing.Brush that determines the characteristics of the fill.</param>
        ''' <param name="rects">Array of System.Drawing.Rectangle structures that represent the rectangles to
        ''' fill.</param>
        Public Sub FillRectangles(brush As Brush, rects() As Rectangle)
            For Each rect In rects
                Call FillRectangle(brush, rect.ToFloat)
            Next
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
        Public MustOverride Sub FillRegion(brush As Brush, region As Region)
        '
        ' Summary:
        '     Forces execution of all pending graphics operations and returns immediately without
        '     waiting for the operations to finish.
        Public MustOverride Sub Flush()
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
        Public MustOverride Sub Flush(intention As FlushIntention)
        '
        ' Summary:
        '     Updates the clip region of this System.Drawing.Graphics to the intersection of
        '     the current clip region and the specified System.Drawing.Region.
        '
        ' Parameters:
        '   region:
        '     System.Drawing.Region to intersect with the current region.
        Public MustOverride Sub IntersectClip(region As Region)
        '
        ' Summary:
        '     Updates the clip region of this System.Drawing.Graphics to the intersection of
        '     the current clip region and the specified System.Drawing.RectangleF structure.
        '
        ' Parameters:
        '   rect:
        '     System.Drawing.RectangleF structure to intersect with the current clip region.
        Public MustOverride Sub IntersectClip(rect As RectangleF)
        '
        ' Summary:
        '     Updates the clip region of this System.Drawing.Graphics to the intersection of
        '     the current clip region and the specified System.Drawing.Rectangle structure.
        '
        ' Parameters:
        '   rect:
        '     System.Drawing.Rectangle structure to intersect with the current clip region.
        Public MustOverride Sub IntersectClip(rect As Rectangle)
        '
        ' Summary:
        '     Multiplies the world transformation of this System.Drawing.Graphics and specified
        '     the System.Drawing.Drawing2D.Matrix.
        '
        ' Parameters:
        '   matrix:
        '     4x4 System.Drawing.Drawing2D.Matrix that multiplies the world transformation.
        Public MustOverride Sub MultiplyTransform(matrix As Matrix)
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
        Public MustOverride Sub MultiplyTransform(matrix As Matrix, order As MatrixOrder)

        ''
        '' Summary:
        ''     Releases a device context handle obtained by a previous call to the System.Drawing.Graphics.GetHdc
        ''     method of this System.Drawing.Graphics.
        ''
        '' Parameters:
        ''   hdc:
        ''     Handle to a device context obtained by a previous call to the System.Drawing.Graphics.GetHdc
        ''     method of this System.Drawing.Graphics.
        '<EditorBrowsable(EditorBrowsableState.Advanced)>
        'Public MustOverride Sub ReleaseHdc(hdc As IntPtr)
        ''
        '' Summary:
        ''     Releases a handle to a device context.
        ''
        '' Parameters:
        ''   hdc:
        ''     Handle to a device context.
        '<EditorBrowsable(EditorBrowsableState.Never)>
        'Public MustOverride Sub ReleaseHdcInternal(hdc As IntPtr)
        '
        ' Summary:
        '     Resets the clip region of this System.Drawing.Graphics to an infinite region.
        Public MustOverride Sub ResetClip()
        '
        ' Summary:
        '     Resets the world transformation matrix of this System.Drawing.Graphics to the
        '     identity matrix.
        Public MustOverride Sub ResetTransform()
        ''
        '' Summary:
        ''     Restores the state of this System.Drawing.Graphics to the state represented by
        ''     a System.Drawing.Drawing2D.GraphicsState.
        ''
        '' Parameters:
        ''   gstate:
        ''     System.Drawing.Drawing2D.GraphicsState that represents the state to which to
        ''     restore this System.Drawing.Graphics.
        'Public MustOverride Sub Restore(gstate As GraphicsState)
        '
        ' Summary:
        '     Applies the specified rotation to the transformation matrix of this System.Drawing.Graphics.
        '
        ' Parameters:
        '   angle:
        '     Angle of rotation in degrees.
        Public MustOverride Sub RotateTransform(angle As Single)
        '
        ' Summary:
        '     Applies the specified rotation to the transformation matrix of this System.Drawing.Graphics
        '     in the specified order.
        '
        ' Parameters:
        '   angle:
        '     Angle of rotation in degrees.
        '
        '   order:
        '     Member of the System.Drawing.Drawing2D.MatrixOrder enumeration that specifies
        '     whether the rotation is appended or prepended to the matrix transformation.
        Public MustOverride Sub RotateTransform(angle As Single, order As MatrixOrder)
        '
        ' Summary:
        '     Applies the specified scaling operation to the transformation matrix of this
        '     System.Drawing.Graphics by prepending it to the object's transformation matrix.
        '
        ' Parameters:
        '   sx:
        '     Scale factor in the x direction.
        '
        '   sy:
        '     Scale factor in the y direction.
        Public MustOverride Sub ScaleTransform(sx As Single, sy As Single)
        '
        ' Summary:
        '     Applies the specified scaling operation to the transformation matrix of this
        '     System.Drawing.Graphics in the specified order.
        '
        ' Parameters:
        '   sx:
        '     Scale factor in the x direction.
        '
        '   sy:
        '     Scale factor in the y direction.
        '
        '   order:
        '     Member of the System.Drawing.Drawing2D.MatrixOrder enumeration that specifies
        '     whether the scaling operation is prepended or appended to the transformation
        '     matrix.
        Public MustOverride Sub ScaleTransform(sx As Single, sy As Single, order As MatrixOrder)
        '
        ' Summary:
        '     Sets the clipping region of this System.Drawing.Graphics to the rectangle specified
        '     by a System.Drawing.RectangleF structure.
        '
        ' Parameters:
        '   rect:
        '     System.Drawing.RectangleF structure that represents the new clip region.
        Public MustOverride Sub SetClip(rect As RectangleF)
        '
        ' Summary:
        '     Sets the clipping region of this System.Drawing.Graphics to the specified System.Drawing.Drawing2D.GraphicsPath.
        '
        ' Parameters:
        '   path:
        '     System.Drawing.Drawing2D.GraphicsPath that represents the new clip region.
        Public MustOverride Sub SetClip(path As GraphicsPath)
        '
        ' Summary:
        '     Sets the clipping region of this System.Drawing.Graphics to the rectangle specified
        '     by a System.Drawing.Rectangle structure.
        '
        ' Parameters:
        '   rect:
        '     System.Drawing.Rectangle structure that represents the new clip region.
        Public MustOverride Sub SetClip(rect As Rectangle)
        '
        ' Summary:
        '     Sets the clipping region of this System.Drawing.Graphics to the Clip property
        '     of the specified System.Drawing.Graphics.
        '
        ' Parameters:
        '   g:
        '     System.Drawing.Graphics from which to take the new clip region.
        Public MustOverride Sub SetClip(g As Graphics)
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
        Public MustOverride Sub SetClip(rect As Rectangle, combineMode As CombineMode)
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
        Public MustOverride Sub SetClip(region As Region, combineMode As CombineMode)
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
        Public MustOverride Sub SetClip(path As GraphicsPath, combineMode As CombineMode)
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
        Public MustOverride Sub SetClip(rect As RectangleF, combineMode As CombineMode)
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
        Public MustOverride Sub SetClip(g As Graphics, combineMode As CombineMode)
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
        Public MustOverride Sub TransformPoints(destSpace As CoordinateSpace, srcSpace As CoordinateSpace, pts() As Point)
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
        Public MustOverride Sub TransformPoints(destSpace As CoordinateSpace, srcSpace As CoordinateSpace, pts() As PointF)
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
        Public MustOverride Sub TranslateClip(dx As Single, dy As Single)
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
        Public MustOverride Sub TranslateClip(dx As Integer, dy As Integer)
        '
        ' Summary:
        '     Changes the origin of the coordinate system by prepending the specified translation
        '     to the transformation matrix of this System.Drawing.Graphics.
        '
        ' Parameters:
        '   dx:
        '     The x-coordinate of the translation.
        '
        '   dy:
        '     The y-coordinate of the translation.
        Public MustOverride Sub TranslateTransform(dx As Single, dy As Single)
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
        Public MustOverride Sub TranslateTransform(dx As Single, dy As Single, order As MatrixOrder)

        Protected Overrides Sub Finalize()
            Call MyBase.Finalize()
        End Sub

        '
        ' Summary:
        '     Saves a graphics container with the current state of this System.Drawing.Graphics
        '     and opens and uses a new graphics container.
        '
        ' Returns:
        '     This method returns a System.Drawing.Drawing2D.GraphicsContainer that represents
        '     the state of this System.Drawing.Graphics at the time of the method call.
        Public MustOverride Function BeginContainer() As GraphicsContainer
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
        Public MustOverride Function BeginContainer(dstrect As RectangleF, srcrect As RectangleF, unit As GraphicsUnit) As GraphicsContainer
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
        Public MustOverride Function BeginContainer(dstrect As Rectangle, srcrect As Rectangle, unit As GraphicsUnit) As GraphicsContainer
        '
        ' Summary:
        '     Gets the cumulative graphics context.
        '
        ' Returns:
        '     An System.Object representing the cumulative graphics context.
        <EditorBrowsable(EditorBrowsableState.Never)>
        Public MustOverride Function GetContextInfo() As Object

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
        Public MustOverride Function GetNearestColor(color As Color) As Color
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
        Public MustOverride Function IsVisible(rect As Rectangle) As Boolean
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
        Public MustOverride Function IsVisible(point As PointF) As Boolean
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
        Public MustOverride Function IsVisible(rect As RectangleF) As Boolean
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
        Public MustOverride Function IsVisible(point As Point) As Boolean
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
        Public MustOverride Function IsVisible(x As Single, y As Single) As Boolean
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
        Public MustOverride Function IsVisible(x As Integer, y As Integer) As Boolean
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
        Public MustOverride Function IsVisible(x As Integer, y As Integer, width As Integer, height As Integer) As Boolean
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
        Public MustOverride Function IsVisible(x As Single, y As Single, width As Single, height As Single) As Boolean
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
        Public MustOverride Function MeasureCharacterRanges(text As String, font As Font, layoutRect As RectangleF, stringFormat As StringFormat) As Region()

        ''' <summary>
        ''' Measures the specified string when drawn with the specified <see cref="Font"/>.
        ''' </summary>
        ''' <param name="text">String to measure.</param>
        ''' <param name="font"><see cref="Font"/> that defines the text format of the string.</param>
        ''' <returns>This method returns a <see cref="SizeF"/> structure that represents the size,
        ''' in the units specified by the <see cref="PageUnit"/> property, of the
        ''' string specified by the text parameter as drawn with the font parameter.
        ''' </returns>
        Public MustOverride Function MeasureString(text As String, font As Font) As SizeF

        ''' <summary>
        ''' Measures the specified string when drawn with the specified System.Drawing.Font.
        ''' </summary>
        ''' <param name="text">String to measure.</param>
        ''' <param name="font">System.Drawing.Font that defines the format of the string.</param>
        ''' <param name="width">Maximum width of the string in pixels.</param>
        ''' <returns>This method returns a System.Drawing.SizeF structure that represents the size,
        ''' in the units specified by the System.Drawing.Graphics.PageUnit property, of the
        ''' string specified in the text parameter as drawn with the font parameter.</returns>
        Public MustOverride Function MeasureString(text As String, font As Font, width As Integer) As SizeF
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
        Public MustOverride Function MeasureString(text As String, font As Font, layoutArea As SizeF) As SizeF
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
        Public MustOverride Function MeasureString(text As String, font As Font, width As Integer, format As StringFormat) As SizeF
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
        Public MustOverride Function MeasureString(text As String, font As Font, origin As PointF, stringFormat As StringFormat) As SizeF
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
        Public MustOverride Function MeasureString(text As String, font As Font, layoutArea As SizeF, stringFormat As StringFormat) As SizeF
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
        Public MustOverride Function MeasureString(text As String, font As Font, layoutArea As SizeF, stringFormat As StringFormat, ByRef charactersFitted As Integer, ByRef linesFilled As Integer) As SizeF

    End Class
End Namespace
