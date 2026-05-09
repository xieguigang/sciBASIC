Namespace DinicMaxFlow

    ' ========================================================================
    ' 流量边结构体
    ' 存储每条边的终点、容量、当前流量及反向边索引
    ' ========================================================================
    Public Structure FlowEdge
        Public [To] As Integer        ' 边的终点节点编号
        Public Capacity As Integer    ' 边的最大容量
        Public Flow As Integer        ' 边的当前流量
        Public Rev As Integer         ' 反向边在邻接表中的索引

        Public Sub New(toNode As Integer, cap As Integer, revIdx As Integer)
            [To] = toNode
            Capacity = cap
            Flow = 0
            Rev = revIdx
        End Sub
    End Structure

End Namespace