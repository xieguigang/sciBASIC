#Region "Microsoft.VisualBasic::e343eccc7cce6e1b3aad66cd68a046b6, Microsoft.VisualBasic.Core\src\Extensions\Math\StatisticsMathExtensions\Linq\EnumerableStatsVariance.vb"

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

    '   Total Lines: 554
    '    Code Lines: 111 (20.04%)
    ' Comment Lines: 435 (78.52%)
    '    - Xml Docs: 0.69%
    ' 
    '   Blank Lines: 8 (1.44%)
    '     File Size: 21.81 KB


    '     Module EnumerableStatsVariance
    ' 
    '         Function: (+20 Overloads) Variance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Math.Statistics.Linq

    ''' <summary>
    ''' make sqrt of the variance result will be the standard deviation
    ''' </summary>
    Public Module EnumerableStatsVariance

        '
        ' Summary:
        '     Computes the Variance of a sequence of nullable System.Decimal values.
        '
        ' Parameters:
        '   source:
        '     A sequence of nullable System.Decimal values to calculate the Variance of.
        '
        ' Returns:
        '     The Variance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Decimal.MaxValue.
        <Extension>
        Public Function Variance(source As IEnumerable(Of Decimal?), Optional isSample As Boolean = True) As Decimal
            Dim values As IEnumerable(Of Decimal) = source.Coalesce()
            If values.Any() Then
                Return values.Variance(isSample)
            End If

            Return Nothing
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of System.Decimal values.
        '
        ' Parameters:
        '   source:
        '     A sequence of System.Decimal values to calculate the Variance of.
        '
        ' Returns:
        '     The Variance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Decimal.MaxValue.
        <Extension>
        Public Function Variance(source As IEnumerable(Of Decimal), Optional isSample As Boolean = True) As Decimal
            Return CDec(source.[Select](Function(x) CDbl(x)).Variance(isSample))
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of nullable System.Double values.
        '
        ' Parameters:
        '   source:
        '     A sequence of nullable System.Double values to calculate the Variance of.
        '
        ' Returns:
        '     The Variance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        <Extension>
        Public Function Variance(source As IEnumerable(Of Double?), Optional isSample As Boolean = True) As Double
            Dim values As IEnumerable(Of Double) = source.Coalesce()
            If values.Any() Then
                Return values.Variance(isSample)
            End If

            Return Nothing
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of System.Double values.
        '
        ' Parameters:
        '   source:
        '     A sequence of System.Double values to calculate the Variance of.
        '
        ' Returns:
        '     The Variance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        <Extension>
        Public Function Variance(source As IEnumerable(Of Double), Optional isSample As Boolean = True) As Double
            Dim pull = source.ToArray
            Dim avg As Double = pull.Average()
            Dim d As Double = pull.Aggregate(0.0, Function(total, [next]) As Double
                                                      total += ([next] - avg) ^ 2
                                                      Return total
                                                  End Function)
            Return d / If(isSample, pull.Length - 1, pull.Length)
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of nullable System.Single values.
        '
        ' Parameters:
        '   source:
        '     A sequence of nullable System.Single values to calculate the Variance of.
        '
        ' Returns:
        '     The Variance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        <Extension>
        Public Function Variance(source As IEnumerable(Of Single?), Optional isSample As Boolean = True) As Single
            Dim values As IEnumerable(Of Single) = source.Coalesce()
            If values.Any() Then
                Return values.Variance(isSample)
            End If

            Return Nothing
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of System.Single values.
        '
        ' Parameters:
        '   source:
        '     A sequence of System.Single values to calculate the Variance of.
        '
        ' Returns:
        '     The Variance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        <Extension>
        Public Function Variance(source As IEnumerable(Of Single), Optional isSample As Boolean = True) As Single
            Return CSng(source.[Select](Function(x) CDbl(x)).Variance(isSample))
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of nullable System.Int32 values.
        '
        ' Parameters:
        '   source:
        '     A sequence of nullable System.Int32values to calculate the Variance of.
        '
        ' Returns:
        '     The Variance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Int64.MaxValue.
        <Extension>
        Public Function Variance(source As IEnumerable(Of Integer?), Optional isSample As Boolean = True) As Double
            Dim values As IEnumerable(Of Integer) = source.Coalesce()
            If values.Any() Then
                Return values.Variance(isSample)
            End If

            Return Nothing
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of System.Int32 values.
        '
        ' Parameters:
        '   source:
        '     A sequence of System.Int32 values to calculate the Variance of.
        '
        ' Returns:
        '     The Variance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Int64.MaxValue.
        <Extension>
        Public Function Variance(source As IEnumerable(Of Integer), Optional isSample As Boolean = True) As Double
            Return source.[Select](Function(x) CDbl(x)).Variance(isSample)
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of nullable System.Int64 values.
        '
        ' Parameters:
        '   source:
        '     A sequence of nullable System.Int64 values to calculate the Variance of.
        '
        ' Returns:
        '     The Variance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Int64.MaxValue.
        <Extension>
        Public Function Variance(source As IEnumerable(Of Long?), Optional isSample As Boolean = True) As Double
            Dim values As IEnumerable(Of Long) = source.Coalesce()
            If values.Any() Then
                Return values.Variance(isSample)
            End If

            Return Nothing
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of System.Int64 values.
        '
        ' Parameters:
        '   source:
        '     A sequence of System.Int64 values to calculate the Variance of.
        '
        ' Returns:
        '     The Variance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Int64.MaxValue.
        <Extension>
        Public Function Variance(source As IEnumerable(Of Long), Optional isSample As Boolean = True) As Double
            Return source.Select(Function(x) CDbl(x)).Variance(isSample)
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of nullable System.Decimal values that
        '     are obtained by invoking a transform function on each element of the input
        '     sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Variance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Variance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Decimal.MaxValue.
        <Extension>
        Public Function Variance(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, Decimal?), Optional isSample As Boolean = True) As Decimal
            Return source.[Select](selector).Variance(isSample)
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of System.Decimal values that are obtained
        '     by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values that are used to calculate an Variance.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Variance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Decimal.MaxValue.
        <Extension>
        Public Function Variance(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, Decimal), Optional isSample As Boolean = True) As Decimal
            Return source.[Select](selector).Variance(isSample)
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of nullable System.Double values that
        '     are obtained by invoking a transform function on each element of the input
        '     sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Variance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Variance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        <Extension>
        Public Function Variance(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, Double?), Optional isSample As Boolean = True) As Double
            Return source.[Select](selector).Variance(isSample)
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of System.Double values that are obtained
        '     by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Variance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Variance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        <Extension>
        Public Function Variance(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, Double), Optional isSample As Boolean = True) As Double
            Return source.[Select](selector).Variance(isSample)
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of nullable System.Single values that
        '     are obtained by invoking a transform function on each element of the input
        '     sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Variance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Variance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        <Extension>
        Public Function Variance(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, Single?), Optional isSample As Boolean = True) As Single
            Return source.[Select](selector).Variance(isSample)
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of System.Single values that are obtained
        '     by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Variance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Variance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        <Extension>
        Public Function Variance(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, Single), Optional isSample As Boolean = True) As Single
            Return source.[Select](selector).Variance(isSample)
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of nullable System.Int32 values that are
        '     obtained by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Variance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Variance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Int64.MaxValue.
        <Extension>
        Public Function Variance(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, Integer?), Optional isSample As Boolean = True) As Double
            Return source.[Select](selector).Variance(isSample)
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of System.Int32 values that are obtained
        '     by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Variance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Variance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Int64.MaxValue.
        <Extension>
        Public Function Variance(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, Integer), Optional isSample As Boolean = True) As Double
            Return source.[Select](selector).Variance(isSample)
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of nullable System.Int64 values that are
        '     obtained by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Variance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Variance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        <Extension>
        Public Function Variance(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, Long?), Optional isSample As Boolean = True) As Double
            Return source.Select(selector).Variance(isSample)
        End Function
        '
        ' Summary:
        '     Computes the Variance of a sequence of System.Int64 values that are obtained
        '     by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Variance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Variance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Int64.MaxValue.
        <Extension>
        Public Function Variance(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, Long), Optional isSample As Boolean = True) As Double
            Return source.Select(selector).Variance(isSample)
        End Function
    End Module
End Namespace
