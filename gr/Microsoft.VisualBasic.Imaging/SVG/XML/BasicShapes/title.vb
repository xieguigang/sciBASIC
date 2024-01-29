Imports System.Xml

Namespace SVG.XML

    Public Class SvgTitle : Inherits SvgElement

        Public Property Text As String
            Get
                Return Element.InnerText
            End Get
            Set(value As String)
                Element.InnerText = value
            End Set
        End Property

        Private Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Shared Function Create(parent As XmlElement) As SvgTitle
            Dim element = parent.OwnerDocument.CreateElement("title")
            parent.AppendChild(element)
            Return New SvgTitle(element)
        End Function
    End Class
End Namespace