#Region "Microsoft.VisualBasic::a9902bed4f501833f8f1022d0444e43f, gr\physics\RigidBody\RigidBody.vb"

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

    '   Total Lines: 145
    '    Code Lines: 78 (53.79%)
    ' Comment Lines: 36 (24.83%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 31 (21.38%)
    '     File Size: 5.32 KB


    '     Class RigidBody
    ' 
    '         Properties: ID, InvInertia, InvMass
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetAABB
    ' 
    '         Sub: ApplyForce, ApplyForceAtPoint, ApplyImpulse, ApplyTorque, ClearForces
    '              IntegratePosition, IntegrateVelocity, SetStatic
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 刚体：质量/逆质量、转动惯量/逆惯量、线速度、角速度、力与扭矩累加、半隐式欧拉积分。

Imports Microsoft.VisualBasic.Imaging.Physics.Collision

''' <summary>
''' 2D 刚体。不会发生形变，受力后整体平动 + 转动。
''' 位置 <see cref="Position"/> 表示质心（世界坐标），<see cref="Rotation"/> 为 2D 标量角（弧度）。
''' </summary>


    Public Class RigidBody

    ''' <summary>唯一标识符</summary>
    Public Property ID As String = Guid.NewGuid().ToString("N")

    ''' <summary>质心世界坐标</summary>
    Public Position As Vector2 = Vector2.Zero

    ''' <summary>朝向（弧度，绕 Z 轴）</summary>
    Public Rotation As Double = 0.0

    ''' <summary>线速度</summary>
    Public Velocity As Vector2 = Vector2.Zero

    ''' <summary>角速度（标量）</summary>
    Public AngularVelocity As Double = 0.0

    ''' <summary>质量（kg 或任意单位）。0 / 静态物体用 <see cref="IsStatic"/> 表示</summary>
    Public Mass As Double = 1.0

    ''' <summary>绕质心的转动惯量</summary>
    Public Inertia As Double = 1.0

    ''' <summary>线性阻尼（0 = 无）</summary>
    Public LinearDamping As Double = 0.0

    ''' <summary>角阻尼（0 = 无）</summary>
    Public AngularDamping As Double = 0.05

    ''' <summary>
    ''' 最大线速度（像素/秒）。0 = 不限制。作为简化连续碰撞检测(CCD)的防护，
    ''' 限制单步位移不超过物体尺寸，避免高速物体“穿隧”而过。
    ''' </summary>
    Public MaxSpeed As Double = 0.0

    ''' <summary>物理材质</summary>
    Public Material As PhysicsMaterial = New PhysicsMaterial()

    ''' <summary>碰撞几何体（圆 / 多边形），几何中心即质心</summary>
    Public Shape As Collider

    ''' <summary>静态物体不参与动力学积分，仅作为碰撞边界</summary>
    Public IsStatic As Boolean = False

    Public ReadOnly Property InvMass As Double
        Get
            Return If(IsStatic OrElse Mass <= 0, 0.0, 1.0 / Mass)
        End Get
    End Property

    Public ReadOnly Property InvInertia As Double
        Get
            Return If(IsStatic OrElse Inertia <= 0, 0.0, 1.0 / Inertia)
        End Get
    End Property

    Private ForceAccum As Vector2 = Vector2.Zero
    Private TorqueAccum As Double = 0.0

    Sub New(shape As Collider, Optional mass As Double = 1.0, Optional material As PhysicsMaterial = Nothing)
        Me.Shape = shape
        Me.Mass = mass
        If Not material Is Nothing Then Me.Material = material
        If mass > 0 Then Me.Inertia = shape.ComputeInertia(mass)
    End Sub

    ''' <summary>设为静态物体（无限质量/惯量）</summary>
    Public Sub SetStatic()
        IsStatic = True
    End Sub

    ''' <summary>施加一个集中于质心的力</summary>
    Public Sub ApplyForce(f As Vector2)
        If IsStatic Then Return
        ForceAccum += f
    End Sub

    ''' <summary>在物体的世界坐标点 <paramref name="worldPoint"/> 施加力，会产生扭矩</summary>
    Public Sub ApplyForceAtPoint(worldPoint As Vector2, f As Vector2)
        If IsStatic Then Return
        ForceAccum += f
        TorqueAccum += Cross(worldPoint - Position, f)
    End Sub

    ''' <summary>施加纯扭矩</summary>
    Public Sub ApplyTorque(t As Double)
        If IsStatic Then Return
        TorqueAccum += t
    End Sub

    ''' <summary>在相对质心的接触点 <paramref name="contactVector"/> 处施加冲量（瞬时改变速度）</summary>
    Public Sub ApplyImpulse(impulse As Vector2, contactVector As Vector2)
        If IsStatic Then Return
        Velocity += InvMass * impulse
        AngularVelocity += InvInertia * Cross(contactVector, impulse)
    End Sub

    ''' <summary>
    ''' 半隐式（辛）欧拉：由力更新速度。应在 <see cref="IntegratePosition"/> 之前调用。
    ''' v += (F/m)·dt; ω += (τ/I)·dt
    ''' </summary>
    Public Sub IntegrateVelocity(dt As Double)
        If IsStatic Then Return
        Velocity += (ForceAccum * InvMass) * dt
        AngularVelocity += (TorqueAccum * InvInertia) * dt
        ' 指数阻尼，数值稳定
        Velocity *= 1.0 / (1.0 + LinearDamping * dt)
        AngularVelocity *= 1.0 / (1.0 + AngularDamping * dt)

        ' 简化 CCD：限制最大速度，避免单步位移过大穿隧
        If MaxSpeed > 0.0 Then
            Dim sp = Length(Velocity)
            If sp > MaxSpeed Then Velocity *= MaxSpeed / sp
        End If
    End Sub

    ''' <summary>由速度更新位置：x += v·dt; θ += ω·dt</summary>
    Public Sub IntegratePosition(dt As Double)
        If IsStatic Then Return
        Position += Velocity * dt
        Rotation += AngularVelocity * dt
    End Sub

    ''' <summary>清除力/扭矩累加器（每个子步结束后调用）</summary>
    Public Sub ClearForces()
        ForceAccum = Vector2.Zero
        TorqueAccum = 0.0
    End Sub

    ''' <summary>当前 AABB（包围盒）</summary>
    Public Function GetAABB() As AABB
        Return Shape.GetAABB(Position, Rotation)
    End Function
End Class

