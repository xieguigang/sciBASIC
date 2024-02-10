
Imports Microsoft.VisualBasic.DataMining.Evaluation
Imports Microsoft.VisualBasic.DataMining.KMeans

Namespace AffinityPropagation

    Public Module SimilarityMatrix

        ''' <summary>
        ''' Create the similarity matrix with a user defined distance measure
        ''' </summary>
        ''' <param name="ptr"></param>
        ''' <param name="distance"></param>
        ''' <returns></returns>
        Public Function SparseSimilarityMatrix(ptr As ClusterEntity(), distance As IMetric) As Edge()
            Dim items = New Edge(ptr.Length * ptr.Length - 1) {}
            Dim p = 0
            For i = 0 To ptr.Length - 1 - 1
                For j = i + 1 To ptr.Length - 1
                    items(p) = New Edge(i, j, distance(ptr(i), ptr(j)))
                    items(p + 1) = New Edge(j, i, distance(ptr(i), ptr(j)))
                    p += 2
                Next
            Next
            Return items
        End Function

        Public Function SparseSimilarityMatrix(ptr As ClusterEntity()) As Edge()
            Dim items = New Edge(ptr.Length * ptr.Length - 1) {}
            Dim p = 0
            For i = 0 To ptr.Length - 1 - 1
                For j = i + 1 To ptr.Length - 1
                    items(p) = New Edge(i, j, -ptr(i).DistanceTo(ptr(j)))
                    items(p + 1) = New Edge(j, i, -ptr(i).DistanceTo(ptr(j)))
                    p += 2
                Next
            Next
            Return items
        End Function
    End Module
End Namespace