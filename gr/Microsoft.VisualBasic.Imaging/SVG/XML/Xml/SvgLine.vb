Imports System.Xml
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML
    Public NotInheritable Class SvgLine
        Inherits SvgBasicShape
        Private Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Shared Function Create(parent As XmlElement) As SvgLine
            Dim element = parent.OwnerDocument.CreateElement("line")
            parent.AppendChild(element)
            Return New SvgLine(element)
        End Function

        Public Property X1 As Double
            Get
                Return Element.GetAttribute("x1", Attributes.Position.X)
            End Get
            Set(value As Double)
                Element.SetAttribute("x1", value)
            End Set
        End Property

        Public Property Y1 As Double
            Get
                Return Element.GetAttribute("y1", Attributes.Position.Y)
            End Get
            Set(value As Double)
                Element.SetAttribute("y1", value)
            End Set
        End Property

        Public Property X2 As Double
            Get
                Return Element.GetAttribute("x2", Attributes.Position.X)
            End Get
            Set(value As Double)
                Element.SetAttribute("x2", value)
            End Set
        End Property

        Public Property Y2 As Double
            Get
                Return Element.GetAttribute("y2", Attributes.Position.Y)
            End Get
            Set(value As Double)
                Element.SetAttribute("y2", value)
            End Set
        End Property
    End Class
End Namespace
