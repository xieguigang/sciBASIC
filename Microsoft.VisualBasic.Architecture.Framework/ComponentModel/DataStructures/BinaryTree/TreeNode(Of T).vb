Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace ComponentModel.DataStructures.BinaryTree

    ''' <summary>
    ''' Define tree nodes
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TreeNode(Of T) : Implements sIdEnumerable

        Public Property Name As String Implements sIdEnumerable.Identifier
        Public Property Value As T
        Public Property Left As TreeNode(Of T)
        Public Property Right As TreeNode(Of T)

        ''' <summary>
        ''' Constructor  to create a single node 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="d"></param>
        Public Sub New(name As String, d As T)
            Me.Name = name
            Me.Value = d
        End Sub

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return Name & " ==> " & Value.ToString
        End Function
    End Class
End Namespace