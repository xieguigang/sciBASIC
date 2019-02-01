Namespace ComponentModel.Algorithm.BinaryTree

    Public Class Iterator(Of K, V)

        Public _tree As RBTree(Of K, V)
        Public _ancestors As Stack(Of RBNode(Of K, V))
        Public _cursor As RBNode(Of K, V)

        Public Sub New(tree As RBTree(Of K, V))
            Me._tree = tree
            Me._ancestors = New Stack(Of RBNode(Of K, V))
            Me._cursor = Nothing
        End Sub

        Public ReadOnly Property data() As V
            Get
                Return If(Me._cursor IsNot Nothing, Me._cursor.Value, Nothing)
            End Get
        End Property

        ''' <summary>
        ''' if null-iterator, returns first node
        ''' otherwise, returns next node
        ''' </summary>
        ''' <returns></returns>
        Public Function [next]() As V
            If Me._cursor Is Nothing Then
                Dim root = Me._tree.root
                If root IsNot Nothing Then
                    Me._minNode(root)
                End If
            Else
                If Me._cursor.Right Is Nothing Then
                    ' no greater node in subtree, go up to parent
                    ' if coming from a right child, continue up the stack
                    Dim save
                    Do
                        save = Me._cursor
                        If Me._ancestors.Count > 0 Then
                            Me._cursor = Me._ancestors.Pop()
                        Else
                            Me._cursor = Nothing
                            Exit Do
                        End If
                    Loop While Me._cursor.Right = save
                Else
                    ' get the next node from the subtree
                    Me._ancestors.Push(Me._cursor)
                    Me._minNode(Me._cursor.Right)
                End If
            End If
            Return If(Me._cursor IsNot Nothing, Me._cursor.Value, Nothing)
        End Function

        ''' <summary>
        ''' if null-iterator, returns last node
        ''' otherwise, returns previous node
        ''' </summary>
        ''' <returns></returns>
        Public Function prev() As V
            If Me._cursor Is Nothing Then
                Dim root = Me._tree.root
                If root IsNot Nothing Then
                    Me._maxNode(root)
                End If
            Else
                If Me._cursor.Left Is Nothing Then
                    Dim save
                    Do
                        save = Me._cursor
                        If Me._ancestors.Count > 0 Then
                            Me._cursor = Me._ancestors.Pop()
                        Else
                            Me._cursor = Nothing
                            Exit Do
                        End If
                    Loop While Me._cursor.Left = save
                Else
                    Me._ancestors.Push(Me._cursor)
                    Me._maxNode(Me._cursor.Left)
                End If
            End If
            Return If(Me._cursor IsNot Nothing, Me._cursor.Value, Nothing)
        End Function

        Public Sub _minNode(start As RBNode(Of K, V))
            While start.Left IsNot Nothing
                Me._ancestors.Push(start)
                start = start.Left
            End While
            Me._cursor = start
        End Sub

        Public Sub _maxNode(start As Object)
            While start.right IsNot Nothing
                Me._ancestors.Push(start)
                start = start.right
            End While
            Me._cursor = start
        End Sub
    End Class
End Namespace