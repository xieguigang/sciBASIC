#Region "Microsoft.VisualBasic::f223f40fb8de1cf15e013b8ca33c01d3, gr\physics\Boids\Boid.vb"

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

    '   Total Lines: 67
    '    Code Lines: 49
    ' Comment Lines: 6
    '   Blank Lines: 12
    '     File Size: 2.25 KB


    '     Class Boid
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetAngle, GetPosition, GetSpeed
    ' 
    '         Sub: Accelerate, MoveForward
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Math2D
Imports std = System.Math

Namespace Boids

    ''' <summary>
    ''' Boids flocking algorithm
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/swharden/Csharp-Data-Visualization/tree/main/dev/old/drawing/boids
    ''' </remarks>
    Public Class Boid : Inherits Vector2D
        Implements Layout2D

        Public Xvel As Double
        Public Yvel As Double

        Public Sub New(x As Double, y As Double, xVel As Double, yVel As Double)
            Call (x, y, xVel, yVel).Set(Me.x, Me.y, Me.Xvel, Me.Yvel)
        End Sub

        Public Sub New(rand As Random, width As Double, height As Double)
            x = rand.NextDouble() * width
            y = rand.NextDouble() * height
            Xvel = (rand.NextDouble() - 0.5)
            Yvel = (rand.NextDouble() - 0.5)
        End Sub

        Public Sub MoveForward(Optional minSpeed As Double = 1, Optional maxSpeed As Double = 5)
            x += Xvel
            y += Yvel

            Dim speed = GetSpeed()
            If speed > maxSpeed Then
                Xvel = Xvel / speed * maxSpeed
                Yvel = Yvel / speed * maxSpeed
            ElseIf speed < minSpeed Then
                Xvel = Xvel / speed * minSpeed
                Yvel = Yvel / speed * minSpeed
            End If

            If Double.IsNaN(Xvel) Then Xvel = 0
            If Double.IsNaN(Yvel) Then Yvel = 0
        End Sub

        Public Function GetPosition(time As Double) As (Double, Double)
            Return (x + Xvel * time, y + Yvel * time)
        End Function

        Public Sub Accelerate(Optional scale As Double = 1.0)
            Xvel *= scale
            Yvel *= scale
        End Sub

        Public Function GetAngle() As Double
            If Double.IsNaN(Xvel) OrElse Double.IsNaN(Yvel) Then Return 0
            If Xvel = 0 AndAlso Yvel = 0 Then Return 0
            Dim angle = std.Atan(Yvel / Xvel) * 180 / std.PI - 90
            If Xvel < 0 Then angle += 180
            Return angle
        End Function

        Public Function GetSpeed() As Double
            Return std.Sqrt(Xvel * Xvel + Yvel * Yvel)
        End Function
    End Class
End Namespace
