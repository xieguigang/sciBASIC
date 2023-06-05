Imports stdNum = System.Math

Namespace HDBSCAN.Distance
    ''' <summary>
    ''' Computes the supremum distance between two points, d = max[(x1-y1), (x2-y2), ... ,(xn-yn)].
    ''' </summary>
    Public Class SupremumDistance
        Implements IDistanceCalculator(Of Double())
        Public Function ComputeDistance(indexOne As Integer, indexTwo As Integer, attributesOne As Double(), attributesTwo As Double()) As Double Implements IDistanceCalculator(Of Double()).ComputeDistance
            Dim distance As Double = 0
            Dim i = 0

            While i < attributesOne.Length AndAlso i < attributesTwo.Length
                Dim difference = stdNum.Abs(attributesOne(i) - attributesTwo(i))
                If difference > distance Then distance = difference
                i += 1
            End While
            Return distance
        End Function
    End Class
End Namespace
