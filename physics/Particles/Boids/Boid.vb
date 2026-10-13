#Region "Microsoft.VisualBasic::ebb2f4df116ef3b750d69c7c43e39a00, physics\Particles\Boids\Boid.vb"

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

    '   Total Lines: 122
    '    Code Lines: 81 (66.39%)
    ' Comment Lines: 23 (18.85%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 18 (14.75%)
    '     File Size: 3.72 KB


    '     Structure Boid
    ' 
    '         Properties: x, Xvel, y, Yvel
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetAngle, GetPosition, GetSpeed
    ' 
    '         Sub: Accelerate, MoveForward
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

Namespace Boids

    ''' <summary>
    ''' Boids flocking algorithm
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/swharden/Csharp-Data-Visualization/tree/main/dev/old/drawing/boids
    ''' </remarks>
    Public Structure Boid : Implements Layout2D

        Friend ReadOnly state As FlockState
        Friend ReadOnly index As Integer

        Friend Sub New(state As FlockState, index As Integer)
            Me.state = state
            Me.index = index
        End Sub

        ''' <summary>
        ''' position x
        ''' </summary>
        ''' <returns></returns>
        Public Property x As Double Implements Layout2D.X
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
        ''' <returns></returns>
        Public Property y As Double Implements Layout2D.Y
            Get
                Return state.py(index)
            End Get
            Set(value As Double)
                state.py(index) = value
            End Set
        End Property

        Public Property Xvel As Double
            Get
                Return state.vx(index)
            End Get
            Set(value As Double)
                state.vx(index) = value
            End Set
        End Property

        Public Property Yvel As Double
            Get
                Return state.vy(index)
            End Get
            Set(value As Double)
                state.vy(index) = value
            End Set
        End Property

        ''' <summary>
        ''' Moves the boid forward in time and clamps its speed into the 
        ''' ``[minSpeed, maxSpeed]`` range. This is a view operation: it mutates
        ''' the underlying simulation state slot.
        ''' </summary>
        Public Sub MoveForward(Optional minSpeed As Double = 1, Optional maxSpeed As Double = 5)
            Dim s = state
            Dim i = index
            Dim vxv = s.vx(i)
            Dim vyv = s.vy(i)

            s.px(i) += vxv
            s.py(i) += vyv

            Dim speed = std.Sqrt(vxv * vxv + vyv * vyv)

            If speed > maxSpeed Then
                vxv = vxv / speed * maxSpeed
                vyv = vyv / speed * maxSpeed
            ElseIf speed < minSpeed Then
                vxv = vxv / speed * minSpeed
                vyv = vyv / speed * minSpeed
            End If

            If Double.IsNaN(vxv) Then vxv = 0
            If Double.IsNaN(vyv) Then vyv = 0

            s.vx(i) = vxv
            s.vy(i) = vyv
        End Sub

        Public Function GetPosition(time As Double) As (Double, Double)
            Return (x + Xvel * time, y + Yvel * time)
        End Function

        Public Sub Accelerate(Optional scale As Double = 1.0)
            state.vx(index) *= scale
            state.vy(index) *= scale
        End Sub

        Public Function GetAngle() As Double
            If Double.IsNaN(Xvel) OrElse Double.IsNaN(Yvel) Then Return 0
            If Xvel = 0 AndAlso Yvel = 0 Then Return 0
            Dim angle = std.Atan(Yvel / Xvel) * 180 / std.PI - 90
            If Xvel < 0 Then angle += 180
            Return angle
        End Function

        ''' <summary>
        ''' Velocity
        ''' </summary>
        ''' <returns></returns>
        Public Function GetSpeed() As Double
            Return std.Sqrt(Xvel * Xvel + Yvel * Yvel)
        End Function
    End Structure
End Namespace
