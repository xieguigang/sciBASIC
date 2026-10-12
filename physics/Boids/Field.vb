#Region "Microsoft.VisualBasic::c8491a05bc057d4554ac9419b297fdc4, gr\physics\Boids\Field.vb"

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

    ' The <see cref="Field"/> is the public facade of the flocking simulation. 
    ' Internally it orchestrates three phases per frame:

    '   1. <see cref="FlockGrid.Build"/>  - counting-sort spatial grid, O(N), zero allocation
    '   2. <see cref="FlockSolver"/>     - the four flocking rules, parallel over boid ranges
    '   3. <see cref="FlockIntegrate"/>  - SIMD streaming integration + wall rules

    ' The public surface is source compatible with the legacy implementation 
    ' except that <see cref="Boid"/> is now a value type view instead of a 
    ' heap object (see the notes on <see cref="Boid"/>).

    ' Code Statistics:

    '   Total Lines: 246
    '    Code Lines: 148 (60.16%)
    ' Comment Lines: 32 (13.01%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 66 (26.83%)
    '     File Size: 9.14 KB


    '     Class Field
    ' 
    '         Properties: Entity, Grid, Height, MaxSpeed, PredatorCount, State, Width
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Sub: Advance
    '         Class BoidCollection
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: GetEnumerator
    '             Class BoidEnumerator
    ' 
    '                 Constructor: (+1 Overloads) Sub New
    '                 Sub: Dispose
    '                 Function: MoveNext, get_Current
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Parallel
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Boids

    ''' <summary>
    ''' Boids flocking algorithm
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/swharden/Csharp-Data-Visualization/tree/main/dev/old/drawing/boids
    ''' </remarks>
    Public Class Field : Implements IContainer(Of Boid)

        Public ReadOnly Property Width As Double Implements IContainer(Of Boid).Width
        Public ReadOnly Property Height As Double Implements IContainer(Of Boid).Height

        ''' <summary>
        ''' a read only collection view of all boids inside this field; enumerating
        ''' it yields zero allocation <see cref="Boid"/> value views.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Entity As IReadOnlyCollection(Of Boid) Implements IContainer(Of Boid).Entity
            Get
                Return _entity
            End Get
        End Property

        Public ReadOnly Property MaxSpeed As Double = 6

        Public Property PredatorCount As Integer = 6

        Default Public ReadOnly Property Item(i As Integer) As Boid
            Get
                Return New Boid(_state, i)
            End Get
        End Property

        ''' <summary>
        ''' the SoA simulation state; exposes the px/py/vx/vy component arrays for
        ''' consumers that want to stream over the boids without going through
        ''' the <see cref="Item"/> views (e.g. the SIMD renderer).
        ''' </summary>
        Public ReadOnly Property State As FlockState
            Get
                Return _state
            End Get
        End Property

        ''' <summary>
        ''' the spatial grid of the most recent <see cref="Advance"/> call
        ''' </summary>
        Friend ReadOnly Property Grid As FlockGrid
            Get
                Return _grid
            End Get
        End Property

        Friend ReadOnly _state As FlockState
        Friend ReadOnly _grid As FlockGrid
        Friend ReadOnly _task As FlockSolver
        Friend ReadOnly _entity As BoidCollection

        ''' <summary>
        ''' the spatial grid cell size, in simulation units (pixels)
        ''' </summary>
        Friend ReadOnly Radius As Single = 15

        Public Sub New(width As Double, height As Double, Optional boidCount As Integer = 100)
            If boidCount < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(boidCount))
            End If

            Me.Width = width
            Me.Height = height

            _state = New FlockState(boidCount)
            _grid = New FlockGrid(boidCount)
            _entity = New BoidCollection(_state)
            _task = New FlockSolver(Me)

            ' keep the legacy initialization sequence so that a given random 
            ' seed still produces the same initial flock layout
            Dim rand As Random = randf.seeds

            For i As Integer = 0 To boidCount - 1
                _state.px(i) = rand.NextDouble() * width
                _state.py(i) = rand.NextDouble() * height
                _state.vx(i) = rand.NextDouble() - 0.5
                _state.vy(i) = rand.NextDouble() - 0.5
            Next

            VectorTask.n_threads = App.CPUCoreNumbers
        End Sub

        ''' <summary>
        ''' Advances the simulation by one time step: rebuild the spatial grid, 
        ''' evaluate the flocking rules in parallel, then run the SIMD integration 
        ''' kernel with the wall rules.
        ''' </summary>
        Public Sub Advance(Optional bounceOffWalls As Boolean = True, Optional wrapAroundEdges As Boolean = False)
            _grid.Build(_state, Radius)
            _task.Run()
            FlockIntegrate.Integrate(_state, Width, Height, bounceOffWalls, wrapAroundEdges,
                                     minSpeed:=1, maxSpeed:=MaxSpeed)
        End Sub
    End Class

    ''' <summary>
    ''' a read only, allocation free collection view over the boid slots
    ''' </summary>
    Friend NotInheritable Class BoidCollection
        Implements IReadOnlyCollection(Of Boid)

        ReadOnly _state As FlockState

        Public Sub New(state As FlockState)
            _state = state
        End Sub

        Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of Boid).Count
            Get
                Return _state.count
            End Get
        End Property

        Public Function GetEnumerator() As IEnumerator(Of Boid) Implements IEnumerable(Of Boid).GetEnumerator
            Return New BoidEnumerator(_state)
        End Function

        Private Function GetEnumeratorObject() As IEnumerator Implements IEnumerable.GetEnumerator
            Return Me.GetEnumerator()
        End Function
    End Class

    Friend NotInheritable Class BoidEnumerator
        Implements IEnumerator(Of Boid)

        ReadOnly _state As FlockState
        Dim _index As Integer = -1

        Public Sub New(state As FlockState)
            _state = state
        End Sub

        Public ReadOnly Property Current As Boid Implements IEnumerator(Of Boid).Current
            Get
                Return New Boid(_state, _index)
            End Get
        End Property

        Private ReadOnly Property CurrentObject As Object Implements IEnumerator.Current
            Get
                Return Me.Current
            End Get
        End Property

        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            _index += 1
            Return _index < _state.count
        End Function

        Public Sub Reset() Implements IEnumerator.Reset
            _index = -1
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
        End Sub
    End Class
End Namespace
