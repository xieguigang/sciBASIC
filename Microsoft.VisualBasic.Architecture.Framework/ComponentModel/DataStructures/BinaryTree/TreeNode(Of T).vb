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

        Public ReadOnly Property IsLeaf As Boolean
            Get
                Return Left Is Nothing AndAlso
                    Right Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' 递归的得到子节点的数目
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Count As Integer
            Get
                Dim n As Integer

                If Not Left Is Nothing Then
                    n += 1
                    n += Left.Count
                End If

                If Not Right Is Nothing Then
                    n += 1
                    n += Right.Count
                End If

                Return n
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Name & " ==> " & Value.ToString
        End Function

        ''' <summary>
        ''' 最多只有两个元素
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function GetEnumerator() As IEnumerator(Of TreeNode(Of T))
            If Not Left Is Nothing Then
                Yield Left
            End If
            If Not Right Is Nothing Then
                Yield Right
            End If
        End Function
    End Class
End Namespace