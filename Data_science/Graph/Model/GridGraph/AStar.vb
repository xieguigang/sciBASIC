#Region "Microsoft.VisualBasic::2663fce1191daeee42ac105d3689e601, Data_science\Graph\Model\GridGraph\AStar.vb"

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

    '   Total Lines: 150
    '    Code Lines: 102 (68.00%)
    ' Comment Lines: 25 (16.67%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 23 (15.33%)
    '     File Size: 7.20 KB


    '     Class OrthogonalRouter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FindPath, ReconstructPath
    ' 
    '         Sub: FillObstacles
    ' 
    '     Class PathNode
    ' 
    '         Properties: F, G, H, Parent, Position
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports std = System.Math

Namespace GridGraph

    Public Class OrthogonalRouter

        ' 网格大小（像素），决定了路径的精细程度
        Dim _gridSize As Integer = 10
        ' 障碍物集合，存储被节点占据的区域
        Dim _obstacles As HashSet(Of GridCell(Of Rectangle))

        Public Sub New(obstacleRects As List(Of Rectangle), Optional gridSize As Integer = 10)
            _gridSize = gridSize
            _obstacles = New HashSet(Of GridCell(Of Rectangle))()

            ' 将节点矩形区域转换为网格点并加入障碍物集合
            For Each rect In obstacleRects
                FillObstacles(rect)
            Next
        End Sub

        ' 将矩形区域标记为障碍物
        Private Sub FillObstacles(rect As Rectangle)
            ' 将像素坐标转换为网格索引
            Dim startX As Integer = std.Floor(rect.X / _gridSize)
            Dim endX As Integer = std.Ceiling((rect.X + rect.Width) / _gridSize)
            Dim startY As Integer = std.Floor(rect.Y / _gridSize)
            Dim endY As Integer = std.Ceiling((rect.Y + rect.Height) / _gridSize)

            For x As Integer = startX To endX - 1
                For y As Integer = startY To endY - 1
                    _obstacles.Add(New GridCell(Of Rectangle)(x, y, rect))
                Next
            Next
        End Sub

        ' 计算路径
        ' startPt, endPt: 像素坐标
        Public Function FindPath(startPt As Point, endPt As Point) As List(Of Point)
            ' 1. 坐标离散化（转为网格坐标）
            Dim startGrid As New GridCell(Of Rectangle)(std.Floor(startPt.X / _gridSize), std.Floor(startPt.Y / _gridSize))
            Dim endGrid As New GridCell(Of Rectangle)(std.Floor(endPt.X / _gridSize), std.Floor(endPt.Y / _gridSize))

            ' 2. 初始化Open和Closed列表
            Dim openList As New List(Of PathNode(Of Rectangle))()
            Dim closedList As New HashSet(Of GridCell(Of Rectangle))()

            Dim startNode As New PathNode(Of Rectangle)(startGrid)
            openList.Add(startNode)

            ' 3. A* 主循环
            While openList.Count > 0
                ' 获取F值最小的节点 (简单的排序模拟优先队列，数据量大时建议用优先队列)
                Dim currentNode As PathNode(Of Rectangle) = openList.OrderBy(Function(n) n.F).First()

                ' 移出OpenList，加入ClosedList
                openList.Remove(currentNode)
                closedList.Add(currentNode.Position)

                ' 到达终点？
                If currentNode.Position.Equals(endGrid) Then
                    Return ReconstructPath(currentNode)
                End If

                ' 4. 探索邻居 (上下左右)
                Dim neighbors As GridCell(Of Rectangle)() = {
                    New GridCell(Of Rectangle)(currentNode.Position.X, currentNode.Position.Y - 1), ' 上
                    New GridCell(Of Rectangle)(currentNode.Position.X, currentNode.Position.Y + 1), ' 下
                    New GridCell(Of Rectangle)(currentNode.Position.X - 1, currentNode.Position.Y), ' 左
                    New GridCell(Of Rectangle)(currentNode.Position.X + 1, currentNode.Position.Y)  ' 右
                }

                For Each neighborPos In neighbors
                    ' 检查碰撞（边界检查 + 障碍物检查）
                    If _obstacles.Contains(neighborPos) OrElse closedList.Contains(neighborPos) Then
                        Continue For
                    End If

                    ' 计算G值：基础距离1 + 转弯惩罚
                    Dim movementCost As Integer = 1
                    ' 如果父节点存在，且方向发生变化，增加惩罚（例如+2），减少折线
                    If currentNode.Parent IsNot Nothing Then
                        Dim dx1 As Integer = currentNode.Position.X - currentNode.Parent.Position.X
                        Dim dy1 As Integer = currentNode.Position.Y - currentNode.Parent.Position.Y
                        Dim dx2 As Integer = neighborPos.X - currentNode.Position.X
                        Dim dy2 As Integer = neighborPos.Y - currentNode.Position.Y
                        If dx1 <> dx2 AndAlso dy1 <> dy2 Then
                            movementCost += 5 ' 转弯惩罚权重，数值越大路径越倾向于直线
                        End If
                    End If

                    Dim tentativeG As Integer = currentNode.G + movementCost

                    ' 检查邻居是否已在OpenList中
                    Dim existingNode As PathNode(Of Rectangle) = openList.FirstOrDefault(Function(n) n.Position.Equals(neighborPos))

                    If existingNode Is Nothing Then
                        ' 是新节点
                        Dim neighborNode As New PathNode(Of Rectangle)(neighborPos)
                        neighborNode.Parent = currentNode
                        neighborNode.G = tentativeG
                        neighborNode.H = std.Abs(neighborPos.X - endGrid.X) + std.Abs(neighborPos.Y - endGrid.Y) ' 曼哈顿距离
                        neighborNode.F = neighborNode.G + neighborNode.H
                        openList.Add(neighborNode)
                    ElseIf tentativeG < existingNode.G Then
                        ' 找到更优路径
                        existingNode.G = tentativeG
                        existingNode.Parent = currentNode
                        existingNode.F = existingNode.G + existingNode.H
                        ' 注意：这里在List中更新了对象，不需要重新排序，下次循环开头会OrderBy
                    End If
                Next
            End While

            ' 无法找到路径（此时可以返回直线或空）
            Return New List(Of Point)() From {startPt, endPt}
        End Function

        ' 回溯路径
        Private Function ReconstructPath(node As PathNode(Of Rectangle)) As List(Of Point)
            Dim path As New List(Of Point)()
            Dim curr As PathNode(Of Rectangle) = node
            While curr IsNot Nothing
                ' 转换回像素坐标（取网格中心点）
                path.Add(New Point(curr.Position.X * _gridSize + _gridSize / 2, curr.Position.Y * _gridSize + _gridSize / 2))
                curr = curr.Parent
            End While
            path.Reverse()
            Return path
        End Function
    End Class

    ' 寻路节点类
    Public Class PathNode(Of T)
        Public Property Position As GridCell(Of T)
        Public Property Parent As PathNode(Of T)
        Public Property G As Integer ' 起点到当前的成本
        Public Property H As Integer ' 当前到终点的估算成本
        Public Property F As Integer ' 总成本

        Public Sub New(pos As GridCell(Of T))
            Me.Position = pos
            Me.Parent = Nothing
            Me.G = 0
            Me.H = 0
            Me.F = 0
        End Sub
    End Class
End Namespace
