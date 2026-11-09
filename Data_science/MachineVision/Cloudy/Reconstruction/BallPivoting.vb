Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Features
Imports std = System.Math

Namespace Reconstruction

    ''' <summary>
    ''' BPA(Ball Pivoting Algorithm)滚球法重建。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>原理</b>：用一个半径为 <c>r</c> 的球在点云表面“滚动”。
    ''' 球同时接触三个点时，这三个点构成一个三角形；
    ''' 球滚到相邻位置时继续生成三角形，直到覆盖整个表面。
    ''' 判据是<b>空球条件</b>：球内不能包含任何其他点，否则说明球穿过了表面。
    ''' </para>
    ''' <para>
    ''' <b>与泊松重建的差别</b>：BPA 生成的网格<b>严格通过原始点</b>（保形），
    ''' 因此适合“尺寸检测”；泊松会做全局平滑（水密但会削平细节）。
    ''' BPA 的缺点是半径需要根据点距选择，且对噪声敏感（噪声会让球“卡住”）。
    ''' </para>
    ''' <para>
    ''' <b>本实现的简化（必须知悉）</b>：完整 BPA 需要一个“前沿(front)”数据结构，
    ''' 按球的滚动方向逐个扩张三角形。这里实现的是<b>逐点候选三角形枚举</b>版本：
    ''' 对每个点枚举其近邻点对，检查“存在一个半径 <c>r</c> 的球同时接触三点、
    ''' 且球心附近没有其他点”，满足则生成三角形，最后按顶点三元组去重。
    ''' 这样省掉了复杂的前沿管理，得到的是同一组“合法三角形”的子集
    ''' （可能漏掉少量边角三角形），但对扫描数据通常能重建出封闭区域，
    ''' 且不会产生自交。需要严格复刻时请替换为前沿驱动版本。
    ''' </para>
    ''' <para>
    ''' <b>复杂度</b>：<c>O(n·k²)</c>（<c>k</c> 为 <c>2r</c> 内的邻点数）。
    ''' 建议先体素降采样到 1e4~1e5 点再重建。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class BallPivoting

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 滚球法重建。
        ''' </summary>
        ''' <param name="cloud">输入点云（缺法向时自动估计）</param>
        ''' <param name="radius">滚球半径；<c>≤ 0</c> 表示按平均点距自适应（2.5 倍）</param>
        ''' <param name="maxNeighbors">单点最多枚举多少个邻点（控制 <c>k²</c> 开销，默认 24）</param>
        ''' <param name="emptyBallTolerance">空球判定的容差（默认 0.9，即球心 0.9r 内不能有点）</param>
        ''' <param name="options">并行选项</param>
        Public Shared Function Reconstruct(cloud As PointCloudBuffer, Optional radius As Double = 0.0,
                                           Optional maxNeighbors As Integer = 24,
                                           Optional emptyBallTolerance As Double = 0.9,
                                           Optional options As CloudyOptions = Nothing) As MeshData
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim n As Integer = cloud.Count
            Dim global_ As CloudyOptions = If(options, CloudyOptions.Default)

            If n < 3 Then Return New MeshData(cloud.ToVector3d(), Array.Empty(Of Integer)())

            Dim source As PointCloudBuffer = cloud

            If Not source.HasNormals Then source = NormalEstimation.Estimate(cloud, 20, 0.0, global_)

            Dim r As Double = If(radius > 0.0, radius, std.Max(source.EstimateAverageSpacing() * 2.5, 1.0E-9))
            Dim tree As New KdTree3D(source)
            Dim emptyRadius As Double = std.Max(0.1, std.Min(1.0, emptyBallTolerance)) * r

            Dim found As New Dictionary(Of Long, Integer())()
            Dim sync As New Object()
            Dim k As Integer = std.Max(6, maxNeighbors)

            Call global_.ForRange(n,
                Sub(start, ends)
                    Dim query As New NeighborhoodQuery(tree, k + 1)
                    Dim buffer As Neighborhood = query.Rent()
                    Dim local As New List(Of Integer())(16)

                    For i As Integer = start To ends - 1
                        Call tree.RadiusSearch(source.X(i), source.Y(i), source.Z(i), 2.0 * r, buffer, i, k)

                        Dim neighbors As Integer() = buffer.ToIndexArray()
                        Dim normals(neighbors.Length - 1) As Vector3d

                        For j As Integer = 0 To neighbors.Length - 1
                            normals(j) = source.Normal(neighbors(j))
                        Next

                        Dim sourceNormal As Vector3d = source.Normal(i)

                        For a As Integer = 0 To neighbors.Length - 1
                            For b As Integer = a + 1 To neighbors.Length - 1
                                Dim triangle As Integer() = TryTriangle(source, tree, i, neighbors(a), neighbors(b),
                                                                        r, emptyRadius, sourceNormal, normals(a), normals(b))

                                If triangle IsNot Nothing Then Call local.Add(triangle)
                            Next
                        Next
                    Next

                    SyncLock sync
                        For Each triangle As Integer() In local
                            Dim key As Long = TriangleKey(triangle(0), triangle(1), triangle(2), n)

                            If Not found.ContainsKey(key) Then found(key) = triangle
                        Next
                    End SyncLock
                End Sub, 512)

            Dim triangles As New List(Of Integer())(found.Count)

            For Each entry In found
                Call triangles.Add(entry.Value)
            Next

            ' 排序保证输出确定性
            Call triangles.Sort(Function(x, y)
                                    Dim cmp As Integer = x(0).CompareTo(y(0))

                                    If cmp <> 0 Then Return cmp

                                    cmp = x(1).CompareTo(y(1))

                                    If cmp <> 0 Then Return cmp

                                    Return x(2).CompareTo(y(2))
                                End Function)

            Dim flat As Integer() = New Integer(triangles.Count * 3 - 1) {}

            For i As Integer = 0 To triangles.Count - 1
                flat(i * 3) = triangles(i)(0)
                flat(i * 3 + 1) = triangles(i)(1)
                flat(i * 3 + 2) = triangles(i)(2)
            Next

            Dim mesh As New MeshData(source.ToVector3d(), flat)

            Call mesh.RecomputeNormals()

            Return mesh
        End Function

        ''' <summary>
        ''' 检查三点是否构成一个“合法”的滚球三角形。
        ''' </summary>
        Private Shared Function TryTriangle(cloud As PointCloudBuffer, tree As KdTree3D,
                                            a As Integer, b As Integer, c As Integer,
                                            radius As Double, emptyRadius As Double,
                                            normalA As Vector3d, normalB As Vector3d, normalC As Vector3d) As Integer()
            Dim pa As Vector3d = cloud.Item(a)
            Dim pb As Vector3d = cloud.Item(b)
            Dim pc As Vector3d = cloud.Item(c)

            Dim ab As Vector3d = pb - pa
            Dim ac As Vector3d = pc - pa
            Dim normal As Vector3d = Vector3d.Cross(ab, ac)
            Dim twiceArea As Double = normal.Length

            If twiceArea <= 1.0E-12 Then Return Nothing

            normal = normal / twiceArea

            ' 三点法向必须先一致，否则会把不同的面连在一起
            If Vector3d.Dot(normal, normalA) < 0.2 Then Return Nothing
            If Vector3d.Dot(normal, normalB) < 0.2 Then Return Nothing
            If Vector3d.Dot(normal, normalC) < 0.2 Then Return Nothing

            ' 外接圆心与半径
            Dim center As Vector3d
            Dim circumradius As Double = Circumcircle(pa, pb, pc, center)

            If circumradius > radius * 1.05 Then Return Nothing

            ' 球心：沿法向偏移 √(r² − ρ²)
            Dim h2 As Double = radius * radius - circumradius * circumradius

            If h2 < 0.0 Then Return Nothing

            Dim offset As Double = std.Sqrt(h2)
            Dim sphereCenter As Vector3d = center + normal * offset
            Dim nearest As (index As Integer, distanceSquared As Double) =
                tree.Nearest(sphereCenter.X, sphereCenter.Y, sphereCenter.Z)

            If nearest.index >= 0 AndAlso std.Sqrt(nearest.distanceSquared) < emptyRadius Then
                ' 球内有点：换另一侧的球心再试
                Dim other As Vector3d = center - normal * offset

                nearest = tree.Nearest(other.X, other.Y, other.Z)

                If nearest.index >= 0 AndAlso std.Sqrt(nearest.distanceSquared) < emptyRadius Then
                    Return Nothing
                End If
            End If

            ' 按法向确定环绕方向（保证 CCW）
            If Vector3d.Dot(Vector3d.Cross(pb - pa, pc - pa), normalA) >= 0.0 Then
                Return {a, b, c}
            End If

            Return {a, c, b}
        End Function

        ''' <summary>
        ''' 三点外接圆（返回半径与圆心）。
        ''' </summary>
        Private Shared Function Circumcircle(a As Vector3d, b As Vector3d, c As Vector3d, ByRef center As Vector3d) As Double
            Dim ab As Vector3d = b - a
            Dim ac As Vector3d = c - a
            Dim normal As Vector3d = Vector3d.Cross(ab, ac)
            Dim area2 As Double = normal.Length

            If area2 <= 1.0E-15 Then
                center = a

                Return Double.MaxValue
            End If

            ' |ab|²·(ac×n) + |ac|²·(n×ab) 形式的圆心公式
            Dim term1 As Vector3d = Vector3d.Cross(ac, normal) * ab.LengthSquared
            Dim term2 As Vector3d = Vector3d.Cross(normal, ab) * ac.LengthSquared
            Dim offset As Vector3d = (term1 + term2) / (2.0 * area2 * area2)

            center = a + offset

            Return offset.Length
        End Function

        Private Shared Function TriangleKey(a As Integer, b As Integer, c As Integer, n As Integer) As Long
            Dim x As Integer = std.Min(a, std.Min(b, c))
            Dim z As Integer = std.Max(a, std.Max(b, c))
            Dim y As Integer = a + b + c - x - z

            Return (CLng(x) * n + y) * n + z
        End Function

    End Class
End Namespace
