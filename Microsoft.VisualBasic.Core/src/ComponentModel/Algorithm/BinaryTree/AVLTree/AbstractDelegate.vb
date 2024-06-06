Namespace ComponentModel.Algorithm.BinaryTree

    Public Delegate Sub TreeKeyInsertHandler(Of K, V)(keyNode As BinaryTree(Of K, V), newValue As V)

    Public Class DelegateTreeInsertCallback(Of K, V)

        ''' <summary>
        ''' usually for add cluster member into cluster
        ''' </summary>
        Public insertDuplicated As TreeKeyInsertHandler(Of K, V) =
            Sub(tree, key)
                ' do nothing 
            End Sub
        Public insertRight As TreeKeyInsertHandler(Of K, V) =
            Sub(tree, key)
                ' do nothing
            End Sub
        Public insertLeft As TreeKeyInsertHandler(Of K, V) =
            Sub(tree, key)
                ' do nothing
            End Sub

    End Class

End Namespace