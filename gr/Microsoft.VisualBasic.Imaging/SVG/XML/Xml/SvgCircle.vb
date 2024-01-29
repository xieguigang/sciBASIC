Imports System.Xml

Namespace SvgLib
    Public NotInheritable Class SvgCircle
        Inherits SvgBasicShape
        Private Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Shared Function Create(parent As XmlElement) As SvgCircle
            Dim element = parent.OwnerDocument.CreateElement("circle")
            parent.AppendChild(element)
            Return New SvgCircle(element)
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

        Public Property R As Double
            Get
                Return Element.GetAttribute("r", Attributes.Radius.R)
            End Get
            Set(value As Double)
                Element.SetAttribute("r", value)
            End Set
        End Property
    End Class
End Namespace
