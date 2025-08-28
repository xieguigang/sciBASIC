#Region "Microsoft.VisualBasic::8e46cc06287480a1c8bc0e45597e5830, Data_science\DataMining\DataMining\Clustering\KMeans\KMeans.vb"

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

    '   Total Lines: 325
    '    Code Lines: 221 (68.00%)
    ' Comment Lines: 53 (16.31%)
    '    - Xml Docs: 77.36%
    ' 
    '   Blank Lines: 51 (15.69%)
    '     File Size: 13.84 KB


    '     Class KMeansAlgorithm
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CheckParallel, (+3 Overloads) ClusterDataSet, ClusterDataSetLoop, (+2 Overloads) CreateInitialCenters, means
    '                   minIndex, ParallelEuclideanDistance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.Clustering
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Correlations
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace KMeans

    ''' <summary>
    ''' This class implement a KMeans clustering algorithm.
    ''' (请注意，实体对象的属性必须要长度一致)
    ''' </summary>
    Public Class KMeansAlgorithm(Of T As EntityBase(Of Double)) : Inherits TraceBackAlgorithm

        ReadOnly debug As Boolean = False
        ''' <summary>
        ''' the max iteration loop number
        ''' </summary>
        ReadOnly max_iters% = -1
        ReadOnly n_threads As Integer = 16
        ReadOnly auto_parallel As Boolean = True

        ''' <param name="n_threads">
        ''' 默认是使用并行化的计算代码以通过牺牲内存空间的代价来获取高性能的计算，非并行化的代码比较适合低内存的设备上面运行
        ''' </param>
        ''' <param name="max_iters">
        ''' the max iteration loop number
        ''' </param>
        Sub New(Optional debug As Boolean = False,
                Optional max_iters% = -1,
                Optional n_threads As Integer = 16,
                Optional auto_parallel As Boolean = True,
                Optional traceback As Boolean = False)

            Me.auto_parallel = auto_parallel
            Me.debug = debug
            Me.max_iters = max_iters
            Me.n_threads = n_threads

            If traceback Then
                Me.traceback = New TraceBackIterator
            End If
        End Sub

        Private Function CreateInitialCenters(data As T(), k As Integer) As ClusterCollection(Of T)
            Dim clusterNumbers As New List(Of Integer)
            Dim clusterNumber As Integer = 0
            Dim cluster As KMeansCluster(Of T) = Nothing
            Dim clusters As New ClusterCollection(Of T)
            Dim rowCount As Integer = data.Length

            If debug Then
                Call "Init assigned random clusters...".debug
            End If

            While clusterNumbers.Count < k
                clusterNumber = randf.seeds.[Next](0, rowCount - 1)

                If Not clusterNumbers.Contains(clusterNumber) Then
                    cluster = New KMeansCluster(Of T)
                    clusterNumbers.Add(clusterNumber)
                    cluster.Add(data(clusterNumber))
                    clusters.Add(cluster)
                End If
            End While

            Return clusters
        End Function

        Private Function CreateInitialCenters(canopy As CanopySeeds, activator As Func(Of IVector, T)) As ClusterCollection(Of T)
            Dim k_seeds As New ClusterCollection(Of T)
            Dim ki As KMeansCluster(Of T)

            For Each seed As IVector In canopy.Canopy
                ki = New KMeansCluster(Of T)()
                ki.Add(activator(seed))
                k_seeds.Add(ki)
            Next

            Return k_seeds
        End Function

        ''' <summary>
        ''' Seperates a dataset into clusters or groups with similar characteristics
        ''' </summary>
        ''' <param name="k">
        ''' The number of clusters or groups to form.(当这个参数值为0的时候，函数也会返回一个空集合)
        ''' </param>
        ''' <param name="source">
        ''' An array containing data that will be clustered, the elements number must greater than 2, at least 3 elements.
        ''' (里面的元素至少需要三个)
        ''' </param>
        ''' <returns>A collection of clusters of data</returns>
        ''' <remarks>
        ''' if the <paramref name="k"/> parameter value is greater than the
        ''' element count of the <paramref name="source"/> collection, then this api 
        ''' function will throw an exception
        ''' </remarks>
        Public Function ClusterDataSet(source As IEnumerable(Of T), k%) As ClusterCollection(Of T)
            Dim data As T() = source.ToArray
            Dim rowCount As Integer = data.Length
            Dim clusters As ClusterCollection(Of T) = CreateInitialCenters(data, k)
            Dim [stop] = Me.max_iters

            If k >= rowCount Then
                Throw New Exception($"[cluster.count:={k}] >= [source.length:={rowCount}], this will caused a dead loop!")
            End If
            If [stop] <= 0 Then
                [stop] = k * rowCount
            End If
            If n_threads > 1 Then
                Call $"Kmeans have {n_threads} CPU core for parallel computing.".debug
            End If

            Return ClusterDataSetLoop(clusters, data, [stop])
        End Function

        Public Function ClusterDataSet(source As IEnumerable(Of T), canopy As CanopySeeds, activator As Func(Of IVector, T)) As ClusterCollection(Of T)
            Dim data As T() = source.ToArray
            Dim rowCount As Integer = data.Length
            Dim k As Integer = canopy.k
            Dim clusters As ClusterCollection(Of T) = CreateInitialCenters(canopy, activator)
            Dim [stop] = Me.max_iters

            If k >= rowCount Then
                Throw New Exception($"[cluster.count:={k}] >= [source.length:={rowCount}], this will caused a dead loop!")
            End If
            If [stop] <= 0 Then
                [stop] = k * rowCount
            End If
            If n_threads > 1 Then
                Call $"Kmeans have {n_threads} CPU core for parallel computing.".debug
            End If

            Return ClusterDataSetLoop(clusters, data, [stop])
        End Function

        Const NoMember$ = "Cluster count cannot be ZERO!"

        Private Function ClusterDataSetLoop(clusters As ClusterCollection(Of T), data As T(), [stop] As Integer) As ClusterCollection(Of T)
            Dim lastStables%
            Dim hits%
            Dim stableClustersCount As Integer = 0
            Dim iterationCount As Integer = 0

            If debug Then
                Call "Start kmeans clustering....".debug
            End If

            While stableClustersCount <> clusters.NumOfCluster
                Dim newClusters As ClusterCollection(Of T) = ClusterDataSet(clusters, data)

                stableClustersCount = 0

                For clusterIndex As Integer = 0 To clusters.NumOfCluster - 1
                    Dim x As KMeansCluster(Of T) = newClusters(clusterIndex)  ' 这一次迭代的聚类结果
                    Dim y As KMeansCluster(Of T) = clusters(clusterIndex)  ' 上一次迭代的结果

                    If x.NumOfEntity = 0 OrElse y.NumOfEntity = 0 Then
                        Continue For ' ??? 为什么有些聚类是0？？
                    End If

                    ' 假若上一次的迭代结果和这一次迭代的结果一样，则距离是0，得到了一个稳定的聚类结果
                    If (DistanceMethods.EuclideanDistance(x.ClusterMean, y.ClusterMean)) = 0 Then
                        stableClustersCount += 1
                    End If
                Next

                iterationCount += 1
                clusters = newClusters

                ' 迭代的次数已经超过了最大的迭代次数了，则退出计算，否则可能会在这里出现死循环
                If iterationCount > [stop] Then
                    Exit While
                ElseIf hits > 25 Then
                    hits = 0

                    Return clusters

                    ' 随机混淆若干个稳定的cluster
                    ' clusters = clusters.CrossOver
                Else
                    If debug Then
                        Call $"[{iterationCount}/{[stop]}] stable <> NumOfCluster -> {stableClustersCount} <> {clusters.NumOfCluster} = {stableClustersCount <> clusters.NumOfCluster}".debug
                    End If
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
        ''' Seperates a dataset into clusters or groups with similar characteristics
        ''' </summary>
        ''' <param name="clusters">A collection of data clusters</param>
        ''' <param name="data">An array containing data to be clustered</param>
        ''' <returns>A collection of clusters of data</returns>
        ''' 
        Public Function ClusterDataSet(clusters As ClusterCollection(Of T), data As T()) As ClusterCollection(Of T)
            ' number of features
            Dim dataWidth As Integer = data(Scan0).Length
            Dim newClusters As New ClusterCollection(Of T)

            ' create a new collection of clusters
            For count As Integer = 0 To clusters.NumOfCluster - 1
                Call newClusters.Add(New KMeansCluster(Of T))
            Next

            If clusters.NumOfCluster <= 0 Then
                Throw New SystemException(NoMember)
            End If

            If CheckParallel(ncols:=dataWidth, clusters.NumOfCluster) Then
                Dim min As SeqValue(Of Double)()
                Dim index As Integer

                ' Kmeans并行算法
                For Each x As T In data
                    min = ParallelEuclideanDistance(clusters, x).ToArray
                    ' 升序排序就可以得到距离最小的cluster的distance，最后取出下标值
                    index = min _
                        .OrderBy(Function(distance) distance.value) _
                        .First.i

                    Call newClusters(index).Add(x)
                Next
            Else
                '((20+30)/2), ((170+160)/2), ((80+120)/2)
                For Each x As T In data
                    Call newClusters(minIndex(clusters, x)).Add(x)
                Next
            End If

            Return newClusters
        End Function

        Private Function CheckParallel(ncols As Integer, k As Integer) As Boolean
            If n_threads <= 1 Then
                Return False
            End If

            If auto_parallel Then
                If ncols <= 6 Then
                    Return False
                ElseIf k <= 30 Then
                    Return False
                Else
                    Return True
                End If
            Else
                Return True
            End If
        End Function

        Private Function ParallelEuclideanDistance(clusters As ClusterCollection(Of T), x As T) As IEnumerable(Of SeqValue(Of Double))
            Dim width As Integer = x.Length

            Return From c As SeqValue(Of KMeansCluster(Of T))
                   In clusters.SeqIterator _
                       .AsParallel _
                       .WithDegreeOfParallelism(n_threads)
                   Let cluster As KMeansCluster(Of T) = c.value
                   Let clusterMean As Double() = means(cluster, width)
                   Let distance As Double = x.entityVector.EuclideanDistance(clusterMean) ' 计算出当前的cluster和当前的实体对象之间的距离
                   Select New SeqValue(Of Double) With {
                       .i = c.i,
                       .value = distance
                   }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function means(cluster As KMeansCluster(Of T), width As Integer) As Double()
            Return If(cluster.NumOfEntity = 0, New Double(width - 1) {}, cluster.ClusterMean)
        End Function

        ''' <summary>
        ''' find index for non parallel code
        ''' </summary>
        ''' <param name="clusters"></param>
        ''' <param name="dataPoint"></param>
        ''' <returns></returns>
        Private Shared Function minIndex(clusters As ClusterCollection(Of T), dataPoint As T) As Integer
            Dim position As Integer = 0
            Dim clusterMean As Double()
            Dim firstClusterDistance As Double = 0.0
            Dim secondClusterDistance As Double = 0.0

            For cluster As Integer = 0 To clusters.NumOfCluster - 1
                Dim x As KMeansCluster(Of T) = clusters(cluster)

                If x.NumOfEntity = 0 Then
                    clusterMean = New Double(dataPoint.Length - 1) {}
                Else
                    clusterMean = x.ClusterMean
                End If

                If cluster = 0 Then
                    firstClusterDistance = EuclideanDistance(dataPoint.entityVector, clusterMean)
                    position = cluster
                Else
                    secondClusterDistance = EuclideanDistance(dataPoint.entityVector, clusterMean)

                    ' 相比前一个cluster的计算结果，在这里有一个更好的计算结果，则使用这个对象的下标
                    If firstClusterDistance > secondClusterDistance Then
                        firstClusterDistance = secondClusterDistance
                        ' 得到某个聚类和当前的数据对象距离最小的下标
                        position = cluster
                    End If
                End If
            Next

            Return position
        End Function
    End Class
End Namespace
