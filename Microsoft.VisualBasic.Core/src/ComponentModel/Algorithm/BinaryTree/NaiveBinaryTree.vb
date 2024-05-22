#Region "Microsoft.VisualBasic::464f8286a4a7e7470a1a695d3019fac4, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\BinaryTree\NaiveBinaryTree.vb"

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

    '   Total Lines: 359
    '    Code Lines: 178 (49.58%)
    ' Comment Lines: 140 (39.00%)
    '    - Xml Docs: 55.71%
    ' 
    '   Blank Lines: 41 (11.42%)
    '     File Size: 15.12 KB


    '     Class NaiveBinaryTree
    ' 
    '         Properties: Length
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: add, drawNode, findSuccessor, FindSymbol, insert
    '                   ToString
    ' 
    '         Sub: Clear, delete, KillTree
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

' Software License Agreement (BSD License)
'* 
'* Copyright (c) 2003, Herbert M Sauro
'* All rights reserved.
'*
'* Redistribution and use in source and binary forms, with or without
'* modification, are permitted provided that the following conditions are met:
'*     * Redistributions of source code must retain the above copyright
'*       notice, this list of conditions and the following disclaimer.
'*     * Redistributions in binary form must reproduce the above copyright
'*       notice, this list of conditions and the following disclaimer in the
'*       documentation and/or other materials provided with the distribution.
'*     * Neither the name of Herbert M Sauro nor the
'*       names of its contributors may be used to endorse or promote products
'*       derived from this software without specific prior written permission.
'*
'* THIS SOFTWARE IS PROVIDED BY <copyright holder> ``AS IS'' AND ANY
'* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
'* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
'* DISCLAIMED. IN NO EVENT SHALL <copyright holder> BE LIABLE FOR ANY
'* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
'* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
'* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
'* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
'* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
'* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
'

Namespace ComponentModel.Algorithm.BinaryTree

    ''' <summary>
    ''' The Binary tree itself. 朴素二叉树
    ''' 
    ''' A very basic Binary Search Tree. Not generalized, stores
    ''' name/value pairs in the tree nodes. name is the node key.
    ''' The advantage of a binary tree is its fast insert and lookup
    ''' characteristics. This version does not deal with tree balancing.
    ''' (二叉搜索树，用于建立对repository的索引文件)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class NaiveBinaryTree(Of K, V) : Inherits TreeBase(Of K, V)

        ''' <summary>
        ''' Returns the number of nodes in the tree
        ''' </summary>
        ''' <returns>Number of nodes in the tree</returns>
        Public ReadOnly Property Length As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return stack.Count
            End Get
        End Property

        Public Sub New(key As K, value As V, compares As Comparison(Of K), Optional views As Func(Of K, String) = Nothing)
            Call MyBase.New(compares, views)
            Call Me.insert(key, value)
        End Sub

        Public Sub New(compares As Comparison(Of K), Optional views As Func(Of K, String) = Nothing)
            MyBase.New(compares, views)
        End Sub

        ''' <summary>
        ''' Recursive destruction of binary search tree, called by method clear
        ''' and destroy. Can be used to kill a sub-tree of a larger tree.
        ''' This is a hanger on from its Delphi origins, it might be dispensable
        ''' given the garbage collection abilities of .NET
        ''' </summary>
        ''' <param name="p"></param>
        Private Sub KillTree(ByRef p As BinaryTree(Of K, V))
            If p IsNot Nothing Then
                KillTree(p.Left)
                KillTree(p.Right)
                p = Nothing
            End If
        End Sub

        ''' <summary>
        ''' Clear the binary tree.
        ''' </summary>
        Public Overrides Sub Clear()
            Call KillTree(root)
            Call stack.Clear()
        End Sub

        ''' <summary>
        ''' Find name in tree. Return a reference to the node
        ''' if symbol found else return null to indicate failure.
        ''' </summary>
        ''' <param name="key">Name of node to locate</param>
        ''' <returns>Returns null if it fails to find the node, else returns reference to node</returns>
        Public Function FindSymbol(key As K, Optional ByRef parent As BinaryTree(Of K, V) = Nothing) As BinaryTree(Of K, V)
            Dim np As BinaryTree(Of K, V) = root
            Dim cmp As Integer

            parent = Nothing

            While np IsNot Nothing
                cmp = compares(key, np.Key)

                If cmp = 0 Then
                    ' found !
                    Return np
                Else
                    parent = np
                End If

                If cmp < 0 Then
                    np = np.Left
                Else
                    np = np.Right
                End If
            End While

            ' Return null to indicate failure to find name
            Return Nothing
        End Function

        ''' <summary>
        ''' Recursively locates an empty slot in the binary tree and inserts the node
        ''' </summary>
        ''' <param name="node"></param>
        ''' <param name="tree"></param>
        ''' <returns>
        ''' 当处于append模式下，append值的时候不会返回节点，而是返回nothing
        ''' </returns>
        Private Function add(node As BinaryTree(Of K, V), ByRef tree As BinaryTree(Of K, V), append As Boolean) As BinaryTree(Of K, V)
            Do While True
                ' If we find a node with the same name then it's 
                ' a duplicate and we can't continue
                Dim comparison As Integer = compares(node.Key, tree.Key)

                If comparison = 0 Then
                    ' Duplicated node was found!
                    If append Then
                        ' clustering
                        DirectCast(tree!values, List(Of V)).Add(node.Value)
                        Return Nothing
                    Else
                        ' Value replace when not append
                        tree.Value = node.Value
                        Return node
                    End If
                ElseIf comparison < 0 Then
                    If Not tree.Left Is Nothing Then
                        tree = tree.Left
                    Else
                        tree.Left = node
                        Return node
                    End If
                Else
                    If Not tree.Right Is Nothing Then
                        tree = tree.Right
                    Else
                        tree.Right = node
                        Return node
                    End If
                End If
            Loop

            Throw New NotImplementedException("This exception will never happends!")
        End Function

        ''' <summary>
        ''' Add a symbol to the tree if it's a new one. Returns reference to the new
        ''' node if a new node inserted, else returns null to indicate node already present.
        ''' </summary>
        ''' <param name="append">
        ''' If this argument value is set to ``TRUE``, then it means do value key clustering. 
        ''' </param>
        ''' <returns> Returns reference to the new node is the node was inserted.
        ''' If a duplicate node (same name was located then returns null</returns>
        Public Function insert(key As K, obj As V, Optional append As Boolean = True) As BinaryTree(Of K, V)
            Dim node As New BinaryTree(Of K, V)(key, obj, toString:=views)

            Try
                If root Is Nothing Then
                    _root = node
                Else
                    node = add(node, root, append)
                End If

                If Not node Is Nothing Then
                    Call stack.Add(node)
                End If

                Return node
            Catch generatedExceptionName As Exception
                Dim ex = New Exception(node.ToString, generatedExceptionName)
                Return App.LogException(ex)
            End Try
        End Function

        ''' <summary>
        ''' Find the next ordinal node starting at node startNode.
        ''' Due to the structure of a binary search tree, the
        ''' successor node is simply the left most node on the right branch.
        ''' </summary>
        ''' <param name="startNode">Name key to use for searching</param>
        ''' <param name="parent">Returns the parent node if search successful</param>
        ''' <returns>Returns a reference to the node if successful, else null</returns>
        Public Function findSuccessor(startNode As BinaryTree(Of K, V), ByRef parent As BinaryTree(Of K, V)) As BinaryTree(Of K, V)
            parent = startNode
            ' Look for the left-most node on the right side
            startNode = startNode.Right
            While startNode.Left IsNot Nothing
                parent = startNode
                startNode = startNode.Left
            End While
            Return startNode
        End Function

        ''' <summary>
        ''' Delete a given node. This is the more complex method in the binary search
        ''' class. The method considers three senarios, 
        ''' 
        ''' + 1) the deleted node has no children; 
        ''' + 2) the deleted node as one child; 
        ''' + 3) the deleted node has two children. 
        ''' 
        ''' Case one and two are relatively simple to handle, the only unusual considerations 
        ''' are when the node is the root node. Case ``3)`` is much more complicated. It 
        ''' requires the location of the successor node.
        ''' 
        ''' The node to be deleted is then replaced by the sucessor node and the
        ''' successor node itself deleted. Throws an exception if the method fails
        ''' to locate the node for deletion.
        ''' </summary>
        ''' <param name="key">Name key of node to delete</param>
        Public Sub delete(key As K)
            Dim parent As BinaryTree(Of K, V) = Nothing
            ' First find the node to delete and its parent
            Dim nodeToDelete As BinaryTree(Of K, V) = FindSymbol(key, parent)

            If nodeToDelete Is Nothing Then
                Throw New Exception("Unable to delete node: " & key.ToString())
            End If
            ' can't find node, then say so 
            ' Three cases to consider, leaf, one child, two children

            ' If it is a simple leaf then just null what the parent is pointing to
            If (nodeToDelete.Left Is Nothing) AndAlso (nodeToDelete.Right Is Nothing) Then
                If parent Is Nothing Then
                    _root = Nothing
                    Return
                End If

                ' find out whether left or right is associated 
                ' with the parent and null as appropriate
                If parent.Left Is nodeToDelete Then
                    parent.Left = Nothing
                Else
                    parent.Right = Nothing
                End If

                Call stack.Remove(nodeToDelete)

                Return
            End If

            ' One of the children is null, in this case
            ' delete the node and move child up
            If nodeToDelete.Left Is Nothing Then
                ' Special case if we're at the root
                If parent Is Nothing Then
                    _root = nodeToDelete.Right
                    Return
                End If

                ' Identify the child and point the parent at the child
                If parent.Left Is nodeToDelete Then
                    parent.Right = nodeToDelete.Right
                Else
                    parent.Left = nodeToDelete.Right
                End If
                nodeToDelete = Nothing
                ' Clean up the deleted node
                Call stack.Remove(nodeToDelete)

                Return
            End If

            ' One of the children is null, in this case
            ' delete the node and move child up
            If nodeToDelete.Right Is Nothing Then
                ' Special case if we're at the root			
                If parent Is Nothing Then
                    _root = nodeToDelete.Left
                    Return
                End If

                ' Identify the child and point the parent at the child
                If parent.Left Is nodeToDelete Then
                    parent.Left = nodeToDelete.Left
                Else
                    parent.Right = nodeToDelete.Left
                End If
                nodeToDelete = Nothing
                ' Clean up the deleted node
                Call stack.Remove(nodeToDelete)

                Return
            End If

            ' Both children have nodes, therefore find the successor, 
            ' replace deleted node with successor and remove successor
            ' The parent argument becomes the parent of the successor
            Dim successor As BinaryTree(Of K, V) = findSuccessor(nodeToDelete, parent)
            ' Make a copy of the successor node
            Dim tmp As New BinaryTree(Of K, V)(successor.Key, successor.Value)
            ' Find out which side the successor parent is pointing to the
            ' successor and remove the successor
            If parent.Left Is successor Then
                parent.Left = Nothing
            Else
                parent.Right = Nothing
            End If

            ' Copy over the successor values to the deleted node position
            Call nodeToDelete.Copy(tmp)
            Call stack.Remove(nodeToDelete)
        End Sub

        ''' <summary>
        ''' Simple 'drawing' routines
        ''' </summary>
        ''' <param name="node"></param>
        ''' <returns></returns>
        Private Function drawNode(node As BinaryTree(Of K, V)) As String
            If node Is Nothing Then
                Return "empty"
            End If

            If (node.Left Is Nothing) AndAlso (node.Right Is Nothing) Then
                Return views(node.Key)
            End If
            If (node.Left IsNot Nothing) AndAlso (node.Right Is Nothing) Then
                Return views(node.Key) & "(" & drawNode(node.Left) & ", _)"
            End If

            If (node.Right IsNot Nothing) AndAlso (node.Left Is Nothing) Then
                Return views(node.Key) & "(_, " & drawNode(node.Right) & ")"
            End If

            Return views(node.Key) & "(" & drawNode(node.Left) & ", " & drawNode(node.Right) & ")"
        End Function

        ''' <summary>
        ''' Return the tree depicted as a simple string, useful for debugging, eg
        ''' 50(40(30(20, 35), 45(44, 46)), 60)
        ''' </summary>
        ''' <returns>Returns the tree</returns>
        Public Overrides Function ToString() As String
            Return drawNode(root)
        End Function
    End Class
End Namespace
