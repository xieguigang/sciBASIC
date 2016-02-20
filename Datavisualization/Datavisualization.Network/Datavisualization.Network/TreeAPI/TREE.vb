Imports Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree

Namespace TreeAPI

    Public Enum NodeTypes
        Path
        Leaf
        LeafX
        ROOT
    End Enum

    Public Class LeafX : Inherits TreeNode(Of NodeTypes)

        Public Property LeafX As FileStream.NetworkEdge()

        Sub New(parent As String)
            Call MyBase.New(parent & "-LeafX", NodeTypes.LeafX)
        End Sub
    End Class

    Public Class Leaf : Inherits TreeNode(Of NodeTypes)

        Sub New(parent As String)
            Call MyBase.New(parent & "-Leaf", NodeTypes.Leaf)
        End Sub
    End Class
End Namespace