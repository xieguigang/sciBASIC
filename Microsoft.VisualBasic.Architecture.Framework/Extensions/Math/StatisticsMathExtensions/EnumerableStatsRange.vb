Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.CompilerServices

Namespace StatisticsMathExtensions

    Public Module EnumerableStatsRange
        '
        ' Summary:
        '     Computes the Range of a sequence of nullable System.Decimal values.
        '
        ' Parameters:
        '   source:
        '     A sequence of nullable System.Decimal values to calculate the Range of.
        '
        ' Returns:
        '     The Range of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Decimal.MaxValue.
        <Extension>
        Public Function Range(source As IEnumerable(Of System.Nullable(Of Decimal))) As System.Nullable(Of Decimal)
            Dim values As IEnumerable(Of Decimal) = source.Coalesce()
            If values.Any() Then
                Return values.Range()
            End If

            Return Nothing
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of System.Decimal values.
        '
        ' Parameters:
        '   source:
        '     A sequence of System.Decimal values to calculate the Range of.
        '
        ' Returns:
        '     The Range of the sequence of values.
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
        Public Function Range(source As IEnumerable(Of Decimal)) As Decimal
            Return source.Max() - source.Min()
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of nullable System.Double values.
        '
        ' Parameters:
        '   source:
        '     A sequence of nullable System.Double values to calculate the Range of.
        '
        ' Returns:
        '     The Range of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        <Extension>
        Public Function Range(source As IEnumerable(Of System.Nullable(Of Double))) As System.Nullable(Of Double)
            Dim values As IEnumerable(Of Double) = source.Coalesce()
            If values.Any() Then
                Return values.Range()
            End If

            Return Nothing
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of System.Double values.
        '
        ' Parameters:
        '   source:
        '     A sequence of System.Double values to calculate the Range of.
        '
        ' Returns:
        '     The Range of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        <Extension>
        Public Function Range(source As IEnumerable(Of Double)) As Double
            Return source.Max() - source.Min()
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of nullable System.Single values.
        '
        ' Parameters:
        '   source:
        '     A sequence of nullable System.Single values to calculate the Range of.
        '
        ' Returns:
        '     The Range of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        <Extension>
        Public Function Range(source As IEnumerable(Of System.Nullable(Of Single))) As System.Nullable(Of Single)
            Dim values As IEnumerable(Of Single) = source.Coalesce()
            If values.Any() Then
                Return values.Range()
            End If

            Return Nothing
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of System.Single values.
        '
        ' Parameters:
        '   source:
        '     A sequence of System.Single values to calculate the Range of.
        '
        ' Returns:
        '     The Range of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        <Extension>
        Public Function Range(source As IEnumerable(Of Single)) As Single
            Return source.Max() - source.Min()
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of nullable System.Int32 values.
        '
        ' Parameters:
        '   source:
        '     A sequence of nullable System.Int32values to calculate the Range of.
        '
        ' Returns:
        '     The Range of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Int64.MaxValue.
        <Extension>
        Public Function Range(source As IEnumerable(Of System.Nullable(Of Integer))) As System.Nullable(Of Integer)
            Dim values As IEnumerable(Of Integer) = source.Coalesce()
            If values.Any() Then
                Return values.Range()
            End If

            Return Nothing
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of System.Int32 values.
        '
        ' Parameters:
        '   source:
        '     A sequence of System.Int32 values to calculate the Range of.
        '
        ' Returns:
        '     The Range of the sequence of values.
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
        Public Function Range(source As IEnumerable(Of Integer)) As Integer
            Return source.Max() - source.Min()
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of nullable System.Int64 values.
        '
        ' Parameters:
        '   source:
        '     A sequence of nullable System.Int64 values to calculate the Range of.
        '
        ' Returns:
        '     The Range of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Int64.MaxValue.
        <Extension>
        Public Function Range(source As IEnumerable(Of System.Nullable(Of Long))) As System.Nullable(Of Long)
            Dim values As IEnumerable(Of Long) = source.Coalesce()
            If values.Any() Then
                Return values.Range()
            End If

            Return Nothing
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of System.Int64 values.
        '
        ' Parameters:
        '   source:
        '     A sequence of System.Int64 values to calculate the Range of.
        '
        ' Returns:
        '     The Range of the sequence of values.
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
        Public Function Range(source As IEnumerable(Of Long)) As Long
            Return source.Max() - source.Min()
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of nullable System.Decimal values that
        '     are obtained by invoking a transform function on each element of the input
        '     sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Range of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Range of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Decimal.MaxValue.
        <Extension>
        Public Function Range(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, System.Nullable(Of Decimal))) As System.Nullable(Of Decimal)
            Return source.[Select](selector).Range()
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of System.Decimal values that are obtained
        '     by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values that are used to calculate an Range.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Range of the sequence of values.
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
        Public Function Range(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, Decimal)) As Decimal
            Return source.[Select](selector).Range()
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of nullable System.Double values that
        '     are obtained by invoking a transform function on each element of the input
        '     sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Range of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Range of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        <Extension>
        Public Function Range(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, System.Nullable(Of Double))) As System.Nullable(Of Double)
            Return source.[Select](selector).Range()
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of System.Double values that are obtained
        '     by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Range of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Range of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        <Extension>
        Public Function Range(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, Double)) As Double
            Return source.[Select](selector).Range()
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of nullable System.Single values that
        '     are obtained by invoking a transform function on each element of the input
        '     sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Range of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Range of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        <Extension>
        Public Function Range(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, System.Nullable(Of Single))) As System.Nullable(Of Single)
            Return source.[Select](selector).Range()
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of System.Single values that are obtained
        '     by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Range of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Range of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        <Extension>
        Public Function Range(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, Single)) As Single
            Return source.[Select](selector).Range()
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of nullable System.Int32 values that are
        '     obtained by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Range of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Range of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Int64.MaxValue.
        <Extension>
        Public Function Range(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, System.Nullable(Of Integer))) As System.Nullable(Of Double)
            Return source.[Select](selector).Range()
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of System.Int32 values that are obtained
        '     by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Range of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Range of the sequence of values.
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
        Public Function Range(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, Integer)) As Double
            Return source.[Select](selector).Range()
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of nullable System.Int64 values that are
        '     obtained by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Range of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Range of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        <Extension>
        Public Function Range(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, System.Nullable(Of Long))) As System.Nullable(Of Double)
            Return source.[Select](selector).Range()
        End Function
        '
        ' Summary:
        '     Computes the Range of a sequence of System.Int64 values that are obtained
        '     by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Range of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Range of the sequence of values.
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
        Public Function Range(Of TSource)(source As IEnumerable(Of TSource), selector As Func(Of TSource, Long)) As Double
            Return source.[Select](selector).Range()
        End Function
    End Module
End Namespace