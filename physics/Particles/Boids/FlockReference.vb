#Region "Microsoft.VisualBasic::06898043eb83b7eddf8fd56e12b842d5, physics\Particles\Boids\FlockReference.vb"

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

    '   Total Lines: 159
    '    Code Lines: 115 (72.33%)
    ' Comment Lines: 17 (10.69%)
    '    - Xml Docs: 76.47%
    ' 
    '   Blank Lines: 27 (16.98%)
    '     File Size: 6.54 KB


    '     Class FlockReference
    ' 
    '         Sub: Advance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Boids

    ''' <summary>
    ''' Legacy-semantics reference engine (development validation only).
    ''' </summary>
    ''' <remarks>
    ''' The rules, the constants, the accumulation orders and the integrate 
    ''' semantics mirror the legacy <see cref="Field"/> implementation exactly, 
    ''' including its neighbor candidate restriction: the legacy spatial hash 
    ''' used 15px cells and only probed the 3x3 cell block around each boid 
    ''' (so the effective perception set is the intersection of that box with 
    ''' the distance filter, NOT the full distance circle). The solve is two 
    ''' phase here so the result is deterministic; neighbor enumeration order 
    ''' differs from the grid order by ULP amounts only.
    ''' </remarks>
    Friend NotInheritable Class FlockReference

        Const cellSize As Double = 15.0
        Const flockDistance As Double = 50.0
        Const avoidDistance As Double = 20.0
        Const predatorDistance As Double = 150.0

        Const cohesionPower As Double = 0.0003
        Const alignPower As Double = 0.01
        Const avoidPower As Double = 0.001
        Const predatorPower As Double = 0.00005

        Const bouncePad As Double = 50.0
        Const bounceTurn As Double = 0.5

        Friend Shared Sub Advance(state As FlockState, width As Double, height As Double,
                                  bounceOffWalls As Boolean, wrapAroundEdges As Boolean,
                                  predatorCount As Integer)
            Dim n As Integer = state.count
            If n = 0 Then Return

            Dim px = state.px
            Dim py = state.py
            Dim vx = state.vx
            Dim vy = state.vy
            Dim dvx = state.dvx
            Dim dvy = state.dvy

            Dim predEnd As Integer = std.Min(predatorCount, n) - 1

            ' phase 1: rule forces from the frame-start state
            For i As Integer = 0 To n - 1
                Dim bx = px(i)
                Dim by = py(i)
                Dim icx As Integer = CInt(bx / cellSize)
                Dim icy As Integer = CInt(by / cellSize)

                Dim sumX As Double = 0.0
                Dim sumY As Double = 0.0
                Dim sumVX As Double = 0.0
                Dim sumVY As Double = 0.0
                Dim nCount As Integer = 0
                Dim avoidX As Double = 0.0
                Dim avoidY As Double = 0.0

                For j As Integer = 0 To n - 1
                    ' legacy candidate restriction: only the 3x3 cell block 
                    ' around the boid's own cell is probed
                    Dim jcx As Integer = CInt(px(j) / cellSize)
                    Dim jcy As Integer = CInt(py(j) / cellSize)

                    If jcx < icx - 1 OrElse jcx > icx + 1 OrElse
                       jcy < icy - 1 OrElse jcy > icy + 1 Then Continue For

                    Dim ddx As Double = bx - px(j)
                    Dim ddy As Double = by - py(j)
                    Dim d2 As Double = ddx * ddx + ddy * ddy

                    If d2 < flockDistance * flockDistance Then
                        sumX += px(j)
                        sumY += py(j)
                        sumVX += vx(j)
                        sumVY += vy(j)
                        nCount += 1

                        If d2 < avoidDistance * avoidDistance Then
                            Dim closeness As Double = avoidDistance - std.Sqrt(d2)
                            avoidX += ddx * closeness
                            avoidY += ddy * closeness
                        End If
                    End If
                Next

                Dim invN As Double = 1.0 / nCount
                Dim flockX As Double = (sumX * invN - bx) * cohesionPower
                Dim flockY As Double = (sumY * invN - by) * cohesionPower
                Dim alignX As Double = (sumVX * invN - vx(i)) * alignPower
                Dim alignY As Double = (sumVY * invN - vy(i)) * alignPower
                Dim avoidFx As Double = avoidX * avoidPower
                Dim avoidFy As Double = avoidY * avoidPower

                Dim predX As Double = 0.0
                Dim predY As Double = 0.0

                For p As Integer = 0 To predEnd
                    Dim ddx As Double = bx - px(p)
                    Dim ddy As Double = by - py(p)
                    Dim d2 As Double = ddx * ddx + ddy * ddy

                    If d2 < predatorDistance * predatorDistance Then
                        Dim closeness As Double = predatorDistance - std.Sqrt(d2)
                        predX += ddx * closeness
                        predY += ddy * closeness
                    End If
                Next

                dvx(i) = flockX + avoidFx + alignX + predX * predatorPower
                dvy(i) = flockY + avoidFy + alignY + predY * predatorPower
            Next

            ' phase 2: integrate, exactly like the legacy MoveForward + wall rules
            For i As Integer = 0 To n - 1
                Dim vxv As Double = vx(i) + dvx(i)
                Dim vyv As Double = vy(i) + dvy(i)
                Dim x As Double = px(i) + vxv
                Dim y As Double = py(i) + vyv

                Dim speed As Double = std.Sqrt(vxv * vxv + vyv * vyv)

                If speed > 6.0 Then
                    vxv = vxv / speed * 6.0
                    vyv = vyv / speed * 6.0
                ElseIf speed < 1.0 Then
                    vxv = vxv / speed * 1.0
                    vyv = vyv / speed * 1.0
                End If

                If Double.IsNaN(vxv) Then vxv = 0
                If Double.IsNaN(vyv) Then vyv = 0

                If bounceOffWalls Then
                    If x < bouncePad Then vxv += bounceTurn
                    If x > width - bouncePad Then vxv -= bounceTurn
                    If y < bouncePad Then vyv += bounceTurn
                    If y > height - bouncePad Then vyv -= bounceTurn
                End If

                If wrapAroundEdges Then
                    If x < 0 Then x += width
                    If x > width Then x -= width
                    If y < 0 Then y += height
                    If y > height Then y -= height
                End If

                px(i) = x
                py(i) = y
                vx(i) = vxv
                vy(i) = vyv
            Next
        End Sub
    End Class
End Namespace
