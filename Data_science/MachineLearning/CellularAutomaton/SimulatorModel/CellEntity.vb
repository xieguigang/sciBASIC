#Region "Microsoft.VisualBasic::e406ac14d6d65a4eb4dd95457937f332, sciBASIC#\Data_science\MachineLearning\CellularAutomaton\SimulatorModel\CellEntity.vb"

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
'    Code Lines: 26
' Comment Lines: 0
'   Blank Lines: 7
'     File Size: 975.00 B


' Class CellEntity
' 
'     Constructor: (+1 Overloads) Sub New
' 
'     Function: getAdjacents
' 
'     Sub: config, Tick
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.GraphTheory.GridGraph

Public Class CellEntity(Of T As Individual) : Inherits GridCell(Of T)

    Protected adjacents As CellEntity(Of T)()

    Sub New(i As Integer, j As Integer, obj As T)
        data = obj
        index = New Point(i, j)
    End Sub

    ''' <summary>
    ''' 依据指定的邻居拓扑，一次性构建相邻细胞的引用集合。
    ''' 邻居数量随拓扑动态决定（冯·诺依曼 4 / 摩尔 8 / 扩展摩尔 24）。
    ''' </summary>
    ''' <param name="grid">所属模拟器，用于解析邻居坐标（含边界模式处理）。</param>
    ''' <param name="type">邻居拓扑类型。</param>
    Friend Sub config(grid As Simulator(Of T), type As NeighborhoodType)
        Dim offsets = Neighborhoods.Offsets(type)

        Me.adjacents = New CellEntity(Of T)(offsets.Length - 1) {}

        For i As Integer = 0 To offsets.Length - 1
            Dim dx = offsets(i).X
            Dim dy = offsets(i).Y

            adjacents(i) = grid(index.X + dx, index.Y + dy)
        Next
    End Sub

    Private Iterator Function getAdjacents() As IEnumerable(Of Individual)
        For Each cell As CellEntity(Of T) In adjacents
            If Not cell Is Nothing Then
                Yield cell.data
            End If
        Next
    End Function

    ''' <summary>
    ''' 计算下一代状态（写入缓冲，不改变当前状态）。
    ''' </summary>
    Sub Tick()
        Call data.Tick(getAdjacents)
    End Sub

    ''' <summary>
    ''' 提交下一代状态（双缓冲），应在全部细胞 <see cref="Tick"/> 完成后调用。
    ''' </summary>
    Sub Commit()
        Call data.Commit()
    End Sub

    ''' <summary>
    ''' 该细胞所承载的个体数据，供外部（如演示程序）读取与写入细胞状态。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Value As T
        Get
            Return data
        End Get
    End Property
End Class
