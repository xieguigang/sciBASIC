
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml

''' <summary>
''' 1. ``nodename`` 选取此节点的所有子节点。
''' 2. ``/``        从根节点选取。
''' 3. ``//``       从匹配选择的当前节点选择文档中的节点，而不考虑它们的位置。
''' 4. ``.``        选取当前节点。
''' 5. ``..``       选取当前节点的父节点。
''' 6. ``@``        选取属性。
''' </summary>
Public MustInherit Class XPath

    Public Property expression As String
    Public Property selectNext As XPath

    Public MustOverride Function Query(document As IXmlDocumentTree) As IXmlNode()

    Public Shared Function Parse(expression As String) As XPath
        Return XPathParser.Parse(expression)
    End Function

End Class

''' <summary>
''' 选取此节点的所有子节点。
''' </summary>
Public Class SelectByNodeName : Inherits XPath

    Public Overrides Function ToString() As String
        Return $"{expression}{selectNext}"
    End Function

    Public Overrides Function Query(document As IXmlDocumentTree) As IXmlNode()
        Return document _
            .GetAllChildsByNodeName(expression) _
            .Select(Function(n) n.GetAllChilds) _
            .IteratesALL _
            .ToArray
    End Function
End Class

''' <summary>
''' 从根节点选取。
''' </summary>
Public Class RootPathSelector : Inherits XPath

    Public Overrides Function ToString() As String
        Return $"/{expression}{selectNext}"
    End Function

    Public Overrides Function Query(document As IXmlDocumentTree) As IXmlNode()
        Throw New NotImplementedException()
    End Function
End Class

''' <summary>
''' 从匹配选择的当前节点选择文档中的节点，而不考虑它们的位置。
''' </summary>
Public Class CurrentNodes : Inherits XPath

    Public Overrides Function ToString() As String
        Return $"//{expression}{selectNext}"
    End Function

    Public Overrides Function Query(document As IXmlDocumentTree) As IXmlNode()
        Return document.GetAllChildsByNodeName(expression)
    End Function
End Class

''' <summary>
''' 选取当前节点。
''' </summary>
Public Class CurrentNode : Inherits XPath

    Public Overrides Function ToString() As String
        Return $".{expression}{selectNext}"
    End Function

    Public Overrides Function Query(document As IXmlDocumentTree) As IXmlNode()
        Throw New NotImplementedException()
    End Function
End Class

''' <summary>
''' 选取当前节点的父节点。
''' </summary>
Public Class ParentNode : Inherits XPath

    Public Overrides Function ToString() As String
        Return $"..{expression}{selectNext}"
    End Function

    Public Overrides Function Query(document As IXmlDocumentTree) As IXmlNode()
        Throw New NotImplementedException()
    End Function
End Class

''' <summary>
''' 选取属性。
''' </summary>
Public Class SelectAttributes : Inherits XPath

    Public Overrides Function ToString() As String
        Return $"@{expression}{selectNext}"
    End Function

    Public Overrides Function Query(document As IXmlDocumentTree) As IXmlNode()
        Throw New NotImplementedException()
    End Function
End Class