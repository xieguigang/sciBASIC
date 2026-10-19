Imports Microsoft.VisualBasic.Parallel
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Boids

    ''' <summary>
    ''' The three dimensional boids flocking algorithm: a cubic field of boids
    ''' that keep separation, alignment, cohesion and predator avoidance.
    ''' </summary>
    ''' <remarks>
    ''' This class is the three dimensional counterpart of <see cref="Field"/> and
    ''' is used by the directx scene renderer of the boids demo. The two
    ''' dimensional field is not touched by it: both simulations can run in the
    ''' very same process and they only share the numeric helpers of
    ''' <see cref="BoidMath"/>.
    '''
    ''' The simulated volume is the axis aligned box
    ''' <c>[0, Width) x [0, Height) x [0, Depth)</c>; the wall rule steers the
    ''' boids away from the six faces of that box (or wraps them, when the caller
    ''' asks for it).
    ''' </remarks>
    Public Class Field3D

        ''' <summary>
        ''' the size of the simulated box along the x axis
        ''' </summary>
        Public ReadOnly Property Width As Double
        ''' <summary>
        ''' the size of the simulated box along the y axis
        ''' </summary>
        Public ReadOnly Property Height As Double
        ''' <summary>
        ''' the size of the simulated box along the z axis
        ''' </summary>
        Public ReadOnly Property Depth As Double

        ''' <summary>
        ''' the highest speed that a boid may reach, in simulation units per step
        ''' </summary>
        Public ReadOnly Property MaxSpeed As Double = 6

        ''' <summary>
        ''' the number of the leading boids that act as predators
        ''' </summary>
        Public Property PredatorCount As Integer = 6

        ''' <summary>
        ''' the number of the boids inside this field
        ''' </summary>
        Public ReadOnly Property BoidCount As Integer
            Get
                Return _state.count
            End Get
        End Property

        ''' <summary>
        ''' the SoA simulation state; exposes the px/py/pz/vx/vy/vz component
        ''' arrays for consumers that want to stream over the boids without going
        ''' through the <see cref="Item"/> views (e.g. the directx instance
        ''' packer).
        ''' </summary>
        Public ReadOnly Property State As FlockState3D
            Get
                Return _state
            End Get
        End Property

        ''' <summary>
        ''' the spatial grid of the most recent <see cref="Advance"/> call
        ''' </summary>
        Friend ReadOnly Property Grid As FlockGrid3D
            Get
                Return _grid
            End Get
        End Property

        Default Public ReadOnly Property Item(i As Integer) As Boid3D
            Get
                Return New Boid3D(_state, i)
            End Get
        End Property

        Friend ReadOnly _state As FlockState3D
        Friend ReadOnly _grid As FlockGrid3D
        Friend ReadOnly _task As FlockSolver3D

        ''' <summary>
        ''' the spatial grid cell size, in simulation units
        ''' </summary>
        Friend ReadOnly Radius As Single = 15

        ''' <summary>
        ''' create a new cubic field of the given size and populate it with
        ''' uniformly distributed boids
        ''' </summary>
        ''' <param name="width">the size of the box along the x axis</param>
        ''' <param name="height">the size of the box along the y axis</param>
        ''' <param name="depth">the size of the box along the z axis</param>
        ''' <param name="boidCount">the number of the boids of the flock</param>
        Public Sub New(width As Double, height As Double, depth As Double, Optional boidCount As Integer = 100)
            If boidCount < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(boidCount))
            End If

            If width <= 0 OrElse height <= 0 OrElse depth <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(width), "the size of the field must be positive on every axis")
            End If

            Me.Width = width
            Me.Height = height
            Me.Depth = depth

            _state = New FlockState3D(boidCount)
            _grid = New FlockGrid3D(boidCount)
            _task = New FlockSolver3D(Me)

            ' the same initialization sequence that the two dimensional field
            ' uses: a given random seed still produces the same initial layout
            Dim rand As Random = randf.seeds

            For i As Integer = 0 To boidCount - 1
                _state.px(i) = rand.NextDouble() * width
                _state.py(i) = rand.NextDouble() * height
                _state.pz(i) = rand.NextDouble() * depth
                _state.vx(i) = rand.NextDouble() - 0.5
                _state.vy(i) = rand.NextDouble() - 0.5
                _state.vz(i) = rand.NextDouble() - 0.5
            Next

            VectorTask.n_threads = App.CPUCoreNumbers
        End Sub

        ''' <summary>
        ''' Advances the simulation by one time step: rebuild the spatial grid,
        ''' evaluate the flocking rules in parallel, then run the integration
        ''' kernel with the wall rules.
        ''' </summary>
        ''' <param name="bounceOffWalls">
        ''' steer the boids that approach a wall back into the box
        ''' </param>
        ''' <param name="wrapAroundEdges">
        ''' teleport the boids that leave the box to its opposite side
        ''' </param>
        Public Sub Advance(Optional bounceOffWalls As Boolean = True, Optional wrapAroundEdges As Boolean = False)
            _grid.Build(_state, Radius)
            _task.Run()
            FlockIntegrate3D.Integrate(_state, Width, Height, Depth, bounceOffWalls, wrapAroundEdges,
                                       minSpeed:=1, maxSpeed:=MaxSpeed)
        End Sub

        ''' <summary>
        ''' enumerate the boids of this field as zero allocation value views
        ''' </summary>
        Public Iterator Function Enumerate() As IEnumerable(Of Boid3D)
            For i As Integer = 0 To _state.count - 1
                Yield New Boid3D(_state, i)
            Next
        End Function
    End Class
End Namespace
