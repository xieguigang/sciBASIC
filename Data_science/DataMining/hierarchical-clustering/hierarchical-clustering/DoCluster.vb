Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
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

    ''' <summary>
    ''' Run hierarchical clustering
    ''' </summary>
    ''' <param name="objects"></param>
    ''' <param name="algorithm">Default is <see cref="DefaultClusteringAlgorithm"/></param>
    ''' <param name="linkageStrategy">Default is <see cref="AverageLinkageStrategy"/></param>
    ''' <returns></returns>
    <Extension>
    Public Function RunCluster(objects As IEnumerable(Of DataSet),
                               Optional algorithm As ClusteringAlgorithm = Nothing,
                               Optional linkageStrategy As LinkageStrategy = Nothing) As Cluster

        Dim list = objects.ToArray
        Dim distances = list.DistanceMatrix
        Dim names$() = list.Keys

        ' with (algorithm or new DefaultClusteringAlgorithm as default) if algorithm is nothing
        With algorithm Or New DefaultClusteringAlgorithm().AsDefault ' (Function(alg) alg Is Nothing)

            ' using (linkageStrategy or new AverageLinkageStrategy as default) if linkageStrategy is nothing
            Dim cluster As Cluster = .performClustering(
                distances,
                names,
                linkageStrategy Or New AverageLinkageStrategy().AsDefault)

            Return cluster
        End With
    End Function
End Module
