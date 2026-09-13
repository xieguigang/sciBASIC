#Region "Microsoft.VisualBasic::ClusteringTableExtensions, Data_science\DataMining\DataMining\Clustering\ClusteringTableExtensions.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.DataMining.ComponentModel.EntityModels
Imports Microsoft.VisualBasic.DataMining.HDBSCAN.Distance
Imports Microsoft.VisualBasic.DataMining.HDBSCAN.Runner
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Bisecting = Microsoft.VisualBasic.DataMining.KMeans.Bisecting
Imports LloydsNS = Microsoft.VisualBasic.DataMining.Lloyds
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace Clustering

    ''' <summary>
    ''' 统一的二维表聚类扩展入口集合。
    ''' 
    ''' 这里的每一个扩展方法都以经过预处理之后的纯数值二维表<see cref="NumericTable"/>
    ''' 作为数据输入，并且将聚类结果写入标签矩阵，最终返回写入结果之后的原表对象。
    ''' </summary>
    <HideModuleName>
    Public Module ClusteringTableExtensions

#Region "DBSCAN"

        ''' <summary>
        ''' 基于密度的 DBSCAN 聚类，结果写入 ``cluster`` 标签列（噪声点标记为 0）。
        ''' </summary>
        <Extension>
        Public Function dbscan(source As NumericTable,
                               epsilon As Double,
                               minPts As Integer,
                               Optional filterNoise As Boolean = True) As NumericTable

            Dim rows = source.NumericRows()
            Dim ids = source.RowNamesOrDefault()
            Dim points = rows _
                .SeqIterator _
                .Select(Function(r) New NumericRow(ids(r.i), r.value, r.i)) _
                .ToArray
            Dim algorithm As New DBSCAN.DbscanAlgorithm(Of NumericRow)(
                metricFunc:=Function(x, y) DistanceMethods.EuclideanDistance(x.data, y.data)
            )
            Dim clusterSets = algorithm.ComputeClusterDBSCAN(
                allPoints:=points,
                epsilon:=epsilon,
                minPts:=minPts,
                filterNoise:=filterNoise
            )
            Dim labels As Integer() = New Integer(rows.Length - 1) {}
            Dim class_id As Integer = 0

            For Each cluster In clusterSets
                class_id += 1

                For Each p As NumericRow In cluster.value
                    labels(p.index) = class_id
                Next
            Next

            Return source.SetLabel("cluster", labels)
        End Function

#End Region

#Region "HDBSCAN"

        ''' <summary>
        ''' 层次密度聚类 HDBSCAN，结果写入 ``cluster`` 标签列。
        ''' </summary>
        <Extension>
        Public Function hdbscan(source As NumericTable,
                                minPoints As Integer,
                                minClusterSize As Integer,
                                Optional distance As IDistanceCalculator(Of Double()) = Nothing) As NumericTable

            Dim parameters As New HdbscanParameters(Of Double()) With {
                .DataSet = source.NumericRows(),
                .DistanceFunction = If(distance, New EuclideanDistance()),
                .MinPoints = minPoints,
                .MinClusterSize = minClusterSize,
                .MaxDegreeOfParallelism = 0
            }
            Dim result As HdbscanResult = HdbscanRunner.Run(parameters)

            Return source.SetLabel("cluster", result.Labels)
        End Function

#End Region

#Region "Spectral"

        ''' <summary>
        ''' 谱聚类，结果写入 ``cluster`` 标签列。
        ''' </summary>
        <Extension>
        Public Function spectralCluster(source As NumericTable, k As Integer) As NumericTable
            Dim matrix As New NumericMatrix(source.NumericRows())
            Dim spectral As New Spectral(matrix)

            Call spectral.set_centers(k)

            Return source.SetLabel("cluster", spectral.cluster())
        End Function

#End Region

#Region "Bisecting KMeans"

        ''' <summary>
        ''' 二分 KMeans 聚类，结果写入 ``cluster`` 标签列。
        ''' </summary>
        <Extension>
        Public Function bisectingKMeans(source As NumericTable,
                                        Optional k As Integer = 6,
                                        Optional iterations As Integer = 6,
                                        Optional n_threads As Integer = 16) As NumericTable

            Dim entities = ToIndexedEntities(source)
            Dim algorithm As New Bisecting.BisectingKMeans(entities, k, iterations, n_threads:=n_threads)
            Dim labels As Integer() = New Integer(source.nsamples - 1) {}
            Dim class_id As Integer = 0

            For Each cluster As Bisecting.Cluster In algorithm.runBisectingKMeans()
                class_id += 1

                For Each p As ClusterEntity In cluster.DataPoints
                    labels(Integer.Parse(p.uid)) = class_id
                Next
            Next

            Return source.SetLabel("cluster", labels)
        End Function

#End Region

#Region "Lloyds"

        ''' <summary>
        ''' Lloyd 算法（Voronoi 迭代）聚类，结果写入 ``cluster`` 标签列。
        ''' </summary>
        <Extension>
        Public Function lloydsCluster(source As NumericTable, k As Integer) As NumericTable
            Dim points = source _
                .NumericRows() _
                .Select(Function(row) New LloydsNS.Point(row)) _
                .ToArray
            Dim algorithm As New LloydsNS.LloydsMethodClustering(points, k)

            Call algorithm.Clustering()

            Dim labels As Integer() = points _
                .Select(Function(p) p.cluster) _
                .ToArray

            Return source.SetLabel("cluster", labels)
        End Function

#End Region

#Region "Canopy"

        ''' <summary>
        ''' Canopy 粗聚类，结果写入 ``cluster`` 标签列。
        ''' </summary>
        <Extension>
        Public Function canopyCluster(source As NumericTable,
                                      Optional t1 As Double = Double.NaN,
                                      Optional t2 As Double = Double.NaN) As NumericTable

            Dim entities = ToNamedEntities(source)
            Dim builder As CanopyBuilder

            If Double.IsNaN(t1) OrElse Double.IsNaN(t2) Then
                builder = New CanopyBuilder(entities)
            Else
                builder = New CanopyBuilder(entities, t1, t2)
            End If

            Dim canopies = builder.Solve()
            Dim rows = source.NumericRows()
            Dim labels As Integer() = New Integer(rows.Length - 1) {}

            For i As Integer = 0 To rows.Length - 1
                Dim minDistance As Double = Double.MaxValue
                Dim best As Integer = 0

                For c As Integer = 0 To canopies.Length - 1
                    Dim distance As Double = DistanceMethods.EuclideanDistance(rows(i), canopies(c).centroid)

                    If distance < minDistance Then
                        minDistance = distance
                        best = c
                    End If
                Next

                labels(i) = best + 1
            Next

            Return source.SetLabel("cluster", labels)
        End Function

#End Region

#Region "Density"

        ''' <summary>
        ''' 计算每一个样本的局部密度（到 k 个最近邻平均距离的倒数），写入 ``density`` 标签列。
        ''' </summary>
        <Extension>
        Public Function densityScore(source As NumericTable, Optional k As Integer = 6) As NumericTable
            Dim rows = source.NumericRows()
            Dim points = rows _
                .SeqIterator _
                .Select(Function(r) New NumericRow(r.i.ToString, r.value, r.i)) _
                .ToArray
            Dim result = Density.GetDensity(Of NumericRow)(
                dataset:=points,
                metric:=Function(x, y) DistanceMethods.EuclideanDistance(x.data, y.data),
                k:=k
            )
            Dim labels As Double() = New Double(rows.Length - 1) {}

            For Each item As NamedValue(Of Double) In result
                labels(Integer.Parse(item.Name)) = item.Value
            Next

            Return source.SetLabel("density", labels)
        End Function

#End Region

#Region "KNN"

        ''' <summary>
        ''' KNN 有监督分类：以 <paramref name="train"/> 表中的 ``cluster`` 标签列作为类别，
        ''' 对 <paramref name="test"/> 表进行分类，预测结果写入 ``knn`` 标签列。
        ''' </summary>
        <Extension>
        Public Function knnClassify(train As NumericTable, test As NumericTable, k As Integer) As NumericTable
            Dim label As Double() = train.GetLabel("cluster")
            Dim entities = ToIndexedEntities(train)

            For i As Integer = 0 To entities.Length - 1
                entities(i).cluster = CInt(label(i))
            Next

            Dim classNames As Dictionary(Of Integer, String) = label _
                .Select(Function(d) CInt(d)) _
                .Distinct() _
                .ToDictionary(Function(c) c, Function(c) c.ToString)
            Dim classifier As New KNN(entities, classNames)
            Dim testNames As String() = test.RowNamesOrDefault()
            Dim nameIndex As Dictionary(Of String, Integer) = testNames _
                .SeqIterator _
                .ToDictionary(Function(r) r.value, Function(r) r.i)
            Dim testSet As NamedCollection(Of Double)() = testNames _
                .SeqIterator _
                .Select(Function(r) New NamedCollection(Of Double)(r.value, test(r.i))) _
                .ToArray
            Dim predicted As Integer() = New Integer(test.nsamples - 1) {}

            For Each row As NamedCollection(Of NamedValue(Of Integer)) In classifier.Classify(testSet, k)
                Dim index As Integer = nameIndex(row.name)
                Dim win = row _
                    .value _
                    .GroupBy(Function(v) v.Value) _
                    .OrderByDescending(Function(g) g.Count()) _
                    .First()

                predicted(index) = win.Key
            Next

            Return test.SetLabel("knn", predicted)
        End Function

#End Region

#Region "KNN Cluster"

        ''' <summary>
        ''' KNN 聚类（基于近邻密度扩展），结果写入 ``cluster`` 标签列。
        ''' </summary>
        <Extension>
        Public Function knnCluster(source As NumericTable,
                                   Optional k As Integer = 32,
                                   Optional p As Double = 0.8) As NumericTable

            Dim entities = ToIndexedEntities(source)
            Dim labels As Integer() = New Integer(entities.Length - 1) {}

            For Each entity As ClusterEntity In entities.MakeKNNCluster(k, p)
                labels(Integer.Parse(entity.uid)) = entity.cluster
            Next

            Return source.SetLabel("cluster", labels)
        End Function

#End Region

#Region "KMedoids"

        ''' <summary>
        ''' 围绕中心点划分算法 PAM/KMedoids，结果写入 ``cluster`` 标签列。
        ''' </summary>
        <Extension>
        Public Function kmedoids(source As NumericTable, k As Integer, Optional maxSteps As Integer = 1000) As NumericTable
            Dim labels As Integer() = runKMedoids(source.NumericRows(), k, maxSteps)

            Return source.SetLabel("cluster", labels)
        End Function

        Private Function runKMedoids(points As Double()(), k As Integer, maxSteps As Integer) As Integer()
            Dim n As Integer = points.Length

            If k > n OrElse k < 1 Then
                Throw New Exception("K must be between 0 and set size")
            End If

            Dim medoids As Integer() = New Integer(k - 1) {}
            Dim picked As New List(Of Integer)

            For i As Integer = 0 To k - 1
                Dim r As Integer

                Do
                    r = randf.NextInteger(n)
                Loop While picked.Contains(r)

                medoids(i) = r
                Call picked.Add(r)
            Next

            Dim bestAssign As Integer() = Nothing
            Dim bestCost As Double = Double.MaxValue
            Dim assign As Integer() = New Integer(n - 1) {}

            For [step] As Integer = 0 To maxSteps
                Dim cost As Double = 0

                For i As Integer = 0 To n - 1
                    Dim minDistance As Double = Double.MaxValue
                    Dim best As Integer = 0

                    For c As Integer = 0 To k - 1
                        Dim distance As Double = points(i).EuclideanDistance(points(medoids(c)))

                        If distance < minDistance Then
                            minDistance = distance
                            best = c
                        End If
                    Next

                    assign(i) = best
                    cost += minDistance
                Next

                If cost < bestCost Then
                    bestCost = cost
                    bestAssign = DirectCast(assign.Clone(), Integer())
                End If

                Dim swapCost As Integer() = New Integer(k - 1) {}
                Dim candidate As Integer() = New Integer(k - 1) {}

                For i As Integer = 0 To k - 1
                    candidate(i) = -1
                Next

                For i As Integer = 0 To n - 1
                    Dim randomValue As Integer = randf.NextInteger(Integer.MaxValue)
                    Dim c As Integer = assign(i)

                    If swapCost(c) < randomValue AndAlso Not picked.Contains(i) Then
                        candidate(c) = i
                        swapCost(c) = randomValue
                    End If
                Next

                For i As Integer = 0 To k - 1
                    If candidate(i) >= 0 Then
                        medoids(i) = candidate(i)
                    End If
                Next
            Next

            Dim labels As Integer() = New Integer(n - 1) {}

            For i As Integer = 0 To n - 1
                labels(i) = bestAssign(i) + 1
            Next

            Return labels
        End Function

#End Region

#Region "Evaluation"

        ''' <summary>
        ''' Silhouette 轮廓系数，基于特征矩阵与 ``cluster`` 标签列计算。
        ''' </summary>
        <Extension>
        Public Function silhouette(source As NumericTable) As Double
            Dim rows = source.NumericRows()
            Dim cluster = source.GetLabel("cluster")
            Dim n As Integer = rows.Length

            If n <= 1 Then
                Return 0
            End If

            Dim sum As Double = 0

            For i As Integer = 0 To n - 1
                Dim same As New List(Of Integer)
                Dim others As New Dictionary(Of Integer, List(Of Integer))

                For j As Integer = 0 To n - 1
                    If j = i Then
                        Continue For
                    ElseIf cluster(j) = cluster(i) Then
                        Call same.Add(j)
                    Else
                        Dim key As Integer = CInt(cluster(j))

                        If Not others.ContainsKey(key) Then
                            others(key) = New List(Of Integer)
                        End If

                        Call others(key).Add(j)
                    End If
                Next

                Dim rowIndex As Integer = i
                Dim a As Double = If(same.Count = 0, 0.0, same.Average(Function(m) DistanceMethods.EuclideanDistance(rows(rowIndex), rows(m))))
                Dim b As Double = Double.MaxValue

                For Each members As List(Of Integer) In others.Values
                    Dim mean As Double = members.Average(Function(m) DistanceMethods.EuclideanDistance(rows(rowIndex), rows(m)))

                    If mean < b Then
                        b = mean
                    End If
                Next

                If b = Double.MaxValue Then
                    b = 0
                End If

                Dim denominator As Double = std.Max(a, b)

                If denominator > 0 Then
                    sum += (b - a) / denominator
                End If
            Next

            Return sum / n
        End Function

        ''' <summary>
        ''' Dunn 指数，基于特征矩阵与 ``cluster`` 标签列计算。
        ''' </summary>
        <Extension>
        Public Function dunn(source As NumericTable) As Double
            Dim rows = source.NumericRows()
            Dim cluster = source.GetLabel("cluster")
            Dim groups = rows _
                .SeqIterator _
                .GroupBy(Function(r) CInt(cluster(r.i))) _
                .ToArray

            If groups.Length < 2 Then
                Return 0
            End If

            Dim maxIn As Double = Double.MinValue
            Dim minOut As Double = Double.MaxValue

            For Each g In groups
                Dim members = g.Select(Function(x) x.i).ToArray

                For i As Integer = 0 To members.Length - 1
                    For j As Integer = i + 1 To members.Length - 1
                        Dim d As Double = DistanceMethods.EuclideanDistance(rows(members(i)), rows(members(j)))

                        If d > maxIn Then
                            maxIn = d
                        End If
                    Next
                Next
            Next

            For Each g1 In groups
                For Each g2 In groups
                    If g1.Key = g2.Key Then
                        Continue For
                    End If

                    For Each i As Integer In g1.Select(Function(x) x.i)
                        For Each j As Integer In g2.Select(Function(x) x.i)
                            Dim d As Double = DistanceMethods.EuclideanDistance(rows(i), rows(j))

                            If d < minOut Then
                                minOut = d
                            End If
                        Next
                    Next
                Next
            Next

            If maxIn <= 0 Then
                Return 0
            Else
                Return minOut / maxIn
            End If
        End Function

        ''' <summary>
        ''' Calinski-Harabasz 指数，基于特征矩阵与 ``cluster`` 标签列计算。
        ''' </summary>
        <Extension>
        Public Function calinskiHarabasz(source As NumericTable) As Double
            Dim rows = source.NumericRows()
            Dim cluster = source.GetLabel("cluster")
            Dim n As Integer = rows.Length
            Dim k As Integer = cluster.Distinct().Count()

            If k < 2 OrElse n <= k Then
                Return 0
            End If

            Dim globalMean As Double() = mean(rows)
            Dim ssb As Double = 0
            Dim ssw As Double = 0
            Dim groups = rows _
                .SeqIterator _
                .GroupBy(Function(r) CInt(cluster(r.i)))

            For Each g In groups
                Dim members = g.Select(Function(x) x.value).ToArray
                Dim center As Double() = mean(members)
                Dim between As Double = DistanceMethods.EuclideanDistance(center, globalMean)

                ssb += members.Length * (between * between)

                For Each m As Double() In members
                    Dim within As Double = DistanceMethods.EuclideanDistance(m, center)

                    ssw += within * within
                Next
            Next

            If ssw = 0 Then
                Return 0
            Else
                Return (ssb / (k - 1)) / (ssw / (n - k))
            End If
        End Function

        Private Function mean(rows As Double()()) As Double()
            If rows.Length = 0 Then
                Return New Double() {}
            End If

            Dim width As Integer = rows(Scan0).Length
            Dim sum As Double() = New Double(width - 1) {}

            For Each row As Double() In rows
                For i As Integer = 0 To width - 1
                    sum(i) = sum(i) + row(i)
                Next
            Next

            For i As Integer = 0 To width - 1
                sum(i) = sum(i) / rows.Length
            Next

            Return sum
        End Function

#End Region

#Region "Adapter"

        ''' <summary>
        ''' 将二维表的数值行转换为以行下标作为 ID 的实体对象集合（用于过渡期的算法适配）
        ''' </summary>
        <Extension>
        Public Function ToIndexedEntities(source As NumericTable) As ClusterEntity()
            Dim rows = source.NumericRows()
            Dim entities As ClusterEntity() = New ClusterEntity(rows.Length - 1) {}

            For i As Integer = 0 To rows.Length - 1
                entities(i) = New ClusterEntity With {
                    .uid = i.ToString,
                    .entityVector = rows(i)
                }
            Next

            Return entities
        End Function

        ''' <summary>
        ''' 将二维表的数值行转换为以行名作为 ID 的实体对象集合（用于过渡期的算法适配）
        ''' </summary>
        <Extension>
        Public Function ToNamedEntities(source As NumericTable) As ClusterEntity()
            Dim rows = source.NumericRows()
            Dim ids = source.RowNamesOrDefault()
            Dim entities As ClusterEntity() = New ClusterEntity(rows.Length - 1) {}

            For i As Integer = 0 To rows.Length - 1
                entities(i) = New ClusterEntity(ids(i), rows(i))
            Next

            Return entities
        End Function

#End Region

    End Module
End Namespace
