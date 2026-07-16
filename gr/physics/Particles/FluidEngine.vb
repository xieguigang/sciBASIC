#Region "Microsoft.VisualBasic::63ad0f9b90d1834876540eb698d107bd, gr\physics\Particles\FluidEngine.vb"

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

    '   Total Lines: 306
    '    Code Lines: 228 (74.51%)
    ' Comment Lines: 16 (5.23%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 62 (20.26%)
    '     File Size: 11.57 KB


    ' Class FluidEngine
    ' 
    '     Properties: Entity, Height, Width
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: CalculateDensity, ComputeExternalForce, NearPressureFromDensity, PressureFromDensity
    ' 
    '     Sub: ApplyExternalForces, CalculateDensities, CalculatePressureForce, CalculateViscosity, HandleCollisions
    '          Resize, RunDebugStep, RunSimulationStep, UpdatePositions
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Math
Imports Microsoft.VisualBasic.Data.GraphTheory.GridGraph
Imports Microsoft.VisualBasic.Imaging.Physics
Imports par = System.Threading.Tasks.Parallel

Public Class FluidEngine : Implements IContainer(Of Particle)

    ' Settings
    ReadOnly NumParticles As UInteger

    Dim Gravity As Single = 100
    Dim DeltaTime As Single = 1 / 60
    Dim CollisionDamping As Single = 0.95
    Dim SmoothingRadius As Single = 50
    Dim TargetDensity As Single = 5
    Dim PressureMultiplier As Single = 5
    Dim NearPressureMultiplier As Single = 2
    Dim ViscosityStrength As Single = 0.9
    Dim BoundsSize As Vector2
    Dim InteractionInputPoint As Vector2
    Dim InteractionInputStrength As Single
    Dim InteractionInputRadius As Single
    Dim ParticleSize As Single = 5

    Dim ObstacleSize As Vector2
    Dim ObstacleCentre As Vector2

    ReadOnly Kernels As FluidKernels

    ' Buffers
    Dim Particles As Particle()
    Dim Spatial As Grid(Of Particle())

    Public ReadOnly Property Entity As IReadOnlyCollection(Of Particle) Implements IContainer(Of Particle).Entity
        Get
            Return Particles
        End Get
    End Property

    Public ReadOnly Property Width As Double Implements IContainer(Of Particle).Width
    Public ReadOnly Property Height As Double Implements IContainer(Of Particle).Height

    Sub New(n As Integer, canvas As Size, Optional smoothingRadius As Single = 25)
        Kernels = New FluidKernels(smoothingRadius)

        Me.SmoothingRadius = smoothingRadius
        Me.Particles = New Particle(n - 1) {}
        Me.Width = canvas.Width
        Me.Height = canvas.Height
        Me.NumParticles = n

        For i As Integer = 0 To n - 1
            Particles(i) = New Particle(i, canvas)
        Next

        Call Resize(canvas.Width, canvas.Height)
    End Sub

    Public Sub RunDebugStep()
        For i As Integer = 0 To NumParticles - 1
            ApplyExternalForces(i)
        Next

        Spatial = Me.EncodeGrid(SmoothingRadius)

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
    End Sub

    Public Sub RunSimulationStep()
        Call par.For(0, NumParticles, Sub(i) ApplyExternalForces(i))
        Spatial = Me.EncodeGrid(SmoothingRadius)
        Call par.For(0, NumParticles, Sub(i) CalculateDensities(i))
        Call par.For(0, NumParticles, Sub(i) CalculatePressureForce(i))
        Call par.For(0, NumParticles, Sub(i) CalculateViscosity(i))
        Call par.For(0, NumParticles, Sub(i) UpdatePositions(i))
    End Sub

    Public Sub Resize(w As Integer, h As Integer)
        _Width = w
        _Height = h

        BoundsSize = New Vector2(w, h)
    End Sub

    Private Function CalculateDensity(pos As Vector2, id As Integer) As Vector2
        Dim originCell = GetCell2D(pos, SmoothingRadius)
        Dim sqrRadius As Single = SmoothingRadius * SmoothingRadius
        Dim density As Single = 0
        Dim nearDensity As Single = 0
        Dim neighbours = Spatial.SpatialLookup(originCell)

        ' Neighbour search
        For Each neighbour In neighbours
            If neighbour.Index = id Then
                Continue For
            End If

            Dim neighbourPos As Vector2 = neighbour.PredictedPosition
            Dim offsetToNeighbour As Vector2 = neighbourPos - pos
            Dim sqrDstToNeighbour As Single = Dot(offsetToNeighbour, offsetToNeighbour)

            ' Skip if not within radius
            If sqrDstToNeighbour > sqrRadius Then
                Continue For
            End If

            ' Calculate density and near density
            Dim dst As Single = Sqrt(sqrDstToNeighbour)
            density += Kernels.SpikyKernelPow2(dst, SmoothingRadius)
            nearDensity += Kernels.SpikyKernelPow3(dst, SmoothingRadius)
        Next

        Return New Vector2(density, nearDensity)
    End Function

    Private Function PressureFromDensity(density As Single) As Single
        Return (density - TargetDensity) * PressureMultiplier
    End Function

    Private Function NearPressureFromDensity(nearDensity As Single) As Single
        Return NearPressureMultiplier * nearDensity
    End Function

    Private Function ComputeExternalForce(pos As Vector2, velocity As Vector2) As Vector2
        ' Gravity
        Dim gravityAccel As New Vector2(0, Gravity)

        ' Input interactions modify gravity
        If InteractionInputStrength <> 0 Then
            Dim inputPointOffset As Vector2 = InteractionInputPoint - pos
            Dim sqrDst As Single = Dot(inputPointOffset, inputPointOffset)
            If sqrDst < InteractionInputRadius * InteractionInputRadius Then
                Dim dst As Single = Sqrt(sqrDst)
                Dim edgeT As Single = (dst / InteractionInputRadius)
                Dim centreT As Single = 1 - edgeT
                Dim dirToCentre As Vector2 = inputPointOffset / dst

                'INSTANT VB WARNING: Instant VB cannot determine whether both operands of this division are integer types - if they are then you should use the VB integer division operator:
                Dim gravityWeight As Single = 1 - (centreT * saturate(InteractionInputStrength / 10))
                Dim accel As Vector2 = gravityAccel * gravityWeight + dirToCentre * centreT * InteractionInputStrength
                accel -= velocity * centreT
                Return accel
            End If
        End If

        Return gravityAccel
    End Function

    Private Sub HandleCollisions(particleIndex As UInteger)
        Dim pos As Vector2 = Particles(particleIndex).Position
        Dim vel As Vector2 = Particles(particleIndex).Velocity

        Dim padding = Me.ParticleSize * 5

        If pos.x > Width - padding Then
            vel.x *= -1 * CollisionDamping
            pos.x = Width - padding
        ElseIf pos.x < padding Then
            vel.x *= -1 * CollisionDamping
            pos.x = padding
        End If
        If pos.y > Height - padding Then
            vel.y *= -1 * CollisionDamping
            pos.y = Height - padding
        ElseIf pos.y < padding Then
            vel.y *= -1 * CollisionDamping
            pos.y = padding
        End If

        ' Update position and velocity
        Particles(particleIndex).Position = pos
        Particles(particleIndex).Velocity = vel
    End Sub

    Private Sub ApplyExternalForces(id As Integer)
        If id >= NumParticles Then
            Return
        End If

        ' External forces (gravity and input interaction)
        Particles(id).Velocity += ComputeExternalForce(Particles(id).Position, Particles(id).Velocity) * DeltaTime

        ' Predict
        Const predictionFactor As Single = 1 / 120.0
        Particles(id).PredictedPosition = Particles(id).Position + Particles(id).Velocity * predictionFactor
    End Sub

    Private Sub CalculateDensities(id As Integer)
        If id >= NumParticles Then
            Return
        End If

        Dim pos As Vector2 = Particles(id).PredictedPosition
        Particles(id).Density = CalculateDensity(pos, id)
    End Sub

    Private Sub CalculatePressureForce(id As Integer)
        If id >= NumParticles Then
            Return
        End If

        Dim density As Single = Particles(id).Density(0)
        Dim densityNear As Single = Particles(id).Density(1)
        Dim pressure As Single = PressureFromDensity(density)
        Dim nearPressure As Single = NearPressureFromDensity(densityNear)
        Dim pressureForce As Vector2 = Vector2.Zero

        Dim pos As Vector2 = Particles(id).PredictedPosition
        Dim originCell = GetCell2D(pos, SmoothingRadius)
        Dim sqrRadius As Single = SmoothingRadius * SmoothingRadius

        Dim neighbours = Spatial.SpatialLookup(originCell)

        ' Neighbour search
        For Each neighbour In neighbours
            If neighbour.Index = id Then
                Continue For
            End If

            Dim neighbourPos As Vector2 = neighbour.PredictedPosition
            Dim offsetToNeighbour As Vector2 = neighbourPos - pos
            Dim sqrDstToNeighbour As Single = Dot(offsetToNeighbour, offsetToNeighbour)

            ' Skip if not within radius
            If sqrDstToNeighbour > sqrRadius Then
                Continue For
            End If

            ' Calculate pressure force
            Dim dst As Single = Sqrt(sqrDstToNeighbour)
            Dim dirToNeighbour As Vector2 = If(dst > 0, offsetToNeighbour / dst, New Vector2(0, 1))

            Dim neighbourDensity As Single = neighbour.Density(0)
            Dim neighbourNearDensity As Single = neighbour.Density(1)
            Dim neighbourPressure As Single = PressureFromDensity(neighbourDensity)
            Dim neighbourNearPressure As Single = NearPressureFromDensity(neighbourNearDensity)

            Dim sharedPressure As Single = (pressure + neighbourPressure) * 0.5
            Dim sharedNearPressure As Single = (nearPressure + neighbourNearPressure) * 0.5

            pressureForce += dirToNeighbour * Kernels.DerivativeSpikyPow2(dst, SmoothingRadius) * sharedPressure / neighbourDensity
            pressureForce += dirToNeighbour * Kernels.DerivativeSpikyPow3(dst, SmoothingRadius) * sharedNearPressure / neighbourNearDensity
        Next

        Dim acceleration As Vector2 = pressureForce / density
        Particles(id).Velocity += acceleration * DeltaTime
    End Sub

    Private Sub CalculateViscosity(id As Integer)
        If id >= NumParticles Then
            Return
        End If

        Dim pos As Vector2 = Particles(id).PredictedPosition
        Dim originCell = GetCell2D(pos, SmoothingRadius)
        Dim sqrRadius As Single = SmoothingRadius * SmoothingRadius

        Dim viscosityForce As Vector2 = Vector2.Zero
        Dim velocity As Vector2 = Particles(id).Velocity

        Dim neighbours = Spatial.SpatialLookup(originCell)

        ' Neighbour search
        For Each neighbour In neighbours
            If neighbour.Index = id Then
                Continue For
            End If

            Dim neighbourPos As Vector2 = neighbour.PredictedPosition
            Dim offsetToNeighbour As Vector2 = neighbourPos - pos
            Dim sqrDstToNeighbour As Single = Dot(offsetToNeighbour, offsetToNeighbour)

            ' Skip if not within radius
            If sqrDstToNeighbour > sqrRadius Then
                Continue For
            End If

            Dim dst As Single = Sqrt(sqrDstToNeighbour)
            Dim neighbourVelocity As Vector2 = neighbour.Velocity
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
