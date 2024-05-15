#Region "Microsoft.VisualBasic::b96b1e2a603e3c2a2514c3187a47eb74, Microsoft.VisualBasic.Core\src\Extensions\Image\Bitmap\Imager.vb"

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

    '   Total Lines: 137
    '    Code Lines: 65
    ' Comment Lines: 55
    '   Blank Lines: 17
    '     File Size: 5.86 KB


    '     Module Imager
    ' 
    '         Function: GetEncoderInfo, ImageCrop, PutOnCanvas, PutOnWhiteCanvas, Resize
    '                   (+2 Overloads) ResizeScaled
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Runtime.CompilerServices

Namespace Imaging.BitmapImage

    ''' <summary>
    ''' image resizing and cropping
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/omuleanu/imager
    ''' </remarks>
    Public Module Imager

        ''' <summary>
        ''' get codec info by mime type
        ''' </summary>
        ''' <param name="mimeType"></param>
        ''' <returns></returns>
        Public Function GetEncoderInfo(mimeType As String) As ImageCodecInfo
            Return ImageCodecInfo.GetImageEncoders().FirstOrDefault(Function(t) Equals(t.MimeType, mimeType))
        End Function

        ''' <summary>
        ''' the image remains the same size, and it is placed in the middle of the new canvas
        ''' </summary>
        ''' <param name="image">image to put on canvas</param>
        ''' <param name="width">canvas width</param>
        ''' <param name="height">canvas height</param>
        ''' <param name="canvasColor">canvas color</param>
        ''' <returns></returns>
        Public Function PutOnCanvas(image As Image, width As Integer, height As Integer, canvasColor As Color) As Image
            Dim res = New Bitmap(width, height)
            Using g = Graphics.FromImage(res)
                g.Clear(canvasColor)
                Dim x = (width - image.Width) / 2
                Dim y = (height - image.Height) / 2
                g.DrawImageUnscaled(image, x, y, image.Width, image.Height)
            End Using

            Return res
        End Function

        ''' <summary>
        ''' the image remains the same size, and it is placed in the middle of the new canvas
        ''' </summary>
        ''' <param name="image">image to put on canvas</param>
        ''' <param name="width">canvas width</param>
        ''' <param name="height">canvas height</param>
        ''' <returns></returns>
        Public Function PutOnWhiteCanvas(image As Image, width As Integer, height As Integer) As Image
            Return PutOnCanvas(image, width, height, Color.White)
        End Function

        ''' <summary>
        ''' resize image based on the <see cref="Graphics.DrawImage(Image, Rectangle)"/>
        ''' </summary>
        ''' <param name="image"></param>
        ''' <param name="newSize"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this aspect ratio of the given <paramref name="image"/> will not be keeped.
        ''' </remarks>
        <Extension>
        Public Function ResizeScaled(image As Image, newSize As Size, Optional interpolate As InterpolationMode = InterpolationMode.HighQualityBilinear) As Image
            Using g As Graphics2D = newSize.CreateGDIDevice
                g.CompositingQuality = CompositingQuality.HighQuality
                g.InterpolationMode = interpolate
                g.DrawImage(image, New RectangleF(New PointF, g.Size))

                Return g.ImageResource
            End Using
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ResizeScaled(image As Image, newSize As Integer()) As Image
            Return image.ResizeScaled(New Size(newSize(0), newSize(1)))
        End Function

        ''' <summary>
        ''' resize an image and maintain aspect ratio
        ''' </summary>
        ''' <param name="image">image to resize</param>
        ''' <param name="newWidth">desired width</param>
        ''' <param name="maxHeight">max height</param>
        ''' <param name="onlyResizeIfWider">if image width is smaller than newWidth use image width</param>
        ''' <returns>resized image</returns>
        ''' <remarks>
        ''' the aspect ratio and size scale factor will be evaluated from the <paramref name="newWidth"/> 
        ''' and the original <see cref="Image.Size"/> 
        ''' </remarks>
        <Extension>
        Public Function Resize(image As Image, newWidth As Integer,
                               Optional maxHeight As Integer = Integer.MaxValue,
                               Optional onlyResizeIfWider As Boolean = False,
                               Optional scale As InterpolationMode = InterpolationMode.HighQualityBicubic) As Image

            If onlyResizeIfWider AndAlso image.Width <= newWidth Then newWidth = image.Width

            Dim newHeight As Integer = image.Height * newWidth / image.Width

            If newHeight > maxHeight Then
                ' Resize with height instead
                newWidth = image.Width * maxHeight / image.Height
                newHeight = maxHeight
            End If

            Dim res As New Bitmap(width:=newWidth, height:=newHeight)

            Using graphic = Graphics.FromImage(res)
                graphic.InterpolationMode = scale
                graphic.SmoothingMode = SmoothingMode.HighQuality
                graphic.PixelOffsetMode = PixelOffsetMode.HighQuality
                graphic.CompositingQuality = CompositingQuality.HighQuality
                graphic.DrawImage(image, 0, 0, newWidth, newHeight)
            End Using

            Return res
        End Function

        ''' <summary>
        ''' Crop an image 
        ''' </summary>
        ''' <param name="img">image to crop</param>
        ''' <param name="cropArea">rectangle to crop</param>
        ''' <returns>resulting image</returns>
        ''' 
        <Extension>
        Public Function ImageCrop(img As Image, cropArea As Rectangle) As Image
            Dim bmpImage = New Bitmap(img)
            Dim bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat)
            Return bmpCrop
        End Function
    End Module
End Namespace
