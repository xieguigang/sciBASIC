Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Language

Namespace Hola

    ''' <summary>
    ''' HOLA 主控类。按照 hola2015.md 第 2 节与 8.2 节描述的阶段顺序，
    ''' 协调各松弛/路由阶段，把最终坐标写回 <see cref="NodeData.initialPostion"/>。
    ''' </summary>
    Public Class HolaLayouter

        ''' <summary>
        ''' 对给定的网络图执行 HOLA 正交布局。
        ''' 阶段顺序：初始布局 → 分层扫描去交叉 → 对齐松弛 → 再清一次交叉
        '''          → 扩散去重叠 → 写回坐标 → 最终正交路由（写入边 bends）。
        ''' </summary>
        ''' <param name="graph">要布局的图；节点坐标通过 NodeData.initialPostion 读写</param>
        ''' <param name="opts">可选参数，缺省时使用默认参数</param>
        ''' <returns>同一个 graph 实例（已写回坐标）</returns>
        Public Function Layout(graph As NetworkGraph, Optional opts As HolaOptions = Nothing) As NetworkGraph
            If opts Is Nothing Then opts = New HolaOptions()

            ' 阶段 1：初始布局
            Dim state = InitialLayout.Build(graph, opts)

            If state.nodes.Length = 0 Then
                Return graph
            End If

            Call Console.WriteLine($"[HOLA] start: {state.nodes.Length} nodes, {state.edges.Length} edges, initial stress={state.TotalStress(opts):F2}")

            ' 阶段 2：分层扫描松弛（去边交叉）
            Call LayerScanRelaxation.Relax(state, opts)

            ' 阶段 3：对齐松弛（轴对齐）
            Call AlignRelaxation.Relax(state, opts)

            ' 阶段 3b：对齐可能引入交叉，再做一次扫描清理
            Call LayerScanRelaxation.Relax(state, opts)

            ' 阶段 4：扩散松弛（去节点/边重叠）
            Call SpreadRelaxation.Relax(state, opts)

            Call Console.WriteLine($"[HOLA] after relax: stress={state.TotalStress(opts):F2}")

            ' 阶段 5：把内部坐标写回到节点
            Call state.WriteBack()

            ' 阶段 6：最终正交路由（把折点写入 EdgeData.bends）
            Call OrthogonalRouter.Route(graph, opts)

            Return graph
        End Function
    End Class
End Namespace
