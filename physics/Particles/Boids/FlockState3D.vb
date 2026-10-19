Namespace Boids

    ''' <summary>
    ''' The SoA (structure of arrays) state of the three dimensional flocking
    ''' simulation: positions and velocities are kept in nine contiguous
    ''' <see cref="Double"/> arrays so that the vectorized kernels can stream over
    ''' them sequentially.
    ''' </summary>
    ''' <remarks>
    ''' This is the three dimensional counterpart of <see cref="FlockState"/>: the
    ''' two classes are intentionally isomorphic, the only difference is the extra
    ''' Z axis. Keeping both of them in the very same shape is what makes the
    ''' solvers and the integration kernels readable side by side.
    ''' </remarks>
    Public NotInheritable Class FlockState3D

        ''' <summary>
        ''' position x, indexed by boid id
        ''' </summary>
        Public ReadOnly Property px As Double()
        ''' <summary>
        ''' position y, indexed by boid id
        ''' </summary>
        Public ReadOnly Property py As Double()
        ''' <summary>
        ''' position z, indexed by boid id
        ''' </summary>
        Public ReadOnly Property pz As Double()
        ''' <summary>
        ''' velocity x, indexed by boid id
        ''' </summary>
        Public ReadOnly Property vx As Double()
        ''' <summary>
        ''' velocity y, indexed by boid id
        ''' </summary>
        Public ReadOnly Property vy As Double()
        ''' <summary>
        ''' velocity z, indexed by boid id
        ''' </summary>
        Public ReadOnly Property vz As Double()
        ''' <summary>
        ''' velocity delta along x produced by the flocking rules for the current
        ''' frame; it is applied by the integration kernel and acts as the
        ''' two-phase buffer that keeps the parallel solve race free (every boid
        ''' reads the frame-start positions and velocities of its neighbors)
        ''' </summary>
        Public ReadOnly Property dvx As Double()
        ''' <summary>
        ''' velocity delta along y produced by the flocking rules for the current
        ''' frame
        ''' </summary>
        Public ReadOnly Property dvy As Double()
        ''' <summary>
        ''' velocity delta along z produced by the flocking rules for the current
        ''' frame
        ''' </summary>
        Public ReadOnly Property dvz As Double()

        ''' <summary>
        ''' number of boids inside the current simulation state
        ''' </summary>
        Public ReadOnly Property count As Integer

        ''' <summary>
        ''' create a new simulation state that holds the given number of boids
        ''' </summary>
        ''' <param name="count">the number of the boid slots to allocate</param>
        Public Sub New(count As Integer)
            If count < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(count))
            End If

            Me.count = count

            If count = 0 Then
                Me.px = Array.Empty(Of Double)()
                Me.py = Array.Empty(Of Double)()
                Me.pz = Array.Empty(Of Double)()
                Me.vx = Array.Empty(Of Double)()
                Me.vy = Array.Empty(Of Double)()
                Me.vz = Array.Empty(Of Double)()
                Me.dvx = Array.Empty(Of Double)()
                Me.dvy = Array.Empty(Of Double)()
                Me.dvz = Array.Empty(Of Double)()
            Else
                Me.px = New Double(count - 1) {}
                Me.py = New Double(count - 1) {}
                Me.pz = New Double(count - 1) {}
                Me.vx = New Double(count - 1) {}
                Me.vy = New Double(count - 1) {}
                Me.vz = New Double(count - 1) {}
                Me.dvx = New Double(count - 1) {}
                Me.dvy = New Double(count - 1) {}
                Me.dvz = New Double(count - 1) {}
            End If
        End Sub

        ''' <summary>
        ''' Makes a deep copy of the current simulation state, this function is
        ''' used by the unit test for comparing the simulation result between the
        ''' vectorized engine and the reference implementation.
        ''' </summary>
        ''' <returns>an independent copy of this state</returns>
        Public Function Clone() As FlockState3D
            Dim copy As New FlockState3D(count)

            If count > 0 Then
                Array.Copy(px, copy.px, count)
                Array.Copy(py, copy.py, count)
                Array.Copy(pz, copy.pz, count)
                Array.Copy(vx, copy.vx, count)
                Array.Copy(vy, copy.vy, count)
                Array.Copy(vz, copy.vz, count)
                Array.Copy(dvx, copy.dvx, count)
                Array.Copy(dvy, copy.dvy, count)
                Array.Copy(dvz, copy.dvz, count)
            End If

            Return copy
        End Function
    End Class
End Namespace
