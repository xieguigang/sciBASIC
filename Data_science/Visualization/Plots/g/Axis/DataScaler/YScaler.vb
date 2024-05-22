#Region "Microsoft.VisualBasic::8e394ae54ca77a26461c087a30c254b5, Data_science\Visualization\Plots\g\Axis\DataScaler\YScaler.vb"

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

    '   Total Lines: 67
    '    Code Lines: 34 (50.75%)
    ' Comment Lines: 22 (32.84%)
    '    - Xml Docs: 90.91%
    ' 
    '   Blank Lines: 11 (16.42%)
    '     File Size: 2.12 KB


    '     Class YScaler
    ' 
    '         Properties: region, Y
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) TranslateHeight, (+2 Overloads) TranslateY
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.d3js.scale
Imports stdNum = System.Math

Namespace Graphic.Axis

    Public Class YScaler

        Public Property Y As LinearScale

        ''' <summary>
        ''' The charting region in <see cref="Rectangle"/> data structure.
        ''' </summary>
        Public Property region As Rectangle

        ''' <summary>
        ''' 是否需要将Y坐标轴上下翻转颠倒
        ''' </summary>
        Dim reversed As Boolean

        Sub New(reversed As Boolean)
            Me.reversed = reversed
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="y#"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' ###### 2018-1-16
        ''' 
        ''' 因为绘图的时候有margin的，故而Y不是从零开始的，而是从margin的top开始的
        ''' 所以需要额外的加上一个top值
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TranslateY(y#) As Double
            If reversed Then
                Return Me.Y(y)
            Else
                Return region.Bottom - Me.Y(y) + region.Top
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TranslateY(y As IEnumerable(Of Double)) As IEnumerable(Of Double)
            Return y.Select(AddressOf TranslateY)
        End Function

        Public Function TranslateHeight(y1 As Double, y2 As Double) As Double
            y1 = TranslateY(y1)
            y2 = TranslateY(y2)

            Return stdNum.Max(y1, y2) - stdNum.Min(y1, y2)
        End Function

        ''' <summary>
        ''' 从原始数据计算出绘图的实际高度
        ''' </summary>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Function TranslateHeight(y As Double) As Double
            Return region.Bottom - Me.Y(y)
        End Function
    End Class
End Namespace
