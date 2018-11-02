Imports System.Runtime.CompilerServices

Namespace ComponentModel.Algorithm.BinaryTree

    Public MustInherit Class TreeBase(Of K, V)

        ''' <summary>
        ''' The root node of this binary tree
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property root As BinaryTree(Of K, V)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _root
            End Get
        End Property

        Protected _root As BinaryTree(Of K, V)

        Protected ReadOnly compares As Comparison(Of K)
        Protected ReadOnly views As Func(Of K, String)
        Protected ReadOnly stack As New List(Of BinaryTree(Of K, V))

        ''' <summary>
        ''' Create an instance of the AVL binary tree.
        ''' </summary>
        ''' <param name="compares">Compare between two keys.</param>
        ''' <param name="views">Display the key as string</param>
        Sub New(compares As Comparison(Of K), Optional views As Func(Of K, String) = Nothing)
            Me.compares = compares
            Me.views = views
        End Sub

        Public Function GetAllNodes() As BinaryTree(Of K, V)()
            Return stack.ToArray
        End Function
    End Class
End Namespace