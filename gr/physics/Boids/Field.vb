Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace Boids.Model
    Public Class Field
        Public ReadOnly Width As Double
        Public ReadOnly Height As Double
        Public ReadOnly Boids As List(Of Boid) = New List(Of Boid)()
        Private ReadOnly Rand As Random = New Random()

        Public Sub New(ByVal width As Double, ByVal height As Double, ByVal Optional boidCount As Integer = 100)
            Call (width, height).Set(Me.Width, Me.Height)

            For i = 0 To boidCount - 1
                Boids.Add(New Boid(Rand, width, height))
            Next
        End Sub

        Public Sub Advance(ByVal Optional bounceOffWalls As Boolean = True, ByVal Optional wrapAroundEdges As Boolean = False)
            Dim flockXvel As Double = Nothing, flockYvel As Double = Nothing, alignXvel As Double = Nothing, alignYvel As Double = Nothing, avoidXvel As Double = Nothing, avoidYvel As Double = Nothing, predXvel As Double = Nothing, predYval As Double = Nothing
            ' update void speed and direction (velocity) based on rules
            For Each boid In Boids
                Flock(boid, 50, 0.0003).Set(flockXvel, flockYvel)
                Align(boid, 50, 0.01).Set(alignXvel, alignYvel)
                Avoid(boid, 20, 0.001).Set(avoidXvel, avoidYvel)
                Predator(boid, 150, 0.00005).Set(predXvel, predYval)

                boid.Xvel += flockXvel + avoidXvel + alignXvel + predXvel
                boid.Yvel += flockYvel + avoidYvel + alignYvel + predYval
            Next

            ' move all boids forward in time
            For Each boid In Boids
                boid.MoveForward()
                If bounceOffWalls Then Me.BounceOffWalls(boid)
                If wrapAroundEdges Then WrapAround(boid)
            Next
        End Sub

        Private Function Flock(ByVal boid As Boid, ByVal distance As Double, ByVal power As Double) As (Double, Double)
            ' point toward the center of the flock (mean flock boid position)
            Dim neighbors = Boids.Where(Function(x) x.GetDistance(boid) < distance)
            Dim meanX As Double = neighbors.Sum(Function(x) x.X) / neighbors.Count()
            Dim meanY As Double = neighbors.Sum(Function(x) x.Y) / neighbors.Count()
            Dim deltaCenterX = meanX - boid.X
            Dim deltaCenterY = meanY - boid.Y
            Return (deltaCenterX * power, deltaCenterY * power)
        End Function

        Private Function Avoid(ByVal boid As Boid, ByVal distance As Double, ByVal power As Double) As (Double, Double)
            ' point away as boids get close
            Dim neighbors = Boids.Where(Function(x) x.GetDistance(boid) < distance)
            Dim sumClosenessX As Double = Nothing, sumClosenessY As Double = Nothing

            For Each neighbor In neighbors
                Dim closeness = distance - boid.GetDistance(neighbor)
                sumClosenessX += (boid.X - neighbor.X) * closeness
                sumClosenessY += (boid.Y - neighbor.Y) * closeness
            Next
            Return (sumClosenessX * power, sumClosenessY * power)
        End Function

        Public PredatorCount As Integer = 3
        Private Function Predator(ByVal boid As Boid, ByVal distance As Double, ByVal power As Double) As (Double, Double)
            ' point away as predators get close
            Dim sumClosenessX As Double = Nothing, sumClosenessY As Double = Nothing

            For i = 0 To PredatorCount - 1
                Dim lPredator = Boids(i)
                Dim distanceAway = boid.GetDistance(lPredator)
                If distanceAway < distance Then
                    Dim closeness = distance - distanceAway
                    sumClosenessX += (boid.X - lPredator.X) * closeness
                    sumClosenessY += (boid.Y - lPredator.Y) * closeness
                End If
            Next
            Return (sumClosenessX * power, sumClosenessY * power)
        End Function

        Private Function Align(ByVal boid As Boid, ByVal distance As Double, ByVal power As Double) As (Double, Double)
            ' point toward the center of the flock (mean flock boid position)
            Dim neighbors = Boids.Where(Function(x) x.GetDistance(boid) < distance)
            Dim meanXvel As Double = neighbors.Sum(Function(x) x.Xvel) / neighbors.Count()
            Dim meanYvel As Double = neighbors.Sum(Function(x) x.Yvel) / neighbors.Count()
            Dim dXvel = meanXvel - boid.Xvel
            Dim dYvel = meanYvel - boid.Yvel
            Return (dXvel * power, dYvel * power)
        End Function

        Private Sub BounceOffWalls(ByVal boid As Boid)
            Dim pad As Double = 50
            Dim turn = 0.5
            If boid.X < pad Then boid.Xvel += turn
            If boid.X > Width - pad Then boid.Xvel -= turn
            If boid.Y < pad Then boid.Yvel += turn
            If boid.Y > Height - pad Then boid.Yvel -= turn
        End Sub

        Private Sub WrapAround(ByVal boid As Boid)
            If boid.X < 0 Then boid.X += Width
            If boid.X > Width Then boid.X -= Width
            If boid.Y < 0 Then boid.Y += Height
            If boid.Y > Height Then boid.Y -= Height
        End Sub
    End Class
End Namespace
