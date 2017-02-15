Namespace ComponentModel.DataStructures.Tree

    Public Class SpecialTreeNode
        Inherits TreeNodeBase(Of SpecialTreeNode)

        Public Sub New(name As String)
            MyBase.New(name)
        End Sub

        Protected Overrides ReadOnly Property MySelf() As SpecialTreeNode
            Get
                Return Me
            End Get
        End Property

        Public Property IsSpecial() As Boolean

        Public Function GetSpecialNodes() As List(Of SpecialTreeNode)
            Return ChildNodes.Where(Function(x) x.IsSpecial).ToList()
        End Function

        Public Function GetSpecialLeafNodes() As List(Of SpecialTreeNode)
            Return ChildNodes.Where(Function(x) x.IsSpecial AndAlso x.IsLeaf).ToList()
        End Function

        Public Function GetSpecialNonLeafNodes() As List(Of SpecialTreeNode)
            Return ChildNodes.Where(Function(x) x.IsSpecial AndAlso Not x.IsLeaf).ToList()
        End Function

    End Class
End Namespace