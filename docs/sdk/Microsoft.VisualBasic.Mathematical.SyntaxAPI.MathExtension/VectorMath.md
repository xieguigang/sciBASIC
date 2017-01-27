# VectorMath
_namespace: [Microsoft.VisualBasic.Mathematical.SyntaxAPI.MathExtension](./index.md)_





### Methods

#### BesselI
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.MathExtension.VectorMath.BesselI(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector,Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector,System.Boolean)
```
Bessel Functions of integer and fractional order, of first and second kind, J(nu) and Y(nu), 
 and Modified Bessel functions (of first and third kind), I(nu) and K(nu).

_returns: 
 Numeric vector with the (scaled, if expon.scaled = TRUE) values of the corresponding Bessel function.
 The length of the result is the maximum of the lengths of the parameters. All parameters are recycled to that length.
 _

#### Exp
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.MathExtension.VectorMath.Exp(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector)
```
log computes logarithms, by default natural logarithms, log10 computes common (i.e., base 10) logarithms, 
 and log2 computes binary (i.e., base 2) logarithms. 
 The general form log(x, base) computes logarithms with base base.
 log1p(x) computes log(1+x) accurately also for |x| << 1.
 exp computes the exponential function.
 expm1(x) computes exp(x) - 1 accurately also for |x| << 1.

|Parameter Name|Remarks|
|--------------|-------|
|x|a numeric or complex vector.|


#### Log
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.MathExtension.VectorMath.Log(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector,System.Double)
```


|Parameter Name|Remarks|
|--------------|-------|
|x|a numeric or complex vector.|
|base|a positive or complex number: the base with respect to which logarithms are computed. Defaults to e=exp(1).|


#### Order
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.MathExtension.VectorMath.Order(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector,System.Boolean,System.Boolean)
```
order returns a permutation which rearranges its first argument into ascending or descending order, breaking ties by further arguments. sort.list is the same, using only one argument.

#### Sort
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.MathExtension.VectorMath.Sort(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector,System.Boolean)
```
Sorting or Ordering Vectors
 Sort (or order) a vector or factor (partially) into ascending or descending order. For ordering along more than one variable, e.g., for sorting data frames, see order.

#### Sqrt
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.MathExtension.VectorMath.Sqrt(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector)
```
abs(x) computes the absolute value of x, sqrt(x) computes the (principal) square root of x, √{x}.

|Parameter Name|Remarks|
|--------------|-------|
|x|a numeric or complex vector or array.|


#### Trunc
```csharp
Microsoft.VisualBasic.Mathematical.SyntaxAPI.MathExtension.VectorMath.Trunc(Microsoft.VisualBasic.Mathematical.LinearAlgebra.Vector)
```
Rounding of Numbers

|Parameter Name|Remarks|
|--------------|-------|
|x|-|



