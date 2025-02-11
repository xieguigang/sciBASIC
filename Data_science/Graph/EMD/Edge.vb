Namespace EMD

    ''' <summary>
    ''' @author Telmo Menezes (telmo@telmomenezes.com)
    ''' 
    ''' </summary>
    Friend Class Edge
        Friend Sub New([to] As Integer, cost As Long)
            _to = [to]
            _cost = cost
        End Sub

        Friend _to As Integer
        Friend _cost As Long

        Public Overrides Function ToString() As String
            Return $"->{_to} (cost:{_cost})"
        End Function
    End Class

    Friend Class Edge0
        Friend Sub New([to] As Integer, cost As Long, flow As Long)
            _to = [to]
            _cost = cost
            _flow = flow
        End Sub

        Friend _to As Integer
        Friend _cost As Long
        Friend _flow As Long

        Public Overrides Function ToString() As String
            Return $"->{_to} (cost:{_cost}; flow:{_flow})"
        End Function
    End Class

    Friend Class Edge1
        Friend Sub New([to] As Integer, reduced_cost As Long)
            _to = [to]
            _reduced_cost = reduced_cost
        End Sub

        Friend _to As Integer
        Friend _reduced_cost As Long

        Public Overrides Function ToString() As String
            Return $"->{_to} (reduced_cost:{_reduced_cost})"
        End Function
    End Class

    Friend Class Edge2
        Friend Sub New([to] As Integer, reduced_cost As Long, residual_capacity As Long)
            _to = [to]
            _reduced_cost = reduced_cost
            _residual_capacity = residual_capacity
        End Sub

        Friend _to As Integer
        Friend _reduced_cost As Long
        Friend _residual_capacity As Long

        Public Overrides Function ToString() As String
            Return $"->{_to} (reduced_cost:{_reduced_cost}; residual_capacity:{_residual_capacity})"
        End Function
    End Class

    Friend Class Edge3
        Friend Sub New()
            _to = 0
            _dist = 0
        End Sub

        Friend Sub New([to] As Integer, dist As Long)
            _to = [to]
            _dist = dist
        End Sub

        Friend _to As Integer
        Friend _dist As Long

        Public Overrides Function ToString() As String
            Return $"->{_to} (dist:{_dist})"
        End Function
    End Class

End Namespace