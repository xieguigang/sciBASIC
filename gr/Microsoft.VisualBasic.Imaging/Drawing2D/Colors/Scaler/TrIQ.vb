Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Math.Distributions.BinBox
Imports stdNum = System.Math

Namespace Drawing2D.Colors.Scaler

    Public Module TrIQ

        <Extension>
        Public Function CDF(bin As IEnumerable(Of DataBinBox(Of Double)), N As Integer) As Double
            Dim sumHk As Double = Aggregate hi As DataBinBox(Of Double) In bin Into Sum(hi.Count)
            Dim p As Double = sumHk / N

            Return p
        End Function

        <Extension>
        Public Function FindThreshold(data As IEnumerable(Of Double), q As Double,
                                      Optional N As Integer = 100,
                                      Optional eps As Double = 0.1) As Double

            Return CutBins _
                .FixedWidthBins(data, N, Function(x) x) _
                .FindThreshold(q, eps)
        End Function

        <Extension>
        Public Function FindThreshold(data As IEnumerable(Of DataBinBox(Of Double)), q As Double, Optional eps As Double = 0.1) As Double
            Dim sample As DataBinBox(Of Double)() = data.OrderBy(Function(b) b.Raw.First).ToArray
            Dim N As Integer = Aggregate point In sample Into Sum(point.Count)
            Dim minK As Integer
            Dim minD As Double = Double.MaxValue

            For k As Integer = 1 To sample.Length - 1
                Dim d As Double = stdNum.Abs(sample.Take(k).CDF(N) - q)

                If d <= eps Then
                    Return sample(k).Raw.Max
                ElseIf d < minD Then
                    minD = d
                    minK = k
                End If
            Next

            Return sample(minK).Raw.Min
        End Function

        <Extension>
        Public Function DiscreteLevels(data As IEnumerable(Of Double),
                                       Optional n As Integer = 30,
                                       Optional T As Double? = Nothing) As IEnumerable(Of Integer)

            Dim f As Double() = data.ToArray
            Dim minf As Double = f.Min

            If T Is Nothing Then
                T = f.Max
            End If

            Dim levelRange As New DoubleRange(0, n)
            Dim scaler As New DoubleRange(minf, T)

            Return From w As Double
                   In f
                   Let q As Double = If(w >= T, n - 1, scaler.ScaleMapping(w, levelRange))
                   Select CInt(q)
        End Function

    End Module
End Namespace