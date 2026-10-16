Imports std = System.Math

Namespace FFT

    ''' <summary>
    ''' the in place radix-2 complex fast fourier transformation on the split
    ''' real/imaginary data arrays.
    ''' 
    ''' ### the difference from the other FFT implementations of current library
    ''' 
    ''' the library already contains three other fourier transformation implementations,
    ''' they are **not** interchangeable with this one because of the data layout and the
    ''' scaling convention:
    ''' 
    ''' | implementation | data layout | scaling on the forward transform | length limit |
    ''' |----------------|-------------|----------------------------------|--------------|
    ''' | ``FFT.Radix2`` (this module) | two separate ``Double()`` arrays | **no** normalization | any power of two |
    ''' | ``FFT.FourierTransform.FFT`` | ``Complex()`` | divides by ``n`` | ``n`` in ``[2, 16384]`` |
    ''' | ``FFT.DoubleFFT_1D`` | interleaved complex | IEEE convention | arbitrary |
    ''' | ``FFT.TFftAlgorithm`` | real input, cos/sin tables | ``2/n`` | arbitrary |
    ''' 
    ''' the caller of this module is responsible for the normalization: the forward
    ''' transform keeps the raw butterfly sums, so a magnitude spectrum must be scaled
    ''' by the window gain itself (see <see cref="Spectrum.PowerSpectrum(Double(), Boolean)"/>).
    ''' </summary>
    Public Module Radix2

        ''' <summary>
        ''' in-place radix-2 complex FFT, the length of the data must be a power of two
        ''' </summary>
        ''' <param name="re">the real part of the complex signal, modified in place</param>
        ''' <param name="im">the imaginary part of the complex signal, modified in place</param>
        ''' <param name="inverse">run the inverse transform (the result is divided by ``n``)</param>
        Public Sub Fft(re As Double(), im As Double(), Optional inverse As Boolean = False)
            Dim n As Integer = re.Length

            If n <= 1 Then
                Return
            End If

            If (n And (n - 1)) <> 0 Then
                Throw New ArgumentException($"the data length {n} of the FFT is not a power of two!")
            End If

            ' bit reversal permutation
            Dim j As Integer = 0
            For i As Integer = 0 To n - 1
                If i < j Then
                    Dim tmp As Double = re(i)
                    re(i) = re(j)
                    re(j) = tmp
                    tmp = im(i)
                    im(i) = im(j)
                    im(j) = tmp
                End If

                Dim m As Integer = n >> 1

                While j >= m AndAlso m > 0
                    j -= m
                    m >>= 1
                End While

                j += m
            Next

            ' butterfly
            Dim len As Integer = 2
            Dim sign As Double = If(inverse, 1.0, -1.0)

            While len <= n
                Dim ang As Double = 2 * std.PI / len * sign
                Dim wr As Double = std.Cos(ang)
                Dim wi As Double = std.Sin(ang)
                Dim step_ As Integer = len \ 2
                Dim i As Integer = 0

                While i < n
                    Dim cr As Double = 1.0
                    Dim ci As Double = 0.0

                    For k As Integer = 0 To step_ - 1
                        Dim xr As Double = re(i + k + step_)
                        Dim xi As Double = im(i + k + step_)
                        Dim vr As Double = xr * cr - xi * ci
                        Dim vi As Double = xr * ci + xi * cr
                        Dim ur As Double = re(i + k)
                        Dim ui As Double = im(i + k)

                        re(i + k) = ur + vr
                        im(i + k) = ui + vi
                        re(i + k + step_) = ur - vr
                        im(i + k + step_) = ui - vi

                        Dim ncr As Double = cr * wr - ci * wi
                        ci = cr * wi + ci * wr
                        cr = ncr
                    Next

                    i += len
                End While

                len <<= 1
            End While

            If inverse Then
                For i As Integer = 0 To n - 1
                    re(i) /= n
                    im(i) /= n
                Next
            End If
        End Sub

        ''' <summary>
        ''' round the input value up to the nearest power of two, it is used for
        ''' zero padding a signal into the size which the radix-2 FFT requires.
        ''' </summary>
        Public Function RoundupPow2(n As Integer) As Integer
            Dim p As Integer = 1

            While p < n
                p <<= 1
            End While

            Return p
        End Function
    End Module
End Namespace
