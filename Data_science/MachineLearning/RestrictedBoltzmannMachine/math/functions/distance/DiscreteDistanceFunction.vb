
Namespace math.functions.distance

    ''' <summary>
    ''' Created by kenny on 2/16/14.
    ''' </summary>
    Public Class DiscreteDistanceFunction
        Implements DistanceFunction

        Private Const DELTA As Double = 0.05

        Public Overridable Function distance(item1 As DenseMatrix, item2 As DenseMatrix) As Double Implements DistanceFunction.distance
            For i = 0 To item1.columns() - 1
                If System.Math.Abs(item1.get(0, i) - item2.get(0, i)) > DELTA Then
                    Return 1.0
                End If
            Next
            Return 0.0
        End Function

        Public Overrides Function ToString() As String
            Return "DiscreteDistanceFunction"
        End Function

    End Class

End Namespace
