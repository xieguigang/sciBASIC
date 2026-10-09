' /********************************************************************************/
'
'   SphImpeller3D.vb
'
'   运动边界 —— 旋转搅拌桨（Rushton 圆盘涡轮 + 搅拌轴）
'
'   作用：
'       发酵罐底部的旋转搅拌桨是流场的唯一驱动源。桨叶是"运动固体"，
'       这里用速度驱动（moving boundary forcing）建模：桨体及其影响带内的
'       粒子被逐渐混合到桨的表面速度 v = ω × r（切向）+ 轴向泵送分量，
'       而不是硬覆盖速度（硬覆盖会让邻近粒子速度突变，进而炸掉 SPH 求解）。
'
'   几何：
'       - 圆盘：以 (CenterX, CenterY, ZCenter) 为中心，半径 Radius，
'         z 方向厚度 BladeHeight
'       - 搅拌轴：半径 ShaftRadius，从桨盘向上延伸到 ShaftTop
'       - 影响带：固体表面外 Influence 距离内，混合权重线性衰减到 0
'
'   速度模型（与 CFDEngine.Application.Stirrer 一致）：
'       ω 向量 = (0, 0, ω)，r = (x - cx, y - cy, 0)
'       v = ω × r = (-ω·dy, ω·dx, 0)，轴向分量取 AxialVelocity
'
' /********************************************************************************/

Imports System.Math

''' <summary>
''' A rotating impeller (Rushton style disc turbine plus its shaft) modelled as
''' a moving boundary that drags the surrounding SPH particles.
''' </summary>
Public Class SphImpeller3D

    ''' <summary>rotation axis position along x</summary>
    Public Property CenterX As Single
    ''' <summary>rotation axis position along y</summary>
    Public Property CenterY As Single
    ''' <summary>z of the disc centre</summary>
    Public Property ZCenter As Single

    ''' <summary>disc (blade) radius</summary>
    Public Property Radius As Single
    ''' <summary>disc thickness along z</summary>
    Public Property BladeHeight As Single

    ''' <summary>shaft radius (0 disables the shaft)</summary>
    Public Property ShaftRadius As Single = 0.0F
    ''' <summary>upper end of the shaft</summary>
    Public Property ShaftTop As Single = 0.0F

    ''' <summary>
    ''' angular velocity around +Z [rad per time unit]. positive = counter
    ''' clockwise when looking down from +Z.
    ''' </summary>
    Public Property AngularVelocity As Single

    ''' <summary>axial (pumping) velocity applied inside the impeller region</summary>
    Public Property AxialVelocity As Single = 0.0F

    ''' <summary>
    ''' thickness of the velocity forcing band around the solid body: the
    ''' blending weight decays linearly from 1 (on the surface) to 0 at this
    ''' distance.
    ''' </summary>
    Public Property Influence As Single = 1.0F

    ''' <summary>
    ''' how fast the particle velocity is blended toward the impeller surface
    ''' velocity [1 / time unit]. the effective per step blend factor is
    ''' <c>min(1, BlendRate * weight * dt)</c> so the forcing stays stable for
    ''' any step size.
    ''' </summary>
    Public Property BlendRate As Single = 30.0F

    ''' <summary>accumulated rotation angle (for reporting / animation)</summary>
    Public Property Angle As Single

    ''' <summary>current revolution per minute (derived from <see cref="AngularVelocity"/>)</summary>
    Public ReadOnly Property Rpm As Single
        Get
            Return AngularVelocity * 60.0F / (2 * PI)
        End Get
    End Property

    Sub New(centerX As Single, centerY As Single, zCenter As Single,
            radius As Single, bladeHeight As Single, angularVelocity As Single)

        Me.CenterX = centerX
        Me.CenterY = centerY
        Me.ZCenter = zCenter
        Me.Radius = radius
        Me.BladeHeight = bladeHeight
        Me.AngularVelocity = angularVelocity
    End Sub

    ''' <summary>set the rotation speed in revolution per minute</summary>
    Public Sub SetRpm(rpm As Single)
        AngularVelocity = rpm * 2 * PI / 60.0F
    End Sub

    ''' <summary>advance the rotation angle</summary>
    Public Sub Advance(dt As Single)
        Angle += AngularVelocity * dt
    End Sub

    ''' <summary>
    ''' squared distance from the particle position to the impeller solid body
    ''' (0 when the particle is inside the solid).
    ''' </summary>
    Private Function DistanceToBody(x As Single, y As Single, z As Single) As Single
        Dim dx = x - CenterX
        Dim dy = y - CenterY
        Dim r = CSng(Sqrt(dx * dx + dy * dy))
        Dim outR = r - Radius
        Dim dz = Abs(z - ZCenter) - BladeHeight * 0.5F
        Dim dDisc As Single

        If outR <= 0 AndAlso dz <= 0 Then
            dDisc = 0
        Else
            Dim a = If(outR > 0, outR, 0.0F)
            Dim b = If(dz > 0, dz, 0.0F)
            dDisc = CSng(Sqrt(a * a + b * b))
        End If

        If ShaftRadius > 0 AndAlso z >= ZCenter - BladeHeight * 0.5F AndAlso z <= ShaftTop Then
            Dim dShaft = r - ShaftRadius
            If dShaft < 0 Then dShaft = 0
            If dShaft < dDisc Then Return dShaft
        End If

        Return dDisc
    End Function

    ''' <summary>
    ''' test whether the given point is inside the impeller solid body.
    ''' </summary>
    Public Function IsInside(x As Single, y As Single, z As Single) As Boolean
        Return DistanceToBody(x, y, z) <= 0.0F
    End Function

    ''' <summary>
    ''' apply the moving boundary forcing: blend the velocity of the particles
    ''' that live inside (or close to) the impeller body toward the impeller
    ''' surface velocity.
    ''' </summary>
    ''' <param name="state">the SoA particle state</param>
    ''' <param name="dt">sub step size</param>
    Public Sub Drive(state As SphState3D, dt As Single)
        If AngularVelocity = 0 AndAlso AxialVelocity = 0 Then Return

        Dim n = state.Count
        Dim band = If(Influence > 0, Influence, 0.000001F)
        Dim omega = AngularVelocity

        For i As Integer = 0 To n - 1
            Dim d = DistanceToBody(state.px(i), state.py(i), state.pz(i))
            If d > band Then Continue For

            Dim w = 1.0F - d / band
            If w <= 0 Then Continue For

            Dim blend = BlendRate * w * dt
            If blend > 1.0F Then blend = 1.0F
            If blend <= 0 Then Continue For

            Dim dx = state.px(i) - CenterX
            Dim dy = state.py(i) - CenterY

            ' v = omega x r
            Dim tvx = -omega * dy
            Dim tvy = omega * dx
            Dim tvz = AxialVelocity

            state.vx(i) += (tvx - state.vx(i)) * blend
            state.vy(i) += (tvy - state.vy(i)) * blend
            state.vz(i) += (tvz - state.vz(i)) * blend
        Next
    End Sub

End Class
