#Region "Microsoft.VisualBasic::522998436f9f06fa4823faf27b550620, Data_science\Visualization\DataPlot\Canvas\Mapper.vb"

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

    '   Total Lines: 51
    '    Code Lines: 22 (43.14%)
    ' Comment Lines: 21 (41.18%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 8 (15.69%)
    '     File Size: 1.75 KB


    '     Class Mapper
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ScallingWidth
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
'  Mapper.vb - Canvas 兼容层的数据-绘图坐标映射器
'
'  从旧 Plots 项目的 g/Mapper.vb 迁移而来的最小化版本：
'  保留 xrange/yrange 值域与 ScallingWidth 映射，供条形图等布局计算使用。
' ============================================================================

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Canvas

    ''' <summary>
    ''' 将数据坐标转换为绘图坐标（宽度/高度比例）
    ''' </summary>
    Public Class Mapper

        ''' <summary>
        ''' 坐标轴的数据
        ''' </summary>
        Public ReadOnly xAxis, yAxis As Vector

        ''' <summary>
        ''' x,y轴分别的最大值和最小值的差值
        ''' </summary>
        Public ReadOnly dx#, dy#
        Public ReadOnly xmin, ymin As Single

        ''' <summary>
        '''Create a new mapper from a given data range.
        ''' </summary>
        Sub New(xrange As DoubleRange, yrange As DoubleRange)
            xmin = xrange.Min
            dx = xrange.Max - xrange.Min
            ymin = yrange.Min
            dy = yrange.Max - yrange.Min
        End Sub

        ''' <summary>
        ''' 将数据值 <paramref name="x"/> 映射为在总宽度 <paramref name="width"/> 中的像素宽度
        ''' </summary>
        Public Function ScallingWidth(x As Double, width As Integer) As Single
            If dx = 0 Then
                Return 0
            End If

            Return width * (x - xmin) / dx
        End Function
    End Class
End Namespace
