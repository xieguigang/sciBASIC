#Region "Microsoft.VisualBasic::63614448a740e6e95d51f680f9879af3, gr\network-visualization\network_layout\Circular\CircularLayoutParameters.vb"

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

    '   Total Lines: 33
    '    Code Lines: 20 (60.61%)
    ' Comment Lines: 3 (9.09%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (30.30%)
    '     File Size: 1.26 KB


    '     Class CircularLayoutParameters
    ' 
    '         Properties: CenterX, CenterY, MaxSwaps, OptimizeCrossing, Radius
    '                     SortByDegree
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace Circular

    ''' <summary>
    ''' 环形布局参数，可在 PropertyGrid 中编辑
    ''' </summary>
    Public Class CircularLayoutParameters

        <Category("布局"), DisplayName("半径"), Description("圆周半径（像素），<=0 时自动推算")>
        Public Property Radius As Double = Double.NaN

        <Category("布局"), DisplayName("圆心 X")>
        Public Property CenterX As Double = 500.0

        <Category("布局"), DisplayName("圆心 Y")>
        Public Property CenterY As Double = 500.0

        <Category("布局"), DisplayName("按度排序"), Description("按节点度排序以优化视觉")>
        Public Property SortByDegree As Boolean = True

        <Category("优化"), DisplayName("交叉优化"), Description("是否启用 2-opt 交叉最小化（较慢）")>
        Public Property OptimizeCrossing As Boolean = False

        <Category("优化"), DisplayName("最大交换次数"), Description("2-opt 局部搜索的最大交换次数")>
        Public Property MaxSwaps As Integer = 1000

        Public Overrides Function ToString() As String
            Return "Circular"
        End Function
    End Class

End Namespace

