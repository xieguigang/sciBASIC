Imports System.Runtime.CompilerServices

Public Module TupleHelper

    <Extension>
    Public Sub [Set](Of T1, T2)(t As (T1, T2), ByRef a As T1, ByRef b As T2)
        a = t.Item1
        b = t.Item2
    End Sub

    <Extension>
    Public Sub [Set](Of T1, T2, T3, T4)(t As (T1, T2, T3, T4), ByRef a As T1, ByRef b As T2, ByRef c As T3, ByRef d As T4)
        a = t.Item1
        b = t.Item2
        c = t.Item3
        d = t.Item4
    End Sub
End Module
