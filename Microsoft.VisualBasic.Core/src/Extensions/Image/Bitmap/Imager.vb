Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Linq
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
        ''' resize an image and maintain aspect ratio
        ''' </summary>
        ''' <param name="image">image to resize</param>
        ''' <param name="newWidth">desired width</param>
        ''' <param name="maxHeight">max height</param>
        ''' <param name="onlyResizeIfWider">if image width is smaller than newWidth use image width</param>
        ''' <returns>resized image</returns>
        ''' 
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
