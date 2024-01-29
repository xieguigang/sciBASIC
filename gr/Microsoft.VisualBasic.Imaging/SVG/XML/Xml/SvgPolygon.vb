Imports System
Imports System.Globalization
Imports System.Linq
Imports System.Xml

Namespace SvgLib
    Public NotInheritable Class SvgPolygon
        Inherits SvgElement
        Private Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Shared Function Create(parent As XmlElement) As SvgPolygon
            Dim element = parent.OwnerDocument.CreateElement("polygon")
            parent.AppendChild(element)
            Return New SvgPolygon(element)
        End Function

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
    End Class
End Namespace
