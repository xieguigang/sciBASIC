Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Interpolation
Imports Microsoft.VisualBasic.Mathematical.Plots

Public Module CubicSplineTest

    Sub Test()
        Dim data#()() = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\data\Spline_Interpolation\duom2.txt" _
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
            .SaveAs("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\data\Spline_Interpolation\duom2-cubic-spline.png")

        result = CatmullRomSpline.CatmullRomSpline(points)

        Dim CRInterplot = result.FromPoints(
            lineColor:="orange",
            ptSize:=3,
            lineType:=DashStyle.Dot,
            lineWidth:=3,
            title:="duom2: Catmull-Rom Spline")

        Call Scatter.Plot({raw, CRInterplot}, size:=New Size(3000, 1400)) _
            .SaveAs("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\data\Spline_Interpolation\duom2-CatmullRomSpline.png")

        'result = CentripetalCatmullRomSpline.CatmulRom(points)

        'Dim CRInterplotCenter = result.FromPoints(
        '    lineColor:="lime",
        '    ptSize:=3,
        '    lineType:=DashStyle.Dot,
        '    lineWidth:=3,
        '    title:="duom2: Centripetal Catmull–Rom spline")

        'Call Scatter.Plot({raw, CRInterplotCenter}, size:=New Size(3000, 1400)) _
        '    .SaveAs("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\data\Spline_Interpolation\duom2-Centripetal-CatmullRom-Spline.png")


        result = B_Spline.Compute(points, 1.5, RESOLUTION:=100)

        Dim B_interplot = result.FromPoints(
            lineColor:="green",
            ptSize:=5,
            title:="duom2: B-spline",
            lineType:=DashStyle.Dot,
            lineWidth:=3)


        Call Scatter.Plot({raw, B_interplot}, size:=New Size(3000, 1400)) _
        .SaveAs("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\data\Spline_Interpolation\duom2-B-spline.png")


        Call Scatter.Plot({raw, CRInterplot, B_interplot, interplot}, size:=New Size(3000, 1400)) _
            .SaveAs("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\data\Spline_Interpolation\duom2-compares.png")


        Dim bsplines = {
            B_Spline.Compute(points, 0.5, RESOLUTION:=100) _
                    .FromPoints(lineColor:="green", ptSize:=5, title:="duom2: B-spline, 0.5 degree", lineWidth:=3),
            B_Spline.Compute(points, 1, RESOLUTION:=100) _
                    .FromPoints(lineColor:="skyblue", ptSize:=3, title:="duom2: B-spline, 1 degree", lineWidth:=2),
            B_Spline.Compute(points, 2, RESOLUTION:=100) _
                    .FromPoints(lineColor:="yellow", ptSize:=3, title:="duom2: B-spline, 2 degree", lineWidth:=2),
            B_Spline.Compute(points, 3, RESOLUTION:=100) _
                    .FromPoints(lineColor:="lime", ptSize:=3, title:="duom2: B-spline, 3 degree", lineWidth:=2),
            B_Spline.Compute(points, 4, RESOLUTION:=100) _
                    .FromPoints(lineColor:="orange", ptSize:=3, title:="duom2: B-spline, 4 degree", lineWidth:=2),
            B_Spline.Compute(points, 5, RESOLUTION:=100) _
                    .FromPoints(lineColor:="darkred", ptSize:=3, title:="duom2: B-spline, 5 degree", lineWidth:=2),
            B_Spline.Compute(points, 10, RESOLUTION:=100) _
                    .FromPoints(lineColor:="red", ptSize:=3, title:="duom2: B-spline, 10 degree", lineWidth:=2),
            B_Spline.Compute(points, 20, RESOLUTION:=100) _
                    .FromPoints(lineColor:="gray", ptSize:=3, title:="duom2: B-spline, 20 degree", lineWidth:=2),
            B_Spline.Compute(points, 30, RESOLUTION:=100) _
                    .FromPoints(lineColor:="lightgreen", ptSize:=3, title:="duom2: B-spline, 30 degree", lineWidth:=2)
        }

        Call Scatter.Plot(raw.Join(bsplines), size:=New Size(3000, 1400)) _
            .SaveAs("G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematical\data\Spline_Interpolation\duom2-B-splines.png")

        Pause()
    End Sub
End Module
