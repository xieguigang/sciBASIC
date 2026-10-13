Imports Microsoft.VisualBasic.Engine.Physics.Collision3D
Imports Microsoft.VisualBasic.Engine.Physics.ForceFields3D
Imports Microsoft.VisualBasic.Engine.Physics.Joints3D
Imports Microsoft.VisualBasic.Engine.Physics.Math3D

''' <summary>
''' 3D 物理世界。以 <see cref="PhysicsWorld3D.Step"/> 固定步长驱动，内部按子步执行：
''' 施加马达扭矩 → 重力与力场 → 由力积分速度 → 宽相位 → 窄相位 →
''' 顺序冲量求解（接触 + 关节）→ 由速度积分位置 → 关节位置修正 → 清除累加器。
''' 可直接接入游戏主循环（每帧调用一次 <see cref="PhysicsWorld3D.Step"/> 传入真实 dt）。
''' </summary>
''' <remarks>
''' 坐标系约定：Y 轴向上，默认重力 (0, -9.81, 0)。
''' </remarks>
Public Class PhysicsWorld3D

    ''' <summary>所有刚体。</summary>
    Public Bodies As New List(Of RigidBody3D)

    ''' <summary>所有几何约束 / 关节。</summary>
    Public Constraints As New List(Of IConstraint3D)

    ''' <summary>所有角度马达（每子步开始时自动施力）。</summary>
    Public Motors As New List(Of AngularMotor3D)

    ''' <summary>所有力场。</summary>
    Public ForceFields As New List(Of ForceField3D)

    ''' <summary>全局重力。</summary>
    Public Gravity As Vector3 = New Vector3(0, -9.81, 0)

    ''' <summary>速度 / 位置约束求解迭代次数。</summary>
    Public Iterations As Integer = 16

    ''' <summary>固定时间步长（秒）。</summary>
    Public FixedDt As Double = 1.0 / 60.0

    ''' <summary>每个固定步长内的子步数（越多越稳定、越慢）。</summary>
    Public Substeps As Integer = 4

    ''' <summary>接触位置修正强度（Baumgarte）。</summary>
    Public Property Baumgarte As Double = 0.2

    ''' <summary>允许的穿透容差，避免接触抖动。</summary>
    Public Property Slop As Double = 0.01

    ''' <summary>
    ''' 跨帧热启动：把上一子步同一接触点上累积的法向/摩擦冲量继承下来并立即施加。
    ''' 堆叠与静止接触的收敛速度完全依赖它，关闭后物体会持续抖动。
    ''' </summary>
    Public Property WarmStarting As Boolean = True

    ''' <summary>热启动时匹配上一帧接触点的位置容差（米）。</summary>
    Public Property WarmStartRadius As Double = 0.08

    ''' <summary>是否允许刚体休眠（低速持续一段时间后冻结，既防抖又省 CPU）。</summary>
    Public Property AllowSleep As Boolean = True

    ''' <summary>进入休眠的线速度阈值（m/s）。</summary>
    Public Property SleepLinearTolerance As Double = 0.08
    ''' <summary>进入休眠的角速度阈值（rad/s）。</summary>
    Public Property SleepAngularTolerance As Double = 0.15
    ''' <summary>低速需持续的时间（秒）。</summary>
    Public Property SleepTimeRequired As Double = 0.6

    ''' <summary>
    ''' 子步钩子：每个子步都会被调用一次，用于施加"持续存在"的外部力
    ''' （平衡反射、步态驱动等）。见 <see cref="IStepHook"/>。
    ''' </summary>
    Public ReadOnly Property Hooks As New List(Of IStepHook)()

    ''' <summary>最近一次求解得到的接触流形（供上层查询"脚是否踩到地面"）。</summary>
    Public ReadOnly Property Contacts As New List(Of Manifold3D)()

    Private accumulator As Double = 0.0

    ''' <summary>上一子步的接触流形，按刚体对索引，供热启动继承冲量。</summary>
    Private ReadOnly contactCache As New Dictionary(Of String, Manifold3D)()

    ''' <summary>添加刚体，并唤醒它附近休眠的物体（例如新物体落在已入睡的堆上）。</summary>
    Public Sub Add(b As RigidBody3D)
        Bodies.Add(b)
        Call WakeNear(b)
    End Sub

    ''' <summary>移除刚体（同时移除引用它的角度马达）。</summary>
    Public Function Remove(b As RigidBody3D) As Boolean
        If b Is Nothing Then Return False

        Dim ok As Boolean = Bodies.Remove(b)

        If ok Then
            Call RemoveMotorsOf(b)
            Call contactCache.Clear()
        End If

        Return ok
    End Function

    ''' <summary>按 <see cref="RigidBody3D.ID"/> 或 <see cref="RigidBody3D.Label"/> 移除刚体。</summary>
    Public Function Remove(idOrLabel As String) As Boolean
        Dim b As RigidBody3D = Find(idOrLabel)

        If b Is Nothing Then Return False

        Return Remove(b)
    End Function

    ''' <summary>按 <see cref="RigidBody3D.ID"/> 或 <see cref="RigidBody3D.Label"/> 查找刚体。</summary>
    Public Function Find(idOrLabel As String) As RigidBody3D
        If String.IsNullOrEmpty(idOrLabel) Then Return Nothing

        For Each b In Bodies
            If b.ID = idOrLabel OrElse b.Label = idOrLabel Then Return b
        Next

        Return Nothing
    End Function

    ''' <summary>清空世界中的全部动态内容（刚体、约束、马达、力场、接触缓存）。</summary>
    Public Sub Clear()
        Call Bodies.Clear()
        Call Constraints.Clear()
        Call Motors.Clear()
        Call ForceFields.Clear()
        Call Contacts.Clear()
        Call contactCache.Clear()

        accumulator = 0.0
    End Sub

    Private Sub RemoveMotorsOf(b As RigidBody3D)
        For i As Integer = Motors.Count - 1 To 0 Step -1
            Dim m As AngularMotor3D = Motors(i)

            If m.A Is b OrElse m.B Is b Then Call Motors.RemoveAt(i)
        Next
    End Sub

    Private Sub WakeNear(b As RigidBody3D)
        Dim box As AABB3D = b.GetAABB()

        For Each other As RigidBody3D In Bodies
            If other Is b OrElse other.IsStatic OrElse Not other.IsSleeping Then Continue For

            If other.GetAABB().Overlaps(box) Then Call other.Wake()
        Next
    End Sub

    ''' <summary>添加约束 / 关节。</summary>
    Public Sub Add(j As IConstraint3D)
        Constraints.Add(j)
    End Sub

    ''' <summary>添加角度马达。</summary>
    Public Sub Add(m As AngularMotor3D)
        Motors.Add(m)
    End Sub

    ''' <summary>添加力场。</summary>
    Public Sub Add(f As ForceField3D)
        ForceFields.Add(f)
    End Sub

    ''' <summary>注册一个子步钩子。</summary>
    Public Sub Add(h As IStepHook)
        Hooks.Add(h)
    End Sub

    ''' <summary>
    ''' 固定步长推进。传入真实帧间隔 <paramref name="frameDt"/>（秒），
    ''' 内部用累加器以 <see cref="FixedDt"/> 为单位执行若干子步，避免可变步长不稳定。
    ''' </summary>
    Public Sub [Step](frameDt As Double)
        accumulator += frameDt

        Dim steps As Integer = 0

        While accumulator >= FixedDt AndAlso steps < 4
            Call StepInternal(FixedDt)

            accumulator -= FixedDt
            steps += 1
        End While

        ' 防止"死亡螺旋"：连续滞后时丢弃积压时间
        If steps >= 4 Then accumulator = 0.0
    End Sub

    Private Sub StepInternal(dt As Double)
        For Each b In Bodies
            b.HadContact = False
        Next

        Dim sub_dt As Double = dt / Substeps

        For s As Integer = 1 To Substeps
            Call StepSub(sub_dt)
        Next

        If AllowSleep Then
            Call UpdateSleep(dt)
        End If
    End Sub

    Private Sub StepSub(dt As Double)
        ' 1. 角度马达（必须在力场之前，否则会被本子步末尾的清力操作吞掉）
        For Each m In Motors
            Call m.ApplyTorques(dt)
        Next

        ' 1.5 子步钩子：外部控制器补充持续力
        For Each h In Hooks
            Call h.BeforeSubstep(dt)
        Next

        ' 2. 全局重力 + 力场
        For Each b In Bodies
            If b.IsStatic Then Continue For
            Call b.ApplyForce(Gravity * b.Mass)
        Next
        For Each ff In ForceFields
            Call ff.Apply(Bodies)
        Next

        ' 3. 由力积分速度
        For Each b In Bodies
            Call b.IntegrateVelocity(dt)
        Next

        ' 4. 宽相位
        Dim pairs = BroadPhase3D.ComputePairs(Bodies)

        ' 5. 窄相位 → 流形
        Contacts.Clear()

        For Each pr In pairs
            Dim m = NarrowPhase3D.Collide(pr.A, pr.B)

            If m IsNot Nothing AndAlso m.HasContacts AndAlso m.Penetration > 0 Then
                ' 运动物体会撞醒与之接触的休眠物体：必须在热启动之前唤醒，
                ' 否则继承下来的冲量施加到一个不参与积分的物体上没有意义
                If pr.A.IsSleeping AndAlso Not pr.B.IsSleeping Then Call MaybeWake(pr.A, pr.B)
                If pr.B.IsSleeping AndAlso Not pr.A.IsSleeping Then Call MaybeWake(pr.B, pr.A)

                Call m.InitSolver(dt, Baumgarte, Slop)

                If WarmStarting Then
                    Call ApplyWarmStart(m)
                End If

                Contacts.Add(m)

                pr.A.HadContact = True
                pr.B.HadContact = True
            End If
        Next

        If WarmStarting Then
            Call contactCache.Clear()

            For Each m In Contacts
                contactCache(PairKey(m.A, m.B)) = m
            Next
        End If

        ' 6. 速度约束求解（接触 + 关节），多次迭代
        For i As Integer = 1 To Iterations
            For Each m In Contacts
                Call ContactSolver3D.Solve(m, dt)
            Next
            For Each j In Constraints
                Call j.SolveVelocity(dt)
            Next
        Next

        ' 7. 由速度积分位置
        For Each b In Bodies
            Call b.IntegratePosition(dt)
        Next

        ' 8. 关节位置修正（消除漂移）
        For Each j In Constraints
            Call j.SolvePosition(dt)
        Next

        ' 9. 清除力 / 扭矩累加器
        For Each b In Bodies
            Call b.ClearForces()
        Next
    End Sub

    ' ---------------- 热启动与休眠 ----------------

    Private Shared Function PairKey(a As RigidBody3D, b As RigidBody3D) As String
        Return a.ID & "|" & b.ID
    End Function

    ''' <summary>
    ''' 继承上一子步同一接触点上的法向累积冲量并立即施加到速度上。
    ''' 只继承位置足够接近、且法向没有大幅翻转的接触点。
    ''' </summary>
    ''' <remarks>
    ''' 这里刻意<b>不</b>继承摩擦冲量：摩擦是"有方向记忆"的冲量，一旦继承，
    ''' 求解器在切向速度已接近 0 时几乎不再修正，于是上一子步的摩擦方向会被
    ''' 原样反复施加，物体沿该方向被持续加速（实测球体落在斜面上会被"推"上坡）。
    ''' 法向冲量没有这个问题：过量时求解器会在同一子步内用负冲量把它扣回去。
    ''' </remarks>
    Private Sub ApplyWarmStart(m As Manifold3D)
        Dim prev As Manifold3D = Nothing

        If Not contactCache.TryGetValue(PairKey(m.A, m.B), prev) Then Return
        If Vector3Math.Dot(prev.Normal, m.Normal) < 0.9 Then Return

        Dim r2 As Double = WarmStartRadius * WarmStartRadius

        For Each c In m.Contacts
            Dim best As ContactPoint3D = Nothing
            Dim bestD2 As Double = r2

            For Each pc In prev.Contacts
                Dim d2 As Double = Vector3Math.DistanceSquared(pc.Position, c.Position)

                If d2 < bestD2 Then
                    bestD2 = d2
                    best = pc
                End If
            Next

            If best Is Nothing Then Continue For

            c.NormalImpulse = best.NormalImpulse

            Dim P As Vector3 = m.Normal * c.NormalImpulse

            Call m.A.ApplyImpulse(P * -1.0, c.rA)
            Call m.B.ApplyImpulse(P, c.rB)
        Next
    End Sub

    Private Sub MaybeWake(sleeper As RigidBody3D, other As RigidBody3D)
        Dim v As Double = Vector3Math.Length(other.Velocity)
        Dim w As Double = Vector3Math.Length(other.AngularVelocity)

        If v > SleepLinearTolerance OrElse w > SleepAngularTolerance Then
            Call sleeper.Wake()
        End If
    End Sub

    Private Sub UpdateSleep(dt As Double)
        For Each b In Bodies
            If b.IsStatic Then Continue For

            Dim v As Double = Vector3Math.Length(b.Velocity)
            Dim w As Double = Vector3Math.Length(b.AngularVelocity)

            If v < SleepLinearTolerance AndAlso w < SleepAngularTolerance Then
                b.SleepTimer += dt

                If b.SleepTimer >= SleepTimeRequired AndAlso Not b.IsSleeping Then
                    b.IsSleeping = True
                    b.Velocity = New Vector3(0, 0, 0)
                    b.AngularVelocity = New Vector3(0, 0, 0)
                End If
            Else
                b.SleepTimer = 0.0
                b.IsSleeping = False
            End If
        Next
    End Sub

    ' ---------------- 工厂方法 ----------------

    ''' <summary>创建一个长方体刚体。</summary>
    Public Shared Function Box(width As Double, height As Double, depth As Double,
                               mass As Double, Optional material As PhysicsMaterial = Nothing) As RigidBody3D
        Return New RigidBody3D(New BoxCollider3D(width, height, depth), mass, material)
    End Function

    ''' <summary>创建一个球体刚体。</summary>
    Public Shared Function Sphere(radius As Double, mass As Double,
                                  Optional material As PhysicsMaterial = Nothing) As RigidBody3D
        Return New RigidBody3D(New SphereCollider3D(radius), mass, material)
    End Function

    ''' <summary>创建一个胶囊刚体（局部 Y 轴为轴向）。</summary>
    Public Shared Function Capsule(radius As Double, height As Double, mass As Double,
                                   Optional material As PhysicsMaterial = Nothing) As RigidBody3D
        Return New RigidBody3D(New CapsuleCollider3D(radius, height), mass, material)
    End Function

    ''' <summary>创建一个静态无限平面，平面方程 <c>dot(normal, p) = constant</c>。</summary>
    Public Shared Function StaticPlane(normal As Vector3, constant As Double,
                                       Optional material As PhysicsMaterial = Nothing) As RigidBody3D
        Dim body As RigidBody3D = New RigidBody3D(New PlaneCollider3D(normal, constant), 0.0, material)

        Call body.SetStatic()

        Return body
    End Function

    ''' <summary>
    ''' 创建一个凸多面体刚体（四面体、圆锥、圆柱、楔形斜坡、任意凸壳）。
    ''' 惯量由网格的四面体分解解析积分得到。
    ''' </summary>
    Public Shared Function Convex(mesh As ConvexMesh3D, mass As Double,
                                  Optional material As PhysicsMaterial = Nothing) As RigidBody3D
        Return New RigidBody3D(New ConvexPolyhedronCollider3D(mesh), mass, material)
    End Function

    ''' <summary>创建一个静态凸多面体（关卡几何：斜坡、楔形平台、异形障碍）。</summary>
    Public Shared Function StaticConvex(mesh As ConvexMesh3D, position As Vector3,
                                        Optional material As PhysicsMaterial = Nothing) As RigidBody3D
        Dim body As RigidBody3D = Convex(mesh, 1.0, material)

        body.Position = position
        Call body.SetStatic()

        Return body
    End Function

    ''' <summary>创建一个静态长方体（关卡几何：台阶、障碍、墙）。</summary>
    Public Shared Function StaticBox(width As Double, height As Double, depth As Double,
                                     position As Vector3,
                                     Optional material As PhysicsMaterial = Nothing) As RigidBody3D
        Dim body As RigidBody3D = Box(width, height, depth, 1.0, material)

        body.Position = position
        Call body.SetStatic()

        Return body
    End Function

    ''' <summary>水平地面：<c>y = height</c>，法向朝上。</summary>
    Public Shared Function GroundPlane(height As Double, Optional material As PhysicsMaterial = Nothing) As RigidBody3D
        Return StaticPlane(New Vector3(0, 1, 0), height, material)
    End Function

    ''' <summary>把所有刚体的速度清零（用于复位）。</summary>
    Public Sub ClearVelocities()
        For Each b In Bodies
            b.Velocity = New Vector3(0, 0, 0)
            b.AngularVelocity = New Vector3(0, 0, 0)
            Call b.ClearForces()
        Next
    End Sub
End Class
