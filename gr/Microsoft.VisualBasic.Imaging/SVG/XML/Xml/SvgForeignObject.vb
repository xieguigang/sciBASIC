Imports System.Xml

Namespace SVG.XML

    Public Class SvgForeignObject : Inherits SvgElement

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgForeignObject
            Dim element = parent.OwnerDocument.CreateElement("foreignObject")
            parent.AppendChild(element)
            Return New SvgForeignObject(element)
        End Function
    End Class
End Namespace