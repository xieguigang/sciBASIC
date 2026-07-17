#Region "Microsoft.VisualBasic::00000000000000000000000000000000, sciBASIC#\Data_science\MachineLearning\CellularAutomaton\SimulatorModel\Neighborhood.vb"

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

' Class NeighborhoodType
' 
'     Enum: VonNeumann, Moore, ExtendedMoore
' 
' Class BoundaryMode
' 
'     Enum: Bounded, Toroidal
' 
' Module Neighborhoods
' 
'     Function: Offsets
' 
' /********************************************************************************/

#End Region

Imports System.Drawing

''' <summary>
''' 元胞自动机的邻居拓扑类型。
''' </summary>
Public Enum NeighborhoodType
    ''' <summary>
    ''' 冯·诺依曼型（Von Neumann）：切比雪夫半径 1 的十字形，共 4 个邻居（上、下、左、右）。
    ''' </summary>
    VonNeumann
    ''' <summary>
    ''' 摩尔型（Moore）：切比雪夫半径 1 的正方形包围，共 8 个邻居。
    ''' </summary>
    Moore
    ''' <summary>
    ''' 扩展摩尔型（Extended Moore）：切比雪夫半径 2 的扩展正方形包围，共 24 个邻居。
    ''' </summary>
    ExtendedMoore
End Enum

''' <summary>
''' 网格边界处理模式。
''' </summary>
Public Enum BoundaryMode
    ''' <summary>
    ''' 有界边界：越界位置视为不存在邻居（等同于死细胞）。
    ''' </summary>
    Bounded
    ''' <summary>
    ''' 环面边界：左右、上下缝合环绕（toroidal wrapping）。
    ''' </summary>
    Toroidal
End Enum

''' <summary>
''' 邻居拓扑偏移表的生成器。各拓扑的偏移集合集中维护，新增拓扑仅需扩充本模块，符合开闭原则。
''' </summary>
Public Module Neighborhoods

    ''' <summary>
    ''' 返回指定邻居类型相对于中心细胞的相对偏移集合（不含中心 (0,0)）。
    ''' </summary>
    ''' <param name="type">邻居拓扑类型。</param>
    ''' <returns>相对偏移点数组，每个点的 X 为列偏移、Y 为行偏移。</returns>
    Public Function Offsets(type As NeighborhoodType) As Point()
        Select Case type
            Case NeighborhoodType.VonNeumann
                Return VonNeumannOffsets()
            Case NeighborhoodType.Moore
                Return MooreOffsets(1)
            Case NeighborhoodType.ExtendedMoore
                Return MooreOffsets(2)
            Case Else
                Return VonNeumannOffsets()
        End Select
    End Function

    ''' <summary>
    ''' 冯·诺依曼型：曼哈顿距离 1 的 4 个轴向邻居。
    ''' </summary>
    Private Function VonNeumannOffsets() As Point()
        Return {
            New Point(0, -1),  ' 上
            New Point(0, 1),   ' 下
            New Point(-1, 0),  ' 左
            New Point(1, 0)    ' 右
        }
    End Function

    ''' <summary>
    ''' 摩尔型（含扩展摩尔型）：切比雪夫半径 r 内、除中心外的所有邻居。
    ''' 半径 1 共 8 个；半径 2 共 24 个。
    ''' </summary>
    ''' <param name="radius">切比雪夫半径。</param>
    Private Function MooreOffsets(radius As Integer) As Point()
        Dim list As New List(Of Point)

        For dy As Integer = -radius To radius
            For dx As Integer = -radius To radius
                If dx = 0 AndAlso dy = 0 Then
                    Continue For
                End If

                list.Add(New Point(dx, dy))
            Next
        Next

        Return list.ToArray
    End Function
End Module
