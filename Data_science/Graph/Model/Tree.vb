Imports Microsoft.VisualBasic.Linq

''' <summary>
''' Tree node with data.(可以直接被使用的树对象类型)
''' </summary>
''' <typeparam name="T"></typeparam>
Public Class Tree(Of T) : Inherits AbstractTree(Of Tree(Of T))
    Public Property Data As T
End Class

Public Class AbstractTree(Of T As AbstractTree(Of T)) : Inherits Vertex

    Public Property Childs As List(Of T)
    Public Property Parent As T

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

    ''' <summary>
    ''' 计算出所有的叶节点的总数，包括自己的child的叶节点
    ''' </summary>
    ''' <returns></returns>
    Public Function CountLeafs() As Integer
        Return CountLeafs(Me, 0)
    End Function

    ''' <summary>
    ''' 对某一个节点的所有的叶节点进行计数
    ''' </summary>
    ''' <param name="node"></param>
    ''' <param name="count"></param>
    ''' <returns></returns>
    Public Shared Function CountLeafs(node As T, count As Integer) As Integer
        If node.IsLeaf Then
            count += 1
        End If

        For Each child As T In node.Childs.SafeQuery
            count += child.CountLeafs()
        Next

        Return count
    End Function
End Class
