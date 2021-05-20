Namespace Text.Xml

    Public Interface IXmlDocumentTree : Inherits IXmlNode

        Function GetAllChilds() As IXmlNode()
        Function GetAllChildsByNodeName(nodename As String) As IXmlDocumentTree()

    End Interface

    ''' <summary>
    ''' includes tree node and text node
    ''' </summary>
    Public Interface IXmlNode

        Function GetInnerText() As String

    End Interface
End Namespace