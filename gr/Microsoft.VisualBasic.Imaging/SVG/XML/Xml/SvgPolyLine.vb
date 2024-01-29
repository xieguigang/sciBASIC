Imports System
Imports System.Globalization
Imports System.Linq
Imports System.Xml

Namespace SvgLib
    Public NotInheritable Class SvgPolyLine
        Inherits SvgElement
        Private Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Shared Function Create(parent As XmlElement) As SvgPolyLine
            Dim element = parent.OwnerDocument.CreateElement("polyline")
            parent.AppendChild(element)
            Return New SvgPolyLine(element)
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
    End Class
End Namespace
