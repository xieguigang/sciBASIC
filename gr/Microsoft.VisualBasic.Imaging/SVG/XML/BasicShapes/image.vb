Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' The &lt;image> SVG element includes images inside SVG documents. It can display raster image files or other SVG files.
    '''
    ''' The only image formats SVG software must support are JPEG, PNG, And other SVG files. Animated GIF behavior Is undefined.
    '''
    ''' SVG files displayed With &lt;image> are treated As an image: external resources aren't loaded, :visited styles aren't
    ''' applied, and they cannot be interactive. To include dynamic SVG elements, try &lt;use> with an external URL. To include 
    ''' SVG files and run scripts inside them, try &lt;object> inside of &lt;foreignObject>.
    ''' </summary>
    Public NotInheritable Class SvgImage : Inherits SvgBasicShape

        Public Property X As Double
            Get
                Return Element.GetAttribute("x", Attributes.Position.X)
            End Get
            Set(value As Double)
                Element.SetAttribute("x", value)
            End Set
        End Property

        Public Property Y As Double
            Get
                Return Element.GetAttribute("y", Attributes.Position.Y)
            End Get
            Set(value As Double)
                Element.SetAttribute("y", value)
            End Set
        End Property

        Public Property Width As Double
            Get
                Return Element.GetAttribute("width", Attributes.Size.Width)
            End Get
            Set(value As Double)
                Element.SetAttribute("width", value)
            End Set
        End Property

        Public Property Height As Double
            Get
                Return Element.GetAttribute("height", Attributes.Size.Height)
            End Get
            Set(value As Double)
                Element.SetAttribute("height", value)
            End Set
        End Property

        ''' <summary>
        ''' the image data, usually be a base64 encoded uri string value.
        ''' </summary>
        ''' <returns></returns>
        Public Property HRef As String
            Get
                Return Element.GetAttribute(Of String)("xlink:href", "")
            End Get
            Set(value As String)
                Element.SetAttribute("xlink:href", value)
            End Set
        End Property

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetImage(img As Image)
            HRef = New DataURI(img).ToString
        End Sub

        Public Sub SetRectangle(rect As RectangleF)
            Width = rect.Width
            Height = rect.Height
            X = rect.Left
            Y = rect.Top
        End Sub

        ''' <summary>
        ''' create a new empty image node
        ''' </summary>
        ''' <param name="parent"></param>
        ''' <returns></returns>
        Friend Overloads Shared Function Create(parent As XmlElement) As SvgImage
            Dim element = parent.OwnerDocument.CreateElement("image")
            parent.AppendChild(element)
            Return New SvgImage(element)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Function Create(parent As XmlElement, image As Bitmap, Optional size As SizeF = Nothing) As SvgImage
            Return Create(parent, CType(image, Image), size)
        End Function

        Public Overloads Shared Function Create(parent As XmlElement, image As Image, Optional size As SizeF = Nothing) As SvgImage
            Dim element = parent.OwnerDocument.CreateElement("image")
            parent.AppendChild(element)
            Dim img As New SvgImage(element)
            img.HRef = New DataURI(image).ToString
            With size Or image.Size.SizeF.AsDefault(Function() size.IsEmpty)
                img.Width = .Width
                img.Height = .Height
            End With
            Return img
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetGDIObject() As Bitmap
            Return Base64Codec.GetImage(DataURI.URIParser(HRef).base64)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SaveAs(fileName$, Optional format As ImageFormats = ImageFormats.Png) As Boolean
            Return GetGDIObject.SaveAs(fileName, format)
        End Function
    End Class
End Namespace
