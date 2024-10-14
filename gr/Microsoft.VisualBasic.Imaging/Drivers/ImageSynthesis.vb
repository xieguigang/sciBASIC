#Region "Microsoft.VisualBasic::92ed1bc5ed56fcb61313da8869a43171, gr\Microsoft.VisualBasic.Imaging\Drivers\ImageSynthesis.vb"

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

    '   Total Lines: 343
    '    Code Lines: 94 (27.41%)
    ' Comment Lines: 233 (67.93%)
    '    - Xml Docs: 18.88%
    ' 
    '   Blank Lines: 16 (4.66%)
    '     File Size: 13.34 KB


    '     Module ImageSynthesis
    ' 
    '         Sub: (+10 Overloads) DrawImage, (+4 Overloads) DrawImageUnscaled, DrawImageUnscaledAndClipped
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
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
