Imports System.Xml

Namespace SVG.XML
    Public NotInheritable Class SvgMarker
        Inherits SvgContainer
        Private Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Shared Function Create(parent As XmlElement) As SvgMarker
            Dim element = parent.OwnerDocument.CreateElement("marker")
            parent.AppendChild(element)
            Return New SvgMarker(element)
        End Function

        Public Property RefX As Double
            Get
                Return Element.GetAttribute("refX", 0.0)
            End Get
            Set(value As Double)
                Element.SetAttribute("refX", value)
            End Set
        End Property

        Public Property RefY As Double
            Get
                Return Element.GetAttribute("refY", 0.0)
            End Get
            Set(value As Double)
                Element.SetAttribute("refY", value)
            End Set
        End Property

        Public Property Width As Double
            Get
                Return Element.GetAttribute("markerWidth", 0.0)
            End Get
            Set(value As Double)
                Element.SetAttribute("markerWidth", value)
            End Set
        End Property

        Public Property Height As Double
            Get
                Return Element.GetAttribute("markerHeight", 0.0)
            End Get
            Set(value As Double)
                Element.SetAttribute("markerHeight", value)
            End Set
        End Property

        Public Property ViewBox As SvgViewBox
            Get
                Return Element.GetAttribute("viewBox", New SvgViewBox())
            End Get
            Set(value As SvgViewBox)
                Element.SetAttribute("viewBox", value.ToString())
            End Set
        End Property
    End Class
End Namespace
