Imports Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree

Namespace TreeAPI

    Public Enum NodeTypes
        Path
        Leaf
        LeafX
        ROOT
    End Enum

    Public Class TreeNode : Inherits TreeNode(Of NodeTypes)

    End Class

    Public Class LeafX : Inherits TreeNode(Of NodeTypes)

        Public Property LeafX As FileStream.Node()
    End Class
End Namespace