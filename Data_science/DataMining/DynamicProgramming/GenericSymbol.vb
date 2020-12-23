Public Class GenericSymbol(Of T)

    Friend ReadOnly m_equals As Func(Of T, T, Boolean)
    Friend ReadOnly m_similarity As Func(Of T, T, Double)

End Class
