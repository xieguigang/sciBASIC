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

Imports System.Math
Imports par = System.Threading.Tasks.Parallel

''' <summary>
''' A 3D Smoothed Particle Hydrodynamics (SPH) water simulation engine.
''' 
''' This is the three dimensional counterpart of the 2D <see cref="FluidEngine"/>.
''' The simulation pipeline is identical to the 2D version:
''' 
''' 1. apply the external forces(gravity + shake disturbance) and predict positions
''' 2. rebuild the spatial hash(<see cref="Grid3D"/>) for the neighbour search
''' 3. calculate the density and near density of each particle
''' 4. calculate the pressure force
''' 5. calculate the viscosity force
''' 6. integrate the positions and resolve the box boundary collisions
''' 
''' The fluid is contained inside an axis aligned box volume
''' <c>[0,BoxSize.x] x [0,BoxSize.y] x [0,BoxSize.z]</c> and gravity pulls the
''' water toward the +Y face(the bottom of the container).
''' </summary>
Public Class FluidEngine3D : Implements IContainer3D(Of Particle3D)

    ' Settings, exposed as read/write properties so that they can be bound to
    ' the WinForm PropertyGrid and tuned live.
    '
    ' NOTE: the 3D SPH density magnitude is intrinsically tiny (it scales as
    ' ~2 / s^3 with the particle spacing s). For the default 1500 particles
    ' inside a 200^3 box the rest density is roughly 3.4E-4, so TargetDensity,
    ' PressureMultiplier and NearPressureMultiplier are tuned against that
    ' scale - not against the (much larger) 2D values.
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
        End Set
    End Property

    Private _smoothingRadius As Single = 25

    ''' <summary>
    ''' the size(x/y/z extent) of the box volume that holds the fluid.
    ''' </summary>
    Public Property BoxSize As Vector3 Implements IContainer3D(Of Particle3D).BoxSize

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

    ReadOnly NumParticles As Integer

    Private Kernels As FluidKernels3D

    ' Buffers
    Private Particles As Particle3D()
    Private Spatial As Dictionary(Of (Integer, Integer, Integer), Particle3D())

    Public ReadOnly Property Entity As IReadOnlyCollection(Of Particle3D) Implements IContainer3D(Of Particle3D).Entity
        Get
            Return Particles
        End Get
    End Property

    ''' <summary>
    ''' the total number of the water particles inside this simulation.
    ''' </summary>
    Public ReadOnly Property Count As Integer
        Get
            Return NumParticles
        End Get
    End Property

    Sub New(n As Integer, box As Vector3, Optional smoothingRadius As Single = 25)
        Me.NumParticles = n
        Me.BoxSize = box
        Me.SmoothingRadius = smoothingRadius
        Me.Particles = New Particle3D(n - 1) {}

        For i As Integer = 0 To n - 1
            Particles(i) = New Particle3D(i, box)
        Next
    End Sub

    ''' <summary>
    ''' re-scatter all of the particles randomly inside the current box volume
    ''' and clear their velocities.
    ''' </summary>
    Public Sub Reset()
        For i As Integer = 0 To NumParticles - 1
            Particles(i) = New Particle3D(i, BoxSize)
        Next
        DisturbAccel = Vector3.Zero
    End Sub

    Public Sub RunDebugStep()
        For i As Integer = 0 To NumParticles - 1
            ApplyExternalForces(i)
        Next

        Spatial = Particles.EncodeGrid3D(SmoothingRadius)

        For i As Integer = 0 To NumParticles - 1
            CalculateDensities(i)
        Next
        For i As Integer = 0 To NumParticles - 1
            CalculatePressureForce(i)
        Next
        For i As Integer = 0 To NumParticles - 1
            CalculateViscosity(i)
        Next
        For i As Integer = 0 To NumParticles - 1
            UpdatePositions(i)
        Next

        DecayDisturbance()
    End Sub

    Public Sub RunSimulationStep()
        Call par.For(0, NumParticles, Sub(i) ApplyExternalForces(i))
        Spatial = Particles.EncodeGrid3D(SmoothingRadius)
        Call par.For(0, NumParticles, Sub(i) CalculateDensities(i))
        Call par.For(0, NumParticles, Sub(i) CalculatePressureForce(i))
        Call par.For(0, NumParticles, Sub(i) CalculateViscosity(i))
        Call par.For(0, NumParticles, Sub(i) UpdatePositions(i))

        Call DecayDisturbance()
    End Sub

    ''' <summary>
    ''' decay the external shake disturbance so the water settles back down.
    ''' </summary>
    Private Sub DecayDisturbance()
        DisturbAccel = DisturbAccel * DisturbDamping

        If DisturbAccel.Magnitude < 0.001 Then
            DisturbAccel = Vector3.Zero
        End If
    End Sub

    Private Function CalculateDensity(pos As Vector3, id As Integer) As Vector3
        Dim originCell = GetCell3D(pos, SmoothingRadius)
        Dim sqrRadius As Single = SmoothingRadius * SmoothingRadius
        Dim density As Single = 0
        Dim nearDensity As Single = 0
        Dim neighbours = Spatial.SpatialLookup3D(originCell)

        For Each neighbour In neighbours
            If neighbour.Index = id Then
                Continue For
            End If

            Dim offsetToNeighbour As Vector3 = neighbour.PredictedPosition - pos
            Dim sqrDstToNeighbour As Single = Vector3.Dot(offsetToNeighbour, offsetToNeighbour)

            If sqrDstToNeighbour > sqrRadius Then
                Continue For
            End If

            Dim dst As Single = Sqrt(sqrDstToNeighbour)
            density += Kernels.SpikyKernelPow2(dst, SmoothingRadius)
            nearDensity += Kernels.SpikyKernelPow3(dst, SmoothingRadius)
        Next

        Return New Vector3(density, nearDensity, 0)
    End Function

    Private Function PressureFromDensity(density As Single) As Single
        Return (density - TargetDensity) * PressureMultiplier
    End Function

    Private Function NearPressureFromDensity(nearDensity As Single) As Single
        Return NearPressureMultiplier * nearDensity
    End Function

    Private Function ComputeExternalForce() As Vector3
        ' Gravity pulls the water toward the +Y face(the bottom of the box),
        ' plus the shake disturbance acceleration injected by the host.
        Return New Vector3(DisturbAccel.x, Gravity + DisturbAccel.y, DisturbAccel.z)
    End Function

    Private Sub HandleCollisions(particleIndex As Integer)
        Dim pos As Vector3 = Particles(particleIndex).Position
        Dim vel As Vector3 = Particles(particleIndex).Velocity
        Dim padding As Single = ParticleSize

        If pos.x > BoxSize.x - padding Then
            vel.x *= -1 * CollisionDamping
            pos.x = BoxSize.x - padding
        ElseIf pos.x < padding Then
            vel.x *= -1 * CollisionDamping
            pos.x = padding
        End If

        If pos.y > BoxSize.y - padding Then
            vel.y *= -1 * CollisionDamping
            pos.y = BoxSize.y - padding
        ElseIf pos.y < padding Then
            vel.y *= -1 * CollisionDamping
            pos.y = padding
        End If

        If pos.z > BoxSize.z - padding Then
            vel.z *= -1 * CollisionDamping
            pos.z = BoxSize.z - padding
        ElseIf pos.z < padding Then
            vel.z *= -1 * CollisionDamping
            pos.z = padding
        End If

        Particles(particleIndex).Position = pos
        Particles(particleIndex).Velocity = vel
    End Sub

    Private Sub ApplyExternalForces(id As Integer)
        If id >= NumParticles Then
            Return
        End If

        Particles(id).Velocity += ComputeExternalForce() * DeltaTime

        ' Predict
        Const predictionFactor As Single = 1 / 120.0
        Particles(id).PredictedPosition = Particles(id).Position + Particles(id).Velocity * predictionFactor
    End Sub

    Private Sub CalculateDensities(id As Integer)
        If id >= NumParticles Then
            Return
        End If

        Dim pos As Vector3 = Particles(id).PredictedPosition
        Particles(id).Density = CalculateDensity(pos, id)
    End Sub

    Private Sub CalculatePressureForce(id As Integer)
        If id >= NumParticles Then
            Return
        End If

        Dim density As Single = Particles(id).Density.x
        Dim densityNear As Single = Particles(id).Density.y
        Dim pressure As Single = PressureFromDensity(density)
        Dim nearPressure As Single = NearPressureFromDensity(densityNear)
        Dim pressureForce As Vector3 = Vector3.Zero

        Dim pos As Vector3 = Particles(id).PredictedPosition
        Dim originCell = GetCell3D(pos, SmoothingRadius)
        Dim sqrRadius As Single = SmoothingRadius * SmoothingRadius
        Dim neighbours = Spatial.SpatialLookup3D(originCell)

        For Each neighbour In neighbours
            If neighbour.Index = id Then
                Continue For
            End If

            Dim offsetToNeighbour As Vector3 = neighbour.PredictedPosition - pos
            Dim sqrDstToNeighbour As Single = Vector3.Dot(offsetToNeighbour, offsetToNeighbour)

            If sqrDstToNeighbour > sqrRadius Then
                Continue For
            End If

            Dim dst As Single = Sqrt(sqrDstToNeighbour)
            Dim dirToNeighbour As Vector3 = If(dst > 0, offsetToNeighbour / dst, New Vector3(0, 1, 0))

            Dim neighbourDensity As Single = neighbour.Density.x
            Dim neighbourNearDensity As Single = neighbour.Density.y

            If neighbourDensity = 0 OrElse neighbourNearDensity = 0 Then
                Continue For
            End If

            Dim neighbourPressure As Single = PressureFromDensity(neighbourDensity)
            Dim neighbourNearPressure As Single = NearPressureFromDensity(neighbourNearDensity)

            Dim sharedPressure As Single = (pressure + neighbourPressure) * 0.5
            Dim sharedNearPressure As Single = (nearPressure + neighbourNearPressure) * 0.5

            pressureForce += dirToNeighbour * Kernels.DerivativeSpikyPow2(dst, SmoothingRadius) * sharedPressure / neighbourDensity
            pressureForce += dirToNeighbour * Kernels.DerivativeSpikyPow3(dst, SmoothingRadius) * sharedNearPressure / neighbourNearDensity
        Next

        If density > 0 Then
            Dim acceleration As Vector3 = pressureForce / density
            Particles(id).Velocity += acceleration * DeltaTime
        End If
    End Sub

    Private Sub CalculateViscosity(id As Integer)
        If id >= NumParticles Then
            Return
        End If

        Dim pos As Vector3 = Particles(id).PredictedPosition
        Dim originCell = GetCell3D(pos, SmoothingRadius)
        Dim sqrRadius As Single = SmoothingRadius * SmoothingRadius
        Dim viscosityForce As Vector3 = Vector3.Zero
        Dim velocity As Vector3 = Particles(id).Velocity
        Dim neighbours = Spatial.SpatialLookup3D(originCell)

        For Each neighbour In neighbours
            If neighbour.Index = id Then
                Continue For
            End If

            Dim offsetToNeighbour As Vector3 = neighbour.PredictedPosition - pos
            Dim sqrDstToNeighbour As Single = Vector3.Dot(offsetToNeighbour, offsetToNeighbour)

            If sqrDstToNeighbour > sqrRadius Then
                Continue For
            End If

            Dim dst As Single = Sqrt(sqrDstToNeighbour)
            Dim neighbourVelocity As Vector3 = neighbour.Velocity
            viscosityForce += (neighbourVelocity - velocity) * Kernels.SmoothingKernelPoly6(dst, SmoothingRadius)
        Next

        Particles(id).Velocity += viscosityForce * ViscosityStrength * DeltaTime
    End Sub

    Private Sub UpdatePositions(id As Integer)
        If id >= NumParticles Then
            Return
        End If

        Particles(id).Position += Particles(id).Velocity * DeltaTime
        HandleCollisions(id)
    End Sub
End Class
