
Namespace math.functions.distance

    ''' <summary>
    ''' Created by kenny on 2/16/14.
    ''' </summary>
    Public Class EuclideanDistanceFunction
        Implements DistanceFunction

        Public Overridable Function distance(item1 As DenseMatrix, item2 As DenseMatrix) As Double Implements DistanceFunction.distance
            Dim sumSq As Double = 0
            For i = 0 To item1.columns() - 1
                sumSq += System.Math.Pow(item1.get(0, i) - item2.get(0, i), 2)
            Next
            Return System.Math.Sqrt(sumSq)
        End Function

        Public Overrides Function ToString() As String
            Return "EuclideanDistanceFunction"
        End Function

    End Class

End Namespace
