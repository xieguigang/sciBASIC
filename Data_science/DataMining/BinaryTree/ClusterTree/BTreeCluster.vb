#Region "Microsoft.VisualBasic::a0ebc0ad925cda6db8d7cfdadc8074c0, Data_science\DataMining\BinaryTree\ClusterTree\BTreeCluster.vb"

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

    '   Total Lines: 62
    '    Code Lines: 42
    ' Comment Lines: 10
    '   Blank Lines: 10
    '     File Size: 2.27 KB


    ' Class BTreeCluster
    ' 
    '     Properties: data, left, members, right, uuid
    ' 
    '     Function: (+2 Overloads) GetClusters, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' Clustering data via binary tree
''' </summary>
Public Class BTreeCluster : Implements INamedValue

    Public Property left As BTreeCluster
    Public Property right As BTreeCluster

    Public Property uuid As String Implements INamedValue.Key

    ''' <summary>
    ''' 这个列表之中已经保存有<see cref="uuid"/>了
    ''' </summary>
    ''' <returns></returns>
    Public Property members As String()
    Public Property data As Dictionary(Of String, Object)

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return $"[{uuid}] ({members.Length} members) {members.JoinBy(", ")}"
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Shared Function GetClusters(btree As AVLTree(Of String, String), Optional compares As ComparisonProvider = Nothing) As BTreeCluster
        Return GetClusters(btree.root, compares)
    End Function

    Private Shared Function GetClusters(btree As BinaryTree(Of String, String), compares As ComparisonProvider) As BTreeCluster
        Dim data As Dictionary(Of String, Object) = Nothing

        If btree Is Nothing Then
            Return Nothing
        End If

        If Not compares Is Nothing Then
            ' add root
            data = New Dictionary(Of String, Object) From {
                {btree.Key, compares.GetObject(btree.Key)}
            }
            ' and then add members
            btree.Members _
                .SafeQuery _
                .DoEach(Sub(id)
                            ' assign members to the result collection
                            data(id) = compares.GetObject(id)
                        End Sub)
        End If

        Return New BTreeCluster With {
            .uuid = btree.Key,
            .members = btree.Members,
            .left = GetClusters(btree.Left, compares),
            .right = GetClusters(btree.Right, compares),
            .data = data
        }
    End Function
End Class
