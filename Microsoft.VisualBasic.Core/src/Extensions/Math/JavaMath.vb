#Region "Microsoft.VisualBasic::ab064f778036d5d8e9ea89c277fd84da, Microsoft.VisualBasic.Core\src\Language\Language\Java\JavaMath.vb"

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

'   Total Lines: 2225
'    Code Lines: 199 (8.94%)
' Comment Lines: 1910 (85.84%)
'    - Xml Docs: 42.67%
' 
'   Blank Lines: 116 (5.21%)
'     File Size: 110.72 KB


'     Module JavaMath
' 
'         Function: (+4 Overloads) abs, acos, (+2 Overloads) addExact, asin, atan
'                   atan2, ceil, cos, cosh, (+2 Overloads) decrementExact
'                   exp, Expm1, floor, (+2 Overloads) floorDiv, (+2 Overloads) floorMod
'                   IEEEremainder, (+2 Overloads) incrementExact, log, log10, log1m
'                   log1p, (+2 Overloads) max, (+2 Overloads) min, (+2 Overloads) multiplyExact, (+2 Overloads) negateExact
'                   pow, random, sin, sinh, sqrt
'                   (+2 Overloads) subtractExact, tan, tanh, toDegrees, toIntExact
'                   toRadians
' 
' 
' /********************************************************************************/

#End Region

Imports std = System.Math

'
' * Copyright (c) 1994, 2013, Oracle and/or its affiliates. All rights reserved.
' * ORACLE PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
' *
Namespace Math

    ''' <summary>
    ''' The class {@code Math} contains methods for performing basic
    ''' numeric operations such as the elementary exponential, logarithm,
    ''' square root, and trigonometric functions.
    ''' 
    ''' Unlike some of the numeric methods of class
    ''' {@code StrictMath}, all implementations of the equivalent
    ''' functions of class {@code Math} are not defined to return the
    ''' bit-for-bit same results.  This relaxation permits
    ''' better-performing implementations where strict reproducibility is
    ''' not required.
    ''' 
    ''' By default many of the {@code Math} methods simply call
    ''' the equivalent method in {@code StrictMath} for their
    ''' implementation.  Code generators are encouraged to use
    ''' platform-specific native libraries or microprocessor instructions,
    ''' where available, to provide higher-performance implementations of
    ''' {@code Math} methods.  Such higher-performance
    ''' implementations still must conform to the specification for
    ''' {@code Math}.
    ''' 
    ''' The quality of implementation specifications concern two
    ''' properties, accuracy of the returned result and monotonicity of the
    ''' method.  Accuracy of the floating-point {@code Math} methods is
    ''' measured in terms of _ulps_, units in the last place.  For a
    ''' given floating-point format, an #ulp(double) ulp of a
    ''' specific real number value is the distance between the two
    ''' floating-point values bracketing that numerical value.  When
    ''' discussing the accuracy of a method as a whole rather than at a
    ''' specific argument, the number of ulps cited is for the worst-case
    ''' error at any argument.  If a method always has an error less than
    ''' 0.5 ulps, the method always returns the floating-point number
    ''' nearest the exact result; such a method is _correctly
    ''' rounded_.  A correctly rounded method is generally the best a
    ''' floating-point approximation can be; however, it is impractical for
    ''' many floating-point methods to be correctly rounded.  Instead, for
    ''' the {@code Math} [Class], a larger error bound of 1 or 2 ulps is
    ''' allowed for certain methods.  Informally, with a 1 ulp error bound,
    ''' when the exact result is a representable number, the exact result
    ''' should be returned as the computed result; otherwise, either of the
    ''' two floating-point values which bracket the exact result may be
    ''' returned.  For exact results large in magnitude, one of the
    ''' endpoints of the bracket may be infinite.  Besides accuracy at
    ''' individual arguments, maintaining proper relations between the
    ''' method at different arguments is also important.  Therefore, most
    ''' methods with more than 0.5 ulp errors are required to be
    ''' _semi-monotonic_: whenever the mathematical function is
    ''' non-decreasing, so is the floating-point approximation, likewise,
    ''' whenever the mathematical function is non-increasing, so is the
    ''' floating-point approximation.  Not all approximations that have 1
    ''' ulp accuracy will automatically meet the monotonicity requirements.
    ''' 
    ''' 
    ''' The platform uses signed two's complement integer arithmetic with
    ''' int and long primitive types.  The developer should choose
    ''' the primitive type to ensure that arithmetic operations consistently
    ''' produce correct results, which in some cases means the operations
    ''' will not overflow the range of values of the computation.
    ''' The best practice is to choose the primitive type and algorithm to avoid
    ''' overflow. In cases where the size is {@code int} or {@code long} and
    ''' overflow errors need to be detected, the methods {@code addExact},
    ''' {@code subtractExact}, {@code multiplyExact}, and {@code toIntExact}
    ''' throw an {@code ArithmeticException} when the results overflow.
    ''' For other arithmetic operations such as divide, absolute value,
    ''' increment, decrement, and negation overflow occurs only with
    ''' a specific minimum or maximum value and should be checked against
    ''' the minimum or maximum as appropriate.
    ''' 
    ''' @author  unascribed
    ''' @author  Joseph D. Darcy
    ''' @since   JDK1.0
    ''' </summary>
    Public Module JavaMath

        ''' <summary>
        ''' Returns the sum of its arguments,
        ''' throwing an exception if the result overflows an {@code int}.
        ''' </summary>
        ''' <param name="x"> the first value </param>
        ''' <param name="y"> the second value </param>
        ''' <returns> the result </returns>
        ''' <exception cref="ArithmeticException"> if the result overflows an int
        ''' @since 1.8 </exception>
        Public Function addExact(x As Integer, y As Integer) As Integer
            Dim r As Integer = x + y
            ' HD 2-12 Overflow iff both arguments have the opposite sign of the result
            If ((x Xor r) And (y Xor r)) < 0 Then Throw New ArithmeticException("integer overflow")
            Return r
        End Function

        ''' <summary>
        ''' Returns the sum of its arguments,
        ''' throwing an exception if the result overflows a {@code long}.
        ''' </summary>
        ''' <param name="x"> the first value </param>
        ''' <param name="y"> the second value </param>
        ''' <returns> the result </returns>
        ''' <exception cref="ArithmeticException"> if the result overflows a long
        ''' @since 1.8 </exception>
        Public Function addExact(x As Long, y As Long) As Long
            Dim r As Long = x + y
            ' HD 2-12 Overflow iff both arguments have the opposite sign of the result
            If ((x Xor r) And (y Xor r)) < 0 Then Throw New ArithmeticException("long overflow")
            Return r
        End Function

        ''' <summary>
        ''' Returns the difference of the arguments,
        ''' throwing an exception if the result overflows an {@code int}.
        ''' </summary>
        ''' <param name="x"> the first value </param>
        ''' <param name="y"> the second value to subtract from the first </param>
        ''' <returns> the result </returns>
        ''' <exception cref="ArithmeticException"> if the result overflows an int
        ''' @since 1.8 </exception>
        Public Function subtractExact(x As Integer, y As Integer) As Integer
            Dim r As Integer = x - y
            ' HD 2-12 Overflow iff the arguments have different signs and
            ' the sign of the result is different than the sign of x
            If ((x Xor y) And (x Xor r)) < 0 Then Throw New ArithmeticException("integer overflow")
            Return r
        End Function

        ''' <summary>
        ''' Returns the difference of the arguments,
        ''' throwing an exception if the result overflows a {@code long}.
        ''' </summary>
        ''' <param name="x"> the first value </param>
        ''' <param name="y"> the second value to subtract from the first </param>
        ''' <returns> the result </returns>
        ''' <exception cref="ArithmeticException"> if the result overflows a long
        ''' @since 1.8 </exception>
        Public Function subtractExact(x As Long, y As Long) As Long
            Dim r As Long = x - y
            ' HD 2-12 Overflow iff the arguments have different signs and
            ' the sign of the result is different than the sign of x
            If ((x Xor y) And (x Xor r)) < 0 Then Throw New ArithmeticException("long overflow")
            Return r
        End Function

        ''' <summary>
        ''' Returns the product of the arguments,
        ''' throwing an exception if the result overflows an {@code int}.
        ''' </summary>
        ''' <param name="x"> the first value </param>
        ''' <param name="y"> the second value </param>
        ''' <returns> the result </returns>
        ''' <exception cref="ArithmeticException"> if the result overflows an int
        ''' @since 1.8 </exception>
        Public Function multiplyExact(x As Integer, y As Integer) As Integer
            Dim r As Long = CLng(x) * CLng(y)
            If CInt(r) <> r Then Throw New ArithmeticException("integer overflow")
            Return CInt(r)
        End Function

        ''' <summary>
        ''' Returns the product of the arguments,
        ''' throwing an exception if the result overflows a {@code long}.
        ''' </summary>
        ''' <param name="x"> the first value </param>
        ''' <param name="y"> the second value </param>
        ''' <returns> the result </returns>
        ''' <exception cref="ArithmeticException"> if the result overflows a long
        ''' @since 1.8 </exception>
        Public Function multiplyExact(x As Long, y As Long) As Long
            Dim r As Long = x * y
            Dim ax As Long = std.Abs(x)
            Dim ay As Long = std.Abs(y)
            If (CInt(CUInt((ax Or ay)) >> 31 <> 0)) Then
                ' Some bits greater than 2^31 that might cause overflow
                ' Check the result using the divide operator
                ' and check for the special case of java.lang.[Long].MIN_VALUE * -1
                If ((y <> 0) AndAlso (r \ y <> x)) OrElse (x = [Int64].MinValue AndAlso y = -1) Then Throw New ArithmeticException("long overflow")
            End If
            Return r
        End Function

        ''' <summary>
        ''' Returns the argument incremented by one, throwing an exception if the
        ''' result overflows an {@code int}.
        ''' </summary>
        ''' <param name="a"> the value to increment </param>
        ''' <returns> the result </returns>
        ''' <exception cref="ArithmeticException"> if the result overflows an int
        ''' @since 1.8 </exception>
        Public Function incrementExact(a As Integer) As Integer
            If a = [Int32].MaxValue Then Throw New ArithmeticException("integer overflow")

            Return a + 1
        End Function

        ''' <summary>
        ''' Returns the argument incremented by one, throwing an exception if the
        ''' result overflows a {@code long}.
        ''' </summary>
        ''' <param name="a"> the value to increment </param>
        ''' <returns> the result </returns>
        ''' <exception cref="ArithmeticException"> if the result overflows a long
        ''' @since 1.8 </exception>
        Public Function incrementExact(a As Long) As Long
            If a = [Int64].MaxValue Then Throw New ArithmeticException("long overflow")

            Return a + 1L
        End Function

        ''' <summary>
        ''' Returns the argument decremented by one, throwing an exception if the
        ''' result overflows an {@code int}.
        ''' </summary>
        ''' <param name="a"> the value to decrement </param>
        ''' <returns> the result </returns>
        ''' <exception cref="ArithmeticException"> if the result overflows an int
        ''' @since 1.8 </exception>
        Public Function decrementExact(a As Integer) As Integer
            If a = [Int32].MinValue Then Throw New ArithmeticException("integer overflow")

            Return a - 1
        End Function

        ''' <summary>
        ''' Returns the argument decremented by one, throwing an exception if the
        ''' result overflows a {@code long}.
        ''' </summary>
        ''' <param name="a"> the value to decrement </param>
        ''' <returns> the result </returns>
        ''' <exception cref="ArithmeticException"> if the result overflows a long
        ''' @since 1.8 </exception>
        Public Function decrementExact(a As Long) As Long
            If a = [Int64].MinValue Then Throw New ArithmeticException("long overflow")

            Return a - 1L
        End Function

        ''' <summary>
        ''' Returns the negation of the argument, throwing an exception if the
        ''' result overflows an {@code int}.
        ''' </summary>
        ''' <param name="a"> the value to negate </param>
        ''' <returns> the result </returns>
        ''' <exception cref="ArithmeticException"> if the result overflows an int
        ''' @since 1.8 </exception>
        Public Function negateExact(a As Integer) As Integer
            If a = [Int32].MinValue Then Throw New ArithmeticException("integer overflow")

            Return -a
        End Function

        ''' <summary>
        ''' Returns the negation of the argument, throwing an exception if the
        ''' result overflows a {@code long}.
        ''' </summary>
        ''' <param name="a"> the value to negate </param>
        ''' <returns> the result </returns>
        ''' <exception cref="ArithmeticException"> if the result overflows a long
        ''' @since 1.8 </exception>
        Public Function negateExact(a As Long) As Long
            If a = [Int64].MinValue Then Throw New ArithmeticException("long overflow")

            Return -a
        End Function

        ''' <summary>
        ''' Returns the value of the {@code long} argument;
        ''' throwing an exception if the value overflows an {@code int}.
        ''' </summary>
        ''' <param name="value"> the long value </param>
        ''' <returns> the argument as an int </returns>
        ''' <exception cref="ArithmeticException"> if the {@code argument} overflows an int
        ''' @since 1.8 </exception>
        Public Function toIntExact(value As Long) As Integer
            If CInt(value) <> value Then Throw New ArithmeticException("integer overflow")
            Return CInt(value)
        End Function

        ''' <summary>
        ''' Returns the largest (closest to positive infinity)
        ''' {@code int} value that is less than or equal to the algebraic quotient.
        ''' There is one special case, if the dividend is the
        '''  Integer#MIN_VALUE  java.lang.[Integer].MIN_VALUE"/> and the divisor is {@code -1},
        ''' then integer overflow occurs and
        ''' the result is equal to the {@code  java.lang.[Integer].MIN_VALUE}.
        ''' 
        ''' Normal integer division operates under the round to zero rounding mode
        ''' (truncation).  This operation instead acts under the round toward
        ''' negative infinity (floor) rounding mode.
        ''' The floor rounding mode gives different results than truncation
        ''' when the exact result is negative.
        ''' 
        '''   + If the signs of the arguments are the same, the results of
        '''       {@code floorDiv} and the {@code /} operator are the same.  
        '''       For example, {@code floorDiv(4, 3) == 1} and {@code (4 / 3) == 1}.
        '''   + If the signs of the arguments are different,  the quotient is negative and
        '''       {@code floorDiv} returns the integer less than or equal to the quotient
        '''       and the {@code /} operator returns the integer closest to zero.
        '''       For example, {@code floorDiv(-4, 3) == -2},
        '''       whereas {@code (-4 / 3) == -1}.
        ''' </summary>
        ''' <param name="x"> the dividend </param>
        ''' <param name="y"> the divisor </param>
        ''' <returns> the largest (closest to positive infinity)
        ''' {@code int} value that is less than or equal to the algebraic quotient. </returns>
        ''' <exception cref="ArithmeticException"> if the divisor {@code y} is zero </exception>
        Public Function floorDiv(x As Integer, y As Integer) As Integer
            Dim r As Integer = x \ y
            ' if the signs are different and modulo not zero, round down
            If (x Xor y) < 0 AndAlso (r * y <> x) Then r -= 1
            Return r
        End Function

        ''' <summary>
        ''' Returns the largest (closest to positive infinity)
        ''' {@code long} value that is less than or equal to the algebraic quotient.
        ''' There is one special case, if the dividend is the
        '''  Long#MIN_VALUE java.lang.[Long].MIN_VALUE"/> and the divisor is {@code -1},
        ''' then integer overflow occurs and
        ''' the result is equal to the {@code java.lang.[Long].MIN_VALUE}.
        ''' 
        ''' Normal integer division operates under the round to zero rounding mode
        ''' (truncation).  This operation instead acts under the round toward
        ''' negative infinity (floor) rounding mode.
        ''' The floor rounding mode gives different results than truncation
        ''' when the exact result is negative.
        ''' 
        ''' For examples, see  #floorDiv(int, int)"/>.
        ''' </summary>
        ''' <param name="x"> the dividend </param>
        ''' <param name="y"> the divisor </param>
        ''' <returns> the largest (closest to positive infinity)
        ''' {@code long} value that is less than or equal to the algebraic quotient. </returns>
        ''' <exception cref="ArithmeticException"> if the divisor {@code y} is zero </exception>
        Public Function floorDiv(x As Long, y As Long) As Long
            Dim r As Long = x \ y
            ' if the signs are different and modulo not zero, round down
            If (x Xor y) < 0 AndAlso (r * y <> x) Then r -= 1
            Return r
        End Function

        ''' <summary>
        ''' Returns the floor modulus of the {@code int} arguments.
        ''' 
        ''' The floor modulus is {@code x - (floorDiv(x, y) * y)},
        ''' has the same sign as the divisor {@code y}, and
        ''' is in the range of {@code -abs(y) &lt; r &lt; +abs(y)}.
        ''' 
        ''' 
        ''' The relationship between {@code floorDiv} and {@code floorMod} is such that:
        ''' 
        '''   + {@code floorDiv(x, y) * y + floorMod(x, y) == x}
        ''' 
        ''' 
        ''' The difference in values between {@code floorMod} and
        ''' the {@code %} operator is due to the difference between
        ''' {@code floorDiv} that returns the integer less than or equal to the quotient
        ''' and the {@code /} operator that returns the integer closest to zero.
        ''' 
        ''' Examples:
        ''' 
        '''   + If the signs of the arguments are the same, the results
        '''       of {@code floorMod} and the {@code %} operator are the same.   
        '''       
        '''       + {@code floorMod(4, 3) == 1};   and {@code (4 % 3) == 1} 
        '''       
        '''   + If the signs of the arguments are different, the results differ from the {@code %} operator. 
        '''      
        '''      + {@code floorMod(+4, -3) == -2};   and {@code (+4 % -3) == +1}  
        '''      + {@code floorMod(-4, +3) == +2};   and {@code (-4 % +3) == -1}  
        '''      + {@code floorMod(-4, -3) == -1};   and {@code (-4 % -3) == -1 }  
        '''      
        '''    
        ''' 
        ''' 
        ''' If the signs of arguments are unknown and a positive modulus
        ''' is needed it can be computed as {@code (floorMod(x, y) + abs(y)) % abs(y)}.
        ''' </summary>
        ''' <param name="x"> the dividend </param>
        ''' <param name="y"> the divisor </param>
        ''' <returns> the floor modulus {@code x - (floorDiv(x, y) * y)} </returns>
        ''' <exception cref="ArithmeticException"> if the divisor {@code y} is zero </exception>
        Public Function floorMod(x As Integer, y As Integer) As Integer
            Dim r As Integer = x - floorDiv(x, y) * y
            Return r
        End Function

        ''' <summary>
        ''' Returns the floor modulus of the {@code long} arguments.
        ''' 
        ''' The floor modulus is {@code x - (floorDiv(x, y) * y)},
        ''' has the same sign as the divisor {@code y}, and
        ''' is in the range of {@code -abs(y) &lt; r &lt; +abs(y)}.
        ''' 
        ''' 
        ''' The relationship between {@code floorDiv} and {@code floorMod} is such that:
        ''' 
        '''   + {@code floorDiv(x, y) * y + floorMod(x, y) == x}
        ''' 
        ''' 
        ''' For examples, see  #floorMod(int, int)"/>.
        ''' </summary>
        ''' <param name="x"> the dividend </param>
        ''' <param name="y"> the divisor </param>
        ''' <returns> the floor modulus {@code x - (floorDiv(x, y) * y)} </returns>
        ''' <exception cref="ArithmeticException"> if the divisor {@code y} is zero </exception>
        Public Function floorMod(x As Long, y As Long) As Long
            Return x - floorDiv(x, y) * y
        End Function

        ''' <summary>
        ''' Returns _e_<sup>x</sup> -1.  Note that for values of
        ''' _x_ near 0, the exact sum of
        ''' {@code expm1(x)} + 1 is much closer to the true
        ''' result of _e_<sup>x</sup> than {@code exp(x)}.
        ''' 
        ''' Special cases:
        ''' 
        ''' + If the argument is NaN, the result is NaN.
        ''' 
        ''' + If the argument is positive infinity, then the result is
        ''' positive infinity.
        ''' 
        ''' + If the argument is negative infinity, then the result is
        ''' -1.0.
        ''' 
        ''' + If the argument is zero, then the result is a zero with the
        ''' same sign as the argument.
        ''' 
        ''' 
        ''' 
        ''' The computed result must be within 1 ulp of the exact result.
        ''' Results must be semi-monotonic.  The result of
        ''' {@code expm1} for any finite input must be greater than or
        ''' equal to {@code -1.0}.  Note that once the exact result of
        ''' _e_<sup>{@code x}</sup> - 1 is within 1/2
        ''' ulp of the limit value -1, {@code -1.0} should be
        ''' returned.
        ''' </summary>
        ''' <param name="x">   the exponent to raise _e_ to in the computation of
        '''              _e_<sup>{@code x}</sup> -1. </param>
        ''' <returns>  the value _e_<sup>{@code x}</sup> - 1.
        ''' @since 1.5 </returns>
        Public Function Expm1(x As Double) As Double
            If std.Abs(x) < 0.00001 Then
                Return x + 0.5 * x * x
            Else
                Return std.Exp(x) - 1.0
            End If
        End Function

        ''' <summary>
        ''' Returns the natural logarithm of the sum of the argument and 1.
        ''' Note that for small values {@code x}, the result of
        ''' {@code log1p(x)} is much closer to the true result of ln(1
        ''' + {@code x}) than the floating-point evaluation of
        ''' {@code log(1.0+x)}.
        ''' 
        ''' Special cases:
        ''' 
        ''' 
        ''' 
        ''' + If the argument is NaN or less than -1, then the result is
        ''' NaN.
        ''' 
        ''' + If the argument is positive infinity, then the result is
        ''' positive infinity.
        ''' 
        ''' + If the argument is negative one, then the result is
        ''' negative infinity.
        ''' 
        ''' + If the argument is zero, then the result is a zero with the
        ''' same sign as the argument.
        ''' 
        ''' 
        ''' 
        ''' The computed result must be within 1 ulp of the exact result.
        ''' Results must be semi-monotonic.
        ''' </summary>
        ''' <param name="x">   a value </param>
        ''' <returns> the value ln({@code x} + 1), the natural
        ''' log of {@code x} + 1
        ''' @since 1.5 </returns>
        ''' <remarks>http://www.johndcook.com/csharp_log_one_plus_x.html</remarks>
        Public Function log1p(x As Double) As Double
            If x <= -1.0 Then
                Return [Double].NaN
            End If

            If std.Abs(x) > 0.0001 Then
                Return std.Log(1.0 + x)
            End If

            ' Use Taylor approx. log(1 + x) = x - x^2/2 with error roughly x^3/3
            ' Since |x| < 10^-4, |x|^3 < 10^-12, relative error less than 10^-8
            Return (-0.5 * x + 1.0) * x
        End Function

        'Public Function Log1p(x As Double) As Double
        '    Dim y = x
        '    Return If(1 + y = 1, y, y * (Math.Log(1 + y) / (1 + y - 1)))
        'End Function

        ''' <summary>
        ''' Computes log(1-x) without losing precision for small values of x.
        ''' </summary>
        ''' 
        Public Function log1m(x As Double) As Double
            If x >= 1.0 Then
                Return [Double].NaN
            End If

            If std.Abs(x) > 0.0001 Then
                Return std.Log(1.0 - x)
            End If

            ' Use Taylor approx. log(1 + x) = x - x^2/2 with error roughly x^3/3
            ' Since |x| < 10^-4, |x|^3 < 10^-12, relative error less than 10^-8
            Return -(0.5 * x + 1.0) * x
        End Function

    End Module
End Namespace
