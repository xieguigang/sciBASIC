#Region "Microsoft.VisualBasic::6ec17e2391f6dc39637597adf6f958ee, mime\application%xml\XPath\XPathQuery.vb"

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

    '   Total Lines: 18
    '    Code Lines: 13
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 452 B


    ' Class XPathQuery
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: QueryAll, QuerySingle
    ' 
    ' /********************************************************************************/

#End Region

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
