#Region "Microsoft.VisualBasic::c3596bb305a11d1630b4a5182304e5eb, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\lang\Java\JavaMath.vb"

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

Imports sys = System.Math

'
' * Copyright (c) 1994, 2013, Oracle and/or its affiliates. All rights reserved.
' * ORACLE PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' 

Namespace Language.Java

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
        ''' The {@code double} value that is closer than any other to
        ''' _e_, the base of the natural logarithms.
        ''' </summary>
        Public Const E As Double = 2.7182818284590451

        ''' <summary>
        ''' The {@code double} value that is closer than any other to
        ''' _pi_, the ratio of the circumference of a circle to its
        ''' diameter.
        ''' </summary>
        Public Const PI As Double = 3.1415926535897931

        ''' <summary>
        ''' Returns the trigonometric sine of an angle.  Special cases:
        ''' + If the argument is NaN or an infinity, then the
        ''' result is NaN.
        ''' + If the argument is zero, then the result is a zero with the
        ''' same sign as the argument.
        ''' 
        ''' The computed result must be within 1 ulp of the exact result.
        ''' Results must be semi-monotonic.
        ''' </summary>
        ''' <param name="a">   an angle, in radians. </param>
        ''' <returns>  the sine of the argument. </returns>
        Public Function sin(a As Double) As Double
            Return sys.Sin(a) ' default impl. delegates to StrictMath
        End Function

        ''' <summary>
        ''' Returns the trigonometric cosine of an angle. Special cases:
        ''' + If the argument is NaN or an infinity, then the
        ''' result is NaN.
        ''' 
        ''' The computed result must be within 1 ulp of the exact result.
        ''' Results must be semi-monotonic.
        ''' </summary>
        ''' <param name="a">   an angle, in radians. </param>
        ''' <returns>  the cosine of the argument. </returns>
        Public Function cos(a As Double) As Double
            Return sys.Cos(a) ' default impl. delegates to StrictMath
        End Function

        ''' <summary>
        ''' Returns the trigonometric tangent of an angle.  Special cases:
        ''' + If the argument is NaN or an infinity, then the result
        ''' is NaN.
        ''' + If the argument is zero, then the result is a zero with the
        ''' same sign as the argument.
        ''' 
        ''' The computed result must be within 1 ulp of the exact result.
        ''' Results must be semi-monotonic.
        ''' </summary>
        ''' <param name="a">   an angle, in radians. </param>
        ''' <returns>  the tangent of the argument. </returns>
        Public Function tan(a As Double) As Double
            Return sys.Tan(a) ' default impl. delegates to StrictMath
        End Function

        ''' <summary>
        ''' Returns the arc sine of a value; the returned angle is in the
        ''' range -_pi_/2 through _pi_/2.  Special cases:
        ''' + If the argument is NaN or its absolute value is greater
        ''' than 1, then the result is NaN.
        ''' + If the argument is zero, then the result is a zero with the
        ''' same sign as the argument.
        ''' 
        ''' The computed result must be within 1 ulp of the exact result.
        ''' Results must be semi-monotonic.
        ''' </summary>
        ''' <param name="a">   the value whose arc sine is to be returned. </param>
        ''' <returns>  the arc sine of the argument. </returns>
        Public Function asin(a As Double) As Double
            Return sys.Asin(a) ' default impl. delegates to StrictMath
        End Function

        ''' <summary>
        ''' Returns the arc cosine of a value; the returned angle is in the
        ''' range 0.0 through _pi_.  Special case:
        ''' + If the argument is NaN or its absolute value is greater
        ''' than 1, then the result is NaN.
        ''' 
        ''' The computed result must be within 1 ulp of the exact result.
        ''' Results must be semi-monotonic.
        ''' </summary>
        ''' <param name="a">   the value whose arc cosine is to be returned. </param>
        ''' <returns>  the arc cosine of the argument. </returns>
        Public Function acos(a As Double) As Double
            Return sys.Acos(a) ' default impl. delegates to StrictMath
        End Function

        ''' <summary>
        ''' Returns the arc tangent of a value; the returned angle is in the
        ''' range -_pi_/2 through _pi_/2.  Special cases:
        ''' + If the argument is NaN, then the result is NaN.
        ''' + If the argument is zero, then the result is a zero with the
        ''' same sign as the argument.
        ''' 
        ''' The computed result must be within 1 ulp of the exact result.
        ''' Results must be semi-monotonic.
        ''' </summary>
        ''' <param name="a">   the value whose arc tangent is to be returned. </param>
        ''' <returns>  the arc tangent of the argument. </returns>
        Public Function atan(a As Double) As Double
            Return sys.Atan(a) ' default impl. delegates to StrictMath
        End Function

        ''' <summary>
        ''' Converts an angle measured in degrees to an approximately
        ''' equivalent angle measured in radians.  The conversion from
        ''' degrees to radians is generally inexact.
        ''' </summary>
        ''' <param name="angdeg">   an angle, in degrees </param>
        ''' <returns>  the measurement of the angle {@code angdeg}
        '''          in radians.
        ''' @since   1.2 </returns>
        Public Function toRadians(angdeg As Double) As Double
            Return angdeg / 180.0 * PI
        End Function

        ''' <summary>
        ''' Converts an angle measured in radians to an approximately
        ''' equivalent angle measured in degrees.  The conversion from
        ''' radians to degrees is generally inexact; users should
        ''' _not_ expect {@code cos(toRadians(90.0))} to exactly
        ''' equal {@code 0.0}.
        ''' </summary>
        ''' <param name="angrad">   an angle, in radians </param>
        ''' <returns>  the measurement of the angle {@code angrad}
        '''          in degrees.
        ''' @since   1.2 </returns>
        Public Function toDegrees(angrad As Double) As Double
            Return angrad * 180.0 / PI
        End Function

        ''' <summary>
        ''' Returns Euler's number _e_ raised to the power of a
        ''' {@code double} value.  Special cases:
        ''' + If the argument is NaN, the result is NaN.
        ''' + If the argument is positive infinity, then the result is
        ''' positive infinity.
        ''' + If the argument is negative infinity, then the result is
        ''' positive zero.
        ''' 
        ''' The computed result must be within 1 ulp of the exact result.
        ''' Results must be semi-monotonic.
        ''' </summary>
        ''' <param name="a">   the exponent to raise _e_ to. </param>
        ''' <returns>  the value _e_{@code a},
        '''          where _e_ is the base of the natural logarithms. </returns>
        Public Function exp(a As Double) As Double
            Return sys.Exp(a) ' default impl. delegates to StrictMath
        End Function

        ''' <summary>
        ''' Returns the natural logarithm (base _e_) of a {@code double}
        ''' value.  Special cases:
        ''' + If the argument is NaN or less than zero, then the result
        ''' is NaN.
        ''' + If the argument is positive infinity, then the result is
        ''' positive infinity.
        ''' + If the argument is positive zero or negative zero, then the
        ''' result is negative infinity.
        ''' 
        ''' The computed result must be within 1 ulp of the exact result.
        ''' Results must be semi-monotonic.
        ''' </summary>
        ''' <param name="a">   a value </param>
        ''' <returns>  the value ln {@code a}, the natural logarithm of
        '''          {@code a}. </returns>
        Public Function log(a As Double) As Double
            Return sys.Log(a) ' default impl. delegates to StrictMath
        End Function

        ''' <summary>
        ''' Returns the base 10 logarithm of a {@code double} value.
        ''' Special cases:
        ''' 
        ''' + If the argument is NaN or less than zero, then the result
        ''' is NaN.
        ''' + If the argument is positive infinity, then the result is
        ''' positive infinity.
        ''' + If the argument is positive zero or negative zero, then the
        ''' result is negative infinity.
        ''' +  If the argument is equal to 10<sup>_n_</sup> for
        ''' integer _n_, then the result is _n_.
        ''' 
        ''' 
        ''' The computed result must be within 1 ulp of the exact result.
        ''' Results must be semi-monotonic.
        ''' </summary>
        ''' <param name="a">   a value </param>
        ''' <returns>  the base 10 logarithm of  {@code a}.
        ''' @since 1.5 </returns>
        Public Function log10(a As Double) As Double
            Return sys.Log10(a) ' default impl. delegates to StrictMath
        End Function

        ''' <summary>
        ''' Returns the correctly rounded positive square root of a
        ''' {@code double} value.
        ''' Special cases:
        ''' + If the argument is NaN or less than zero, then the result
        ''' is NaN.
        ''' + If the argument is positive infinity, then the result is positive
        ''' infinity.
        ''' + If the argument is positive zero or negative zero, then the
        ''' result is the same as the argument.
        ''' Otherwise, the result is the {@code double} value closest to
        ''' the true mathematical square root of the argument value.
        ''' </summary>
        ''' <param name="a">   a value. </param>
        ''' <returns>  the positive square root of {@code a}.
        '''          If the argument is NaN or less than zero, the result is NaN. </returns>
        Public Function sqrt(a As Double) As Double
            Return sys.Sqrt(a) ' default impl. delegates to StrictMath
            ' Note that hardware sqrt instructions
            ' frequently can be directly used by JITs
            ' and should be much faster than doing
            ' sys.sqrt in software.
        End Function

        '''' <summary>
        '''' Returns the cube root of a {@code double} value.  For
        '''' positive finite {@code x}, {@code cbrt(-x) ==
        '''' -cbrt(x)}; that is, the cube root of a negative value is
        '''' the negative of the cube root of that value's magnitude.
        '''' 
        '''' Special cases:
        '''' 
        '''' 
        '''' 
        '''' + If the argument is NaN, then the result is NaN.
        '''' 
        '''' + If the argument is infinite, then the result is an infinity
        '''' with the same sign as the argument.
        '''' 
        '''' + If the argument is zero, then the result is a zero with the
        '''' same sign as the argument.
        '''' 
        '''' 
        '''' 
        '''' The computed result must be within 1 ulp of the exact result.
        '''' </summary>
        '''' <param name="a">   a value. </param>
        '''' <returns>  the cube root of {@code a}.
        '''' @since 1.5 </returns>
        'Public Function cbrt(a As Double) As Double
        '    Return sys.cbrt(a)
        'End Function

        ''' <summary>
        ''' Computes the remainder operation on two arguments as prescribed
        ''' by the IEEE 754 standard.
        ''' The remainder value is mathematically equal to
        ''' ``f1 - f2`` x _n_,
        ''' where _n_ is the mathematical integer closest to the exact
        ''' mathematical value of the quotient {@code f1/f2}, and if two
        ''' mathematical integers are equally close to {@code f1/f2},
        ''' then _n_ is the integer that is even. If the remainder is
        ''' zero, its sign is the same as the sign of the first argument.
        ''' Special cases:
        ''' + If either argument is NaN, or the first argument is infinite,
        ''' or the second argument is positive zero or negative zero, then the
        ''' result is NaN.
        ''' + If the first argument is finite and the second argument is
        ''' infinite, then the result is the same as the first argument.
        ''' </summary>
        ''' <param name="f1">   the dividend. </param>
        ''' <param name="f2">   the divisor. </param>
        ''' <returns>  the remainder when {@code f1} is divided by
        '''          {@code f2}. </returns>
        Public Function IEEEremainder(f1 As Double, f2 As Double) As Double
            Return sys.IEEERemainder(f1, f2) ' delegate to StrictMath
        End Function

        ''' <summary>
        ''' Returns the smallest (closest to negative infinity)
        ''' {@code double} value that is greater than or equal to the
        ''' argument and is equal to a mathematical  java.lang.[Integer]. Special cases:
        ''' + If the argument value is already equal to a
        ''' mathematical integer, then the result is the same as the
        ''' argument.  + If the argument is NaN or an infinity or
        ''' positive zero or negative zero, then the result is the same as
        ''' the argument.  + If the argument value is less than zero but
        ''' greater than -1.0, then the result is negative zero. Note
        ''' that the value of {@code sys.ceil(x)} is exactly the
        ''' value of {@code -Math.floor(-x)}.
        ''' 
        ''' </summary>
        ''' <param name="a">   a value. </param>
        ''' <returns>  the smallest (closest to negative infinity)
        '''          floating-point value that is greater than or equal to
        '''          the argument and is equal to a mathematical  java.lang.[Integer]. </returns>
        Public Function ceil(a As Double) As Double
            Return sys.Ceiling(a) ' default impl. delegates to StrictMath
        End Function

        ''' <summary>
        ''' Returns the largest (closest to positive infinity)
        ''' {@code double} value that is less than or equal to the
        ''' argument and is equal to a mathematical  java.lang.[Integer]. Special cases:
        ''' + If the argument value is already equal to a
        ''' mathematical integer, then the result is the same as the
        ''' argument.  + If the argument is NaN or an infinity or
        ''' positive zero or negative zero, then the result is the same as
        ''' the argument.
        ''' </summary>
        ''' <param name="a">   a value. </param>
        ''' <returns>  the largest (closest to positive infinity)
        '''          floating-point value that less than or equal to the argument
        '''          and is equal to a mathematical  java.lang.[Integer]. </returns>
        Public Function floor(a As Double) As Double
            Return sys.Floor(a) ' default impl. delegates to StrictMath
        End Function

        '''' <summary>
        '''' Returns the {@code double} value that is closest in value
        '''' to the argument and is equal to a mathematical  java.lang.[Integer]. If two
        '''' {@code double} values that are mathematical integers are
        '''' equally close, the result is the integer value that is
        '''' even. Special cases:
        '''' + If the argument value is already equal to a mathematical
        '''' integer, then the result is the same as the argument.
        '''' + If the argument is NaN or an infinity or positive zero or negative
        '''' zero, then the result is the same as the argument.
        '''' </summary>
        '''' <param name="a">   a {@code double} value. </param>
        '''' <returns>  the closest floating-point value to {@code a} that is
        ''''          equal to a mathematical  java.lang.[Integer]. </returns>
        'Public Function rint(a As Double) As Double
        '    Return sys.rint(a) ' default impl. delegates to StrictMath
        'End Function

        ''' <summary>
        ''' Returns the angle _theta_ from the conversion of rectangular
        ''' coordinates ({@code x}, {@code y}) to polar
        ''' coordinates (r, _theta_).
        ''' This method computes the phase _theta_ by computing an arc tangent
        ''' of {@code y/x} in the range of -_pi_ to _pi_. Special
        ''' cases:
        ''' + If either argument is NaN, then the result is NaN.
        ''' + If the first argument is positive zero and the second argument
        ''' is positive, or the first argument is positive and finite and the
        ''' second argument is positive infinity, then the result is positive
        ''' zero.
        ''' + If the first argument is negative zero and the second argument
        ''' is positive, or the first argument is negative and finite and the
        ''' second argument is positive infinity, then the result is negative zero.
        ''' + If the first argument is positive zero and the second argument
        ''' is negative, or the first argument is positive and finite and the
        ''' second argument is negative infinity, then the result is the
        ''' {@code double} value closest to _pi_.
        ''' + If the first argument is negative zero and the second argument
        ''' is negative, or the first argument is negative and finite and the
        ''' second argument is negative infinity, then the result is the
        ''' {@code double} value closest to -_pi_.
        ''' + If the first argument is positive and the second argument is
        ''' positive zero or negative zero, or the first argument is positive
        ''' infinity and the second argument is finite, then the result is the
        ''' {@code double} value closest to _pi_/2.
        ''' + If the first argument is negative and the second argument is
        ''' positive zero or negative zero, or the first argument is negative
        ''' infinity and the second argument is finite, then the result is the
        ''' {@code double} value closest to -_pi_/2.
        ''' + If both arguments are positive infinity, then the result is the
        ''' {@code double} value closest to _pi_/4.
        ''' + If the first argument is positive infinity and the second argument
        ''' is negative infinity, then the result is the {@code double}
        ''' value closest to 3*_pi_/4.
        ''' + If the first argument is negative infinity and the second argument
        ''' is positive infinity, then the result is the {@code double} value
        ''' closest to -_pi_/4.
        ''' + If both arguments are negative infinity, then the result is the
        ''' {@code double} value closest to -3*_pi_/4.
        ''' 
        ''' The computed result must be within 2 ulps of the exact result.
        ''' Results must be semi-monotonic.
        ''' </summary>
        ''' <param name="y">   the ordinate coordinate </param>
        ''' <param name="x">   the abscissa coordinate </param>
        ''' <returns>  the _theta_ component of the point
        '''          (_r_, _theta_)
        '''          in polar coordinates that corresponds to the point
        '''          (_x_, _y_) in Cartesian coordinates. </returns>
        Public Function atan2(y As Double, x As Double) As Double
            Return sys.Atan2(y, x) ' default impl. delegates to StrictMath
        End Function

        ''' <summary>
        ''' Returns the value of the first argument raised to the power of the
        ''' second argument. Special cases:
        ''' 
        ''' + If the second argument is positive or negative zero, then the
        ''' result is 1.0.
        ''' + If the second argument is 1.0, then the result is the same as the
        ''' first argument.
        ''' + If the second argument is NaN, then the result is NaN.
        ''' + If the first argument is NaN and the second argument is nonzero,
        ''' then the result is NaN.
        ''' 
        ''' + If
        ''' 
        ''' + the absolute value of the first argument is greater than 1
        ''' and the second argument is positive infinity, or
        ''' + the absolute value of the first argument is less than 1 and
        ''' the second argument is negative infinity,
        ''' 
        ''' then the result is positive infinity.
        ''' 
        ''' + If
        ''' 
        ''' + the absolute value of the first argument is greater than 1 and
        ''' the second argument is negative infinity, or
        ''' + the absolute value of the
        ''' first argument is less than 1 and the second argument is positive
        ''' infinity,
        ''' 
        ''' then the result is positive zero.
        ''' 
        ''' + If the absolute value of the first argument equals 1 and the
        ''' second argument is infinite, then the result is NaN.
        ''' 
        ''' + If
        ''' 
        ''' + the first argument is positive zero and the second argument
        ''' is greater than zero, or
        ''' + the first argument is positive infinity and the second
        ''' argument is less than zero,
        ''' 
        ''' then the result is positive zero.
        ''' 
        ''' + If
        ''' 
        ''' + the first argument is positive zero and the second argument
        ''' is less than zero, or
        ''' + the first argument is positive infinity and the second
        ''' argument is greater than zero,
        ''' 
        ''' then the result is positive infinity.
        ''' 
        ''' + If
        ''' 
        ''' + the first argument is negative zero and the second argument
        ''' is greater than zero but not a finite odd integer, or
        ''' + the first argument is negative infinity and the second
        ''' argument is less than zero but not a finite odd integer,
        ''' 
        ''' then the result is positive zero.
        ''' 
        ''' + If
        ''' 
        ''' + the first argument is negative zero and the second argument
        ''' is a positive finite odd integer, or
        ''' + the first argument is negative infinity and the second
        ''' argument is a negative finite odd integer,
        ''' 
        ''' then the result is negative zero.
        ''' 
        ''' + If
        ''' 
        ''' + the first argument is negative zero and the second argument
        ''' is less than zero but not a finite odd integer, or
        ''' + the first argument is negative infinity and the second
        ''' argument is greater than zero but not a finite odd integer,
        ''' 
        ''' then the result is positive infinity.
        ''' 
        ''' + If
        ''' 
        ''' + the first argument is negative zero and the second argument
        ''' is a negative finite odd integer, or
        ''' + the first argument is negative infinity and the second
        ''' argument is a positive finite odd integer,
        ''' 
        ''' then the result is negative infinity.
        ''' 
        ''' + If the first argument is finite and less than zero
        ''' 
        ''' +  if the second argument is a finite even integer, the
        ''' result is equal to the result of raising the absolute value of
        ''' the first argument to the power of the second argument
        ''' 
        ''' + if the second argument is a finite odd integer, the result
        ''' is equal to the negative of the result of raising the absolute
        ''' value of the first argument to the power of the second
        ''' argument
        ''' 
        ''' + if the second argument is finite and not an integer, then
        ''' the result is NaN.
        ''' 
        ''' 
        ''' + If both arguments are integers, then the result is exactly equal
        ''' to the mathematical result of raising the first argument to the power
        ''' of the second argument if that result can in fact be represented
        ''' exactly as a {@code double} value.
        ''' 
        ''' (In the foregoing descriptions, a floating-point value is
        ''' considered to be an integer if and only if it is finite and a
        ''' fixed point of the method #ceil ceil or,
        ''' equivalently, a fixed point of the method {@link #floor
        ''' floor}. A value is a fixed point of a one-argument
        ''' method if and only if the result of applying the method to the
        ''' value is equal to the value.)
        ''' 
        ''' The computed result must be within 1 ulp of the exact result.
        ''' Results must be semi-monotonic.
        ''' </summary>
        ''' <param name="a">   the base. </param>
        ''' <param name="b">   the exponent. </param>
        ''' <returns>  the value {@code a}<sup>{@code b}</sup>. </returns>
        Public Function pow(a As Double, b As Double) As Double
            Return sys.Pow(a, b) ' default impl. delegates to StrictMath
        End Function

        '''' <summary>
        '''' Returns the closest {@code int} to the argument, with ties
        '''' rounding to positive infinity.
        '''' 
        '''' 
        '''' Special cases:
        '''' + If the argument is NaN, the result is 0.
        '''' + If the argument is negative infinity or any value less than or
        '''' equal to the value of {@code  java.lang.[Integer].MIN_VALUE}, the result is
        '''' equal to the value of {@code  java.lang.[Integer].MIN_VALUE}.
        '''' + If the argument is positive infinity or any value greater than or
        '''' equal to the value of {@code  java.lang.[Integer].MAX_VALUE}, the result is
        '''' equal to the value of {@code  java.lang.[Integer].MAX_VALUE}.
        '''' </summary>
        '''' <param name="a">   a floating-point value to be rounded to an  java.lang.[Integer]. </param>
        '''' <returns>  the value of the argument rounded to the nearest
        ''''          {@code int} value. </returns>
        '''' <seealso cref=     java.lang.Integer#MAX_VALUE </seealso>
        '''' <seealso cref=     java.lang.Integer#MIN_VALUE </seealso>
        'Public Function round(a As Single) As Integer
        '    Dim intBits As Integer = Float.floatToRawIntBits(a)
        '    Dim biasedExp As Integer = (intBits And sun.misc.FloatConsts.EXP_BIT_MASK) >> (sun.misc.FloatConsts.SIGNIFICAND_WIDTH - 1)
        '    Dim shift As Integer = (sun.misc.FloatConsts.SIGNIFICAND_WIDTH - 2 + sun.misc.FloatConsts.EXP_BIAS) - biasedExp
        '    If (shift And -32) = 0 Then ' shift >= 0 && shift < 32
        '        ' a is a finite number such that pow(2,-32) <= ulp(a) < 1
        '        Dim r As Integer = ((intBits And sun.misc.FloatConsts.SIGNIF_BIT_MASK) Or (sun.misc.FloatConsts.SIGNIF_BIT_MASK + 1))
        '        If intBits < 0 Then r = -r
        '        ' In the comments below each Java expression evaluates to the value
        '        ' the corresponding mathematical expression:
        '        ' (r) evaluates to a / ulp(a)
        '        ' (r >> shift) evaluates to floor(a * 2)
        '        ' ((r >> shift) + 1) evaluates to floor((a + 1/2) * 2)
        '        ' (((r >> shift) + 1) >> 1) evaluates to floor(a + 1/2)
        '        Return ((r >> shift) + 1) >> 1
        '    Else
        '        ' a is either
        '        ' - a finite number with abs(a) < exp(2,FloatConsts.SIGNIFICAND_WIDTH-32) < 1/2
        '        ' - a finite number with ulp(a) >= 1 and hence a is a mathematical integer
        '        ' - an infinity or NaN
        '        Return CInt(Fix(a))
        '    End If
        'End Function

        '''' <summary>
        '''' Returns the closest {@code long} to the argument, with ties
        '''' rounding to positive infinity.
        '''' 
        '''' Special cases:
        '''' + If the argument is NaN, the result is 0.
        '''' + If the argument is negative infinity or any value less than or
        '''' equal to the value of {@code java.lang.[Long].MIN_VALUE}, the result is
        '''' equal to the value of {@code java.lang.[Long].MIN_VALUE}.
        '''' + If the argument is positive infinity or any value greater than or
        '''' equal to the value of {@code java.lang.[Long].MAX_VALUE}, the result is
        '''' equal to the value of {@code java.lang.[Long].MAX_VALUE}.
        '''' </summary>
        '''' <param name="a">   a floating-point value to be rounded to a
        ''''          {@code long}. </param>
        '''' <returns>  the value of the argument rounded to the nearest
        ''''          {@code long} value. </returns>
        '''' <seealso cref=     java.lang.Long#MAX_VALUE </seealso>
        '''' <seealso cref=     java.lang.Long#MIN_VALUE </seealso>
        'Public Function round(a As Double) As Long
        '    Dim longBits As Long = Java.lang.[Double].doubleToRawLongBits(a)
        '    Dim biasedExp As Long = (longBits And sun.misc.DoubleConsts.EXP_BIT_MASK) >> (sun.misc.DoubleConsts.SIGNIFICAND_WIDTH - 1)
        '    Dim shift As Long = (sun.misc.DoubleConsts.SIGNIFICAND_WIDTH - 2 + sun.misc.DoubleConsts.EXP_BIAS) - biasedExp
        '    If (shift And -64) = 0 Then ' shift >= 0 && shift < 64
        '        ' a is a finite number such that pow(2,-64) <= ulp(a) < 1
        '        Dim r As Long = ((longBits And sun.misc.DoubleConsts.SIGNIF_BIT_MASK) Or (sun.misc.DoubleConsts.SIGNIF_BIT_MASK + 1))
        '        If longBits < 0 Then r = -r
        '        ' In the comments below each Java expression evaluates to the value
        '        ' the corresponding mathematical expression:
        '        ' (r) evaluates to a / ulp(a)
        '        ' (r >> shift) evaluates to floor(a * 2)
        '        ' ((r >> shift) + 1) evaluates to floor((a + 1/2) * 2)
        '        ' (((r >> shift) + 1) >> 1) evaluates to floor(a + 1/2)
        '        Return ((r >> shift) + 1) >> 1
        '    Else
        '        ' a is either
        '        ' - a finite number with abs(a) < exp(2,DoubleConsts.SIGNIFICAND_WIDTH-64) < 1/2
        '        ' - a finite number with ulp(a) >= 1 and hence a is a mathematical integer
        '        ' - an infinity or NaN
        '        Return CLng(Fix(a))
        '    End If
        'End Function

        Private NotInheritable Class RandomNumberGeneratorHolder
            Friend Shared ReadOnly randomNumberGenerator As New Random
        End Class

        ''' <summary>
        ''' Returns a {@code double} value with a positive sign, greater
        ''' than or equal to {@code 0.0} and less than {@code 1.0}.
        ''' Returned values are chosen pseudorandomly with (approximately)
        ''' uniform distribution from that range.
        ''' 
        ''' When this method is first called, it creates a single new
        ''' pseudorandom-number generator, exactly as if by the expression
        ''' 
        ''' <blockquote>{@code new java.util.Random()}</blockquote>
        ''' 
        ''' This new pseudorandom-number generator is used thereafter for
        ''' all calls to this method and is used nowhere else.
        ''' 
        ''' This method is properly synchronized to allow correct use by
        ''' more than one thread. However, if many threads need to generate
        ''' pseudorandom numbers at a great rate, it may reduce contention
        ''' for each thread to have its own pseudorandom-number generator.
        ''' </summary>
        ''' <returns>  a pseudorandom {@code double} greater than or equal
        ''' to {@code 0.0} and less than {@code 1.0}. </returns>
        Public Function random() As Double
            Return RandomNumberGeneratorHolder.randomNumberGenerator.NextDouble()
        End Function

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
            Dim ax As Long = sys.Abs(x)
            Dim ay As Long = sys.Abs(y)
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
        ''' Returns the absolute value of an {@code int} value.
        ''' If the argument is not negative, the argument is returned.
        ''' If the argument is negative, the negation of the argument is returned.
        ''' 
        ''' Note that if the argument is equal to the value of
        '''  Integer#MIN_VALUE"/>, the most negative representable
        ''' {@code int} value, the result is that same value, which is
        ''' negative.
        ''' </summary>
        ''' <param name="a">   the argument whose absolute value is to be determined </param>
        ''' <returns>  the absolute value of the argument. </returns>
        Public Function abs(a As Integer) As Integer
            Return If(a < 0, -a, a)
        End Function

        ''' <summary>
        ''' Returns the absolute value of a {@code long} value.
        ''' If the argument is not negative, the argument is returned.
        ''' If the argument is negative, the negation of the argument is returned.
        ''' 
        ''' Note that if the argument is equal to the value of
        '''  Long#MIN_VALUE"/>, the most negative representable
        ''' {@code long} value, the result is that same value, which
        ''' is negative.
        ''' </summary>
        ''' <param name="a">   the argument whose absolute value is to be determined </param>
        ''' <returns>  the absolute value of the argument. </returns>
        Public Function abs(a As Long) As Long
            Return If(a < 0, -a, a)
        End Function

        ''' <summary>
        ''' Returns the absolute value of a {@code float} value.
        ''' If the argument is not negative, the argument is returned.
        ''' If the argument is negative, the negation of the argument is returned.
        ''' Special cases:
        ''' + If the argument is positive zero or negative zero, the
        ''' result is positive zero.
        ''' + If the argument is infinite, the result is positive infinity.
        ''' + If the argument is NaN, the result is NaN.
        ''' In other words, the result is the same as the value of the expression:
        ''' {@code Float.intBitsToFloat(0x7fffffff &amp; Float.floatToIntBits(a))}
        ''' </summary>
        ''' <param name="a">   the argument whose absolute value is to be determined </param>
        ''' <returns>  the absolute value of the argument. </returns>
        Public Function abs(a As Single) As Single
            Return If(a <= 0.0F, 0.0F - a, a)
        End Function

        ''' <summary>
        ''' Returns the absolute value of a {@code double} value.
        ''' If the argument is not negative, the argument is returned.
        ''' If the argument is negative, the negation of the argument is returned.
        ''' Special cases:
        ''' + If the argument is positive zero or negative zero, the result
        ''' is positive zero.
        ''' + If the argument is infinite, the result is positive infinity.
        ''' + If the argument is NaN, the result is NaN.
        ''' In other words, the result is the same as the value of the expression:
        ''' {@code java.lang.[Double].longBitsToDouble((Double.doubleToLongBits(a)&lt;&lt;1)>>>1)}
        ''' </summary>
        ''' <param name="a">   the argument whose absolute value is to be determined </param>
        ''' <returns>  the absolute value of the argument. </returns>
        Public Function abs(a As Double) As Double
            Return If(a <= 0.0R, 0.0R - a, a)
        End Function

        ''' <summary>
        ''' Returns the greater of two {@code int} values. That is, the
        ''' result is the argument closer to the value of
        '''  Integer#MAX_VALUE"/>. If the arguments have the same value,
        ''' the result is that same value.
        ''' </summary>
        ''' <param name="a">   an argument. </param>
        ''' <param name="b">   another argument. </param>
        ''' <returns>  the larger of {@code a} and {@code b}. </returns>
        Public Function max(a As Integer, b As Integer) As Integer
            Return If(a >= b, a, b)
        End Function

        ''' <summary>
        ''' Returns the greater of two {@code long} values. That is, the
        ''' result is the argument closer to the value of
        '''  Long#MAX_VALUE"/>. If the arguments have the same value,
        ''' the result is that same value.
        ''' </summary>
        ''' <param name="a">   an argument. </param>
        ''' <param name="b">   another argument. </param>
        ''' <returns>  the larger of {@code a} and {@code b}. </returns>
        Public Function max(a As Long, b As Long) As Long
            Return If(a >= b, a, b)
        End Function

        '' Use raw bit-wise conversions on guaranteed non-NaN arguments.
        'Private negativeZeroFloatBits As Long = Float.floatToRawIntBits(-0.0F)
        'Private negativeZeroDoubleBits As Long = Java.lang.[Double].doubleToRawLongBits(-0.0R)

        '''' <summary>
        '''' Returns the greater of two {@code float} values.  That is,
        '''' the result is the argument closer to positive infinity. If the
        '''' arguments have the same value, the result is that same
        '''' value. If either value is NaN, then the result is NaN.  Unlike
        '''' the numerical comparison operators, this method considers
        '''' negative zero to be strictly smaller than positive zero. If one
        '''' argument is positive zero and the other negative zero, the
        '''' result is positive zero.
        '''' </summary>
        '''' <param name="a">   an argument. </param>
        '''' <param name="b">   another argument. </param>
        '''' <returns>  the larger of {@code a} and {@code b}. </returns>
        'Public Function max(a As Single, b As Single) As Single
        '    If a <> a Then Return a ' a is NaN
        '    If (a = 0.0F) AndAlso (b = 0.0F) AndAlso (Float.floatToRawIntBits(a) = negativeZeroFloatBits) Then Return b
        '    Return If(a >= b, a, b)
        'End Function

        '''' <summary>
        '''' Returns the greater of two {@code double} values.  That
        '''' is, the result is the argument closer to positive infinity. If
        '''' the arguments have the same value, the result is that same
        '''' value. If either value is NaN, then the result is NaN.  Unlike
        '''' the numerical comparison operators, this method considers
        '''' negative zero to be strictly smaller than positive zero. If one
        '''' argument is positive zero and the other negative zero, the
        '''' result is positive zero.
        '''' </summary>
        '''' <param name="a">   an argument. </param>
        '''' <param name="b">   another argument. </param>
        '''' <returns>  the larger of {@code a} and {@code b}. </returns>
        'Public Function max(a As Double, b As Double) As Double
        '    If a <> a Then Return a ' a is NaN
        '    If (a = 0.0R) AndAlso (b = 0.0R) AndAlso (Double.doubleToRawLongBits(a) = negativeZeroDoubleBits) Then Return b
        '    Return If(a >= b, a, b)
        'End Function

        ''' <summary>
        ''' Returns the smaller of two {@code int} values. That is,
        ''' the result the argument closer to the value of
        '''  Integer#MIN_VALUE"/>.  If the arguments have the same
        ''' value, the result is that same value.
        ''' </summary>
        ''' <param name="a">   an argument. </param>
        ''' <param name="b">   another argument. </param>
        ''' <returns>  the smaller of {@code a} and {@code b}. </returns>
        Public Function min(a As Integer, b As Integer) As Integer
            Return If(a <= b, a, b)
        End Function

        ''' <summary>
        ''' Returns the smaller of two {@code long} values. That is,
        ''' the result is the argument closer to the value of
        '''  Long#MIN_VALUE"/>. If the arguments have the same
        ''' value, the result is that same value.
        ''' </summary>
        ''' <param name="a">   an argument. </param>
        ''' <param name="b">   another argument. </param>
        ''' <returns>  the smaller of {@code a} and {@code b}. </returns>
        Public Function min(a As Long, b As Long) As Long
            Return If(a <= b, a, b)
        End Function

        '''' <summary>
        '''' Returns the smaller of two {@code float} values.  That is,
        '''' the result is the value closer to negative infinity. If the
        '''' arguments have the same value, the result is that same
        '''' value. If either value is NaN, then the result is NaN.  Unlike
        '''' the numerical comparison operators, this method considers
        '''' negative zero to be strictly smaller than positive zero.  If
        '''' one argument is positive zero and the other is negative zero,
        '''' the result is negative zero.
        '''' </summary>
        '''' <param name="a">   an argument. </param>
        '''' <param name="b">   another argument. </param>
        '''' <returns>  the smaller of {@code a} and {@code b}. </returns>
        'Public Function min(a As Single, b As Single) As Single
        '    If a <> a Then Return a ' a is NaN
        '    If (a = 0.0F) AndAlso (b = 0.0F) AndAlso (Float.floatToRawIntBits(b) = negativeZeroFloatBits) Then Return b
        '    Return If(a <= b, a, b)
        'End Function

        '''' <summary>
        '''' Returns the smaller of two {@code double} values.  That
        '''' is, the result is the value closer to negative infinity. If the
        '''' arguments have the same value, the result is that same
        '''' value. If either value is NaN, then the result is NaN.  Unlike
        '''' the numerical comparison operators, this method considers
        '''' negative zero to be strictly smaller than positive zero. If one
        '''' argument is positive zero and the other is negative zero, the
        '''' result is negative zero.
        '''' </summary>
        '''' <param name="a">   an argument. </param>
        '''' <param name="b">   another argument. </param>
        '''' <returns>  the smaller of {@code a} and {@code b}. </returns>
        'Public Function min(a As Double, b As Double) As Double
        '    If a <> a Then Return a ' a is NaN
        '    If (a = 0.0R) AndAlso (b = 0.0R) AndAlso (Double.doubleToRawLongBits(b) = negativeZeroDoubleBits) Then Return b
        '    Return If(a <= b, a, b)
        'End Function

        '''' <summary>
        '''' Returns the size of an ulp of the argument.  An ulp, unit in
        '''' the last place, of a {@code double} value is the positive
        '''' distance between this floating-point value and the {@code
        '''' double} value next larger in magnitude.  Note that for non-NaN
        '''' _x_, <code>ulp(-_x_) == ulp(_x_)</code>.
        '''' 
        '''' Special Cases:
        '''' 
        '''' +  If the argument is NaN, then the result is NaN.
        '''' +  If the argument is positive or negative infinity, then the
        '''' result is positive infinity.
        '''' +  If the argument is positive or negative zero, then the result is
        '''' {@code java.lang.[Double].MIN_VALUE}.
        '''' +  If the argument is &plusmn;{@code java.lang.[Double].MAX_VALUE}, then
        '''' the result is equal to 2<sup>971</sup>.
        '''' 
        '''' </summary>
        '''' <param name="d"> the floating-point value whose ulp is to be returned </param>
        '''' <returns> the size of an ulp of the argument
        '''' @author Joseph D. Darcy
        '''' @since 1.5 </returns>
        'Public Function ulp(d As Double) As Double
        '    Dim exp As Integer = getExponent(d)

        '    Select Case exp
        '        Case sun.misc.DoubleConsts.MAX_EXPONENT + 1 ' NaN or infinity
        '            Return sys.Abs(d)

        '        Case sun.misc.DoubleConsts.MIN_EXPONENT - 1 ' zero or subnormal
        '            Return Java.lang.[Double].MIN_VALUE

        '        Case Else
        '            Debug.Assert(exp <= sun.misc.DoubleConsts.MAX_EXPONENT AndAlso exp >= sun.misc.DoubleConsts.MIN_EXPONENT)

        '            ' ulp(x) is usually 2^(SIGNIFICAND_WIDTH-1)*(2^ilogb(x))
        '            exp = exp - (sun.misc.DoubleConsts.SIGNIFICAND_WIDTH - 1)
        '            If exp >= sun.misc.DoubleConsts.MIN_EXPONENT Then
        '                Return powerOfTwoD(exp)
        '            Else
        '                ' return a subnormal result; left shift integer
        '                ' representation of java.lang.[Double].MIN_VALUE appropriate
        '                ' number of positions
        '                Return Java.lang.[Double].longBitsToDouble(1L << (exp - (sun.misc.DoubleConsts.MIN_EXPONENT - (sun.misc.DoubleConsts.SIGNIFICAND_WIDTH - 1))))
        '            End If
        '    End Select
        'End Function

        '''' <summary>
        '''' Returns the size of an ulp of the argument.  An ulp, unit in
        '''' the last place, of a {@code float} value is the positive
        '''' distance between this floating-point value and the {@code
        '''' float} value next larger in magnitude.  Note that for non-NaN
        '''' _x_, <code>ulp(-_x_) == ulp(_x_)</code>.
        '''' 
        '''' Special Cases:
        '''' 
        '''' +  If the argument is NaN, then the result is NaN.
        '''' +  If the argument is positive or negative infinity, then the
        '''' result is positive infinity.
        '''' +  If the argument is positive or negative zero, then the result is
        '''' {@code Float.MIN_VALUE}.
        '''' +  If the argument is &plusmn;{@code Float.MAX_VALUE}, then
        '''' the result is equal to 2<sup>104</sup>.
        '''' 
        '''' </summary>
        '''' <param name="f"> the floating-point value whose ulp is to be returned </param>
        '''' <returns> the size of an ulp of the argument
        '''' @author Joseph D. Darcy
        '''' @since 1.5 </returns>
        'Public Function ulp(f As Single) As Single
        '    Dim exp As Integer = getExponent(f)

        '    Select Case exp
        '        Case sun.misc.FloatConsts.MAX_EXPONENT + 1 ' NaN or infinity
        '            Return sys.Abs(f)

        '        Case sun.misc.FloatConsts.MIN_EXPONENT - 1 ' zero or subnormal
        '            Return sun.misc.FloatConsts.MIN_VALUE

        '        Case Else
        '            Debug.Assert(exp <= sun.misc.FloatConsts.MAX_EXPONENT AndAlso exp >= sun.misc.FloatConsts.MIN_EXPONENT)

        '            ' ulp(x) is usually 2^(SIGNIFICAND_WIDTH-1)*(2^ilogb(x))
        '            exp = exp - (sun.misc.FloatConsts.SIGNIFICAND_WIDTH - 1)
        '            If exp >= sun.misc.FloatConsts.MIN_EXPONENT Then
        '                Return powerOfTwoF(exp)
        '            Else
        '                ' return a subnormal result; left shift integer
        '                ' representation of FloatConsts.MIN_VALUE appropriate
        '                ' number of positions
        '                Return Float.intBitsToFloat(1 << (exp - (sun.misc.FloatConsts.MIN_EXPONENT - (sun.misc.FloatConsts.SIGNIFICAND_WIDTH - 1))))
        '            End If
        '    End Select
        'End Function

        '''' <summary>
        '''' Returns the signum function of the argument; zero if the argument
        '''' is zero, 1.0 if the argument is greater than zero, -1.0 if the
        '''' argument is less than zero.
        '''' 
        '''' Special Cases:
        '''' 
        '''' +  If the argument is NaN, then the result is NaN.
        '''' +  If the argument is positive zero or negative zero, then the
        ''''      result is the same as the argument.
        '''' 
        '''' </summary>
        '''' <param name="d"> the floating-point value whose signum is to be returned </param>
        '''' <returns> the signum function of the argument
        '''' @author Joseph D. Darcy
        '''' @since 1.5 </returns>
        'Public Function signum(d As Double) As Double
        '    Return If(d = 0.0 OrElse [Double].IsNaN(d), d, copySign(1.0, d))
        'End Function

        '''' <summary>
        '''' Returns the signum function of the argument; zero if the argument
        '''' is zero, 1.0f if the argument is greater than zero, -1.0f if the
        '''' argument is less than zero.
        '''' 
        '''' Special Cases:
        '''' 
        '''' +  If the argument is NaN, then the result is NaN.
        '''' +  If the argument is positive zero or negative zero, then the
        ''''      result is the same as the argument.
        '''' 
        '''' </summary>
        '''' <param name="f"> the floating-point value whose signum is to be returned </param>
        '''' <returns> the signum function of the argument
        '''' @author Joseph D. Darcy
        '''' @since 1.5 </returns>
        'Public Function signum(f As Single) As Single
        '    Return If(f = 0.0F OrElse Single.IsNaN(f), f, copySign(1.0F, f))
        'End Function

        ''' <summary>
        ''' Returns the hyperbolic sine of a {@code double} value.
        ''' The hyperbolic sine of _x_ is defined to be
        ''' (_e<sup>x</sup> - e<sup>-x</sup>_)/2
        ''' where _e_ is  Math#E Euler's number"/>.
        ''' 
        ''' Special cases:
        ''' 
        ''' 
        ''' + If the argument is NaN, then the result is NaN.
        ''' 
        ''' + If the argument is infinite, then the result is an infinity
        ''' with the same sign as the argument.
        ''' 
        ''' + If the argument is zero, then the result is a zero with the
        ''' same sign as the argument.
        ''' 
        ''' 
        ''' 
        ''' The computed result must be within 2.5 ulps of the exact result.
        ''' </summary>
        ''' <param name="x"> The number whose hyperbolic sine is to be returned. </param>
        ''' <returns>  The hyperbolic sine of {@code x}.
        ''' @since 1.5 </returns>
        Public Function sinh(x As Double) As Double
            Return sys.Sinh(x)
        End Function

        ''' <summary>
        ''' Returns the hyperbolic cosine of a {@code double} value.
        ''' The hyperbolic cosine of _x_ is defined to be
        ''' (_e<sup>x</sup> + e<sup>-x</sup>_)/2
        ''' where _e_ is  Math#E Euler's number"/>.
        ''' 
        ''' Special cases:
        ''' 
        ''' 
        ''' + If the argument is NaN, then the result is NaN.
        ''' 
        ''' + If the argument is infinite, then the result is positive
        ''' infinity.
        ''' 
        ''' + If the argument is zero, then the result is {@code 1.0}.
        ''' 
        ''' 
        ''' 
        ''' The computed result must be within 2.5 ulps of the exact result.
        ''' </summary>
        ''' <param name="x"> The number whose hyperbolic cosine is to be returned. </param>
        ''' <returns>  The hyperbolic cosine of {@code x}.
        ''' @since 1.5 </returns>
        Public Function cosh(x As Double) As Double
            Return sys.Cosh(x)
        End Function

        ''' <summary>
        ''' Returns the hyperbolic tangent of a {@code double} value.
        ''' The hyperbolic tangent of _x_ is defined to be
        ''' (_e<sup>x</sup> - e<sup>-x</sup>_)/(_e<sup>x</sup> + e<sup>-x</sup>_),
        ''' in other words, {@link Math#sinh
        ''' sinh(_x_)}/ Math#cosh cosh(_x_)"/>.  Note
        ''' that the absolute value of the exact tanh is always less than
        ''' 1.
        ''' 
        ''' Special cases:
        ''' 
        ''' 
        ''' + If the argument is NaN, then the result is NaN.
        ''' 
        ''' + If the argument is zero, then the result is a zero with the
        ''' same sign as the argument.
        ''' 
        ''' + If the argument is positive infinity, then the result is
        ''' {@code +1.0}.
        ''' 
        ''' + If the argument is negative infinity, then the result is
        ''' {@code -1.0}.
        ''' 
        ''' 
        ''' 
        ''' The computed result must be within 2.5 ulps of the exact result.
        ''' The result of {@code tanh} for any finite input must have
        ''' an absolute value less than or equal to 1.  Note that once the
        ''' exact result of tanh is within 1/2 of an ulp of the limit value
        ''' of 1, correctly signed {@code 1.0} should be returned.
        ''' </summary>
        ''' <param name="x"> The number whose hyperbolic tangent is to be returned. </param>
        ''' <returns>  The hyperbolic tangent of {@code x}.
        ''' @since 1.5 </returns>
        Public Function tanh(x As Double) As Double
            Return sys.Tanh(x)
        End Function

        '''' <summary>
        '''' Returns sqrt(_x_<sup>2</sup> +_y_<sup>2</sup>)
        '''' without intermediate overflow or underflow.
        '''' 
        '''' Special cases:
        '''' 
        '''' 
        '''' +  If either argument is infinite, then the result
        '''' is positive infinity.
        '''' 
        '''' +  If either argument is NaN and neither argument is infinite,
        '''' then the result is NaN.
        '''' 
        '''' 
        '''' 
        '''' The computed result must be within 1 ulp of the exact
        '''' result.  If one parameter is held constant, the results must be
        '''' semi-monotonic in the other parameter.
        '''' </summary>
        '''' <param name="x"> a value </param>
        '''' <param name="y"> a value </param>
        '''' <returns> sqrt(_x_<sup>2</sup> +_y_<sup>2</sup>)
        '''' without intermediate overflow or underflow
        '''' @since 1.5 </returns>
        'Public Function hypot(x As Double, y As Double) As Double
        '    Return sys.hypot(x, y)
        'End Function

        '''' <summary>
        '''' Returns _e_<sup>x</sup> -1.  Note that for values of
        '''' _x_ near 0, the exact sum of
        '''' {@code expm1(x)} + 1 is much closer to the true
        '''' result of _e_<sup>x</sup> than {@code exp(x)}.
        '''' 
        '''' Special cases:
        '''' 
        '''' + If the argument is NaN, the result is NaN.
        '''' 
        '''' + If the argument is positive infinity, then the result is
        '''' positive infinity.
        '''' 
        '''' + If the argument is negative infinity, then the result is
        '''' -1.0.
        '''' 
        '''' + If the argument is zero, then the result is a zero with the
        '''' same sign as the argument.
        '''' 
        '''' 
        '''' 
        '''' The computed result must be within 1 ulp of the exact result.
        '''' Results must be semi-monotonic.  The result of
        '''' {@code expm1} for any finite input must be greater than or
        '''' equal to {@code -1.0}.  Note that once the exact result of
        '''' _e_<sup>{@code x}</sup> - 1 is within 1/2
        '''' ulp of the limit value -1, {@code -1.0} should be
        '''' returned.
        '''' </summary>
        '''' <param name="x">   the exponent to raise _e_ to in the computation of
        ''''              _e_<sup>{@code x}</sup> -1. </param>
        '''' <returns>  the value _e_<sup>{@code x}</sup> - 1.
        '''' @since 1.5 </returns>
        'Public Function expm1(x As Double) As Double
        '    Return sys.expm1(x)
        'End Function

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

            If sys.Abs(x) > 0.0001 Then
                Return sys.Log(1.0 + x)
            End If

            ' Use Taylor approx. log(1 + x) = x - x^2/2 with error roughly x^3/3
            ' Since |x| < 10^-4, |x|^3 < 10^-12, relative error less than 10^-8
            Return (-0.5 * x + 1.0) * x
        End Function

        ''' <summary>
        ''' Computes log(1-x) without losing precision for small values of x.
        ''' </summary>
        ''' 
        Public Function Log1m(x As Double) As Double
            If x >= 1.0 Then
                Return [Double].NaN
            End If

            If sys.Abs(x) > 0.0001 Then
                Return sys.Log(1.0 - x)
            End If

            ' Use Taylor approx. log(1 + x) = x - x^2/2 with error roughly x^3/3
            ' Since |x| < 10^-4, |x|^3 < 10^-12, relative error less than 10^-8
            Return -(0.5 * x + 1.0) * x
        End Function

        '''' <summary>
        '''' Returns the first floating-point argument with the sign of the
        '''' second floating-point argument.  Note that unlike the {@link
        '''' StrictMath#copySign(double, double) StrictMath.copySign}
        '''' method, this method does not require NaN {@code sign}
        '''' arguments to be treated as positive values; implementations are
        '''' permitted to treat some NaN arguments as positive and other NaN
        '''' arguments as negative to allow greater performance.
        '''' </summary>
        '''' <param name="magnitude">  the parameter providing the magnitude of the result </param>
        '''' <param name="sign">   the parameter providing the sign of the result </param>
        '''' <returns> a value with the magnitude of {@code magnitude}
        '''' and the sign of {@code sign}.
        '''' @since 1.6 </returns>
        'Public Function copySign(magnitude As Double, sign As Double) As Double
        '    Return Java.lang.[Double].longBitsToDouble((Double.doubleToRawLongBits(sign) And (sun.misc.DoubleConsts.SIGN_BIT_MASK)) Or (Double.doubleToRawLongBits(magnitude) And (sun.misc.DoubleConsts.EXP_BIT_MASK Or sun.misc.DoubleConsts.SIGNIF_BIT_MASK)))
        'End Function

        '''' <summary>
        '''' Returns the first floating-point argument with the sign of the
        '''' second floating-point argument.  Note that unlike the {@link
        '''' StrictMath#copySign(float, float) StrictMath.copySign}
        '''' method, this method does not require NaN {@code sign}
        '''' arguments to be treated as positive values; implementations are
        '''' permitted to treat some NaN arguments as positive and other NaN
        '''' arguments as negative to allow greater performance.
        '''' </summary>
        '''' <param name="magnitude">  the parameter providing the magnitude of the result </param>
        '''' <param name="sign">   the parameter providing the sign of the result </param>
        '''' <returns> a value with the magnitude of {@code magnitude}
        '''' and the sign of {@code sign}.
        '''' @since 1.6 </returns>
        'Public Function copySign(magnitude As Single, sign As Single) As Single
        '    Return Float.intBitsToFloat((Float.floatToRawIntBits(sign) And (sun.misc.FloatConsts.SIGN_BIT_MASK)) Or (Float.floatToRawIntBits(magnitude) And (sun.misc.FloatConsts.EXP_BIT_MASK Or sun.misc.FloatConsts.SIGNIF_BIT_MASK)))
        'End Function

        '''' <summary>
        '''' Returns the unbiased exponent used in the representation of a
        '''' {@code float}.  Special cases:
        '''' 
        '''' 
        '''' + If the argument is NaN or infinite, then the result is
        ''''  Float#MAX_EXPONENT"/> + 1.
        '''' + If the argument is zero or subnormal, then the result is
        ''''  Float#MIN_EXPONENT"/> -1.
        ''''  </summary>
        '''' <param name="f"> a {@code float} value </param>
        '''' <returns> the unbiased exponent of the argument
        '''' @since 1.6 </returns>
        'Public Function getExponent(f As Single) As Integer
        '    '        
        '    '         * Bitwise convert f to integer, mask out exponent bits, shift
        '    '         * to the right and then subtract out float's bias adjust to
        '    '         * get true exponent value
        '    '         
        '    Return ((Float.floatToRawIntBits(f) And sun.misc.FloatConsts.EXP_BIT_MASK) >> (sun.misc.FloatConsts.SIGNIFICAND_WIDTH - 1)) - sun.misc.FloatConsts.EXP_BIAS
        'End Function

        '''' <summary>
        '''' Returns the unbiased exponent used in the representation of a
        '''' {@code double}.  Special cases:
        '''' 
        '''' 
        '''' + If the argument is NaN or infinite, then the result is
        ''''  Double#MAX_EXPONENT"/> + 1.
        '''' + If the argument is zero or subnormal, then the result is
        ''''  Double#MIN_EXPONENT"/> -1.
        ''''  </summary>
        '''' <param name="d"> a {@code double} value </param>
        '''' <returns> the unbiased exponent of the argument
        '''' @since 1.6 </returns>
        'Public Function getExponent(d As Double) As Integer
        '    '        
        '    '         * Bitwise convert d to long, mask out exponent bits, shift
        '    '         * to the right and then subtract out double's bias adjust to
        '    '         * get true exponent value.
        '    '         
        '    Return CInt(Fix(((Double.doubleToRawLongBits(d) And sun.misc.DoubleConsts.EXP_BIT_MASK) >> (sun.misc.DoubleConsts.SIGNIFICAND_WIDTH - 1)) - sun.misc.DoubleConsts.EXP_BIAS))
        'End Function

        '''' <summary>
        '''' Returns the floating-point number adjacent to the first
        '''' argument in the direction of the second argument.  If both
        '''' arguments compare as equal the second argument is returned.
        '''' 
        '''' 
        '''' Special cases:
        '''' 
        '''' +  If either argument is a NaN, then NaN is returned.
        '''' 
        '''' +  If both arguments are signed zeros, {@code direction}
        '''' is returned unchanged (as implied by the requirement of
        '''' returning the second argument if the arguments compare as
        '''' equal).
        '''' 
        '''' +  If {@code start} is
        '''' &plusmn; Double#MIN_VALUE"/> and {@code direction}
        '''' has a value such that the result should have a smaller
        '''' magnitude, then a zero with the same sign as {@code start}
        '''' is returned.
        '''' 
        '''' +  If {@code start} is infinite and
        '''' {@code direction} has a value such that the result should
        '''' have a smaller magnitude,  Double#MAX_VALUE"/> with the
        '''' same sign as {@code start} is returned.
        '''' 
        '''' +  If {@code start} is equal to &plusmn;
        ''''  Double#MAX_VALUE"/> and {@code direction} has a
        '''' value such that the result should have a larger magnitude, an
        '''' infinity with same sign as {@code start} is returned.
        '''' 
        '''' </summary>
        '''' <param name="start">  starting floating-point value </param>
        '''' <param name="direction"> value indicating which of
        '''' {@code start}'s neighbors or {@code start} should
        '''' be returned </param>
        '''' <returns> The floating-point number adjacent to {@code start} in the
        '''' direction of {@code direction}.
        '''' @since 1.6 </returns>
        'Public Function nextAfter(start As Double, direction As Double) As Double
        '    '        
        '    '         * The cases:
        '    '         *
        '    '         * nextAfter(+infinity, 0)  == MAX_VALUE
        '    '         * nextAfter(+infinity, +infinity)  == +infinity
        '    '         * nextAfter(-infinity, 0)  == -MAX_VALUE
        '    '         * nextAfter(-infinity, -infinity)  == -infinity
        '    '         *
        '    '         * are naturally handled without any additional testing
        '    '         

        '    ' First check for NaN values
        '    If Java.lang.[Double].IsNaN(start) OrElse Java.lang.[Double].IsNaN(direction) Then
        '        ' return a NaN derived from the input NaN(s)
        '        Return start + direction
        '    ElseIf start = direction Then
        '        Return direction ' start > direction or start < direction
        '    Else
        '        ' Add +0.0 to get rid of a -0.0 (+0.0 + -0.0 => +0.0)
        '        ' then bitwise convert start to  java.lang.[Integer].
        '        Dim transducer As Long = Java.lang.[Double].doubleToRawLongBits(start + 0.0R)

        '        '            
        '        '             * IEEE 754 floating-point numbers are lexicographically
        '        '             * ordered if treated as signed- magnitude integers .
        '        '             * Since Java's integers are two's complement,
        '        '             * incrementing" the two's complement representation of a
        '        '             * logically negative floating-point value *decrements*
        '        '             * the signed-magnitude representation. Therefore, when
        '        '             * the integer representation of a floating-point values
        '        '             * is less than zero, the adjustment to the representation
        '        '             * is in the opposite direction than would be expected at
        '        '             * first .
        '        '             
        '        If direction > start Then ' Calculate next greater value
        '            transducer = transducer + (If(transducer >= 0L, 1L, -1L)) ' Calculate next lesser value
        '        Else
        '            Debug.Assert(direction < start)
        '            If transducer > 0L Then
        '                transducer -= 1
        '            Else
        '                If transducer < 0L Then
        '                    transducer += 1
        '                    '                    
        '                    '                     * transducer==0, the result is -MIN_VALUE
        '                    '                     *
        '                    '                     * The transition from zero (implicitly
        '                    '                     * positive) to the smallest negative
        '                    '                     * signed magnitude value must be done
        '                    '                     * explicitly.
        '                    '                     
        '                Else
        '                    transducer = sun.misc.DoubleConsts.SIGN_BIT_MASK Or 1L
        '                End If
        '            End If
        '        End If

        '        Return Java.lang.[Double].longBitsToDouble(transducer)
        '    End If
        'End Function

        '''' <summary>
        '''' Returns the floating-point number adjacent to the first
        '''' argument in the direction of the second argument.  If both
        '''' arguments compare as equal a value equivalent to the second argument
        '''' is returned.
        '''' 
        '''' 
        '''' Special cases:
        '''' 
        '''' +  If either argument is a NaN, then NaN is returned.
        '''' 
        '''' +  If both arguments are signed zeros, a value equivalent
        '''' to {@code direction} is returned.
        '''' 
        '''' +  If {@code start} is
        '''' &plusmn; Float#MIN_VALUE"/> and {@code direction}
        '''' has a value such that the result should have a smaller
        '''' magnitude, then a zero with the same sign as {@code start}
        '''' is returned.
        '''' 
        '''' +  If {@code start} is infinite and
        '''' {@code direction} has a value such that the result should
        '''' have a smaller magnitude,  Float#MAX_VALUE"/> with the
        '''' same sign as {@code start} is returned.
        '''' 
        '''' +  If {@code start} is equal to &plusmn;
        ''''  Float#MAX_VALUE"/> and {@code direction} has a
        '''' value such that the result should have a larger magnitude, an
        '''' infinity with same sign as {@code start} is returned.
        '''' 
        '''' </summary>
        '''' <param name="start">  starting floating-point value </param>
        '''' <param name="direction"> value indicating which of
        '''' {@code start}'s neighbors or {@code start} should
        '''' be returned </param>
        '''' <returns> The floating-point number adjacent to {@code start} in the
        '''' direction of {@code direction}.
        '''' @since 1.6 </returns>
        'Public Function nextAfter(start As Single, direction As Double) As Single
        '    '        
        '    '         * The cases:
        '    '         *
        '    '         * nextAfter(+infinity, 0)  == MAX_VALUE
        '    '         * nextAfter(+infinity, +infinity)  == +infinity
        '    '         * nextAfter(-infinity, 0)  == -MAX_VALUE
        '    '         * nextAfter(-infinity, -infinity)  == -infinity
        '    '         *
        '    '         * are naturally handled without any additional testing
        '    '         

        '    ' First check for NaN values
        '    If Float.IsNaN(start) OrElse Java.lang.[Double].IsNaN(direction) Then
        '        ' return a NaN derived from the input NaN(s)
        '        Return start + CSng(direction)
        '    ElseIf start = direction Then
        '        Return CSng(direction) ' start > direction or start < direction
        '    Else
        '        ' Add +0.0 to get rid of a -0.0 (+0.0 + -0.0 => +0.0)
        '        ' then bitwise convert start to  java.lang.[Integer].
        '        Dim transducer As Integer = Float.floatToRawIntBits(start + 0.0F)

        '        '            
        '        '             * IEEE 754 floating-point numbers are lexicographically
        '        '             * ordered if treated as signed- magnitude integers .
        '        '             * Since Java's integers are two's complement,
        '        '             * incrementing" the two's complement representation of a
        '        '             * logically negative floating-point value *decrements*
        '        '             * the signed-magnitude representation. Therefore, when
        '        '             * the integer representation of a floating-point values
        '        '             * is less than zero, the adjustment to the representation
        '        '             * is in the opposite direction than would be expected at
        '        '             * first.
        '        '             
        '        If direction > start Then ' Calculate next greater value
        '            transducer = transducer + (If(transducer >= 0, 1, -1)) ' Calculate next lesser value
        '        Else
        '            Debug.Assert(direction < start)
        '            If transducer > 0 Then
        '                transducer -= 1
        '            Else
        '                If transducer < 0 Then
        '                    transducer += 1
        '                    '                    
        '                    '                     * transducer==0, the result is -MIN_VALUE
        '                    '                     *
        '                    '                     * The transition from zero (implicitly
        '                    '                     * positive) to the smallest negative
        '                    '                     * signed magnitude value must be done
        '                    '                     * explicitly.
        '                    '                     
        '                Else
        '                    transducer = sun.misc.FloatConsts.SIGN_BIT_MASK Or 1
        '                End If
        '            End If
        '        End If

        '        Return Float.intBitsToFloat(transducer)
        '    End If
        'End Function

        '''' <summary>
        '''' Returns the floating-point value adjacent to {@code d} in
        '''' the direction of positive infinity.  This method is
        '''' semantically equivalent to {@code nextAfter(d,
        '''' java.lang.[Double].POSITIVE_INFINITY)}; however, a {@code nextUp}
        '''' implementation may run faster than its equivalent
        '''' {@code nextAfter} call.
        '''' 
        '''' Special Cases:
        '''' 
        '''' +  If the argument is NaN, the result is NaN.
        '''' 
        '''' +  If the argument is positive infinity, the result is
        '''' positive infinity.
        '''' 
        '''' +  If the argument is zero, the result is
        ''''  Double#MIN_VALUE"/>
        '''' 
        '''' 
        '''' </summary>
        '''' <param name="d"> starting floating-point value </param>
        '''' <returns> The adjacent floating-point value closer to positive
        '''' infinity.
        '''' @since 1.6 </returns>
        'Public Function nextUp(d As Double) As Double
        '    If Java.lang.[Double].IsNaN(d) OrElse d = Java.lang.[Double].PositiveInfinity Then
        '        Return d
        '    Else
        '        d += 0.0R
        '        Return Java.lang.[Double].longBitsToDouble(Double.doubleToRawLongBits(d) + (If(d >= 0.0R, +1L, -1L)))
        '    End If
        'End Function

        '''' <summary>
        '''' Returns the floating-point value adjacent to {@code f} in
        '''' the direction of positive infinity.  This method is
        '''' semantically equivalent to {@code nextAfter(f,
        '''' Float.POSITIVE_INFINITY)}; however, a {@code nextUp}
        '''' implementation may run faster than its equivalent
        '''' {@code nextAfter} call.
        '''' 
        '''' Special Cases:
        '''' 
        '''' +  If the argument is NaN, the result is NaN.
        '''' 
        '''' +  If the argument is positive infinity, the result is
        '''' positive infinity.
        '''' 
        '''' +  If the argument is zero, the result is
        ''''  Float#MIN_VALUE"/>
        '''' 
        '''' 
        '''' </summary>
        '''' <param name="f"> starting floating-point value </param>
        '''' <returns> The adjacent floating-point value closer to positive
        '''' infinity.
        '''' @since 1.6 </returns>
        'Public Function nextUp(f As Single) As Single
        '    If Float.IsNaN(f) OrElse f = sun.misc.FloatConsts.POSITIVE_INFINITY Then
        '        Return f
        '    Else
        '        f += 0.0F
        '        Return Float.intBitsToFloat(Float.floatToRawIntBits(f) + (If(f >= 0.0F, +1, -1)))
        '    End If
        'End Function

        '''' <summary>
        '''' Returns the floating-point value adjacent to {@code d} in
        '''' the direction of negative infinity.  This method is
        '''' semantically equivalent to {@code nextAfter(d,
        '''' java.lang.[Double].NEGATIVE_INFINITY)}; however, a
        '''' {@code nextDown} implementation may run faster than its
        '''' equivalent {@code nextAfter} call.
        '''' 
        '''' Special Cases:
        '''' 
        '''' +  If the argument is NaN, the result is NaN.
        '''' 
        '''' +  If the argument is negative infinity, the result is
        '''' negative infinity.
        '''' 
        '''' +  If the argument is zero, the result is
        '''' {@code -Double.MIN_VALUE}
        '''' 
        '''' 
        '''' </summary>
        '''' <param name="d">  starting floating-point value </param>
        '''' <returns> The adjacent floating-point value closer to negative
        '''' infinity.
        '''' @since 1.8 </returns>
        'Public Function nextDown(d As Double) As Double
        '    If Java.lang.[Double].IsNaN(d) OrElse d = Java.lang.[Double].NegativeInfinity Then
        '        Return d
        '    Else
        '        If d = 0.0 Then
        '            Return -Double.MIN_VALUE
        '        Else
        '            Return Java.lang.[Double].longBitsToDouble(Double.doubleToRawLongBits(d) + (If(d > 0.0R, -1L, +1L)))
        '        End If
        '    End If
        'End Function

        '''' <summary>
        '''' Returns the floating-point value adjacent to {@code f} in
        '''' the direction of negative infinity.  This method is
        '''' semantically equivalent to {@code nextAfter(f,
        '''' Float.NEGATIVE_INFINITY)}; however, a
        '''' {@code nextDown} implementation may run faster than its
        '''' equivalent {@code nextAfter} call.
        '''' 
        '''' Special Cases:
        '''' 
        '''' +  If the argument is NaN, the result is NaN.
        '''' 
        '''' +  If the argument is negative infinity, the result is
        '''' negative infinity.
        '''' 
        '''' +  If the argument is zero, the result is
        '''' {@code -Float.MIN_VALUE}
        '''' 
        '''' 
        '''' </summary>
        '''' <param name="f">  starting floating-point value </param>
        '''' <returns> The adjacent floating-point value closer to negative
        '''' infinity.
        '''' @since 1.8 </returns>
        'Public Function nextDown(f As Single) As Single
        '    If Float.IsNaN(f) OrElse f = Float.NegativeInfinity Then
        '        Return f
        '    Else
        '        If f = 0.0F Then
        '            Return -Float.MIN_VALUE
        '        Else
        '            Return Float.intBitsToFloat(Float.floatToRawIntBits(f) + (If(f > 0.0F, -1, +1)))
        '        End If
        '    End If
        'End Function

        '''' <summary>
        '''' Returns {@code d} &times;
        '''' 2<sup>{@code scaleFactor}</sup> rounded as if performed
        '''' by a single correctly rounded floating-point multiply to a
        '''' member of the double value set.  See the Java
        '''' Language Specification for a discussion of floating-point
        '''' value sets.  If the exponent of the result is between {@link
        '''' Double#MIN_EXPONENT} and  Double#MAX_EXPONENT"/>, the
        '''' answer is calculated exactly.  If the exponent of the result
        '''' would be larger than {@code java.lang.[Double].MAX_EXPONENT}, an
        '''' infinity is returned.  Note that if the result is subnormal,
        '''' precision may be lost; that is, when {@code scalb(x, n)}
        '''' is subnormal, {@code scalb(scalb(x, n), -n)} may not equal
        '''' _x_.  When the result is non-NaN, the result has the same
        '''' sign as {@code d}.
        '''' 
        '''' Special cases:
        '''' 
        '''' +  If the first argument is NaN, NaN is returned.
        '''' +  If the first argument is infinite, then an infinity of the
        '''' same sign is returned.
        '''' +  If the first argument is zero, then a zero of the same
        '''' sign is returned.
        '''' 
        '''' </summary>
        '''' <param name="d"> number to be scaled by a power of two. </param>
        '''' <param name="scaleFactor"> power of 2 used to scale {@code d} </param>
        '''' <returns> {@code d} &times; 2<sup>{@code scaleFactor}</sup>
        '''' @since 1.6 </returns>
        'Public Function scalb(d As Double, scaleFactor As Integer) As Double
        '    '        
        '    '         * This method does not need to be declared strictfp to
        '    '         * compute the same correct result on all platforms.  When
        '    '         * scaling up, it does not matter what order the
        '    '         * multiply-store operations are done; the result will be
        '    '         * finite or overflow regardless of the operation ordering.
        '    '         * However, to get the correct result when scaling down, a
        '    '         * particular ordering must be used.
        '    '         *
        '    '         * When scaling down, the multiply-store operations are
        '    '         * sequenced so that it is not possible for two consecutive
        '    '         * multiply-stores to return subnormal results.  If one
        '    '         * multiply-store result is subnormal, the next multiply will
        '    '         * round it away to zero.  This is done by first multiplying
        '    '         * by 2 ^ (scaleFactor % n) and then multiplying several
        '    '         * times by by 2^n as needed where n is the exponent of number
        '    '         * that is a covenient power of two.  In this way, at most one
        '    '         * real rounding error occurs.  If the double value set is
        '    '         * being used exclusively, the rounding will occur on a
        '    '         * multiply.  If the double-extended-exponent value set is
        '    '         * being used, the products will (perhaps) be exact but the
        '    '         * stores to d are guaranteed to round to the double value
        '    '         * set.
        '    '         *
        '    '         * It is _not_ a valid implementation to first multiply d by
        '    '         * 2^MIN_EXPONENT and then by 2 ^ (scaleFactor %
        '    '         * MIN_EXPONENT) since even in a strictfp program double
        '    '         * rounding on underflow could occur; e.g. if the scaleFactor
        '    '         * argument was (MIN_EXPONENT - n) and the exponent of d was a
        '    '         * little less than -(MIN_EXPONENT - n), meaning the final
        '    '         * result would be subnormal.
        '    '         *
        '    '         * Since exact reproducibility of this method can be achieved
        '    '         * without any undue performance burden, there is no
        '    '         * compelling reason to allow double rounding on underflow in
        '    '         * scalb.
        '    '         

        '    ' magnitude of a power of two so large that scaling a finite
        '    ' nonzero value by it would be guaranteed to over or
        '    ' underflow; due to rounding, scaling down takes takes an
        '    ' additional power of two which is reflected here
        '    Dim MAX_SCALE As Integer = sun.misc.DoubleConsts.MAX_EXPONENT + -sun.misc.DoubleConsts.MIN_EXPONENT + sun.misc.DoubleConsts.SIGNIFICAND_WIDTH + 1
        '    Dim exp_adjust As Integer = 0
        '    Dim scale_increment As Integer = 0
        '    Dim exp_delta As Double = Java.lang.[Double].NaN

        '    ' Make sure scaling factor is in a reasonable range

        '    If scaleFactor < 0 Then
        '        scaleFactor = sys.Max(scaleFactor, -MAX_SCALE)
        '        scale_increment = -512
        '        exp_delta = twoToTheDoubleScaleDown
        '    Else
        '        scaleFactor = sys.Min(scaleFactor, MAX_SCALE)
        '        scale_increment = 512
        '        exp_delta = twoToTheDoubleScaleUp
        '    End If

        '    ' Calculate (scaleFactor % +/-512), 512 = 2^9, using
        '    ' technique from "Hacker's Delight" section 10-2.
        '    Dim t As Integer = CInt(CUInt((scaleFactor >> 9 - 1)) >> 32 - 9)
        '    exp_adjust = ((scaleFactor + t) And (512 - 1)) - t

        '    d *= powerOfTwoD(exp_adjust)
        '    scaleFactor -= exp_adjust

        '    Do While scaleFactor <> 0
        '        d *= exp_delta
        '        scaleFactor -= scale_increment
        '    Loop
        '    Return d
        'End Function

        '''' <summary>
        '''' Returns {@code f} &times;
        '''' 2<sup>{@code scaleFactor}</sup> rounded as if performed
        '''' by a single correctly rounded floating-point multiply to a
        '''' member of the float value set.  See the Java
        '''' Language Specification for a discussion of floating-point
        '''' value sets.  If the exponent of the result is between {@link
        '''' Float#MIN_EXPONENT} and  Float#MAX_EXPONENT"/>, the
        '''' answer is calculated exactly.  If the exponent of the result
        '''' would be larger than {@code Float.MAX_EXPONENT}, an
        '''' infinity is returned.  Note that if the result is subnormal,
        '''' precision may be lost; that is, when {@code scalb(x, n)}
        '''' is subnormal, {@code scalb(scalb(x, n), -n)} may not equal
        '''' _x_.  When the result is non-NaN, the result has the same
        '''' sign as {@code f}.
        '''' 
        '''' Special cases:
        '''' 
        '''' +  If the first argument is NaN, NaN is returned.
        '''' +  If the first argument is infinite, then an infinity of the
        '''' same sign is returned.
        '''' +  If the first argument is zero, then a zero of the same
        '''' sign is returned.
        '''' 
        '''' </summary>
        '''' <param name="f"> number to be scaled by a power of two. </param>
        '''' <param name="scaleFactor"> power of 2 used to scale {@code f} </param>
        '''' <returns> {@code f} &times; 2<sup>{@code scaleFactor}</sup>
        '''' @since 1.6 </returns>
        'Public Function scalb(f As Single, scaleFactor As Integer) As Single
        '    ' magnitude of a power of two so large that scaling a finite
        '    ' nonzero value by it would be guaranteed to over or
        '    ' underflow; due to rounding, scaling down takes takes an
        '    ' additional power of two which is reflected here
        '    Dim MAX_SCALE As Integer = sun.misc.FloatConsts.MAX_EXPONENT + -sun.misc.FloatConsts.MIN_EXPONENT + sun.misc.FloatConsts.SIGNIFICAND_WIDTH + 1

        '    ' Make sure scaling factor is in a reasonable range
        '    scaleFactor = sys.Max(System.Math.Min(scaleFactor, MAX_SCALE), -MAX_SCALE)

        '    '        
        '    '         * Since + MAX_SCALE for float fits well within the double
        '    '         * exponent range and + float -> double conversion is exact
        '    '         * the multiplication below will be exact. Therefore, the
        '    '         * rounding that occurs when the double product is cast to
        '    '         * float will be the correctly rounded float result.  Since
        '    '         * all operations other than the final multiply will be exact,
        '    '         * it is not necessary to declare this method strictfp.
        '    '         
        '    Return CSng(CDbl(f) * powerOfTwoD(scaleFactor))
        'End Function

        '' Constants used in scalb
        'Friend twoToTheDoubleScaleUp As Double = powerOfTwoD(512)
        'Friend twoToTheDoubleScaleDown As Double = powerOfTwoD(-512)

        '''' <summary>
        '''' Returns a floating-point power of two in the normal range.
        '''' </summary>
        'Friend Function powerOfTwoD(n As Integer) As Double
        '    Assert(n >= sun.misc.DoubleConsts.MIN_EXPONENT AndAlso n <= sun.misc.DoubleConsts.MAX_EXPONENT)
        '    Return java.lang.[Double].longBitsToDouble(((CLng(n) + CLng(Fix(sun.misc.DoubleConsts.EXP_BIAS))) << (sun.misc.DoubleConsts.SIGNIFICAND_WIDTH - 1)) And sun.misc.DoubleConsts.EXP_BIT_MASK)
        'End Function

        '''' <summary>
        '''' Returns a floating-point power of two in the normal range.
        '''' </summary>
        'Friend Function powerOfTwoF(n As Integer) As Single
        '    Assert(n >= sun.misc.FloatConsts.MIN_EXPONENT AndAlso n <= sun.misc.FloatConsts.MAX_EXPONENT)
        '    Return Float.intBitsToFloat(((n + sun.misc.FloatConsts.EXP_BIAS) << (sun.misc.FloatConsts.SIGNIFICAND_WIDTH - 1)) And sun.misc.FloatConsts.EXP_BIT_MASK)
        'End Function
    End Module
End Namespace
