Imports System.Runtime.CompilerServices

Namespace Source

    '
    '* Advanced Signal class for operations on two Signals.
    '* Extend this class and implement the calculate method which is used by all the other methods.
    '
    Public MustInherit Class ArithmeticSignal

        Protected Friend audioPosA, audioPosB As Double

        Public Overridable Property SignalA As Signal
        Public Overridable Property SignalB As Signal

        Public Sub New(ByVal aa As Signal, ByVal bb As Signal)
            SignalA = aa
            SignalB = bb
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
        Public Overridable ReadOnly Property GraphData As TimeSignal()
            Get
                Dim data(Signal.GRAPH_SAMPLES - 1) As TimeSignal
                Dim x As Double = 0, y As Double = 0
                Dim stepX = CLng(SignalA.Freq * 3) \ Signal.GRAPH_SAMPLES
                Dim stepY = CLng(SignalB.Freq * 3) \ Signal.GRAPH_SAMPLES

                For i As Integer = 0 To Signal.GRAPH_SAMPLES - 1
                    data(i) = New TimeSignal With {
                        .time = i,
                        .intensity = calculate(x, SignalA.Phase, y, SignalB.Phase)
                    }
                    x += stepX
                    y += stepY
                Next

                Return data
            End Get
        End Property

        ''' <summary>
        ''' Generates a byte array with Signal.BUFFER_SIZE samples to be played back, resembling the sound of the signal.
        ''' This method uses the internal phase and frequency values of its operand members. </summary>
        ''' <returns>    	the samples </returns>
        Public Overridable ReadOnly Property AudioBytes As SByte()
            Get
                Dim samples(Signal.BUFFER_SIZE - 1) As SByte

                For i As Integer = 0 To Signal.BUFFER_SIZE - 1
                    samples(i) = CSByte(Signal.AMPLITUDE * calculate(audioPosA, SignalA.Phase, audioPosB, SignalB.Phase))
                    ' 2 * Math.PI * freq / SAMPLE_RATE;
                    audioPosA += 100 * SignalA.Freq / Signal.SAMPLE_RATE
                    audioPosB += 100 * SignalB.Freq / Signal.SAMPLE_RATE
                Next

                Return samples
            End Get
        End Property
    End Class

    Public Class SumSignal : Inherits ArithmeticSignal

        Public Sub New(ByVal aa As Signal, ByVal bb As Signal)
            MyBase.New(aa, bb)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(ByVal freq1 As Double, ByVal phase1 As Double, ByVal freq2 As Double, ByVal phase2 As Double) As Double
            Return SignalA.calculate(freq1, phase1) + SignalB.calculate(freq2, phase2)
        End Function
    End Class

    Public Class MultiplySignal : Inherits ArithmeticSignal

        Public Sub New(ByVal aa As Signal, ByVal bb As Signal)
            MyBase.New(aa, bb)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(ByVal freq1 As Double, ByVal phase1 As Double, ByVal freq2 As Double, ByVal phase2 As Double) As Double
            Return SignalA.calculate(freq1, phase1) * SignalB.calculate(freq2, phase2)
        End Function
    End Class

    Public Class DivisionSignal : Inherits ArithmeticSignal

        Public Sub New(ByVal aa As Signal, ByVal bb As Signal)
            MyBase.New(aa, bb)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(ByVal freq1 As Double, ByVal phase1 As Double, ByVal freq2 As Double, ByVal phase2 As Double) As Double
            Return SignalA.calculate(freq1, phase1) / SignalB.calculate(freq2, phase2)
        End Function
    End Class

    Public Class DifferenceSignal : Inherits ArithmeticSignal

        Public Sub New(ByVal aa As Signal, ByVal bb As Signal)
            MyBase.New(aa, bb)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(ByVal freq1 As Double, ByVal phase1 As Double, ByVal freq2 As Double, ByVal phase2 As Double) As Double
            Return SignalA.calculate(freq1, phase1) - SignalB.calculate(freq2, phase2)
        End Function
    End Class
End Namespace