Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports sys = System.Math

''' <summary>
''' + 圆的半径大小直接与集合的大小相关
''' + 圆的交集部分的面积大小与两个集合之间的重合度相关
'''   1. 当没有重合度的时候，两个圆心距离为r1+r2，完全没有交集部分
'''   2. 当完全重合的时候，两个圆心距离为0，最小的集合的圆将会完全容纳在大一些的集合的圆之中
''' </summary>
Public Module VennPlot

    ''' <summary>
    ''' 绘制两个集合间的文氏图
    ''' </summary>
    ''' <param name="opacity">
    ''' 为了更加清楚的显示出交集区域，在这里会使用一个统一的透明度值
    ''' </param>
    ''' <returns></returns>
    Public Function Venn2(a As VennSet, b As VennSet,
                          Optional size$ = "3000,2600",
                          Optional margin$ = g.DefaultPadding,
                          Optional bg$ = "white",
                          Optional opacity# = 0.85) As GraphicsData
        Dim plotInternal =
            Sub(ByRef g As IGraphics, rectangle As GraphicsRegion)
                Dim region As Rectangle = rectangle.PlotRegion
                ' 计算两个圆的半径大小
                ' ra + rb = width
                Dim ra = a.Size / (a.Size + b.Size) * region.Width / 2
                Dim rb = b.Size / (a.Size + b.Size) * region.Width / 2
                ' 将交集大小转换为圆心的偏移量
                Dim offset = a.intersections(b.Name) / sys.Min(a.Size, b.Size) * sys.Min(ra, rb)
                Dim dx = (region.Width - (ra + rb + (ra + rb - offset))) / 2
                Dim x, y As Integer
                Dim fill As Brush

                ' 绘制代表两个集合的圆
                x = region.Left + dx + ra
                y = region.Top + (region.Height - ra) / 2
                fill = a.color.Opacity(opacity)

                Call g.DrawCircle(New PointF(x, y), ra, fill)

                x = region.Right - dx - rb
                y = region.Top + (region.Height - rb) / 2
                fill = b.color.Opacity(opacity)

                Call g.DrawCircle(New PointF(x, y), rb, fill)
            End Sub

        Return g.GraphicsPlots(size.SizeParser, margin, bg, plotInternal)
    End Function
End Module

Public Class VennSet : Implements INamedValue

    ''' <summary>
    ''' 对当前的这个集合的唯一标记字符串
    ''' </summary>
    ''' <returns></returns>
    Public Property Name As String Implements IKeyedEntity(Of String).Key

    ''' <summary>
    ''' 显示标题(可能不唯一)
    ''' </summary>
    ''' <returns></returns>
    Public Property Title As String
    ''' <summary>
    ''' 集合之中的元素数量
    ''' </summary>
    ''' <returns></returns>
    Public Property Size As Integer
    Public Property color As Brush

    ''' <summary>
    ''' 当前的这个集合与其他的集合之间的交集大小
    ''' </summary>
    ''' <returns></returns>
    Public Property intersections As Dictionary(Of String, Integer)

End Class