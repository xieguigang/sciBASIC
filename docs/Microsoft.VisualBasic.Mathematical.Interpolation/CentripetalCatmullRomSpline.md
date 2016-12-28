# CentripetalCatmullRomSpline
_namespace: [Microsoft.VisualBasic.Mathematical.Interpolation](./index.md)_

###### Centripetal Catmull–Rom spline
 
 In computer graphics, centripetal Catmull–Rom spline is a variant form of 
 Catmull-Rom spline formulated by Edwin Catmull and Raphael Rom according 
 to the work of Barry and Goldman. It is a type of interpolating spline 
 (a curve that goes through its control points) defined by four control points
 P0, P1, P2, P3, with the curve drawn only from P1 to P2.
 
 > https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline#cite_ref-1



### Methods

#### CatmulRom
```csharp
Microsoft.VisualBasic.Mathematical.Interpolation.CentripetalCatmullRomSpline.CatmulRom(System.Drawing.PointF,System.Drawing.PointF,System.Drawing.PointF,System.Drawing.PointF,System.Single,System.Single)
```
In computer graphics, centripetal Catmull–Rom spline is a variant form of 
 Catmull-Rom spline formulated by Edwin Catmull and Raphael Rom according 
 to the work of Barry and Goldman. It is a type of interpolating spline 
 (a curve that goes through its control points) defined by four control points
 P0, P1, P2, P3, with the curve drawn only from P1 to P2.

|Parameter Name|Remarks|
|--------------|-------|
|pa|four control points P0, P1, P2, P3, with the curve drawn only from P1 to P2.|
|alpha!|set from 0-1|
|amountOfPoints!|How many points you want on the curve|


_returns: points on the Catmull curve so we can visualize them_


