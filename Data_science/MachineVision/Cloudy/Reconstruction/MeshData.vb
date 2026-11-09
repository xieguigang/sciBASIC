Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Reconstruction

    ''' <summary>
    ''' 三角网格容器：顶点 + 三角形索引 + 可选法向/颜色。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>数据布局</b>：顶点用 <see cref="Vector3d"/> 数组（AoS），三角形用<b>扁平</b>的
    ''' <c>Integer()</c>（每 3 个元素一个三角形）。与点云选择 SoA 的理由不同：
    ''' 网格算法的瓶颈在<b>拓扑遍历</b>（邻接、边、环）而不是 SIMD 算术，
    ''' 因此结构体数组的可读性优势更重要；扁平索引数组则避免了
    ''' “每个三角形一个 <c>Integer()</c>”带来的数万个小对象。
    ''' </para>
    ''' <para>
    ''' <b>环绕方向</b>：三角形按右手定则逆时针（CCW）存储，法向由
    ''' <c>(b−a)×(c−a)</c> 决定。包围体积、法向、补洞都依赖这一约定。
    ''' </para>
    ''' <para>
    ''' <b>属性</b>：法向与颜色都是可选的（<c>Nothing</c> 表示未计算/无颜色）；
    ''' 长度必须与顶点数一致，由 <see cref="SetNormals(Vector3d())"/> 校验。
    ''' </para>
    ''' </remarks>
    Public Class MeshData

        Private m_vertices As Vector3d()
        Private m_normals As Vector3d()
        Private m_colors As Integer()

        ''' <summary>三角形索引（每 3 个元素构成一个三角形，CCW）。</summary>
        Public Property Triangles As Integer()

        Public Sub New(vertices As Vector3d(), triangles As Integer())
            If vertices Is Nothing Then Throw New ArgumentNullException(NameOf(vertices))
            If triangles Is Nothing Then Throw New ArgumentNullException(NameOf(triangles))
            If triangles.Length Mod 3 <> 0 Then
                Throw New ArgumentException($"the triangle index array length ({triangles.Length}) should be a multiple of 3!", NameOf(triangles))
            End If

            m_vertices = vertices
            Me.Triangles = triangles
        End Sub

        ''' <summary>顶点数组。</summary>
        Public ReadOnly Property Vertices As Vector3d()
            Get
                Return m_vertices
            End Get
        End Property

        ''' <summary>顶点数。</summary>
        Public ReadOnly Property VertexCount As Integer
            Get
                Return m_vertices.Length
            End Get
        End Property

        ''' <summary>三角形数。</summary>
        Public ReadOnly Property TriangleCount As Integer
            Get
                Return If(Triangles Is Nothing, 0, Triangles.Length \ 3)
            End Get
        End Property

        ''' <summary>顶点法向（未计算时为 <c>Nothing</c>）。</summary>
        Public ReadOnly Property Normals As Vector3d()
            Get
                Return m_normals
            End Get
        End Property

        ''' <summary>顶点颜色（ARGB；未提供时为 <c>Nothing</c>）。</summary>
        Public ReadOnly Property Colors As Integer()
            Get
                Return m_colors
            End Get
        End Property

        ''' <summary>设置顶点法向。</summary>
        Public Sub SetNormals(normals As Vector3d())
            If normals IsNot Nothing AndAlso normals.Length <> VertexCount Then
                Throw New ArgumentException($"the normal count ({normals.Length}) should equal the vertex count ({VertexCount})!", NameOf(normals))
            End If

            m_normals = normals
        End Sub

        ''' <summary>设置顶点颜色。</summary>
        Public Sub SetColors(colors As Integer())
            If colors IsNot Nothing AndAlso colors.Length <> VertexCount Then
                Throw New ArgumentException($"the color count ({colors.Length}) should equal the vertex count ({VertexCount})!", NameOf(colors))
            End If

            m_colors = colors
        End Sub

        ''' <summary>取第 <paramref name="index"/> 个三角形的三个顶点。</summary>
        Public Function Triangle(index As Integer) As (a As Vector3d, b As Vector3d, c As Vector3d)
            Dim offset As Integer = index * 3

            Return (m_vertices(Triangles(offset)), m_vertices(Triangles(offset + 1)), m_vertices(Triangles(offset + 2)))
        End Function

        ''' <summary>第 <paramref name="index"/> 个三角形的单位法向（退化时为零向量）。</summary>
        Public Function FaceNormal(index As Integer) As Vector3d
            Dim tri As (a As Vector3d, b As Vector3d, c As Vector3d) = Triangle(index)
            Dim normal As Vector3d = Vector3d.Cross(tri.b - tri.a, tri.c - tri.a)

            Return normal.Normalize()
        End Function

        ''' <summary>包围盒。</summary>
        Public Function Bounds() As Aabb3
            Return Aabb3.FromPoints(m_vertices)
        End Function

        ''' <summary>表面积。</summary>
        Public Function SurfaceArea() As Double
            Dim area As Double = 0.0

            For i As Integer = 0 To TriangleCount - 1
                Dim tri As (a As Vector3d, b As Vector3d, c As Vector3d) = Triangle(i)

                area += Vector3d.Cross(tri.b - tri.a, tri.c - tri.a).Length * 0.5
            Next

            Return area
        End Function

        ''' <summary>
        ''' 有符号体积（对闭合网格有效；正负取决于环绕方向）。
        ''' </summary>
        ''' <remarks>
        ''' 用散度定理：<c>V = 1/6·Σ (a × b)·c</c>，即每个三角形与原点构成的
        ''' 四面体有符号体积之和。对封闭网格，原点的位置不影响结果；
        ''' 对开放网格结果没有几何意义（这也是它常被用来检测“是否封闭”的原因）。
        ''' </remarks>
        Public Function Volume() As Double
            Dim total As Double = 0.0

            For i As Integer = 0 To TriangleCount - 1
                Dim tri As (a As Vector3d, b As Vector3d, c As Vector3d) = Triangle(i)

                total += Vector3d.Dot(Vector3d.Cross(tri.a, tri.b), tri.c)
            Next

            Return total / 6.0
        End Function

        ''' <summary>
        ''' 是否为封闭网格（每条边恰好被两个三角形共享）。
        ''' </summary>
        Public Function IsClosed() As Boolean
            Dim counts As Dictionary(Of Long, Integer) = EdgeCounts()

            For Each entry As KeyValuePair(Of Long, Integer) In counts
                If entry.Value <> 2 Then Return False
            Next

            Return True
        End Function

        ''' <summary>
        ''' 欧拉示性数 <c>χ = V − E + F</c>（闭合可定向曲面：球面 2、环面 0）。
        ''' </summary>
        Public Function EulerCharacteristic() As Integer
            Return VertexCount - EdgeCounts().Count + TriangleCount
        End Function

        ''' <summary>边的共享次数（键为 <c>min·N + max</c> 的打包值）。</summary>
        Public Function EdgeCounts() As Dictionary(Of Long, Integer)
            Dim counts As New Dictionary(Of Long, Integer)(TriangleCount * 3)
            Dim n As Long = VertexCount

            For i As Integer = 0 To TriangleCount - 1
                Dim a As Integer = Triangles(i * 3)
                Dim b As Integer = Triangles(i * 3 + 1)
                Dim c As Integer = Triangles(i * 3 + 2)

                Call AddEdge(counts, a, b, n)
                Call AddEdge(counts, b, c, n)
                Call AddEdge(counts, c, a, n)
            Next

            Return counts
        End Function

        Private Shared Sub AddEdge(counts As Dictionary(Of Long, Integer), a As Integer, b As Integer, n As Long)
            Dim lo As Integer = std.Min(a, b)
            Dim hi As Integer = std.Max(a, b)
            Dim key As Long = lo * n + hi
            Dim count As Integer

            counts.TryGetValue(key, count)
            counts(key) = count + 1
        End Sub

        ''' <summary>
        ''' 提取边界环（只被一个三角形使用的边构成的闭合路径）。
        ''' </summary>
        ''' <remarks>
        ''' 用于补洞：每个边界环对应一个洞。实现为“半边图上的环路追踪”：
        ''' 以边界边为有向边，按起点索引继续寻找下一条边界边，直到回到起点。
        ''' 复杂度 <c>O(E)</c>。
        ''' </remarks>
        Public Function BoundaryLoops() As List(Of Integer())
            Dim counts As Dictionary(Of Long, Integer) = EdgeCounts()
            Dim n As Long = VertexCount
            Dim boundary As New Dictionary(Of Integer, List(Of Integer))()

            ' 收集有向边界边（保留原三角形中的方向）
            For i As Integer = 0 To TriangleCount - 1
                Dim a As Integer = Triangles(i * 3)
                Dim b As Integer = Triangles(i * 3 + 1)
                Dim c As Integer = Triangles(i * 3 + 2)

                Call AddBoundary(counts, boundary, a, b, n)
                Call AddBoundary(counts, boundary, b, c, n)
                Call AddBoundary(counts, boundary, c, a, n)
            Next

            Dim loops As New List(Of Integer())()
            Dim visited As New HashSet(Of Long)()

            For Each entry As KeyValuePair(Of Integer, List(Of Integer)) In boundary
                For Each nextVertex As Integer In entry.Value
                    Dim start As Integer = entry.Key
                    Dim key As Long = CLng(start) * n + nextVertex

                    If visited.Contains(key) Then Continue For

                    Dim cycle As New List(Of Integer)()
                    Dim current As Integer = start
                    Dim following As Integer = nextVertex
                    Dim guard As Integer = 0

                    While guard < VertexCount + 4
                        guard += 1
                        Call cycle.Add(current)
                        Call visited.Add(CLng(current) * n + following)

                        Dim candidates As List(Of Integer) = Nothing

                        If Not boundary.TryGetValue(following, candidates) Then Exit While

                        Dim advanced As Boolean = False

                        For Each candidate As Integer In candidates
                            Dim candidateKey As Long = CLng(following) * n + candidate

                            If Not visited.Contains(candidateKey) Then
                                current = following
                                following = candidate
                                advanced = True

                                Exit For
                            End If
                        Next

                        If Not advanced Then
                            Call cycle.Add(following)

                            Exit While
                        End If

                        If following = start Then
                            Call cycle.Add(following)

                            Exit While
                        End If
                    End While

                    If cycle.Count >= 3 Then Call loops.Add(cycle.ToArray())
                Next
            Next

            Return loops
        End Function

        Private Shared Sub AddBoundary(counts As Dictionary(Of Long, Integer), boundary As Dictionary(Of Integer, List(Of Integer)),
                                       a As Integer, b As Integer, n As Long)
            Dim lo As Integer = std.Min(a, b)
            Dim hi As Integer = std.Max(a, b)

            If counts(lo * n + hi) <> 1 Then Return

            Dim list As List(Of Integer) = Nothing

            If Not boundary.TryGetValue(a, list) Then
                list = New List(Of Integer)(2)
                boundary(a) = list
            End If

            Call list.Add(b)
        End Sub

        ''' <summary>把网格转换为点云（顶点 + 法向 + 颜色）。</summary>
        Public Function ToPointCloud() As PointCloudBuffer
            Dim cloud As PointCloudBuffer = PointCloudBuffer.NewCloud(VertexCount)

            For i As Integer = 0 To VertexCount - 1
                cloud.X(i) = m_vertices(i).X
                cloud.Y(i) = m_vertices(i).Y
                cloud.Z(i) = m_vertices(i).Z
            Next

            If m_normals IsNot Nothing Then
                Dim nx(VertexCount - 1) As Double
                Dim ny(VertexCount - 1) As Double
                Dim nz(VertexCount - 1) As Double

                For i As Integer = 0 To VertexCount - 1
                    nx(i) = m_normals(i).X
                    ny(i) = m_normals(i).Y
                    nz(i) = m_normals(i).Z
                Next

                Call cloud.SetNormals(nx, ny, nz)
            End If

            If m_colors IsNot Nothing Then cloud.Color = DirectCast(m_colors.Clone(), Integer())

            Return cloud
        End Function

        ''' <summary>
        ''' 由点云生成“无三角形”的网格容器（顶点与三角形数组由后续算法填充）。
        ''' </summary>
        Public Shared Function FromPointCloud(cloud As PointCloudBuffer) As MeshData
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim vertices As Vector3d() = cloud.ToVector3d()
            Dim mesh As New MeshData(vertices, Array.Empty(Of Integer)())

            If cloud.HasNormals Then
                Dim normals(cloud.Count - 1) As Vector3d

                For i As Integer = 0 To cloud.Count - 1
                    normals(i) = cloud.Normal(i)
                Next

                Call mesh.SetNormals(normals)
            End If

            If cloud.Color IsNot Nothing Then mesh.SetColors(DirectCast(cloud.Color.Clone(), Integer()))

            Return mesh
        End Function

        ''' <summary>
        ''' 面积加权的顶点法向重算（网格后处理的必备步骤）。
        ''' </summary>
        Public Sub RecomputeNormals()
            Dim accumulated As Vector3d() = New Vector3d(VertexCount - 1) {}

            For i As Integer = 0 To TriangleCount - 1
                Dim tri As (a As Vector3d, b As Vector3d, c As Vector3d) = Triangle(i)
                Dim normal As Vector3d = Vector3d.Cross(tri.b - tri.a, tri.c - tri.a)

                ' 叉积长度即两倍面积，因此这里天然是面积加权
                accumulated(Triangles(i * 3)) += normal
                accumulated(Triangles(i * 3 + 1)) += normal
                accumulated(Triangles(i * 3 + 2)) += normal
            Next

            For i As Integer = 0 To VertexCount - 1
                accumulated(i) = accumulated(i).Normalize()
            Next

            m_normals = accumulated
        End Sub

        ''' <summary>合并多个网格（顶点偏移后拼接三角形）。</summary>
        Public Shared Function Merge(meshes As IEnumerable(Of MeshData)) As MeshData
            If meshes Is Nothing Then Throw New ArgumentNullException(NameOf(meshes))

            Dim vertices As New List(Of Vector3d)()
            Dim triangles As New List(Of Integer)()

            For Each mesh As MeshData In meshes
                If mesh Is Nothing Then Continue For

                Dim offset As Integer = vertices.Count

                Call vertices.AddRange(mesh.Vertices)

                For Each index As Integer In mesh.Triangles
                    Call triangles.Add(index + offset)
                Next
            Next

            Return New MeshData(vertices.ToArray(), triangles.ToArray())
        End Function

        Public Overrides Function ToString() As String
            Return $"mesh: {VertexCount} vertices, {TriangleCount} triangles, closed={IsClosed()}"
        End Function

    End Class
End Namespace
