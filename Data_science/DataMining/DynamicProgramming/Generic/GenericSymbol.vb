Public Class GenericSymbol(Of T)

    Friend ReadOnly m_equals As Func(Of T, T, Boolean)
    Friend ReadOnly m_similarity As Func(Of T, T, Double)
    Friend ReadOnly m_viewChar As Func(Of T, Char)
    Friend ReadOnly m_empty As Func(Of T)

    Sub New(equals As Func(Of T, T, Boolean), similarity As Func(Of T, T, Double), toChar As Func(Of T, Char), Optional empty As Func(Of T) = Nothing)
        Me.m_equals = equals
        Me.m_similarity = similarity
        Me.m_viewChar = toChar

        If empty Is Nothing Then
            m_empty = Function() Nothing
        Else
            m_empty = empty
        End If
    End Sub

    Public Function getEmpty() As T
        Return m_empty()
    End Function

End Class
