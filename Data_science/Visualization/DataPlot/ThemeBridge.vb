' ============================================================================
'  ThemeBridge.vb - ggplot 旧 CSS 主题到 DataPlot PlotTheme 的桥接层
'
'  ggplot 的 theme 体系（Canvas.Theme）是一套 CSS 字符串字段模型，
'  而 DataPlot 的 PlotTheme 是强类型模型。这里把前者单向转换为后者，
'  使得 ggplot 在渐进迁移期间不需要重写 ggplotTheme.vb 的 CSS 写出逻辑。
'  转换过程中所有不可解析的字段都会安全回退到 PlotTheme 的默认值。
' ============================================================================

Imports System.Drawing
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Data.Plots.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Font = Microsoft.VisualBasic.Imaging.Font

''' <summary>
''' 旧 <see cref="Theme"/>（CSS 字符串模型）到 <see cref="PlotTheme"/>（强类型模型）的桥接器
''' </summary>
Public Module ThemeBridge

    ''' <summary>
    ''' 用于 em 单位字体换算的基准画布尺寸（与 ggplot 默认 2K 画布同比例）
    ''' </summary>
    Private ReadOnly BaseSize As New Size(3600, 2400)

    ''' <summary>把 ggplot 的 CSS 主题转换为 DataPlot 的强类型主题</summary>
    Public Function ToPlotTheme(theme As Theme) As PlotTheme
        Dim t As New PlotTheme(If(theme.padding, "padding: 70px 30px 70px 80px;"))
        Dim env As New CSSEnvirnment(BaseSize)

        ' ---- 背景与绘图区 ----
        t.BackgroundColor = translateColor(theme.background, Color.White)
        t.PlotAreaColor = translateColor(theme.gridFill, Color.White)

        ' ---- 轴线 / 网格 ----
        t.AxisColor = strokeColor(theme.axisStroke, Color.FromArgb(60, 60, 60))
        t.GridColor = strokeColor(theme.gridStrokeX, Color.FromArgb(220, 220, 220))
        t.AxisLineWidth = strokeWidth(theme.axisStroke, 1.0F)
        t.GridLineWidth = strokeWidth(theme.gridStrokeX, 0.7F)

        ' ---- 文本颜色 ----
        t.TitleColor = translateColor(theme.mainTextColor, Color.FromArgb(20, 20, 20))
        t.TextColor = fontColor(theme.axisTickCSS, translateColor(theme.mainTextColor, Color.FromArgb(40, 40, 40)))
        t.SubTitleColor = fontColor(theme.subtitleCSS, Color.FromArgb(90, 90, 90))

        ' ---- 图例 ----
        t.LegendBackgroundColor = translateColor(theme.legendBoxBackground, Color.FromArgb(250, 250, 250))
        t.LegendBorderColor = strokeColor(theme.legendBoxStroke, Color.FromArgb(200, 200, 200))
        t.BorderColor = strokeColor(theme.shapeStroke, Color.FromArgb(180, 180, 180))

        ' ---- 字体 ----
        t.TitleFont = getFont(env, theme.mainCSS, t.TitleFont)
        t.SubTitleFont = getFont(env, theme.subtitleCSS, t.SubTitleFont)
        t.AxisLabelFont = getFont(env, theme.axisLabelCSS, t.AxisLabelFont)
        t.TickLabelFont = getFont(env, theme.axisTickCSS, t.TickLabelFont)
        t.LegendFont = getFont(env, theme.legendLabelCSS, t.LegendFont)
        t.AnnotationFont = getFont(env, theme.tagCSS, t.AnnotationFont)

        ' ---- 尺寸与开关 ----
        t.MarkerSize = If(theme.pointSize > 0, theme.pointSize, t.MarkerSize)
        t.ShowGrid = theme.drawGrid
        t.ShowLegendBorder = True
        t.Palette = getPalette(theme.colorSet, t.Palette)

        Return t
    End Function

    ''' <summary>把 ggplot 的 CSS 主题转换为 3D 绘图所使用的 <see cref="Plot3D.Plot3DTheme"/></summary>
    Public Function ToPlot3DTheme(theme As Theme) As Plot3D.Plot3DTheme
        Return New Plot3D.Plot3DTheme With {
            .padding = If(theme.padding, "padding: 70px 30px 70px 80px;"),
            .background = If(theme.background, "white"),
            .axisStroke = If(theme.axisStroke, "stroke: black; stroke-width: 2px; stroke-dash: solid;"),
            .axisLabelCSS = If(theme.axisLabelCSS, "font-style: normal; font-size: 28;"),
            .tagCSS = If(theme.tagCSS, "font-style: normal; font-size: 16;"),
            .tagColor = If(theme.tagColor, "black"),
            .mainTextColor = If(theme.mainTextColor, "black"),
            .legendBoxStroke = If(theme.legendBoxStroke, "stroke: black; stroke-width: 1px; stroke-dash: solid;"),
            .drawLegend = theme.drawLegend,
            .drawLabels = theme.drawLabels
        }
    End Function

    Private Function translateColor(expr As String, fallback As Color) As Color
        If expr.StringEmpty Then Return fallback

        Try
            Return expr.TranslateColor
        Catch
            Return fallback
        End Try
    End Function

    ''' <summary>从 CSS stroke 字符串之中取出 stroke 颜色</summary>
    Private Function strokeColor(stroke As String, fallback As Color) As Color
        Dim m As Match = Regex.Match(If(stroke, ""), "stroke\s*:\s*([^;]+)", RegexOptions.IgnoreCase)

        If Not m.Success OrElse m.Groups(1).Value.StringEmpty Then Return fallback

        Return translateColor(m.Groups(1).Value.Trim, fallback)
    End Function

    ''' <summary>从 CSS stroke 字符串之中取出线宽</summary>
    Private Function strokeWidth(stroke As String, fallback As Single) As Single
        Dim m As Match = Regex.Match(If(stroke, ""), "stroke-width\s*:\s*(\d+(?:\.\d+)?)", RegexOptions.IgnoreCase)

        If Not m.Success Then Return fallback

        Dim w As Single

        If Single.TryParse(m.Groups(1).Value, w) AndAlso w > 0 Then
            Return w
        Else
            Return fallback
        End If
    End Function

    ''' <summary>从 CSS 字体字符串之中取出字体颜色</summary>
    Private Function fontColor(css As String, fallback As Color) As Color
        Dim m As Match = Regex.Match(If(css, ""), "color\s*:\s*([^;]+)", RegexOptions.IgnoreCase)

        If Not m.Success OrElse m.Groups(1).Value.StringEmpty Then Return fallback

        Return translateColor(m.Groups(1).Value.Trim, fallback)
    End Function

    Private Function getFont(env As CSSEnvirnment, css As String, fallback As Font) As Font
        If css.StringEmpty Then Return fallback

        Try
            Return env.GetFont(css)
        Catch
            Return fallback
        End Try
    End Function

    ''' <summary>
    ''' 解析 ggplot 的 colorSet：既支持直接的 html 颜色序列，
    ''' 也支持 Set1:c9 之类的调色板名称
    ''' </summary>
    Private Function getPalette(colorSet As String, fallback As Color()) As Color()
        If colorSet.StringEmpty Then Return fallback

        Try
            Return Designer.GetColors(colorSet, fallback.Length)
        Catch
            Return fallback
        End Try
    End Function
End Module