#Region "Microsoft.VisualBasic::216ba640b8f32fbb45c0022a8705be67, Data_science\Mathematica\Math\Math\Numerics\Half.vb"

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

    '   Total Lines: 1058
    '    Code Lines: 387 (36.58%)
    ' Comment Lines: 535 (50.57%)
    '    - Xml Docs: 96.64%
    ' 
    '   Blank Lines: 136 (12.85%)
    '     File Size: 51.14 KB


    '     Structure Half
    ' 
    '         Constructor: (+7 Overloads) Sub New
    '         Function: Abs, Add, (+2 Overloads) CompareTo, Divide, (+2 Overloads) Equals
    '                   GetBits, GetBytes, GetHashCode, GetTypeCode, IConvertible_GetTypeCode
    '                   IConvertible_ToBoolean, IConvertible_ToByte, IConvertible_ToChar, IConvertible_ToDateTime, IConvertible_ToDecimal
    '                   IConvertible_ToDouble, IConvertible_ToInt16, IConvertible_ToInt32, IConvertible_ToInt64, IConvertible_ToSByte
    '                   IConvertible_ToSingle, IConvertible_ToString, IConvertible_ToType, IConvertible_ToUInt16, IConvertible_ToUInt32
    '                   IConvertible_ToUInt64, IsInfinity, IsNaN, IsNegativeInfinity, IsPositiveInfinity
    '                   Max, Min, Multiply, Negate, (+4 Overloads) Parse
    '                   Sign, Subtract, (+2 Overloads) ToHalf, (+4 Overloads) ToString, (+2 Overloads) TryParse
    '         Operators: (+3 Overloads) -, (+3 Overloads) *, (+2 Overloads) /, (+2 Overloads) ^, (+4 Overloads) +
    '                    <, <=, <>, =, >
    '                    >=
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports stdNum = System.Math

Namespace Numerics

    ''' <summary>
    ''' Represents a half-precision floating point number. 
    ''' </summary>
    ''' <remarks>
    ''' Note:
    '''     Half is not fast enought and precision is also very bad, 
    '''     so is should not be used for matemathical computation (use Single instead).
    '''     The main advantage of Half type is lower memory cost: two bytes per number. 
    '''     Half is typically used in graphical applications.
    '''     
    ''' Note: 
    '''     All functions, where is used conversion half->float/float->half, 
    '''     are approx. ten times slower than float->double/double->float, i.e. ~3ns on 2GHz CPU.
    '''
    ''' References:
    '''     - Fast Half Float Conversions, Jeroen van der Zijp, link: http://www.fox-toolkit.org/ftp/fasthalffloatconversion.pdf
    '''     - IEEE 754 revision, link: http://grouper.ieee.org/groups/754/
    ''' </remarks>
    <Serializable>
    Public Structure Half
        Implements IComparable
        Implements IFormattable
        Implements IConvertible
        Implements IComparable(Of Half)
        Implements IEquatable(Of Half)

        ''' <summary>
        ''' Internal representation of the half-precision floating-point number.
        ''' </summary>
        Friend Value As UShort

#Region "Constants"

        ''' <summary>
        ''' Represents the smallest positive System.Half value greater than zero. This field is constant.
        ''' </summary>
        Public Shared ReadOnly Epsilon As Half = Half.ToHalf(&H1)

        ''' <summary>
        ''' Represents the largest possible value of System.Half. This field is constant.
        ''' </summary>
        Public Shared ReadOnly MaxValue As Half = Half.ToHalf(&H7BFF)

        ''' <summary>
        ''' Represents the smallest possible value of System.Half. This field is constant.
        ''' </summary>
        Public Shared ReadOnly MinValue As Half = Half.ToHalf(&HFBFF)

        ''' <summary>
        ''' Represents not a number (NaN). This field is constant.
        ''' </summary>
        Public Shared ReadOnly NaN As Half = Half.ToHalf(&HFE00)

        ''' <summary>
        ''' Represents negative infinity. This field is constant.
        ''' </summary>
        Public Shared ReadOnly NegativeInfinity As Half = Half.ToHalf(&HFC00)

        ''' <summary>
        ''' Represents positive infinity. This field is constant.
        ''' </summary>
        Public Shared ReadOnly PositiveInfinity As Half = Half.ToHalf(&H7C00)

#End Region

#Region "Constructors"

        ''' <summary>
        ''' Initializes a new instance of System.Half to the value of the specified single-precision floating-point number.
        ''' </summary>
        ''' <param name="value">The value to represent as a System.Half.</param>
        Public Sub New(value As Single)
            Me.Value = HalfHelper.SingleToHalf(value).Value
        End Sub

        ''' <summary>
        ''' Initializes a new instance of System.Half to the value of the specified 32-bit signed integer.
        ''' </summary>
        ''' <param name="value">The value to represent as a System.Half.</param>
        Public Sub New(value As Integer)
            Me.New(CSng(value))
        End Sub

        ''' <summary>
        ''' Initializes a new instance of System.Half to the value of the specified 64-bit signed integer.
        ''' </summary>
        ''' <param name="value">The value to represent as a System.Half.</param>
        Public Sub New(value As Long)
            Me.New(CSng(value))
        End Sub

        ''' <summary>
        ''' Initializes a new instance of System.Half to the value of the specified double-precision floating-point number.
        ''' </summary>
        ''' <param name="value">The value to represent as a System.Half.</param>
        Public Sub New(value As Double)
            Me.New(CSng(value))
        End Sub

        ''' <summary>
        ''' Initializes a new instance of System.Half to the value of the specified decimal number.
        ''' </summary>
        ''' <param name="value">The value to represent as a System.Half.</param>
        Public Sub New(value As Decimal)
            Me.New(CSng(value))
        End Sub

        ''' <summary>
        ''' Initializes a new instance of System.Half to the value of the specified 32-bit unsigned integer.
        ''' </summary>
        ''' <param name="value">The value to represent as a System.Half.</param>
        Public Sub New(value As UInteger)
            Me.New(CSng(value))
        End Sub

        ''' <summary>
        ''' Initializes a new instance of System.Half to the value of the specified 64-bit unsigned integer.
        ''' </summary>
        ''' <param name="value">The value to represent as a System.Half.</param>
        Public Sub New(value As ULong)
            Me.New(CSng(value))
        End Sub

#End Region

#Region "Numeric operators"

        ''' <summary>
        ''' Returns the result of multiplying the specified System.Half value by negative one.
        ''' </summary>
        ''' <param name="half">A System.Half.</param>
        ''' <returns>A System.Half with the value of half, but the opposite sign. -or- Zero, if half is zero.</returns>
        Public Shared Function Negate(half As Half) As Half
            Return -half
        End Function

        ''' <summary>
        ''' Adds two specified System.Half values.
        ''' </summary>
        ''' <param name="half1">A System.Half.</param>
        ''' <param name="half2">A System.Half.</param>
        ''' <returns>A System.Half value that is the sum of half1 and half2.</returns>
        Public Shared Function Add(half1 As Half, half2 As Half) As Half
            Return half1 + half2
        End Function

        ''' <summary>
        ''' Subtracts one specified System.Half value from another.
        ''' </summary>
        ''' <param name="half1">A System.Half (the minuend).</param>
        ''' <param name="half2">A System.Half (the subtrahend).</param>
        ''' <returns>The System.Half result of subtracting half2 from half1.</returns>
        Public Shared Function Subtract(half1 As Half, half2 As Half) As Half
            Return half1 - half2
        End Function

        ''' <summary>
        ''' Multiplies two specified System.Half values.
        ''' </summary>
        ''' <param name="half1">A System.Half (the multiplicand).</param>
        ''' <param name="half2">A System.Half (the multiplier).</param>
        ''' <returns>A System.Half that is the result of multiplying half1 and half2.</returns>
        Public Shared Function Multiply(half1 As Half, half2 As Half) As Half
            Return half1 * half2
        End Function

        ''' <summary>
        ''' Divides two specified System.Half values.
        ''' </summary>
        ''' <param name="half1">A System.Half (the dividend).</param>
        ''' <param name="half2">A System.Half (the divisor).</param>
        ''' <returns>The System.Half that is the result of dividing half1 by half2.</returns>
        ''' <exception cref="System.DivideByZeroException">half2 is zero.</exception>
        Public Shared Function Divide(half1 As Half, half2 As Half) As Half
            Return half1 / half2
        End Function


        ''' <summary>
        ''' Returns the value of the System.Half operand (the sign of the operand is unchanged).
        ''' </summary>
        ''' <param name="half">The System.Half operand.</param>
        ''' <returns>The value of the operand, half.</returns>
        Public Shared Operator +(half As Half) As Half
            Return half
        End Operator

        ''' <summary>
        ''' Negates the value of the specified System.Half operand.
        ''' </summary>
        ''' <param name="half">The System.Half operand.</param>
        ''' <returns>The result of half multiplied by negative one (-1).</returns>
        Public Shared Operator -(half As Half) As Half
            Return HalfHelper.Negate(half)
        End Operator

        '      ''' <summary>
        '      ''' Increments the System.Half operand by 1.
        '      ''' </summary>
        '      ''' <param name="half">The System.Half operand.</param>
        '      ''' <returns>The value of half incremented by 1.</returns>
        '      Public Shared Operator ++(half As Half) As Half
        '          Return CType(half + 1.0F, Half)
        '      End Operator

        '      ''' <summary>
        '      ''' Decrements the System.Half operand by one.
        '      ''' </summary>
        '      ''' <param name="half">The System.Half operand.</param>
        '      ''' <returns>The value of half decremented by 1.</returns>
        '      Public Shared Operator (half As Half) As Half
        '	Return CType(half - 1F, Half)
        'End Operator

        ''' <summary>
        ''' Adds two specified System.Half values.
        ''' </summary>
        ''' <param name="half1">A System.Half.</param>
        ''' <param name="half2">A System.Half.</param>
        ''' <returns>The System.Half result of adding half1 and half2.</returns>
        Public Shared Operator +(half1 As Half, half2 As Half) As Half
            Return CType(half1 + CSng(half2), Half)
        End Operator

        Public Shared Operator +(half As Half, value As Single) As Single
            Return CSng(half) + value
        End Operator

        Public Shared Operator +(half As Half, value As Double) As Double
            Return CSng(half) + value
        End Operator

        ''' <summary>
        ''' Subtracts two specified System.Half values.
        ''' </summary>
        ''' <param name="half1">A System.Half.</param>
        ''' <param name="half2">A System.Half.</param>
        ''' <returns>The System.Half result of subtracting half1 and half2.</returns>        
        Public Shared Operator -(half1 As Half, half2 As Half) As Half
            Return CType(half1 - CSng(half2), Half)
        End Operator

        Public Shared Operator -(half As Half, value As Single) As Single
            Return CSng(half) - value
        End Operator

        Public Shared Operator ^(half As Half, power As Single) As Single
            Return CSng(half) ^ power
        End Operator

        Public Shared Operator ^(half As Half, power As Double) As Single
            Return CSng(half) ^ power
        End Operator

        ''' <summary>
        ''' Multiplies two specified System.Half values.
        ''' </summary>
        ''' <param name="half1">A System.Half.</param>
        ''' <param name="half2">A System.Half.</param>
        ''' <returns>The System.Half result of multiplying half1 by half2.</returns>
        Public Shared Operator *(half1 As Half, half2 As Half) As Half
            Return CType(half1 * CSng(half2), Half)
        End Operator

        Public Shared Operator *(half As Half, value As Single) As Single
            Return CSng(half) * value
        End Operator

        Public Shared Operator *(half As Half, value As Double) As Double
            Return CSng(half) * value
        End Operator

        ''' <summary>
        ''' Divides two specified System.Half values.
        ''' </summary>
        ''' <param name="half1">A System.Half (the dividend).</param>
        ''' <param name="half2">A System.Half (the divisor).</param>
        ''' <returns>The System.Half result of half1 by half2.</returns>
        Public Shared Operator /(half1 As Half, half2 As Half) As Half
            Return CType(half1 / CSng(half2), Half)
        End Operator

        Public Shared Operator /(half As Half, value As Single) As Single
            Return CSng(half) / value
        End Operator

        ''' <summary>
        ''' Returns a value indicating whether two instances of System.Half are equal.
        ''' </summary>
        ''' <param name="half1">A System.Half.</param>
        ''' <param name="half2">A System.Half.</param>
        ''' <returns>true if half1 and half2 are equal; otherwise, false.</returns>
        Public Shared Operator =(half1 As Half, half2 As Half) As Boolean
            Return Not IsNaN(half1) AndAlso half1.Value = half2.Value
        End Operator

        ''' <summary>
        ''' Returns a value indicating whether two instances of System.Half are not equal.
        ''' </summary>
        ''' <param name="half1">A System.Half.</param>
        ''' <param name="half2">A System.Half.</param>
        ''' <returns>true if half1 and half2 are not equal; otherwise, false.</returns>
        Public Shared Operator <>(half1 As Half, half2 As Half) As Boolean
            Return half1.Value <> half2.Value
        End Operator

        ''' <summary>
        ''' Returns a value indicating whether a specified System.Half is less than another specified System.Half.
        ''' </summary>
        ''' <param name="half1">A System.Half.</param>
        ''' <param name="half2">A System.Half.</param>
        ''' <returns>true if half1 is less than half1; otherwise, false.</returns>
        Public Shared Operator <(half1 As Half, half2 As Half) As Boolean
            Return half1 < CSng(half2)
        End Operator

        ''' <summary>
        ''' Returns a value indicating whether a specified System.Half is greater than another specified System.Half.
        ''' </summary>
        ''' <param name="half1">A System.Half.</param>
        ''' <param name="half2">A System.Half.</param>
        ''' <returns>true if half1 is greater than half2; otherwise, false.</returns>
        Public Shared Operator >(half1 As Half, half2 As Half) As Boolean
            Return half1 > CSng(half2)
        End Operator

        ''' <summary>
        ''' Returns a value indicating whether a specified System.Half is less than or equal to another specified System.Half.
        ''' </summary>
        ''' <param name="half1">A System.Half.</param>
        ''' <param name="half2">A System.Half.</param>
        ''' <returns>true if half1 is less than or equal to half2; otherwise, false.</returns>
        Public Shared Operator <=(half1 As Half, half2 As Half) As Boolean
            Return half1 = half2 OrElse half1 < half2
        End Operator

        ''' <summary>
        ''' Returns a value indicating whether a specified System.Half is greater than or equal to another specified System.Half.
        ''' </summary>
        ''' <param name="half1">A System.Half.</param>
        ''' <param name="half2">A System.Half.</param>
        ''' <returns>true if half1 is greater than or equal to half2; otherwise, false.</returns>
        Public Shared Operator >=(half1 As Half, half2 As Half) As Boolean
            Return half1 = half2 OrElse half1 > half2
        End Operator

#End Region

#Region "Type casting operators"

        ''' <summary>
        ''' Converts an 8-bit unsigned integer to a System.Half.
        ''' </summary>
        ''' <param name="value">An 8-bit unsigned integer.</param>
        ''' <returns>A System.Half that represents the converted 8-bit unsigned integer.</returns>
        Public Shared Widening Operator CType(value As Byte) As Half
            Return New Half(CSng(value))
        End Operator

        ''' <summary>
        ''' Converts a 16-bit signed integer to a System.Half.
        ''' </summary>
        ''' <param name="value">A 16-bit signed integer.</param>
        ''' <returns>A System.Half that represents the converted 16-bit signed integer.</returns>
        Public Shared Widening Operator CType(value As Short) As Half
            Return New Half(CSng(value))
        End Operator

        ''' <summary>
        ''' Converts a Unicode character to a System.Half.
        ''' </summary>
        ''' <param name="value">A Unicode character.</param>
        ''' <returns>A System.Half that represents the converted Unicode character.</returns>
        Public Shared Widening Operator CType(value As Char) As Half
            Return New Half(CSng(AscW(value)))
        End Operator

        ''' <summary>
        ''' Converts a 32-bit signed integer to a System.Half.
        ''' </summary>
        ''' <param name="value">A 32-bit signed integer.</param>
        ''' <returns>A System.Half that represents the converted 32-bit signed integer.</returns>
        Public Shared Widening Operator CType(value As Integer) As Half
            Return New Half(CSng(value))
        End Operator

        ''' <summary>
        ''' Converts a 64-bit signed integer to a System.Half.
        ''' </summary>
        ''' <param name="value">A 64-bit signed integer.</param>
        ''' <returns>A System.Half that represents the converted 64-bit signed integer.</returns>
        Public Shared Widening Operator CType(value As Long) As Half
            Return New Half(CSng(value))
        End Operator

        ''' <summary>
        ''' Converts a single-precision floating-point number to a System.Half.
        ''' </summary>
        ''' <param name="value">A single-precision floating-point number.</param>
        ''' <returns>A System.Half that represents the converted single-precision floating point number.</returns>
        Public Shared Narrowing Operator CType(value As Single) As Half
            Return New Half(value)
        End Operator

        ''' <summary>
        ''' Converts a double-precision floating-point number to a System.Half.
        ''' </summary>
        ''' <param name="value">A double-precision floating-point number.</param>
        ''' <returns>A System.Half that represents the converted double-precision floating point number.</returns>
        Public Shared Narrowing Operator CType(value As Double) As Half
            Return New Half(CSng(value))
        End Operator

        ''' <summary>
        ''' Converts a decimal number to a System.Half.
        ''' </summary>
        ''' <param name="value">decimal number</param>
        ''' <returns>A System.Half that represents the converted decimal number.</returns>
        Public Shared Narrowing Operator CType(value As Decimal) As Half
            Return New Half(CSng(value))
        End Operator

        ''' <summary>
        ''' Converts a System.Half to an 8-bit unsigned integer.
        ''' </summary>
        ''' <param name="value">A System.Half to convert.</param>
        ''' <returns>An 8-bit unsigned integer that represents the converted System.Half.</returns>
        Public Shared Narrowing Operator CType(value As Half) As Byte
            Return CByte(stdNum.Truncate(CSng(value)))
        End Operator

        ''' <summary>
        ''' Converts a System.Half to a Unicode character.
        ''' </summary>
        ''' <param name="value">A System.Half to convert.</param>
        ''' <returns>A Unicode character that represents the converted System.Half.</returns>
        Public Shared Narrowing Operator CType(value As Half) As Char
            Return Chr(CSng(value))
        End Operator

        ''' <summary>
        ''' Converts a System.Half to a 16-bit signed integer.
        ''' </summary>
        ''' <param name="value">A System.Half to convert.</param>
        ''' <returns>A 16-bit signed integer that represents the converted System.Half.</returns>
        Public Shared Narrowing Operator CType(value As Half) As Short
            Return CShort(stdNum.Truncate(CSng(value)))
        End Operator

        ''' <summary>
        ''' Converts a System.Half to a 32-bit signed integer.
        ''' </summary>
        ''' <param name="value">A System.Half to convert.</param>
        ''' <returns>A 32-bit signed integer that represents the converted System.Half.</returns>
        Public Shared Narrowing Operator CType(value As Half) As Integer
            Return CInt(stdNum.Truncate(CSng(value)))
        End Operator

        ''' <summary>
        ''' Converts a System.Half to a 64-bit signed integer.
        ''' </summary>
        ''' <param name="value">A System.Half to convert.</param>
        ''' <returns>A 64-bit signed integer that represents the converted System.Half.</returns>
        Public Shared Narrowing Operator CType(value As Half) As Long
            Return CLng(stdNum.Truncate(CSng(value)))
        End Operator

        ''' <summary>
        ''' Converts a System.Half to a single-precision floating-point number.
        ''' </summary>
        ''' <param name="value">A System.Half to convert.</param>
        ''' <returns>A single-precision floating-point number that represents the converted System.Half.</returns>
        Public Shared Widening Operator CType(value As Half) As Single
            Return HalfHelper.HalfToSingle(value)
        End Operator

        ''' <summary>
        ''' Converts a System.Half to a double-precision floating-point number.
        ''' </summary>
        ''' <param name="value">A System.Half to convert.</param>
        ''' <returns>A double-precision floating-point number that represents the converted System.Half.</returns>
        Public Shared Widening Operator CType(value As Half) As Double
            Return CSng(value)
        End Operator

        ''' <summary>
        ''' Converts a System.Half to a decimal number.
        ''' </summary>
        ''' <param name="value">A System.Half to convert.</param>
        ''' <returns>A decimal number that represents the converted System.Half.</returns>
        Public Shared Narrowing Operator CType(value As Half) As Decimal
            Return CDec(CSng(value))
        End Operator

        ''' <summary>
        ''' Converts an 8-bit signed integer to a System.Half.
        ''' </summary>
        ''' <param name="value">An 8-bit signed integer.</param>
        ''' <returns>A System.Half that represents the converted 8-bit signed integer.</returns>
        Public Shared Widening Operator CType(value As SByte) As Half
            Return New Half(CSng(value))
        End Operator

        ''' <summary>
        ''' Converts a 16-bit unsigned integer to a System.Half.
        ''' </summary>
        ''' <param name="value">A 16-bit unsigned integer.</param>
        ''' <returns>A System.Half that represents the converted 16-bit unsigned integer.</returns>
        Public Shared Widening Operator CType(value As UShort) As Half
            Return New Half(CSng(value))
        End Operator

        ''' <summary>
        ''' Converts a 32-bit unsigned integer to a System.Half.
        ''' </summary>
        ''' <param name="value">A 32-bit unsigned integer.</param>
        ''' <returns>A System.Half that represents the converted 32-bit unsigned integer.</returns>
        Public Shared Widening Operator CType(value As UInteger) As Half
            Return New Half(CSng(value))
        End Operator

        ''' <summary>
        ''' Converts a 64-bit unsigned integer to a System.Half.
        ''' </summary>
        ''' <param name="value">A 64-bit unsigned integer.</param>
        ''' <returns>A System.Half that represents the converted 64-bit unsigned integer.</returns>
        Public Shared Widening Operator CType(value As ULong) As Half
            Return New Half(CSng(value))
        End Operator

        ''' <summary>
        ''' Converts a System.Half to an 8-bit signed integer.
        ''' </summary>
        ''' <param name="value">A System.Half to convert.</param>
        ''' <returns>An 8-bit signed integer that represents the converted System.Half.</returns>
        Public Shared Narrowing Operator CType(value As Half) As SByte
            Return CSByte(stdNum.Truncate(CSng(value)))
        End Operator

        ''' <summary>
        ''' Converts a System.Half to a 16-bit unsigned integer.
        ''' </summary>
        ''' <param name="value">A System.Half to convert.</param>
        ''' <returns>A 16-bit unsigned integer that represents the converted System.Half.</returns>
        Public Shared Narrowing Operator CType(value As Half) As UShort
            Return CUShort(stdNum.Truncate(CSng(value)))
        End Operator

        ''' <summary>
        ''' Converts a System.Half to a 32-bit unsigned integer.
        ''' </summary>
        ''' <param name="value">A System.Half to convert.</param>
        ''' <returns>A 32-bit unsigned integer that represents the converted System.Half.</returns>
        Public Shared Narrowing Operator CType(value As Half) As UInteger
            Return CUInt(stdNum.Truncate(CSng(value)))
        End Operator

        ''' <summary>
        ''' Converts a System.Half to a 64-bit unsigned integer.
        ''' </summary>
        ''' <param name="value">A System.Half to convert.</param>
        ''' <returns>A 64-bit unsigned integer that represents the converted System.Half.</returns>
        Public Shared Narrowing Operator CType(value As Half) As ULong
            Return CULng(stdNum.Truncate(CSng(value)))
        End Operator

#End Region

        ''' <summary>
        ''' Compares this instance to a specified System.Half object.
        ''' </summary>
        ''' <param name="other">A System.Half object.</param>
        ''' <returns>
        ''' A signed number indicating the relative values of this instance and value.
        ''' Return Value Meaning Less than zero This instance is less than value. Zero
        ''' This instance is equal to value. Greater than zero This instance is greater than value.
        ''' </returns>
        Public Function CompareTo(other As Half) As Integer Implements IComparable(Of Half).CompareTo
            Dim result = 0
            If Me < other Then
                result = -1
            ElseIf Me > other Then
                result = 1
            ElseIf Me <> other Then
                If Not IsNaN(Me) Then
                    result = 1
                ElseIf Not IsNaN(other) Then
                    result = -1
                End If
            End If

            Return result
        End Function

        ''' <summary>
        ''' Compares this instance to a specified System.Object.
        ''' </summary>
        ''' <param name="obj">An System.Object or null.</param>
        ''' <returns>
        ''' A signed number indicating the relative values of this instance and value.
        ''' Return Value Meaning Less than zero This instance is less than value. Zero
        ''' This instance is equal to value. Greater than zero This instance is greater
        ''' than value. -or- value is null.
        ''' </returns>
        ''' <exception cref="System.ArgumentException">value is not a System.Half</exception>
        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            If obj Is Nothing Then
                Return 1
            ElseIf Not obj.GetType Is GetType(Half) Then
                Throw New ArgumentException("Object must be of type Half.")
            Else
                Return CompareTo(CType(obj, Half))
            End If
        End Function

        ''' <summary>
        ''' Returns a value indicating whether this instance and a specified System.Half object represent the same value.
        ''' </summary>
        ''' <param name="other">A System.Half object to compare to this instance.</param>
        ''' <returns>true if value is equal to this instance; otherwise, false.</returns>
        Public Overloads Function Equals(other As Half) As Boolean Implements IEquatable(Of Half).Equals
            Return other = Me OrElse IsNaN(other) AndAlso IsNaN(Me)
        End Function

        ''' <summary>
        ''' Returns a value indicating whether this instance and a specified System.Object
        ''' represent the same type and value.
        ''' </summary>
        ''' <param name="obj">An System.Object.</param>
        ''' <returns>true if value is a System.Half and equal to this instance; otherwise, false.</returns>
        Public Overrides Function Equals(obj As Object) As Boolean
            Dim result = False
            If Not (TypeOf obj Is Half) Then
                Return False
            End If
            Dim half = CType(obj, Half)
            If half = Me OrElse IsNaN(half) AndAlso IsNaN(Me) Then
                result = True
            End If

            Return result
        End Function

        ''' <summary>
        ''' Returns the hash code for this instance.
        ''' </summary>
        ''' <returns>A 32-bit signed integer hash code.</returns>
        Public Overrides Function GetHashCode() As Integer
            Return Value.GetHashCode()
        End Function

        ''' <summary>
        ''' Returns the System.TypeCode for value type System.Half.
        ''' </summary>
        ''' <returns>The enumerated constant (TypeCode)255.</returns>
        Public Function GetTypeCode() As TypeCode
            Return CType(255, TypeCode)
        End Function

#Region "BitConverter & Math methods for Half"

        ''' <summary>
        ''' Returns the specified half-precision floating point value as an array of bytes.
        ''' </summary>
        ''' <param name="value">The number to convert.</param>
        ''' <returns>An array of bytes with length 2.</returns>
        Public Shared Function GetBytes(value As Half) As Byte()
            Return BitConverter.GetBytes(value.Value)
        End Function

        ''' <summary>
        ''' Converts the value of a specified instance of System.Half to its equivalent binary representation.
        ''' </summary>
        ''' <param name="value">A System.Half value.</param>
        ''' <returns>A 16-bit unsigned integer that contain the binary representation of value.</returns>        
        Public Shared Function GetBits(value As Half) As UShort
            Return value.Value
        End Function

        ''' <summary>
        ''' Returns a half-precision floating point number converted from two bytes
        ''' at a specified position in a byte array.
        ''' </summary>
        ''' <param name="value">An array of bytes.</param>
        ''' <param name="startIndex">The starting position within value.</param>
        ''' <returns>A half-precision floating point number formed by two bytes beginning at startIndex.</returns>
        ''' <exception cref="System.ArgumentException">
        ''' startIndex is greater than or equal to the length of value minus 1, and is
        ''' less than or equal to the length of value minus 1.
        ''' </exception>
        ''' <exception cref="System.ArgumentNullException">value is null.</exception>
        ''' <exception cref="System.ArgumentOutOfRangeException">startIndex is less than zero or greater than the length of value minus 1.</exception>
        Public Shared Function ToHalf(value As Byte(), startIndex As Integer) As Half
            Return ToHalf(CUShort(BitConverter.ToInt16(value, startIndex)))
        End Function

        ''' <summary>
        ''' Returns a half-precision floating point number converted from its binary representation.
        ''' </summary>
        ''' <param name="bits">Binary representation of System.Half value</param>
        ''' <returns>A half-precision floating point number formed by its binary representation.</returns>
        Public Shared Function ToHalf(bits As UShort) As Half
            Return New Half With {
                .Value = bits
            }
        End Function

        ''' <summary>
        ''' Returns a value indicating the sign of a half-precision floating-point number.
        ''' </summary>
        ''' <param name="value">A signed number.</param>
        ''' <returns>
        ''' A number indicating the sign of value. Number Description -1 value is less
        ''' than zero. 0 value is equal to zero. 1 value is greater than zero.
        ''' </returns>
        ''' <exception cref="System.ArithmeticException">value is equal to System.Half.NaN.</exception>
        Public Shared Function Sign(value As Half) As Integer
            If value < 0 Then
                Return -1
            End If

            If value > 0 Then
                Return 1
            End If

            If value <> 0 Then
                Throw New ArithmeticException("Function does not accept floating point Not-a-Number values.")
            End If

            Return 0
        End Function

        ''' <summary>
        ''' Returns the absolute value of a half-precision floating-point number.
        ''' </summary>
        ''' <param name="value">A number in the range System.Half.MinValue ≤ value ≤ System.Half.MaxValue.</param>
        ''' <returns>A half-precision floating-point number, x, such that 0 ≤ x ≤System.Half.MaxValue.</returns>
        Public Shared Function Abs(value As Half) As Half
            Return HalfHelper.Abs(value)
        End Function

        ''' <summary>
        ''' Returns the larger of two half-precision floating-point numbers.
        ''' </summary>
        ''' <param name="value1">The first of two half-precision floating-point numbers to compare.</param>
        ''' <param name="value2">The second of two half-precision floating-point numbers to compare.</param>
        ''' <returns>
        ''' Parameter value1 or value2, whichever is larger. If value1, or value2, or both val1
        ''' and value2 are equal to System.Half.NaN, System.Half.NaN is returned.
        ''' </returns>
        Public Shared Function Max(value1 As Half, value2 As Half) As Half
            Return If(value1 < value2, value2, value1)
        End Function

        ''' <summary>
        ''' Returns the smaller of two half-precision floating-point numbers.
        ''' </summary>
        ''' <param name="value1">The first of two half-precision floating-point numbers to compare.</param>
        ''' <param name="value2">The second of two half-precision floating-point numbers to compare.</param>
        ''' <returns>
        ''' Parameter value1 or value2, whichever is smaller. If value1, or value2, or both val1
        ''' and value2 are equal to System.Half.NaN, System.Half.NaN is returned.
        ''' </returns>
        Public Shared Function Min(value1 As Half, value2 As Half) As Half
            Return If(value1 < value2, value1, value2)
        End Function

#End Region

        ''' <summary>
        ''' Returns a value indicating whether the specified number evaluates to not a number (System.Half.NaN).
        ''' </summary>
        ''' <param name="half">A half-precision floating-point number.</param>
        ''' <returns>true if value evaluates to not a number (System.Half.NaN); otherwise, false.</returns>
        Public Shared Function IsNaN(half As Half) As Boolean
            Return HalfHelper.IsNaN(half)
        End Function

        ''' <summary>
        ''' Returns a value indicating whether the specified number evaluates to negative or positive infinity.
        ''' </summary>
        ''' <param name="half">A half-precision floating-point number.</param>
        ''' <returns>true if half evaluates to System.Half.PositiveInfinity or System.Half.NegativeInfinity; otherwise, false.</returns>
        Public Shared Function IsInfinity(half As Half) As Boolean
            Return HalfHelper.IsInfinity(half)
        End Function

        ''' <summary>
        ''' Returns a value indicating whether the specified number evaluates to negative infinity.
        ''' </summary>
        ''' <param name="half">A half-precision floating-point number.</param>
        ''' <returns>true if half evaluates to System.Half.NegativeInfinity; otherwise, false.</returns>
        Public Shared Function IsNegativeInfinity(half As Half) As Boolean
            Return HalfHelper.IsNegativeInfinity(half)
        End Function

        ''' <summary>
        ''' Returns a value indicating whether the specified number evaluates to positive infinity.
        ''' </summary>
        ''' <param name="half">A half-precision floating-point number.</param>
        ''' <returns>true if half evaluates to System.Half.PositiveInfinity; otherwise, false.</returns>
        Public Shared Function IsPositiveInfinity(half As Half) As Boolean
            Return HalfHelper.IsPositiveInfinity(half)
        End Function

#Region "String operations (Parse and ToString)"

        ''' <summary>
        ''' Converts the string representation of a number to its System.Half equivalent.
        ''' </summary>
        ''' <param name="value">The string representation of the number to convert.</param>
        ''' <returns>The System.Half number equivalent to the number contained in value.</returns>
        ''' <exception cref="System.ArgumentNullException">value is null.</exception>
        ''' <exception cref="System.FormatException">value is not in the correct format.</exception>
        ''' <exception cref="System.OverflowException">value represents a number less than System.Half.MinValue or greater than System.Half.MaxValue.</exception>
        Public Shared Function Parse(value As String) As Half
            Return CType(Single.Parse(value, CultureInfo.InvariantCulture), Half)
        End Function

        ''' <summary>
        ''' Converts the string representation of a number to its System.Half equivalent 
        ''' using the specified culture-specific format information.
        ''' </summary>
        ''' <param name="value">The string representation of the number to convert.</param>
        ''' <param name="provider">An System.IFormatProvider that supplies culture-specific parsing information about value.</param>
        ''' <returns>The System.Half number equivalent to the number contained in s as specified by provider.</returns>
        ''' <exception cref="System.ArgumentNullException">value is null.</exception>
        ''' <exception cref="System.FormatException">value is not in the correct format.</exception>
        ''' <exception cref="System.OverflowException">value represents a number less than System.Half.MinValue or greater than System.Half.MaxValue.</exception>
        Public Shared Function Parse(value As String, provider As IFormatProvider) As Half
            Return CType(Single.Parse(value, provider), Half)
        End Function

        ''' <summary>
        ''' Converts the string representation of a number in a specified style to its System.Half equivalent.
        ''' </summary>
        ''' <param name="value">The string representation of the number to convert.</param>
        ''' <param name="style">
        ''' A bitwise combination of System.Globalization.NumberStyles values that indicates
        ''' the style elements that can be present in value. A typical value to specify is
        ''' System.Globalization.NumberStyles.Number.
        ''' </param>
        ''' <returns>The System.Half number equivalent to the number contained in s as specified by style.</returns>
        ''' <exception cref="System.ArgumentNullException">value is null.</exception>
        ''' <exception cref="System.ArgumentException">
        ''' style is not a System.Globalization.NumberStyles value. -or- style is the
        ''' System.Globalization.NumberStyles.AllowHexSpecifier value.
        ''' </exception>
        ''' <exception cref="System.FormatException">value is not in the correct format.</exception>
        ''' <exception cref="System.OverflowException">value represents a number less than System.Half.MinValue or greater than System.Half.MaxValue.</exception>
        Public Shared Function Parse(value As String, style As NumberStyles) As Half
            Return CType(Single.Parse(value, style, CultureInfo.InvariantCulture), Half)
        End Function

        ''' <summary>
        ''' Converts the string representation of a number to its System.Half equivalent 
        ''' using the specified style and culture-specific format.
        ''' </summary>
        ''' <param name="value">The string representation of the number to convert.</param>
        ''' <param name="style">
        ''' A bitwise combination of System.Globalization.NumberStyles values that indicates
        ''' the style elements that can be present in value. A typical value to specify is 
        ''' System.Globalization.NumberStyles.Number.
        ''' </param>
        ''' <param name="provider">An System.IFormatProvider object that supplies culture-specific information about the format of value.</param>
        ''' <returns>The System.Half number equivalent to the number contained in s as specified by style and provider.</returns>
        ''' <exception cref="System.ArgumentNullException">value is null.</exception>
        ''' <exception cref="System.ArgumentException">
        ''' style is not a System.Globalization.NumberStyles value. -or- style is the
        ''' System.Globalization.NumberStyles.AllowHexSpecifier value.
        ''' </exception>
        ''' <exception cref="System.FormatException">value is not in the correct format.</exception>
        ''' <exception cref="System.OverflowException">value represents a number less than System.Half.MinValue or greater than System.Half.MaxValue.</exception>
        Public Shared Function Parse(value As String, style As NumberStyles, provider As IFormatProvider) As Half
            Return CType(Single.Parse(value, style, provider), Half)
        End Function

        ''' <summary>
        ''' Converts the string representation of a number to its System.Half equivalent.
        ''' A return value indicates whether the conversion succeeded or failed.
        ''' </summary>
        ''' <param name="value">The string representation of the number to convert.</param>
        ''' <param name="result">
        ''' When this method returns, contains the System.Half number that is equivalent
        ''' to the numeric value contained in value, if the conversion succeeded, or is zero
        ''' if the conversion failed. The conversion fails if the s parameter is null,
        ''' is not a number in a valid format, or represents a number less than System.Half.MinValue
        ''' or greater than System.Half.MaxValue. This parameter is passed uninitialized.
        ''' </param>
        ''' <returns>true if s was converted successfully; otherwise, false.</returns>
        Public Shared Function TryParse(value As String, ByRef result As Half) As Boolean
            Dim f As Single

            If Single.TryParse(value, f) Then
                result = CType(f, Half)
                Return True
            End If

            result = New Half()
            Return False
        End Function

        ''' <summary>
        ''' Converts the string representation of a number to its System.Half equivalent
        ''' using the specified style and culture-specific format. A return value indicates
        ''' whether the conversion succeeded or failed.
        ''' </summary>
        ''' <param name="value">The string representation of the number to convert.</param>
        ''' <param name="style">
        ''' A bitwise combination of System.Globalization.NumberStyles values that indicates
        ''' the permitted format of value. A typical value to specify is System.Globalization.NumberStyles.Number.
        ''' </param>
        ''' <param name="provider">An System.IFormatProvider object that supplies culture-specific parsing information about value.</param>
        ''' <param name="result">
        ''' When this method returns, contains the System.Half number that is equivalent
        ''' to the numeric value contained in value, if the conversion succeeded, or is zero
        ''' if the conversion failed. The conversion fails if the s parameter is null,
        ''' is not in a format compliant with style, or represents a number less than
        ''' System.Half.MinValue or greater than System.Half.MaxValue. This parameter is passed uninitialized.
        ''' </param>
        ''' <returns>true if s was converted successfully; otherwise, false.</returns>
        ''' <exception cref="System.ArgumentException">
        ''' style is not a System.Globalization.NumberStyles value. -or- style 
        ''' is the System.Globalization.NumberStyles.AllowHexSpecifier value.
        ''' </exception>
        Public Shared Function TryParse(value As String, style As NumberStyles, provider As IFormatProvider, ByRef result As Half) As Boolean
            Dim parseResult = False
            Dim f As Single

            If Single.TryParse(value, style, provider, f) Then
                result = CType(f, Half)
                parseResult = True
            Else
                result = New Half()
            End If

            Return parseResult
        End Function

        ''' <summary>
        ''' Converts the numeric value of this instance to its equivalent string representation.
        ''' </summary>
        ''' <returns>A string that represents the value of this instance.</returns>
        Public Overrides Function ToString() As String
            Return CSng(Me).ToString(CultureInfo.InvariantCulture)
        End Function

        ''' <summary>
        ''' Converts the numeric value of this instance to its equivalent string representation
        ''' using the specified culture-specific format information.
        ''' </summary>
        ''' <param name="formatProvider">An System.IFormatProvider that supplies culture-specific formatting information.</param>
        ''' <returns>The string representation of the value of this instance as specified by provider.</returns>
        Public Overloads Function ToString(formatProvider As IFormatProvider) As String
            Return CSng(Me).ToString(formatProvider)
        End Function

        ''' <summary>
        ''' Converts the numeric value of this instance to its equivalent string representation, using the specified format.
        ''' </summary>
        ''' <param name="format">A numeric format string.</param>
        ''' <returns>The string representation of the value of this instance as specified by format.</returns>
        Public Overloads Function ToString(format As String) As String
            Return CSng(Me).ToString(format, CultureInfo.InvariantCulture)
        End Function

        ''' <summary>
        ''' Converts the numeric value of this instance to its equivalent string representation 
        ''' using the specified format and culture-specific format information.
        ''' </summary>
        ''' <param name="format">A numeric format string.</param>
        ''' <param name="formatProvider">An System.IFormatProvider that supplies culture-specific formatting information.</param>
        ''' <returns>The string representation of the value of this instance as specified by format and provider.</returns>
        ''' <exception cref="System.FormatException">format is invalid.</exception>
        Public Overloads Function ToString(format As String, formatProvider As IFormatProvider) As String Implements IFormattable.ToString
            Return CSng(Me).ToString(format, formatProvider)
        End Function

#End Region

#Region "IConvertible Members"

        Private Function IConvertible_ToSingle(provider As IFormatProvider) As Single Implements IConvertible.ToSingle
            Return Me
        End Function

        Private Function IConvertible_GetTypeCode() As TypeCode Implements IConvertible.GetTypeCode
            Return GetTypeCode()
        End Function

        Private Function IConvertible_ToBoolean(provider As IFormatProvider) As Boolean Implements IConvertible.ToBoolean
            Return Convert.ToBoolean(Me)
        End Function

        Private Function IConvertible_ToByte(provider As IFormatProvider) As Byte Implements IConvertible.ToByte
            Return Convert.ToByte(Me)
        End Function

        Private Function IConvertible_ToChar(provider As IFormatProvider) As Char Implements IConvertible.ToChar
            Throw New InvalidCastException(String.Format(CultureInfo.CurrentCulture, "Invalid cast from '{0}' to '{1}'.", "Half", "Char"))
        End Function

        Private Function IConvertible_ToDateTime(provider As IFormatProvider) As DateTime Implements IConvertible.ToDateTime
            Throw New InvalidCastException(String.Format(CultureInfo.CurrentCulture, "Invalid cast from '{0}' to '{1}'.", "Half", "DateTime"))
        End Function

        Private Function IConvertible_ToDecimal(provider As IFormatProvider) As Decimal Implements IConvertible.ToDecimal
            Return Convert.ToDecimal(Me)
        End Function

        Private Function IConvertible_ToDouble(provider As IFormatProvider) As Double Implements IConvertible.ToDouble
            Return Convert.ToDouble(Me)
        End Function

        Private Function IConvertible_ToInt16(provider As IFormatProvider) As Short Implements IConvertible.ToInt16
            Return Convert.ToInt16(Me)
        End Function

        Private Function IConvertible_ToInt32(provider As IFormatProvider) As Integer Implements IConvertible.ToInt32
            Return Convert.ToInt32(Me)
        End Function

        Private Function IConvertible_ToInt64(provider As IFormatProvider) As Long Implements IConvertible.ToInt64
            Return Convert.ToInt64(Me)
        End Function

        Private Function IConvertible_ToSByte(provider As IFormatProvider) As SByte Implements IConvertible.ToSByte
            Return Convert.ToSByte(Me)
        End Function

        Private Function IConvertible_ToString(provider As IFormatProvider) As String Implements IConvertible.ToString
            Return Convert.ToString(Me, CultureInfo.InvariantCulture)
        End Function

        Private Function IConvertible_ToType(conversionType As Type, provider As IFormatProvider) As Object Implements IConvertible.ToType
            Return TryCast(CSng(Me), IConvertible).ToType(conversionType, provider)
        End Function

        Private Function IConvertible_ToUInt16(provider As IFormatProvider) As UShort Implements IConvertible.ToUInt16
            Return Convert.ToUInt16(Me)
        End Function

        Private Function IConvertible_ToUInt32(provider As IFormatProvider) As UInteger Implements IConvertible.ToUInt32
            Return Convert.ToUInt32(Me)
        End Function

        Private Function IConvertible_ToUInt64(provider As IFormatProvider) As ULong Implements IConvertible.ToUInt64
            Return Convert.ToUInt64(Me)
        End Function

#End Region
    End Structure
End Namespace
