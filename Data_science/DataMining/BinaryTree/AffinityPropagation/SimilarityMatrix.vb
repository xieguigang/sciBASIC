
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.Evaluation
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.Correlations.DistanceMethods

Namespace AffinityPropagation

    Public Module SimilarityMatrix

        ''' <summary>
        ''' Create the similarity matrix with a user defined distance measure
        ''' </summary>
        ''' <param name="ptr"></param>
        ''' <param name="distance"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function SparseSimilarityMatrix(ptr As ClusterEntity(), distance As IMetric) As Edge()
            Dim items = New Edge(ptr.Length * ptr.Length - 1) {}
            Dim p = 0

            For i As Integer = 0 To ptr.Length - 1 - 1
                For j As Integer = i + 1 To ptr.Length - 1
                    items(p) = New Edge(i, j, distance(ptr(i), ptr(j)))
                    items(p + 1) = New Edge(j, i, distance(ptr(i), ptr(j)))
                    p += 2
                Next
            Next
            Return items
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function SparseSimilarityMatrix(ptr As ClusterEntity()) As Edge()
            Return ptr.SparseSimilarityMatrix(distance:=Function(a, b) -a.EuclideanDistance(b))
        End Function
    End Module
End Namespace