Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.SIMD
Imports Microsoft.VisualBasic.Parallel
Imports std = System.Math

Namespace Boids

    ''' <summary>
    ''' Evaluate the three dimensional flocking rules for a contiguous boid index
    ''' range.
    ''' </summary>
    ''' <remarks>
    ''' Runs inside a <see cref="VectorTask"/> parallel range; the grid is
    ''' read-only during the solve and each boid only writes its own delta slot,
    ''' so the parallel execution is race free. The rule constants, the
    ''' accumulation order and the resulting force expression are identical to the
    ''' two dimensional <see cref="FlockSolver"/> - only the number of the probed
    ''' cells changes (twenty seven instead of nine) and every distance becomes a
    ''' three component one.
    ''' </remarks>
    Friend NotInheritable Class FlockSolver3D
        Inherits VectorTask

        ''' <summary> cohesion / alignment perception radius </summary>
        Private Const flockDistance As Double = 50.0
        Private Const flockDistance2 As Double = flockDistance * flockDistance
        ''' <summary> separation radius </summary>
        Private Const avoidDistance As Double = 20.0
        Private Const avoidDistance2 As Double = avoidDistance * avoidDistance
        ''' <summary> predator perception radius </summary>
        Private Const predatorDistance As Double = 150.0
        Private Const predatorDistance2 As Double = predatorDistance * predatorDistance

        Private Const cohesionPower As Double = 0.0003
        Private Const alignPower As Double = 0.01
        Private Const avoidPower As Double = 0.001
        Private Const predatorPower As Double = 0.00005

        Friend ReadOnly field As Field3D

        ''' <summary>
        ''' create a solver for the given three dimensional field
        ''' </summary>
        ''' <param name="field">the field whose flocking rules are evaluated</param>
        Public Sub New(field As Field3D)
            MyBase.New(field.State.count)
            Me.field = field
        End Sub

        ''' <summary>
        ''' evaluate the rules for the boids of the slot range [start, ends]
        ''' </summary>
        ''' <param name="start">the first slot of the cell sorted boid id list</param>
        ''' <param name="ends">the last (inclusive) slot of the range</param>
        ''' <param name="cpu_id">the id of the worker that owns the range</param>
        Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
            Dim state = field.State
            Dim grid = field.Grid
            Dim n As Integer = state.count

            If n = 0 OrElse grid.nCells = 0 Then Return

            Dim px = state.px
            Dim py = state.py
            Dim pz = state.pz
            Dim vx = state.vx
            Dim vy = state.vy
            Dim vz = state.vz
            Dim dvx = state.dvx
            Dim dvy = state.dvy
            Dim dvz = state.dvz
            Dim cellStart = grid.cellStart
            Dim sorted = grid.sortedIds
            Dim cellOfX = grid.cellOfX
            Dim cellOfY = grid.cellOfY
            Dim cellOfZ = grid.cellOfZ
            Dim nx = grid.nx
            Dim ny = grid.ny
            Dim nz = grid.nz
            Dim minCx = grid.minCx
            Dim minCy = grid.minCy
            Dim minCz = grid.minCz

            Dim predCount As Integer = field.PredatorCount
            If predCount > n Then predCount = n
            Dim predEnd As Integer = predCount - 1

            ' The task partitions are interpreted as ranges over the cell-sorted
            ' boid id list (<see cref="FlockGrid3D.sortedIds"/>): consecutive boids
            ' share grid cells, so their neighbor data stays hot in cache; the
            ' per-boid math, the neighbor enumeration order and the accumulation
            ' order are all identical to processing the boids in index order.
            For slot As Integer = start To ends
                Dim i As Integer = sorted(slot)
                Dim bx = px(i)
                Dim by = py(i)
                Dim bz = pz(i)
                Dim bvx = vx(i)
                Dim bvy = vy(i)
                Dim bvz = vz(i)

                ' cohesion (flock) + alignment accumulators over the neighbors
                ' within the perception radius
                Dim sumX As Double = 0.0
                Dim sumY As Double = 0.0
                Dim sumZ As Double = 0.0
                Dim sumVX As Double = 0.0
                Dim sumVY As Double = 0.0
                Dim sumVZ As Double = 0.0
                Dim nCount As Integer = 0
                ' separation accumulator over the neighbors within the avoid radius
                Dim avoidX As Double = 0.0
                Dim avoidY As Double = 0.0
                Dim avoidZ As Double = 0.0

                Dim cx = cellOfX(i)
                Dim cy = cellOfY(i)
                Dim cz = cellOfZ(i)

                For dz As Integer = -1 To 1
                    Dim gz As Integer = cz + dz - minCz
                    If gz < 0 OrElse gz >= nz Then Continue For
                    Dim planeOffset As Integer = gz * ny * nx

                    For dy As Integer = -1 To 1
                        Dim gy As Integer = cy + dy - minCy
                        If gy < 0 OrElse gy >= ny Then Continue For
                        Dim rowOffset As Integer = planeOffset + gy * nx

                        For dx As Integer = -1 To 1
                            Dim gx As Integer = cx + dx - minCx
                            If gx < 0 OrElse gx >= nx Then Continue For

                            Dim cid As Integer = rowOffset + gx
                            Dim kEnd As Integer = cellStart(cid + 1)

                            For k As Integer = cellStart(cid) To kEnd - 1
                                Dim j As Integer = sorted(k)
                                Dim ddx As Double = bx - px(j)
                                Dim ddy As Double = by - py(j)
                                Dim ddz As Double = bz - pz(j)
                                Dim d2 As Double = ddx * ddx + ddy * ddy + ddz * ddz

                                If d2 < flockDistance2 Then
                                    sumX += px(j)
                                    sumY += py(j)
                                    sumZ += pz(j)
                                    sumVX += vx(j)
                                    sumVY += vy(j)
                                    sumVZ += vz(j)
                                    nCount += 1

                                    If d2 < avoidDistance2 Then
                                        Dim closeness As Double = avoidDistance - std.Sqrt(d2)
                                        avoidX += ddx * closeness
                                        avoidY += ddy * closeness
                                        avoidZ += ddz * closeness
                                    End If
                                End If
                            Next
                        Next
                    Next
                Next

                ' self is always its own neighbor (distance 0), so nCount >= 1
                Dim invN As Double = 1.0 / nCount
                Dim flockX As Double = (sumX * invN - bx) * cohesionPower
                Dim flockY As Double = (sumY * invN - by) * cohesionPower
                Dim flockZ As Double = (sumZ * invN - bz) * cohesionPower
                Dim alignX As Double = (sumVX * invN - bvx) * alignPower
                Dim alignY As Double = (sumVY * invN - bvy) * alignPower
                Dim alignZ As Double = (sumVZ * invN - bvz) * alignPower
                Dim avoidFx As Double = avoidX * avoidPower
                Dim avoidFy As Double = avoidY * avoidPower
                Dim avoidFz As Double = avoidZ * avoidPower

                ' predators: the first PredatorCount boids of the simulation
                Dim predX As Double = 0.0
                Dim predY As Double = 0.0
                Dim predZ As Double = 0.0

                For p As Integer = 0 To predEnd
                    Dim ddx As Double = bx - px(p)
                    Dim ddy As Double = by - py(p)
                    Dim ddz As Double = bz - pz(p)
                    Dim d2 As Double = ddx * ddx + ddy * ddy + ddz * ddz

                    If d2 < predatorDistance2 Then
                        Dim closeness As Double = predatorDistance - std.Sqrt(d2)
                        predX += ddx * closeness
                        predY += ddy * closeness
                        predZ += ddz * closeness
                    End If
                Next

                ' same accumulation order as the two dimensional solver:
                ' v += cohesion + separation + alignment + predator
                dvx(i) = flockX + avoidFx + alignX + predX * predatorPower
                dvy(i) = flockY + avoidFy + alignY + predY * predatorPower
                dvz(i) = flockZ + avoidFz + alignZ + predZ * predatorPower
            Next
        End Sub
    End Class

    ''' <summary>
    ''' The streaming integration kernel of the three dimensional flocking
    ''' simulation: apply the rule forces, integrate the positions, clamp the
    ''' speed, bounce off the six walls and optionally wrap around the edges.
    ''' </summary>
    Friend Module FlockIntegrate3D

        ''' <summary> wall bounce steering band, in simulation units </summary>
        Private Const bouncePad As Double = 50.0
        ''' <summary> wall bounce steering force </summary>
        Private Const bounceTurn As Double = 0.5

        ''' <summary>
        ''' Advances the whole simulation state by one time step.
        ''' </summary>
        ''' <param name="state">the SoA state to integrate</param>
        ''' <param name="width">the size of the box along x</param>
        ''' <param name="height">the size of the box along y</param>
        ''' <param name="depth">the size of the box along z</param>
        ''' <param name="bounceOffWalls">steer the boids away from the walls</param>
        ''' <param name="wrapAroundEdges">teleport the boids that leave the box</param>
        ''' <param name="minSpeed">the lowest speed that a boid keeps</param>
        ''' <param name="maxSpeed">the highest speed that a boid may reach</param>
        Friend Sub Integrate(state As FlockState3D, width As Double, height As Double, depth As Double,
                             bounceOffWalls As Boolean, wrapAroundEdges As Boolean,
                             minSpeed As Double, maxSpeed As Double)
            Dim n As Integer = state.count

            If n = 0 Then Return

            If SIMDEnvironment.IsEnabled AndAlso Vector.IsHardwareAccelerated Then
                IntegrateSimd(state, width, height, depth, bounceOffWalls, wrapAroundEdges, minSpeed, maxSpeed)
            Else
                IntegrateScalar(state, width, height, depth, bounceOffWalls, wrapAroundEdges, minSpeed, maxSpeed)
            End If
        End Sub

        ''' <summary>
        ''' 4-lane (AVX2) streaming integration over the SoA state.
        ''' </summary>
        Private Sub IntegrateSimd(state As FlockState3D, width As Double, height As Double, depth As Double,
                                  bounce As Boolean, wrap As Boolean,
                                  minSpeed As Double, maxSpeed As Double)
            Dim px = state.px
            Dim py = state.py
            Dim pz = state.pz
            Dim vx = state.vx
            Dim vy = state.vy
            Dim vz = state.vz
            Dim dvx = state.dvx
            Dim dvy = state.dvy
            Dim dvz = state.dvz
            Dim n As Integer = state.count
            Dim vecCount As Integer = Vector(Of Double).Count

            Dim vZero As Vector(Of Double) = Vector(Of Double).Zero
            Dim vMin As Vector(Of Double) = New Vector(Of Double)(minSpeed)
            Dim vMax As Vector(Of Double) = New Vector(Of Double)(maxSpeed)
            Dim vPad As Vector(Of Double) = New Vector(Of Double)(bouncePad)
            Dim vTurn As Vector(Of Double) = New Vector(Of Double)(bounceTurn)
            Dim vRight As Vector(Of Double) = New Vector(Of Double)(width - bouncePad)
            Dim vTop As Vector(Of Double) = New Vector(Of Double)(height - bouncePad)
            Dim vBack As Vector(Of Double) = New Vector(Of Double)(depth - bouncePad)
            Dim vWidth As Vector(Of Double) = New Vector(Of Double)(width)
            Dim vHeight As Vector(Of Double) = New Vector(Of Double)(height)
            Dim vDepth As Vector(Of Double) = New Vector(Of Double)(depth)

            Dim i As Integer = 0

            If n >= vecCount Then
                Dim last As Integer = n - vecCount

                Do While i <= last
                    ' the rule force delta is applied to the velocity FIRST, the
                    ' position is then integrated with the updated velocity, which
                    ' is the legacy MoveForward ordering
                    Dim vecVX As Vector(Of Double) = New Vector(Of Double)(vx, i) + New Vector(Of Double)(dvx, i)
                    Dim vecVY As Vector(Of Double) = New Vector(Of Double)(vy, i) + New Vector(Of Double)(dvy, i)
                    Dim vecVZ As Vector(Of Double) = New Vector(Of Double)(vz, i) + New Vector(Of Double)(dvz, i)
                    Dim vecX As Vector(Of Double) = New Vector(Of Double)(px, i) + vecVX
                    Dim vecY As Vector(Of Double) = New Vector(Of Double)(py, i) + vecVY
                    Dim vecZ As Vector(Of Double) = New Vector(Of Double)(pz, i) + vecVZ

                    ' speed clamp: factor = min(max(speed, minSpeed), maxSpeed) / speed
                    Dim sp As Vector(Of Double) = Vector.SquareRoot(vecVX * vecVX + vecVY * vecVY + vecVZ * vecVZ)
                    Dim factor As Vector(Of Double) = Vector.Divide(Vector.Min(Vector.Max(sp, vMin), vMax), sp)

                    vecVX = vecVX * factor
                    vecVY = vecVY * factor
                    vecVZ = vecVZ * factor

                    ' NaN guard: a lane whose velocity is NaN (or zero, which stays
                    ' zero) is written back as zero, matching the legacy semantics
                    ' where 0/0 -> NaN -> 0
                    Dim keepX As Vector(Of Double) = Vector.BitwiseOr(Vector.LessThan(vecVX, vZero), Vector.GreaterThan(vecVX, vZero))
                    Dim keepY As Vector(Of Double) = Vector.BitwiseOr(Vector.LessThan(vecVY, vZero), Vector.GreaterThan(vecVY, vZero))
                    Dim keepZ As Vector(Of Double) = Vector.BitwiseOr(Vector.LessThan(vecVZ, vZero), Vector.GreaterThan(vecVZ, vZero))

                    vecVX = Vector.ConditionalSelect(keepX, vecVX, vZero)
                    vecVY = Vector.ConditionalSelect(keepY, vecVY, vZero)
                    vecVZ = Vector.ConditionalSelect(keepZ, vecVZ, vZero)

                    If bounce Then
                        vecVX = Vector.ConditionalSelect(Vector.LessThan(vecX, vPad), vecVX + vTurn, vecVX)
                        vecVX = Vector.ConditionalSelect(Vector.GreaterThan(vecX, vRight), vecVX - vTurn, vecVX)
                        vecVY = Vector.ConditionalSelect(Vector.LessThan(vecY, vPad), vecVY + vTurn, vecVY)
                        vecVY = Vector.ConditionalSelect(Vector.GreaterThan(vecY, vTop), vecVY - vTurn, vecVY)
                        vecVZ = Vector.ConditionalSelect(Vector.LessThan(vecZ, vPad), vecVZ + vTurn, vecVZ)
                        vecVZ = Vector.ConditionalSelect(Vector.GreaterThan(vecZ, vBack), vecVZ - vTurn, vecVZ)
                    End If

                    If wrap Then
                        vecX = Vector.ConditionalSelect(Vector.LessThan(vecX, vZero), vecX + vWidth, vecX)
                        vecX = Vector.ConditionalSelect(Vector.GreaterThan(vecX, vWidth), vecX - vWidth, vecX)
                        vecY = Vector.ConditionalSelect(Vector.LessThan(vecY, vZero), vecY + vHeight, vecY)
                        vecY = Vector.ConditionalSelect(Vector.GreaterThan(vecY, vHeight), vecY - vHeight, vecY)
                        vecZ = Vector.ConditionalSelect(Vector.LessThan(vecZ, vZero), vecZ + vDepth, vecZ)
                        vecZ = Vector.ConditionalSelect(Vector.GreaterThan(vecZ, vDepth), vecZ - vDepth, vecZ)
                    End If

                    vecX.CopyTo(px, i)
                    vecY.CopyTo(py, i)
                    vecZ.CopyTo(pz, i)
                    vecVX.CopyTo(vx, i)
                    vecVY.CopyTo(vy, i)
                    vecVZ.CopyTo(vz, i)

                    i += vecCount
                Loop
            End If

            ' scalar tail
            For k As Integer = i To n - 1
                StepScalar(px, py, pz, vx, vy, vz, dvx, dvy, dvz, k,
                           width, height, depth, bounce, wrap, minSpeed, maxSpeed)
            Next
        End Sub

        ''' <summary>
        ''' Single thread streaming integration, also used as the fallback path
        ''' when the SIMD is disabled or the hardware is not accelerated.
        ''' </summary>
        Private Sub IntegrateScalar(state As FlockState3D, width As Double, height As Double, depth As Double,
                                    bounce As Boolean, wrap As Boolean,
                                    minSpeed As Double, maxSpeed As Double)
            Dim px = state.px
            Dim py = state.py
            Dim pz = state.pz
            Dim vx = state.vx
            Dim vy = state.vy
            Dim vz = state.vz
            Dim dvx = state.dvx
            Dim dvy = state.dvy
            Dim dvz = state.dvz
            Dim n As Integer = state.count

            For i As Integer = 0 To n - 1
                StepScalar(px, py, pz, vx, vy, vz, dvx, dvy, dvz, i,
                           width, height, depth, bounce, wrap, minSpeed, maxSpeed)
            Next
        End Sub

        ''' <summary>
        ''' Advance one boid slot: apply the rule force delta, integrate the
        ''' position, clamp the speed and apply the wall rules.
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub StepScalar(px As Double(), py As Double(), pz As Double(),
                               vx As Double(), vy As Double(), vz As Double(),
                               dvx As Double(), dvy As Double(), dvz As Double(), i As Integer,
                               width As Double, height As Double, depth As Double,
                               bounce As Boolean, wrap As Boolean,
                               minSpeed As Double, maxSpeed As Double)
            Dim vxv As Double = vx(i) + dvx(i)
            Dim vyv As Double = vy(i) + dvy(i)
            Dim vzv As Double = vz(i) + dvz(i)
            Dim x As Double = px(i) + vxv
            Dim y As Double = py(i) + vyv
            Dim z As Double = pz(i) + vzv

            Dim speed As Double = std.Sqrt(vxv * vxv + vyv * vyv + vzv * vzv)

            If speed > maxSpeed Then
                vxv = vxv / speed * maxSpeed
                vyv = vyv / speed * maxSpeed
                vzv = vzv / speed * maxSpeed
            ElseIf speed < minSpeed Then
                vxv = vxv / speed * minSpeed
                vyv = vyv / speed * minSpeed
                vzv = vzv / speed * minSpeed
            End If

            If Double.IsNaN(vxv) Then vxv = 0
            If Double.IsNaN(vyv) Then vyv = 0
            If Double.IsNaN(vzv) Then vzv = 0

            If bounce Then
                If x < bouncePad Then vxv += bounceTurn
                If x > width - bouncePad Then vxv -= bounceTurn
                If y < bouncePad Then vyv += bounceTurn
                If y > height - bouncePad Then vyv -= bounceTurn
                If z < bouncePad Then vzv += bounceTurn
                If z > depth - bouncePad Then vzv -= bounceTurn
            End If

            If wrap Then
                If x < 0 Then x += width
                If x > width Then x -= width
                If y < 0 Then y += height
                If y > height Then y -= height
                If z < 0 Then z += depth
                If z > depth Then z -= depth
            End If

            px(i) = x
            py(i) = y
            pz(i) = z
            vx(i) = vxv
            vy(i) = vyv
            vz(i) = vzv
        End Sub
    End Module
End Namespace
