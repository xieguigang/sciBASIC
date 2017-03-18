#Region "Microsoft.VisualBasic::d4e9899c151d3f2bff40ae04987560e4, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Math\Math.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Mathematical

    <PackageNamespace("VBMath", Publisher:="xie.guigang@gmail.com")>
    Public Module VBMathExtensions

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

            Do While (from = from.value + by) <= [to]
                Yield from
            Loop
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
        Public Function LogN(x As Double, N As Double) As Double
            Return Math.Log(x) / Math.Log(N)
        End Function

        ''' <summary>
        ''' return the maximum of a, b and c </summary>
        ''' <param name="a"> </param>
        ''' <param name="b"> </param>
        ''' <param name="c">
        ''' @return </param>
        Public Function Max(a As Integer, b As Integer, c As Integer) As Integer
            Return Math.Max(a, Math.Max(b, c))
        End Function

        ''' <summary>
        '''  sqrt(a^2 + b^2) without under/overflow.
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>

        Public Function Hypot(a As Double, b As Double) As Double
            Dim r As Double

            If Math.Abs(a) > Math.Abs(b) Then
                r = b / a
                r = Math.Abs(a) * Math.Sqrt(1 + r * r)
            ElseIf b <> 0 Then
                r = a / b
                r = Math.Abs(b) * Math.Sqrt(1 + r * r)
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

        ''' <summary>
        ''' Logical true values are regarded as one, false values as zero. For historical reasons, NULL is accepted and treated as if it were integer(0).
        ''' </summary>
        ''' <param name="bc"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Sum")>
        <Extension> Public Function Sum(bc As IEnumerable(Of Boolean)) As Double
            If bc.IsNullOrEmpty Then
                Return 0
            End If

            Dim LQuery = (From b In bc Select If(b, 1, 0)).ToArray
            Dim value As Double = LQuery.Sum
            Return value
        End Function

        ''' <summary>
        ''' Standard Deviation
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("STD", Info:="Standard Deviation")>
        <Extension> Public Function STD(values As IEnumerable(Of Double)) As Double
            Dim Avg As Double = values.Average
            Dim LQuery = (From n As Double In values Select (n - Avg) ^ 2).ToArray
            Return Math.Sqrt(LQuery.Sum / LQuery.Length)
        End Function

        ''' <summary>
        ''' Standard Deviation
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("STD", Info:="Standard Deviation")>
        <Extension> Public Function STD(values As IEnumerable(Of Integer)) As Double
            Return values.Select(Function(x) CDbl(x)).STD
        End Function

        ''' <summary>
        ''' Standard Deviation
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("STD", Info:="Standard Deviation")>
        <Extension> Public Function STD(values As IEnumerable(Of Long)) As Double
            Return values.Select(Function(x) CDbl(x)).STD
        End Function

        ''' <summary>
        ''' Standard Deviation
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("STD", Info:="Standard Deviation")>
        <Extension> Public Function STD(values As IEnumerable(Of Single)) As Double
            Return values.Select(Function(x) CDbl(x)).STD
        End Function

        ''' <summary>
        ''' 多位坐标的欧几里得距离
        ''' </summary>
        ''' <param name="Vector"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Euclidean", Info:="Euclidean Distance")>
        <Extension> Public Function EuclideanDistance(Vector As IEnumerable(Of Double)) As Double
            Return Math.Sqrt((From n In Vector Select n ^ 2).Sum)
        End Function

        <ExportAPI("Euclidean", Info:="Euclidean Distance")>
        <Extension> Public Function EuclideanDistance(Vector As IEnumerable(Of Integer)) As Double
            Return Math.Sqrt((From n In Vector Select n ^ 2).Sum)
        End Function

        <ExportAPI("Euclidean", Info:="Euclidean Distance")>
        <Extension> Public Function EuclideanDistance(a As IEnumerable(Of Integer), b As IEnumerable(Of Integer)) As Double
            If a.Count <> b.Count Then
                Return -1
            Else
                Return Math.Sqrt((From i As Integer In a.Sequence Select (a(i) - b(i)) ^ 2).Sum)
            End If
        End Function

        <ExportAPI("Euclidean", Info:="Euclidean Distance")>
        <Extension> Public Function EuclideanDistance(a As IEnumerable(Of Double), b As IEnumerable(Of Double)) As Double
            Return EuclideanDistance(a.ToArray, b.ToArray)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="a">Point A</param>
        ''' <param name="b">Point B</param>
        ''' <returns></returns>
        <ExportAPI("Euclidean", Info:="Euclidean Distance")>
        <Extension> Public Function EuclideanDistance(a As Byte(), b As Byte()) As Double
            If a.Length <> b.Length Then
                Return -1.0R
            Else
                Return Math.Sqrt((From i As Integer In a.Sequence Select (CInt(a(i)) - CInt(b(i))) ^ 2).Sum)
            End If
        End Function

        ''' <summary>
        ''' 计算两个向量之间的欧氏距离，请注意，这两个向量的长度必须要相等
        ''' </summary>
        ''' <param name="a">Point A</param>
        ''' <param name="b">Point B</param>
        ''' <returns></returns>
        <ExportAPI("Euclidean", Info:="Euclidean Distance")>
        <Extension> Public Function EuclideanDistance(a As Double(), b As Double()) As Double
            If a.Length <> b.Length Then
                Return -1.0R
            Else
                Return Math.Sqrt((From i As Integer In a.Sequence Select (a(i) - b(i)) ^ 2).Sum)
            End If
        End Function

        ''' <summary>
        ''' Continues multiply operations.(连续乘法)
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        <ExportAPI("PI")>
        <Extension> Public Function PI(data As IEnumerable(Of Double)) As Double
            Dim value As Double = 1
            For Each n In data
                value = value * n
            Next

            Return value
        End Function

        <ExportAPI("RangesAt")>
        <Extension> Public Function RangesAt(n As Double, LowerBound As Double, UpBound As Double) As Boolean
            Return n <= UpBound AndAlso n > LowerBound
        End Function

        <ExportAPI("RangesAt")>
        <Extension> Public Function RangesAt(n As Integer, LowerBound As Double, UpBound As Double) As Boolean
            Return n <= UpBound AndAlso n > LowerBound
        End Function

        <ExportAPI("RangesAt")>
        <Extension> Public Function RangesAt(n As Long, LowerBound As Double, UpBound As Double) As Boolean
            Return n <= UpBound AndAlso n > LowerBound
        End Function

        ''' <summary>
        ''' Root mean square.(均方根)
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <ExportAPI("RMS", Info:="Root mean square")>
        <Extension> Public Function RMS(data As IEnumerable(Of Double)) As Double
            Dim LQuery = (From n In data.AsParallel Select n ^ 2).ToArray
            Return Math.Sqrt(LQuery.Sum / LQuery.Length)
        End Function

        ''' <summary>
        ''' Returns the PDF value at x for the specified Poisson distribution.
        ''' </summary>
        ''' 
        <ExportAPI("Poisson.PDF", Info:="Returns the PDF value at x for the specified Poisson distribution.")>
        Public Function PoissonPDF(x As Integer, lambda As Double) As Double
            Dim result As Double = Math.Exp(-lambda)
            Dim k As Integer = x
            While k >= 1
                result *= lambda / k
                k -= 1
            End While
            Return result
        End Function

        <Extension>
        Public Function FormatNumeric(v As IEnumerable(Of Double), Optional digitals% = 2) As String()
            Return v.ToArray(Function(x) x.FormatNumeric(digitals))
        End Function
    End Module
End Namespace
