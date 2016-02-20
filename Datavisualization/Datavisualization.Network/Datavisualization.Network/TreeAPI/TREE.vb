Imports Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree
Imports Microsoft.VisualBasic.Linq

Namespace TreeAPI

    Public Enum NodeTypes
        Path
        Leaf
        LeafX
        ROOT
    End Enum

    Public MustInherit Class TreeNode : Inherits TreeNode(Of NodeTypes)

        Public MustOverride Function GetEntities() As String()

        Sub New(parent As String, myType As NodeTypes)
            Call MyBase.New(parent, myType)
        End Sub
    End Class

    Public Class LeafX : Inherits TreeNode

        Public Property LeafX As FileStream.NetworkEdge()

        Sub New(parent As String)
            Call MyBase.New(parent & "-LeafX", NodeTypes.LeafX)
        End Sub

        Public Overrides Function GetEntities() As String()
            Return LeafX.ToArray(Function(x) x.ToNode)
        End Function
    End Class

    Public Class Leaf : Inherits TreeNode

        Sub New(parent As String)
            Call MyBase.New(parent & "-Leaf", NodeTypes.Leaf)
        End Sub

        Public Overrides Function GetEntities() As String()
            Return Me.GetEnumerator.ToArray(Function(x) x.Name)
        End Function
    End Class
End Namespace