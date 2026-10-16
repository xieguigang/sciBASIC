Imports std = System.Math

Namespace Waves

    ''' <summary>
    ''' the per pixel cell intensity estimator of the analog signal
    ''' </summary>
    Public Enum DemodulationMode

        ''' <summary>
        ''' the signed mean of the cell samples.
        ''' 
        ''' this is the estimator which is used by the legacy decoder and it gives the
        ''' cleanest result: the image brightness is the local level of the analog signal,
        ''' and the plain mean is a linear low pass filter of the sample data.
        ''' </summary>
        Mean = 0

        ''' <summary>
        ''' the mean of the absolute value of the cell samples (the full wave rectified
        ''' amplitude). the rectification is a non linear operation: the noise of the
        ''' analog signal is folded into a positive offset and it is amplified, so the
        ''' decoded image is noisier than the ``Mean`` mode.
        ''' </summary>
        Absolute

        ''' <summary>
        ''' the root mean square of the cell samples
        ''' </summary>
        Rms
    End Enum

    ''' <summary>
    ''' the demodulation parameters of a scan line analog signal.
    ''' 
    ''' the pixel cell number of a single scan line and the samples per pixel cell (the
    ''' pixel clock) are the properties of the concrete data source, they are NOT included
    ''' in this class as the default values: the caller must assign
    ''' <see cref="pixels"/> and either <see cref="pixel_pitch"/> or
    ''' <see cref="slots_per_period"/>.
    ''' </summary>
    Public Class DemodulateOptions

        ''' <summary>
        ''' the number of the image pixel cells inside a single scan line (the image rows),
        ''' it must be assigned by the caller
        ''' </summary>
        ''' <returns></returns>
        Public Property pixels As Integer = 0

        ''' <summary>
        ''' the samples per pixel cell, zero means it is evaluated from the scan line
        ''' period and the pixel clock slots of the data source
        ''' (see <see cref="slots_per_period"/>)
        ''' </summary>
        ''' <returns></returns>
        Public Property pixel_pitch As Double = 0

        ''' <summary>
        ''' the number of the pixel cell slots inside a single scan line period.
        ''' 
        ''' one scan line period usually contains more slots than the image pixel number
        ''' because of the leading blanking/sync region, so the pixel pitch is NOT
        ''' ``period / pixels``: using the image pixel number as the slot count would miss
        ''' the blanking region and would scale the decoded image in the vertical
        ''' direction. this value must be calibrated from the known geometry of the data
        ''' source (for example a calibration circle image), and it is only used when
        ''' <see cref="pixel_pitch"/> is not assigned.
        ''' </summary>
        ''' <returns></returns>
        Public Property slots_per_period As Double = 0

        ''' <summary>
        ''' the number of the samples which are skipped after the sync pulse position
        ''' </summary>
        ''' <returns></returns>
        Public Property guard As Double = 0

        ''' <summary>
        ''' remove the per scan line baseline drift before the rectification
        ''' </summary>
        ''' <returns></returns>
        Public Property remove_baseline As Boolean = True

        ''' <summary>
        ''' the intensity estimator of a pixel cell, the default ``Mean`` mode is the
        ''' signed mean which matches the legacy decoder result
        ''' </summary>
        ''' <returns></returns>
        Public Property mode As DemodulationMode = DemodulationMode.Mean

        ''' <summary>
        ''' use the rms value instead of the signed mean of the cell samples
        ''' </summary>
        ''' <returns></returns>
        Public Property use_rms As Boolean = False

        ''' <summary>
        ''' the length (in samples) of the baseline tracking window, zero for the
        ''' whole scan line mean value
        ''' </summary>
        ''' <returns></returns>
        Public Property baseline_window As Integer = 0
    End Class

    ''' <summary>
    ''' the brightness demodulation of a scan line analog signal.
    ''' 
    ''' the pixel cells are sampled with a sub sample precision, and the baseline drift of
    ''' each scan line is removed before the intensity combination, those two steps are
    ''' required for removing the "water wave" like streaks on the decoded image.
    ''' </summary>
    Public Module CellDemodulator

        ''' <summary>
        ''' the samples per pixel cell which is evaluated from the scan line period and the
        ''' pixel clock slots of the data source.
        ''' </summary>
        ''' <param name="period">the measured scan line period in samples</param>
        ''' <param name="slotsPerPeriod">the number of the pixel cell slots inside a single
        ''' scan line period (including the blanking region)</param>
        Public Function PixelPitch(period As Double, slotsPerPeriod As Double) As Double
            If slotsPerPeriod <= 0 Then
                Throw New ArgumentException("the pixel cell slots per scan line period must be a positive number!")
            End If

            Return period / slotsPerPeriod
        End Function

        ''' <summary>
        ''' parse the name of the intensity estimator: ``mean``/``signed``, ``abs`` and ``rms``
        ''' </summary>
        ''' <param name="name">the estimator name</param>
        Public Function ParseMode(name As String) As DemodulationMode
            Select Case If(name, "").Trim.ToLower
                Case "mean", "signed", "" : Return DemodulationMode.Mean
                Case "abs", "absolute", "rectified" : Return DemodulationMode.Absolute
                Case "rms" : Return DemodulationMode.Rms
                Case Else
                    Throw New ArgumentException($"invalid demodulation mode name '{name}', it should be mean, abs or rms!")
            End Select
        End Function

        ''' <summary>
        ''' demodulate the scan line grid into the image raster.
        ''' 
        ''' the scan line of the analog signal is one image column, so the returned
        ''' matrix is already in the image row major layout: ``matrix(row)(column)``
        ''' </summary>
        ''' <param name="data">the sample data of a single audio channel</param>
        ''' <param name="grid">the scan line time grid</param>
        ''' <param name="options"></param>
        Public Function Demodulate(data As Single(), grid As LineGrid, options As DemodulateOptions) As Single()()
            If options.pixels <= 0 Then
                Throw New ArgumentException("the pixel cell number of a single scan line must be assigned!")
            End If

            Dim pitch As Double = options.pixel_pitch

            If pitch <= 0 Then
                pitch = PixelPitch(grid.period, options.slots_per_period)
            End If

            Dim rows As Integer = options.pixels
            Dim cols As Integer = grid.rows.Length
            Dim image As Single()() = New Single(rows - 1)() {}

            For i As Integer = 0 To rows - 1
                image(i) = New Single(cols - 1) {}
            Next

            For c As Integer = 0 To cols - 1
                Dim origin As Double = grid.rows(c).start + grid.sync_offset + options.guard
                Dim baseline As Double = 0

                If options.remove_baseline Then
                    baseline = LineBaseline(data, origin, grid.period, options.baseline_window)
                End If

                For r As Integer = 0 To rows - 1
                    image(r)(c) = PixelValue(data, origin + r * pitch, pitch, baseline, options.mode)
                Next
            Next

            Return image
        End Function

        ''' <summary>
        ''' the area weighted pixel intensity of a pixel cell which is started at the
        ''' given fractional sample position.
        ''' 
        ''' the samples of the cell are combined by the given estimator, the ``Mean`` mode
        ''' (the signed mean) is a linear low pass filter of the analog signal and it is
        ''' the estimator which gives the cleanest image, the ``Absolute``/``Rms`` modes
        ''' rectifies the signal before the combination and they are much noisier.
        ''' </summary>
        ''' <param name="data">the sample data of a single audio channel</param>
        ''' <param name="start">the fractional sample position of the cell start</param>
        ''' <param name="pitch">the samples per pixel cell</param>
        ''' <param name="baseline">the baseline level which is removed from the samples</param>
        ''' <param name="mode">the intensity estimator</param>
        Public Function PixelValue(data As Single(), start As Double, pitch As Double,
                                   baseline As Double, mode As DemodulationMode) As Single

            Dim [end] As Double = start + pitch
            Dim i0 As Integer = CInt(std.Floor(start))
            Dim i1 As Integer = CInt(std.Floor([end]))
            Dim frac0 As Double = start - i0
            Dim frac1 As Double = [end] - i1
            Dim sum As Double = 0
            Dim weight As Double = 0

            Dim leftWeight As Double = 1 - frac0

            If leftWeight > 0 Then
                Call Accumulate(sum, weight, MathUtils.SampleOrZero(data, i0) - baseline, leftWeight, mode)
            End If

            For i As Integer = i0 + 1 To i1 - 1
                Call Accumulate(sum, weight, MathUtils.SampleOrZero(data, i) - baseline, 1, mode)
            Next

            If frac1 > 0 Then
                Call Accumulate(sum, weight, MathUtils.SampleOrZero(data, i1) - baseline, frac1, mode)
            End If

            If weight <= 0 Then
                Return 0
            End If

            Dim value As Double = sum / weight

            Return If(mode = DemodulationMode.Rms, std.Sqrt(value), value)
        End Function

        Private Sub Accumulate(ByRef sum As Double, ByRef weight As Double, v As Double,
                               w As Double, mode As DemodulationMode)
            Select Case mode
                Case DemodulationMode.Mean
                    sum += v * w
                Case DemodulationMode.Absolute
                    sum += std.Abs(v) * w
                Case Else
                    sum += v * v * w
            End Select

            weight += w
        End Sub

        ''' <summary>
        ''' the baseline of a scan line: the mean level of the analog signal on the line.
        ''' 
        ''' the baseline is removed before the amplitude combination, otherwise the baseline
        ''' drift of the analog recording appears as the streaks on the decoded image.
        ''' </summary>
        ''' <param name="data">the sample data of a single audio channel</param>
        ''' <param name="origin">the fractional sample position of the scan line start</param>
        ''' <param name="period">the scan line period in samples</param>
        ''' <param name="window">the length of the baseline tracking window, zero for the
        ''' whole scan line mean value (the local tracking window is not supported by the
        ''' whole line scan, the whole line mean value is used instead)</param>
        Public Function LineBaseline(data As Single(), origin As Double, period As Double,
                                     window As Integer) As Double

            If window > 0 Then
                ' the local baseline is not supported by the whole line scan,
                ' the whole line mean value is used instead
                window = 0
            End If

            Dim i0 As Integer = std.Max(0, CInt(std.Floor(origin)))
            Dim i1 As Integer = std.Min(data.Length, CInt(std.Floor(origin + period)))
            Dim sum As Double = 0
            Dim n As Integer = 0

            For i As Integer = i0 To i1 - 1
                sum += data(i)
                n += 1
            Next

            If n = 0 Then
                Return 0
            End If

            Return sum / n
        End Function
    End Module
End Namespace
