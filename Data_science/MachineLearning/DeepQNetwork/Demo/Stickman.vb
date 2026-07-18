#Region "Microsoft.VisualBasic::6c0299c3b2d9b40d9155380a7bd11271, Data_science\MachineLearning\DeepQNetwork\Demo\Stickman.vb"

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

    '   Total Lines: 283
    '    Code Lines: 206 (72.79%)
    ' Comment Lines: 46 (16.25%)
    '    - Xml Docs: 63.04%
    ' 
    '   Blank Lines: 31 (10.95%)
    '     File Size: 12.35 KB


    ' Enum StickmanAction
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class Stickman
    ' 
    '     Properties: KIN_STATE_LEN
    ' 
    '     Function: AvgFootY, Bodies, IsFallen, KinematicState, MakeBox
    '               MakeCircle, Skeleton, TorsoAngle, TorsoX, TorsoY
    ' 
    '     Sub: AddJoint, ApplyAction, ApplyJump, Build, MotorStep
    '          Pivot
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 火柴棍小人布偶：用 RigidBody + RevoluteJoint 搭建头/躯干/四肢/脚，
' 通过 PD 电机（在关节处对父子刚体施加反向力矩）驱动行走，并暴露状态提取与摔倒判定。

Imports System
Imports Microsoft.VisualBasic.Imaging.Physics
Imports Microsoft.VisualBasic.Imaging.Physics.Collision
Imports Microsoft.VisualBasic.Imaging.Physics.Joints

''' <summary>离散动作集合（索引即 Q 网络输出下标）</summary>
Public Enum StickmanAction
    Idle = 0
    LeftHipForward = 1
    LeftHipBack = 2
    RightHipForward = 3
    RightHipBack = 4
    KneesBend = 5
    Jump = 6
    LeanForward = 7
End Enum

''' <summary>
''' 火柴棍小人。所有刚体以躯干中心为基准按站立姿态摆放，关节用“世界锚点”法构造，
''' 保证相邻刚体的锚点重合。行走由 PD 电机驱动：每个可控关节（髋/膝）按目标相对角
''' 对子体与父体施加等大反向力矩；躯干另有轻微自扶正 PD 以保持不立即摔倒。
''' </summary>
Public Class Stickman

    ' ----- 可调控制增益（如演示中动作过于僵硬/瘫软，请在此调参）-----
    Public Shared Kp As Double = 90000.0      ' 髋/膝电机比例增益
    Public Shared Kd As Double = 9000.0       ' 髋/膝电机微分增益
    Public Shared KpTorso As Double = 45000.0 ' 躯干自扶正比例增益
    Public Shared KdTorso As Double = 4500.0  ' 躯干自扶正微分增益
    Public Shared JumpImpulse As Double = 220.0 ' 跳跃时每个刚体获得的向上速度增量

    ' ----- 刚体 -----
    Public head As RigidBody
    Public torso As RigidBody
    Public upperArmL, upperArmR, lowerArmL, lowerArmR As RigidBody
    Public thighL, thighR, shinL, shinR, footL, footR As RigidBody

    Private all As New List(Of RigidBody)

    ' ----- 电机目标（由动作设置，由 MotorStep 每子步执行）-----
    Private hipTargetL, hipTargetR, kneeTargetL, kneeTargetR As Double
    Private torsoTarget As Double

    ' 局部尺寸常量
    Private Const TorsoW As Double = 16, TorsoH As Double = 48
    Private Const HeadR As Double = 12
    Private Const UpperArmH As Double = 30
    Private Const LowerArmH As Double = 28
    Private Const ThighH As Double = 34
    Private Const ShinH As Double = 32
    Private Const FootW As Double = 18, FootH As Double = 8

    ''' <summary>
    ''' 在给定世界坐标（躯干中心）处搭建小人并加入物理世界。
    ''' </summary>
    Public Sub Build(world As PhysicsWorld, startX As Double, startY As Double)
        all.Clear()

        Dim matLimb = New PhysicsMaterial(0.4, 0.0)
        Dim matFoot = New PhysicsMaterial(0.95, 0.0)

        ' 局部偏移（相对躯干中心，y 向下为正）
        Dim pHead = New Vector2(startX + 0, startY - 36)
        Dim pTorso = New Vector2(startX + 0, startY + 0)
        Dim pUpArmL = New Vector2(startX - 8, startY - 3)
        Dim pLoArmL = New Vector2(startX - 8, startY + 26)
        Dim pUpArmR = New Vector2(startX + 8, startY - 3)
        Dim pLoArmR = New Vector2(startX + 8, startY + 26)
        Dim pThighL = New Vector2(startX - 6, startY + 41)
        Dim pShinL = New Vector2(startX - 6, startY + 74)
        Dim pFootL = New Vector2(startX - 6, startY + 94)
        Dim pThighR = New Vector2(startX + 6, startY + 41)
        Dim pShinR = New Vector2(startX + 6, startY + 74)
        Dim pFootR = New Vector2(startX + 6, startY + 94)

        head = MakeCircle(world, HeadR, 1.0, matLimb, pHead)
        torso = MakeBox(world, TorsoW, TorsoH, 5.0, matLimb, pTorso)
        upperArmL = MakeBox(world, 8, UpperArmH, 1.0, matLimb, pUpArmL)
        lowerArmL = MakeBox(world, 7, LowerArmH, 0.8, matLimb, pLoArmL)
        upperArmR = MakeBox(world, 8, UpperArmH, 1.0, matLimb, pUpArmR)
        lowerArmR = MakeBox(world, 7, LowerArmH, 0.8, matLimb, pLoArmR)
        thighL = MakeBox(world, 10, ThighH, 2.5, matLimb, pThighL)
        shinL = MakeBox(world, 9, ShinH, 1.8, matLimb, pShinL)
        footL = MakeBox(world, FootW, FootH, 1.2, matFoot, pFootL)
        thighR = MakeBox(world, 10, ThighH, 2.5, matLimb, pThighR)
        shinR = MakeBox(world, 9, ShinH, 1.8, matLimb, pShinR)
        footR = MakeBox(world, FootW, FootH, 1.2, matFoot, pFootR)

        ' 关节：世界锚点 -> 局部锚点
        ' 颈
        AddJoint(world, torso, head, New Vector2(startX + 0, startY - 24))
        ' 左臂
        AddJoint(world, torso, upperArmL, New Vector2(startX - 8, startY - 18))
        AddJoint(world, upperArmL, lowerArmL, New Vector2(startX - 8, startY + 12))
        ' 右臂
        AddJoint(world, torso, upperArmR, New Vector2(startX + 8, startY - 18))
        AddJoint(world, upperArmR, lowerArmR, New Vector2(startX + 8, startY + 12))
        ' 左腿
        AddJoint(world, torso, thighL, New Vector2(startX - 6, startY + 24))
        AddJoint(world, thighL, shinL, New Vector2(startX - 6, startY + 58))
        AddJoint(world, shinL, footL, New Vector2(startX - 6, startY + 90))
        ' 右腿
        AddJoint(world, torso, thighR, New Vector2(startX + 6, startY + 24))
        AddJoint(world, thighR, shinR, New Vector2(startX + 6, startY + 58))
        AddJoint(world, shinR, footR, New Vector2(startX + 6, startY + 90))
    End Sub

    Private Function MakeBox(world As PhysicsWorld, w As Double, h As Double, m As Double, mat As PhysicsMaterial, pos As Vector2) As RigidBody
        Dim b = world.Box(w, h, m, mat)
        b.Position = pos
        b.MaxSpeed = 1400
        b.AngularDamping = 0.5
        all.Add(b)
        Return b
    End Function

    Private Function MakeCircle(world As PhysicsWorld, r As Double, m As Double, mat As PhysicsMaterial, pos As Vector2) As RigidBody
        Dim b = world.Circle(r, m, mat)
        b.Position = pos
        b.MaxSpeed = 1400
        b.AngularDamping = 0.5
        all.Add(b)
        Return b
    End Function

    Private Sub AddJoint(world As PhysicsWorld, a As RigidBody, b As RigidBody, worldAnchor As Vector2)
        Dim anchorA = worldAnchor - a.Position
        Dim anchorB = worldAnchor - b.Position
        world.Add(New RevoluteJoint(a, b, anchorA, anchorB))
    End Sub

    ''' <summary>所有刚体（供渲染遍历）</summary>
    Public Function Bodies() As List(Of RigidBody)
        Return all
    End Function

    ''' <summary>躯干中心 X（用于相机跟随 / 奖励）</summary>
    Public Function TorsoX() As Double
        Return torso.Position.x
    End Function

    ''' <summary>躯干中心 Y</summary>
    Public Function TorsoY() As Double
        Return torso.Position.y
    End Function

    ''' <summary>双脚平均 Y（近似脚底高度）</summary>
    Public Function AvgFootY() As Double
        Return (footL.Position.y + footR.Position.y) * 0.5
    End Function

    ''' <summary>设定离散动作对应的电机目标（跳跃为一次性冲量）</summary>
    Public Sub ApplyAction(action As StickmanAction)
        ' 复位为目标静止姿态
        hipTargetL = 0 : hipTargetR = 0
        kneeTargetL = 0 : kneeTargetR = 0
        torsoTarget = 0

        Select Case action
            Case StickmanAction.Idle
                ' 保持
            Case StickmanAction.LeftHipForward
                hipTargetL = -0.6 : hipTargetR = 0.2
            Case StickmanAction.LeftHipBack
                hipTargetL = 0.6 : hipTargetR = -0.2
            Case StickmanAction.RightHipForward
                hipTargetR = -0.6 : hipTargetL = 0.2
            Case StickmanAction.RightHipBack
                hipTargetR = 0.6 : hipTargetL = -0.2
            Case StickmanAction.KneesBend
                kneeTargetL = 0.7 : kneeTargetR = 0.7
            Case StickmanAction.LeanForward
                torsoTarget = -0.25
            Case StickmanAction.Jump
                ApplyJump()
        End Select
    End Sub

    Private Sub ApplyJump()
        Dim imp = New Vector2(0, -JumpImpulse)
        For Each b In all
            b.ApplyImpulse(imp * b.Mass, Vector2.Zero)
        Next
    End Sub

    ''' <summary>
    ''' 每个物理子步调用：对髋/膝施加 PD 电机力矩，对躯干施加自扶正力矩。
    ''' 必须在 <see cref="PhysicsWorld.Step"/> 之前调用。
    ''' </summary>
    Public Sub MotorStep()
        Pivot(thighL, torso, hipTargetL)
        Pivot(thighR, torso, hipTargetR)
        Pivot(shinL, thighL, kneeTargetL)
        Pivot(shinR, thighR, kneeTargetR)

        ' 躯干自扶正（相对世界保持直立）
        Dim tErr = torsoTarget - torso.Rotation
        Dim tTorque = KpTorso * tErr - KdTorso * torso.AngularVelocity
        torso.ApplyTorque(tTorque)
    End Sub

    ''' <summary>
    ''' 在 parent-child 关节上施加 PD 电机：对 child 施加 +torque，对 parent 施加 -torque。
    ''' 相对角 = child.Rotation - parent.Rotation。
    ''' </summary>
    Private Sub Pivot(child As RigidBody, parent As RigidBody, targetRel As Double)
        Dim cur = child.Rotation - parent.Rotation
        Dim err = targetRel - cur
        Dim dOmega = child.AngularVelocity - parent.AngularVelocity
        Dim torque = Kp * err - Kd * dOmega
        child.ApplyTorque(torque)
        parent.ApplyTorque(-torque)
    End Sub

    ''' <summary>躯干相对世界的角度（弧度），已归一化到 [-π, π]</summary>
    Public Function TorsoAngle() As Double
        Dim a = torso.Rotation
        While a > Math.PI : a -= 2 * Math.PI : End While
        While a < -Math.PI : a += 2 * Math.PI : End While
        Return a
    End Function

    ''' <summary>是否摔倒：躯干过度倾斜或躯干/头部触地</summary>
    Public Function IsFallen(groundTopY As Double) As Boolean
        If Math.Abs(TorsoAngle()) > 1.2 Then Return True
        If torso.Position.y > groundTopY - 14 Then Return True
        If head.Position.y > groundTopY - HeadR - 2 Then Return True
        Return False
    End Function

    ''' <summary>
    ''' 运动学状态向量（长度为 KIN_STATE_LEN），已归一化到大致 [-1, 1]。
    ''' </summary>
    Public Function KinematicState() As Double()
        Dim s(15) As Double
        s(0) = TorsoAngle() / Math.PI
        s(1) = torso.AngularVelocity / 10.0
        s(2) = torso.Velocity.x / 10.0
        s(3) = torso.Velocity.y / 10.0
        s(4) = (thighL.Rotation - torso.Rotation) / Math.PI
        s(5) = (thighR.Rotation - torso.Rotation) / Math.PI
        s(6) = (shinL.Rotation - thighL.Rotation) / Math.PI
        s(7) = (shinR.Rotation - thighR.Rotation) / Math.PI
        s(8) = (thighL.AngularVelocity - torso.AngularVelocity) / 10.0
        s(9) = (thighR.AngularVelocity - torso.AngularVelocity) / 10.0
        s(10) = (shinL.AngularVelocity - thighL.AngularVelocity) / 10.0
        s(11) = (shinR.AngularVelocity - thighR.AngularVelocity) / 10.0
        s(12) = footL.Position.y / 100.0
        s(13) = footR.Position.y / 100.0
        s(14) = torso.Position.y / 100.0
        s(15) = (torso.Position.y - AvgFootY()) / 100.0
        Return s
    End Function

    ''' <summary>运动学状态长度</summary>
    Public Shared ReadOnly Property KIN_STATE_LEN As Integer
        Get
            Return 16
        End Get
    End Property

    ''' <summary>骨架连线（供绘制火柴人）：返回相邻刚体中心的成对世界坐标</summary>
    Public Function Skeleton() As List(Of (a As Vector2, b As Vector2))
        Dim lines As New List(Of (Vector2, Vector2)) From {
            (head.Position, torso.Position),
            (torso.Position, upperArmL.Position),
            (upperArmL.Position, lowerArmL.Position),
            (torso.Position, upperArmR.Position),
            (upperArmR.Position, lowerArmR.Position),
            (torso.Position, thighL.Position),
            (thighL.Position, shinL.Position),
            (shinL.Position, footL.Position),
            (torso.Position, thighR.Position),
            (thighR.Position, shinR.Position),
            (shinR.Position, footR.Position)
        }
        Return lines
    End Function
End Class
