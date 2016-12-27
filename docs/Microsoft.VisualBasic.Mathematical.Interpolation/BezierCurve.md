# BezierCurve
_namespace: [Microsoft.VisualBasic.Mathematical.Interpolation](./index.md)_

A Bezier curve is a parametric curve frequently used in computer graphics and related fields. 
 In vector graphics, Bezier curves are used to model smooth curves that can be scaled indefinitely. 
 There are many ways to construct a Bezier curve. This simple program uses the midpoint algorithm 
 of constructing a Bezier curve. To show the nature of the divide and conquer approach in the 
 algorithm, a recursive function has been used to implement the construction of the piece of 
 Bezier curve.

> 
>  http://www.codeproject.com/Articles/223159/Midpoint-Algorithm-Divide-and-Conquer-Method-for-D
>  


### Methods

#### __interpolation
```csharp
Microsoft.VisualBasic.Mathematical.Interpolation.BezierCurve.__interpolation(System.Double[],System.Int32)
```


|Parameter Name|Remarks|
|--------------|-------|
|X|-|
|iteration|-|


#### BezierSmoothInterpolation
```csharp
Microsoft.VisualBasic.Mathematical.Interpolation.BezierCurve.BezierSmoothInterpolation(System.Double[],System.Int32,System.Int32,System.Boolean)
```


|Parameter Name|Remarks|
|--------------|-------|
|data|-|
|parallel|并行版本的|
|windowSize|数据采样的窗口大小，默认大小是**`data`**的百分之1|

> 先对数据进行采样，然后插值，最后返回插值后的平滑曲线数据以用于下一步分析

#### CreateBezier
```csharp
Microsoft.VisualBasic.Mathematical.Interpolation.BezierCurve.CreateBezier(System.Drawing.PointF,System.Drawing.PointF,System.Drawing.PointF)
```
create a bezier curve

|Parameter Name|Remarks|
|--------------|-------|
|ctrl1|first initial point|
|ctrl2|second initial point|
|ctrl3|third initial point|


#### MidPoint
```csharp
Microsoft.VisualBasic.Mathematical.Interpolation.BezierCurve.MidPoint(System.Drawing.PointF,System.Drawing.PointF)
```
Find mid point

|Parameter Name|Remarks|
|--------------|-------|
|controlPoint1|first control point|
|controlPoint2|second control point|


#### PopulateBezierPoints
```csharp
Microsoft.VisualBasic.Mathematical.Interpolation.BezierCurve.PopulateBezierPoints(System.Drawing.PointF,System.Drawing.PointF,System.Drawing.PointF,System.Int32)
```
Recursivly call to construct the bezier curve with control points

|Parameter Name|Remarks|
|--------------|-------|
|ctrl1|first control point of bezier curve segment|
|ctrl2|second control point of bezier curve segment|
|ctrl3|third control point of bezier curve segment|
|currentIteration|the current interation of a branch|

> 
>  http://www.codeproject.com/Articles/223159/Midpoint-Algorithm-Divide-and-Conquer-Method-for-D
>  

#### ReCalculate
```csharp
Microsoft.VisualBasic.Mathematical.Interpolation.BezierCurve.ReCalculate(System.Drawing.PointF,System.Drawing.PointF,System.Drawing.PointF,System.Int32)
```
recreate the bezier curve.

|Parameter Name|Remarks|
|--------------|-------|
|ctrl1|first initial point|
|ctrl2|second initial point|
|ctrl3|third initial point|
|iteration|number of iteration of the algorithm|


_returns: the list of points in the curve_


### Properties

#### BezierPoints
store the list of points in the bezier curve
#### InitPointsList
store the list of initial points
#### Iterations
store the number of iterations
