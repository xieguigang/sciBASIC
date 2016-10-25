# CubicSpline
A simple VB.NET tool demonstrating the use of cubic spline interpolation. This tool was adapted from the original work of @CrushedPixel : [CubicSplineDemo](https://github.com/CrushedPixel/CubicSplineDemo)

## How to use
Too simple!

```vbnet
Imports Microsoft.VisualBasic.Mathematical

Dim data As Point() ' = ...
Dim result = CubicSpline.RecalcSpline(data).ToArray
```

## Example

```vbnet
Dim data#()() = "./duom2.txt" _
    .IterateAllLines _
    .ToArray(Function(s) Regex.Replace(s, "\s+", " ") _
        .Trim _
        .Split _
        .ToArray(AddressOf Val))
Dim points As Point() = data _
    .ToArray(Function(c) New Point With {
        .X = c(Scan0),
        .Y = c(1)
    })
Dim result = CubicSpline.RecalcSpline(points).ToArray

Dim interplot = result.FromPoints(
    lineColor:="red",
    ptSize:=15,
    title:="duom2: CubicSpline.RecalcSpline",
    lineType:=DashStyle.Dash,
    lineWidth:=3)
Dim raw = points.FromPoints(
    lineColor:="skyblue",
    ptSize:=40,
    title:="duom2: raw")

Call Scatter.Plot({raw, interplot}) _
    .SaveAs("./duom2.png")
```

![](./duom2.png)
