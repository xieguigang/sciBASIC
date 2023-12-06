Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace math.functions

    ''' <summary>
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class RandomDouble
        Inherits DoubleFunction

        Private ReadOnly scalar As Double

        Public Sub New()
            Me.New(1.0)
        End Sub

        Public Sub New(scalar As Double)
            Me.scalar = scalar
        End Sub

        Public Overrides Function apply(v As Double) As Double
            Return randf.NextDouble() * scalar
        End Function

    End Class

End Namespace
