#Region "Microsoft.VisualBasic::f1e5673571b9d97da130e5aa0388ca3e, Data_science\MachineLearning\DeepQNetwork\Demo\StickmanEnv.vb"

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

    '   Total Lines: 229
    '    Code Lines: 170 (74.24%)
    ' Comment Lines: 24 (10.48%)
    '    - Xml Docs: 16.67%
    ' 
    '   Blank Lines: 35 (15.28%)
    '     File Size: 7.67 KB


    ' Class StickmanEnv
    ' 
    '     Properties: ActionCount, Actions, CurrentState, IsDone, LastReward
    '                 StateSize
    ' 
    '     Function: [Step], AssembleState, NextFeature, PhaseText, Reset
    ' 
    '     Sub: AddStaticBox, BuildWorld
    '     Structure Feature
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 火柴人强化学习环境：平地 → 台阶 → 障碍 → 终点 地形；离散动作驱动小人，
' 奖励塑形为「前进 + 上台阶/越障/到终点奖励 − 摔倒惩罚」，实现 IEnvironment。

Imports System
Imports Microsoft.VisualBasic.Imaging.Physics
Imports Microsoft.VisualBasic.Imaging.Physics.Collision
Imports Microsoft.VisualBasic.Imaging.Physics.Joints
Imports DeepQNetwork

''' <summary>
''' 火柴人跑步/上下台阶/跳跃越障环境。
''' </summary>
Public Class StickmanEnv : Implements IEnvironment

    ' ----- 关卡常量（可按需调整难度）-----
    Public Const GroundTopY As Double = 520
    Public Const StartX As Double = 120
    Public Const StartY As Double = GroundTopY - 110
    Public Const GoalX As Double = 1700
    Public Const MaxSteps As Integer = 900
    Public Const ActionRepeat As Integer = 4
    Public Const Dt As Double = 1.0 / 60.0

    ' ----- 运行态 -----
    Public world As PhysicsWorld
    Public stick As Stickman
    Public terrainBodies As New List(Of RigidBody)

    Private stepX1, stepX2, stepTop As Double
    Private obsX1, obsX2, obsTop As Double
    Private obstacleCleared, stepCleared As Boolean
    Private steps As Integer

    Private curState As Double()
    Private lastR As Double
    Private doneFlag As Boolean

    Private Structure Feature
        Public xStart As Double
        Public xEnd As Double
        Public topY As Double
    End Structure

    Private features As New List(Of Feature)

    ' ---------------- IEnvironment ----------------

    Public ReadOnly Property StateSize As Integer Implements IEnvironment.StateSize
        Get
            Return Stickman.KIN_STATE_LEN + 4
        End Get
    End Property

    Public ReadOnly Property ActionCount As Integer Implements IEnvironment.ActionCount
        Get
            Return [Enum].GetValues(GetType(StickmanAction)).Length
        End Get
    End Property

    Public ReadOnly Property Actions As Array Implements IEnvironment.Actions
        Get
            Return [Enum].GetValues(GetType(StickmanAction))
        End Get
    End Property

    Public ReadOnly Property CurrentState As Double() Implements IEnvironment.CurrentState
        Get
            Return curState
        End Get
    End Property

    Public ReadOnly Property LastReward As Double Implements IEnvironment.LastReward
        Get
            Return lastR
        End Get
    End Property

    Public ReadOnly Property IsDone As Boolean Implements IEnvironment.IsDone
        Get
            Return doneFlag
        End Get
    End Property

    Public Function Reset() As Double() Implements IEnvironment.Reset
        steps = 0
        obstacleCleared = False
        stepCleared = False
        doneFlag = False
        lastR = 0
        BuildWorld()
        curState = AssembleState()
        Return curState
    End Function

    Public Function [Step](action As Integer) As StepResult Implements IEnvironment.Step
        Dim a = CType(action, StickmanAction)
        stick.ApplyAction(a)

        Dim prevX = stick.TorsoX()
        For i As Integer = 1 To ActionRepeat
            stick.MotorStep()
            world.Step(Dt)
        Next
        Dim newX = stick.TorsoX()
        Dim dx = newX - prevX

        Dim reward = 0.0
        ' 前进奖励
        reward += dx * 0.1
        ' 存活小奖励
        reward += 0.01

        ' 上台阶奖励（一次性）
        If Not stepCleared AndAlso newX > stepX2 Then
            stepCleared = True
            reward += 3.0
        End If
        ' 越障奖励（一次性）
        If Not obstacleCleared AndAlso newX > obsX2 Then
            obstacleCleared = True
            reward += 5.0
        End If
        ' 到达终点
        If newX >= GoalX Then
            reward += 20.0
            doneFlag = True
        End If
        ' 摔倒
        If stick.IsFallen(GroundTopY) Then
            reward -= 10.0
            doneFlag = True
        End If

        steps += 1
        If steps >= MaxSteps Then doneFlag = True
        If newX < StartX - 150 Then doneFlag = True

        lastR = reward
        curState = AssembleState()
        Return New StepResult With {.state = curState, .reward = reward, .done = doneFlag}
    End Function

    ' ---------------- 关卡构建 ----------------

    Private Sub BuildWorld()
        world = New PhysicsWorld()
        world.Gravity = New Vector2(0, 600)
        world.FixedDt = Dt
        world.Substeps = 4
        world.Iterations = 20
        terrainBodies.Clear()

        ' 台阶
        stepX1 = 560 : stepX2 = 760 : stepTop = 470
        ' 障碍块（需跳跃越过）
        obsX1 = 1080 : obsX2 = 1180 : obsTop = 460

        ' 地面（静态，顶面 y = GroundTopY）
        AddStaticBox(1000, GroundTopY, 2600, 240, 0.6)
        ' 台阶平台（顶面 y = stepTop）
        AddStaticBox((stepX1 + stepX2) / 2, stepTop, stepX2 - stepX1, 240, 0.7)
        ' 障碍块（顶面 y = obsTop）
        AddStaticBox((obsX1 + obsX2) / 2, obsTop, obsX2 - obsX1, 140, 0.7)

        features.Clear()
        features.Add(New Feature With {.xStart = stepX1, .xEnd = stepX2, .topY = stepTop})
        features.Add(New Feature With {.xStart = obsX1, .xEnd = obsX2, .topY = obsTop})

        stick = New Stickman()
        stick.Build(world, StartX, StartY)
    End Sub

    Private Sub AddStaticBox(cx As Double, topY As Double, w As Double, h As Double, friction As Double)
        Dim b = world.Box(w, h, 0, New PhysicsMaterial(friction, 0.0))
        b.Position = New Vector2(cx, topY + h / 2)
        b.SetStatic()
        world.Add(b)
        terrainBodies.Add(b)
    End Sub

    ' ---------------- 状态组装 ----------------

    Private Function NextFeature() As Feature
        Dim tx = stick.TorsoX()
        For Each f In features
            If f.xEnd > tx Then
                Return f
            End If
        Next
        ' 已越过所有地形特征
        Return New Feature With {.xStart = GoalX + 1000, .xEnd = GoalX + 2000, .topY = GroundTopY}
    End Function

    Private Function AssembleState() As Double()
        Dim kin = stick.KinematicState()
        Dim f = NextFeature()
        Dim dx = f.xStart - stick.TorsoX()
        Dim dxN = Math.Max(-1, Math.Min(1, dx / 300.0))
        Dim hRel = (f.topY - stick.AvgFootY()) / 100.0
        Dim onGround = If(stick.AvgFootY() > GroundTopY - 30, 1.0, 0.0)
        Dim prog = (stick.TorsoX() - StartX) / 1000.0

        Dim extra(3) As Double
        extra(0) = dxN
        extra(1) = Math.Max(-1, Math.Min(1, hRel))
        extra(2) = onGround
        extra(3) = Math.Max(0, Math.Min(2, prog))

        Dim total = kin.Length + 4
        Dim outp(total - 1) As Double
        Array.Copy(kin, outp, kin.Length)
        For i As Integer = 0 To 3
            outp(kin.Length + i) = extra(i)
        Next
        Return outp
    End Function

    ''' <summary>当前所处关卡阶段文字（供 HUD 显示）</summary>
    Public Function PhaseText() As String
        Dim x = stick.TorsoX()
        If stick.IsFallen(GroundTopY) Then Return "摔倒"
        If x >= GoalX Then Return "到达终点"
        If x >= obsX1 AndAlso x <= obsX2 Then Return "跨越障碍"
        If x >= stepX1 AndAlso x <= stepX2 Then Return "上台阶"
        If x > obsX2 Then Return "下台阶/平地"
        Return "平地奔跑"
    End Function
End Class
