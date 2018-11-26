Namespace de.rub.dks.signal.generator.sound

    '
    ' * Basic Signal class for providing all kinds of signal-data to the GUI.
    ' * Extend this class and implement the calculate method which is used by all the other methods.
    '

    ''' <summary>
    ''' > https://github.com/dks-rub/signalgenerator
    ''' </summary>
    Public MustInherit Class Signal
		Public Const GRAPH_SAMPLES As Integer = 800
		Public Const BUFFER_SIZE As Integer = 4096
		Public Const SAMPLE_RATE As Integer = 44100
        Public Shared ReadOnly AMPLITUDE As SByte = SByte.MaxValue

        Protected Friend audioPos, fr, ph As Double

		''' <summary>
		''' Calculates the signals value at given frequency and phase </summary>
		''' <param name="freq">   the signal's frequency </param>
		''' <param name="phase">	the signal's phase </param>
		Public MustOverride Function calculate(ByVal freq As Double, ByVal phase As Double) As Double

		''' <summary>
		''' Calculates the signals value with the last used frequency and phase
		''' </summary>
		Public Overridable Function calculate() As Double
			Return calculate(fr, ph)
		End Function

		''' <summary>
		''' Generates a GraphViewData array with GRAPH_SAMPLES entries.
		''' This method also sets the internal phase and frequency to those passed in the parameters. </summary>
		''' <param name="freq">   the signal's frequency </param>
		''' <param name="phase">	the signal's phase </param>
		''' <returns>    	the samples </returns>
		Public Overridable Function getGraphData(ByVal freq As Double, ByVal phase As Double) As com.jjoe64.graphview.GraphView.GraphViewData()
			fr = freq
			ph = phase
			Dim data(GRAPH_SAMPLES - 1) As com.jjoe64.graphview.GraphView.GraphViewData
			Dim x As Double = 0
			Dim [step] As Double = freq * 3 \ GRAPH_SAMPLES
			For i As Integer = 0 To GRAPH_SAMPLES - 1
				data(i) = New com.jjoe64.graphview.GraphView.GraphViewData(i, calculate(x, phase))
				x += [step]
			Next i
			Return data
		End Function

		''' <summary>
		''' Generates a byte array with BUFFER_SIZE samples to be played back, resembling the sound of the signal.
		''' This method also sets the internal phase and frequency to those passed in the parameters. </summary>
		''' <param name="freq">   the signal's frequency </param>
		''' <param name="phase">	the signal's phase </param>
		''' <returns>    	the samples </returns>
		Public Overridable Function getAudioBytes(ByVal freq As Double, ByVal phase As Double) As SByte()
			fr = freq
			ph = phase
			Dim samples(BUFFER_SIZE - 1) As SByte
			For i As Integer = 0 To BUFFER_SIZE - 1
				samples(i) = CByte(AMPLITUDE * calculate(audioPos, phase))
				' audioPos += 2 * Math.PI * freq / SAMPLE_RATE;
				audioPos += 440 * freq / SAMPLE_RATE
			Next i
			Return samples
		End Function

		' Getters and setters for the internal variables

		Public Overridable Property Freq As Double
			Get
				Return fr
			End Get
			Set(ByVal fr As Double)
				Me.fr = fr
			End Set
		End Property


		Public Overridable Property Phase As Double
			Get
				Return ph
			End Get
			Set(ByVal ph As Double)
				Me.ph = ph
			End Set
		End Property


	End Class

End Namespace