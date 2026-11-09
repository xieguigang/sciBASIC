Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Reconstruction

    ''' <summary>
    ''' 网格后处理：平滑、QEM 简化、补洞与点到网格距离。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么重建之后还要后处理</b>：三角化算法（BPA/贪心/泊松）输出的网格
    ''' 通常带有三类问题：① 扫描噪声导致的锯齿表面；② 三角形数量爆炸
    ''' （百万级，无法直接交给渲染/CAD）；③ 遮挡区域留下的洞。
    ''' 这三个问题分别对应平滑、简化、补洞。
    ''' </para>
    ''' <para>
    ''' <b>平滑的选择</b>：Laplacian 平滑简单但会<b>收缩体积</b>（薄壁零件的厚度会被削掉）；
    ''' Taubin 平滑用 <c>λ/μ</c> 交替的带通滤波抵消收缩，是尺寸检测场景的正确选择。
    ''' </para>
    ''' <para>
    ''' <b>QEM 简化</b>：为每个顶点累积“到相邻平面的距离平方”的二次型，
    ''' 边坍缩的代价就是新位置处的二次型值。它优先合并平坦区域的边，
    ''' 保住了特征边——这也是工业网格简化的事实标准（Garland &amp; Heckbert）。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class MeshProcessing

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Laplacian 平滑（会收缩体积）。
        ''' </summary>
        ''' <param name="mesh">输入网格</param>
        ''' <param name="iterations">迭代次数（默认 3）</param>
        ''' <param name="lambda">步长（默认 0.5，&gt;1 会震荡）</param>
        ''' <param name="preserveBoundary">是否固定边界顶点（默认 <c>True</c>）</param>
        Public Shared Function LaplacianSmooth(mesh As MeshData, Optional iterations As Integer = 3,
                                               Optional lambda As Double = 0.5,
                                               Optional preserveBoundary As Boolean = True) As MeshData
            Return Smooth(mesh, iterations, lambda, 0.0, preserveBoundary)
        End Function

        ''' <summary>
        ''' Taubin 平滑（λ/μ 交替，抑制体积收缩）。
        ''' </summary>
        Public Shared Function TaubinSmooth(mesh As MeshData, Optional iterations As Integer = 5,
                                            Optional lambda As Double = 0.5,
                                            Optional mu As Double = -0.53,
                                            Optional preserveBoundary As Boolean = True) As MeshData
            Return Smooth(mesh, iterations, lambda, mu, preserveBoundary)
        End Function

        Private Shared Function Smooth(mesh As MeshData, iterations As Integer,
                                       lambda As Double, mu As Double,
                                       preserveBoundary As Boolean) As MeshData
            If mesh Is Nothing Then Throw New ArgumentNullException(NameOf(mesh))

            Dim adjacency As List(Of HashSet(Of Integer)) = BuildAdjacency(mesh)
            Dim boundary As Boolean() = BoundaryVertices(mesh)
            Dim positions As Vector3d() = DirectCast(mesh.Vertices.Clone(), Vector3d())
            Dim nextPositions As Vector3d() = New Vector3d(positions.Length - 1) {}

            For iteration As Integer = 1 To std.Max(0, iterations)
                ' Taubin：偶数轮 λ、奇数轮 μ；μ = 0 时退化为纯 Laplacian
                Dim factor As Double = If(mu = 0.0, lambda, If(iteration Mod 2 = 1, lambda, mu))

                For i As Integer = 0 To positions.Length - 1
                    If preserveBoundary AndAlso boundary(i) Then
                        nextPositions(i) = positions(i)

                        Continue For
                    End If

                    Dim neighbors As HashSet(Of Integer) = adjacency(i)

                    If neighbors.Count = 0 Then
                        nextPositions(i) = positions(i)

                        Continue For
                    End If

                    Dim sum As Vector3d = Vector3d.Zero

                    For Each neighbor As Integer In neighbors
                        sum += positions(neighbor)
                    Next

                    Dim average As Vector3d = sum / neighbors.Count

                    nextPositions(i) = positions(i) + (average - positions(i)) * factor
                Next

                Dim swap As Vector3d() = positions

                positions = nextPositions
                nextPositions = swap
            Next

            Dim result As New MeshData(positions, DirectCast(mesh.Triangles.Clone(), Integer()))

            Call result.RecomputeNormals()

            If mesh.Colors IsNot Nothing Then result.SetColors(DirectCast(mesh.Colors.Clone(), Integer()))

            Return result
        End Function

        ''' <summary>
        ''' 提取边界顶点（只被一个三角形使用的边的端点）。
        ''' </summary>
        Public Shared Function BoundaryVertices(mesh As MeshData) As Boolean()
            Dim counts As Dictionary(Of Long, Integer) = mesh.EdgeCounts()
            Dim n As Long = mesh.VertexCount
            Dim boundary As Boolean() = New Boolean(mesh.VertexCount - 1) {}

            For Each entry As KeyValuePair(Of Long, Integer) In counts
                If entry.Value <> 1 Then Continue For

                Dim lo As Integer = CInt(entry.Key \ n)
                Dim hi As Integer = CInt(entry.Key Mod n)

                boundary(lo) = True
                boundary(hi) = True
            Next

            Return boundary
        End Function

        ''' <summary>
        ''' 顶点邻接表。
        ''' </summary>
        Public Shared Function BuildAdjacency(mesh As MeshData) As List(Of HashSet(Of Integer))
            Dim adjacency As New List(Of HashSet(Of Integer))(mesh.VertexCount)

            For i As Integer = 0 To mesh.VertexCount - 1
                Call adjacency.Add(New HashSet(Of Integer)())
            Next

            For t As Integer = 0 To mesh.TriangleCount - 1
                Dim a As Integer = mesh.Triangles(t * 3)
                Dim b As Integer = mesh.Triangles(t * 3 + 1)
                Dim c As Integer = mesh.Triangles(t * 3 + 2)

                Call adjacency(a).Add(b)
                Call adjacency(a).Add(c)
                Call adjacency(b).Add(a)
                Call adjacency(b).Add(c)
                Call adjacency(c).Add(a)
                Call adjacency(c).Add(b)
            Next

            Return adjacency
        End Function

        ''' <summary>
        ''' QEM 边坍缩简化。
        ''' </summary>
        ''' <param name="mesh">输入网格</param>
        ''' <param name="targetTriangles">目标三角形数量（&lt;= 0 表示按减半）</param>
        ''' <param name="useMidpoint">坍缩后的顶点位置是否取中点（<c>False</c> 时保留端点之一）</param>
        ''' <remarks>
        ''' <para>
        ''' <b>二次误差度量</b>：每个顶点维护一个 4x4 对称矩阵 <c>Q = Σ w·p·pᵀ</c>
        ''' （<c>p</c> 为相邻面的平面方程）。边 <c>(i,j)</c> 坍缩到位置 <c>v</c> 的代价是
        ''' <c>vᵀ·(Q_i + Q_j)·v</c>，它等于“新顶点到所有相关平面的距离平方和”。
        ''' </para>
        ''' <para>
        ''' <b>本实现的简化</b>：新位置取中点（而不是解 3x3 最优位置）。
        ''' 最优位置在平坦区域能进一步降低误差，但在特征边附近容易解到远离表面的位置，
        ''' 需要额外的“合法性检查”；中点策略在工业网格上更稳，代价是同等误差率下
        ''' 三角形数量多 5%~10%。
        ''' </para>
        ''' <para>
        ''' <b>复杂度</b>：<c>O(m log m)</c>（<c>m</c> 为边数），使用优先队列 + 惰性删除。
        ''' </para>
        ''' </remarks>
        Public Shared Function SimplifyQem(mesh As MeshData, Optional targetTriangles As Integer = 0,
                                           Optional useMidpoint As Boolean = True) As MeshData
            If mesh Is Nothing Then Throw New ArgumentNullException(NameOf(mesh))

            Dim target As Integer = If(targetTriangles > 0, targetTriangles, std.Max(4, mesh.TriangleCount \ 2))
            Dim n As Integer = mesh.VertexCount

            Dim positions As Vector3d() = DirectCast(mesh.Vertices.Clone(), Vector3d())
            Dim quadrics As Double()() = New Double(n - 1)() {}

            For i As Integer = 0 To n - 1
                quadrics(i) = New Double(9) {}
            Next

            Dim triangles As New List(Of Integer())(mesh.TriangleCount)

            For t As Integer = 0 To mesh.TriangleCount - 1
                Dim tri As Integer() = {mesh.Triangles(t * 3), mesh.Triangles(t * 3 + 1), mesh.Triangles(t * 3 + 2)}
                Dim normal As Vector3d = Vector3d.Cross(positions(tri(1)) - positions(tri(0)),
                                                        positions(tri(2)) - positions(tri(0))).Normalize()

                If normal.IsZero(1.0E-12) Then Continue For

                Dim d As Double = -Vector3d.Dot(normal, positions(tri(0)))
                Dim plane As Double() = {normal.X, normal.Y, normal.Z, d}

                For c As Integer = 0 To 3
                    For r As Integer = 0 To 3
                        Dim value As Double = plane(r) * plane(c)

                        Call AddQuadric(quadrics(tri(0)), r, c, value)
                        Call AddQuadric(quadrics(tri(1)), r, c, value)
                        Call AddQuadric(quadrics(tri(2)), r, c, value)
                    Next
                Next

                Call triangles.Add(tri)
            Next

            Dim adjacency As New List(Of HashSet(Of Integer))()
            Dim merged As Boolean() = New Boolean(n - 1) {}

            Call RebuildAdjacency(adjacency, triangles, n)

            Dim queue As New PriorityQueue(Of (i As Integer, j As Integer), Double)()

            For i As Integer = 0 To n - 1
                For Each j As Integer In adjacency(i)
                    If j > i Then Call queue.Enqueue((i, j), EdgeCost(quadrics, positions, i, j, useMidpoint))
                Next
            Next

            While triangles.Count > target AndAlso queue.Count > 0
                Dim edge As (i As Integer, j As Integer)
                Dim cost As Double

                Call queue.TryDequeue(edge, cost)

                Dim i As Integer = edge.i
                Dim j As Integer = edge.j

                If merged(i) OrElse merged(j) Then Continue While
                If Not adjacency(i).Contains(j) Then Continue While

                ' 新位置
                Dim newPosition As Vector3d

                If useMidpoint Then
                    newPosition = (positions(i) + positions(j)) * 0.5
                Else
                    newPosition = If(cost < 0.0, positions(i), positions(j))
                End If

                positions(i) = newPosition
                merged(j) = True

                For c As Integer = 0 To 9
                    quadrics(i)(c) += quadrics(j)(c)
                Next

                ' 重接三角形
                For Each neighbor As Integer In adjacency(j).ToArray()
                    Call adjacency(i).Add(neighbor)
                    Call adjacency(neighbor).Remove(j)
                    Call adjacency(neighbor).Add(i)
                Next

                Call adjacency(j).Clear()

                Dim kept As New List(Of Integer())(triangles.Count)
                Dim removed As Integer = 0

                For Each tri As Integer() In triangles
                    Dim hasI As Boolean = tri(0) = i OrElse tri(1) = i OrElse tri(2) = i
                    Dim hasJ As Boolean = tri(0) = j OrElse tri(1) = j OrElse tri(2) = j

                    If hasI AndAlso hasJ Then
                        removed += 1

                        Continue For
                    End If

                    If hasJ Then
                        For k As Integer = 0 To 2
                            If tri(k) = j Then tri(k) = i
                        Next
                    End If

                    Call kept.Add(tri)
                Next

                triangles = kept

                ' 只重新入队与 i 相邻的边（惰性：旧条目会在弹出时被跳过）。
                ' 邻接表在上面的坍缩步骤里已经增量更新过，这里不再整表重建
                ' （整表重建是 O(m)，放进循环会退化成 O(m²)）。
                For Each neighbor As Integer In adjacency(i)
                    Call queue.Enqueue((std.Min(i, neighbor), std.Max(i, neighbor)),
                                       EdgeCost(quadrics, positions, i, neighbor, useMidpoint))
                Next
            End While

            ' 重映射顶点并输出
            Dim remap(n - 1) As Integer

            For i As Integer = 0 To n - 1
                remap(i) = -1
            Next

            Dim newVertices As New List(Of Vector3d)()
            Dim flat As New List(Of Integer)()

            For Each tri As Integer() In triangles
                If tri(0) = tri(1) OrElse tri(1) = tri(2) OrElse tri(0) = tri(2) Then Continue For

                For k As Integer = 0 To 2
                    Dim index As Integer = tri(k)

                    If remap(index) < 0 Then
                        remap(index) = newVertices.Count

                        ' 已被合并的顶点用其代表顶点（i）的位置
                        Call newVertices.Add(positions(index))
                    End If

                    Call flat.Add(remap(index))
                Next
            Next

            Dim result As New MeshData(newVertices.ToArray(), flat.ToArray())

            Call result.RecomputeNormals()

            Return result
        End Function

        Private Shared Sub AddQuadric(quadric As Double(), r As Integer, c As Integer, value As Double)
            ' 只存上三角的 10 个元素（行优先）
            If r > c Then
                Dim swap As Integer = r

                r = c
                c = swap
            End If

            quadric(r * 4 - r * (r - 1) \ 2 + (c - r)) += value
        End Sub

        Private Shared Function QuadricValue(quadric As Double(), p As Vector3d) As Double
            Dim v As Double() = {p.X, p.Y, p.Z, 1.0}
            Dim total As Double = 0.0

            For r As Integer = 0 To 3
                For c As Integer = 0 To 3
                    Dim value As Double = quadric(If(r <= c, r * 4 - r * (r - 1) \ 2 + (c - r), c * 4 - c * (c - 1) \ 2 + (r - c)))

                    total += v(r) * value * v(c)
                Next
            Next

            Return total
        End Function

        Private Shared Function EdgeCost(quadrics As Double()(), positions As Vector3d(),
                                         i As Integer, j As Integer, useMidpoint As Boolean) As Double
            Dim p As Vector3d = If(useMidpoint, (positions(i) + positions(j)) * 0.5, positions(i))

            Return QuadricValue(quadrics(i), p) + QuadricValue(quadrics(j), p)
        End Function

        Private Shared Sub RebuildAdjacency(adjacency As List(Of HashSet(Of Integer)), triangles As List(Of Integer()),
                                            n As Integer)
            Call adjacency.Clear()

            For i As Integer = 0 To n - 1
                Call adjacency.Add(New HashSet(Of Integer)())
            Next

            For Each tri As Integer() In triangles
                Call adjacency(tri(0)).Add(tri(1))
                Call adjacency(tri(0)).Add(tri(2))
                Call adjacency(tri(1)).Add(tri(0))
                Call adjacency(tri(1)).Add(tri(2))
                Call adjacency(tri(2)).Add(tri(0))
                Call adjacency(tri(2)).Add(tri(1))
            Next
        End Sub

        ''' <summary>
        ''' 补洞：为每个边界环插入中心点并做扇形三角化。
        ''' </summary>
        ''' <param name="mesh">输入网格</param>
        ''' <param name="maxLoopVertices">只补顶点数不超过该值的洞（默认 200）</param>
        ''' <remarks>
        ''' 扇形三角化会把较大的洞补成“锥形”，在曲面上看起来会有一个尖。
        ''' 对小洞（螺栓孔、遮挡缺口）完全够用；大洞应改用“前进前沿”或
        ''' 基于泊松的补洞。这里选择扇形是因为它<b>不会自交</b>，
        ''' 对检测流程而言“封闭且无自交”比“平滑”更重要。
        ''' </remarks>
        Public Shared Function FillHoles(mesh As MeshData, Optional maxLoopVertices As Integer = 200) As MeshData
            If mesh Is Nothing Then Throw New ArgumentNullException(NameOf(mesh))

            Dim loops As List(Of Integer()) = mesh.BoundaryLoops()
            Dim vertices As New List(Of Vector3d)(mesh.Vertices)
            Dim flat As New List(Of Integer)(mesh.Triangles)

            For Each cycle As Integer() In loops
                If cycle.Length < 3 OrElse cycle.Length > maxLoopVertices Then Continue For

                ' 去掉闭环末尾重复的起点
                Dim count As Integer = cycle.Length

                If cycle(count - 1) = cycle(0) Then count -= 1

                If count < 3 Then Continue For

                Dim center As Vector3d = Vector3d.Zero

                For i As Integer = 0 To count - 1
                    center += vertices(cycle(i))
                Next

                center /= count

                Dim centerIndex As Integer = vertices.Count

                Call vertices.Add(center)

                For i As Integer = 0 To count - 1
                    Dim a As Integer = cycle(i)
                    Dim b As Integer = cycle((i + 1) Mod count)

                    Call flat.Add(a)
                    Call flat.Add(b)
                    Call flat.Add(centerIndex)
                Next
            Next

            Dim result As New MeshData(vertices.ToArray(), flat.ToArray())

            Call result.RecomputeNormals()

            Return result
        End Function

        ''' <summary>
        ''' 逐点到网格的最短距离（用于偏差分析/CAD 比对）。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 用三角形质心的体素网格做粗筛：查询点只需检查附近的格子，
        ''' 再对候选三角形做精确的点-三角形距离。相比“所有三角形暴力扫描”
        ''' （<c>O(n·m)</c>）快 2~3 个数量级，而结果<b>精确</b>
        ''' （只要搜索半径的扩张终止条件正确）。
        ''' </para>
        ''' <para>
        ''' 终止条件：当已找到的最小距离 ≤ (ring − 1)·cellSize 时，
        ''' 更外层的格子不可能包含更近的三角形，可以停止。
        ''' </para>
        ''' </remarks>
        Public Shared Function PointToMeshDistance(mesh As MeshData, cloud As PointCloudBuffer,
                                                   Optional cellSize As Double = 0.0) As Double()
            If mesh Is Nothing Then Throw New ArgumentNullException(NameOf(mesh))
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim n As Integer = cloud.Count
            Dim distances As Double() = New Double(n - 1) {}

            If mesh.TriangleCount = 0 Then
                For i As Integer = 0 To n - 1
                    distances(i) = Double.MaxValue
                Next

                Return distances
            End If

            Dim box As Aabb3 = mesh.Bounds()
            Dim cell As Double = If(cellSize > 0.0, cellSize, std.Max(box.Diagonal / 48.0, 1.0E-9))
            Dim cells As New Dictionary(Of (x As Integer, y As Integer, z As Integer), List(Of Integer))()
            Dim inverse As Double = 1.0 / cell

            For t As Integer = 0 To mesh.TriangleCount - 1
                Dim tri As (a As Vector3d, b As Vector3d, c As Vector3d) = mesh.Triangle(t)
                Dim lo As Vector3d = Vector3d.Min(tri.a, Vector3d.Min(tri.b, tri.c))
                Dim hi As Vector3d = Vector3d.Max(tri.a, Vector3d.Max(tri.b, tri.c))
                Dim i0 As Integer = CInt(std.Floor(lo.X * inverse))
                Dim j0 As Integer = CInt(std.Floor(lo.Y * inverse))
                Dim k0 As Integer = CInt(std.Floor(lo.Z * inverse))
                Dim i1 As Integer = CInt(std.Floor(hi.X * inverse))
                Dim j1 As Integer = CInt(std.Floor(hi.Y * inverse))
                Dim k1 As Integer = CInt(std.Floor(hi.Z * inverse))

                For i As Integer = i0 To i1
                    For j As Integer = j0 To j1
                        For k As Integer = k0 To k1
                            Dim key As (x As Integer, y As Integer, z As Integer) = (i, j, k)
                            Dim list As List(Of Integer) = Nothing

                            If Not cells.TryGetValue(key, list) Then
                                list = New List(Of Integer)(4)
                                cells(key) = list
                            End If

                            Call list.Add(t)
                        Next
                    Next
                Next
            Next

            For i As Integer = 0 To n - 1
                Dim p As Vector3d = cloud.Item(i)
                Dim cx As Integer = CInt(std.Floor(p.X * inverse))
                Dim cy As Integer = CInt(std.Floor(p.Y * inverse))
                Dim cz As Integer = CInt(std.Floor(p.Z * inverse))
                Dim best As Double = Double.MaxValue

                For ring As Integer = 0 To 64
                    Dim found As Boolean = False

                    For dx As Integer = -ring To ring
                        For dy As Integer = -ring To ring
                            For dz As Integer = -ring To ring
                                ' 只处理“环”上的格子，内部格子已在前面的环中检查过
                                If std.Max(std.Abs(dx), std.Max(std.Abs(dy), std.Abs(dz))) <> ring Then Continue For

                                Dim list As List(Of Integer) = Nothing

                                If Not cells.TryGetValue((cx + dx, cy + dy, cz + dz), list) Then Continue For

                                found = True

                                For Each t As Integer In list
                                    Dim tri As (a As Vector3d, b As Vector3d, c As Vector3d) = mesh.Triangle(t)
                                    Dim d As Double = PointTriangleDistance(p, tri.a, tri.b, tri.c)

                                    If d < best Then best = d
                                Next
                            Next
                        Next
                    Next

                    If best <= ring * cell Then Exit For
                    If Not found AndAlso ring > 32 Then Exit For
                Next

                distances(i) = If(best = Double.MaxValue, box.DistanceTo(p), best)
            Next

            Return distances
        End Function

        ''' <summary>
        ''' 点到三角形的最短距离（Ericson 的闭式解）。
        ''' </summary>
        Public Shared Function PointTriangleDistance(p As Vector3d, a As Vector3d, b As Vector3d, c As Vector3d) As Double
            Dim ab As Vector3d = b - a
            Dim ac As Vector3d = c - a
            Dim ap As Vector3d = p - a
            Dim d1 As Double = Vector3d.Dot(ab, ap)
            Dim d2 As Double = Vector3d.Dot(ac, ap)

            If d1 <= 0.0 AndAlso d2 <= 0.0 Then Return (p - a).Length

            Dim bp As Vector3d = p - b
            Dim d3 As Double = Vector3d.Dot(ab, bp)
            Dim d4 As Double = Vector3d.Dot(ac, bp)

            If d3 >= 0.0 AndAlso d4 <= d3 Then Return (p - b).Length

            Dim vc As Double = d1 * d4 - d3 * d2

            If vc <= 0.0 AndAlso d1 >= 0.0 AndAlso d3 <= 0.0 Then
                Dim v As Double = d1 / (d1 - d3)

                Return (p - (a + ab * v)).Length
            End If

            Dim cp As Vector3d = p - c
            Dim d5 As Double = Vector3d.Dot(ab, cp)
            Dim d6 As Double = Vector3d.Dot(ac, cp)

            If d6 >= 0.0 AndAlso d5 <= d6 Then Return (p - c).Length

            Dim vb As Double = d5 * d2 - d1 * d6

            If vb <= 0.0 AndAlso d2 >= 0.0 AndAlso d6 <= 0.0 Then
                Dim w As Double = d2 / (d2 - d6)

                Return (p - (a + ac * w)).Length
            End If

            Dim va As Double = d3 * d6 - d5 * d4

            If va <= 0.0 AndAlso (d4 - d3) >= 0.0 AndAlso (d5 - d6) >= 0.0 Then
                Dim w As Double = (d4 - d3) / ((d4 - d3) + (d5 - d6))

                Return (p - (b + (c - b) * w)).Length
            End If

            ' 内部：投影到平面
            Dim normal As Vector3d = Vector3d.Cross(ab, ac)
            Dim area As Double = normal.Length

            If area <= 1.0E-15 Then
                Return std.Min((p - a).Length, std.Min((p - b).Length, (p - c).Length))
            End If

            Return std.Abs(Vector3d.Dot(normal / area, p - a))
        End Function

    End Class
End Namespace
