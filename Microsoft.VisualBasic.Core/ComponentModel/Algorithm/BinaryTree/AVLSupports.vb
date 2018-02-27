Imports System.Runtime.CompilerServices

Namespace ComponentModel.Algorithm.BinaryTree

    ''' <summary>
    ''' Binary tree balance helper
    ''' </summary>
    Public Module AVLSupports

        <Extension>
        Public Function RotateLL(Of K, V)(node As BinaryTree(Of K, V)) As BinaryTree(Of K, V)
            Dim top = node.Left

            node.Left = top.Right
            top.Right = node

            node.PutHeight
            top.PutHeight

            Return top
        End Function

        <Extension>
        Public Function RotateRR(Of K, V)(node As BinaryTree(Of K, V)) As BinaryTree(Of K, V)
            Dim top = node.Right

            node.Right = top.Left
            top.Left = node

            Call node.PutHeight
            Call top.PutHeight

            Return top
        End Function

        <Extension>
        Public Function RotateLR(Of K, V)(node As BinaryTree(Of K, V)) As BinaryTree(Of K, V)
            node.Left = node.Left.RotateRR
            Return node.RotateLL
        End Function

        <Extension>
        Public Function RotateRL(Of K, V)(node As BinaryTree(Of K, V)) As BinaryTree(Of K, V)
            node.Right = node.Right.RotateLL
            Return node.RotateRR
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Function height(Of K, V)(node As BinaryTree(Of K, V)) As Double
            Return If(node Is Nothing, -1, CDbl(node!height))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Sub PutHeight(Of K, V)(node As BinaryTree(Of K, V))
            node.SetValue("height", Math.Max(node.Left.height, node.Right.height) + 1)
        End Sub
    End Module

    Public Class AVLTree(Of K, V)

        ''' <summary>
        ''' The root node of this binary tree
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property root As BinaryTree(Of K, V)

        ReadOnly compares As Comparison(Of K)
        ReadOnly views As Func(Of K, String)

        Sub New(compares As Comparison(Of K), Optional views As Func(Of K, String) = Nothing)
            Me.compares = compares
            Me.views = views
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(key As K, value As V)
            _root = Add(key, value, _root)
        End Sub

        Public Function Add(key As K, value As V, tree As BinaryTree(Of K, V)) As BinaryTree(Of K, V)
            If tree Is Nothing Then
                tree = New BinaryTree(Of K, V)(key, value, Nothing, views)
            End If

            Select Case compares(key, tree.Key)
                Case < 0 : Call appendLeft(tree, key, value)
                Case > 0 : Call appendRight(tree, key, value)
                Case = 0

                    ' 将value追加到附加值中（也可对应重复元素）
                    tree.Value = value

                Case Else
                    ' This will never happend!
                    Throw New Exception("????")
            End Select

            tree.PutHeight

            Return tree
        End Function

        Private Sub appendRight(ByRef tree As BinaryTree(Of K, V), key As K, value As V)
            tree.Right = Add(key, value, tree.Right)

            If tree.Right.height - tree.Left.height = 2 Then
                If compares(key, tree.Right.Key) > 0 Then
                    tree = tree.RotateRR
                Else
                    tree = tree.RotateRL
                End If
            End If
        End Sub

        Private Sub appendLeft(ByRef tree As BinaryTree(Of K, V), key As K, value As V)
            tree.Left = Add(key, value, tree.Left)

            If tree.Left.height - tree.Right.height = 2 Then
                If compares(key, tree.Left.Key) < 0 Then
                    tree = tree.RotateLL
                Else
                    tree = tree.RotateLR
                End If
            End If
        End Sub
    End Class
End Namespace