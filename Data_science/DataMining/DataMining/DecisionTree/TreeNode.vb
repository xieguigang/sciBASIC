Imports System.Collections.Generic

Namespace DecisionTree

    Public Class TreeNode
        Public Sub New(name__1 As String, tableIndex__2 As Integer, nodeAttribute__3 As MyAttribute, edge__4 As String)
            Name = name__1
            TableIndex = tableIndex__2
            NodeAttribute = nodeAttribute__3
            ChildNodes = New List(Of TreeNode)()
            Edge = edge__4
        End Sub

        Public Sub New(isleaf__1 As Boolean, name__2 As String, edge__3 As String)
            IsLeaf = isleaf__1
            Name = name__2
            Edge = edge__3
        End Sub

        Public ReadOnly Property Name() As String

        Public ReadOnly Property Edge() As String

        Public ReadOnly Property NodeAttribute() As MyAttribute

        Public ReadOnly Property ChildNodes() As List(Of TreeNode)

        Public ReadOnly Property TableIndex() As Integer

        Public ReadOnly Property IsLeaf() As Boolean
    End Class
End Namespace