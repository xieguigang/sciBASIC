Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace COW

    ''' <summary>
    ''' This is the parameters of correlation optimized warping algorithm.
    ''' Please see Nielsen et.al. J. Chromatogr. A 805, 17–35 (1998).
    ''' </summary>
    Public Class CowParameter

        Public Property ReferenceID As Integer
        Public Property Slack As Integer
        Public Property SegmentSize As Integer

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        ''' <summary>
        ''' The point of dynamic programming based alignment is to get the suitable reference chromatogram.
        ''' Selecting the reference chromatogram which should look like 'center' of chromatograms will be better to get nice alignment results.
        ''' So, this program is used to get the suitable reference chromatogram from imported chromatograms.
        ''' Please see Tsugawa et. al. Front. Genet. 5:471, 2015
        ''' </summary>
        ''' <param name="chromatograms"></param>
        ''' <returns></returns>
        Public Shared Function AutomaticParameterDefinder(chromatograms As List(Of Double())) As CowParameter
            Dim chromatogramsNumber = chromatograms(0).Length - 3
            Dim gravityArray = New Double(chromatogramsNumber - 1) {}
            Dim totalIntensity, sum As Double, maxIntensity = Double.MinValue

            For i As Integer = 0 To chromatogramsNumber - 1
                sum = 0
                totalIntensity = 0

                For j = 0 To chromatograms.Count - 1
                    sum += chromatograms(j)(1) * chromatograms(j)(3 + i)
                    totalIntensity += chromatograms(j)(3 + i)
                    If maxIntensity < chromatograms(j)(3 + i) Then maxIntensity = chromatograms(j)(3 + i)
                Next

                gravityArray(i) = sum / totalIntensity
            Next

            Dim maxGravity, minGravity, centerGravity As Double
            maxGravity = gravityArray.Max
            minGravity = gravityArray.Min
            centerGravity = (maxGravity + minGravity) / 2

            Dim referenceID = which.Min((New Vector(gravityArray) - centerGravity).Abs)
            Dim slack As Integer = (maxGravity - minGravity) * (chromatograms(chromatograms.Count - 1)(0) - chromatograms(0)(0)) / (chromatograms(chromatograms.Count - 1)(1) - chromatograms(0)(1))
            Dim alignmentParameter As New CowParameter() With {
                .ReferenceID = referenceID,
                .Slack = slack
            }

            Return alignmentParameter
        End Function
    End Class
End Namespace