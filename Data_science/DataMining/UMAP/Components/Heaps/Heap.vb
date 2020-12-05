Imports System.Runtime.CompilerServices

Public NotInheritable Class Heap

    ReadOnly _values As New List(Of Double()())

    Default Public ReadOnly Property Item(index As Integer) As Double()()
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return _values(index)
        End Get
    End Property

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Add(value As Double()())
        Call _values.Add(value)
    End Sub
End Class