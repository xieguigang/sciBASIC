Imports System.Xml
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' An &lt;ellipse> is a more general form of the &lt;circle> element, where you 
    ''' can scale the x and y radius (commonly referred to as the semimajor and 
    ''' semiminor axes in maths) of the circle separately.
    ''' </summary>
    Public NotInheritable Class SvgEllipse
        Inherits SvgBasicShape

        ''' <summary>
        ''' The x position of the center of the ellipse.
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
        ''' The y position of the center of the ellipse.
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
        ''' The x radius of the ellipse.
        ''' </summary>
        ''' <returns></returns>
        Public Property RX As Double
            Get
                Return Element.GetAttribute("rx", Attributes.Radius.RX)
            End Get
            Set(value As Double)
                Element.SetAttribute("rx", value)
            End Set
        End Property

        ''' <summary>
        ''' The y radius of the ellipse.
        ''' </summary>
        ''' <returns></returns>
        Public Property RY As Double
            Get
                Return Element.GetAttribute("ry", Attributes.Radius.RY)
            End Get
            Set(value As Double)
                Element.SetAttribute("ry", value)
            End Set
        End Property

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgEllipse
            Dim element = parent.OwnerDocument.CreateElement("ellipse")
            parent.AppendChild(element)
            Return New SvgEllipse(element)
        End Function
    End Class
End Namespace
