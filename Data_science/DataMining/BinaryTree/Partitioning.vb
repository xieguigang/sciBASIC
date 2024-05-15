#Region "Microsoft.VisualBasic::7e38debca4027d5a5f53dbd81914ddba, Data_science\DataMining\BinaryTree\Partitioning.vb"

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

    '   Total Lines: 113
    '    Code Lines: 84
    ' Comment Lines: 11
    '   Blank Lines: 18
    '     File Size: 3.57 KB


    ' Module Partitioning
    ' 
    '     Function: CreateClusterPartitions, getAnyData, GetClusterResult, GetClusterResultInternal
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq

Public Module Partitioning

    ''' <summary>
    ''' do tree cut
    ''' </summary>
    ''' <param name="btree"></param>
    ''' <param name="depth"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function CreateClusterPartitions(btree As BTreeCluster, Optional depth As Integer = 3) As IEnumerable(Of BTreeCluster)
        Throw New NotImplementedException
    End Function

    <Extension>
    Public Function GetClusterResult(btree As BTreeCluster, Optional vnames As String() = Nothing) As IEnumerable(Of EntityClusterModel)
        If vnames Is Nothing Then
            vnames = btree _
                .getAnyData _
                .Select(Function(d, i) $"x_{i + 1}") _
                .ToArray
        End If

        Return btree.GetClusterResultInternal(vnames)
    End Function

    ''' <summary>
    ''' Just get data vector for generates the names
    ''' </summary>
    ''' <param name="btree"></param>
    ''' <returns></returns>
    <Extension>
    Private Function getAnyData(btree As BTreeCluster) As Double()
        If btree Is Nothing Then
            Return Nothing
        End If

        If Not btree.data.IsNullOrEmpty Then
            Return btree.data.Values.First
        End If

        Dim data As Object = getAnyData(btree.left)

        If Not data Is Nothing Then
            Return data
        Else
            data = getAnyData(btree.right)
        End If

        Return data
    End Function

    <Extension>
    Private Iterator Function GetClusterResultInternal(btree As BTreeCluster, vnames As String()) As IEnumerable(Of EntityClusterModel)
        Dim data = btree.data

        If data.IsNullOrEmpty Then
            Yield New EntityClusterModel With {
                .ID = btree.uuid,
                .Cluster = btree.uuid
            }

            For Each id As String In btree.members.SafeQuery
                Yield New EntityClusterModel With {
                    .ID = id,
                    .Cluster = btree.uuid
                }
            Next
        Else
            Dim ds As Double() = data(btree.uuid)
            Dim v As New Dictionary(Of String, Double)

            For i As Integer = 0 To vnames.Length - 1
                Call v.Add(vnames(i), ds(i))
            Next

            Yield New EntityClusterModel With {
                .ID = btree.uuid,
                .Cluster = btree.uuid,
                .Properties = v
            }

            For Each id As String In btree.members.SafeQuery
                ds = data(id)
                v = New Dictionary(Of String, Double)

                For i As Integer = 0 To vnames.Length - 1
                    Call v.Add(vnames(i), ds(i))
                Next

                Yield New EntityClusterModel With {
                    .ID = id,
                    .Cluster = btree.uuid,
                    .Properties = v
                }
            Next
        End If

        If Not btree.left Is Nothing Then
            For Each obj As EntityClusterModel In btree.left.GetClusterResultInternal(vnames)
                Yield obj
            Next
        End If
        If Not btree.right Is Nothing Then
            For Each obj As EntityClusterModel In btree.right.GetClusterResultInternal(vnames)
                Yield obj
            Next
        End If
    End Function
End Module
