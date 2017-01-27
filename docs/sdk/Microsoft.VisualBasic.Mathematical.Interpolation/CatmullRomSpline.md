# CatmullRomSpline
_namespace: [Microsoft.VisualBasic.Mathematical.Interpolation](./index.md)_

Calculates interpolated point between two points using Catmull-Rom Spline

> https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline


### Methods

#### CatmullRomSpline
```csharp
Microsoft.VisualBasic.Mathematical.Interpolation.CatmullRomSpline.CatmullRomSpline(System.Collections.Generic.IEnumerable{System.Drawing.PointF},System.Double,System.Boolean)
```
Catmull-Rom splines are a family of cubic interpolating splines formulated such 
 that the tangent at each point **Pi** Is calculated using the previous And next 
 point on the spline

|Parameter Name|Remarks|
|--------------|-------|
|points|-|
|interpolationStep#|-|
|isPolygon|-|

> http://www.codeproject.com/Articles/747928/Spline-Interpolation-history-theory-and-implementa

#### PointOnCurve
```csharp
Microsoft.VisualBasic.Mathematical.Interpolation.CatmullRomSpline.PointOnCurve(System.Drawing.PointF,System.Drawing.PointF,System.Drawing.PointF,System.Drawing.PointF,System.Double)
```
Calculates interpolated point between two points using Catmull-Rom Spline

|Parameter Name|Remarks|
|--------------|-------|
|p0|First Point|
|p1|Second Point|
|p2|Third Point|
|p3|Fourth Point|
|t|
 Normalised distance between second and third point where the spline point will be calculated|


_returns: Calculated Spline Point_
> 
>  Points calculated exist on the spline between points two and three.


