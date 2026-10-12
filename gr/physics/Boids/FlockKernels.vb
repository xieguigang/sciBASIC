#Region "Microsoft.VisualBasic::9c3d72e0a18b45d6f4b2c8e0d1a7f534, gr\physics\Boids\FlockKernels.vb"

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

    ' The SIMD compute kernels of the flocking engine.

    ' <see cref="FlockSolver"/> evaluates the four flocking rules (cohesion, 
    ' alignment, separation, predator avoidance) for a boid index range on top 
    ' of the counting-sort grid. It is two-phase: rule forces are accumulated 
    ' into per-boid delta arrays so that every boid reads the frame-start 
    ' velocities of its neighbors (the legacy parallel implementation raced on 
    ' partially updated velocities). Distances are pre-filtered by squared 
    ' magnitude so that sqrt is only evaluated for accepted pairs.

    ' <see cref="FlockIntegrate"/> is a single streaming pass that applies the 
    ' rule forces, integrates the positions, clamps the speed and applies the 
    ' wall bounce / wrap rules. It runs 4 lanes of <see cref="Vector(Of Double)"/> 
    ' (AVX2) when the hardware and <see cref="Math.SIMD.SIMDEnvironment"/> allow 
    ' it, and falls back to the scalar path otherwise (the 
    ' <c>SIMDConfiguration.disable</c> escape hatch keeps working).

    '     Class FlockSolver
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Solve
    ' 
    '     Module FlockIntegrate
    ' 
    '         Sub: Integrate, IntegrateScalar, IntegrateSimd, StepScalar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.SIMD
Imports Microsoft.VisualBasic.Parallel
Imports std = System.Math

Namespace Boids

    ''' <summary>
    ''' Evaluate the flocking rules for a contiguous boid index range.
    ''' </summary>
    ''' <remarks>
    ''' Runs inside a <see cref="VectorTask"/> parallel range; the grid is 
    ''' read-only during the solve and each boid only writes its own delta slot, 
    ''' so the parallel execution is race free.
    ''' </remarks>
    Friend NotInheritable Class FlockSolver
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

        Friend ReadOnly field As Field

        Public Sub New(field As Field)
            MyBase.New(field.State.count)
            Me.field = field
        End Sub

        Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
            Dim state = field.State
            Dim grid = field.Grid
            Dim n As Integer = state.count

            If n = 0 OrElse grid.nCells = 0 Then Return

            Dim px = state.px
            Dim py = state.py
            Dim vx = state.vx
            Dim vy = state.vy
            Dim dvx = state.dvx
            Dim dvy = state.dvy
            Dim cellStart = grid.cellStart
            Dim sorted = grid.sortedIds
            Dim cellOfX = grid.cellOfX
            Dim cellOfY = grid.cellOfY
            Dim nx = grid.nx
            Dim ny = grid.ny
            Dim minCx = grid.minCx
            Dim minCy = grid.minCy

            Dim predCount As Integer = field.PredatorCount
            If predCount > n Then predCount = n
            Dim predEnd As Integer = predCount - 1

            For i As Integer = start To ends
                Dim bx = px(i)
                Dim by = py(i)
                Dim bvx = vx(i)
                Dim bvy = vy(i)

                ' cohesion (flock) + alignment accumulators over neighbors within 50
                Dim sumX As Double = 0.0
                Dim sumY As Double = 0.0
                Dim sumVX As Double = 0.0
                Dim sumVY As Double = 0.0
                Dim nCount As Integer = 0
                ' separation accumulator over neighbors within 20
                Dim avoidX As Double = 0.0
                Dim avoidY As Double = 0.0

                Dim cx = cellOfX(i)
                Dim cy = cellOfY(i)

                For dy As Integer = -1 To 1
                    Dim gy As Integer = cy + dy - minCy + 1
                    If gy < 0 OrElse gy >= ny Then Continue For
                    Dim rowOffset As Integer = gy * nx

                    For dx As Integer = -1 To 1
                        Dim gx As Integer = cx + dx - minCx + 1
                        If gx < 0 OrElse gx >= nx Then Continue For

                        Dim cid As Integer = rowOffset + gx
                        Dim kEnd As Integer = cellStart(cid + 1)

                        For k As Integer = cellStart(cid) To kEnd - 1
                            Dim j As Integer = sorted(k)
                            Dim ddx As Double = bx - px(j)
                            Dim ddy As Double = by - py(j)
                            Dim d2 As Double = ddx * ddx + ddy * ddy

                            If d2 < flockDistance2 Then
                                sumX += px(j)
                                sumY += py(j)
                                sumVX += vx(j)
                                sumVY += vy(j)
                                nCount += 1

                                If d2 < avoidDistance2 Then
                                    Dim closeness As Double = avoidDistance - std.Sqrt(d2)
                                    avoidX += ddx * closeness
                                    avoidY += ddy * closeness
                                End If
                            End If
                        Next
                    Next
                Next

                ' self is always its own neighbor (distance 0), so nCount >= 1
                Dim invN As Double = 1.0 / nCount
                Dim flockX As Double = (sumX * invN - bx) * cohesionPower
                Dim flockY As Double = (sumY * invN - by) * cohesionPower
                Dim alignX As Double = (sumVX * invN - bvx) * alignPower
                Dim alignY As Double = (sumVY * invN - bvy) * alignPower
                Dim avoidFx As Double = avoidX * avoidPower
                Dim avoidFy As Double = avoidY * avoidPower

                ' predators: the first PredatorCount boids of the simulation
                Dim predX As Double = 0.0
                Dim predY As Double = 0.0

                For p As Integer = 0 To predEnd
                    Dim ddx As Double = bx - px(p)
                    Dim ddy As Double = by - py(p)
                    Dim d2 As Double = ddx * ddx + ddy * ddy

                    If d2 < predatorDistance2 Then
                        Dim closeness As Double = predatorDistance - std.Sqrt(d2)
                        predX += ddx * closeness
                        predY += ddy * closeness
                    End If
                Next

                ' same accumulation order as the legacy implementation:
                ' v += cohesion + separation + alignment + predator
                dvx(i) = flockX + avoidFx + alignX + predX * predatorPower
                dvy(i) = flockY + avoidFy + alignY + predY * predatorPower
            Next
        End Sub
    End Class

    ''' <summary>
    ''' The streaming integration kernel: apply rule forces, integrate positions, 
    ''' clamp speed, bounce off walls and optionally wrap around edges.
    ''' </summary>
    Friend Module FlockIntegrate

        ''' <summary> wall bounce steering band, in simulation units </summary>
        Private Const bouncePad As Double = 50.0
        ''' <summary> wall bounce steering force </summary>
        Private Const bounceTurn As Double = 0.5

        ''' <summary>
        ''' Advances the whole simulation state by one time step.
        ''' </summary>
        Friend Sub Integrate(state As FlockState, width As Double, height As Double,
                             bounceOffWalls As Boolean, wrapAroundEdges As Boolean,
                             minSpeed As Double, maxSpeed As Double)
            Dim n As Integer = state.count

            If n = 0 Then Return

            If SIMDEnvironment.IsEnabled AndAlso Vector.IsHardwareAccelerated Then
                IntegrateSimd(state, width, height, bounceOffWalls, wrapAroundEdges, minSpeed, maxSpeed)
            Else
                IntegrateScalar(state, width, height, bounceOffWalls, wrapAroundEdges, minSpeed, maxSpeed)
            End If
        End Sub

        ''' <summary>
        ''' 4-lane (AVX2) streaming integration over the SoA state.
        ''' </summary>
        Private Sub IntegrateSimd(state As FlockState, width As Double, height As Double,
                                  bounce As Boolean, wrap As Boolean,
                                  minSpeed As Double, maxSpeed As Double)
            Dim px = state.px
            Dim py = state.py
            Dim vx = state.vx
            Dim vy = state.vy
            Dim dvx = state.dvx
            Dim dvy = state.dvy
            Dim n As Integer = state.count
            Dim vecCount As Integer = Vector(Of Double).Count

            Dim vZero As Vector(Of Double) = Vector(Of Double).Zero
            Dim vMin As Vector(Of Double) = New Vector(Of Double)(minSpeed)
            Dim vMax As Vector(Of Double) = New Vector(Of Double)(maxSpeed)
            Dim vPad As Vector(Of Double) = New Vector(Of Double)(bouncePad)
            Dim vTurn As Vector(Of Double) = New Vector(Of Double)(bounceTurn)
            Dim vRight As Vector(Of Double) = New Vector(Of Double)(width - bouncePad)
            Dim vBottom As Vector(Of Double) = New Vector(Of Double)(height - bouncePad)
            Dim vWidth As Vector(Of Double) = New Vector(Of Double)(width)
            Dim vHeight As Vector(Of Double) = New Vector(Of Double)(height)

            Dim i As Integer = 0

            If n >= vecCount Then
                Dim last As Integer = n - vecCount

                Do While i <= last
                    Dim X As Vector(Of Double) = New Vector(Of Double)(px, i) + New Vector(Of Double)(vx, i)
                    Dim Y As Vector(Of Double) = New Vector(Of Double)(py, i) + New Vector(Of Double)(vy, i)
                    Dim VX As Vector(Of Double) = New Vector(Of Double)(vx, i) + New Vector(Of Double)(dvx, i)
                    Dim VY As Vector(Of Double) = New Vector(Of Double)(vy, i) + New Vector(Of Double)(dvy, i)

                    ' speed clamp: factor = min(max(speed, minSpeed), maxSpeed) / speed
                    Dim sp2 As Vector(Of Double) = VX * VX + VY * VY
                    Dim sp As Vector(Of Double) = Vector.SquareRoot(sp2)
                    Dim factor As Vector(Of Double) = Vector.Divide(Vector.Min(Vector.Max(sp, vMin), vMax), sp)

                    VX = VX * factor
                    VY = VY * factor

                    ' NaN guard: a lane whose velocity is NaN (or zero, which stays
                    ' zero) is written back as zero, matching the legacy semantics
                    ' where 0/0 -> NaN -> 0
                    Dim keepX As Vector(Of Double) = Vector.BitwiseOr(Vector.LessThan(VX, vZero), Vector.GreaterThan(VX, vZero))
                    Dim keepY As Vector(Of Double) = Vector.BitwiseOr(Vector.LessThan(VY, vZero), Vector.GreaterThan(VY, vZero))

                    VX = Vector.ConditionalSelect(keepX, VX, vZero)
                    VY = Vector.ConditionalSelect(keepY, VY, vZero)

                    If bounce Then
                        VX = Vector.ConditionalSelect(Vector.LessThan(X, vPad), VX + vTurn, VX)
                        VX = Vector.ConditionalSelect(Vector.GreaterThan(X, vRight), VX - vTurn, VX)
                        VY = Vector.ConditionalSelect(Vector.LessThan(Y, vPad), VY + vTurn, VY)
                        VY = Vector.ConditionalSelect(Vector.GreaterThan(Y, vBottom), VY - vTurn, VY)
                    End If

                    If wrap Then
                        X = Vector.ConditionalSelect(Vector.LessThan(X, vZero), X + vWidth, X)
                        X = Vector.ConditionalSelect(Vector.GreaterThan(X, vWidth), X - vWidth, X)
                        Y = Vector.ConditionalSelect(Vector.LessThan(Y, vZero), Y + vHeight, Y)
                        Y = Vector.ConditionalSelect(Vector.GreaterThan(Y, vHeight), Y - vHeight, Y)
                    End If

                    X.CopyTo(px, i)
                    Y.CopyTo(py, i)
                    VX.CopyTo(vx, i)
                    VY.CopyTo(vy, i)

                    i += vecCount
                Loop
            End If

            ' scalar tail
            For k As Integer = i To n - 1
                StepScalar(px, py, vx, vy, dvx, dvy, k, width, height, bounce, wrap, minSpeed, maxSpeed)
            Next
        End Sub

        ''' <summary>
        ''' Single thread streaming integration, also used as the fallback path
        ''' when the SIMD is disabled or the hardware is not accelerated.
        ''' </summary>
        Private Sub IntegrateScalar(state As FlockState, width As Double, height As Double,
                                    bounce As Boolean, wrap As Boolean,
                                    minSpeed As Double, maxSpeed As Double)
            Dim px = state.px
            Dim py = state.py
            Dim vx = state.vx
            Dim vy = state.vy
            Dim dvx = state.dvx
            Dim dvy = state.dvy
            Dim n As Integer = state.count

            For i As Integer = 0 To n - 1
                StepScalar(px, py, vx, vy, dvx, dvy, i, width, height, bounce, wrap, minSpeed, maxSpeed)
            Next
        End Sub

        ''' <summary>
        ''' Advance one boid slot: apply the rule force delta, integrate the
        ''' position, clamp the speed and apply the wall rules.
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub StepScalar(px As Double(), py As Double(), vx As Double(), vy As Double(),
                               dvx As Double(), dvy As Double(), i As Integer,
                               width As Double, height As Double,
                               bounce As Boolean, wrap As Boolean,
                               minSpeed As Double, maxSpeed As Double)
            Dim vxv As Double = vx(i) + dvx(i)
            Dim vyv As Double = vy(i) + dvy(i)
            Dim x As Double = px(i) + vxv
            Dim y As Double = py(i) + vyv

            Dim speed As Double = std.Sqrt(vxv * vxv + vyv * vyv)

            If speed > maxSpeed Then
                vxv = vxv / speed * maxSpeed
                vyv = vyv / speed * maxSpeed
            ElseIf speed < minSpeed Then
                vxv = vxv / speed * minSpeed
                vyv = vyv / speed * minSpeed
            End If

            If Double.IsNaN(vxv) Then vxv = 0
            If Double.IsNaN(vyv) Then vyv = 0

            If bounce Then
                If x < bouncePad Then vxv += bounceTurn
                If x > width - bouncePad Then vxv -= bounceTurn
                If y < bouncePad Then vyv += bounceTurn
                If y > height - bouncePad Then vyv -= bounceTurn
            End If

            If wrap Then
                If x < 0 Then x += width
                If x > width Then x -= width
                If y < 0 Then y += height
                If y > height Then y -= height
            End If

            px(i) = x
            py(i) = y
            vx(i) = vxv
            vy(i) = vyv
        End Sub
    End Module
End Namespace
