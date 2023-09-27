Imports std = System.Math

Namespace Imaging.Math2D

    ''' <summary>
    ''' This function is useful for calculating Moran's I 
    ''' Without holding an entire distance matrix in memory
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/mcooper/moranfast/blob/master/src/calc_moran.cpp
    ''' </remarks>
    Public Module Moran

        Public Function distanceCalculate(x1 As Double, y1 As Double, x2 As Double, y2 As Double) As Double
            Dim x = x1 - x2
            Dim y = y1 - y2

            If x = 0.0 AndAlso y = 0.0 Then
                Return 0
            Else
                Return 1 / std.Sqrt(x ^ 2 + y ^ 2)
            End If
        End Function

        Public Function normalize(x As Double()) As Double()
            Dim x_bar = x.Average
            Dim x_norm As Double() = Math.SIMD.Subtract.f64_op_subtract_f64_scalar(x, x_bar)

            Return x_norm
        End Function

        Public Function calc_moran(x As Double(), c1 As Double(), c2 As Double()) As (observed As Double, expected As Double, sd As Double)
            ' Easy variables to calculate
            Dim x_norm = normalize(x)
            Dim N = x.Length
            Dim denom = Math.SIMD.Multiply.f64_op_multiply_f64(x_norm, x_norm).Sum
            ' Variables to calculate through iteration
            Dim w As Double = 0
            Dim num As Double = 0
            Dim S1, S2 As Double

            For i As Integer = 0 To N - 1
                Dim S2_a As Double = 0
                For j As Integer = 0 To N - 1
                    Dim w_ij = distanceCalculate(c1(i), c2(i), c1(j), c2(j))
                    w += w_ij
                    Dim w_x1_x2 = w_ij * x_norm(i) * x_norm(j)
                    num += w_x1_x2
                    S1 += (2 * w_ij) ^ 2
                    S2_a += w_ij
                Next

                S2 += (2 * S2_a) ^ 2
            Next

            S1 = S1 / 2

            Dim ei = -(1 / (N - 1))
            Dim k = (Math.SIMD.Divide.f64_op_divide_f64_scalar(Math.SIMD.Exponent.f64_op_exponent_f64_scalar(x_norm, 4), N).Sum) / ((denom / N) ^ 2)
            Dim up = (N * (((N ^ 2) - 3 * N + 3) * S1 - N * S2 + 3 * (w ^ 2)) - k * (N * (N - 1) * S1 - 2 * N * S2 + 6 * (w ^ 2)))
            Dim down = ((N - 1) * (N - 2) * (N - 3) * (w ^ 2))
            Dim sd2 = up / down - (ei ^ 2)
            Dim sd = std.Sqrt(std.Abs(sd2))
            Dim moran = (N / w) * (num / denom)

            Return (moran, ei, sd)
        End Function
    End Module
End Namespace