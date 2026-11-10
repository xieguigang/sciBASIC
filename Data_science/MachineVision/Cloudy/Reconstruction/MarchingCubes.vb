Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Reconstruction

    ''' <summary>
    ''' 等值面提取（Marching Cubes 的四面体分解实现）。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么用四面体分解</b>：经典 Marching Cubes 的 256 种立方体情形中存在
    ''' <b>二义性</b>（同一个立方体可以连出两种不同拓扑），需要额外的
    ''' 二义性消解表才能保证水密。Marching Tetrahedra 把每个立方体拆成 6 个四面体，
    ''' 而四面体只有 16 种情形、<b>没有二义性</b>，生成的网格天然一致且封闭。
    ''' 代价是三角形数量略多（约 +20%），对重建质量没有影响。
    ''' </para>
    ''' <para>
    ''' <b>顶点去重</b>：同一个交点会被相邻的四面体共享，因此用
    ''' “两个栅格点的打包编号”作为键去重。这样输出网格的顶点数约等于
    ''' 等值面穿过的栅格边数，而不是三角形数的 3 倍。
    ''' </para>
    ''' <para>
    ''' <b>用途</b>：泊松重建（<see cref="PoissonReconstruction"/>）与
    ''' TSDF 融合（<see cref="TsdfFusion"/>）都输出标量场，最后统一由这里提取表面。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class MarchingCubes

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 从标量场提取等值面。
        ''' </summary>
        ''' <param name="field">标量场 <c>field(i)(j)(k)</c>（至少 2×2×2）</param>
        ''' <param name="origin">场原点（<c>i=j=k=0</c> 处的世界坐标）</param>
        ''' <param name="spacing">三个方向的采样间隔</param>
        ''' <param name="isovalue">等值面阈值</param>
        ''' <returns>网格（法向自动重算）</returns>
        Public Shared Function Extract(field As Double()()(), origin As Vector3d, spacing As Vector3d,
                                       isovalue As Double) As MeshData
            If field Is Nothing Then Throw New ArgumentNullException(NameOf(field))

            Dim nx As Integer = field.Length

            If nx < 2 Then Throw New ArgumentException("the field should have at least 2 samples along each axis!", NameOf(field))

            Dim ny As Integer = field(0).Length
            Dim nz As Integer = If(ny > 0, field(0)(0).Length, 0)

            If ny < 2 OrElse nz < 2 Then
                Throw New ArgumentException("the field should have at least 2 samples along each axis!", NameOf(field))
            End If

            Dim vertices As New List(Of Vector3d)()
            Dim triangles As New List(Of Integer)()
            Dim lookup As New Dictionary(Of Long, Integer)()

            Dim cubeCorners As Integer()() = {
                New Integer() {0, 0, 0}, New Integer() {1, 0, 0},
                New Integer() {0, 1, 0}, New Integer() {1, 1, 0},
                New Integer() {0, 0, 1}, New Integer() {1, 0, 1},
                New Integer() {0, 1, 1}, New Integer() {1, 1, 1}
            }

            ' Kuhn（Freudenthal）分解：以<b>体对角线</b> 0-7 为公共边，
            ' 六个四面体对应“坐标分量加入顺序”的 6 种排列。
            ' 必须是体对角线——如果用面对角线（例如 0-6）分解，
            ' 相邻立方体在共享面上的三角化方式不一致，会在网格里留下裂缝（不水密）。
            Dim tetrahedra As Integer()() = {
                New Integer() {0, 1, 3, 7}, New Integer() {0, 1, 5, 7},
                New Integer() {0, 2, 3, 7}, New Integer() {0, 2, 6, 7},
                New Integer() {0, 4, 5, 7}, New Integer() {0, 4, 6, 7}
            }

            Dim cornerIndex As Integer() = New Integer(7) {}
            Dim cornerPosition As Vector3d() = New Vector3d(7) {}
            Dim cornerValue As Double() = New Double(7) {}

            For i As Integer = 0 To nx - 2
                For j As Integer = 0 To ny - 2
                    For k As Integer = 0 To nz - 2
                        For c As Integer = 0 To 7
                            Dim ci As Integer = i + cubeCorners(c)(0)
                            Dim cj As Integer = j + cubeCorners(c)(1)
                            Dim ck As Integer = k + cubeCorners(c)(2)

                            cornerIndex(c) = Pack(ci, cj, ck, ny, nz)
                            cornerPosition(c) = New Vector3d(origin.X + ci * spacing.X,
                                                             origin.Y + cj * spacing.Y,
                                                             origin.Z + ck * spacing.Z)
                            cornerValue(c) = field(ci)(cj)(ck)
                        Next

                        For Each tet As Integer() In tetrahedra
                            Call ExtractTetrahedron(tet, cornerIndex, cornerPosition, cornerValue,
                                                    isovalue, vertices, triangles, lookup)
                        Next
                    Next
                Next
            Next

            Dim mesh As New MeshData(vertices.ToArray(), triangles.ToArray())

            Call mesh.RecomputeNormals()

            Return mesh
        End Function

        Private Shared Function Pack(i As Integer, j As Integer, k As Integer, ny As Integer, nz As Integer) As Long
            Return (CLng(i) * ny + j) * nz + k
        End Function

        ''' <summary>
        ''' 单个四面体的等值面片。
        ''' </summary>
        Private Shared Sub ExtractTetrahedron(tet As Integer(), indices As Integer(), positions As Vector3d(),
                                              values As Double(), isovalue As Double,
                                              vertices As List(Of Vector3d), triangles As List(Of Integer),
                                              lookup As Dictionary(Of Long, Integer))
            Dim inside As Integer = 0
            Dim mask As Integer = 0

            For c As Integer = 0 To 3
                If values(tet(c)) < isovalue Then
                    mask = mask Or (1 << c)
                    inside += 1
                End If
            Next

            If inside = 0 OrElse inside = 4 Then Return

            ' 参考方向：从“内部区域质心”指向“外部区域质心”。
            ' 等值面的法向应当沿这个方向，用它来统一环绕方向（比按 mask 猜要可靠）。
            Dim insideSum As Vector3d = Vector3d.Zero
            Dim outsideSum As Vector3d = Vector3d.Zero
            Dim insideCount As Integer = 0
            Dim outsideCount As Integer = 0

            For c As Integer = 0 To 3
                If values(tet(c)) < isovalue Then
                    insideSum += positions(tet(c))
                    insideCount += 1
                Else
                    outsideSum += positions(tet(c))
                    outsideCount += 1
                End If
            Next

            Dim reference As Vector3d = (outsideSum / outsideCount) - (insideSum / insideCount)

            Dim edges As Integer()()

            Select Case inside
                Case 1
                    Dim v As Integer = IndexOfInside(tet, values, isovalue)

                    edges = {
                        New Integer() {v, (v + 1) Mod 4},
                        New Integer() {v, (v + 2) Mod 4},
                        New Integer() {v, (v + 3) Mod 4}
                    }

                    Call EmitTriangle(edges, tet, indices, positions, values, isovalue, vertices, triangles, lookup, reference)
                Case 3
                    ' 一个外部点：等价于“1 个内部点”的翻转版本
                    Dim v As Integer = IndexOfOutside(tet, values, isovalue)

                    edges = {
                        New Integer() {v, (v + 1) Mod 4},
                        New Integer() {v, (v + 2) Mod 4},
                        New Integer() {v, (v + 3) Mod 4}
                    }

                    Call EmitTriangle(edges, tet, indices, positions, values, isovalue, vertices, triangles, lookup, reference)
                Case 2
                    ' 两个内部点：交线围成四边形
                    Dim a As Integer = -1
                    Dim b As Integer = -1

                    For c As Integer = 0 To 3
                        If values(tet(c)) < isovalue Then
                            If a < 0 Then a = c Else b = c
                        End If
                    Next

                    Dim others As New List(Of Integer)(2)

                    For c As Integer = 0 To 3
                        If c <> a AndAlso c <> b Then Call others.Add(c)
                    Next

                    Dim o0 As Integer = others(0)
                    Dim o1 As Integer = others(1)

                    Dim quad As Integer()() = {
                        New Integer() {a, o0}, New Integer() {b, o0},
                        New Integer() {b, o1}, New Integer() {a, o1}
                    }

                    Call EmitQuad(quad, tet, indices, positions, values, isovalue, vertices, triangles, lookup, reference)
            End Select
        End Sub

        Private Shared Function IndexOfInside(tet As Integer(), values As Double(), isovalue As Double) As Integer
            For c As Integer = 0 To 3
                If values(tet(c)) < isovalue Then Return c
            Next

            Return -1
        End Function

        Private Shared Function IndexOfOutside(tet As Integer(), values As Double(), isovalue As Double) As Integer
            For c As Integer = 0 To 3
                If values(tet(c)) >= isovalue Then Return c
            Next

            Return -1
        End Function

        Private Shared Sub EmitTriangle(edges As Integer()(), tet As Integer(), indices As Integer(),
                                        positions As Vector3d(), values As Double(), isovalue As Double,
                                        vertices As List(Of Vector3d), triangles As List(Of Integer),
                                        lookup As Dictionary(Of Long, Integer), reference As Vector3d)
            Dim a As Integer = Intersect(edges(0), tet, indices, positions, values, isovalue, vertices, lookup)
            Dim b As Integer = Intersect(edges(1), tet, indices, positions, values, isovalue, vertices, lookup)
            Dim c As Integer = Intersect(edges(2), tet, indices, positions, values, isovalue, vertices, lookup)

            ' 等值面正好穿过栅格点时，交点会退化到同一个顶点；
            ' 这种零面积三角形必须丢弃，否则会在网格里留下“只被一个三角形使用的边”，
            ' 使网格不再水密（IsClosed 判定失败）。
            If a = b OrElse b = c OrElse a = c Then Return

            Dim pa As Vector3d = vertices(a)
            Dim pb As Vector3d = vertices(b)
            Dim pc As Vector3d = vertices(c)
            Dim normal As Vector3d = Vector3d.Cross(pb - pa, pc - pa)

            If Vector3d.Dot(normal, reference) >= 0.0 Then
                Call triangles.Add(a)
                Call triangles.Add(b)
                Call triangles.Add(c)
            Else
                Call triangles.Add(a)
                Call triangles.Add(c)
                Call triangles.Add(b)
            End If
        End Sub

        Private Shared Sub EmitQuad(quad As Integer()(), tet As Integer(), indices As Integer(),
                                    positions As Vector3d(), values As Double(), isovalue As Double,
                                    vertices As List(Of Vector3d), triangles As List(Of Integer),
                                    lookup As Dictionary(Of Long, Integer), reference As Vector3d)
            Dim v0 As Integer = Intersect(quad(0), tet, indices, positions, values, isovalue, vertices, lookup)
            Dim v1 As Integer = Intersect(quad(1), tet, indices, positions, values, isovalue, vertices, lookup)
            Dim v2 As Integer = Intersect(quad(2), tet, indices, positions, values, isovalue, vertices, lookup)
            Dim v3 As Integer = Intersect(quad(3), tet, indices, positions, values, isovalue, vertices, lookup)

            Dim p0 As Vector3d = vertices(v0)
            Dim p1 As Vector3d = vertices(v1)
            Dim p2 As Vector3d = vertices(v2)
            Dim normal As Vector3d = Vector3d.Cross(p1 - p0, p2 - p0)
            Dim flip As Boolean = Vector3d.Dot(normal, reference) < 0.0

            ' 每个三角形单独检查退化（四边形在等值面穿过栅格点时会塌缩成三角形）
            Call EmitIfValid(triangles, v0, If(flip, v2, v1), If(flip, v1, v2))
            Call EmitIfValid(triangles, v0, If(flip, v3, v2), If(flip, v2, v3))
        End Sub

        Private Shared Sub EmitIfValid(triangles As List(Of Integer), a As Integer, b As Integer, c As Integer)
            If a = b OrElse b = c OrElse a = c Then Return

            Call triangles.Add(a)
            Call triangles.Add(b)
            Call triangles.Add(c)
        End Sub

        ''' <summary>
        ''' 求边与等值面的交点（带顶点去重）。
        ''' </summary>
        Private Shared Function Intersect(edge As Integer(), tet As Integer(), indices As Integer(),
                                          positions As Vector3d(), values As Double(), isovalue As Double,
                                          vertices As List(Of Vector3d), lookup As Dictionary(Of Long, Integer)) As Integer
            Dim ga As Integer = indices(tet(edge(0)))
            Dim gb As Integer = indices(tet(edge(1)))
            ' 注意必须先把操作数提升到 Long：栅格索引最大可达 nx·ny·nz（数十万），
            ' 直接用 Integer 相乘会静默溢出（本项目开启了整数溢出检查，会直接抛异常）。
            Dim key As Long = (CLng(std.Min(ga, gb)) << 32) Or CLng(std.Max(ga, gb))
            Dim existing As Integer

            If lookup.TryGetValue(key, existing) Then Return existing

            Dim va As Double = values(tet(edge(0)))
            Dim vb As Double = values(tet(edge(1)))
            Dim denominator As Double = vb - va
            Dim t As Double = If(std.Abs(denominator) > 1.0E-15, (isovalue - va) / denominator, 0.5)

            t = std.Max(0.0, std.Min(1.0, t))

            Dim pa As Vector3d = positions(tet(edge(0)))
            Dim pb As Vector3d = positions(tet(edge(1)))
            Dim point As Vector3d = pa + (pb - pa) * t
            Dim index As Integer = vertices.Count

            Call vertices.Add(point)
            lookup(key) = index

            Return index
        End Function

        ''' <summary>
        ''' 从深度图反投影生成标量场的辅助：把深度值线性缩放为“到相机的距离”。
        ''' </summary>
        Public Shared Function DepthToField(depth As Double()(), background As Double) As Double()()
            If depth Is Nothing Then Throw New ArgumentNullException(NameOf(depth))

            Dim h As Integer = depth.Length
            Dim w As Integer = If(h > 0, depth(0).Length, 0)
            Dim field As Double()() = New Double(h - 1)() {}

            For i As Integer = 0 To h - 1
                field(i) = New Double(std.Max(0, w) - 1) {}

                For j As Integer = 0 To w - 1
                    field(i)(j) = If(Double.IsNaN(depth(i)(j)) OrElse depth(i)(j) <= 0.0, background, depth(i)(j))
                Next
            Next

            Return field
        End Function

    End Class
End Namespace
