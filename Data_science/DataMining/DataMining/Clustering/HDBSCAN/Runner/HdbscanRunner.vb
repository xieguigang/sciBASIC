#Region "Microsoft.VisualBasic::06a6a7802c8a0d98880f0414cd9ef0b9, Data_science\DataMining\DataMining\Clustering\HDBSCAN\Runner\HdbscanRunner.vb"

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

    '   Total Lines: 160
    '    Code Lines: 114 (71.25%)
    ' Comment Lines: 13 (8.12%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 33 (20.62%)
    '     File Size: 10.07 KB


    '     Class HdbscanRunner
    ' 
    '         Function: DetermineInternalDistanceFunc, PrecomputeSparseMatrixDistancesIfApplicable, Run
    ' 
    '         Sub: PrecomputeNormalMatrixDistancesIfApplicable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.HDBSCAN.Distance
Imports Microsoft.VisualBasic.DataMining.HDBSCAN.Hdbscanstar
Imports Microsoft.VisualBasic.Math.SIMD
Imports Parallel0 = System.Threading.Tasks.Parallel
Imports std = System.Math

Namespace HDBSCAN.Runner
    Public Class HdbscanRunner
        Public Shared Function Run(Of T)(parameters As HdbscanParameters(Of T)) As HdbscanResult
            Dim numPoints = If(parameters.DataSet?.Length, parameters.Distances.Length)

            PrecomputeNormalMatrixDistancesIfApplicable(parameters, numPoints)
            Dim sparseDistance = PrecomputeSparseMatrixDistancesIfApplicable(parameters, numPoints)
            Dim internalDistanceFunc = DetermineInternalDistanceFunc(parameters, sparseDistance, numPoints)

            ' Compute core distances
            Dim coreDistances = HdbscanAlgorithm.CalculateCoreDistances(internalDistanceFunc, numPoints, parameters.MinPoints)

            ' Calculate minimum spanning tree
            Dim mst = HdbscanAlgorithm.ConstructMst(internalDistanceFunc, numPoints, coreDistances, True)
            mst.QuicksortByEdgeWeight()

            Dim pointNoiseLevels = New Double(numPoints - 1) {}
            Dim pointLastClusters = New Integer(numPoints - 1) {}
            Dim hierarchy = New List(Of Integer())()

            ' Compute hierarchy and cluster tree
            Dim clusters = HdbscanAlgorithm.ComputeHierarchyAndClusterTree(mst, parameters.MinClusterSize, parameters.Constraints, hierarchy, pointNoiseLevels, pointLastClusters)

            ' Propagate clusters
            Dim infiniteStability = HdbscanAlgorithm.PropagateTree(clusters)

            ' Compute final flat partitioning
            Dim prominentClusters = HdbscanAlgorithm.FindProminentClusters(clusters, hierarchy, numPoints)

            ' Compute outlier scores for each point
            Dim scores = HdbscanAlgorithm.CalculateOutlierScores(clusters, pointNoiseLevels, pointLastClusters, coreDistances)

            Return New HdbscanResult With {
    .Labels = prominentClusters,
    .OutliersScore = scores,
    .HasInfiniteStability = infiniteStability
}
        End Function

        Private Shared Function DetermineInternalDistanceFunc(Of T)(parameters As HdbscanParameters(Of T), sparseDistance As IReadOnlyDictionary(Of Integer, Double), numPoints As Integer) As Func(Of Integer, Integer, Double)
            ' Sparse matrix with caching.
            If sparseDistance IsNot Nothing Then Return Function(a, b)
                                                            If a < b Then Return If(sparseDistance.ContainsKey(a * numPoints + b), sparseDistance(a * numPoints + b), 1)

                                                            Return If(sparseDistance.ContainsKey(b * numPoints + a), sparseDistance(b * numPoints + a), 1)
                                                        End Function

            ' Normal matrix with caching（要求矩阵确实已经物化，否则回退到按需计算）
            If parameters.Distances IsNot Nothing Then Return Function(a, b) parameters.Distances(a)(b)

            ' No cache
            Return Function(a, b) parameters.DistanceFunction.ComputeDistance(a, b, parameters.DataSet(a), parameters.DataSet(b))
        End Function

        Private Shared Function PrecomputeSparseMatrixDistancesIfApplicable(Of T)(parameters As HdbscanParameters(Of T), numPoints As Integer) As Dictionary(Of Integer, Double)
            Dim sparseDistance As Dictionary(Of Integer, Double) = Nothing

            ' 稀疏矩阵的缓存规模由"非默认值条目数"决定，而不是 n²，
            ' 因此不受 MaxCachedPoints / DistanceCacheMode 的限制（默认仍然启用，保持既有行为）。
            If parameters.Distances Is Nothing AndAlso TypeOf parameters.DataSet Is Dictionary(Of Integer, Integer)() Then
                sparseDistance = New Dictionary(Of Integer, Double)()
                If parameters.DistanceFunction.GetType() IsNot GetType(ISparseMatrixSupport) Then
                    Throw New NotSupportedException("The distance function used does not support sparse matrix.")
                End If

                Dim sparseMatrixSupport = CType(parameters.DistanceFunction, ISparseMatrixSupport)
                Dim mostCommonDistanceValueForSparseMatrix = sparseMatrixSupport.GetMostCommonDistanceValueForSparseMatrix()

                If parameters.MaxDegreeOfParallelism = 0 OrElse parameters.MaxDegreeOfParallelism > 1 Then
                    Dim size = numPoints * numPoints

                    Dim maxDegreeOfParallelism = parameters.MaxDegreeOfParallelism
                    If maxDegreeOfParallelism = 0 Then
                        ' Not specified. Use all threads.
                        maxDegreeOfParallelism = Environment.ProcessorCount
                    End If

                    Dim [option] = New ParallelOptions With {.MaxDegreeOfParallelism = std.Max(1, maxDegreeOfParallelism)}

                    Call Parallel0.For(0, [option].MaxDegreeOfParallelism, [option], Sub(indexThread)
                                                                                         Dim distanceThread = New Dictionary(Of Integer, Double)()

                                                                                         For index = 0 To size - 1
                                                                                             If index Mod [option].MaxDegreeOfParallelism <> indexThread Then Continue For

                                                                                             Dim i = index Mod numPoints
                                                                                             Dim j = index \ numPoints
                                                                                             If i >= j Then Continue For

                                                                                             Dim distance = parameters.DistanceFunction.ComputeDistance(i, j, parameters.DataSet(i), parameters.DataSet(j))

                                                                                             ' ReSharper disable once CompareOfFloatsByEqualityOperator
                                                                                             If distance <> mostCommonDistanceValueForSparseMatrix Then distanceThread.Add(i * numPoints + j, distance)
                                                                                         Next

                                                                                         SyncLock sparseDistance
                                                                                             For Each d In distanceThread
                                                                                                 sparseDistance.Add(d.Key, d.Value)
                                                                                             Next
                                                                                         End SyncLock
                                                                                     End Sub)
                Else
                    For i = 0 To numPoints - 1
                        For j = 0 To i - 1
                            Dim distance = parameters.DistanceFunction.ComputeDistance(i, j, parameters.DataSet(i), parameters.DataSet(j))

                            ' ReSharper disable once CompareOfFloatsByEqualityOperator
                            If distance <> mostCommonDistanceValueForSparseMatrix Then sparseDistance.Add(i * numPoints + j, distance)
                        Next
                    Next
                End If
            End If

            Return sparseDistance
        End Function

        Private Shared Sub PrecomputeNormalMatrixDistancesIfApplicable(Of T)(parameters As HdbscanParameters(Of T), numPoints As Integer)
            If parameters.Distances IsNot Nothing OrElse parameters.DistanceCacheMode = DistanceCacheMode.None Then
                Return
            End If
            If Not (TypeOf parameters.DataSet Is Double()()) Then
                Return
            End If

            ' 大样本下 n×n 物化会直接 OOM（n=20000 → 3.2 GB），显式要求缓存也要受上限保护
            If numPoints > parameters.MaxCachedPoints Then
                Call Microsoft.VisualBasic.VBDebugger.EchoLine($"HDBSCAN: {numPoints} points exceeds MaxCachedPoints({parameters.MaxCachedPoints}), distance cache is disabled.")

                parameters.DistanceCacheMode = DistanceCacheMode.None

                Return
            End If

            Dim dataSet As T() = parameters.DataSet
            Dim distances As Double()() = New Double(numPoints - 1)() {}

            For i As Integer = 0 To distances.Length - 1
                distances(i) = New Double(numPoints - 1) {}
            Next

            ' 按"行块"并行：每行只与 j > i 计算一次距离并对称写回，
            ' 相比旧实现（Parallel.For 遍历 n² 个下标、用 index Mod n / index \ n 反解行号）
            ' 少掉了整除运算与跨行随机写入，且距离计算量减半。
            Dim distanceFunc = parameters.DistanceFunction

            Call SimdParallel.ForRange(numPoints,
                Sub(start, ends)
                    For i As Integer = start To ends - 1
                        Dim row As Double() = distances(i)
                        Dim xi As T = dataSet(i)

                        For j As Integer = i + 1 To numPoints - 1
                            Dim d As Double = distanceFunc.ComputeDistance(i, j, xi, dataSet(j))

                            row(j) = d
                            distances(j)(i) = d
                        Next
                    Next
                End Sub, 8)

            parameters.Distances = distances
        End Sub
    End Class
End Namespace
