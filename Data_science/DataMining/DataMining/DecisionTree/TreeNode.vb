Namespace DecisionTree

    Public Class TreeNode

        Public ReadOnly Property name As String
        Public ReadOnly Property edge As String
        Public ReadOnly Property nodeAttr As MyAttribute
        Public ReadOnly Property childNodes As List(Of TreeNode)
        Public ReadOnly Property index As Integer
        Public ReadOnly Property isLeaf As Boolean

        Public Sub New(name As String, tableIndex As Integer, nodeAttribute As MyAttribute, edge As String)
            Me.name = name
            index = tableIndex
            nodeAttr = nodeAttribute
            childNodes = New List(Of TreeNode)()
            Me.edge = edge
        End Sub

        Public Sub New(isleaf As Boolean, name As String, edge As String)
            Me.isLeaf = isleaf
            Me.name = name
            Me.edge = edge
        End Sub

    End Class
End Namespace