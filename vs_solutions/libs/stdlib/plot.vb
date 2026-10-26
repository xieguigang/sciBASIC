Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Scripting.Runtime
' the skia based raster/svg/pdf driver lives in a separate assembly; aliased because the
' Microsoft.VisualBasic.Drawing namespace also defines Color/Image aliases
Imports Skia = Microsoft.VisualBasic.Drawing.SkiaDriver

''' <summary>
''' 数据可视化模块。
''' <para>
''' 每个绘图函数都一次性完成渲染并返回 <see cref="PlotImage"/>（脚本可见的图片门面，
''' 不暴露底层的 <c>GraphicsData</c>）；由 <see cref="save"/> 或
''' <c>PlotImage.save</c> 落盘（扩展名决定 png / svg / pdf 等格式）并释放资源，
''' 因此脚本不需要 ``Using``，也不需要手动 Dispose。
''' </para>
''' <para>
''' 绘图驱动（Skia）在第一次调用时自动注册，脚本不需要再调用 <c>SkiaDriver.Register()</c>。
''' </para>
''' </summary>
Public Module plot

    Private _driverReady As Boolean = False

    ''' <summary>
    ''' 散点图；给出 <paramref name="classId"/> 时按类别自动分组并分别配色。
    ''' </summary>
    Public Function scatter(x As Double(), y As Double(),
                            Optional classId As String() = Nothing,
                            Optional title As String = "scatter",
                            Optional subtitle As String = Nothing,
                            Optional xLabel As String = "x",
                            Optional yLabel As String = "y",
                            Optional theme As String = "nature",
                            Optional size As String = "800,600",
                            Optional dpi As Integer = 200) As PlotImage

        Dim serials As Series() = SeriesOf(x, y, classId)
        Dim canvas As Size = size.SizeParser

        Using plt As New ScatterPlot(canvas.Width, canvas.Height, PlotThemeOf(theme))
            Call ApplyLabels(plt, title, subtitle, xLabel, yLabel)

            Return New PlotImage(plt.Plot(serials, canvas, dpi))
        End Using
    End Function

    ''' <summary>折线图；给出 <paramref name="classId"/> 时按类别分成多条线。</summary>
    Public Function line(x As Double(), y As Double(),
                         Optional classId As String() = Nothing,
                         Optional title As String = "line",
                         Optional subtitle As String = Nothing,
                         Optional xLabel As String = "x",
                         Optional yLabel As String = "y",
                         Optional smooth As Boolean = False,
                         Optional theme As String = "nature",
                         Optional size As String = "800,600",
                         Optional dpi As Integer = 200) As PlotImage

        Dim serials As Series() = SeriesOf(x, y, classId)
        Dim canvas As Size = size.SizeParser

        Using plt As New LinePlot(canvas.Width, canvas.Height, PlotThemeOf(theme))
            Call ApplyLabels(plt, title, subtitle, xLabel, yLabel)

            plt.Smooth = smooth

            Return New PlotImage(plt.Plot(serials, canvas, dpi))
        End Using
    End Function

    ''' <summary>直方图。</summary>
    Public Function histogram(values As Double(),
                              Optional bins As Integer = 30,
                              Optional density As Boolean = False,
                              Optional title As String = "histogram",
                              Optional xLabel As String = "value",
                              Optional yLabel As String = "count",
                              Optional theme As String = "nature",
                              Optional size As String = "800,600",
                              Optional dpi As Integer = 200) As PlotImage

        Dim canvas As Size = size.SizeParser

        Using plt As New HistogramPlot(canvas.Width, canvas.Height, PlotThemeOf(theme))
            Call ApplyLabels(plt, title, Nothing, xLabel, yLabel)

            plt.Data = values
            plt.Bins = If(bins <= 0, 30, bins)
            plt.Density = density

            Return New PlotImage(plt.Plot(canvas, dpi))
        End Using
    End Function

    ''' <summary>
    ''' 热图；<paramref name="matrix"/> 为行主序的二维矩阵。
    ''' </summary>
    Public Function heatmap(matrix As Double()(),
                            Optional rowLabels As String() = Nothing,
                            Optional colLabels As String() = Nothing,
                            Optional title As String = "heatmap",
                            Optional palette As String = "plasma",
                            Optional showValues As Boolean = False,
                            Optional theme As String = "light",
                            Optional size As String = "800,600",
                            Optional dpi As Integer = 200) As PlotImage

        Dim canvas As Size = size.SizeParser

        Using plt As New HeatmapPlot(canvas.Width, canvas.Height, PlotThemeOf(theme))
            Call ApplyLabels(plt, title, Nothing, Nothing, Nothing)

            plt.Matrix = ToMatrix(matrix)
            plt.RowLabels = rowLabels
            plt.ColLabels = colLabels
            plt.ColorMap = PaletteOf(palette)
            plt.ShowValues = showValues

            Return New PlotImage(plt.Plot(canvas, dpi))
        End Using
    End Function

    ''' <summary>
    ''' 把图片写入文件（扩展名决定格式：png / jpg / bmp / svg / pdf），返回绝对路径。
    ''' <para>写盘后立即释放图片资源，脚本无需再关心 Dispose。</para>
    ''' </summary>
    Public Function save(image As PlotImage, file As String) As String
        If image Is Nothing OrElse image.graphics Is Nothing Then
            Throw New ArgumentNullException(NameOf(image), "no plotted graphics is given")
        End If

        Dim path As String = System.IO.Path.GetFullPath(file)
        Dim dir As String = System.IO.Path.GetDirectoryName(path)

        If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
            Call Directory.CreateDirectory(dir)
        End If

        Try
            Call image.graphics.Save(path)
        Finally
            image.graphics.Dispose()
        End Try

        Return path
    End Function

    ' ======================================================== 内部辅助

    ''' <summary>第一次绘图前注册 Skia 绘图驱动（可重复调用）。</summary>
    Private Sub EnsureDriver()
        If _driverReady Then
            Return
        End If

        Skia.Register()
        _driverReady = True
    End Sub

    Private Function SeriesOf(x As Double(), y As Double(), classId As String()) As Series()
        Call EnsureDriver()

        If classId Is Nothing Then
            Return {
                New Series With {
                    .Name = "data",
                    .X = x,
                    .Y = y,
                    .MarkerShape = MarkerShape.Circle
                }
            }
        End If

        If classId.Length <> x.Length Then
            Throw New ArgumentException($"the class labels length ({classId.Length}) should equals the sample count ({x.Length})", NameOf(classId))
        End If

        Return DataSerials(x, y, classId).ToArray
    End Function

    Private Sub ApplyLabels(plt As PlotEngine, title As String, subtitle As String, xLabel As String, yLabel As String)
        Call EnsureDriver()

        If Not String.IsNullOrEmpty(title) Then
            plt.Title = title
        End If
        If Not String.IsNullOrEmpty(subtitle) Then
            plt.SubTitle = subtitle
        End If
        If Not String.IsNullOrEmpty(xLabel) Then
            plt.XLabel = xLabel
        End If
        If Not String.IsNullOrEmpty(yLabel) Then
            plt.YLabel = yLabel
        End If
    End Sub

    ''' <summary>
    ''' 取绘图主题。因为本函数总是作为图表构造函数的实参被求值，
    ''' 在这里注册绘图驱动可以保证驱动一定早于 <c>New Plot(...)</c> 就位。
    ''' </summary>
    Private Function PlotThemeOf(name As String) As PlotTheme
        Call EnsureDriver()

        Select Case If(name, "").Trim.ToLower
            Case "light", "default" : Return PlotTheme.Light()
            Case "dark" : Return PlotTheme.Dark()
            Case "science" : Return PlotTheme.Science()
            Case "grayscale", "grey", "gray" : Return PlotTheme.Grayscale()
            Case Else : Return PlotTheme.Nature()
        End Select
    End Function

    Private Function PaletteOf(name As String) As ScalerPalette
        Select Case If(name, "").Trim.ToLower
            Case "viridis" : Return ScalerPalette.viridis
            Case "cividis" : Return ScalerPalette.cividis
            Case "mako" : Return ScalerPalette.mako
            Case "rocket" : Return ScalerPalette.rocket
            Case "inferno" : Return ScalerPalette.inferno
            Case "magma" : Return ScalerPalette.magma
            Case "turbo" : Return ScalerPalette.turbo
            Case "jet" : Return ScalerPalette.jet
            Case "rainbow" : Return ScalerPalette.rainbow
            Case "seismic" : Return ScalerPalette.seismic
            Case "gray", "grey" : Return ScalerPalette.gray
            Case "hot" : Return ScalerPalette.hot
            Case Else : Return ScalerPalette.Plasma
        End Select
    End Function

    ''' <summary>交错数组转换为 heatmap 需要的二维数组。</summary>
    Private Function ToMatrix(m As Double()()) As Double(,)
        If m Is Nothing OrElse m.Length = 0 Then
            Throw New ArgumentException("the matrix is empty", NameOf(m))
        End If

        Dim rows As Integer = m.Length
        Dim cols As Integer = m(0).Length
        Dim out(rows - 1, cols - 1) As Double

        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To cols - 1
                out(i, j) = m(i)(j)
            Next
        Next

        Return out
    End Function
End Module
