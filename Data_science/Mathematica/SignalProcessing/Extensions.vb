Imports System.Runtime.CompilerServices

Public Module Extensions

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsSignal(signals As IEnumerable(Of TimeSignal)) As Signal
        Return New Signal(signals)
    End Function
End Module
