Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq

Namespace KMeans

    Public Class EvaluationScore

        Public Property silhouette As Double
        Public Property dunn As Double
        Public Property clusters As Dictionary(Of String, String())

        Public ReadOnly Property num_class As Integer
            Get
                Return clusters.Count
            End Get
        End Property

        Public Shared Function Evaluate(data As IEnumerable(Of ClusterEntity)) As EvaluationScore
            Dim class_groups As Bisecting.Cluster() = CreateClusters(data).ToArray
            Dim dunn = Evaluation.Dunn(class_groups)
            Dim silhouette = Evaluation.Silhouette(class_groups)

            Return New EvaluationScore With {
                .clusters = class_groups _
                    .ToDictionary(Function(c) c.First.cluster.ToString,
                                  Function(c)
                                      Return c.Keys.ToArray
                                  End Function),
                .dunn = dunn,
                .silhouette = silhouette
            }
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data">
        ''' all of the data points inside this given collection should 
        ''' be tagged with the <see cref="ClusterEntity.cluster"/> labels.
        ''' </param>
        ''' <returns></returns>
        Public Shared Iterator Function CreateClusters(data As IEnumerable(Of ClusterEntity)) As IEnumerable(Of Bisecting.Cluster)
            Dim class_groups = data _
                .GroupBy(Function(c) c.cluster) _
                .Select(Function(c) c.ToArray) _
                .ToArray

            For Each group As ClusterEntity() In class_groups
                Yield New Bisecting.Cluster(group.CalculateClusterMean, group) With {
                    .Cluster = group.First
                }
            Next
        End Function

    End Class
End Namespace