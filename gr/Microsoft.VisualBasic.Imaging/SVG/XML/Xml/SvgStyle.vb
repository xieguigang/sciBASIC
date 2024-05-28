Imports System.Xml

Namespace SVG.XML

    Public Class SvgStyle : Inherits SvgElement

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgStyle
            Dim element = parent.OwnerDocument.CreateElement("style")
            parent.AppendChild(element)
            Return New SvgStyle(element)
        End Function
    End Class
End Namespace