Imports System.Xml

Namespace SVG.XML
    Public MustInherit Class SvgBasicShape
        Inherits SvgElement
        Protected Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub
    End Class
End Namespace
