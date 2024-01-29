Imports System.Xml
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' The &lt;circle> SVG element is an SVG basic shape, used to draw circles based on a center point and a radius.
    ''' </summary>
    Public NotInheritable Class SvgCircle : Inherits SvgBasicShape

        ''' <summary>
        ''' The x-axis coordinate of the center of the circle. Value type: &lt;length>|&lt;percentage>; 
        ''' Default value: 0; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property CX As Double
            Get
                Return Element.GetAttribute("cx", Attributes.Position.CX)
            End Get
            Set(value As Double)
                Element.SetAttribute("cx", value)
            End Set
        End Property

        ''' <summary>
        ''' The y-axis coordinate of the center of the circle. Value type: &lt;length>|&lt;percentage>; 
        ''' Default value: 0; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property CY As Double
            Get
                Return Element.GetAttribute("cy", Attributes.Position.CY)
            End Get
            Set(value As Double)
                Element.SetAttribute("cy", value)
            End Set
        End Property

        ''' <summary>
        ''' The radius of the circle. A value lower or equal to zero disables rendering of the circle.
        ''' Value type: &lt;length>|&lt;percentage> ; Default value: 0; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property R As Double
            Get
                Return Element.GetAttribute("r", Attributes.Radius.R)
            End Get
            Set(value As Double)
                Element.SetAttribute("r", value)
            End Set
        End Property

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgCircle
            Dim element = parent.OwnerDocument.CreateElement("circle")
            parent.AppendChild(element)
            Return New SvgCircle(element)
        End Function
    End Class
End Namespace
