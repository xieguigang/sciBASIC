#Region "Microsoft.VisualBasic::30a01b4abbdec39735a81190b3579948, ..\sciBASIC#\Data_science\Mathematical\Plots\g\Scaling.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' 将数据坐标转换为绘图坐标
''' </summary>
Public Class Scaling

    Public ReadOnly dx!, dy!
    Public ReadOnly xmin, ymin As Single

    ReadOnly serials As SerialData()
    ReadOnly hist As HistogramGroup

    Public ReadOnly type As Type

    Sub New(array As SerialData())
        dx = Scaling(array, Function(p) p.pt.X, xmin)
        dy = Scaling(array, Function(p) p.pt.Y, ymin)
        serials = array
        type = GetType(Scatter)
    End Sub

    Sub New(data As HistogramGroup)
        dx = Scaling(data, Function(x) {x.x1, x.x2}, xmin)
        dy = Scaling(data, Function(x) {x.y}, ymin)
        hist = data
        type = GetType(Histogram)
    End Sub

    Sub New(hist As BarDataGroup, stacked As Boolean, horizontal As Boolean)
        Dim h As List(Of Double) = If(
            stacked,
            New List(Of Double)(hist.Samples.Select(Function(s) s.StackedSum)),
            hist.Samples.Select(Function(s) s.data).Unlist)

        If Not horizontal Then
            ymin! = h.Min

            If ymin > 0 Then  ' 由於bar是有一定高度的，所以儅直接使用數據之中的最小值作爲繪圖的最小參考值的畫，會出現最小的那個bar會沒有高度的bug，在這裏統一修改為0為最小參考值
                ymin = 0
            End If

            dy = h.Max - ymin
        Else
            xmin! = h.Min

            If xmin > 0 Then
                xmin = 0
            End If

            dx = h.Max - xmin
        End If

        type = GetType(BarPlot)
    End Sub

    ''' <summary>
    ''' 返回的系列是已经被转换过的，直接使用来进行画图
    ''' </summary>
    ''' <returns></returns>
    Public Iterator Function ForEach(size As Size, margin As Size) As IEnumerable(Of SerialData)
        Dim bottom As Integer = size.Height - margin.Height
        Dim width As Integer = size.Width - margin.Width * 2
        Dim height As Integer = size.Height - margin.Height * 2

        For Each s As SerialData In serials
            Dim pts = LinqAPI.Exec(Of PointData) <=
 _
                From p As PointData
                In s.pts
                Let px As Single = margin.Width + width * (p.pt.X - xmin) / dx
                Let yh As Single = If(dy = 0R, height / 2, height * (p.pt.Y - ymin) / dy) ' 如果y没有变化，则是一条居中的水平直线
                Let py As Single = bottom - yh
                Select New PointData(px, py) With {
                    .errMinus = p.errMinus,
                    .errPlus = p.errPlus,
                    .Tag = p.Tag,
                    .value = p.value,
                    .Statics = p.Statics
                }

            Yield New SerialData With {
                .color = s.color,
                .lineType = s.lineType,
                .PointSize = s.PointSize,
                .pts = pts,
                .title = s.title,
                .width = s.width,
                .annotations = s.annotations
            }
        Next
    End Function

    ''' <summary>
    ''' 返回的系列是已经被转换过的，直接使用来进行画图
    ''' </summary>
    ''' <returns></returns>
    Public Iterator Function ForEach_histSample(size As Size, margin As Size) As IEnumerable(Of HistProfile)
        Dim bottom As Integer = size.Height - margin.Height
        Dim width As Integer = size.Width - margin.Width * 2
        Dim height As Integer = size.Height - margin.Height * 2

        For Each histData As HistProfile In hist.Samples
            Dim pts = LinqAPI.Exec(Of HistogramData) <=
 _
                From p As HistogramData
                In histData.data
                Let px1 As Single = margin.Width + width * (p.x1 - xmin) / dx
                Let px2 As Single = margin.Width + width * (p.x2 - xmin) / dx
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

    Public Function PointScaler(size As Size, margin As Size) As Func(Of PointF, PointF)
        Dim bottom As Integer = size.Height - margin.Height
        Dim width As Integer = size.Width - margin.Width * 2
        Dim height As Integer = size.Height - margin.Height * 2

        Return Function(pt)
                   Dim px As Single = margin.Width + width * (pt.X - xmin) / dx
                   Dim py As Single = bottom - height * (pt.Y - ymin) / dy

                   Return New PointF(px, py)
               End Function
    End Function

    Public Function PointScaler(r As GraphicsRegion, pt As PointF) As PointF
        Dim bottom As Integer = r.Size.Height - r.Margin.Height
        Dim width As Integer = r.Size.Width - r.Margin.Width * 2
        Dim height As Integer = r.Size.Height - r.Margin.Height * 2
        Dim px As Single = r.Margin.Width + width * (pt.X - xmin) / dx
        Dim py As Single = bottom - height * (pt.Y - ymin) / dy

        Return New PointF(px!, py!)
    End Function

    Public Function XScaler(size As Size, margin As Size) As Func(Of Single, Single)
        Dim bottom As Integer = size.Height - margin.Height
        Dim width As Integer = size.Width - margin.Width * 2
        Dim height As Integer = size.Height - margin.Height * 2

        Return Function(x) margin.Width + width * (x - xmin) / dx
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="avg">当这个参数值是一个有效的数字的时候，返回的Y将会以这个平均值为零点</param>
    ''' <returns></returns>
    Public Function YScaler(size As Size, margin As Size, Optional avg# = Double.NaN) As Func(Of Single, Single)
        Dim bottom As Integer = size.Height - margin.Height
        Dim height As Integer = size.Height - margin.Height * 2   ' 绘图区域的高度

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

    Public Shared Function Average(hist As BarDataGroup) As Double
        Return hist.Samples.Select(Function(x) x.data).IteratesALL.Average()
    End Function

    ''' <summary>
    ''' 返回dx或者dy
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function Scaling(data As IEnumerable(Of SerialData), [get] As Func(Of PointData, Single), ByRef min!) As Single
        Dim array!() = data.Select(Function(s) s.pts).IteratesALL.ToArray([get])
        Return __scaling(array!, min!)
    End Function

    Private Shared Function __scaling(array!(), ByRef min!) As Single
        Dim max! = array.Max : min! = array.Min
        Dim d As Single = max - min
        Return d
    End Function

    ''' <summary>
    ''' 返回dx或者dy
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function Scaling(data As HistogramGroup, [get] As Func(Of HistogramData, Single()), ByRef min!) As Single
        Dim array!() = data.Samples _
            .Select(Function(s) s.data) _
            .IteratesALL _
            .ToArray([get]) _
            .IteratesALL _
            .ToArray
        Return __scaling(array!, min!)
    End Function
End Class
