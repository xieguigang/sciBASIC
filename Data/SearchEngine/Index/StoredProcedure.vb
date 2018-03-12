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
End Module
