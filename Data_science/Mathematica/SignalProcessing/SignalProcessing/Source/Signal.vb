#Region "Microsoft.VisualBasic::705d047bf34d602c110b89a93d10e69b, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\Signal.vb"

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

    '   Total Lines: 89
    '    Code Lines: 42 (47.19%)
    ' Comment Lines: 29 (32.58%)
    '    - Xml Docs: 86.21%
    ' 
    '   Blank Lines: 18 (20.22%)
    '     File Size: 3.49 KB


    '     Class Signal
    ' 
    '         Properties: Freq, Phase
    ' 
    '         Function: CalcAudioBytes, calculate, GetGraphData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Source

    ''' <summary>
    ''' Basic Signal class for providing all kinds of signal-data to the GUI.
    ''' Extend this class and implement the calculate method which is used 
    ''' by all the other methods.
    ''' 
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
        Public MustOverride Function calculate(freq As Double, phase As Double) As Double

        ''' <summary>
        ''' Calculates the signals value with the last used frequency and phase
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function calculate() As Double
            Return calculate(Freq, Phase)
        End Function

        ''' <summary>
        ''' Generates a GraphViewData array with GRAPH_SAMPLES entries.
        ''' This method also sets the internal phase and frequency to those passed in the parameters. </summary>
        ''' <param name="freq">   the signal's frequency </param>
        ''' <param name="phase">	the signal's phase </param>
        ''' <returns>    	the samples </returns>
        Public Overridable Function GetGraphData(freq As Double, phase As Double) As TimeSignal()
            Dim data(GRAPH_SAMPLES - 1) As TimeSignal
            Dim x As Double = 0

            Me.Freq = freq
            Me.Phase = phase

            Dim [step] As Double = freq * 3 / GRAPH_SAMPLES

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
        Public Overridable Function CalcAudioBytes(freq As Double, phase As Double) As SByte()
            Dim samples(BUFFER_SIZE - 1) As SByte

            Me.Freq = freq
            Me.Phase = phase

            For i As Integer = 0 To BUFFER_SIZE - 1
                samples(i) = CSByte(AMPLITUDE * calculate(audioPos, phase))
                ' audioPos += 2 * stdNum.PI * freq / SAMPLE_RATE;
                audioPos += 440 * freq / SAMPLE_RATE
            Next

            Return samples
        End Function
    End Class
End Namespace
