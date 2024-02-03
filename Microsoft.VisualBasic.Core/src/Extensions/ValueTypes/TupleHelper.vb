Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

''' <summary>
''' Deconstruct of the tuple data via byref out
''' </summary>
Public Module TupleHelper

    <Extension>
    Public Sub [Set](Of T1, T2)(t As (T1, T2), <Out> ByRef a As T1, <Out> ByRef b As T2)
        a = t.Item1
        b = t.Item2
    End Sub

    <Extension>
    Public Sub [Set](Of T1, T2, T3)(t As (T1, T2, T3), <Out> ByRef a As T1, <Out> ByRef b As T2, <Out> ByRef c As T3)
        a = t.Item1
        b = t.Item2
        c = t.Item3
    End Sub

    <Extension>
    Public Sub [Set](Of T1, T2, T3, T4)(t As (T1, T2, T3, T4), <Out> ByRef a As T1, <Out> ByRef b As T2, <Out> ByRef c As T3, <Out> ByRef d As T4)
        a = t.Item1
        b = t.Item2
        c = t.Item3
        d = t.Item4
    End Sub
End Module
