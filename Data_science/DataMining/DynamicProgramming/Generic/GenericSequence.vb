Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Public Class GenericSequence(Of T)

    Dim m_seq As T()
    Dim m_equals As Func(Of T, T, Boolean)

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

    Sub New(seq As IEnumerable(Of T), equals As Func(Of T, T, Boolean))
        m_seq = seq.ToArray
        m_equals = equals
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
        Return New GenericSequence(Of T)(a.m_seq.JoinIterates(b.m_seq), a.m_equals)
    End Operator

    Public Shared Operator =(a As GenericSequence(Of T), b As GenericSequence(Of T)) As Boolean
        If a Is b Then
            Return True
        ElseIf a.length <> b.length Then
            Return False
        Else
            For i As Integer = 0 To a.length - 1
                If Not a.m_equals(a.m_seq(i), b.m_seq(i)) Then
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
End Class
