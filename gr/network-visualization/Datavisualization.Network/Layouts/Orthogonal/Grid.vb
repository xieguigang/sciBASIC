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
        Public Property node As Node

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"[{index.X},{index.Y}] x:={location.X}, y:={location.Y}; {node.label}"
        End Function

    End Class

    Public Class Grid

        Dim gridCells As GridCell()()
        Dim gridIndex As GridIndex

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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FindIndex(x#, y#) As Point
            Return gridIndex.Index(x, y)
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

            For Each node As Node In network.vertex
                gridCells(x(++i))(y(++j)).node = node
            Next

            Return Me
        End Function
    End Class
End Namespace