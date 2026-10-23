Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Plots.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

' ============================================================================
'  Bubble.vb - 气泡图门面（承接旧 Bubble.Plot 的调用语义）
'
'  旧模型的 PointData.value 就是气泡的第三维（大小），
'  PointData.tag 是气泡的文本标签，两者都直接映射到新引擎的 BubbleSeries。
' ============================================================================

''' <summary>气泡图门面</summary>
Public Module Bubble

    ''' <summary>
    ''' 绘制气泡图。
    ''' </summary>
    ''' <param name="data">序列数据（旧模型 <see cref="SerialData"/>，<c>PointData.value</c> 作为气泡大小）</param>
    ''' <param name="title">图表主标题</param>
    ''' <param name="xlabel">X 轴标题</param>
    ''' <param name="ylabel">Y 轴标题</param>
    ''' <param name="size">画布尺寸</param>
    ''' <param name="dpi">出图分辨率</param>
    ''' <param name="driver">绘图驱动</param>
    ''' <param name="theme">绘图主题，留空时使用浅色主题</param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(data As IEnumerable(Of SerialData),
                         Optional title$ = Nothing,
                         Optional xlabel$ = Nothing,
                         Optional ylabel$ = Nothing,
                         Optional size$ = "2800,2800",
                         Optional dpi% = 100,
                         Optional driver As Drivers = Drivers.Default,
                         Optional theme As PlotTheme = Nothing) As GraphicsData

        Dim sz As Size = size.SizeParser

        Using plt As New BubblePlot(sz.Width, sz.Height, If(theme, PlotTheme.Light()))
            plt.Title = If(title, "")
            plt.XLabel = If(xlabel, "")
            plt.YLabel = If(ylabel, "")

            Return plt.Plot(data.ToBubbleSeries(), sz, dpi, driver)
        End Using
    End Function
End Module
