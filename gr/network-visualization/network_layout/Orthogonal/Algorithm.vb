#Region "Microsoft.VisualBasic::b419f71fbc372c10ee4954c4892f31bd, sciBASIC#\gr\network-visualization\Datavisualization.Network\Layouts\Orthogonal\Algorithm.vb"

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

    '   Total Lines: 267
    '    Code Lines: 180
    ' Comment Lines: 46
    '   Blank Lines: 41
    '     File Size: 11.42 KB


    '     Module Algorithm
    ' 
    '         Function: neighboursMedianX, neighboursMedianY, ResetNodeSize
    ' 
    '         Sub: DoLayout, SwapNearbyNode, TrySwapTwoNode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce
Imports Microsoft.VisualBasic.Math.Statistics.Linq
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports stdNum = System.Math

Namespace Layouts.Orthogonal

    Public Module Algorithm

        <Extension>
        Public Function ResetNodeSize(g As NetworkGraph, size$) As NetworkGraph
            Dim sizeVals As Double() = size.Split(","c).Select(AddressOf Val).ToArray

            For Each node As Node In g.vertex
                node.data.size = sizeVals
            Next

            Return g
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="graph"></param>
        ''' <param name="gridSize"></param>
        ''' <param name="delta">
        ''' some minimum distance between node has to be ensured
        ''' </param>
        <Extension>
        Public Sub DoLayout(graph As NetworkGraph, gridSize As Size,
                            Optional delta# = 1,
                            Optional defaultNodeSize# = 1,
                            Optional iterationCount% = -1,
                            Optional debug As Boolean = True)

            ' 只针对非孤立的网络节点来进行布局的计算
            ' 孤立节点会在for循环中的swap步骤进行被动布局
            Dim V As Node() = graph.GetConnectedVertex.ToArray
            Dim compactionDir = True

            If iterationCount <= 0 Then
                iterationCount = 100 * V.Length
            End If

            For i As Integer = 0 To V.Length - 1
                If V(i).data.size.IsNullOrEmpty Then
                    V(i).data.size = {defaultNodeSize, defaultNodeSize}
                ElseIf V(i).data.size.Length = 1 Then
                    V(i).data.size = {V(i).data.size(Scan0), V(i).data.size(Scan0)}
                End If
                If V(i).data.initialPostion Is Nothing Then
                    V(i).data.initialPostion = AbstractVector.Vector2D(0, 0)
                End If
            Next

            ' T的作用是用来计算交换的范围
            ' 随着迭代的进行T将会越来越小
            ' 交换的范围从开始的非常大到最终的非常小
            ' 从而使网络的布局变化从变化剧烈到稳定
            Dim T As Double = 2 * V.Length
            Dim k As Double = (0.2 / T) ^ (1 / iterationCount)
            Dim cellSize As Double = V.GridCellSize

            Call "Initialize grid model....".__INFO_ECHO

            Dim grid As New Grid(gridSize, cellSize)
            Dim workspace As New Workspace With {
                .g = graph,
                .grid = grid,
                .V = V,
                .cellSize = cellSize,
                .width = V.ToDictionary(Function(n) n.label, Function(n) n.width(cellSize, delta)),
                .height = V.ToDictionary(Function(n) n.label, Function(n) n.height(cellSize, delta))
            }

            Call "Create random node layout...".__INFO_ECHO

            Call grid.PutRandomNodes(graph)

            Call "Running layout iteration...".__INFO_ECHO
            Call "Phase 1".__INFO_ECHO

            Dim [stop] = iterationCount \ 2
            Dim totalEdgeLength As Double = workspace.totalEdgeLength

            For i As Integer = 0 To [stop]
                For j As Integer = 0 To V.Length - 1
                    ' To perform local optimization, every node is moved to a location that minimizes
                    ' the total length of its adjacent edges.
                    Dim x = graph.neighboursMedianX(V(j)) + randf.randf(-T, T)
                    Dim y = graph.neighboursMedianY(V(j)) + randf.randf(-T, T)
                    Dim cell As Point = grid.FindIndex(x, y)
                    Dim gridCell As GridCell = grid(cell)
                    Dim currentCell As GridCell = grid.FindCell(V(j).label)

                    ' if vj has not changed it’s place from the previous iteration then
                    If Not gridCell Is currentCell AndAlso Not gridCell.data Is V(j) Then
                        ' Call grid.SwapNode(currentCell.index, gridCell.index)
                        Call workspace.TrySwapTwoNode(origin:=gridCell, target:=currentCell)
                    Else
                        ' Try to swap vj with nodes nearby;
                        Call workspace.SwapNearbyNode(origin:=gridCell)
                    End If
                Next

                If debug Then
                    Call Console.WriteLine("[{0}] {1}%, T={2}, total:={3}", i, (100 * i / [stop]).ToString("F2"), T, totalEdgeLength)
                End If

                If iterationCount Mod 9 = 0 Then
                    workspace.compact(compactionDir, 3, False)
                    compactionDir = Not compactionDir
                End If

                T = T * k
                totalEdgeLength = workspace.totalEdgeLength
            Next

            Call "Phase 2".__INFO_ECHO

            workspace.compact(True, 3, True)
            workspace.compact(False, 3, True)

            For i As Integer = iterationCount \ 2 + 1 To iterationCount
                For j As Integer = 0 To V.Length - 1
                    Dim wj = workspace.width(V(j).label)
                    Dim hj = workspace.height(V(j).label)
                    Dim x = graph.neighboursMedianX(V(j)) + randf.randf(-T * wj, T * wj)
                    Dim y = graph.neighboursMedianY(V(j)) + randf.randf(-T * hj, T * hj)
                    Dim cell As Point = grid.FindIndex(x, y)
                    Dim gridCell As GridCell = grid(cell)
                    Dim currentCell As GridCell = grid.FindCell(V(j).label)

                    ' if vj has not changed it’s place from the previous iteration then
                    If Not gridCell Is currentCell AndAlso Not gridCell.data Is V(j) Then
                        ' Call grid.SwapNode(currentCell.index, gridCell.index)
                        Call workspace.TrySwapTwoNode(origin:=gridCell, target:=currentCell)
                    Else
                        ' Try to swap vj with nodes nearby;
                        Call workspace.SwapNearbyNode(origin:=gridCell)
                    End If
                Next

                If debug Then
                    Call Console.WriteLine("[{0}] {1}%, T={2}", i, (100 * i / iterationCount).ToString("F2"), T)
                End If

                If iterationCount Mod 9 = 0 Then
                    workspace.compact(compactionDir, stdNum.Max(1, 1 + 2 * (iterationCount - i - 30) / (0.5 * iterationCount)), False)
                    compactionDir = Not compactionDir
                End If

                T = T * k
            Next

            Call "Do layout job done!".__INFO_ECHO
        End Sub

        <Extension>
        Private Sub TrySwapTwoNode(workspace As Workspace, origin As GridCell, target As GridCell)
            Dim totalLenBefore As Double = workspace.totalEdgeLength
            Dim totalIntersectionsBefore As Double = workspace.totalIntersections
            Dim gainLen As Double
            Dim gainInter As Double

            If target.data Is Nothing Then
                ' 附近的单元格是没有节点的，直接放置进去?
                Call workspace.grid.MoveNode(origin.index, target.index)
            Else
                Call workspace.grid.SwapNode(origin.index, target.index)
            End If

            gainLen = workspace.totalEdgeLength - totalLenBefore
            gainInter = totalIntersectionsBefore - workspace.totalIntersections

            ' 目的是减少相交的边连接
            ' If gainLen * gainInter > 0 Then
            If gainLen < 0 AndAlso gainInter > 0 Then
                Return
                'End If
                '
                'totalAfter = workspace.totalEdgeLength
                'gain = totalAfter - totalLenBefore
                '
                'If gain > 0 Then
                'Exit For
            Else
                ' restore
                Call workspace.grid.SwapNode(origin.index, target.index)
            End If
        End Sub

        <Extension>
        Private Sub SwapNearbyNode(workspace As Workspace, origin As GridCell)
            ' 因为为了避免重复计算totalEdgeLength而带来的性能损失
            ' 在这里就不和TrySwapTwoNode函数的调用进行合并了
            Dim totalLenBefore As Double = workspace.totalEdgeLength
            Dim totalIntersectionsBefore As Double = workspace.totalIntersections
            Dim gainLen As Double
            Dim gainInter As Double

            For Each nearby As GridCell In workspace.grid.GetAdjacentCells(origin.index).Shuffles
                If nearby.data Is Nothing Then
                    ' 附近的单元格是没有节点的，直接放置进去?
                    Call workspace.grid.MoveNode(origin.index, nearby.index)
                Else
                    Call workspace.grid.SwapNode(origin.index, nearby.index)
                End If

                gainLen = workspace.totalEdgeLength - totalLenBefore
                gainInter = totalIntersectionsBefore - workspace.totalIntersections

                ' 目的是减少相交的边连接
                ' If gainLen * gainInter > 0 Then
                If gainLen < 0 AndAlso gainInter > 0 Then
                    Exit For
                    'End If
                    '
                    'totalAfter = workspace.totalEdgeLength
                    'gain = totalAfter - totalLenBefore
                    '
                    'If gain > 0 Then
                    'Exit For
                Else
                    ' restore
                    Call workspace.grid.SwapNode(origin.index, nearby.index)
                End If
            Next
        End Sub

        <Extension>
        Private Function neighboursMedianX(g As NetworkGraph, v As Node) As Double
            Dim edges = g.GetEdges(v).ToArray
            Dim x = edges _
                .Select(Function(e)
                            If e.U Is v Then
                                Return e.V.data.initialPostion.x
                            Else
                                Return e.U.data.initialPostion.x
                            End If
                        End Function) _
                .ToArray
            Dim median As Double = x.Median

            Return median
        End Function

        <Extension>
        Private Function neighboursMedianY(g As NetworkGraph, v As Node) As Double
            Dim edges = g.GetEdges(v)
            Dim y = edges _
                .Select(Function(e)
                            If e.U Is v Then
                                Return e.V.data.initialPostion.y
                            Else
                                Return e.U.data.initialPostion.y
                            End If
                        End Function)
            Dim median = y.Median

            Return median
        End Function
    End Module
End Namespace
