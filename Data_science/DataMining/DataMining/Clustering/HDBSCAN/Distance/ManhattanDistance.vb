Imports stdNum = System.Math

Namespace HDBSCAN.Distance
    ''' <summary>
    ''' Computes the manhattan distance between two points, d = |x1-y1| + |x2-y2| + ... + |xn-yn|.
    ''' </summary>
    Public Class ManhattanDistance
        Implements IDistanceCalculator(Of Double())
        Public Function ComputeDistance(indexOne As Integer, indexTwo As Integer, attributesOne As Double(), attributesTwo As Double()) As Double Implements IDistanceCalculator(Of Double()).ComputeDistance
            Dim distance As Double = 0
            Dim i = 0

            While i < attributesOne.Length AndAlso i < attributesTwo.Length
                distance += stdNum.Abs(attributesOne(i) - attributesTwo(i))
                i += 1
            End While
            Return distance
        End Function
    End Class
End Namespace
