Namespace SVG.PathHelper

    Public Class A : Inherits Command
        Public Property Rx As Double
        Public Property Ry As Double
        Public Property XRot As Double
        Public Property Large As Boolean
        Public Property Sweep As Boolean
        Public Property X As Double
        Public Property Y As Double

        Public Sub New(rx As Double, ry As Double, xRot As Double, large As Boolean, sweep As Boolean, x As Double, y As Double)
            Me.Rx = rx
            Me.Ry = ry
            Me.XRot = xRot
            Me.Large = large
            Me.Sweep = sweep
            Me.X = x
            Me.Y = y
        End Sub

        Public Sub New(text As String, Optional isRelative As Boolean = False)
            MyBase.isRelative = isRelative
            Dim tokens = Parse(text)
            Me.MapTokens(tokens)
        End Sub

        Public Sub New(tokens As List(Of String), Optional isRelative As Boolean = False)
            MyBase.isRelative = isRelative
            Me.MapTokens(tokens)
        End Sub

        Private Sub MapTokens(tokens As List(Of String))
            If (tokens.Count = 7) Then
                Me.Rx = Double.Parse(tokens(0))
                Me.Ry = Double.Parse(tokens(1))
                Me.XRot = Double.Parse(tokens(2))
                Me.Large = System.Convert.ToBoolean(Integer.Parse(tokens(3)))
                Me.Sweep = System.Convert.ToBoolean(Integer.Parse(tokens(4)))
                Me.X = Double.Parse(tokens(5))
                Me.Y = Double.Parse(tokens(6))
            End If
        End Sub


        Public Overrides Sub Scale(factor As Double)
            Rx *= factor
            Ry *= factor
            X *= factor
            Y *= factor
        End Sub

        Public Overrides Sub Translate(deltaX As Double, deltaY As Double)
            X += deltaX
            Y += deltaY
        End Sub

        Public Overrides Function ToString() As String
            Return $"{If(isRelative, "a"c, "A"c)}{Rx},{Ry} {XRot} {Convert.ToInt32(Large)},{Convert.ToInt32(Sweep)} {X},{Y}"
        End Function

    End Class
End Namespace