#Region "Microsoft.VisualBasic::22ece2099416edf696da00f872fe4a5f, sciBASIC#\Data_science\MachineLearning\CellularAutomaton\SimulatorModel\Simulator.vb"

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

    '   Total Lines: 101
    '    Code Lines: 72
    ' Comment Lines: 14
    '   Blank Lines: 15
    '     File Size: 2.83 KB


    ' Class Simulator
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: RandomCells, Snapshot
    ' 
    '     Sub: [Step], Run, runRandom
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.Framework
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Public Class Simulator(Of T As Individual) : Inherits Iterator.Kernel

    Friend ReadOnly grid As CellEntity(Of T)()()
    Public ReadOnly size As Size
    Friend ReadOnly neighborType As NeighborhoodType
    Friend ReadOnly boundary As BoundaryMode

    ''' <summary>
    ''' 获取指定行列坐标处的细胞。坐标越界时依据边界模式处理：
    ''' <see cref="BoundaryMode.Bounded"/> 返回 Nothing（视为无邻居）；
    ''' <see cref="BoundaryMode.Toroidal"/> 环绕缝合。
    ''' </summary>
    ''' <param name="i">行编号</param>
    ''' <param name="j">列编号</param>
    ''' <returns></returns>
    Default Public ReadOnly Property getCell(i As Integer, j As Integer) As CellEntity(Of T)
        Get
            If boundary = BoundaryMode.Toroidal Then
                i = ((i Mod size.Height) + size.Height) Mod size.Height
                j = ((j Mod size.Width) + size.Width) Mod size.Width

                Return grid(i)(j)
            Else
                If i < 0 OrElse j < 0 Then
                    Return Nothing
                ElseIf i >= size.Height OrElse j >= size.Width Then
                    Return Nothing
                Else
                    Return grid(i)(j)
                End If
            End If
        End Get
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="size">the grid size</param>
    ''' <param name="activator">用于初始化每个细胞的工厂函数。</param>
    ''' <param name="neighborType">邻居拓扑类型，默认冯·诺依曼型。</param>
    ''' <param name="boundary">边界处理模式，默认有界。</param>
    Sub New(size As Size,
            activator As Func(Of T),
            Optional neighborType As NeighborhoodType = NeighborhoodType.VonNeumann,
            Optional boundary As BoundaryMode = BoundaryMode.Bounded)
        Dim grid = RectangularArray.Matrix(Of CellEntity(Of T))(size.Height, size.Width)

        Me.grid = grid
        Me.size = size
        Me.neighborType = neighborType
        Me.boundary = boundary

        For j As Integer = 0 To size.Width - 1
            For i As Integer = 0 To size.Height - 1
                grid(i)(j) = New CellEntity(Of T)(i, j, activator())
            Next
        Next

        For j As Integer = 0 To size.Width - 1
            For i As Integer = 0 To size.Height - 1
                Call grid(i)(j).config(Me, neighborType)
            Next
        Next
    End Sub

    Public Overloads Sub Run(Optional random As Boolean = True)
        If random Then
            Call runRandom()
        Else
            Call [Step](Nothing)
        End If
    End Sub

    ''' <summary>
    ''' 同步演化一个世代：先让所有细胞依据邻居“当前状态”计算下一代（Tick），
    ''' 再统一提交（Commit），避免同世代内顺序更新污染。
    ''' </summary>
    ''' <param name="itr"></param>
    Protected Overrides Sub [Step](itr As Integer)
        ' 阶段一：计算全部细胞的下一代状态（仅写入缓冲）
        For j As Integer = 0 To size.Width - 1
            For i As Integer = 0 To size.Height - 1
                Call grid(i)(j).Tick()
            Next
        Next

        ' 阶段二：统一提交下一代状态
        For j As Integer = 0 To size.Width - 1
            For i As Integer = 0 To size.Height - 1
                Call grid(i)(j).Commit()
            Next
        Next
    End Sub

    Private Sub runRandom()
        Dim x As Integer() = size.Width.SeqRandom
        Dim y As Integer() = size.Height.SeqRandom

        ' 阶段一：以随机顺序计算下一代状态（仅写入缓冲）
        For Each xi As Integer In x
            For Each yi As Integer In y
                Call grid(yi)(xi).Tick()
            Next
        Next

        ' 阶段二：统一提交
        For Each xi As Integer In x
            For Each yi As Integer In y
                Call grid(yi)(xi).Commit()
            Next
        Next
    End Sub

    Public Iterator Function RandomCells() As IEnumerable(Of CellEntity(Of T))
        Dim y, x As Integer

        Do While True
            x = randf.seeds.Next(0, size.Width)
            y = randf.seeds.Next(0, size.Height)

            Yield grid(y)(x)
        Loop
    End Function

    Public Iterator Function Snapshot() As IEnumerable(Of T)
        For Each row In grid
            For Each individual In row
                Yield individual.data
            Next
        Next
    End Function

    ''' <summary>
    ''' 获取指定行列处细胞所承载的个体数据，便于外部读取/写入状态（如渲染或鼠标绘制）。
    ''' 越界时依据边界模式：有界返回 Nothing；环面返回环绕后的细胞。
    ''' </summary>
    ''' <param name="i">行编号</param>
    ''' <param name="j">列编号</param>
    ''' <returns></returns>
    Public Function CellData(i As Integer, j As Integer) As T
        Dim c = Me(i, j)

        If c Is Nothing Then
            Return Nothing
        Else
            Return c.Value
        End If
    End Function
End Class
