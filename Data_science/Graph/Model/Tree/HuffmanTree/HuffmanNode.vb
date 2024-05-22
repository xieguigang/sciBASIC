#Region "Microsoft.VisualBasic::c14e7ef646b46fec3307703f59e561fd, Data_science\Graph\Model\Tree\HuffmanTree\HuffmanNode.vb"

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

    '   Total Lines: 16
    '    Code Lines: 8 (50.00%)
    ' Comment Lines: 4 (25.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (25.00%)
    '     File Size: 424 B


    '     Interface HuffmanNode
    ' 
    '         Properties: code, frequency, parent
    ' 
    '         Function: merge
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace HuffmanTree

    ''' <summary>
    ''' Created by fangy on 13-12-17.
    ''' 哈夫曼树结点接口
    ''' </summary>
    Public Interface HuffmanNode : Inherits IComparable(Of HuffmanNode)

        Property code As Integer
        Property frequency As Integer
        Property parent As HuffmanNode

        Function merge(sibling As HuffmanNode) As HuffmanNode

    End Interface
End Namespace
