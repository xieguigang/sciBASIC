Public Class Chain

    Public Property obSequence As Array

    Friend ReadOnly equalsTo As Func(Of Object, Object, Boolean)

    Default Public ReadOnly Property Item(i As Integer) As Object
        Get
            Return _obSequence(i)
        End Get
    End Property

    Public ReadOnly Property length As Integer
        Get
            Return obSequence.Length
        End Get
    End Property

    Sub New(equalsTo As Func(Of Object, Object, Boolean))
        Me.equalsTo = equalsTo
    End Sub

End Class
