Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Plots.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

' ============================================================================
'  Scatter.vb - 散点图门面（承接旧 Scatter.Plot / Scatter2D 的调用语义）
'
'  输入仍然是旧模型的 SerialData（逐点配色、逐点标签、逐点大小），
'  内部转换成新引擎的 Series 后交给 ScatterPlot 渲染，
'  因此可以直接切换 png / svg / pdf / postscript 输出。
' ============================================================================

''' <summary>散点 / 折线图门面</summary>
Public Module Scatter

    ''' <summary>
    ''' 绘制散点图（可选连线）。逐点配色会按颜色拆成多条序列，图例名称保留原序列名。
    ''' </summary>
    ''' <param name="serials">序列数据（旧模型 <see cref="SerialData"/>）</param>
    ''' <param name="title">图表主标题</param>
    ''' <param name="xlabel">X 轴标题</param>
    ''' <param name="ylabel">Y 轴标题</param>
    ''' <param name="drawLine">是否把相邻的点连成折线</param>
    ''' <param name="size">画布尺寸，例如 <c>3000,3000</c></param>
    ''' <param name="dpi">出图分辨率</param>
    ''' <param name="driver">绘图驱动（决定 png / svg / postscript / pdf 输出）</param>
    ''' <param name="theme">绘图主题，留空时使用浅色主题</param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(serials As IEnumerable(Of SerialData),
                         Optional title$ = Nothing,
                         Optional xlabel$ = Nothing,
                         Optional ylabel$ = Nothing,
                         Optional drawLine As Boolean = False,
                         Optional size$ = "3000,3000",
                         Optional dpi% = 100,
                         Optional driver As Drivers = Drivers.Default,
                         Optional theme As PlotTheme = Nothing) As GraphicsData

        Dim sz As Size = size.SizeParser

        Using plt As New ScatterPlot(sz.Width, sz.Height, If(theme, PlotTheme.Light()))
            plt.Title = If(title, "")
            plt.XLabel = If(xlabel, "")
            plt.YLabel = If(ylabel, "")

            Return plt.Plot(serials.ToSeries(drawLine), sz, dpi, driver)
        End Using
    End Function
End Module
