Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Public Class GenericSequence(Of T) : Implements IEnumerable(Of T)

    Dim m_seq As T()
    Dim m_symbol As GenericSymbol(Of T)

    Default Public ReadOnly Property Item(i As Integer) As T
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return m_seq(i)
        End Get
    End Property

    Public ReadOnly Property length As Integer
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return m_seq.Length
        End Get
    End Property

    Sub New(seq As IEnumerable(Of T), symbol As GenericSymbol(Of T))
        m_seq = seq.ToArray
        m_symbol = symbol
    End Sub

    Private Sub New()
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetAtOrDefault(i As Integer) As T
        If i >= m_seq.Length Then
            Return Nothing
        Else
            Return m_seq(i)
        End If
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator &(a As GenericSequence(Of T), b As GenericSequence(Of T)) As GenericSequence(Of T)
        Return New GenericSequence(Of T)(a.m_seq.JoinIterates(b.m_seq), a.m_symbol)
    End Operator

    Public Shared Operator =(a As GenericSequence(Of T), b As GenericSequence(Of T)) As Boolean
        If a Is b Then
            Return True
        ElseIf a.length <> b.length Then
            Return False
        Else
            For i As Integer = 0 To a.length - 1
                If Not a.m_symbol.m_equals(a.m_seq(i), b.m_seq(i)) Then
                    Return False
                End If
            Next

            Return True
        End If
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Operator <>(a As GenericSequence(Of T), b As GenericSequence(Of T)) As Boolean
        Return Not a = b
    End Operator

    Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
        For Each x As T In m_seq
            Yield x
        Next
    End Function

    Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function
End Class
