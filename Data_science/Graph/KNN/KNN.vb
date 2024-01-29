Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree.ApproximateNearNeighbor
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace KNearNeighbors

    ''' <summary>
    ''' KNN search handler for phenograph
    ''' </summary>
    Public Class KNN

        ReadOnly score As ScoreMetric

        ''' <summary>
        ''' knn score cutoff
        ''' </summary>
        ReadOnly cutoff As Double

        Sub New(metric As ScoreMetric, knn_cutoff As Double)
            Me.score = metric
            Me.cutoff = knn_cutoff

            score.cutoff = knn_cutoff
        End Sub

        ''' <summary>
        ''' the output keeps the same order as the given input <paramref name="data"/>
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="k"></param>
        ''' <returns>
        ''' the generates index value keeps the same order with input original <paramref name="data"/> matrix rows.
        ''' </returns>
        Public Function FindNeighbors(data As GeneralMatrix, Optional k As Integer = 30) As IEnumerable(Of KNeighbors)
            Dim matrix As TagVector() = data.PopulateVectors.ToArray
            Dim knnQuery = matrix _
                .AsParallel _
                .Select(Function(v)
                            Return (v.index, FindNeighbors(v, matrix, k, score))
                        End Function) _
                .OrderBy(Function(r) r.index) _
                .ToArray

            Return KNeighbors(knnQuery.Select(Function(r) r.Item2))
        End Function

        Public Shared Iterator Function KNeighbors(knn As IEnumerable(Of (TagVector, w As Double)())) As IEnumerable(Of KNeighbors)
            For Each nn2 As (TagVector, w As Double)() In knn
                Dim index As Integer() = nn2.Select(Function(xi) xi.Item1.index).ToArray
                Dim weights As Double() = nn2.Select(Function(xi) xi.w).ToArray

                Yield New KNeighbors(index.Length, index, weights)
            Next
        End Function

        Public Shared Function FindNeighbors(v As TagVector, matrix As TagVector(), k As Integer, score As ScoreMetric) As (TagVector, w As Double)()
            Dim vec As Double() = v.vector.ToArray

            Return matrix _
                .Select(Function(i)
                            Dim w As Double = score.eval(vec, i.vector)
                            Return (i, w)
                        End Function) _
                .Where(Function(a) a.w > score.cutoff) _
                .OrderByDescending(Function(a) a.w) _
                .Take(k) _
                .ToArray
        End Function
    End Class
End Namespace