#Region "Microsoft.VisualBasic::ccc7b98d4153d413b2fef299f5e98d9c, mime\application%xml\XPath\XPath.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 129
    '    Code Lines: 80 (62.02%)
    ' Comment Lines: 26 (20.16%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 23 (17.83%)
    '     File Size: 3.99 KB


    ' Class XPath
    ' 
    '     Properties: expression, selectNext
    ' 
    '     Function: Parse
    ' 
    ' Class SelectByNodeName
    ' 
    '     Function: Query, ToString
    ' 
    ' Class RootPathSelector
    ' 
    '     Function: Query, ToString
    ' 
    ' Class CurrentNodes
    ' 
    '     Function: Query, ToString
    ' 
    ' Class CurrentNode
    ' 
    '     Function: Query, ToString
    ' 
    ' Class ParentNode
    ' 
    '     Function: Query, ToString
    ' 
    ' Class SelectAttributes
    ' 
    '     Function: Query, ToString
    ' 
    ' /********************************************************************************/

#End Region

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
        If selectNext Is Nothing Then
            Return document _
                .GetAllChilds _
                .Where(Function(node)
                           Return node.nodeName.TextEquals(expression)
                       End Function) _
                .ToArray
        Else
            Return document _
                .GetAllChilds _
                .Where(Function(node)
                           Return node.nodeName.TextEquals(expression)
                       End Function) _
                .Select(Function(root)
                            Return selectNext.Query(root)
                        End Function) _
                .IteratesALL _
                .ToArray
        End If
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
