#Region "Microsoft.VisualBasic::367e0b4851290a7085cb39e0517832b6, Microsoft.VisualBasic.Core\Extensions\Math\Math.vb"

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

    '     Module VBMath
    ' 
    '         Function: (+7 Overloads) Abs, Acos, Asin, Atan, Atan2
    '                   BigMul, (+2 Overloads) Ceiling, Cos, Cosh, Covariance
    '                   CumSum, Distance, (+2 Overloads) DivRem, (+6 Overloads) EuclideanDistance, Exp
    '                   Factorial, FactorialSequence, (+2 Overloads) Floor, FormatNumeric, Hypot
    '                   IEEERemainder, IsPowerOf2, (+2 Overloads) Log, Log10, (+2 Overloads) Log2
    '                   LogN, (+12 Overloads) Max, (+11 Overloads) Min, PoissonPDF, Pow
    '                   Pow2, (+3 Overloads) ProductALL, (+3 Overloads) RangesAt, RMS, RMSE
    '                   (+8 Overloads) Round, RSD, (+4 Overloads) SD, (+2 Overloads) seq, (+7 Overloads) Sign
    '                   Sin, Sinh, Sqrt, (+5 Overloads) Sum, Tan
    '                   Tanh, (+2 Overloads) Truncate, WeighedAverage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.ConstrainedExecution
Imports System.Security
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports stdNum = System.Math

Namespace Math

    ''' <summary>
    ''' Provides constants and static methods for trigonometric, logarithmic, and other
    ''' common mathematical functions.To browse the .NET Framework source code for this
    ''' type, see the Reference Source.
    ''' </summary>
    <Package("VBMath", Publisher:="xie.guigang@gmail.com")>
    Public Module VBMath

        ''' <summary>
        ''' Represents the ratio of the circumference of a circle to its diameter, specified
        ''' by the constant, π.
        ''' </summary>
        Public Const PI# = stdNum.PI
        ''' <summary>
        ''' Represents the natural logarithmic base, specified by the constant, e.
        ''' </summary>
        Public Const E# = stdNum.E

#Region "Imports System.Math"

        ''' <summary>
        ''' Returns the smaller of two 8-bit unsigned integers.
        ''' </summary>
        ''' <param name="val1">The first of two 8-bit unsigned integers to compare.</param>
        ''' <param name="val2">The second of two 8-bit unsigned integers to compare.</param>
        ''' <returns>Parameter val1 or val2, whichever is smaller.</returns>
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Min(val1 As Byte, val2 As Byte) As Byte
            If val1 < val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function

        ''' <summary>
        ''' Returns the smaller of two 8-bit signed integers.
        ''' </summary>
        ''' <param name="val1">The first of two 8-bit signed integers to compare.</param>
        ''' <param name="val2">The second of two 8-bit signed integers to compare.</param>
        ''' <returns>Parameter val1 or val2, whichever is smaller.</returns>
        <CLSCompliant(False)> <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Min(val1 As SByte, val2 As SByte) As SByte
            If val1 < val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function

        ''' <summary>
        ''' Returns the larger of two decimal numbers.
        ''' </summary>
        ''' <param name="val1">The first of two decimal numbers to compare.</param>
        ''' <param name="val2">The second of two decimal numbers to compare.</param>
        ''' <returns>Parameter val1 or val2, whichever is larger.</returns>
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Max(val1 As Decimal, val2 As Decimal) As Decimal
            If val1 > val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function

        ''' <summary>
        ''' Returns the larger of two double-precision floating-point numbers.
        ''' </summary>
        ''' <param name="val1">The first of two double-precision floating-point numbers to compare.</param>
        ''' <param name="val2">The second of two double-precision floating-point numbers to compare.</param>
        ''' <returns>Parameter val1 or val2, whichever is larger. If val1, val2, or both val1 and
        ''' val2 are equal to System.Double.NaN, System.Double.NaN is returned.</returns>
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Max(val1 As Double, val2 As Double) As Double
            If val1 > val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function

        ''' <summary>
        ''' Returns the larger of two single-precision floating-point numbers.
        ''' </summary>
        ''' <param name="val1">The first of two single-precision floating-point numbers to compare.</param>
        ''' <param name="val2">The second of two single-precision floating-point numbers to compare.</param>
        ''' <returns>Parameter val1 or val2, whichever is larger. If val1, or val2, or both val1 and
        ''' val2 are equal to System.Single.NaN, System.Single.NaN is returned.</returns>
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Max(val1 As Single, val2 As Single) As Single
            If val1 > val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function

        ''' <summary>
        ''' Returns the larger of two 64-bit unsigned integers.
        ''' </summary>
        ''' <param name="val1">The first of two 64-bit unsigned integers to compare.</param>
        ''' <param name="val2">The second of two 64-bit unsigned integers to compare.</param>
        ''' <returns>Parameter val1 or val2, whichever is larger.</returns>
        <CLSCompliant(False)> <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Max(val1 As ULong, val2 As ULong) As ULong
            If val1 > val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function

        ''' <summary>
        ''' Returns the larger of two 64-bit signed integers.
        ''' </summary>
        ''' <param name="val1">The first of two 64-bit signed integers to compare.</param>
        ''' <param name="val2">The second of two 64-bit signed integers to compare.</param>
        ''' <returns>Parameter val1 or val2, whichever is larger.</returns>
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Max(val1 As Long, val2 As Long) As Long
            If val1 > val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function

        ''' <summary>
        ''' Returns the larger of two 32-bit unsigned integers.
        ''' </summary>
        ''' <param name="val1">The first of two 32-bit unsigned integers to compare.</param>
        ''' <param name="val2">The second of two 32-bit unsigned integers to compare.</param>
        ''' <returns>Parameter val1 or val2, whichever is larger.</returns>
        <CLSCompliant(False)> <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Max(val1 As UInteger, val2 As UInteger) As UInteger
            If val1 > val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function
        '
        ' Summary:
        '     Returns the larger of two 32-bit signed integers.
        '
        ' Parameters:
        '   val1:
        '     The first of two 32-bit signed integers to compare.
        '
        '   val2:
        '     The second of two 32-bit signed integers to compare.
        '
        ' Returns:
        '     Parameter val1 or val2, whichever is larger.
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Max(val1 As Integer, val2 As Integer) As Integer
            If val1 > val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function
        '
        ' Summary:
        '     Returns the larger of two 16-bit unsigned integers.
        '
        ' Parameters:
        '   val1:
        '     The first of two 16-bit unsigned integers to compare.
        '
        '   val2:
        '     The second of two 16-bit unsigned integers to compare.
        '
        ' Returns:
        '     Parameter val1 or val2, whichever is larger.
        <CLSCompliant(False)> <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Max(val1 As UShort, val2 As UShort) As UShort
            If val1 > val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function
        '
        ' Summary:
        '     Returns the larger of two 16-bit signed integers.
        '
        ' Parameters:
        '   val1:
        '     The first of two 16-bit signed integers to compare.
        '
        '   val2:
        '     The second of two 16-bit signed integers to compare.
        '
        ' Returns:
        '     Parameter val1 or val2, whichever is larger.
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Max(val1 As Short, val2 As Short) As Short
            If val1 > val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function
        '
        ' Summary:
        '     Returns the larger of two 8-bit unsigned integers.
        '
        ' Parameters:
        '   val1:
        '     The first of two 8-bit unsigned integers to compare.
        '
        '   val2:
        '     The second of two 8-bit unsigned integers to compare.
        '
        ' Returns:
        '     Parameter val1 or val2, whichever is larger.
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Max(val1 As Byte, val2 As Byte) As Byte
            If val1 > val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function
        '
        ' Summary:
        '     Returns the larger of two 8-bit signed integers.
        '
        ' Parameters:
        '   val1:
        '     The first of two 8-bit signed integers to compare.
        '
        '   val2:
        '     The second of two 8-bit signed integers to compare.
        '
        ' Returns:
        '     Parameter val1 or val2, whichever is larger.
        <CLSCompliant(False)> <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Max(val1 As SByte, val2 As SByte) As SByte
            If val1 > val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function
        '
        ' Summary:
        '     Returns the absolute value of a System.Decimal number.
        '
        ' Parameters:
        '   value:
        '     A number that is greater than or equal to System.Decimal.MinValue, but less than
        '     or equal to System.Decimal.MaxValue.
        '
        ' Returns:
        '     A decimal number, x, such that 0 ≤ x ≤System.Decimal.MaxValue.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Abs(value As Decimal) As Decimal
            If value < 0 Then
                Return -value
            Else
                Return value
            End If
        End Function
        '
        ' Summary:
        '     Returns the smaller of two 16-bit signed integers.
        '
        ' Parameters:
        '   val1:
        '     The first of two 16-bit signed integers to compare.
        '
        '   val2:
        '     The second of two 16-bit signed integers to compare.
        '
        ' Returns:
        '     Parameter val1 or val2, whichever is smaller.
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Min(val1 As Short, val2 As Short) As Short
            If val1 < val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function
        '
        ' Summary:
        '     Returns the smaller of two 16-bit unsigned integers.
        '
        ' Parameters:
        '   val1:
        '     The first of two 16-bit unsigned integers to compare.
        '
        '   val2:
        '     The second of two 16-bit unsigned integers to compare.
        '
        ' Returns:
        '     Parameter val1 or val2, whichever is smaller.
        <CLSCompliant(False)> <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Min(val1 As UShort, val2 As UShort) As UShort
            If val1 < val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function
        '
        ' Summary:
        '     Returns the smaller of two 32-bit signed integers.
        '
        ' Parameters:
        '   val1:
        '     The first of two 32-bit signed integers to compare.
        '
        '   val2:
        '     The second of two 32-bit signed integers to compare.
        '
        ' Returns:
        '     Parameter val1 or val2, whichever is smaller.
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Min(val1 As Integer, val2 As Integer) As Integer
            If val1 < val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function
        '
        ' Summary:
        '     Returns the smaller of two 32-bit unsigned integers.
        '
        ' Parameters:
        '   val1:
        '     The first of two 32-bit unsigned integers to compare.
        '
        '   val2:
        '     The second of two 32-bit unsigned integers to compare.
        '
        ' Returns:
        '     Parameter val1 or val2, whichever is smaller.
        <CLSCompliant(False)> <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Min(val1 As UInteger, val2 As UInteger) As UInteger
            If val1 < val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function
        '
        ' Summary:
        '     Produces the full product of two 32-bit numbers.
        '
        ' Parameters:
        '   a:
        '     The first number to multiply.
        '
        '   b:
        '     The second number to multiply.
        '
        ' Returns:
        '     The number containing the product of the specified numbers.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function BigMul(a As Integer, b As Integer) As Long
            Return stdNum.BigMul(a, b)
        End Function
        '
        ' Summary:
        '     Returns an integer that indicates the sign of a decimal number.
        '
        ' Parameters:
        '   value:
        '     A signed decimal number.
        '
        ' Returns:
        '     A number that indicates the sign of value, as shown in the following table.Return
        '     value Meaning -1 value is less than zero. 0 value is equal to zero. 1 value is
        '     greater than zero.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Sign(value As Decimal) As Integer
            If value > 0 Then
                Return 1
            ElseIf value < 0 Then
                Return -1
            Else
                Return 0
            End If
        End Function

        ''' <summary>
        ''' Returns an integer that indicates the sign of a double-precision floating-point
        ''' number.
        ''' </summary>
        ''' <param name="value">A signed number.</param>
        ''' <returns>
        ''' A number that indicates the sign of value, as shown in the following table.Return
        ''' value Meaning -1 value is less than zero. 0 value is equal to zero. 1 value is
        ''' greater than zero.
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Sign(value As Double) As Integer
            If value > 0 Then
                Return 1
            ElseIf value < 0 Then
                Return -1
            Else
                Return 0
            End If
        End Function
        '
        ' Summary:
        '     Returns an integer that indicates the sign of a single-precision floating-point
        '     number.
        '
        ' Parameters:
        '   value:
        '     A signed number.
        '
        ' Returns:
        '     A number that indicates the sign of value, as shown in the following table.Return
        '     value Meaning -1 value is less than zero. 0 value is equal to zero. 1 value is
        '     greater than zero.
        '
        ' Exceptions:
        '   T:System.ArithmeticException:
        '     value is equal to System.Single.NaN.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Sign(value As Single) As Integer
            If value > 0 Then
                Return 1
            ElseIf value < 0 Then
                Return -1
            Else
                Return 0
            End If
        End Function
        '
        ' Summary:
        '     Returns an integer that indicates the sign of a 64-bit signed integer.
        '
        ' Parameters:
        '   value:
        '     A signed number.
        '
        ' Returns:
        '     A number that indicates the sign of value, as shown in the following table.Return
        '     value Meaning -1 value is less than zero. 0 value is equal to zero. 1 value is
        '     greater than zero.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Sign(value As Long) As Integer
            If value > 0 Then
                Return 1
            ElseIf value < 0 Then
                Return -1
            Else
                Return 0
            End If
        End Function
        '
        ' Summary:
        '     Returns an integer that indicates the sign of a 32-bit signed integer.
        '
        ' Parameters:
        '   value:
        '     A signed number.
        '
        ' Returns:
        '     A number that indicates the sign of value, as shown in the following table.Return
        '     value Meaning -1 value is less than zero. 0 value is equal to zero. 1 value is
        '     greater than zero.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Sign(value As Integer) As Integer
            If value > 0 Then
                Return 1
            ElseIf value < 0 Then
                Return -1
            Else
                Return 0
            End If
        End Function

        ''' <summary>
        ''' Returns the absolute value of a double-precision floating-point number.
        ''' </summary>
        ''' <param name="value">A number that is greater than or equal to System.Double.MinValue, but less than
        ''' or equal to System.Double.MaxValue.</param>
        ''' <returns>A double-precision floating-point number, x, such that 0 ≤ x ≤System.Double.MaxValue.</returns>
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Abs(value As Double) As Double
            If value < 0 Then
                Return -value
            Else
                Return value
            End If
        End Function
        '
        ' Summary:
        '     Returns an integer that indicates the sign of a 16-bit signed integer.
        '
        ' Parameters:
        '   value:
        '     A signed number.
        '
        ' Returns:
        '     A number that indicates the sign of value, as shown in the following table.Return
        '     value Meaning -1 value is less than zero. 0 value is equal to zero. 1 value is
        '     greater than zero.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Sign(value As Short) As Integer
            If value < 0 Then
                Return -value
            Else
                Return value
            End If
        End Function

        ' Returns:
        '     One of the values in the following table. (+Infinity denotes System.Double.PositiveInfinity,
        '     -Infinity denotes System.Double.NegativeInfinity, and NaN denotes System.Double.NaN.)anewBaseReturn
        '     valuea> 0(0 <newBase< 1) -or-(newBase> 1)lognewBase(a)a< 0(any value)NaN(any
        '     value)newBase< 0NaNa != 1newBase = 0NaNa != 1newBase = +InfinityNaNa = NaN(any
        '     value)NaN(any value)newBase = NaNNaN(any value)newBase = 1NaNa = 00 <newBase<
        '     1 +Infinitya = 0newBase> 1-Infinitya = +Infinity0 <newBase< 1-Infinitya = +InfinitynewBase>
        '     1+Infinitya = 1newBase = 00a = 1newBase = +Infinity0
        ''' <summary>
        ''' Returns the logarithm of a specified number in a specified base.
        ''' </summary>
        ''' <param name="a">The number whose logarithm is to be found.</param>
        ''' <param name="newBase">The base of the logarithm.</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Log(a#, newBase#) As Double
            Return stdNum.Log(a, newBase)
        End Function

        ''' <summary>
        ''' Returns the smaller of two decimal numbers.
        ''' </summary>
        ''' <param name="val1">The first of two decimal numbers to compare.</param>
        ''' <param name="val2">The second of two decimal numbers to compare.</param>
        ''' <returns>Parameter val1 or val2, whichever is smaller.</returns>
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Min(val1 As Decimal, val2 As Decimal) As Decimal
            If val1 < val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function
        '
        ' Summary:
        '     Returns the smaller of two double-precision floating-point numbers.
        '
        ' Parameters:
        '   val1:
        '     The first of two double-precision floating-point numbers to compare.
        '
        '   val2:
        '     The second of two double-precision floating-point numbers to compare.
        '
        ' Returns:
        '     Parameter val1 or val2, whichever is smaller. If val1, val2, or both val1 and
        '     val2 are equal to System.Double.NaN, System.Double.NaN is returned.
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Min(val1 As Double, val2 As Double) As Double
            If val1 < val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function

        '
        ' Summary:
        '     Returns the smaller of two single-precision floating-point numbers.
        '
        ' Parameters:
        '   val1:
        '     The first of two single-precision floating-point numbers to compare.
        '
        '   val2:
        '     The second of two single-precision floating-point numbers to compare.
        '
        ' Returns:
        '     Parameter val1 or val2, whichever is smaller. If val1, val2, or both val1 and
        '     val2 are equal to System.Single.NaN, System.Single.NaN is returned.
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Min(val1 As Single, val2 As Single) As Single
            If val1 < val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function
        '
        ' Summary:
        '     Returns the smaller of two 64-bit unsigned integers.
        '
        ' Parameters:
        '   val1:
        '     The first of two 64-bit unsigned integers to compare.
        '
        '   val2:
        '     The second of two 64-bit unsigned integers to compare.
        '
        ' Returns:
        '     Parameter val1 or val2, whichever is smaller.
        <CLSCompliant(False)> <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Min(val1 As ULong, val2 As ULong) As ULong
            If val1 < val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function
        '
        ' Summary:
        '     Returns the smaller of two 64-bit signed integers.
        '
        ' Parameters:
        '   val1:
        '     The first of two 64-bit signed integers to compare.
        '
        '   val2:
        '     The second of two 64-bit signed integers to compare.
        '
        ' Returns:
        '     Parameter val1 or val2, whichever is smaller.
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Min(val1 As Long, val2 As Long) As Long
            If val1 < val2 Then
                Return val1
            Else
                Return val2
            End If
        End Function
        '
        ' Summary:
        '     Returns an integer that indicates the sign of an 8-bit signed integer.
        '
        ' Parameters:
        '   value:
        '     A signed number.
        '
        ' Returns:
        '     A number that indicates the sign of value, as shown in the following table.Return
        '     value Meaning -1 value is less than zero. 0 value is equal to zero. 1 value is
        '     greater than zero.
        <CLSCompliant(False)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Sign(value As SByte) As Integer
            If value > 0 Then
                Return 1
            ElseIf value < 0 Then
                Return -1
            Else
                Return 0
            End If
        End Function

        ''' <summary>
        ''' Returns the absolute value of a single-precision floating-point number.
        ''' </summary>
        ''' <param name="value">A number that is greater than or equal to System.Single.MinValue, but less than
        ''' or equal to System.Single.MaxValue.</param>
        ''' <returns>
        ''' A single-precision floating-point number, x, such that 0 ≤ x ≤System.Single.MaxValue.
        ''' </returns>
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Abs(value As Single) As Single
            If value < 0 Then
                Return -value
            Else
                Return value
            End If
        End Function

        ''' <summary>
        ''' Returns the absolute value of a 64-bit signed integer.
        ''' </summary>
        ''' <param name="value">
        ''' A number that is greater than System.Int64.MinValue, but less than or equal to
        ''' System.Int64.MaxValue.</param>
        ''' <returns>A 64-bit signed integer, x, such that 0 ≤ x ≤System.Int64.MaxValue.</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Abs(value As Long) As Long
            If value < 0 Then
                Return -value
            Else
                Return value
            End If
        End Function

        ''' <summary>
        ''' Returns the absolute value of a 32-bit signed integer.
        ''' </summary>
        ''' <param name="value">
        ''' A number that is greater than System.Int32.MinValue, but less than or equal to
        ''' System.Int32.MaxValue.
        ''' </param>
        ''' <returns>A 32-bit signed integer, x, such that 0 ≤ x ≤System.Int32.MaxValue.</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Abs(value As Integer) As Integer
            If value < 0 Then
                Return -value
            Else
                Return value
            End If
        End Function
        '
        ' Summary:
        '     Returns the hyperbolic tangent of the specified angle.
        '
        ' Parameters:
        '   value:
        '     An angle, measured in radians.
        '
        ' Returns:
        '     The hyperbolic tangent of value. If value is equal to System.Double.NegativeInfinity,
        '     this method returns -1. If value is equal to System.Double.PositiveInfinity,
        '     this method returns 1. If value is equal to System.Double.NaN, this method returns
        '     System.Double.NaN.
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Tanh(value As Double) As Double
            Return stdNum.Tanh(value)
        End Function
        '
        ' Summary:
        '     Returns the hyperbolic sine of the specified angle.
        '
        ' Parameters:
        '   value:
        '     An angle, measured in radians.
        '
        ' Returns:
        '     The hyperbolic sine of value. If value is equal to System.Double.NegativeInfinity,
        '     System.Double.PositiveInfinity, or System.Double.NaN, this method returns a System.Double
        '     equal to value.
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Sinh(value As Double) As Double
            Return stdNum.Sinh(value)
        End Function
        '
        ' Summary:
        '     Returns the tangent of the specified angle.
        '
        ' Parameters:
        '   a:
        '     An angle, measured in radians.
        '
        ' Returns:
        '     The tangent of a. If a is equal to System.Double.NaN, System.Double.NegativeInfinity,
        '     or System.Double.PositiveInfinity, this method returns System.Double.NaN.
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Tan(a As Double) As Double
            Return stdNum.Tan(a)
        End Function
        '
        ' Summary:
        '     Returns the sine of the specified angle.
        '
        ' Parameters:
        '   a:
        '     An angle, measured in radians.
        '
        ' Returns:
        '     The sine of a. If a is equal to System.Double.NaN, System.Double.NegativeInfinity,
        '     or System.Double.PositiveInfinity, this method returns System.Double.NaN.
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Sin(a As Double) As Double
            Return stdNum.Sin(a)
        End Function
        '
        ' Summary:
        '     Returns the largest integer less than or equal to the specified double-precision
        '     floating-point number.
        '
        ' Parameters:
        '   d:
        '     A double-precision floating-point number.
        '
        ' Returns:
        '     The largest integer less than or equal to d. If d is equal to System.Double.NaN,
        '     System.Double.NegativeInfinity, or System.Double.PositiveInfinity, that value
        '     is returned.
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Floor(d As Double) As Double
            Return stdNum.Floor(d)
        End Function


        '
        ' Summary:
        '     Returns the largest integer less than or equal to the specified decimal number.
        '
        ' Parameters:
        '   d:
        '     A decimal number.
        '
        ' Returns:
        '     The largest integer less than or equal to d. Note that the method returns an
        '     integral value of type System.Math.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Floor(d As Decimal) As Decimal
            Return stdNum.Floor(d)
        End Function

        '
        ' Summary:
        '     Rounds a double-precision floating-point value to the nearest integral value.
        '
        ' Parameters:
        '   a:
        '     A double-precision floating-point number to be rounded.
        '
        ' Returns:
        '     The integer nearest a. If the fractional component of a is halfway between two
        '     integers, one of which is even and the other odd, then the even number is returned.
        '     Note that this method returns a System.Double instead of an integral type.
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Round(a As Double) As Double
            Return stdNum.Round(a)
        End Function

        ''' <summary>
        ''' Returns the hyperbolic cosine of the specified angle.
        ''' </summary>
        ''' <param name="value">An angle, measured in radians.</param>
        ''' <returns>
        ''' The hyperbolic cosine of value. If value is equal to System.Double.NegativeInfinity
        ''' or System.Double.PositiveInfinity, System.Double.PositiveInfinity is returned.
        ''' If value is equal to System.Double.NaN, System.Double.NaN is returned.
        ''' </returns>
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Cosh(value As Double) As Double
            Return stdNum.Cosh(value)
        End Function
        '
        ' Summary:
        '     Returns the smallest integral value that is greater than or equal to the specified
        '     double-precision floating-point number.
        '
        ' Parameters:
        '   a:
        '     A double-precision floating-point number.
        '
        ' Returns:
        '     The smallest integral value that is greater than or equal to a. If a is equal
        '     to System.Double.NaN, System.Double.NegativeInfinity, or System.Double.PositiveInfinity,
        '     that value is returned. Note that this method returns a System.Double instead
        '     of an integral type.
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Ceiling(a As Double) As Double
            Return stdNum.Ceiling(a)
        End Function
        '
        ' Summary:
        '     Returns the smallest integral value that is greater than or equal to the specified
        '     decimal number.
        '
        ' Parameters:
        '   d:
        '     A decimal number.
        '
        ' Returns:
        '     The smallest integral value that is greater than or equal to d. Note that this
        '     method returns a System.Decimal instead of an integral type.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Ceiling(d As Decimal) As Decimal
            Return stdNum.Ceiling(d)
        End Function
        '
        ' Summary:
        '     Returns the angle whose tangent is the quotient of two specified numbers.
        '
        ' Parameters:
        '   y:
        '     The y coordinate of a point.
        '
        '   x:
        '     The x coordinate of a point.
        '
        ' Returns:
        '     An angle, θ, measured in radians, such that -π≤θ≤π, and tan(θ) = y / x, where
        '     (x, y) is a point in the Cartesian plane. Observe the following: For (x, y) in
        '     quadrant 1, 0 < θ < π/2.For (x, y) in quadrant 2, π/2 < θ≤π.For (x, y) in quadrant
        '     3, -π < θ < -π/2.For (x, y) in quadrant 4, -π/2 < θ < 0.For points on the boundaries
        '     of the quadrants, the return value is the following:If y is 0 and x is not negative,
        '     θ = 0.If y is 0 and x is negative, θ = π.If y is positive and x is 0, θ = π/2.If
        '     y is negative and x is 0, θ = -π/2.If y is 0 and x is 0, θ = 0. If x or y is
        '     System.Double.NaN, or if x and y are either System.Double.PositiveInfinity or
        '     System.Double.NegativeInfinity, the method returns System.Double.NaN.
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Atan2(y As Double, x As Double) As Double
            Return stdNum.Atan2(y, x)
        End Function
        '
        ' Summary:
        '     Returns the angle whose tangent is the specified number.
        '
        ' Parameters:
        '   d:
        '     A number representing a tangent.
        '
        ' Returns:
        '     An angle, θ, measured in radians, such that -π/2 ≤θ≤π/2.-or- System.Double.NaN
        '     if d equals System.Double.NaN, -π/2 rounded to double precision (-1.5707963267949)
        '     if d equals System.Double.NegativeInfinity, or π/2 rounded to double precision
        '     (1.5707963267949) if d equals System.Double.PositiveInfinity.
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Atan(d As Double) As Double
            Return stdNum.Atan(d)
        End Function
        '
        ' Summary:
        '     Returns the angle whose sine is the specified number.
        '
        ' Parameters:
        '   d:
        '     A number representing a sine, where d must be greater than or equal to -1, but
        '     less than or equal to 1.
        '
        ' Returns:
        '     An angle, θ, measured in radians, such that -π/2 ≤θ≤π/2 -or- System.Double.NaN
        '     if d < -1 or d > 1 or d equals System.Double.NaN.
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Asin(d As Double) As Double
            Return stdNum.Asin(d)
        End Function
        '
        ' Summary:
        '     Returns the angle whose cosine is the specified number.
        '
        ' Parameters:
        '   d:
        '     A number representing a cosine, where d must be greater than or equal to -1,
        '     but less than or equal to 1.
        '
        ' Returns:
        '     An angle, θ, measured in radians, such that 0 ≤θ≤π-or- System.Double.NaN if d
        '     < -1 or d > 1 or d equals System.Double.NaN.
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Acos(d As Double) As Double
            Return stdNum.Acos(d)
        End Function

        ''' <summary>
        ''' Returns the cosine of the specified angle.
        ''' </summary>
        ''' <param name="d">An angle, measured in radians.</param>
        ''' <returns></returns>
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Cos(d As Double) As Double
            Return stdNum.Cos(d)
        End Function
        '
        ' Summary:
        '     Calculates the quotient of two 32-bit signed integers and also returns the remainder
        '     in an output parameter.
        '
        ' Parameters:
        '   a:
        '     The dividend.
        '
        '   b:
        '     The divisor.
        '
        '   result:
        '     The remainder.
        '
        ' Returns:
        '     The quotient of the specified numbers.
        '
        ' Exceptions:
        '   T:System.DivideByZeroException:
        '     b is zero.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function DivRem(a As Integer, b As Integer, ByRef result As Integer) As Integer
            Return stdNum.DivRem(a, b, result)
        End Function

        ''' <summary>
        ''' Rounds a double-precision floating-point value to a specified number of fractional
        ''' digits.
        ''' </summary>
        ''' <param name="value">A double-precision floating-point number to be rounded.</param>
        ''' <param name="digits">The number of fractional digits in the return value.</param>
        ''' <returns>The number nearest to value that contains a number of fractional digits equal
        ''' to digits.</returns>
        '''   
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Round(value As Double, digits As Integer) As Double
            Return stdNum.Round(value, digits)
        End Function
        '
        ' Summary:
        '     Rounds a double-precision floating-point value to a specified number of fractional
        '     digits. A parameter specifies how to round the value if it is midway between
        '     two numbers.
        '
        ' Parameters:
        '   value:
        '     A double-precision floating-point number to be rounded.
        '
        '   digits:
        '     The number of fractional digits in the return value.
        '
        '   mode:
        '     Specification for how to round value if it is midway between two other numbers.
        '
        ' Returns:
        '     The number nearest to value that has a number of fractional digits equal to digits.
        '     If value has fewer fractional digits than digits, value is returned unchanged.
        '
        ' Exceptions:
        '   T:System.ArgumentOutOfRangeException:
        '     digits is less than 0 or greater than 15.
        '
        '   T:System.ArgumentException:
        '     mode is not a valid value of System.MidpointRounding.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Round(value As Double, digits As Integer, mode As MidpointRounding) As Double
            Return stdNum.Round(value, digits, mode)
        End Function
        '
        ' Summary:
        '     Returns the absolute value of a 16-bit signed integer.
        '
        ' Parameters:
        '   value:
        '     A number that is greater than System.Int16.MinValue, but less than or equal to
        '     System.Int16.MaxValue.
        '
        ' Returns:
        '     A 16-bit signed integer, x, such that 0 ≤ x ≤System.Int16.MaxValue.
        '
        ' Exceptions:
        '   T:System.OverflowException:
        '     value equals System.Int16.MinValue.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Abs(value As Short) As Short
            If value < 0 Then
                Return -value
            Else
                Return value
            End If
        End Function
        '
        ' Summary:
        '     Returns the absolute value of an 8-bit signed integer.
        '
        ' Parameters:
        '   value:
        '     A number that is greater than System.SByte.MinValue, but less than or equal to
        '     System.SByte.MaxValue.
        '
        ' Returns:
        '     An 8-bit signed integer, x, such that 0 ≤ x ≤System.SByte.MaxValue.
        '
        ' Exceptions:
        '   T:System.OverflowException:
        '     value equals System.SByte.MinValue.
        <CLSCompliant(False)>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Abs(value As SByte) As SByte
            If value < 0 Then
                Return -value
            Else
                Return value
            End If
        End Function
        '
        ' Summary:
        '     Returns the remainder resulting from the division of a specified number by another
        '     specified number.
        '
        ' Parameters:
        '   x:
        '     A dividend.
        '
        '   y:
        '     A divisor.
        '
        ' Returns:
        '     A number equal to x - (y Q), where Q is the quotient of x / y rounded to the
        '     nearest integer (if x / y falls halfway between two integers, the even integer
        '     is returned).If x - (y Q) is zero, the value +0 is returned if x is positive,
        '     or -0 if x is negative.If y = 0, System.Double.NaN is returned.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IEEERemainder(x As Double, y As Double) As Double
            Return stdNum.IEEERemainder(x, y)
        End Function

        ''' <summary>
        ''' Returns a specified number raised to the specified power.
        ''' </summary>
        ''' <param name="x">A double-precision floating-point number to be raised to a power.</param>
        ''' <param name="y">A double-precision floating-point number that specifies a power.</param>
        ''' <returns>The number x raised to the power y.</returns>
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Pow(x As Double, y As Double) As Double
            Return x ^ y
        End Function

        ''' <summary>
        ''' Returns e raised to the specified power.
        ''' </summary>
        ''' <param name="d">A number specifying a power.</param>
        ''' <returns>The number e raised to the power d. If d equals System.Double.NaN or System.Double.PositiveInfinity,
        ''' that value is returned. If d equals System.Double.NegativeInfinity, 0 is returned.</returns>
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Exp(d As Double) As Double
            Return stdNum.Exp(d)
        End Function
        '
        ' Summary:
        '     Returns the base 10 logarithm of a specified number.
        '
        ' Parameters:
        '   d:
        '     A number whose logarithm is to be found.
        '
        ' Returns:
        '     One of the values in the following table. d parameter Return value Positive The
        '     base 10 log of d; that is, log 10d. Zero System.Double.NegativeInfinityNegative
        '     System.Double.NaNEqual to System.Double.NaNSystem.Double.NaNEqual to System.Double.PositiveInfinitySystem.Double.PositiveInfinity
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Log10(d As Double) As Double
            Return stdNum.Log10(d)
        End Function
        '
        ' Summary:
        '     Rounds a double-precision floating-point value to the nearest integer. A parameter
        '     specifies how to round the value if it is midway between two numbers.
        '
        ' Parameters:
        '   value:
        '     A double-precision floating-point number to be rounded.
        '
        '   mode:
        '     Specification for how to round value if it is midway between two other numbers.
        '
        ' Returns:
        '     The integer nearest value. If value is halfway between two integers, one of which
        '     is even and the other odd, then mode determines which of the two is returned.
        '
        ' Exceptions:
        '   T:System.ArgumentException:
        '     mode is not a valid value of System.MidpointRounding.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Round(value As Double, mode As MidpointRounding) As Double
            Return stdNum.Round(value, mode)
        End Function
        '
        ' Summary:
        '     Returns the natural (base e) logarithm of a specified number.
        '
        ' Parameters:
        '   d:
        '     The number whose logarithm is to be found.
        '
        ' Returns:
        '     One of the values in the following table. d parameterReturn value Positive The
        '     natural logarithm of d; that is, ln d, or log edZero System.Double.NegativeInfinityNegative
        '     System.Double.NaNEqual to System.Double.NaNSystem.Double.NaNEqual to System.Double.PositiveInfinitySystem.Double.PositiveInfinity
        <SecuritySafeCritical>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Log(d As Double) As Double
            Return stdNum.Log(d)
        End Function

        ''' <summary>
        ''' Calculates the integral part of a specified double-precision floating-point number.
        ''' </summary>
        ''' <param name="d">A number to truncate.</param>
        ''' <returns>The integral part of d; that is, the number that remains after any fractional
        ''' digits have been discarded, or one of the values listed in the following table.
        ''' |<paramref name="d"/>          |Return value                  |
        ''' |------------------------------|------------------------------|
        ''' |System.Double.NaN             |System.Double.NaN             |
        ''' |System.Double.NegativeInfinity|System.Double.NegativeInfinity|
        ''' |System.Double.PositiveInfinity|System.Double.PositiveInfinity|
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Truncate(d As Double) As Double
            Return stdNum.Truncate(d)
        End Function
        '
        ' Summary:
        '     Calculates the integral part of a specified decimal number.
        '
        ' Parameters:
        '   d:
        '     A number to truncate.
        '
        ' Returns:
        '     The integral part of d; that is, the number that remains after any fractional
        '     digits have been discarded.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Truncate(d As Decimal) As Decimal
            Return stdNum.Truncate(d)
        End Function
        '
        ' Summary:
        '     Rounds a decimal value to a specified number of fractional digits. A parameter
        '     specifies how to round the value if it is midway between two numbers.
        '
        ' Parameters:
        '   d:
        '     A decimal number to be rounded.
        '
        '   decimals:
        '     The number of decimal places in the return value.
        '
        '   mode:
        '     Specification for how to round d if it is midway between two other numbers.
        '
        ' Returns:
        '     The number nearest to d that contains a number of fractional digits equal to
        '     decimals. If d has fewer fractional digits than decimals, d is returned unchanged.
        '
        ' Exceptions:
        '   T:System.ArgumentOutOfRangeException:
        '     decimals is less than 0 or greater than 28.
        '
        '   T:System.ArgumentException:
        '     mode is not a valid value of System.MidpointRounding.
        '
        '   T:System.OverflowException:
        '     The result is outside the range of a System.Decimal.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Round(d As Decimal, decimals As Integer, mode As MidpointRounding) As Decimal
            Return stdNum.Round(d, decimals, mode)
        End Function
        '
        ' Summary:
        '     Rounds a decimal value to the nearest integer. A parameter specifies how to round
        '     the value if it is midway between two numbers.
        '
        ' Parameters:
        '   d:
        '     A decimal number to be rounded.
        '
        '   mode:
        '     Specification for how to round d if it is midway between two other numbers.
        '
        ' Returns:
        '     The integer nearest d. If d is halfway between two numbers, one of which is even
        '     and the other odd, then mode determines which of the two is returned.
        '
        ' Exceptions:
        '   T:System.ArgumentException:
        '     mode is not a valid value of System.MidpointRounding.
        '
        '   T:System.OverflowException:
        '     The result is outside the range of a System.Decimal.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Round(d As Decimal, mode As MidpointRounding) As Decimal
            Return stdNum.Round(d, mode)
        End Function
        '
        ' Summary:
        '     Rounds a decimal value to a specified number of fractional digits.
        '
        ' Parameters:
        '   d:
        '     A decimal number to be rounded.
        '
        '   decimals:
        '     The number of decimal places in the return value.
        '
        ' Returns:
        '     The number nearest to d that contains a number of fractional digits equal to
        '     decimals.
        '
        ' Exceptions:
        '   T:System.ArgumentOutOfRangeException:
        '     decimals is less than 0 or greater than 28.
        '
        '   T:System.OverflowException:
        '     The result is outside the range of a System.Decimal.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Round(d As Decimal, decimals As Integer) As Decimal
            Return stdNum.Round(d, decimals)
        End Function
        '
        ' Summary:
        '     Rounds a decimal value to the nearest integral value.
        '
        ' Parameters:
        '   d:
        '     A decimal number to be rounded.
        '
        ' Returns:
        '     The integer nearest parameter d. If the fractional component of d is halfway
        '     between two integers, one of which is even and the other odd, the even number
        '     is returned. Note that this method returns a System.Decimal instead of an integral
        '     type.
        '
        ' Exceptions:
        '   T:System.OverflowException:
        '     The result is outside the range of a System.Decimal.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Round(d As Decimal) As Decimal
            Return stdNum.Round(d)
        End Function

        ''' <summary>
        ''' Returns the square root of a specified number.
        ''' </summary>
        ''' <param name="d">The number whose square root is to be found.</param>
        ''' <returns>One of the values in the following table. d parameter Return value Zero or positive
        ''' The positive square root of d. Negative System.Double.NaNEquals System.Double.NaNSystem.Double.NaNEquals
        ''' System.Double.PositiveInfinitySystem.Double.PositiveInfinity</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)> <SecuritySafeCritical>
        Public Function Sqrt(d As Double) As Double
            Return stdNum.Sqrt(d)
        End Function
        '
        ' Summary:
        '     Calculates the quotient of two 64-bit signed integers and also returns the remainder
        '     in an output parameter.
        '
        ' Parameters:
        '   a:
        '     The dividend.
        '
        '   b:
        '     The divisor.
        '
        '   result:
        '     The remainder.
        '
        ' Returns:
        '     The quotient of the specified numbers.
        '
        ' Exceptions:
        '   T:System.DivideByZeroException:
        '     b is zero.
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function DivRem(a As Long, b As Long, ByRef result As Long) As Long
            Return stdNum.DivRem(a, b, result)
        End Function
#End Region

        ''' <summary>
        ''' ``Math.Log(x, newBase:=2)``
        ''' </summary>
        ''' <param name="x#"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function Log2(x#) As Double
            Return stdNum.Log(x, newBase:=2)
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

            Do While (from = from.Value + by) <= [to]
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
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function LogN(x As Double, N As Double) As Double
            Return stdNum.Log(x) / stdNum.Log(N)
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
            Return stdNum.Max(a, stdNum.Max(b, c))
        End Function

        ''' <summary>
        '''  sqrt(a^2 + b^2) without under/overflow.
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>

        Public Function Hypot(a As Double, b As Double) As Double
            Dim r As Double

            If stdNum.Abs(a) > stdNum.Abs(b) Then
                r = b / a
                r = stdNum.Abs(a) * stdNum.Sqrt(1 + r * r)
            ElseIf b <> 0 Then
                r = a / b
                r = stdNum.Abs(b) * stdNum.Sqrt(1 + r * r)
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
            If bc Is Nothing Then
                Return 0
            Else
                Return bc _
                    .Select(Function(b) If(True = b, 1.0R, 0R)) _
                    .Sum
            End If
        End Function

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
        <Extension> Public Function SD(values As IEnumerable(Of Double)) As Double
            Dim data#() = values.ToArray
            Dim avg# = data.Average
            Dim sumValue# = Aggregate n As Double In data Into Sum((n - avg) ^ 2)
            Return stdNum.Sqrt(sumValue / data.Length)
        End Function

        ''' <summary>
        ''' Standard Deviation
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <Extension> Public Function SD(values As IEnumerable(Of Integer)) As Double
            Return values.Select(Function(x) CDbl(x)).SD
        End Function

        ''' <summary>
        ''' Standard Deviation
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <Extension> Public Function SD(values As IEnumerable(Of Long)) As Double
            Return values.Select(Function(x) CDbl(x)).SD
        End Function

        ''' <summary>
        ''' Standard Deviation
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <Extension> Public Function SD(values As IEnumerable(Of Single)) As Double
            Return values.Select(Function(x) CDbl(x)).SD
        End Function

        ''' <summary>
        ''' 多位坐标的欧几里得距离，与坐标点0进行比较
        ''' </summary>
        ''' <param name="vector"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function EuclideanDistance(vector As IEnumerable(Of Double)) As Double
            ' 由于是和令进行比较，减零仍然为原来的数，所以这里直接使用n^2了
            Return stdNum.Sqrt((From n In vector Select n ^ 2).Sum)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function EuclideanDistance(Vector As IEnumerable(Of Integer)) As Double
            Return stdNum.Sqrt((From n In Vector Select n ^ 2).Sum)
        End Function

        <Extension>
        Public Function EuclideanDistance(a As IEnumerable(Of Integer), b As IEnumerable(Of Integer)) As Double
            If a.Count <> b.Count Then
                Return -1
            Else
                Return stdNum.Sqrt((From i As Integer In a.Sequence Select (a(i) - b(i)) ^ 2).Sum)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function EuclideanDistance(a As IEnumerable(Of Double), b As IEnumerable(Of Double)) As Double
            Return EuclideanDistance(a.ToArray, b.ToArray)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="a">Point A</param>
        ''' <param name="b">Point B</param>
        ''' <returns></returns>
        <Extension>
        Public Function EuclideanDistance(a As Byte(), b As Byte()) As Double
            If a.Length <> b.Length Then
                Return -1.0R
            Else
                Return stdNum.Sqrt((From i As Integer In a.Sequence Select (CInt(a(i)) - CInt(b(i))) ^ 2).Sum)
            End If
        End Function

        ''' <summary>
        ''' 计算两个向量之间的欧氏距离，请注意，这两个向量的长度必须要相等
        ''' </summary>
        ''' <param name="a">Point A</param>
        ''' <param name="b">Point B</param>
        ''' <returns></returns>
        <Extension>
        Public Function EuclideanDistance(a As Double(), b As Double()) As Double
            If a.Length <> b.Length Then
                Return -1.0R
            Else
                Return stdNum.Sqrt((From i As Integer In a.Sequence Select (a(i) - b(i)) ^ 2).Sum)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Distance(pt As (X#, Y#), x#, y#) As Double
            Return {pt.X, pt.Y}.EuclideanDistance({x, y})
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("RangesAt")>
        <Extension> Public Function RangesAt(n As Double, LowerBound As Double, UpBound As Double) As Boolean
            Return n <= UpBound AndAlso n > LowerBound
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("RangesAt")>
        <Extension> Public Function RangesAt(n As Integer, LowerBound As Double, UpBound As Double) As Boolean
            Return n <= UpBound AndAlso n > LowerBound
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("RangesAt")>
        <Extension> Public Function RangesAt(n As Long, LowerBound As Double, UpBound As Double) As Boolean
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
                Return stdNum.Sqrt(.Sum / .Length)
            End With
        End Function

        Public Function RMSE(a#(), b#()) As Double
            Dim sum#
            Dim n% = a.Length

            For i As Integer = 0 To n - 1
                sum += (a(i) - b(i)) ^ 2
            Next

            Return stdNum.Sqrt(sum)
        End Function

        ''' <summary>
        ''' ``相对标准偏差（RSD）= 标准偏差（SD）/ 计算结果的算术平均值（X）* 100%``
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

        ''' <summary>
        ''' Returns the PDF value at x for the specified Poisson distribution.
        ''' </summary>
        ''' 
        <ExportAPI("Poisson.PDF")>
        Public Function PoissonPDF(x As Integer, lambda As Double) As Double
            Dim result As Double = stdNum.Exp(-lambda)
            Dim k As Integer = x

            While k >= 1
                result *= lambda / k
                k -= 1
            End While

            Return result
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function FormatNumeric(v As IEnumerable(Of Double), Optional digitals% = 2) As String()
            Return v.Select(Function(x) x.ToString("F" & digitals)).ToArray
        End Function
    End Module
End Namespace
