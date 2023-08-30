Imports std = System.Math

Namespace COW

    ''' <summary>
    ''' Do alignment of the time signal peaks between multiple sample data files, 
    ''' example as the signal peak data is the ms1 ion peak which is generated 
    ''' from the LCMS sample MS1 data
    ''' </summary>
    ''' <remarks>
    ''' http://prime.psc.riken.jp/compms/msdial/main.html
    ''' </remarks>
    Public NotInheritable Class CowAlignment

        Private Sub New()
        End Sub

        Public Shared Function GaussianFunction(ByVal normalizedValue As Double, ByVal mean As Double, ByVal standardDeviation As Double, ByVal variable As Double) As Double
            Dim result = normalizedValue * std.Exp(-1 * std.Pow(variable - mean, 2) / (2 * std.Pow(standardDeviation, 2)))
            Return result
        End Function

        ''' <summary>
        ''' This is the alignment program of correlation optimized warping. (see Nielsen et.al. J. Chromatogr. A 805, 17–35 (1998).)
        ''' This program returns the chromatogram information as the list of ChromatogramPeak containing scan number, retention time, m/z, intensity.
        ''' As long as you use 'Constant' enum as the borderlimit, you do not have to mind maxSlack (second arg).
        ''' Now I'm making some border limits but please do not use others except for 'Constant' yet.
        ''' The first argument, minSlack, should be 1 or 2 as long as ODS columns or GC are used.
        ''' The second argument is please the same as the first argument.
        ''' The third argument, segment size, should be set to the data point number of detected peaks (recommended).
        ''' The sample chromatogram will be aligned to the reference chromatogram.
        ''' The border limit please should be set to constant.
        ''' </summary>
        ''' <param name="minSlack"></param>
        ''' <param name="maxSlack"></param>
        ''' <param name="segmentSize"></param>
        ''' <param name="referenceChromatogram"></param>
        ''' <param name="sampleChromatogram"></param>
        ''' <param name="borderLimit"></param>
        ''' <returns></returns>
        Public Shared Function CorrelationOptimizedWarping(minSlack As Integer, maxSlack As Integer, segmentSize As Integer, referenceChromatogram As List(Of ChromatogramPeak), sampleChromatogram As List(Of ChromatogramPeak), borderLimit As BorderLimit) As List(Of ChromatogramPeak)
            Dim alignedChromatogram = New List(Of ChromatogramPeak)()
            Dim referenceDatapointNumber = referenceChromatogram.Count, sampleDatapointNumber = sampleChromatogram.Count

            Dim segmentNumber As Integer = sampleDatapointNumber / segmentSize
            Dim delta As Integer = referenceDatapointNumber / sampleDatapointNumber * segmentSize - segmentSize
            Dim enabledLength = (segmentSize + delta) * segmentNumber

            Dim functionMatrixBean As FunctionMatrix = New FunctionMatrix(segmentNumber + 1, enabledLength + 1)
            Dim functionElementBean As FunctionElement

            'Slack parameter set
#Region ""
            Dim slack As List(Of Integer) = New List(Of Integer)()
            For i = 0 To segmentNumber - 1
                If borderLimit = BorderLimit.Constant Then
                    slack.Add(minSlack)
                ElseIf borderLimit = BorderLimit.Linear Then
                    slack.Add(minSlack + CInt((maxSlack - minSlack) * i / (segmentNumber - 1)))
                ElseIf borderLimit = BorderLimit.Quad Then
                    slack.Add(minSlack + CDbl(maxSlack - minSlack) * i / std.Pow(segmentNumber, 2))
                ElseIf borderLimit = BorderLimit.Diamond Then
                    slack.Add(maxSlack - 2 / (CDbl(segmentNumber) - 1) * std.Abs(i - (CDbl(segmentNumber) - 1) / 2))
                ElseIf borderLimit = BorderLimit.Gaussian Then
                    slack.Add(CInt(GaussianFunction(maxSlack - minSlack, segmentNumber / 2, segmentNumber / 4, i)) + minSlack)
                End If
            Next
#End Region

            'Initialize
#Region ""
            For i = 0 To segmentNumber
                For j = 0 To enabledLength
                    functionElementBean = New FunctionElement(Double.MinValue, 0)
                    functionMatrixBean(i, j) = functionElementBean
                Next
            Next
            functionMatrixBean(segmentNumber, enabledLength).Score = 0
#End Region

            'score matrix calculation
#Region ""
            Dim intervalStart, intervalEnd As Integer
            Dim cumCoefficient As Double
            For i = segmentNumber - 1 To 0 Step -1
                intervalStart = std.Max(i * (segmentSize + delta - slack(i)), enabledLength - (segmentNumber - i) * (segmentSize + delta + slack(i)))
                intervalEnd = std.Min(i * (segmentSize + delta + slack(i)), enabledLength - (segmentNumber - i) * (segmentSize + delta - slack(i)))

                'bool amplitudeCheck = checkAmplitude(1000, referenceChromatogram, intervalStart, intervalEnd + segmentSize + slack[i]);

                For x As Integer = intervalStart To intervalEnd
                    For u = delta - slack(i) To delta + slack(i)
                        If 0 <= x + segmentSize + u AndAlso x + segmentSize + u <= enabledLength Then
                            cumCoefficient = functionMatrixBean(i + 1, x + segmentSize + u).Score + cowFunctionCalculation(i, x, u, segmentSize, referenceChromatogram, sampleChromatogram)

                            If cumCoefficient > functionMatrixBean(i, x).Score Then
                                functionMatrixBean(i, x).Score = cumCoefficient
                                functionMatrixBean(i, x).Warp = u
                            End If
                        End If
                    Next
                Next
            Next
#End Region

            'Backtrace
#Region ""
            Dim endPosition, positionFlont, totalWarp, warp, counter As Integer, positionEnd = 0
            Dim warpedPosition, fraction, score As Double

            'Initialize
            endPosition = 0
            warp = 0
            score = 0
            totalWarp = 0
            counter = 0
            For i = 0 To segmentNumber - 1
                warp = functionMatrixBean(i, endPosition).Warp
                score = functionMatrixBean(i, endPosition).Score

                If totalWarp > slack(i) * 2 Then
                    warp = -slack(i)
                ElseIf totalWarp < -1 * slack(i) * 2 Then
                    warp = slack(i)
                End If

                For j = 0 To segmentSize + warp - 1
                    If endPosition + j > referenceDatapointNumber - 1 Then Exit For

                    'get warped position, and linear interpolation
                    warpedPosition = CDbl(j) * segmentSize / (segmentSize + warp)
                    If std.Floor(warpedPosition) < 0 Then warpedPosition = 0
                    If std.Ceiling(warpedPosition) > segmentSize Then warpedPosition = segmentSize

                    fraction = warpedPosition - std.Floor(warpedPosition)
                    positionFlont = i * segmentSize + CInt(std.Floor(warpedPosition))
                    positionEnd = i * segmentSize + CInt(std.Ceiling(warpedPosition))

                    If positionFlont > sampleDatapointNumber - 1 Then positionFlont = sampleDatapointNumber - 1
                    If positionEnd > sampleDatapointNumber - 1 Then positionEnd = sampleDatapointNumber - 1

                    'Set
                    Dim peakInformation = New ChromatogramPeak(referenceChromatogram(counter).ID, (1 - fraction) * sampleChromatogram(positionFlont).Mass + fraction * sampleChromatogram(positionEnd).Mass, (1 - fraction) * sampleChromatogram(positionFlont).Intensity + fraction * sampleChromatogram(positionEnd).Intensity, referenceChromatogram(counter).time)
                    alignedChromatogram.Add(peakInformation)
                    counter += 1
                Next
                endPosition += segmentSize + warp
                totalWarp += warp
                'Debug.Print(endPosition + "\t" + totalWarp + "\t" + warp);

            Next

            'Reminder
            If enabledLength < referenceDatapointNumber Then
                For i = enabledLength To referenceDatapointNumber - 1
                    positionEnd += 1
                    If positionEnd > sampleDatapointNumber - 1 Then positionEnd = sampleDatapointNumber - 1

                    Dim peakInformation = New ChromatogramPeak(referenceChromatogram(counter).ID, sampleChromatogram(positionEnd).Mass, sampleChromatogram(positionEnd).Intensity, referenceChromatogram(counter).time)
                    alignedChromatogram.Add(peakInformation)
                    counter += 1
                Next
            End If
#End Region

            'Debug.Print(referenceChromatogram.Count + "\t" + sampleChromatogram.Count + "\t" + alignedChromatogram.Count);

            Return alignedChromatogram
        End Function

        ''' <summary>
        ''' The point of dynamic programming based alignment is to get the suitable reference chromatogram.
        ''' Selecting the reference chromatogram which should look like 'center' of chromatograms will be better to get nice alignment results.
        ''' So, this program is used to get the suitable reference chromatogram from imported chromatograms.
        ''' Please see Tsugawa et. al. Front. Genet. 5:471, 2015
        ''' </summary>
        ''' <param name="chromatograms"></param>
        ''' <returns></returns>
        Public Shared Function AutomaticParameterDefinder(chromatograms As List(Of Double())) As CowParameter
            Dim chromatogramsNumber = chromatograms(0).Length - 3
            Dim gravityArray = New Double(chromatogramsNumber - 1) {}
            Dim totalIntensity, sum As Double, maxIntensity = Double.MinValue
            For i = 0 To chromatogramsNumber - 1
                sum = 0
                totalIntensity = 0
                For j = 0 To chromatograms.Count - 1
                    sum += chromatograms(j)(1) * chromatograms(j)(3 + i)
                    totalIntensity += chromatograms(j)(3 + i)
                    If maxIntensity < chromatograms(j)(3 + i) Then maxIntensity = chromatograms(j)(3 + i)
                Next
                gravityArray(i) = sum / totalIntensity
            Next

            Dim maxGravity, minGravity, centerGravity As Double
            maxGravity = gravityArray.Max
            minGravity = gravityArray.Min
            centerGravity = (maxGravity + minGravity) / 2

            Dim referenceID = GetNearestIndex(gravityArray, centerGravity)
            Dim slack As Integer = (maxGravity - minGravity) * (chromatograms(chromatograms.Count - 1)(0) - chromatograms(0)(0)) / (chromatograms(chromatograms.Count - 1)(1) - chromatograms(0)(1))

            Dim alignmentParameter As CowParameter = New CowParameter() With {
                .ReferenceID = referenceID,
                .Slack = slack
            }

            Return alignmentParameter
        End Function

        Public Shared Function GetNearestIndex(ByVal x As Double(), ByVal value As Double) As Integer
            Dim index = -1
            Dim diff = Double.MaxValue
            For i = 0 To x.Length - 1

                If std.Abs(x(i) - value) < diff Then
                    diff = std.Abs(x(i) - value)
                    index = i
                End If
            Next

            Return index
        End Function

        Public Shared Function Mean(ByVal list As List(Of Double()), ByVal index As Integer) As Double
            Dim sum As Double = 0
            For i = 0 To list.Count - 1
                sum += list(i)(index)
            Next
            Return sum / list.Count
        End Function

        ''' <summary>
        ''' This is the simple alignment program (maybe can be used to GC).
        ''' The sample chromatogram will be aligned to reference chromatogram so that the correlation coefficient should be maximum withing moveTime param.
        ''' See Jonsson, P. et al. Anal. Chem. 76, 1738–45 (2004).
        ''' </summary>
        ''' <param name="moveTime"></param>
        ''' <param name="referenceChromatogram"></param>
        ''' <param name="sampleChromatogram"></param>
        ''' <returns></returns>
        Public Shared Function LinearAlignment(moveTime As Double, referenceChromatogram As List(Of Double()), sampleChromatogram As List(Of Double())) As List(Of Double())
            Dim alignedChromatogram As List(Of Double()) = New List(Of Double())()
            Dim referenceDatapointNumber = referenceChromatogram.Count, sampleDatapointNumber = sampleChromatogram.Count
            Dim movePoint As Integer = moveTime * referenceDatapointNumber / (referenceChromatogram(referenceDatapointNumber - 1)(1) - referenceChromatogram(0)(1))

            Dim referenceMean = Mean(referenceChromatogram, 3)
            Dim sampleMean = Mean(sampleChromatogram, 3)

            Dim covariance As Double, covarianceMax = Double.MinValue
            Dim covarianceMaxId = 0
            For i = -movePoint To movePoint
                covariance = 0
                For j = 0 To referenceChromatogram.Count - 1
                    If j + i < 0 Then Continue For
                    If j + i > sampleChromatogram.Count - 1 Then Exit For
                    covariance += (referenceChromatogram(j)(3) - referenceMean) * (sampleChromatogram(j + i)(3) - sampleMean)
                Next
                If covariance > covarianceMax Then
                    covarianceMax = covariance
                    covarianceMaxId = i
                End If
            Next

            If covarianceMaxId < 0 Then
                For j = 0 To referenceDatapointNumber - 1
                    If j + covarianceMaxId < 0 Then
                        alignedChromatogram.Add(New Double() {j, referenceChromatogram(j)(1), sampleChromatogram(0)(0), sampleChromatogram(0)(3)})
                    ElseIf j + covarianceMaxId > sampleDatapointNumber - 1 Then
                        alignedChromatogram.Add(New Double() {j, referenceChromatogram(j)(1), sampleChromatogram(sampleDatapointNumber - 1)(0), sampleChromatogram(sampleDatapointNumber - 1)(3)})
                    Else
                        alignedChromatogram.Add(New Double() {j, referenceChromatogram(j)(1), sampleChromatogram(j + covarianceMaxId)(0), sampleChromatogram(j + covarianceMaxId)(3)})
                    End If
                Next
            ElseIf covarianceMaxId > 0 Then
                For j = 0 To referenceDatapointNumber - 1
                    If j + covarianceMaxId > sampleDatapointNumber - 1 Then
                        alignedChromatogram.Add(New Double() {j, referenceChromatogram(j)(1), sampleChromatogram(sampleDatapointNumber - 1)(0), sampleChromatogram(sampleDatapointNumber - 1)(3)})
                    Else
                        alignedChromatogram.Add(New Double() {j, referenceChromatogram(j)(1), sampleChromatogram(j + covarianceMaxId)(0), sampleChromatogram(j + covarianceMaxId)(3)})
                    End If
                Next
            Else
                For j = 0 To referenceDatapointNumber - 1
                    If j > sampleDatapointNumber - 1 Then
                        alignedChromatogram.Add(New Double() {j, referenceChromatogram(j)(1), sampleChromatogram(sampleDatapointNumber - 1)(0), sampleChromatogram(sampleDatapointNumber - 1)(3)})
                    Else
                        alignedChromatogram.Add(New Double() {j, referenceChromatogram(j)(1), sampleChromatogram(j)(0), sampleChromatogram(j)(3)})
                    End If
                Next
            End If
            Return alignedChromatogram
        End Function

        Private Shared Function cowFunctionCalculation(rowPosition As Integer, columnPosition As Integer, u As Integer, segmentSize As Integer, referenceChromatogram As List(Of ChromatogramPeak), sampleChromatogram As List(Of ChromatogramPeak)) As Double
            Dim positionFlont, positionEnd As Integer
            Dim warpedPosition, fraction, wT, wS As Double
            Dim targetArray = New Double(segmentSize + u + 1 - 1) {}, alingedArray = New Double(segmentSize + u + 1 - 1) {}

            For j = 0 To segmentSize + u
                If columnPosition + j >= referenceChromatogram.Count Then Exit For

                'get warped position, and linear interpolation
                warpedPosition = j * segmentSize / (segmentSize + u)
                If std.Floor(warpedPosition) < 0 Then warpedPosition = 0
                If std.Ceiling(warpedPosition) > segmentSize Then warpedPosition = segmentSize

                fraction = warpedPosition - std.Floor(warpedPosition)
                positionFlont = rowPosition * segmentSize + CInt(std.Floor(warpedPosition))
                positionEnd = rowPosition * segmentSize + CInt(std.Ceiling(warpedPosition))

                If positionFlont >= sampleChromatogram.Count Then positionFlont = sampleChromatogram.Count - 1
                If positionEnd >= sampleChromatogram.Count Then positionEnd = sampleChromatogram.Count - 1

                wT = referenceChromatogram(columnPosition + j).Intensity
                wS = (1 - fraction) * sampleChromatogram(positionFlont).Intensity + fraction * sampleChromatogram(positionEnd).Intensity

                targetArray(j) = wT
                alingedArray(j) = wS
            Next
            Return std.Abs(Coefficient(targetArray, alingedArray))
        End Function

        Public Shared Function Coefficient(ByVal array1 As Double(), ByVal array2 As Double()) As Double
            Dim sum1 As Double = 0, sum2 As Double = 0, mean1 As Double = 0, mean2 As Double = 0, covariance As Double = 0, sqrt1 As Double = 0, sqrt2 As Double = 0
            For i = 0 To array1.Length - 1
                sum1 += array1(i)
                sum2 += array2(i)
            Next
            mean1 = sum1 / array1.Length
            mean2 = sum2 / array2.Length

            For i = 0 To array1.Length - 1
                covariance += (array1(i) - mean1) * (array2(i) - mean2)
                sqrt1 += std.Pow(array1(i) - mean1, 2)
                sqrt2 += std.Pow(array2(i) - mean2, 2)
            Next
            If sqrt1 = 0 OrElse sqrt2 = 0 Then
                Return 0
            Else
                Return covariance / std.Sqrt(sqrt1 * sqrt2)
            End If
        End Function
    End Class
End Namespace
