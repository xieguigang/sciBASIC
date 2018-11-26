Namespace Source

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

        Protected Friend audioPos As Double

        ' Getters and setters for the internal variables

        Public Overridable Property Freq As Double
        Public Overridable Property Phase As Double

        ''' <summary>
        ''' Calculates the signals value at given frequency and phase </summary>
        ''' <param name="freq">   the signal's frequency </param>
        ''' <param name="phase">	the signal's phase </param>
        Public MustOverride Function calculate(ByVal freq As Double, ByVal phase As Double) As Double

        ''' <summary>
        ''' Calculates the signals value with the last used frequency and phase
        ''' </summary>
        Public Overridable Function calculate() As Double
            Return calculate(Freq, Phase)
        End Function

        ''' <summary>
        ''' Generates a GraphViewData array with GRAPH_SAMPLES entries.
        ''' This method also sets the internal phase and frequency to those passed in the parameters. </summary>
        ''' <param name="freq">   the signal's frequency </param>
        ''' <param name="phase">	the signal's phase </param>
        ''' <returns>    	the samples </returns>
        Public Overridable Function getGraphData(ByVal freq As Double, ByVal phase As Double) As TimeSignal()
            Dim data(GRAPH_SAMPLES - 1) As TimeSignal
            Dim x As Double = 0

            Me.Freq = freq
            Me.Phase = phase

            Dim [step] As Integer = CInt(freq) * 3 \ GRAPH_SAMPLES

            For i As Integer = 0 To GRAPH_SAMPLES - 1
                data(i) = New TimeSignal With {
                    .time = i,
                    .intensity = calculate(x, phase)
                }
                x += [step]
            Next

            Return data
        End Function

        ''' <summary>
        ''' Generates a byte array with BUFFER_SIZE samples to be played back, resembling the sound of the signal.
        ''' This method also sets the internal phase and frequency to those passed in the parameters. </summary>
        ''' <param name="freq">   the signal's frequency </param>
        ''' <param name="phase">	the signal's phase </param>
        ''' <returns>    	the samples </returns>
        Public Overridable Function getAudioBytes(ByVal freq As Double, ByVal phase As Double) As SByte()
            Dim samples(BUFFER_SIZE - 1) As SByte

            Me.Freq = freq
            Me.Phase = phase

            For i As Integer = 0 To BUFFER_SIZE - 1
                samples(i) = CSByte(AMPLITUDE * calculate(audioPos, phase))
                ' audioPos += 2 * Math.PI * freq / SAMPLE_RATE;
                audioPos += 440 * freq / SAMPLE_RATE
            Next

            Return samples
        End Function
    End Class
End Namespace