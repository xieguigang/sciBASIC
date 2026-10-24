Imports Microsoft.VisualBasic.Engine.Physics.Math3D
Imports std = System.Math

Namespace Joints3D

    ''' <summary>
    ''' 移动副（棱柱关节）：两个刚体之间只保留沿指定轴的一个平移自由度，
    ''' 其余两个方向的平移与三个方向的旋转全部锁死，并可选轴向行程限位。
    ''' </summary>
    ''' <remarks>
    ''' 夹爪手指就是典型的移动副：手指只能沿 link6 的局部轴开合，
    ''' 由 <see cref="JointServo3D"/> 的力模式给出受限的夹持力（<c>MaxForce</c>），
    ''' 因此"夹持力不足 → 库仑摩擦上限不足 → 积木滑落"是求解器自然产生的行为，
    ''' 不需要任何抓取后硬绑定的取巧。
    ''' </remarks>
    Public Class PrismaticJoint3D
        Implements IConstraint3D
        Implements IJointDof3D

        ''' <summary>参考刚体（上游连杆）。</summary>
        Public ReadOnly Property A As RigidBody3D Implements IJointDof3D.A
            Get
                Return _a
            End Get
        End Property

        ''' <summary>被约束刚体（手指）。</summary>
        Public ReadOnly Property B As RigidBody3D Implements IJointDof3D.B
            Get
                Return _b
            End Get
        End Property

        ''' <summary>A 局部系下的锚点（相对质心）。</summary>
        Public Property AnchorA As Vector3
        ''' <summary>B 局部系下的锚点（相对质心）。</summary>
        Public Property AnchorB As Vector3
        ''' <summary>A 局部系下的滑动轴（单位向量）。</summary>
        Public Property LocalAxis As Vector3

        ''' <summary>行程下限（米）。</summary>
        Public Property LowerLimit As Double = -0.05 Implements IJointDof3D.LowerLimit
        ''' <summary>行程上限（米）。</summary>
        Public Property UpperLimit As Double = 0.05 Implements IJointDof3D.UpperLimit
        ''' <summary>是否启用行程限位。</summary>
        Public Property EnableLimit As Boolean = True

        ''' <summary>位置修正强度。</summary>
        Public Property Baumgarte As Double = 0.2
        ''' <summary>限位修正的 Baumgarte 系数。</summary>
        Public Property LimitBaumgarte As Double = 0.3
        ''' <summary>修正速度上限（m/s 或 rad/s）。</summary>
        Public Property BiasSpeedLimit As Double = 2.0

        ''' <summary>关节名称（供上层显示与调试）。</summary>
        Public Property Label As String

        Private ReadOnly _a As RigidBody3D
        Private ReadOnly _b As RigidBody3D
        Private _refRelative As Quaternion = Quaternion.Identity
        Private _refExtension As Double = 0.0

        ''' <summary>创建一个移动副。</summary>
        Sub New(a As RigidBody3D, b As RigidBody3D,
                anchorA As Vector3, anchorB As Vector3,
                localAxis As Vector3,
                Optional lowerLimit As Double = -0.05,
                Optional upperLimit As Double = 0.05,
                Optional label As String = Nothing)

            _a = a
            _b = b
            Me.AnchorA = anchorA
            Me.AnchorB = anchorB
            Me.LocalAxis = Vector3Math.Normalize(localAxis)
            Me.LowerLimit = lowerLimit
            Me.UpperLimit = upperLimit
            Me.Label = label

            Call SetZeroReference()
        End Sub

        ''' <summary>世界系下的滑动轴。</summary>
        Public ReadOnly Property WorldAxis As Vector3 Implements IJointDof3D.WorldAxis
            Get
                Return _a.Orientation.Rotate(Vector3Math.Normalize(LocalAxis))
            End Get
        End Property

        ''' <summary>相对零位的伸长量（米）。</summary>
        Public ReadOnly Property Position As Double Implements IJointDof3D.Position
            Get
                Return CurrentExtension()
            End Get
        End Property

        ''' <summary>轴向相对速度（m/s）。</summary>
        Public ReadOnly Property Speed As Double Implements IJointDof3D.Speed
            Get
                Dim rA As Vector3 = _a.Orientation.Rotate(AnchorA)
                Dim rB As Vector3 = _b.Orientation.Rotate(AnchorB)
                Dim v As Vector3 = RelativeVelocity(rA, rB)
                Return Vector3Math.Dot(v, WorldAxis)
            End Get
        End Property

        ''' <summary>两个锚点之间的偏差（应收敛到 0）。</summary>
        Public ReadOnly Property AnchorError As Vector3
            Get
                Return (_b.Position + _b.Orientation.Rotate(AnchorB)) -
                       (_a.Position + _a.Orientation.Rotate(AnchorA))
            End Get
        End Property

        ''' <summary>越过行程的量（米），0 表示在行程内。</summary>
        Public ReadOnly Property LimitError As Double
            Get
                Dim ext As Double = CurrentExtension()

                If ext > UpperLimit Then Return ext - UpperLimit
                If ext < LowerLimit Then Return ext - LowerLimit

                Return 0.0
            End Get
        End Property

        ''' <summary>沿轴施加一个力（正方向为 <see cref="WorldAxis"/>）。</summary>
        Public Sub ApplyControlForce(value As Double) Implements IJointDof3D.ApplyControlForce
            If value = 0.0 Then Return

            Dim axis As Vector3 = WorldAxis
            Dim force As Vector3 = axis * value
            Dim rA As Vector3 = _a.Orientation.Rotate(AnchorA)
            Dim rB As Vector3 = _b.Orientation.Rotate(AnchorB)

            ' 休眠体会忽略外力
            Call _a.Wake()
            Call _b.Wake()

            ' 力施加在锚点上，这样即使锚点不在质心也能正确产生力矩
            Call _b.ApplyForceAtPoint(_b.Position + rB, force)
            Call _a.ApplyForceAtPoint(_a.Position + rA, force * -1.0)
        End Sub

        ''' <summary>把当前位姿记为行程零位。</summary>
        Public Sub SetZeroReference() Implements IJointDof3D.SetZeroReference
            _refRelative = Quaternion.Compose(_a.Orientation.Inverse(), _b.Orientation)
            _refExtension = Vector3Math.Dot(AnchorError, WorldAxis)
        End Sub

        ' /********************************************************************************/
        '  约束求解
        ' /********************************************************************************/

        Public Sub SolveVelocity(dt As Double) Implements IConstraint3D.SolveVelocity
            Dim invIA As Matrix3x3 = _a.InvInertiaWorld()
            Dim invIB As Matrix3x3 = _b.InvInertiaWorld()
            Dim axis As Vector3 = WorldAxis
            Dim err As Vector3 = AnchorError

            ' 1. 锁死两个垂直于轴的平移方向
            Dim u1 As Vector3 = Vector3Math.OrthogonalOf(axis)
            Dim u2 As Vector3 = Vector3Math.Normalize(Vector3Math.Cross(axis, u1))

            Call SolveLinearAxis(u1, -ClampBias(Baumgarte / dt * Vector3Math.Dot(err, u1)), invIA, invIB)
            Call SolveLinearAxis(u2, -ClampBias(Baumgarte / dt * Vector3Math.Dot(err, u2)), invIA, invIB)

            ' 2. 锁死全部三个转动自由度
            Call SolveAngularLock(dt, invIA, invIB)

            ' 3. 轴向行程限位
            If EnableLimit Then
                Dim ext As Double = CurrentExtension()
                Dim excess As Double = 0.0

                If ext > UpperLimit Then
                    excess = ext - UpperLimit
                ElseIf ext < LowerLimit Then
                    excess = ext - LowerLimit
                End If

                If excess <> 0.0 Then
                    Call SolveLinearAxis(axis, -ClampBias(LimitBaumgarte / dt * excess), invIA, invIB)
                End If
            End If
        End Sub

        Public Sub SolvePosition(dt As Double) Implements IConstraint3D.SolvePosition
            Dim err As Vector3 = AnchorError
            Dim axis As Vector3 = WorldAxis
            Dim invSum As Double = _a.InvMass + _b.InvMass

            If invSum < 1.0E-12 Then Return

            ' 只修正垂直于轴的分量：轴向位移是自由度的合法运动
            Dim corr As Vector3 = err - axis * Vector3Math.Dot(err, axis)
            corr = corr * 0.2

            _a.Position = _a.Position + corr * (_a.InvMass / invSum)
            _b.Position = _b.Position - corr * (_b.InvMass / invSum)

            ' 行程限位的位置修正：把越界部分推回行程内
            If EnableLimit Then
                Dim ext As Double = CurrentExtension()
                Dim excess As Double = 0.0

                If ext > UpperLimit Then
                    excess = ext - UpperLimit
                ElseIf ext < LowerLimit Then
                    excess = ext - LowerLimit
                End If

                If excess <> 0.0 Then
                    Dim push As Vector3 = axis * (excess * 0.2)
                    _a.Position = _a.Position + push * (_a.InvMass / invSum)
                    _b.Position = _b.Position - push * (_b.InvMass / invSum)
                End If
            End If
        End Sub

        ''' <summary>沿任意世界轴求解一个线性约束（目标相对速度由外部给出）。</summary>
        Private Sub SolveLinearAxis(u As Vector3, targetSpeed As Double,
                                    invIA As Matrix3x3, invIB As Matrix3x3)
            Dim rA As Vector3 = _a.Orientation.Rotate(AnchorA)
            Dim rB As Vector3 = _b.Orientation.Rotate(AnchorB)
            Dim rnA As Vector3 = Vector3Math.Cross(rA, u)
            Dim rnB As Vector3 = Vector3Math.Cross(rB, u)
            Dim k As Double = _a.InvMass + _b.InvMass +
                              Vector3Math.Dot(rnA, invIA.MultiplyLeft(rnA)) +
                              Vector3Math.Dot(rnB, invIB.MultiplyLeft(rnB))

            If k < 1.0E-12 Then Return

            Dim v As Vector3 = RelativeVelocity(rA, rB)
            Dim c As Double = Vector3Math.Dot(v, u)
            Dim j As Double = (targetSpeed - c) * (1.0 / k)
            Dim P As Vector3 = u * j

            Call _a.ApplyImpulse(P * -1.0, rA)
            Call _b.ApplyImpulse(P, rB)
        End Sub

        ''' <summary>把相对姿态拉回零位（三个转动自由度全部锁死）。</summary>
        Private Sub SolveAngularLock(dt As Double, invIA As Matrix3x3, invIB As Matrix3x3)
            Dim relative As Quaternion = Quaternion.Compose(_a.Orientation.Inverse(), _b.Orientation)
            Dim delta As Quaternion = Quaternion.Compose(_refRelative.Inverse(), relative)

            Dim axisV As Vector3 = New Vector3(0, 1, 0)
            Dim angle As Double = 0.0

            Call delta.ToAxisAngle(axisV, angle)

            If angle < 1.0E-9 Then Return

            ' delta 在 A 局部系中表达，转到世界系后逐轴修正
            Dim wErr As Vector3 = _a.Orientation.Rotate(axisV * angle)
            Dim wRel As Vector3 = _b.AngularVelocity - _a.AngularVelocity
            Dim basis As Vector3() = {New Vector3(1, 0, 0), New Vector3(0, 1, 0), New Vector3(0, 0, 1)}

            For Each u As Vector3 In basis
                Dim c As Double = Vector3Math.Dot(wRel, u)
                Dim k As Double = Vector3Math.Dot(u, invIA.MultiplyLeft(u)) +
                                  Vector3Math.Dot(u, invIB.MultiplyLeft(u))

                If k < 1.0E-12 Then Continue For

                Dim target As Double = -ClampBias(Baumgarte / dt * Vector3Math.Dot(wErr, u))
                Dim j As Double = (target - c) * (1.0 / k)
                Dim torque As Vector3 = u * j

                _a.AngularVelocity = _a.AngularVelocity - invIA.MultiplyLeft(torque)
                _b.AngularVelocity = _b.AngularVelocity + invIB.MultiplyLeft(torque)
            Next
        End Sub

        Private Function RelativeVelocity(rA As Vector3, rB As Vector3) As Vector3
            Return (_b.Velocity + Vector3Math.Cross(_b.AngularVelocity, rB)) -
                   (_a.Velocity + Vector3Math.Cross(_a.AngularVelocity, rA))
        End Function

        Private Function CurrentExtension() As Double
            Return Vector3Math.Dot(AnchorError, WorldAxis) - _refExtension
        End Function

        Private Function ClampBias(bias As Double) As Double
            If BiasSpeedLimit <= 0.0 Then Return bias

            Return std.Min(BiasSpeedLimit, std.Max(-BiasSpeedLimit, bias))
        End Function
    End Class

End Namespace
