Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.EdgeBundling
Imports Microsoft.VisualBasic.Language

Namespace Layouts.Orthogonal

    Public Module Router

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g">请注意，在这个网络图中的节点应该是在完成了布局之后的，具有了各自的布局位置之后的节点</param>
        ''' <returns></returns>
        <Extension>
        Public Function DoEdgeLayout(g As NetworkGraph) As NetworkGraph
            Dim x, y As Double

            ' 初始化基本布局
            For Each edge As Edge In g.graphEdges
                Dim a = edge.U.data.initialPostion.Point2D
                Dim b = edge.V.data.initialPostion.Point2D
                Dim points As New List(Of Handle)

                points += New Handle(a)
                points += New Handle(New PointF(b.X, a.Y))
                points += New Handle(b)

                edge.data.bends = points.ToArray
            Next

            Return g
        End Function
    End Module
End Namespace