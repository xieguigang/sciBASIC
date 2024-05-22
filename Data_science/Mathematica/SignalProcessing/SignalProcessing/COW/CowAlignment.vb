#Region "Microsoft.VisualBasic::b549b730fcdef7cd2ec7fc2b6ca2a62f, Data_science\Mathematica\SignalProcessing\SignalProcessing\COW\CowAlignment.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 319
    '    Code Lines: 231 (72.41%)
    ' Comment Lines: 39 (12.23%)
    '    - Xml Docs: 76.92%
    ' 
    '   Blank Lines: 49 (15.36%)
    '     File Size: 17.11 KB


    '     Class CowAlignment
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Coefficient, CorrelationOptimizedWarping, cowFunctionCalculation, LinearAlignment, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Distributions
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
    Public NotInheritable Class CowAlignment(Of S As {IPeak2D})

        ReadOnly peak2D As IDelegateCreatePeak2D(Of S)

        Sub New(createPeak As IDelegateCreatePeak2D(Of S))
            peak2D = createPeak
        End Sub

        Public Overrides Function ToString() As String
            Return $"COW({GetType(S).Name})"
        End Function

        ''' <summary>
        ''' This is the alignment program of correlation optimized warping. (see Nielsen et.al. J. Chromatogr. A 805, 17–35 (1998).)
        ''' This program returns the chromatogram information as the list of ChromatogramPeak containing scan number, retention time, m/z, intensity.
        ''' As long as you use 'Constant' enum as the borderlimit, you do not have to mind maxSlack (second arg).
        ''' Now I'm making some border limits but please do not use others except for 'Constant' yet.
        ''' </summary>
        ''' <param name="minSlack">The first argument, minSlack, should be 1 or 2 as long as ODS columns or GC are used.</param>
        ''' <param name="maxSlack">The second argument is please the same as the first argument.</param>
        ''' <param name="segmentSize">The third argument, segment size, should be set to the data point number of detected peaks (recommended).</param>
        ''' <param name="referenceChromatogram">The sample chromatogram will be aligned to the reference chromatogram.</param>
        ''' <param name="sampleChromatogram"></param>
        ''' <param name="borderLimit">The border limit please should be set to constant.</param>
        ''' <returns></returns>
        Public Iterator Function CorrelationOptimizedWarping(referenceChromatogram As List(Of S), sampleChromatogram As List(Of S),
                                                             Optional segmentSize As Integer = 3,
                                                             Optional minSlack As Integer = 1,
                                                             Optional maxSlack As Integer = 1,
                                                             Optional borderLimit As BorderLimit = BorderLimit.Constant) As IEnumerable(Of S)

            Dim referenceDatapointNumber = referenceChromatogram.Count
            Dim sampleDatapointNumber = sampleChromatogram.Count

            Dim segmentNumber As Integer = sampleDatapointNumber / segmentSize
            Dim delta As Integer = referenceDatapointNumber / sampleDatapointNumber * segmentSize - segmentSize
            Dim enabledLength = (segmentSize + delta) * segmentNumber

            Dim functionMatrixBean As FunctionMatrix = New FunctionMatrix(segmentNumber + 1, enabledLength + 1)

#Region "Slack parameter set"
            Dim slack As New List(Of Integer)()

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
                    ' original function calls
                    ' GaussianFunction(normalizedValue, mean, standardDeviation, variable)
                    '    normalizedValue * std.Exp(-1 * std.Pow(variable - mean, 2) / (2 * std.Pow(standardDeviation, 2)))
                    slack.Add(CInt(Gaussian.Gaussian(
                              a:=maxSlack - minSlack,
                              b:=segmentNumber / 2,
                              sigma:=segmentNumber / 4,
                              x:=i)) + minSlack)
                End If
            Next
#End Region

            Call functionMatrixBean.Initialize(segmentNumber, enabledLength)

#Region "score matrix calculation"
            Dim intervalStart, intervalEnd As Integer
            Dim cumCoefficient As Double

            For i = segmentNumber - 1 To 0 Step -1
                intervalStart = std.Max(i * (segmentSize + delta - slack(i)), enabledLength - (segmentNumber - i) * (segmentSize + delta + slack(i)))
                intervalEnd = std.Min(i * (segmentSize + delta + slack(i)), enabledLength - (segmentNumber - i) * (segmentSize + delta - slack(i)))

                'bool amplitudeCheck = checkAmplitude(1000, referenceChromatogram, intervalStart, intervalEnd + segmentSize + slack[i]);

                For x As Integer = intervalStart To intervalEnd
                    For u = delta - slack(i) To delta + slack(i)
                        If 0 <= x + segmentSize + u AndAlso x + segmentSize + u <= enabledLength Then
                            cumCoefficient = functionMatrixBean(i + 1, x + segmentSize + u).Score +
                                cowFunctionCalculation(i, x, u, segmentSize, referenceChromatogram, sampleChromatogram)

                            If cumCoefficient > functionMatrixBean(i, x).Score Then
                                functionMatrixBean(i, x).Score = cumCoefficient
                                functionMatrixBean(i, x).Warp = u
                            End If
                        End If
                    Next
                Next
            Next
#End Region

#Region "Backtrace"
            Dim endPosition, positionFlont, totalWarp, warp, counter As Integer, positionEnd = 0
            Dim warpedPosition, fraction, score As Double

            'Initialize
            endPosition = 0
            warp = 0
            score = 0
            totalWarp = 0
            counter = 0
            For i As Integer = 0 To segmentNumber - 1
                If endPosition >= functionMatrixBean.dims.col Then
                    Continue For
                End If

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
                    Dim peakInformation As S = peak2D(
                        id:=referenceChromatogram(counter).ID,
                        dim1:=(1 - fraction) * sampleChromatogram(positionFlont).Dimension1 + fraction * sampleChromatogram(positionEnd).Dimension1,
                        intensity:=(1 - fraction) * sampleChromatogram(positionFlont).Intensity + fraction * sampleChromatogram(positionEnd).Intensity,
                        dim2:=referenceChromatogram(counter).Dimension2
                    )
                    counter += 1

                    Yield peakInformation
                Next

                endPosition += segmentSize + warp
                totalWarp += warp
            Next

            'Reminder
            If enabledLength < referenceDatapointNumber Then
                For i = enabledLength To referenceDatapointNumber - 1
                    positionEnd += 1

                    If positionEnd > sampleDatapointNumber - 1 Then
                        positionEnd = sampleDatapointNumber - 1
                    End If

                    If counter >= referenceChromatogram.Count Then
                        Continue For
                    End If
                    If positionEnd >= sampleChromatogram.Count Then
                        Continue For
                    End If

                    Dim peakInformation = peak2D(
                        id:=referenceChromatogram(counter).ID,
                        dim1:=sampleChromatogram(positionEnd).Dimension1,
                        intensity:=sampleChromatogram(positionEnd).Intensity,
                        dim2:=referenceChromatogram(counter).Dimension2
                    )
                    counter += 1

                    Yield peakInformation
                Next
            End If
#End Region
            Call functionMatrixBean.Dispose()
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
            Dim referenceMean = Aggregate x As Double() In referenceChromatogram Let xi = x(3) Into Average(xi)
            Dim sampleMean = Aggregate x As Double()
                             In sampleChromatogram
                             Let xi As Double = x(3)
                             Into Average(xi)

            Dim covariance As Double, covarianceMax = Double.MinValue
            Dim covarianceMaxId = 0
            For i As Integer = -movePoint To movePoint
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

        Private Shared Function cowFunctionCalculation(rowPosition As Integer,
                                                       columnPosition As Integer,
                                                       u As Integer,
                                                       segmentSize As Integer,
                                                       referenceChromatogram As List(Of S),
                                                       sampleChromatogram As List(Of S)) As Double

            Dim positionFlont, positionEnd As Integer
            Dim warpedPosition, fraction, wT, wS As Double
            Dim targetArray = New Double(segmentSize + u + 1 - 1) {}
            Dim alingedArray = New Double(segmentSize + u + 1 - 1) {}

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

        Public Shared Function Coefficient(array1 As Double(), array2 As Double()) As Double
            Dim sum1 As Double = 0, sum2 As Double = 0
            Dim covariance As Double = 0
            Dim sqrt1 As Double = 0, sqrt2 As Double = 0

            For i = 0 To array1.Length - 1
                sum1 += array1(i)
                sum2 += array2(i)
            Next

            Dim mean1 = sum1 / array1.Length
            Dim mean2 = sum2 / array2.Length

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
