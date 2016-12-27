# CubicSpline
_namespace: [Microsoft.VisualBasic.Mathematical.Interpolation](./index.md)_

Cubic spline interpolation

> 
>  https://github.com/CrushedPixel/CubicSplineDemo
>  


### Methods

#### RecalcSpline
```csharp
Microsoft.VisualBasic.Mathematical.Interpolation.CubicSpline.RecalcSpline(System.Collections.Generic.IEnumerable{System.Drawing.PointF},System.Double)
```
三次样本曲线插值

|Parameter Name|Remarks|
|--------------|-------|
|source|原始数据点集合，请注意，这些数据点之间都是有顺序分别的|
|expected|所期望的数据点的个数|


#### RecalcSpline``1
```csharp
Microsoft.VisualBasic.Mathematical.Interpolation.CubicSpline.RecalcSpline``1(System.Collections.Generic.IEnumerable{``0},System.Func{System.Single,System.Single,System.Single,``0},System.Double)
```
应用于3维空间的点对象的三次插值

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|newPoint|如何进行点对象的创建工作？|
|expected#|所期望的数据点的个数|



