#Region "Microsoft.VisualBasic::38f0479071ac97be84356c55ee29f444, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\ArithmeticSignal.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 122
    '    Code Lines: 78
    ' Comment Lines: 20
    '   Blank Lines: 24
    '     File Size: 5.12 KB


    '     Class ArithmeticSignal
    ' 
    '         Properties: AudioBytes, GraphData, SignalA, SignalB
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class SumSignal
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: calculate
    ' 
    '     Class MultiplySignal
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: calculate
    ' 
    '     Class DivisionSignal
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: calculate
    ' 
    '     Class DifferenceSignal
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: calculate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Source.Arithmetic

    ''' <summary>
    ''' Advanced Signal class for operations on two Signals.
    ''' Extend this class and implement the calculate method 
    ''' which is used by all the other methods.
    ''' </summary>
    Public MustInherit Class ArithmeticSignal

        Protected Friend audioPosA, audioPosB As Double

        Public Overridable Property SignalA As Signal
        Public Overridable Property SignalB As Signal

        Public Sub New(aa As Signal, bb As Signal)
            SignalA = aa
            SignalB = bb
        End Sub

        ''' <summary>
        ''' Calculates the desired operation of the two operand's values at given frequency and phase </summary>
        ''' <param name="freq1">  the first signal's frequency </param>
        ''' <param name="phase1">	the first signal's phase </param>
        ''' <param name="freq2">  the seconds signal's frequency </param>
        ''' <param name="phase2">	the seconds signal's phase </param>
        Public MustOverride Function calculate(freq1 As Double, phase1 As Double, freq2 As Double, phase2 As Double) As Double

        ''' <summary>
        ''' Generates a GraphViewData array with Signal.GRAPH_SAMPLES entries.
        ''' This method uses the internal phase and frequency values of its operand members. </summary>
        ''' <returns>    	the samples </returns>
        Public Overridable ReadOnly Property GraphData As TimeSignal()
            Get
                Dim data(Signal.GRAPH_SAMPLES - 1) As TimeSignal
                Dim x As Double = 0
                Dim y As Double = 0
                Dim stepX = (SignalA.Freq * 3) / Signal.GRAPH_SAMPLES
                Dim stepY = (SignalB.Freq * 3) / Signal.GRAPH_SAMPLES

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
        ''' <returns>the samples </returns>
        Public Overridable ReadOnly Property AudioBytes As SByte()
            Get
                Dim samples(Signal.BUFFER_SIZE - 1) As SByte

                For i As Integer = 0 To Signal.BUFFER_SIZE - 1
                    samples(i) = CSByte(Signal.AMPLITUDE * calculate(audioPosA, SignalA.Phase, audioPosB, SignalB.Phase))
                    ' 2 * stdNum.PI * freq / SAMPLE_RATE;
                    audioPosA += 100 * SignalA.Freq / Signal.SAMPLE_RATE
                    audioPosB += 100 * SignalB.Freq / Signal.SAMPLE_RATE
                Next

                Return samples
            End Get
        End Property
    End Class

    Public Class SumSignal : Inherits ArithmeticSignal

        Public Sub New(aa As Signal, bb As Signal)
            MyBase.New(aa, bb)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(freq1 As Double, phase1 As Double, freq2 As Double, phase2 As Double) As Double
            Return SignalA.calculate(freq1, phase1) + SignalB.calculate(freq2, phase2)
        End Function
    End Class

    Public Class MultiplySignal : Inherits ArithmeticSignal

        Public Sub New(aa As Signal, bb As Signal)
            MyBase.New(aa, bb)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(freq1 As Double, phase1 As Double, freq2 As Double, phase2 As Double) As Double
            Return SignalA.calculate(freq1, phase1) * SignalB.calculate(freq2, phase2)
        End Function
    End Class

    Public Class DivisionSignal : Inherits ArithmeticSignal

        Public Sub New(aa As Signal, bb As Signal)
            MyBase.New(aa, bb)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(freq1 As Double, phase1 As Double, freq2 As Double, phase2 As Double) As Double
            Return SignalA.calculate(freq1, phase1) / SignalB.calculate(freq2, phase2)
        End Function
    End Class

    Public Class DifferenceSignal : Inherits ArithmeticSignal

        Public Sub New(aa As Signal, bb As Signal)
            MyBase.New(aa, bb)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function calculate(freq1 As Double, phase1 As Double, freq2 As Double, phase2 As Double) As Double
            Return SignalA.calculate(freq1, phase1) - SignalB.calculate(freq2, phase2)
        End Function
    End Class
End Namespace
