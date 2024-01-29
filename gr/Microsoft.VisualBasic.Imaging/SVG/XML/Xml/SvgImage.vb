Imports System.Xml
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML
    Public NotInheritable Class SvgImage
        Inherits SvgBasicShape
        Private Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Shared Function Create(parent As XmlElement) As SvgImage
            Dim element = parent.OwnerDocument.CreateElement("image")
            parent.AppendChild(element)
            Return New SvgImage(element)
        End Function

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
    End Class
End Namespace
