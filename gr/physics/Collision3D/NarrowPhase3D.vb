Imports System.Math
Imports Microsoft.VisualBasic.Imaging.Physics.Math3D
Imports std = System.Math

Namespace Collision3D

    ''' <summary>
    ''' 3D 窄相位碰撞检测：按碰撞体种类分派，输出接触流形。
    ''' </summary>
    ''' <remarks>
    ''' 覆盖的组合：球-球、球-胶囊、胶囊-胶囊、球-盒、胶囊-盒、球-平面、胶囊-平面。
    ''' 盒-盒与盒-平面在本库中返回空流形：火柴人的身体全部由胶囊/球体构成，
    ''' 而盒体只用于静态关卡几何，静态-静态对在宽相位就已被剔除。
    ''' </remarks>
    Public Module NarrowPhase3D

        ''' <summary>
        ''' 检测一对刚体的碰撞。无接触时返回 <c>Nothing</c>。
        ''' </summary>
        Public Function Collide(a As RigidBody3D, b As RigidBody3D) As Manifold3D
            If a Is Nothing OrElse b Is Nothing Then
                Return Nothing
            End If

            Dim ka As ShapeKind3D = a.Shape.Kind
            Dim kb As ShapeKind3D = b.Shape.Kind

            ' 规范化顺序，减少需要实现的组合数量
            If CInt(ka) > CInt(kb) Then
                Dim m As Manifold3D = CollideSorted(b, a)
                If m IsNot Nothing Then
                    m.Normal = m.Normal * -1.0
                End If
                Return m
            End If

            Return CollideSorted(a, b)
        End Function

        Private Function CollideSorted(a As RigidBody3D, b As RigidBody3D) As Manifold3D
            Select Case a.Shape.Kind
                Case ShapeKind3D.Sphere
                    Select Case b.Shape.Kind
                        Case ShapeKind3D.Sphere : Return SphereSphere(a, b)
                        Case ShapeKind3D.Capsule : Return SphereCapsule(a, b)
                        Case ShapeKind3D.Box : Return SphereBox(a, b)
                        Case ShapeKind3D.Plane : Return SpherePlane(a, b)
                    End Select
                Case ShapeKind3D.Capsule
                    Select Case b.Shape.Kind
                        Case ShapeKind3D.Capsule : Return CapsuleCapsule(a, b)
                        Case ShapeKind3D.Box : Return CapsuleBox(a, b)
                        Case ShapeKind3D.Plane : Return CapsulePlane(a, b)
                    End Select
            End Select

            Return Nothing
        End Function

        ' /********************************************************************************/
        '  球 / 胶囊 与平面
        ' /********************************************************************************/

        Private Function SpherePlane(sphere As RigidBody3D, plane As RigidBody3D) As Manifold3D
            Dim pl = DirectCast(plane.Shape, PlaneCollider3D)
            Dim sp = DirectCast(sphere.Shape, SphereCollider3D)
            Dim distance As Double = pl.SignedDistance(sphere.Position)

            If distance >= sp.Radius Then
                Return Nothing
            End If

            Dim penetration As Double = sp.Radius - distance
            Dim normal As Vector3 = pl.Normal * -1.0
            Dim point As Vector3 = sphere.Position + normal * (sp.Radius - penetration * 0.5)

            Return Build(sphere, plane, normal, {point}, {penetration})
        End Function

        Private Function CapsulePlane(capsule As RigidBody3D, plane As RigidBody3D) As Manifold3D
            Dim pl = DirectCast(plane.Shape, PlaneCollider3D)
            Dim cp = DirectCast(capsule.Shape, CapsuleCollider3D)
            Dim points As New List(Of Vector3)()
            Dim penetrations As New List(Of Double)()

            For Each tip As Vector3 In CapsuleTips(capsule, cp)
                Dim distance As Double = pl.SignedDistance(tip)

                If distance < cp.Radius Then
                    Dim penetration As Double = cp.Radius - distance
                    Dim normal As Vector3 = pl.Normal * -1.0

                    points.Add(tip + normal * (cp.Radius - penetration * 0.5))
                    penetrations.Add(penetration)
                End If
            Next

            If points.Count = 0 Then
                Return Nothing
            End If

            Return Build(capsule, plane, pl.Normal * -1.0, points, penetrations)
        End Function

        ' /********************************************************************************/
        '  球 / 胶囊 之间
        ' /********************************************************************************/

        Private Function SphereSphere(a As RigidBody3D, b As RigidBody3D) As Manifold3D
            Dim sa = DirectCast(a.Shape, SphereCollider3D)
            Dim sb = DirectCast(b.Shape, SphereCollider3D)
            Dim d As Vector3 = b.Position - a.Position
            Dim dist As Double = Vector3Math.Length(d)
            Dim radius As Double = sa.Radius + sb.Radius

            If dist >= radius Then
                Return Nothing
            End If

            Dim normal As Vector3

            If dist < 1.0E-9 Then
                normal = New Vector3(0, 1, 0)
                dist = 0.0
            Else
                normal = d / dist
            End If

            Dim penetration As Double = radius - dist
            Dim point As Vector3 = a.Position + normal * (sa.Radius - penetration * 0.5)

            Return Build(a, b, normal, {point}, {penetration})
        End Function

        ''' <summary>球与胶囊：先求胶囊轴线段上距球心最近的点，再退化成球-球。</summary>
        Private Function SphereCapsule(sphere As RigidBody3D, capsule As RigidBody3D) As Manifold3D
            Dim sp = DirectCast(sphere.Shape, SphereCollider3D)
            Dim cp = DirectCast(capsule.Shape, CapsuleCollider3D)
            Dim tipA As Vector3 = capsule.Position + capsule.Orientation.Rotate(cp.LocalEndA)
            Dim tipB As Vector3 = capsule.Position + capsule.Orientation.Rotate(cp.LocalEndB)
            Dim closest As Vector3 = ClosestPointOnSegment(sphere.Position, tipA, tipB)
            Dim d As Vector3 = closest - sphere.Position
            Dim dist As Double = Vector3Math.Length(d)

            If dist >= sp.Radius + cp.Radius Then
                Return Nothing
            End If

            Dim normal As Vector3

            If dist < 1.0E-9 Then
                normal = New Vector3(0, 1, 0)
                dist = 0.0
            Else
                normal = d / dist
            End If

            Dim penetration As Double = sp.Radius + cp.Radius - dist
            Dim point As Vector3 = sphere.Position + normal * (sp.Radius - penetration * 0.5)

            Return Build(sphere, capsule, normal, {point}, {penetration})
        End Function

        ''' <summary>胶囊与胶囊：求两条轴线段的最近点对，再退化成球-球。</summary>
        Private Function CapsuleCapsule(a As RigidBody3D, b As RigidBody3D) As Manifold3D
            Dim ca = DirectCast(a.Shape, CapsuleCollider3D)
            Dim cb = DirectCast(b.Shape, CapsuleCollider3D)
            Dim a0 As Vector3 = a.Position + a.Orientation.Rotate(ca.LocalEndA)
            Dim a1 As Vector3 = a.Position + a.Orientation.Rotate(ca.LocalEndB)
            Dim b0 As Vector3 = b.Position + b.Orientation.Rotate(cb.LocalEndA)
            Dim b1 As Vector3 = b.Position + b.Orientation.Rotate(cb.LocalEndB)
            Dim pa As Vector3 = Nothing
            Dim pb As Vector3 = Nothing

            Call ClosestPointsBetweenSegments(a0, a1, b0, b1, pa, pb)

            Dim d As Vector3 = pb - pa
            Dim dist As Double = Vector3Math.Length(d)

            If dist >= ca.Radius + cb.Radius Then
                Return Nothing
            End If

            Dim normal As Vector3

            If dist < 1.0E-9 Then
                normal = New Vector3(0, 1, 0)
                dist = 0.0
            Else
                normal = d / dist
            End If

            Dim penetration As Double = ca.Radius + cb.Radius - dist
            Dim point As Vector3 = pa + normal * (ca.Radius - penetration * 0.5)

            Return Build(a, b, normal, {point}, {penetration})
        End Function

        ' /********************************************************************************/
        '  球 / 胶囊 与盒
        ' /********************************************************************************/

        Private Function SphereBox(sphere As RigidBody3D, box As RigidBody3D) As Manifold3D
            Dim sp = DirectCast(sphere.Shape, SphereCollider3D)
            Dim local As Vector3 = box.Orientation.Unrotate(sphere.Position - box.Position)
            Dim h As Vector3 = DirectCast(box.Shape, BoxCollider3D).HalfExtents
            Dim contact As Vector3 = Nothing
            Dim penetration As Double = 0.0
            Dim normalLocal As Vector3 = Nothing

            If Not BoxSphereContact(local, h, sp.Radius, contact, normalLocal, penetration) Then
                Return Nothing
            End If

            Dim normalWorld As Vector3 = box.Orientation.Rotate(normalLocal) * -1.0
            Dim point As Vector3 = box.Position + box.Orientation.Rotate(contact)

            Return Build(sphere, box, normalWorld, {point}, {penetration})
        End Function

        Private Function CapsuleBox(capsule As RigidBody3D, box As RigidBody3D) As Manifold3D
            Dim cp = DirectCast(capsule.Shape, CapsuleCollider3D)
            Dim h As Vector3 = DirectCast(box.Shape, BoxCollider3D).HalfExtents
            Dim points As New List(Of Vector3)()
            Dim penetrations As New List(Of Double)()
            Dim normalSum As New Vector3(0, 0, 0)

            For Each tip As Vector3 In CapsuleTips(capsule, cp)
                Dim local As Vector3 = box.Orientation.Unrotate(tip - box.Position)
                Dim contact As Vector3 = Nothing
                Dim normalLocal As Vector3 = Nothing
                Dim penetration As Double = 0.0

                If BoxSphereContact(local, h, cp.Radius, contact, normalLocal, penetration) Then
                    points.Add(box.Position + box.Orientation.Rotate(contact))
                    penetrations.Add(penetration)
                    normalSum = normalSum + normalLocal
                End If
            Next

            If points.Count = 0 Then
                Return Nothing
            End If

            Dim normalWorld As Vector3 = Vector3Math.Normalize(box.Orientation.Rotate(normalSum)) * -1.0

            Return Build(capsule, box, normalWorld, points, penetrations)
        End Function

        ''' <summary>
        ''' 盒局部坐标系下的球-盒检测。<paramref name="normalLocal"/> 由球心指向盒面。
        ''' </summary>
        Private Function BoxSphereContact(localCenter As Vector3, h As Vector3, radius As Double,
                                          ByRef contact As Vector3,
                                          ByRef normalLocal As Vector3,
                                          ByRef penetration As Double) As Boolean
            Dim clamped As Vector3 = New Vector3(
                Clamp(localCenter.x, -h.x, h.x),
                Clamp(localCenter.y, -h.y, h.y),
                Clamp(localCenter.z, -h.z, h.z))
            Dim inside As Boolean = (clamped.x = localCenter.x) AndAlso
                                    (clamped.y = localCenter.y) AndAlso
                                    (clamped.z = localCenter.z)

            If inside Then
                ' 球心在盒内：沿穿透最浅的一个面推出
                Dim dx As Double = h.x - std.Abs(localCenter.x)
                Dim dy As Double = h.y - std.Abs(localCenter.y)
                Dim dz As Double = h.z - std.Abs(localCenter.z)
                Dim minDepth As Double = std.Min(dx, std.Min(dy, dz))
                Dim n As New Vector3(0, 0, 0)

                If minDepth = dx Then
                    n = New Vector3(Sign(localCenter.x), 0, 0)
                ElseIf minDepth = dy Then
                    n = New Vector3(0, Sign(localCenter.y), 0)
                Else
                    n = New Vector3(0, 0, Sign(localCenter.z))
                End If

                normalLocal = n * -1.0
                penetration = minDepth + radius
                contact = localCenter + n * minDepth
                Return True
            End If

            Dim d As Vector3 = localCenter - clamped
            Dim dist As Double = Vector3Math.Length(d)

            If dist >= radius Then
                Return False
            End If

            normalLocal = (d / std.Max(dist, 1.0E-9)) * -1.0
            penetration = radius - dist
            contact = clamped
            Return True
        End Function

        ' /********************************************************************************/
        '  几何辅助
        ' /********************************************************************************/

        ''' <summary>胶囊两个端点球心的世界坐标。</summary>
        Public Function CapsuleTips(body As RigidBody3D, shape As CapsuleCollider3D) As Vector3()
            Return {
                body.Position + body.Orientation.Rotate(shape.LocalEndA),
                body.Position + body.Orientation.Rotate(shape.LocalEndB)
            }
        End Function

        ''' <summary>点 <paramref name="p"/> 到线段 [a,b] 的最近点。</summary>
        Public Function ClosestPointOnSegment(p As Vector3, a As Vector3, b As Vector3) As Vector3
            Dim ab As Vector3 = b - a
            Dim len2 As Double = Vector3Math.LengthSquared(ab)

            If len2 < 1.0E-12 Then
                Return a
            End If

            Dim t As Double = Vector3Math.Dot(p - a, ab) / len2
            t = std.Min(1.0, std.Max(0.0, t))

            Return a + ab * t
        End Function

        ''' <summary>两条线段之间的最近点对（Ericson, Real-Time Collision Detection）。</summary>
        Public Sub ClosestPointsBetweenSegments(p1 As Vector3, q1 As Vector3,
                                                p2 As Vector3, q2 As Vector3,
                                                ByRef c1 As Vector3, ByRef c2 As Vector3)
            Dim d1 As Vector3 = q1 - p1
            Dim d2 As Vector3 = q2 - p2
            Dim r As Vector3 = p1 - p2
            Dim a As Double = Vector3Math.Dot(d1, d1)
            Dim e As Double = Vector3Math.Dot(d2, d2)
            Dim f As Double = Vector3Math.Dot(d2, r)
            Dim s As Double, t As Double

            Const eps As Double = 1.0E-12

            If a <= eps AndAlso e <= eps Then
                c1 = p1
                c2 = p2
                Return
            End If

            If a <= eps Then
                s = 0.0
                t = std.Min(1.0, std.Max(0.0, f / e))
            Else
                Dim c As Double = Vector3Math.Dot(d1, r)

                If e <= eps Then
                    t = 0.0
                    s = std.Min(1.0, std.Max(0.0, -c / a))
                Else
                    Dim b As Double = Vector3Math.Dot(d1, d2)
                    Dim denom As Double = a * e - b * b

                    s = If(denom <> 0.0, std.Min(1.0, std.Max(0.0, (b * f - c * e) / denom)), 0.0)
                    t = b * s + f

                    If t < 0.0 Then
                        t = 0.0
                        s = std.Min(1.0, std.Max(0.0, -c / a))
                    ElseIf t > e Then
                        t = 1.0
                        s = std.Min(1.0, std.Max(0.0, (b - c) / a))
                    End If
                End If
            End If

            c1 = p1 + d1 * s
            c2 = p2 + d2 * t
        End Sub

        Private Function Clamp(v As Double, lo As Double, hi As Double) As Double
            Return std.Min(hi, std.Max(lo, v))
        End Function

        Private Function Sign(v As Double) As Double
            Return If(v >= 0.0, 1.0, -1.0)
        End Function

        ''' <summary>组装流形，顺便从双方材质组合出摩擦与恢复系数。</summary>
        Private Function Build(a As RigidBody3D, b As RigidBody3D, normal As Vector3,
                               points As IEnumerable(Of Vector3),
                               penetrations As IEnumerable(Of Double)) As Manifold3D
            Dim m As New Manifold3D With {
                .A = a,
                .B = b,
                .Normal = Vector3Math.Normalize(normal),
                .Friction = PhysicsMaterial.CombineFriction(a.Material, b.Material),
                .Restitution = PhysicsMaterial.CombineRestitution(a.Material, b.Material)
            }
            Dim pts As Vector3() = points.ToArray()
            Dim pens As Double() = penetrations.ToArray()

            For i As Integer = 0 To pts.Length - 1
                m.Contacts.Add(New ContactPoint3D(pts(i), pens(i)))
            Next

            Return m
        End Function
    End Module
End Namespace
