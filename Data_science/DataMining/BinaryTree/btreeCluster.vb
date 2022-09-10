#Region "Microsoft.VisualBasic::f318b146e4efcd307636f7c99f1b62d0, sciBASIC#\Data_science\DataMining\BinaryTree\btreeCluster.vb"

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

    '   Total Lines: 37
    '    Code Lines: 26
    ' Comment Lines: 4
    '   Blank Lines: 7
    '     File Size: 1.27 KB


    ' Class BTreeCluster
    ' 
    '     Properties: left, members, right, uuid
    ' 
    '     Function: (+2 Overloads) GetClusters, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Public Class BTreeCluster : Implements INamedValue

    Public Property left As BTreeCluster
    Public Property right As BTreeCluster

    Public Property uuid As String Implements INamedValue.Key

    ''' <summary>
    ''' 这个列表之中已经保存有<see cref="uuid"/>了
    ''' </summary>
    ''' <returns></returns>
    Public Property members As String()

    Public Overrides Function ToString() As String
        Return $"[{uuid}] ({members.Length} members) {members.JoinBy(", ")}"
    End Function

    Public Shared Function GetClusters(btree As AVLTree(Of String, String)) As BTreeCluster
        Return GetClusters(btree.root)
    End Function

    Private Shared Function GetClusters(btree As BinaryTree(Of String, String)) As BTreeCluster
        If btree Is Nothing Then
            Return Nothing
        Else
            Return New BTreeCluster With {
                .uuid = btree.Key,
                .members = btree.Members,
                .left = GetClusters(btree.Left),
                .right = GetClusters(btree.Right)
            }
        End If
    End Function
End Class
