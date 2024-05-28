Imports System.Xml

Namespace SVG.XML

    ''' <summary>
    ''' the svg RDF metadata node
    ''' </summary>
    Public Class SvgMetadata : Inherits SvgElement

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgMetadata
            Dim element = parent.OwnerDocument.CreateElement("metadata")
            parent.AppendChild(element)
            Return New SvgMetadata(element)
        End Function
    End Class
End Namespace