Imports System

Namespace HdbscanSharp.Distance
    ''' <summary>
    ''' Computes the supremum distance between two points, d = max[(x1-y1), (x2-y2), ... ,(xn-yn)].
    ''' </summary>
    Public Class SupremumDistance
        Implements IDistanceCalculator(Of Double())
        Public Function ComputeDistance(ByVal indexOne As Integer, ByVal indexTwo As Integer, ByVal attributesOne As Double(), ByVal attributesTwo As Double()) As Double Implements IDistanceCalculator(Of Double()).ComputeDistance
            Dim distance As Double = 0
            Dim i = 0

            While i < attributesOne.Length AndAlso i < attributesTwo.Length
                Dim difference = Math.Abs(attributesOne(i) - attributesTwo(i))
                If difference > distance Then distance = difference
                i += 1
            End While
            Return distance
        End Function
    End Class
End Namespace
