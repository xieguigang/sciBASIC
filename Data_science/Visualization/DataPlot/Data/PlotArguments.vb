Imports System.Drawing
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D

' ============================================================================
'  PlotArguments.vb - 热图绘制上下文
'
'  这两个类型（PlotArguments / DrawElements）原本定义在 Legacy\Models.vb 里，
'  但它们的命名空间是 Statistics.Heatmap，而且被 HCTreePlot、eggHTS 与 metaProfiler
'  真实引用（用来控制热图行/列元素与树状图的绘制范围）。删除 Legacy 目录时把它们
'  上移到 Data\ 下单独保留，避免破坏这些下游工程。
' ============================================================================

Namespace Statistics.Heatmap

    ''' <summary>热图绘制的布局参数</summary>
    Public Class PlotArguments

        ''' <summary>矩阵区域相对画布的左边界</summary>
        Public left!

        ''' <summary>
        ''' 绘制矩阵之中的方格在xy上面的步进值
        ''' </summary>
        Public ReadOnly Property dStep As SizeF
            Get
                Return New SizeF With {
                    .Width = matrixPlotRegion.Width / ColOrders.Length,
                    .Height = matrixPlotRegion.Height / RowOrders.Length
                }
            End Get
        End Property

        ''' <summary>
        ''' 矩阵区域的大小和位置
        ''' </summary>
        Public matrixPlotRegion As Rectangle
        ''' <summary>行 / 列标签对应的数据集</summary>
        Public levels As Dictionary(Of String, DataSet)
        ''' <summary>矩阵区域相对画布的上边界</summary>
        Public top!
        ''' <summary>色阶画刷</summary>
        Public colors As SolidBrush()
        ''' <summary>行顺序（绘制顺序）</summary>
        Public RowOrders$()
        ''' <summary>列顺序（绘制顺序）</summary>
        Public ColOrders$()

    End Class

    ''' <summary>
    ''' Draw a specific heatmap element
    ''' </summary>
    Public Enum DrawElements As Byte
        ''' <summary>
        ''' Draw nothing
        ''' </summary>
        None = 0
        ''' <summary>
        ''' Only draw the heatmap element on matrix row
        ''' </summary>
        Rows = 2
        ''' <summary>
        ''' Only draw the heatmap element on the column
        ''' </summary>
        Cols = 4
        ''' <summary>
        ''' Draw both row and column heatmap elements
        ''' </summary>
        Both = 8
    End Enum

    ''' <summary>绘制热图元素的回调</summary>
    ''' <param name="g"></param>
    ''' <param name="region"></param>
    ''' <param name="args"></param>
    Public Delegate Sub HowtoDoPlot(g As IGraphics, region As GraphicsRegion, args As PlotArguments)

End Namespace
