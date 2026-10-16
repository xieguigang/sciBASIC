Imports System.Linq
Imports std = System.Math

Namespace Waves

    ''' <summary>
    ''' a single scan line on the analog image signal
    ''' </summary>
    Public Structure LineRow

        ''' <summary>
        ''' the zero based scan line index
        ''' </summary>
        Public ReadOnly index As Integer

        ''' <summary>
        ''' the start sample position of this scan line on the data section
        ''' </summary>
        Public ReadOnly start As Double

        ''' <summary>
        ''' the scan line period of this row
        ''' </summary>
        Public ReadOnly period As Double

        Sub New(index As Integer, start As Double, period As Double)
            Me.index = index
            Me.start = start
            Me.period = period
        End Sub
    End Structure

    ''' <summary>
    ''' the scan line time grid of an image data section
    ''' </summary>
    Public Class LineGrid

        ''' <summary>
        ''' the measured scan line period (in samples)
        ''' </summary>
        ''' <returns></returns>
        Public Property period As Double

        ''' <summary>
        ''' the normalized auto correlation score of the scan line period, a value
        ''' near 1.0 means a stable line structure
        ''' </summary>
        ''' <returns></returns>
        Public Property score As Double

        ''' <summary>
        ''' the sample position of the sync pulse inside one scan line period,
        ''' it is the reference origin of the image data cells
        ''' </summary>
        ''' <returns></returns>
        Public Property sync_offset As Double

        ''' <summary>
        ''' the number of the scan lines on the data section
        ''' </summary>
        ''' <returns></returns>
        Public Property count As Integer

        ''' <summary>
        ''' the standard deviation (in samples) of the per scan line alignment shifts,
        ''' it is the residual jitter of the analog recording which is removed by the
        ''' per scan line synchronization
        ''' </summary>
        ''' <returns></returns>
        Public Property jitter_std As Double

        ''' <summary>
        ''' the correlation score of the per scan line alignment template, a value near
        ''' one means a reliable synchronization
        ''' </summary>
        ''' <returns></returns>
        Public Property align_score As Double

        ''' <summary>
        ''' has the per scan line alignment been applied on current grid
        ''' </summary>
        ''' <returns></returns>
        Public Property aligned As Boolean

        Public Property rows As LineRow()

        Public Overrides Function ToString() As String
            Return $"{count} scan lines, period={period.ToString("F4")} samples, sync at {sync_offset.ToString("F2")}, jitter={jitter_std.ToString("F3")}"
        End Function
    End Class

    ''' <summary>
    ''' the scan line synchronization of a periodic scan line analog signal.
    ''' 
    ''' the analog scan line period is usually NOT an integer number of the sample frames,
    ''' so the period must be estimated with a sub sample precision, otherwise a tiny
    ''' period error accumulates along the scan lines and smears the decoded image. the
    ''' module measures the period from the auto correlation of the signal envelope,
    ''' refines it by the vertical coherence of the folded raster, and locates the sync
    ''' pulse offset by the high pass filtered mean scan line profile.
    ''' </summary>
    Public Module ScanLineSync

        ''' <summary>
        ''' detect the scan line time grid of a given image data section
        ''' </summary>
        ''' <param name="data">the sample data of a single audio channel</param>
        ''' <param name="lines">the expected number of the scan lines, zero for auto detect</param>
        ''' <param name="period">the scan line period, zero for auto detect and refine</param>
        ''' <param name="align">apply the per scan line synchronization for removing the
        ''' residual jitter of the analog recording</param>
        ''' <param name="alignWindow">the maximal per scan line alignment shift (in samples)</param>
        Public Function Sync(data As Single(), Optional lines As Integer = 0, Optional period As Double = 0,
                             Optional align As Boolean = True, Optional alignWindow As Integer = 48) As LineGrid
            Dim score As Double = 0

            If period <= 0 Then
                Dim estimation As New PeriodEstimation

                Call PeriodAnalysis.EstimateLinePeriod(MathUtils.Envelope(data, 96), estimation)

                score = estimation.score
                period = RefinePeriod(data, 0, estimation.period, std.Min(256, CInt(data.Length / estimation.period)))
            End If

            If lines <= 0 Then
                lines = CInt(std.Floor(data.Length / period))
            End If

            Dim grid As New LineGrid With {
                .period = period,
                .score = score,
                .count = lines,
                .sync_offset = EstimateSyncOffset(data, period, std.Min(lines, 256))
            }
            Dim rows As LineRow() = New LineRow(lines - 1) {}

            For i As Integer = 0 To lines - 1
                rows(i) = New LineRow(i, i * period, period)
            Next

            grid.rows = rows

            If align Then
                Call AlignRows(data, grid, alignWindow)
            End If

            Return grid
        End Function

        ''' <summary>
        ''' refine the start sample position of each scan line by aligning the scan line
        ''' head region (the sync pulse and the blanking level) against the mean scan line
        ''' template.
        ''' 
        ''' the scan line period of the analog recording is not exactly constant, the
        ''' residual phase jitter of each scan line appears as the "water wave" streaks
        ''' and the vertical bands on the decoded image. the jitter is measured from the
        ''' sync pulse of each scan line and is removed by shifting the demodulation
        ''' window of the scan line.
        ''' </summary>
        Public Sub AlignRows(data As Single(), grid As LineGrid, Optional window As Integer = 48)
            Dim rows As LineRow() = grid.rows
            Dim count As Integer = rows.Length
            Dim feature As Integer = CInt(std.Floor(grid.period)) - 2 * window

            If count < 4 OrElse feature < 64 Then
                Return
            End If

            feature = std.Min(feature, 1024)

            Dim mean As Double() = New Double(feature - 1) {}
            Dim i As Integer
            Dim t As Integer

            For i = 0 To count - 1
                Dim origin As Double = rows(i).start

                For t = 0 To feature - 1
                    mean(t) += MathUtils.LerpClamped(data, origin + t)
                Next
            Next

            For t = 0 To feature - 1
                mean(t) /= count
            Next

            ' the mean scan line template: the sync pulse is highlighted by the averaging
            ' while the image payload content is cancelled out
            Dim smooth As Double() = MathUtils.MovingAverage(mean, 128)
            Dim template As Double() = New Double(feature - 1) {}

            For t = 0 To feature - 1
                template(t) = mean(t) - smooth(t)
            Next

            Call RemoveMean(template)

            Dim templateNorm As Double = Norm(template)

            If templateNorm = 0 Then
                Return
            End If

            Dim shifts As Double() = New Double(count - 1) {}
            Dim segment As Double() = New Double(feature - 1) {}
            Dim scores As Double() = New Double(2 * window) {}
            Dim score As Double = 0
            Dim aligned As Integer = 0

            For i = 0 To count - 1
                Dim origin As Double = rows(i).start

                For t = 0 To feature - 1
                    segment(t) = MathUtils.LerpClamped(data, origin + t)
                Next

                ' remove the slow trend of the scan line itself, only the sharp sync
                ' pulse shape is kept for the alignment
                Dim segSmooth As Double() = MathUtils.MovingAverage(segment, 128)

                For t = 0 To feature - 1
                    segment(t) -= segSmooth(t)
                Next

                Call RemoveMean(segment)

                Dim segmentNorm As Double = Norm(segment)

                If segmentNorm = 0 Then
                    shifts(i) = 0
                    Continue For
                End If

                Dim best As Integer = 0
                Dim bestScore As Double = Double.MinValue

                For s As Integer = -window To window
                    Dim value As Double = Correlation(segment, template, s, feature)

                    scores(s + window) = value

                    If value > bestScore Then
                        bestScore = value
                        best = s
                    End If
                Next

                ' the sub sample refinement via the parabolic interpolation
                Dim delta As Double = 0
                Dim index As Integer = best + window

                If index > 0 AndAlso index < scores.Length - 1 Then
                    delta = PeakFinding.LocalMaxima.ParabolicPeak(scores, index) - index
                End If

                shifts(i) = -(best + delta)
                score += bestScore
                aligned += 1
            Next

            If aligned = 0 Then
                Return
            End If

            ' the coherence score of the two possible shift signs is measured, the
            ' better one is kept, this makes the sign convention of the correlation
            ' independent from the implementation detail
            Dim direct As Double = CoherenceScore(data, grid.period, shifts, count, 8)
            Dim inverted As Double() = New Double(count - 1) {}

            For i = 0 To count - 1
                inverted(i) = -shifts(i)
            Next

            Dim flipped As Double = CoherenceScore(data, grid.period, inverted, count, 8)
            Dim sign As Double = If(flipped < direct, -1.0, 1.0)
            Dim sum As Double = 0

            For i = 0 To count - 1
                shifts(i) = shifts(i) * sign
                sum += shifts(i)
            Next

            ' the alignment shifts have no absolute meaning, they are re-centred
            Dim base As Double = sum / count

            For i = 0 To count - 1
                rows(i) = New LineRow(i, i * grid.period + shifts(i) - base, grid.period)
            Next

            grid.align_score = score / aligned
            grid.jitter_std = StdDev(shifts)
            grid.aligned = True
        End Sub

        Private Function StdDev(values As Double()) As Double
            Dim mean As Double = 0

            For i As Integer = 0 To values.Length - 1
                mean += values(i)
            Next

            mean /= values.Length

            Dim sum As Double = 0

            For i As Integer = 0 To values.Length - 1
                sum += (values(i) - mean) ^ 2
            Next

            Return std.Sqrt(sum / values.Length)
        End Function

        Private Sub RemoveMean(values As Double())
            Dim mean As Double = 0

            For i As Integer = 0 To values.Length - 1
                mean += values(i)
            Next

            mean /= values.Length

            For i As Integer = 0 To values.Length - 1
                values(i) -= mean
            Next
        End Sub

        Private Function Norm(values As Double()) As Double
            Dim sum As Double = 0

            For i As Integer = 0 To values.Length - 1
                sum += values(i) * values(i)
            Next

            Return std.Sqrt(sum)
        End Function

        ''' <summary>
        ''' the correlation coefficient of ``a(t)`` and ``b(t + shift)`` on the overlapped region
        ''' </summary>
        Private Function Correlation(a As Double(), b As Double(), shift As Integer, n As Integer) As Double
            Dim from As Integer = std.Max(0, -shift)
            Dim [to] As Integer = std.Min(n - 1, n - 1 - shift)
            Dim sum As Double = 0
            Dim aa As Double = 0
            Dim bb As Double = 0

            For t As Integer = from To [to]
                Dim x As Double = a(t)
                Dim y As Double = b(t + shift)

                sum += x * y
                aa += x * x
                bb += y * y
            Next

            If aa = 0 OrElse bb = 0 Then
                Return 0
            End If

            Return sum / std.Sqrt(aa * bb)
        End Function

        ''' <summary>
        ''' the vertical coherence score of the aligned raster: the mean squared difference
        ''' between two adjacent scan lines. the smaller the score is, the better the
        ''' scan line alignment is.
        ''' </summary>
        Public Function CoherenceScore(data As Single(), period As Double, shifts As Double(),
                                       lines As Integer, decimate As Integer) As Double

            Dim samplesPerRow As Integer = CInt(std.Floor(period)) \ decimate
            Dim total As Double = 0
            Dim scale As Double = 0
            Dim previous As Double() = Nothing
            Dim current As Double() = New Double(samplesPerRow - 1) {}

            For r As Integer = 0 To lines - 1
                Dim origin As Double = r * period + shifts(r)

                For c As Integer = 0 To samplesPerRow - 1
                    current(c) = MathUtils.LerpClamped(data, origin + c * decimate)
                    scale += current(c) * current(c)
                Next

                If previous IsNot Nothing Then
                    Dim sum As Double = 0

                    For c As Integer = 0 To samplesPerRow - 1
                        Dim d As Double = current(c) - previous(c)

                        sum += d * d
                    Next

                    total += sum / samplesPerRow
                End If

                previous = current
                current = New Double(samplesPerRow - 1) {}
            Next

            scale /= lines * samplesPerRow

            If scale = 0 Then
                Return Double.MaxValue
            End If

            Return total / (lines - 1) / scale
        End Function

        ''' <summary>
        ''' the vertical coherence score of a folded raster: the mean squared difference
        ''' between two adjacent scan lines. the smaller the score is, the better the
        ''' folding period matches the real scan line period of the analog signal.
        ''' </summary>
        Public Function CoherenceScore(data As Single(), start As Integer, period As Double,
                                       lines As Integer, decimate As Integer) As Double

            Dim samplesPerRow As Integer = CInt(std.Floor(period)) \ decimate
            Dim previous As Double() = Nothing
            Dim total As Double = 0
            Dim scale As Double = 0
            Dim rows As Integer = 0

            For r As Integer = 0 To lines - 1
                Dim origin As Double = start + r * period
                Dim current As Double() = New Double(samplesPerRow - 1) {}

                For c As Integer = 0 To samplesPerRow - 1
                    current(c) = MathUtils.LerpClamped(data, origin + c * decimate)
                    scale += current(c) * current(c)
                Next

                If previous IsNot Nothing Then
                    Dim sum As Double = 0

                    For c As Integer = 0 To samplesPerRow - 1
                        Dim d As Double = current(c) - previous(c)

                        sum += d * d
                    Next

                    total += sum / samplesPerRow
                End If

                previous = current
                rows += 1
            Next

            scale /= rows * samplesPerRow

            If scale = 0 Then
                Return Double.MaxValue
            End If

            Return total / (rows - 1) / scale
        End Function

        ''' <summary>
        ''' refine the scan line period by the vertical coherence of the folded raster.
        ''' 
        ''' the analog scan line period is not an integer number of the sample frames, a
        ''' tiny period error accumulates as a sample phase drift along the scan lines,
        ''' which smears the decoded image in the horizontal direction. the period is
        ''' refined by searching the minimum of the folded raster coherence score.
        ''' </summary>
        Public Function RefinePeriod(data As Single(), start As Integer, guess As Double,
                                     lines As Integer, Optional range As Double = 12,
                                     Optional step_ As Double = 0.05) As Double

            Dim best As Double = guess
            Dim bestScore As Double = Double.MaxValue
            Dim p As Double = guess - range

            While p <= guess + range
                Dim score As Double = CoherenceScore(data, start, p, lines, 8)

                If score < bestScore Then
                    bestScore = score
                    best = p
                End If

                p += step_
            End While

            Dim fineBest As Double = best
            Dim fineScore As Double = Double.MaxValue
            Dim q As Double = best - step_

            While q <= best + step_
                Dim score As Double = CoherenceScore(data, start, q, lines * 2, 4)

                If score < fineScore Then
                    fineScore = score
                    fineBest = q
                End If

                q += 0.002
            End While

            Return fineBest
        End Function

        ''' <summary>
        ''' the sample position of the sync pulse inside the scan line period.
        ''' 
        ''' the sync pulse is a sharp transient on the analog signal, so it is located
        ''' by the high pass filtered mean scan line profile.
        ''' </summary>
        Public Function EstimateSyncOffset(data As Single(), period As Double, lines As Integer) As Double
            Dim samplesPerRow As Integer = CInt(std.Floor(period))
            Dim mean As Double() = FoldAnalysis.MeanLine(data, 0, period, lines, samplesPerRow)
            Dim smooth As Double() = MathUtils.MovingAverage(mean, 128)
            Dim highpass As Double() = New Double(samplesPerRow - 1) {}
            Dim peak As Double = 0
            Dim position As Integer = 0

            For i As Integer = 0 To samplesPerRow - 1
                highpass(i) = mean(i) - smooth(i)
            Next

            ' the search range excludes the wrap around region of the line
            For i As Integer = 0 To samplesPerRow - 1
                If std.Abs(highpass(i)) > peak Then
                    peak = std.Abs(highpass(i))
                    position = i
                End If
            Next

            Return position
        End Function
    End Module
End Namespace
