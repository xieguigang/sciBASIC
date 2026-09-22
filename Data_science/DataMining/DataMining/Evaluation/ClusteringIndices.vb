#Region "Microsoft.VisualBasic::7b34ec856f484b1f237e2b27307f9856, Data_science\DataMining\DataMining\Evaluation\ClusteringIndices.vb"

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

'   Total Lines: 504
'    Code Lines: 340 (67.46%)
' Comment Lines: 43 (8.53%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 121 (24.01%)
'     File Size: 18.32 KB


'  
' 
'     Function: AdjustedRandIndex, CalinskiHarabasz, Combinations, ContingencyTable, DaviesBouldin
'               Dunn, Entropy, MaximumDiameter, NormalizedMutualInformation, Purity
'               Silhouette
' 
' 
' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Evaluation

    ''' <summary>
    ''' 聚类质量指标的**唯一实现**。
    ''' 
    ''' 输入统一为「行主序特征矩阵 + 整数簇标签」，不依赖任何具体的聚类算法模型
    ''' （避免耦合已经标记为 ``Obsolete`` 的 ``ClusterEntity`` / ``KMeans.Bisecting.Cluster``）。
    ''' 
    ''' 提供两类指标：
    ''' 
    ''' + **内部指标**（不需要真值标签）：<see cref="Silhouette"/>、<see cref="Dunn"/>、
    '''   <see cref="DaviesBouldin"/>、<see cref="CalinskiHarabasz"/>、<see cref="MaximumDiameter"/>
    ''' + **外部指标**（需要真值标签）：<see cref="Purity"/>、<see cref="AdjustedRandIndex"/>、
    '''   <see cref="NormalizedMutualInformation"/>
    ''' </summary>
    Public Module ClusteringIndices

#Region "distance && grouping helpers"

        ''' <summary>
        ''' 欧氏距离。
        ''' </summary>
        Public Function Distance(a As Double(), b As Double()) As Double
            Dim sum As Double = 0
            Dim n As Integer = std.Min(a.Length, b.Length)

            For i As Integer = 0 To n - 1
                Dim d As Double = a(i) - b(i)
                sum += d * d
            Next

            Return std.Sqrt(sum)
        End Function

        ''' <summary>
        ''' 计算每一个簇的质心。
        ''' </summary>
        Public Function Centroids(features As Double()(), labels As Integer()) As Dictionary(Of Integer, Double())
            Dim groups As Dictionary(Of Integer, List(Of Integer)) = GroupIndices(labels)
            Dim result As New Dictionary(Of Integer, Double())()

            For Each cluster As KeyValuePair(Of Integer, List(Of Integer)) In groups
                Dim dims As Integer = features(cluster.Value(0)).Length
                Dim centroid As Double() = New Double(dims - 1) {}

                For Each index As Integer In cluster.Value
                    For j As Integer = 0 To dims - 1
                        centroid(j) += features(index)(j)
                    Next
                Next

                For j As Integer = 0 To dims - 1
                    centroid(j) /= cluster.Value.Count
                Next

                result(cluster.Key) = centroid
            Next

            Return result
        End Function

        ''' <summary>
        ''' 把簇标签转换为「簇编号 → 样本下标列表」的分组。
        ''' </summary>
        Public Function GroupIndices(labels As Integer()) As Dictionary(Of Integer, List(Of Integer))
            Dim groups As New Dictionary(Of Integer, List(Of Integer))()

            For i As Integer = 0 To labels.Length - 1
                Dim key As Integer = labels(i)

                If Not groups.ContainsKey(key) Then
                    groups(key) = New List(Of Integer)()
                End If

                groups(key).Add(i)
            Next

            Return groups
        End Function

        ''' <summary>
        ''' 当样本数量过大时，返回等间距抽样之后的下标序列（用于 O(n^2) 指标的近似计算）。
        ''' </summary>
        Private Function SampleIndices(n As Integer, maxPoints As Integer) As Integer()
            If maxPoints <= 0 OrElse n <= maxPoints Then
                Return Enumerable.Range(0, n).ToArray
            End If

            Dim stride As Double = n / maxPoints
            Dim result As New List(Of Integer)()

            For i As Integer = 0 To maxPoints - 1
                Dim index As Integer = CInt(std.Floor(i * stride))

                If index < n Then
                    result.Add(index)
                End If
            Next

            Return result.Distinct().OrderBy(Function(i) i).ToArray
        End Function

#End Region

#Region "internal indices"

        ''' <summary>
        ''' Silhouette 系数（标准定义）：逐样本 ``s(i) = (b - a) / max(a, b)`` 之后取均值，
        ''' 其中 ``a`` 为到同簇其它点的平均距离，``b`` 为到最近其它簇的平均距离。
        ''' </summary>
        ''' <param name="features"></param>
        ''' <param name="labels"></param>
        ''' <param name="maxPoints">
        ''' 最大参与计算的样本数量（``0`` 表示不限制）；超过时会等间距抽样近似。
        ''' </param>
        ''' <returns>``[-1, 1]`` 区间内的系数；簇数量小于 2 时返回 ``0``。</returns>
        Public Function Silhouette(features As Double()(), labels As Integer(), Optional maxPoints As Integer = 0) As Double
            If features Is Nothing OrElse labels Is Nothing OrElse features.Length <> labels.Length OrElse features.Length = 0 Then
                Return 0
            End If

            Dim groups As Dictionary(Of Integer, List(Of Integer)) = GroupIndices(labels)

            If groups.Count < 2 Then
                Return 0
            End If

            Dim samples As Integer() = SampleIndices(features.Length, maxPoints)
            Dim total As Double = 0

            For Each i As Integer In samples
                Dim own As List(Of Integer) = groups(labels(i))
                Dim a As Double = 0

                If own.Count > 1 Then
                    For Each j As Integer In own
                        If j <> i Then
                            a += Distance(features(i), features(j))
                        End If
                    Next

                    a /= (own.Count - 1)
                End If

                Dim b As Double = Double.MaxValue
                Dim hasOther As Boolean = False

                For Each cluster As KeyValuePair(Of Integer, List(Of Integer)) In groups
                    If cluster.Key = labels(i) Then
                        Continue For
                    End If

                    hasOther = True

                    Dim mean As Double = 0

                    For Each j As Integer In cluster.Value
                        mean += Distance(features(i), features(j))
                    Next

                    mean /= cluster.Value.Count

                    If mean < b Then
                        b = mean
                    End If
                Next

                If Not hasOther Then
                    Continue For
                End If

                Dim denominator As Double = std.Max(a, b)

                If denominator = 0 Then
                    Continue For
                End If

                total += (b - a) / denominator
            Next

            If samples.Length = 0 Then
                Return 0
            End If

            Return total / samples.Length
        End Function

        ''' <summary>
        ''' 最大簇内直径：任意属于同一个簇的两个样本之间的最大距离。
        ''' </summary>
        Public Function MaximumDiameter(features As Double()(), labels As Integer()) As Double
            If features Is Nothing OrElse labels Is Nothing OrElse features.Length <> labels.Length Then
                Return 0
            End If

            Dim groups As Dictionary(Of Integer, List(Of Integer)) = GroupIndices(labels)
            Dim maximum As Double = 0

            For Each cluster As KeyValuePair(Of Integer, List(Of Integer)) In groups
                For a As Integer = 0 To cluster.Value.Count - 1
                    For b As Integer = a + 1 To cluster.Value.Count - 1
                        Dim d As Double = Distance(features(cluster.Value(a)), features(cluster.Value(b)))

                        If d > maximum Then
                            maximum = d
                        End If
                    Next
                Next
            Next

            Return maximum
        End Function

        ''' <summary>
        ''' Dunn 指数：``最小簇间距离 / 最大簇内直径``，数值越大聚类效果越好。
        ''' </summary>
        Public Function Dunn(features As Double()(), labels As Integer()) As Double
            If features Is Nothing OrElse labels Is Nothing OrElse features.Length <> labels.Length Then
                Return 0
            End If

            Dim groups As Dictionary(Of Integer, List(Of Integer)) = GroupIndices(labels)

            If groups.Count < 2 Then
                Return 0
            End If

            Dim minOut As Double = Double.MaxValue
            Dim keys As Integer() = groups.Keys.ToArray

            For a As Integer = 0 To keys.Length - 1
                For b As Integer = a + 1 To keys.Length - 1
                    For Each i As Integer In groups(keys(a))
                        For Each j As Integer In groups(keys(b))
                            Dim d As Double = Distance(features(i), features(j))

                            If d < minOut Then
                                minOut = d
                            End If
                        Next
                    Next
                Next
            Next

            Dim maxIn As Double = MaximumDiameter(features, labels)

            If maxIn = 0 Then
                Return 0
            End If

            Return minOut / maxIn
        End Function

        ''' <summary>
        ''' Davies–Bouldin 指数：``mean_c max_{d != c} (S_c + S_d) / dist(centroid_c, centroid_d)``，
        ''' 其中 ``S_c`` 为簇内样本到质心的平均距离。数值越小聚类效果越好。
        ''' </summary>
        Public Function DaviesBouldin(features As Double()(), labels As Integer()) As Double
            If features Is Nothing OrElse labels Is Nothing OrElse features.Length <> labels.Length Then
                Return 0
            End If

            Dim groups As Dictionary(Of Integer, List(Of Integer)) = GroupIndices(labels)

            If groups.Count < 2 Then
                Return 0
            End If

            Dim clusterCentroids As Dictionary(Of Integer, Double()) = Centroids(features, labels)
            Dim scatter As New Dictionary(Of Integer, Double)()
            Dim keys As Integer() = groups.Keys.ToArray

            For Each key As Integer In keys
                Dim sum As Double = 0

                For Each index As Integer In groups(key)
                    sum += Distance(features(index), clusterCentroids(key))
                Next

                scatter(key) = sum / groups(key).Count
            Next

            Dim total As Double = 0

            For Each key As Integer In keys
                Dim maxRatio As Double = 0

                For Each other As Integer In keys
                    If other = key Then
                        Continue For
                    End If

                    Dim separation As Double = Distance(clusterCentroids(key), clusterCentroids(other))

                    If separation = 0 Then
                        Continue For
                    End If

                    Dim ratio As Double = (scatter(key) + scatter(other)) / separation

                    If ratio > maxRatio Then
                        maxRatio = ratio
                    End If
                Next

                total += maxRatio
            Next

            Return total / keys.Length
        End Function

        ''' <summary>
        ''' Calinski–Harabasz 指数：``(SSB / (k - 1)) / (SSW / (n - k))``，
        ''' 数值越大聚类效果越好。
        ''' </summary>
        Public Function CalinskiHarabasz(features As Double()(), labels As Integer()) As Double
            If features Is Nothing OrElse labels Is Nothing OrElse features.Length <> labels.Length OrElse features.Length = 0 Then
                Return 0
            End If

            Dim groups As Dictionary(Of Integer, List(Of Integer)) = GroupIndices(labels)
            Dim k As Integer = groups.Count
            Dim n As Integer = features.Length

            If k < 2 OrElse n <= k Then
                Return 0
            End If

            Dim dims As Integer = features(0).Length
            Dim overall As Double() = New Double(dims - 1) {}

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To dims - 1
                    overall(j) += features(i)(j)
                Next
            Next

            For j As Integer = 0 To dims - 1
                overall(j) /= n
            Next

            Dim clusterCentroids As Dictionary(Of Integer, Double()) = Centroids(features, labels)
            Dim ssb As Double = 0
            Dim ssw As Double = 0

            For Each cluster As KeyValuePair(Of Integer, List(Of Integer)) In groups
                Dim centroid As Double() = clusterCentroids(cluster.Key)
                Dim between As Double = 0

                For j As Integer = 0 To dims - 1
                    Dim d As Double = centroid(j) - overall(j)
                    between += d * d
                Next

                ssb += cluster.Value.Count * between

                For Each index As Integer In cluster.Value
                    Dim within As Double = 0

                    For j As Integer = 0 To dims - 1
                        Dim d As Double = features(index)(j) - centroid(j)
                        within += d * d
                    Next

                    ssw += within
                Next
            Next

            If ssw = 0 Then
                Return 0
            End If

            Return (ssb / (k - 1)) / (ssw / (n - k))
        End Function

#End Region

#Region "external indices"

        ''' <summary>
        ''' 构建「簇标签 × 真值标签」的列联表 ``n(cluster)(truth)``。
        ''' </summary>
        ''' <returns>
        ''' ``clusters`` 与 ``truths`` 为去重后的标签序列，``table(i)(j)`` 为同时落入
        ''' 第 i 个簇与第 j 个真值类别的样本数量。
        ''' </returns>
        Public Function ContingencyTable(cluster As Integer(), truth As Integer()) As (clusters As Integer(), truths As Integer(), table As Integer()())
            If cluster Is Nothing OrElse truth Is Nothing OrElse cluster.Length <> truth.Length Then
                Throw New DataMisalignedException("the cluster and truth label vectors must have the same length!")
            End If

            Dim clusters As Integer() = cluster.Distinct().OrderBy(Function(i) i).ToArray
            Dim truths As Integer() = truth.Distinct().OrderBy(Function(i) i).ToArray
            Dim clusterIndex As Dictionary(Of Integer, Integer) = clusters _
                .Select(Function(v, i) (v, i)) _
                .ToDictionary(Function(t) t.v, Function(t) t.i)
            Dim truthIndex As Dictionary(Of Integer, Integer) = truths _
                .Select(Function(v, i) (v, i)) _
                .ToDictionary(Function(t) t.v, Function(t) t.i)

            Dim table As Integer()() = New Integer(clusters.Length - 1)() {}

            For i As Integer = 0 To clusters.Length - 1
                table(i) = New Integer(truths.Length - 1) {}
            Next

            For i As Integer = 0 To cluster.Length - 1
                table(clusterIndex(cluster(i)))(truthIndex(truth(i))) += 1
            Next

            Return (clusters, truths, table)
        End Function

        ''' <summary>
        ''' Purity（纯度）：``Σ_k max_j |C_k ∩ T_j| / n``，数值越大聚类效果越好。
        ''' </summary>
        Public Function Purity(cluster As Integer(), truth As Integer()) As Double
            If cluster Is Nothing OrElse truth Is Nothing OrElse cluster.Length = 0 Then
                Return 0
            End If

            Dim result = ContingencyTable(cluster, truth)
            Dim total As Double = 0

            For i As Integer = 0 To result.clusters.Length - 1
                Dim maximum As Integer = 0

                For j As Integer = 0 To result.truths.Length - 1
                    If result.table(i)(j) > maximum Then
                        maximum = result.table(i)(j)
                    End If
                Next

                total += maximum
            Next

            Return total / cluster.Length
        End Function

        ''' <summary>
        ''' 调整兰德指数（Adjusted Rand Index），取值 ``[-1, 1]``，``1`` 表示完全一致。
        ''' </summary>
        Public Function AdjustedRandIndex(cluster As Integer(), truth As Integer()) As Double
            If cluster Is Nothing OrElse truth Is Nothing OrElse cluster.Length = 0 Then
                Return 0
            End If

            Dim result = ContingencyTable(cluster, truth)
            Dim n As Integer = cluster.Length
            Dim index As Double = 0
            Dim rowSums As New List(Of Double)()
            Dim colSums As New List(Of Double)()

            For i As Integer = 0 To result.clusters.Length - 1
                Dim row As Double = 0

                For j As Integer = 0 To result.truths.Length - 1
                    index += Combinations(result.table(i)(j))
                    row += result.table(i)(j)
                Next

                rowSums.Add(row)
            Next

            For j As Integer = 0 To result.truths.Length - 1
                Dim col As Double = 0

                For i As Integer = 0 To result.clusters.Length - 1
                    col += result.table(i)(j)
                Next

                colSums.Add(col)
            Next

            Dim sumRow As Double = rowSums.Sum(Function(v) Combinations(v))
            Dim sumCol As Double = colSums.Sum(Function(v) Combinations(v))
            Dim totalPairs As Double = Combinations(n)

            If totalPairs = 0 Then
                Return 0
            End If

            Dim expected As Double = sumRow * sumCol / totalPairs
            Dim maximum As Double = (sumRow + sumCol) / 2
            Dim denominator As Double = maximum - expected

            If denominator = 0 Then
                Return 0
            End If

            Return (index - expected) / denominator
        End Function

        ''' <summary>
        ''' 标准化互信息（NMI）：``2 * I(U;V) / (H(U) + H(V))``，取值 ``[0, 1]``。
        ''' </summary>
        Public Function NormalizedMutualInformation(cluster As Integer(), truth As Integer()) As Double
            If cluster Is Nothing OrElse truth Is Nothing OrElse cluster.Length = 0 Then
                Return 0
            End If

            Dim result = ContingencyTable(cluster, truth)
            Dim n As Double = cluster.Length
            Dim mutual As Double = 0

            For i As Integer = 0 To result.clusters.Length - 1
                For j As Integer = 0 To result.truths.Length - 1
                    Dim nij As Double = result.table(i)(j)

                    If nij = 0 Then
                        Continue For
                    End If

                    Dim rowSum As Double = 0
                    Dim colSum As Double = 0

                    For k As Integer = 0 To result.truths.Length - 1
                        rowSum += result.table(i)(k)
                    Next

                    For k As Integer = 0 To result.clusters.Length - 1
                        colSum += result.table(k)(j)
                    Next

                    mutual += (nij / n) * std.Log(nij * n / (rowSum * colSum))
                Next
            Next

            Dim hCluster As Double = Entropy(result.table, result.truths.Length, result.clusters.Length, cluster:=True)
            Dim hTruth As Double = Entropy(result.table, result.truths.Length, result.clusters.Length, cluster:=False)
            Dim denominator As Double = hCluster + hTruth

            If denominator = 0 Then
                Return 0
            End If

            Return 2 * mutual / denominator
        End Function

        Private Function Entropy(table As Integer()(), truths As Integer, clusters As Integer, cluster As Boolean) As Double
            Dim n As Double = 0
            Dim h As Double = 0

            If cluster Then
                For i As Integer = 0 To clusters - 1
                    Dim sum As Double = 0

                    For j As Integer = 0 To truths - 1
                        sum += table(i)(j)
                    Next

                    n += sum
                Next

                For i As Integer = 0 To clusters - 1
                    Dim sum As Double = 0

                    For j As Integer = 0 To truths - 1
                        sum += table(i)(j)
                    Next

                    If sum > 0 Then
                        Dim p As Double = sum / n
                        h -= p * std.Log(p)
                    End If
                Next
            Else
                For j As Integer = 0 To truths - 1
                    Dim sum As Double = 0

                    For i As Integer = 0 To clusters - 1
                        sum += table(i)(j)
                    Next

                    n += sum
                Next

                For j As Integer = 0 To truths - 1
                    Dim sum As Double = 0

                    For i As Integer = 0 To clusters - 1
                        sum += table(i)(j)
                    Next

                    If sum > 0 Then
                        Dim p As Double = sum / n
                        h -= p * std.Log(p)
                    End If
                Next
            End If

            Return h
        End Function

        ''' <summary>
        ''' 组合数 ``C(n, 2) = n * (n - 1) / 2``。
        ''' </summary>
        Private Function Combinations(n As Double) As Double
            Return n * (n - 1) / 2
        End Function

#End Region

    End Module

End Namespace
