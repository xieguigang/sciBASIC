Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Features
Imports std = System.Math

Namespace Reconstruction

    ''' <summary>
    ''' 贪心三角化：局部投影 + 二维 Delaunay + 裁剪，逐点增量构造网格。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么需要它</b>：泊松重建输出的是“平滑但偏移”的隐式曲面，
    ''' BPA 要求精确的空球条件；贪心三角化介于两者之间——
    ''' 它<b>直接使用原始点作为顶点</b>（保形），对噪声的容忍度比 BPA 高，
    ''' 而且不需要法向一致性（只用法向做方向判断）。
    ''' </para>
    ''' <para>
    ''' <b>算法</b>（对每个点）：
    ''' </para>
    ''' <list type="number">
    ''' <item><description>取 <c>k</c> 个近邻，投影到以该点法向为 <c>z</c> 轴的切平面上；</description></item>
    ''' <item><description>对投影后的二维点做 Delaunay 三角化（<see cref="Delaunay2D"/>）；</description></item>
    ''' <item><description>只保留满足“边长 ≤ 阈值、法向与原法向一致”的三角形；</description></item>
    ''' <item><description>按顶点三元组去重（相邻点会重复生成同一三角形）。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>裁剪条件</b>：边长阈值控制“不能跨过空洞”，法向夹角阈值控制
    ''' “不能连接折角两侧”。两者缺一不可——只限边长会在平行薄壁之间架桥，
    ''' 只限法向会在空洞上生成大三角形。
    ''' </para>
    ''' <para>
    ''' <b>复杂度</b>：<c>O(n·(k log k + k))</c>。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class GreedyTriangulation

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 贪心三角化重建。
        ''' </summary>
        ''' <param name="cloud">输入点云（缺法向时自动估计）</param>
        ''' <param name="k">每个点的近邻数（默认 24）</param>
        ''' <param name="maxEdgeLength">最大边长；<c>≤ 0</c> 表示按平均点距自适应（6 倍）</param>
        ''' <param name="maxNormalDeviationDegrees">三角形法向与顶点法向的最大夹角（默认 45°）</param>
        ''' <param name="options">并行选项</param>
        Public Shared Function Reconstruct(cloud As PointCloudBuffer, Optional k As Integer = 24,
                                           Optional maxEdgeLength As Double = 0.0,
                                           Optional maxNormalDeviationDegrees As Double = 45.0,
                                           Optional options As CloudyOptions = Nothing) As MeshData
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim n As Integer = cloud.Count
            Dim global_ As CloudyOptions = If(options, CloudyOptions.Default)

            If n < 3 Then Return New MeshData(cloud.ToVector3d(), Array.Empty(Of Integer)())

            Dim source As PointCloudBuffer = cloud

            If Not source.HasNormals Then source = NormalEstimation.Estimate(cloud, 20, 0.0, global_)

            Dim spacing As Double = source.EstimateAverageSpacing()
            Dim maxEdge As Double = If(maxEdgeLength > 0.0, maxEdgeLength, std.Max(spacing * 6.0, 1.0E-9))
            Dim cosLimit As Double = std.Cos(std.Max(0.0, std.Min(180.0, maxNormalDeviationDegrees)) * std.PI / 180.0)

            Dim tree As New KdTree3D(source)
            Dim triangles As New Dictionary(Of Long, Integer())()
            Dim sync As New Object()
            Dim kk As Integer = std.Max(6, k)

            Call global_.ForRange(n,
                Sub(start, ends)
                    Dim query As New NeighborhoodQuery(tree, kk + 1)
                    Dim buffer As Neighborhood = query.Rent()
                    Dim local As New List(Of Integer())(32)

                    For i As Integer = start To ends - 1
                        Call tree.Knn(source.X(i), source.Y(i), source.Z(i), kk, buffer, i)

                        Dim count As Integer = buffer.Count

                        If count < 3 Then Continue For

                        Dim normal As Vector3d = source.Normal(i)

                        If normal.IsZero(1.0E-9) Then Continue For

                        Dim reference As Vector3d = If(std.Abs(normal.X) < 0.9, Vector3d.UnitX, Vector3d.UnitY)
                        Dim u As Vector3d = (reference - normal * Vector3d.Dot(reference, normal)).Normalize()
                        Dim v As Vector3d = Vector3d.Cross(normal, u)

                        Dim xs(count + 1) As Double
                        Dim ys(count + 1) As Double
                        Dim ids(count + 1) As Integer

                        ' 第 0 个局部点是中心点自身
                        xs(0) = 0.0
                        ys(0) = 0.0
                        ids(0) = i

                        For j As Integer = 0 To count - 1
                            Dim q As Integer = buffer.Indices(j)
                            Dim d As Vector3d = source.Item(q) - source.Item(i)

                            xs(j + 1) = Vector3d.Dot(d, u)
                            ys(j + 1) = Vector3d.Dot(d, v)
                            ids(j + 1) = q
                        Next

                        Dim localTriangles As Integer()() = Delaunay2D.Triangulate(xs, ys)

                        For Each tri As Integer() In localTriangles
                            Dim a As Integer = ids(tri(0))
                            Dim b As Integer = ids(tri(1))
                            Dim c As Integer = ids(tri(2))

                            Dim pa As Vector3d = source.Item(a)
                            Dim pb As Vector3d = source.Item(b)
                            Dim pc As Vector3d = source.Item(c)

                            If (pb - pa).Length > maxEdge Then Continue For
                            If (pc - pb).Length > maxEdge Then Continue For
                            If (pa - pc).Length > maxEdge Then Continue For

                            Dim face As Vector3d = Vector3d.Cross(pb - pa, pc - pa).Normalize()

                            If std.Abs(Vector3d.Dot(face, normal)) < cosLimit Then Continue For

                            Dim swap As Boolean = Vector3d.Dot(face, normal) < 0.0

                            If swap Then
                                Call local.Add({a, c, b})
                            Else
                                Call local.Add({a, b, c})
                            End If
                        Next
                    Next

                    SyncLock sync
                        For Each tri As Integer() In local
                            Dim key As Long = TriangleKey(tri, n)

                            If Not triangles.ContainsKey(key) Then triangles(key) = tri
                        Next
                    End SyncLock
                End Sub, 256)

            Dim list As New List(Of Integer())(triangles.Count)

            For Each entry In triangles
                Call list.Add(entry.Value)
            Next

            Call list.Sort(Function(x, y)
                               Dim cmp As Integer = x(0).CompareTo(y(0))

                               If cmp <> 0 Then Return cmp

                               cmp = x(1).CompareTo(y(1))

                               If cmp <> 0 Then Return cmp

                               Return x(2).CompareTo(y(2))
                           End Function)

            Dim flat As Integer() = New Integer(list.Count * 3 - 1) {}

            For i As Integer = 0 To list.Count - 1
                flat(i * 3) = list(i)(0)
                flat(i * 3 + 1) = list(i)(1)
                flat(i * 3 + 2) = list(i)(2)
            Next

            Dim mesh As New MeshData(source.ToVector3d(), flat)

            If source.HasNormals Then
                Dim normals(source.Count - 1) As Vector3d

                For i As Integer = 0 To source.Count - 1
                    normals(i) = source.Normal(i)
                Next

                Call mesh.SetNormals(normals)
            End If

            Return mesh
        End Function

        Private Shared Function TriangleKey(tri As Integer(), n As Integer) As Long
            Dim x As Integer = std.Min(tri(0), std.Min(tri(1), tri(2)))
            Dim z As Integer = std.Max(tri(0), std.Max(tri(1), tri(2)))
            Dim y As Integer = tri(0) + tri(1) + tri(2) - x - z

            Return (CLng(x) * n + y) * n + z
        End Function

    End Class
End Namespace
