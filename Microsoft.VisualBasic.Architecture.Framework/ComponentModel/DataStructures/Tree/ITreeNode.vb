Namespace ComponentModel.DataStructures.Tree

    Public Interface ITreeNode(Of T)

        Property Parent() As T

        ReadOnly Property IsLeaf() As Boolean
        ReadOnly Property IsRoot() As Boolean

        Function GetRootNode() As T

        Function GetFullyQualifiedName() As String
    End Interface
End Namespace