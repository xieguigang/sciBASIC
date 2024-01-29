Imports System.Xml
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' The &lt;marker> element defines a graphic used for drawing arrowheads or
    ''' polymarkers on a given &lt;path>, &lt;line>, &lt;polyline> or &lt;polygon>
    ''' element.
    '''
    ''' Markers can be attached To shapes Using the marker-start, marker-mid, And 
    ''' marker-End properties.
    ''' </summary>
    Public NotInheritable Class SvgMarker : Inherits SvgContainer

        ''' <summary>
        ''' This attribute defines the x coordinate for the reference point of the marker. 
        ''' Value type: left|center|right|&lt;coordinate> ; Default value: 0; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property RefX As Double
            Get
                Return Element.GetAttribute("refX", 0.0)
            End Get
            Set(value As Double)
                Element.SetAttribute("refX", value)
            End Set
        End Property

        ''' <summary>
        ''' This attribute defines the y coordinate for the reference point of the marker. 
        ''' Value type: top|center|bottom|&lt;coordinate> ; Default value: 0; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property RefY As Double
            Get
                Return Element.GetAttribute("refY", 0.0)
            End Get
            Set(value As Double)
                Element.SetAttribute("refY", value)
            End Set
        End Property

        ''' <summary>
        ''' This attribute defines the width of the marker viewport. Value type: &lt;length> ; 
        ''' Default value: 3; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property Width As Double
            Get
                Return Element.GetAttribute("markerWidth", 0.0)
            End Get
            Set(value As Double)
                Element.SetAttribute("markerWidth", value)
            End Set
        End Property

        ''' <summary>
        ''' This attribute defines the height of the marker viewport. Value type: &lt;length> ; 
        ''' Default value: 3; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property Height As Double
            Get
                Return Element.GetAttribute("markerHeight", 0.0)
            End Get
            Set(value As Double)
                Element.SetAttribute("markerHeight", value)
            End Set
        End Property

        ''' <summary>
        ''' This attribute defines the bound of the SVG viewport for the current SVG fragment. 
        ''' Value type: &lt;list-of-numbers> ; Default value: none; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property ViewBox As SvgViewBox
            Get
                Return Element.GetAttribute("viewBox", New SvgViewBox())
            End Get
            Set(value As SvgViewBox)
                Element.SetAttribute("viewBox", value.ToString())
            End Set
        End Property

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgMarker
            Dim element = parent.OwnerDocument.CreateElement("marker")
            parent.AppendChild(element)
            Return New SvgMarker(element)
        End Function
    End Class
End Namespace
