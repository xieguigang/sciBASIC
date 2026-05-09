Namespace DinicMaxFlow

    ' ========================================================================
    ' 节点流量分布信息
    ' 记录每个节点的流入、流出、净流量等信息
    ' ========================================================================
    Public Class NodeFlowInfo
        Public NodeIndex As Integer       ' 节点编号
        Public TotalInFlow As Integer     ' 总流入量
        Public TotalOutFlow As Integer    ' 总流出量
        Public NetFlow As Integer         ' 净流量 (流入 - 流出)
        Public IsSource As Boolean        ' 是否为源点
        Public IsSink As Boolean          ' 是否为汇点

        Public Overrides Function ToString() As String
            Dim role As String = ""
            If IsSource Then role = " [源点]"
            If IsSink Then role = " [汇点]"
            Return String.Format(
                "节点{0}: 流入={1}, 流出={2}, 净流量={3}{4}",
                NodeIndex, TotalInFlow, TotalOutFlow, NetFlow, role)
        End Function
    End Class
End Namespace