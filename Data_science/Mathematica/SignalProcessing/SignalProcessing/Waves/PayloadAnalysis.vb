Imports System.Linq
Imports std = System.Math

Namespace Waves

    ''' <summary>
    ''' how the brightness of the image pixel is modulated on the carrier of
    ''' the periodic scan line signal
    ''' </summary>
    Public Structure PayloadAnalysis
        Public peaks_per_pixel As Double
        Public spectral_peaks As Double()
        Public high_band_ratio As Double
        Public encoding As String
        Public notes As String()
    End Structure

    ''' <summary>
    ''' determine how the brightness of the image pixel is modulated on the carrier.
    ''' 
    ''' the analysis is done on a middle block of the data section (8 scan lines), it
    ''' measures the number of the pulses inside a single pixel cell, the spectrum of the
    ''' payload block and the ratio of the energy which is above the pixel clock nyquist
    ''' frequency, then it classifies the brightness encoding scheme.
    ''' </summary>
    Public Module ModulationAnalysis

        ''' <summary>
        ''' determine how the brightness of the image pixel is modulated on the carrier
        ''' </summary>
        ''' <param name="data">the sample data of a single audio channel</param>
        ''' <param name="pulses">the sub sample positions of the sync pulses</param>
        ''' <param name="samplesPerPixel">the measured samples per pixel cell</param>
        ''' <param name="sampleRate">the sample rate of the signal in hertz, it is used for
        ''' converting the spectrum bins into the frequency</param>
        ''' <param name="maxHz">the upper frequency limit of the reported spectral peaks</param>
        Public Function AnalysePayload(data As Single(), pulses As Double(), samplesPerPixel As Double,
                                       sampleRate As Integer, Optional maxHz As Double = 12000) As PayloadAnalysis

            Dim result As New PayloadAnalysis
            Dim notes As New List(Of String)
            Dim start As Integer
            Dim [end] As Integer

            If pulses.Length >= 2 AndAlso samplesPerPixel > 0 Then
                ' take 8 scan lines from the middle of the data section
                Dim mid As Integer = pulses.Length \ 2
                start = CInt(std.Floor(pulses(mid)))
                [end] = CInt(std.Floor(pulses(std.Min(pulses.Length - 1, mid + 8))))
            Else
                start = 0
                [end] = std.Min(data.Length, 4096 * 8)
            End If

            If [end] - start < 128 Then
                result.encoding = "unknown"
                result.notes = {"not enough data for the payload analysis!"}
                result.spectral_peaks = New Double() {}
                Return result
            End If

            Dim block As Single() = data.Skip(start).Take([end] - start).ToArray
            Dim window As Integer = std.Min(block.Length, 8192)
            Dim spectrumInput As Double() = New Double(window - 1) {}

            For i As Integer = 0 To window - 1
                spectrumInput(i) = block(i + (block.Length - window) \ 2)
            Next

            ' only report the low frequency peaks
            result.spectral_peaks = FFT.Spectrum.TopSpectralPeaks(spectrumInput, sampleRate, maxHz)

            ' narrow band peak counting: the mean number of the signal peaks inside
            ' a single pixel cell
            Dim cell As Integer = std.Max(1, CInt(std.Round(samplesPerPixel)))
            Dim pixelCount As Integer = block.Length \ cell
            Dim peakCount As Double = 0
            Dim cells As Integer = 0
            Dim energy0 As Double = 0
            Dim energy1 As Double = 0

            For p As Integer = 0 To pixelCount - 1
                Dim cellStart As Integer = p * cell
                Dim peak As Double = 0
                Dim energy As Double = 0

                For i As Integer = cellStart To cellStart + cell - 1
                    Dim v As Double = std.Abs(block(i))

                    If v > peak Then
                        peak = v
                    End If

                    energy += block(i) * block(i)
                Next

                ' a "pulse" inside the cell: the peak magnitude is far above the cell mean
                If energy > 0 AndAlso peak / std.Sqrt(energy / cell) > 2.5 Then
                    peakCount += 1
                End If

                cells += 1
            Next

            If cells > 0 Then
                result.peaks_per_pixel = peakCount / cells
            End If

            ' the high band energy ratio: the energy above the pixel clock nyquist
            Dim spec As Double() = FFT.Spectrum.PowerSpectrum(spectrumInput)
            Dim nyquistBin As Double = (spec.Length - 1) / samplesPerPixel

            For i As Integer = 1 To spec.Length - 1
                If i > nyquistBin Then
                    energy1 += spec(i) * spec(i)
                Else
                    energy0 += spec(i) * spec(i)
                End If
            Next

            Dim total As Double = energy0 + energy1

            result.high_band_ratio = If(total = 0, 0, energy1 / total)

            ' ------------------------------------------------------------------
            ' determine the brightness encoding scheme
            ' ------------------------------------------------------------------
            If result.peaks_per_pixel > 1.35 Then
                result.encoding = "pulse-density"
            ElseIf result.peaks_per_pixel > 0.65 Then
                result.encoding = "pulse-amplitude"
            Else
                result.encoding = "amplitude-continuous"
            End If

            Call notes.Add($"payload block: {block.Length} samples, {pixelCount} pixel cells of {cell} samples")

            result.notes = notes.ToArray

            Return result
        End Function
    End Module
End Namespace
