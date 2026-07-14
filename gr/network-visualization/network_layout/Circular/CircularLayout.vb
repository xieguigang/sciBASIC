Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports stdNum = System.Math

Namespace Circular

    ''' <summary>
    ''' 环形（圆形）布局：将所有节点沿一个圆周均匀分布。
    ''' 支持按度排序或按自定义键排序以优化视觉表现 / 减少边交叉。
    ''' </summary>
    Public Module CircularLayout

        ''' <summary>
        ''' 将所有节点均匀排布在一个圆周上。
        ''' </summary>
        ''' <param name="g">网络图。</param>
        ''' <param name="radius">
        ''' 圆半径（像素）。若未指定或传入 NaN，则按 sqrt(area/nodeCount) 自动推算。
        ''' </param>
        ''' <param name="cx">圆心 X 坐标，默认 500。</param>
        ''' <param name="cy">圆心 Y 坐标，默认 500。</param>
        ''' <param name="sortByDegree">
        ''' True 时按节点度升序排列（高度节点靠近 → 邻边更短，视觉更紧凑）；
        ''' False 时保持原有 vertex 遍历顺序。
        ''' </param>
        ''' <returns>布局完成后的网络图。</returns>
        Public Function LayoutNodes(g As NetworkGraph,
                                   Optional radius As Double = Double.NaN,
                                   Optional cx As Double = 500.0,
                                   Optional cy As Double = 500.0,
                                   Optional sortByDegree As Boolean = True) As NetworkGraph

            Dim nodes As Node() = g.vertex.ToArray
            Dim n As Integer = nodes.Length

            If n = 0 Then Return g

            ' 自动推算半径
            If Double.IsNaN(radius) OrElse radius <= 0 Then
                radius = n * 12.0
                ' 保证半径不小于 100
                If radius < 100 Then radius = 100.0
            End If

            ' 按度排序以优化视觉：度相近的节点放在相邻位置，有助于减少边交叉
            Dim ordered As IEnumerable(Of Node)
            If sortByDegree Then
                Dim degrees = g.ComputeNodeDegrees()
                ordered = nodes.OrderBy(Function(v)
                                            Dim d As Integer = 0
                                            degrees.TryGetValue(v.label, d)
                                            Return d
                                        End Function)
            Else
                ordered = nodes
            End If
            Dim orderedArr As Node() = ordered.ToArray()

            Dim deltaAngle As Double = 2.0 * stdNum.PI / n
            Dim angle As Double = 0.0

            For i As Integer = 0 To n - 1
                Dim node As Node = orderedArr(i)

                node.data.initialPostion = New FDGVector2(
                    cx + radius * stdNum.Cos(angle),
                    cy + radius * stdNum.Sin(angle)
                )

                angle += deltaAngle
            Next

            Return g
        End Function

        ''' <summary>
        ''' 环形布局 + 边数量最优化：使用启发式（贪心 2-opt 局部搜索）对圆形上节点顺序做
        ''' 邻接权重交换，尽可能减少直连边在圆内穿越的长度与交叉。
        ''' 适用于边数量较多（>100）且对交叉敏感的场合，速度较 LayoutNodes 慢。
        ''' </summary>
        ''' <param name="g">网络图。</param>
        ''' <param name="radius">圆半径（NaN → 自动推算）。</param>
        ''' <param name="cx">圆心 X。</param>
        ''' <param name="cy">圆心 Y。</param>
        ''' <param name="maxSwaps">2-opt 局部搜索最大交换次数（默认 1000）。设 0 则跳过优化仅做按度排序。</param>
        ''' <returns>布局完成后的网络图。</returns>
        Public Function LayoutNodesWithCrossingOptimization(g As NetworkGraph,
                                                            Optional radius As Double = Double.NaN,
                                                            Optional cx As Double = 500.0,
                                                            Optional cy As Double = 500.0,
                                                            Optional maxSwaps As Integer = 1000) As NetworkGraph

            Dim nodes As Node() = g.vertex.ToArray
            Dim n As Integer = nodes.Length
            If n = 0 Then Return g

            If Double.IsNaN(radius) OrElse radius <= 0 Then
                radius = n * 12.0
                If radius < 100 Then radius = 100.0
            End If

            ' 初始顺序：按度排序
            Dim degDict = g.ComputeNodeDegrees()
            Dim order As Node() = nodes.OrderBy(Function(v)
                                                    Dim d As Integer = 0
                                                    degDict.TryGetValue(v.label, d)
                                                    Return d
                                                End Function).ToArray()

            ' 构建邻接集合用于快速判断两点间是否有边
            Dim adjacency As New HashSet(Of (String, String))
            For Each e As Edge In g.graphEdges
                Dim a = e.U.label, b = e.V.label
                If String.CompareOrdinal(a, b) < 0 Then
                    adjacency.Add((a, b))
                Else
                    adjacency.Add((b, a))
                End If
            Next

            ' 辅助函数：判断 i 和 j 之间是否有边
            Dim hasEdge As Func(Of Integer, Integer, Boolean) =
                Function(i As Integer, j As Integer) As Boolean
                    If i = j Then Return False
                    Dim a = order(i).label, b = order(j).label
                    If String.CompareOrdinal(a, b) < 0 Then
                        Return adjacency.Contains((a, b))
                    Else
                        Return adjacency.Contains((b, a))
                    End If
                End Function

            ' 2-opt 局部搜索：贪心地在圆上交换相邻节点以增大"相邻且相连"的对数
            '（即让有边相连的节点在圆上尽量相邻）。
            If maxSwaps > 0 AndAlso n > 2 Then
                Dim improved As Boolean = True
                Dim swaps As Integer = 0

                While improved AndAlso swaps < maxSwaps
                    improved = False

                    For i As Integer = 0 To n - 1
                        Dim ni As Integer = (i + 1) Mod n

                        Dim oldScore As Integer = 0
                        If hasEdge(i, ni) Then oldScore += 1

                        For j As Integer = i + 2 To n - 1
                            If j = i OrElse (j + 1) Mod n = i Then Continue For

                            Dim nj As Integer = (j + 1) Mod n

                            Dim newScore As Integer = 0
                            If hasEdge(i, j) Then newScore += 1
                            If hasEdge(ni, nj) Then newScore += 1

                            oldScore = 0
                            If hasEdge(i, ni) Then oldScore += 1
                            If hasEdge(j, nj) Then oldScore += 1

                            If newScore > oldScore Then
                                ' 交换 i+1 .. j 段
                                Dim left As Integer = ni
                                Dim right As Integer = j
                                While left < right
                                    Dim tmp As Node = order(left)
                                    order(left) = order(right)
                                    order(right) = tmp
                                    left += 1
                                    right -= 1
                                End While

                                improved = True
                                swaps += 1
                            End If
                        Next
                    End While
                End While
            End If

            ' 将最终顺序映射到圆上
            Dim deltaAngle As Double = 2.0 * stdNum.PI / n
            Dim angle As Double = 0.0

            For i As Integer = 0 To n - 1
                order(i).data.initialPostion = New FDGVector2(
                    cx + radius * stdNum.Cos(angle),
                    cy + radius * stdNum.Sin(angle)
                )
                angle += deltaAngle
            Next

            Return g
        End Function

    End Module
End Namespace
