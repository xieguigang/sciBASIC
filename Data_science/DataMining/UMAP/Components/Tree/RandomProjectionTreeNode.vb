Namespace Tree

    Public NotInheritable Class RandomProjectionTreeNode
        Public Property IsLeaf As Boolean
        Public Property Indices As Integer()
        Public Property LeftChild As RandomProjectionTreeNode
        Public Property RightChild As RandomProjectionTreeNode
        Public Property Hyperplane As Single()
        Public Property Offset As Single
    End Class
End Namespace