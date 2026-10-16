Imports System.Linq
Imports std = System.Math

Namespace Waves

    ''' <summary>
    ''' the amplitude statistics of a single scan line
    ''' </summary>
    Public Structure LineStat
        Public index As Integer
        Public mean As Double
        Public rms As Double
        Public peak As Double
        Public [start] As Double
    End Structure

    ''' <summary>
    ''' the visualization and the diagnostic helpers for make the raw waveform structure
    ''' visible:
    ''' 
    ''' 1. <see cref="Oscillogram"/>: the classic amplitude waveform view of a sample block
    ''' 2. <see cref="Fold"/>: fold the signal at the estimated scan line period, this makes
    '''    the scan line structure of the periodic analog signal directly visible
    ''' 3. <see cref="FoldPixel"/>/<see cref="DecodeRaster"/>: fold the signal and decode the
    '''    pixel cells, this gives a first fast preview of the decoded image
    ''' 
    ''' the module also provides the mean scan line template (<see cref="MeanLine"/>) which
    ''' is the base of the per scan line alignment, and the per scan line amplitude
    ''' statistics (<see cref="LineStats"/>) which are used for checking the baseline drift
    ''' of the analog recording.
    ''' 
    ''' ### the merged duplicates
    ''' 
    ''' the scan line period refinement (the ``CoherenceScore`` and the ``RefinePeriod``
    ''' functions) lived in this module as well, but they were the byte identical copies of
    ''' <see cref="ScanLineSync.CoherenceScore(Single(), Integer, Double, Integer, Integer)"/>
    ''' and <see cref="ScanLineSync.RefinePeriod"/>, so the duplicated copies are removed
    ''' and one authoritative implementation is kept in <see cref="ScanLineSync"/>.
    ''' 
    ''' the matrix transpose helper is also removed from this module, the core library
    ''' already provides the linq extension ``MatrixTranspose`` for the jagged matrix.
    ''' </summary>
    Public Module FoldAnalysis

        ''' <summary>
        ''' the classic amplitude oscillogram of a sample block
        ''' </summary>
        ''' <param name="data">the sample data of a single audio channel</param>
        ''' <param name="start">the zero based start position of the block</param>
        ''' <param name="count">the number of the samples on the block</param>
        ''' <param name="width">the width of the output image in pixels</param>
        ''' <param name="height">the height of the output image in pixels</param>
        Public Function Oscillogram(data As Single(), start As Integer, count As Integer,
                                    width As Integer, height As Integer) As Double()()

            Dim image As Double()() = NewMatrix(height, width)
            Dim maxAbs As Double = 0

            For i As Integer = start To std.Min(data.Length - 1, start + count - 1)
                Dim v As Double = std.Abs(data(i))

                If v > maxAbs Then
                    maxAbs = v
                End If
            Next

            If maxAbs = 0 Then
                maxAbs = 1
            End If

            Dim block As Double = count / width
            Dim middle As Double = height / 2.0

            For x As Integer = 0 To width - 1
                Dim from As Integer = start + CInt(std.Floor(x * block))
                Dim [to] As Integer = std.Min(start + count, start + CInt(std.Floor((x + 1) * block)))
                Dim mn As Double = Double.MaxValue
                Dim mx As Double = Double.MinValue

                For i As Integer = from To [to] - 1
                    If i < 0 OrElse i >= data.Length Then
                        Continue For
                    End If

                    Dim v As Double = data(i)

                    If v < mn Then
                        mn = v
                    End If

                    If v > mx Then
                        mx = v
                    End If
                Next

                If mn > mx Then
                    Continue For
                End If

                Dim y1 As Integer = CInt(std.Round(middle - mx / maxAbs * (middle - 1)))
                Dim y2 As Integer = CInt(std.Round(middle - mn / maxAbs * (middle - 1)))

                For y As Integer = std.Max(0, std.Min(y1, y2)) To std.Min(height - 1, std.Max(y1, y2))
                    image(y)(x) = 1
                Next

                image(CInt(middle))(x) = 0.5 ' the zero reference line
            Next

            Return image
        End Function

        ''' <summary>
        ''' fold the signal at the given scan line period, one sample per pixel column
        ''' </summary>
        ''' <param name="data">the sample data of a single audio channel</param>
        ''' <param name="start">the zero based start position of the data section</param>
        ''' <param name="period">the scan line period in samples</param>
        ''' <param name="lines">the number of the scan lines</param>
        ''' <param name="samplesPerRow">the number of the sample columns, zero for the rounded period</param>
        Public Function Fold(data As Single(), start As Integer, period As Double, lines As Integer,
                             Optional samplesPerRow As Integer = 0) As Double()()

            If samplesPerRow <= 0 Then
                samplesPerRow = CInt(std.Floor(period))
            End If

            Dim image As Double()() = NewMatrix(lines, samplesPerRow)

            For r As Integer = 0 To lines - 1
                Dim origin As Double = start + r * period

                For c As Integer = 0 To samplesPerRow - 1
                    image(r)(c) = MathUtils.LerpClamped(data, origin + c)
                Next
            Next

            Return image
        End Function

        ''' <summary>
        ''' fold the signal at the scan line period and decode the intensity of each pixel cell,
        ''' this gives a fast preview of the decoded image
        ''' </summary>
        ''' <param name="data">the sample data of a single audio channel</param>
        ''' <param name="start">the zero based start position of the data section</param>
        ''' <param name="period">the scan line period in samples</param>
        ''' <param name="lines">the number of the scan lines</param>
        ''' <param name="pixels">the number of the pixel cells inside a scan line</param>
        ''' <param name="rms">use the rms value instead of the sum of the raw sample amplitudes</param>
        Public Function FoldPixel(data As Single(), start As Integer, period As Double, lines As Integer,
                                  pixels As Integer, Optional rms As Boolean = True) As Double()()

            Dim image As Double()() = NewMatrix(lines, pixels)
            Dim cell As Double = period / pixels

            For r As Integer = 0 To lines - 1
                Dim origin As Double = start + r * period

                For c As Integer = 0 To pixels - 1
                    Dim from As Double = origin + c * cell
                    Dim [to] As Double = from + cell
                    Dim v As Double = 0
                    Dim n As Integer = 0
                    Dim p As Integer = CInt(std.Floor(from))

                    While p < [to] AndAlso p < data.Length
                        If p >= 0 Then
                            If rms Then
                                v += data(p) ^ 2
                            Else
                                v += data(p)
                            End If

                            n += 1
                        End If

                        p += 1
                    End While

                    If n > 0 Then
                        image(r)(c) = If(rms, std.Sqrt(v / n), v / n)
                    End If
                Next
            Next

            Return image
        End Function

        ''' <summary>
        ''' decode the image data section into the transposed raster layout:
        ''' each scan line of the analog signal is one image column, and the pixel cells
        ''' inside the scan line are the image rows.
        ''' </summary>
        ''' <param name="data">the sample data of a single audio channel</param>
        ''' <param name="start">the zero based start position of the data section</param>
        ''' <param name="period">the scan line period in samples</param>
        ''' <param name="lines">the number of the scan lines</param>
        ''' <param name="cells">the number of the pixel cells inside a single scan line</param>
        ''' <param name="cellOffset">the number of the sample cells which are skipped at the
        ''' beginning of each scan line (the sync/blanking region)</param>
        ''' <param name="rms">use the rms value instead of the sum of the raw sample amplitudes</param>
        Public Function DecodeRaster(data As Single(), start As Integer, period As Double, lines As Integer,
                                     cells As Integer, Optional cellOffset As Integer = 0,
                                     Optional rms As Boolean = True) As Double()()

            Dim image As Double()() = NewMatrix(cells, lines)
            Dim cell As Double = period / (cells + cellOffset)

            For r As Integer = 0 To lines - 1
                Dim origin As Double = start + r * period

                For c As Integer = 0 To cells - 1
                    Dim from As Double = origin + (c + cellOffset) * cell
                    Dim [to] As Double = from + cell
                    Dim v As Double = 0
                    Dim n As Integer = 0
                    Dim p As Integer = CInt(std.Floor(from))

                    While p < [to] AndAlso p < data.Length
                        If p >= 0 Then
                            If rms Then
                                v += data(p) ^ 2
                            Else
                                v += data(p)
                            End If

                            n += 1
                        End If

                        p += 1
                    End While

                    If n > 0 Then
                        image(c)(r) = If(rms, std.Sqrt(v / n), v / n)
                    End If
                Next
            Next

            Return image
        End Function

        ''' <summary>
        ''' the measured bounding box of the calibration circle on the transposed raster,
        ''' it is used for calibrating the pixel clock: the calibration circle is a true
        ''' circle, so the horizontal extent (in scan lines) and the vertical extent
        ''' (in pixel cells) of its ring must be equal when the pixel clock is correct.
        ''' </summary>
        ''' <param name="data">the sample data of a single audio channel</param>
        ''' <param name="start">the zero based start position of the data section</param>
        ''' <param name="period">the scan line period in samples</param>
        ''' <param name="lines">the number of the scan lines</param>
        ''' <param name="cells">the number of the pixel cells inside a single scan line</param>
        ''' <param name="dx">the measured horizontal extent of the ring in scan lines</param>
        ''' <param name="dy">the measured vertical extent of the ring in pixel cells</param>
        ''' <param name="cellOffset">the number of the sample cells which are skipped at the
        ''' beginning of each scan line</param>
        Public Function MeasureCircle(data As Single(), start As Integer, period As Double, lines As Integer,
                                      cells As Integer, ByRef dx As Integer, ByRef dy As Integer,
                                      Optional cellOffset As Integer = 0) As Boolean

            Dim image As Double()() = DecodeRaster(data, start, period, lines, cells, cellOffset)
            Dim rowFrom As Integer = std.Min(cells - 1, std.Max(0, CInt(cells * 0.08)))
            Dim colFrom As Integer = std.Max(0, CInt(lines * 0.10))
            Dim colTo As Integer = std.Min(lines - 1, CInt(lines * 0.92))
            Dim values As New List(Of Double)

            For y As Integer = rowFrom To cells - 1
                For x As Integer = colFrom To colTo
                    values.Add(image(y)(x))
                Next
            Next

            Dim threshold As Double = MathUtils.Percentile(values.ToArray, 0.975)
            Dim minX As Integer = Integer.MaxValue
            Dim maxX As Integer = Integer.MinValue
            Dim minY As Integer = Integer.MaxValue
            Dim maxY As Integer = Integer.MinValue

            For y As Integer = rowFrom To cells - 1
                For x As Integer = colFrom To colTo
                    If image(y)(x) >= threshold Then
                        If x < minX Then minX = x
                        If x > maxX Then maxX = x
                        If y < minY Then minY = y
                        If y > maxY Then maxY = y
                    End If
                Next
            Next

            If minX > maxX Then
                dx = 0
                dy = 0

                Return False
            End If

            dx = maxX - minX + 1
            dy = maxY - minY + 1

            Return True
        End Function

        ''' <summary>
        ''' the mean scan line waveform, the sync pulse shape is highlighted by the averaging
        ''' while the image payload content is cancelled out
        ''' </summary>
        ''' <param name="data">the sample data of a single audio channel</param>
        ''' <param name="start">the zero based start position of the data section</param>
        ''' <param name="period">the scan line period in samples</param>
        ''' <param name="lines">the number of the scan lines</param>
        ''' <param name="samplesPerRow">the number of the sample columns, zero for the rounded period</param>
        Public Function MeanLine(data As Single(), start As Integer, period As Double, lines As Integer,
                                 Optional samplesPerRow As Integer = 0) As Double()

            If samplesPerRow <= 0 Then
                samplesPerRow = CInt(std.Floor(period))
            End If

            Dim mean As Double() = New Double(samplesPerRow - 1) {}

            For r As Integer = 0 To lines - 1
                Dim origin As Double = start + r * period

                For c As Integer = 0 To samplesPerRow - 1
                    mean(c) += MathUtils.LerpClamped(data, origin + c)
                Next
            Next

            For c As Integer = 0 To samplesPerRow - 1
                mean(c) /= lines
            Next

            Return mean
        End Function

        ''' <summary>
        ''' the amplitude statistics of each scan line, it is used for checking the
        ''' baseline drift of the analog recording
        ''' </summary>
        ''' <param name="data">the sample data of a single audio channel</param>
        ''' <param name="start">the zero based start position of the data section</param>
        ''' <param name="period">the scan line period in samples</param>
        ''' <param name="lines">the number of the scan lines</param>
        ''' <param name="samplesPerRow">the number of the sample columns, zero for the rounded period</param>
        Public Function LineStats(data As Single(), start As Integer, period As Double, lines As Integer,
                                  Optional samplesPerRow As Integer = 0) As LineStat()

            If samplesPerRow <= 0 Then
                samplesPerRow = CInt(std.Floor(period))
            End If

            Dim result As LineStat() = New LineStat(lines - 1) {}

            For r As Integer = 0 To lines - 1
                Dim origin As Integer = start + CInt(std.Round(r * period))
                Dim [end] As Integer = std.Min(data.Length, origin + samplesPerRow)
                Dim sum As Double = 0
                Dim square As Double = 0
                Dim peak As Double = 0
                Dim n As Integer = [end] - origin

                For i As Integer = origin To [end] - 1
                    Dim v As Double = data(i)

                    sum += v
                    square += v * v

                    If std.Abs(v) > peak Then
                        peak = std.Abs(v)
                    End If
                Next

                If n <= 0 Then
                    n = 1
                End If

                result(r) = New LineStat With {
                    .index = r,
                    .mean = sum / n,
                    .rms = std.Sqrt(square / n),
                    .peak = peak,
                    .start = origin
                }
            Next

            Return result
        End Function

        ''' <summary>
        ''' allocate a row major double matrix
        ''' </summary>
        Private Function NewMatrix(rows As Integer, cols As Integer) As Double()()
            Dim m As Double()() = New Double(rows - 1)() {}

            For i As Integer = 0 To rows - 1
                m(i) = New Double(cols - 1) {}
            Next

            Return m
        End Function
    End Module
End Namespace
