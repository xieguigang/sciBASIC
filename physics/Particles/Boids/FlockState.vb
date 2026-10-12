#Region "Microsoft.VisualBasic::3a1f0c9d81b24e579a6d3f5c1b8e0427, gr\physics\Boids\FlockState.vb"

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

    ' Structure-of-arrays simulation state for the flocking engine. All four 
    ' component arrays are allocated once and reused across frames, so a 
    ' simulation step performs zero heap allocation for the state itself.

    '     Class FlockState
    ' 
    '         Properties: count, px, py, vx, vy
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Clone
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Boids

    ''' <summary>
    ''' The SoA (structure of arrays) state of the flocking simulation: positions 
    ''' and velocities are kept in four contiguous <see cref="Double"/> arrays so 
    ''' that the vectorized kernels can stream over them sequentially.
    ''' </summary>
    Public NotInheritable Class FlockState

        ''' <summary>
        ''' position x, indexed by boid id
        ''' </summary>
        Public ReadOnly Property px As Double()
        ''' <summary>
        ''' position y, indexed by boid id
        ''' </summary>
        Public ReadOnly Property py As Double()
        ''' <summary>
        ''' velocity x, indexed by boid id
        ''' </summary>
        Public ReadOnly Property vx As Double()
        ''' <summary>
        ''' velocity y, indexed by boid id
        ''' </summary>
        Public ReadOnly Property vy As Double()
        ''' <summary>
        ''' velocity delta produced by the flocking rules for the current frame;
        ''' it is applied by the integration kernel and acts as the two-phase
        ''' buffer that keeps the parallel solve race free (every boid reads the
        ''' frame-start velocities of its neighbors).
        ''' </summary>
        Public ReadOnly Property dvx As Double()
        ''' <summary>
        ''' velocity delta produced by the flocking rules for the current frame
        ''' </summary>
        Public ReadOnly Property dvy As Double()

        ''' <summary>
        ''' number of boids inside the current simulation state
        ''' </summary>
        Public ReadOnly Property count As Integer

        Public Sub New(count As Integer)
            If count < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(count))
            End If

            Me.count = count

            If count = 0 Then
                Me.px = Array.Empty(Of Double)()
                Me.py = Array.Empty(Of Double)()
                Me.vx = Array.Empty(Of Double)()
                Me.vy = Array.Empty(Of Double)()
                Me.dvx = Array.Empty(Of Double)()
                Me.dvy = Array.Empty(Of Double)()
            Else
                Me.px = New Double(count - 1) {}
                Me.py = New Double(count - 1) {}
                Me.vx = New Double(count - 1) {}
                Me.vy = New Double(count - 1) {}
                Me.dvx = New Double(count - 1) {}
                Me.dvy = New Double(count - 1) {}
            End If
        End Sub

        ''' <summary>
        ''' Makes a deep copy of the current simulation state, this function is 
        ''' used by the unit test for compare the simulation result between 
        ''' the SIMD engine and the legacy reference implementation.
        ''' </summary>
        ''' <returns></returns>
        Public Function Clone() As FlockState
            Dim copy As New FlockState(count)

            If count > 0 Then
                Array.Copy(px, copy.px, count)
                Array.Copy(py, copy.py, count)
                Array.Copy(vx, copy.vx, count)
                Array.Copy(vy, copy.vy, count)
                Array.Copy(dvx, copy.dvx, count)
                Array.Copy(dvy, copy.dvy, count)
            End If

            Return copy
        End Function
    End Class
End Namespace
