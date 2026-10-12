Imports Microsoft.VisualBasic.Engine.Physics
Imports Microsoft.VisualBasic.Engine.Physics.Collision3D
Imports Microsoft.VisualBasic.Engine.Physics.Math3D
Imports std = System.Math

''' <summary>
''' 凸多面体碰撞能力的自测：质量属性、面绕向、盒-盒、盒-平面、球-凸、斜面滑动、
''' 堆叠静止与深穿透恢复。任何一项失败都会以非零退出码结束。
''' </summary>
Module Program

    Private Passed As Integer = 0
    Private Failed As Integer = 0

    Sub Main()
        Call RampDebug()
        Call MeshProperties()
        Call Winding()
        Call BoxOnGround()
        Call SphereOnGround()
        Call SphereOnStaticBox()
        Call BoxOnStaticBox()
        Call BoxOnRamp()
        Call SphereOnRamp()
        Call Stack()
        Call DeepPenetration()

        Console.WriteLine()
        Console.WriteLine($"assertions passed = {Passed}, failed = {Failed}")

        If Failed > 0 Then
            Environment.Exit(1)
        End If
    End Sub

    Private Sub RampDebug()
        Console.WriteLine("-- ramp debug: direct manifold probe")

        Dim ramp As ConvexMesh3D = ConvexMesh3D.CreateWedge(8, 3, 8)
        Dim wedge As RigidBody3D = PhysicsWorld3D.StaticConvex(ramp, New Vector3(0, ramp.PivotOffset.y, 0))

        wedge.Label = "wedge"

        For Each y As Double In {3.6, 3.3, 3.15, 3.1, 3.05, 3.0}
            Dim probe As RigidBody3D = PhysicsWorld3D.Box(1, 1, 1, 1.0)

            probe.Label = "box"
            probe.Position = New Vector3(3.0, y, 0)

            Dim m As Manifold3D = NarrowPhase3D.Collide(probe, wedge)

            If m Is Nothing Then
                Console.WriteLine($"  y={y:F2} -> no manifold")
            Else
                Dim pts As String = ""

                For Each c In m.Contacts
                    pts &= $" ({c.Position.x:F2},{c.Position.y:F2},{c.Position.z:F2})/pen={c.Penetration:F3}"
                Next

                Console.WriteLine($"  y={y:F2} -> A={m.A.Label} B={m.B.Label} n=({m.Normal.x:F3},{m.Normal.y:F3},{m.Normal.z:F3}){pts}")
            End If
        Next

        Console.WriteLine("-- ramp debug trace")

        Dim ramp As ConvexMesh3D = ConvexMesh3D.CreateWedge(8, 3, 8)
        Dim w As New PhysicsWorld3D()

        w.Add(PhysicsWorld3D.GroundPlane(0.0))
        w.Add(PhysicsWorld3D.StaticConvex(ramp, New Vector3(0, ramp.PivotOffset.y, 0)))

        Dim b As RigidBody3D = PhysicsWorld3D.Box(1, 1, 1, 1.0)

        b.Position = New Vector3(3.0, 6.0, 0)
        b.Material.Friction = 0.2
        w.Add(b)

        For i As Integer = 1 To 180
            w.Step(1.0 / 60.0)

            If i Mod 10 = 0 OrElse i < 40 Then
                Dim info As String = ""

                For Each m In w.Contacts
                    If m.A Is b OrElse m.B Is b Then
                        info &= $" n=({m.Normal.x:F2},{m.Normal.y:F2},{m.Normal.z:F2}) pen={m.Penetration:F3} pts={m.Contacts.Count}"
                    End If
                Next

                Console.WriteLine($"  t={i / 60.0:F2} x={b.Position.x:F3} y={b.Position.y:F3} vx={b.Velocity.x:F3} vy={b.Velocity.y:F3} | {info}")
            End If
        Next
    End Sub

    ' /********************************************************************************/
    '  断言工具
    ' /********************************************************************************/

    Private Sub Check(name As String, ok As Boolean, Optional detail As String = "")
        If ok Then
            Passed += 1
            Console.WriteLine($"  [ok]   {name}")
        Else
            Failed += 1
            Console.WriteLine($"  [FAIL] {name}  {detail}")
        End If
    End Sub

    Private Sub Near(name As String, actual As Double, expected As Double, tol As Double)
        Check(name, std.Abs(actual - expected) <= tol, $"actual={actual:F6} expected={expected:F6} tol={tol}")
    End Sub

    Private Sub Run(world As PhysicsWorld3D, seconds As Double)
        Dim steps As Integer = CInt(seconds * 60.0)

        For i As Integer = 1 To steps
            Call world.Step(1.0 / 60.0)
        Next
    End Sub

    Private Function Finite(v As Vector3) As Boolean
        Return Not Double.IsNaN(v.x) AndAlso Not Double.IsNaN(v.y) AndAlso Not Double.IsNaN(v.z) AndAlso
               Not Double.IsInfinity(v.x) AndAlso Not Double.IsInfinity(v.y) AndAlso Not Double.IsInfinity(v.z)
    End Function

    ''' <summary>刚体最低顶点的世界 y（用于判断"是否恰好贴在支撑面上"）。</summary>
    Private Function LowestY(b As RigidBody3D) As Double
        Dim mesh As ConvexMesh3D = ConvexNarrowPhase3D.ConvexOf(b)
        Dim vs As Vector3() = mesh.WorldVertices(b.Position, b.Orientation)
        Dim m As Double = Double.MaxValue

        For Each v As Vector3 In vs
            m = std.Min(m, v.y)
        Next

        Return m
    End Function

    Private Function Speed(b As RigidBody3D) As Double
        Return Vector3Math.Length(b.Velocity)
    End Function

    Private Function RandomOrientation(seed As Integer) As Quaternion
        Dim rnd As New Random(seed)
        Dim axis As Vector3 = Vector3Math.Normalize(New Vector3(rnd.NextDouble() * 2 - 1,
                                                                rnd.NextDouble() * 2 - 1,
                                                                rnd.NextDouble() * 2 - 1))
        Return Quaternion.FromAxisAngle(axis, rnd.NextDouble() * std.PI * 2)
    End Function

    ' /********************************************************************************/
    '  网格质量属性
    ' /********************************************************************************/

    Private Sub MeshProperties()
        Console.WriteLine("-- mesh mass properties")

        Dim box As ConvexMesh3D = ConvexMesh3D.CreateBox(2, 3, 4)

        Near("box volume = 24", box.Volume(), 24.0, 1.0E-6)

        Dim mine As Matrix3x3 = box.InertiaAboutCentroid(1.0)
        Dim analytic As Matrix3x3 = New BoxCollider3D(2, 3, 4).ComputeInertia(1.0)

        Near("box Ixx", mine.m11, analytic.m11, 1.0E-6)
        Near("box Iyy", mine.m22, analytic.m22, 1.0E-6)
        Near("box Izz", mine.m33, analytic.m33, 1.0E-6)
        Check("box inertia symmetric", std.Abs(mine.m12 - mine.m21) < 1.0E-9 AndAlso
                                       std.Abs(mine.m13 - mine.m31) < 1.0E-9 AndAlso
                                       std.Abs(mine.m23 - mine.m32) < 1.0E-9)

        ' 圆柱：V = πr²h（64 边形近似，误差 ~1e-3），Iyy = mr²/2，Ixx = m(3r²+h²)/12
        Dim cyl As ConvexMesh3D = ConvexMesh3D.CreateCylinder(1.0, 2.0, 64)

        Near("cylinder volume ≈ πr²h", cyl.Volume(), 2.0 * std.PI, 5.0E-3)

        Dim ic As Matrix3x3 = cyl.InertiaAboutCentroid(1.0)

        Near("cylinder Iyy = mr²/2", ic.m22, 0.5, 5.0E-3)
        Near("cylinder Ixx = m(3r²+h²)/12", ic.m11, (3.0 * 1.0 + 4.0) / 12.0, 1.0E-2)

        ' 圆锥：V = πr²h/3；质心在离底面 h/4 处 → 构造原点偏移应为 -h/4
        Dim cone As ConvexMesh3D = ConvexMesh3D.CreateCone(1.0, 2.0, 64)

        Near("cone volume ≈ πr²h/3", cone.Volume(), 2.0 * std.PI / 3.0, 5.0E-3)
        Near("cone pivot offset y = -h/4", cone.PivotOffset.y, -0.5, 1.0E-3)
        Check("cone inertia positive definite", cone.InertiaAboutCentroid(2.0).Determinant() > 0.0)

        Dim tetra As ConvexMesh3D = ConvexMesh3D.CreateTetrahedron(1.5)

        Check("tetra volume > 0", tetra.Volume() > 0.0, $"V={tetra.Volume()}")
        Check("tetra inertia positive definite", tetra.InertiaAboutCentroid(1.0).Determinant() > 0.0)

        Dim wedge As ConvexMesh3D = ConvexMesh3D.CreateWedge(6, 3, 4)

        Near("wedge volume = w·h·d/2", wedge.Volume(), 36.0, 1.0E-6)

        Dim ball As ConvexMesh3D = ConvexMesh3D.CreateSphereMesh(1.0, 12, 8)
        Dim maxErr As Double = 0.0

        For Each v As Vector3 In ball.Vertices
            maxErr = std.Max(maxErr, std.Abs(Vector3Math.Length(v) - 1.0))
        Next

        Check("sphere mesh vertices on radius", maxErr < 1.0E-9, $"maxErr={maxErr:E3}")
    End Sub

    Private Sub Winding()
        Console.WriteLine("-- face winding (all normals must point outward)")

        Dim meshes As ConvexMesh3D() = {
            ConvexMesh3D.CreateBox(1, 2, 3),
            ConvexMesh3D.CreateTetrahedron(1.4),
            ConvexMesh3D.CreateCone(1, 2, 12),
            ConvexMesh3D.CreateCylinder(1, 2, 12),
            ConvexMesh3D.CreateWedge(4, 2, 3),
            ConvexMesh3D.CreateSphereMesh(1, 12, 8)
        }
        Dim names As String() = {"box", "tetra", "cone", "cylinder", "wedge", "sphere"}

        For k As Integer = 0 To meshes.Length - 1
            Dim mesh As ConvexMesh3D = meshes(k)
            Dim bad As Integer = 0

            For i As Integer = 0 To mesh.Faces.Length - 1
                Dim mid As New Vector3(0, 0, 0)

                For Each idx As Integer In mesh.Faces(i)
                    mid = mid + mesh.Vertices(idx)
                Next

                mid = mid * (1.0 / mesh.Faces(i).Length)

                If Vector3Math.Dot(mesh.FaceNormals(i), mid) <= 0.0 Then bad += 1
            Next

            Check($"{names(k)}: all {mesh.Faces.Length} faces outward", bad = 0, $"inward={bad}")
        Next
    End Sub

    ' /********************************************************************************/
    '  动力学
    ' /********************************************************************************/

    Private Sub BoxOnGround()
        Console.WriteLine("-- box falls on ground plane (box-plane)")

        Dim w As New PhysicsWorld3D()

        w.Add(PhysicsWorld3D.GroundPlane(0.0))

        Dim b As RigidBody3D = PhysicsWorld3D.Box(1, 1, 1, 1.0)

        b.Position = New Vector3(0, 5, 0)
        b.Orientation = RandomOrientation(7)
        w.Add(b)

        Run(w, 5.0)

        Check("box: finite state", Finite(b.Position) AndAlso Finite(b.Velocity))
        Near("box: rests on ground (lowest vertex y=0)", LowestY(b), 0.0, 0.06)
        Check("box: at rest", Speed(b) < 0.05, $"|v|={Speed(b):F4}")
        Check("box: sleeping", b.IsSleeping)
    End Sub

    Private Sub SphereOnGround()
        Console.WriteLine("-- sphere falls on ground (sphere-plane)")

        Dim w As New PhysicsWorld3D()

        w.Add(PhysicsWorld3D.GroundPlane(0.0))

        Dim s As RigidBody3D = PhysicsWorld3D.Sphere(0.5, 1.0)

        s.Position = New Vector3(0, 5, 0)
        w.Add(s)

        Run(w, 5.0)

        Near("sphere: rests at y = r", s.Position.y, 0.5, 0.03)
        Check("sphere: at rest", Speed(s) < 0.05, $"|v|={Speed(s):F4}")
    End Sub

    Private Sub SphereOnStaticBox()
        Console.WriteLine("-- sphere on static box (sphere-box)")

        Dim w As New PhysicsWorld3D()

        w.Add(PhysicsWorld3D.GroundPlane(0.0))
        w.Add(PhysicsWorld3D.StaticBox(4, 1, 4, New Vector3(0, 0.5, 0)))

        Dim s As RigidBody3D = PhysicsWorld3D.Sphere(0.5, 1.0)

        s.Position = New Vector3(0, 5, 0)
        w.Add(s)

        Run(w, 5.0)

        Near("sphere: rests on box top (y = 1.5)", s.Position.y, 1.5, 0.03)
        Check("sphere: at rest on box", Speed(s) < 0.05, $"|v|={Speed(s):F4}")
    End Sub

    Private Sub BoxOnStaticBox()
        Console.WriteLine("-- box on static box (box-box SAT)")

        Dim w As New PhysicsWorld3D()

        w.Add(PhysicsWorld3D.GroundPlane(0.0))
        w.Add(PhysicsWorld3D.StaticBox(4, 1, 4, New Vector3(0, 0.5, 0)))

        Dim b As RigidBody3D = PhysicsWorld3D.Box(1, 1, 1, 1.0)

        b.Position = New Vector3(0, 2.0, 0)
        w.Add(b)

        Run(w, 5.0)

        Check("box-on-box: finite", Finite(b.Position))
        Near("box-on-box: rests at y = 1.5", b.Position.y, 1.5, 0.05)
        Check("box-on-box: at rest", Speed(b) < 0.05, $"|v|={Speed(b):F4}")
        Check("box-on-box: no tunneling", b.Position.y > 1.0, $"y={b.Position.y:F4}")
    End Sub

    Private Sub BoxOnRamp()
        Console.WriteLine("-- box on wedge ramp (box-convex + sliding)")

        Dim ramp As ConvexMesh3D = ConvexMesh3D.CreateWedge(8, 3, 8)
        Dim w As New PhysicsWorld3D()

        w.Add(PhysicsWorld3D.GroundPlane(0.0))

        Dim platform As RigidBody3D = PhysicsWorld3D.StaticConvex(ramp, New Vector3(0, ramp.PivotOffset.y, 0))

        platform.Material.Friction = 0.2
        w.Add(platform)

        Dim b As RigidBody3D = PhysicsWorld3D.Box(1, 1, 1, 1.0)

        b.Position = New Vector3(3.0, 6.0, 0)
        b.Material.Friction = 0.2
        w.Add(b)

        Dim minY As Double = Double.MaxValue

        For i As Integer = 1 To 180
            w.Step(1.0 / 60.0)
            minY = std.Min(minY, LowestY(b))
        Next

        Check("ramp: box finite", Finite(b.Position) AndAlso Finite(b.Velocity))
        Check("ramp: box did not tunnel through", minY > -0.15, $"minY={minY:F4}")
        Check("ramp: box slid down (-x)", b.Position.x < 2.6, $"x={b.Position.x:F3}")
        Check("ramp: box stayed above ground", b.Position.y > 0.0, $"y={b.Position.y:F3}")
    End Sub

    Private Sub SphereOnRamp()
        Console.WriteLine("-- sphere on wedge ramp (sphere-convex)")

        Dim ramp As ConvexMesh3D = ConvexMesh3D.CreateWedge(8, 3, 8)
        Dim w As New PhysicsWorld3D()

        w.Add(PhysicsWorld3D.GroundPlane(0.0))
        w.Add(PhysicsWorld3D.StaticConvex(ramp, New Vector3(0, ramp.PivotOffset.y, 0)))

        Dim s As RigidBody3D = PhysicsWorld3D.Sphere(0.5, 1.0)

        s.Position = New Vector3(2.5, 6.0, 0)
        s.Material.Friction = 0.1
        w.Add(s)

        Dim minY As Double = Double.MaxValue

        For i As Integer = 1 To 180
            w.Step(1.0 / 60.0)
            minY = std.Min(minY, s.Position.y)
        Next

        Check("sphere-ramp: finite", Finite(s.Position))
        Check("sphere-ramp: no tunneling", minY > 0.0, $"minY={minY:F4}")
        Check("sphere-ramp: rolled down (-x)", s.Position.x < 2.0, $"x={s.Position.x:F3}")
    End Sub

    Private Sub Stack()
        Console.WriteLine("-- three stacked boxes (warm starting)")

        Dim w As New PhysicsWorld3D()

        w.Add(PhysicsWorld3D.GroundPlane(0.0))

        Dim boxes(2) As RigidBody3D

        For i As Integer = 0 To 2
            boxes(i) = PhysicsWorld3D.Box(1, 1, 1, 1.0)
            boxes(i).Position = New Vector3(0, 0.5 + i * 1.02, 0)
            w.Add(boxes(i))
        Next

        Run(w, 6.0)

        For i As Integer = 0 To 2
            Check($"stack[{i}]: finite", Finite(boxes(i).Position))
            Near($"stack[{i}]: y = {0.5 + i}", boxes(i).Position.y, 0.5 + i, 0.08)
            Check($"stack[{i}]: at rest", Speed(boxes(i)) < 0.06, $"|v|={Speed(boxes(i)):F4}")
        Next
    End Sub

    Private Sub DeepPenetration()
        Console.WriteLine("-- deep penetration recovery")

        Dim w As New PhysicsWorld3D()

        w.Add(PhysicsWorld3D.GroundPlane(0.0))

        Dim b As RigidBody3D = PhysicsWorld3D.Box(1, 1, 1, 1.0)

        b.Position = New Vector3(0, 0.05, 0)
        w.Add(b)

        Run(w, 5.0)

        Check("deep: finite", Finite(b.Position) AndAlso Finite(b.Velocity))
        Near("deep: pushed out to y ≈ 0.5", b.Position.y, 0.5, 0.06)
        Check("deep: at rest", Speed(b) < 0.05, $"|v|={Speed(b):F4}")
    End Sub
End Module
