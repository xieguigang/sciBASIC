# VBMathExtensions
_namespace: [Microsoft.VisualBasic.Mathematical](./index.md)_





### Methods

#### EuclideanDistance
```csharp
Microsoft.VisualBasic.Mathematical.VBMathExtensions.EuclideanDistance(System.Double[],System.Double[])
```


|Parameter Name|Remarks|
|--------------|-------|
|a|Point A|
|b|Point B|


#### Hypot
```csharp
Microsoft.VisualBasic.Mathematical.VBMathExtensions.Hypot(System.Double,System.Double)
```
sqrt(a^2 + b^2) without under/overflow.

|Parameter Name|Remarks|
|--------------|-------|
|a|-|
|b|-|


#### IsPowerOf2
```csharp
Microsoft.VisualBasic.Mathematical.VBMathExtensions.IsPowerOf2(System.Int32)
```
Checks if the specified integer is power of 2.

|Parameter Name|Remarks|
|--------------|-------|
|x|Integer number to check.|


_returns: Returns true if the specified number is power of 2.
 Otherwise returns false._

#### Log2
```csharp
Microsoft.VisualBasic.Mathematical.VBMathExtensions.Log2(System.Int32)
```
Get base of binary logarithm.

|Parameter Name|Remarks|
|--------------|-------|
|x|Source integer number.|


_returns: Power of the number (base of binary logarithm)._

#### LogN
```csharp
Microsoft.VisualBasic.Mathematical.VBMathExtensions.LogN(System.Double,System.Double)
```
以 N 为底的对数 ``LogN(X) = Log(X) / Log(N)``

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|N|-|


#### Max
```csharp
Microsoft.VisualBasic.Mathematical.VBMathExtensions.Max(System.Int32,System.Int32,System.Int32)
```
return the maximum of a, b and c

|Parameter Name|Remarks|
|--------------|-------|
|a|-|
|b|-|
|c|
 @return |


#### PI
```csharp
Microsoft.VisualBasic.Mathematical.VBMathExtensions.PI(System.Collections.Generic.IEnumerable{System.Double})
```
Continues multiply operations.(连续乘法)

|Parameter Name|Remarks|
|--------------|-------|
|data|-|


#### PoissonPDF
```csharp
Microsoft.VisualBasic.Mathematical.VBMathExtensions.PoissonPDF(System.Int32,System.Double)
```
Returns the PDF value at x for the specified Poisson distribution.

#### Pow2
```csharp
Microsoft.VisualBasic.Mathematical.VBMathExtensions.Pow2(System.Int32)
```
Calculates power of 2.

|Parameter Name|Remarks|
|--------------|-------|
|power|Power to raise in.|


_returns: Returns specified power of 2 in the case if power is in the range of
 [0, 30]. Otherwise returns 0._

#### RMS
```csharp
Microsoft.VisualBasic.Mathematical.VBMathExtensions.RMS(System.Collections.Generic.IEnumerable{System.Double})
```
Root mean square.(均方根)

#### seq
```csharp
Microsoft.VisualBasic.Mathematical.VBMathExtensions.seq(Microsoft.VisualBasic.Language.Value{System.Double},System.Double,System.Double)
```
[Sequence Generation] Generate regular sequences. seq is a standard generic with a default method.

|Parameter Name|Remarks|
|--------------|-------|
|From|
 the starting and (maximal) end values of the sequence. Of length 1 unless just from is supplied as an unnamed argument.
 |
|To|
 the starting and (maximal) end values of the sequence. Of length 1 unless just from is supplied as an unnamed argument.
 |
|By|number: increment of the sequence|


#### STD
```csharp
Microsoft.VisualBasic.Mathematical.VBMathExtensions.STD(System.Collections.Generic.IEnumerable{System.Single})
```
Standard Deviation

#### Sum
```csharp
Microsoft.VisualBasic.Mathematical.VBMathExtensions.Sum(System.Collections.Generic.IEnumerable{System.Boolean})
```
Logical true values are regarded as one, false values as zero. For historical reasons, NULL is accepted and treated as if it were integer(0).

|Parameter Name|Remarks|
|--------------|-------|
|bc|-|


#### WeighedAverage
```csharp
Microsoft.VisualBasic.Mathematical.VBMathExtensions.WeighedAverage(System.Collections.Generic.IEnumerable{System.Double},System.Double[])
```
请注意,**`data`**的元素数量必须要和**`weights`**的长度相等

|Parameter Name|Remarks|
|--------------|-------|
|data|-|
|weights|这个数组里面的值的和必须要等于1|



