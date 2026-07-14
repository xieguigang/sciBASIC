' Copyright (c) 2018 GPL3 Licensed
' 弹簧关节：用胡克定律 + 阻尼模拟弹性连接。

Imports Microsoft.VisualBasic.Imaging.Physics.RigidBody

Namespace Joints

    ''' <summary>
    ''' 弹簧关节：在两个刚体的锚点之间施加胡克弹力（与形变成正比）和阻尼力
    ''' （与相对速度成正比），模拟弹性连接（如绳索、弹簧、悬挂）。
    ''' </summary>
    Public Class SpringJoint : Implements IConstraint
        Public A As RigidBody
        Public B As RigidBody
        Public anchorA As Vector2
        Public anchorB As Vector2

        ''' <summary>弹簧刚度 k（胡克定律 F = -k·Δx）</summary>
        Public stiffness As Double = 50.0

        ''' <summary>阻尼系数 c（与相对速度相反，抑制振荡）</summary>
        Public damping As Double = 2.0

        ''' <summary>自然长度（无受力时的间距）</summary>
        Public restLength As Double = 0.0

        Sub New(a As RigidBody, b As RigidBody, anchorA As Vector2, anchorB As Vector2,
                Optional stiffness As Double = 50.0, Optional damping As Double = 2.0, Optional restLength As Double = 0.0)
            Me.A = a
            Me.B = b
            Me.anchorA = anchorA
            Me.anchorB = anchorB
            Me.stiffness = stiffness
            Me.damping = damping
            Me.restLength = restLength
        End Sub

        Public Sub SolveVelocity(dt As Double) Implements IConstraint.SolveVelocity
            Dim rA = Rotate(anchorA, A.Rotation)
            Dim rB = Rotate(anchorB, B.Rotation)
            Dim pa = A.Position + rA
            Dim pb = B.Position + rB
            Dim delta = pb - pa
            Dim dist = Length(delta)
            If dist < 1.0e-9 Then Return

            Dim dir = delta / dist
            Dim relVel = Dot(B.Velocity + Cross(B.AngularVelocity, rB) - A.Velocity - Cross(A.AngularVelocity, rA), dir)
            Dim f = -stiffness * (dist - restLength) - damping * relVel

            ' 力转化为本步冲量
            Dim impulse = dir * (f * dt)
            A.ApplyImpulse(-impulse, rA)
            B.ApplyImpulse(impulse, rB)
        End Sub

        Public Sub SolvePosition(dt As Double) Implements IConstraint.SolvePosition
            ' 弹簧是柔性约束，不做硬位置修正
        End Sub
    End Class
End Namespace
