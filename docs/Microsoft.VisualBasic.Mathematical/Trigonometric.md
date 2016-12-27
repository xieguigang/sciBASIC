# Trigonometric
_namespace: [Microsoft.VisualBasic.Mathematical](./index.md)_





### Methods

#### Angle
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.Angle(System.Drawing.PointF)
```
计算结果为角度

|Parameter Name|Remarks|
|--------------|-------|
|p|-|


#### Arccos
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.Arccos(System.Double)
```
Inverse Cosine（反余弦） ``Arccos(X) = Atn(-X / Sqr(-X * X + 1)) + 2 * Atn(1)``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### Arccosec
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.Arccosec(System.Double)
```
Inverse Cosecant（反余割） ``Arccosec(X) = Atn(X / Sqr(X * X - 1)) + (Sgn(X) - 1) * (2 * Atn(1))``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### Arccotan
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.Arccotan(System.Double)
```
Inverse Cotangent（反余切） ``Arccotan(X) = Atn(X) + 2 * Atn(1)``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### Arcsec
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.Arcsec(System.Double)
```
Inverse Secant（反正割） ``Arcsec(X) = Atn(X / Sqr(X * X - 1)) + Sgn((X) - 1) * (2 * Atn(1))``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### Arcsin
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.Arcsin(System.Double)
```
Inverse Sine（反正弦） ``Arcsin(X) = Atn(X / Sqr(-X * X + 1))``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### Atn
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.Atn(System.Double)
```
Taylor Atan

|Parameter Name|Remarks|
|--------------|-------|
|x|-|

> Atan测试没有问题

#### Cosec
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.Cosec(System.Double)
```
Cosecant（余割） ``Cosec(X) = 1 / Sin(X)``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### Cotan
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.Cotan(System.Double)
```
Cotangent（余切） ``Cotan(X) = 1 / Tan(X)``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### GetAngleVector
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.GetAngleVector(System.Single,System.Double)
```


|Parameter Name|Remarks|
|--------------|-------|
|radian|``0 -> 2*@``F:System.Math.PI````|


#### HArccos
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.HArccos(System.Double)
```
Inverse Hyperbolic Cosine（反双曲余弦） ``HArccos(X) = Log(X + Sqr(X * X - 1))``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### HArccosec
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.HArccosec(System.Double)
```
Inverse Hyperbolic Cosecant（反双曲余割） ``HArccosec(X) = Log((Sgn(X) * Sqr(X * X + 1) + 1) / X)``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### HArccotan
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.HArccotan(System.Double)
```
Inverse Hyperbolic Cotangent（反双曲余切） ``HArccotan(X) = Log((X + 1) / (X - 1)) / 2``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### HArcsec
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.HArcsec(System.Double)
```
Inverse Hyperbolic Secant（反双曲正割） ``HArcsec(X) = Log((Sqr(-X * X + 1) + 1) / X)``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### HArcsin
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.HArcsin(System.Double)
```
Inverse Hyperbolic Sine（反双曲正弦） ``HArcsin(X) = Log(X + Sqr(X * X + 1))``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### Harctan
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.Harctan(System.Double)
```
Inverse Hyperbolic Tangent（反双曲正切） ``HArctan(X) = Log((1 + X) / (1 - X)) / 2``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### HCos
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.HCos(System.Double)
```
Hyperbolic Cosine（双曲余弦） ``HCos(X) = (Exp(X) + Exp(-X)) / 2``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### HCosec
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.HCosec(System.Double)
```
Hyperbolic Cosecant（双曲余割） ``HCosec(X) = 2 / (Exp(X) - Exp(-X))``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### HCotan
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.HCotan(System.Double)
```
Hyperbolic Cotangent（双曲余切） ``HCotan(X) = (Exp(X) + Exp(-X)) / (Exp(X) - Exp(-X))``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### HSec
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.HSec(System.Double)
```
Hyperbolic Secant（双曲正割） ``HSec(X) = 2 / (Exp(X) + Exp(-X))``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### HSin
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.HSin(System.Double)
```
Hyperbolic Sine（双曲正弦） ``HSin(X) = (Exp(X) - Exp(-X)) / 2``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### HTan
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.HTan(System.Double)
```
Hyperbolic Tangent（双曲正切） ``HTan(X) = (Exp(X) - Exp(-X)) / (Exp(X) + Exp(-X))``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### Sec
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.Sec(System.Double)
```
Secant（正割） ``Sec(X) = 1 / Cos(X)``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### toDegrees
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.toDegrees(System.Double)
```
Converts an angle measured in radians to an approximately
 equivalent angle measured in degrees. The conversion from
 radians to degrees is generally inexact; users should
 not expect {@code cos(toRadians(90.0))} to exactly
 equal {@code 0.0}.

|Parameter Name|Remarks|
|--------------|-------|
|angrad|   an angle, in radians |


_returns:   the measurement of the angle {@code angrad}
          in degrees.
 @since   1.2 _

#### toRadians
```csharp
Microsoft.VisualBasic.Mathematical.Trigonometric.toRadians(System.Double)
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

#### AtanPrecise
通过这个参数来控制计算精度，这个参数值越大，计算精度越高
