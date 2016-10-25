Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.Plots

Public Module CubicSplineTest

    Sub Test()
        Dim data#()() = "E:\GCModeller\src\runtime\visualbasic_App\Data_science\Mathematical\data\CubicSpline\duom2.txt" _
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

        Call Scatter.Plot({raw, interplot}, size:=New Size(3000, 1400)) _
            .SaveAs("./duom2.png")

        Pause()
    End Sub
End Module
