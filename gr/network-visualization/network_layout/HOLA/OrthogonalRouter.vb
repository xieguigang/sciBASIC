Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.EdgeBundling
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Language

Namespace Hola

    ''' <summary>
    ''' HOLA 阶段 6：最终正交路由（Final Orthogonal Route）。
    ''' 把相邻节点之间的连线生成为轴对齐的正交折线（Z 形），并把折点写入
    ''' <see cref="EdgeData.bends"/>（用相对比例偏移的 WayPointVector 描述，
    ''' 使节点位置变化时路径形状可跟随保持）。
    ''' </summary>
    Public Module OrthogonalRouter

        ''' <summary>
        ''' 为图中每条边生成正交路由折点并写回 <see cref="EdgeData.bends"/>。
        ''' </summary>
        Public Sub Route(graph As NetworkGraph, opts As HolaOptions)
            For Each e As Edge In graph.graphEdges
                Dim U = e.U, V = e.V
                If U Is Nothing OrElse V Is Nothing Then Continue For

                Dim pu = AsPoint(U.data.initialPostion)
                Dim pv = AsPoint(V.data.initialPostion)

                ' Z 形正交路径：U -> (midX, U.y) -> (midX, V.y) -> V
                ' 两个拐点都必须相对整条边 (U -> V) 用 CreateVector 生成，
                ' 以保证比例语义一致，渲染层用 GetPoint(U, V) 还原
                Dim midX = (pu.X + pv.X) / 2.0F

                Dim bend1 = WayPointVector.CreateVector(pu, pv, midX, pu.Y)
                Dim bend2 = WayPointVector.CreateVector(pu, pv, midX, pv.Y)

                e.data.bends = {bend1, bend2}
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function AsPoint(v As AbstractVector) As System.Drawing.PointF
            Return New System.Drawing.PointF(CSng(v.x), CSng(v.y))
        End Function
    End Module
End Namespace
