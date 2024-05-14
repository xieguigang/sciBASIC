#Region "Microsoft.VisualBasic::dc90f7174a4ea02270534874eee37df7, Microsoft.VisualBasic.Core\src\Extensions\Math\Math.vb"

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

    '   Total Lines: 725
    '    Code Lines: 410
    ' Comment Lines: 237
    '   Blank Lines: 78
    '     File Size: 25.37 KB


    '     Module VBMath
    ' 
    '         Function: Clamp, Clip, Covariance, CumSum, Factorial
    '                   FactorialSequence, Hypot, (+2 Overloads) IsPowerOf2, (+2 Overloads) Log2, LogN
    '                   (+2 Overloads) Max, Permut, PoissonPDF, Pow2, (+3 Overloads) ProductALL
    '                   (+3 Overloads) RangesAt, RMS, RMSE, (+2 Overloads) RSD, (+4 Overloads) SD
    '                   (+2 Overloads) seq, (+5 Overloads) Sum, WeighedAverage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Statistics.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports std = System.Math

Namespace Math

    ''' <summary>
    ''' Provides constants and static methods for trigonometric, logarithmic, and other
    ''' common mathematical functions.To browse the .NET Framework source code for this
    ''' type, see the Reference Source.
    ''' </summary>
    <Package("VBMath", Publisher:="xie.guigang@gmail.com")>
    Public Module VBMath

        ''' <summary>
        ''' Standard clamping of a value into a fixed range
        ''' </summary>
        Public Function Clip(x As Double, clipValue As Double) As Double
            If x > clipValue Then
                Return clipValue
            ElseIf x < -clipValue Then
                Return -clipValue
            Else
                Return x
            End If
        End Function

#If NET48 Then

        ' Clamp function is missing in .NET 4.8 

        ''' <summary>
        ''' 返回范围内的一个数值。可以使用 clamp 函数将不断增加、减小或随机变化的数值限制在一系列的值中。
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="min"></param>
        ''' <param name="max"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 最小数值和最大数值指定返回值的范围。
        ''' 参数是值要钳制在范围内的属性或变量。
        ''' 如果参数位于最小数值和最大数值之间的数值范围内，则该函数将返回参数值。
        ''' 如果参数大于范围，该函数将返回最大数值。
        ''' 如果参数小于范围，该函数将返回最小数值。
        ''' </remarks>
        Public Function Clamp(x As Single, min As Single, max As Single) As Single
            If x < min Then
                Return min
            ElseIf x > max Then
                Return max
            Else
                Return x
            End If
        End Function
#End If

        Public Function Permut(k As Integer, n As Integer) As Long
            Dim nfactors As Integer() = (n - 1).SeqIterator(offset:=1).ToArray
            Dim nkfactors As Integer() = (n - k - 1).SeqIterator(offset:=1).ToArray

            ' removes the same factor element
            Dim nf As Integer()
            Dim nk As Integer()

            With nfactors.Indexing
                nk = nkfactors.Where(Function(x) .IndexOf(x:=x) = -1).ToArray
            End With
            With nkfactors.Indexing
                nf = nfactors.Where(Function(x) .IndexOf(x:=x) = -1).ToArray
            End With

            Dim i As Long = 1

            For Each x In nf
                i = i * x
            Next

            Dim j As Long = 1

            For Each x In nk
                j = j * x
            Next

            Return i / j
        End Function

        ''' <summary>
        ''' ``Math.Log(x, newBase:=2)``
        ''' </summary>
        ''' <param name="x#"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function Log2(x#) As Double
            Return std.Log(x, newBase:=2)
        End Function

        <Extension>
        Public Iterator Function CumSum(vector As IEnumerable(Of Double)) As IEnumerable(Of Double)
            Dim sum#

            For Each x As Double In vector
                sum += x
                Yield sum
            Next
        End Function

        ''' <summary>
        ''' 阶乘
        ''' </summary>
        ''' <param name="a"></param>
        ''' <returns></returns>
        Public Function Factorial(a As Integer) As Double
            If a <= 1 Then
                Return 1
            Else
                Dim n As Double = a

                For i As Integer = a - 1 To 1 Step -1
                    n *= i
                Next

                Return n
            End If
        End Function

        Public Iterator Function FactorialSequence(a As Integer) As IEnumerable(Of Integer)
            If a <= 1 Then
                Yield 1
            Else
                For i As Integer = a To 1 Step -1
                    Yield i
                Next
            End If
        End Function

        ''' <summary>
        ''' Returns the covariance of two data vectors. </summary>
        ''' <param name="a">	double[] of data </param>
        ''' <param name="b">	double[] of data
        ''' @return	the covariance of a and b, cov(a,b) </param>
        Public Function Covariance(a As Double(), b As Double()) As Double
            If a.Length <> b.Length Then
                Throw New ArgumentException("Cannot take covariance of different dimension vectors.")
            End If

            Dim divisor As Double = a.Length - 1
            Dim sum As Double = 0
            Dim aMean As Double = a.Average
            Dim bMean As Double = b.Average

            For i As Integer = 0 To a.Length - 1
                sum += (a(i) - aMean) * (b(i) - bMean)
            Next

            Return sum / divisor
        End Function

        ''' <summary>
        ''' 请注意,<paramref name="data"/>的元素数量必须要和<paramref name="weights"/>的长度相等
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="weights">这个数组里面的值的和必须要等于1</param>
        ''' <returns></returns>
        <Extension>
        Public Function WeighedAverage(data As IEnumerable(Of Double), ParamArray weights As Double()) As Double
            Dim avg#

            For Each x As SeqValue(Of Double) In data.SeqIterator
                avg += (x.value * weights(x))
            Next

            Return avg
        End Function

        ''' <summary>
        ''' [Sequence Generation] Generate regular sequences. seq is a standard generic with a default method.
        ''' </summary>
        ''' <param name="From">
        ''' the starting and (maximal) end values of the sequence. Of length 1 unless just from is supplied as an unnamed argument.
        ''' </param>
        ''' <param name="To">
        ''' the starting and (maximal) end values of the sequence. Of length 1 unless just from is supplied as an unnamed argument.
        ''' </param>
        ''' <param name="By">number: increment of the sequence</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <Extension>
        Public Iterator Function seq([from] As Value(Of Double), [to] As Double, Optional by As Double = 0.1) As IEnumerable(Of Double)
            Yield from

            If by > 0 Then
                Do While (from = from.Value + by) <= [to]
                    Yield from
                Loop
            Else
                Do While (from = from.Value + by) >= [to]
                    Yield from
                Loop
            End If
        End Function

        <Extension>
        Public Iterator Function seq(range As DoubleRange, Optional steps# = 0.1) As IEnumerable(Of Double)
            For Each x# In seq(range.Min, range.Max, steps)
                Yield x#
            Next
        End Function

        ''' <summary>
        ''' 以 N 为底的对数 ``LogN(X) = Log(X) / Log(N)`` 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="N"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function LogN(x As Double, N As Double) As Double
            Return std.Log(x) / std.Log(N)
        End Function

        ''' <summary>
        ''' return the maximum of a, b and c </summary>
        ''' <param name="a"> </param>
        ''' <param name="b"> </param>
        ''' <param name="c">
        ''' @return </param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Max(a As Integer, b As Integer, c As Integer) As Integer
            Return std.Max(a, std.Max(b, c))
        End Function

        ''' <summary>
        ''' return the maximum of a, b and c </summary>
        ''' <param name="a"> </param>
        ''' <param name="b"> </param>
        ''' <param name="c">
        ''' @return </param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Max(a As Double, b As Double, c As Double) As Double
            Return std.Max(a, std.Max(b, c))
        End Function

        ''' <summary>
        '''  sqrt(a^2 + b^2) without under/overflow.
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>

        Public Function Hypot(a As Double, b As Double) As Double
            Dim r As Double

            If std.Abs(a) > std.Abs(b) Then
                r = b / a
                r = std.Abs(a) * std.Sqrt(1 + r * r)
            ElseIf b <> 0 Then
                r = a / b
                r = std.Abs(b) * std.Sqrt(1 + r * r)
            Else
                r = 0.0
            End If

            Return r
        End Function

        ''' <summary>
        ''' Calculates power of 2.
        ''' </summary>
        ''' 
        ''' <param name="power">Power to raise in.</param>
        ''' 
        ''' <returns>Returns specified power of 2 in the case if power is in the range of
        ''' [0, 30]. Otherwise returns 0.</returns>
        ''' 
        Public Function Pow2(power As Integer) As Integer
            Return If(((power >= 0) AndAlso (power <= 30)), (1 << power), 0)
        End Function

        ''' <summary>
        ''' Get base of binary logarithm.
        ''' </summary>
        ''' 
        ''' <param name="x">Source integer number.</param>
        ''' 
        ''' <returns>Power of the number (base of binary logarithm).</returns>
        ''' 
        Public Function Log2(x As Integer) As Integer
            If x <= 65536 Then
                If x <= 256 Then
                    If x <= 16 Then
                        If x <= 4 Then
                            If x <= 2 Then
                                If x <= 1 Then
                                    Return 0
                                End If
                                Return 1
                            End If
                            Return 2
                        End If
                        If x <= 8 Then
                            Return 3
                        End If
                        Return 4
                    End If
                    If x <= 64 Then
                        If x <= 32 Then
                            Return 5
                        End If
                        Return 6
                    End If
                    If x <= 128 Then
                        Return 7
                    End If
                    Return 8
                End If
                If x <= 4096 Then
                    If x <= 1024 Then
                        If x <= 512 Then
                            Return 9
                        End If
                        Return 10
                    End If
                    If x <= 2048 Then
                        Return 11
                    End If
                    Return 12
                End If
                If x <= 16384 Then
                    If x <= 8192 Then
                        Return 13
                    End If
                    Return 14
                End If
                If x <= 32768 Then
                    Return 15
                End If
                Return 16
            End If

            If x <= 16777216 Then
                If x <= 1048576 Then
                    If x <= 262144 Then
                        If x <= 131072 Then
                            Return 17
                        End If
                        Return 18
                    End If
                    If x <= 524288 Then
                        Return 19
                    End If
                    Return 20
                End If
                If x <= 4194304 Then
                    If x <= 2097152 Then
                        Return 21
                    End If
                    Return 22
                End If
                If x <= 8388608 Then
                    Return 23
                End If
                Return 24
            End If
            If x <= 268435456 Then
                If x <= 67108864 Then
                    If x <= 33554432 Then
                        Return 25
                    End If
                    Return 26
                End If
                If x <= 134217728 Then
                    Return 27
                End If
                Return 28
            End If
            If x <= 1073741824 Then
                If x <= 536870912 Then
                    Return 29
                End If
                Return 30
            End If
            Return 31
        End Function

        ''' <summary>
        ''' Checks if the specified integer is power of 2.
        ''' </summary>
        ''' 
        ''' <param name="x">Integer number to check.</param>
        ''' 
        ''' <returns>Returns <b>true</b> if the specified number is power of 2.
        ''' Otherwise returns <b>false</b>.</returns>
        ''' 
        <Extension>
        Public Function IsPowerOf2(x As Integer) As Boolean
            Return If((x > 0), ((x And (x - 1)) = 0), False)
        End Function

        <Extension()>
        Public Function IsPowerOf2(n As Long) As Boolean
            If n > 0 AndAlso (n And n - 1) = 0 Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Logical true values are regarded as one, false values as zero. For historical reasons, NULL is accepted and treated as if it were integer(0).
        ''' </summary>
        ''' <param name="bc"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Sum")>
        <Extension>
        Public Function Sum(bc As IEnumerable(Of Boolean)) As Double
            If bc Is Nothing Then
                Return 0
            Else
                Return bc _
                    .Select(Function(b) If(True = b, 1.0R, 0R)) _
                    .Sum
            End If
        End Function

#If NET_48 = 1 Or netcore5 = 1 Then

#Region "Sum all tuple members"

        ''' <summary>
        ''' Sum all tuple members
        ''' </summary>
        ''' <param name="t"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Sum(t As ValueTuple(Of Double, Double)) As Double
            Return t.Item1 + t.Item2
        End Function

        ''' <summary>
        ''' Sum all tuple members
        ''' </summary>
        ''' <param name="t"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Sum(t As ValueTuple(Of Double, Double, Double)) As Double
            Return t.Item1 + t.Item2 + t.Item3
        End Function

        ''' <summary>
        ''' Sum all tuple members
        ''' </summary>
        ''' <param name="t"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Sum(t As ValueTuple(Of Double, Double, Double, Double)) As Double
            Return t.Item1 + t.Item2 + t.Item3 + t.Item4
        End Function

        ''' <summary>
        ''' Sum all tuple members
        ''' </summary>
        ''' <param name="t"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Sum(t As ValueTuple(Of Double, Double, Double, Double, Double)) As Double
            Return t.Item1 + t.Item2 + t.Item3 + t.Item4 + t.Item5
        End Function
#End Region

#End If

        ''' <summary>
        ''' 计算出所有的数的乘积
        ''' </summary>
        ''' <param name="[in]"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ProductALL([in] As IEnumerable(Of Double)) As Double
            Dim product# = 1

            ' 因为会存在 0 * Inf = NaN
            ' 所以在下面做了一下if判断来避免出现这种情况的NaN值

            For Each x As Double In [in]
                ' 0乘上任何数应该都是零来的
                If x = 0R Then
                    Return 0
                Else
                    product *= x
                End If
            Next

            Return product
        End Function

        ''' <summary>
        ''' 计算出所有的数的乘积
        ''' </summary>
        ''' <param name="[in]"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ProductALL([in] As IEnumerable(Of Integer)) As Double
            Return [in].Select(Function(x) CDbl(x)).ProductALL
        End Function

        ''' <summary>
        ''' 计算出所有的数的乘积
        ''' </summary>
        ''' <param name="[in]"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ProductALL([in] As IEnumerable(Of Long)) As Double
            Return [in].Select(Function(x) CDbl(x)).ProductALL
        End Function

        ''' <summary>
        ''' ## Standard Deviation
        ''' 
        ''' In statistics, the standard deviation (SD, also represented by the Greek letter sigma σ or the Latin letter s) 
        ''' is a measure that is used to quantify the amount of variation or dispersion of a set of data values. A low 
        ''' standard deviation indicates that the data points tend to be close to the mean (also called the expected value) 
        ''' of the set, while a high standard deviation indicates that the data points are spread out over a wider range of 
        ''' values.
        ''' 
        ''' > https://en.wikipedia.org/wiki/Standard_deviation
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <Extension>
        Public Function SD(values As IEnumerable(Of Double), Optional isSample As Boolean = False) As Double
            Dim x As Double() = values.ToArray
            Dim n As Integer = x.Length
            Dim var As Double = x.Variance

            If isSample Then
                Return std.Sqrt(var / (n - 1))
            Else
                Return std.Sqrt(var / n)
            End If
        End Function

        ''' <summary>
        ''' Standard Deviation
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <Extension>
        Public Function SD(values As IEnumerable(Of Integer)) As Double
            Return values.Select(Function(x) CDbl(x)).SD
        End Function

        ''' <summary>
        ''' Standard Deviation
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <Extension>
        Public Function SD(values As IEnumerable(Of Long)) As Double
            Return values.Select(Function(x) CDbl(x)).SD
        End Function

        ''' <summary>
        ''' Standard Deviation
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <Extension>
        Public Function SD(values As IEnumerable(Of Single)) As Double
            Return values.Select(Function(x) CDbl(x)).SD
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("RangesAt")>
        <Extension>
        Public Function RangesAt(n As Double, LowerBound As Double, UpBound As Double) As Boolean
            Return n <= UpBound AndAlso n > LowerBound
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("RangesAt")>
        <Extension>
        Public Function RangesAt(n As Integer, LowerBound As Double, UpBound As Double) As Boolean
            Return n <= UpBound AndAlso n > LowerBound
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("RangesAt")>
        <Extension>
        Public Function RangesAt(n As Long, LowerBound As Double, UpBound As Double) As Boolean
            Return n <= UpBound AndAlso n > LowerBound
        End Function

        ''' <summary>
        ''' Root mean square.(均方根)
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <ExportAPI("RMS")>
        <Extension>
        Public Function RMS(data As IEnumerable(Of Double)) As Double
            With (From n In data Select n ^ 2).ToArray
                Return std.Sqrt(.Sum / .Length)
            End With
        End Function

        Public Function RMSE(a#(), b#()) As Double
            Dim sum#
            Dim n% = a.Length

            For i As Integer = 0 To n - 1
                sum += (a(i) - b(i)) ^ 2
            Next

            Return std.Sqrt(sum)
        End Function

        ''' <summary>
        ''' ## relative standard deviation
        ''' 
        ''' Relative standard deviation is a common formula 
        ''' used in statistics and probability theory to determine
        ''' a standardized measure of the ratio of the standard
        ''' deviation to the mean. This formula is useful in
        ''' various situations including when comparing your 
        ''' own data to other related data and in financial 
        ''' settings such as the stock market.
        ''' 
        ''' Relative standard deviation, which also may be referred 
        ''' to as RSD or the coefficient of variation, is used 
        ''' to determine if the standard deviation of a set of 
        ''' data is small or large when compared to the mean.
        ''' In other words, the relative standard deviation can
        ''' tell you how precise the average of your results is.
        ''' This formula is most frequently used in chemistry, 
        ''' statistics and other math-related settings but can 
        ''' also be used in the business world when assessing
        ''' finances and the stock market.
        ''' 
        ''' The relative standard deviation Of a Set Of data can be
        ''' depicted As either a percentage Or As a number. The 
        ''' higher the relative standard deviation, the more spread 
        ''' out the results are from the mean Of the data. On the
        ''' other hand, a lower relative standard deviation means 
        ''' that the measurement Of data Is more precise.
        ''' 
        ''' (``相对标准偏差（RSD）= 标准偏差（SD）/ 计算结果的算术平均值（X）* 100%``)
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' RSD is also an alias of ``CV%``
        ''' </remarks>
        <Extension>
        Public Function RSD(data As IEnumerable(Of Double)) As Double
            Dim vec As Double() = data.ToArray
            Dim sd As Double = vec.SD

            If sd = 0.0 Then
                Return 0
            Else
                Return sd / vec.Average
            End If
        End Function

        <Extension>
        Public Function RSD(data As IEnumerable(Of Double), maxSize As Integer) As Double
            Dim vec As Double() = data.ToArray
            Dim sd As Double

            If vec.Length < maxSize Then
                vec = vec _
                    .JoinIterates(0.0.Repeats(maxSize - vec.Length)) _
                    .ToArray
            End If

            sd = vec.SD

            If sd = 0.0 Then
                Return 0
            Else
                Return sd / vec.Average
            End If
        End Function

        ''' <summary>
        ''' Returns the PDF value at x for the specified 
        ''' Poisson distribution.
        ''' </summary>
        ''' 
        Public Function PoissonPDF(x As Integer, lambda As Double) As Double
            Dim result As Double = std.Exp(-lambda)
            Dim k As Integer = x

            While k >= 1
                result *= lambda / k
                k -= 1
            End While

            Return result
        End Function
    End Module
End Namespace
