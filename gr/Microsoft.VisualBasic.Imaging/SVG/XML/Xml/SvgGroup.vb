Imports System.Xml

Namespace SVG.XML
    Public NotInheritable Class SvgGroup
        Inherits SvgContainer
        Private Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Shared Function Create(parent As XmlElement) As SvgGroup
            Dim element = parent.OwnerDocument.CreateElement("g")
            parent.AppendChild(element)
            Return New SvgGroup(element)
        End Function
    End Class
End Namespace
