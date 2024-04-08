Namespace Orthogonal.util
    Public Class Pair(Of T1, T2)
        Public m_a As T1
        Public m_b As T2

        Public Sub New(a As T1, b As T2)
            m_a = a
            m_b = b
        End Sub

        Public Overrides Function Equals(o As Object) As Boolean
            If Not (TypeOf o Is Pair(Of T1, T2)) Then
                Return False
            End If
            If m_a Is Nothing AndAlso CType(o, Pair(Of T1, T2)).m_a IsNot Nothing Then
                Return False
            End If
            If m_b Is Nothing AndAlso CType(o, Pair(Of T1, T2)).m_b IsNot Nothing Then
                Return False
            End If
            If Not m_a.Equals(CType(o, Pair(Of T1, T2)).m_a) Then
                Return False
            End If
            If Not m_b.Equals(CType(o, Pair(Of T1, T2)).m_b) Then
                Return False
            End If
            Return True
        End Function

        Public Overrides Function ToString() As String
            Return "<" & m_a.ToString() & "," & m_b.ToString() & ">"
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return 31 * m_a.GetHashCode() + m_b.GetHashCode()
            ' http://stackoverflow.com/questions/299304/why-does-javas-hashcode-in-string-use-31-as-a-multiplier
        End Function
    End Class

End Namespace
