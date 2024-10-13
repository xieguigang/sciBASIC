#Region "Microsoft.VisualBasic::2a137f4d7aff4a089a8fec5dab0d92c6, Data_science\Visualization\Plots\VennPlot.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 213
    '    Code Lines: 128 (60.09%)
    ' Comment Lines: 55 (25.82%)
    '    - Xml Docs: 76.36%
    ' 
    '   Blank Lines: 30 (14.08%)
    '     File Size: 9.08 KB


    ' Module VennPlot
    ' 
    '     Function: Venn2, Venn3
    ' 
    '     Sub: fixSetCompleteness
    ' 
    ' Class VennSet
    ' 
    '     Properties: color, intersections, Name, Size, Title
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports std = System.Math

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
Imports FontStyle = System.Drawing.FontStyle
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
#End If

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
                          Optional opacity# = 0.85,
                          Optional strokeCSS$ = Stroke.AxisStroke,
                          Optional regionTitleFontCSS$ = CSSFont.Win7Large,
                          Optional ppi As Integer = 100) As GraphicsData

        Call {a, b}.fixSetCompleteness

        Dim plotInternal =
            Sub(ByRef g As IGraphics, rectangle As GraphicsRegion)
                Dim region As Rectangle = rectangle.PlotRegion
                Dim css As CSSEnvirnment = g.LoadEnvironment
                Dim regionTitleFont As Font = css.GetFont(CSSFont.TryParse(regionTitleFontCSS))
                Dim strokePen As Pen = css.GetPen(Stroke.TryParse(strokeCSS))

                ' 计算两个圆的半径大小
                ' ra + rb = width
                Dim ra = a.Size / (a.Size + b.Size) * region.Width / 2
                Dim rb = b.Size / (a.Size + b.Size) * region.Width / 2
                ' 将交集大小转换为圆心的偏移量
                Dim offset = a.intersections(b.Name) / std.Min(a.Size, b.Size) * std.Min(ra, rb)
                Dim dx = (region.Width - (ra + rb + (ra + rb - offset))) / 2
                Dim x, y As Integer
                Dim fill As Color

                ' 绘制代表两个集合的圆
                ' 集合a
                x = region.Left + dx + ra
                y = region.Top + (region.Height - 2 * ra) / 2 + ra
                fill = a.color.Opacity(opacity)

                Call g.DrawCircle(New PointF(x, y), fill, strokePen, ra)

                ' 集合b
                x = region.Right - dx - rb
                y = region.Top + (region.Height - 2 * rb) / 2 + rb
                fill = b.color.Opacity(opacity)

                Call g.DrawCircle(New PointF(x, y), fill, strokePen, rb)
            End Sub

        Return g.GraphicsPlots(size.SizeParser, margin, bg, plotInternal)
    End Function

    <Extension>
    Private Sub fixSetCompleteness(sets As VennSet())
        Dim setTable = sets.ToDictionary(Function(s) s.Name)

        For Each [set] As VennSet In sets
            For Each key As String In setTable.Keys.Where(Function(k) k <> [set].Name)
                If Not [set].intersections.ContainsKey(key) Then
                    Call [set].intersections.Add(key, setTable(key).intersections([set].Name))
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' 绘制三个集合之间的文氏图
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <param name="c"></param>
    ''' <param name="size$"></param>
    ''' <param name="margin$"></param>
    ''' <param name="bg$"></param>
    ''' <param name="opacity#"></param>
    ''' <param name="strokeCSS$"></param>
    ''' <param name="regionTitleFontCSS$"></param>
    ''' <returns></returns>
    Public Function Venn3(a As VennSet, b As VennSet, c As VennSet,
                          Optional size$ = "3000,2600",
                          Optional margin$ = g.DefaultPadding,
                          Optional bg$ = "white",
                          Optional opacity# = 0.85,
                          Optional strokeCSS$ = Stroke.AxisStroke,
                          Optional regionTitleFontCSS$ = CSSFont.Win7Large,
                          Optional ppi As Integer = 100) As GraphicsData

        Call {a, b, c}.fixSetCompleteness

        Dim plotInternal =
            Sub(ByRef g As IGraphics, rectangle As GraphicsRegion)
                Dim region As Rectangle = rectangle.PlotRegion
                Dim css As CSSEnvirnment = g.LoadEnvironment
                Dim strokePen As Pen = css.GetPen(Stroke.TryParse(strokeCSS))
                Dim regionTitleFont As Font = css.GetFont(CSSFont.TryParse(regionTitleFontCSS))
                ' 计算三个圆的半径大小
                ' ra + rb = width
                Dim maxTop = std.Max(a.Size, b.Size)
                Dim ra = a.Size / (a.Size + b.Size) * region.Width / 2
                Dim rb = b.Size / (a.Size + b.Size) * region.Width / 2
                Dim rc = c.Size / (c.Size + maxTop) * region.Height / 2

                ' 将交集大小转换为圆心的偏移量
                Dim offsetX = a.intersections(b.Name) / std.Min(a.Size, b.Size) * std.Min(ra, rb)
                Dim offsetY = std.Max(a.intersections(c.Name), b.intersections(c.Name)) / {a.Size, b.Size, c.Size}.Min * {ra, rb, rc}.Min
                Dim dx = (region.Width - (ra + rb + (ra + rb - offsetX))) / 2
                Dim dy = (region.Height - (maxTop + rc + (maxTop - offsetY))) / 2
                Dim x, y As Integer
                Dim fill As Color

                ' 绘制代表三个集合的圆
                ' 集合a
                x = region.Left + dx + ra
                y = region.Top + (region.Height - 2 * ra) / 2 + ra + dy
                fill = a.color.Opacity(opacity)

                Call g.DrawCircle(New PointF(x, y), fill, strokePen, ra)

                ' 集合b
                x = region.Right - dx - rb
                y = region.Top + (region.Height - 2 * rb) / 2 + rb + dy
                fill = b.color.Opacity(opacity)

                Call g.DrawCircle(New PointF(x, y), fill, strokePen, rb)

                ' 集合b
                x = (region.Width - 2 * rc) / 2
                y = region.Height - rc - dy
                fill = c.color.Opacity(opacity)

                Call g.DrawCircle(New PointF(x, y), fill, strokePen, rc)
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
    Public Property color As Color

    ''' <summary>
    ''' 当前的这个集合与其他的集合之间的交集大小
    ''' </summary>
    ''' <returns></returns>
    Public Property intersections As Dictionary(Of String, Integer)

End Class
