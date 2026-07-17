Imports System
Imports Microsoft.VisualBasic.MachineLearning.CellularAutomaton

    ''' <summary>
    ''' 康威生命游戏（Conway's Game of Life）的细胞实现。
    ''' 规则 B3/S23：存活细胞在 2 或 3 个活邻居时存活，死亡细胞在恰好 3 个活邻居时出生。
    ''' 邻居数量由所配置的 <see cref="NeighborhoodType"/> 决定（冯·诺依曼 4 / 摩尔 8 / 扩展摩尔 24）。
    ''' </summary>
    Public Class ConwayCell
        Implements Individual

        ''' <summary>当前世代的存活状态。</summary>
        Public Property State As Boolean

        ''' <summary>下一代（缓冲）状态，由 <see cref="Tick"/> 计算、<see cref="Commit"/> 提交。</summary>
        Private nextState As Boolean

        Public Sub New(Optional alive As Boolean = False)
            State = alive
            nextState = alive
        End Sub

        ''' <summary>
        ''' 依据邻居的当前状态，按 B3/S23 规则计算下一代状态（仅写入缓冲，不修改当前状态）。
        ''' </summary>
        Public Sub Tick(adjacents As IEnumerable(Of Individual)) Implements Individual.Tick
            Dim aliveNeighbors As Integer = 0

            For Each adj As Individual In adjacents
                If CType(adj, ConwayCell).State Then
                    aliveNeighbors += 1
                End If
            Next

            If State Then
                ' 存活规则 S23：2 或 3 个活邻居存活，否则死亡。
                nextState = (aliveNeighbors = 2 OrElse aliveNeighbors = 3)
            Else
                ' 出生规则 B3：恰好 3 个活邻居时出生。
                nextState = (aliveNeighbors = 3)
            End If
        End Sub

        ''' <summary>
        ''' 将缓冲的下一代状态提交为当前状态。
        ''' </summary>
        Public Sub Commit() Implements Individual.Commit
            State = nextState
        End Sub
    End Class
