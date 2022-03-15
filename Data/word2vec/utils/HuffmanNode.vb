#Region "Microsoft.VisualBasic::c73901ef8aad04987419a8b1648bdf77, sciBASIC#\Data\word2vec\utils\HuffmanNode.vb"

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

    '   Total Lines: 14
    '    Code Lines: 9
    ' Comment Lines: 4
    '   Blank Lines: 1
    '     File Size: 411.00 B


    '     Interface HuffmanNode
    ' 
    '         Properties: frequency, parent
    ' 
    '         Function: merge
    ' 
    '         Sub: SetCode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace utils
    ''' <summary>
    ''' Created by fangy on 13-12-17.
    ''' 哈夫曼树结点接口
    ''' </summary>
    Public Interface HuffmanNode
        Inherits IComparable(Of HuffmanNode)

        Sub SetCode(value As Integer)
        Property frequency As Integer
        Property parent As HuffmanNode
        Function merge(sibling As HuffmanNode) As HuffmanNode
    End Interface
End Namespace
