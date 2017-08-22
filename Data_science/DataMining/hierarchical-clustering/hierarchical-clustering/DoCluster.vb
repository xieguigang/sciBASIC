Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq

Public Module DoCluster

    <Extension>
    Public Function DistanceMatrix(objects As IEnumerable(Of DataSet)) As Double()()
        Dim list = objects.ToArray
        Dim keys = list _
            .Select(Function(obj) obj.Properties.Keys) _
            .IteratesALL _
            .Distinct _
            .ToArray

        Return list _
            .Select(Function(x)
                        Return list _
                            .Select(Function(y) x.EuclideanDistance(y, keys)) _
                            .ToArray
                    End Function) _
            .ToArray
    End Function

    <Extension>
    Public Function RunCluster(objects As IEnumerable(Of DataSet)) As Cluster
        Dim list = objects.ToArray
        Dim distances = list.DistanceMatrix
        Dim names$() = list.Keys
        Dim alg As ClusteringAlgorithm = New DefaultClusteringAlgorithm
        Dim cluster As Cluster = alg.performClustering(distances, names, New AverageLinkageStrategy)
        Return cluster
    End Function
End Module
