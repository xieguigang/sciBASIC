Imports System

Namespace Boids.Model
    Public Class Boid
        Public X As Double
        Public Y As Double
        Public Xvel As Double
        Public Yvel As Double

        Public Sub New(ByVal x As Double, ByVal y As Double, ByVal xVel As Double, ByVal yVel As Double)
            Call (x, y, xVel, yVel).Set(Me.X, Me.Y, Me.Xvel, Me.Yvel)
        End Sub

        Public Sub New(ByVal rand As Random, ByVal width As Double, ByVal height As Double)
            X = rand.NextDouble() * width
            Y = rand.NextDouble() * height
            Xvel = (rand.NextDouble() - 0.5)
            Yvel = (rand.NextDouble() - 0.5)
        End Sub

        Public Sub MoveForward(ByVal Optional minSpeed As Double = 1, ByVal Optional maxSpeed As Double = 5)
            X += Xvel
            Y += Yvel

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

        Public Function GetPosition(ByVal time As Double) As (Double, Double)
            Return (X + Xvel * time, Y + Yvel * time)
        End Function

        Public Sub Accelerate(ByVal Optional scale As Double = 1.0)
            Xvel *= scale
            Yvel *= scale
        End Sub

        Public Function GetAngle() As Double
            If Double.IsNaN(Xvel) OrElse Double.IsNaN(Yvel) Then Return 0
            If Xvel = 0 AndAlso Yvel = 0 Then Return 0
            Dim angle = Math.Atan(Yvel / Xvel) * 180 / Math.PI - 90
            If Xvel < 0 Then angle += 180
            Return angle
        End Function

        Public Function GetSpeed() As Double
            Return Math.Sqrt(Xvel * Xvel + Yvel * Yvel)
        End Function

        Public Function GetDistance(ByVal otherBoid As Boid) As Double
            Dim dX = otherBoid.X - X
            Dim dY = otherBoid.Y - Y
            Dim dist = Math.Sqrt(dX * dX + dY * dY)
            Return dist
        End Function
    End Class
End Namespace
