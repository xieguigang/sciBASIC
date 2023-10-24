Imports System.Security.Cryptography
Imports Microsoft.VisualBasic.Imaging.Physics
Imports System.Math
Imports Microsoft.VisualBasic.Data.GraphTheory.GridGraph

Public Class Form5

	Public Class Engine : Implements IContainer(Of Vector2)

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
		Dim Positions As Vector2()
		Dim PredictedPositions As Vector2()
		Dim Velocities As Vector2()
		Dim Densities As Vector2() ' Density, Near Density

		Dim spatial As Grid(Of Vector2())

		Public ReadOnly Property Entity As IReadOnlyCollection(Of Vector2) Implements IContainer(Of Vector2).Entity
			Get
				Return Positions
			End Get
		End Property

		Public ReadOnly Property Width As Double Implements IContainer(Of Vector2).Width
		Public ReadOnly Property Height As Double Implements IContainer(Of Vector2).Height

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

		Private Function CalculateDensity(ByVal pos As Vector2) As Vector2
			Dim originCell As int2 = GetCell2D(pos, smoothingRadius)
			Dim sqrRadius As Single = smoothingRadius * smoothingRadius
			Dim density As Single = 0
			Dim nearDensity As Single = 0

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
					Dim neighbourPos As Vector2 = PredictedPositions(neighbourIndex)
					Dim offsetToNeighbour As Vector2 = neighbourPos - pos
					Dim sqrDstToNeighbour As Single = dot(offsetToNeighbour, offsetToNeighbour)

					' Skip if not within radius
					If sqrDstToNeighbour > sqrRadius Then
						Continue Do
					End If

					' Calculate density and near density
					Dim dst As Single = Sqrt(sqrDstToNeighbour)
					density += DensityKernel(dst, smoothingRadius)
					nearDensity += NearDensityKernel(dst, smoothingRadius)
				Loop
			Next i

			Return New Vector2(density, nearDensity)
		End Function

		Private Function PressureFromDensity(ByVal density As Single) As Single
			Return (density - targetDensity) * pressureMultiplier
		End Function

		Private Function NearPressureFromDensity(ByVal nearDensity As Single) As Single
			Return nearPressureMultiplier * nearDensity
		End Function

	End Class

End Class