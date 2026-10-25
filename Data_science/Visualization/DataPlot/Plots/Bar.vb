Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports std = System.Math

' ============================================================================
'  Bar.vb - 条形图门面（承接旧 LevelBarplot / BiDirectionBarPlot 的调用语义）
'
'   · LevelPlot        —— 横向条形图，柱子按数值大小取色阶颜色（旧 LevelBarplot）
'   · BiDirectionPlot  —— 左右镜像的双向条形图（旧 BiDirectionBarPlot）
' ============================================================================

''' <summary>条形图门面</summary>
Public Module Bar

    ''' <summary>
    ''' 绘制「按数值取色」的横向条形图（旧 <c>LevelBarplot.Plot</c> 的等价实现）。
    ''' </summary>
    ''' <param name="data">分类名与数值</param>
    ''' <param name="title">图表主标题</param>
    ''' <param name="legendTitle">色阶图例标题（当前实现用逐柱配色代替连续色条，此参数仅保留语义）</param>
    ''' <param name="valueTitle">数值轴标题</param>
    ''' <param name="colorSchema">色阶方案名，例如 <c>jet</c> / <c>Set1:c6</c></param>
    ''' <param name="colorLevels">色阶级数</param>
    ''' <param name="maxLabelLength">分类标签的最大长度（超出会被截断并加省略号）</param>
    ''' <param name="trimLabel">是否截断过长的分类标签</param>
    ''' <param name="size">画布尺寸</param>
    ''' <param name="dpi">出图分辨率</param>
    ''' <param name="driver">绘图驱动</param>
    ''' <param name="theme">绘图主题，留空时使用浅色主题</param>
    ''' <returns></returns>
    Public Function LevelPlot(data As IEnumerable(Of NamedValue(Of Double)),
                              Optional title$ = Nothing,
                              Optional legendTitle$ = Nothing,
                              Optional valueTitle$ = Nothing,
                              Optional colorSchema$ = "jet",
                              Optional colorLevels% = 30,
                              Optional maxLabelLength% = 48,
                              Optional trimLabel As Boolean = True,
                              Optional size$ = "2700,2100",
                              Optional dpi% = 100,
                              Optional driver As Drivers = Drivers.Default,
                              Optional theme As PlotTheme = Nothing) As GraphicsData

        Dim values As NamedValue(Of Double)() = data.SafeQuery.ToArray

        If values.Length = 0 Then
            Throw New ArgumentException("The bar plot input data is empty!", NameOf(data))
        End If

        Dim sz As Size = size.SizeParser
        Dim colors As Color() = colorsOf(colorSchema, colorLevels)
        Dim lo As Double = values.Min(Function(v) v.Value)
        Dim hi As Double = values.Max(Function(v) v.Value)
        Dim serials As BarSerial() = values _
            .Select(Function(v)
                        Return New BarSerial With {
                            .Label = If(trimLabel, trim(v.Name, maxLabelLength), v.Name),
                            .Value = v.Value,
                            .Color = levelColor(v.Value, lo, hi, colors)
                        }
                    End Function) _
            .ToArray

        Using plt As New BarPlot(sz.Width, sz.Height, If(theme, PlotTheme.Light()))
            plt.Title = If(title, "")
            plt.XLabel = If(valueTitle, "")
            plt.Horizontal = True
            plt.Serials = serials

            Return plt.Plot(sz, dpi, driver)
        End Using
    End Function

    ''' <summary>
    ''' 绘制左右镜像的双向条形图（旧 <c>BiDirectionBarPlot</c> 的等价实现）。
    ''' </summary>
    ''' <param name="bar">双向数据：每个样本的两个因子取值</param>
    ''' <param name="colorUp">Factor1 的颜色</param>
    ''' <param name="colorDown">Factor2 的颜色</param>
    ''' <param name="title">图表主标题</param>
    ''' <param name="xlabel">数值轴标题</param>
    ''' <param name="size">画布尺寸</param>
    ''' <param name="dpi">出图分辨率</param>
    ''' <param name="driver">绘图驱动</param>
    ''' <param name="theme">绘图主题，留空时使用浅色主题</param>
    ''' <returns></returns>
    Public Function BiDirectionPlot(bar As BiDirectionData,
                                    colorUp As Color,
                                    colorDown As Color,
                                    Optional title$ = Nothing,
                                    Optional xlabel$ = Nothing,
                                    Optional size$ = "3000,2400",
                                    Optional dpi% = 100,
                                    Optional driver As Drivers = Drivers.Default,
                                    Optional theme As PlotTheme = Nothing) As GraphicsData

        If bar Is Nothing OrElse bar.samples.IsNullOrEmpty Then
            Throw New ArgumentException("The bidirectional bar data is empty!", NameOf(bar))
        End If

        Dim sz As Size = size.SizeParser
        Dim t As PlotTheme = If(theme, PlotTheme.Light()).Clone()
        Dim n As Integer = bar.samples.Length
        Dim matrix As Double(,) = New Double(1, n - 1) {}

        t.Palette = {colorUp, colorDown}

        For j As Integer = 0 To n - 1
            Dim sample As BarDataSample = bar.samples(j)
            Dim up As Double = If(sample.data.Length > 0, sample.data(0), 0)
            Dim [down] As Double = If(sample.data.Length > 1, sample.data(1), 0)

            ' 左向（Factor1）取负，右向（Factor2）取正，构成以 0 为对称轴的发散柱状图
            matrix(0, j) = -std.Abs(up)
            matrix(1, j) = std.Abs([down])
        Next

        Using plt As New BarPlot(sz.Width, sz.Height, t)
            plt.Title = If(title, "")
            plt.XLabel = If(xlabel, "")
            plt.Categories = bar.samples.Select(Function(s) s.tag).ToArray
            plt.SeriesNames = {bar.Factor1, bar.Factor2}
            plt.MultiValues = matrix
            plt.Horizontal = True
            plt.Diverging = True

            Return plt.Plot(sz, dpi, driver)
        End Using
    End Function

    ''' <summary>解析色阶方案名，失败时回退到 theme 调色板</summary>
    Private Function colorsOf(schema As String, levels As Integer) As Color()
        Try
            Dim colors As Color() = Designer.GetColors(schema, n:=std.Max(2, levels))

            If colors.IsNullOrEmpty Then
                Return Designer.GetColors("jet", n:=30)
            Else
                Return colors
            End If
        Catch
            Return Designer.GetColors("jet", n:=30)
        End Try
    End Function

    ''' <summary>按数值在值域中的位置取色阶内的颜色</summary>
    Private Function levelColor(value As Double, lo As Double, hi As Double, colors As Color()) As Color
        If hi <= lo Then
            Return colors(colors.Length \ 2)
        End If

        Dim t As Double = (value - lo) / (hi - lo)
        Dim idx As Integer = CInt(std.Round(t * (colors.Length - 1)))

        Return colors(std.Max(0, std.Min(colors.Length - 1, idx)))
    End Function

    ''' <summary>截断过长的标签</summary>
    Private Function trim(label As String, maxLen As Integer) As String
        If label Is Nothing Then Return ""
        If maxLen <= 3 OrElse label.Length <= maxLen Then Return label

        Return Mid(label, 1, maxLen) & "..."
    End Function
End Module
