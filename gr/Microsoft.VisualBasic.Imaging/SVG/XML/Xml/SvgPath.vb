Imports System.Xml

Namespace SvgLib
    Public NotInheritable Class SvgPath
        Inherits SvgElement
        Private Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Shared Function Create(parent As XmlElement) As SvgPath
            Dim element = parent.OwnerDocument.CreateElement("path")
            parent.AppendChild(element)
            Return New SvgPath(element)
        End Function

        Public Property D As String
            Get
                Return Element.GetAttribute("d")
            End Get
            Set(value As String)
                Element.SetAttribute("d", value)
            End Set
        End Property

        Public Property Length As Double
            Get
                Return Element.GetAttribute("pathLength", 0.0)
            End Get
            Set(value As Double)
                Element.SetAttribute("pathLength", value)
            End Set
        End Property

        Public Property FillRule As SvgFillRule
            Get
                Return Element.GetAttribute(Of SvgFillRule)("fill-rule", Attributes.FillAndStroke.FillRule)
            End Get
            Set(value As SvgFillRule)
                Element.SetAttribute("fill-rule", value)
            End Set
        End Property
    End Class
End Namespace
