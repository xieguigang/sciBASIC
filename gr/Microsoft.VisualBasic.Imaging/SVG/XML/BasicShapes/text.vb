Imports System.Xml
Imports Microsoft.VisualBasic.Imaging.SVG.XML.Enums
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' The SVG &lt;text> element draws a graphics element consisting of text. It's possible 
    ''' to apply a gradient, pattern, clipping path, mask, or filter to &lt;text>, like any 
    ''' other SVG graphics element.
    '''
    ''' If text Is included In SVG Not inside Of a &lt;text> element, it Is Not rendered. 
    ''' This Is different than being hidden by Default, As setting the display Property won't 
    ''' show the text.
    ''' </summary>
    Public NotInheritable Class SvgText
        Inherits SvgElement

        ''' <summary>
        ''' The x coordinate of the starting point of the text baseline. Value type: &lt;length>|&lt;percentage> ; Default value: 0; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property X As Double
            Get
                Return Element.GetAttribute("x", Attributes.Position.X)
            End Get
            Set(value As Double)
                Element.SetAttribute("x", value)
            End Set
        End Property

        ''' <summary>
        ''' The y coordinate of the starting point of the text baseline. Value type: &lt;length>|&lt;percentage> ; Default value: 0; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property Y As Double
            Get
                Return Element.GetAttribute("y", Attributes.Position.Y)
            End Get
            Set(value As Double)
                Element.SetAttribute("y", value)
            End Set
        End Property

        ''' <summary>
        ''' Shifts the text position horizontally from a previous text element. Value type: &lt;length>|&lt;percentage> ; Default value: none; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property DX As Double
            Get
                Return Element.GetAttribute("dx", Attributes.Position.DX)
            End Get
            Set(value As Double)
                Element.SetAttribute("dx", value)
            End Set
        End Property

        ''' <summary>
        ''' Shifts the text position vertically from a previous text element. Value type: &lt;length>|&lt;percentage> ; Default value: none; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
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

        ''' <summary>
        ''' A width that the text should be scaled to fit. Value type: &lt;length>|&lt;percentage> ; Default value: none; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
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

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Public Function SetPosition(x As Double, y As Double) As SvgText
            Me.X = x
            Me.Y = y
            Return Me
        End Function

        Public Function SetText(text As String) As SvgText
            Me.Text = text
            Return Me
        End Function

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgText
            Dim element = parent.OwnerDocument.CreateElement("text")
            parent.AppendChild(element)
            Return New SvgText(element)
        End Function
    End Class
End Namespace
