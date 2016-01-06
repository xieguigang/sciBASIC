Imports System.Runtime.CompilerServices
Imports System.Drawing
Imports System.Text.RegularExpressions
Imports System.Text
Imports System
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports System.Reflection
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.ComponentModel
Imports System.Drawing.Imaging
Imports System.Drawing.Graphics

''' <summary>
''' GDI+ device handle for encapsulates a GDI+ drawing surface.(GDI+绘图设备句柄)
''' </summary>
''' <remarks></remarks>
Public Class GDIPlusDeviceHandle : Inherits MarshalByRefObject
    Implements IDisposable, IDeviceContext

    ''' <summary>
    ''' GDI+ device handle.(GDI+绘图设备句柄)
    ''' </summary>
    ''' <remarks></remarks>
    Public ReadOnly Property Gr_Device As Graphics
    ''' <summary>
    ''' GDI+ device handle memory
    ''' </summary>
    ''' <remarks></remarks>
    Friend __BitmapResources As Image

    ''' <summary>
    ''' GDI+ device handle memory.(GDI+设备之中的图像数据)
    ''' </summary>
    ''' <remarks></remarks>
    Public ReadOnly Property ImageResource As Image
        Get
            Return __BitmapResources
        End Get
    End Property

    Public ReadOnly Property Width As Integer
        Get
            Return __BitmapResources.Width
        End Get
    End Property

    Public ReadOnly Property Height As Integer
        Get
            Return __BitmapResources.Height
        End Get
    End Property

    ''' <summary>
    ''' 图像的大小
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Size As Size
        Get
            Return __BitmapResources.Size
        End Get
    End Property

    ''' <summary>
    ''' 在图象上面的中心的位置点
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Center As Point

    ''' <summary>
    ''' 将GDI+设备之中的图像数据保存到指定的文件路径之中，默认的图像文件的格式为PNG格式
    ''' </summary>
    ''' <param name="Path"></param>
    ''' <param name="Format">默认为png格式</param>
    ''' <returns></returns>
    Public Function Save(Path As String, Optional Format As ImageFormat.ImageFormats = ImageFormats.Png) As Boolean
        Return Save(Path, Format.GetFormat)
    End Function

    ''' <summary>
    ''' 将GDI+设备之中的图像数据保存到指定的文件路径之中，默认的图像文件的格式为PNG格式
    ''' </summary>
    ''' <param name="Path"></param>
    ''' <param name="Format">默认为png格式</param>
    ''' <returns></returns>
    Public Function Save(Path As String, Optional Format As System.Drawing.Imaging.ImageFormat = Nothing) As Boolean
        If Format Is Nothing Then
            Format = System.Drawing.Imaging.ImageFormat.Png
        End If

        Try
            Call __save(Path, Format)
        Catch ex As Exception
            Return App.LogException(ex, MethodBase.GetCurrentMethod.GetFullName)
        End Try

        Return True
    End Function

    Private Sub __save(path As String, format As System.Drawing.Imaging.ImageFormat)
        Call FileIO.FileSystem.CreateDirectory(FileIO.FileSystem.GetParentPath(path))
        Call __BitmapResources.Save(path, format)
    End Sub

    Public Overrides Function ToString() As String
        Return __BitmapResources.Size.ToString
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="r"></param>
    ''' <param name="filled">所填充的颜色</param>
    ''' <returns></returns>
    Public Shared Function CreateDevice(r As Drawing.Size, Optional filled As Color = Nothing) As GDIPlusDeviceHandle
        Return r.CreateGDIDevice(filled)
    End Function

    Public Shared Narrowing Operator CType(obj As GDIPlusDeviceHandle) As Image
        Return obj.__BitmapResources
    End Operator

    Public Shared Widening Operator CType(obj As Image) As GDIPlusDeviceHandle
        Dim Gr As Graphics = Graphics.FromImage(obj)
        Return New GDIPlusDeviceHandle With {
            .__BitmapResources = obj,
            ._Gr_Device = Gr,
            ._Center = New Point(CInt(obj.Width / 2), CInt(obj.Height / 2))
        }
    End Operator

    Public Shared Widening Operator CType(obj As Bitmap) As GDIPlusDeviceHandle
        Dim Gr As Graphics = Graphics.FromImage(obj)
        Return New GDIPlusDeviceHandle With {
            .__BitmapResources = obj,
            ._Gr_Device = Gr,
            ._Center = New Point(CInt(obj.Width / 2), CInt(obj.Height / 2))
        }
    End Operator

    Friend Shared Function CreateObject(g As Graphics, res As Image) As GDIPlusDeviceHandle
        Return New GDIPlusDeviceHandle With {
            .__BitmapResources = res,
            ._Gr_Device = g,
            ._Center = New Point(CInt(res.Width / 2), CInt(res.Height / 2))
        }
    End Function

    ''' <summary>
    ''' Releases all resources used by this System.Drawing.Graphics.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        Call Me.Gr_Device.Dispose()
        Call Me.__BitmapResources.Dispose()
    End Sub

#Region "Implements Class Graphics"

    '
    ' Summary:
    '     Gets or sets a System.Drawing.Region that limits the drawing region of this System.Drawing.Graphics.
    '
    ' Returns:
    '     A System.Drawing.Region that limits the portion of this System.Drawing.Graphics
    '     that is currently available for drawing.
    Public Property Clip As Region
        Get
            Return Gr_Device.Clip
        End Get
        Set(value As Region)
            Gr_Device.Clip = value
        End Set
    End Property
    '
    ' Summary:
    '     Gets a System.Drawing.RectangleF structure that bounds the clipping region of
    '     this System.Drawing.Graphics.
    '
    ' Returns:
    '     A System.Drawing.RectangleF structure that represents a bounding rectangle for
    '     the clipping region of this System.Drawing.Graphics.
    Public ReadOnly Property ClipBounds As RectangleF
        Get
            Return Gr_Device.ClipBounds
        End Get
    End Property

    ''' <summary>
    ''' Gets a value that specifies how composited images are drawn to this System.Drawing.Graphics.
    ''' </summary>
    ''' <returns>
    ''' This property specifies a member of the System.Drawing.Drawing2D.CompositingMode enumeration. 
    ''' The default is System.Drawing.Drawing2D.CompositingMode.SourceOver.
    ''' </returns>
    Public Property CompositingMode As CompositingMode
        Get
            Return Gr_Device.CompositingMode
        End Get
        Set(value As CompositingMode)
            Gr_Device.CompositingMode = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the rendering quality of composited images drawn to this System.Drawing.Graphics.
    ''' </summary>
    ''' <returns>
    ''' This property specifies a member of the System.Drawing.Drawing2D.CompositingQuality enumeration. 
    ''' The default is System.Drawing.Drawing2D.CompositingQuality.Default.
    ''' </returns>
    Public Property CompositingQuality As CompositingQuality
        Get
            Return Gr_Device.CompositingQuality
        End Get
        Set(value As CompositingQuality)
            Gr_Device.CompositingQuality = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the horizontal resolution of this System.Drawing.Graphics.
    ''' </summary>
    ''' <returns>
    ''' The value, in dots per inch, for the horizontal resolution supported by this System.Drawing.Graphics.
    ''' </returns>
    Public ReadOnly Property DpiX As Single
        Get
            Return Gr_Device.DpiX
        End Get
    End Property
    '
    ' Summary:
    '     Gets the vertical resolution of this System.Drawing.Graphics.
    '
    ' Returns:
    '     The value, in dots per inch, for the vertical resolution supported by this System.Drawing.Graphics.
    Public ReadOnly Property DpiY As Single
        Get
            Return Gr_Device.DpiY
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the interpolation mode associated with this System.Drawing.Graphics.
    ''' </summary>
    ''' <returns>One of the System.Drawing.Drawing2D.InterpolationMode values.</returns>
    Public Property InterpolationMode As InterpolationMode
        Get
            Return Gr_Device.InterpolationMode
        End Get
        Set(value As InterpolationMode)
            Gr_Device.InterpolationMode = value
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
    Public ReadOnly Property IsClipEmpty As Boolean
        Get
            Return Gr_Device.IsClipEmpty
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
    Public ReadOnly Property IsVisibleClipEmpty As Boolean
        Get
            Return Gr_Device.IsVisibleClipEmpty
        End Get
    End Property
    '
    ' Summary:
    '     Gets or sets the scaling between world units and page units for this System.Drawing.Graphics.
    '
    ' Returns:
    '     This property specifies a value for the scaling between world units and page
    '     units for this System.Drawing.Graphics.
    Public Property PageScale As Single
        Get
            Return Gr_Device.PageScale
        End Get
        Set(value As Single)
            Gr_Device.PageScale = value
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
    Public Property PageUnit As GraphicsUnit
        Get
            Return Gr_Device.PageUnit
        End Get
        Set(value As GraphicsUnit)
            Gr_Device.PageUnit = value
        End Set
    End Property
    '
    ' Summary:
    '     Gets or set a value specifying how pixels are offset during rendering of this
    '     System.Drawing.Graphics.
    '
    ' Returns:
    '     This property specifies a member of the System.Drawing.Drawing2D.PixelOffsetMode
    '     enumeration
    Public Property PixelOffsetMode As PixelOffsetMode
        Get
            Return Gr_Device.PixelOffsetMode
        End Get
        Set(value As PixelOffsetMode)
            Gr_Device.PixelOffsetMode = value
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
    Public Property RenderingOrigin As Point
        Get
            Return Gr_Device.RenderingOrigin
        End Get
        Set(value As Point)
            Gr_Device.RenderingOrigin = value
        End Set
    End Property
    '
    ' Summary:
    '     Gets or sets the rendering quality for this System.Drawing.Graphics.
    '
    ' Returns:
    '     One of the System.Drawing.Drawing2D.SmoothingMode values.
    Public Property SmoothingMode As SmoothingMode
        Get
            Return Gr_Device.SmoothingMode
        End Get
        Set(value As SmoothingMode)
            Gr_Device.SmoothingMode = value
        End Set
    End Property
    '
    ' Summary:
    '     Gets or sets the gamma correction value for rendering text.
    '
    ' Returns:
    '     The gamma correction value used for rendering antialiased and ClearType text.
    Public Property TextContrast As Integer
        Get
            Return Gr_Device.TextContrast
        End Get
        Set(value As Integer)
            Gr_Device.TextContrast = value
        End Set
    End Property
    '
    ' Summary:
    '     Gets or sets the rendering mode for text associated with this System.Drawing.Graphics.
    '
    ' Returns:
    '     One of the System.Drawing.Text.TextRenderingHint values.
    Public Property TextRenderingHint As TextRenderingHint
        Get
            Return Gr_Device.TextRenderingHint
        End Get
        Set(value As TextRenderingHint)
            Gr_Device.TextRenderingHint = value
        End Set
    End Property
    '
    ' Summary:
    '     Gets or sets a copy of the geometric world transformation for this System.Drawing.Graphics.
    '
    ' Returns:
    '     A copy of the System.Drawing.Drawing2D.Matrix that represents the geometric world
    '     transformation for this System.Drawing.Graphics.
    Public Property Transform As Drawing2D.Matrix
        Get
            Return Gr_Device.Transform
        End Get
        Set(value As Drawing2D.Matrix)
            Gr_Device.Transform = value
        End Set
    End Property
    '
    ' Summary:
    '     Gets the bounding rectangle of the visible clipping region of this System.Drawing.Graphics.
    '
    ' Returns:
    '     A System.Drawing.RectangleF structure that represents a bounding rectangle for
    '     the visible clipping region of this System.Drawing.Graphics.
    Public ReadOnly Property VisibleClipBounds As RectangleF
        Get
            Return Gr_Device.VisibleClipBounds
        End Get
    End Property

    '
    ' Summary:
    '     Adds a comment to the current System.Drawing.Imaging.Metafile.
    '
    ' Parameters:
    '   data:
    '     Array of bytes that contains the comment.
    Public Sub AddMetafileComment(data() As Byte)
        Call Gr_Device.AddMetafileComment(data)
    End Sub
    '
    ' Summary:
    '     Clears the entire drawing surface and fills it with the specified background
    '     color.
    '
    ' Parameters:
    '   color:
    '     System.Drawing.Color structure that represents the background color of the drawing
    '     surface.
    Public Sub Clear(color As Color)
        Call Gr_Device.Clear(color)
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
    ' Exceptions:
    '   T:System.ComponentModel.Win32Exception:
    '     The operation failed.
    Public Sub CopyFromScreen(upperLeftSource As Point, upperLeftDestination As Point, blockRegionSize As Size)
        Call Gr_Device.CopyFromScreen(upperLeftSource, upperLeftDestination, blockRegionSize)
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
    Public Sub CopyFromScreen(upperLeftSource As Point, upperLeftDestination As Point, blockRegionSize As Size, copyPixelOperation As CopyPixelOperation)
        Call Gr_Device.CopyFromScreen(upperLeftSource, upperLeftDestination, blockRegionSize, copyPixelOperation)
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
    Public Sub CopyFromScreen(sourceX As Integer, sourceY As Integer, destinationX As Integer, destinationY As Integer, blockRegionSize As Size)
        Call Gr_Device.CopyFromScreen(sourceX, sourceY, destinationX, destinationY, blockRegionSize)
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
    Public Sub CopyFromScreen(sourceX As Integer, sourceY As Integer, destinationX As Integer, destinationY As Integer, blockRegionSize As Size, copyPixelOperation As CopyPixelOperation)
        Call Gr_Device.CopyFromScreen(sourceX, sourceY, destinationX, destinationY, blockRegionSize, copyPixelOperation)
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
    Public Sub DrawArc(pen As Pen, rect As Rectangle, startAngle As Single, sweepAngle As Single)

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
    Public Sub DrawArc(pen As Pen, rect As RectangleF, startAngle As Single, sweepAngle As Single)

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
    Public Sub DrawArc(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)

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
    Public Sub DrawArc(pen As Pen, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)

    End Sub
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
    Public Sub DrawBezier(pen As Pen, pt1 As Point, pt2 As Point, pt3 As Point, pt4 As Point)

    End Sub
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
    Public Sub DrawBezier(pen As Pen, pt1 As PointF, pt2 As PointF, pt3 As PointF, pt4 As PointF)

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
    Public Sub DrawBezier(pen As Pen, x1 As Single, y1 As Single, x2 As Single, y2 As Single, x3 As Single, y3 As Single, x4 As Single, y4 As Single)

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
    Public Sub DrawBeziers(pen As Pen, points() As Point)

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
    Public Sub DrawBeziers(pen As Pen, points() As PointF)

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
    Public Sub DrawClosedCurve(pen As Pen, points() As Point)

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
    Public Sub DrawClosedCurve(pen As Pen, points() As PointF)

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
    Public Sub DrawClosedCurve(pen As Pen, points() As Point, tension As Single, fillmode As FillMode)

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
    Public Sub DrawClosedCurve(pen As Pen, points() As PointF, tension As Single, fillmode As FillMode)

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
    Public Sub DrawCurve(pen As Pen, points() As Point)

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
    Public Sub DrawCurve(pen As Pen, points() As PointF)

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
    Public Sub DrawCurve(pen As Pen, points() As Point, tension As Single)

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
    Public Sub DrawCurve(pen As Pen, points() As PointF, tension As Single)

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
    Public Sub DrawCurve(pen As Pen, points() As PointF, offset As Integer, numberOfSegments As Integer)

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
    Public Sub DrawCurve(pen As Pen, points() As Point, offset As Integer, numberOfSegments As Integer, tension As Single)

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
    Public Sub DrawCurve(pen As Pen, points() As PointF, offset As Integer, numberOfSegments As Integer, tension As Single)

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
    Public Sub DrawEllipse(pen As Pen, rect As Rectangle)

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
    Public Sub DrawEllipse(pen As Pen, rect As RectangleF)

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
    Public Sub DrawEllipse(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer)

    End Sub
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
    Public Sub DrawEllipse(pen As Pen, x As Single, y As Single, width As Single, height As Single)

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
    Public Sub DrawIcon(icon As Icon, targetRect As Rectangle)

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
    Public Sub DrawIcon(icon As Icon, x As Integer, y As Integer)

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
    Public Sub DrawIconUnstretched(icon As Icon, targetRect As Rectangle)

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
    Public Sub DrawImage(image As Image, rect As RectangleF)

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
    Public Sub DrawImage(image As Image, rect As Rectangle)

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
    Public Sub DrawImage(image As Image, destPoints() As Point)

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
    Public Sub DrawImage(image As Image, destPoints() As PointF)

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
    '     System.Drawing.Point structure that represents the location of the upper-left
    '     corner of the drawn image.
    '
    ' Exceptions:
    '   T:System.ArgumentNullException:
    '     image is null.
    Public Sub DrawImage(image As Image, point As Point)

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
    Public Sub DrawImage(image As Image, point As PointF)

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
    Public Sub DrawImage(image As Image, x As Integer, y As Integer)

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
    Public Sub DrawImage(image As Image, x As Single, y As Single)

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
    Public Sub DrawImage(image As Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit)

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
    Public Sub DrawImage(image As Image, destRect As Rectangle, srcRect As Rectangle, srcUnit As GraphicsUnit)

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
    ' Exceptions:
    '   T:System.ArgumentNullException:
    '     image is null.
    Public Sub DrawImage(image As Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit)

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
    Public Sub DrawImage(image As Image, destRect As RectangleF, srcRect As RectangleF, srcUnit As GraphicsUnit)

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
    Public Sub DrawImage(image As Image, x As Single, y As Single, srcRect As RectangleF, srcUnit As GraphicsUnit)

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
    Public Sub DrawImage(image As Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)

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
    Public Sub DrawImage(image As Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)

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
    Public Sub DrawImage(image As Image, x As Integer, y As Integer, srcRect As Rectangle, srcUnit As GraphicsUnit)

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
    Public Sub DrawImage(image As Image, x As Integer, y As Integer, width As Integer, height As Integer)

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
    Public Sub DrawImage(image As Image, x As Single, y As Single, width As Single, height As Single)

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
    Public Sub DrawImage(image As Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort)

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
    Public Sub DrawImage(image As Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort)

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
    Public Sub DrawImage(image As Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit)

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
    Public Sub DrawImage(image As Image, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort, callbackData As Integer)

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
    Public Sub DrawImage(image As Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit)

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
    Public Sub DrawImage(image As Image, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort, callbackData As Integer)

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
    Public Sub DrawImage(image As Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)

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
    Public Sub DrawImage(image As Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes)

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
    Public Sub DrawImage(image As Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort)

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
    Public Sub DrawImage(image As Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As DrawImageAbort)

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
    Public Sub DrawImage(image As Image, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As DrawImageAbort, callbackData As IntPtr)

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
    Public Sub DrawImage(image As Image, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As DrawImageAbort, callbackData As IntPtr)

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
    Public Sub DrawImageUnscaled(image As Image, rect As Rectangle)

    End Sub
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
    Public Sub DrawImageUnscaled(image As Image, point As Point)

    End Sub
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
    Public Sub DrawImageUnscaled(image As Image, x As Integer, y As Integer)

    End Sub
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
    Public Sub DrawImageUnscaled(image As Image, x As Integer, y As Integer, width As Integer, height As Integer)

    End Sub
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
    Public Sub DrawImageUnscaledAndClipped(image As Image, rect As Rectangle)

    End Sub
    '
    ' Summary:
    '     Draws a line connecting two System.Drawing.Point structures.
    '
    ' Parameters:
    '   pen:
    '     System.Drawing.Pen that determines the color, width, and style of the line.
    '
    '   pt1:
    '     System.Drawing.Point structure that represents the first point to connect.
    '
    '   pt2:
    '     System.Drawing.Point structure that represents the second point to connect.
    '
    ' Exceptions:
    '   T:System.ArgumentNullException:
    '     pen is null.
    Public Sub DrawLine(pen As Pen, pt1 As Point, pt2 As Point)

    End Sub
    '
    ' Summary:
    '     Draws a line connecting two System.Drawing.PointF structures.
    '
    ' Parameters:
    '   pen:
    '     System.Drawing.Pen that determines the color, width, and style of the line.
    '
    '   pt1:
    '     System.Drawing.PointF structure that represents the first point to connect.
    '
    '   pt2:
    '     System.Drawing.PointF structure that represents the second point to connect.
    '
    ' Exceptions:
    '   T:System.ArgumentNullException:
    '     pen is null.
    Public Sub DrawLine(pen As Pen, pt1 As PointF, pt2 As PointF)

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
    Public Sub DrawLine(pen As Pen, x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer)

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
    Public Sub DrawLine(pen As Pen, x1 As Single, y1 As Single, x2 As Single, y2 As Single)

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
    Public Sub DrawLines(pen As Pen, points() As Point)

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
    Public Sub DrawLines(pen As Pen, points() As PointF)

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
    Public Sub DrawPath(pen As Pen, path As GraphicsPath)

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
    Public Sub DrawPie(pen As Pen, rect As Rectangle, startAngle As Single, sweepAngle As Single)

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
    Public Sub DrawPie(pen As Pen, rect As RectangleF, startAngle As Single, sweepAngle As Single)

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
    Public Sub DrawPie(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)

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

    Public Sub DrawPie(pen As Pen, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)

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
    Public Sub DrawPolygon(pen As Pen, points() As Point)

    End Sub
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
    Public Sub DrawPolygon(pen As Pen, points() As PointF)

    End Sub
    '
    ' Summary:
    '     Draws a rectangle specified by a System.Drawing.Rectangle structure.
    '
    ' Parameters:
    '   pen:
    '     A System.Drawing.Pen that determines the color, width, and style of the rectangle.
    '
    '   rect:
    '     A System.Drawing.Rectangle structure that represents the rectangle to draw.
    '
    ' Exceptions:
    '   T:System.ArgumentNullException:
    '     pen is null.
    Public Sub DrawRectangle(pen As Pen, rect As Rectangle)

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
    Public Sub DrawRectangle(pen As Pen, x As Integer, y As Integer, width As Integer, height As Integer)

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
    Public Sub DrawRectangle(pen As Pen, x As Single, y As Single, width As Single, height As Single)

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
    Public Sub DrawRectangles(pen As Pen, rects() As Rectangle)

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
    Public Sub DrawRectangles(pen As Pen, rects() As RectangleF)

    End Sub
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
    Public Sub DrawString(s As String, font As Font, brush As Brush, layoutRectangle As RectangleF)
        Call Gr_Device.DrawString(s, font, brush, layoutRectangle)
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
    '   point:
    '     System.Drawing.PointF structure that specifies the upper-left corner of the drawn
    '     text.
    '
    ' Exceptions:
    '   T:System.ArgumentNullException:
    '     brush is null.-or-s is null.
    Public Sub DrawString(s As String, font As Font, brush As Brush, point As PointF)
        Call Gr_Device.DrawString(s, font, brush, point)
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
    Public Sub DrawString(s As String, font As Font, brush As Brush, point As PointF, format As StringFormat)
        Call Gr_Device.DrawString(s, font, brush, point, format)
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
    Public Sub DrawString(s As String, font As Font, brush As Brush, layoutRectangle As RectangleF, format As StringFormat)
        Call Gr_Device.DrawString(s, font, brush, layoutRectangle, format)
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
    Public Sub DrawString(s As String, font As Font, brush As Brush, x As Single, y As Single)
        Call Gr_Device.DrawString(s, font, brush, x, y)
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
    Public Sub DrawString(s As String, font As Font, brush As Brush, x As Single, y As Single, format As StringFormat)
        Call Gr_Device.DrawString(s, font, brush, x, y, format)
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
    Public Sub EndContainer(container As GraphicsContainer)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, callback As EnumerateMetafileProc)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, callback As EnumerateMetafileProc)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, callback As EnumerateMetafileProc)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, callback As EnumerateMetafileProc)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, callback As EnumerateMetafileProc)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, callback As EnumerateMetafileProc)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, callback As EnumerateMetafileProc, callbackData As IntPtr)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, callback As EnumerateMetafileProc, callbackData As IntPtr)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, callback As EnumerateMetafileProc, callbackData As IntPtr)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, callback As EnumerateMetafileProc, callbackData As IntPtr)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, callback As EnumerateMetafileProc, callbackData As IntPtr)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, callback As EnumerateMetafileProc, callbackData As IntPtr)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoint As Point, srcRect As Rectangle, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoints() As PointF, srcRect As RectangleF, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destRect As Rectangle, srcRect As Rectangle, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destRect As RectangleF, srcRect As RectangleF, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoints() As Point, srcRect As Rectangle, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

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
    Public Sub EnumerateMetafile(metafile As Metafile, destPoint As PointF, srcRect As RectangleF, unit As GraphicsUnit, callback As EnumerateMetafileProc, callbackData As IntPtr, imageAttr As ImageAttributes)

    End Sub
    '
    ' Summary:
    '     Updates the clip region of this System.Drawing.Graphics to exclude the area specified
    '     by a System.Drawing.Region.
    '
    ' Parameters:
    '   region:
    '     System.Drawing.Region that specifies the region to exclude from the clip region.
    Public Sub ExcludeClip(region As Region)

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
    Public Sub ExcludeClip(rect As Rectangle)

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
    Public Sub FillClosedCurve(brush As Brush, points() As Point)

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
    Public Sub FillClosedCurve(brush As Brush, points() As PointF)

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
    Public Sub FillClosedCurve(brush As Brush, points() As Point, fillmode As FillMode)

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
    Public Sub FillClosedCurve(brush As Brush, points() As PointF, fillmode As FillMode)

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
    Public Sub FillClosedCurve(brush As Brush, points() As Point, fillmode As FillMode, tension As Single)

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
    Public Sub FillClosedCurve(brush As Brush, points() As PointF, fillmode As FillMode, tension As Single)

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
    Public Sub FillEllipse(brush As Brush, rect As Rectangle)

    End Sub
    '
    ' Summary:
    '     Fills the interior of an ellipse defined by a bounding rectangle specified by
    '     a System.Drawing.RectangleF structure.
    '
    ' Parameters:
    '   brush:
    '     System.Drawing.Brush that determines the characteristics of the fill.
    '
    '   rect:
    '     System.Drawing.RectangleF structure that represents the bounding rectangle that
    '     defines the ellipse.
    '
    ' Exceptions:
    '   T:System.ArgumentNullException:
    '     brush is null.
    Public Sub FillEllipse(brush As Brush, rect As RectangleF)

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
    Public Sub FillEllipse(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer)

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
    Public Sub FillEllipse(brush As Brush, x As Single, y As Single, width As Single, height As Single)

    End Sub
    '
    ' Summary:
    '     Fills the interior of a System.Drawing.Drawing2D.GraphicsPath.
    '
    ' Parameters:
    '   brush:
    '     System.Drawing.Brush that determines the characteristics of the fill.
    '
    '   path:
    '     System.Drawing.Drawing2D.GraphicsPath that represents the path to fill.
    '
    ' Exceptions:
    '   T:System.ArgumentNullException:
    '     brush is null.-or-path is null.
    Public Sub FillPath(brush As Brush, path As GraphicsPath)

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
    Public Sub FillPie(brush As Brush, rect As Rectangle, startAngle As Single, sweepAngle As Single)

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
    Public Sub FillPie(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer, startAngle As Integer, sweepAngle As Integer)

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
    Public Sub FillPie(brush As Brush, x As Single, y As Single, width As Single, height As Single, startAngle As Single, sweepAngle As Single)

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
    Public Sub FillPolygon(brush As Brush, points() As Point)

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
    Public Sub FillPolygon(brush As Brush, points() As PointF)

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
    Public Sub FillPolygon(brush As Brush, points() As Point, fillMode As FillMode)

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
    Public Sub FillPolygon(brush As Brush, points() As PointF, fillMode As FillMode)

    End Sub
    '
    ' Summary:
    '     Fills the interior of a rectangle specified by a System.Drawing.Rectangle structure.
    '
    ' Parameters:
    '   brush:
    '     System.Drawing.Brush that determines the characteristics of the fill.
    '
    '   rect:
    '     System.Drawing.Rectangle structure that represents the rectangle to fill.
    '
    ' Exceptions:
    '   T:System.ArgumentNullException:
    '     brush is null.
    Public Sub FillRectangle(brush As Brush, rect As Rectangle)
        Call Gr_Device.FillRectangle(brush, rect)
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
    Public Sub FillRectangle(brush As Brush, rect As RectangleF)
        Call Gr_Device.FillRectangle(brush, rect)
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
    Public Sub FillRectangle(brush As Brush, x As Integer, y As Integer, width As Integer, height As Integer)

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
    Public Sub FillRectangle(brush As Brush, x As Single, y As Single, width As Single, height As Single)

    End Sub
    '
    ' Summary:
    '     Fills the interiors of a series of rectangles specified by System.Drawing.Rectangle
    '     structures.
    '
    ' Parameters:
    '   brush:
    '     System.Drawing.Brush that determines the characteristics of the fill.
    '
    '   rects:
    '     Array of System.Drawing.Rectangle structures that represent the rectangles to
    '     fill.
    '
    ' Exceptions:
    '   T:System.ArgumentNullException:
    '     brush is null.-or-rects is null.
    '
    '   T:System.ArgumentException:
    '     rects is a zero-length array.
    Public Sub FillRectangles(brush As Brush, rects() As Rectangle)

    End Sub
    '
    ' Summary:
    '     Fills the interiors of a series of rectangles specified by System.Drawing.RectangleF
    '     structures.
    '
    ' Parameters:
    '   brush:
    '     System.Drawing.Brush that determines the characteristics of the fill.
    '
    '   rects:
    '     Array of System.Drawing.RectangleF structures that represent the rectangles to
    '     fill.
    '
    ' Exceptions:
    '   T:System.ArgumentNullException:
    '     brush is null.-or-rects is null.
    '
    '   T:System.ArgumentException:
    '     Rects is a zero-length array.
    Public Sub FillRectangles(brush As Brush, rects() As RectangleF)

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
    Public Sub FillRegion(brush As Brush, region As Region)

    End Sub
    '
    ' Summary:
    '     Forces execution of all pending graphics operations and returns immediately without
    '     waiting for the operations to finish.
    Public Sub Flush()

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
    Public Sub Flush(intention As FlushIntention)

    End Sub
    '
    ' Summary:
    '     Updates the clip region of this System.Drawing.Graphics to the intersection of
    '     the current clip region and the specified System.Drawing.RectangleF structure.
    '
    ' Parameters:
    '   rect:
    '     System.Drawing.RectangleF structure to intersect with the current clip region.
    Public Sub IntersectClip(rect As RectangleF)

    End Sub
    '
    ' Summary:
    '     Updates the clip region of this System.Drawing.Graphics to the intersection of
    '     the current clip region and the specified System.Drawing.Region.
    '
    ' Parameters:
    '   region:
    '     System.Drawing.Region to intersect with the current region.
    Public Sub IntersectClip(region As Region)

    End Sub
    '
    ' Summary:
    '     Updates the clip region of this System.Drawing.Graphics to the intersection of
    '     the current clip region and the specified System.Drawing.Rectangle structure.
    '
    ' Parameters:
    '   rect:
    '     System.Drawing.Rectangle structure to intersect with the current clip region.
    Public Sub IntersectClip(rect As Rectangle)

    End Sub
    '
    ' Summary:
    '     Multiplies the world transformation of this System.Drawing.Graphics and specified
    '     the System.Drawing.Drawing2D.Matrix.
    '
    ' Parameters:
    '   matrix:
    '     4x4 System.Drawing.Drawing2D.Matrix that multiplies the world transformation.
    Public Sub MultiplyTransform(matrix As Drawing2D.Matrix)

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
    Public Sub MultiplyTransform(matrix As Drawing2D.Matrix, order As MatrixOrder)

    End Sub
    '
    ' Summary:
    '     Releases a device context handle obtained by a previous call to the System.Drawing.Graphics.GetHdc
    '     method of this System.Drawing.Graphics.
    Public Sub ReleaseHdc() Implements IDeviceContext.ReleaseHdc

    End Sub
    '
    ' Summary:
    '     Releases a device context handle obtained by a previous call to the System.Drawing.Graphics.GetHdc
    '     method of this System.Drawing.Graphics.
    '
    ' Parameters:
    '   hdc:
    '     Handle to a device context obtained by a previous call to the System.Drawing.Graphics.GetHdc
    '     method of this System.Drawing.Graphics.
    <EditorBrowsable(EditorBrowsableState.Advanced)>
    Public Sub ReleaseHdc(hdc As IntPtr)

    End Sub
    '
    ' Summary:
    '     Releases a handle to a device context.
    '
    ' Parameters:
    '   hdc:
    '     Handle to a device context.
    <EditorBrowsable(EditorBrowsableState.Never)>
    Public Sub ReleaseHdcInternal(hdc As IntPtr)

    End Sub
    '
    ' Summary:
    '     Resets the clip region of this System.Drawing.Graphics to an infinite region.
    Public Sub ResetClip()

    End Sub
    '
    ' Summary:
    '     Resets the world transformation matrix of this System.Drawing.Graphics to the
    '     identity matrix.
    Public Sub ResetTransform()

    End Sub
    '
    ' Summary:
    '     Restores the state of this System.Drawing.Graphics to the state represented by
    '     a System.Drawing.Drawing2D.GraphicsState.
    '
    ' Parameters:
    '   gstate:
    '     System.Drawing.Drawing2D.GraphicsState that represents the state to which to
    '     restore this System.Drawing.Graphics.
    Public Sub Restore(gstate As GraphicsState)

    End Sub
    '
    ' Summary:
    '     Applies the specified rotation to the transformation matrix of this System.Drawing.Graphics.
    '
    ' Parameters:
    '   angle:
    '     Angle of rotation in degrees.
    Public Sub RotateTransform(angle As Single)

    End Sub
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
    Public Sub RotateTransform(angle As Single, order As MatrixOrder)

    End Sub
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
    Public Sub ScaleTransform(sx As Single, sy As Single)

    End Sub
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
    Public Sub ScaleTransform(sx As Single, sy As Single, order As MatrixOrder)

    End Sub
    '
    ' Summary:
    '     Sets the clipping region of this System.Drawing.Graphics to the rectangle specified
    '     by a System.Drawing.Rectangle structure.
    '
    ' Parameters:
    '   rect:
    '     System.Drawing.Rectangle structure that represents the new clip region.
    Public Sub SetClip(rect As Rectangle)

    End Sub
    '
    ' Summary:
    '     Sets the clipping region of this System.Drawing.Graphics to the rectangle specified
    '     by a System.Drawing.RectangleF structure.
    '
    ' Parameters:
    '   rect:
    '     System.Drawing.RectangleF structure that represents the new clip region.
    Public Sub SetClip(rect As RectangleF)

    End Sub
    '
    ' Summary:
    '     Sets the clipping region of this System.Drawing.Graphics to the specified System.Drawing.Drawing2D.GraphicsPath.
    '
    ' Parameters:
    '   path:
    '     System.Drawing.Drawing2D.GraphicsPath that represents the new clip region.
    Public Sub SetClip(path As GraphicsPath)

    End Sub
    '
    ' Summary:
    '     Sets the clipping region of this System.Drawing.Graphics to the Clip property
    '     of the specified System.Drawing.Graphics.
    '
    ' Parameters:
    '   g:
    '     System.Drawing.Graphics from which to take the new clip region.
    Public Sub SetClip(g As Graphics)

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
    Public Sub SetClip(region As Region, combineMode As CombineMode)

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
    Public Sub SetClip(rect As RectangleF, combineMode As CombineMode)

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
    Public Sub SetClip(rect As Rectangle, combineMode As CombineMode)

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
    Public Sub SetClip(path As GraphicsPath, combineMode As CombineMode)

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
    Public Sub SetClip(g As Graphics, combineMode As CombineMode)

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
    Public Sub TransformPoints(destSpace As CoordinateSpace, srcSpace As CoordinateSpace, pts() As Point)

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
    Public Sub TransformPoints(destSpace As CoordinateSpace, srcSpace As CoordinateSpace, pts() As PointF)

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
    Public Sub TranslateClip(dx As Integer, dy As Integer)

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
    Public Sub TranslateClip(dx As Single, dy As Single)

    End Sub
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
    Public Sub TranslateTransform(dx As Single, dy As Single)

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
    Public Sub TranslateTransform(dx As Single, dy As Single, order As MatrixOrder)

    End Sub
    Protected Overrides Sub Finalize()

    End Sub


    '
    ' Summary:
    '     Saves a graphics container with the current state of this System.Drawing.Graphics
    '     and opens and uses a new graphics container.
    '
    ' Returns:
    '     This method returns a System.Drawing.Drawing2D.GraphicsContainer that represents
    '     the state of this System.Drawing.Graphics at the time of the method call.
    Public Function BeginContainer() As GraphicsContainer
        Throw New NotImplementedException
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
    Public Function BeginContainer(dstrect As Rectangle, srcrect As Rectangle, unit As GraphicsUnit) As GraphicsContainer
        Throw New NotImplementedException
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
    Public Function BeginContainer(dstrect As RectangleF, srcrect As RectangleF, unit As GraphicsUnit) As GraphicsContainer
        Throw New NotImplementedException
    End Function
    '
    ' Summary:
    '     Gets the cumulative graphics context.
    '
    ' Returns:
    '     An System.Object representing the cumulative graphics context.
    <EditorBrowsable(EditorBrowsableState.Never)>
    Public Function GetContextInfo() As Object
        Throw New NotImplementedException
    End Function
    '
    ' Summary:
    '     Gets the handle to the device context associated with this System.Drawing.Graphics.
    '
    ' Returns:
    '     Handle to the device context associated with this System.Drawing.Graphics.
    Public Function GetHdc() As IntPtr Implements IDeviceContext.GetHdc
        Throw New NotImplementedException
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
    Public Function GetNearestColor(color As Color) As Color
        Throw New NotImplementedException
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
    Public Function IsVisible(rect As Rectangle) As Boolean
        Throw New NotImplementedException
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
    Public Function IsVisible(rect As RectangleF) As Boolean
        Throw New NotImplementedException
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
    Public Function IsVisible(point As PointF) As Boolean
        Throw New NotImplementedException
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
    Public Function IsVisible(point As Point) As Boolean
        Throw New NotImplementedException
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
    Public Function IsVisible(x As Single, y As Single) As Boolean
        Throw New NotImplementedException
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
    Public Function IsVisible(x As Integer, y As Integer) As Boolean
        Throw New NotImplementedException
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
    Public Function IsVisible(x As Single, y As Single, width As Single, height As Single) As Boolean
        Throw New NotImplementedException
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
    Public Function IsVisible(x As Integer, y As Integer, width As Integer, height As Integer) As Boolean
        Throw New NotImplementedException
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
    Public Function MeasureCharacterRanges(text As String, font As Font, layoutRect As RectangleF, stringFormat As StringFormat) As Region()
        Throw New NotImplementedException
    End Function
    '
    ' Summary:
    '     Measures the specified string when drawn with the specified System.Drawing.Font.
    '
    ' Parameters:
    '   text:
    '     String to measure.
    '
    '   font:
    '     System.Drawing.Font that defines the text format of the string.
    '
    ' Returns:
    '     This method returns a System.Drawing.SizeF structure that represents the size,
    '     in the units specified by the System.Drawing.Graphics.PageUnit property, of the
    '     string specified by the text parameter as drawn with the font parameter.
    '
    ' Exceptions:
    '   T:System.ArgumentException:
    '     font is null.
    Public Function MeasureString(text As String, font As Font) As SizeF
        Return Gr_Device.MeasureString(text, font)
    End Function
    '
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
    Public Function MeasureString(text As String, font As Font, width As Integer) As SizeF
        Return Gr_Device.MeasureString(text, font, width)
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
    Public Function MeasureString(text As String, font As Font, layoutArea As SizeF) As SizeF
        Return Gr_Device.MeasureString(text, font, layoutArea)
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
    Public Function MeasureString(text As String, font As Font, layoutArea As SizeF, stringFormat As StringFormat) As SizeF
        Return Gr_Device.MeasureString(text, font, layoutArea, stringFormat)
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
    Public Function MeasureString(text As String, font As Font, width As Integer, format As StringFormat) As SizeF
        Return Gr_Device.MeasureString(text, font, width, format)
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
    Public Function MeasureString(text As String, font As Font, origin As PointF, stringFormat As StringFormat) As SizeF
        Return Gr_Device.MeasureString(text, font, origin, stringFormat)
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
    Public Function MeasureString(text As String, font As Font, layoutArea As SizeF, stringFormat As StringFormat, ByRef charactersFitted As Integer, ByRef linesFilled As Integer) As SizeF
        Return Gr_Device.MeasureString(text, font, layoutArea, stringFormat, charactersFitted, linesFilled)
    End Function
    '
    ' Summary:
    '     Saves the current state of this System.Drawing.Graphics and identifies the saved
    '     state with a System.Drawing.Drawing2D.GraphicsState.
    '
    ' Returns:
    '     This method returns a System.Drawing.Drawing2D.GraphicsState that represents
    '     the saved state of this System.Drawing.Graphics.
    Public Function Save() As GraphicsState
        Return Gr_Device.Save
    End Function

#End Region

End Class
