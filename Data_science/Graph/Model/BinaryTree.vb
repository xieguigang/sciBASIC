#Region "Microsoft.VisualBasic::71c9b0bcb1c3cd08fc1a68839ef1d599, ..\sciBASIC#\Data_science\Graph\Model\BinaryTree.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Public Class BinaryTree(Of T As IComparable(Of T)) : Inherits Tree(Of T)

#Region "Childs"
    Public Property Left As BinaryTree(Of T)
        Get
            Return Childs.ElementAtOrDefault(0)
        End Get
        Set
            If Childs.IsNullOrEmpty Then
                Childs = New List(Of Tree(Of T)) From {Value, Nothing}
            Else
                Childs(0) = Value
            End If
        End Set
    End Property

    Public Property Right As BinaryTree(Of T)
        Get
            Return Childs.ElementAtOrDefault(1)
        End Get
        Set
            If Childs.IsNullOrEmpty OrElse Childs.Count < 2 Then
                Childs = New List(Of Tree(Of T)) From {Nothing, Value}
            Else
                Childs(1) = Value
            End If
        End Set
    End Property
#End Region

    Const Duplicated$ = "Duplicated node was found!"

    Sub New()
    End Sub

    Sub New(name$)
        Label = name
    End Sub

    Public Shared Function ROOT() As BinaryTree(Of T)
        Return New BinaryTree(Of T)("<ROOT>")
    End Function

    ''' <summary>
    ''' Recursively locates an empty slot in the binary tree and inserts the node
    ''' </summary>
    ''' <param name="node"></param>
    ''' <param name="tree"></param>
    ''' <param name="[overrides]">
    ''' 0不复写，函数自动处理
    ''' &lt;0  LEFT
    ''' >0 RIGHT
    ''' </param>
    Private Sub InternalAdd(node As BinaryTree(Of T), ByRef tree As BinaryTree(Of T), Optional overrides% = 0)
        If tree Is Nothing Then
            tree = node
        Else
            ' If we find a node with the same name then it's 
            ' a duplicate and we can't continue
            Dim comparison%

            If [overrides] = 0 Then
                comparison = node.Data.CompareTo(tree.Data)

                If comparison = 0 Then
                    Throw New Exception(Duplicated)
                End If
            Else
                comparison = [overrides]
            End If

            ' 在这里进行递归的比较查找，直到二叉树节点是一个空节点为止
            If comparison < 0 Then
                InternalAdd(node, tree.Left)
                tree.Left.Parent = tree
            Else
                InternalAdd(node, tree.Right)
                tree.Right.Parent = tree
            End If
        End If
    End Sub

    ''' <summary>
    ''' Add a symbol to the tree if it's a new one. Returns reference to the new
    ''' node if a new node inserted, else returns null to indicate node already present.
    ''' </summary>
    ''' <param name="name">Name of node to add to tree</param>
    ''' <param name="data">Value of node</param>
    ''' <returns> Returns reference to the new node is the node was inserted.
    ''' If a duplicate node (same name was located then returns null</returns>
    Public Function Insert(name$, data As T) As BinaryTree(Of T)
        Dim node As New BinaryTree(Of T) With {
            .Label = name,
            .Data = data,
            .ID = Count
        }

        Try
            Call InternalAdd(node, Me)
            Return node
        Catch e As Exception
            Dim ex As New Exception(node.ToString, e)
            Return App.LogException(ex)
        End Try
    End Function
End Class
