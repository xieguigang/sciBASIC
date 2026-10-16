Imports System.Linq
Imports std = System.Math

Namespace PeakFinding

    ''' <summary>
    ''' the local maxima locator by the amplitude threshold and the minimal peak distance.
    ''' 
    ''' ### the difference from <see cref="ElevationAlgorithm"/>
    ''' 
    ''' the library also provides the ``ElevationAlgorithm`` peak finder, but the two
    ''' algorithms are not interchangeable:
    ''' 
    ''' - ``ElevationAlgorithm`` works on the accumulated line of a chromatogram: it cuts
    '''   the slope segments which are flatter than the given angle, the peaks are the
    '''   regions between those flat segments. it needs the time/intensity tick list and it
    '''   reports the peak area integration.
    ''' - ``LocalMaxima.FindPeaks`` works on a plain sample array: it scans the local maxima
    '''   which are above the threshold and then applies a greedy non maximum suppression
    '''   by the minimal peak distance, so that only one peak is kept per neighbor region.
    ''' 
    ''' the scan line synchronization and the envelope pulse detection of the analog image
    ''' data use this locator.
    ''' </summary>
    Public Module LocalMaxima

        ''' <summary>
        ''' the local maxima positions <see cref="Integer"/> index on the given signal,
        ''' a peak is reported when its magnitude is above <paramref name="threshold"/>
        ''' and it is the maximum value inside the ``minDistance`` neighbor region.
        ''' </summary>
        ''' <remarks>
        ''' the peaks are detected via a small window local maxima scan and then a
        ''' greedy non maximum suppression pass, so that the complexity stays linear
        ''' even when the minimal peak distance is large.
        ''' </remarks>
        ''' <param name="x">the signal samples</param>
        ''' <param name="threshold">the minimal magnitude of a peak</param>
        ''' <param name="minDistance">the minimal distance in samples between two peaks</param>
        ''' <returns>the ascending ordered peak positions</returns>
        Public Function FindPeaks(x As Double(), threshold As Double, minDistance As Integer) As Integer()
            Dim candidates As New List(Of Integer)

            For i As Integer = 1 To x.Length - 2
                If x(i) < threshold Then
                    Continue For
                End If

                If x(i) < x(i - 1) OrElse x(i) < x(i + 1) Then
                    Continue For
                End If

                candidates.Add(i)
            Next

            If candidates.Count = 0 Then
                Return New Integer() {}
            End If

            ' greedy non maximum suppression by the peak magnitude
            Dim ordered As Integer() = candidates _
                .OrderByDescending(Function(i) x(i)) _
                .ToArray
            Dim accepted As New List(Of Integer)
            Dim blocked As Boolean() = New Boolean(x.Length - 1) {}

            For Each i As Integer In ordered
                Dim from As Integer = std.Max(0, i - minDistance)
                Dim [to] As Integer = std.Min(x.Length - 1, i + minDistance)
                Dim suppressed As Boolean = False

                For j As Integer = from To [to]
                    If blocked(j) Then
                        suppressed = True
                        Exit For
                    End If
                Next

                If Not suppressed Then
                    accepted.Add(i)

                    For j As Integer = from To [to]
                        blocked(j) = True
                    Next
                End If
            Next

            Return accepted.OrderBy(Function(i) i).ToArray
        End Function

        ''' <summary>
        ''' sub sample position of a local maximum via the parabolic interpolation.
        ''' 
        ''' the three points around the integer peak position are fitted by a parabola and
        ''' the position of the parabola vertex is returned, it gives a fractional sample
        ''' position which is much more accurate than the integer peak index.
        ''' </summary>
        ''' <param name="x">the signal samples</param>
        ''' <param name="i">the integer index of the local maximum</param>
        Public Function ParabolicPeak(x As Double(), i As Integer) As Double
            If i <= 0 OrElse i >= x.Length - 1 Then
                Return i
            End If

            Dim y1 As Double = x(i - 1)
            Dim y2 As Double = x(i)
            Dim y3 As Double = x(i + 1)
            Dim denom As Double = y1 - 2 * y2 + y3

            If denom = 0 Then
                Return i
            End If

            Dim delta As Double = 0.5 * (y1 - y3) / denom

            If std.Abs(delta) > 1 Then
                Return i
            End If

            Return i + delta
        End Function
    End Module
End Namespace
