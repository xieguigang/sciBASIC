Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D

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
                         Optional xsteps! = Single.NaN,
                         Optional ysteps! = Single.NaN) As Bitmap

        Return GraphicsPlots(
           size, margin,
           bg$,
           Sub(ByRef g, region)

               Dim data As Point3D() = fun _
                   .__getData(region.PlotRegion.Size,
                              xrange, yrange,
                              xsteps, ysteps)

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

        Dim data = DataProvider.Evaluate(fun, xrange, yrange, xsteps, ysteps)

    End Function
End Module
