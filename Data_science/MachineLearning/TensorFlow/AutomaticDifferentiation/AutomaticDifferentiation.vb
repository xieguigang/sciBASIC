Public MustInherit Class AutomaticDifferentiation(Of Tlhs, Trhs)

    Protected ReadOnly lhs As Tlhs
    Protected ReadOnly rhs As Trhs

    Sub New(lhs As Tlhs, rhs As Trhs)
        Me.lhs = lhs
        Me.rhs = rhs
    End Sub

    Public MustOverride Sub Differentiation(dx As Double)

End Class

''' <summary>
''' dy
''' </summary>
''' <param name="dx"></param>
Public Delegate Sub Differentiation(dx As Double)