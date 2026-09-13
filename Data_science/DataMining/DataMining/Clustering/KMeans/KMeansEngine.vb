#Region "Microsoft.VisualBasic::KMeansEngine, Data_science\DataMining\DataMining\Clustering\KMeans\KMeansEngine.vb"

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
    ' along with this program.  If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Correlations

Namespace KMeans

    ''' <summary>
    ''' 基于统一二维表<see cref="NumericTable"/>的 KMeans 聚类引擎。
    ''' 
    ''' 算法直接消费数值行（``Double()``）而不再依赖
    ''' <see cref="ComponentModel.EntityBase(Of Double)"/> 实体对象，
    ''' 运算结果可以方便的写入<see cref="NumericTable.labels"/>标签矩阵之中。
    ''' </summary>
    Public Class KMeansEngine

        ReadOnly debug As Boolean
        ''' <summary>
        ''' the max iteration loop number
        ''' </summary>
        ReadOnly max_iters As Integer
        ReadOnly n_threads As Integer
        ReadOnly auto_parallel As Boolean

        ''' <param name="n_threads">
        ''' 默认是使用并行化的计算代码以通过牺牲内存空间的代价来获取高性能的计算，非并行化的代码比较适合低内存的设备上面运行
        ''' </param>
        ''' <param name="max_iters">the max iteration loop number</param>
        Sub New(Optional debug As Boolean = False,
                Optional max_iters As Integer = -1,
                Optional n_threads As Integer = 16,
                Optional auto_parallel As Boolean = True)

            Me.debug = debug
            Me.max_iters = max_iters
            Me.n_threads = n_threads
            Me.auto_parallel = auto_parallel
        End Sub

        ''' <summary>
        ''' 对二维表中的样本特征进行 KMeans 聚类
        ''' </summary>
        ''' <param name="source">目标二维表</param>
        ''' <param name="k">簇的数量</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ClusterDataSet(source As NumericTable, k As Integer) As NumericClusterCollection
            Return ClusterDataSet(source.NumericRows(), source.RowNamesOrDefault(), k)
        End Function

        ''' <summary>
        ''' 对数值行集合进行 KMeans 聚类
        ''' </summary>
        ''' <param name="rows">行主序的数值矩阵</param>
        ''' <param name="ids">每一行所对应的样本 ID/行名</param>
        ''' <param name="k">簇的数量</param>
        ''' <returns></returns>
        Public Function ClusterDataSet(rows As Double()(), ids As String(), k As Integer) As NumericClusterCollection
            If k <= 0 Then
                Return New NumericClusterCollection
            End If

            Dim rowCount As Integer = rows.Length

            If k >= rowCount Then
                Throw New Exception($"[cluster.count:={k}] >= [source.length:={rowCount}], this will caused a dead loop!")
            End If

            Dim clusters As NumericClusterCollection = CreateInitialCenters(rows, ids, k)
            Dim [stop] As Integer = Me.max_iters

            If [stop] <= 0 Then
                [stop] = k * rowCount
            End If

            Return ClusterDataSetLoop(clusters, rows, ids, [stop])
        End Function

        Private Function CreateInitialCenters(rows As Double()(), ids As String(), k As Integer) As NumericClusterCollection
            Dim picked As New List(Of Integer)
            Dim clusters As New NumericClusterCollection
            Dim rowCount As Integer = rows.Length
            Dim clusterNumber As Integer

            While picked.Count < k
                clusterNumber = RandomExtensions.seeds.[Next](0, rowCount - 1)

                If Not picked.Contains(clusterNumber) Then
                    Call picked.Add(clusterNumber)

                    Dim cluster As New NumericKMeansCluster
                    Call cluster.Add(rows(clusterNumber), clusterNumber, ids(clusterNumber))
                    Call clusters.Add(cluster)
                End If
            End While

            Return clusters
        End Function

        Const NoMember$ = "Cluster count cannot be ZERO!"

        Private Function ClusterDataSetLoop(clusters As NumericClusterCollection, rows As Double()(), ids As String(), [stop] As Integer) As NumericClusterCollection
            Dim lastStables As Integer
            Dim hits As Integer = 0
            Dim stableClustersCount As Integer = 0
            Dim iterationCount As Integer = 0
            Dim newClusters As NumericClusterCollection

            While stableClustersCount <> clusters.NumOfCluster
                newClusters = ClusterDataSet(clusters, rows, ids)
                stableClustersCount = 0

                For clusterIndex As Integer = 0 To clusters.NumOfCluster - 1
                    Dim x As NumericKMeansCluster = newClusters(clusterIndex)
                    Dim y As NumericKMeansCluster = clusters(clusterIndex)

                    If x.NumOfEntity = 0 OrElse y.NumOfEntity = 0 Then
                        Continue For
                    End If

                    If DistanceMethods.EuclideanDistance(x.ClusterMean, y.ClusterMean) = 0 Then
                        stableClustersCount += 1
                    End If
                Next

                iterationCount += 1
                clusters = newClusters

                If iterationCount > [stop] Then
                    Exit While
                ElseIf hits > 25 Then
                    Return clusters
                Else
                    If lastStables = stableClustersCount Then
                        hits += 1
                    Else
                        lastStables = stableClustersCount
                        hits = 0
                    End If
                End If
            End While

            Return clusters
        End Function

        ''' <summary>
        ''' 依据当前的簇中心，将所有的样本重新分配到距离最近的那个簇之中
        ''' </summary>
        Public Function ClusterDataSet(clusters As NumericClusterCollection, rows As Double()(), ids As String()) As NumericClusterCollection
            Dim width As Integer = If(rows.Length = 0, 0, rows(Scan0).Length)
            Dim newClusters As New NumericClusterCollection

            If clusters.NumOfCluster <= 0 Then
                Throw New SystemException(NoMember)
            End If

            For count As Integer = 0 To clusters.NumOfCluster - 1
                Call newClusters.Add(New NumericKMeansCluster)
            Next

            For i As Integer = 0 To rows.Length - 1
                Dim minIndex As Integer = 0
                Dim minDistance As Double = Double.MaxValue
                Dim row As Double() = rows(i)

                For ci As Integer = 0 To clusters.NumOfCluster - 1
                    Dim mean As Double() = clusters(ci).ClusterMean

                    If mean Is Nothing OrElse mean.Length = 0 Then
                        mean = New Double(width - 1) {}
                    End If

                    Dim distance As Double = row.EuclideanDistance(mean)

                    If distance < minDistance Then
                        minDistance = distance
                        minIndex = ci
                    End If
                Next

                Call newClusters(minIndex).Add(row, i, ids(i))
            Next

            Return newClusters
        End Function
    End Class
End Namespace
