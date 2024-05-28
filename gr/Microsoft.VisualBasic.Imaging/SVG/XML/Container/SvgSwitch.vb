Imports System.Xml

Namespace SVG.XML

    Public Class SvgSwitch : Inherits SvgContainer

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgSwitch
            Dim element = parent.OwnerDocument.CreateElement("switch")
            parent.AppendChild(element)
            Return New SvgSwitch(element)
        End Function

    End Class
End Namespace