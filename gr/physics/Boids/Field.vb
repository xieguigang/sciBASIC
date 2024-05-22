#Region "Microsoft.VisualBasic::96792582374f5a7ffb1fb43023248a94, gr\physics\Boids\Field.vb"

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

    '   Total Lines: 162
    '    Code Lines: 119 (73.46%)
    ' Comment Lines: 14 (8.64%)
    '    - Xml Docs: 42.86%
    ' 
    '   Blank Lines: 29 (17.90%)
    '     File Size: 7.19 KB


    '     Class Field
    ' 
    '         Properties: Entity, Height, MaxSpeed, Width
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Align, Avoid, Flock, Predator
    ' 
    '         Sub: Advance, BounceOffWalls, WrapAround
    '         Class PhysicTask
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: Solve
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory.GridGraph
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
        Public ReadOnly Property Entity As IReadOnlyCollection(Of Boid) Implements IContainer(Of Boid).Entity
            Get
                Return Boids
            End Get
        End Property

        Public ReadOnly Property MaxSpeed As Double = 6

        Default Public ReadOnly Property Item(i As Integer) As Boid
            Get
                Return Boids(i)
            End Get
        End Property

        Dim Boids As New List(Of Boid)
        Dim grid As Grid(Of Boid())
        Dim radius As Single = 15

        Public Sub New(width As Double, height As Double, Optional boidCount As Integer = 100)
            Call (width, height).Set(Me.Width, Me.Height)

            For i = 0 To boidCount - 1
                Boids.Add(New Boid(randf.seeds, width, height))
            Next

            VectorTask.n_threads = App.CPUCoreNumbers
            task = New PhysicTask(Me)
        End Sub

        Dim task As PhysicTask

        Public Sub Advance(Optional bounceOffWalls As Boolean = True, Optional wrapAroundEdges As Boolean = False)
            grid = Me.EncodeGrid(radius)
            task.Run()

            ' move all boids forward in time
            ' just update the fields of each boid
            For Each boid In Boids
                boid.MoveForward(maxSpeed:=MaxSpeed)
                If bounceOffWalls Then Me.BounceOffWalls(boid)
                If wrapAroundEdges Then WrapAround(boid)
            Next
        End Sub

        Private Class PhysicTask : Inherits VectorTask

            ReadOnly field As Field
            ReadOnly boids As List(Of Boid)

            Sub New(field As Field)
                Call MyBase.New(field.Boids.Count)
                Me.field = field
                Me.boids = field.Boids
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                Dim flockXvel As Double = Nothing, flockYvel As Double = Nothing,
               alignXvel As Double = Nothing, alignYvel As Double = Nothing,
               avoidXvel As Double = Nothing, avoidYvel As Double = Nothing,
               predXvel As Double = Nothing, predYval As Double = Nothing

                Const distance As Double = 50

                ' update void speed and direction (velocity) based on rules
                For i As Integer = start To ends
                    Dim boid As Boid = boids(i)
                    Dim neighbors As Boid() = field.grid.SpatialLookup(boid, field.radius).Where(Function(x) x.GetDistance(boid) < distance).ToArray

                    field.Flock(boid, neighbors, 0.0003).Set(flockXvel, flockYvel)
                    field.Align(boid, neighbors, 0.01).Set(alignXvel, alignYvel)
                    field.Avoid(boid, 20, 0.001).Set(avoidXvel, avoidYvel)
                    field.Predator(boid, 150, 0.00005).Set(predXvel, predYval)

                    boid.Xvel += flockXvel + avoidXvel + alignXvel + predXvel
                    boid.Yvel += flockYvel + avoidYvel + alignYvel + predYval
                Next
            End Sub
        End Class

        Private Function Flock(boid As Boid, neighbors As Boid(), power As Double) As (Double, Double)
            ' point toward the center of the flock (mean flock boid position)
            Dim meanX As Double = neighbors.Sum(Function(x) x.x) / neighbors.Count()
            Dim meanY As Double = neighbors.Sum(Function(x) x.y) / neighbors.Count()
            Dim deltaCenterX = meanX - boid.x
            Dim deltaCenterY = meanY - boid.y
            Return (deltaCenterX * power, deltaCenterY * power)
        End Function

        Private Function Avoid(boid As Boid, distance As Double, power As Double) As (Double, Double)
            ' point away as boids get close
            Dim neighbors = grid.SpatialLookup(boid, radius).Where(Function(x) x.GetDistance(boid) < distance)
            Dim sumClosenessX As Double = Nothing, sumClosenessY As Double = Nothing

            For Each neighbor As Boid In neighbors
                Dim closeness = distance - boid.GetDistance(neighbor)
                sumClosenessX += (boid.x - neighbor.x) * closeness
                sumClosenessY += (boid.y - neighbor.y) * closeness
            Next
            Return (sumClosenessX * power, sumClosenessY * power)
        End Function

        Public PredatorCount As Integer = 6

        Private Function Predator(boid As Boid, distance As Double, power As Double) As (Double, Double)
            ' point away as predators get close
            Dim sumClosenessX As Double = Nothing, sumClosenessY As Double = Nothing

            For i As Integer = 0 To PredatorCount - 1
                Dim lPredator = Boids(i)
                Dim distanceAway = boid.GetDistance(lPredator)
                If distanceAway < distance Then
                    Dim closeness = distance - distanceAway
                    sumClosenessX += (boid.x - lPredator.x) * closeness
                    sumClosenessY += (boid.y - lPredator.y) * closeness
                End If
            Next
            Return (sumClosenessX * power, sumClosenessY * power)
        End Function

        Private Function Align(boid As Boid, neighbors As Boid(), power As Double) As (Double, Double)
            ' point toward the center of the flock (mean flock boid position)
            ' Dim neighbors = grid.SpatialLookup(boid, radius).Where(Function(x) x.GetDistance(boid) < distance).ToArray
            Dim meanXvel As Double = neighbors.Sum(Function(x) x.Xvel) / neighbors.Count()
            Dim meanYvel As Double = neighbors.Sum(Function(x) x.Yvel) / neighbors.Count()
            Dim dXvel = meanXvel - boid.Xvel
            Dim dYvel = meanYvel - boid.Yvel
            Return (dXvel * power, dYvel * power)
        End Function

        Private Sub BounceOffWalls(boid As Boid)
            Dim pad As Double = 50
            Dim turn = 0.5
            If boid.x < pad Then boid.Xvel += turn
            If boid.x > Width - pad Then boid.Xvel -= turn
            If boid.y < pad Then boid.Yvel += turn
            If boid.y > Height - pad Then boid.Yvel -= turn
        End Sub

        Private Sub WrapAround(boid As Boid)
            If boid.x < 0 Then boid.x += Width
            If boid.x > Width Then boid.x -= Width
            If boid.y < 0 Then boid.y += Height
            If boid.y > Height Then boid.y -= Height
        End Sub
    End Class
End Namespace
