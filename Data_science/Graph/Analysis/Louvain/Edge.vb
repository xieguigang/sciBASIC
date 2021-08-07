﻿
Namespace Analysis.Louvain

    Friend Class Edge

        ''' <summary>
        ''' v表示连接点的编号,w表示此边的权值
        ''' </summary>
        Friend v As Integer
        Friend weight As Double
        ''' <summary>
        ''' next负责连接和此点相关的边
        ''' </summary>
        Friend [next] As Integer

        Friend Sub New()
        End Sub
    End Class
End Namespace