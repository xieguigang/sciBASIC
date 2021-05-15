
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

    Public Shared Function Parse(expression As String) As XPath
        Return XPathParser.Parse(expression)
    End Function

End Class

Public Class SelectByNodeName : Inherits XPath

End Class

Public Class RootPathSelector : Inherits XPath

End Class

Public Class CurrentNodes : Inherits XPath

End Class

Public Class CurrentNode : Inherits XPath

End Class

Public Class ParentNode : Inherits XPath


End Class

Public Class SelectAttributes : Inherits XPath


End Class