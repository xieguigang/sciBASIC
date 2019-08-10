Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Layouts.EdgeBundling

    ''' <summary>
    ''' 质心法进行边链接线条的插值
    ''' </summary>
    Public Module Barycentre

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g">请注意，在这个网络图中的节点应该是在完成了布局之后的，具有了各自的布局位置之后的节点</param>
        ''' <returns></returns>
        <Extension>
        Public Function DoBarycentreEdgeLayout(g As NetworkGraph) As NetworkGraph
            ' 遍历每一个节点
            ' 得到与该节点连接的所有的边
            For Each node As Node In g.vertex
                Dim links As Edge() = node.adjacencies _
                    .EnumerateAllEdges _
                    .Distinct _
                    .ToArray
                Dim centras As New List(Of PointF)

                If links.IsNullOrEmpty Then
                    Continue For
                End If

                ' 然后计算出每一条边的质心
                For Each link As Edge In links
                    centras += link.Barycentre
                    link.data!Barycentre = centras.Last.GetJson
                Next

                ' 然后计算出所有的这些质心的中心点
                ' 这个中心点就是插值的控制点
                Dim centra As PointF = centras.Centre

                For Each link As Edge In links
                    If link.data.HasProperty("control") Then
                        With link.data!control.LoadJSON(Of PointF)
                            link.data!control = New PointF((.X + centra.X) / 2, (.Y + centra.Y) / 2).GetJson
                        End With
                    Else
                        link.data!control = centra.GetJson
                    End If
                Next
            Next

            Return g
        End Function

        <Extension>
        Public Function Barycentre(link As Edge) As PointF
            Dim w1 = link.data.weight + 1.0E-20
            Dim w2 = link.data.weight + 1.0E-20

            w1 = w1 / (w1 + w2)
            w2 = w2 / (w1 + w2)

            Dim pu = link.U.data.initialPostion
            Dim pv = link.V.data.initialPostion
            Dim x! = (pu.x * w1 + pv.x * w2)
            Dim y! = (pu.x * w1 + pv.x * w2)

            Return New PointF With {.X = x, .Y = y}
        End Function
    End Module
End Namespace