Imports System.Xml

Namespace SvgLib
    Public NotInheritable Class SvgText
        Inherits SvgElement
        Private Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Shared Function Create(parent As XmlElement) As SvgText
            Dim element = parent.OwnerDocument.CreateElement("text")
            parent.AppendChild(element)
            Return New SvgText(element)
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

        Public Property DX As Double
            Get
                Return Element.GetAttribute("dx", Attributes.Position.DX)
            End Get
            Set(value As Double)
                Element.SetAttribute("dx", value)
            End Set
        End Property

        Public Property DY As Double
            Get
                Return Element.GetAttribute("dy", Attributes.Position.DY)
            End Get
            Set(value As Double)
                Element.SetAttribute("dy", value)
            End Set
        End Property

        ' rotate

        ' lengthAdjust

        Public Property TextLength As Double
            Get
                Return Element.GetAttribute("textLength", Attributes.Size.Width)
            End Get
            Set(value As Double)
                Element.GetAttribute("textLength", value)
            End Set
        End Property

        Public Property FontFamily As String
            Get
                Return GetStyle("font-family")
            End Get
            Set(value As String)
                SetStyle("font-family", value)
            End Set
        End Property

        Public Property FontSize As Double
            Get
                Return Element.GetAttribute("font-size", 0.0)
            End Get
            Set(value As Double)
                Element.SetAttribute("font-size", value)
            End Set
        End Property

        Public Property Text As String
            Get
                Return Element.InnerText
            End Get
            Set(value As String)
                Element.InnerText = value
            End Set
        End Property

        Public Property TextAnchor As SvgTextAnchor
            Get
                Return Element.GetAttribute(Of SvgTextAnchor)("text-anchor", Attributes.Text.TextAnchor)
            End Get
            Set(value As SvgTextAnchor)
                Element.SetAttribute("text-anchor", value)
            End Set
        End Property

        Public Property DominantBaseline As SvgDominantBaseline
            Get
                Return Element.GetAttribute(Of SvgDominantBaseline)("dominant-baseline", Attributes.Text.DominantBaseline)
            End Get
            Set(value As SvgDominantBaseline)
                Element.SetAttribute("dominant-baseline", value)
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
