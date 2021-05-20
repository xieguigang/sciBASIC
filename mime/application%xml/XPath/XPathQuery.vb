Imports Microsoft.VisualBasic.Text.Xml

Public Class XPathQuery

    Dim xpath As XPath

    Sub New(xpath As XPath)
        Me.xpath = xpath
    End Sub

    Public Function QuerySingle(document As IXmlDocumentTree) As IXmlNode
        Return xpath.Query(document).FirstOrDefault
    End Function

    Public Function QueryAll(document As IXmlDocumentTree) As IXmlNode()
        Return xpath.Query(document)
    End Function
End Class
