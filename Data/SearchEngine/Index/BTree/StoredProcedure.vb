#Region "Microsoft.VisualBasic::39203e6124720880104bb948c51f19dc, sciBASIC#\Data\SearchEngine\Index\BTree\StoredProcedure.vb"

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

    '   Total Lines: 89
    '    Code Lines: 75
    ' Comment Lines: 0
    '   Blank Lines: 14
    '     File Size: 3.14 KB


    ' Module StoredProcedure
    ' 
    '     Function: AppendTree, BinaryTree, ToIndex
    ' 
    '     Sub: indexInternal
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Language

Public Module StoredProcedure

    <Extension>
    Public Function BinaryTree(Of K, V)(repo As Repository(Of K, V)) As BinaryTree(Of K, V)
        With repo.GetRootNode
            Return New BinaryTree(Of K, V)(.Key, .Value).AppendTree(.ByRef, repo.Index)
        End With
    End Function

    <Extension>
    Private Function AppendTree(Of K, V)(tree As BinaryTree(Of K, V), root As BinaryTreeIndex(Of K, V), repo As BinaryTreeIndex(Of K, V)()) As BinaryTree(Of K, V)
        With tree
            Call .SetValue("values", root.Additionals)

            If root.Left > -1 Then
                Dim left = repo(root.Left)
                Dim leftNode As New BinaryTree(Of K, V)(left.Key, left.Value)

                .Left = leftNode.AppendTree(left, repo)
            End If
            If root.Right > -1 Then
                Dim right = repo(root.Right)
                Dim rightNode As New BinaryTree(Of K, V)(right.Key, right.Value)

                .Right = rightNode.AppendTree(right, repo)
            End If
        End With

        Return tree
    End Function

    <Extension>
    Public Function ToIndex(Of K, V)(tree As BinaryTree(Of K, V)) As Repository(Of K, V)
        Dim root As New BinaryTreeIndex(Of K, V) With {
            .Key = tree.Key,
            .My = Scan0,
            .Value = tree.Value,
            .Additionals = DirectCast(tree!values, IEnumerable(Of V)).ToArray
        }

        With New List(Of BinaryTreeIndex(Of K, V))
            Call .Add(root)
            Call .indexInternal(root, tree)

            Return New Repository(Of K, V)(.ToArray) With {
                .Root = root.My
            }
        End With
    End Function

    <Extension>
    Private Sub indexInternal(Of K, V)(stack As List(Of BinaryTreeIndex(Of K, V)), root As BinaryTreeIndex(Of K, V), tree As BinaryTree(Of K, V))
        If Not tree.Left Is Nothing Then
            Dim leftNode = tree.Left
            Dim left As New BinaryTreeIndex(Of K, V) With {
                .Key = leftNode.Key,
                .Value = leftNode.Value,
                .Additionals = DirectCast(leftNode!values, IEnumerable(Of V)).ToArray,
                .My = stack.Count
            }

            stack.Add(left)
            root.Left = left.My
            stack.indexInternal(left, leftNode)
        Else
            root.Left = -1
        End If

        If Not tree.Right Is Nothing Then
            Dim rightNode = tree.Right
            Dim right As New BinaryTreeIndex(Of K, V) With {
                .Key = rightNode.Key,
                .Value = rightNode.Value,
                .Additionals = DirectCast(rightNode!values, IEnumerable(Of V)).ToArray,
                .My = stack.Count
            }

            stack.Add(right)
            root.Right = right.My
            stack.indexInternal(right, rightNode)
        Else
            root.Right = -1
        End If
    End Sub
End Module
