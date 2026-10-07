' ============================================================================
'  Plot3DTheme.vb - 3D 绘图主题（从旧 Plots g/Graphic.Theme 精简而来）
'
'  只保留 3D 散点图 / 3D 饼图绘制所需要的那一组样式参数，
'  其余的旧 Theme 字段在这个新的 DataPlot 引擎之中不再需要。
' ============================================================================

Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace Plot3D

    ''' <summary>
    ''' 3D 绘图（散点图、饼图）的主题样式参数
    ''' </summary>
    Public Class Plot3DTheme

        ''' <summary>画布页边距 CSS 表达式</summary>
        Public Property padding As String = g.DefaultPadding

        ''' <summary>画布背景色表达式</summary>
        Public Property background As String = "white"

        ''' <summary>坐标轴线画笔 CSS</summary>
        Public Property axisStroke As String = Stroke.AxisStroke

        ''' <summary>坐标轴标签字体 CSS</summary>
        Public Property axisLabelCSS As String = CSSFont.Win7Normal

        ''' <summary>数据点标签字体 CSS</summary>
        Public Property tagCSS As String = CSSFont.Win10Normal

        ''' <summary>数据点标签颜色</summary>
        Public Property tagColor As String = "black"

        ''' <summary>轴标签文本颜色</summary>
        Public Property mainTextColor As String = "black"

        ''' <summary>图例框描边 CSS</summary>
        Public Property legendBoxStroke As String = Stroke.StrongHighlightStroke

        ''' <summary>是否绘制图例</summary>
        Public Property drawLegend As Boolean = True

        ''' <summary>是否绘制数据点文本标签</summary>
        Public Property drawLabels As Boolean = True

    End Class
End Namespace