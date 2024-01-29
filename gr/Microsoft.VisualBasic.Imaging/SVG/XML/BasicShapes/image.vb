Imports System.Xml
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

        Public Property HRef As String
            Get
                Return Element.GetAttribute(Of String)("xlink:href", "")
            End Get
            Set(value As String)
                Element.SetAttribute("xlink:href", value)
            End Set
        End Property

        Private Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Shared Function Create(parent As XmlElement) As SvgImage
            Dim element = parent.OwnerDocument.CreateElement("image")
            parent.AppendChild(element)
            Return New SvgImage(element)
        End Function
    End Class
End Namespace
