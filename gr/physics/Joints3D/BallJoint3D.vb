Imports System.Math
Imports Microsoft.VisualBasic.Imaging.Physics.Math3D
Imports std = System.Math

Namespace Joints3D

    ''' <summary>
    ''' 球窝关节：把两个刚体在各自的局部锚点处钉在一起，允许绕该点自由旋转（3 个转动自由度）。
    ''' 这是火柴人骨骼的主力关节（肩、髋、踝等）。
    ''' </summary>
    ''' <remarks>
    ''' 速度层用三个世界坐标轴依次做高斯-赛德尔迭代（与 2D <see cref="Joints.RevoluteJoint"/>
    ''' 逐轴求解的写法一致），位置层用 Baumgarte 软修正消除漂移。
    ''' </remarks>
    Public Class BallJoint3D : Implements IConstraint3D

        ''' <summary>第一个刚体。</summary>
        Public A As RigidBody3D
        ''' <summary>第二个刚体。</summary>
        Public B As RigidBody3D
        ''' <summary>物体 A 局部坐标系下的锚点（相对质心）。</summary>
        Public anchorA As Vector3
        ''' <summary>物体 B 局部坐标系下的锚点（相对质心）。</summary>
        Public anchorB As Vector3

        ''' <summary>
        ''' 位置修正强度（Baumgarte 系数）。
        ''' 修正速度 = <c>β/dt·误差</c>，子步很小时需要相应调小。
        ''' </summary>
        Public Property Baumgarte As Double = 0.15

        ''' <summary>
        ''' 位置修正速度的上限（m/s）。
        ''' </summary>
        ''' <remarks>
        ''' 起跳 / 着地瞬间关节会在一两帧内被拉开几厘米，<c>β/dt·误差</c> 会给出
        ''' 几十 m/s 的修正速度，把整个人弹飞。这里夹住上限，误差靠
        ''' <see cref="SolvePosition"/> 在多个子步里慢慢收敛。
        ''' </remarks>
        Public Property BiasSpeedLimit As Double = 2.0

        ''' <summary>
        ''' 锥角限位（弧度）。设为 ≥ π 表示不限位。
        ''' 限制 B 的 <see cref="limitAxisB"/> 相对 A 的 <see cref="limitAxisA"/> 的最大偏角，
        ''' 用于防止膝盖/肘关节反折。
        ''' </summary>
        Public Property MaxConeAngle As Double = std.PI
        ''' <summary>锥角限位在 A 局部系下的中心轴。</summary>
        Public limitAxisA As Vector3 = New Vector3(0, -1, 0)
        ''' <summary>锥角限位在 B 局部系下的中心轴。</summary>
        Public limitAxisB As Vector3 = New Vector3(0, 1, 0)

        Sub New(a As RigidBody3D, b As RigidBody3D, anchorA As Vector3, anchorB As Vector3)
            Me.A = a
            Me.B = b
            Me.anchorA = anchorA
            Me.anchorB = anchorB
        End Sub

        ''' <summary>世界坐标下的锚点（A 侧）。</summary>
        Public ReadOnly Property WorldAnchorA As Vector3
            Get
                Return A.Position + A.Orientation.Rotate(anchorA)
            End Get
        End Property

        ''' <summary>世界坐标下的锚点（B 侧）。</summary>
        Public ReadOnly Property WorldAnchorB As Vector3
            Get
                Return B.Position + B.Orientation.Rotate(anchorB)
            End Get
        End Property

        ''' <summary>两个锚点之间的偏差（应收敛到 0）。</summary>
        Public ReadOnly Property AnchorError As Vector3
            Get
                Return WorldAnchorB - WorldAnchorA
            End Get
        End Property

        Public Sub SolveVelocity(dt As Double) Implements IConstraint3D.SolveVelocity
            Dim rA As Vector3 = A.Orientation.Rotate(anchorA)
            Dim rB As Vector3 = B.Orientation.Rotate(anchorB)
            Dim pa As Vector3 = A.Position + rA
            Dim pb As Vector3 = B.Position + rB
            Dim err As Vector3 = pb - pa
            Dim v As Vector3 = (B.Velocity + Vector3Math.Cross(B.AngularVelocity, rB)) -
                               (A.Velocity + Vector3Math.Cross(A.AngularVelocity, rA))
            Dim invIA As Matrix3x3 = A.InvInertiaWorld()
            Dim invIB As Matrix3x3 = B.InvInertiaWorld()

            Call SolveAxis(New Vector3(1, 0, 0), rA, rB, v, err, invIA, invIB, dt)
            Call SolveAxis(New Vector3(0, 1, 0), rA, rB, v, err, invIA, invIB, dt)
            Call SolveAxis(New Vector3(0, 0, 1), rA, rB, v, err, invIA, invIB, dt)

            If MaxConeAngle < std.PI Then
                Call SolveConeLimit(invIA, invIB, dt)
            End If
        End Sub

        Private Sub SolveAxis(axis As Vector3, rA As Vector3, rB As Vector3,
                              v As Vector3, err As Vector3,
                              invIA As Matrix3x3, invIB As Matrix3x3, dt As Double)
            Dim rnA As Vector3 = Vector3Math.Cross(rA, axis)
            Dim rnB As Vector3 = Vector3Math.Cross(rB, axis)
            Dim k As Double = A.InvMass + B.InvMass +
                              Vector3Math.Dot(rnA, invIA.MultiplyLeft(rnA)) +
                              Vector3Math.Dot(rnB, invIB.MultiplyLeft(rnB))

            If k < 1.0E-12 Then Return

            Dim vn As Double = Vector3Math.Dot(v, axis)
            Dim bias As Double = ClampBias(Baumgarte / dt * Vector3Math.Dot(err, axis))
            Dim j As Double = (-vn - bias) * (1.0 / k)
            Dim P As Vector3 = axis * j

            Call A.ApplyImpulse(P * -1.0, rA)
            Call B.ApplyImpulse(P, rB)
        End Sub

        ''' <summary>
        ''' 锥角限位：把 B 的局部轴向拉回到以 A 的局部轴为中心、半角
        ''' <see cref="MaxConeAngle"/> 的锥内。
        ''' </summary>
        Private Sub SolveConeLimit(invIA As Matrix3x3, invIB As Matrix3x3, dt As Double)
            Dim axisA As Vector3 = A.Orientation.Rotate(limitAxisA)
            Dim axisB As Vector3 = B.Orientation.Rotate(limitAxisB)
            Dim cosAngle As Double = std.Min(1.0, std.Max(-1.0, Vector3Math.Dot(axisA, axisB)))
            Dim angle As Double = std.Acos(cosAngle)

            If angle <= MaxConeAngle Then
                Return
            End If

            Dim n As Vector3 = Vector3Math.Cross(axisB, axisA)

            If Vector3Math.LengthSquared(n) < 1.0E-12 Then
                Return
            End If

            Dim u As Vector3 = Vector3Math.Normalize(n)
            Dim excess As Double = angle - MaxConeAngle
            Dim wRel As Vector3 = B.AngularVelocity - A.AngularVelocity
            Dim c As Double = Vector3Math.Dot(wRel, u)
            Dim k As Double = Vector3Math.Dot(u, invIA.MultiplyLeft(u)) +
                              Vector3Math.Dot(u, invIB.MultiplyLeft(u))

            If k < 1.0E-12 Then Return

            ' 绕 u 正向转动会把 axisB 拉向 axisA
            Dim j As Double = (ClampBias(Baumgarte / dt * excess) - c) * (1.0 / k)
            Dim torque As Vector3 = u * j

            A.AngularVelocity = A.AngularVelocity - invIA.MultiplyLeft(torque)
            B.AngularVelocity = B.AngularVelocity + invIB.MultiplyLeft(torque)
        End Sub

        Public Sub SolvePosition(dt As Double) Implements IConstraint3D.SolvePosition
            Dim err As Vector3 = AnchorError
            Dim invSum As Double = A.InvMass + B.InvMass

            If invSum < 1.0E-12 Then
                Return
            End If

            Dim corr As Vector3 = err * 0.2

            A.Position = A.Position + corr * (A.InvMass / invSum)
            B.Position = B.Position - corr * (B.InvMass / invSum)
        End Sub
        ''' <summary>把 Baumgarte 修正速度夹在 <see cref="BiasSpeedLimit"/> 以内。</summary>
        Private Function ClampBias(bias As Double) As Double
            If BiasSpeedLimit <= 0.0 Then
                Return bias
            End If

            Return std.Min(BiasSpeedLimit, std.Max(-BiasSpeedLimit, bias))
        End Function
    End Class
End Namespace
