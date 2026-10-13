#Region "Microsoft.VisualBasic::772e4170d6ff45578325a6d602762201, physics\Particles\SphParams3D.vb"

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

    '   Total Lines: 156
    '    Code Lines: 51 (32.69%)
    ' Comment Lines: 86 (55.13%)
    '    - Xml Docs: 69.77%
    ' 
    '   Blank Lines: 19 (12.18%)
    '     File Size: 7.15 KB


    ' Class SphParams3D
    ' 
    '     Properties: GradSpikyPow2, GradSpikyPow3, MaxAccel, MaxDensityRatio, MinDensityRatio
    '                 MinPressureRatio, NearPressureK, Poly6, PressureK, RestDensity
    '                 RestNearDensity, SelfDensity, SelfNearDensity, SmoothingRadius, SpikyPow2
    '                 SpikyPow3, SqrRadius, Viscosity, VolumeScale
    ' 
    '     Function: FromKernels
    ' 
    ' Interface ISphCompute3D
    ' 
    '     Properties: Name
    ' 
    '     Function: RunSubstep
    ' 
    ' /********************************************************************************/

#End Region

' /********************************************************************************/
'
'   SphParams3D.vb
'
'   3D SPH 求解参数包 + 可插拔计算后端接口
'
'   作用：
'       把一次子步所需的全部核常数与物理系数打包成一个扁平对象：
'
'         - 核常数（缩放因子）在构造时一次性算好，热循环里只做乘加
'         - CPU 后端与 CUDA 后端消费同一份参数，保证两条路径物理一致
'         - ISphCompute3D 让 physics 项目本身**不依赖 CUDA**：
'           GPU 实现由上层（SPHEngine）注入
'
'   物理口径（归一化）：
'       - 密度用 rest density 归一化：rho = n / n0，静止液体内部 rho ≈ 1
'       - 粒子"体积/质量" m = 1 / n0，于是 rho_physical = m * n = rho（无量纲=1）
'       - 压力 p = K * (rho - 1)，K = c^2（c 为人工声速），
'         于是密度相对偏差 = p / c^2，可直接由声速控制可压缩性
'       - 压力加速度采用动量守恒的对称形式（Monaghan）：
'           a_i = -m * Σ_j (p_i/rho_i^2 + p_j/rho_j^2) * ∇_i W_ij
'
' /********************************************************************************/

''' <summary>
''' The flat parameter bundle of one 3D SPH sub step.
''' </summary>
Public Class SphParams3D

    ''' <summary>SPH smoothing radius h</summary>
    Public Property SmoothingRadius As Single
    ''' <summary>h*h (pre computed)</summary>
    Public Property SqrRadius As Single

    ''' <summary>rest density of the (h-r)^2 kernel sum</summary>
    Public Property RestDensity As Single
    ''' <summary>rest density of the (h-r)^3 near kernel sum</summary>
    Public Property RestNearDensity As Single

    ''' <summary>pressure stiffness K (= c^2, the squared artificial sound speed)</summary>
    Public Property PressureK As Single
    ''' <summary>near pressure stiffness (always repulsive, Clavet)</summary>
    Public Property NearPressureK As Single

    ''' <summary>viscosity relaxation rate [1/time]</summary>
    Public Property Viscosity As Single

    ''' <summary>upper bound of the acceleration magnitude (0 = disabled)</summary>
    Public Property MaxAccel As Single

    ''' <summary>kernel scaling factor of (h-r)^2</summary>
    Public Property SpikyPow2 As Single
    ''' <summary>kernel scaling factor of (h-r)^3</summary>
    Public Property SpikyPow3 As Single
    ''' <summary>kernel scaling factor of |d/dr (h-r)^2| (positive)</summary>
    Public Property GradSpikyPow2 As Single
    ''' <summary>kernel scaling factor of |d/dr (h-r)^3| (positive)</summary>
    Public Property GradSpikyPow3 As Single
    ''' <summary>kernel scaling factor of the poly6 viscosity kernel</summary>
    Public Property Poly6 As Single

    ''' <summary>self contribution W(0) of the density kernel</summary>
    Public Property SelfDensity As Single
    ''' <summary>self contribution W(0) of the near density kernel</summary>
    Public Property SelfNearDensity As Single

    ''' <summary>
    ''' lower clamp of the normalized density ratio: protects the symmetric
    ''' pressure term against the (near zero) density of free surface particles.
    ''' </summary>
    Public Property MinDensityRatio As Single = 0.6F
    ''' <summary>upper clamp of the normalized density ratio</summary>
    Public Property MaxDensityRatio As Single = 3.0F
    ''' <summary>
    ''' lower clamp of the pressure, given as a fraction of <see cref="PressureK"/>.
    ''' allows a mild cohesion (keeps the liquid together) but prevents the
    ''' tensile blow up of the surface particles.
    ''' </summary>
    Public Property MinPressureRatio As Single = -0.3F

    ''' <summary>particle "volume" m = 1 / RestDensity (the SPH mass of unit density)</summary>
    Public ReadOnly Property VolumeScale As Single
        Get
            If RestDensity <= 0 Then Return 1
            Return 1.0F / RestDensity
        End Get
    End Property

    ''' <summary>
    ''' build the parameter bundle from the smoothing kernels.
    ''' </summary>
    ''' <param name="kernels">
    ''' the (correctly normalized) kernels. the normalization constants are read
    ''' straight out of this bundle, so a 3D caller must pass a 3D bundle
    ''' (<c>FluidKernels.For3D</c>) and a 2D caller a 2D one.
    ''' </param>
    ''' <param name="h">smoothing radius</param>
    Public Shared Function FromKernels(kernels As FluidKernels, h As Single) As SphParams3D
        Dim p As New SphParams3D With {
            .SmoothingRadius = h,
            .SqrRadius = h * h,
            .RestDensity = 1.0F,
            .RestNearDensity = 1.0F,
            .PressureK = 100.0F,
            .NearPressureK = 10.0F,
            .Viscosity = 0.5F,
            .MaxAccel = 0.0F
        }

        ' 归一化常数一律由 FluidKernels 提供（单一数据源）：这些常数此前在本文件里
        ' 还有一份硬编码副本，与 FluidKernels/FluidKernels3D 构成三处重复，正是
        ' "改了一处忘了另一处" 这类错误的温床。kernels 缺省时按三维构造一份默认核。
        Dim k As FluidKernels = If(kernels, FluidKernels.For3D(h))

        p.SpikyPow2 = k.SpikyPow2Factor
        p.SpikyPow3 = k.SpikyPow3Factor
        p.GradSpikyPow2 = k.GradSpikyPow2Factor
        p.GradSpikyPow3 = k.GradSpikyPow3Factor
        p.Poly6 = k.Poly6Factor
        p.SelfDensity = k.SelfDensityKernel()
        p.SelfNearDensity = k.SelfNearDensityKernel()

        Return p
    End Function

End Class

''' <summary>
''' A pluggable 3D SPH compute backend. The default implementation is the
''' parallel CPU backend <see cref="SphCpuCompute3D"/>; the SPHEngine project
''' injects a CUDA implementation of the same interface so that the physics
''' library itself stays free of any CUDA dependency.
''' </summary>
Public Interface ISphCompute3D

    ''' <summary>backend display name ("CPU-Parallel", "CUDA-F32", ...)</summary>
    ReadOnly Property Name As String

    ''' <summary>
    ''' Run one SPH sub step:
    ''' 1. rebuild the density / near density of every particle
    ''' 2. evaluate the pressure + viscosity acceleration into ax/ay/az
    ''' </summary>
    ''' <param name="state">the SoA particle state (predicted positions are used)</param>
    ''' <param name="grid">the uniform grid that has been built from the predicted positions</param>
    ''' <param name="params">kernel constants and physical coefficients</param>
    ''' <param name="dt">sub step size</param>
    ''' <returns>true when the backend did the work; false = caller must fall back</returns>
    ''' <remarks>
    ''' The backend never integrates: gravity integration, the velocity clamp,
    ''' the boundary projection and the impeller forcing are applied by the host
    ''' engine so that the CPU and the GPU path stay bit comparable.
    ''' </remarks>
    Function RunSubstep(state As SphState3D, grid As UniformGrid3D, params As SphParams3D, dt As Single) As Boolean

End Interface
