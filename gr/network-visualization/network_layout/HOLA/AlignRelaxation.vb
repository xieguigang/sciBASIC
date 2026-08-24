#Region "Microsoft.VisualBasic::8601903e3350dab9d0a86c453a91b813, gr\network-visualization\network_layout\HOLA\AlignRelaxation.vb"

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

    '   Total Lines: 45
    '    Code Lines: 25 (55.56%)
    ' Comment Lines: 13 (28.89%)
    '    - Xml Docs: 61.54%
    ' 
    '   Blank Lines: 7 (15.56%)
    '     File Size: 2.27 KB


    '     Module AlignRelaxation
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
    ''' HOLA 阶段 4：对齐松弛（Align Relaxation）。
    ''' 将坐标轴上相互接近（差距小于 alignEpsilon）的节点对强制对齐到同一网格线，
    ''' 形成轴对齐的正交结构，这是 HOLA "Human-like" 观感的关键来源。
    ''' </summary>
    Public Module AlignRelaxation

        ''' <summary>
        ''' 执行对齐松弛：在 x 轴与 y 轴上分别检测接近的节点对，施加等式对齐约束并投影求解。
        ''' </summary>
        Public Sub Relax(state As HolaLayoutState, opts As HolaOptions)
            ' 在 x 轴上对齐（使节点的 y 轴方向不强制，只对齐 x）
            Dim xPairs As New List(Of (left As Integer, right As Integer, gap As Double))
            ' 在 y 轴上对齐
            Dim yPairs As New List(Of (left As Integer, right As Integer, gap As Double))

            Dim n = state.nodes.Length
            For i As Integer = 0 To n - 1
                For j As Integer = i + 1 To n - 1
                    Dim dx = System.Math.Abs(state.positions(i).x - state.positions(j).x)
                    Dim dy = System.Math.Abs(state.positions(i).y - state.positions(j).y)

                    If dx <= opts.alignEpsilon Then
                        ' 两节点 x 接近：在 x 轴施加对齐（gap=0 的等式约束）
                        xPairs.Add((i, j, 0.0))
                    End If
                    If dy <= opts.alignEpsilon Then
                        yPairs.Add((i, j, 0.0))
                    End If
                Next
            Next

            ' 先处理 x 对齐，再处理 y 对齐；分轴投影避免互相干扰
            Dim mx = ConstraintHelper.ProjectConstraints(state, ConstraintHelper.Axis.Horizontal, xPairs.ToArray, equality:=True, opts)
            Dim my = ConstraintHelper.ProjectConstraints(state, ConstraintHelper.Axis.Vertical, yPairs.ToArray, equality:=True, opts)

            ' 对齐可能引入新的边交叉，这里不递归；由主控阶段再回到扫描松弛做一次清理。
        End Sub
    End Module
End Namespace

