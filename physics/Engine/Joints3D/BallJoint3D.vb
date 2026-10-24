#Region "Microsoft.VisualBasic::9c290dfa6a00ab2c7641f65d23d27b5c, physics\Engine\Joints3D\BallJoint3D.vb"

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

    '   Total Lines: 179
    '    Code Lines: 109 (60.89%)
    ' Comment Lines: 40 (22.35%)
    '    - Xml Docs: 97.50%
    ' 
    '   Blank Lines: 30 (16.76%)
    '     File Size: 7.92 KB


    '     Class BallJoint3D
    ' 
    '         Properties: AnchorError, Baumgarte, BiasSpeedLimit, MaxConeAngle, WorldAnchorA
    '                     WorldAnchorB
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ClampBias
    ' 
    '         Sub: SolveAxis, SolveConeLimit, SolvePosition, SolveVelocity
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Engine.Physics.Math3D
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
        ''' 逐轴求解的松弛因子（0 &lt; r ≤ 1）。
        ''' </summary>
        ''' <remarks>
        ''' 三个平移轴是分别求解的，但它们的有效质量矩阵有非对角耦合：当锚点不在质心上时，
        ''' 沿 x 的冲量会在锚点处产生绕 y/z 的角冲量。逐轴求解忽略这部分耦合，
        ''' 每轮都按"满额"施加冲量，多个迭代累积后就会严重过冲
        ''' （实测：把 yaw 关节的角速度设为 5 rad/s，10 步后变成 0 —— 等于给铰链加了巨大阻尼）。
        ''' 用一个小于 1 的松弛因子把每次冲量缩小，迭代仍然收敛到同一个解，但不会过冲。
        ''' </remarks>
        Public Property Relaxation As Double = 1.0

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
            Dim v As Vector3 = RelativeVelocity(rA, rB)
            Dim invIA As Matrix3x3 = A.InvInertiaWorld()
            Dim invIB As Matrix3x3 = B.InvInertiaWorld()

            ' 三轴**耦合**求解：当锚点不在质心上时，沿一个轴的冲量会在锚点处产生绕另两个轴的
            ' 角冲量。逐轴独立求解会忽略这部分耦合，迭代累积后把绕轴旋转的角速度耗散掉
            ' （实测：yaw 关节给定 5 rad/s，10 步后变成 0，相当于给铰链加了巨大阻尼）。
            ' 这里直接构造 3×3 有效质量矩阵一次解出三个拉格朗日乘子。
            Dim axes As Vector3() = {New Vector3(1, 0, 0), New Vector3(0, 1, 0), New Vector3(0, 0, 1)}
            Dim rnA(2) As Vector3
            Dim rnB(2) As Vector3
            Dim k(2, 2) As Double

            For i As Integer = 0 To 2
                rnA(i) = Vector3Math.Cross(rA, axes(i))
                rnB(i) = Vector3Math.Cross(rB, axes(i))
            Next

            For i As Integer = 0 To 2
                For j As Integer = 0 To 2
                    Dim value As Double = Vector3Math.Dot(rnA(i), invIA.MultiplyLeft(rnA(j))) +
                                          Vector3Math.Dot(rnB(i), invIB.MultiplyLeft(rnB(j)))

                    If i = j Then value += A.InvMass + B.InvMass

                    k(i, j) = value
                Next
            Next

            Dim rhs(2) As Double

            For i As Integer = 0 To 2
                Dim bias As Double = ClampBias(Baumgarte / dt * Vector3Math.Dot(err, axes(i)))
                rhs(i) = -(Vector3Math.Dot(v, axes(i)) + bias)
            Next

            Dim lambda As Double() = Solve3x3(k, rhs)

            If lambda Is Nothing Then Return

            For i As Integer = 0 To 2
                Dim impulse As Vector3 = axes(i) * (lambda(i) * Relaxation)

                Call A.ApplyImpulse(impulse * -1.0, rA)
                Call B.ApplyImpulse(impulse, rB)
            Next

            If MaxConeAngle < std.PI Then
                Call SolveConeLimit(invIA, invIB, dt)
            End If
        End Sub

        ''' <summary>3×3 线性方程组（克拉默法则），奇异时返回 Nothing。</summary>
        Private Shared Function Solve3x3(a As Double(,), b As Double()) As Double()
            Dim det As Double = Determinant3(a)

            If std.Abs(det) < 1.0E-14 Then Return Nothing

            Dim out(2) As Double

            For col As Integer = 0 To 2
                Dim m(2, 2) As Double

                For i As Integer = 0 To 2
                    For j As Integer = 0 To 2
                        m(i, j) = If(j = col, b(i), a(i, j))
                    Next
                Next

                out(col) = Determinant3(m) / det
            Next

            Return out
        End Function

        Private Shared Function Determinant3(m As Double(,)) As Double
            Return m(0, 0) * (m(1, 1) * m(2, 2) - m(1, 2) * m(2, 1)) -
                   m(0, 1) * (m(1, 0) * m(2, 2) - m(1, 2) * m(2, 0)) +
                   m(0, 2) * (m(1, 0) * m(2, 1) - m(1, 1) * m(2, 0))
        End Function

        ''' <summary>锚点处的相对速度（B 相对 A）。</summary>
        Private Function RelativeVelocity(rA As Vector3, rB As Vector3) As Vector3
            Return (B.Velocity + Vector3Math.Cross(B.AngularVelocity, rB)) -
                   (A.Velocity + Vector3Math.Cross(A.AngularVelocity, rA))
        End Function

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
            Dim j As Double = (-vn - bias) * (1.0 / k) * Relaxation
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

            ' 位置修正系数必须很小：这个函数在**每个子步**都被调用一次，若系数取 0.2，
            ' 一步之内就会做 8 次 20% 的"传送式"修正。传送只改位置不改速度，
            ' 等于把快速运动产生的位移凭空抹掉，表现为巨大的关节阻尼
            ' （实测 20 N·m 驱动力矩下 yaw 关节一秒只转 0.11 rad）。
            Dim corr As Vector3 = err * PositionCorrection

            A.Position = A.Position + corr * (A.InvMass / invSum)
            B.Position = B.Position - corr * (B.InvMass / invSum)
        End Sub

        ''' <summary>每个子步的位置修正比例（默认 0.02）。</summary>
        Public Property PositionCorrection As Double = 0.02
        ''' <summary>把 Baumgarte 修正速度夹在 <see cref="BiasSpeedLimit"/> 以内。</summary>
        Private Function ClampBias(bias As Double) As Double
            If BiasSpeedLimit <= 0.0 Then
                Return bias
            End If

            Return std.Min(BiasSpeedLimit, std.Max(-BiasSpeedLimit, bias))
        End Function
    End Class
End Namespace
