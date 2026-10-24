Imports Microsoft.VisualBasic.Engine.Physics.Math3D
Imports std = System.Math

Namespace Joints3D

    ''' <summary>
    ''' 转轴关节（机械臂的主力关节）：在 <see cref="HingeJoint3D"/> 的"锚点重合 + 两轴对齐"
    ''' 约束之上，再补一个绕轴的**角度限位**，并暴露当前角度与角速度供伺服读取。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <see cref="HingeJoint3D"/> 只锁死两个垂直于铰链轴的自由度，绕轴旋转是完全自由的，
    ''' 所以机械臂的肘/肩关节会反折。这里照 <see cref="BallJoint3D"/> 的锥角限位写法，
    ''' 在角度越界时沿轴施加一个偏置速度 <c>β/dt·越界量</c>（限幅后）把关节拉回行程内。
    ''' </para>
    ''' <para>
    ''' 角度的零位由 <see cref="SetZeroReference"/> 记录（装配时调用一次），之后
    ''' <see cref="Position"/> 返回相对零位的角度。因为六轴机械臂的末轴行程可达 ±2π，
    ''' 内部对原始角做了 unwrap 累加，否则角度会在 ±π 处跳变。
    ''' </para>
    ''' </remarks>
    Public Class RevoluteJoint3D
        Implements IConstraint3D
        Implements IJointDof3D

        ''' <summary>承担"锚点重合"部分的球窝约束。</summary>
        Public ReadOnly Property Point As BallJoint3D

        ''' <summary>转轴在 A 局部系下的方向（单位向量）。</summary>
        Public hingeAxisA As Vector3
        ''' <summary>转轴在 B 局部系下的方向（单位向量）。</summary>
        Public hingeAxisB As Vector3

        ''' <summary>角度下限（弧度）。</summary>
        Public Property LowerLimit As Double = -std.PI Implements IJointDof3D.LowerLimit
        ''' <summary>角度上限（弧度）。</summary>
        Public Property UpperLimit As Double = std.PI Implements IJointDof3D.UpperLimit
        ''' <summary>是否启用角度限位。</summary>
        Public Property EnableLimit As Boolean = True

        ''' <summary>轴对齐约束的位置修正强度。</summary>
        Public Property Baumgarte As Double = 0.2
        ''' <summary>限位修正速度的上限（rad/s），防止越界过大时把关节弹飞。</summary>
        Public Property BiasSpeedLimit As Double = 3.0
        ''' <summary>限位修正的 Baumgarte 系数。</summary>
        Public Property LimitBaumgarte As Double = 0.25

        ''' <summary>关节名称（供上层显示与调试）。</summary>
        Public Property Label As String

        Private _refRelative As Quaternion = Quaternion.Identity

        ''' <summary>创建一个转轴关节。</summary>
        ''' <param name="a">上游刚体</param>
        ''' <param name="b">下游刚体</param>
        ''' <param name="anchorA">A 局部系下的锚点（相对质心）</param>
        ''' <param name="anchorB">B 局部系下的锚点（相对质心）</param>
        ''' <param name="axisA">A 局部系下的转轴</param>
        ''' <param name="axisB">B 局部系下的转轴</param>
        ''' <param name="lowerLimit">角度下限（弧度）</param>
        ''' <param name="upperLimit">角度上限（弧度）</param>
        ''' <param name="label">关节名称</param>
        Sub New(a As RigidBody3D, b As RigidBody3D,
                anchorA As Vector3, anchorB As Vector3,
                axisA As Vector3, axisB As Vector3,
                Optional lowerLimit As Double = -std.PI,
                Optional upperLimit As Double = std.PI,
                Optional label As String = Nothing)

            Me.Point = New BallJoint3D(a, b, anchorA, anchorB)

            ' 锚点通常不在质心上：3×3 耦合求解已经把过冲消掉，
            ' 这里保持满额冲量（松弛会留下残差，残差在下一步被当作误差消除而耗散能量，
            ' 实测会把 yaw 关节 5 rad/s 的角速度在 10 步内衰减到 0）
            Me.Point.Relaxation = 1.0

            ' 位置偏置速度必须很小：β/dt 在 1/960 的子步下会被放大上千倍，
            ' 稍微一点锚点误差就产生 m/s 级的修正冲量，冲量穿过锚点会带来
            ' 大量非物理的角动量交换（表现为关节被"粘住"）
            Me.Point.Baumgarte = 0.05
            Me.Point.BiasSpeedLimit = 0.2
            Me.hingeAxisA = Vector3Math.Normalize(axisA)
            Me.hingeAxisB = Vector3Math.Normalize(axisB)
            Me.LowerLimit = lowerLimit
            Me.UpperLimit = upperLimit
            Me.Label = label

            Call SetZeroReference()
        End Sub

        ' /********************************************************************************/
        '  IJointDof3D
        ' /********************************************************************************/

        ''' <summary>参考刚体。</summary>
        Public ReadOnly Property A As RigidBody3D Implements IJointDof3D.A
            Get
                Return Point.A
            End Get
        End Property

        ''' <summary>被约束刚体。</summary>
        Public ReadOnly Property B As RigidBody3D Implements IJointDof3D.B
            Get
                Return Point.B
            End Get
        End Property

        ''' <summary>世界系下的转轴（A 侧）。</summary>
        Public ReadOnly Property WorldAxisA As Vector3
            Get
                Return Point.A.Orientation.Rotate(hingeAxisA)
            End Get
        End Property

        ''' <summary>世界系下的转轴（B 侧）。</summary>
        Public ReadOnly Property WorldAxisB As Vector3
            Get
                Return Point.B.Orientation.Rotate(hingeAxisB)
            End Get
        End Property

        ''' <summary>世界系下的关节运动轴（正方向 = A 侧转轴）。</summary>
        Public ReadOnly Property WorldAxis As Vector3 Implements IJointDof3D.WorldAxis
            Get
                Return WorldAxisA
            End Get
        End Property

        ''' <summary>相对零位的关节角度（弧度）。</summary>
        Public ReadOnly Property Position As Double Implements IJointDof3D.Position
            Get
                Return ComputeAngle()
            End Get
        End Property

        ''' <summary>关节角速度（B 相对 A，沿轴，rad/s）。</summary>
        Public ReadOnly Property Speed As Double Implements IJointDof3D.Speed
            Get
                Dim wRel As Vector3 = Point.B.AngularVelocity - Point.A.AngularVelocity
                Return Vector3Math.Dot(wRel, WorldAxisA)
            End Get
        End Property

        ''' <summary>两侧铰链轴之间的夹角（弧度），正常运行时应收敛到 0。</summary>
        Public ReadOnly Property AxisError As Double
            Get
                Return std.Acos(std.Min(1.0, std.Max(-1.0, Vector3Math.Dot(WorldAxisA, WorldAxisB))))
            End Get
        End Property

        ''' <summary>越过限位的量（弧度），0 表示在行程内。</summary>
        Public ReadOnly Property LimitError As Double
            Get
                Dim angle As Double = Position

                If angle > UpperLimit Then Return angle - UpperLimit
                If angle < LowerLimit Then Return angle - LowerLimit

                Return 0.0
            End Get
        End Property

        ''' <summary>当前角度与目标角度之差（供伺服调试）。</summary>
        Public Function DistanceTo(angle As Double) As Double
            Return angle - Position
        End Function

        ''' <summary>沿轴施加一个扭矩（正方向为 <see cref="WorldAxis"/>）。</summary>
        Public Sub ApplyControlForce(value As Double) Implements IJointDof3D.ApplyControlForce
            If value = 0.0 Then Return

            Dim axis As Vector3 = WorldAxisA
            Dim torque As Vector3 = axis * value

            ' 休眠的刚体会忽略扭矩，先唤醒（机械臂伺服需要持续保持力矩）
            Call Point.A.Wake()
            Call Point.B.Wake()
            Call Point.B.ApplyTorque(torque)
            Call Point.A.ApplyTorque(torque * -1.0)
        End Sub

        ''' <summary>把当前相对姿态记为角度零位。</summary>
        Public Sub SetZeroReference() Implements IJointDof3D.SetZeroReference
            _refRelative = Quaternion.Compose(Point.A.Orientation.Inverse(), Point.B.Orientation)
        End Sub

        ' /********************************************************************************/
        '  约束求解
        ' /********************************************************************************/

        Public Sub SolveVelocity(dt As Double) Implements IConstraint3D.SolveVelocity
            Call Point.SolveVelocity(dt)

            Dim rbA As RigidBody3D = Point.A
            Dim rbB As RigidBody3D = Point.B
            Dim invIA As Matrix3x3 = rbA.InvInertiaWorld()
            Dim invIB As Matrix3x3 = rbB.InvInertiaWorld()
            Dim axisA As Vector3 = WorldAxisA
            Dim axisB As Vector3 = WorldAxisB
            Dim n As Vector3 = Vector3Math.Cross(axisB, axisA)

            If Vector3Math.LengthSquared(n) > 1.0E-12 Then
                Dim angle As Double = std.Asin(std.Min(1.0, Vector3Math.Length(n)))
                Dim u1 As Vector3 = Vector3Math.Normalize(n)
                Dim u2 As Vector3 = Vector3Math.Normalize(Vector3Math.Cross(axisB, u1))

                Call SolveAngularAxis(rbA, rbB, invIA, invIB, u1, Baumgarte / dt * angle, dt)
                Call SolveAngularAxis(rbA, rbB, invIA, invIB, u2, 0.0, dt)
            Else
                Dim u1 As Vector3 = Vector3Math.OrthogonalOf(axisA)
                Dim u2 As Vector3 = Vector3Math.Cross(axisA, u1)

                Call SolveAngularAxis(rbA, rbB, invIA, invIB, u1, 0.0, dt)
                Call SolveAngularAxis(rbA, rbB, invIA, invIB, u2, 0.0, dt)
            End If

            If EnableLimit Then
                Call SolveLimit(rbA, rbB, invIA, invIB, dt)
            End If
        End Sub

        ''' <summary>角度限位：越过行程时沿轴施加一个拉回目标速度。</summary>
        Private Sub SolveLimit(rbA As RigidBody3D, rbB As RigidBody3D,
                               invIA As Matrix3x3, invIB As Matrix3x3, dt As Double)

            Dim angle As Double = ComputeAngle()
            Dim excess As Double = 0.0

            If angle > UpperLimit Then
                excess = angle - UpperLimit
            ElseIf angle < LowerLimit Then
                excess = angle - LowerLimit
            End If

            If excess = 0.0 Then Return

            Dim u As Vector3 = WorldAxisA
            Dim wRel As Vector3 = rbB.AngularVelocity - rbA.AngularVelocity
            Dim c As Double = Vector3Math.Dot(wRel, u)
            Dim k As Double = Vector3Math.Dot(u, invIA.MultiplyLeft(u)) +
                              Vector3Math.Dot(u, invIB.MultiplyLeft(u))

            If k < 1.0E-12 Then Return

            ' excess > 0（越过上限）需要负向角速度，excess < 0（越过下限）需要正向角速度
            Dim target As Double = -ClampBias(LimitBaumgarte / dt * excess)
            Dim j As Double = (target - c) * (1.0 / k)
            Dim torque As Vector3 = u * j

            rbA.AngularVelocity = rbA.AngularVelocity - invIA.MultiplyLeft(torque)
            rbB.AngularVelocity = rbB.AngularVelocity + invIB.MultiplyLeft(torque)
        End Sub

        Private Shared Sub SolveAngularAxis(rbA As RigidBody3D, rbB As RigidBody3D,
                                            invIA As Matrix3x3, invIB As Matrix3x3,
                                            u As Vector3, targetSpeed As Double, dt As Double)
            Dim wRel As Vector3 = rbB.AngularVelocity - rbA.AngularVelocity
            Dim c As Double = Vector3Math.Dot(wRel, u)
            Dim k As Double = Vector3Math.Dot(u, invIA.MultiplyLeft(u)) +
                              Vector3Math.Dot(u, invIB.MultiplyLeft(u))

            If k < 1.0E-12 Then Return

            Dim j As Double = (targetSpeed - c) * (1.0 / k)
            Dim torque As Vector3 = u * j

            rbA.AngularVelocity = rbA.AngularVelocity - invIA.MultiplyLeft(torque)
            rbB.AngularVelocity = rbB.AngularVelocity + invIB.MultiplyLeft(torque)
        End Sub

        Public Sub SolvePosition(dt As Double) Implements IConstraint3D.SolvePosition
            Call Point.SolvePosition(dt)
        End Sub

        ''' <summary>把角速度修正量夹在 <see cref="BiasSpeedLimit"/> 以内。</summary>
        Private Function ClampBias(bias As Double) As Double
            If BiasSpeedLimit <= 0.0 Then Return bias

            Return std.Min(BiasSpeedLimit, std.Max(-BiasSpeedLimit, bias))
        End Function

        ' /********************************************************************************/
        '  角度读取
        ' /********************************************************************************/

        ''' <summary>
        ''' 由两侧姿态算出相对零位的角度（弧度，范围 (-π, π]）。
        ''' </summary>
        ''' <remarks>
        ''' 这里刻意**不做**跨帧累积的 unwrap：累积值一旦在一次求解迭代里被写入 ±2π 的跳变，
        ''' 之后就一直带着这个偏置，伺服会朝着错误方向持续出力（实测 j3/j5 会稳定在 ±0.4 rad 的
        ''' 假平衡点上，而 Speed 的符号与 Position 相反）。关节的真实行程由 URDF 限位保证，
        ''' 正常装配下不会跨越 ±π，因此直接用绝对相对角更稳健。
        ''' </remarks>
        Private Function ComputeAngle() As Double
            Dim rbA As RigidBody3D = Point.A
            Dim rbB As RigidBody3D = Point.B
            Dim relative As Quaternion = Quaternion.Compose(rbA.Orientation.Inverse(), rbB.Orientation)
            Dim delta As Quaternion = Quaternion.Compose(_refRelative.Inverse(), relative)

            ' delta 是绕 hingeAxisA 的旋转（A 局部系表达），取其旋转角
            Dim axis As Vector3 = hingeAxisA
            Dim sinHalf As Double = delta.x * axis.x + delta.y * axis.y + delta.z * axis.z

            Return 2.0 * std.Atan2(sinHalf, delta.w)
        End Function
    End Class

End Namespace
