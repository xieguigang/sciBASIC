#Region "Microsoft.VisualBasic::386f4666f498cdf2e8c0408435a662ea, Data_science\Mathematica\SignalProcessing\SignalProcessing\TimeSignals.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Class TimeSignal
    ' 
    '     Properties: intensity, time
    ' 
    '     Function: SignalSequence, ToString
    ' 
    ' Class Signal
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Operators: +
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq

Public Class TimeSignal

    Public Property time As Double
    Public Property intensity As Double

    Public Overrides Function ToString() As String
        Return $"[{time}, {intensity}]"
    End Function

    Public Shared Iterator Function SignalSequence(data As IEnumerable(Of Double)) As IEnumerable(Of TimeSignal)
        For Each p As SeqValue(Of Double) In data.SeqIterator(offset:=1)
            Yield New TimeSignal With {
                .time = p.i,
                .intensity = p.value
            }
        Next
    End Function
End Class

Public Class Signal : Inherits Vector(Of TimeSignal)

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Sub New(data As IEnumerable(Of TimeSignal))
        Call MyBase.New(data)
    End Sub

    Public Shared Operator +(a As Signal, b As Signal) As Signal
        Throw New NotImplementedException
    End Operator
End Class
