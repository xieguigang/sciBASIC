#Region "Microsoft.VisualBasic::6690d5f49d730ac928d0c161ee12a906, Data_science\Mathematica\Math\Math.Statistics\MomentFunctions\ProductMoments.vb"

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

    '   Total Lines: 184
    '    Code Lines: 86 (46.74%)
    ' Comment Lines: 74 (40.22%)
    '    - Xml Docs: 68.92%
    ' 
    '   Blank Lines: 24 (13.04%)
    '     File Size: 7.92 KB


    '     Class ProductMoments
    ' 
    '         Properties: data, kurtosis, max, mean, median
    '                     min, sampleSize, skewness, standardDeviation
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) AbsoluteMoment, (+2 Overloads) CentralMoment, (+2 Overloads) Moment, SecondMoment, ThirdCentralMoment
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Statistics.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports std = System.Math

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace MomentFunctions

    ''' <summary>
    ''' In statistics, moments are a set of numerical characteristics that describe the shape and features of a probability distribution. 
    ''' Sample moments are the same concept applied to a sample of data, rather than an entire population. They are used to estimate 
    ''' the corresponding population moments and to understand the properties of the data distribution.
    ''' 
    ''' Here's a basic introduction to the concept of sample moments:
    ''' 
    ''' ### Definition:
    ''' 
    ''' 1. **Sample Mean (First Moment):**
    '''    The sample mean is the average of the data points in a sample. It is a measure of the central tendency of the data.
    '''      \[
    '''      \bar{x} = \frac{1}{n} \sum_{i=1}^{n} x_i
    '''      \]
    '''    where \( x_i \) are the data points and \( n \) is the number of data points in the sample.
    ''' 2. **Sample Variance (Second Central Moment):**
    '''    The sample variance measures the spread or dispersion of the data points around the sample mean.
    '''      \[
    '''      s^2 = \frac{1}{n-1} \sum_{i=1}^{n} (x_i - \bar{x})^2
    '''      \]
    '''    The denominator \( n-1 \) is used instead of \( n \) to provide an unbiased estimate of the population variance.
    ''' 3. **Sample Standard Deviation:**
    '''    The sample standard deviation is the square root of the sample variance and is also a measure of dispersion.
    '''      \[
    '''      s = \sqrt{s^2}
    '''      \]
    ''' 4. **Higher-Order Sample Moments:**
    '''    Higher-order moments describe the shape of the distribution. For example:
    '''    - **Third Moment:** Measures skewness, which indicates the asymmetry of the data distribution.
    '''    - **Fourth Moment:** Measures kurtosis, which indicates the "tailedness" of the data distribution.
    '''    
    ''' ### Calculation:
    ''' 
    ''' To calculate sample moments, you simply apply the formulas to your data set. For instance, to find the sample mean,
    ''' you add up all the data points and divide by the number of points.
    ''' 
    ''' ### Use:
    ''' 
    ''' Sample moments are used to:
    ''' - Estimate population parameters.
    ''' - Assess the shape of the data distribution (e.g., normality, skewness, kurtosis).
    ''' - Form the basis for many statistical tests and procedures.
    ''' 
    ''' ### Properties:
    ''' - **Unbiasedness:** Some sample moments are designed to be unbiased estimators, meaning that the expected value of the sample moment equals the population moment.
    ''' - **Efficiency:** Different sample moments may have different levels of variability; some are more efficient than others.
    ''' - **Robustness:** Certain moments are more robust to outliers than others.
    ''' 
    ''' ### Example:
    ''' If you have a sample of data: \( \{2, 4, 4, 4, 5, 5, 7, 9\} \), you can calculate the sample mean, variance, 
    ''' and other moments to understand the central tendency, dispersion, and shape of the data distribution.
    ''' 
    ''' sample moments are fundamental tools in statistics for summarizing and understanding the characteristics of
    ''' a data set. They provide a way to quantify features such as location, spread, and shape, which are essential 
    ''' for further statistical analysis.
    ''' 
    ''' @author Will_and_Sara
    ''' </summary>
    Public Class ProductMoments

        Public ReadOnly Property median As Double
        Public ReadOnly Property skewness As Double
        Public ReadOnly Property kurtosis As Double
        Public ReadOnly Property min As Double
        Public ReadOnly Property max As Double
        Public ReadOnly Property mean As Double
        Public ReadOnly Property standardDeviation As Double
        Public ReadOnly Property sampleSize As Integer

        ''' <summary>
        ''' the input raw data vector
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property data As IReadOnlyCollection(Of Double)

        Public Sub New(data As Double())
            Dim count = data.Length
            Dim BPM As New BasicProductMoments(data)

            _min = BPM.Min()
            _max = BPM.Max()
            _mean = BPM.Mean()
            _standardDeviation = BPM.StDev()
            _sampleSize = count
            _data = data

            Dim skewsums As Double = 0
            Dim ksums As Double = 0

            For i As Integer = 0 To data.Length - 1
                skewsums += std.Pow((data(i) - _mean), 3)
                ksums += std.Pow(((data(i) - _mean) / _standardDeviation), 4)
            Next

            'just alittle more math...
            ksums *= (count * (count + 1)) \ ((count - 1) * (count - 2) * (count - 3))
            _skewness = (count * skewsums) / ((count - 1) * (count - 2) * std.Pow(_standardDeviation, 3))
            _kurtosis = ksums - ((3 * (std.Pow(count - 1, 2))) / ((count - 2) * (count - 3)))

            'figure out an efficent algorithm for median...
            median = data.Median
        End Sub

        Public Overrides Function ToString() As String
            Return GetJson
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Moment(n As Integer) As Double
            Return Moment(data, n)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CentralMoment(n As Integer) As Double
            Return CentralMoment(data, n)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function AbsoluteMoment(n As Integer) As Double
            Return AbsoluteMoment(data, n)
        End Function

        ' 函数用于计算数据的第n阶绝对矩
        Public Shared Function AbsoluteMoment(data As IEnumerable(Of Double), n As Integer) As Double
            Dim sum As Double = 0
            Dim i As Integer = 0

            For Each x As Double In data
                sum += std.Abs(x) ^ n
                i += 1
            Next

            Return sum / i
        End Function


        ' 函数用于计算数据的第n阶原点矩
        Public Shared Function Moment(data As IEnumerable(Of Double), n As Integer) As Double
            Dim sum As Double = 0
            Dim i As Integer = 0

            For Each x As Double In data
                sum += x ^ n
                i += 1
            Next

            Return sum / i
        End Function

        ' 函数用于计算数据的第n阶中心矩
        Public Shared Function CentralMoment(data As IEnumerable(Of Double), n As Integer) As Double
            Dim pool As Double() = data.SafeQuery.ToArray
            Dim mean As Double = pool.Average()
            Dim sum As Double = 0
            For Each x As Double In pool
                sum += (x - mean) ^ n
            Next
            Return sum / pool.Length
        End Function

        ' 函数用于计算2阶矩（方差）
        Public Shared Function SecondMoment(data As IEnumerable(Of Double)) As Double
            Return Moment(data, 2)
        End Function

        ' 函数用于计算3阶中心矩
        Public Shared Function ThirdCentralMoment(data As IEnumerable(Of Double)) As Double
            Return CentralMoment(data, 3)
        End Function
    End Class
End Namespace
