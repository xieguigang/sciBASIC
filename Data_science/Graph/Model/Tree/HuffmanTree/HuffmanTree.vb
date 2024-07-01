#Region "Microsoft.VisualBasic::910aa7be3472f69bbb1e2db756c5c941, Data_science\Graph\Model\Tree\HuffmanTree\HuffmanTree.vb"

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

    '   Total Lines: 36
    '    Code Lines: 24 (66.67%)
    ' Comment Lines: 4 (11.11%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (22.22%)
    '     File Size: 1.05 KB


    '     Module HuffmanTreeTools
    ' 
    '         Function: getPath
    ' 
    '         Sub: make
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataStructures

Namespace HuffmanTree

    ''' <summary>
    ''' Created by fangy on 13-12-17.
    ''' 哈夫曼树
    ''' </summary>
    Public Module HuffmanTreeTools

        Public Sub make(Of T1 As HuffmanNode)(nodes As ICollection(Of T1))
            Dim tree As New SortedSet(Of HuffmanNode)(nodes)

            While tree.Count > 1
                Dim left As HuffmanNode = tree.PollFirst()
                Dim right As HuffmanNode = tree.PollFirst()
                Dim parent = left.merge(right)
                tree.Add(parent)
            End While
        End Sub

        Public Function getPath(leafNode As HuffmanNode) As IList(Of HuffmanNode)
            Dim nodes As New List(Of HuffmanNode)()
            Dim hn = leafNode

            While hn IsNot Nothing
                nodes.Add(hn)
                hn = hn.parent
            End While

            nodes.Reverse()

            Return nodes
        End Function
    End Module
End Namespace
