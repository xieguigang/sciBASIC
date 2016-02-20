Imports System.Collections

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

Namespace ComponentModel.DataStructures.BinaryTree

    ''' <summary>
    ''' The Binary tree itself.
    ''' 
    ''' A very basic Binary Search Tree. Not generalized, stores
    ''' name/value pairs in the tree nodes. name is the node key.
    ''' The advantage of a binary tree is its fast insert and lookup
    ''' characteristics. This version does not deal with tree balancing.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class BinaryTree(Of T)

        Dim Root As TreeNode(Of T)

        ''' <summary>
        ''' Points to the root of the tree
        ''' </summary>
        ''' <remarks></remarks>
        Dim _Count As Integer = 0

        Public Sub New()
        End Sub

        ' Recursive destruction of binary search tree, called by method clear
        ' and destroy. Can be used to kill a sub-tree of a larger tree.
        ' This is a hanger on from its Delphi origins, it might be dispensable
        ' given the garbage collection abilities of .NET
        Private Sub KillTree(ByRef p As TreeNode(Of T))
            If p IsNot Nothing Then
                KillTree(p.Left)
                KillTree(p.Right)
                p = Nothing
            End If
        End Sub

        ''' <summary>
        ''' Clear the binary tree.
        ''' </summary>
        Public Sub clear()
            Call KillTree(Root)
            _Count = 0
        End Sub

        ''' <summary>
        ''' Returns the number of nodes in the tree
        ''' </summary>
        ''' <returns>Number of nodes in the tree</returns>
        Public ReadOnly Property Length As Integer
            Get
                Return _Count
            End Get
        End Property

        ''' <summary>
        ''' Find name in tree. Return a reference to the node
        ''' if symbol found else return null to indicate failure.
        ''' </summary>
        ''' <param name="name">Name of node to locate</param>
        ''' <returns>Returns null if it fails to find the node, else returns reference to node</returns>
        Public Function FindSymbol(Name As String) As TreeNode(Of T)
            Dim np As TreeNode(Of T) = Root
            Dim cmp As Integer
            While np IsNot Nothing
                cmp = [String].Compare(Name, np.Name)
                If cmp = 0 Then
                    ' found !
                    Return np
                End If

                If cmp < 0 Then
                    np = np.Left
                Else
                    np = np.Right
                End If
            End While
            Return Nothing
            ' Return null to indicate failure to find name
        End Function


        ' Recursively locates an empty slot in the binary tree and inserts the node
        Private Sub add(node As TreeNode(Of T), ByRef tree As TreeNode(Of T))
            If tree Is Nothing Then
                tree = node
            Else
                ' If we find a node with the same name then it's 
                ' a duplicate and we can't continue
                Dim comparison As Integer = [String].Compare(node.Name, tree.Name)
                If comparison = 0 Then
                    Throw New Exception()
                End If

                If comparison < 0 Then
                    add(node, tree.Left)
                Else
                    add(node, tree.Right)
                End If
            End If
        End Sub

        ''' <summary>
        ''' Add a symbol to the tree if it's a new one. Returns reference to the new
        ''' node if a new node inserted, else returns null to indicate node already present.
        ''' </summary>
        ''' <param name="name">Name of node to add to tree</param>
        ''' <param name="d">Value of node</param>
        ''' <returns> Returns reference to the new node is the node was inserted.
        ''' If a duplicate node (same name was located then returns null</returns>
        Public Function insert(name As String, d As T) As TreeNode(Of T)
            Dim node As New TreeNode(Of T)(name, d)
            Try
                If Root Is Nothing Then
                    Root = node
                Else
                    add(node, Root)
                End If
                _Count += 1
                Return node
            Catch generatedExceptionName As Exception
                Return Nothing
            End Try
        End Function

        ' Searches for a node with name key, name. If found it returns a reference
        ' to the node and to thenodes parent. Else returns null.
        Private Function findParent(name As String, ByRef parent As TreeNode(Of T)) As TreeNode(Of T)
            Dim np As TreeNode(Of T) = Root
            parent = Nothing
            Dim cmp As Integer
            While np IsNot Nothing
                cmp = [String].Compare(name, np.Name)
                If cmp = 0 Then
                    ' found !
                    Return np
                End If

                If cmp < 0 Then
                    parent = np
                    np = np.Left
                Else
                    parent = np
                    np = np.Right
                End If
            End While
            Return Nothing
            ' Return null to indicate failure to find name
        End Function

        ''' <summary>
        ''' Find the next ordinal node starting at node startNode.
        ''' Due to the structure of a binary search tree, the
        ''' successor node is simply the left most node on the right branch.
        ''' </summary>
        ''' <param name="startNode">Name key to use for searching</param>
        ''' <param name="parent">Returns the parent node if search successful</param>
        ''' <returns>Returns a reference to the node if successful, else null</returns>
        Public Function findSuccessor(startNode As TreeNode(Of T), ByRef parent As TreeNode(Of T)) As TreeNode(Of T)
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
        ''' class. The method considers three senarios, 1) the deleted node has no
        ''' children; 2) the deleted node as one child; 3) the deleted node has two
        ''' children. Case one and two are relatively simple to handle, the only
        ''' unusual considerations are when the node is the root node. Case 3) is
        ''' much more complicated. It requires the location of the successor node.
        ''' The node to be deleted is then replaced by the sucessor node and the
        ''' successor node itself deleted. Throws an exception if the method fails
        ''' to locate the node for deletion.
        ''' </summary>
        ''' <param name="key">Name key of node to delete</param>
        Public Sub delete(key As String)
            Dim parent As TreeNode(Of T) = Nothing
            ' First find the node to delete and its parent
            Dim nodeToDelete As TreeNode(Of T) = findParent(key, parent)
            If nodeToDelete Is Nothing Then
                Throw New Exception("Unable to delete node: " & key.ToString())
            End If
            ' can't find node, then say so 
            ' Three cases to consider, leaf, one child, two children

            ' If it is a simple leaf then just null what the parent is pointing to
            If (nodeToDelete.Left Is Nothing) AndAlso (nodeToDelete.Right Is Nothing) Then
                If parent Is Nothing Then
                    Root = Nothing
                    Return
                End If

                ' find out whether left or right is associated 
                ' with the parent and null as appropriate
                If parent.Left Is nodeToDelete Then
                    parent.Left = Nothing
                Else
                    parent.Right = Nothing
                End If
                _Count -= 1
                Return
            End If

            ' One of the children is null, in this case
            ' delete the node and move child up
            If nodeToDelete.Left Is Nothing Then
                ' Special case if we're at the root
                If parent Is Nothing Then
                    Root = nodeToDelete.Right
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
                _Count -= 1
                Return
            End If

            ' One of the children is null, in this case
            ' delete the node and move child up
            If nodeToDelete.Right Is Nothing Then
                ' Special case if we're at the root			
                If parent Is Nothing Then
                    Root = nodeToDelete.Left
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
                _Count -= 1
                Return
            End If

            ' Both children have nodes, therefore find the successor, 
            ' replace deleted node with successor and remove successor
            ' The parent argument becomes the parent of the successor
            Dim successor As TreeNode(Of T) = findSuccessor(nodeToDelete, parent)
            ' Make a copy of the successor node
            Dim tmp As New TreeNode(Of T)(successor.Name, successor.Value)
            ' Find out which side the successor parent is pointing to the
            ' successor and remove the successor
            If parent.Left Is successor Then
                parent.Left = Nothing
            Else
                parent.Right = Nothing
            End If

            ' Copy over the successor values to the deleted node position
            nodeToDelete.Name = tmp.Name
            nodeToDelete.Value = tmp.Value
            _Count -= 1
        End Sub

        ' Simple 'drawing' routines
        Private Function drawNode(node As TreeNode(Of T)) As String
            If node Is Nothing Then
                Return "empty"
            End If

            If (node.Left Is Nothing) AndAlso (node.Right Is Nothing) Then
                Return node.Name
            End If
            If (node.Left IsNot Nothing) AndAlso (node.Right Is Nothing) Then
                Return node.Name & "(" & drawNode(node.Left) & ", _)"
            End If

            If (node.Right IsNot Nothing) AndAlso (node.Left Is Nothing) Then
                Return node.Name & "(_, " & drawNode(node.Right) & ")"
            End If

            Return node.Name & "(" & drawNode(node.Left) & ", " & drawNode(node.Right) & ")"
        End Function

        ''' <summary>
        ''' Return the tree depicted as a simple string, useful for debugging, eg
        ''' 50(40(30(20, 35), 45(44, 46)), 60)
        ''' </summary>
        ''' <returns>Returns the tree</returns>
        Public Overrides Function ToString() As String
            Return drawNode(Root)
        End Function
    End Class
End Namespace
