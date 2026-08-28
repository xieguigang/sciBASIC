#Region "Microsoft.VisualBasic::cc5b95800d769d55a36fd63439209a93, gr\network-visualization\network_layout\HOLA\HolaOptions.vb"

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

    '   Total Lines: 66
    '    Code Lines: 26 (39.39%)
    ' Comment Lines: 29 (43.94%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (16.67%)
    '     File Size: 2.50 KB


    '     Class HolaOptions
    ' 
    '         Properties: alignEpsilon, convergeEpsilon, desiredEdgeLength, maxIterations, nodeGap
    '                     nodeRadiusPadding, routeGridSize
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Clone
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Language

Namespace Hola

    ''' <summary>
    ''' 可调节的 HOLA 算法参数集合。所有数值约定为 GDI 坐标系（y 轴向下，
    ''' NORTH 对应 y 值减小）。集中管理便于在不同数据集上调参。
    ''' </summary>
    Public Class HolaOptions

        ''' <summary>
        ''' 节点之间允许的最小间距（像素），用于扩散松弛阶段的分离约束。
        ''' </summary>
        Public Property nodeGap As Double = 30.0

        ''' <summary>
        ''' 对齐阈值：两个节点在某一坐标轴上相差小于该值时，
        ''' 在 align 松弛阶段被强制对齐到同一网格线。
        ''' </summary>
        Public Property alignEpsilon As Double = 4.0

        ''' <summary>
        ''' 期望的边长 L，用于保持相邻节点之间舒适的距离。
        ''' </summary>
        Public Property desiredEdgeLength As Double = 60.0

        ''' <summary>
        ''' 应力收敛阈值，低于该值即认为布局已稳定。
        ''' </summary>
        Public Property convergeEpsilon As Double = 0.01

        ''' <summary>
        ''' 各松弛阶段的最大迭代次数，防止病态输入下死循环。
        ''' </summary>
        Public Property maxIterations As Integer = 200

        ''' <summary>
        ''' 正交路由阶段使用的网格分辨率（像素），折点会吸附到该网格。
        ''' </summary>
        Public Property routeGridSize As Double = 10.0

        ''' <summary>
        ''' 正交路由时节点的大小半径附加补偿，避免连线路由穿过节点。
        ''' </summary>
        Public Property nodeRadiusPadding As Double = 8.0

        Public Sub New()
        End Sub

        ''' <summary>
        ''' 创建一份参数副本，避免多次布局之间共享可变状态。
        ''' </summary>
        Public Function Clone() As HolaOptions
            Return New HolaOptions With {
                .nodeGap = nodeGap,
                .alignEpsilon = alignEpsilon,
                .desiredEdgeLength = desiredEdgeLength,
                .convergeEpsilon = convergeEpsilon,
                .maxIterations = maxIterations,
                .routeGridSize = routeGridSize,
                .nodeRadiusPadding = nodeRadiusPadding
            }
        End Function
    End Class
End Namespace

