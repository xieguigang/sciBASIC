Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Drawing.Text
Imports System.Runtime.CompilerServices

Public Module ImageExtensions

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="originalImage"></param>
    ''' <param name="brightness"></param>
    ''' <param name="contrast"></param>
    ''' <param name="gamma"></param>
    ''' <remarks>
    ''' 1 means no changed
    ''' </remarks>
    Public Sub Adjust(ByRef originalImage As Bitmap, Optional brightness As Single = 1, Optional contrast As Single = 1, Optional gamma As Single = 1)
        Dim size As New Size(originalImage.Width, originalImage.Height)
        Dim adjustedImage As New Bitmap(size.Width, size.Height)
        Dim adjustedBrightness As Single = brightness - 1.0F
        ' create matrix that will brighten and contrast the image
        Dim ptsArray()() As Single = {
            New Single() {contrast, 0, 0, 0, 0}, ' scale red
            New Single() {0, contrast, 0, 0, 0}, ' scale green
            New Single() {0, 0, contrast, 0, 0}, ' scale blue
            New Single() {0, 0, 0, 1.0F, 0}, ' don't scale alpha
            New Single() {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}
        }

        Dim imageAttributes As New ImageAttributes()
        imageAttributes.ClearColorMatrix()
        imageAttributes.SetColorMatrix(New ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap)
        imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap)

        Using g As Graphics = Graphics.FromImage(adjustedImage)
            Call g.DrawImage(originalImage, New Rectangle(0, 0, size.Width, size.Height), 0, 0, size.Width, size.Height, GraphicsUnit.Pixel, imageAttributes)
        End Using

        originalImage = adjustedImage
    End Sub
End Module
