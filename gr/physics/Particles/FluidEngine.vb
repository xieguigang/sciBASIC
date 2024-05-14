#Region "Microsoft.VisualBasic::ced47e48a30a0d5fdf82a96f9f2c1744, gr\physics\Particles\FluidEngine.vb"

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

    '   Total Lines: 403
    '    Code Lines: 286
    ' Comment Lines: 41
    '   Blank Lines: 76
    '     File Size: 15.40 KB


    ' Class FluidEngine
    ' 
    '     Properties: Entity, Height, Width
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: CalculateDensity, DensityDerivative, DensityKernel, DerivativeSpikyPow2, DerivativeSpikyPow3
    '               ExternalForces, NearDensityDerivative, NearDensityKernel, NearPressureFromDensity, PressureFromDensity
    '               SmoothingKernelPoly6, SpikyKernelPow2, SpikyKernelPow3, ViscosityKernel
    ' 
    '     Sub: CalculateDensities, CalculatePressureForce, CalculateViscosity, ExternalForces, HandleCollisions
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
    ReadOnly numParticles As UInteger

    Dim gravity As Single = 100
    Dim deltaTime As Single = 1 / 60
    Dim collisionDamping As Single = 0.95
    Dim smoothingRadius As Single = 50
    Dim targetDensity As Single = 5
    Dim pressureMultiplier As Single = 5
    Dim nearPressureMultiplier As Single = 2
    Dim viscosityStrength As Single = 0.9
    Dim boundsSize As Vector2
    Dim interactionInputPoint As Vector2
    Dim interactionInputStrength As Single
    Dim interactionInputRadius As Single
    Dim particleSize As Single = 5

    Dim obstacleSize As Vector2
    Dim obstacleCentre As Vector2


    ReadOnly Poly6ScalingFactor As Single
    ReadOnly SpikyPow3ScalingFactor As Single
    ReadOnly SpikyPow2ScalingFactor As Single
    ReadOnly SpikyPow3DerivativeScalingFactor As Single
    ReadOnly SpikyPow2DerivativeScalingFactor As Single

    ' Buffers
    Dim particles As Particle()
    Dim spatial As Grid(Of Particle())
    Public ReadOnly Property Entity As IReadOnlyCollection(Of Particle) Implements IContainer(Of Particle).Entity
        Get
            Return particles
        End Get
    End Property

    Public ReadOnly Property Width As Double Implements IContainer(Of Particle).Width
    Public ReadOnly Property Height As Double Implements IContainer(Of Particle).Height

    Sub New(n As Integer, canvas As Size, Optional smoothingRadius As Single = 25)
        Poly6ScalingFactor = 4 / (PI * Pow(smoothingRadius, 8))
        SpikyPow3ScalingFactor = 10 / (PI * Pow(smoothingRadius, 5))
        SpikyPow2ScalingFactor = 6 / (PI * Pow(smoothingRadius, 4))
        SpikyPow3DerivativeScalingFactor = 30 / (Pow(smoothingRadius, 5) * PI)
        SpikyPow2DerivativeScalingFactor = 12 / (Pow(smoothingRadius, 4) * PI)

        Me.smoothingRadius = smoothingRadius
        Me.particles = New Particle(n - 1) {}
        Me.Width = canvas.Width
        Me.Height = canvas.Height
        Me.numParticles = n

        For i As Integer = 0 To n - 1
            particles(i) = New Particle(i, canvas)
        Next

        Call Resize(canvas.Width, canvas.Height)
    End Sub

    Public Sub RunDebugStep()
        For i As Integer = 0 To numParticles - 1
            ExternalForces(i)
        Next

        spatial = Me.EncodeGrid(smoothingRadius)

        For i As Integer = 0 To numParticles - 1
            CalculateDensities(i)
        Next
        For i As Integer = 0 To numParticles - 1
            CalculatePressureForce(i)
        Next
        For i As Integer = 0 To numParticles - 1
            CalculateViscosity(i)
        Next
        For i As Integer = 0 To numParticles - 1
            UpdatePositions(i)
        Next
    End Sub

    Public Sub RunSimulationStep()
        Call par.For(0, numParticles, Sub(i) ExternalForces(i))
        spatial = Me.EncodeGrid(smoothingRadius)
        Call par.For(0, numParticles, Sub(i) CalculateDensities(i))
        Call par.For(0, numParticles, Sub(i) CalculatePressureForce(i))
        Call par.For(0, numParticles, Sub(i) CalculateViscosity(i))
        Call par.For(0, numParticles, Sub(i) UpdatePositions(i))
    End Sub

    Public Sub Resize(w As Integer, h As Integer)
        _Width = w
        _Height = h

        boundsSize = New Vector2(w, h)
    End Sub

    Private Function SmoothingKernelPoly6(dst As Single, radius As Single) As Single
        If dst < radius Then
            Dim v As Single = radius * radius - dst * dst
            Return v * v * v * Poly6ScalingFactor
        End If
        Return 0
    End Function

    Private Function SpikyKernelPow3(dst As Single, radius As Single) As Single
        If dst < radius Then
            Dim v As Single = radius - dst
            Return v * v * v * SpikyPow3ScalingFactor
        End If
        Return 0
    End Function

    Private Function SpikyKernelPow2(dst As Single, radius As Single) As Single
        If dst < radius Then
            Dim v As Single = radius - dst
            Return v * v * SpikyPow2ScalingFactor
        End If
        Return 0
    End Function

    Private Function DerivativeSpikyPow3(dst As Single, radius As Single) As Single
        If dst <= radius Then
            Dim v As Single = radius - dst
            Return -v * v * SpikyPow3DerivativeScalingFactor
        End If
        Return 0
    End Function

    Private Function DerivativeSpikyPow2(dst As Single, radius As Single) As Single
        If dst <= radius Then
            Dim v As Single = radius - dst
            Return -v * SpikyPow2DerivativeScalingFactor
        End If
        Return 0
    End Function

    Private Function DensityKernel(dst As Single, radius As Single) As Single
        Return SpikyKernelPow2(dst, radius)
    End Function

    Private Function NearDensityKernel(dst As Single, radius As Single) As Single
        Return SpikyKernelPow3(dst, radius)
    End Function

    Private Function DensityDerivative(dst As Single, radius As Single) As Single
        Return DerivativeSpikyPow2(dst, radius)
    End Function

    Private Function NearDensityDerivative(dst As Single, radius As Single) As Single
        Return DerivativeSpikyPow3(dst, radius)
    End Function

    Private Function ViscosityKernel(dst As Single, radius As Single) As Single
        Return SmoothingKernelPoly6(dst, smoothingRadius)
    End Function

    Private Function CalculateDensity(pos As Vector2, id As Integer) As Vector2
        Dim originCell = GetCell2D(pos, smoothingRadius)
        Dim sqrRadius As Single = smoothingRadius * smoothingRadius
        Dim density As Single = 0
        Dim nearDensity As Single = 0
        Dim neighbours = spatial.SpatialLookup(originCell)

        ' Neighbour search
        For Each neighbour In neighbours
            If neighbour.index = id Then
                Continue For
            End If

            Dim neighbourPos As Vector2 = neighbour.predictedPosition
            Dim offsetToNeighbour As Vector2 = neighbourPos - pos
            Dim sqrDstToNeighbour As Single = Dot(offsetToNeighbour, offsetToNeighbour)

            ' Skip if not within radius
            If sqrDstToNeighbour > sqrRadius Then
                Continue For
            End If

            ' Calculate density and near density
            Dim dst As Single = Sqrt(sqrDstToNeighbour)
            density += DensityKernel(dst, smoothingRadius)
            nearDensity += NearDensityKernel(dst, smoothingRadius)
        Next


        Return New Vector2(density, nearDensity)
    End Function

    Private Function PressureFromDensity(density As Single) As Single
        Return (density - targetDensity) * pressureMultiplier
    End Function

    Private Function NearPressureFromDensity(nearDensity As Single) As Single
        Return nearPressureMultiplier * nearDensity
    End Function

    Private Function ExternalForces(pos As Vector2, velocity As Vector2) As Vector2
        ' Gravity
        Dim gravityAccel As New Vector2(0, gravity)

        ' Input interactions modify gravity
        If interactionInputStrength <> 0 Then
            Dim inputPointOffset As Vector2 = interactionInputPoint - pos
            Dim sqrDst As Single = Dot(inputPointOffset, inputPointOffset)
            If sqrDst < interactionInputRadius * interactionInputRadius Then
                Dim dst As Single = Sqrt(sqrDst)
                Dim edgeT As Single = (dst / interactionInputRadius)
                Dim centreT As Single = 1 - edgeT
                Dim dirToCentre As Vector2 = inputPointOffset / dst

                'INSTANT VB WARNING: Instant VB cannot determine whether both operands of this division are integer types - if they are then you should use the VB integer division operator:
                Dim gravityWeight As Single = 1 - (centreT * saturate(interactionInputStrength / 10))
                Dim accel As Vector2 = gravityAccel * gravityWeight + dirToCentre * centreT * interactionInputStrength
                accel -= velocity * centreT
                Return accel
            End If
        End If

        Return gravityAccel
    End Function

    Private Sub HandleCollisions(particleIndex As UInteger)
        Dim pos As Vector2 = particles(particleIndex).position
        Dim vel As Vector2 = particles(particleIndex).velocity

        '' Keep particle inside bounds
        'Dim halfSize As Vector2 = boundsSize * 0.5
        'Dim edgeDst As Vector2 = halfSize - Vector2Math.Abs(pos)

        'If edgeDst.x <= 0 Then
        '    pos.x = halfSize.x * Sign(pos.x)
        '    vel.x *= -1 * collisionDamping
        'End If
        'If edgeDst.y <= 0 Then
        '    pos.y = halfSize.y * Sign(pos.y)
        '    vel.y *= -1 * collisionDamping
        'End If

        'If Not obstacleSize Is Nothing Then
        '    ' Collide particle against the test obstacle
        '    Dim obstacleHalfSize As Vector2 = obstacleSize * 0.5
        '    Dim obstacleEdgeDst As Vector2 = obstacleHalfSize - Vector2Math.Abs(pos - obstacleCentre)

        '    If obstacleEdgeDst.x >= 0 AndAlso obstacleEdgeDst.y >= 0 Then
        '        If obstacleEdgeDst.x < obstacleEdgeDst.y Then
        '            pos.x = obstacleHalfSize.x * Sign(pos.x - obstacleCentre.x) + obstacleCentre.x
        '            vel.x *= -1 * collisionDamping
        '        Else
        '            pos.y = obstacleHalfSize.y * Sign(pos.y - obstacleCentre.y) + obstacleCentre.y
        '            vel.y *= -1 * collisionDamping
        '        End If
        '    End If
        'End If
        Dim padding = Me.particleSize * 5

        If pos.x > Width - padding Then
            vel.x *= -1 * collisionDamping
            pos.x = Width - padding
        ElseIf pos.x < padding Then
            vel.x *= -1 * collisionDamping
            pos.x = padding
        End If
        If pos.y > Height - padding Then
            vel.y *= -1 * collisionDamping
            pos.y = Height - padding
        ElseIf pos.y < padding Then
            vel.y *= -1 * collisionDamping
            pos.y = padding
        End If

        ' Update position and velocity
        particles(particleIndex).position = pos
        particles(particleIndex).velocity = vel
    End Sub

    Private Sub ExternalForces(id As Integer)
        If id >= numParticles Then
            Return
        End If

        ' External forces (gravity and input interaction)
        particles(id).velocity += ExternalForces(particles(id).position, particles(id).velocity) * deltaTime

        ' Predict
        Const predictionFactor As Single = 1 / 120.0
        particles(id).predictedPosition = particles(id).position + particles(id).velocity * predictionFactor
    End Sub

    Private Sub CalculateDensities(id As Integer)
        If id >= numParticles Then
            Return
        End If

        Dim pos As Vector2 = particles(id).predictedPosition
        particles(id).density = CalculateDensity(pos, id)
    End Sub
    Private Sub CalculatePressureForce(id As Integer)
        If id >= numParticles Then
            Return
        End If

        Dim density As Single = particles(id).density(0)
        Dim densityNear As Single = particles(id).density(1)
        Dim pressure As Single = PressureFromDensity(density)
        Dim nearPressure As Single = NearPressureFromDensity(densityNear)
        Dim pressureForce As Vector2 = Vector2.zero

        Dim pos As Vector2 = particles(id).predictedPosition
        Dim originCell = GetCell2D(pos, smoothingRadius)
        Dim sqrRadius As Single = smoothingRadius * smoothingRadius

        Dim neighbours = spatial.SpatialLookup(originCell)

        ' Neighbour search
        For Each neighbour In neighbours
            If neighbour.index = id Then
                Continue For
            End If

            Dim neighbourPos As Vector2 = neighbour.predictedPosition
            Dim offsetToNeighbour As Vector2 = neighbourPos - pos
            Dim sqrDstToNeighbour As Single = Dot(offsetToNeighbour, offsetToNeighbour)

            ' Skip if not within radius
            If sqrDstToNeighbour > sqrRadius Then
                Continue For
            End If

            ' Calculate pressure force
            Dim dst As Single = Sqrt(sqrDstToNeighbour)
            Dim dirToNeighbour As Vector2 = If(dst > 0, offsetToNeighbour / dst, New Vector2(0, 1))

            Dim neighbourDensity As Single = neighbour.density(0)
            Dim neighbourNearDensity As Single = neighbour.density(1)
            Dim neighbourPressure As Single = PressureFromDensity(neighbourDensity)
            Dim neighbourNearPressure As Single = NearPressureFromDensity(neighbourNearDensity)

            Dim sharedPressure As Single = (pressure + neighbourPressure) * 0.5
            Dim sharedNearPressure As Single = (nearPressure + neighbourNearPressure) * 0.5

            pressureForce += dirToNeighbour * DensityDerivative(dst, smoothingRadius) * sharedPressure / neighbourDensity
            pressureForce += dirToNeighbour * NearDensityDerivative(dst, smoothingRadius) * sharedNearPressure / neighbourNearDensity
        Next

        Dim acceleration As Vector2 = pressureForce / density
        particles(id).velocity += acceleration * deltaTime
    End Sub
    Private Sub CalculateViscosity(id As Integer)
        If id >= numParticles Then
            Return
        End If


        Dim pos As Vector2 = particles(id).predictedPosition
        Dim originCell = GetCell2D(pos, smoothingRadius)
        Dim sqrRadius As Single = smoothingRadius * smoothingRadius

        Dim viscosityForce As Vector2 = Vector2.zero
        Dim velocity As Vector2 = particles(id).velocity

        Dim neighbours = spatial.SpatialLookup(originCell)

        ' Neighbour search
        For Each neighbour In neighbours
            If neighbour.index = id Then
                Continue For
            End If

            Dim neighbourPos As Vector2 = neighbour.predictedPosition
            Dim offsetToNeighbour As Vector2 = neighbourPos - pos
            Dim sqrDstToNeighbour As Single = Dot(offsetToNeighbour, offsetToNeighbour)

            ' Skip if not within radius
            If sqrDstToNeighbour > sqrRadius Then
                Continue For
            End If

            Dim dst As Single = Sqrt(sqrDstToNeighbour)
            Dim neighbourVelocity As Vector2 = neighbour.velocity
            viscosityForce += (neighbourVelocity - velocity) * ViscosityKernel(dst, smoothingRadius)
        Next

        particles(id).velocity += viscosityForce * viscosityStrength * deltaTime
    End Sub

    Private Sub UpdatePositions(id As Integer)
        If id >= numParticles Then
            Return
        End If

        particles(id).position += particles(id).velocity * deltaTime
        HandleCollisions(id)
    End Sub

End Class
