#Region "Microsoft.VisualBasic::5627d797e358395d6809a740869d3048, Data_science\Mathematica\Math\Math\Distributions\Abundance.vb"

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

'   Total Lines: 47
'    Code Lines: 36 (76.60%)
' Comment Lines: 6 (12.77%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 5 (10.64%)
'     File Size: 1.89 KB


'     Module Abundance
' 
'         Function: RelativeAbundances
' 
'     Interface ISample
' 
'         Properties: Samples
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

Namespace Distributions

    Public Module Abundance

        ''' <summary>
        ''' x除以最大的值就是相对丰度
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function RelativeAbundances(Of T As ISample)(source As IEnumerable(Of T)) As IEnumerable(Of T)
            Dim array As T() = source.ToArray
            Dim allTags As String() = array _
                .Select(Function(x) x.Samples.Keys) _
                .IteratesALL _
                .Distinct _
                .ToArray
            Dim max As Dictionary(Of String, Double) = (
                From tag As String
                In allTags
                Select mxVal = array _
                    .Where(Function(x) x.Samples.ContainsKey(tag)) _
                    .Select(Function(x) x.Samples(tag)).Max, tag).ToDictionary(Function(x) x.tag, Function(x) x.mxVal)
            For Each x As T In array
                x.Samples = (From tag As String
                             In allTags
                             Select tag,
                                 value = x.Samples.TryGetValue(tag) / max(tag)) _
                                 .ToDictionary(Function(xx) xx.tag,
                                               Function(xx)
                                                   Return xx.value
                                               End Function)
                Yield x
            Next
        End Function

        ''' <summary>
        ''' **Skewness**
        ''' 
        ''' Skewness is a fundamental statistical measure used to describe the asymmetry of the probability distribution of a 
        ''' real-valued random variable. It provides insights into the direction and extent of the deviation from a symmetric 
        ''' distribution.
        ''' 
        ''' ### Key Aspects of Skewness:
        ''' 
        ''' 1. **Definition**:
        ''' 
        '''    - Skewness is the third standardized moment of a distribution.
        '''    - It is calculated as the average of the cubed deviations of the data from its mean, standardized by the standard deviation raised to the third power.
        '''    
        ''' 2. **Types of Skewness**:
        ''' 
        '''    - **Zero Skewness**: Indicates a symmetric distribution where the mean, median, and mode are all equal.
        '''    - **Positive Skewness (Right-Skewed)**: The tail on the right side of the distribution is longer or fatter. In this case, the mean is greater than the median.
        '''    - **Negative Skewness (Left-Skewed)**: The tail on the left side of the distribution is longer or fatter. Here, the mean is less than the median.
        '''    
        ''' 3. **Interpretation**:
        ''' 
        '''    - Skewness values close to zero suggest a nearly symmetric distribution.
        '''    - Positive values indicate right-skewed distributions, while negative values indicate left-skewed distributions.
        '''    - The magnitude of the skewness value reflects the degree of asymmetry.
        '''    
        ''' 4. **Applications**:
        ''' 
        '''    - **Finance**: Used to analyze the distribution of returns on investments, helping investors understand the potential for extreme outcomes.
        '''    - **Economics**: Assists in examining income distributions, enabling economists to assess income inequality.
        '''    - **Natural Sciences**: Describes the distribution of experimental data in scientific research.
        '''    
        ''' 5. **Considerations**:
        ''' 
        '''    - Skewness is just one aspect of distribution shape and should be considered alongside other statistical measures like kurtosis for a comprehensive understanding.
        '''    - For small sample sizes, the estimation of skewness can be unreliable.
        '''    
        ''' In essence, skewness is a statistical tool for understanding the asymmetry of data distributions, 
        ''' with wide-ranging applications in various fields such as finance, economics, and the natural 
        ''' sciences.
        ''' 
        ''' </summary>
        ''' <remarks>
        ''' If x contains missings and these are not removed, the skewness is NA.
        ''' Otherwise, write xi for the non-missing elements of x, n for their number, μ for their mean, s for their standard deviation, and 
        ''' mr =∑i (xi −μ) ^ r /n for the sample moments of order r.
        '''
        ''' Joanes and Gill (1998) discuss three methods for estimating skewness:
        '''
        ''' Type 1: g1 = m3 / m2 ^ (3/2). This is the typical definition used in many older textbooks.
        ''' Type 2: G1 = g1 * sqrt(n(n−1)) /(n−2). Used in SAS and SPSS.
        ''' Type 3: b1 = m3 /s^3 = g1 * ((n−1)/n) ^ (3/2) . Used in MINITAB and BMDP.
        '''
        ''' All three skewness measures are unbiased under normality.
        ''' </remarks>
        <Extension>
        Public Function Skewness(x As IEnumerable(Of Double), Optional type As AlgorithmType = AlgorithmType.Classical) As Double
            ' Remove missing values (assuming missing values are represented as Double.NaN)
            Dim cleanData As IEnumerable(Of Double) = x.Where(Function(value) Not Double.IsNaN(value))

            ' If there are missing values and they are not removed, return NaN
            If cleanData.Count() <> x.Count() Then
                Return Double.NaN
            End If

            ' Calculate the number of non-missing elements
            Dim n As Integer = cleanData.Count()

            ' If there are not enough data points, return NaN
            If n < 3 Then
                Return Double.NaN
            End If

            ' Calculate the mean
            Dim mean As Double = cleanData.Average()

            ' Calculate the standard deviation
            Dim stdDev As Double = std.Sqrt(cleanData.Sum(Function(xi) std.Pow(xi - mean, 2)) / (n - 1))

            ' Calculate the third moment
            Dim m3 As Double = cleanData.Sum(Function(xi) std.Pow(xi - mean, 3)) / n

            ' Calculate the second moment (variance)
            Dim m2 As Double = cleanData.Sum(Function(xi) std.Pow(xi - mean, 2)) / n

            ' Calculate skewness based on the selected algorithm
            Select Case type
                Case AlgorithmType.Classical
                    ' Type 1: g1 = m3 / m2 ^ (3/2)
                    Return m3 / std.Pow(m2, 1.5)

                Case AlgorithmType.SAS
                    ' Type 2: G1 = g1 * sqrt(n(n−1)) /(n−2)
                    Dim g1 As Double = m3 / std.Pow(m2, 1.5)
                    Return g1 * std.Sqrt(n * (n - 1)) / (n - 2)

                Case AlgorithmType.MINITAB
                    ' Type 3: b1 = m3 /s^3 = g1 * ((n−1)/n) ^ (3/2)
                    Dim g1 As Double = m3 / std.Pow(m2, 1.5)
                    Return g1 * std.Pow((n - 1) / n, 1.5)

                Case Else
                    Throw New NotImplementedException("The specified algorithm type is not implemented.")
            End Select
        End Function

        ''' <summary>
        ''' the algorithms for computing kurtosis/skewness
        ''' </summary>
        Public Enum AlgorithmType
            ''' <summary>
            ''' This is the typical definition used in many older textbooks.
            ''' </summary>
            Classical = 1
            ''' <summary>
            ''' Used in SAS and SPSS.
            ''' </summary>
            SAS = 2
            ''' <summary>
            ''' Used in MINITAB and BMDP.
            ''' </summary>
            MINITAB = 3
        End Enum

        ''' <summary>
        ''' **Kurtosis** is a statistical measure that describes the "tailedness" of the probability distribution of a
        ''' real-valued random variable. In simpler terms, it indicates the extent to which the tails of the distribution 
        ''' differ from those of a normal distribution.
        ''' 
        ''' ### Key Points about Kurtosis:
        ''' 
        ''' 1. **Definition**:
        ''' 
        '''    - Kurtosis is the fourth standardized moment of a distribution.
        '''    - It is calculated as the average of the squared deviations of the data from its mean, raised to the fourth power, standardized by the standard deviation raised to the fourth power.
        '''    
        ''' 2. **Types of Kurtosis**:
        ''' 
        '''    - **Mesokurtic**: Distributions with kurtosis similar to that of the normal distribution (kurtosis value of 3). The tails of a mesokurtic distribution are neither particularly fat nor particularly thin.
        '''    - **Leptokurtic**: Distributions with positive kurtosis greater than 3. These distributions have "fat tails" and a sharp peak, indicating more frequent large deviations from the mean than a normal distribution.
        '''    - **Platykurtic**: Distributions with kurtosis less than 3. These distributions have "thin tails" and a flatter peak, indicating fewer large deviations from the mean than a normal distribution.
        '''    
        ''' 3. **Excess Kurtosis**:
        ''' 
        '''    - Often, kurtosis is reported as "excess kurtosis," which is the kurtosis value minus 3. This adjustment makes the kurtosis of a normal distribution equal to 0.
        '''    - Positive excess kurtosis indicates a leptokurtic distribution, while negative excess kurtosis indicates a platykurtic distribution.
        '''    
        ''' 4. **Interpretation**:
        ''' 
        '''    - High kurtosis in a data set is an indicator that data has heavy tails or outliers. This can affect the performance of statistical models and methods that assume normality.
        '''    - Low kurtosis indicates that the data has light tails and lacks outliers.
        '''    
        ''' 5. **Applications**:
        ''' 
        '''    - In finance, kurtosis is used to describe the distribution of returns of an investment. A high kurtosis indicates a higher risk of extreme returns.
        '''    - In data analysis, kurtosis helps in understanding the shape of the data distribution and identifying potential outliers.
        '''    
        ''' 6. **Calculation in R**:
        ''' 
        '''    - The `kurtosis()` function in the `e1071` package can be used to calculate kurtosis in R.
        '''    - Alternatively, kurtosis can be calculated manually using the formula:
        '''    
        ''' ```R
        ''' kurtosis &lt;- sum((data - mean(data))^4) / ((length(data) - 1) * sd(data)^4) - 3
        ''' ```
        ''' 
        ''' kurtosis is a statistical measure for understanding the shape of a data distribution, particularly the behavior 
        ''' of its tails. It is widely used in various fields, including finance, data analysis, and statistics.
        ''' </summary>
        ''' <remarks>
        ''' If x contains missings and these are not removed, the kurtosis is NA.
        '''
        ''' Otherwise, write xi for the non-missing elements of x, n for their number, μ for their mean, s for their standard deviation, and 
        ''' mr = ∑i (xi −μ) ^ r /n for the sample moments of order r.
        '''
        ''' Joanes and Gill (1998) discuss three methods for estimating kurtosis:
        '''
        ''' Type 1: g2 = m4/m2 ^ 2 −3. This is the typical definition used in many older textbooks.
        ''' Type 2: G2 = ((n+1)*g2 +6)∗(n−1)/((n−2)(n−3)). Used in SAS and SPSS.
        ''' Type 3: b2 = m4 /s ^ 4 −3 = (g2 +3)(1−1/n) ^ 2 −3. Used in MINITAB and BMDP.
        '''
        ''' Only G2 (corresponding to type = 2) is unbiased under normality.
        ''' </remarks>
        <Extension>
        Public Function Kurtosis(x As IEnumerable(Of Double), Optional type As AlgorithmType = AlgorithmType.Classical) As Double
            ' 移除缺失值
            Dim cleanData As Double() = x.Where(Function(value) Not Double.IsNaN(value)).ToArray()
            Dim n As Integer = cleanData.Length

            ' 如果没有数据或数据不足，则返回NaN
            If n = 0 OrElse n < 4 Then
                Return Double.NaN
            End If

            ' 计算均值
            Dim mean As Double = cleanData.Average()

            ' 计算二阶、四阶样本矩
            Dim m2 As Double = cleanData.Sum(Function(xi) (xi - mean) ^ 2) / n
            Dim m4 As Double = cleanData.Sum(Function(xi) (xi - mean) ^ 4) / n

            ' 计算峰度
            Dim g2 As Double = m4 / (m2 ^ 2) - 3
            Dim result As Double = 0.0

            Select Case type
                Case AlgorithmType.Classical
                    result = g2
                Case AlgorithmType.SAS
                    result = ((n + 1) * g2 + 6) * (n - 1) / ((n - 2) * (n - 3))
                Case AlgorithmType.MINITAB
                    result = (g2 + 3) * ((1 - 1 / n) ^ 2) - 3
                Case Else
                    Throw New NotImplementedException("The specified algorithm type is not implemented.")
            End Select

            Return result
        End Function
    End Module

    Public Interface ISample : Inherits INamedValue

        Property Samples As Dictionary(Of String, Double)
    End Interface
End Namespace
