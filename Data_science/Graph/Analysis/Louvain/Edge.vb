
Namespace Analysis.Louvain

    Public Class Edge
        Friend v As Integer 'v表示连接点的编号,w表示此边的权值
        Friend weight As Double
        Friend [next] As Integer 'next负责连接和此点相关的边

        Friend Sub New()
        End Sub
    End Class
End Namespace