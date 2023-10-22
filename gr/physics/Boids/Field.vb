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
                boid.MoveForward()
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

            Protected Overrides Sub Solve(start As Integer, ends As Integer)
                Dim flockXvel As Double = Nothing, flockYvel As Double = Nothing,
               alignXvel As Double = Nothing, alignYvel As Double = Nothing,
               avoidXvel As Double = Nothing, avoidYvel As Double = Nothing,
               predXvel As Double = Nothing, predYval As Double = Nothing

                ' update void speed and direction (velocity) based on rules
                For i As Integer = start To ends
                    Dim boid As Boid = boids(i)

                    field.Flock(boid, 50, 0.0003).Set(flockXvel, flockYvel)
                    field.Align(boid, 50, 0.01).Set(alignXvel, alignYvel)
                    field.Avoid(boid, 20, 0.001).Set(avoidXvel, avoidYvel)
                    field.Predator(boid, 150, 0.00005).Set(predXvel, predYval)

                    boid.Xvel += flockXvel + avoidXvel + alignXvel + predXvel
                    boid.Yvel += flockYvel + avoidYvel + alignYvel + predYval
                Next
            End Sub
        End Class

        Private Function Flock(boid As Boid, distance As Double, power As Double) As (Double, Double)
            ' point toward the center of the flock (mean flock boid position)
            Dim neighbors = grid.SpatialLookup(boid, radius).Where(Function(x) x.GetDistance(boid) < distance).ToArray
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

            For Each neighbor In neighbors
                Dim closeness = distance - boid.GetDistance(neighbor)
                sumClosenessX += (boid.x - neighbor.x) * closeness
                sumClosenessY += (boid.y - neighbor.y) * closeness
            Next
            Return (sumClosenessX * power, sumClosenessY * power)
        End Function

        Public PredatorCount As Integer = 3

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

        Private Function Align(boid As Boid, distance As Double, power As Double) As (Double, Double)
            ' point toward the center of the flock (mean flock boid position)
            Dim neighbors = grid.SpatialLookup(boid, radius).Where(Function(x) x.GetDistance(boid) < distance).ToArray
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
