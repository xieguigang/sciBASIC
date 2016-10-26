Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Interpolation
Imports Microsoft.VisualBasic.Mathematical.Plots

Public Module CubicSplineTest

    Sub Test()
        Dim data#()() = "E:\GCModeller\src\runtime\visualbasic_App\Data_science\Mathematical\data\Spline_Interpolation\duom2.txt" _
            .IterateAllLines _
            .ToArray(Function(s) Regex.Replace(s, "\s+", " ") _
                .Trim _
                .Split _
                .ToArray(AddressOf Val))
        Dim points As PointF() = data _
            .ToArray(Function(c) New PointF With {
                .X = c(Scan0),
                .Y = c(1)
            })
        Dim result = CubicSpline.RecalcSpline(points).ToArray

        Dim interplot = result.FromPoints(
            lineColor:="red",
            ptSize:=5,
            title:="duom2: CubicSpline.RecalcSpline",
            lineType:=DashStyle.Dash,
            lineWidth:=3)
        Dim raw = points.FromPoints(
            lineColor:="skyblue",
            ptSize:=40,
            title:="duom2: raw")

        Call Scatter.Plot({raw, interplot}, size:=New Size(3000, 1400)) _
            .SaveAs("E:\GCModeller\src\runtime\visualbasic_App\Data_science\Mathematical\data\Spline_Interpolation\duom2-cubic-spline.png")


        result = B_Spline.Compute(points, 1.5, RESOLUTION:=100)

        Dim B_interplot = result.FromPoints(
            lineColor:="green",
            ptSize:=5,
            title:="duom2: B-spline",
            lineType:=DashStyle.Dot,
            lineWidth:=3)

        Call Scatter.Plot({raw, B_interplot}, size:=New Size(3000, 1400)) _
            .SaveAs("E:\GCModeller\src\runtime\visualbasic_App\Data_science\Mathematical\data\Spline_Interpolation\duom2-B-spline.png")


        Call Scatter.Plot({raw, B_interplot, interplot}, size:=New Size(3000, 1400)) _
            .SaveAs("E:\GCModeller\src\runtime\visualbasic_App\Data_science\Mathematical\data\Spline_Interpolation\duom2-compares.png")

        Pause()
    End Sub
End Module
