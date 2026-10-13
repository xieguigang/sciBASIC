#Region "Microsoft.VisualBasic::882438ef62b24e290ab944aa31b561ce, physics\Engine\Joints3D\HingeJoint3D.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 109
    '    Code Lines: 73 (66.97%)
    ' Comment Lines: 16 (14.68%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 20 (18.35%)
    '     File Size: 4.81 KB


    '     Class HingeJoint3D
    ' 
    '         Properties: AxisError, Baumgarte, Point, WorldAxisA, WorldAxisB
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: SolveAngularAxis, SolvePosition, SolveVelocity
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Engine.Physics.Math3D
Imports std = System.Math

Namespace Joints3D

    ''' <summary>
    ''' 铰链关节：在球窝关节的基础上锁死两个转动自由度，只允许绕铰链轴相对旋转。
    ''' 用于膝盖与肘部这类单轴关节。
    ''' </summary>
    ''' <remarks>
    ''' 两个单位轴要保持对齐需要 2 个角向约束（绕轴自身的扭转是自由的），
    ''' 这里取 <c>u1 = normalize(axisB × axisA)</c>（面内摆动，带位置偏置）
    ''' 与 <c>u2 = normalize(axisB × u1)</c>（面外摆动，目标速度 0）。
    ''' </remarks>
    Public Class HingeJoint3D : Implements IConstraint3D

        ''' <summary>承担"锚点重合"部分的球窝约束。</summary>
        Public ReadOnly Property Point As BallJoint3D

        ''' <summary>铰链轴在 A 局部系下的方向。</summary>
        Public hingeAxisA As Vector3
        ''' <summary>铰链轴在 B 局部系下的方向。</summary>
        Public hingeAxisB As Vector3

        ''' <summary>位置修正强度（Baumgarte 系数）。</summary>
        Public Property Baumgarte As Double = 0.2

        Sub New(a As RigidBody3D, b As RigidBody3D,
                anchorA As Vector3, anchorB As Vector3,
                hingeAxisA As Vector3, hingeAxisB As Vector3)

            Me.Point = New BallJoint3D(a, b, anchorA, anchorB)
            Me.hingeAxisA = Vector3Math.Normalize(hingeAxisA)
            Me.hingeAxisB = Vector3Math.Normalize(hingeAxisB)
        End Sub

        ''' <summary>铰链轴在世界系下的方向（A 侧）。</summary>
        Public ReadOnly Property WorldAxisA As Vector3
            Get
                Return Point.A.Orientation.Rotate(hingeAxisA)
            End Get
        End Property

        ''' <summary>铰链轴在世界系下的方向（B 侧）。</summary>
        Public ReadOnly Property WorldAxisB As Vector3
            Get
                Return Point.B.Orientation.Rotate(hingeAxisB)
            End Get
        End Property

        ''' <summary>两侧铰链轴之间的夹角（弧度）。</summary>
        Public ReadOnly Property AxisError As Double
            Get
                Return std.Acos(std.Min(1.0, std.Max(-1.0, Vector3Math.Dot(WorldAxisA, WorldAxisB))))
            End Get
        End Property

        Public Sub SolveVelocity(dt As Double) Implements IConstraint3D.SolveVelocity
            Call Point.SolveVelocity(dt)

            Dim A As RigidBody3D = Point.A
            Dim B As RigidBody3D = Point.B
            Dim invIA As Matrix3x3 = A.InvInertiaWorld()
            Dim invIB As Matrix3x3 = B.InvInertiaWorld()
            Dim axisA As Vector3 = WorldAxisA
            Dim axisB As Vector3 = WorldAxisB
            Dim n As Vector3 = Vector3Math.Cross(axisB, axisA)

            If Vector3Math.LengthSquared(n) > 1.0E-12 Then
                Dim sinAngle As Double = std.Min(1.0, Vector3Math.Length(n))
                Dim angle As Double = std.Asin(sinAngle)
                Dim u1 As Vector3 = Vector3Math.Normalize(n)
                Dim u2 As Vector3 = Vector3Math.Normalize(Vector3Math.Cross(axisB, u1))

                Call SolveAngularAxis(A, B, invIA, invIB, u1, Baumgarte / dt * angle, dt)
                Call SolveAngularAxis(A, B, invIA, invIB, u2, 0.0, dt)
            Else
                Dim u1 As Vector3 = Vector3Math.OrthogonalOf(axisA)
                Dim u2 As Vector3 = Vector3Math.Cross(axisA, u1)

                Call SolveAngularAxis(A, B, invIA, invIB, u1, 0.0, dt)
                Call SolveAngularAxis(A, B, invIA, invIB, u2, 0.0, dt)
            End If
        End Sub

        Private Shared Sub SolveAngularAxis(A As RigidBody3D, B As RigidBody3D,
                                            invIA As Matrix3x3, invIB As Matrix3x3,
                                            u As Vector3, targetSpeed As Double, dt As Double)
            Dim wRel As Vector3 = B.AngularVelocity - A.AngularVelocity
            Dim c As Double = Vector3Math.Dot(wRel, u)
            Dim k As Double = Vector3Math.Dot(u, invIA.MultiplyLeft(u)) +
                              Vector3Math.Dot(u, invIB.MultiplyLeft(u))

            If k < 1.0E-12 Then
                Return
            End If

            Dim j As Double = (targetSpeed - c) * (1.0 / k)
            Dim torque As Vector3 = u * j

            A.AngularVelocity = A.AngularVelocity - invIA.MultiplyLeft(torque)
            B.AngularVelocity = B.AngularVelocity + invIB.MultiplyLeft(torque)
        End Sub

        Public Sub SolvePosition(dt As Double) Implements IConstraint3D.SolvePosition
            Call Point.SolvePosition(dt)
        End Sub
    End Class
End Namespace

