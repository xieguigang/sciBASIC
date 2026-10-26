Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Hola
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Imaging.Driver
Imports inode = Microsoft.VisualBasic.Data.visualize.Network.Graph.Node
' the HOLA layout module lives in a namespace of the same name (...Layouts.Hola.HOLA),
' so it is aliased to avoid the namespace/type ambiguity
Imports HolaLayout = Microsoft.VisualBasic.Data.visualize.Network.Layouts.Hola.HOLA

''' <summary>
''' 网络图构建 / 布局 / 出图模块。
''' <para>
''' 典型流程：<c>net.create()</c> → 若干 <c>net.addNode</c> / <c>net.addEdge</c> →
''' <c>net.hola</c>（正交布局） → <c>net.draw</c> 输出 png。
''' </para>
''' </summary>
Public Module net

    ''' <summary>创建一个空的网络图。</summary>
    Public Function create() As NetworkGraph
        Return New NetworkGraph()
    End Function

    ''' <summary>
    ''' 添加节点并返回它；给出坐标时作为布局的初始位置（HOLA 布局会以此为起点）。
    ''' </summary>
    Public Function addNode(graph As NetworkGraph, label As String,
                            Optional x As Double = Double.NaN,
                            Optional y As Double = Double.NaN,
                            Optional size As Double = 18) As inode

        Dim data As New NodeData With {
            .label = label,
            .size = {size, size}
        }

        If Not Double.IsNaN(x) AndAlso Not Double.IsNaN(y) Then
            data.initialPostion = New FDGVector2(x, y)
        End If

        Dim node As New inode With {
            .label = label,
            .data = data
        }

        Return graph.AddNode(node)
    End Function

    ''' <summary>添加一条无向边（两个节点不存在时会自动创建）。</summary>
    Public Function addEdge(graph As NetworkGraph, u As String, v As String, Optional weight As Double = 0) As NetworkGraph
        Call graph.AddEdge(u, v, weight)

        Return graph
    End Function

    ''' <summary>执行 HOLA 正交布局，节点的最终坐标写回 <c>data.initialPostion</c>。</summary>
    Public Function hola(graph As NetworkGraph) As NetworkGraph
        Return HolaLayout.DoLayout(graph)
    End Function

    ''' <summary>节点数量。</summary>
    Public Function nodeCount(graph As NetworkGraph) As Integer
        Dim n As Integer = 0

        For Each node As inode In graph.connectedNodes
            n += 1
        Next

        Return n
    End Function

    ''' <summary>边的数量。</summary>
    Public Function edgeCount(graph As NetworkGraph) As Integer
        Dim n As Integer = 0

        For Each edge As Edge In graph.graphEdges
            n += 1
        Next

        Return n
    End Function

    ''' <summary>带有正交折点（bend）的边数量。</summary>
    Public Function bendCount(graph As NetworkGraph) As Integer
        Dim n As Integer = 0

        For Each edge As Edge In graph.graphEdges
            If edge.data IsNot Nothing AndAlso edge.data.bends IsNot Nothing AndAlso edge.data.bends.Length > 0 Then
                n += 1
            End If
        Next

        Return n
    End Function

    ''' <summary>
    ''' 把网络图渲染为图片并返回绝对路径（格式由扩展名决定）。
    ''' </summary>
    Public Function draw(graph As NetworkGraph, file As String,
                         Optional size As String = "1400,1400",
                         Optional drawBends As Boolean = True,
                         Optional minLinkWidth As Integer = 8,
                         Optional displayId As Boolean = False,
                         Optional labelerIterations As Integer = -1,
                         Optional ppi As Integer = 100) As String

        Dim graphData As GraphicsData = NetworkVisualizer.DrawImage(
            graph,
            canvasSize:=If(size, "1400,1400"),
            displayId:=displayId,
            drawEdgeBends:=drawBends,
            labelerIterations:=labelerIterations,
            minLinkWidth:=minLinkWidth,
            ppi:=ppi)

        Return plot.save(graphData, file)
    End Function
End Module
