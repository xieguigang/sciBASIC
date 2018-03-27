Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Text.Levenshtein
Imports Microsoft.VisualBasic.Text.Levenshtein.LevenshteinDistance

Public Module GlobalMatch

    Public Function Similarity(a As Vector, b As Vector, threshold#) As Double
        Dim equals As Equals(Of Double) = Function(x, y)
                                              x = threshold <= x
                                              y = threshold <= y
                                              Return x = y
                                          End Function
        Dim dist = LevenshteinDistance.ComputeDistance(
            a.ToArray,
            b.ToArray,
            equals,
            Function(x) If(x <= threshold, "0"c, "1"c)
        )

        If dist Is Nothing Then
            Return 0
        Else
            Return dist.MatchSimilarity
        End If
    End Function
End Module
