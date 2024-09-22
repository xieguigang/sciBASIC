#Region "Microsoft.VisualBasic::1a279b8e2613e66c11007faed924f282, Microsoft.VisualBasic.Core\src\Extensions\Image\GDI+\Interface.vb"

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

    '   Total Lines: 4457
    '    Code Lines: 296 (6.64%)
    ' Comment Lines: 4094 (91.86%)
    '    - Xml Docs: 8.74%
    ' 
    '   Blank Lines: 67 (1.50%)
    '     File Size: 204.74 KB


    '     Class IGraphics
    ' 
    '         Properties: Background, Dpi, Font, Stroke
    ' 
    '         Function: (+4 Overloads) IsVisible
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
Imports System.Runtime.CompilerServices
Imports std = System.Math

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

        Public ReadOnly Property Width As Integer
            Get
                Return Size.Width
            End Get
        End Property

        Public ReadOnly Property Height As Integer
            Get
                Return Size.Height
            End Get
        End Property

        ''' <summary>
        ''' set background via <see cref="Clear"/> method.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Background As Color

#Region "Default canvas style values, apply for the css styling system"
        ''' <summary>
        ''' Default pen for drawing
        ''' </summary>
        ''' <returns></returns>
        Public Property Stroke As Pen
        ''' <summary>
        ''' Default font value for text drawing
        ''' </summary>
        ''' <returns></returns>
        Public Property Font As Font
#End Region

        ''' <summary>
        ''' max value of the [DpiX, DpiY]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Dpi As Single

        Public Sub New(dpi As Integer)
            Me.Dpi = dpi
        End Sub

        Sub New(dpiXY As Size)
            Call Me.New(std.max(dpiXY.Width, dpiXY.Height))
        End Sub

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
        '     Gets or sets the gamma correction value for rendering text.
        '
        ' Returns:
        '     The gamma correction value used for rendering antialiased and ClearType text.
        Public MustOverride Property TextContrast As Integer

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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Clear(bg As Brush)
            Call FillRectangle(bg, New RectangleF(New PointF, Size.SizeF))
        End Sub

        ''' <summary>
        ''' Clears the entire drawing surface and fills it with the specified background
        ''' color.
        ''' </summary>
        ''' <param name="color"></param>
        Protected MustOverride Sub ClearCanvas(color As Color)

        ''' <summary>
        ''' Releases all resources used by this System.Drawing.Graphics.    
        ''' </summary>
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

        Public MustOverride Sub DrawString(s As String, font As Font, brush As Brush, ByRef x As Single, ByRef y As Single, angle As Single)

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

        ''' <summary>
        ''' Fills the interior of a polygon defined by an array of points specified by System.Drawing.PointF
        ''' structures.
        ''' </summary>
        ''' <param name="brush">System.Drawing.Brush that determines the characteristics of the fill.</param>
        ''' <param name="points">Array of System.Drawing.PointF structures that represent the vertices of the
        ''' polygon to fill.</param>
        Public MustOverride Sub FillPolygon(brush As Brush, points() As PointF)

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
        '     Forces execution of all pending graphics operations and returns immediately without
        '     waiting for the operations to finish.
        Public MustOverride Sub Flush()

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
        '     Sets the clipping region of this System.Drawing.Graphics to the rectangle specified
        '     by a System.Drawing.RectangleF structure.
        '
        ' Parameters:
        '   rect:
        '     System.Drawing.RectangleF structure that represents the new clip region.
        Public MustOverride Sub SetClip(rect As RectangleF)

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


        Protected Overrides Sub Finalize()
            Call MyBase.Finalize()
        End Sub

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

        ''' <summary>
        ''' Indicates whether the specified System.Drawing.PointF structure is contained
        ''' within the visible clip region of this System.Drawing.Graphics.
        ''' </summary>
        ''' <param name="point">System.Drawing.PointF structure to test for visibility.</param>
        ''' <returns>
        ''' true if the point specified by the point parameter is contained within the visible
        ''' clip region of this System.Drawing.Graphics; otherwise, false.
        ''' </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsVisible(point As PointF) As Boolean
            Return IsVisible(point.X, point.Y)
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
        Public MustOverride Function IsVisible(rect As RectangleF) As Boolean

        ''' <summary>
        ''' Indicates whether the specified System.Drawing.Point structure is contained within
        ''' the visible clip region of this System.Drawing.Graphics.
        ''' </summary>
        ''' <param name="point">System.Drawing.Point structure to test for visibility.</param>
        ''' <returns>
        ''' true if the point specified by the point parameter is contained within the visible
        ''' clip region of this System.Drawing.Graphics; otherwise, false.
        ''' </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsVisible(point As Point) As Boolean
            Return IsVisible(point.X, point.Y)
        End Function

        ''' <summary>
        ''' Indicates whether the point specified by a pair of coordinates is contained within
        ''' the visible clip region of this System.Drawing.Graphics.
        ''' </summary>
        ''' <param name="x">The x-coordinate of the point to test for visibility.</param>
        ''' <param name="y">The y-coordinate of the point to test for visibility.</param>
        ''' <returns>
        ''' true if the point defined by the x and y parameters is contained within the visible
        ''' clip region of this System.Drawing.Graphics; otherwise, false.
        ''' </returns>
        Public Function IsVisible(x As Single, y As Single) As Boolean
            If x > Size.Width OrElse x < 0 Then
                Return False
            ElseIf y > Size.Height OrElse y < 0 Then
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' Indicates whether the point specified by a pair of coordinates is contained within
        ''' the visible clip region of this System.Drawing.Graphics.
        ''' </summary>
        ''' <param name="x">The x-coordinate of the point to test for visibility.</param>
        ''' <param name="y">The y-coordinate of the point to test for visibility.</param>
        ''' <returns>
        ''' true if the point defined by the x and y parameters is contained within the visible
        ''' clip region of this System.Drawing.Graphics; otherwise, false.
        ''' </returns>
        Public Function IsVisible(x As Integer, y As Integer) As Boolean
            If x > Size.Width OrElse x < 0 Then
                Return False
            ElseIf y > Size.Height OrElse y < 0 Then
                Return False
            End If

            Return True
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
        Public MustOverride Function IsVisible(x As Integer, y As Integer, width As Integer, height As Integer) As Boolean

        ''' <summary>
        ''' Indicates whether the rectangle specified by a pair of coordinates, a width,
        ''' and a height is contained within the visible clip region of this System.Drawing.Graphics.
        ''' </summary>
        ''' <param name="x">The x-coordinate of the upper-left corner of the rectangle to test for visibility.</param>
        ''' <param name="y">The y-coordinate of the upper-left corner of the rectangle to test for visibility.</param>
        ''' <param name="width">Width of the rectangle to test for visibility.</param>
        ''' <param name="height">Height of the rectangle to test for visibility.</param>
        ''' <returns>true if the rectangle defined by the x, y, width, and height parameters is contained
        ''' within the visible clip region of this System.Drawing.Graphics; otherwise, false.</returns>
        Public MustOverride Function IsVisible(x As Single, y As Single, width As Single, height As Single) As Boolean


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

        ''' <summary>
        ''' Measures the specified string when drawn with the specified System.Drawing.Font
        ''' within the specified layout area.
        ''' </summary>
        ''' <param name="text">String to measure.</param>
        ''' <param name="font">System.Drawing.Font defines the text format of the string.</param>
        ''' <param name="layoutArea">System.Drawing.SizeF structure that specifies the maximum layout area for the
        ''' text.</param>
        ''' <returns>This method returns a System.Drawing.SizeF structure that represents the size,
        ''' in the units specified by the System.Drawing.Graphics.PageUnit property, of the
        ''' string specified by the text parameter as drawn with the font parameter.</returns>
        Public MustOverride Function MeasureString(text As String, font As Font, layoutArea As SizeF) As SizeF

    End Class
End Namespace
