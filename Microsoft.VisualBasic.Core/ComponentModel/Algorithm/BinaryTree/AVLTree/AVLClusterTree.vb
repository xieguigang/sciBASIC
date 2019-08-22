Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.Algorithm.BinaryTree

    Public Enum ComparisonDirectionPrefers
        Left
        Right
    End Enum

    Public Class ClusterKey(Of K)

        ReadOnly members As New List(Of K)
        ReadOnly views As Func(Of K, String)

        Public ReadOnly Property NumberOfKey As Integer
            Get
                Return members.Count
            End Get
        End Property

        Default Public ReadOnly Property Item(index As Integer) As K
            Get
                Return members(index)
            End Get
        End Property

        Sub New([single] As K, views As Func(Of K, String))
            Me.views = views

            ' Add a initial single member object
            Call members.Add([single])
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(newMember As K)
            Call members.Add(newMember)
        End Sub

        Public Overrides Function ToString() As String
            If members.Count = 1 Then
                Return views(members(Scan0))
            Else
                Return views(members(Scan0)) & $", and with {members.Count} cluster members.."
            End If
        End Function

        ''' <summary>
        ''' 在这里应该是多个key比较一个query
        ''' </summary>
        ''' <param name="compares"></param>
        ''' <returns></returns>
        Public Shared Function DoComparison(compares As Comparison(Of K), prefer As ComparisonDirectionPrefers) As Func(Of ClusterKey(Of K), K, Integer)
            Return Function(cluster, key) As Integer
                       Dim compareVal As Value(Of Integer) = -100
                       Dim left As Boolean = False
                       Dim right As Boolean

                       For Each index As K In cluster.members
                           If (compareVal = compares(index, key)) = 0 Then
                               Return 0
                           Else
                               If compareVal.Equals(1) Then
                                   right = True
                               Else
                                   left = True
                               End If
                           End If
                       Next

                       If prefer = ComparisonDirectionPrefers.Left Then
                           If left Then
                               Return -1
                           Else
                               Return 1
                           End If
                       Else
                           If right Then
                               Return 1
                           Else
                               Return -1
                           End If
                       End If
                   End Function
        End Function
    End Class

    Public Class AVLClusterTree(Of K) : Implements IEnumerable(Of ClusterKey(Of K))

        ReadOnly avltree As AVLTree(Of ClusterKey(Of K), K)
        ReadOnly views As Func(Of K, String)

        Sub New(compares As Comparison(Of K),
                Optional views As Func(Of K, String) = Nothing,
                Optional prefer As ComparisonDirectionPrefers = ComparisonDirectionPrefers.Left)

            Me.views = views
            Me.avltree = New AVLTree(Of ClusterKey(Of K), K)(doCompares(compares, prefer), Function(c) c.ToString)
        End Sub

        Private Shared Function doCompares(compares As Comparison(Of K), prefer As ComparisonDirectionPrefers) As Comparison(Of ClusterKey(Of K))
            Dim unsymmetricalCompares = ClusterKey(Of K).DoComparison(compares, prefer)

            ' 在AVL数据模块中，比较代码中，待插入的key都是放在左边，即X参数部分 
            Return Function(x, y)
                       Return unsymmetricalCompares(y, x(Scan0))
                   End Function
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(key As K)
            Call avltree.Add(New ClusterKey(Of K)(key, views), key, Sub(node, null) node.Key.Add(key))
        End Sub

        Public Sub Clear()
            Call avltree.Clear()
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of ClusterKey(Of K)) Implements IEnumerable(Of ClusterKey(Of K)).GetEnumerator
            For Each cluster In avltree.root.PopulateNodes
                Yield cluster.Key
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace