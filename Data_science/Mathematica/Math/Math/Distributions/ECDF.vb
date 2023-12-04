Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Distributions.BinBox
Imports std = System.Math

Namespace Distributions

    Public Class ECDF

        ReadOnly data As DataBinBox(Of Double)()

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(x As IEnumerable(Of Double), Optional k As Integer = 100)
            data = CutBins.FixedWidthBins(x, k, Function(xi) xi).ToArray
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="q"></param>
        ''' <param name="eps"></param>
        ''' <returns>
        ''' the upper bound raw value of the threshold
        ''' </returns>
        Public Function FindThreshold(q As Double, Optional eps As Double = 0.1) As Double
            Dim sample As DataBinBox(Of Double)() = data.OrderBy(Function(b) b.Boundary.Min).ToArray
            Dim N As Integer = Aggregate point As DataBinBox(Of Double)
                               In sample
                               Into Sum(point.Count)
            Dim minK As Integer = 1
            Dim minD As Double = Double.MaxValue

            If sample.Length = 0 Then
                Return 0
            End If

            For k As Integer = 1 To sample.Length - 1
                Dim cdf As Double = ECDF.CDF(sample.Take(k), N)

                If cdf > q Then
                    Exit For
                End If

                Dim d As Double = std.Abs(cdf - q)

                If d <= eps Then
                    Return sample(k).Boundary.Min
                ElseIf d < minD Then
                    minD = d
                    minK = k
                End If
            Next

            Return sample(minK - 1).Boundary.Max
        End Function

        ''' <summary>
        ''' T computation involves the cumulative distributive
        ''' function p(k)(CDF), defined As
        ''' 
        ''' ```
        ''' q ~ p(k) = sum(h(i)) / N
        ''' ```
        '''
        ''' h(i) stands For the i bin's frequency within an image 
        ''' histogram, N is the image’s pixel count. Given a target 
        ''' probability q it Is possible to find the bin k whose 
        ''' CDF closely resembles q. Then, the upper limit Of the 
        ''' bin k In h will be used As the threshold value T.
        ''' </summary>
        ''' <param name="bin"></param>
        ''' <param name="N"></param>
        ''' <returns></returns>
        Public Shared Function CDF(bin As IEnumerable(Of DataBinBox(Of Double)), N As Integer) As Double
            Dim sumHk As Double = Aggregate hi As DataBinBox(Of Double) In bin Into Sum(hi.Count)
            Dim p As Double = sumHk / N

            Return p
        End Function
    End Class
End Namespace