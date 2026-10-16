Imports System.Linq
Imports std = System.Math

Namespace Waves

    ''' <summary>
    ''' the relationship between two audio channels of a stereo signal: the signal energy
    ''' of each channel, the best correlation lag (in samples) and the correlation
    ''' coefficient at that lag
    ''' </summary>
    Public Structure ChannelCorrelation
        Public best_lag As Integer
        Public correlation As Double
        Public energy_left As Double
        Public energy_right As Double
    End Structure

    ''' <summary>
    ''' the correlation analysis between the left and the right channels.
    ''' 
    ''' the two channels of the analog recording may carry the same signal with a small
    ''' time offset or they may carry two different signal components (for example a
    ''' staggered track layout), the best correlation lag tells which case it is.
    ''' </summary>
    Public Module ChannelAnalysis

        ''' <summary>
        ''' the correlation relationship between the left and the right channel
        ''' </summary>
        ''' <param name="left">the sample data of the left channel</param>
        ''' <param name="right">the sample data of the right channel</param>
        ''' <param name="k">the decimation factor for the correlation computation</param>
        ''' <param name="maxLagLimit">the maximal lag (in samples) of the correlation search</param>
        ''' <param name="maxCount">the maximal number of the decimated samples which are used</param>
        Public Function AnalyseChannels(left As Single(), right As Single(),
                                        Optional k As Integer = 16,
                                        Optional maxLagLimit As Integer = 400,
                                        Optional maxCount As Integer = 32768) As ChannelCorrelation

            Dim result As New ChannelCorrelation With {
                .energy_left = MathUtils.Rms(left),
                .energy_right = MathUtils.Rms(right)
            }
            Dim l As Double() = ToDoubles(left)
            Dim r As Double() = ToDoubles(right)
            Dim decL As Double() = MathUtils.Decimate(l, k)
            Dim decR As Double() = MathUtils.Decimate(r, k)
            Dim n As Integer = std.Min(std.Min(decL.Length, decR.Length), maxCount)
            Dim maxLag As Integer = std.Min(maxLagLimit, n \ 4)
            Dim bestScore As Double = Double.MinValue
            Dim bestLag As Integer = 0
            Dim meanL As Double = 0
            Dim meanR As Double = 0

            For i As Integer = 0 To n - 1
                meanL += decL(i)
                meanR += decR(i)
            Next

            meanL /= n
            meanR /= n

            For lag As Integer = -maxLag To maxLag
                Dim sum As Double = 0

                For i As Integer = 0 To n - 1
                    Dim j As Integer = i + lag

                    If j < 0 OrElse j >= n Then
                        Continue For
                    End If

                    sum += (decL(i) - meanL) * (decR(j) - meanR)
                Next

                If sum > bestScore Then
                    bestScore = sum
                    bestLag = lag
                End If
            Next

            Dim normL As Double = 0
            Dim normR As Double = 0

            For i As Integer = 0 To n - 1
                normL += (decL(i) - meanL) ^ 2
                normR += (decR(i) - meanR) ^ 2
            Next

            result.best_lag = bestLag * k
            result.correlation = If(normL = 0 OrElse normR = 0, 0, bestScore / std.Sqrt(normL * normR))

            Return result
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
