Namespace ComponentModel.DataStructures.Tree

    Public Interface ITreeNode(Of T)

        Property Parent() As T

        ReadOnly Property FullyQualifiedName() As String
        ReadOnly Property IsLeaf() As Boolean
        ReadOnly Property IsRoot() As Boolean

        Function GetRootNode() As T
        Function IteratesAllChilds() As IEnumerable(Of T)

    End Interface
End Namespace