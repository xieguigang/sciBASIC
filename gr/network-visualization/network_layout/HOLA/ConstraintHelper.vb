#Region "Microsoft.VisualBasic::d32b70fcb383f42ac877afc5202b68ab, gr\network-visualization\network_layout\HOLA\ConstraintHelper.vb"

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

    '   Total Lines: 107
    '    Code Lines: 65 (60.75%)
    ' Comment Lines: 30 (28.04%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 12 (11.21%)
    '     File Size: 4.84 KB


    '     Module ConstraintHelper
    ' 
    ' 
    '         Enum Axis
    ' 
    '             Horizontal, Vertical
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: GetCoord, ProjectConstraints
    ' 
    '     Sub: SetCoord
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola
Imports Microsoft.VisualBasic.Language

Namespace Hola

    ''' <summary>
    ''' 封装对 CoLa 约束求解器（投影梯度下降）的调用，供 HOLA 各松弛阶段复用。
    ''' 坐标系统一为 GDI（y 向下），NORTH 对应 y 减小。
    ''' </summary>
    Public Module ConstraintHelper

        ''' <summary>
        ''' 坐标轴枚举：HORIZONTAL 处理 x 值，VERTICAL 处理 y 值。
        ''' </summary>
        Public Enum Axis
            Horizontal
            Vertical
        End Enum

        ''' <summary>
        ''' 读取某个节点在指定轴上的坐标分量。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetCoord(p As FDGVector2, ax As Axis) As Double
            If ax = Axis.Horizontal Then Return p.x Else Return p.y
        End Function

        ''' <summary>
        ''' 写入某个节点在指定轴上的坐标分量（保持另一轴不变）。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetCoord(ByRef p As FDGVector2, ax As Axis, value As Double)
            If ax = Axis.Horizontal Then
                p = New FDGVector2(value, p.y)
            Else
                p = New FDGVector2(p.x, value)
            End If
        End Sub

        ''' <summary>
        ''' 在单一坐标轴上对一组节点施加约束并投影求解。
        ''' 约束语义遵循 CoLa：<c>right.coord - left.coord >= gap</c>；
        ''' 当 <paramref name="equality"/> 为 True 时退化为对齐约束 <c>right.coord == left.coord + gap</c>
        '''（gap 取 0 即强制右节点与左节点对齐）。
        ''' </summary>
        ''' <param name="state">布局状态（坐标会被原地更新）</param>
        ''' <param name="ax">投影轴</param>
        ''' <param name="pairs">约束对 (leftIndex, rightIndex, gap)</param>
        ''' <param name="equality">True 表示对齐（等式）约束，False 表示分离（不等式）约束</param>
        ''' <param name="opts">算法参数（使用 maxIterations）</param>
        ''' <returns>投影后最大的单步坐标变化量，用于收敛判断</returns>
        Public Function ProjectConstraints(state As HolaLayoutState,
                                           ax As Axis,
                                           pairs As (left As Integer, right As Integer, gap As Double)(),
                                           equality As Boolean,
                                           opts As HolaOptions) As Double
            If pairs.Length = 0 Then Return 0.0

            ' 收集参与约束的变量（去重）
            Dim used As New HashSet(Of Integer)
            For Each pr In pairs
                used.Add(pr.left)
                used.Add(pr.right)
            Next
            Dim vars = used.ToArray
            Array.Sort(vars)

            ' 建立 variableIndex -> 在 variables 数组中的位置
            Dim vmap As New Dictionary(Of Integer, Integer)
            Dim variables(used.Count - 1) As Variable
            For k As Integer = 0 To vars.Length - 1
                Dim idx = vars(k)
                vmap(idx) = k
                Dim c = GetCoord(state.positions(idx), ax)
                ' 期望值即当前值（投影梯度下降的初值）；weight 取 1 保持中性
                variables(k) = New Variable(c, 1.0)
                variables(k).index = k
            Next

            ' 构建约束：CoLa 语法 Constraint(left, right, gap, equality) 表示 right - left >= gap
            Dim constraints(pairs.Length - 1) As Constraint
            For i As Integer = 0 To pairs.Length - 1
                Dim pr = pairs(i)
                constraints(i) = New Constraint(variables(vmap(pr.left)), variables(vmap(pr.right)), pr.gap, equality)
            Next

            Dim solver = New Solver(variables, constraints)
            solver.solve()

            ' 写回坐标并计算最大位移
            Dim maxMove As Double = 0.0
            For k As Integer = 0 To vars.Length - 1
                Dim idx = vars(k)
                Dim newC = variables(k).desiredPosition
                Dim oldC = GetCoord(state.positions(idx), ax)
                Dim move = System.Math.Abs(newC - oldC)
                If move > maxMove Then maxMove = move
                SetCoord(state.positions(idx), ax, newC)
            Next

            Return maxMove
        End Function
    End Module
End Namespace
