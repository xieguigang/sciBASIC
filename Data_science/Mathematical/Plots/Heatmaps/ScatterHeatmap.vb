Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Mathematical

Public Module ScatterHeatmap

    ''' <summary>
    ''' steps步长值默认值为长度平分到每一个像素点
    ''' </summary>
    ''' <param name="fun"></param>
    ''' <param name="xrange"></param>
    ''' <param name="yrange"></param>
    ''' <param name="colorMap$">
    ''' Default using colorbrewer ``Spectral:c10`` schema.
    ''' </param>
    ''' <param name="xsteps!"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(fun As Func(Of Double, Double, Double),
                         xrange As DoubleRange,
                         yrange As DoubleRange,
                         Optional colorMap$ = "Spectral:c10",
                         Optional mapLevels% = 100,
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional legendTitle$ = "",
                         Optional legendFont As Font = Nothing,
                         Optional xsteps! = Single.NaN,
                         Optional ysteps! = Single.NaN) As Bitmap

        Return GraphicsPlots(
           size, margin,
           bg$,
           Sub(ByRef g, region)

               ' 返回来的数据
               Dim data As Point3D() = fun _
                   .__getData(region.PlotRegion.Size,
                              xrange, yrange,
                              xsteps, ysteps)
               Dim scaler As New Scaling(data)
               Dim xf = scaler.XScaler(region.Size, region.Margin)
               Dim yf = scaler.YScaler(region.Size, region.Margin)
               Dim colors As SolidBrush() = Designer.GetBrushes(colorMap, mapLevels)
               Dim lv = data.Select(Function(z) CDbl(z.Z)).GenerateMapping(mapLevels)

               Call g.DrawAxis(size, margin, scaler, False)

               For i As Integer = 0 To data.Length - 1
                   Dim p As Point3D = data(i)
                   Dim c As SolidBrush = colors(lv(i) - 1)

                   Call g.FillPie(
                        c,
                        xf(p.X), yf(p.Y), 1, 1,
                        0, 360)
               Next

               ' Draw legends
               Dim legend As Bitmap = colors.ColorMapLegend(
                   haveUnmapped:=False,
                   min:=Math.Round(data.Min(Function(z) z.Z), 1),
                   max:=Math.Round(data.Max(Function(z) z.Z), 1),
                   title:=legendTitle,
                   titleFont:=legendFont)
               Dim lsize As Size = legend.Size
               Dim lmargin As Integer = size.Width - size.Height + margin.Width

               Dim left% = size.Width - lmargin
               Dim top% = size.Height / 3

               Dim scale# = lmargin / lsize.Width
               Dim lh% = CInt(scale * (size.Height * 2 / 3))

               Call g.DrawImage(legend, left, top, lmargin, lh)
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
    ''' <returns></returns>
    <Extension>
    Private Function __getData(fun As Func(Of Double, Double, Double), size As Size, xrange As DoubleRange, yrange As DoubleRange, ByRef xsteps!, ByRef ysteps!) As Point3D()
        If Single.IsNaN(xsteps) Then
            xsteps = xrange.Length / size.Width
        End If
        If Single.IsNaN(ysteps) Then
            ysteps = yrange.Length / size.Height
        End If

        ' x: a -> b
        ' 每一行数据都是y在发生变化
        Dim data = DataProvider.Evaluate(fun, xrange, yrange, xsteps, ysteps).ToArray

        If data.Length > size.Width + 10 Then
            Dim stepDelta = data.Length / size.Width
            Dim splt = data.Split(stepDelta)

        Else ' 数据不足


        End If

        Return data.ToVector
    End Function
End Module
