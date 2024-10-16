#Region "Microsoft.VisualBasic::f646cf4333fcd0b9098df14114b7dc77, Data_science\Visualization\Plots\g\Mapper.vb"

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

    '   Total Lines: 287
    '    Code Lines: 215 (74.91%)
    ' Comment Lines: 33 (11.50%)
    '    - Xml Docs: 93.94%
    ' 
    '   Blank Lines: 39 (13.59%)
    '     File Size: 11.78 KB


    '     Class Mapper
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ForEach, ForEach_histSample, (+3 Overloads) PointScaler, ScallingWidth, TupleScaler
    '                   XScaler, YScaler
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Histogram
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace Graphic

    ''' <summary>
    ''' 将数据坐标转换为绘图坐标
    ''' </summary>
    Public Class Mapper

        ''' <summary>
        ''' 坐标轴的数据
        ''' </summary>
        Public ReadOnly xAxis, yAxis As Vector

        ReadOnly serials As SerialData()
        ReadOnly hist As HistogramGroup

        ''' <summary>
        ''' x,y轴分别的最大值和最小值的差值
        ''' </summary>
        Public ReadOnly dx#, dy#
        Public ReadOnly xmin, ymin As Single

        Sub New(range As Scaling,
                Optional parts% = 10,
                Optional XabsoluteScalling As Boolean = False,
                Optional YabsoluteScalling As Boolean = False,
                Optional ignoreAxis As Boolean = False,
                Optional ignoreX As Boolean = False,
                Optional ignoreY As Boolean = False)

            Call Me.New(range.xrange, range.yrange, parts,
                        XabsoluteScalling,
                        YabsoluteScalling,
                        ignoreAxis, ignoreX, ignoreY)

            serials = range.serials
            hist = range.hist
        End Sub

        Sub New(xrange As DoubleRange, yrange As DoubleRange,
                Optional parts% = 10,
                Optional XabsoluteScalling As Boolean = False,
                Optional YabsoluteScalling As Boolean = False,
                Optional ignoreAxis As Boolean = False,
                Optional ignoreX As Boolean = False,
                Optional ignoreY As Boolean = False)

            If Not ignoreAxis Then

                If Not ignoreX Then
                    xAxis = New Vector(xrange.CreateAxisTicks(parts / 2))
                Else
                    xAxis = New Vector({0R})
                End If
                If Not ignoreY Then
                    yAxis = New Vector(yrange.CreateAxisTicks(parts))
                Else
                    yAxis = New Vector({0R})
                End If

                If xAxis.Length = 0 Then
                    dx = 0
                    xmin = 0
                Else
                    dx = xAxis.Max - xAxis.Min
                    xmin = xAxis.Min
                End If

                dy = yAxis.Max - yAxis.Min
                ymin = yAxis.Min
            Else
                dx = xrange.Max - xrange.Min
                dy = yrange.Max - yrange.Min
                xmin = xrange.Min
                ymin = yrange.Min
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="range">所获取得到的绘图所使用的系列的数据</param>
        ''' <param name="ignoreAxis"></param>
        ''' <param name="ignoreX"></param>
        ''' <param name="ignoreY"></param>
        Sub New(x As AxisProvider, y As AxisProvider, Optional range As Scaling = Nothing,
                Optional ignoreAxis As Boolean = False,
                Optional ignoreX As Boolean = False,
                Optional ignoreY As Boolean = False)

            If Not ignoreAxis Then

                If Not ignoreX Then
                    xAxis = New Vector(x.AxisTicks)
                Else
                    xAxis = New Vector({0R})
                End If
                If Not ignoreY Then
                    yAxis = New Vector(y.AxisTicks)
                Else
                    yAxis = New Vector({0R})
                End If

                dx = xAxis.Max - xAxis.Min
                dy = yAxis.Max - yAxis.Min
                xmin = xAxis.Min
                ymin = yAxis.Min
            Else
                Dim xrange As DoubleRange = x.Range
                Dim yrange As DoubleRange = y.Range

                dx = xrange.Max - xrange.Min
                dy = yrange.Max - yrange.Min
                xmin = xrange.Min
                ymin = yrange.Min
            End If

            If Not range Is Nothing Then
                hist = range.hist
                serials = range.serials
            End If
        End Sub

        Public Function ScallingWidth(x As Double, width%) As Single
            Return width * (x - xmin) / dx
        End Function

        ''' <summary>
        ''' 返回的系列是已经被转换过的，直接使用来进行画图
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function ForEach(css As CSSEnvirnment, margin As Padding) As IEnumerable(Of SerialData)
            Dim size As Size = css.canvas
            Dim padding = PaddingLayout.EvaluateFromCSS(css, margin)
            Dim bottom As Integer = size.Height - padding.Bottom
            Dim width As Integer = size.Width - margin.Horizontal(css)
            Dim height As Integer = size.Height - margin.Vertical(css)

            For Each s As SerialData In serials
                Dim pts = LinqAPI.Exec(Of PointData) <=
                                                       _
                From p As PointData
                In s.pts
                Let px As Single = padding.Left + width * (p.pt.X - xmin) / dx
                Let yh As Single = If(dy = 0R, height / 2, height * (p.pt.Y - ymin) / dy) ' 如果y没有变化，则是一条居中的水平直线
                Let py As Single = bottom - yh
                Select New PointData(px, py) With {
                    .errMinus = p.errMinus,
                    .errPlus = p.errPlus,
                    .tag = p.tag,
                    .value = p.value,
                    .Statics = p.Statics,
                    .color = p.color,
                    .stroke = p.stroke
                }

                Yield New SerialData With {
                    .color = s.color,
                    .lineType = s.lineType,
                    .pointSize = s.pointSize,
                    .pts = pts,
                    .title = s.title,
                    .width = s.width,
                    .DataAnnotations = s.DataAnnotations
                }
            Next
        End Function

        ''' <summary>
        ''' 返回的系列是已经被转换过的，直接使用来进行画图
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function ForEach_histSample(css As CSSEnvirnment, margin As Padding) As IEnumerable(Of HistProfile)
            Dim size As Size = css.canvas
            Dim padding = PaddingLayout.EvaluateFromCSS(css, margin)
            Dim bottom As Integer = size.Height - padding.Bottom
            Dim width As Integer = size.Width - margin.Horizontal(css)
            Dim height As Integer = size.Height - margin.Vertical(css)

            For Each histData As HistProfile In hist.Samples
                Dim pts = LinqAPI.Exec(Of HistogramData) <=
                                                           _
                From p As HistogramData
                In histData.data
                Let px1 As Single = padding.Left + width * (p.x1 - xmin) / dx
                Let px2 As Single = padding.Left + width * (p.x2 - xmin) / dx
                Let py As Single = bottom - height * (p.y - ymin) / dy
                Select New HistogramData With {
                    .x1 = px1,
                    .x2 = px2,
                    .y = py
                }

                Yield New HistProfile With {
                    .legend = histData.legend,
                    .data = pts
                }
            Next
        End Function

        Public Function PointScaler(css As CSSEnvirnment, padding As Padding) As Func(Of PointF, PointF)
            Dim size As Size = css.canvas
            Dim layout = PaddingLayout.EvaluateFromCSS(css, padding)
            Dim bottom As Integer = size.Height - layout.Bottom
            Dim width As Integer = size.Width - padding.Horizontal(css)
            Dim height As Integer = size.Height - padding.Vertical(css)

            Return Function(pt)
                       Dim px As Single = layout.Left + width * (pt.X - xmin) / dx
                       Dim py As Single = bottom - height * (pt.Y - ymin) / dy

                       Return New PointF(px, py)
                   End Function
        End Function

        Public Function PointScaler(rect As GraphicsRegion) As Func(Of PointF, PointF)
            Return PointScaler(New CSSEnvirnment(rect.Size), rect.Padding)
        End Function

        Public Function TupleScaler(rect As GraphicsRegion) As Func(Of (x#, y#), PointF)
            Dim point = PointScaler(New CSSEnvirnment(rect.Size), rect.Padding)
            Return Function(pt) point(New PointF(pt.x, pt.y))
        End Function

        Public Function PointScaler(r As GraphicsRegion, pt As PointF) As PointF
            Dim css As New CSSEnvirnment(r.Size)
            Dim padding = PaddingLayout.EvaluateFromCSS(css, r.Padding)
            Dim bottom As Integer = r.Size.Height - padding.Bottom
            Dim width As Integer = r.Size.Width - r.Padding.Horizontal(css)
            Dim height As Integer = r.Size.Height - r.Padding.Vertical(css)
            Dim px As Single = padding.Left + width * (pt.X - xmin) / dx
            Dim py As Single = bottom - height * (pt.Y - ymin) / dy

            Return New PointF(px!, py!)
        End Function

        Public Function XScaler(size As Size, margin As Padding) As Func(Of Single, Single)
            Dim css As New CSSEnvirnment(size)
            Dim padding = PaddingLayout.EvaluateFromCSS(css, margin)
            Dim bottom As Integer = size.Height - padding.Bottom
            Dim width As Integer = size.Width - margin.Horizontal(css)
            Dim height As Integer = size.Height - margin.Vertical(css)

            Return Function(x) padding.Left + width * (x - xmin) / dx
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="size"></param>
        ''' <param name="margin"></param>
        ''' <param name="avg">当这个参数值是一个有效的数字的时候，返回的Y将会以这个平均值为零点</param>
        ''' <returns></returns>
        Public Function YScaler(size As Size, margin As Padding, Optional avg# = Double.NaN) As Func(Of Single, Single)
            Dim css As New CSSEnvirnment(size)
            Dim bottom As Integer = size.Height - css.GetHeight(margin.Bottom)
            Dim height As Integer = size.Height - margin.Vertical(css)    ' 绘图区域的高度

            If Double.IsNaN(avg#) Then
                Return Function(y!) bottom - height * (y - ymin) / dy
            Else
                Dim half As Single = height / 2
                Dim middle As Single = bottom - half

                Return Function(y!) As Single
                           Dim d! = y - avg

                           If d >= 0F Then  ' 在上面
                               Return middle - half * (y - avg) / dy
                           Else
                               Return middle + half * (avg - y) / dy
                           End If
                       End Function
            End If
        End Function
    End Class
End Namespace
