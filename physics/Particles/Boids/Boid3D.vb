Namespace Boids

    ''' <summary>
    ''' A zero allocation value view of one boid slot of a
    ''' <see cref="FlockState3D"/>.
    ''' </summary>
    ''' <remarks>
    ''' The structure does not own any data: every property reads or writes the
    ''' SoA slot of the given index, so enumerating a <see cref="Field3D"/> does
    ''' not allocate a single object per boid. It is the three dimensional
    ''' counterpart of <see cref="Boid"/>.
    ''' </remarks>
    Public Structure Boid3D

        Friend ReadOnly state As FlockState3D
        Friend ReadOnly index As Integer

        Friend Sub New(state As FlockState3D, index As Integer)
            Me.state = state
            Me.index = index
        End Sub

        ''' <summary>
        ''' position x
        ''' </summary>
        Public Property x As Double
            Get
                Return state.px(index)
            End Get
            Set(value As Double)
                state.px(index) = value
            End Set
        End Property

        ''' <summary>
        ''' position y
        ''' </summary>
        Public Property y As Double
            Get
                Return state.py(index)
            End Get
            Set(value As Double)
                state.py(index) = value
            End Set
        End Property

        ''' <summary>
        ''' position z
        ''' </summary>
        Public Property z As Double
            Get
                Return state.pz(index)
            End Get
            Set(value As Double)
                state.pz(index) = value
            End Set
        End Property

        ''' <summary>
        ''' velocity x
        ''' </summary>
        Public Property Xvel As Double
            Get
                Return state.vx(index)
            End Get
            Set(value As Double)
                state.vx(index) = value
            End Set
        End Property

        ''' <summary>
        ''' velocity y
        ''' </summary>
        Public Property Yvel As Double
            Get
                Return state.vy(index)
            End Get
            Set(value As Double)
                state.vy(index) = value
            End Set
        End Property

        ''' <summary>
        ''' velocity z
        ''' </summary>
        Public Property Zvel As Double
            Get
                Return state.vz(index)
            End Get
            Set(value As Double)
                state.vz(index) = value
            End Set
        End Property

        ''' <summary>
        ''' the speed of this boid, in simulation units per step
        ''' </summary>
        ''' <returns>the length of the velocity vector</returns>
        Public Function GetSpeed() As Double
            Dim vxv As Double = Xvel
            Dim vyv As Double = Yvel
            Dim vzv As Double = Zvel

            Return System.Math.Sqrt(vxv * vxv + vyv * vyv + vzv * vzv)
        End Function
    End Structure
End Namespace
