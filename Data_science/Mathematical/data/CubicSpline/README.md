# CubicSpline
A simple VB.NET tool demonstrating the use of cubic spline interpolation. This tool was adapted from the original work of @CrushedPixel : [CubicSplineDemo](https://github.com/CrushedPixel/CubicSplineDemo)

## How to use
Too simple!

```vbnet
Imports Microsoft.VisualBasic.Mathematical

Dim data As Point() ' = ...
Dim result = CubicSpline.RecalcSpline(data).ToArray
```