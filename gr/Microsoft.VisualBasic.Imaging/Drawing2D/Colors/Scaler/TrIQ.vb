Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
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

        ''' <summary>
        ''' get the best value range for level scaler via TrIQ algorithm. 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="q"></param>
        ''' <param name="N"></param>
        ''' <param name="eps"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetTrIQRange(data As IEnumerable(Of Double), q As Double,
                                     Optional N As Integer = 100,
                                     Optional eps As Double = 0.1) As DoubleRange

            Dim raw As Double() = data.SafeQuery.ToArray

            If raw.Length = 0 Then
                Return New DoubleRange(0, 0)
            End If

            Dim max As Double = raw.FindThreshold(q, N, eps)
            Dim range As New DoubleRange(raw.Min, max)

            Return range
        End Function

        <Extension>
        Public Function FindThreshold(data As IEnumerable(Of DataBinBox(Of Double)), q As Double, Optional eps As Double = 0.1) As Double
            Dim sample As DataBinBox(Of Double)() = data.OrderBy(Function(b) b.Boundary.Min).ToArray
            Dim N As Integer = Aggregate point As DataBinBox(Of Double)
                               In sample
                               Into Sum(point.Count)
            Dim minK As Integer = 1
            Dim minD As Double = Double.MaxValue

            For k As Integer = 1 To sample.Length - 1
                Dim cdf As Double = sample.Take(k).CDF(N)

                If cdf > q Then
                    Exit For
                End If

                Dim d As Double = stdNum.Abs(cdf - q)

                If d <= eps Then
                    Return sample(k).Boundary.Min
                ElseIf d < minD Then
                    minD = d
                    minK = k
                End If
            Next

            Return sample(minK - 1).Boundary.Max
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