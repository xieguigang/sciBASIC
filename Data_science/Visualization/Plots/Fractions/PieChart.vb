#Region "Microsoft.VisualBasic::cca7f67a5ce7e1c264b2acee0f56e224, Data_science\Visualization\Plots\Fractions\PieChart.vb"

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

    '     Module PieChart
    ' 
    '         Function: Fractions, FromData, FromPercentages, Plot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports stdNum = System.Math

Namespace Fractions

    Public Module PieChart

        ''' <summary>
        ''' Plot pie chart
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="size"></param>
        ''' <param name="bg"></param>
        ''' <param name="legendAlt">不再绘制出传统的legend，而是将标签信息跟随pie的位置而变化</param>
        ''' <param name="legendBorder"></param>
        ''' <param name="minRadius">
        ''' 当这个参数值大于0的时候，除了扇形的面积会不同外，半径也会不同，这个参数指的是最小的半径
        ''' </param>
        ''' <param name="reorder">
        ''' 是否按照数据比例重新对数据排序？
        ''' +  0 : 不需要
        ''' +  1 : 从小到大排序
        ''' + -1 : 从大到小排序 
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' ''' 生成饼图的文本的布局位置
        ''' 
        ''' + 根据startAngle + 0.5 * sweepAngle来判断文本的位置
        ''' +   0 -  90  右下
        ''' +  90 - 180  左下
        ''' + 180 - 270  左上
        ''' + 270 - 360  右上
        ''' + 文本的位置应该是startAngle + 0.5 * sweepAngle的更加大的半径的一个圆的位置
        ''' </remarks>
        <Extension>
        Public Function Plot(data As IEnumerable(Of FractionData),
                             Optional size$ = "1600,1200",
                             Optional padding$ = g.DefaultPadding,
                             Optional bg$ = "white",
                             Optional valueLabel As ValueLabels = ValueLabels.Percentage,
                             Optional valueLabelStyle$ = CSSFont.Win7Bold,
                             Optional legendAlt As Boolean = True,
                             Optional legendFont$ = CSSFont.Win7LargeBold,
                             Optional legendBorder As Stroke = Nothing,
                             Optional minRadius As Single = -1,
                             Optional reorder% = 0,
                             Optional legendUnitSize$ = "60,50",
                             Optional shadowDistance# = 80,
                             Optional shadowAngle# = 35) As GraphicsData

            Dim margin As Padding = padding
            Dim font As Font = CSSFont.TryParse(legendFont)

#Const DEBUG = 0
            If reorder <> 0 Then
                If reorder > 0 Then
                    data = data.OrderBy(
                    Function(x) x.Percentage)
                Else
                    data = data.OrderByDescending(
                    Function(x) x.Percentage)
                End If
            End If

            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim gSize = region.PlotRegion.Size
                    Dim r# = stdNum.Min(gSize.Width, gSize.Height - shadowDistance) / 2 ' 最大的半径值
                    Dim topLeft As New Point(margin.Left, margin.Top)
                    Dim valueLabelFont As Font = CSSFont.TryParse(valueLabelStyle)
                    Dim layoutRect As Rectangle

                    If minRadius <= 0 OrElse CDbl(minRadius) >= r Then  ' 半径固定不变的样式

                        Dim start As New f64
                        Dim sweep As New f64
                        Dim alpha As Single, pt As PointF
                        Dim labelSize As SizeF
                        Dim label$
                        Dim br As SolidBrush
                        Dim centra As Point

                        layoutRect = New Rectangle(topLeft, New Size(r * 2, r * 2))
                        centra = layoutRect.Centre

                        ' 首先需要进行阴影的绘制
                        With topLeft.MovePoint(shadowDistance, shadowAngle)
                            Dim circle As New GraphicsPath

                            Call circle.AddEllipse(.X, .Y, CSng(r * 2), CSng(r * 2))
                            Call circle.CloseAllFigures()
                            Call Shadow.DropdownShadows(g, polygon:=circle)
                        End With

                        ' 填充浅灰色底层
                        Call g.FillPie(Brushes.LightGray, layoutRect, 0, 360)

                        For Each x As FractionData In data
                            br = New SolidBrush(x.Color)
                            Call g.FillPie(br, layoutRect,
                                           CSng(start = ((+start) + (sweep = CSng(360 * x.Percentage)))) - CSng(sweep.Value),
                                           CSng(sweep))

                            alpha = (+start) - (+sweep / 2)
                            ' 在这里r/1.5是因为这些百分比的值的标签需要显示在pie的内部
                            pt = (r / 1.5, alpha).ToCartesianPoint()
                            pt = New PointF(pt.X + centra.X, pt.Y + centra.Y)
                            label = x.GetValueLabel(valueLabel)
                            labelSize = g.MeasureString(label, valueLabelFont)
                            pt = New Point(pt.X - labelSize.Width / 2, pt.Y)

                            Call g.DrawString(label, valueLabelFont, Brushes.White, pt)

                            If Not legendAlt Then

                                ' 标签文本信息跟随pie的值而变化的
                                Dim layout As New PointF With {
                                    .X = (r * 1.15 * stdNum.Cos((start / 360) * (2 * stdNum.PI))) + centra.X,
                                    .Y = (r * 1.15 * stdNum.Sin((start / 360) * (2 * stdNum.PI))) + centra.Y
                                }

                                labelSize = g.MeasureString(x.Name, font)

                                If layout.X < centra.X Then
                                    ' 在左边，则需要剪掉size的width
                                    layout = New PointF(layout.X - labelSize.Width, layout.Y)
                                End If

                                g.DrawString(x.Name, font, Brushes.Black, layout)

                                ' 还需要绘制标签文本和pie的连接线
                                With (r, alpha).ToCartesianPoint()
                                    pt = New PointF(centra.X + .X, centra.Y + .Y)
                                End With

                                ' 绘制pt和layout之间的连接线
                                g.DrawLine(Pens.Gray, pt, layout)
                            End If
                        Next
                    Else  ' 半径也会有变化
                        Dim a As New Value(Of Single)
                        Dim sweep! = 360 / data.Count
                        Dim maxp# = data.Max(Function(x) x.Percentage)
#If DEBUG Then
                             Dim list As New List(Of Rectangle)
#End If
                        For Each x As FractionData In data
                            Dim r2# = minRadius + (r - minRadius) * (x.Percentage / maxp)
                            Dim vTopleft As New Point(gSize.Width / 2 - r2, gSize.Height / 2 - r2)
                            Dim rect As New Rectangle(vTopleft, New Size(r2 * 2, r2 * 2))
                            Dim br As New SolidBrush(x.Color)

                            Call g.FillPie(br, rect, (a = (a.Value + sweep)), sweep)
#If DEBUG Then
                                 list += rect
#End If
                        Next
#If DEBUG Then
                             For Each rect In list
                                 Call g.DrawRectangle(Pens.Red, rect)
                             Next
#End If
                    End If

                    If legendAlt Then
                        Dim maxL = g.MeasureString(data.MaxLengthString(Function(x) x.Name), font).Width
                        Dim left = layoutRect.Right + margin.Left
                        Dim legends As New List(Of Legend)
                        Dim d = font.Size
                        Dim height! = (d + g.MeasureString("1", font).Height) * data.Count
                        ' Excel之中的饼图的示例样式位置为默认右居中的
                        Dim top = (gSize.Height - height) / 2 - margin.Top

                        For Each x As FractionData In data
                            legends += New Legend With {
                                .color = x.Color.RGBExpression,
                                .style = LegendStyles.Square,
                                .title = x.Name,
                                .fontstyle = legendFont
                            }
                        Next

                        Call g.DrawLegends(
                            topLeft:=New Point(left, top),
                            legends:=legends,
                            gSize:=legendUnitSize,
                            d:=d,
                            shapeBorder:=legendBorder
                        )
                    End If
                End Sub

            Return g.GraphicsPlots(size.SizeParser, margin, bg, plotInternal)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data">每个标记的数量，函数会自动根据这些数量计算出百分比</param>
        ''' <param name="colors"></param>
        ''' <returns></returns>
        <Extension>
        Public Function FromData(data As IEnumerable(Of NamedValue(Of Integer)), Optional colors$() = Nothing) As FractionData()
            Dim array As NamedValue(Of Integer)() = data.ToArray
            Dim all = array.Select(Function(x) x.Value).Sum
            Dim s = From x
                    In array
                    Select New NamedValue(Of Double) With {
                        .Name = x.Name,
                        .Value = x.Value / all,
                        .Description = x.Value
                    }

            Return colors _
                .FromNames(array.Length) _
                .DoCall(AddressOf s.FromPercentages)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data">每个标记的数量，函数会自动根据这些数量计算出百分比</param>
        ''' <param name="schema"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Fractions(data As IEnumerable(Of NamedValue(Of Integer)), Optional schema$ = NameOf(OfficeColorThemes.Office2016)) As FractionData()
            Dim array As NamedValue(Of Integer)() = data.ToArray
            Dim all As Integer = array _
                .Select(Function(x) x.Value) _
                .Sum
            Dim sections = From x
                           In array
                           Select New NamedValue(Of Double) With {
                               .Name = x.Name,
                               .Value = x.Value / all,
                               .Description = x.Value
                           }
            Dim colors As Color() = Designer.FromSchema(
                schema, array.Length
            )
            Return sections.FromPercentages(colors)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data">手工计算出来的百分比</param>
        ''' <param name="colors">Default is using schema of <see cref="Office2016"/></param>
        ''' <returns></returns>
        <Extension>
        Public Function FromPercentages(data As IEnumerable(Of NamedValue(Of Double)), Optional colors As Color() = Nothing) As FractionData()
            Dim array = data.ToArray
            Dim out As FractionData() = New FractionData(array.Length - 1) {}
            Dim c As Color()

            If colors.IsNullOrEmpty Then
                c = NameOf(OfficeColorThemes.Office2016) _
                    .DoCall(Function(term)
                                Return Designer.FromSchema(term, array.Length)
                            End Function)
            Else
                c = colors
            End If

            Dim tag$, val#

            For i As Integer = 0 To array.Length - 1
                With array(i)
                    tag = .Name
                    val# = .Value

                    out(i) = New FractionData With {
                        .Color = c(i),
                        .Name = tag,
                        .Percentage = val#,
                        .Value = Conversion.Val(array(i).Description)
                    }
                End With
            Next

            Return out
        End Function
    End Module
End Namespace
