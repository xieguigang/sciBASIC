Imports stdNum = System.Math

Namespace HDBSCAN.Distance
    ''' <summary>
    ''' Computes the euclidean distance between two points, d = 1 - (cov(X,Y) / (std_dev(X) * std_dev(Y)))
    ''' </summary>
    Public Class PearsonCorrelation
        Implements IDistanceCalculator(Of Double())
        Public Function ComputeDistance(indexOne As Integer, indexTwo As Integer, attributesOne As Double(), attributesTwo As Double()) As Double Implements IDistanceCalculator(Of Double()).ComputeDistance
            Dim meanOne As Double = 0
            Dim meanTwo As Double = 0
            Dim i = 0

            While i < attributesOne.Length AndAlso i < attributesTwo.Length
                meanOne += attributesOne(i)
                meanTwo += attributesTwo(i)
                i += 1
            End While
            meanOne = meanOne / attributesOne.Length
            meanTwo = meanTwo / attributesTwo.Length
            Dim covariance As Double = 0
            Dim standardDeviationOne As Double = 0
            Dim standardDeviationTwo As Double = 0

            i = 0

            While i < attributesOne.Length AndAlso i < attributesTwo.Length
                covariance += (attributesOne(i) - meanOne) * (attributesTwo(i) - meanTwo)
                standardDeviationOne += (attributesOne(i) - meanOne) * (attributesOne(i) - meanOne)
                standardDeviationTwo += (attributesTwo(i) - meanTwo) * (attributesTwo(i) - meanTwo)
                i += 1
            End While
            Return stdNum.Max(0, 1 - covariance / stdNum.Sqrt(standardDeviationOne * standardDeviationTwo))
        End Function
    End Class
End Namespace
