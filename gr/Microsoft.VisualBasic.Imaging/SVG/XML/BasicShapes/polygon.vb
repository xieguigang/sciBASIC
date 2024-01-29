Imports System.Globalization
Imports System.Xml
Imports Microsoft.VisualBasic.Imaging.SVG.XML.Enums
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' A &lt;polygon> is similar to a &lt;polyline>, in that it is composed of straight
    ''' line segments connecting a list of points. For polygons though, the path 
    ''' automatically connects the last point with the first, creating a closed shape.
    ''' </summary>
    Public NotInheritable Class SvgPolygon
        Inherits SvgElement

        ''' <summary>
        ''' A list of points, each number separated by a space, comma, EOL, or a line
        ''' feed character with additional whitespace permitted. Each point must contain 
        ''' two numbers: an x coordinate and a y coordinate. So, the list (0,0), (1,1), 
        ''' and (2,2) could be written as 0, 0 1, 1 2, 2. The drawing then closes the 
        ''' path, so a final straight line would be drawn from (2,2) to (0,0).
        ''' </summary>
        ''' <returns></returns>
        Public Property Points As Double()
            Get
                Dim stringArray = Element.GetAttribute("points")
                Return stringArray.Split(New String() {", "}, StringSplitOptions.RemoveEmptyEntries).[Select](Function(value) Double.Parse(value, CultureInfo.InvariantCulture)).ToArray()
            End Get
            Set(value As Double())
                Dim lPoints = String.Join(", ", value.Select(Function(x) x.ToString("G", CultureInfo.InvariantCulture)))
                Element.SetAttribute("points", lPoints)
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

        Private Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Shared Function Create(parent As XmlElement) As SvgPolygon
            Dim element = parent.OwnerDocument.CreateElement("polygon")
            parent.AppendChild(element)
            Return New SvgPolygon(element)
        End Function
    End Class
End Namespace
