Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports stdNum = System.Math

Namespace Radial

    Public Module RadialLayout

        ''' <summary>
        ''' 径向树布局：以最高度节点为根，将邻居节点按角度均匀分布在同心圆环上，
        ''' 递归地将每个节点的邻居分布以该节点为中心的下一层圆环上。
        ''' </summary>
        ''' <param name="g">输入网络图。节点 initialPostion 为空时自动用随机位置初始化。</param>
        ''' <param name="radius">
        ''' 每层圆环之间的间距（像素）。默认 120，若传入 NaN 则按 sqrt(area/nodeCount) 自动推算。
        ''' </param>
        ''' <returns>布局完成后的网络图。</returns>
        Public Function LayoutNodes(g As NetworkGraph,
                                   Optional radius As Double = Double.NaN) As NetworkGraph

            Dim nodeCount As Integer = g.vertex.Count

            If Double.IsNaN(radius) OrElse radius <= 0 Then
                ' 根据节点数自动推算合理环间距
                radius = stdNum.Sqrt(1000.0 * 1000.0 / stdNum.Max(nodeCount, 1)) * 1.5
            End If

            ' 计算度排序，最高度节点作为根
            Dim degrees As Dictionary(Of String, Integer) = g.ComputeNodeDegrees()
            Dim degreeOrders = degrees.OrderByDescending(Function(a) a.Value).ToArray
            Dim rootId As String = degreeOrders.First.Key

            ' 确保根节点的中心位置不为 Nothing
            Dim root As Node = g.GetElementByID(rootId)
            If root.data.initialPostion Is Nothing Then
                root.data.initialPostion = New FDGVector2(0, 0)
            End If

            ' 所有尚未布局的节点如果在后续递归中始终没有被放置，则保持在原位（不额外处理）
            Call g.layoutCurrentCenter(centerId:=rootId,
                                       degrees:=degrees,
                                       radius:=radius,
                                       level:=0)

            Return g
        End Function

        ''' <summary>
        ''' 以 <paramref name="centerId"/> 为圆心，将其直接邻居按度排序后分布在
        ''' 距离 <paramref name="radius"/> 的圆环上，然后递归处理每个邻居。
        ''' </summary>
        <Extension>
        Private Sub layoutCurrentCenter(g As NetworkGraph,
                                        centerId As String,
                                        degrees As Dictionary(Of String, Integer),
                                        radius As Double,
                                        level As Integer)

            Dim center As Node = g.GetElementByID(centerId)

            ' 获取当前中心节点的所有直接邻居（已按度升序排列）
            Dim connected As Node() = g.GetEdges(center) _
                .Select(Function(e) e.Other(center)) _
                .Where(Function(n) degrees.ContainsKey(n.label) AndAlso n.label <> centerId) _
                .OrderBy(Function(n) degrees(n.label)) _
                .ToArray()

            ' 无邻居或所有邻居都已在更外层布局过 → 终止递归
            If connected.Length = 0 Then
                Return
            End If

            ' 确保中心节点位置已初始化
            If center.data.initialPostion Is Nothing Then
                center.data.initialPostion = New FDGVector2(0, 0)
            End If

            Dim cx As Double = center.data.initialPostion.x
            Dim cy As Double = center.data.initialPostion.y

            Dim deltaAngle As Double = 2.0 * stdNum.PI / connected.Length
            Dim angle As Double = 0.0

            ' 将已布局的邻居从度字典中移除，避免递归时重复处理
            For Each node As Node In connected
                degrees.Remove(node.label)
            Next

            For Each node As Node In connected
                ' 关键修正：原代码为 center.{x,y} * Cos/Sin（乘法），
                ' 正确公式应为 center.{x,y} + radius * Cos/Sin（加法），
                ' 使子节点分布在以 center 为圆心、radius 为半径的圆环上。
                node.data.initialPostion = New FDGVector2(
                    cx + radius * stdNum.Cos(angle),
                    cy + radius * stdNum.Sin(angle)
                )

                ' 递归：以当前节点为新的圆心，进入下一层（半径缩小以避免无限增长）
                Call g.layoutCurrentCenter(
                    centerId:=node.label,
                    degrees:=degrees,
                    radius:=radius * 0.75,
                    level:=level + 1)

                angle += deltaAngle
            Next
        End Sub

    End Module
End Namespace
