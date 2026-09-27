Imports System.Math
Imports Microsoft.VisualBasic.Imaging.Physics.Math3D
Imports Microsoft.VisualBasic.Imaging.Physics.Collision3D
Imports std = System.Math

''' <summary>
''' 3D 刚体：不会发生形变，受力后整体平动 + 绕质心转动。
''' 位置 <see cref="Position"/> 为质心世界坐标，姿态 <see cref="Orientation"/> 为四元数。
''' </summary>
''' <remarks>
''' 与 2D 的 <see cref="RigidBody"/> 一一对应：同样采用半隐式（辛）欧拉积分与指数阻尼，
''' 区别只是转动量由标量角推广为角速度向量，惯性由标量推广为 3x3 惯性张量。
''' </remarks>
Public Class RigidBody3D

    ''' <summary>唯一标识符。</summary>
    Public Property ID As String = Guid.NewGuid().ToString("N")

    ''' <summary>便于外部（如火柴人骨架）按名字索引的标签。</summary>
    Public Property Label As String

    ''' <summary>质心世界坐标。</summary>
    Public Position As Vector3 = Vector3.Zero

    ''' <summary>姿态（单位四元数）。局部基向量：X=right, Y=up, Z=forward。</summary>
    Public Orientation As Quaternion = Quaternion.Identity

    ''' <summary>线速度。</summary>
    Public Velocity As Vector3 = Vector3.Zero

    ''' <summary>角速度（世界系，弧度/秒）。</summary>
    Public AngularVelocity As Vector3 = Vector3.Zero

    ''' <summary>质量。0 或 <see cref="IsStatic"/> 表示不可推动。</summary>
    Public Mass As Double = 1.0

    ''' <summary>局部惯性张量（相对质心）。</summary>
    Public InertiaLocal As Matrix3x3 = Matrix3x3.Identity

    ''' <summary>线性阻尼（0 = 无）。</summary>
    Public LinearDamping As Double = 0.0

    ''' <summary>角阻尼（0 = 无）。</summary>
    Public AngularDamping As Double = 0.05

    ''' <summary>
    ''' 最大线速度。0 = 不限制。作为简化连续碰撞检测(CCD)的防护，
    ''' 限制单步位移不超过物体尺寸，避免高速物体"穿隧"。
    ''' </summary>
    Public MaxSpeed As Double = 0.0

    ''' <summary>最大角速度（弧度/秒）。0 = 不限制。</summary>
    Public MaxAngularSpeed As Double = 100.0

    ''' <summary>物理材质（摩擦 / 恢复）。</summary>
    Public Material As PhysicsMaterial = New PhysicsMaterial()

    ''' <summary>碰撞几何体，几何中心即质心。</summary>
    Public Shape As Collider3D

    ''' <summary>静态物体不参与动力学积分，仅作为碰撞边界。</summary>
    Public IsStatic As Boolean = False

    ''' <summary>自身所属的碰撞分组（位标志）。</summary>
    Public Property CollisionGroup As Integer = 1

    ''' <summary>允许与之碰撞的分组掩码；0 表示不与任何东西碰撞。</summary>
    Public Property CollisionMask As Integer = -1

    Private ForceAccum As Vector3 = Vector3.Zero
    Private TorqueAccum As Vector3 = Vector3.Zero

    ''' <summary>是否产生过接触（供上层判断"脚是否踩到地面"）。</summary>
    Public Property HadContact As Boolean = False

    Sub New(shape As Collider3D, Optional mass As Double = 1.0, Optional material As PhysicsMaterial = Nothing)
        Me.Shape = shape
        Me.Mass = mass

        If material IsNot Nothing Then Me.Material = material
        If mass > 0 Then Me.InertiaLocal = shape.ComputeInertia(mass)

        If Me.InertiaLocal.Determinant() < 1.0E-12 Then
            ' 退化惯量（如平面）：退化为不转动的球体惯量，避免求逆得到 NaN
            Me.InertiaLocal = Matrix3x3.Identity
        End If
    End Sub

    ''' <summary>设为静态物体（无限质量 / 惯量）。</summary>
    Public Sub SetStatic()
        IsStatic = True
    End Sub

    ''' <summary>设为运动学物体：不受力，但仍以给定速度运动（本库中按静态处理积分）。</summary>
    Public Sub SetKinematic()
        IsStatic = True
    End Sub

    ' /********************************************************************************/
    '  质量属性
    ' /********************************************************************************/

    Public ReadOnly Property InvMass As Double
        Get
            Return If(IsStatic OrElse Mass <= 0, 0.0, 1.0 / Mass)
        End Get
    End Property

    ''' <summary>局部惯性张量的逆。</summary>
    Public ReadOnly Property InvInertiaLocal As Matrix3x3
        Get
            If IsStatic Then
                Return Matrix3x3.Zero
            End If

            Return InertiaLocal.Inverse()
        End Get
    End Property

    ''' <summary>
    ''' 世界系逆惯性张量 <c>R · I⁻¹ · Rᵀ</c>。每步姿态变化后都要重新取值。
    ''' </summary>
    Public Function InvInertiaWorld() As Matrix3x3
        If IsStatic Then
            Return Matrix3x3.Zero
        End If

        Dim m As Matrix3x3 = Orientation.ToMatrix3x3()

        ' 行向量约定下 R = Mᵀ，故 invI_world = Mᵀ · invI_local · M
        Return m.Transpose().Multiply(InertiaLocal.Inverse()).Multiply(m)
    End Function

    ''' <summary>
    ''' 世界系惯性张量 <c>R · I · Rᵀ</c>。把期望角加速度换算成扭矩时使用。
    ''' </summary>
    Public Function InertiaWorld() As Matrix3x3
        If IsStatic Then
            Return Matrix3x3.Zero
        End If

        Dim m As Matrix3x3 = Orientation.ToMatrix3x3()

        Return m.Transpose().Multiply(InertiaLocal).Multiply(m)
    End Function

    ''' <summary>把期望角加速度换算成扭矩并施加（内部乘上世界系惯性张量）。</summary>
    Public Sub ApplyAngularAcceleration(alpha As Vector3)
        If IsStatic Then Return
        Call ApplyTorque(InertiaWorld().MultiplyLeft(alpha))
    End Sub

    ' /********************************************************************************/
    '  坐标变换
    ' /********************************************************************************/

    ''' <summary>局部坐标 → 世界坐标。</summary>
    Public Function ToWorld(localPoint As Vector3) As Vector3
        Return Position + Orientation.Rotate(localPoint)
    End Function

    ''' <summary>世界坐标 → 局部坐标。</summary>
    Public Function ToLocal(worldPoint As Vector3) As Vector3
        Return Orientation.Unrotate(worldPoint - Position)
    End Function

    ''' <summary>局部 X 轴（right）在世界系下的方向。</summary>
    Public ReadOnly Property Right As Vector3
        Get
            Return Orientation.Rotate(New Vector3(1, 0, 0))
        End Get
    End Property

    ''' <summary>局部 Y 轴（up）在世界系下的方向。</summary>
    Public ReadOnly Property Up As Vector3
        Get
            Return Orientation.Rotate(New Vector3(0, 1, 0))
        End Get
    End Property

    ''' <summary>局部 Z 轴（forward）在世界系下的方向。</summary>
    Public ReadOnly Property Forward As Vector3
        Get
            Return Orientation.Rotate(New Vector3(0, 0, 1))
        End Get
    End Property

    ' /********************************************************************************/
    '  施力
    ' /********************************************************************************/

    ''' <summary>施加一个集中于质心的力。</summary>
    Public Sub ApplyForce(f As Vector3)
        If IsStatic Then Return
        ForceAccum = ForceAccum + f
    End Sub

    ''' <summary>在世界坐标点 <paramref name="worldPoint"/> 施加力，会同时产生扭矩。</summary>
    Public Sub ApplyForceAtPoint(worldPoint As Vector3, f As Vector3)
        If IsStatic Then Return
        ForceAccum = ForceAccum + f
        TorqueAccum = TorqueAccum + Vector3Math.Cross(worldPoint - Position, f)
    End Sub

    ''' <summary>施加纯扭矩（世界系）。</summary>
    Public Sub ApplyTorque(t As Vector3)
        If IsStatic Then Return
        TorqueAccum = TorqueAccum + t
    End Sub

    ''' <summary>
    ''' 在相对质心的接触点 <paramref name="contactVector"/> 处施加冲量（瞬时改变速度）。
    ''' </summary>
    Public Sub ApplyImpulse(impulse As Vector3, contactVector As Vector3)
        If IsStatic Then Return

        Velocity = Velocity + impulse * InvMass
        AngularVelocity = AngularVelocity + InvInertiaWorld().MultiplyLeft(Vector3Math.Cross(contactVector, impulse))
    End Sub

    ' /********************************************************************************/
    '  积分
    ' /********************************************************************************/

    ''' <summary>
    ''' 半隐式（辛）欧拉：由力更新速度。应在 <see cref="IntegratePosition"/> 之前调用。
    ''' </summary>
    Public Sub IntegrateVelocity(dt As Double)
        If IsStatic Then Return

        ' v += (F/m)·dt
        Velocity = Velocity + (ForceAccum * InvMass) * dt
        ' ω += (I⁻¹·τ)·dt
        AngularVelocity = AngularVelocity + InvInertiaWorld().MultiplyLeft(TorqueAccum) * dt

        ' 指数阻尼，数值稳定
        Velocity = Velocity * (1.0 / (1.0 + LinearDamping * dt))
        AngularVelocity = AngularVelocity * (1.0 / (1.0 + AngularDamping * dt))

        If MaxSpeed > 0.0 Then
            Dim sp As Double = Vector3Math.Length(Velocity)
            If sp > MaxSpeed Then Velocity = Velocity * (MaxSpeed / sp)
        End If

        If MaxAngularSpeed > 0.0 Then
            Dim av As Double = Vector3Math.Length(AngularVelocity)
            If av > MaxAngularSpeed Then AngularVelocity = AngularVelocity * (MaxAngularSpeed / av)
        End If
    End Sub

    ''' <summary>由速度更新位置与姿态（四元数积分后重新归一化）。</summary>
    Public Sub IntegratePosition(dt As Double)
        If IsStatic Then Return

        Position = Position + Velocity * dt
        Orientation = Orientation.Integrate(AngularVelocity, dt).Normalize()
    End Sub

    ''' <summary>清除力 / 扭矩累加器（每个子步结束后调用）。</summary>
    Public Sub ClearForces()
        ForceAccum = Vector3.Zero
        TorqueAccum = Vector3.Zero
    End Sub

    ''' <summary>当前 AABB。</summary>
    Public Function GetAABB() As AABB3D
        Return Shape.GetAABB(Position, Orientation)
    End Function

    ''' <summary>动能，用于调试与收敛观测。</summary>
    Public ReadOnly Property KineticEnergy As Double
        Get
            Dim lin As Double = 0.5 * Mass * Vector3Math.LengthSquared(Velocity)
            Dim w As Vector3 = InvInertiaWorld().MultiplyLeft(AngularVelocity)
            Dim rot As Double = 0.5 * Vector3Math.Dot(AngularVelocity, w)

            Return lin + rot
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"{If(Label, ID)} @ {Position}"
    End Function
End Class
