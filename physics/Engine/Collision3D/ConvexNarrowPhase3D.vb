Imports System.Linq
Imports Microsoft.VisualBasic.Engine.Physics.Math3D
Imports std = System.Math

Namespace Collision3D

    ''' <summary>
    ''' 凸多面体在世界坐标下的临时视图：世界顶点、世界面法向、边表与包围球。
    ''' 每个接触对在每个子步临时构建一次，供 SAT 与面裁剪使用。
    ''' </summary>
    Public NotInheritable Class ConvexView

        ''' <summary>所属刚体。</summary>
        Public Body As RigidBody3D
        ''' <summary>质心（即刚体位置）。</summary>
        Public Center As Vector3
        ''' <summary>包围球半径（局部最大顶点半径）。</summary>
        Public Radius As Double
        ''' <summary>世界坐标顶点。</summary>
        Public WorldVerts As Vector3()
        ''' <summary>面表（顶点索引环）。</summary>
        Public Faces As Integer()()
        ''' <summary>世界坐标面法向（外向）。</summary>
        Public Normals As Vector3()
        ''' <summary>去重边表（顶点索引对）。</summary>
        Public Edges As Integer()()

        ''' <summary>由刚体构建视图；形状不是盒或凸多面体时返回 Nothing。</summary>
        Public Shared Function Create(body As RigidBody3D) As ConvexView
            Dim mesh As ConvexMesh3D = ConvexNarrowPhase3D.ConvexOf(body)

            If mesh Is Nothing Then Return Nothing

            Dim q As Quaternion = body.Orientation
            Dim src As Vector3() = mesh.FaceNormals
            Dim nf(src.Length - 1) As Vector3

            For i As Integer = 0 To src.Length - 1
                nf(i) = q.Rotate(src(i))
            Next

            Return New ConvexView With {
                .Body = body,
                .Center = body.Position,
                .Radius = mesh.BoundRadius,
                .WorldVerts = mesh.WorldVertices(body.Position, q),
                .Faces = mesh.Faces,
                .Normals = nf,
                .Edges = mesh.Edges
            }
        End Function

        ''' <summary>第 <paramref name="i"/> 个面的参考点（该面第一个顶点）。</summary>
        Public Function FacePoint(i As Integer) As Vector3
            Return WorldVerts(Faces(i)(0))
        End Function

        ''' <summary>点 <paramref name="p"/> 到第 <paramref name="i"/> 个面所在平面的有符号距离（正 = 在外侧）。</summary>
        Public Function PlaneDistance(i As Integer, p As Vector3) As Double
            Return Vector3Math.Dot(Normals(i), p - FacePoint(i))
        End Function
    End Class

    ''' <summary>
    ''' 凸多面体窄相位：盒-盒、盒-凸、凸-凸（SAT + 参考面/入射面裁剪）、
    ''' 凸-平面（顶点投影）、球-凸（最近点）。
    ''' </summary>
    ''' <remarks>
    ''' 盒体通过 <see cref="BoxCollider3D.AsConvex"/> 复用同一套例程，
    ''' 因此"立方体落在斜面 / 平台上"这类组合只需维护一份实现。
    ''' </remarks>
    Public Module ConvexNarrowPhase3D

        ''' <summary>单个流形最多保留的接触点数（多点流形才撑得住面接触）。</summary>
        Public Const MaxContacts As Integer = 4
        ''' <summary>边×边候选轴数量上限，防止高分段圆柱/圆锥把 SAT 变成 O(n²) 灾难。</summary>
        Private Const MaxEdgePairs As Integer = 600

        ''' <summary>取刚体可用的凸网格；盒体返回其等价凸壳，其它形状返回 Nothing。</summary>
        Public Function ConvexOf(body As RigidBody3D) As ConvexMesh3D
            If body Is Nothing OrElse body.Shape Is Nothing Then Return Nothing

            If TypeOf body.Shape Is ConvexPolyhedronCollider3D Then
                Return DirectCast(body.Shape, ConvexPolyhedronCollider3D).Mesh
            End If

            If TypeOf body.Shape Is BoxCollider3D Then
                Return DirectCast(body.Shape, BoxCollider3D).AsConvex
            End If

            Return Nothing
        End Function

        ' /********************************************************************************/
        '  凸 vs 凸
        ' /********************************************************************************/

        ''' <summary>凸-凸（含盒-盒、盒-凸）碰撞：SAT 求最小穿透轴 + 面裁剪生成接触流形。</summary>
        Public Function ConvexConvex(a As RigidBody3D, b As RigidBody3D) As Manifold3D
            Dim va As ConvexView = ConvexView.Create(a)
            Dim vb As ConvexView = ConvexView.Create(b)

            If va Is Nothing OrElse vb Is Nothing Then Return Nothing

            ' 0) 包围球早退：绝大多数宽相位通过的配对在这一步就被否掉
            Dim d As Vector3 = vb.Center - va.Center
            Dim rsum As Double = va.Radius + vb.Radius

            If Vector3Math.LengthSquared(d) > rsum * rsum Then Return Nothing

            ' 1) SAT：双方面法向 + 边×边，取重叠最小的轴
            Dim axis As Vector3 = Nothing
            Dim bestOverlap As Double = Double.MaxValue

            For Each n As Vector3 In va.Normals
                Dim o As Double = 0.0

                If Not OverlapOnAxis(n, va, vb, o) Then Return Nothing
                If o < bestOverlap Then bestOverlap = o : axis = n
            Next

            For Each n As Vector3 In vb.Normals
                Dim o As Double = 0.0

                If Not OverlapOnAxis(n, va, vb, o) Then Return Nothing
                If o < bestOverlap Then bestOverlap = o : axis = n
            Next

            If va.Edges.Length * vb.Edges.Length <= MaxEdgePairs Then
                For Each ea As Integer() In va.Edges
                    Dim da As Vector3 = va.WorldVerts(ea(1)) - va.WorldVerts(ea(0))

                    For Each eb As Integer() In vb.Edges
                        Dim db As Vector3 = vb.WorldVerts(eb(1)) - vb.WorldVerts(eb(0))
                        Dim n As Vector3 = Vector3Math.Cross(da, db)

                        If Vector3Math.LengthSquared(n) < 1.0E-8 Then Continue For

                        n = Vector3Math.Normalize(n)

                        Dim o As Double = 0.0

                        If Not OverlapOnAxis(n, va, vb, o) Then Return Nothing
                        If o < bestOverlap Then bestOverlap = o : axis = n
                    Next
                Next
            End If

            If axis Is Nothing Then Return Nothing

            ' 2) 法向统一指向 A → B（求解器按 A 受 -P、B 受 +P 施加冲量）
            If Vector3Math.Dot(axis, vb.Center - va.Center) < 0.0 Then
                axis = axis * -1.0
            End If

            ' 3) 参考面 / 入射面
            Dim faceA As Integer = MostAlignedFace(va, axis)
            Dim faceB As Integer = MostAlignedFace(vb, axis * -1.0)
            Dim relA As Double = Vector3Math.Dot(va.Normals(faceA), axis)
            Dim relB As Double = Vector3Math.Dot(vb.Normals(faceB), axis * -1.0)
            Dim refIsA As Boolean = (relA >= relB - 0.001)

            Dim refView As ConvexView = If(refIsA, va, vb)
            Dim incView As ConvexView = If(refIsA, vb, va)
            Dim refFace As Integer = If(refIsA, faceA, faceB)
            Dim refNormal As Vector3 = refView.Normals(refFace)
            ' 参考体为 A 时其面外法向即 A→B；参考体为 B 时外法向是 B→A，需取反
            Dim manifoldNormal As Vector3 = If(refIsA, refNormal, refNormal * -1.0)

            ' 入射面：法向与参考面法向最反向的那个面
            Dim incFace As Integer = 0
            Dim minDot As Double = Double.MaxValue

            For i As Integer = 0 To incView.Normals.Length - 1
                Dim dp As Double = Vector3Math.Dot(incView.Normals(i), refNormal)

                If dp < minDot Then minDot = dp : incFace = i
            Next

            ' 4) 用参考面的侧平面裁剪入射面多边形
            Dim refIdx As Integer() = refView.Faces(refFace)
            Dim refPoint As Vector3 = refView.FacePoint(refFace)
            Dim poly As New List(Of Vector3)()

            For Each idx As Integer In incView.Faces(incFace)
                poly.Add(incView.WorldVerts(idx))
            Next

            For i As Integer = 0 To refIdx.Length - 1
                Dim e0 As Vector3 = refView.WorldVerts(refIdx(i))
                Dim e1 As Vector3 = refView.WorldVerts(refIdx((i + 1) Mod refIdx.Length))
                Dim side As Vector3 = Vector3Math.Normalize(Vector3Math.Cross(e1 - e0, refNormal))

                If Vector3Math.LengthSquared(side) < 0.5 Then Continue For

                poly = ClipPolygon(poly, side, Vector3Math.Dot(side, e0))

                If poly.Count = 0 Then Exit For
            Next

            ' 5) 保留落在参考面内侧（穿透）的点
            Dim points As New List(Of Vector3)()
            Dim pens As New List(Of Double)()

            For Each p As Vector3 In poly
                Dim depth As Double = Vector3Math.Dot(refNormal, refPoint - p)

                If depth >= -1.0E-4 Then
                    points.Add(p)
                    pens.Add(std.Max(depth, 0.0))
                End If
            Next

            If points.Count > MaxContacts Then
                points = KeepDeepest(points, pens, MaxContacts)
            End If

            If points.Count = 0 Then
                ' 裁剪退化（例如极小的面或数值异常）：回退到"顶点穿透"判据
                Return VertexPenetration(va, vb, manifoldNormal)
            End If

            Return NarrowPhase3D.Build(a, b, manifoldNormal, points, pens)
        End Function

        ''' <summary>取重叠量；返回 False 表示该轴分离（可立即判定无碰撞）。</summary>
        Private Function OverlapOnAxis(n As Vector3, va As ConvexView, vb As ConvexView, ByRef overlap As Double) As Boolean
            Dim lo1 As Double, hi1 As Double, lo2 As Double, hi2 As Double

            Call Project(va, n, lo1, hi1)
            Call Project(vb, n, lo2, hi2)

            Dim o As Double = std.Min(hi1, hi2) - std.Max(lo1, lo2)

            overlap = o

            Return o > 0.0
        End Function

        Private Sub Project(view As ConvexView, n As Vector3, ByRef lo As Double, ByRef hi As Double)
            lo = Double.MaxValue
            hi = Double.MinValue

            For Each v As Vector3 In view.WorldVerts
                Dim p As Double = Vector3Math.Dot(v, n)

                If p < lo Then lo = p
                If p > hi Then hi = p
            Next
        End Sub

        Private Function MostAlignedFace(view As ConvexView, dir As Vector3) As Integer
            Dim best As Integer = 0
            Dim bestDot As Double = Double.MinValue

            For i As Integer = 0 To view.Normals.Length - 1
                Dim dp As Double = Vector3Math.Dot(view.Normals(i), dir)

                If dp > bestDot Then bestDot = dp : best = i
            Next

            Return best
        End Function

        ''' <summary>用半空间 <c>dot(n, p) &lt;= offset</c> 裁剪多边形（Sutherland–Hodgman）。</summary>
        Private Function ClipPolygon(poly As List(Of Vector3), n As Vector3, offset As Double) As List(Of Vector3)
            Dim out As New List(Of Vector3)()

            If poly.Count = 0 Then Return out

            Dim prev As Vector3 = poly(poly.Count - 1)
            Dim prevD As Double = Vector3Math.Dot(n, prev) - offset

            For Each cur As Vector3 In poly
                Dim curD As Double = Vector3Math.Dot(n, cur) - offset

                If curD <= 0.0 Then
                    If prevD > 0.0 Then
                        Dim t As Double = prevD / (prevD - curD)
                        out.Add(prev + (cur - prev) * t)
                    End If

                    out.Add(cur)
                ElseIf prevD <= 0.0 Then
                    Dim t As Double = prevD / (prevD - curD)
                    out.Add(prev + (cur - prev) * t)
                End If

                prev = cur
                prevD = curD
            Next

            Return out
        End Function

        ''' <summary>按穿透深度降序保留前 <paramref name="count"/> 个点。</summary>
        Private Function KeepDeepest(points As List(Of Vector3), pens As List(Of Double), count As Integer) As List(Of Vector3)

            Dim order As Integer() = Enumerable.Range(0, points.Count).ToArray()

            Array.Sort(order, Function(x As Integer, y As Integer) pens(y).CompareTo(pens(x)))

            Dim keep As New List(Of Vector3)()
            Dim keptPen As New List(Of Double)()

            For i As Integer = 0 To count - 1
                keep.Add(points(order(i)))
                keptPen.Add(pens(order(i)))
            Next

            points.Clear()
            pens.Clear()
            points.AddRange(keep)
            pens.AddRange(keptPen)

            Return points
        End Function

        ''' <summary>
        ''' 回退路径：取"入射体落在参考体内部的顶点"作为接触点。
        ''' 面裁剪在退化情形下可能一个点都裁不出来，此时必须仍能给出可用流形，
        ''' 否则物体会直接穿透掉落。
        ''' </summary>
        Private Function VertexPenetration(va As ConvexView, vb As ConvexView, fallbackNormal As Vector3) As Manifold3D
            Dim pts As New List(Of Vector3)()
            Dim pens As New List(Of Double)()
            Dim nrm As Vector3 = Nothing

            ' B 的顶点落在 A 内
            Dim deepest As Double = Double.MinValue

            For Each v As Vector3 In vb.WorldVerts
                Dim shallow As Double = Double.MinValue
                Dim shallowN As Vector3 = Nothing

                For i As Integer = 0 To va.Normals.Length - 1
                    Dim d As Double = va.PlaneDistance(i, v)

                    If d > shallow Then shallow = d : shallowN = va.Normals(i)
                Next

                If shallow <= 0.0 AndAlso shallowN IsNot Nothing Then
                    pts.Add(v)
                    pens.Add(-shallow)

                    If -shallow > deepest Then deepest = -shallow : nrm = shallowN
                End If
            Next

            If pts.Count > 0 Then
                If pts.Count > MaxContacts Then pts = KeepDeepest(pts, pens, MaxContacts)
                ' A 的外向面法向即 A→B
                Return NarrowPhase3D.Build(va.Body, vb.Body, nrm, pts, pens)
            End If

            ' A 的顶点落在 B 内
            deepest = Double.MinValue

            For Each v As Vector3 In va.WorldVerts
                Dim shallow As Double = Double.MinValue
                Dim shallowN As Vector3 = Nothing

                For i As Integer = 0 To vb.Normals.Length - 1
                    Dim d As Double = vb.PlaneDistance(i, v)

                    If d > shallow Then shallow = d : shallowN = vb.Normals(i)
                Next

                If shallow <= 0.0 AndAlso shallowN IsNot Nothing Then
                    pts.Add(v)
                    pens.Add(-shallow)

                    If -shallow > deepest Then deepest = -shallow : nrm = shallowN
                End If
            Next

            If pts.Count = 0 Then Return Nothing

            If pts.Count > MaxContacts Then pts = KeepDeepest(pts, pens, MaxContacts)

            ' B 的外向面法向是 B→A，取反得 A→B
            Return NarrowPhase3D.Build(va.Body, vb.Body, nrm * -1.0, pts, pens)
        End Function

        ' /********************************************************************************/
        '  凸 vs 无限平面
        ' /********************************************************************************/

        ''' <summary>
        ''' 平面（A）与凸多面体（B）：把顶点投影到平面法向，取穿透最深的若干个点。
        ''' 法向 A→B 即平面外法向，求解器会把凸体沿外法向推开。
        ''' </summary>
        Public Function PlaneConvex(plane As RigidBody3D, convex As RigidBody3D) As Manifold3D
            Dim pl As PlaneCollider3D = TryCast(plane.Shape, PlaneCollider3D)
            Dim mesh As ConvexMesh3D = ConvexOf(convex)

            If pl Is Nothing OrElse mesh Is Nothing Then Return Nothing

            Dim verts As Vector3() = mesh.WorldVertices(convex.Position, convex.Orientation)
            Dim pts As New List(Of Vector3)()
            Dim pens As New List(Of Double)()

            For Each v As Vector3 In verts
                Dim d As Double = pl.SignedDistance(v)

                If d < 0.0 Then
                    pts.Add(v)
                    pens.Add(-d)
                End If
            Next

            If pts.Count = 0 Then Return Nothing
            If pts.Count > MaxContacts Then pts = KeepDeepest(pts, pens, MaxContacts)

            Return NarrowPhase3D.Build(plane, convex, pl.Normal, pts, pens)
        End Function

        ''' <summary>
        ''' 凸多面体（A）与无限平面（B）：与 <see cref="PlaneConvex"/> 是同一对组合的反向分派
        ''' （盒=2 &lt; 平面=3，规范化顺序后盒在前）。法向 A→B = 平面外法向的反向，
        ''' 这样求解器给 A 施加的 -P 正好沿平面外法向把凸体推出去。
        ''' </summary>
        Public Function ConvexPlane(convex As RigidBody3D, plane As RigidBody3D) As Manifold3D
            Dim pl As PlaneCollider3D = TryCast(plane.Shape, PlaneCollider3D)
            Dim mesh As ConvexMesh3D = ConvexOf(convex)

            If pl Is Nothing OrElse mesh Is Nothing Then Return Nothing

            Dim verts As Vector3() = mesh.WorldVertices(convex.Position, convex.Orientation)
            Dim pts As New List(Of Vector3)()
            Dim pens As New List(Of Double)()

            For Each v As Vector3 In verts
                Dim d As Double = pl.SignedDistance(v)

                If d < 0.0 Then
                    pts.Add(v)
                    pens.Add(-d)
                End If
            Next

            If pts.Count = 0 Then Return Nothing
            If pts.Count > MaxContacts Then pts = KeepDeepest(pts, pens, MaxContacts)

            Return NarrowPhase3D.Build(convex, plane, pl.Normal * -1.0, pts, pens)
        End Function

        ' /********************************************************************************/
        '  球 vs 凸
        ' /********************************************************************************/

        ''' <summary>
        ''' 球（A）与凸多面体（B）：先做半空间分离测试，再求球心在凸体上的最近点，
        ''' 得到正确的接触法向（面、棱、角三种情形都覆盖）。
        ''' </summary>
        Public Function SphereConvex(sphere As RigidBody3D, convex As RigidBody3D) As Manifold3D
            Dim sp As SphereCollider3D = TryCast(sphere.Shape, SphereCollider3D)
            Dim mesh As ConvexMesh3D = ConvexOf(convex)

            If sp Is Nothing OrElse mesh Is Nothing Then Return Nothing

            Dim view As ConvexView = ConvexView.Create(convex)
            Dim c As Vector3 = sphere.Position
            Dim r As Double = sp.Radius

            ' 包围球早退
            If Vector3Math.LengthSquared(c - view.Center) > (r + view.Radius) * (r + view.Radius) Then
                Return Nothing
            End If

            ' 1) 半空间分离测试：球心到某面平面的距离超过半径即分离
            Dim maxD As Double = Double.MinValue
            Dim maxFace As Integer = 0

            For i As Integer = 0 To view.Normals.Length - 1
                Dim d As Double = view.PlaneDistance(i, c)

                If d > maxD Then maxD = d : maxFace = i
            Next

            If maxD > r Then Return Nothing

            ' 2) 最近点
            If maxD <= 0.0 Then
                ' 球心已在凸体内部：沿最浅的面外法向逃逸
                Dim nOut As Vector3 = view.Normals(maxFace)
                Dim pen As Double = r - maxD
                Dim p As Vector3 = c + nOut * -1.0 * std.Max(0.0, r - pen * 0.5)

                ' A→B：由球指向凸体 = 与凸体外法向相反
                Return NarrowPhase3D.Build(sphere, convex, nOut * -1.0, {p}, {pen})
            End If

            Dim q As Vector3 = ClampToFace(view, maxFace, c - view.Normals(maxFace) * maxD)
            Dim delta As Vector3 = c - q
            Dim dist As Double = Vector3Math.Length(delta)

            If dist > r Then Return Nothing

            If dist > 1.0E-9 Then
                Dim nAB As Vector3 = delta * (-1.0 / dist)
                Dim pen2 As Double = r - dist
                Dim p2 As Vector3 = c + nAB * std.Max(0.0, r - pen2 * 0.5)

                Return NarrowPhase3D.Build(sphere, convex, nAB, {p2}, {pen2})
            End If

            ' 退化：球心正好落在面上
            Dim nFace As Vector3 = view.Normals(maxFace)
            Dim p3 As Vector3 = c + nFace * -0.5 * r

            Return NarrowPhase3D.Build(sphere, convex, nFace * -1.0, {p3}, {r})
        End Function

        ''' <summary>把点 <paramref name="p"/> 限制在第 <paramref name="face"/> 个面的多边形内部。</summary>
        Private Function ClampToFace(view As ConvexView, face As Integer, p As Vector3) As Vector3
            Dim idx As Integer() = view.Faces(face)
            Dim n As Vector3 = view.Normals(face)
            Dim q As Vector3 = p

            ' 两轮迭代：凸多边形上单轮裁剪在角点处可能仍未收敛
            For pass As Integer = 1 To 2
                For i As Integer = 0 To idx.Length - 1
                    Dim a As Vector3 = view.WorldVerts(idx(i))
                    Dim b As Vector3 = view.WorldVerts(idx((i + 1) Mod idx.Length))
                    Dim edge As Vector3 = Vector3Math.Normalize(Vector3Math.Cross(b - a, n))

                    If Vector3Math.LengthSquared(edge) < 0.5 Then Continue For

                    Dim d As Double = Vector3Math.Dot(edge, q - a)

                    If d > 0.0 Then q = q + edge * -d
                Next
            Next

            Return q
        End Function
    End Module
End Namespace
