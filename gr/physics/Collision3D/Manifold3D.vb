Imports System.Math
Imports Microsoft.VisualBasic.Imaging.Physics.Math3D
Imports std = System.Math

Namespace Collision3D

    ''' <summary>
    ''' 单个接触点。除位置与穿透深度外，还缓存求解所需的力臂、有效质量与累积冲量
    ''' （顺序冲量法需要跨迭代累积冲量以收敛）。
    ''' </summary>
    Public Class ContactPoint3D

        ''' <summary>世界坐标下的接触点。</summary>
        Public Position As Vector3
        ''' <summary>穿透深度（正值表示互相侵入）。</summary>
        Public Penetration As Double

        ''' <summary>法向累积冲量（≥ 0）。</summary>
        Public NormalImpulse As Double = 0.0
        ''' <summary>第一条切线方向的累积摩擦冲量。</summary>
        Public TangentImpulse1 As Double = 0.0
        ''' <summary>第二条切线方向的累积摩擦冲量。</summary>
        Public TangentImpulse2 As Double = 0.0

        ''' <summary>接触点相对 A 质心的力臂。</summary>
        Friend rA As Vector3
        ''' <summary>接触点相对 B 质心的力臂。</summary>
        Friend rB As Vector3

        ''' <summary>法向有效质量的倒数。</summary>
        Friend normalMass As Double
        ''' <summary>两条切线有效质量的倒数。</summary>
        Friend tangentMass1 As Double
        ''' <summary>第二条切线有效质量的倒数。</summary>
        Friend tangentMass2 As Double
        ''' <summary>Baumgarte 位置修正偏置速度。</summary>
        Friend bias As Double
        ''' <summary>恢复（弹性）目标速度。</summary>
        Friend restitutionBias As Double

        ''' <summary>接触处的两条切线（与法向构成正交基）。</summary>
        Friend tangent1 As Vector3
        ''' <summary>接触处的第二条切线。</summary>
        Friend tangent2 As Vector3

        Sub New(position As Vector3, penetration As Double)
            Me.Position = position
            Me.Penetration = penetration
        End Sub
    End Class

    ''' <summary>
    ''' 接触流形：一对刚体之间的一批接触点，法向由 A 指向 B。
    ''' </summary>
    Public Class Manifold3D

        ''' <summary>参与碰撞的第一个刚体。</summary>
        Public A As RigidBody3D
        ''' <summary>参与碰撞的第二个刚体。</summary>
        Public B As RigidBody3D
        ''' <summary>接触法向（单位向量，由 A 指向 B）。</summary>
        Public Normal As Vector3
        ''' <summary>接触点列表。</summary>
        Public Contacts As New List(Of ContactPoint3D)()
        ''' <summary>恢复系数（由双方材质组合而来）。</summary>
        Public Restitution As Double = 0.0
        ''' <summary>摩擦系数（由双方材质组合而来）。</summary>
        Public Friction As Double = 0.4

        ''' <summary>是否存在有效接触。</summary>
        Public ReadOnly Property HasContacts As Boolean
            Get
                Return Contacts IsNot Nothing AndAlso Contacts.Count > 0
            End Get
        End Property

        ''' <summary>最大穿透深度。</summary>
        Public ReadOnly Property Penetration As Double
            Get
                Dim maxPen As Double = 0.0

                For Each c In Contacts
                    If c.Penetration > maxPen Then maxPen = c.Penetration
                Next

                Return maxPen
            End Get
        End Property

        ''' <summary>
        ''' 在求解开始前预计算每个接触点的力臂、有效质量与偏置速度。
        ''' </summary>
        ''' <param name="dt">子步长，用于 Baumgarte 系数换算。</param>
        ''' <param name="baumgarte">位置修正强度（0.2 左右）。</param>
        ''' <param name="slop">允许的穿透容差，避免抖动。</param>
        Public Sub InitSolver(Optional dt As Double = 1.0 / 60.0,
                              Optional baumgarte As Double = 0.2,
                              Optional slop As Double = 0.01)
            Dim n As Vector3 = Normal
            Dim invIA As Matrix3x3 = A.InvInertiaWorld()
            Dim invIB As Matrix3x3 = B.InvInertiaWorld()
            Dim t1 As Vector3 = Vector3Math.OrthogonalOf(n)
            Dim t2 As Vector3 = Vector3Math.Cross(n, t1)

            For Each c In Contacts
                c.rA = c.Position - A.Position
                c.rB = c.Position - B.Position
                c.tangent1 = t1
                c.tangent2 = t2

                c.normalMass = 1.0 / EffectiveMass(A, B, invIA, invIB, c.rA, c.rB, n)
                c.tangentMass1 = 1.0 / EffectiveMass(A, B, invIA, invIB, c.rA, c.rB, t1)
                c.tangentMass2 = 1.0 / EffectiveMass(A, B, invIA, invIB, c.rA, c.rB, t2)

                c.bias = baumgarte / dt * std.Max(0.0, c.Penetration - slop)

                ' 恢复只在接近速度足够大时才生效，避免静止物体的持续抖动
                Dim rv As Vector3 = RelativeVelocity(c.rA, c.rB)
                Dim vn As Double = Vector3Math.Dot(rv, n)

                c.restitutionBias = If(vn < -1.0, -Restitution * vn, 0.0)

                ' 冲量累积在新的一帧开始时清零：本库不做跨帧热启动
                c.NormalImpulse = 0.0
                c.TangentImpulse1 = 0.0
                c.TangentImpulse2 = 0.0
            Next
        End Sub

        ''' <summary>接触点的相对速度（B 相对 A）。</summary>
        Public Function RelativeVelocity(rA As Vector3, rB As Vector3) As Vector3
            Dim va As Vector3 = A.Velocity + Vector3Math.Cross(A.AngularVelocity, rA)
            Dim vb As Vector3 = B.Velocity + Vector3Math.Cross(B.AngularVelocity, rB)

            Return vb - va
        End Function

        ''' <summary>
        ''' 沿方向 <paramref name="dir"/> 的有效质量：
        ''' <c>k = invMassA + invMassB + (rA×dir)·invIA·(rA×dir) + (rB×dir)·invIB·(rB×dir)</c>。
        ''' </summary>
        Private Shared Function EffectiveMass(a As RigidBody3D, b As RigidBody3D,
                                              invIA As Matrix3x3, invIB As Matrix3x3,
                                              rA As Vector3, rB As Vector3,
                                              dir As Vector3) As Double
            Dim rnA As Vector3 = Vector3Math.Cross(rA, dir)
            Dim rnB As Vector3 = Vector3Math.Cross(rB, dir)
            Dim k As Double = a.InvMass + b.InvMass

            k += Vector3Math.Dot(rnA, invIA.MultiplyLeft(rnA))
            k += Vector3Math.Dot(rnB, invIB.MultiplyLeft(rnB))

            Return std.Max(k, 1.0E-12)
        End Function
    End Class
End Namespace
