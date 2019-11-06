Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports GridIndex = Microsoft.VisualBasic.Data.GraphTheory.Grid

Namespace Layouts.Orthogonal

    Public Class GridCell

        Public Property index As Point
        Public Property location As PointF

        Public ReadOnly Property node As Node

        ''' <summary>
        ''' 将目标指定的节点<paramref name="node"/>对象放置在当前的单元格内，然后更新坐标位置为当前的单元格的位置
        ''' </summary>
        ''' <param name="node"></param>
        Public Sub PutNode(node As Node)
            Me._node = node

            node.data.initialPostion.x = location.X
            node.data.initialPostion.y = location.Y
        End Sub

        Public Sub RemoveNode()
            Me._node = Nothing
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"[{index.X},{index.Y}] x:={location.X}, y:={location.Y}; {node.label}"
        End Function

    End Class

    Public Class Grid

        Dim gridCells As GridCell()()
        Dim gridIndex As GridIndex
        Dim nodes As New Dictionary(Of String, GridCell)
        Dim g As NetworkGraph

        Public ReadOnly Property size As Size
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New Size(gridCells.Length, gridCells(0).Length)
            End Get
        End Property

        Default Public ReadOnly Property GetCell(index As Point) As GridCell
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return gridCells(index.X)(index.Y)
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="size"></param>
        ''' <param name="cellSize"><see cref="GridCellSize"/></param>
        Sub New(size As Size, cellSize#)
            Dim y As Double = 0

            gridCells = New GridCell(size.Width - 1)() {}
            gridIndex = New GridIndex(size, New SizeF(cellSize, cellSize))

            For i As Integer = 0 To gridCells.Length - 1
                gridCells(i) = size.Width _
                    .Sequence() _
                    .Select(Function(ix)
#Disable Warning
                                Return New GridCell With {
                                    .index = New Point With {.X = ix, .Y = i},
                                    .location = New PointF With {.X = ix * cellSize, .Y = y}
                                }
#Enable Warning
                            End Function) _
                    .ToArray
            Next
        End Sub

        Public Sub MoveNode(from As Point, [to] As Point)
            Dim fromCell As GridCell = Me(from)
            Dim toCell As GridCell = Me([to])
            Dim node As Node = fromCell.node

            Call moveNode(fromCell, toCell, node)
        End Sub

        ''' <summary>
        ''' Otherwise, we try to swap it with the nodes nearby. We do this
        ''' by checking the nodes residing in adjacent grid cells To vj.
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Public Iterator Function GetAdjacentCells(index As Point) As IEnumerable(Of GridCell)
            Yield gridCells(index.X - 1)(index.Y - 1)  ' 左上
            Yield gridCells(index.X)(index.Y - 1)      ' 上
            Yield gridCells(index.X + 1)(index.Y - 1)  ' 右上
            Yield gridCells(index.X + 1)(index.Y)      ' 右
            Yield gridCells(index.X + 1)(index.Y + 1)  ' 右下
            Yield gridCells(index.X)(index.Y + 1)      ' 下
            Yield gridCells(index.X - 1)(index.Y + 1)  ' 左下
            Yield gridCells(index.X - 1)(index.Y)      ' 左
        End Function

        ''' <summary>
        ''' 将<paramref name="targetNode"/>所代表的节点对象移动到目标<paramref name="to"/>单元格
        ''' </summary>
        ''' <param name="targetNode$"></param>
        ''' <param name="[to]"></param>
        Public Sub MoveNode(targetNode$, [to] As GridCell)
            Dim node As Node = g.GetNode(targetNode)
            Dim fromCell As GridCell = nodes(node.label)

            Call moveNode(fromCell, [to], node)
        End Sub

        Private Sub moveNode(fromCell As GridCell, toCell As GridCell, node As Node)
            nodes(node.label) = toCell
            toCell.PutNode(node)
            fromCell.RemoveNode()
        End Sub

        Public Sub SwapNode(a As Point, b As Point)
            Dim x As GridCell = Me(a)
            Dim y As GridCell = Me(b)

            If x.node Is Nothing Then
                Call MoveNode(b, a)
            ElseIf y.node Is Nothing Then
                Call MoveNode(a, b)
            Else
                Dim vi = x.node
                Dim vj = y.node

                Call moveNode(x, y, vi)
                Call moveNode(y, x, vj)
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FindIndex(x#, y#) As Point
            Return gridIndex.Index(x, y)
        End Function

        ''' <summary>
        ''' Get node by label and the associated grid cell.
        ''' </summary>
        ''' <param name="node"><see cref="Node.label"/></param>
        ''' <returns></returns>
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
            Dim i As i32 = Scan0
            Dim j As i32 = Scan0
            Dim cell As GridCell

            g = network

            For Each node As Node In network.vertex
                cell = gridCells(x(++i))(y(++j))

                Call cell.PutNode(node)
                Call nodes.Add(node.label, cell)
            Next

            Return Me
        End Function
    End Class
End Namespace