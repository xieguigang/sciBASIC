Imports Microsoft.VisualBasic.Linq

Public Class Tree(Of T) : Inherits Vertex

    Public Property Childs As Tree(Of T)()
    Public Property Parent As Tree(Of T)
    Public Property Data As T

    ''' <summary>
    ''' Not null child count in this tree node.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Count As Integer
        Get
            Dim childs = Me.Childs _
                .SafeQuery _
                .Where(Function(c) Not c Is Nothing) _
                .ToArray

            If childs.IsNullOrEmpty Then
                Return 1  ' 自己算一个节点，所以数量总是1的
            Else
                Dim n% = childs.Length

                For Each node In childs
                    n += node.Count ' 如果节点没有childs，则会返回1，因为他自身就是一个节点
                Next

                Return n
            End If
        End Get
    End Property

    Public ReadOnly Property QualifyName As String
        Get
            If Not Parent Is Nothing Then
                Return Parent.QualifyName & "." & Label
            Else
                Return Label
            End If
        End Get
    End Property

    Public ReadOnly Property IsRoot As Boolean
        Get
            Return Parent Is Nothing
        End Get
    End Property

    Public ReadOnly Property IsLeaf As Boolean
        Get
            Return Childs.IsNullOrEmpty
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return QualifyName
    End Function
End Class