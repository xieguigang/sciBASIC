Imports Microsoft.VisualBasic.Data
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
''' 对外的图对象是 <see cref="Graph"/> 门面，脚本不需要接触底层的
''' <c>NetworkGraph</c> / <c>Node</c> / <c>Edge</c> 类型；既可以写
''' <c>net.addNode(g, "a")</c>，也可以写 <c>g.addNode("a")</c>。
''' </para>
''' <para>
''' 典型流程：<c>net.create()</c> → 若干 addNode / addEdge → <c>net.hola</c> → <c>net.draw</c>。
''' </para>
''' </summary>
Public Module net

    ''' <summary>创建一个空的网络图。</summary>
    Public Function create() As Graph
        Return New Graph(New NetworkGraph())
    End Function

    ''' <summary>
    ''' 添加节点并返回本图；给出坐标时作为布局的初始位置（HOLA 布局以此为起点）。
    ''' </summary>
    Public Function addNode(graph As Graph, label As String,
                            Optional x As Double = Double.NaN,
                            Optional y As Double = Double.NaN,
                            Optional size As Double = 18) As Graph

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

        Call graph.inner.AddNode(node)

        Return graph
    End Function

    ''' <summary>添加一条边（节点不存在时会自动创建），返回本图。</summary>
    Public Function addEdge(graph As Graph, u As String, v As String, Optional weight As Double = 0) As Graph
        Call graph.inner.AddEdge(u, v, weight)

        Return graph
    End Function

    ''' <summary>执行 HOLA 正交布局，节点的最终坐标写回 <c>data.initialPostion</c>。</summary>
    Public Function hola(graph As Graph) As Graph
        Call HolaLayout.DoLayout(graph.inner)

        Return graph
    End Function

    ''' <summary>节点数量。</summary>
    Public Function nodeCount(graph As Graph) As Integer
        Dim n As Integer = 0

        For Each node As inode In graph.inner.connectedNodes
            n += 1
        Next

        Return n
    End Function

    ''' <summary>边的数量。</summary>
    Public Function edgeCount(graph As Graph) As Integer
        Dim n As Integer = 0

        For Each edge As Edge In graph.inner.graphEdges
            n += 1
        Next

        Return n
    End Function

    ''' <summary>带有正交折点（bend）的边数量。</summary>
    Public Function bendCount(graph As Graph) As Integer
        Dim n As Integer = 0

        For Each edge As Edge In graph.inner.graphEdges
            If edge.data IsNot Nothing AndAlso edge.data.bends IsNot Nothing AndAlso edge.data.bends.Length > 0 Then
                n += 1
            End If
        Next

        Return n
    End Function

    ''' <summary>把所有节点按布局坐标导出为表（行名 = 节点名，特征列 x / y）。</summary>
    Public Function coordinates(graph As Graph) As NumericTable
        Dim names As New List(Of String)
        Dim rows As New List(Of Double())

        For Each node As inode In graph.inner.connectedNodes
            Dim position = node.data.initialPostion

            Call names.Add(node.label)
            Call rows.Add({position.x, position.y})
        Next

        Return NumericTable.FromRows(names.ToArray, rows.ToArray, {"x", "y"})
    End Function

    ''' <summary>
    ''' 把网络图渲染为图片并返回绝对路径（格式由扩展名决定）。
    ''' </summary>
    Public Function draw(graph As Graph, file As String,
                         Optional size As String = "1400,1400",
                         Optional drawBends As Boolean = True,
                         Optional minLinkWidth As Integer = 8,
                         Optional displayId As Boolean = False,
                         Optional labelerIterations As Integer = -1,
                         Optional ppi As Integer = 100) As String

        Dim graphData As GraphicsData = NetworkVisualizer.DrawImage(
            graph.inner,
            canvasSize:=If(size, "1400,1400"),
            displayId:=displayId,
            drawEdgeBends:=drawBends,
            labelerIterations:=labelerIterations,
            minLinkWidth:=minLinkWidth,
            ppi:=ppi)

        Return plot.save(New PlotImage(graphData), file)
    End Function
End Module
