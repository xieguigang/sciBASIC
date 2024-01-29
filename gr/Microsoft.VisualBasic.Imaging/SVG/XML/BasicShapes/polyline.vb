Imports System.Globalization
Imports System.Xml

Namespace SVG.XML

    ''' <summary>
    ''' A &lt;polyline> is a group of connected straight lines. Since the 
    ''' list of points can get quite long, all the points are included in 
    ''' one attribute
    ''' </summary>
    Public NotInheritable Class SvgPolyLine : Inherits SvgElement

        ''' <summary>
        ''' A list of points. Each number must be separated by a space, comma, EOL,
        ''' or a line feed character with additional whitespace permitted. Each point 
        ''' must contain two numbers: an x coordinate and a y coordinate. So, the 
        ''' list (0,0), (1,1), and (2,2) could be written as 0, 0 1, 1 2, 2.
        ''' </summary>
        ''' <returns></returns>
        Public Property Points As Double()
            Get
                Dim stringArray = Element.GetAttribute("points")
                Return stringArray.Split(New String() {", "}, StringSplitOptions.RemoveEmptyEntries).Select(Function(value) Double.Parse(value, CultureInfo.InvariantCulture)).ToArray()
            End Get
            Set(value As Double())
                Dim lPoints = String.Join(", ", value.Select(Function(x) x.ToString("G", CultureInfo.InvariantCulture)))
                Element.SetAttribute("points", lPoints)
            End Set
        End Property

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgPolyLine
            Dim element = parent.OwnerDocument.CreateElement("polyline")
            parent.AppendChild(element)
            Return New SvgPolyLine(element)
        End Function
    End Class
End Namespace
