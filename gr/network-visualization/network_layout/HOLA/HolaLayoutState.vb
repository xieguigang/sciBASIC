Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts

Namespace Hola

    ''' <summary>
    ''' HOLA 布局过程中的共享中间状态。所有阶段都围绕这份状态读写节点坐标，
    ''' 最终由主控阶段把状态写回到 <see cref="NodeData.initialPostion"/>。
    ''' </summary>
    Public Class HolaLayoutState

        ''' <summary>
        ''' 参与布局的节点列表（按连通分量拆分前为全图节点）。
        ''' </summary>
        Public ReadOnly Property nodes As Node()

        ''' <summary>
        ''' 无向边列表，每条边用节点索引对 (u, v) 表示。
        ''' </summary>
        Public ReadOnly Property edges As (u As Integer, v As Integer)()

        ''' <summary>
        ''' 节点索引 -> 在 <see cref="nodes"/> 数组中的位置。
        ''' </summary>
        Private ReadOnly indexOf As Dictionary(Of Node, Integer)

        ''' <summary>
        ''' 当前布局坐标（与 nodes 一一对应）。算法内部只操作这份浮点坐标，
        ''' 避免频繁访问 NodeData 的装箱/拆箱。
        ''' </summary>
        Public ReadOnly positions As FDGVector2()

        ''' <summary>
        ''' 每个节点的邻居索引集合（无向），用于扫描松弛阶段的局部处理。
        ''' </summary>
        Public ReadOnly neighbours As Integer()()

        Sub New(nodes As Node(), edges As (u As Integer, v As Integer)(), opts As HolaOptions)
            Me.nodes = nodes
            Me.edges = edges
            Me.indexOf = New Dictionary(Of Node, Integer)
            Me.positions = New FDGVector2(nodes.Length - 1) {}
            Me.neighbours = New Integer(nodes.Length - 1)() {}

            For i As Integer = 0 To nodes.Length - 1
                indexOf(nodes(i)) = i
                Dim pos = nodes(i).data.initialPostion
                If pos Is Nothing Then
                    ' 没有初始坐标时给一个确定性的网格起点，保证可复现
                    positions(i) = New FDGVector2((i Mod 32) * opts.nodeGap * 2, (i \ 32) * opts.nodeGap * 2)
                Else
                    positions(i) = New FDGVector2(pos.x, pos.y)
                End If
                neighbours(i) = {}
            Next

            ' 构建无向邻居表
            Dim adj As New List(Of Integer())(nodes.Length)
            For i As Integer = 0 To nodes.Length - 1
                adj.Add(New List(Of Integer))
            Next
            For Each e In edges
                adj(e.u).Add(e.v)
                adj(e.v).Add(e.u)
            Next
            For i As Integer = 0 To nodes.Length - 1
                neighbours(i) = adj(i).ToArray
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IndexOf(n As Node) As Integer
            Return indexOf(n)
        End Function

        ''' <summary>
        ''' 把当前内部坐标写回到每个节点的 <see cref="NodeData.initialPostion"/>。
        ''' </summary>
        Public Sub WriteBack()
            For i As Integer = 0 To nodes.Length - 1
                nodes(i).data.initialPostion = New FDGVector2(positions(i).x, positions(i).y)
            Next
        End Sub

        ''' <summary>
        ''' 计算全部边的当前总应力（边长与期望边长之差的平方和），
        ''' 用于收敛判断与日志。
        ''' </summary>
        Public Function TotalStress(opts As HolaOptions) As Double
            Dim sum As Double = 0.0
            For Each e In edges
                Dim dx = positions(e.u).x - positions(e.v).x
                Dim dy = positions(e.u).y - positions(e.v).y
                Dim d = System.Math.Sqrt(dx * dx + dy * dy)
                Dim diff = d - opts.desiredEdgeLength
                sum += diff * diff
            Next
            Return sum
        End Function
    End Class
End Namespace
