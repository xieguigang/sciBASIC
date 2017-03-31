#Region "Microsoft.VisualBasic::59dc0509df443ce8f1482d4ea2424e98, ..\sciBASIC#\Data_science\Mathematical\Plots\Fractions\PieChart.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Public Module PieChart

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="size"></param>
    ''' <param name="bg"></param>
    ''' <param name="legend"></param>
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
    <Extension>
    Public Function Plot(data As IEnumerable(Of Fractions),
                         Optional size As Size = Nothing,
                         Optional padding$ = g.DefaultPadding,
                         Optional bg$ = "white",
                         Optional valueLabel As Fractions.ValueLabels = Fractions.ValueLabels.Percentage,
                         Optional valueLabelStyle$ = CSSFont.Win7Bold,
                         Optional legend As Boolean = True,
                         Optional legendFont$ = CSSFont.Win7LargeBold,
                         Optional legendBorder As Stroke = Nothing,
                         Optional minRadius As Single = -1,
                         Optional reorder% = 0) As Bitmap

        Dim margin As Padding = padding

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

        Dim __plot As Action(Of Graphics) =
            Sub(g As Graphics)
                Dim r# = (Math.Min(size.Width, size.Height) - margin.LayoutVector.Max) / 2  ' 最大的半径值
                Dim topLeft As New Point(size.Width / 2 - r, size.Height / 2 - r)
                Dim valueLabelFont As Font = CSSFont.TryParse(valueLabelStyle)

                If minRadius <= 0 OrElse CDbl(minRadius) >= r Then  ' 半径固定不变的样式
                    Dim rect As New Rectangle(topLeft, New Size(r * 2, r * 2))
                    Dim start As New Value(Of Single)
                    Dim sweep As New Value(Of Single)
                    Dim alpha As Double, pt As PointF
                    Dim centra As Point = rect.Centre
                    Dim labelSize As SizeF
                    Dim label$

                    Call g.FillPie(Brushes.LightGray, rect, 0, 360)

                    For Each x As Fractions In data
                        Call g.FillPie(New SolidBrush(x.Color), rect, (start = ((+start) + (sweep = CSng(360 * x.Percentage)))) - sweep.value, sweep)

                        alpha = (+start) - (+sweep / 2)
                        pt = (r / 1.5).ToPoint(alpha)
                        pt = New PointF(pt.X + centra.X, pt.Y + centra.Y)
                        label = x.GetValueLabel(valueLabel)
                        labelSize = g.MeasureString(label, valueLabelFont)

                        '   If alpha > 90 AndAlso alpha < 270 Then
                        pt = New Point(pt.X - labelSize.Width / 2, pt.Y)
                        '  End If

                        Call g.DrawString(label, valueLabelFont, Brushes.White, pt)
                    Next
                Else  ' 半径也会有变化
                    Dim a As New Value(Of Single)
                    Dim sweep! = 360 / data.Count
                    Dim maxp# = data.Max(Function(x) x.Percentage)
#If DEBUG Then
                         Dim list As New List(Of Rectangle)
#End If
                    For Each x As Fractions In data
                        Dim r2# = minRadius + (r - minRadius) * (x.Percentage / maxp)
                        Dim vTopleft As New Point(size.Width / 2 - r2, size.Height / 2 - r2)
                        Dim rect As New Rectangle(vTopleft, New Size(r2 * 2, r2 * 2))
                        Dim br As New SolidBrush(x.Color)

                        Call g.FillPie(br, rect, (a = (a.value + sweep)), sweep)
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

                If legend Then
                    Dim font As Font = CSSFont.TryParse(legendFont)
                    Dim maxL = data.Select(Function(x) g.MeasureString(x.Name, font).Width).Max
                    Dim left = size.Width - (margin.Horizontal) - maxL
                    Dim top = margin.Top
                    Dim legends As New List(Of Legend)

                    For Each x As Fractions In data
                        legends += New Legend With {
                                .color = x.Color.RGBExpression,
                                .style = LegendStyles.Square,
                                .title = x.Name,
                                .fontstyle = legendFont
                             }
                    Next

                    Call g.DrawLegends(New Point(left, top), legends, ,, legendBorder)
                End If
            End Sub

        Return __plot.GraphicsPlots(size, margin, bg)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data">每个标记的数量，函数会自动根据这些数量计算出百分比</param>
    ''' <param name="colors"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FromData(data As IEnumerable(Of NamedValue(Of Integer)), Optional colors As String() = Nothing) As Fractions()
        Dim array As NamedValue(Of Integer)() = data.ToArray
        Dim all = array.Select(Function(x) x.Value).Sum
        Dim s = From x
                In array
                Select New NamedValue(Of Double) With {
                    .Name = x.Name,
                    .Value = x.Value / all,
                    .Description = x.Value
                }
        Return s.FromPercentages(colors.FromNames(array.Length))
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data">每个标记的数量，函数会自动根据这些数量计算出百分比</param>
    ''' <param name="schema"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FromData(data As IEnumerable(Of NamedValue(Of Integer)), Optional schema$ = NameOf(Office2016)) As Fractions()
        Dim array As NamedValue(Of Integer)() = data.ToArray
        Dim all As Integer = array _
            .Select(Function(x) x.Value) _
            .Sum
        Dim s = From x
                In array
                Select New NamedValue(Of Double) With {
                    .Name = x.Name,
                    .Value = x.Value / all,
                    .Description = x.Value
                }
        Dim colors As Color() = Designer.FromSchema(
            schema, array.Length
        )
        Return s.FromPercentages(colors)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data">手工计算出来的百分比</param>
    ''' <param name="colors">Default is using schema of <see cref="Office2016"/></param>
    ''' <returns></returns>
    <Extension>
    Public Function FromPercentages(data As IEnumerable(Of NamedValue(Of Double)), Optional colors As Color() = Nothing) As Fractions()
        Dim array = data.ToArray
        Dim out As Fractions() = New Fractions(array.Length - 1) {}
        Dim c As Color() = If(
            colors.IsNullOrEmpty,
            Designer.FromSchema(NameOf(Office2016), array.Length),
            colors
        )

        For i As Integer = 0 To array.Length - 1
            With array(i)
                Dim tag = .Name
                Dim v# = .Value

                out(i) = New Fractions With {
                    .Color = c(i),
                    .Name = tag,
                    .Percentage = v#,
                    .Value = Val(array(i).Description)
                }
            End With
        Next

        Return out
    End Function
End Module
