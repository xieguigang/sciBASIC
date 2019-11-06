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
            Dim nodeLabel$

            If node Is Nothing Then
                nodeLabel = "<none>"
            Else
                nodeLabel = node.label
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
                               Return Not cell.node Is Nothing
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
            If index.Y > 0 Then
                If index.X > 0 Then
                    Yield gridCells(index.Y - 1)(index.X - 1) ' 左上
                End If

                Yield gridCells(index.Y - 1)(index.X)     ' 上

                If index.X < size.Width Then
                    Yield gridCells(index.Y - 1)(index.X + 1) ' 右上
                End If
            End If

            If index.X < size.Width Then
                Yield gridCells(index.Y)(index.X + 1)     ' 右
            End If

            If index.Y < size.Height Then
                If index.X < size.Width Then
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
            Dim node As Node = g.GetNode(targetNode)
            Dim fromCell As GridCell = nodes(node.label)

            Call moveNode(fromCell, [to], node)
        End Sub

        Private Sub moveNode(fromCell As GridCell, toCell As GridCell, node As Node)
            nodes(node.label) = toCell
            toCell.PutNode(node)
            fromCell.RemoveNode()

            If toCell.node Is Nothing AndAlso fromCell.node Is Nothing Then
                Throw New NoNullAllowedException
            End If
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
                cell = gridCells(y(++j))(x(++i))

                Call cell.PutNode(node)
                Call nodes.Add(node.label, cell)
            Next

            Return Me
        End Function
    End Class
End Namespace