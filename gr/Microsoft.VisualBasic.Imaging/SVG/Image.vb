Imports System.Drawing
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Net.Http.Base64Codec

Namespace SVG

    Public Class Image
        <XmlAttribute> Public Property width As String
        <XmlAttribute> Public Property height As String
        <XmlAttribute("image.data")> Public Property data As String

        Sub New()
        End Sub

        Sub New(image As Bitmap, Optional size As Size = Nothing)
            data = image.ToBase64String
        End Sub
    End Class
End Namespace