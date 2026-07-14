' Copyright (c) 2018 GPL3 Licensed
' Narrow Phase：圆-圆、圆-多边形、多边形-多边形(SAT+裁剪) 精确碰撞，生成接触流形。

Imports System
Imports System.Math
Imports Microsoft.VisualBasic.Imaging.Physics.RigidBody
Imports std = System.Math

Namespace Collision

    ''' <summary>
    ''' Narrow Phase 窄相位精确碰撞检测。根据几何体类型分派到具体算法，
    ''' 生成 <see cref="Manifold"/>（法向由 A 指向 B、穿透深度、接触点）。
    ''' </summary>
    Public Module NarrowPhase

        ''' <summary>计算 a 与 b 之间的碰撞流形；无碰撞时返回空流形（无接触点）</summary>
        Public Function Collide(a As RigidBody, b As RigidBody) As Manifold
            If a.Shape.Kind = Collider.ShapeKind.Circle AndAlso b.Shape.Kind = Collider.ShapeKind.Circle Then
                Return CircleCircle(a, b)
            ElseIf a.Shape.Kind = Collider.ShapeKind.Circle Then
                Return CirclePolygon(a, b)
            ElseIf b.Shape.Kind = Collider.ShapeKind.Circle Then
                Dim m = CirclePolygon(b, a)   ' A=circle(=b), B=poly(=a), normal b->a
                Dim tmp = m.A : m.A = m.B : m.B = tmp
                m.Normal = -m.Normal
                Return m
            Else
                Return PolygonPolygon(a, b)
            End If
        End Function

        ' ---------------- 圆 - 圆 ----------------

        Private Function CircleCircle(a As RigidBody, b As RigidBody) As Manifold
            Dim ca = a.Position, cb = b.Position
            Dim rA = CType(a.Shape, CircleCollider).Radius
            Dim rB = CType(b.Shape, CircleCollider).Radius
            Dim normal = cb - ca
            Dim dist = Length(normal)
            Dim m = New Manifold With {.A = a, .B = b}

            If dist >= rA + rB Then
                m.Contacts = New Vector2() {}
                m.Penetration = 0
                Return m
            End If

            If dist < 1.0e-9 Then
                m.Penetration = rA
                m.Normal = New Vector2(1, 0)
                m.Contacts = New Vector2() {ca}
            Else
                m.Penetration = rA + rB - dist
                m.Normal = normal / dist
                m.Contacts = New Vector2() {m.Normal * rA + ca}
            End If

            m.Restitution = PhysicsMaterial.CombineRestitution(a.Material, b.Material)
            m.Friction = PhysicsMaterial.CombineFriction(a.Material, b.Material)
            Return m
        End Function

        ' ---------------- 圆 - 多边形 ----------------

        Private Function CirclePolygon(circle As RigidBody, poly As RigidBody) As Manifold
            Dim pc = CType(poly.Shape, PolygonCollider)
            Dim r = CType(circle.Shape, CircleCollider).Radius
            Dim center = circle.Position
            Dim d = Rotate(center - poly.Position, -poly.Rotation)  ' 变换到多边形局部空间

            Dim m = New Manifold With {.A = circle, .B = poly}
            m.Restitution = PhysicsMaterial.CombineRestitution(circle.Material, poly.Material)
            m.Friction = PhysicsMaterial.CombineFriction(circle.Material, poly.Material)

            Dim separation = Double.NegativeInfinity
            Dim faceNormal = 0

            For i = 0 To pc.Count - 1
                Dim s = Dot(pc.normals(i), d - pc.vertices(i))
                If s > r Then Return EmptyManifold(circle, poly)
                If s > separation Then separation = s : faceNormal = i
            Next

            Dim v1 = pc.vertices(faceNormal)
            Dim v2 = pc.vertices((faceNormal + 1) Mod pc.Count)

            If separation < 1.0e-9 Then
                ' 圆心在多边形内部
                Dim nLocal = pc.normals(faceNormal)
                m.Normal = -Rotate(nLocal, poly.Rotation)
                m.Contacts = New Vector2() {Rotate(v1, poly.Rotation) + poly.Position}
                m.Penetration = r
                Return m
            End If

            m.Penetration = r - separation
            Dim dot1 = Dot(d - v1, v2 - v1)
            Dim dot2 = Dot(d - v2, v1 - v2)

            If dot1 <= 0 Then
                If DistSq(d, v1) > r * r Then Return EmptyManifold(circle, poly)
                Dim nLocal = Normalize(v1 - d)
                m.Normal = Rotate(nLocal, poly.Rotation)
                m.Contacts = New Vector2() {Rotate(v1, poly.Rotation) + poly.Position}
            ElseIf dot2 <= 0 Then
                If DistSq(d, v2) > r * r Then Return EmptyManifold(circle, poly)
                Dim nLocal = Normalize(v2 - d)
                m.Normal = Rotate(nLocal, poly.Rotation)
                m.Contacts = New Vector2() {Rotate(v2, poly.Rotation) + poly.Position}
            Else
                Dim nLocal = pc.normals(faceNormal)
                If Dot(d - v1, nLocal) > r Then Return EmptyManifold(circle, poly)
                m.Normal = -Rotate(nLocal, poly.Rotation)
                m.Contacts = New Vector2() {m.Normal * r + center}
            End If

            Return m
        End Function

        ' ---------------- 多边形 - 多边形 (SAT + 裁剪) ----------------

        Private Function PolygonPolygon(a As RigidBody, b As RigidBody) As Manifold
            Dim m = New Manifold With {.A = a, .B = b}

            Dim penA = FindAxisLeastPenetration(a, b)
            If penA.Item2 >= 0 Then Return EmptyManifold(a, b)
            Dim penB = FindAxisLeastPenetration(b, a)
            If penB.Item2 >= 0 Then Return EmptyManifold(a, b)

            Dim refBody, incBody As RigidBody
            Dim refIndex As Integer
            Dim flip As Boolean

            If BiasGreaterThan(penA.Item2, penB.Item2) Then
                refBody = a : incBody = b : refIndex = penA.Item1 : flip = False
            Else
                refBody = b : incBody = a : refIndex = penB.Item1 : flip = True
            End If

            Dim refPoly = CType(refBody.Shape, PolygonCollider)
            Dim incidentFace = FindIncidentFace(refBody, incBody, refIndex)

            Dim v1 = refPoly.WorldVertex(refIndex, refBody.Position, refBody.Rotation)
            refIndex = (refIndex + 1) Mod refPoly.Count
            Dim v2 = refPoly.WorldVertex(refIndex, refBody.Position, refBody.Rotation)

            Dim sidePlaneNormal = Normalize(v2 - v1)
            Dim refFaceNormal = New Vector2(sidePlaneNormal.y, -sidePlaneNormal.x)

            Dim refC = Dot(refFaceNormal, v1)
            Dim negSide = -Dot(sidePlaneNormal, v1)
            Dim posSide = Dot(sidePlaneNormal, v2)

            If Clip(-sidePlaneNormal, negSide, incidentFace) < 2 Then Return EmptyManifold(a, b)
            If Clip(sidePlaneNormal, posSide, incidentFace) < 2 Then Return EmptyManifold(a, b)

            m.Normal = If(flip, -refFaceNormal, refFaceNormal)

            Dim contacts As New List(Of Vector2)
            Dim penSum = 0.0
            For Each p In incidentFace
                Dim sep = Dot(refFaceNormal, p) - refC
                If sep <= 0 Then
                    contacts.Add(p)
                    penSum += -sep
                End If
            Next
            If contacts.Count = 0 Then Return EmptyManifold(a, b)

            m.Contacts = contacts.ToArray()
            m.Penetration = penSum / contacts.Count
            m.Restitution = PhysicsMaterial.CombineRestitution(a.Material, b.Material)
            m.Friction = PhysicsMaterial.CombineFriction(a.Material, b.Material)
            Return m
        End Function

        ''' <summary>找到参考多边形上穿透最浅（即分离轴）的面索引与穿透值</summary>
        Private Function FindAxisLeastPenetration(A As RigidBody, B As RigidBody) As (Integer, Double)
            Dim polyA = CType(A.Shape, PolygonCollider)
            Dim polyB = CType(B.Shape, PolygonCollider)
            Dim best = Double.NegativeInfinity
            Dim bestIndex = 0

            For i = 0 To polyA.Count - 1
                Dim nw = polyA.WorldNormal(i, A.Rotation)
                Dim vA = polyA.WorldVertex(i, A.Position, A.Rotation)
                Dim sB = polyB.WorldSupport(-nw, B.Position, B.Rotation)
                Dim d = Dot(nw, sB - vA)
                If d > best Then
                    best = d
                    bestIndex = i
                End If
            Next

            Return (bestIndex, best)
        End Function

        ''' <summary>找到入射多边形上法向与参考面法向最相反的边（世界坐标两点）</summary>
        Private Function FindIncidentFace(refBody As RigidBody, incBody As RigidBody, referenceIndex As Integer) As Vector2()
            Dim refPoly = CType(refBody.Shape, PolygonCollider)
            Dim incPoly = CType(incBody.Shape, PolygonCollider)
            Dim refNormal = refPoly.WorldNormal(referenceIndex, refBody.Rotation)

            Dim minDot = Double.PositiveInfinity
            Dim incidentFace = 0
            For i = 0 To incPoly.Count - 1
                Dim n = incPoly.WorldNormal(i, incBody.Rotation)
                Dim d = Dot(refNormal, n)
                If d < minDot Then
                    minDot = d
                    incidentFace = i
                End If
            Next

            Dim v0 = incPoly.WorldVertex(incidentFace, incBody.Position, incBody.Rotation)
            Dim v1 = incPoly.WorldVertex((incidentFace + 1) Mod incPoly.Count, incBody.Position, incBody.Rotation)
            Return New Vector2() {v0, v1}
        End Function

        ''' <summary>将面 <paramref name="face"/> 裁切到平面 n·x = c 的背面，返回保留点数</summary>
        Private Function Clip(n As Vector2, c As Double, ByRef face As Vector2()) As Integer
            Dim outp(1) As Vector2
            Dim sp = 0
            Dim d1 = Dot(n, face(0)) - c
            Dim d2 = Dot(n, face(1)) - c

            If d1 <= 0 Then outp(sp) = face(0) : sp += 1
            If d2 <= 0 Then outp(sp) = face(1) : sp += 1
            If d1 * d2 < 0 Then
                Dim alpha = d1 / (d1 - d2)
                outp(sp) = face(0) + alpha * (face(1) - face(0))
                sp += 1
            End If

            face(0) = outp(0)
            face(1) = outp(1)
            Return sp
        End Function

        ''' <summary>带偏置的比较：用于选择参考面（消除翻转抖动）</summary>
        Private Function BiasGreaterThan(a As Double, b As Double) As Boolean
            Const relative = 0.95
            Const absolute = 0.01
            Return a >= b * relative + a * absolute
        End Function

        Private Function DistSq(a As Vector2, b As Vector2) As Double
            Return LengthSquared(a - b)
        End Function

        Private Function EmptyManifold(a As RigidBody, b As RigidBody) As Manifold
            Return New Manifold With {.A = a, .B = b, .Contacts = New Vector2() {}, .Penetration = 0}
        End Function
    End Module
End Namespace
