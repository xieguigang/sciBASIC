# LeastSquares
_namespace: [Microsoft.VisualBasic.Data.Bootstrapping](./index.md)_

曲线拟合类，只适用于线性拟合：
 
 + ``y = a*x + b``
 + ``y = a + a1*x + a2*x^2 + ... + an*x^n``



### Methods

#### getSeriesLength
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.LeastSquares.getSeriesLength(System.Collections.Generic.IEnumerable{System.Double},System.Collections.Generic.IEnumerable{System.Double})
```
获取两个vector的安全size

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|y|-|


_returns: 最小的一个长度_

#### LinearFit
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.LeastSquares.LinearFit(Microsoft.VisualBasic.Language.List{System.Double},Microsoft.VisualBasic.Language.List{System.Double})
```
直线拟合-一元回归,拟合的结果可以使用getFactor获取，或者使用getSlope获取斜率，getIntercept获取截距

|Parameter Name|Remarks|
|--------------|-------|
|x|观察值的x|
|y|观察值的y|


#### PolyFit
```csharp
Microsoft.VisualBasic.Data.Bootstrapping.LeastSquares.PolyFit(Microsoft.VisualBasic.Language.List{System.Double},Microsoft.VisualBasic.Language.List{System.Double},System.Int32)
```
多项式拟合，拟合y=a0+a1*x+a2*x^2+……+apoly_n*x^poly_n

|Parameter Name|Remarks|
|--------------|-------|
|x|观察值的x|
|y|观察值的y|
|poly_n|期望拟合的阶数，若poly_n=2，则y=a0+a1*x+a2*x^2|



