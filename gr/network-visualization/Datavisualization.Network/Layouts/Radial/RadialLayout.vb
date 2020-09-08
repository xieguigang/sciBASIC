Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports stdNum = System.Math

Namespace Layouts.Radial

    Public Module RadialLayout

        Public Function LayoutNodes(g As NetworkGraph) As NetworkGraph
            ' 首先计算出所有节点的连接度
            ' 将最高连接度的节点作为布局的中心点
            Dim degreeOrders = g.ComputeNodeDegrees.OrderByDescending(Function(a) a.Value).ToArray
            Dim layout = g.layoutCurrentCenter(cid:=degreeOrders.First.Key, degreeOrders.ToDictionary)

            Return layout
        End Function

        <Extension>
        Private Function layoutCurrentCenter(g As NetworkGraph, cid As String, degrees As Dictionary(Of String, Integer)) As NetworkGraph
            ' 其余的节点与中心节点的距离与度有关，度越高距离越远
            Dim center As Node = g.GetElementByID(cid)
            Dim connected As Node() = g.GetEdges(center) _
                .Select(Function(a) a.Other(center)) _
                .Where(Function(a) degrees.ContainsKey(a.label)) _
                .OrderBy(Function(n) degrees(n.label)) _
                .ToArray

            degrees.Remove(cid)

            ' 当前节点为孤立节点或者已经被布局过了
            If connected.Length = 0 OrElse degrees.Count = 0 Then
                Return g
            End If

            Dim deltaAngle As Double = 2 * stdNum.PI / connected.Length
            Dim angle As Double

            For Each node As Node In connected
                node.data.initialPostion = New FDGVector2 With {
                    .x = center.data.initialPostion.x * stdNum.Cos(angle),
                    .y = center.data.initialPostion.y * stdNum.Sin(angle)
                }
                g.layoutCurrentCenter(node.label, degrees)
                angle += deltaAngle
            Next

            Return g
        End Function
    End Module
End Namespace