Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports std = System.Math

Namespace Drawing3D.Math3D.Delaunay3D

    ''' <summary>
    ''' 四面体的四个顶点索引（指向输入点集的下标）。
    ''' </summary>
    Public Structure TetraIndices

        Public A As Integer
        Public B As Integer
        Public C As Integer
        Public D As Integer

        Sub New(a As Integer, b As Integer, c As Integer, d As Integer)
            Me.A = a
            Me.B = b
            Me.C = c
            Me.D = d
        End Sub

        Public Overrides Function ToString() As String
            Return $"#{A}, #{B}, #{C}, #{D}"
        End Function
    End Structure

    ''' <summary>
    ''' 三维 Delaunay 四面体化：Bowyer-Watson 逐点插入算法。
    ''' 
    ''' 实现要点：
    ''' 1. 输入坐标内部归一化到单位盒，保证 InSphere 行列式判定的数值稳定性；
    ''' 2. 以一个巨大的正定向超四面体（super-tetrahedron）覆盖全部输入点作为初始网格；
    ''' 3. 逐点插入：从最近邻已插入点的关联四面体出发（点定位，借助
    '''    <see cref="Octree(Of T)"/> 最近邻搜索做跳走加速），BFS 收集外接球包含
    '''    新点的"坏四面体"空腔，删除空腔并以新点与空腔边界面重构四面体；
    ''' 4. 面邻接通过"有序面键 → 四面体"哈希映射维护，重建空腔时自动完成邻接修补；
    ''' 5. 导出时剔除含超四面体顶点的四面体与零体积退化四面体，并统一为正定向。
    ''' 
    ''' 注意：面键采用 3×21bit 位打包，要求点数（含超顶点）不超过 2^21 - 1。
    ''' </summary>
    Public Class DelaunayTriangulation3D

        Const FaceBits As Integer = 21
        Const FaceMask As Long = &H1FFFFF

        ' 空腔收集的 InSphere 相对容差（坐标已归一化到单位盒）
        Const InSphereEps As Double = 0.000000000001
        ' 重复点判定阈值（归一化坐标空间）
        Const DuplicateEps As Double = 0.000000001
        ' 导出时体积退化阈值（6 倍有向体积，归一化空间）
        Const VolumeEps As Double = 0.000000000000001

        Dim _points As List(Of Point3D)
        Dim _wx As Double(), _wy As Double(), _wz As Double()
        Dim _tets As List(Of Tetra)
        Dim _faceMap As Dictionary(Of Long, Integer)
        Dim _pointTets As List(Of List(Of Integer))
        Dim _octree As Octree(Of Integer)
        Dim _skipped As Integer
        Dim _seed As Integer
        Dim _shuffle As Boolean

        Private Structure Tetra
            Dim V As Integer()
            Dim N As Integer()
            Dim Alive As Boolean
        End Structure

        Private Sub New(points As List(Of Point3D), seed As Integer, shuffle As Boolean)
            _points = points
            _seed = seed
            _shuffle = shuffle
        End Sub

        ''' <summary>因重复点或退化而跳过的输入点数。</summary>
        Public ReadOnly Property SkippedPoints As Integer
            Get
                Return _skipped
            End Get
        End Property

        ''' <summary>调试用：最近一次运行的状态统计。</summary>
        Public Shared LastRunSkipped As Integer
        Public Shared LastRunAliveTets As Integer
        Public Shared LastRunTotalTets As Integer

        ''' <summary>
        ''' 对输入点集做三维 Delaunay 四面体化。
        ''' </summary>
        ''' <param name="points">输入点集（至少 4 个不共面的点）。</param>
        ''' <param name="seed">插入顺序随机化种子（保证结果可重现）。</param>
        ''' <param name="shuffle">是否随机化插入顺序（推荐开启，避免最坏复杂度）。</param>
        Public Shared Function Triangulate(points As List(Of Point3D),
                                           Optional seed As Integer = 42,
                                           Optional shuffle As Boolean = True) As List(Of TetraIndices)

            Dim triangulation As New DelaunayTriangulation3D(points, seed, shuffle)
            Call triangulation.Run()

            LastRunSkipped = triangulation._skipped
            LastRunTotalTets = triangulation._tets.Count
            LastRunAliveTets = 0

            For Each t As Tetra In triangulation._tets
                If t.Alive Then
                    LastRunAliveTets += 1
                End If
            Next

            Return triangulation.Export()
        End Function

        ''' <summary>
        ''' 提取四面体集合的边界面（仅被一个四面体使用的面），
        ''' 顶点序统一为外法向（背离四面体内部）。
        ''' </summary>
        Public Shared Function BoundaryTriangles(points As List(Of Point3D), tetra As List(Of TetraIndices)) As List(Of Integer())
            Dim faceCount As New Dictionary(Of Long, List(Of Integer()))
            Dim span As Double = PointSpan(points)

            For Each t As TetraIndices In tetra
                Dim v As Integer() = {t.A, t.B, t.C, t.D}

                If Not IsOriented(points, t) Then
                    v = New Integer() {t.A, t.B, t.D, t.C}
                End If

                For slot As Integer = 0 To 3
                    Dim a As Integer = v((slot + 1) Mod 4)
                    Dim b As Integer = v((slot + 2) Mod 4)
                    Dim c As Integer = v((slot + 3) Mod 4)
                    Dim key As Long = PackFace(a, b, c)

                    If Not faceCount.ContainsKey(key) Then
                        faceCount(key) = New List(Of Integer())
                    End If

                    faceCount(key).Add({a, b, c})
                Next
            Next

            Dim result As New List(Of Integer())

            For Each faces In faceCount.Values
                If faces.Count = 1 Then
                    result.Add(faces(0))
                End If
            Next

            Return result
        End Function

        Private Shared Function IsOriented(points As List(Of Point3D), t As TetraIndices) As Boolean
            Dim a As Point3D = points(t.A)
            Dim b As Point3D = points(t.B)
            Dim c As Point3D = points(t.C)
            Dim d As Point3D = points(t.D)
            Return New Tetrahedron(a, b, c, d).SignedVolume6 > 0
        End Function

        Private Shared Function PointSpan(points As List(Of Point3D)) As Double
            If points.Count = 0 Then
                Return 1
            End If

            Dim minX As Double = points(0).X, maxX As Double = minX
            Dim minY As Double = points(0).Y, maxY As Double = minY
            Dim minZ As Double = points(0).Z, maxZ As Double = minZ

            For Each p As Point3D In points
                minX = std.Min(minX, p.X) : maxX = std.Max(maxX, p.X)
                minY = std.Min(minY, p.Y) : maxY = std.Max(maxY, p.Y)
                minZ = std.Min(minZ, p.Z) : maxZ = std.Max(maxZ, p.Z)
            Next

            Return std.Max(std.Max(maxX - minX, maxY - minY), std.Max(maxZ - minZ, 1.0E-30))
        End Function

        ' ---------------------------------------------------------------------
        ' 主流程
        ' ---------------------------------------------------------------------

        Private Sub Run()
            LastDebugLog = New List(Of String)
            Dim n As Integer = _points.Count

            If n < 4 Then
                _tets = New List(Of Tetra)
                _faceMap = New Dictionary(Of Long, Integer)
                _pointTets = New List(Of List(Of Integer))
                _octree = New Octree(Of Integer)(New BoundingBox3D(-1, -1, -1, 1, 1, 1))
                Return
            End If

            If n + 4 > FaceMask Then
                Throw New NotSupportedException(
                    $"DelaunayTriangulation3D supports up to {FaceMask} points, but got {n}.")
            End If

            Call normalizePoints()

            ' 超四面体：以归一化点云为中心的正四面体，外接半径取包围球半径的 3 倍以上，
            ' 保证全部输入点位于其内部
            Dim r As Double = 0.75 * std.Sqrt(3.0)   ' 点云包围球半径上界（坐标 ∈ [-0.5, 0.5]）
            Dim bigR As Double = 3.2 * r
            Dim f As Double = bigR / std.Sqrt(3.0)
            Dim n4 As Integer = n

            ReDim Preserve _wx(n4 + 3)
            ReDim Preserve _wy(n4 + 3)
            ReDim Preserve _wz(n4 + 3)
            _wx(n4) = f : _wy(n4) = f : _wz(n4) = f
            _wx(n4 + 1) = f : _wy(n4 + 1) = -f : _wz(n4 + 1) = -f
            _wx(n4 + 2) = -f : _wy(n4 + 2) = f : _wz(n4 + 2) = -f
            _wx(n4 + 3) = -f : _wy(n4 + 3) = -f : _wz(n4 + 3) = f

            _tets = New List(Of Tetra)
            _faceMap = New Dictionary(Of Long, Integer)
            _pointTets = New List(Of List(Of Integer))

            For i As Integer = 0 To n4 + 3
                _pointTets.Add(New List(Of Integer))
            Next

            ' 根四面体（保证正定向）
            Dim root As New Tetra With {
                .V = {n4, n4 + 1, n4 + 2, n4 + 3},
                .N = {-1, -1, -1, -1},
                .Alive = True}

            If signedVolume6(root.V) < 0 Then
                Call SwapVal(root.V(2), root.V(3))
            End If

            _tets.Add(root)

            For slot As Integer = 0 To 3
                _faceMap(FaceKeyOf(0, slot)) = 0
            Next

            ' 点定位用的最近邻八叉树（覆盖超四面体范围）
            Dim bounds As New BoundingBox3D(-bigR, -bigR, -bigR, bigR, bigR, bigR)
            _octree = New Octree(Of Integer)(bounds, capacity:=16, maxDepth:=24)

            ' 随机化插入顺序（Fisher-Yates，固定种子保证可重现）
            Dim order As Integer() = New Integer(n - 1) {}

            For i As Integer = 0 To n - 1
                order(i) = i
            Next

            If _shuffle Then
                Dim rng As New Random(_seed)

                For i As Integer = n - 1 To 1 Step -1
                    Dim j As Integer = rng.Next(i + 1)
                    Call SwapVal(order(i), order(j))
                Next
            End If

            ' 逐点插入
            For Each q As Integer In order
                Call insertPoint(q)

                If DebugValidateEachStep Then
                    Dim errs As List(Of String) = ValidateAdjacency()

                    For Each e As String In errs
                        LastDebugLog.Add($"[ADJ-ERR] after inserting #{q}: {e}")
                    Next
                End If
            Next
        End Sub

        ''' <summary>调试开关：每步插入后校验邻接一致性。</summary>
        Public Shared DebugValidateEachStep As Boolean = False

        ''' <summary>调试：插入历史日志（空腔/删除/创建的四面体索引）。</summary>
        Public Shared LastDebugLog As New List(Of String)

        ''' <summary>调试：插入历史日志（空腔/删除/创建的四面体索引）。</summary>
        Public ReadOnly DebugLog As New List(Of String)

        ''' <summary>调试：校验全部存活四面体的面邻接一致性（每条内部面恰好两侧一致）。</summary>
        Public Function ValidateAdjacency() As List(Of String)
            Dim errors As New List(Of String)

            For i As Integer = 0 To _tets.Count - 1
                If Not _tets(i).Alive Then
                    Continue For
                End If

                For slot As Integer = 0 To 3
                    Dim key As Long = FaceKeyOf(i, slot)
                    Dim nb As Integer = _tets(i).N(slot)

                    If nb < 0 Then
                        ' 全局边界面：面键应无人持有（或持有者就是自己）
                        If _faceMap.ContainsKey(key) AndAlso _faceMap(key) <> i Then
                            errors.Add($"tet#{i}.N({slot})=-1 but face held by tet#{_faceMap(key)}")
                        End If
                    Else
                        If Not _tets(nb).Alive Then
                            errors.Add($"tet#{i}.N({slot}) -> dead tet#{nb}")
                        ElseIf FaceKeyOf(nb, matchingSlot(nb, key)) <> key Then
                            errors.Add($"tet#{i}.N({slot}) -> tet#{nb} face mismatch")
                        End If
                    End If
                Next
            Next

            Return errors
        End Function

        Private Function matchingSlot(tet As Integer, key As Long) As Integer
            For slot As Integer = 0 To 3
                If FaceKeyOf(tet, slot) = key Then
                    Return slot
                End If
            Next

            Return -1
        End Function

        Private Sub normalizePoints()
            Dim n As Integer = _points.Count
            Dim minX As Double = _points(0).X, maxX As Double = minX
            Dim minY As Double = _points(0).Y, maxY As Double = minY
            Dim minZ As Double = _points(0).Z, maxZ As Double = minZ

            For Each p As Point3D In _points
                minX = std.Min(minX, p.X) : maxX = std.Max(maxX, p.X)
                minY = std.Min(minY, p.Y) : maxY = std.Max(maxY, p.Y)
                minZ = std.Min(minZ, p.Z) : maxZ = std.Max(maxZ, p.Z)
            Next

            Dim cx As Double = (minX + maxX) * 0.5
            Dim cy As Double = (minY + maxY) * 0.5
            Dim cz As Double = (minZ + maxZ) * 0.5
            Dim span As Double = std.Max(std.Max(maxX - minX, maxY - minY), std.Max(maxZ - minZ, 1.0E-30))
            Dim scale As Double = 1.0 / span

            ReDim _wx(n - 1) : ReDim _wy(n - 1) : ReDim _wz(n - 1)

            ' 确定性微抖动：破坏共球/共面退化（如结构网格点），幅度远小于网格尺度，
            ' 且仅参与三角化判定，导出仍使用原始坐标
            Const JitterAmp As Double = 0.0000001
            Dim jitter As New Random(_seed)

            For i As Integer = 0 To n - 1
                _wx(i) = (_points(i).X - cx) * scale + (jitter.NextDouble - 0.5) * 2 * JitterAmp
                _wy(i) = (_points(i).Y - cy) * scale + (jitter.NextDouble - 0.5) * 2 * JitterAmp
                _wz(i) = (_points(i).Z - cz) * scale + (jitter.NextDouble - 0.5) * 2 * JitterAmp
            Next
        End Sub

        Private Sub insertPoint(q As Integer)
            Dim qx As Double = _wx(q)
            Dim qy As Double = _wy(q)
            Dim qz As Double = _wz(q)

            ' --- 点定位：从最近邻点的关联四面体出发 ---
            Dim seeds As New List(Of Integer)
            Dim nearestDist As Double = 0
            Dim nearest As OctreePoint(Of Integer) = _octree.Nearest(qx, qy, qz, nearestDist)

            If nearestDist <> Double.PositiveInfinity Then
                If nearestDist < DuplicateEps Then
                    ' 重复点：跳过
                    _skipped += 1
                    Return
                End If

                For Each t As Integer In _pointTets(nearest.Value)
                    If _tets(t).Alive AndAlso Not seeds.Contains(t) Then
                        seeds.Add(t)
                    End If
                Next
            End If

            ' --- 收集坏四面体空腔 ---
            Dim bad As New List(Of Integer)
            Dim badSet As New HashSet(Of Integer)
            Call collectCavity(qx, qy, qz, seeds, bad, badSet)

            If bad.Count = 0 Then
                ' 跳走起点失效，全量扫描兜底
                Dim all As New List(Of Integer)

                For i As Integer = 0 To _tets.Count - 1
                    all.Add(i)
                Next

                Call collectCavity(qx, qy, qz, all, bad, badSet)
            End If

            If bad.Count = 0 Then
                ' 数值上无法构成空腔（近重复点/退化点），跳过
                _skipped += 1
                Return
            End If

            ' --- 空腔边界面：面邻居为空/已死（全局或暴露边界）或同样属于空腔 ---
            ' 同一个面会被空腔两侧的坏四面体各报告一次，按面键去重
            Dim boundary As New List(Of Integer())
            Dim boundarySeen As New HashSet(Of Long)

            For Each t As Integer In bad
                Dim v As Integer() = _tets(t).V

                For slot As Integer = 0 To 3
                    Dim nb As Integer = _tets(t).N(slot)

                    If nb < 0 OrElse Not _tets(nb).Alive OrElse badSet.Contains(nb) Then
                        Dim key As Long = FaceKeyOf(t, slot)

                        If boundarySeen.Add(key) Then
                            boundary.Add({v((slot + 1) Mod 4), v((slot + 2) Mod 4), v((slot + 3) Mod 4)})
                        End If
                    End If
                Next
            Next

            ' --- 删除坏四面体 ---
            For Each t As Integer In bad
                Call SetAlive(t, False)
            Next

            ' 与空腔相邻的存活四面体（其 N 槽位可能指向被删除的四面体，需要修补）
            Dim toPatch As New HashSet(Of Integer)

            For Each t As Integer In bad
                For slot As Integer = 0 To 3
                    Dim key As Long = FaceKeyOf(t, slot)
                    Dim cur As Integer = -1
                    Dim nb As Integer = _tets(t).N(slot)

                    If nb >= 0 AndAlso _tets(nb).Alive Then
                        toPatch.Add(nb)
                    End If

                    If _faceMap.TryGetValue(key, cur) AndAlso cur = t Then
                        ' 面键当前由本（坏）四面体持有：转交给存活的邻居，
                        ' 或者（邻居同样在空腔内 / 无邻居 / 邻居已死）直接移除，
                        ' 保证面键始终指向存活四面体
                        If nb >= 0 AndAlso _tets(nb).Alive Then
                            _faceMap(key) = nb
                        Else
                            Call _faceMap.Remove(key)
                        End If
                    End If
                Next
            Next

            ' --- 以新点重构空腔 ---
            For i As Integer = 0 To boundary.Count - 1
                Dim face As Integer() = boundary(i)
                Dim tet As New Tetra With {
                    .V = {face(0), face(1), face(2), q},
                    .N = {-1, -1, -1, -1},
                    .Alive = True}

                If signedVolume6(tet.V) < 0 Then
                    Call SwapVal(tet.V(0), tet.V(1))
                End If

                Dim idx As Integer = _tets.Count
                _tets.Add(tet)

                For slot As Integer = 0 To 3
                    Dim key As Long = FaceKeyOf(idx, slot)
                    Dim other As Integer = -1

                    If _faceMap.TryGetValue(key, other) Then
                        Call SetNeighbor(idx, slot, other)

                        ' 修补对面（存活）四面体的邻接槽位
                        Dim patched As Boolean = False

                        For otherSlot As Integer = 0 To 3
                            If FaceKeyOf(other, otherSlot) = key Then
                                Call SetNeighbor(other, otherSlot, idx)
                                patched = True
                                Exit For
                            End If
                        Next

                        If Not patched AndAlso DebugValidateEachStep Then
                            LastDebugLog.Add($"[PATCH-MISS] tet#{idx} slot{slot} key={key} held by tet#{other}(alive={_tets(other).Alive})")
                        End If
                    Else
                        _faceMap(key) = idx
                    End If
                Next

                For slot As Integer = 0 To 3
                    Call _pointTets(tet.V(slot)).Add(idx)
                Next
            Next

            ' --- 修补空腔邻域存活四面体的悬空邻接 ---
            For Each g As Integer In toPatch
                For slot As Integer = 0 To 3
                    Dim nb As Integer = _tets(g).N(slot)

                    If nb >= 0 AndAlso _tets(nb).Alive Then
                        Continue For
                    End If

                    Dim key As Long = FaceKeyOf(g, slot)
                    Dim holder As Integer = -1

                    If _faceMap.TryGetValue(key, holder) AndAlso holder <> g AndAlso _tets(holder).Alive Then
                        Call SetNeighbor(g, slot, holder)
                    Else
                        Call SetNeighbor(g, slot, -1)
                    End If
                Next
            Next

            ' --- 登记新点到八叉树（新四面体与 q 的关联已在重构循环中写入 _pointTets(q)） ---
            Call _octree.Insert(qx, qy, qz, q)

            If DebugValidateEachStep Then
                Dim line As String = $"insert #{q}: bad=[{String.Join(",", bad)}] boundary={boundary.Count} created=[{_tets.Count - boundary.Count}..{_tets.Count - 1}]"
                DebugLog.Add(line)
                LastDebugLog.Add(line)
            End If
        End Sub

        Private Sub collectCavity(qx As Double, qy As Double, qz As Double,
                                  seeds As IEnumerable(Of Integer),
                                  bad As List(Of Integer),
                                  badSet As HashSet(Of Integer))

            Dim stack As New Stack(Of Integer)

            For Each s As Integer In seeds
                If s >= 0 AndAlso s < _tets.Count AndAlso _tets(s).Alive Then
                    stack.Push(s)
                End If
            Next

            While stack.Count > 0
                Dim t As Integer = stack.Pop()

                If badSet.Contains(t) Then
                    Continue While
                End If

                badSet.Add(t)

                If inSphereWork(_tets(t).V, qx, qy, qz) > InSphereEps Then
                    bad.Add(t)

                    For slot As Integer = 0 To 3
                        Dim nb As Integer = _tets(t).N(slot)

                        If nb >= 0 AndAlso _tets(nb).Alive AndAlso Not badSet.Contains(nb) Then
                            stack.Push(nb)
                        End If
                    Next
                End If
            End While
        End Sub

        ' ---------------------------------------------------------------------
        ' 导出
        ' ---------------------------------------------------------------------

        Private Function Export() As List(Of TetraIndices)
            Dim result As New List(Of TetraIndices)
            Dim n As Integer = _points.Count

            For i As Integer = 0 To _tets.Count - 1
                If Not _tets(i).Alive Then
                    Continue For
                End If

                Dim v As Integer() = _tets(i).V
                Dim hasSuper As Boolean = False

                For Each vi As Integer In v
                    If vi >= n Then
                        hasSuper = True
                        Exit For
                    End If
                Next

                If hasSuper Then
                    Continue For
                End If

                Dim vol6 As Double = signedVolume6(v)

                If std.Abs(vol6) < VolumeEps Then
                    Continue For
                End If

                If vol6 < 0 Then
                    result.Add(New TetraIndices(v(0), v(1), v(3), v(2)))
                Else
                    result.Add(New TetraIndices(v(0), v(1), v(2), v(3)))
                End If
            Next

            Return result
        End Function

        ' ---------------------------------------------------------------------
        ' 几何与键值工具
        ' ---------------------------------------------------------------------

        Private Function inSphereWork(v As Integer(), qx As Double, qy As Double, qz As Double) As Double
            Dim ax As Double = _wx(v(0)) - qx, ay As Double = _wy(v(0)) - qy, az As Double = _wz(v(0)) - qz
            Dim bx As Double = _wx(v(1)) - qx, by As Double = _wy(v(1)) - qy, bz As Double = _wz(v(1)) - qz
            Dim cx As Double = _wx(v(2)) - qx, cy As Double = _wy(v(2)) - qy, cz As Double = _wz(v(2)) - qz
            Dim dx As Double = _wx(v(3)) - qx, dy As Double = _wy(v(3)) - qy, dz As Double = _wz(v(3)) - qz
            Dim m As Double(,) = {
                {ax, ay, az, ax * ax + ay * ay + az * az},
                {bx, by, bz, bx * bx + by * by + bz * bz},
                {cx, cy, cz, cx * cx + cy * cy + cz * cz},
                {dx, dy, dz, dx * dx + dy * dy + dz * dz}}
            ' 定向归一化：Shewchuk 约定下 orient3d(a,b,c,d) = -signedVolume6，
            ' 因此取负号使"返回值 > 0 = 点在外接球内"恒成立
            Return -Det4(m) / signedVolume6(v)
        End Function

        Private Function signedVolume6(v As Integer()) As Double
            Dim ax As Double = _wx(v(0)), ay As Double = _wy(v(0)), az As Double = _wz(v(0))
            Dim bx As Double = _wx(v(1)) - ax, by As Double = _wy(v(1)) - ay, bz As Double = _wz(v(1)) - az
            Dim cx As Double = _wx(v(2)) - ax, cy As Double = _wy(v(2)) - ay, cz As Double = _wz(v(2)) - az
            Dim dx As Double = _wx(v(3)) - ax, dy As Double = _wy(v(3)) - ay, dz As Double = _wz(v(3)) - az
            Dim nx As Double = cy * dz - cz * dy
            Dim ny As Double = cz * dx - cx * dz
            Dim nz As Double = cx * dy - cy * dx
            Return bx * nx + by * ny + bz * nz
        End Function

        ''' <summary>四面体第 <paramref name="slot"/> 个顶点所对的面（其余三个顶点）的有序键。</summary>
        Private Function FaceKeyOf(tet As Integer, slot As Integer) As Long
            Dim v As Integer() = _tets(tet).V
            Dim rest As Integer() = {
                v((slot + 1) Mod 4),
                v((slot + 2) Mod 4),
                v((slot + 3) Mod 4)}
            Return PackFace(rest(0), rest(1), rest(2))
        End Function

        ''' <summary>三个顶点索引升序排列后位打包为 Long 键（3 × 21bit）。</summary>
        Friend Shared Function PackFace(ByVal a As Integer, ByVal b As Integer, ByVal c As Integer) As Long
            Dim x As Integer = a, y As Integer = b, z As Integer = c

            If y < x Then
                Call SwapVal(x, y)
            End If

            If z < y Then
                Call SwapVal(y, z)

                If y < x Then
                    Call SwapVal(x, y)
                End If
            End If

            Return CLng(x And FaceMask) Or
                   (CLng(y And FaceMask) << FaceBits) Or
                   (CLng(z And FaceMask) << (2 * FaceBits))
        End Function

        Private Sub SetNeighbor(tet As Integer, slot As Integer, value As Integer)
            If DebugValidateEachStep AndAlso (tet = 5 OrElse tet = 6) Then
                LastDebugLog.Add($"[N] SetNeighbor(tet#{tet}.N({slot}) = {value})")
            End If

            Dim t As Tetra = _tets(tet)
            t.N(slot) = value
            _tets(tet) = t
        End Sub

        Private Sub SetAlive(tet As Integer, alive As Boolean)
            Dim t As Tetra = _tets(tet)
            t.Alive = alive
            _tets(tet) = t
        End Sub

        Private Shared Sub SwapVal(Of T)(ByRef a As T, ByRef b As T)
            Dim tmp As T = a
            a = b
            b = tmp
        End Sub

        Private Shared Function Det3(m As Double(,)) As Double
            Return m(0, 0) * (m(1, 1) * m(2, 2) - m(1, 2) * m(2, 1)) -
                   m(0, 1) * (m(1, 0) * m(2, 2) - m(1, 2) * m(2, 0)) +
                   m(0, 2) * (m(1, 0) * m(2, 1) - m(1, 1) * m(2, 0))
        End Function

        Private Shared Function Det4(m As Double(,)) As Double
            Dim sum As Double = 0
            Dim sign As Double = 1

            For j As Integer = 0 To 3
                Dim minor As Double(,) = New Double(2, 2) {}
                Dim mr As Integer = 0

                For i As Integer = 1 To 3
                    Dim mc As Integer = 0

                    For k As Integer = 0 To 3
                        If k <> j Then
                            minor(mr, mc) = m(i, k)
                            mc += 1
                        End If
                    Next

                    mr += 1
                Next

                sum += sign * m(0, j) * Det3(minor)
                sign = -sign
            Next

            Return sum
        End Function
    End Class
End Namespace
