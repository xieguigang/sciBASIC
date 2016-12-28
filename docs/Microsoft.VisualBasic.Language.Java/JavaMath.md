# JavaMath
_namespace: [Microsoft.VisualBasic.Language.Java](./index.md)_

The class {@code Math} contains methods for performing basic
 numeric operations such as the elementary exponential, logarithm,
 square root, and trigonometric functions.
 
 Unlike some of the numeric methods of class
 {@code StrictMath}, all implementations of the equivalent
 functions of class {@code Math} are not defined to return the
 bit-for-bit same results. This relaxation permits
 better-performing implementations where strict reproducibility is
 not required.
 
 By default many of the {@code Math} methods simply call
 the equivalent method in {@code StrictMath} for their
 implementation. Code generators are encouraged to use
 platform-specific native libraries or microprocessor instructions,
 where available, to provide higher-performance implementations of
 {@code Math} methods. Such higher-performance
 implementations still must conform to the specification for
 {@code Math}.
 
 The quality of implementation specifications concern two
 properties, accuracy of the returned result and monotonicity of the
 method. Accuracy of the floating-point {@code Math} methods is
 measured in terms of _ulps_, units in the last place. For a
 given floating-point format, an #ulp(double) ulp of a
 specific real number value is the distance between the two
 floating-point values bracketing that numerical value. When
 discussing the accuracy of a method as a whole rather than at a
 specific argument, the number of ulps cited is for the worst-case
 error at any argument. If a method always has an error less than
 0.5 ulps, the method always returns the floating-point number
 nearest the exact result; such a method is _correctly
 rounded_. A correctly rounded method is generally the best a
 floating-point approximation can be; however, it is impractical for
 many floating-point methods to be correctly rounded. Instead, for
 the {@code Math} [Class], a larger error bound of 1 or 2 ulps is
 allowed for certain methods. Informally, with a 1 ulp error bound,
 when the exact result is a representable number, the exact result
 should be returned as the computed result; otherwise, either of the
 two floating-point values which bracket the exact result may be
 returned. For exact results large in magnitude, one of the
 endpoints of the bracket may be infinite. Besides accuracy at
 individual arguments, maintaining proper relations between the
 method at different arguments is also important. Therefore, most
 methods with more than 0.5 ulp errors are required to be
 _semi-monotonic_: whenever the mathematical function is
 non-decreasing, so is the floating-point approximation, likewise,
 whenever the mathematical function is non-increasing, so is the
 floating-point approximation. Not all approximations that have 1
 ulp accuracy will automatically meet the monotonicity requirements.
 
 
 The platform uses signed two's complement integer arithmetic with
 int and long primitive types. The developer should choose
 the primitive type to ensure that arithmetic operations consistently
 produce correct results, which in some cases means the operations
 will not overflow the range of values of the computation.
 The best practice is to choose the primitive type and algorithm to avoid
 overflow. In cases where the size is {@code int} or {@code long} and
 overflow errors need to be detected, the methods {@code addExact},
 {@code subtractExact}, {@code multiplyExact}, and {@code toIntExact}
 throw an {@code ArithmeticException} when the results overflow.
 For other arithmetic operations such as divide, absolute value,
 increment, decrement, and negation overflow occurs only with
 a specific minimum or maximum value and should be checked against
 the minimum or maximum as appropriate.
 
 @author unascribed
 @author Joseph D. Darcy
 @since JDK1.0



### Methods

#### abs
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.abs(System.Double)
```
Returns the absolute value of a {@code double} value.
 If the argument is not negative, the argument is returned.
 If the argument is negative, the negation of the argument is returned.
 Special cases:
 + If the argument is positive zero or negative zero, the result
 is positive zero.
 + If the argument is infinite, the result is positive infinity.
 + If the argument is NaN, the result is NaN.
 In other words, the result is the same as the value of the expression:
 {@code java.lang.[Double].longBitsToDouble((Double.doubleToLongBits(a)<<1)>>>1)}

|Parameter Name|Remarks|
|--------------|-------|
|a|   the argument whose absolute value is to be determined |


_returns:   the absolute value of the argument. _

#### acos
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.acos(System.Double)
```
Returns the arc cosine of a value; the returned angle is in the
 range 0.0 through _pi_. Special case:
 + If the argument is NaN or its absolute value is greater
 than 1, then the result is NaN.
 
 The computed result must be within 1 ulp of the exact result.
 Results must be semi-monotonic.

|Parameter Name|Remarks|
|--------------|-------|
|a|   the value whose arc cosine is to be returned. |


_returns:   the arc cosine of the argument. _

#### addExact
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.addExact(System.Int64,System.Int64)
```
Returns the sum of its arguments,
 throwing an exception if the result overflows a {@code long}.

|Parameter Name|Remarks|
|--------------|-------|
|x| the first value |
|y| the second value |


_returns:  the result _

#### asin
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.asin(System.Double)
```
Returns the arc sine of a value; the returned angle is in the
 range -_pi_/2 through _pi_/2. Special cases:
 + If the argument is NaN or its absolute value is greater
 than 1, then the result is NaN.
 + If the argument is zero, then the result is a zero with the
 same sign as the argument.
 
 The computed result must be within 1 ulp of the exact result.
 Results must be semi-monotonic.

|Parameter Name|Remarks|
|--------------|-------|
|a|   the value whose arc sine is to be returned. |


_returns:   the arc sine of the argument. _

#### atan
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.atan(System.Double)
```
Returns the arc tangent of a value; the returned angle is in the
 range -_pi_/2 through _pi_/2. Special cases:
 + If the argument is NaN, then the result is NaN.
 + If the argument is zero, then the result is a zero with the
 same sign as the argument.
 
 The computed result must be within 1 ulp of the exact result.
 Results must be semi-monotonic.

|Parameter Name|Remarks|
|--------------|-------|
|a|   the value whose arc tangent is to be returned. |


_returns:   the arc tangent of the argument. _

#### atan2
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.atan2(System.Double,System.Double)
```
Returns the angle _theta_ from the conversion of rectangular
 coordinates ({@code x}, {@code y}) to polar
 coordinates (r, _theta_).
 This method computes the phase _theta_ by computing an arc tangent
 of {@code y/x} in the range of -_pi_ to _pi_. Special
 cases:
 + If either argument is NaN, then the result is NaN.
 + If the first argument is positive zero and the second argument
 is positive, or the first argument is positive and finite and the
 second argument is positive infinity, then the result is positive
 zero.
 + If the first argument is negative zero and the second argument
 is positive, or the first argument is negative and finite and the
 second argument is positive infinity, then the result is negative zero.
 + If the first argument is positive zero and the second argument
 is negative, or the first argument is positive and finite and the
 second argument is negative infinity, then the result is the
 {@code double} value closest to _pi_.
 + If the first argument is negative zero and the second argument
 is negative, or the first argument is negative and finite and the
 second argument is negative infinity, then the result is the
 {@code double} value closest to -_pi_.
 + If the first argument is positive and the second argument is
 positive zero or negative zero, or the first argument is positive
 infinity and the second argument is finite, then the result is the
 {@code double} value closest to _pi_/2.
 + If the first argument is negative and the second argument is
 positive zero or negative zero, or the first argument is negative
 infinity and the second argument is finite, then the result is the
 {@code double} value closest to -_pi_/2.
 + If both arguments are positive infinity, then the result is the
 {@code double} value closest to _pi_/4.
 + If the first argument is positive infinity and the second argument
 is negative infinity, then the result is the {@code double}
 value closest to 3*_pi_/4.
 + If the first argument is negative infinity and the second argument
 is positive infinity, then the result is the {@code double} value
 closest to -_pi_/4.
 + If both arguments are negative infinity, then the result is the
 {@code double} value closest to -3*_pi_/4.
 
 The computed result must be within 2 ulps of the exact result.
 Results must be semi-monotonic.

|Parameter Name|Remarks|
|--------------|-------|
|y|   the ordinate coordinate |
|x|   the abscissa coordinate |


_returns:   the _theta_ component of the point
          (_r_, _theta_)
          in polar coordinates that corresponds to the point
          (_x_, _y_) in Cartesian coordinates. _

#### ceil
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.ceil(System.Double)
```
Returns the smallest (closest to negative infinity)
 {@code double} value that is greater than or equal to the
 argument and is equal to a mathematical java.lang.[Integer]. Special cases:
 + If the argument value is already equal to a
 mathematical integer, then the result is the same as the
 argument. + If the argument is NaN or an infinity or
 positive zero or negative zero, then the result is the same as
 the argument. + If the argument value is less than zero but
 greater than -1.0, then the result is negative zero. Note
 that the value of {@code Math.ceil(x)} is exactly the
 value of {@code -Math.floor(-x)}.

|Parameter Name|Remarks|
|--------------|-------|
|a|   a value. |


_returns:   the smallest (closest to negative infinity)
          floating-point value that is greater than or equal to
          the argument and is equal to a mathematical  java.lang.[Integer]. _

#### cos
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.cos(System.Double)
```
Returns the trigonometric cosine of an angle. Special cases:
 + If the argument is NaN or an infinity, then the
 result is NaN.
 
 The computed result must be within 1 ulp of the exact result.
 Results must be semi-monotonic.

|Parameter Name|Remarks|
|--------------|-------|
|a|   an angle, in radians. |


_returns:   the cosine of the argument. _

#### cosh
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.cosh(System.Double)
```
Returns the hyperbolic cosine of a {@code double} value.
 The hyperbolic cosine of _x_ is defined to be
 (_ex + e-x_)/2
 where _e_ is Math#E Euler's number"/>.
 
 Special cases:
 
 
 + If the argument is NaN, then the result is NaN.
 
 + If the argument is infinite, then the result is positive
 infinity.
 
 + If the argument is zero, then the result is {@code 1.0}.
 
 
 
 The computed result must be within 2.5 ulps of the exact result.

|Parameter Name|Remarks|
|--------------|-------|
|x| The number whose hyperbolic cosine is to be returned. |


_returns:   The hyperbolic cosine of {@code x}.
 @since 1.5 _

#### decrementExact
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.decrementExact(System.Int64)
```
Returns the argument decremented by one, throwing an exception if the
 result overflows a {@code long}.

|Parameter Name|Remarks|
|--------------|-------|
|a| the value to decrement |


_returns:  the result _

#### exp
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.exp(System.Double)
```
Returns Euler's number _e_ raised to the power of a
 {@code double} value. Special cases:
 + If the argument is NaN, the result is NaN.
 + If the argument is positive infinity, then the result is
 positive infinity.
 + If the argument is negative infinity, then the result is
 positive zero.
 
 The computed result must be within 1 ulp of the exact result.
 Results must be semi-monotonic.

|Parameter Name|Remarks|
|--------------|-------|
|a|   the exponent to raise _e_ to. |


_returns:   the value _e_{@code a},
          where _e_ is the base of the natural logarithms. _

#### floor
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.floor(System.Double)
```
Returns the largest (closest to positive infinity)
 {@code double} value that is less than or equal to the
 argument and is equal to a mathematical java.lang.[Integer]. Special cases:
 + If the argument value is already equal to a
 mathematical integer, then the result is the same as the
 argument. + If the argument is NaN or an infinity or
 positive zero or negative zero, then the result is the same as
 the argument.

|Parameter Name|Remarks|
|--------------|-------|
|a|   a value. |


_returns:   the largest (closest to positive infinity)
          floating-point value that less than or equal to the argument
          and is equal to a mathematical  java.lang.[Integer]. _

#### floorDiv
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.floorDiv(System.Int64,System.Int64)
```
Returns the largest (closest to positive infinity)
 {@code long} value that is less than or equal to the algebraic quotient.
 There is one special case, if the dividend is the
 Long#MIN_VALUE java.lang.[Long].MIN_VALUE"/> and the divisor is {@code -1},
 then integer overflow occurs and
 the result is equal to the {@code java.lang.[Long].MIN_VALUE}.
 
 Normal integer division operates under the round to zero rounding mode
 (truncation). This operation instead acts under the round toward
 negative infinity (floor) rounding mode.
 The floor rounding mode gives different results than truncation
 when the exact result is negative.
 
 For examples, see #floorDiv(int, int)"/>.

|Parameter Name|Remarks|
|--------------|-------|
|x| the dividend |
|y| the divisor |


_returns:  the largest (closest to positive infinity)
 {@code long} value that is less than or equal to the algebraic quotient. _

#### floorMod
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.floorMod(System.Int64,System.Int64)
```
Returns the floor modulus of the {@code long} arguments.
 
 The floor modulus is {@code x - (floorDiv(x, y) * y)},
 has the same sign as the divisor {@code y}, and
 is in the range of {@code -abs(y) < r < +abs(y)}.
 
 
 The relationship between {@code floorDiv} and {@code floorMod} is such that:
 
 + {@code floorDiv(x, y) * y + floorMod(x, y) == x}
 
 
 For examples, see #floorMod(int, int)"/>.

|Parameter Name|Remarks|
|--------------|-------|
|x| the dividend |
|y| the divisor |


_returns:  the floor modulus {@code x - (floorDiv(x, y) * y)} _

#### IEEEremainder
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.IEEEremainder(System.Double,System.Double)
```
Computes the remainder operation on two arguments as prescribed
 by the IEEE 754 standard.
 The remainder value is mathematically equal to
 ``f1 - f2`` x _n_,
 where _n_ is the mathematical integer closest to the exact
 mathematical value of the quotient {@code f1/f2}, and if two
 mathematical integers are equally close to {@code f1/f2},
 then _n_ is the integer that is even. If the remainder is
 zero, its sign is the same as the sign of the first argument.
 Special cases:
 + If either argument is NaN, or the first argument is infinite,
 or the second argument is positive zero or negative zero, then the
 result is NaN.
 + If the first argument is finite and the second argument is
 infinite, then the result is the same as the first argument.

|Parameter Name|Remarks|
|--------------|-------|
|f1|   the dividend. |
|f2|   the divisor. |


_returns:   the remainder when {@code f1} is divided by
          {@code f2}. _

#### incrementExact
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.incrementExact(System.Int64)
```
Returns the argument incremented by one, throwing an exception if the
 result overflows a {@code long}.

|Parameter Name|Remarks|
|--------------|-------|
|a| the value to increment |


_returns:  the result _

#### log
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.log(System.Double)
```
Returns the natural logarithm (base _e_) of a {@code double}
 value. Special cases:
 + If the argument is NaN or less than zero, then the result
 is NaN.
 + If the argument is positive infinity, then the result is
 positive infinity.
 + If the argument is positive zero or negative zero, then the
 result is negative infinity.
 
 The computed result must be within 1 ulp of the exact result.
 Results must be semi-monotonic.

|Parameter Name|Remarks|
|--------------|-------|
|a|   a value |


_returns:   the value ln {@code a}, the natural logarithm of
          {@code a}. _

#### log10
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.log10(System.Double)
```
Returns the base 10 logarithm of a {@code double} value.
 Special cases:
 
 + If the argument is NaN or less than zero, then the result
 is NaN.
 + If the argument is positive infinity, then the result is
 positive infinity.
 + If the argument is positive zero or negative zero, then the
 result is negative infinity.
 + If the argument is equal to 10_n_ for
 integer _n_, then the result is _n_.
 
 
 The computed result must be within 1 ulp of the exact result.
 Results must be semi-monotonic.

|Parameter Name|Remarks|
|--------------|-------|
|a|   a value |


_returns:   the base 10 logarithm of  {@code a}.
 @since 1.5 _

#### Log1m
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.Log1m(System.Double)
```
Computes log(1-x) without losing precision for small values of x.

#### log1p
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.log1p(System.Double)
```
Returns the natural logarithm of the sum of the argument and 1.
 Note that for small values {@code x}, the result of
 {@code log1p(x)} is much closer to the true result of ln(1
 + {@code x}) than the floating-point evaluation of
 {@code log(1.0+x)}.
 
 Special cases:
 
 
 
 + If the argument is NaN or less than -1, then the result is
 NaN.
 
 + If the argument is positive infinity, then the result is
 positive infinity.
 
 + If the argument is negative one, then the result is
 negative infinity.
 
 + If the argument is zero, then the result is a zero with the
 same sign as the argument.
 
 
 
 The computed result must be within 1 ulp of the exact result.
 Results must be semi-monotonic.

|Parameter Name|Remarks|
|--------------|-------|
|x|   a value |


_returns:  the value ln({@code x} + 1), the natural
 log of {@code x} + 1
 @since 1.5 _
> http://www.johndcook.com/csharp_log_one_plus_x.html

#### max
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.max(System.Int64,System.Int64)
```
Returns the greater of two {@code long} values. That is, the
 result is the argument closer to the value of
 Long#MAX_VALUE"/>. If the arguments have the same value,
 the result is that same value.

|Parameter Name|Remarks|
|--------------|-------|
|a|   an argument. |
|b|   another argument. |


_returns:   the larger of {@code a} and {@code b}. _

#### min
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.min(System.Int64,System.Int64)
```
Returns the smaller of two {@code long} values. That is,
 the result is the argument closer to the value of
 Long#MIN_VALUE"/>. If the arguments have the same
 value, the result is that same value.

|Parameter Name|Remarks|
|--------------|-------|
|a|   an argument. |
|b|   another argument. |


_returns:   the smaller of {@code a} and {@code b}. _

#### multiplyExact
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.multiplyExact(System.Int64,System.Int64)
```
Returns the product of the arguments,
 throwing an exception if the result overflows a {@code long}.

|Parameter Name|Remarks|
|--------------|-------|
|x| the first value |
|y| the second value |


_returns:  the result _

#### negateExact
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.negateExact(System.Int64)
```
Returns the negation of the argument, throwing an exception if the
 result overflows a {@code long}.

|Parameter Name|Remarks|
|--------------|-------|
|a| the value to negate |


_returns:  the result _

#### pow
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.pow(System.Double,System.Double)
```
Returns the value of the first argument raised to the power of the
 second argument. Special cases:
 
 + If the second argument is positive or negative zero, then the
 result is 1.0.
 + If the second argument is 1.0, then the result is the same as the
 first argument.
 + If the second argument is NaN, then the result is NaN.
 + If the first argument is NaN and the second argument is nonzero,
 then the result is NaN.
 
 + If
 
 + the absolute value of the first argument is greater than 1
 and the second argument is positive infinity, or
 + the absolute value of the first argument is less than 1 and
 the second argument is negative infinity,
 
 then the result is positive infinity.
 
 + If
 
 + the absolute value of the first argument is greater than 1 and
 the second argument is negative infinity, or
 + the absolute value of the
 first argument is less than 1 and the second argument is positive
 infinity,
 
 then the result is positive zero.
 
 + If the absolute value of the first argument equals 1 and the
 second argument is infinite, then the result is NaN.
 
 + If
 
 + the first argument is positive zero and the second argument
 is greater than zero, or
 + the first argument is positive infinity and the second
 argument is less than zero,
 
 then the result is positive zero.
 
 + If
 
 + the first argument is positive zero and the second argument
 is less than zero, or
 + the first argument is positive infinity and the second
 argument is greater than zero,
 
 then the result is positive infinity.
 
 + If
 
 + the first argument is negative zero and the second argument
 is greater than zero but not a finite odd integer, or
 + the first argument is negative infinity and the second
 argument is less than zero but not a finite odd integer,
 
 then the result is positive zero.
 
 + If
 
 + the first argument is negative zero and the second argument
 is a positive finite odd integer, or
 + the first argument is negative infinity and the second
 argument is a negative finite odd integer,
 
 then the result is negative zero.
 
 + If
 
 + the first argument is negative zero and the second argument
 is less than zero but not a finite odd integer, or
 + the first argument is negative infinity and the second
 argument is greater than zero but not a finite odd integer,
 
 then the result is positive infinity.
 
 + If
 
 + the first argument is negative zero and the second argument
 is a negative finite odd integer, or
 + the first argument is negative infinity and the second
 argument is a positive finite odd integer,
 
 then the result is negative infinity.
 
 + If the first argument is finite and less than zero
 
 + if the second argument is a finite even integer, the
 result is equal to the result of raising the absolute value of
 the first argument to the power of the second argument
 
 + if the second argument is a finite odd integer, the result
 is equal to the negative of the result of raising the absolute
 value of the first argument to the power of the second
 argument
 
 + if the second argument is finite and not an integer, then
 the result is NaN.
 
 
 + If both arguments are integers, then the result is exactly equal
 to the mathematical result of raising the first argument to the power
 of the second argument if that result can in fact be represented
 exactly as a {@code double} value.
 
 (In the foregoing descriptions, a floating-point value is
 considered to be an integer if and only if it is finite and a
 fixed point of the method #ceil ceil or,
 equivalently, a fixed point of the method {@link #floor
 floor}. A value is a fixed point of a one-argument
 method if and only if the result of applying the method to the
 value is equal to the value.)
 
 The computed result must be within 1 ulp of the exact result.
 Results must be semi-monotonic.

|Parameter Name|Remarks|
|--------------|-------|
|a|   the base. |
|b|   the exponent. |


_returns:   the value {@code a}{@code b}. _

#### random
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.random
```
Returns a {@code double} value with a positive sign, greater
 than or equal to {@code 0.0} and less than {@code 1.0}.
 Returned values are chosen pseudorandomly with (approximately)
 uniform distribution from that range.
 
 When this method is first called, it creates a single new
 pseudorandom-number generator, exactly as if by the expression
 
 {@code new java.util.Random()}
 
 This new pseudorandom-number generator is used thereafter for
 all calls to this method and is used nowhere else.
 
 This method is properly synchronized to allow correct use by
 more than one thread. However, if many threads need to generate
 pseudorandom numbers at a great rate, it may reduce contention
 for each thread to have its own pseudorandom-number generator.

_returns:   a pseudorandom {@code double} greater than or equal
 to {@code 0.0} and less than {@code 1.0}. _

#### sin
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.sin(System.Double)
```
Returns the trigonometric sine of an angle. Special cases:
 + If the argument is NaN or an infinity, then the
 result is NaN.
 + If the argument is zero, then the result is a zero with the
 same sign as the argument.
 
 The computed result must be within 1 ulp of the exact result.
 Results must be semi-monotonic.

|Parameter Name|Remarks|
|--------------|-------|
|a|   an angle, in radians. |


_returns:   the sine of the argument. _

#### sinh
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.sinh(System.Double)
```
Returns the hyperbolic sine of a {@code double} value.
 The hyperbolic sine of _x_ is defined to be
 (_ex - e-x_)/2
 where _e_ is Math#E Euler's number"/>.
 
 Special cases:
 
 
 + If the argument is NaN, then the result is NaN.
 
 + If the argument is infinite, then the result is an infinity
 with the same sign as the argument.
 
 + If the argument is zero, then the result is a zero with the
 same sign as the argument.
 
 
 
 The computed result must be within 2.5 ulps of the exact result.

|Parameter Name|Remarks|
|--------------|-------|
|x| The number whose hyperbolic sine is to be returned. |


_returns:   The hyperbolic sine of {@code x}.
 @since 1.5 _

#### sqrt
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.sqrt(System.Double)
```
Returns the correctly rounded positive square root of a
 {@code double} value.
 Special cases:
 + If the argument is NaN or less than zero, then the result
 is NaN.
 + If the argument is positive infinity, then the result is positive
 infinity.
 + If the argument is positive zero or negative zero, then the
 result is the same as the argument.
 Otherwise, the result is the {@code double} value closest to
 the true mathematical square root of the argument value.

|Parameter Name|Remarks|
|--------------|-------|
|a|   a value. |


_returns:   the positive square root of {@code a}.
          If the argument is NaN or less than zero, the result is NaN. _

#### subtractExact
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.subtractExact(System.Int64,System.Int64)
```
Returns the difference of the arguments,
 throwing an exception if the result overflows a {@code long}.

|Parameter Name|Remarks|
|--------------|-------|
|x| the first value |
|y| the second value to subtract from the first |


_returns:  the result _

#### tan
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.tan(System.Double)
```
Returns the trigonometric tangent of an angle. Special cases:
 + If the argument is NaN or an infinity, then the result
 is NaN.
 + If the argument is zero, then the result is a zero with the
 same sign as the argument.
 
 The computed result must be within 1 ulp of the exact result.
 Results must be semi-monotonic.

|Parameter Name|Remarks|
|--------------|-------|
|a|   an angle, in radians. |


_returns:   the tangent of the argument. _

#### tanh
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.tanh(System.Double)
```
Returns the hyperbolic tangent of a {@code double} value.
 The hyperbolic tangent of _x_ is defined to be
 (_ex - e-x_)/(_ex + e-x_),
 in other words, {@link Math#sinh
 sinh(_x_)}/ Math#cosh cosh(_x_)"/>. Note
 that the absolute value of the exact tanh is always less than
 1.
 
 Special cases:
 
 
 + If the argument is NaN, then the result is NaN.
 
 + If the argument is zero, then the result is a zero with the
 same sign as the argument.
 
 + If the argument is positive infinity, then the result is
 {@code +1.0}.
 
 + If the argument is negative infinity, then the result is
 {@code -1.0}.
 
 
 
 The computed result must be within 2.5 ulps of the exact result.
 The result of {@code tanh} for any finite input must have
 an absolute value less than or equal to 1. Note that once the
 exact result of tanh is within 1/2 of an ulp of the limit value
 of 1, correctly signed {@code 1.0} should be returned.

|Parameter Name|Remarks|
|--------------|-------|
|x| The number whose hyperbolic tangent is to be returned. |


_returns:   The hyperbolic tangent of {@code x}.
 @since 1.5 _

#### toDegrees
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.toDegrees(System.Double)
```
Converts an angle measured in radians to an approximately
 equivalent angle measured in degrees. The conversion from
 radians to degrees is generally inexact; users should
 _not_ expect {@code cos(toRadians(90.0))} to exactly
 equal {@code 0.0}.

|Parameter Name|Remarks|
|--------------|-------|
|angrad|   an angle, in radians |


_returns:   the measurement of the angle {@code angrad}
          in degrees.
 @since   1.2 _

#### toIntExact
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.toIntExact(System.Int64)
```
Returns the value of the {@code long} argument;
 throwing an exception if the value overflows an {@code int}.

|Parameter Name|Remarks|
|--------------|-------|
|value| the long value |


_returns:  the argument as an int _

#### toRadians
```csharp
Microsoft.VisualBasic.Language.Java.JavaMath.toRadians(System.Double)
```
Converts an angle measured in degrees to an approximately
 equivalent angle measured in radians. The conversion from
 degrees to radians is generally inexact.

|Parameter Name|Remarks|
|--------------|-------|
|angdeg|   an angle, in degrees |


_returns:   the measurement of the angle {@code angdeg}
          in radians.
 @since   1.2 _


### Properties

#### E
The {@code double} value that is closer than any other to
 _e_, the base of the natural logarithms.
#### PI
The {@code double} value that is closer than any other to
 _pi_, the ratio of the circumference of a circle to its
 diameter.
