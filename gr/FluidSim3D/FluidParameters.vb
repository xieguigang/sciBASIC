' /********************************************************************************/
'
'     FluidParameters - a PropertyGrid bindable bag of the 3D SPH simulation
'     parameters. the host form copies these values into the FluidEngine3D
'     and rebuilds the engine when the particle count or box size changes.
'
' /********************************************************************************/

Imports System.ComponentModel

Namespace FluidSim3D

    ''' <summary>
    ''' tunable parameters for the 3D water simulation, designed to be edited
    ''' live through a WinForm <see cref="PropertyGrid"/>.
    ''' </summary>
    Public Class FluidParameters

        <Category("模拟 Simulation")>
        <Description("水粒子的总数量。修改后需要点击「应用参数」重建模拟器。")>
        <DisplayName("粒子数 ParticleCount")>
        Public Property ParticleCount As Integer = 1500

        <Category("模拟 Simulation")>
        <Description("SPH 平滑半径，决定每个粒子影响邻域的范围（世界单位）。")>
        <DisplayName("平滑半径 SmoothingRadius")>
        Public Property SmoothingRadius As Single = 25.0F

        <Category("模拟 Simulation")>
        <Description("单个水粒子在屏幕上绘制时的半径（像素）。")>
        <DisplayName("粒子大小 ParticleSize")>
        Public Property ParticleSize As Single = 4.0F

        <Category("物理 Physics")>
        <Description("重力大小，方向指向容器 +Y 面（容器底部）。")>
        <DisplayName("重力 Gravity")>
        Public Property Gravity As Single = 100.0F

        <Category("物理 Physics")>
        <Description("每个模拟步的时间步长（秒）。")>
        <DisplayName("时间步长 DeltaTime")>
        Public Property DeltaTime As Single = 1 / 60.0F

        <Category("物理 Physics")>
        <Description("目标密度，流体在该密度下压力为零。")>
        <DisplayName("目标密度 TargetDensity")>
        Public Property TargetDensity As Single = 5.0F

        <Category("物理 Physics")>
        <Description("压力系数，越大流体越「硬」、越不易压缩。")>
        <DisplayName("压力系数 PressureMultiplier")>
        Public Property PressureMultiplier As Single = 20.0F

        <Category("物理 Physics")>
        <Description("近压力系数，用于防止粒子互相穿透。")>
        <DisplayName("近压力系数 NearPressureMultiplier")>
        Public Property NearPressureMultiplier As Single = 8.0F

        <Category("物理 Physics")>
        <Description("黏度强度，让相邻粒子速度趋于一致，使水流更平滑。")>
        <DisplayName("黏度 ViscosityStrength")>
        Public Property ViscosityStrength As Single = 0.06F

        <Category("物理 Physics")>
        <Description("边界碰撞反弹的阻尼系数（0~1，越小越「黏」）。")>
        <DisplayName("碰撞阻尼 CollisionDamping")>
        Public Property CollisionDamping As Single = 0.35F

        <Category("容器 Container")>
        <Description("容纳水模拟的长方体在 X 方向的尺寸（世界单位）。")>
        <DisplayName("容器宽 BoxX")>
        Public Property BoxX As Single = 200.0F

        <Category("容器 Container")>
        <Description("容纳水模拟的长方体在 Y 方向的尺寸（世界单位，重力方向）。")>
        <DisplayName("容器高 BoxY")>
        Public Property BoxY As Single = 200.0F

        <Category("容器 Container")>
        <Description("容纳水模拟的长方体在 Z 方向的尺寸（世界单位）。")>
        <DisplayName("容器深 BoxZ")>
        Public Property BoxZ As Single = 200.0F

        <Category("扰动 Disturbance")>
        <Description("晃动窗口时注入到流体的扰动灵敏度，越大水被晃得越剧烈。")>
        <DisplayName("晃动灵敏度 ShakeSensitivity")>
        Public Property ShakeSensitivity As Single = 1.0F

        ''' <summary>
        ''' build a physics Vector3 describing the box volume from the three
        ''' scalar box dimensions.
        ''' </summary>
        Public Function ToBoxSize() As Microsoft.VisualBasic.Imaging.Physics.Vector3
            Return New Microsoft.VisualBasic.Imaging.Physics.Vector3(BoxX, BoxY, BoxZ)
        End Function

    End Class

End Namespace
