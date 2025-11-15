Imports System.Drawing
Imports System.Runtime.CompilerServices

#If NET48 Then
Imports Bitmap = System.Drawing.Bitmap
#Else
Imports Microsoft.VisualBasic.Imaging.BitmapImage
#End If

Namespace Imaging

    Public Module ImageTools

        ''' <summary>
        ''' Crop an image 
        ''' </summary>
        ''' <param name="img">image to crop</param>
        ''' <param name="cropArea">rectangle to crop</param>
        ''' <returns>resulting image</returns>
        ''' 
        <Extension>
        Public Function ImageCrop(img As Image, cropArea As Rectangle) As Image
            Dim bmpImage As New Bitmap(img)
            Dim bmpCrop As Bitmap

#If NET48 Then
            bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat)
#Else
            Dim buffer As BitmapBuffer = BitmapBuffer.FromBitmap(bmpImage)
            Dim crop As BitmapBuffer = BitmapTools.CropBitmapBuffer(buffer, cropArea)

            bmpCrop = New Bitmap(crop)
#End If
            Return bmpCrop
        End Function


    End Module
End Namespace