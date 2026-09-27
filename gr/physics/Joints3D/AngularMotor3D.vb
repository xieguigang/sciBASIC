Imports System.Math
Imports Microsoft.VisualBasic.Imaging.Physics.Math3D
Imports std = System.Math

Namespace Joints3D

    ''' <summary>
    ''' 角度马达：以 PD 控制把 B 相对 A 的朝向驱动到 <see cref="TargetRelative"/>。
    ''' 这是"通过调节关节姿态驱动火柴人运动"的核心执行器。
    ''' </summary>
    ''' <remarks>
    ''' 控制律：<c>τ = Kp · axis · angle − Kd · (ωB − ωA)</c>，其中 <c>axis/angle</c>
    ''' 取自误差四元数 <c>q_target_world ⊗ q_B⁻¹</c> 的最短轴角分解。
    ''' 扭矩以等大反向的方式施加到 A 与 B，满足动量守恒。
    '''
    ''' 注意：马达必须在世界推进之前施力（<see cref="PhysicsWorld3D"/> 会在每个子步开头
    ''' 自动调用 <see cref="ApplyTorques"/>），因为子步结束时会清除力累加器。
    ''' </remarks>
    Public Class AngularMotor3D : Implements IConstraint3D

        ''' <summary>父刚体（靠近躯干的一侧）。</summary>
        Public A As RigidBody3D
        ''' <summary>子刚体（远离躯干的一侧）。</summary>
        Public B As RigidBody3D

        ''' <summary>B 相对 A 的目标朝向。</summary>
        Public Property TargetRelative As Quaternion = Quaternion.Identity

        ''' <summary>比例增益 Kp（越大越"硬"）。</summary>
        Public Property Stiffness As Double = 300.0

        ''' <summary>微分增益 Kd（抑制振荡）。</summary>
        Public Property Damping As Double = 20.0

        ''' <summary>扭矩上限；0 表示不限幅。</summary>
        Public Property MaxTorque As Double = 0.0

        ''' <summary>关闭后马达不产生任何扭矩（关节变成完全被动）。</summary>
        Public Property Enabled As Boolean = True

        ''' <summary>便于外部按名字索引的标签（如 "KneeL"）。</summary>
        Public Property Label As String

        Sub New(a As RigidBody3D, b As RigidBody3D, Optional target As Quaternion? = Nothing)
            Me.A = a
            Me.B = b
            Me.TargetRelative = If(target.HasValue, target.Value, Quaternion.Identity)
        End Sub

        ''' <summary>B 当前相对 A 的朝向。</summary>
        Public ReadOnly Property CurrentRelative As Quaternion
            Get
                Return Quaternion.Compose(A.Orientation.Conjugate(), B.Orientation)
            End Get
        End Property

        ''' <summary>当前姿态与目标姿态之间的夹角（弧度）。</summary>
        Public ReadOnly Property AngleError As Double
            Get
                Return Quaternion.AngleBetween(CurrentRelative, TargetRelative)
            End Get
        End Property

        ''' <summary>
        ''' 计算并施加本步的马达扭矩。应在 <c>PhysicsWorld3D.Step</c> 之前调用。
        ''' </summary>
        Public Sub ApplyTorques(dt As Double)
            If Not Enabled OrElse A Is Nothing OrElse B Is Nothing Then
                Return
            End If

            If A.IsStatic AndAlso B.IsStatic Then
                Return
            End If

            Dim targetWorld As Quaternion = Quaternion.Compose(A.Orientation, TargetRelative)
            Dim err As Quaternion = Quaternion.Compose(targetWorld, B.Orientation.Conjugate())
            Dim axis As Vector3 = Nothing
            Dim angle As Double = 0.0

            Call err.ToAxisAngle(axis, angle)

            Dim wRel As Vector3 = B.AngularVelocity - A.AngularVelocity
            Dim torque As Vector3 = axis * (angle * Stiffness) - wRel * Damping

            If MaxTorque > 0.0 Then
                Dim mag As Double = Vector3Math.Length(torque)
                If mag > MaxTorque Then
                    torque = torque * (MaxTorque / mag)
                End If
            End If

            ' 等大反向，保持角动量守恒
            Call A.ApplyTorque(torque * -1.0)
            Call B.ApplyTorque(torque)
        End Sub

        ''' <summary>由欧拉角（弧度，按 X→Y→Z 内旋顺序）设置目标相对姿态。</summary>
        Public Sub SetTargetEuler(pitchX As Double, yawY As Double, rollZ As Double)
            TargetRelative = Quaternion.Compose(
                Quaternion.Compose(Quaternion.FromAxisAngle(New Vector3(1, 0, 0), pitchX),
                                   Quaternion.FromAxisAngle(New Vector3(0, 1, 0), yawY)),
                Quaternion.FromAxisAngle(New Vector3(0, 0, 1), rollZ))
        End Sub

        ''' <summary>绕给定局部轴设置目标相对姿态（最常用的单轴关节目标）。</summary>
        Public Sub SetTargetAxisAngle(axis As Vector3, angle As Double)
            TargetRelative = Quaternion.FromAxisAngle(axis, angle)
        End Sub

        ' 马达是"力发生器"而不是几何约束：速度层与位置层都不参与求解。
        Public Sub SolveVelocity(dt As Double) Implements IConstraint3D.SolveVelocity
            ' 扭矩由 ApplyTorques 在每个子步开始前施加
        End Sub

        Public Sub SolvePosition(dt As Double) Implements IConstraint3D.SolvePosition
            ' 马达不做位置修正
        End Sub
    End Class
End Namespace
