#Region "Microsoft.VisualBasic::2704b38dd7d42f0e6e52235b117121a7, sciBASIC#\gr\network-visualization\Datavisualization.Network\Layouts\Orthogonal\Grid.vb"

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

    '   Total Lines: 277
    '    Code Lines: 186
    ' Comment Lines: 43
    '   Blank Lines: 48
    '     File Size: 9.34 KB


    '     Class GridCell
    ' 
    '         Properties: location
    ' 
    '         Function: ToString
    ' 
    '         Sub: PutNode, RemoveNode
    ' 
    '     Class Grid
    ' 
    '         Properties: actualSize, GetAllNodeFilledCells, size
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FindCell, FindIndex, GetAdjacentCells, PutRandomNodes
    ' 
    '         Sub: moveNode, (+2 Overloads) MoveNode, SwapNode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Linq
Imports GridIndex = Microsoft.VisualBasic.Data.GraphTheory.Grid
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Layouts.Orthogonal

    Public Class GridCell : Inherits GridCell(Of Node)

        ''' <summary>
        ''' 实际的物理位置
        ''' </summary>
        ''' <returns></returns>
        Public Property location As PointF

        ''' <summary>
        ''' 将目标指定的节点<paramref name="node"/>对象放置在当前的单元格内，然后更新坐标位置为当前的单元格的位置
        ''' </summary>
        ''' <param name="node"></param>
        Public Sub PutNode(node As Node)
            Me.data = node

            node.data.initialPostion.x = location.X
            node.data.initialPostion.y = location.Y
        End Sub

        Public Sub RemoveNode()
            Me.data = Nothing
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Dim nodeLabel$

            If data Is Nothing Then
                nodeLabel = "<none>"
            Else
                nodeLabel = data.label
            End If

            Return $"[{index.X},{index.Y}] x:={location.X}, y:={location.Y}; {nodeLabel}"
        End Function

    End Class

    Public Class Grid

        Dim gridCells As GridCell()()
        Dim gridIndex As GridIndex
        Dim nodes As New Dictionary(Of String, GridCell)
        Dim g As NetworkGraph

        ''' <summary>
        ''' 单元格的数量，单位为个
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property size As Size
        ''' <summary>
        ''' 网格的实际物理大小，单位为像素
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property actualSize As Size

        Default Public ReadOnly Property GetCell(index As Point) As GridCell
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return gridCells(index.Y)(index.X)
            End Get
        End Property

        Public ReadOnly Property GetAllNodeFilledCells As GridCell()
            Get
                Return gridCells _
                    .IteratesALL _
                    .Where(Function(cell)
                               Return Not cell.data Is Nothing
                           End Function) _
                    .ToArray
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="size"></param>
        ''' <param name="cellSize"><see cref="GridCellSize"/></param>
        Sub New(size As Size, cellSize#)
            Dim y As Double = 0
            Dim index As Integer

            Me.size = size
            Me.actualSize = New Size With {
                .Width = size.Width * cellSize,
                .Height = size.Height * cellSize
            }

            gridCells = New GridCell(size.Width - 1)() {}
            gridIndex = New GridIndex(actualSize, New SizeF(cellSize, cellSize))

            For i As Integer = 0 To gridCells.Length - 1
                index = i
                gridCells(i) = size.Width _
                    .Sequence() _
                    .Select(Function(ix)
                                Return New GridCell With {
                                    .index = New Point With {.X = ix, .Y = index},
                                    .location = New PointF With {.X = ix * cellSize, .Y = y}
                                }
                            End Function) _
                    .ToArray
                y += cellSize
            Next
        End Sub

        Public Sub SwapNode(a As Point, b As Point)
            Dim x As GridCell = Me(a)
            Dim y As GridCell = Me(b)

            If x.data Is Nothing Then
                Call MoveNode(b, a)
            ElseIf y.data Is Nothing Then
                Call MoveNode(a, b)
            Else
                Dim vi = x.data
                Dim vj = y.data

                nodes(vi.label) = y
                y.PutNode(vi)

                nodes(vj.label) = x
                x.PutNode(vj)
            End If
        End Sub

        Public Sub MoveNode(from As Point, [to] As Point)
            Dim fromCell As GridCell = Me(from)
            Dim toCell As GridCell = Me([to])
            Dim node As Node = fromCell.data

            Call moveNode(fromCell, toCell, node)
        End Sub

        ''' <summary>
        ''' Otherwise, we try to swap it with the nodes nearby. We do this
        ''' by checking the nodes residing in adjacent grid cells To vj.
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Public Iterator Function GetAdjacentCells(index As Point) As IEnumerable(Of GridCell)
            If index.Y > 0 Then
                If index.X > 0 Then
                    Yield gridCells(index.Y - 1)(index.X - 1) ' 左上
                End If

                Yield gridCells(index.Y - 1)(index.X)     ' 上

                If index.X < size.Width - 1 Then
                    Yield gridCells(index.Y - 1)(index.X + 1) ' 右上
                End If
            End If

            If index.X < size.Width - 1 Then
                Yield gridCells(index.Y)(index.X + 1)     ' 右
            End If

            If index.Y < size.Height - 1 Then
                If index.X < size.Width - 1 Then
                    Yield gridCells(index.Y + 1)(index.X + 1) ' 右下
                End If

                Yield gridCells(index.Y + 1)(index.X)     ' 下

                If index.X > 0 Then
                    Yield gridCells(index.Y + 1)(index.X - 1) ' 左下
                End If
            End If

            If index.X > 0 Then
                Yield gridCells(index.Y)(index.X - 1)     ' 左
            End If
        End Function

        ''' <summary>
        ''' 将<paramref name="targetNode"/>所代表的节点对象移动到目标<paramref name="to"/>单元格
        ''' </summary>
        ''' <param name="targetNode$"></param>
        ''' <param name="[to]"></param>
        Public Sub MoveNode(targetNode$, [to] As GridCell)
            Dim node As Node = g.GetElementByID(targetNode)
            Dim fromCell As GridCell = nodes(node.label)

            Call moveNode(fromCell, [to], node)
        End Sub

        Private Sub moveNode(fromCell As GridCell, toCell As GridCell, node As Node)
            nodes(node.label) = toCell
            toCell.PutNode(node)
            fromCell.RemoveNode()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FindIndex(x#, y#) As Point
            Dim index As Point = gridIndex.Index(x, y)
            Dim ix, iy As Integer

            If x <= 0 Then
                ix = 0
            ElseIf index.X >= size.Width Then
                ix = size.Width - 1
            Else
                ix = index.X
            End If

            If y <= 0 Then
                iy = 0
            ElseIf index.Y >= size.Height Then
                iy = size.Height - 1
            Else
                iy = index.Y
            End If

            Return New Point(ix, iy)
        End Function

        ''' <summary>
        ''' Get node by label and the associated grid cell.
        ''' </summary>
        ''' <param name="node"><see cref="Node.label"/></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FindCell(node As String) As GridCell
            Return nodes(node)
        End Function

        ''' <summary>
        ''' 将网络中的节点随机的放置在网格上面
        ''' </summary>
        ''' <param name="network"></param>
        ''' <returns></returns>
        Public Function PutRandomNodes(network As NetworkGraph) As Grid
            Dim x As Integer() = size.Width.SeqRandom
            Dim y As Integer() = size.Height.SeqRandom
            Dim cell As GridCell
            Dim V As New Pointer(Of Node)(network.vertex)
            Dim used As New Index(Of String)
            Dim i, j As Integer

            g = network

            Do While Not V
RE_SEED:
                i = randf.seeds.Next(0, x.Length)
                j = randf.seeds.Next(0, y.Length)

                If $"{i}-{j}" Like used Then
                    GoTo RE_SEED
                Else
                    used += $"{i}-{j}"
                End If

                cell = gridCells(y(j))(x(i))

                Call cell.PutNode(++V)
                Call nodes.Add(cell.data.label, cell)
            Loop

            Return Me
        End Function
    End Class
End Namespace
