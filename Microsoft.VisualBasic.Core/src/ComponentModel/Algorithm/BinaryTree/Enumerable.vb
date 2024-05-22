#Region "Microsoft.VisualBasic::63efba3466a045cf8176d20d774ac203, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\BinaryTree\Enumerable.vb"

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

    '   Total Lines: 86
    '    Code Lines: 45 (52.33%)
    ' Comment Lines: 31 (36.05%)
    '    - Xml Docs: 96.77%
    ' 
    '   Blank Lines: 10 (11.63%)
    '     File Size: 3.15 KB


    '     Module Enumerable
    ' 
    '         Function: GetNodeCounts, PopulateNodes, PopulateSequence, (+2 Overloads) Values
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.Algorithm.BinaryTree

    Public Module Enumerable

        <Extension>
        Public Function GetNodeCounts(Of K, V)(tree As BinaryTree(Of K, V)) As Integer
            Dim n As Integer = 1

            If tree.Left IsNot Nothing Then
                n += tree.Left.GetNodeCounts
            End If
            If tree.Right IsNot Nothing Then
                n += tree.Right.GetNodeCounts
            End If

            Return n
        End Function

        ''' <summary>
        ''' Populate an ASC sortted sequence from this binary tree 
        ''' 
        ''' ```
        ''' left -> me -> right
        ''' ```
        ''' </summary>
        ''' <typeparam name="K"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="tree"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这个函数是直接将键名和对应的值取出来，如果是需要取出聚类的簇成员，
        ''' 应该使用<see cref="PopulateNodes(Of K, V)"/>方法
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function PopulateSequence(Of K, V)(tree As BinaryTree(Of K, V)) As IEnumerable(Of Map(Of K, V))
            Return tree.PopulateNodes.Select(Function(n) New Map(Of K, V)(n.Key, n.Value))
        End Function

        ''' <summary>
        ''' 将一个给定的二叉树对象转换为一个数组序列
        ''' </summary>
        ''' <typeparam name="K"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="tree"></param>
        ''' <returns>
        ''' current -> right -> left
        ''' </returns>
        <Extension>
        Public Iterator Function PopulateNodes(Of K, V)(tree As BinaryTree(Of K, V)) As IEnumerable(Of BinaryTree(Of K, V))
            Yield tree

            If Not tree.Right Is Nothing Then
                For Each node In tree.Right.PopulateNodes
                    Yield node
                Next
            End If
            If Not tree.Left Is Nothing Then
                For Each node In tree.Left.PopulateNodes
                    Yield node
                Next
            End If
        End Function

        ''' <summary>
        ''' Get all value property from the childs in this given node
        ''' </summary>
        ''' <typeparam name="K"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="tree"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Values(Of K, V)(tree As BinaryTree(Of K, V)) As IEnumerable(Of V)
            Return tree.PopulateNodes.Values
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Values(Of K, V)(nodes As IEnumerable(Of BinaryTree(Of K, V))) As IEnumerable(Of V)
            Return nodes.Select(Function(n) n.Value)
        End Function
    End Module
End Namespace
