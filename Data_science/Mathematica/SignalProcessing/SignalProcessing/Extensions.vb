#Region "Microsoft.VisualBasic::3f15346b59071c14aebe6d85f4c5d1c1, Data_science\Mathematica\SignalProcessing\SignalProcessing\Extensions.vb"

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

    ' Module Extensions
    ' 
    '     Function: AsSignal
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Public Module Extensions

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsSignal(signals As IEnumerable(Of TimeSignal)) As Signal
        Return New Signal(signals)
    End Function
End Module
