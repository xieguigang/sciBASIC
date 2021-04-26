Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.My.JavaScript

Public Module Utils

    <Extension>
    Public Function findSequence(Of T)(sequence As IEnumerable(Of T), states As T()) As IEnumerable(Of Integer)
        Return sequence.reduce(Function(all, curr)
                                   Return all.Add(states.findIndex(Function(x) x.state = curr))
                                   Return all
                               End Function, New List(Of Integer))
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function gamma(alpha#, beta#, forward#) As Double
        Return (alpha * beta) / forward
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function xi(alpha#, trans#, emiss#, beta#, forward#) As Double
        Return (alpha * trans * emiss * beta) / forward
    End Function
End Module

