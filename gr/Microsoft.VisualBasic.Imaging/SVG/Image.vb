Imports System.Drawing
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Net.Http.Base64Codec
Imports Microsoft.VisualBasic.FileIO

Namespace SVG

    ''' <summary>
    ''' Does SVG support embedding of bitmap images?
    ''' 
    ''' + http://stackoverflow.com/questions/6249664/does-svg-support-embedding-of-bitmap-images
    ''' </summary>
    Public Class Image

        <XmlAttribute> Public Property x As Double
        <XmlAttribute> Public Property y As Double
        <XmlAttribute> Public Property width As String
        <XmlAttribute> Public Property height As String
        <XmlAttribute("image.data")> Public Property data As String

        Sub New()
        End Sub

        Sub New(image As Bitmap, Optional size As Size = Nothing)
            data = "data:image/png;base64," & image.ToBase64String
            If size.IsEmpty Then
                size = image.Size
            End If
            width = size.Width
            height = size.Height
        End Sub

        Sub New(url As String, Optional size As Size = Nothing)
            Call Me.New(MapNetFile(url).LoadImage, size)
        End Sub

        Public Overrides Function ToString() As String
            Return $"<image x=""{x}"" y=""{y}"" width=""{width}"" height=""{height}"" xlink:href=""{data}"">"
        End Function
    End Class
End Namespace