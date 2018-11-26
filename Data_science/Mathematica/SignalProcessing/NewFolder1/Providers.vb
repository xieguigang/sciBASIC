Namespace de.rub.dks.signal.generator.sound

	Public Class OneSignal
		Inherits Signal

		Public Overrides Function calculate(ByVal freq As Double, ByVal phase As Double) As Double
			Return 1
		End Function

	End Class

    Public Class RectangularSignal
        Inherits Signal

        Public Overrides Function calculate(ByVal freq As Double, ByVal phase As Double) As Double
            Return Math.Sign(Math.Sin(freq + phase))
        End Function

    End Class

    Public Class SawtoothSignal
        Inherits Signal

        Public Overrides Function calculate(ByVal freq As Double, ByVal phase As Double) As Double
            Return 2 * ((freq + phase) / (2 * Math.PI) - Math.Floor(0.5 + (freq + phase) / (2 * Math.PI)))
        End Function

    End Class

    Public Class SinusSignal
        Inherits Signal

        Public Overrides Function calculate(ByVal freq As Double, ByVal phase As Double) As Double
            Return Math.Sin(freq + phase)
        End Function


    End Class

    Public Class SiSignal
        Inherits Signal

        Public Overrides Function calculate(ByVal freq As Double, ByVal phase As Double) As Double
            Return Math.Sin(2 * Math.PI * (freq - phase)) / (2 * Math.PI * (freq - phase))
        End Function

    End Class

    Public Class TriangularSignal
        Inherits Signal

        Public Overrides Function calculate(ByVal freq As Double, ByVal phase As Double) As Double
            Return 2 * Math.Abs(2 * ((freq + phase) / (2 * Math.PI) - Math.Floor((freq + phase) / (2 * Math.PI) + 0.5))) - 1
        End Function

    End Class

    Public Class ZeroSignal
        Inherits Signal

        Public Overrides Function calculate(ByVal freq As Double, ByVal phase As Double) As Double
            Return 0
        End Function

    End Class
End Namespace