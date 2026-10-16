Imports System.Linq
Imports std = System.Math

Namespace Waves

    ''' <summary>
    ''' the energy distribution over the frequency bands and the narrow band chroma
    ''' carrier measurement of a multi channel signal
    ''' </summary>
    Public Structure SpectrumBands
        Public band_power As Dictionary(Of String, Double)
        Public has_chroma_component As Boolean
        Public chroma_candidate_hz As Double
        Public chroma_band_ratio As Double
        Public notes As String()
    End Structure

    ''' <summary>
    ''' the spectrum band analysis of a (possibly color) periodic scan line signal.
    ''' 
    ''' the luminance data of the image is a wide band amplitude modulation, while a color
    ''' (chroma) component must be carried by a narrow band sub carrier which is outside of
    ''' the luminance band, so the ratio between a narrow band peak and its local noise
    ''' floor is a strong indicator of the existence of a chroma carrier.
    ''' </summary>
    Public Module BandAnalysis

        ''' <summary>
        ''' the frequency bands for the energy distribution analysis
        ''' </summary>
        Public ReadOnly BandEdges As Double() = {0, 5000, 10000, 20000, 30000, 44700, 60000, 80000, 100000, 150000, 192000}

        ''' <summary>
        ''' detect the narrow band chroma carrier (if any) which is independent from the
        ''' luminance data
        ''' </summary>
        ''' <param name="raw">the sample data of the channels, one array per channel</param>
        ''' <param name="sampleRate">the sample rate of the signal in hertz</param>
        ''' <param name="frequencyEdges">the frequency edges of the reported energy bands, nothing for <see cref="BandEdges"/></param>
        ''' <param name="frameSize">the length of the spectrum analysis frame in samples</param>
        ''' <param name="hop">the hop between two consecutive spectrum frames</param>
        ''' <param name="maxFrames">the maximal number of the frames on a single channel</param>
        Public Function AnalyseSpectrumBands(raw As Single()(), sampleRate As Integer,
                                             Optional frequencyEdges As Double() = Nothing,
                                             Optional frameSize As Integer = 4096,
                                             Optional hop As Integer = 2048,
                                             Optional maxFrames As Integer = 256) As SpectrumBands

            Dim edges As Double() = If(frequencyEdges, BandEdges)
            Dim result As New SpectrumBands With {
                .band_power = New Dictionary(Of String, Double),
                .notes = New String() {}
            }
            Dim bands As New Dictionary(Of String, Double)
            Dim accum As Double() = Nothing
            Dim binHz As Double

            For c As Integer = 0 To raw.Length - 1
                Dim data As Single() = raw(c)
                Dim power As Double() = Nothing
                Dim frames As Integer = 0

                If accum Is Nothing Then
                    Dim spec As Double() = FFT.Spectrum.PowerSpectrum(ToDoubles(data.Take(frameSize).ToArray))
                    accum = New Double(spec.Length - 1) {}
                    binHz = sampleRate / (2.0 * (spec.Length - 1))
                End If

                Dim pos As Integer = 0

                While pos + frameSize < data.Length AndAlso frames < maxFrames
                    Dim block As Double() = New Double(frameSize - 1) {}

                    For i As Integer = 0 To frameSize - 1
                        block(i) = data(pos + i)
                    Next

                    Dim spec As Double() = FFT.Spectrum.PowerSpectrum(block)

                    If power Is Nothing Then
                        power = New Double(spec.Length - 1) {}
                    End If

                    For i As Integer = 0 To spec.Length - 1
                        power(i) += spec(i)
                    Next

                    pos += hop
                    frames += 1
                End While

                If power IsNot Nothing Then
                    For i As Integer = 0 To std.Min(accum.Length, power.Length) - 1
                        accum(i) += power(i) / frames
                    Next
                End If
            Next

            If accum Is Nothing Then
                result.band_power = bands
                result.notes = {"no enough data for the spectrum analysis!"}

                Return result
            End If

            ' energy distribution over the frequency bands
            For i As Integer = 0 To edges.Length - 2
                Dim sum As Double = 0
                Dim name As String = $"{edges(i) / 1000:F0}-{edges(i + 1) / 1000:F0} kHz"

                For b As Integer = 0 To accum.Length - 1
                    Dim f As Double = b * binHz

                    If f >= edges(i) AndAlso f < edges(i + 1) Then
                        sum += accum(b) * accum(b)
                    End If
                Next

                bands(name) = sum
            Next

            Dim total As Double = bands.Values.Sum
            Dim relative As New Dictionary(Of String, Double)

            For Each band In bands
                relative(band.Key) = If(total = 0, 0, band.Value / total)
            Next

            result.band_power = relative

            ' narrow band chroma carrier detection: a strong peak above the luminance band
            Dim minBin As Integer = CInt(60000 / binHz)
            Dim maxBin As Integer = std.Min(accum.Length - 1, CInt(150000 / binHz))
            Dim peakBin As Integer = -1
            Dim peakValue As Double = 0

            For b As Integer = std.Max(2, minBin) To maxBin
                Dim local As Double = LocalMedian(accum, b, 32)

                If local <= 0 Then
                    Continue For
                End If

                Dim score As Double = accum(b) / local

                If score > peakValue Then
                    peakValue = score
                    peakBin = b
                End If
            Next

            If peakBin > 0 Then
                result.chroma_candidate_hz = peakBin * binHz
                result.chroma_band_ratio = peakValue
                result.has_chroma_component = peakValue > 6.0
            End If

            Return result
        End Function

        Private Function LocalMedian(spectrum As Double(), i As Integer, radius As Integer) As Double
            Dim from As Integer = std.Max(0, i - radius)
            Dim [to] As Integer = std.Min(spectrum.Length - 1, i + radius)
            Dim buffer As Double() = New Double([to] - from) {}

            Array.Copy(spectrum, from, buffer, 0, buffer.Length)
            Array.Sort(buffer)

            Return buffer(buffer.Length \ 2)
        End Function

        ''' <summary>
        ''' convert a single precision channel into the double precision sample buffer
        ''' </summary>
        Private Function ToDoubles(x As Single()) As Double()
            Dim d As Double() = New Double(x.Length - 1) {}

            For i As Integer = 0 To x.Length - 1
                d(i) = x(i)
            Next

            Return d
        End Function
    End Module
End Namespace
