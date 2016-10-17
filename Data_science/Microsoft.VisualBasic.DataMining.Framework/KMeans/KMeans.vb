#Region "Microsoft.VisualBasic::b1c2b7816f4470e8c11263a8a4d3fc28, ..\visualbasic_App\Data_science\Microsoft.VisualBasic.DataMining.Framework\KMeans\KMeans.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Parallel.Linq

Namespace KMeans

    ''' <summary>
    ''' This class implement a KMeans clustering algorithm.(请注意，实体对象的属性必须要长度一致)
    ''' </summary>
    Public Module KMeansAlgorithm

        ''' <summary>
        ''' Calculates the Euclidean Distance Measure between two data points
        ''' </summary>
        ''' <param name="X">An array with the values of an object or datapoint</param>
        ''' <param name="Y">An array with the values of an object or datapoint</param>
        ''' <returns>Returns the Euclidean Distance Measure Between Points X and Points Y</returns>
        Public Function EuclideanDistance(X As Double(), Y As Double()) As Double
            Dim count As Integer = 0
            Dim sum As Double = 0.0

            If X.Length <> Y.Length Then
                Throw New ArgumentException(DimNotAgree)
            Else
                count = X.Length
            End If

            For i As Integer = 0 To count - 1
                sum = sum + Math.Pow(Math.Abs(X(i) - Y(i)), 2)
            Next

            Dim distance As Double = Math.Sqrt(sum)
            Return distance
        End Function

        Const DimNotAgree As String = "The number of elements in X must match the number of elements in Y!"

        ''' <summary>
        ''' Calculates the Manhattan Distance Measure between two data points
        ''' </summary>
        ''' <param name="X">An array with the values of an object or datapoint</param>
        ''' <param name="Y">An array with the values of an object or datapoint</param>
        ''' <returns>Returns the Manhattan Distance Measure Between Points X and Points Y</returns>
        Public Function ManhattanDistance(X As Double(), Y As Double()) As Double
            Dim count As Integer = 0
            Dim sum As Double = 0.0

            If X.Length <> Y.Length Then
                Dim ex As New ArgumentException(DimNotAgree)
                ex = New ArgumentException($"len(X):={X.Length}, len(y):={Y.Length}", ex)
                Throw ex
            Else
                count = X.Length
            End If

            For i As Integer = 0 To count - 1
                sum = sum + Math.Abs(X(i) - Y(i))
            Next

            Return sum
        End Function

        ''' <summary>
        ''' Calculates The Mean Of A Cluster OR The Cluster Center
        ''' </summary>
        ''' <param name="cluster">
        ''' A two-dimensional array containing a dataset of numeric values
        ''' </param>
        ''' <returns>
        ''' Returns an Array Defining A Data Point Representing The Cluster Mean or Centroid
        ''' </returns>
        Public Function ClusterMean(cluster As Double(,)) As Double()
            Dim rowCount As Integer = 0
            Dim fieldCount As Integer = 0
            Dim dataSum As Double(,)
            Dim centroid As Double()

            rowCount = cluster.GetUpperBound(0) + 1
            fieldCount = cluster.GetUpperBound(1) + 1
            dataSum = New Double(0, fieldCount - 1) {}
            centroid = New Double(fieldCount - 1) {}

            '((20+30)/2), ((170+160)/2), ((80+120)/2)
            For j As Integer = 0 To fieldCount - 1
                For i As Integer = 0 To rowCount - 1
                    dataSum(0, j) = dataSum(0, j) + cluster(i, j)
                Next

                centroid(j) = (dataSum(0, j) / rowCount)
            Next

            Return centroid
        End Function

        ''' <summary>
        ''' Seperates a dataset into clusters or groups with similar characteristics
        ''' </summary>
        ''' <param name="clusterCount">The number of clusters or groups to form</param>
        ''' <param name="source">An array containing data that will be clustered</param>
        ''' <returns>A collection of clusters of data</returns>
        ''' <param name="parallel">
        ''' 默认是使用并行化的计算代码以通过牺牲内存空间的代价来获取高性能的计算，非并行化的代码比较适合低内存的设备上面运行
        ''' </param>
        <Extension> Public Function ClusterDataSet(Of T As EntityBase(Of Double))(
                                           clusterCount As Integer,
                                                 source As IEnumerable(Of T),
                                           Optional debug As Boolean = False,
                                           Optional [stop] As Integer = -1,
                                           Optional parallel As Boolean = True) As ClusterCollection(Of T)

            Dim data As T() = source.ToArray
            Dim clusterNumber As Integer = 0
            Dim rowCount As Integer = data.Length
            Dim fieldCount As Integer = data(Scan0).Length
            Dim stableClustersCount As Integer = 0
            Dim iterationCount As Integer = 0
            Dim cluster As KMeansCluster(Of T) = Nothing
            Dim clusters As New ClusterCollection(Of T)
            Dim clusterNumbers As New List(Of Integer)
            Dim Random As New Random

            If clusterCount >= rowCount Then
                Dim msg As String =
                    $"[cluster.count:={clusterCount}] >= [source.length:={rowCount}], this will caused a dead loop!"
                Throw New Exception(msg)
            Else
                If debug Then
                    Call "Init assigned random clusters...".__DEBUG_ECHO
                End If
            End If

            While clusterNumbers.Count < clusterCount
                clusterNumber = Random.[Next](0, rowCount - 1)

                If Not clusterNumbers.Contains(clusterNumber) Then
                    cluster = New KMeansCluster(Of T)
                    clusterNumbers.Add(clusterNumber)
                    cluster.Add(data(clusterNumber))
                    clusters.Add(cluster)
                End If
            End While

            If [stop] <= 0 Then
                [stop] = clusterCount * rowCount
            End If
            If debug Then
                Call "Start kmeans clustering....".__DEBUG_ECHO
            End If
            If parallel Then
                Call $"Kmeans have {LQuerySchedule.CPU_NUMBER} CPU core for parallel computing.".__DEBUG_ECHO
            End If

            While stableClustersCount <> clusters.NumOfCluster
                Dim newClusters As ClusterCollection(Of T) = ClusterDataSet(clusters, data, parallel)

                stableClustersCount = 0

                For clusterIndex As Integer = 0 To clusters.NumOfCluster - 1
                    Dim x As KMeansCluster(Of T) = newClusters(clusterIndex)  ' 这一次迭代的聚类结果
                    Dim y As KMeansCluster(Of T) = clusters(clusterIndex)  ' 上一次迭代的结果

                    If x.NumOfEntity = 0 OrElse y.NumOfEntity = 0 Then

                        If debug Then
                            Call "If (x.NumOfEntity = 0 OrElse y.NumOfEntity = 0) Is True".__DEBUG_ECHO
                        End If

                        Continue For ' ??? 为什么有些聚类是0？？
                    End If

                    If (EuclideanDistance(x.ClusterMean, y.ClusterMean)) = 0 Then  ' 假若上一次的迭代结果和这一次迭代的结果一样，则距离是0，得到了一个稳定的聚类结果
                        stableClustersCount += 1
                    End If
                Next

                iterationCount += 1
                clusters = newClusters

                If iterationCount > [stop] Then ' 迭代的次数已经超过了最大的迭代次数了，则退出计算，否则可能会在这里出现死循环
                    Exit While
                Else
                    If debug Then
                        Call $"[{iterationCount}/{[stop]}] stableClustersCount <> clusters.NumOfCluster => {stableClustersCount} <> {clusters.NumOfCluster} = {stableClustersCount <> clusters.NumOfCluster}".__DEBUG_ECHO
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
        ''' <param name="parallel">是否采用并行算法</param>
        ''' <returns>A collection of clusters of data</returns>
        Public Function ClusterDataSet(Of T As EntityBase(Of Double))(clusters As ClusterCollection(Of T), data As T(), Optional parallel As Boolean = False) As ClusterCollection(Of T)
            Dim fieldCount As Integer = data(Scan0).Length
            Dim newClusters As New ClusterCollection(Of T)     ' create a new collection of clusters

            For count As Integer = 0 To clusters.NumOfCluster - 1
                Dim newCluster As New KMeansCluster(Of T)
                newClusters.Add(newCluster)
            Next

            If clusters.NumOfCluster <= 0 Then
                Throw New SystemException("Cluster count cannot be ZERO!")
            End If

            If parallel Then  ' Kmeans并行算法

                For Each x As T In data
                    Dim min = LinqAPI.Exec(Of SeqValue(Of Double)) <=
 _
                        From c As SeqValue(Of KMeansCluster(Of T))
                        In clusters.SeqIterator.AsParallel
                        Let cluster As KMeansCluster(Of T) = c.obj
                        Let clusterMean As Double() = If(
                            cluster.NumOfEntity = 0,
                            New Double(x.Properties.Length - 1) {},
                            cluster.ClusterMean)
                        Let distance As Double = EuclideanDistance(x.Properties, clusterMean) ' 计算出当前的cluster和当前的实体对象之间的距离
                        Select New SeqValue(Of Double) With {
                            .i = c.i,
                            .obj = distance
                        }
                    Dim index As Integer = min _
                        .OrderBy(Function(distance) distance.obj) _
                        .First.i ' 升序排序就可以得到距离最小的cluster的distance，最后取出下标值

                    Call newClusters(index).Add(x)
                Next
            Else
                '((20+30)/2), ((170+160)/2), ((80+120)/2)
                For Each x As T In data
                    Call newClusters(clusters.__minIndex(x)).Add(x)
                Next
            End If

            Return newClusters
        End Function

        <Extension>
        Private Function __minIndex(Of T As EntityBase(Of Double))(clusters As ClusterCollection(Of T), dataPoint As T) As Integer
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
                    firstClusterDistance = EuclideanDistance(dataPoint.Properties, clusterMean)
                    position = cluster
                Else
                    secondClusterDistance = EuclideanDistance(dataPoint.Properties, clusterMean)

                    If firstClusterDistance > secondClusterDistance Then ' 相比前一个cluster的计算结果，在这里有一个更好的计算结果，则使用这个对象的下标
                        firstClusterDistance = secondClusterDistance
                        position = cluster  ' 得到某个聚类和当前的数据对象距离最小的下标
                    End If
                End If
            Next

            Return position
        End Function

        ''' <summary>
        ''' Converts a System.Data.DataTable to a 2-dimensional array
        ''' </summary>
        ''' <param name="table">A System.Data.DataTable containing data to cluster</param>
        ''' <returns>A 2-dimensional array containing data to cluster</returns>
        <Extension> Public Function ToFloatMatrix(table As DataTable) As Double(,)
            Dim rowCount As Integer = table.Rows.Count
            Dim fieldCount As Integer = table.Columns.Count
            Dim dataPoints As Double(,)
            Dim fieldValue As Double = 0.0
            Dim row As DataRow

            dataPoints = New Double(rowCount - 1, fieldCount - 1) {}

            For rowPosition As Integer = 0 To rowCount - 1
                row = table.Rows(rowPosition)

                For fieldPosition As Integer = 0 To fieldCount - 1
                    Try
                        fieldValue = Double.Parse(row(fieldPosition).ToString())
                    Catch ex As System.Exception
                        Dim msg As String = $"Invalid row at {rowPosition.ToString()} and field {fieldPosition.ToString()}"
                        ex = New InvalidCastException(msg, ex)
                        ex.PrintException

                        Throw ex
                    End Try

                    dataPoints(rowPosition, fieldPosition) = fieldValue
                Next
            Next

            Return dataPoints
        End Function
    End Module
End Namespace
