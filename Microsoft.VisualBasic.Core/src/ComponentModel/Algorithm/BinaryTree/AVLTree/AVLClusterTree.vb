#Region "Microsoft.VisualBasic::2299bbfc11de4d2633d71b105e646420, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\BinaryTree\AVLTree\AVLClusterTree.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 94
    '    Code Lines: 59 (62.77%)
    ' Comment Lines: 15 (15.96%)
    '    - Xml Docs: 86.67%
    ' 
    '   Blank Lines: 20 (21.28%)
    '     File Size: 3.78 KB


    '     Class AVLClusterTree
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: doCompares, GenericEnumerator, Search, ToString
    ' 
    '         Sub: (+2 Overloads) Add, Clear
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Algorithm.BinaryTree

    ''' <summary>
    ''' A binary tree model for do data clustering
    ''' </summary>
    ''' <typeparam name="K">
    ''' the key data type andalso the cluster value type
    ''' </typeparam>
    Public Class AVLClusterTree(Of K) : Implements Enumeration(Of ClusterKey(Of K))

        ReadOnly avltree As AVLTree(Of ClusterKey(Of K), K)
        ReadOnly views As Func(Of K, String)

        ''' <summary>
        ''' thread unsafe
        ''' </summary>
        ReadOnly addClusterMember As New DelegateTreeInsertCallback(Of ClusterKey(Of K), K)

        Dim totals As Integer

        Sub New(compares As Comparison(Of K),
                Optional views As Func(Of K, String) = Nothing,
                Optional prefer As ComparisonDirectionPrefers = ComparisonDirectionPrefers.Left)

            Me.views = views
            Me.avltree = New AVLTree(Of ClusterKey(Of K), K)(doCompares(compares, prefer), Function(c) c.ToString)
        End Sub

        Private Shared Function doCompares(compares As Comparison(Of K), prefer As ComparisonDirectionPrefers) As Comparison(Of ClusterKey(Of K))
            Dim unsymmetricalCompares = ClusterKey(Of K).DoComparison(compares, prefer, False)

            ' 在AVL数据模块中，比较代码中，待插入的key都是放在左边，即X参数部分 
            Return Function(x, y)
                       Return unsymmetricalCompares(y, x(Scan0))
                   End Function
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(key As K)
            addClusterMember.insertDuplicated = Sub(node, null) node.Key.Add(key)
            avltree.Add(New ClusterKey(Of K)(key, views), key, callback:=addClusterMember)
            totals += 1
        End Sub

        Public Sub Add(key As K, cluster As Action(Of ClusterKey(Of K), K),
                       Optional left As Action(Of ClusterKey(Of K), K) = Nothing,
                       Optional right As Action(Of ClusterKey(Of K), K) = Nothing)

            If Not cluster Is Nothing Then addClusterMember.insertDuplicated = Sub(node, null) Call cluster(node.Key, key)
            If Not left Is Nothing Then addClusterMember.insertLeft = Sub(node, null) Call left(node.Key, key)
            If Not right Is Nothing Then addClusterMember.insertRight = Sub(node, null) Call right(node.Key, key)

            Dim clusterRoot As New ClusterKey(Of K)(key, views)

            avltree.Add(clusterRoot, key, callback:=addClusterMember)
            totals += 1
        End Sub

        ''' <summary>
        ''' Make key search and then populate all hits cluster member
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        Public Iterator Function Search(key As K) As IEnumerable(Of K)
            Dim hit = avltree.Find(New ClusterKey(Of K)(key, views))

            If hit Is Nothing Then
                Return
            End If

            For Each member As K In hit.Members
                Yield member
            Next
        End Function

        Public Sub Clear()
            avltree.Clear()
            totals = 0
        End Sub

        Public Overrides Function ToString() As String
            Return $"Total {totals} members and {avltree.root.GetNodeCounts} clusters"
        End Function

        Private Iterator Function GenericEnumerator() As IEnumerator(Of ClusterKey(Of K)) Implements Enumeration(Of ClusterKey(Of K)).GenericEnumerator
            For Each cluster In avltree.root.PopulateNodes
                Yield cluster.Key
            Next
        End Function
    End Class
End Namespace
