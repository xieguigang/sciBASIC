Namespace de.rub.dks.signal.generator.sound.arithmetic



	'
	'* Advanced Signal class for operations on two Signals.
	'* Extend this class and implement the calculate method which is used by all the other methods.
	'
	Public MustInherit Class ArithmeticSignal
		Protected Friend a, b As de.rub.dks.signal.generator.sound.Signal
		Protected Friend audioPosA, audioPosB As Double

		Public Sub New(ByVal aa As de.rub.dks.signal.generator.sound.Signal, ByVal bb As de.rub.dks.signal.generator.sound.Signal)
			setOperands(aa, bb)
		End Sub

		''' <summary>
		''' Calculates the desired operation of the two operand's values at given frequency and phase </summary>
		''' <param name="freq1">  the first signal's frequency </param>
		''' <param name="phase1">	the first signal's phase </param>
		''' <param name="freq2">  the seconds signal's frequency </param>
		''' <param name="phase2">	the seconds signal's phase </param>
		Public MustOverride Function calculate(ByVal freq1 As Double, ByVal phase1 As Double, ByVal freq2 As Double, ByVal phase2 As Double) As Double

		''' <summary>
		''' Generates a GraphViewData array with Signal.GRAPH_SAMPLES entries.
		''' This method uses the internal phase and frequency values of its operand members. </summary>
		''' <returns>    	the samples </returns>
		Public Overridable Property GraphData As com.jjoe64.graphview.GraphView.GraphViewData()
			Get
				Dim data(de.rub.dks.signal.generator.sound.Signal.GRAPH_SAMPLES - 1) As com.jjoe64.graphview.GraphView.GraphViewData
				Dim x As Double = 0, y As Double = 0
				Dim stepX As Double = a.Freq * 3 \ de.rub.dks.signal.generator.sound.Signal.GRAPH_SAMPLES
				Dim stepY As Double = b.Freq * 3 \ de.rub.dks.signal.generator.sound.Signal.GRAPH_SAMPLES
				For i As Integer = 0 To de.rub.dks.signal.generator.sound.Signal.GRAPH_SAMPLES - 1
					data(i) = New com.jjoe64.graphview.GraphView.GraphViewData(i, calculate(x, a.Phase, y, b.Phase))
					x += stepX
					y += stepY
				Next i
				Return data
			End Get
		End Property

		''' <summary>
		''' Generates a byte array with Signal.BUFFER_SIZE samples to be played back, resembling the sound of the signal.
		''' This method uses the internal phase and frequency values of its operand members. </summary>
		''' <returns>    	the samples </returns>
		Public Overridable Property AudioBytes As SByte()
			Get
				Dim samples(de.rub.dks.signal.generator.sound.Signal.BUFFER_SIZE - 1) As SByte
				For i As Integer = 0 To de.rub.dks.signal.generator.sound.Signal.BUFFER_SIZE - 1
					samples(i) = CByte(de.rub.dks.signal.generator.sound.Signal.AMPLITUDE * calculate(audioPosA, a.Phase, audioPosB, b.Phase))
					' 2 * Math.PI * freq / SAMPLE_RATE;
					audioPosA += 100 * a.Freq / de.rub.dks.signal.generator.sound.Signal.SAMPLE_RATE
					audioPosB += 100 * b.Freq / de.rub.dks.signal.generator.sound.Signal.SAMPLE_RATE
				Next i
				Return samples
			End Get
		End Property

		' Getters and setters for the internal variables

		Public Overridable Sub setOperands(ByVal aa As de.rub.dks.signal.generator.sound.Signal, ByVal bb As de.rub.dks.signal.generator.sound.Signal)
			a = aa
			b = bb
		End Sub

		Public Overridable Property SignalA As de.rub.dks.signal.generator.sound.Signal
			Set(ByVal aa As de.rub.dks.signal.generator.sound.Signal)
				a = aa
			End Set
			Get
				Return a
			End Get
		End Property

		Public Overridable Property SignalB As de.rub.dks.signal.generator.sound.Signal
			Set(ByVal bb As de.rub.dks.signal.generator.sound.Signal)
				b = bb
			End Set
			Get
				Return b
			End Get
		End Property



	End Class


    Public Class SumSignal
        Inherits ArithmeticSignal

        Public Sub New(ByVal aa As de.rub.dks.signal.generator.sound.Signal, ByVal bb As de.rub.dks.signal.generator.sound.Signal)
            MyBase.New(aa, bb)
        End Sub

        Public Overrides Function calculate(ByVal freq1 As Double, ByVal phase1 As Double, ByVal freq2 As Double, ByVal phase2 As Double) As Double
            Return a.calculate(freq1, phase1) + b.calculate(freq2, phase2)
        End Function


    End Class

    Public Class MultiplySignal
        Inherits ArithmeticSignal

        Public Sub New(ByVal aa As de.rub.dks.signal.generator.sound.Signal, ByVal bb As de.rub.dks.signal.generator.sound.Signal)
            MyBase.New(aa, bb)
        End Sub

        Public Overrides Function calculate(ByVal freq1 As Double, ByVal phase1 As Double, ByVal freq2 As Double, ByVal phase2 As Double) As Double
            Return a.calculate(freq1, phase1) * b.calculate(freq2, phase2)
        End Function

    End Class

    Public Class DivisionSignal
        Inherits ArithmeticSignal

        Public Sub New(ByVal aa As de.rub.dks.signal.generator.sound.Signal, ByVal bb As de.rub.dks.signal.generator.sound.Signal)
            MyBase.New(aa, bb)
        End Sub

        Public Overrides Function calculate(ByVal freq1 As Double, ByVal phase1 As Double, ByVal freq2 As Double, ByVal phase2 As Double) As Double
            Return a.calculate(freq1, phase1) / b.calculate(freq2, phase2)
        End Function

    End Class

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