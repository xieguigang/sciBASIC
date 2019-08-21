Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.Algorithm.BinaryTree

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
        Public Shared Function DoComparison(compares As Comparison(Of K)) As Func(Of ClusterKey(Of K), K, Integer)
            Return Function(cluster, key) As Integer
                       Dim compareVal As Value(Of Integer) = -100
                       Dim values As New List(Of Integer)

                       For Each index As K In cluster.members
                           If (compareVal = compares(index, key)) = 0 Then
                               Return 0
                           Else
                               values += compareVal
                           End If
                       Next

                       If values.Where(Function(x) x = 1).Count > values.Count / 2 Then
                           Return 1
                       Else
                           Return -1
                       End If
                   End Function
        End Function
    End Class

    Public Class AVLClusterTree(Of K)

        ReadOnly avltree As AVLTree(Of ClusterKey(Of K), K)
        ReadOnly views As Func(Of K, String)

        Sub New(compares As Comparison(Of K), Optional views As Func(Of K, String) = Nothing)
            Me.views = views
            Me.avltree = New AVLTree(Of ClusterKey(Of K), K)(doCompares(compares), Function(c) c.ToString)
        End Sub

        Private Shared Function doCompares(compares As Comparison(Of K)) As Comparison(Of ClusterKey(Of K))
            Dim unsymmetricalCompares = ClusterKey(Of K).DoComparison(compares)

            ' 在AVL数据模块中，比较代码中，待插入的key都是放在左边，即X参数部分 
            Return Function(x, y)
                       Return unsymmetricalCompares(y, x(Scan0))
                   End Function
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(key As K)
            Call avltree.Add(New ClusterKey(Of K)(key, views), key)
        End Sub
    End Class
End Namespace