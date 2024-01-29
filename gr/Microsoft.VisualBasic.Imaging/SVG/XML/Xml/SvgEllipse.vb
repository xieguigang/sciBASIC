Imports System.Xml

Namespace SvgLib
    Public NotInheritable Class SvgEllipse
        Inherits SvgBasicShape
        Private Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Shared Function Create(parent As XmlElement) As SvgEllipse
            Dim element = parent.OwnerDocument.CreateElement("ellipse")
            parent.AppendChild(element)
            Return New SvgEllipse(element)
        End Function

        Public Property CX As Double
            Get
                Return Element.GetAttribute("cx", Attributes.Position.CX)
            End Get
            Set(value As Double)
                Element.SetAttribute("cx", value)
            End Set
        End Property

        Public Property CY As Double
            Get
                Return Element.GetAttribute("cy", Attributes.Position.CY)
            End Get
            Set(value As Double)
                Element.SetAttribute("cy", value)
            End Set
        End Property

        Public Property RX As Double
            Get
                Return Element.GetAttribute("rx", Attributes.Radius.RX)
            End Get
            Set(value As Double)
                Element.SetAttribute("rx", value)
            End Set
        End Property

        Public Property RY As Double
            Get
                Return Element.GetAttribute("ry", Attributes.Radius.RY)
            End Get
            Set(value As Double)
                Element.SetAttribute("ry", value)
            End Set
        End Property
    End Class
End Namespace
