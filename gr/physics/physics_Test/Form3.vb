#Region "Microsoft.VisualBasic::b6130c69e9d9456081218b2c10a2c27e, gr\physics\physics_Test\Form3.vb"

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

    '   Total Lines: 287
    '    Code Lines: 51
    ' Comment Lines: 172
    '   Blank Lines: 64
    '     File Size: 10.92 KB


    ' Class Form3
    ' 
    '     Properties: deltaTime
    ' 
    '     Sub: Form3_Load, Form3_SizeChanged, Timer1_Tick
    ' 
    ' Module FluidRender
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Render
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Physics

Public Class Form3

    Public ReadOnly Property deltaTime As Single
        Get
            Return Timer1.Interval / 500
        End Get
    End Property

    Dim engine As FluidEngine

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If Not engine Is Nothing Then
            Call engine.RunSimulationStep()
        End If

        PictureBox1.Image = FluidRender.Render(PictureBox1.Size, engine)
    End Sub

    Private Sub Form3_Load(sender As Object, e As EventArgs) Handles Me.Load
        engine = New FluidEngine(5000, PictureBox1.Size)
    End Sub

    Private Sub Form3_SizeChanged(sender As Object, e As EventArgs) Handles Me.SizeChanged
        If Not engine Is Nothing Then
            engine.Resize(PictureBox1.Width, PictureBox1.Height)
        End If
    End Sub
End Class

'Public Class Engine : Implements IContainer(Of Particle)

'    Public gravity As Single = 10
'    Public particles As Particle()
'    Public particleSize As Single = 5
'    Public collisionDamping As Single = 0.95
'    Public smoothingRadius As Single = 0.35
'    Public particleProperties As Single()
'    Public densities As Single()

'    Public targetDensity As Single = 5.2
'    Public pressureMultiplier As Single = 27.44
'    Public numParticles As Integer
'    Public viscosityStrength As Single = 0.5

'    Const stepSize As Single = 0.001
'    Const mass As Double = 1

'    Friend canvas As Size
'    Friend grid As Grid(Of Particle())

'    Public ReadOnly Property Entity As IReadOnlyCollection(Of Particle) Implements IContainer(Of Particle).Entity
'        Get
'            Return particles
'        End Get
'    End Property

'    Public ReadOnly Property Width As Double Implements IContainer(Of Particle).Width
'        Get
'            Return canvas.Width
'        End Get
'    End Property
'    Public ReadOnly Property Height As Double Implements IContainer(Of Particle).Height
'        Get
'            Return canvas.Height
'        End Get
'    End Property

'    Sub New(numParticles As Integer, canvas As Size)
'        particles = New Particle(numParticles - 1) {}
'        particleProperties = New Single(numParticles - 1) {}
'        densities = New Single(numParticles - 1) {}

'        Me.numParticles = numParticles
'        Me.canvas = canvas

'        For i As Integer = 0 To numParticles - 1
'            particles(i) = New Particle With {.index = i, .position = Vector2.random(box:=canvas),
'            .velocity = Vector2.zero}
'        Next
'    End Sub

'    Public Sub Updates(deltaTime As Single)
'        Parallel.For(0, numParticles, Sub(i)
'                                          particles(i).velocity += Vector2.down * gravity * deltaTime
'                                          ' densities(i) = CalculateDensity(particles(i))
'                                          particles(i).predictedPosition = particles(i).position + particles(i).velocity / 120.0F
'                                      End Sub)

'        grid = Me.EncodeGrid(smoothingRadius)

'        Parallel.For(0, numParticles, Sub(i)
'                                          densities(i) = CalculateDensity(particles(i))
'                                      End Sub)

'        Parallel.For(0, numParticles, Sub(i)
'                                          Dim pressureForce = CalculatePressureForce(i) + CalculateViscosityForce(i)
'                                          Dim a = pressureForce / densities(i)
'                                          particles(i).velocity += a * deltaTime
'                                      End Sub)

'        Parallel.For(0, numParticles, Sub(i)
'                                          particles(i).position += particles(i).velocity * deltaTime
'                                          Call ResolveCollisions(i)
'                                      End Sub)
'    End Sub

'    Public Function CalculateViscosityForce(particleIndex As Integer) As Vector2
'        Dim f = Vector2.zero
'        Dim position = particles(particleIndex).position
'        Dim near = grid.SpatialLookup(particles(particleIndex), smoothingRadius)

'        For Each other In near
'            Dim dst = (position - other.position).magnitude
'            Dim influence = smoothingKernel(dst, smoothingRadius)
'            f += (other.velocity - particles(particleIndex).velocity) * influence
'        Next

'        Return f * viscosityStrength
'    End Function

'    Public Function InteractionForce(inputPos As Vector2, radius As Single, strength As Single, particleIndex As Integer) As Vector2
'        Dim f As Vector2 = Vector2.zero
'        Dim offset = inputPos - particles(particleIndex).position
'        Dim sqrDst = offset.magnitude

'        If sqrDst < radius Then
'            Dim dst = sqrDst
'            Dim dirToInputPoint = If(dst < Single.Epsilon, Vector2.zero, offset / dst)
'            Dim centerT = 1 - dst / radius
'            f += (dirToInputPoint * strength - particles(particleIndex).velocity) * centerT
'        End If

'        Return f
'    End Function

'    Public Function smoothingKernel(dst As Single, radius As Single) As Single
'        If dst >= radius Then
'            Return 0
'        End If

'        Dim volume = PI * radius ^ 4 / 6
'        Return (radius - dst) ^ 2 / volume
'    End Function

'    Public Function SmoothingKernelDerivative(dst As Single, radius As Single) As Single
'        If dst >= radius Then
'            Return 0
'        End If

'        Dim scale = 12 / (PI * radius ^ 4)
'        Return scale * (dst - radius)
'    End Function

'    Public Function ConvertDensityToPressure(density As Single) As Single
'        Dim densityErr = density - targetDensity
'        Dim pressure = densityErr * pressureMultiplier
'        Return pressure
'    End Function



'    Public Function CalculateDensity(samplePoint As Particle) As Single
'        Dim density As Single = 0
'        Dim near = grid.SpatialLookup(samplePoint, smoothingRadius)

'        For Each position As Particle In near
'            Dim dst = (position.predictedPosition - samplePoint.predictedPosition).magnitude
'            Dim influence = smoothingKernel(dst, smoothingRadius)

'            density += mass * influence
'        Next

'        Return density
'    End Function

'    'Public Function CalculateProperty(samplePoint As Vector2) As Single
'    '    Dim prop As Single = 0
'    '    Dim near = grid.SpatialLookup(samplePoint, smoothingRadius)

'    '    For i As Integer = 0 To positions.Length - 1
'    '        Dim dst = (positions(i) - samplePoint).magnitude
'    '        Dim influence = smoothingKernel(dst, smoothingRadius)
'    '        Dim density = CalculateDensity(positions(i))
'    '        prop += particleProperties(i) * influence * mass / density
'    '    Next

'    '    Return prop
'    'End Function

'    Public Function CalculatePressureForce(particleIndex As Integer) As Vector2
'        'Dim dx = CalculateProperty(samplePoint + Vector2.right * stepSize) - CalculateProperty(samplePoint)
'        'Dim dy = CalculateProperty(samplePoint + Vector2.up * stepSize) - CalculateProperty(samplePoint)
'        'Dim gradient As New Vector2(dx / stepSize, dy / stepSize)

'        'Return gradient
'        Dim propertyGradient As Vector2 = Vector2.zero
'        Dim near = grid.SpatialLookup(particles(particleIndex), smoothingRadius)

'        For Each otherParticle As Particle In near

'            If particleIndex = otherParticle.index Then
'                Continue For
'            End If

'            Dim offset = otherParticle.position - particles(particleIndex).position
'            Dim dst = offset.magnitude
'            Dim dir = If(dst = 0.0, GetRandomDir(), offset / dst)
'            Dim slope As Double = SmoothingKernelDerivative(dst, smoothingRadius)
'            Dim density As Double = densities(otherParticle.index)
'            Dim sharedPressure = CalculateSharedPressure(density, densities(particleIndex))

'            propertyGradient += sharedPressure * dir * slope * mass / density
'        Next

'        Return propertyGradient
'    End Function

'    Public Function CalculateSharedPressure(da As Single, db As Single) As Single
'        Dim pa = ConvertDensityToPressure(da)
'        Dim pb = ConvertDensityToPressure(db)
'        Return (pa + pb) / 2
'    End Function

'    Private Function GetRandomDir() As Vector2
'        Return New Vector2(randf.NextDouble(-3, 3), randf.NextDouble(-3, 3))
'    End Function

'    Private Sub ResolveCollisions(i As Integer)
'        Dim particleSize = Me.particleSize * 5

'        If particles(i).position.x > canvas.Width - particleSize Then
'            particles(i).velocity.x *= -1 * collisionDamping
'            particles(i).position.x = canvas.Width - particleSize
'        ElseIf particles(i).position.x < particleSize Then
'            particles(i).velocity.x *= -1 * collisionDamping
'            particles(i).position.x = particleSize
'        End If
'        If particles(i).position.y > canvas.Height - particleSize Then
'            particles(i).velocity.y *= -1 * collisionDamping
'            particles(i).position.y = canvas.Height - particleSize
'        ElseIf particles(i).position.y < particleSize Then
'            particles(i).velocity.y *= -1 * collisionDamping
'            particles(i).position.y = particleSize
'        End If
'    End Sub
'End Class

Public Module FluidRender

    Dim colors As Brush()

    Sub New()
        colors = Designer.GetColors(ScalerPalette.turbo.Description).Select(Function(c) New SolidBrush(c)).ToArray
    End Sub

    Const particleSize As Single = 5

    Public Function Render(canvas As Size, container As FluidEngine) As Bitmap
        Dim bmp As New Bitmap(canvas.Width, canvas.Height)
        Dim maxV As Double = Aggregate p In container.Entity Into Max(p.velocity.magnitude)
        Dim minV As Double = 0

        Using gfx As Graphics = Graphics.FromImage(bmp)
            Call gfx.Clear(Color.Black)

            For Each p In container.Entity
                Dim level As Integer = p.velocity.magnitude / maxV * colors.Length

                If level < 0 Then
                    level = 0
                ElseIf level >= colors.Length Then
                    level = colors.Length - 1
                End If

                Call gfx.DrawCircle(p.position, particleSize, colors(level))
            Next

            Return bmp
        End Using
    End Function

End Module
