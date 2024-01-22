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
            Dim class_groups = data _
                .GroupBy(Function(c) c.cluster) _
                .Select(Function(c) c.ToArray) _
                .ToArray
            Dim dunn = Evaluation.Dunn(class_groups)
            Dim silhouette = Evaluation.Silhouette(class_groups.Select(Function(g) New Cluster(Of ClusterEntity)(g)))

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

    End Class
End Namespace