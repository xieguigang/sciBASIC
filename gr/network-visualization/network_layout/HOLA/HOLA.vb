Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts

Namespace Hola

    ''' <summary>
    ''' HOLA 网络布局算法的静态入口。供 test 项目与其它调用方直接使用：
    ''' <code>Call HOLA.DoLayout(g)</code>
    ''' </summary>
    Public Module [HOLA]

        ''' <summary>
        ''' 对网络图执行 HOLA 正交布局，并把结果写回到节点的 <see cref="NodeData.initialPostion"/>。
        ''' </summary>
        ''' <param name="graph">要布局的网络图</param>
        ''' <param name="opts">可选算法参数</param>
        ''' <returns>已布局的同一图实例</returns>
        <Extension>
        Public Function DoLayout(graph As NetworkGraph, Optional opts As HolaOptions = Nothing) As NetworkGraph
            Return New HolaLayouter().Layout(graph, opts)
        End Function
    End Module
End Namespace
