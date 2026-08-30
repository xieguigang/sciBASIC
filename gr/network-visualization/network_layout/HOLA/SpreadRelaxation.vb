#Region "Microsoft.VisualBasic::f18afbf74a70988c2900ff491d79775e, gr\network-visualization\network_layout\HOLA\SpreadRelaxation.vb"

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
    '    Code Lines: 30 (58.82%)
    ' Comment Lines: 13 (25.49%)
    '    - Xml Docs: 92.31%
    ' 
    '   Blank Lines: 8 (15.69%)
    '     File Size: 2.40 KB


    '     Module SpreadRelaxation
    ' 
    '         Function: SpreadAxis
    ' 
    '         Sub: Relax
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Language

Namespace Hola

    ''' <summary>
    ''' HOLA 阶段 5：扩散松弛（Spread Relaxation）。
    ''' 在两个坐标轴上分别以一维扫描排序的方式施加分离约束（相邻节点间距 >= nodeGap），
    ''' 借助 CoLa 求解器投影，消除节点与边的重叠，使布局疏密得当。
    ''' </summary>
    Public Module SpreadRelaxation

        ''' <summary>
        ''' 执行扩散松弛：先沿 x 轴、再沿 y 轴扫描施加分离约束并投影求解，
        ''' 重复若干轮直到位移收敛或达到最大迭代。
        ''' </summary>
        Public Sub Relax(state As HolaLayoutState, opts As HolaOptions)
            For iter As Integer = 1 To opts.maxIterations
                Dim mx = SpreadAxis(state, ConstraintHelper.Axis.Horizontal, opts)
                Dim my = SpreadAxis(state, ConstraintHelper.Axis.Vertical, opts)

                If System.Math.Max(mx, my) < opts.convergeEpsilon Then
                    Exit For
                End If
            Next
        End Sub

        ''' <summary>
        ''' 在单一坐标轴上扫描：按坐标排序后，对相邻节点施加最小间距约束并投影。
        ''' </summary>
        Private Function SpreadAxis(state As HolaLayoutState, ax As ConstraintHelper.Axis, opts As HolaOptions) As Double
            Dim order(state.nodes.Length - 1) As Integer
            For i As Integer = 0 To order.Length - 1
                order(i) = i
            Next

            Array.Sort(order, Function(a As Integer, b As Integer)
                                  Return ConstraintHelper.GetCoord(state.positions(a), ax).CompareTo(
                                         ConstraintHelper.GetCoord(state.positions(b), ax))
                              End Function)

            ' 相邻节点之间的分离约束：后一个 - 前一个 >= nodeGap
            Dim pairs(state.nodes.Length - 2) As (left As Integer, right As Integer, gap As Double)
            For k As Integer = 0 To order.Length - 2
                pairs(k) = (order(k), order(k + 1), opts.nodeGap)
            Next

            Return ConstraintHelper.ProjectConstraints(state, ax, pairs, equality:=False, opts)
        End Function
    End Module
End Namespace
