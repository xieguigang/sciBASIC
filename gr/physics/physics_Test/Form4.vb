#Region "Microsoft.VisualBasic::198f119e3ca30211d6227847796d5544, gr\physics\physics_Test\Form4.vb"

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

    '   Total Lines: 459
    '    Code Lines: 343 (74.73%)
    ' Comment Lines: 11 (2.40%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 105 (22.88%)
    '     File Size: 14.03 KB


    ' Class Form4
    ' 
    '     Sub: [step], applyViscosity, draw, findNeighbors, Form4_Load
    '          Form4_SizeChanged, Setup, simulate, solveBoundaries, solveFluid
    '          Timer1_Tick
    '     Class Vector
    ' 
    '         Properties: size
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Clear, pushBack
    ' 
    ' 
    ' 
    ' Class Hash
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class Particles
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Math
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Physics
Imports Microsoft.VisualBasic.MIME.Html.CSS

Public Class Form4

    Dim drawOrig As Vector2
    Dim drawScale As Single = 200

    ' global params
    Dim gravity As Single = -10
    Dim particleRadius As Single = 0.01
    Dim unilateral As Boolean = True
    Dim viscosity As Single = 0

    Dim timeStep As Single = 0.01
    Dim numIters As Integer = 1
    Dim numSubSteps As Integer = 10

    Dim maxParticles As Integer = 10000

    Dim numX As Integer = 10
    Dim numY As Integer = 1000
    Dim numParticles As Integer = numX * numY

    ' boundary
    Dim w As Single = 1.0
    Dim h As Single = 2.0

    Dim boundaries As Padding() = {
        New Padding With {.Left = -Width * 0.5 - 0.1, .Right = -Width * 0.5, .Bottom = -0.01, .Top = Height},
        New Padding With {.Left = Width * 0.5, .Right = Width * 0.5 + 0.1, .Bottom = -0.01, .Top = Height}
    }

    Dim fluidOrig As New Padding With {.Left = -0.3, .Bottom = 1.8}

    ' derived params
    Dim particleDiameter As Single = 2 * particleRadius
    Dim restDensity As Single = 1 / (particleDiameter ^ 2)
    Dim kernelRadius As Single = 3 * particleRadius
    Dim h2 As Single = kernelRadius ^ 2
    Dim kernelScale As Single = 4 / (PI * h2 ^ 4)
    ' 2d poly6 (SPH based shallow water simulation)

    Dim gridSpacing As Single = kernelRadius * 1.5
    Dim invGridSpacing As Single = 1 / gridSpacing

    Dim maxVel As Single = 0.4 * particleRadius

    Dim particles As New Particles(maxParticles)

    Dim i, j As Integer


    Public Class Vector

        Public ReadOnly vals As List(Of Integer)

        Public ReadOnly Property size As Integer
            Get
                Return vals.Count
            End Get
        End Property

        Sub New(size As Integer)
            vals = New List(Of Integer)(capacity:=size)
        End Sub

        Public Sub Clear()
            vals.Clear()
        End Sub

        Public Sub pushBack(val As Integer)
            vals.Add(val)
        End Sub

    End Class

    Dim hashSize As Integer = 370111
    Dim hash As New Hash(hashSize, maxParticles)
    Dim firstNeighbor As Integer() = New Integer(maxParticles + 1) {}
    Dim neighbors As Vector = New Vector(10 * maxParticles)

    Public Sub findNeighbors()
        ' hash particles
        hash.currentMark += 1

        For i As Integer = 0 To numParticles - 1
            Dim px = particles.pos(2 * i)
            Dim py = particles.pos(2 * i + 1)

            Dim gx = Floor((px - hash.orig.Left) * invGridSpacing)
            Dim gy = Floor((py - hash.orig.Bottom) * invGridSpacing)

            Dim h = Abs((gx * 92837111) Xor (gy * 689287499)) Mod hash.size

            If hash.marks(h) <> hash.currentMark Then
                hash.marks(h) = hash.currentMark
                hash.first(h) = -1
            End If

            hash.next(i) = hash.first(h)
            hash.first(h) = i
        Next

        ' collect neighbors
        neighbors.Clear()

        Dim h2 = gridSpacing ^ 2

        For i As Integer = 0 To numParticles - 1
            firstNeighbor(i) = neighbors.size

            Dim px = particles.pos(2 * i)
            Dim py = particles.pos(2 * i + 1)

            Dim gx = Floor((px - hash.orig.Left) * invGridSpacing)
            Dim gy = Floor((py - hash.orig.Bottom) * invGridSpacing)

            Dim x, y As Integer

            For x = gx - 1 To gx + 1
                For y = gy - 1 To gy + 1
                    Dim h = Abs((x * 92837111) Xor (y * 689287499)) Mod hash.size

                    If hash.marks(h) <> hash.currentMark Then
                        Continue For
                    End If

                    Dim id = hash.first(h)

                    Do While id >= 0
                        Dim dx = particles.pos(2 * id) - px
                        Dim dy = particles.pos(2 * id + 1) - py

                        If dx ^ 2 + dy ^ 2 < h2 Then
                            neighbors.pushBack(id)
                        End If

                        id = hash.next(id)
                    Loop
                Next
            Next
        Next

        firstNeighbor(numParticles) = neighbors.size
    End Sub

    Dim grads As Single() = New Single(1000 - 1) {}
    Dim sand As Boolean = False

    Public Sub solveFluid()
        Dim h = kernelRadius
        Dim h2 = h ^ 2
        Dim avgRho As Single = 0

        For i As Integer = 0 To numParticles - 1
            Dim px = particles.pos(2 * i)
            Dim py = particles.pos(2 * i + 1)
            Dim first = firstNeighbor(i)
            Dim num = firstNeighbor(i + 1) - first
            Dim rho As Single = 0
            Dim sumGrad2 As Single = 0
            Dim gradix As Single = 0
            Dim gradiy As Single = 0

            For j As Integer = 0 To num - 1
                Dim id = neighbors.vals(first + j)
                Dim nx = particles.pos(2 * id) - px
                Dim ny = particles.pos(2 * id + 1) - py
                Dim r = Sqrt(nx ^ 2 + ny ^ 2)

                If r > 0 Then
                    nx /= r
                    ny /= r
                End If

                If sand Then
                    If r < 2 * particleRadius Then
                        Dim d = 0.5 * (2 * particleRadius - r)
                        particles.pos(2 * i) -= nx * d
                        particles.pos(2 * i + 1) -= ny * d
                        particles.pos(2 * id) += nx * d
                        particles.pos(2 * id + 1) += ny * d
                    End If
                    Continue For
                End If

                If r > h Then
                    grads(2 * j) = 0
                    grads(2 * j + 1) = 0
                Else
                    Dim r2 = r ^ 2
                    Dim w = h2 - r2
                    rho += kernelScale * w ^ 3
                    Dim grad = (kernelScale * 3 * w ^ 2 * (-2 * r)) / restDensity
                    grads(2 * j) = nx * grad
                    grads(2 * j + 1) = ny * grad
                    gradix -= nx * grad
                    gradiy -= ny * grad
                    sumGrad2 += grad ^ 2
                End If
            Next

            sumGrad2 += gradix ^ 2 + gradiy ^ 2
            avgRho += rho

            Dim C = rho / restDensity - 1

            If unilateral AndAlso C < 0 Then
                Continue For
            End If

            Dim lambda = -C / (sumGrad2 + 0.0001)

            For j As Integer = 0 To num - 1
                Dim id = neighbors.vals(first + j)

                If id = i Then
                    particles.pos(2 * id) += lambda * gradix
                    particles.pos(2 * id + 1) += lambda * gradiy
                Else
                    particles.pos(2 * id) += lambda * grads(2 * j)
                    particles.pos(2 * id + 1) += lambda * grads(2 * j + 1)
                End If
            Next
        Next
    End Sub

    Public Sub applyViscosity(pnr As Integer)
        Dim first = firstNeighbor(i)
        Dim num = firstNeighbor(i + 1) - first

        If num = 0 Then
            Return
        End If

        Dim avgVelX As Single = 0
        Dim avgVelY As Single = 0

        For j As Integer = 0 To num - 1
            Dim id = neighbors.vals(first + j)
            avgVelX += particles.vel(2 * id)
            avgVelY += particles.vel(2 * id + 1)
        Next

        avgVelX /= num
        avgVelY /= num

        Dim deltaX = avgVelX - particles.vel(2 * pnr)
        Dim deltaY = avgVelY - particles.vel(2 * pnr + 1)

        particles.vel(2 * pnr) += viscosity * deltaX
        particles.vel(2 * pnr + 1) += viscosity * deltaY
    End Sub

    Public Sub simulate()
        findNeighbors()

        Dim dt = timeStep / numSubSteps

        For [step] As Integer = 0 To numSubSteps - 1
            ' predict
            For i As Integer = 0 To numParticles - 1
                particles.vel(2 * i + 1) += gravity * dt
                particles.prev(2 * i) = particles.pos(2 * i)
                particles.prev(2 * i + 1) = particles.pos(2 * i + 1)
                particles.pos(2 * i) += particles.vel(2 * i) * dt
                particles.pos(2 * i + 1) += particles.vel(2 * i + 1) * dt
            Next

            ' solve
            solveBoundaries()
            solveFluid()

            ' derive velocities

            For i As Integer = 0 To numParticles - 1
                Dim vx = particles.pos(2 * i) - particles.prev(2 * i)
                Dim vy = particles.pos(2 * i + 1) - particles.prev(2 * i + 1)

                ' CFL
                Dim v = Sqrt(vx ^ 2 + vy ^ 2)

                If v > maxVel Then
                    vx *= maxVel / v
                    vy *= maxVel / v
                    particles.pos(2 * i) = particles.prev(2 * i) + vx
                    particles.pos(2 * i + 1) = particles.prev(2 * i + 1) + vy
                End If

                particles.vel(2 * i) = vx / dt
                particles.vel(2 * i + 1) = vy / dt

                applyViscosity(i)
            Next
        Next
    End Sub

    Sub Setup(initNumX As Integer, initNumY As Integer)
        If initNumX * initNumY > maxParticles Then
            Return
        End If

        numX = initNumX
        numY = initNumY
        numParticles = numX * numY

        Dim nr As Integer = 0

        For j As Integer = 0 To numY - 1
            For i As Integer = 0 To numX - 1
                particles.pos(nr) = fluidOrig.Left + i * particleDiameter
                particles.pos(nr) += 0.00001 * (j Mod 2)
                particles.pos(nr + 1) = fluidOrig.Bottom + j * particleDiameter
                particles.vel(nr) = 0
                particles.vel(nr + 1) = 0
                nr += 2
            Next
        Next

        For i As Integer = 0 To hashSize - 1
            hash.first(i) = -1
            hash.marks(i) = 0
        Next
    End Sub

    Private Sub solveBoundaries()
        Dim minX = Canvas.Width * 0.5 / drawScale

        For i As Integer = 0 To numParticles - 1
            Dim px = particles.pos(2 * i)
            Dim py = particles.pos(2 * i + 1)

            If py < 0 Then
                ' ground
                particles.pos(2 * i + 1) = 0
            End If
            If px < -minX Then
                particles.pos(2 * i) = -minX
            End If
            If px > minX Then
                particles.pos(2 * i) = minX
            End If

            For j As Integer = 0 To boundaries.Length - 1
                Dim b = boundaries(j)

                If px < b.Left OrElse px > b.Right OrElse py < b.Bottom OrElse py > b.Top Then
                    Continue For
                End If

                Dim dx, dy As Single

                If px < (b.Left + b.Right) / 2 Then
                    dx = b.Left - px
                Else
                    dx = b.Right - px
                End If
                If py < (b.Bottom + b.Top) / 2 Then
                    dy = b.Bottom - py
                Else
                    dy = b.Top - py
                End If

                If Abs(dx) < Abs(dy) Then
                    particles.pos(2 * i) += dx
                Else
                    particles.pos(2 * i + 1) += dy
                End If
            Next
        Next
    End Sub

    Sub draw()
        Dim bmp As New Bitmap(Canvas.Width, Canvas.Height)
        Dim gfx As IGraphics = Graphics2D.Open(bmp)
        Dim nr As Integer = 0

        For i As Integer = 0 To numParticles - 1
            Dim px = drawOrig.x + particles.pos(nr) * drawScale,
         py = drawOrig.y - particles.pos(nr + 1) * drawScale

            nr += 2
            gfx.DrawCircle(New PointF(px, py), 5, Brushes.Black)
        Next

        Canvas.Image = bmp
    End Sub

    Dim timeFrames As Single = 0
    Dim timeSum As Double

    Sub [step]()
        Dim startTime = Now
        simulate()
        Dim endTime = Now

        timeSum += (endTime - startTime).TotalMilliseconds
        timeFrames += 1

        If timeFrames > 10 Then
            timeSum /= timeFrames
            Text = timeSum.ToString("F3") & "ms" & $"   [{Canvas.Width} x {Canvas.Height}]"
            timeFrames = 0
            timeSum = Nothing
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        [step]()
        draw()
    End Sub

    Private Sub Form4_Load(sender As Object, e As EventArgs) Handles Me.Load
        Setup(10, 100)
    End Sub

    Private Sub Form4_SizeChanged(sender As Object, e As EventArgs) Handles Me.SizeChanged
        drawOrig = New Vector2(Canvas.Width / 2, Canvas.Height - 20)
    End Sub
End Class

Public Class Hash

    Public size As Integer
    Public first As Integer()
    Public marks As Integer()
    Public currentMark As Integer
    Public [next] As Integer()
    Public orig As Padding

    Sub New(hashSize As Integer, maxParticles As Integer)
        size = hashSize
        first = New Integer(hashSize - 1) {}
        marks = New Integer(hashSize - 1) {}
        currentMark = 0
        [next] = New Integer(maxParticles - 1) {}
        orig = New Padding With {.Left = -100, .Bottom = -1}
    End Sub

End Class


Public Class Particles

    Public pos As Single()
    Public prev As Single()
    Public vel As Single()

    Sub New(maxParticles As Integer)
        pos = New Single(2 * maxParticles - 1) {}
        prev = New Single(2 * maxParticles - 1) {}
        vel = New Single(2 * maxParticles - 1) {}
    End Sub

End Class
