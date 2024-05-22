#Region "Microsoft.VisualBasic::50a11c14c627bd89aec86f36f560f720, Data_science\Visualization\Plots\g\Scaling.vb"

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

    '   Total Lines: 222
    '    Code Lines: 127 (57.21%)
    ' Comment Lines: 63 (28.38%)
    '    - Xml Docs: 93.65%
    ' 
    '   Blank Lines: 32 (14.41%)
    '     File Size: 8.19 KB


    '     Class Scaling
    ' 
    '         Properties: xrange, yrange
    ' 
    '         Constructor: (+6 Overloads) Sub New
    '         Function: __barDataProvider, __scaling, Average, (+3 Overloads) Scaling, (+2 Overloads) ScalingTuple
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Histogram
Imports Microsoft.VisualBasic.Data.ChartPlots.Contour
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Linq

Namespace Graphic

    ''' <summary>
    ''' 获取绘图数据的实际范围
    ''' </summary>
    Public Class Scaling

        ''' <summary>
        ''' x,y轴分别的最大值和最小值的差值
        ''' </summary>
        Public ReadOnly dx#, dy#
        Public ReadOnly xmin, ymin As Single

        Friend ReadOnly serials As SerialData()
        Friend ReadOnly hist As HistogramGroup

        ''' <summary>
        ''' 数据集类型
        ''' </summary>
        Public ReadOnly type As Type

        ''' <summary>
        ''' x值的实际范围
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property xrange As DoubleRange
            Get
                Return New DoubleRange(xmin, xmin + dx)
            End Get
        End Property

        ''' <summary>
        ''' y值的实际范围
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property yrange As DoubleRange
            Get
                Return New DoubleRange(ymin, ymin + dy)
            End Get
        End Property

        ''' <summary>
        ''' 线条
        ''' </summary>
        ''' <param name="array"></param>
        ''' <param name="absoluteScaling"></param>
        Sub New(array As SerialData(), absoluteScaling As Boolean)
            dx = Scaling(array, Function(p) p.pt.X, absoluteScaling, xmin)
            dy = Scaling(array, Function(p) p.pt.Y, absoluteScaling, ymin)
            serials = array
            type = GetType(Scatter)
        End Sub

        Sub New(data As (Double, Double)())
            dx = ScalingTuple(data, Function(p) p.X, False, xmin)
            dy = ScalingTuple(data, Function(p) p.y, False, ymin)
            type = GetType(ContourPlot)
        End Sub

        Sub New(data As (X#, y#, z#)())
            dx = ScalingTuple(data, Function(p) p.X, False, xmin)
            dy = ScalingTuple(data, Function(p) p.y, False, ymin)
            type = GetType(ContourPlot)
        End Sub

        ''' <summary>
        ''' 连续的条型数据
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="absoluteScaling"></param>
        Sub New(data As HistogramGroup, absoluteScaling As Boolean)
            dx = Scaling(data, Function(x) {x.x1, x.x2}, xmin, absoluteScaling)
            dy = Scaling(data, Function(x) {x.y}, ymin, absoluteScaling)
            hist = data
            type = GetType(Histogram)
        End Sub

        ''' <summary>
        ''' 分类的条型数据
        ''' </summary>
        ''' <param name="hist"></param>
        ''' <param name="stacked"></param>
        ''' <param name="horizontal"></param>
        Sub New(hist As BarDataGroup, stacked As Boolean, horizontal As Boolean)
            Call Me.New(__barDataProvider(hist, stacked), horizontal)
        End Sub

        Private Shared Function __barDataProvider(hist As BarDataGroup, stacked As Boolean) As IEnumerable(Of Double)
            If stacked Then
                Return hist _
                    .Samples _
                    .Select(Function(s) s.StackedSum)
            Else
                Return hist _
                    .Samples _
                    .Select(Function(s) s.data) _
                    .Unlist
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="horizontal">
        ''' 所进行绘制的条形图是否是水平的？
        ''' </param>
        Sub New(data As IEnumerable(Of Double), horizontal As Boolean)
            Dim h#() = data.ToArray

            ' dx = 100 和 dy = 100 是为了防止tick出错而特意设置的

            If Not horizontal Then
                ymin! = h.Min

                If ymin > 0 Then  ' 由於bar是有一定高度的，所以儅直接使用數據之中的最小值作爲繪圖的最小參考值的畫，會出現最小的那個bar會沒有高度的bug，在這裏統一修改為0為最小參考值
                    ymin = 0
                End If

                ' dx = 100
                dy = h.Max - ymin
            Else
                xmin! = h.Min

                If xmin > 0 Then
                    xmin = 0
                End If

                ' dy = 100
                dx = h.Max - xmin
            End If

            type = GetType(BarPlotAPI)
        End Sub

        Public Shared Function Average(hist As BarDataGroup) As Double
            Return hist.Samples _
                .Select(Function(x) x.data) _
                .IteratesALL _
                .Average()
        End Function

        ''' <summary>
        ''' 返回dx或者dy
        ''' </summary>
        ''' <param name="absoluteScaling">
        ''' 假若值为真，则当min和max都是正数的时候，从min=0开始
        ''' 当min和max都是负数的时候，从max=0结束
        ''' 当min和max的符号不同的时候，只能够使用相对scalling
        ''' </param>
        ''' <returns></returns>
        Public Shared Function Scaling(data As IEnumerable(Of SerialData), [get] As Func(Of PointData, Single), absoluteScaling As Boolean, ByRef min!) As Single
            Dim array!() = data _
                .Select(Function(s) s.pts) _
                .IteratesALL _
                .Select([get]) _
                .ToArray

            Return __scaling(array!, min!, absoluteScaling)
        End Function

        Public Shared Function ScalingTuple(data As IEnumerable(Of (X#, y#, z#)), [get] As Func(Of (X#, y#, z#), Single), absoluteScaling As Boolean, ByRef min!) As Single
            Dim array!() = data.Select([get]).ToArray
            Return __scaling(array!, min!, absoluteScaling)
        End Function

        Public Shared Function ScalingTuple(data As IEnumerable(Of (X#, y#)), [get] As Func(Of (X#, y#), Single), absoluteScaling As Boolean, ByRef min!) As Single
            Dim array!() = data.Select([get]).ToArray
            Return __scaling(array!, min!, absoluteScaling)
        End Function

        Public Shared Function Scaling(data As IEnumerable(Of Point3D), [get] As Func(Of Point3D, Single), absoluteScaling As Boolean, ByRef min!) As Single
            Dim array!() = data.Select([get]).ToArray
            Return __scaling(array!, min!, absoluteScaling)
        End Function

        ''' <summary>
        ''' 返回``max-min``
        ''' </summary>
        ''' <param name="array!"></param>
        ''' <param name="min!"></param>
        ''' <param name="absoluteScaling"></param>
        ''' <returns></returns>
        Private Shared Function __scaling(array!(), ByRef min!, absoluteScaling As Boolean) As Single
            Dim max! = array.Max : min! = array.Min

            If absoluteScaling Then
                If max < 0 Then
                    max = 0
                ElseIf min > 0 Then
                    min = 0
                End If
            End If

            Dim d As Single = max - min
            Return d
        End Function

        ''' <summary>
        ''' 返回dx或者dy
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function Scaling(data As HistogramGroup, [get] As Func(Of HistogramData, Single()), ByRef min!, absoluteScaling As Boolean) As Single
            Dim array!() = data.Samples _
                .Select(Function(s) s.data) _
                .IteratesALL _
                .Select([get]) _
                .IteratesALL _
                .ToArray

            Return __scaling(array!, min!, absoluteScaling)
        End Function
    End Class
End Namespace
