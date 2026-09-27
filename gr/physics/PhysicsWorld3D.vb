Imports Microsoft.VisualBasic.Imaging.Physics.Collision3D
Imports Microsoft.VisualBasic.Imaging.Physics.ForceFields3D
Imports Microsoft.VisualBasic.Imaging.Physics.Joints3D
Imports Microsoft.VisualBasic.Imaging.Physics.Math3D

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

    ''' <summary>最近一次求解得到的接触流形（供上层查询"脚是否踩到地面"）。</summary>
    Public ReadOnly Property Contacts As New List(Of Manifold3D)()

    Private accumulator As Double = 0.0

    ''' <summary>添加刚体。</summary>
    Public Sub Add(b As RigidBody3D)
        Bodies.Add(b)
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
    End Sub

    Private Sub StepSub(dt As Double)
        ' 1. 角度马达（必须在力场之前，否则会被本子步末尾的清力操作吞掉）
        For Each m In Motors
            Call m.ApplyTorques(dt)
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
                Call m.InitSolver(dt, Baumgarte, Slop)
                Contacts.Add(m)

                pr.A.HadContact = True
                pr.B.HadContact = True
            End If
        Next

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
