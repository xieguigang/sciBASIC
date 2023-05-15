#Region "Microsoft.VisualBasic::eb6950d0324c575c9e727f0a29e12344, sciBASIC#\Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\BinaryTree\AVLTree\AVLClusterTree.vb"

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

    '   Total Lines: 129
    '    Code Lines: 99
    ' Comment Lines: 7
    '   Blank Lines: 23
    '     File Size: 4.86 KB


    '     Enum ComparisonDirectionPrefers
    ' 
    '         Left, Right
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class ClusterKey
    ' 
    '         Properties: NumberOfKey
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: DoComparison, ToString
    ' 
    '         Sub: Add
    ' 
    '     Class AVLClusterTree
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: doCompares, GetEnumerator, IEnumerable_GetEnumerator
    ' 
    '         Sub: Add, Clear
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.Algorithm.BinaryTree

    Public Enum ComparisonDirectionPrefers
        Left
        Right
    End Enum

    ''' <summary>
    ''' wrapper for the cluster data
    ''' </summary>
    ''' <typeparam name="K"></typeparam>
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

    ''' <summary>
    ''' A binary tree model for do data clustering
    ''' </summary>
    ''' <typeparam name="K"></typeparam>
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
