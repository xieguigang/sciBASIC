Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.Types

Public Module ScatterHeatmap

    Public Function Compile(exp$) As Func(Of Double, Double, Double)
        With New Expression

            Call .SetVariable("x", 0)
            Call .SetVariable("y", 0)

            Dim func As SimpleExpression = .Compile(exp)

            Return Function(x, y)
                       Call .SetVariable("x", x)
                       Call .SetVariable("y", y)
                       Return func.Evaluate
                   End Function
        End With
    End Function

    <Extension>
    Public Function Plot(exp$, xrange As DoubleRange, yrange As DoubleRange,
                         Optional colorMap$ = "Spectral:c10",
                         Optional mapLevels% = 25,
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional legendTitle$ = "",
                         Optional legendFont As Font = Nothing,
                         Optional xsteps! = Single.NaN,
                         Optional ysteps! = Single.NaN) As Bitmap

        Dim fun As Func(Of Double, Double, Double) = Compile(exp)
        Return fun.Plot(
            xrange, yrange,
            colorMap,
            mapLevels,
            bg, size,
            legendTitle, legendFont,
            xsteps, ysteps)
    End Function

    ''' <summary>
    ''' steps步长值默认值为长度平分到每一个像素点
    ''' </summary>
    ''' <param name="fun"></param>
    ''' <param name="xrange"></param>
    ''' <param name="yrange"></param>
    ''' <param name="colorMap$">
    ''' Default using colorbrewer ``Spectral:c10`` schema.
    ''' </param>
    ''' <param name="size">3000, 2400</param>
    ''' <param name="xsteps!"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(fun As Func(Of Double, Double, Double),
                         xrange As DoubleRange,
                         yrange As DoubleRange,
                         Optional colorMap$ = "Spectral:c10",
                         Optional mapLevels% = 25,
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional legendTitle$ = "",
                         Optional legendFont As Font = Nothing,
                         Optional xsteps! = Single.NaN,
                         Optional ysteps! = Single.NaN,
                         Optional parallel As Boolean = False) As Bitmap

        If size.IsEmpty Then
            size = New Size(3000, 2400)
        End If

        Dim margin As New Size(400, 100)
        Dim offset As New Point(-300, 0)

        Return GraphicsPlots(
           size, margin,
           bg$,
           Sub(ByRef g, region)

               ' 返回来的数据
               Dim data As Point3D() = fun _
                   .__getData(region.PlotRegion.Size,
                              xrange, yrange,
                              xsteps, ysteps,
                              parallel)
               Dim scaler As New Scaling(data)
               Dim xf = scaler.XScaler(region.Size, region.Margin)
               Dim yf = scaler.YScaler(region.Size, region.Margin)
               Dim colors As SolidBrush() = Designer.GetBrushes(colorMap, mapLevels)
               Dim lv%() = data _
                  .Select(Function(z) CDbl(z.Z)) _
                  .GenerateMapping(mapLevels)

               Call g.DrawAxis(size, margin, scaler, False, offset)

               For i As Integer = 0 To data.Length - 1
                   Dim p As Point3D = data(i)
                   Dim c As SolidBrush = colors(
                       If(lv(i) = 0, 0,
                       If(lv(i) >= colors.Length,
                       colors.Length - 1, lv(i))))
                   Dim fill As New RectangleF(xf(p.X) + offset.X, yf(p.Y) + offset.Y, 1, 1)

                   Call g.FillRectangle(c, fill)
                   Call g.DrawRectangle(New Pen(c),
                                        fill.Left, fill.Top,
                                        fill.Width,
                                        fill.Height)
               Next

               ' Draw legends
               Dim legend As Bitmap = colors.ColorMapLegend(
                   haveUnmapped:=False,
                   min:=Math.Round(data.Min(Function(z) z.Z), 1),
                   max:=Math.Round(data.Max(Function(z) z.Z), 1),
                   title:=legendTitle,
                   titleFont:=legendFont)
               Dim lsize As Size = legend.Size
               Dim left% = size.Width - lsize.Width + 100
               Dim top% = size.Height / 3

               Call g.DrawImageUnscaled(legend, left, top)
           End Sub)
    End Function

    ''' <summary>
    ''' 返回去的数据是和<paramref name="size"/>每一个像素点相对应的
    ''' </summary>
    ''' <param name="fun"></param>
    ''' <param name="size"></param>
    ''' <param name="xrange"></param>
    ''' <param name="yrange"></param>
    ''' <param name="xsteps!"></param>
    ''' <param name="ysteps!"></param>
    ''' <param name="parallel">
    ''' 对于例如ODEs计算这类比较重度的计算，可以考虑在这里使用并行
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Private Function __getData(fun As Func(Of Double, Double, Double),
                               size As Size,
                               xrange As DoubleRange,
                               yrange As DoubleRange,
                               ByRef xsteps!,
                               ByRef ysteps!,
                               parallel As Boolean) As Point3D()

        If Single.IsNaN(xsteps) Then
            xsteps = xrange.Length / size.Width
        End If
        If Single.IsNaN(ysteps) Then
            ysteps = yrange.Length / size.Height
        End If

        ' x: a -> b
        ' 每一行数据都是y在发生变化
        Dim data As List(Of Point3D)() =
            DataProvider.Evaluate(
                fun, xrange, yrange,
                xsteps, ysteps,
                parallel).ToArray

        If data.Length > size.Width + 10 Then
            Dim stepDelta = data.Length / size.Width
            Dim splt = data.Split(stepDelta)

        Else ' 数据不足


        End If

        Return data.ToVector
    End Function
End Module
