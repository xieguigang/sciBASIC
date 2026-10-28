Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Data.Plots.Plot3D
Imports Microsoft.VisualBasic.Data.Plots.Plot3D.Legend
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports std = System.Math

''' <summary>
''' 三维图表（散点 / 饼图 / 曲面）的渲染演示。
'''
''' <para>
''' 全部使用合成数据：3 组高斯簇散点、5 段占比、以及一个 sin·cos 混合曲面，
''' 不涉及任何真实研究数据。
''' </para>
''' </summary>
Module Plot3DDemo

    ''' <summary>
    ''' 依次渲染三个三维图表，并把 PNG 输出到 <paramref name="outDir"/>
    ''' </summary>
    ''' <param name="outDir">输出目录（一般为 <c>AppContext.BaseDirectory</c>）</param>
    Friend Sub Run(outDir As String)
        Call ImageDriver.Register()

        Call Scatter3DDemo(outDir.Trim("/"c, "\"c) & "-gdi-backend/")
        Call Pie3DDemo(outDir.Trim("/"c, "\"c) & "-gdi-backend/")
        Call Surface3DDemo(outDir.Trim("/"c, "\"c) & "-gdi-backend/")

        Call SkiaDriver.Register()

        Call Scatter3DDemo(outDir.Trim("/"c, "\"c) & "-skia-backend/")
        Call Pie3DDemo(outDir.Trim("/"c, "\"c) & "-skia-backend/")
        Call Surface3DDemo(outDir.Trim("/"c, "\"c) & "-skia-backend/")
    End Sub

    ' ------------------------------------------------------------------
    '  演示 1：三维散点图
    ' ------------------------------------------------------------------

    ''' <summary>三组高斯簇的三维散点图（含盒框、背板网格、刻度、图例与深度淡化）</summary>
    Private Sub Scatter3DDemo(outDir As String)
        Dim rnd As New Random(20261026)
        Dim serials As New List(Of Serial3D)
        Dim titles As String() = {"Cluster A", "Cluster B", "Cluster C"}
        Dim centers As Double()() = {
            New Double() {0, 0, 0},
            New Double() {6, 5, 4},
            New Double() {-4, 7, -3}
        }
        Dim colors As Color() = {
            ColorTranslator.FromHtml("#2166AC"),
            ColorTranslator.FromHtml("#B2182B"),
            ColorTranslator.FromHtml("#1E8449")
        }

        For k As Integer = 0 To 2
            Dim points As New List(Of NamedValue(Of Point3D))

            For i As Integer = 0 To 59
                points.Add(New NamedValue(Of Point3D) With {
                    .Name = "",
                    .Value = New Point3D(
                        centers(k)(0) + NextNormal(rnd) * 1.4,
                        centers(k)(1) + NextNormal(rnd) * 1.4,
                        centers(k)(2) + NextNormal(rnd) * 1.4)
                })
            Next

            serials.Add(New Serial3D With {
                .Title = titles(k),
                .Color = colors(k),
                .Shape = LegendStyles.Circle,
                .PointSize = 9,
                .Points = points.ToArray
            })
        Next

        Dim camera As New Camera(New Point3D(30, -30, 0)) With {
            .Screen = New Size(1600, 1200),
            .ViewDistance = 10000
        }

        Using g As GraphicsData = serials.Plot(
            camera:=camera,
            bg:="white",
            labX:="PC 1",
            labY:="PC 2",
            labZ:="PC 3",
            title:="Publication style 3D scatter",
            subTitle:="three synthetic gaussian clusters",
            showHull:=True,
            depthFade:=0.55,
            dpi:=200)

            Dim file As String = IO.Path.Combine(outDir, "plot3d-scatter.png")

            Call g.Save(file)

            Console.WriteLine($"3d scatter -> {file}")
        End Using
    End Sub

    ' ------------------------------------------------------------------
    '  演示 2：三维饼图
    ' ------------------------------------------------------------------

    ''' <summary>五段占比的三维饼图（含切片标签、图例与标题）</summary>
    Private Sub Pie3DDemo(outDir As String)
        Dim values As New List(Of NamedValue(Of Double)) From {
            New NamedValue(Of Double) With {.Name = "Group A", .Value = 32},
            New NamedValue(Of Double) With {.Name = "Group B", .Value = 24},
            New NamedValue(Of Double) With {.Name = "Group C", .Value = 18},
            New NamedValue(Of Double) With {.Name = "Group D", .Value = 15},
            New NamedValue(Of Double) With {.Name = "Group E", .Value = 11}
        }
        Dim palette As Color() = Designer.GetColors("Paired:c12")
        Dim slices As PieSlice() = PieSlice.AsSlices(values, palette)
        Dim camera As New Camera(New Point3D(35, 0, 0)) With {
            .Screen = New Size(1600, 1200),
            .ViewDistance = 10000
        }

        Using g As GraphicsData = slices.Plot3D(
            camera:=camera,
            title:="Publication style 3D pie",
            showLegend:=True,
            showLabels:=True,
            thickness:=0.22,
            explode:=0.06,
            dpi:=200)

            Dim file As String = IO.Path.Combine(outDir, "plot3d-pie.png")

            Call g.Save(file)

            Console.WriteLine($"3d pie     -> {file}")
            Console.WriteLine($"  slices   : {String.Join(", ", slices.Select(Function(s) s.ToString))}")
        End Using
    End Sub

    ' ------------------------------------------------------------------
    '  演示 3：三维曲面图
    ' ------------------------------------------------------------------

    ''' <summary>高斯峰 + 正弦褶皱曲面的三维曲面图（含盒框、刻度与色标）</summary>
    Private Sub Surface3DDemo(outDir As String)
        Dim f As Func(Of Double, Double, (Z#, c#)) =
            Function(x, y)
                ' 一个高斯峰叠加一点正弦褶皱，使 z 方向有足够的起伏来展示三维效果
                Dim z As Double = 8 * std.Exp(-(x * x + y * y) / 12) + std.Sin(x) * std.Cos(y)

                Return (z, z)
            End Function
        Dim camera As New Camera(New Point3D(28, -32, 0)) With {
            .Screen = New Size(1600, 1200),
            .ViewDistance = 10000
        }

        Using g As GraphicsData = f.Plot(
            xrange:=New ComponentModel.Ranges.Model.DoubleRange(-8, 8),
            yrange:=New ComponentModel.Ranges.Model.DoubleRange(-8, 8),
            camera:=camera,
            xn:=70,
            yn:=70,
            legendTitle:="z value",
            mapName:="Spectral:c10",
            mapLevels:=25,
            title:="Publication style 3D surface",
            labX:="X",
            labY:="Y",
            labZ:="z = f(x, y)",
            dpi:=200)

            Dim file As String = IO.Path.Combine(outDir, "plot3d-surface.png")

            Call g.Save(file)

            Console.WriteLine($"3d surface -> {file}")
        End Using
    End Sub

    ''' <summary>标准正态分布随机数（Box-Muller）</summary>
    Private Function NextNormal(rnd As Random) As Double
        Dim u1 As Double = 1.0 - rnd.NextDouble()
        Dim u2 As Double = rnd.NextDouble()

        Return std.Sqrt(-2 * std.Log(u1)) * std.Sin(2 * std.PI * u2)
    End Function
End Module
