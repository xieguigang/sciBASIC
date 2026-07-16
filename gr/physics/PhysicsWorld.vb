#Region "Microsoft.VisualBasic::e2e63c677f7ea81b5f1187dc7e5b0603, gr\physics\PhysicsWorld.vb"

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

    '   Total Lines: 147
    '    Code Lines: 87 (59.18%)
    ' Comment Lines: 34 (23.13%)
    '    - Xml Docs: 64.71%
    ' 
    '   Blank Lines: 26 (17.69%)
    '     File Size: 5.16 KB


    ' Class PhysicsWorld
    ' 
    '     Function: Box, Circle
    ' 
    '     Sub: [Step], (+3 Overloads) Add, StepInternal, StepSub
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 物理世界：固定步长游戏循环，整合力场、宽/窄相位碰撞、关节与刚体积分。

Imports Microsoft.VisualBasic.Imaging.Physics.Collision
Imports Microsoft.VisualBasic.Imaging.Physics.ForceFields
Imports Microsoft.VisualBasic.Imaging.Physics.Joints

''' <summary>
''' 2D 物理世界。以 <see cref="PhysicsWorld.Step"/> 固定步长驱动，内部按子步执行：
''' 施加力场 → 由力积分速度 → 宽相位 → 窄相位 → 顺序冲量求解（接触 + 关节）→
''' 由速度积分位置 → 关节位置修正。
''' 可直接接入游戏主循环（每帧调用一次 <see cref="PhysicsWorld.Step"/> 传入真实 dt）。
''' </summary>
Public Class PhysicsWorld

    ''' <summary>所有刚体</summary>
    Public Bodies As New List(Of RigidBody)

    ''' <summary>所有约束/关节</summary>
    Public Joints As New List(Of IConstraint)

    ''' <summary>所有力场</summary>
    Public ForceFields As New List(Of ForceField)

    ''' <summary>全局重力（像素世界下数值通常较大）</summary>
    Public Gravity As Vector2 = New Vector2(0, 200.0)

    ''' <summary>速度/位置约束求解迭代次数</summary>
    Public Iterations As Integer = 12

    ''' <summary>固定时间步长（秒）</summary>
    Public FixedDt As Double = 1.0 / 60.0

    ''' <summary>每个固定步长内的子步数（越多越稳定、越慢）</summary>
    Public Substeps As Integer = 2

    Private accumulator As Double = 0.0

    ''' <summary>添加刚体</summary>
    Public Sub Add(b As RigidBody)
        Bodies.Add(b)
    End Sub

    ''' <summary>添加约束/关节</summary>
    Public Sub Add(j As IConstraint)
        Joints.Add(j)
    End Sub

    ''' <summary>添加力场</summary>
    Public Sub Add(f As ForceField)
        ForceFields.Add(f)
    End Sub

    ''' <summary>
    ''' 固定步长推进。传入真实帧间隔 <paramref name="frameDt"/>（秒），
    ''' 内部用累加器以 <see cref="FixedDt"/> 为单位执行若干子步，避免可变步长不稳定。
    ''' </summary>
    Public Sub [Step](frameDt As Double)
        accumulator += frameDt
        Dim steps = 0
        While accumulator >= FixedDt AndAlso steps < 4
            StepInternal(FixedDt)
            accumulator -= FixedDt
            steps += 1
        End While
        ' 防止“死亡螺旋”：连续滞后时丢弃积压时间
        If steps >= 4 Then accumulator = 0.0
    End Sub

    Private Sub StepInternal(dt As Double)
        Dim sub_dt = dt / Substeps
        For s = 1 To Substeps
            StepSub(sub_dt)
        Next
    End Sub

    Private Sub StepSub(dt As Double)
        ' 1. 全局重力 + 力场
        For Each b In Bodies
            If b.IsStatic Then Continue For
            b.ApplyForce(Gravity * b.Mass)
        Next
        For Each ff In ForceFields
            ff.Apply(Bodies)
        Next

        ' 2. 由力积分速度（半隐式欧拉）
        For Each b In Bodies
            b.IntegrateVelocity(dt)
        Next

        ' 3. 宽相位
        Dim pairs = BroadPhase.ComputePairs(Bodies)

        ' 4. 窄相位 → 流形
        Dim manifolds As New List(Of Manifold)
        For Each pr In pairs
            Dim m = NarrowPhase.Collide(pr.A, pr.B)
            If m.Contacts IsNot Nothing AndAlso m.Contacts.Length > 0 AndAlso m.Penetration > 0 Then
                m.InitSolver()
                manifolds.Add(m)
            End If
        Next

        ' 5. 速度约束求解（接触 + 关节），多次迭代
        For i = 1 To Iterations
            For Each m In manifolds
                ContactSolver.Solve(m, dt)
            Next
            For Each j In Joints
                j.SolveVelocity(dt)
            Next
        Next

        ' 6. 由速度积分位置
        For Each b In Bodies
            b.IntegratePosition(dt)
        Next

        ' 7. 关节位置修正（消除漂移）
        For Each j In Joints
            j.SolvePosition(dt)
        Next

        ' 8. 清除力/扭矩累加器
        For Each b In Bodies
            b.ClearForces()
        Next
    End Sub

    ' ---------------- 工厂方法 ----------------

    ''' <summary>创建一个矩形刚体（多边形碰撞体，顶点相对质心）</summary>
    Public Shared Function Box(width As Double, height As Double, mass As Double, Optional material As PhysicsMaterial = Nothing) As RigidBody
        Dim hw = width / 2, hh = height / 2
        Dim poly = New PolygonCollider({
            New Vector2(-hw, -hh), New Vector2(hw, -hh),
            New Vector2(hw, hh), New Vector2(-hw, hh)
        })
        Return New RigidBody(poly, mass, material)
    End Function

    ''' <summary>创建一个圆形刚体</summary>
    Public Shared Function Circle(radius As Double, mass As Double, Optional material As PhysicsMaterial = Nothing) As RigidBody
        Return New RigidBody(New CircleCollider(radius), mass, material)
    End Function
End Class

