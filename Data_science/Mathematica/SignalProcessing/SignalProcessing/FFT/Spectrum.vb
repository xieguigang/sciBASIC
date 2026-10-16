Imports System.Linq
Imports std = System.Math

Namespace FFT

    ''' <summary>
    ''' the magnitude spectrum helpers which are built on the
    ''' <see cref="Radix2"/> transform.
    ''' </summary>
    Public Module Spectrum

        ''' <summary>
        ''' complex spectrum of a real valued signal, the length of the result is ``n/2+1``.
        ''' 
        ''' the signal is zero padded into the next power of two before the transform, and
        ''' the magnitude of each bin is scaled by the half length of the original signal,
        ''' so the result is directly comparable between the signals of the different length.
        ''' </summary>
        ''' <param name="x">the real valued signal samples</param>
        ''' <param name="window">apply the hann window on the signal before the transform</param>
        Public Function PowerSpectrum(x As Double(), Optional window As Boolean = True) As Double()
            Dim n As Integer = Radix2.RoundupPow2(x.Length)
            Dim re As Double() = New Double(n - 1) {}
            Dim im As Double() = New Double(n - 1) {}

            For i As Integer = 0 To x.Length - 1
                Dim w As Double = If(window, 0.5 - 0.5 * std.Cos(2 * std.PI * i / (x.Length - 1)), 1.0)

                re(i) = x(i) * w
            Next

            Call Radix2.Fft(re, im)

            Dim spec As Double() = New Double(n \ 2) {}

            For i As Integer = 0 To spec.Length - 1
                spec(i) = std.Sqrt(re(i) * re(i) + im(i) * im(i)) / (x.Length / 2)
            Next

            Return spec
        End Function

        ''' <summary>
        ''' the top magnitude positions (hertz) on the given signal spectrum
        ''' </summary>
        ''' <param name="signal">the real valued signal samples</param>
        ''' <param name="sampleRate">the sample rate of the signal in hertz</param>
        ''' <param name="maxHz">the upper frequency limit of the peak search</param>
        Public Function TopSpectralPeaks(signal As Double(), sampleRate As Integer,
                                         Optional maxHz As Double = 100000) As Double()

            Dim spectrum As Double() = PowerSpectrum(signal)
            Dim binHz As Double = sampleRate / (2.0 * (spectrum.Length - 1))
            Dim bins As List(Of Double) = spectrum _
                .Skip(2) _
                .Take(std.Min(spectrum.Length - 2, CInt(maxHz / binHz))) _
                .Select(Function(x, i) New KeyValuePair(Of Integer, Double)(i + 2, x)) _
                .OrderByDescending(Function(x) x.Value) _
                .Take(12) _
                .OrderBy(Function(x) x.Key) _
                .Select(Function(x) (x.Key - 2) * binHz) _
                .ToList

            Return bins.ToArray
        End Function
    End Module
End Namespace
