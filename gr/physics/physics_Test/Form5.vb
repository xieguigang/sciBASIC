Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.GraphTheory.GridGraph
Imports Microsoft.VisualBasic.Imaging.Physics

Public Class Form5

	Public Class Engine : Implements IContainer(Of Particle)

		' Settings
		Dim numParticles As UInteger
		Dim gravity As Single
		Dim deltaTime As Single
		Dim collisionDamping As Single
		Dim smoothingRadius As Single
		Dim targetDensity As Single
		Dim pressureMultiplier As Single
		Dim nearPressureMultiplier As Single
		Dim viscosityStrength As Single
		Dim boundsSize As Vector2
		Dim interactionInputPoint As Vector2
		Dim interactionInputStrength As Single
		Dim interactionInputRadius As Single

		Dim obstacleSize As Vector2
		Dim obstacleCentre As Vector2


		Dim Poly6ScalingFactor As Single
		Dim SpikyPow3ScalingFactor As Single
		Dim SpikyPow2ScalingFactor As Single
		Dim SpikyPow3DerivativeScalingFactor As Single
		Dim SpikyPow2DerivativeScalingFactor As Single


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

		Private Function SmoothingKernelPoly6(ByVal dst As Single, ByVal radius As Single) As Single
			If dst < radius Then
				Dim v As Single = radius * radius - dst * dst
				Return v * v * v * Poly6ScalingFactor
			End If
			Return 0
		End Function

		Private Function SpikyKernelPow3(ByVal dst As Single, ByVal radius As Single) As Single
			If dst < radius Then
				Dim v As Single = radius - dst
				Return v * v * v * SpikyPow3ScalingFactor
			End If
			Return 0
		End Function

		Private Function SpikyKernelPow2(ByVal dst As Single, ByVal radius As Single) As Single
			If dst < radius Then
				Dim v As Single = radius - dst
				Return v * v * SpikyPow2ScalingFactor
			End If
			Return 0
		End Function

		Private Function DerivativeSpikyPow3(ByVal dst As Single, ByVal radius As Single) As Single
			If dst <= radius Then
				Dim v As Single = radius - dst
				Return -v * v * SpikyPow3DerivativeScalingFactor
			End If
			Return 0
		End Function

		Private Function DerivativeSpikyPow2(ByVal dst As Single, ByVal radius As Single) As Single
			If dst <= radius Then
				Dim v As Single = radius - dst
				Return -v * SpikyPow2DerivativeScalingFactor
			End If
			Return 0
		End Function

		Private Function DensityKernel(ByVal dst As Single, ByVal radius As Single) As Single
			Return SpikyKernelPow2(dst, radius)
		End Function

		Private Function NearDensityKernel(ByVal dst As Single, ByVal radius As Single) As Single
			Return SpikyKernelPow3(dst, radius)
		End Function

		Private Function DensityDerivative(ByVal dst As Single, ByVal radius As Single) As Single
			Return DerivativeSpikyPow2(dst, radius)
		End Function

		Private Function NearDensityDerivative(ByVal dst As Single, ByVal radius As Single) As Single
			Return DerivativeSpikyPow3(dst, radius)
		End Function

		Private Function ViscosityKernel(ByVal dst As Single, ByVal radius As Single) As Single
			Return SmoothingKernelPoly6(dst, smoothingRadius)
		End Function

		Public Sub [Step]()

		End Sub

		Private Function CalculateDensity(ByVal pos As Vector2) As Vector2
			Dim originCell = GetCell2D(pos, smoothingRadius)
			Dim sqrRadius As Single = smoothingRadius * smoothingRadius
			Dim density As Single = 0
			Dim nearDensity As Single = 0
			Dim neighbours = spatial.SpatialLookup(originCell)

			' Neighbour search
			For Each neighbour In neighbours
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

		Private Function PressureFromDensity(ByVal density As Single) As Single
			Return (density - targetDensity) * pressureMultiplier
		End Function

		Private Function NearPressureFromDensity(ByVal nearDensity As Single) As Single
			Return nearPressureMultiplier * nearDensity
		End Function

		Private Function ExternalForces(ByVal pos As Vector2, ByVal velocity As Vector2) As Vector2
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

		Private Sub HandleCollisions(ByVal particleIndex As UInteger)
			Dim pos As Vector2 = particles(particleIndex).position
			Dim vel As Vector2 = particles(particleIndex).velocity

			' Keep particle inside bounds
			Dim halfSize As Vector2 = boundsSize * 0.5
			Dim edgeDst As Vector2 = halfSize - Vector2Math.Abs(pos)

			If edgeDst.x <= 0 Then
				pos.x = halfSize.x * Sign(pos.x)
				vel.x *= -1 * collisionDamping
			End If
			If edgeDst.y <= 0 Then
				pos.y = halfSize.y * Sign(pos.y)
				vel.y *= -1 * collisionDamping
			End If

			' Collide particle against the test obstacle
			Dim obstacleHalfSize As Vector2 = obstacleSize * 0.5
			Dim obstacleEdgeDst As Vector2 = obstacleHalfSize - Vector2Math.Abs(pos - obstacleCentre)

			If obstacleEdgeDst.x >= 0 AndAlso obstacleEdgeDst.y >= 0 Then
				If obstacleEdgeDst.x < obstacleEdgeDst.y Then
					pos.x = obstacleHalfSize.x * Sign(pos.x - obstacleCentre.x) + obstacleCentre.x
					vel.x *= -1 * collisionDamping
				Else
					pos.y = obstacleHalfSize.y * Sign(pos.y - obstacleCentre.y) + obstacleCentre.y
					vel.y *= -1 * collisionDamping
				End If
			End If

			' Update position and velocity
			particles(particleIndex).position = pos
			particles(particleIndex).velocity = vel
		End Sub

		Private Sub ExternalForces(ByVal id As Integer)
			If id >= numParticles Then
				Return
			End If

			' External forces (gravity and input interaction)
			Velocities(id) += ExternalForces(Positions(id), Velocities(id)) * deltaTime

			' Predict
			Const predictionFactor As Single = 1 / 120.0
			PredictedPositions(id) = Positions(id) + Velocities(id) * predictionFactor
		End Sub

		Private Sub CalculateDensities(ByVal id As Integer)
			If id >= numParticles Then
				Return
			End If

			Dim pos As Vector2 = particles(id).predictedPosition
			particles(id).density = CalculateDensity(pos)
		End Sub
		Private Sub CalculatePressureForce(ByVal id As Integer)
			If id >= numParticles Then
				Return
			End If

			Dim density As Single = Densities(id)(0)
			Dim densityNear As Single = Densities(id)(1)
			Dim pressure As Single = PressureFromDensity(density)
			Dim nearPressure As Single = NearPressureFromDensity(densityNear)
			Dim pressureForce As Vector2 = Vector2.zero

			Dim pos As Vector2 = PredictedPositions(id)
			Dim originCell As int2 = GetCell2D(pos, smoothingRadius)
			Dim sqrRadius As Single = smoothingRadius * smoothingRadius

			' Neighbour search
			For i As Integer = 0 To 8
				Dim hash As UInteger = HashCell2D(originCell + offsets2D(i))
				Dim key As UInteger = KeyFromHash(hash, numParticles)
				Dim currIndex As UInteger = SpatialOffsets(key)

				Do While currIndex < numParticles
					Dim indexData As uint3 = SpatialIndices(currIndex)
					currIndex += 1
					' Exit if no longer looking at correct bin
					If indexData(2) <> key Then
						Exit Do
					End If
					' Skip if hash does not match
					If indexData(1) <> hash Then
						Continue Do
					End If

					Dim neighbourIndex As UInteger = indexData(0)
					' Skip if looking at self
					If neighbourIndex = id.x Then
						Continue Do
					End If

					Dim neighbourPos As Vector2 = PredictedPositions(neighbourIndex)
					Dim offsetToNeighbour As Vector2 = neighbourPos - pos
					Dim sqrDstToNeighbour As Single = dot(offsetToNeighbour, offsetToNeighbour)

					' Skip if not within radius
					If sqrDstToNeighbour > sqrRadius Then
						Continue Do
					End If

					' Calculate pressure force
					Dim dst As Single = Sqrt(sqrDstToNeighbour)
					Dim dirToNeighbour As Vector2 = If(dst > 0, offsetToNeighbour / dst, New Vector2(0, 1))

					Dim neighbourDensity As Single = Densities(neighbourIndex)(0)
					Dim neighbourNearDensity As Single = Densities(neighbourIndex)(1)
					Dim neighbourPressure As Single = PressureFromDensity(neighbourDensity)
					Dim neighbourNearPressure As Single = NearPressureFromDensity(neighbourNearDensity)

					Dim sharedPressure As Single = (pressure + neighbourPressure) * 0.5
					Dim sharedNearPressure As Single = (nearPressure + neighbourNearPressure) * 0.5

					pressureForce += dirToNeighbour * DensityDerivative(dst, smoothingRadius) * sharedPressure / neighbourDensity
					pressureForce += dirToNeighbour * NearDensityDerivative(dst, smoothingRadius) * sharedNearPressure / neighbourNearDensity
				Loop
			Next i

			Dim acceleration As Vector2 = pressureForce / density
			Velocities(id) += acceleration * deltaTime
		End Sub
		Private Sub CalculateViscosity(ByVal id As Integer)
			If id >= numParticles Then
				Return
			End If


			Dim pos As Vector2 = PredictedPositions(id)
			Dim originCell As int2 = GetCell2D(pos, smoothingRadius)
			Dim sqrRadius As Single = smoothingRadius * smoothingRadius

			Dim viscosityForce As Vector2 = Vector2.zero
			Dim velocity As Vector2 = Velocities(id)

			For i As Integer = 0 To 8
				Dim hash As UInteger = HashCell2D(originCell + offsets2D(i))
				Dim key As UInteger = KeyFromHash(hash, numParticles)
				Dim currIndex As UInteger = SpatialOffsets(key)

				Do While currIndex < numParticles
					Dim indexData As uint3 = SpatialIndices(currIndex)
					currIndex += 1
					' Exit if no longer looking at correct bin
					If indexData(2) <> key Then
						Exit Do
					End If
					' Skip if hash does not match
					If indexData(1) <> hash Then
						Continue Do
					End If

					Dim neighbourIndex As UInteger = indexData(0)
					' Skip if looking at self
					If neighbourIndex = id.x Then
						Continue Do
					End If

					Dim neighbourPos As Vector2 = PredictedPositions(neighbourIndex)
					Dim offsetToNeighbour As Vector2 = neighbourPos - pos
					Dim sqrDstToNeighbour As Single = dot(offsetToNeighbour, offsetToNeighbour)

					' Skip if not within radius
					If sqrDstToNeighbour > sqrRadius Then
						Continue Do
					End If

					Dim dst As Single = Sqrt(sqrDstToNeighbour)
					Dim neighbourVelocity As Vector2 = Velocities(neighbourIndex)
					viscosityForce += (neighbourVelocity - velocity) * ViscosityKernel(dst, smoothingRadius)
				Loop

			Next i
			Velocities(id) += viscosityForce * viscosityStrength * deltaTime
		End Sub

		Private Sub UpdatePositions(ByVal id As Integer)
			If id >= numParticles Then
				Return
			End If

			Positions(id) += Velocities(id) * deltaTime
			HandleCollisions(id)
		End Sub

	End Class

End Class