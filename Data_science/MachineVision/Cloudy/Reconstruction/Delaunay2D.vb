Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Features
Imports std = System.Math

Namespace Reconstruction

    ''' <summary>
    ''' 二维 Delaunay 三角化（Bowyer–Watson）与 2.5 维曲面重建。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>算法</b>：Bowyer–Watson 增量构造。每插入一个点，找出所有
    ''' “外接圆包含该点”的三角形（bad triangles），删掉它们后形成的
    ''' <b>空腔边界多边形</b> 与新点重新连三角形。整个过程只需维护一个三角形列表，
    ''' 不需要半边结构，代码量小且便于并行化前的优化。
    ''' </para>
    ''' <para>
    ''' <b>复杂度</b>：平均 <c>O(n log n)</c>，最坏 <c>O(n²)</c>（点共圆/共线的退化输入）。
    ''' 这里在插入前对点做了随机化打乱（固定种子），把最坏情况压到期望复杂度，
    ''' 同时保证结果可复现。
    ''' </para>
    ''' <para>
    ''' <b>2.5 维重建</b>：真实扫描的表面在局部近似为“高度场”，因此把点云投影到
    ''' 最小二乘平面上做二维三角化，再把三角形索引映射回原始三维顶点，
    ''' 就得到保形网格（<see cref="Triangulate25D"/>）。它适合地形、板材、
    ''' 变形检测这类“单值曲面”，<b>不适合</b>封闭物体（球/圆柱需要泊松或 BPA）。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class Delaunay2D

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 对二维点集做 Delaunay 三角化。
        ''' </summary>
        ''' <param name="xs">X 坐标</param>
        ''' <param name="ys">Y 坐标</param>
        ''' <param name="seed">打乱顺序用的随机种子（保证可复现）</param>
        ''' <returns>三角形索引三元组列表（索引进制与输入一致）</returns>
        Public Shared Function Triangulate(xs As Double(), ys As Double(), Optional seed As Integer = 20240101) As Integer()()
            If xs Is Nothing OrElse ys Is Nothing Then Throw New ArgumentNullException(NameOf(xs))
            If xs.Length <> ys.Length Then
                Throw New ArgumentException($"the coordinate arrays should have the same length: {xs.Length} vs {ys.Length}!")
            End If

            Dim n As Integer = xs.Length

            If n < 3 Then Return Array.Empty(Of Integer())()

            ' 包围盒与超级三角形
            Dim minX As Double = xs(0), maxX As Double = xs(0)
            Dim minY As Double = ys(0), maxY As Double = ys(0)

            For i As Integer = 1 To n - 1
                minX = std.Min(minX, xs(i))
                maxX = std.Max(maxX, xs(i))
                minY = std.Min(minY, ys(i))
                maxY = std.Max(maxY, ys(i))
            Next

            Dim dx As Double = std.Max(maxX - minX, 1.0E-9)
            Dim dy As Double = std.Max(maxY - minY, 1.0E-9)
            Dim dmax As Double = std.Max(dx, dy) * 20.0
            Dim midX As Double = (minX + maxX) * 0.5
            Dim midY As Double = (minY + maxY) * 0.5

            ' 工作点集（最后三个是超级三角形的顶点）
            Dim px As Double() = New Double(n + 2) {}
            Dim py As Double() = New Double(n + 2) {}

            Array.Copy(xs, px, n)
            Array.Copy(ys, py, n)

            px(n) = midX - dmax
            py(n) = midY - dmax
            px(n + 1) = midX + dmax
            py(n + 1) = midY - dmax
            px(n + 2) = midX
            py(n + 2) = midY + dmax

            ' 插入顺序随机化（退化输入下避免 O(n²)）
            Dim order As Integer() = New Integer(n - 1) {}
            Dim rnd As New Random(seed)

            For i As Integer = 0 To n - 1
                order(i) = i
            Next

            For i As Integer = n - 1 To 1 Step -1
                Dim j As Integer = rnd.Next(i + 1)
                Dim swap As Integer = order(i)

                order(i) = order(j)
                order(j) = swap
            Next

            Dim triangles As New List(Of Integer())(2 * n)
            Call triangles.Add({n, n + 1, n + 2})

            Dim bad As New List(Of Integer)()
            Dim polygon As New List(Of Integer())()

            For o As Integer = 0 To n - 1
                Dim p As Integer = order(o)

                Call bad.Clear()

                For t As Integer = 0 To triangles.Count - 1
                    If InCircumcircle(px, py, triangles(t), px(p), py(p)) Then Call bad.Add(t)
                Next

                Call polygon.Clear()

                For Each index As Integer In bad
                    Dim tri As Integer() = triangles(index)

                    For e As Integer = 0 To 2
                        Dim a As Integer = tri(e)
                        Dim b As Integer = tri((e + 1) Mod 3)
                        Dim isShared As Boolean = False

                        For Each otherIndex As Integer In bad
                            If otherIndex = index Then Continue For

                            Dim other As Integer() = triangles(otherIndex)

                            If HasDirectedEdge(other, b, a) Then
                                isShared = True

                                Exit For
                            End If
                        Next

                        If Not isShared Then Call polygon.Add({a, b})
                    Next
                Next

                ' 删除 bad（从后往前，避免下标错位）
                Call bad.Sort()
                Call bad.Reverse()

                For Each index As Integer In bad
                    Call triangles.RemoveAt(index)
                Next

                For Each edge As Integer() In polygon
                    Call triangles.Add({edge(0), edge(1), p})
                Next
            Next

            ' 去掉包含超级顶点的三角形
            Dim result As New List(Of Integer())(triangles.Count)

            For Each tri As Integer() In triangles
                If tri(0) >= n OrElse tri(1) >= n OrElse tri(2) >= n Then Continue For

                Call result.Add(tri)
            Next

            Return result.ToArray()
        End Function

        Private Shared Function HasDirectedEdge(tri As Integer(), a As Integer, b As Integer) As Boolean
            For e As Integer = 0 To 2
                If tri(e) = a AndAlso tri((e + 1) Mod 3) = b Then Return True
            Next

            Return False
        End Function

        ''' <summary>
        ''' 外接圆测试：点 <c>(px,py)</c> 是否严格落在三角形外接圆内。
        ''' </summary>
        ''' <remarks>
        ''' 用 3x3 行列式的符号判定（InCircle 谓词），全程 <c>O(1)</c>。
        ''' 这里的数值实现没有做精确谓词（Shewchuk）处理，
        ''' 因此极端退化输入（四点共圆）可能出现拓扑抖动；
        ''' 对扫描数据而言共圆是零概率事件，工程上可以接受。
        ''' </remarks>
        Private Shared Function InCircumcircle(xs As Double(), ys As Double(), tri As Integer(),
                                               px As Double, py As Double) As Boolean
            Dim ax As Double = xs(tri(0)) - px
            Dim ay As Double = ys(tri(0)) - py
            Dim bx As Double = xs(tri(1)) - px
            Dim by As Double = ys(tri(1)) - py
            Dim cx As Double = xs(tri(2)) - px
            Dim cy As Double = ys(tri(2)) - py

            Dim det As Double =
                (ax * ax + ay * ay) * (bx * cy - cx * by) -
                (bx * bx + by * by) * (ax * cy - cx * ay) +
                (cx * cx + cy * cy) * (ax * by - bx * ay)

            ' 三角形的定向决定符号：统一按 CCW 处理
            Dim orientation As Double = (xs(tri(1)) - xs(tri(0))) * (ys(tri(2)) - ys(tri(0))) -
                (xs(tri(2)) - xs(tri(0))) * (ys(tri(1)) - ys(tri(0)))

            If orientation < 0.0 Then det = -det

            Return det > 0.0
        End Function

        ''' <summary>
        ''' 2.5 维曲面重建：投影到最小二乘平面后做二维三角化。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="options">并行选项</param>
        Public Shared Function Triangulate25D(cloud As PointCloudBuffer,
                                              Optional options As CloudyOptions = Nothing) As MeshData
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim global_ As CloudyOptions = If(options, CloudyOptions.Default)
            Dim n As Integer = cloud.Count

            If n < 3 Then Return New MeshData(cloud.ToVector3d(), Array.Empty(Of Integer)())

            ' 用协方差主轴作为投影平面（比固定 Z 投影更稳，能处理倾斜扫描）
            Dim cx As Double = 0, cy As Double = 0, cz As Double = 0

            For i As Integer = 0 To n - 1
                cx += cloud.X(i)
                cy += cloud.Y(i)
                cz += cloud.Z(i)
            Next

            cx /= n
            cy /= n
            cz /= n

            Dim cxx As Double = 0, cxy As Double = 0, cxz As Double = 0
            Dim cyy As Double = 0, cyz As Double = 0, czz As Double = 0

            For i As Integer = 0 To n - 1
                Dim dx As Double = cloud.X(i) - cx
                Dim dy As Double = cloud.Y(i) - cy
                Dim dz As Double = cloud.Z(i) - cz

                cxx += dx * dx
                cxy += dx * dy
                cxz += dx * dz
                cyy += dy * dy
                cyz += dy * dz
                czz += dz * dz
            Next

            Dim eigen As Linalg.Eigen3 = Linalg.EigenSymmetric3(cxx, cxy, cxz, cyy, cyz, czz)
            Dim u As Vector3d = eigen.V1
            Dim v As Vector3d = eigen.V2

            Dim xs(n - 1) As Double
            Dim ys(n - 1) As Double

            For i As Integer = 0 To n - 1
                xs(i) = Vector3d.Dot(cloud.Item(i), u)
                ys(i) = Vector3d.Dot(cloud.Item(i), v)
            Next

            Dim triangles As Integer()() = Triangulate(xs, ys)
            Dim flat As Integer() = New Integer(triangles.Length * 3 - 1) {}

            For i As Integer = 0 To triangles.Length - 1
                flat(i * 3) = triangles(i)(0)
                flat(i * 3 + 1) = triangles(i)(1)
                flat(i * 3 + 2) = triangles(i)(2)
            Next

            Return New MeshData(cloud.ToVector3d(), flat)
        End Function

        ''' <summary>
        ''' 把三角形索引（指向二维点）转换为三维网格。
        ''' </summary>
        Public Shared Function ToMesh(cloud As PointCloudBuffer, triangles As Integer()()) As MeshData
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If triangles Is Nothing Then Throw New ArgumentNullException(NameOf(triangles))

            Dim flat As Integer() = New Integer(triangles.Length * 3 - 1) {}

            For i As Integer = 0 To triangles.Length - 1
                flat(i * 3) = triangles(i)(0)
                flat(i * 3 + 1) = triangles(i)(1)
                flat(i * 3 + 2) = triangles(i)(2)
            Next

            Dim mesh As New MeshData(cloud.ToVector3d(), flat)

            If cloud.HasNormals Then
                Dim normals(cloud.Count - 1) As Vector3d

                For i As Integer = 0 To cloud.Count - 1
                    normals(i) = cloud.Normal(i)
                Next

                Call mesh.SetNormals(normals)
            End If

            Return mesh
        End Function

    End Class
End Namespace
