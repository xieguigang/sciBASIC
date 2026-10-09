#Region "Microsoft.VisualBasic::ba3069c9d56188f836ca538a920b05fb, gr\physics\Particles\FluidEngine3D.vb"

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

    '   Total Lines: 353
    '    Code Lines: 235 (66.57%)
    ' Comment Lines: 54 (15.30%)
    '    - Xml Docs: 72.22%
    ' 
    '   Blank Lines: 64 (18.13%)
    '     File Size: 13.32 KB


    ' Class FluidEngine3D
    ' 
    '     Properties: BoxSize, CollisionDamping, Count, DeltaTime, DisturbAccel
    '                 DisturbDamping, Entity, Gravity, NearPressureMultiplier, ParticleSize
    '                 PressureMultiplier, SmoothingRadius, TargetDensity, ViscosityStrength
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: CalculateDensity, ComputeExternalForce, NearPressureFromDensity, PressureFromDensity
    ' 
    '     Sub: ApplyExternalForces, CalculateDensities, CalculatePressureForce, CalculateViscosity, DecayDisturbance
    '          HandleCollisions, Reset, RunDebugStep, RunSimulationStep, UpdatePositions
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math
Imports par = System.Threading.Tasks.Parallel

''' <summary>
''' A 3D Smoothed Particle Hydrodynamics (SPH) fluid simulation engine.
''' 
''' This is the three dimensional counterpart of the 2D <see cref="FluidEngine"/>.
''' The simulation pipeline of one (sub) step is:
''' 
''' 1. apply the external forces (gravity + shake disturbance) and predict positions
''' 2. rebuild the flat uniform grid (<see cref="UniformGrid3D"/>) for the neighbour search
''' 3. calculate the density and near density of each particle
''' 4. calculate the pressure force (momentum conserving symmetric form)
''' 5. calculate the viscosity force (density normalized, clamped)
''' 6. integrate, then resolve the boundary / moving impeller constraints
''' 
''' The fluid is contained inside an <see cref="SphBoundary3D"/> which defaults to
''' the axis aligned box volume <c>[0,BoxSize.x] x [0,BoxSize.y] x [0,BoxSize.z]</c>
''' and gravity pulls the water along <see cref="GravityDirection"/> (by default
''' the +Y face, i.e. the bottom of the legacy box container). Cylindrical
''' vessels (<see cref="CylinderBoundary3D"/>) and rotating impellers
''' (<see cref="SphImpeller3D"/>) can be plugged in for stirred tank scenarios.
''' </summary>
''' <remarks>
''' Optimized revision: the particle state has been moved from an array of
''' objects into Structure-of-Arrays buffers (<see cref="SphState3D"/>), the
''' spatial hash from a <c>Dictionary(Of (Integer,Integer,Integer), ...)</c> into a
''' counting sorted flat grid, and every pass is now a race free gather
''' (read only neighbourhood, write only into the own slot) so that the
''' <c>Parallel.For</c> passes are deterministic and match the CUDA backend.
''' </remarks>
Public Class FluidEngine3D : Implements IContainer3D(Of Particle3D)

    ' Settings, exposed as read/write properties so that they can be bound to
    ' the WinForm PropertyGrid and tuned live.
    '
    ' NOTE on the scales: the engine now works with the density normalized by
    ' the (calibrated) rest density, i.e. rho = n / n0 is ~1 inside the liquid.
    ' The pressure stiffness is derived from an artificial sound speed
    ' (K = c^2) instead of being a bare multiplier, which keeps the solver
    ' weakly compressible (~1% density error) and stable for any scene size.
    Public Property Gravity As Single = 100
    Public Property DeltaTime As Single = 1 / 60
    Public Property CollisionDamping As Single = 0.5
    Public Property TargetDensity As Single = 0.00034F
    Public Property PressureMultiplier As Single = 5
    Public Property NearPressureMultiplier As Single = 2
    Public Property ViscosityStrength As Single = 1.5
    Public Property ParticleSize As Single = 4

    ''' <summary>
    ''' The SPH smoothing radius. changing this rebuilds the kernels.
    ''' </summary>
    Public Property SmoothingRadius As Single
        Get
            Return _smoothingRadius
        End Get
        Set(value As Single)
            If value <= 0 Then value = 1
            _smoothingRadius = value
            Kernels = New FluidKernels3D(value)
            paramsDirty = True
            gridStale = True
        End Set
    End Property

    Private _smoothingRadius As Single = 25

    ''' <summary>
    ''' the size(x/y/z extent) of the box volume that holds the fluid.
    ''' </summary>
    Public Property BoxSize As Vector3 Implements IContainer3D(Of Particle3D).BoxSize
        Get
            Dim d = Boundary.Domain()
            Return New Vector3(d.maxX - d.minX, d.maxY - d.minY, d.maxZ - d.minZ)
        End Get
        Set(value As Vector3)
            If Boundary Is Nothing OrElse Not TypeOf Boundary Is BoxBoundary3D Then
                Boundary = New BoxBoundary3D(CSng(value.x), CSng(value.y), CSng(value.z))
            Else
                Dim box = DirectCast(Boundary, BoxBoundary3D)
                box.SizeX = CSng(value.x)
                box.SizeY = CSng(value.y)
                box.SizeZ = CSng(value.z)
            End If
            gridStale = True
        End Set
    End Property

    ''' <summary>
    ''' An external disturbance acceleration injected by the host application
    ''' (for example, the inertial force produced when the user shakes the
    ''' window on the desktop). the engine decays this value every step so the
    ''' water naturally settles down again after a shake.
    ''' </summary>
    Public Property DisturbAccel As Vector3 = Vector3.Zero

    ''' <summary>
    ''' the exponential decay factor(per step) applied to <see cref="DisturbAccel"/>.
    ''' </summary>
    Public Property DisturbDamping As Single = 0.85

    ''' <summary>
    ''' unit vector of the gravity direction (legacy box scenes pull the water
    ''' toward +Y; a fermenter with the Z axis pointing up uses (0,0,-1)).
    ''' </summary>
    Public Property GravityDirection As Vector3 = New Vector3(0, 1, 0)

#Region "扩展：边界 / 搅拌桨 / 后端 / 稳定性"

    ''' <summary>the computational domain (box by default, cylinder for a fermenter)</summary>
    Public Property Boundary As SphBoundary3D

    ''' <summary>an optional rotating impeller (moving boundary forcing)</summary>
    Public Property Impeller As SphImpeller3D

    ''' <summary>
    ''' the pluggable compute backend; defaults to the parallel CPU backend.
    ''' a CUDA implementation can be injected from the host application.
    ''' </summary>
    Public Property Backend As ISphCompute3D

    ''' <summary>
    ''' the particle spacing of the initial fill (used by the CFL condition);
    ''' 0 defaults to SmoothingRadius / 2.
    ''' </summary>
    Public Property ParticleSpacing As Single = 0.0F

    ''' <summary>
    ''' when true the rest density is measured from the current particle fill
    ''' on the first step (or when <see cref="CalibrateDensity"/> is called).
    ''' </summary>
    Public Property AutoCalibrateDensity As Boolean = True

    ''' <summary>the calibrated rest density of the (h-r)^2 kernel sum</summary>
    Public Property RestDensity As Single = 0.0F

    ''' <summary>the calibrated rest density of the (h-r)^3 kernel sum</summary>
    Public Property RestNearDensity As Single = 0.0F

    ''' <summary>
    ''' when true the pressure stiffness is derived from the artificial sound
    ''' speed instead of using <see cref="PressureMultiplier"/> literally.
    ''' </summary>
    Public Property AutoPressure As Boolean = True

    ''' <summary>the artificial sound speed c (0 = derive from the scene)</summary>
    Public Property SoundSpeed As Single = 0.0F

    ''' <summary>c / reference velocity ratio; ~8 keeps the density error near 1%</summary>
    Public Property SoundSpeedFactor As Single = 8.0F

    ''' <summary>
    ''' the reference velocity used to derive the sound speed
    ''' (0 = estimate: sqrt(2*g*H) plus the impeller tip speed).
    ''' </summary>
    Public Property ReferenceSpeed As Single = 0.0F

    ''' <summary>near pressure stiffness, as a fraction of the pressure stiffness</summary>
    Public Property NearPressureRatio As Single = 0.1F

    ''' <summary>CFL coefficient of the acoustic time step limit</summary>
    Public Property CflFactor As Single = 0.35F

    ''' <summary>upper bound of the sub steps per <see cref="RunSimulationStep"/> call</summary>
    Public Property MaxSubSteps As Integer = 64

    ''' <summary>velocity clamp (0 = disabled)</summary>
    Public Property MaxVelocity As Single = 0.0F

    ''' <summary>acceleration clamp (0 = disabled)</summary>
    Public Property MaxAccel As Single = 0.0F

    ''' <summary>
    ''' the position prediction factor of the sub step; 0 defaults to the sub
    ''' step size itself (the legacy engine used a hard coded 1/120 which was
    ''' inconsistent with <see cref="DeltaTime"/>).
    ''' </summary>
    Public Property PredictionFactor As Single = 0.0F

    ''' <summary>wall padding override (0 = use <see cref="ParticleSize"/>)</summary>
    Public Property WallPadding As Single = 0.0F

    ''' <summary>simulated time (accumulated)</summary>
    Public Property Time As Double = 0.0

    ''' <summary>number of executed steps</summary>
    Public Property StepCount As Integer = 0

    ''' <summary>number of sub steps used by the most recent step</summary>
    Public Property LastSubSteps As Integer = 1

    ''' <summary>the (cached) artificial sound speed of the current setup</summary>
    Public ReadOnly Property EffectiveSoundSpeed As Single
        Get
            Return soundSpeedCache
        End Get
    End Property

    ''' <summary>the (cached) pressure stiffness K = c^2 of the current setup</summary>
    Public ReadOnly Property EffectivePressureK As Single
        Get
            Return pressureKCache
        End Get
    End Property

    ''' <summary>the SoA particle state (shared with the compute backends)</summary>
    Public ReadOnly Property State As SphState3D
        Get
            Return _state
        End Get
    End Property

    ''' <summary>the flat uniform grid of the neighbour search</summary>
    Public ReadOnly Property Grid As UniformGrid3D
        Get
            Return _grid
        End Get
    End Property

    ''' <summary>the kernel bundle of the current smoothing radius</summary>
    Public ReadOnly Property Kernels3D As FluidKernels3D
        Get
            Return Kernels
        End Get
    End Property

    ''' <summary>the flat parameter bundle that is handed to the compute backend</summary>
    Public ReadOnly Property Params As SphParams3D
        Get
            Call EnsureParams()
            Return sphParams
        End Get
    End Property

#End Region

    ReadOnly NumParticles As Integer

    Private Kernels As FluidKernels3D

    ' Buffers
    Private Particles As Particle3D()
    Private _state As SphState3D
    Private _grid As UniformGrid3D
    Private sphParams As SphParams3D
    Private paramsDirty As Boolean = True
    Private gridStale As Boolean = True
    Private viewDirty As Boolean = True
    Private soundSpeedCache As Single = 0.0F
    Private pressureKCache As Single = 0.0F
    Private cpuFallback As New SphCpuCompute3D()

    ''' <summary>
    ''' the particles of this simulation (legacy array-of-structures view).
    ''' </summary>
    Public ReadOnly Property Entity As IReadOnlyCollection(Of Particle3D) Implements IContainer3D(Of Particle3D).Entity
        Get
            If viewDirty Then
                Call _state.SyncToParticles(Particles)
                viewDirty = False
            End If
            Return Particles
        End Get
    End Property

    ''' <summary>
    ''' the total number of the water particles inside this simulation.
    ''' </summary>
    Public ReadOnly Property Count As Integer
        Get
            Return _state.Count
        End Get
    End Property

    ''' <summary>
    ''' create the engine for <paramref name="n"/> particles inside a box volume.
    ''' the particles are scattered randomly inside the box (legacy behaviour).
    ''' </summary>
    Sub New(n As Integer, box As Vector3, Optional smoothingRadius As Single = 25)
        Me.NumParticles = n
        Me.SmoothingRadius = smoothingRadius
        Me.Boundary = New BoxBoundary3D(CSng(box.x), CSng(box.y), CSng(box.z))
        Me.Particles = New Particle3D(std.Max(0, n) - 1) {}

        For i As Integer = 0 To n - 1
            Particles(i) = New Particle3D(i, box)
        Next

        Me._state = New SphState3D(std.Max(1, n))
        Me._state.SyncFromParticles(Particles)
        Me.gridStale = True
    End Sub

    ''' <summary>
    ''' create the engine for an arbitrary (cylindrical) domain. no particle is
    ''' created: the host fills <see cref="State"/> and then calls
    ''' <see cref="SetParticleCount"/>.
    ''' </summary>
    Sub New(boundary As SphBoundary3D, Optional smoothingRadius As Single = 25)
        Me.NumParticles = 0
        Me.SmoothingRadius = smoothingRadius
        Me.Boundary = boundary
        Me.Particles = New Particle3D(-1) {}
        Me._state = New SphState3D(1)
        Me._state.Count = 0
        Me.gridStale = True
    End Sub

    ''' <summary>
    ''' set the active particle count after the host filled <see cref="State"/>.
    ''' </summary>
    Public Sub SetParticleCount(n As Integer)
        If n < 0 Then n = 0
        Call _state.EnsureCapacity(n)
        _state.Count = n

        If n = 0 Then
            Particles = New Particle3D(-1) {}
        Else
            ReDim Particles(n - 1)
        End If

        viewDirty = True
        gridStale = True
    End Sub

    ''' <summary>
    ''' re-scatter all of the particles randomly inside the current domain
    ''' and clear their velocities.
    ''' </summary>
    Public Sub Reset()
        Dim d = Boundary.Domain()
        Dim box As New Vector3(d.maxX - d.minX, d.maxY - d.minY, d.maxZ - d.minZ)
        Dim n = _state.Count

        For i As Integer = 0 To n - 1
            _state.px(i) = CSng(Vector3.Random(box).x + d.minX)
            _state.py(i) = CSng(Vector3.Random(box).y + d.minY)
            _state.pz(i) = CSng(Vector3.Random(box).z + d.minZ)
        Next

        Call _state.ClearDynamic()
        DisturbAccel = Vector3.Zero
        viewDirty = True
        gridStale = True
    End Sub

#Region "主循环"

    ''' <summary>single threaded (debug) variant of <see cref="RunSimulationStep"/></summary>
    Public Sub RunDebugStep()
        Dim save = Backend
        Backend = cpuFallback
        Call RunStep(DeltaTime)
        Backend = save
    End Sub

    ''' <summary>
    ''' advance the simulation by <see cref="DeltaTime"/>.
    ''' </summary>
    Public Sub RunSimulationStep()
        Call RunStep(DeltaTime)
    End Sub

    ''' <summary>
    ''' advance the simulation by the given time step (CFL sub stepped).
    ''' </summary>
    Public Sub RunStep(dt As Single)
        If dt <= 0 Then Return

        Call EnsureParams()

        Dim n = _state.Count
        If n = 0 Then Return

        Dim g = ComputeExternalForce()
        Dim subCount = ComputeSubSteps(dt)
        Dim dtSub = dt / subCount

        For s As Integer = 1 To subCount
            Call RunSubStep(dtSub, g.x, g.y, g.z)
        Next

        Call DecayDisturbance()

        LastSubSteps = subCount
        Time += dt
        StepCount += 1
        viewDirty = True
    End Sub

    ''' <summary>
    ''' one CFL sub step: predict -> grid -> density+forces -> integrate ->
    ''' impeller -> boundary.
    ''' </summary>
    Private Sub RunSubStep(dt As Single, gx As Double, gy As Double, gz As Double)
        Dim n = _state.Count
        Dim st = _state

        EnsureGrid()

        ' ---- 1) external force + predicted position ----
        Dim predict = If(PredictionFactor > 0, PredictionFactor, dt)
        Dim maxV2 = If(MaxVelocity > 0, MaxVelocity * MaxVelocity, 0.0F)
        Dim gxf = CSng(gx), gyf = CSng(gy), gzf = CSng(gz)

        Dim predictBody As Action(Of Integer) = Sub(i)
                                                    Dim nvx = st.vx(i) + gxf * dt
                                                    Dim nvy = st.vy(i) + gyf * dt
                                                    Dim nvz = st.vz(i) + gzf * dt

                                                    If maxV2 > 0 Then
                                                        Dim s2 = nvx * nvx + nvy * nvy + nvz * nvz
                                                        If s2 > maxV2 Then
                                                            Dim f = CSng(MaxVelocity / std.Sqrt(s2))
                                                            nvx *= f : nvy *= f : nvz *= f
                                                        End If
                                                    End If

                                                    st.vx(i) = nvx
                                                    st.vy(i) = nvy
                                                    st.vz(i) = nvz

                                                    st.qx(i) = st.px(i) + nvx * predict
                                                    st.qy(i) = st.py(i) + nvy * predict
                                                    st.qz(i) = st.pz(i) + nvz * predict
                                                End Sub

        RunParallel(n, predictBody)

        ' ---- 2) neighbour search ----
        Call _grid.Build(st, predicted:=True)

        ' ---- 3) density + pressure + viscosity (backend) ----
        Dim ok As Boolean = False

        If Backend IsNot Nothing Then
            ok = Backend.RunSubstep(st, _grid, sphParams, dt)
        End If
        If Not ok Then
            Call cpuFallback.RunSubstep(st, _grid, sphParams, dt)
        End If

        ' ---- 4) commit: integrate velocity and position ----
        Dim commitBody As Action(Of Integer) = Sub(i)
                                                   Dim nvx = st.vx(i) + st.ax(i) * dt
                                                   Dim nvy = st.vy(i) + st.ay(i) * dt
                                                   Dim nvz = st.vz(i) + st.az(i) * dt

                                                   If maxV2 > 0 Then
                                                       Dim s2 = nvx * nvx + nvy * nvy + nvz * nvz
                                                       If s2 > maxV2 Then
                                                           Dim f = CSng(MaxVelocity / std.Sqrt(s2))
                                                           nvx *= f : nvy *= f : nvz *= f
                                                       End If
                                                   End If

                                                   st.vx(i) = nvx
                                                   st.vy(i) = nvy
                                                   st.vz(i) = nvz

                                                   st.px(i) += nvx * dt
                                                   st.py(i) += nvy * dt
                                                   st.pz(i) += nvz * dt
                                               End Sub

        RunParallel(n, commitBody)

        ' ---- 5) moving impeller + boundary projection ----
        Dim padding = If(WallPadding > 0, WallPadding, ParticleSize)
        Dim impeller = Me.Impeller
        Dim boundary = Me.Boundary
        Dim damping = CollisionDamping

        If impeller IsNot Nothing Then
            Call impeller.Drive(st, dt)
            Call impeller.Advance(dt)
        End If

        If boundary IsNot Nothing Then
            Dim wallBody As Action(Of Integer) = Sub(i)
                                                     Call boundary.Project(st, i, padding, damping)
                                                 End Sub
            RunParallel(n, wallBody)
        End If
    End Sub

    Private Shared Sub RunParallel(n As Integer, body As Action(Of Integer))
        If n < 2048 Then
            For i As Integer = 0 To n - 1
                Call body(i)
            Next
        Else
            Call par.For(0, n, body)
        End If
    End Sub

    ''' <summary>
    ''' the CFL limited number of sub steps for the requested step size.
    ''' </summary>
    Private Function ComputeSubSteps(dt As Single) As Integer
        Dim spacing = If(ParticleSpacing > 0, ParticleSpacing, SmoothingRadius * 0.5F)
        Dim c = std.Max(0.000001F, soundSpeedCache)
        Dim vmax = _state.MaxSpeed()

        Dim limit = CflFactor * SmoothingRadius / c

        If vmax > 0.000001F Then
            Dim l2 = CflFactor * spacing / vmax
            If l2 < limit Then limit = l2
        End If

        If limit <= 0 Then limit = dt

        Dim n = CInt(std.Ceiling(dt / limit))
        If n < 1 Then n = 1
        If n > MaxSubSteps Then n = MaxSubSteps

        Return n
    End Function

    ''' <summary>
    ''' decay the external shake disturbance so the water settles back down.
    ''' </summary>
    Private Sub DecayDisturbance()
        DisturbAccel = DisturbAccel * DisturbDamping

        If DisturbAccel.Magnitude < 0.001 Then
            DisturbAccel = Vector3.Zero
        End If
    End Sub

    Private Function ComputeExternalForce() As Vector3
        ' Gravity pulls the water along GravityDirection, plus the shake
        ' disturbance acceleration injected by the host.
        Return New Vector3(
            GravityDirection.x * Gravity + DisturbAccel.x,
            GravityDirection.y * Gravity + DisturbAccel.y,
            GravityDirection.z * Gravity + DisturbAccel.z)
    End Function

#End Region

#Region "参数 / 网格 / 校准"

    Private Sub EnsureParams()
        If Not paramsDirty AndAlso sphParams IsNot Nothing Then Return

        If Kernels Is Nothing Then
            Kernels = New FluidKernels3D(_smoothingRadius)
        End If

        sphParams = SphParams3D.FromKernels(Kernels, _smoothingRadius)
        paramsDirty = False

        If RestDensity <= 0 Then
            If AutoCalibrateDensity Then
                Call CalibrateDensity()
            Else
                RestDensity = TargetDensity
                If RestNearDensity <= 0 Then RestNearDensity = TargetDensity
            End If
        End If

        sphParams.RestDensity = RestDensity
        sphParams.RestNearDensity = If(RestNearDensity > 0, RestNearDensity, RestDensity)

        ' ---- 压力刚度（人工声速）----
        Dim c = SoundSpeed
        If c <= 0 Then
            Dim vref = ReferenceSpeed
            If vref <= 0 Then vref = EstimateReferenceSpeed()
            c = SoundSpeedFactor * vref
        End If

        Dim K As Single = c * c

        If Not AutoPressure Then
            K = PressureMultiplier
            c = CSng(std.Sqrt(std.Max(0.000001F, K)))
        End If

        soundSpeedCache = c
        pressureKCache = K

        sphParams.PressureK = K
        sphParams.NearPressureK = NearPressureRatio * K
        sphParams.Viscosity = ViscosityStrength
        sphParams.MaxAccel = MaxAccel
    End Sub

    ''' <summary>
    ''' estimate the reference velocity of the scene: the free fall speed over
    ''' the domain height plus the impeller tip speed.
    ''' </summary>
    Private Function EstimateReferenceSpeed() As Single
        Dim d = Boundary.Domain()
        Dim h = std.Max(d.maxZ - d.minZ, std.Max(d.maxX - d.minX, d.maxY - d.minY))
        Dim v = std.Sqrt(std.Max(0.0F, 2.0F * Gravity * h))

        If Impeller IsNot Nothing Then
            v += std.Abs(Impeller.AngularVelocity * Impeller.Radius)
        End If

        If v < 0.001F Then v = 1.0F
        Return CSng(v)
    End Function

    Private Sub EnsureGrid()
        Dim d = Boundary.Domain()
        Dim need = gridStale OrElse _grid Is Nothing OrElse
                   _grid.CellSize <> _smoothingRadius OrElse
                   _grid.OriginX <> d.minX OrElse _grid.OriginY <> d.minY OrElse _grid.OriginZ <> d.minZ

        If Not need Then
            ' 域尺寸变化时也需要重建
            If _grid.Nx * _grid.CellSize < (d.maxX - d.minX) OrElse
               _grid.Ny * _grid.CellSize < (d.maxY - d.minY) OrElse
               _grid.Nz * _grid.CellSize < (d.maxZ - d.minZ) Then
                need = True
            End If
        End If

        If Not need Then Return

        Dim margin = _smoothingRadius
        _grid = New UniformGrid3D(_smoothingRadius,
                                  d.minX - margin, d.minY - margin, d.minZ - margin,
                                  d.maxX + margin, d.maxY + margin, d.maxZ + margin,
                                  std.Max(1, _state.Count))
        gridStale = False
    End Sub

    ''' <summary>
    ''' measure the rest density of the current particle fill: run one density
    ''' pass and take the mean of the densest quartile (the interior particles
    ''' of a liquid block).
    ''' </summary>
    Public Sub CalibrateDensity()
        Dim n = _state.Count
        If n = 0 Then Return

        If Kernels Is Nothing Then Kernels = New FluidKernels3D(_smoothingRadius)
        If sphParams Is Nothing Then
            sphParams = SphParams3D.FromKernels(Kernels, _smoothingRadius)
        End If

        EnsureGrid()

        Array.Copy(_state.px, _state.qx, n)
        Array.Copy(_state.py, _state.qy, n)
        Array.Copy(_state.pz, _state.qz, n)

        Call _grid.Build(_state, predicted:=True)
        Call cpuFallback.DensityPass(_state, _grid, sphParams)

        RestDensity = TopQuartileMean(_state.dens, n)
        RestNearDensity = TopQuartileMean(_state.densNear, n)

        If RestDensity <= 0.0000001F Then RestDensity = 1.0F
        If RestNearDensity <= 0.0000001F Then RestNearDensity = 1.0F

        sphParams.RestDensity = RestDensity
        sphParams.RestNearDensity = RestNearDensity

        ' 密度变了 → 压力刚度（依赖声速估计）需要重算
        paramsDirty = False
        soundSpeedCache = 0
        Call EnsurePressureScale()
    End Sub

    Private Sub EnsurePressureScale()
        Dim c = SoundSpeed
        If c <= 0 Then
            Dim vref = ReferenceSpeed
            If vref <= 0 Then vref = EstimateReferenceSpeed()
            c = SoundSpeedFactor * vref
        End If

        Dim K As Single = c * c
        If Not AutoPressure Then
            K = PressureMultiplier
            c = CSng(std.Sqrt(std.Max(0.000001F, K)))
        End If

        soundSpeedCache = c
        pressureKCache = K
        sphParams.PressureK = K
        sphParams.NearPressureK = NearPressureRatio * K
    End Sub

    Private Shared Function TopQuartileMean(buf As Single(), n As Integer) As Single
        Dim tmp As Single() = New Single(n - 1) {}
        Array.Copy(buf, tmp, n)
        Array.Sort(tmp)

        Dim from = n - std.Max(1, n \ 4)
        Dim sum As Double = 0
        Dim count = 0

        For i As Integer = from To n - 1
            sum += tmp(i)
            count += 1
        Next

        If count = 0 Then Return buf(0)
        Return CSng(sum / count)
    End Function

    ''' <summary>force a rebuild of the cached parameters on the next step</summary>
    Public Sub InvalidateParams()
        paramsDirty = True
        gridStale = True
        soundSpeedCache = 0
    End Sub

#End Region

#Region "legacy 兼容查询"

    ''' <summary>density and near density of the particle at the given position</summary>
    Private Function CalculateDensity(pos As Vector3, id As Integer) As Vector3
        Return New Vector3(_state.dens(id), _state.densNear(id), 0)
    End Function

    ''' <summary>pressure of the given (normalized) density</summary>
    Private Function PressureFromDensity(density As Single) As Single
        Return pressureKCache * (density / If(RestDensity > 0, RestDensity, 1.0F) - 1.0F)
    End Function

    ''' <summary>near pressure of the given near density</summary>
    Private Function NearPressureFromDensity(nearDensity As Single) As Single
        Return NearPressureRatio * pressureKCache * (nearDensity / If(RestNearDensity > 0, RestNearDensity, 1.0F))
    End Function

    ''' <summary>the pressure of the given particle</summary>
    Public Function GetPressure(i As Integer) As Single
        Return _state.press(i)
    End Function

    ''' <summary>the density of the given particle</summary>
    Public Function GetDensity(i As Integer) As Single
        Return _state.dens(i)
    End Function

    ''' <summary>the normalized density (1 = rest density) of the given particle</summary>
    Public Function GetNormalizedDensity(i As Integer) As Single
        Return _state.dens(i) / If(RestDensity > 0, RestDensity, 1.0F)
    End Function

#End Region

End Class
