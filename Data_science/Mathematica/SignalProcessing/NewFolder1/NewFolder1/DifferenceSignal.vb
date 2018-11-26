Namespace de.rub.dks.signal.generator.sound.arithmetic


	Public Class DifferenceSignal
		Inherits ArithmeticSignal

		Public Sub New(ByVal aa As de.rub.dks.signal.generator.sound.Signal, ByVal bb As de.rub.dks.signal.generator.sound.Signal)
			MyBase.New(aa, bb)
		End Sub


		Public Overrides Function calculate(ByVal freq1 As Double, ByVal phase1 As Double, ByVal freq2 As Double, ByVal phase2 As Double) As Double
			Return a.calculate(freq1, phase1) - b.calculate(freq2, phase2)
		End Function

	End Class
End Namespace