Imports System

Namespace HdbscanSharp.Distance
    ''' <summary>
    ''' Computes the euclidean distance between two points, d = sqrt((x1-y1)^2 + (x2-y2)^2 + ... + (xn-yn)^2).
    ''' </summary>
    Public Class EuclideanDistance
        Implements IDistanceCalculator(Of Double())
        Public Function ComputeDistance(ByVal indexOne As Integer, ByVal indexTwo As Integer, ByVal attributesOne As Double(), ByVal attributesTwo As Double()) As Double Implements IDistanceCalculator(Of Double()).ComputeDistance
            Dim distance As Double = 0
            Dim i = 0

            While i < attributesOne.Length AndAlso i < attributesTwo.Length
                distance += (attributesOne(i) - attributesTwo(i)) * (attributesOne(i) - attributesTwo(i))
                i += 1
            End While
            Return Math.Sqrt(distance)
        End Function
    End Class
End Namespace
