#Region "Microsoft.VisualBasic::c6e618c8dd0a9347bd22d54bc251e8a4, Data_science\DataMining\DataMining\Clustering\HDBSCAN\Runner\HdbscanRunner.vb"

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
    '    Code Lines: 114
    ' Comment Lines: 13
    '   Blank Lines: 33
    '     File Size: 10.08 KB


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
Imports Parallel0 = System.Threading.Tasks.Parallel
Imports stdNum = System.Math

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

            ' Normal matrix with caching.
            If parameters.CacheDistance Then Return Function(a, b) parameters.Distances(a)(b)

            ' No cache
            Return Function(a, b) parameters.DistanceFunction.ComputeDistance(a, b, parameters.DataSet(a), parameters.DataSet(b))
        End Function

        Private Shared Function PrecomputeSparseMatrixDistancesIfApplicable(Of T)(parameters As HdbscanParameters(Of T), numPoints As Integer) As Dictionary(Of Integer, Double)
            Dim sparseDistance As Dictionary(Of Integer, Double) = Nothing
            If parameters.Distances Is Nothing AndAlso parameters.CacheDistance AndAlso TypeOf parameters.DataSet Is Dictionary(Of Integer, Integer)() Then
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

                    Dim [option] = New ParallelOptions With {.MaxDegreeOfParallelism = stdNum.Max(1, maxDegreeOfParallelism)}

                    Call Parallel0.For(0, [option].MaxDegreeOfParallelism, [option], Sub(indexThread)
                                                                                         Dim distanceThread = New Dictionary(Of Integer, Double)()

                                                                                         For index = 0 To size - 1
                                                                                             If index Mod [option].MaxDegreeOfParallelism <> indexThread Then Continue For

                                                                                             Dim i = index Mod numPoints
                                                                                             Dim j = index / numPoints
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
            If parameters.Distances Is Nothing AndAlso parameters.CacheDistance AndAlso TypeOf parameters.DataSet Is Double()() Then
                Dim distances = New Double(numPoints - 1)() {}
                For i = 0 To distances.Length - 1
                    distances(i) = New Double(numPoints - 1) {}
                Next

                If parameters.MaxDegreeOfParallelism = 0 OrElse parameters.MaxDegreeOfParallelism > 1 Then
                    Dim size = numPoints * numPoints

                    Dim maxDegreeOfParallelism = parameters.MaxDegreeOfParallelism
                    If maxDegreeOfParallelism = 0 Then
                        ' Not specified. Use all threads.
                        maxDegreeOfParallelism = Environment.ProcessorCount
                    End If

                    Dim [option] = New ParallelOptions With {.MaxDegreeOfParallelism = stdNum.Max(1, maxDegreeOfParallelism)}

                    Parallel0.For(0, size, [option], Sub(index)
                                                         Dim i = index Mod numPoints
                                                         Dim j = index / numPoints
                                                         If i < j Then
                                                             Dim distance = parameters.DistanceFunction.ComputeDistance(i, j, parameters.DataSet(i), parameters.DataSet(j))
                                                             distances(i)(j) = distance
                                                             distances(j)(i) = distance
                                                         End If
                                                     End Sub)
                Else
                    For i = 0 To numPoints - 1
                        For j = 0 To i - 1
                            Dim distance = parameters.DistanceFunction.ComputeDistance(i, j, parameters.DataSet(i), parameters.DataSet(j))
                            distances(i)(j) = distance
                            distances(j)(i) = distance
                        Next
                    Next
                End If

                parameters.Distances = distances
            End If
        End Sub
    End Class
End Namespace
