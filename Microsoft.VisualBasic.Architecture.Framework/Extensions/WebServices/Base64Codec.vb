Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices

Public Module Base64Codec

    ''' <summary>
    ''' Function to Get Image from Base64 Encoded String
    ''' </summary>
    ''' <param name="Base64String"></param>
    ''' <param name="format"></param>
    ''' <returns></returns>
    <Extension> Public Function GetImage(Base64String As String, Optional format As ImageFormat.ImageFormats = ImageFormats.Png) As Bitmap
        Try
            If String.IsNullOrEmpty(Base64String) Then Return Nothing  ''Checking The Base64 string validity
            Return __getImageFromBase64(Base64String, ImageFormat.GetFormat(format))
        Catch ex As Exception
            Call ex.PrintException
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Function to Get Image from Base64 Encoded String
    ''' </summary>
    ''' <param name="Base64String"></param>
    ''' <param name="format"></param>
    ''' <returns></returns>
    Private Function __getImageFromBase64(Base64String As String, format As System.Drawing.Imaging.ImageFormat) As Bitmap
        Dim bytData As Byte(), streamImage As Bitmap

        bytData = Convert.FromBase64String(Base64String) ''Convert Base64 to Byte Array

        Using ms As New MemoryStream(bytData) ''Using Memory stream to save image
            streamImage = Image.FromStream(ms) ''Converting image from Memory stream
        End Using

        Return streamImage
    End Function

    ''' <summary>
    ''' Convert the Image from Input to Base64 Encoded String
    ''' </summary>
    ''' <param name="ImageInput"></param>
    ''' <returns></returns>
    <Extension> Public Function ToBase64String(ImageInput As Image, Optional format As ImageFormat.ImageFormats = ImageFormats.Png) As String
        Try
            Return __toBase64String(ImageInput, ImageFormat.GetFormat(format))
        Catch ex As Exception
            Call ex.PrintException
            Return ""
        End Try
    End Function

    Private Function __toBase64String(image As Image, format As System.Drawing.Imaging.ImageFormat) As String
        Dim ms As MemoryStream = New MemoryStream()
        image.Save(ms, format)
        Dim Base64Op As String = Convert.ToBase64String(ms.ToArray())
        Return Base64Op
    End Function
End Module
