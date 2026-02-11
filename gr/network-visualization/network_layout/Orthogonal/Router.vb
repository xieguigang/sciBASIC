#Region "Microsoft.VisualBasic::a04876b191046353d815d9a7c557448d, gr\network-visualization\network_layout\Orthogonal\Router.vb"

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

    '   Total Lines: 52
    '    Code Lines: 35 (67.31%)
    ' Comment Lines: 8 (15.38%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (17.31%)
    '     File Size: 2.29 KB


    '     Module Router
    ' 
    '         Sub: AstarRouter
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.GraphTheory.GridGraph
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.EdgeBundling
Imports Microsoft.VisualBasic.Imaging

Namespace Orthogonal

    Public Module Router

        <Extension>
        Public Sub AstarRouter(g As NetworkGraph)
            ' 1. 准备障碍物列表
            ' 假设节点是正方形或矩形，你需要根据 initialPosition 构建矩形
            ' 注意：这里要把起点和终点所在的节点从障碍物中临时排除，或者将障碍物稍微缩小一点，防止堵死出口
            Dim obstacleRects As New List(Of Rectangle)()

            For Each v As Node In g.vertex
                ' 解析你的数据结构
                Dim pos As AbstractVector = v.data.initialPostion
                Dim w As Integer = 5 ' v.data.size(0)
                Dim h As Integer = 5 ' v.data.size.ElementAtOrDefault(1, v.data.size(0))

                ' 假设节点宽高为 40
                Call obstacleRects.Add(New Rectangle(pos.x - w / 2, pos.y - h / 2, w, h))
            Next

            ' 2. 初始化路由器
            Dim router As New OrthogonalRouter(obstacleRects, gridSize:=5)

            ' 3. 计算每一条边
            For Each edge As Edge In g.graphEdges
                Dim u As Node = edge.U ' 假设这里直接引用了vertex对象
                Dim v As Node = edge.V

                Dim startPt As New Point(u.data.initialPostion.x, u.data.initialPostion.y)
                Dim endPt As New Point(v.data.initialPostion.x, v.data.initialPostion.y)
                ' 核心计算：获取路径点集合
                Dim pathPoints As List(Of Point) = router.FindPath(startPt, endPt)
                Dim ps As PointF = startPt.PointF
                Dim pt As PointF = endPt.PointF

                edge.data.bends = pathPoints _
                    .Select(Function(p)
                                Return WayPointVector.CreateVector(ps, pt, p.PointF)
                            End Function) _
                    .ToArray
            Next
        End Sub
    End Module
End Namespace
