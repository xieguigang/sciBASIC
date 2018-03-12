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
            .Additionals = tree!values
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
                .Additionals = leftNode!values,
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
                .Additionals = rightNode!values,
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
