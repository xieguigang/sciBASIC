Namespace DecisionTree

    Public Class TreeNode

        Public Property name As String
        Public Property edge As String
        Public Property attributes As Attributes
        Public Property childNodes As List(Of TreeNode)
        Public Property index As Integer
        Public Property isLeaf As Boolean

        Public Sub New(name As String, tableIndex As Integer, attributes As Attributes, edge As String)
            Me.name = name
            Me.index = tableIndex
            Me.attributes = attributes
            Me.childNodes = New List(Of TreeNode)()
            Me.edge = edge
        End Sub

        Public Sub New(isleaf As Boolean, name As String, edge As String)
            Me.isLeaf = isleaf
            Me.name = name
            Me.edge = edge
        End Sub

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            If isLeaf Then
                Return $"[{name}]"
            Else
                Return name
            End If
        End Function
    End Class
End Namespace