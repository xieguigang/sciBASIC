#Region "Microsoft.VisualBasic::3e0b2394656ba12a22d6e52f0901e039, Data_science\Visualization\DataPlot\Canvas\PointData.vb"

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

    '   Total Lines: 60
    '    Code Lines: 35 (58.33%)
    ' Comment Lines: 15 (25.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (16.67%)
    '     File Size: 1.69 KB


    '     Structure PointData
    ' 
    '         Properties: X, Y
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

Namespace Canvas

    ''' <summary>
    ''' 绘图的点的数据
    ''' </summary>
    Public Structure PointData : Implements IReadOnlyPoint

        ''' <summary>
        ''' 坐标数据不需要进行额外的转换，绘图函数内部会自动进行mapping转换的
        ''' </summary>
        Public pt As PointF
        ''' <summary>
        ''' 正负误差
        ''' </summary>
        Public errPlus#, errMinus#, tag$, value#
        ''' <summary>
        ''' 可能会有数据点在<see cref="errPlus"/>或者<see cref="errMinus"/>范围内，或者范围外
        ''' </summary>
        Public Statics#()
        Public color$
        Public stroke$
        Public size As Single?

        ''' <summary>
        ''' 坐标轴的值模式为字符串模式的时候
        ''' </summary>
        Public axisLabel As String

        Public ReadOnly Property X As Double Implements IReadOnlyPoint.X
            Get
                Return pt.X
            End Get
        End Property

        Public ReadOnly Property Y As Double Implements IReadOnlyPoint.Y
            Get
                Return pt.Y
            End Get
        End Property

        Sub New(x!, y!)
            pt = New PointF(x, y)
        End Sub

        Sub New(pt As PointF)
            Me.pt = pt
        End Sub

        Sub New(pt As Point)
            Me.pt = New PointF(pt.X, pt.Y)
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{pt.ToString}] {value} {color} {tag}"
        End Function
    End Structure
End Namespace
