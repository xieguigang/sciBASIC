Imports System.Linq
Imports std = System.Math

Namespace Waves

    ''' <summary>
    ''' the estimated periodic structure of a scan line signal: the period in samples and
    ''' the normalized auto correlation score of that period
    ''' </summary>
    Public Structure PeriodEstimation
        Public period As Double
        Public score As Double
    End Structure

    ''' <summary>
    ''' the period estimation and the sync pulse detection of a periodic scan line signal.
    ''' 
    ''' the scan line period is measured by the auto correlation of the signal envelope with
    ''' a coarse decimated search and then a fine full resolution search, the sync pulses are
    ''' located by the envelope peaks above an adaptive percentile threshold.
    ''' </summary>
    Public Module PeriodAnalysis

        ''' <summary>
        ''' estimate the scan line period via the auto correlation of the signal envelope
        ''' </summary>
        ''' <param name="envelope">the amplitude envelope of the signal (see <see cref="MathUtils.Envelope"/>)</param>
        ''' <param name="result">the estimated period and the correlation score</param>
        ''' <param name="k">the decimation factor for the coarse auto correlation computation</param>
        Public Sub EstimateLinePeriod(envelope As Double(), ByRef result As PeriodEstimation,
                                      Optional k As Integer = 8)

            ' the line period is expect to be inside the range of [1500, 6000] samples
            Dim minLag As Integer = 1500 \ k
            Dim maxLag As Integer = 6000 \ k
            Dim dec As Double() = MathUtils.Decimate(envelope, k)
            Dim corr As Double() = MathUtils.AutoCorrelation(dec, minLag, maxLag, maxCount:=65536)

            Dim best As Integer = 0

            For i As Integer = 1 To corr.Length - 1
                If corr(i) > corr(best) Then
                    best = i
                End If
            Next

            Dim coarse As Double = (minLag + best) * k

            ' refine the period on the full resolution envelope
            Dim fine As Double() = MathUtils.AutoCorrelation(envelope, std.Max(1, CInt(coarse) - 32), CInt(coarse) + 32, maxCount:=200000)
            Dim fineBest As Integer = 0

            For i As Integer = 1 To fine.Length - 1
                If fine(i) > fine(fineBest) Then
                    fineBest = i
                End If
            Next

            result.period = coarse - 32 + fineBest
            result.score = fine(fineBest)
        End Sub

        ''' <summary>
        ''' measure the scan line period (in samples) of a given channel data
        ''' </summary>
        ''' <param name="data">the sample data of a single audio channel</param>
        ''' <param name="envWindow">the window size of the envelope low pass filter</param>
        Public Function MeasureLinePeriod(data As Single(), Optional envWindow As Integer = 96) As Double
            Dim envelope As Double() = MathUtils.Envelope(data, envWindow)
            Dim result As New PeriodEstimation

            Call EstimateLinePeriod(envelope, result)

            Return result.period
        End Function

        ''' <summary>
        ''' detect the sub sample positions of the sync pulses on the signal envelope
        ''' </summary>
        ''' <param name="envelope">the amplitude envelope of the signal</param>
        ''' <param name="linePeriod">the measured scan line period in samples</param>
        Public Function DetectSyncPulses(envelope As Double(), linePeriod As Double) As Double()
            Dim median As Double = MathUtils.Percentile(envelope, 0.5)
            Dim top As Double = MathUtils.Percentile(envelope, 0.995)
            Dim threshold As Double = median + (top - median) * 0.35
            Dim peaks As Integer() = PeakFinding.LocalMaxima.FindPeaks(envelope, threshold, std.Max(16, CInt(linePeriod * 0.5)))
            Dim positions As Double() = New Double(peaks.Length - 1) {}

            For i As Integer = 0 To peaks.Length - 1
                positions(i) = PeakFinding.LocalMaxima.ParabolicPeak(envelope, peaks(i))
            Next

            Return positions
        End Function

        ''' <summary>
        ''' the intervals between the consecutive sync pulses
        ''' </summary>
        ''' <param name="pulses">the sub sample positions of the sync pulses</param>
        Public Function PulseIntervals(pulses As Double()) As Double()
            If pulses.Length < 2 Then
                Return New Double() {}
            End If

            Dim d As Double() = New Double(pulses.Length - 2) {}

            For i As Integer = 1 To pulses.Length - 1
                d(i - 1) = pulses(i) - pulses(i - 1)
            Next

            Return d
        End Function

        ''' <summary>
        ''' the sample standard deviation of the given values
        ''' </summary>
        ''' <param name="values">the sample data</param>
        Public Function StdError(values As Double()) As Double
            If values.Length < 2 Then
                Return 0
            End If

            Dim mean As Double = MathUtils.Mean(values)
            Dim sum As Double = 0

            For i As Integer = 0 To values.Length - 1
                sum += (values(i) - mean) ^ 2
            Next

            Return std.Sqrt(sum / (values.Length - 1))
        End Function

        ''' <summary>
        ''' the period drift: the mean interval of the second half data minus the first half data
        ''' </summary>
        ''' <param name="pulses">the sub sample positions of the sync pulses</param>
        Public Function IntervalDrift(pulses As Double()) As Double
            If pulses.Length < 8 Then
                Return 0
            End If

            Dim half As Integer = pulses.Length \ 2

            Return (pulses(pulses.Length - 1) - pulses(half)) / (pulses.Length - half) -
                   (pulses(half) - pulses(0)) / half
        End Function

        ''' <summary>
        ''' the dominate values on the interval histogram
        ''' </summary>
        ''' <param name="intervals">the pulse intervals in samples</param>
        ''' <param name="binSize">the histogram bin size in samples</param>
        ''' <param name="top">the number of the dominate bins which are reported</param>
        Public Function IntervalModes(intervals As Double(), binSize As Integer, top As Integer) As Double()
            If intervals.Length = 0 Then
                Return New Double() {}
            End If

            Dim histogram As New Dictionary(Of Integer, Integer)

            For Each interval As Double In intervals
                Dim bin As Integer = CInt(std.Round(interval / binSize)) * binSize
                Dim count As Integer = 0

                If histogram.TryGetValue(bin, count) Then
                    histogram(bin) = count + 1
                Else
                    histogram(bin) = 1
                End If
            Next

            Return histogram _
                .OrderByDescending(Function(x) x.Value) _
                .Take(top) _
                .Select(Function(x) CDbl(x.Key)) _
                .ToArray
        End Function
    End Module
End Namespace
