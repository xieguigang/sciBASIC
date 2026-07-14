' Copyright (c) 2018 GPL3 Licensed
' 滑块关节：允许沿单一轴平移，约束垂直方向的移动与相对旋转。

Imports System.Math
Imports Microsoft.VisualBasic.Imaging.Physics
Imports std = System.Math

Namespace Joints

    ''' <summary>
    ''' 滑块（棱柱）关节：允许两个刚体沿给定世界轴 <see cref="axis"/> 自由滑动，
    ''' 但约束垂直于该轴的相对平移以及相对旋转。
    ''' </summary>
    Public Class PrismaticJoint : Implements IConstraint
        Public A As RigidBody
        Public B As RigidBody

        ''' <summary>允许滑动的世界方向（单位向量）</summary>
        Public axis As Vector2

        Private jAcc As Double = 0.0
        Private angleAcc As Double = 0.0

        Sub New(a As RigidBody, b As RigidBody, axisWorld As Vector2)
            Me.A = a
            Me.B = b
            Me.axis = Normalize(axisWorld)
        End Sub

        Public Sub SolveVelocity(dt As Double) Implements IConstraint.SolveVelocity
            ' 约束垂直方向（ slider 轴线的法向）上的相对速度
            Dim normal = Perpendicular(axis)
            Dim rv = B.Velocity - A.Velocity
            Dim vn = Dot(rv, normal)
            Dim k = A.InvMass + B.InvMass
            If k < 1.0e-12 Then Return

            Dim j = -vn / k
            jAcc += j
            Dim P = normal * j
            A.ApplyImpulse(-P, Vector2.Zero)
            B.ApplyImpulse(P, Vector2.Zero)

            ' 角度约束
            Dim relAng = B.AngularVelocity - A.AngularVelocity
            Dim invSum = A.InvInertia + B.InvInertia
            If invSum < 1.0e-12 Then Return
            Dim ja = -relAng / invSum
            angleAcc += ja
            A.AngularVelocity -= A.InvInertia * ja
            B.AngularVelocity += B.InvInertia * ja
        End Sub

        Public Sub SolvePosition(dt As Double) Implements IConstraint.SolvePosition
            Dim normal = Perpendicular(axis)
            Dim err = Dot(B.Position - A.Position, normal) * 0.2
            Dim corr = normal * err
            A.Position -= A.InvMass * corr
            B.Position += B.InvMass * corr

            Dim errAngle = WrapAngle(B.Rotation - A.Rotation)
            Dim invSum = A.InvInertia + B.InvInertia
            If invSum > 1.0e-12 Then
                Dim acorr = errAngle * 0.2
                A.Rotation -= A.InvInertia * acorr
                B.Rotation += B.InvInertia * acorr
            End If
        End Sub

        Private Function WrapAngle(a As Double) As Double
            While a > std.PI : a -= 2 * std.PI : End While
            While a < -std.PI : a += 2 * std.PI : End While
            Return a
        End Function
    End Class
End Namespace
