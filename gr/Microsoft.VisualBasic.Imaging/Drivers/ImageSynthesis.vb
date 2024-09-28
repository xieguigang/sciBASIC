#Region "Microsoft.VisualBasic::3168f2de99809415664f74df3350525d, gr\Microsoft.VisualBasic.Imaging\Drivers\ImageSynthesis.vb"

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

    '   Total Lines: 1084
    '    Code Lines: 154 (14.21%)
    ' Comment Lines: 897 (82.75%)
    '    - Xml Docs: 4.91%
    ' 
    '   Blank Lines: 33 (3.04%)
    '     File Size: 46.71 KB


    '     Module ImageSynthesis
    ' 
    '         Sub: (+30 Overloads) DrawImage, (+4 Overloads) DrawImageUnscaled, DrawImageUnscaledAndClipped
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Graphics
Imports System.Drawing.Imaging
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.SVG

Namespace Driver

    ''' <summary>
    ''' Makes the ``DrawImage`` function compatible 
    ''' with old gdi+ interface
    ''' </summary>
    Public Module ImageSynthesis

        ''' <summary>
        ''' Draws the specified System.Drawing.Image, using its original physical size, at
        ''' the specified location.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="image">System.Drawing.Image to draw.</param>
        ''' <param name="point">System.Drawing.Point structure that represents the location of the upper-left
        ''' corner of the drawn image.</param>
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, point As Point)
            Call g.DrawImage(image, point.PointF)
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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, destPoints() As Point)

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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, destPoints() As PointF)

        End Sub

        ''' <summary>
        ''' Draws the specified System.Drawing.Image at the specified location and with the
        ''' specified size.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="image">System.Drawing.Image to draw.</param>
        ''' <param name="rect">System.Drawing.Rectangle structure that specifies the location and size of the
        ''' drawn image.</param>
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, rect As Rectangle)
            Call g.DrawImage(image, New RectangleF(rect.Location.PointF, New SizeF(rect.Size.Width, rect.Size.Height)))
        End Sub

        ''' <summary>
        ''' Draws the specified System.Drawing.Image, using its original physical size, at
        ''' the specified location.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="image">System.Drawing.Image to draw.</param>
        ''' <param name="point">System.Drawing.PointF structure that represents the upper-left corner of the
        ''' drawn image.</param>
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, point As PointF)
            If TypeOf g Is GraphicsSVG Then
                Dim svg As GraphicsSVG = DirectCast(g, GraphicsSVG)

                If image.Driver = Drivers.GDI Then
                    Dim gdi As Image = DirectCast(image, ImageData).Image
                    Dim rect As New Rectangle(point.X, point.Y, gdi.Width, gdi.Height)

                    Call svg.DrawImageUnscaled(gdi, rect)
                Else
                    ' 直接合并SVG的节点
                    Dim imageData As SVGDataLayers = DirectCast(image, SVGData).SVG
                    '在这里还需要根据位置计算出位移
                    Throw New NotImplementedException
                End If
            Else
                ' gdi+ engine只允许gdi+图像合并
                If image.Driver = Drivers.SVG Then
                    Throw New NotImplementedException
                Else
                    Dim gdi As Image = DirectCast(image, ImageData).Image
                    Call g.DrawImage(gdi, point)
                End If
            End If
        End Sub

        ''' <summary>
        ''' Draws the specified System.Drawing.Image at the specified location and with the
        ''' specified size.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="image">System.Drawing.Image to draw.</param>
        ''' <param name="rect">System.Drawing.RectangleF structure that specifies the location and size of the
        ''' drawn image.</param>
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, rect As RectangleF)
            If TypeOf g Is GraphicsSVG Then
                Dim svg As GraphicsSVG = DirectCast(g, GraphicsSVG)
                Dim point As PointF = rect.Location

                If image.Driver = Drivers.GDI Then
                    Call svg.DrawImage(DirectCast(image, ImageData).Image, rect)
                Else
                    ' 直接合并SVG的节点
                    ' 还需要根据原始的大小与现在的rect参数之中的大小进行缩放合成
                    Dim imageData As SVGDataLayers = DirectCast(image, SVGData).SVG
                    '在这里还需要根据位置计算出位移
                    Throw New NotImplementedException
                End If
            Else
                If image.Driver = Drivers.SVG Then
                    Throw New NotImplementedException
                Else
                    Dim gdi As Image = DirectCast(image, ImageData).Image
                    Call g.DrawImage(gdi, rect)
                End If
            End If
        End Sub

        ''' <summary>
        ''' Draws the specified image, using its original physical size, at the location
        ''' specified by a coordinate pair.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="image">System.Drawing.Image to draw.</param>
        ''' <param name="x">The x-coordinate of the upper-left corner of the drawn image.</param>
        ''' <param name="y">The y-coordinate of the upper-left corner of the drawn image.</param>
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, x As Integer, y As Integer)
            Call g.DrawImage(image, New PointF(x, y))
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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, x As Single, y As Single)
            Call g.DrawImage(image, New PointF(x, y))
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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, destRect As RectangleF, srcRect As RectangleF, srcUnit As GraphicsUnit)

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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, destRect As Rectangle, srcRect As Rectangle, srcUnit As GraphicsUnit)

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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit)

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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit)

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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, x As Single, y As Single, width As Single, height As Single)
            Call g.DrawImage(image, New RectangleF(New PointF(x, y), New SizeF(width, height)))
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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, destPoints() As Point, srcRect As Rectangle, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)

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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, x As Integer, y As Integer, width As Integer, height As Integer)
            Call g.DrawImage(image, New RectangleF(New PointF(x, y), New SizeF(width, height)))
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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, x As Single, y As Single, srcRect As RectangleF, srcUnit As GraphicsUnit)

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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)

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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, x As Integer, y As Integer, srcRect As Rectangle, srcUnit As GraphicsUnit)

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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, destPoints() As Point,
                             srcRect As Rectangle, srcUnit As GraphicsUnit,
                             imageAttr As ImageAttributes,
                             callback As DrawImageAbort)

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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, destPoints() As PointF,
                             srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort)

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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, destPoints() As Point,
                             srcRect As Rectangle, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort, callbackData As Integer)

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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, destRect As Rectangle,
                             srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit)

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
        <Extension>
        Public Sub DrawImage(g As IGraphics, image As GraphicsData, destRect As Rectangle,
                             srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit)

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
        <Extension> Public Sub DrawImage(g As IGraphics, image As GraphicsData, destPoints() As PointF, srcRect As RectangleF, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort, callbackData As Integer)

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
        <Extension> Public Sub DrawImage(g As IGraphics, image As GraphicsData, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes)

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
        <Extension> Public Sub DrawImage(g As IGraphics, image As GraphicsData, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttr As ImageAttributes)

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
        <Extension> Public Sub DrawImage(g As IGraphics, image As GraphicsData, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttr As ImageAttributes, callback As DrawImageAbort)

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
        <Extension> Public Sub DrawImage(g As IGraphics, image As GraphicsData, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As DrawImageAbort)

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
        <Extension> Public Sub DrawImage(g As IGraphics, image As GraphicsData, destRect As Rectangle, srcX As Single, srcY As Single, srcWidth As Single, srcHeight As Single, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As DrawImageAbort, callbackData As IntPtr)

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
        <Extension> Public Sub DrawImage(g As IGraphics, image As GraphicsData, destRect As Rectangle, srcX As Integer, srcY As Integer, srcWidth As Integer, srcHeight As Integer, srcUnit As GraphicsUnit, imageAttrs As ImageAttributes, callback As DrawImageAbort, callbackData As IntPtr)

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
        <Extension> Public Sub DrawImageUnscaled(g As IGraphics, image As GraphicsData, rect As Rectangle)
            Call g.DrawImage(image, rect.Location.PointF)
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
        <Extension> Public Sub DrawImageUnscaled(g As IGraphics, image As GraphicsData, point As Point)
            Call g.DrawImage(image, point.PointF)
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
        <Extension> Public Sub DrawImageUnscaled(g As IGraphics, image As GraphicsData, x As Integer, y As Integer)
            Call g.DrawImage(image, New PointF(x, y))
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
        <Extension> Public Sub DrawImageUnscaled(g As IGraphics, image As GraphicsData, x As Integer, y As Integer, width As Integer, height As Integer)
            Call g.DrawImage(image, New RectangleF(New PointF(x, y), New SizeF(width, height)))
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
        <Extension> Public Sub DrawImageUnscaledAndClipped(g As IGraphics, image As GraphicsData, rect As Rectangle)

        End Sub
    End Module
End Namespace
