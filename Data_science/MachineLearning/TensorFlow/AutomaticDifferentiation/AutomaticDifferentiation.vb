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

Public Class AddRevRev : Inherits AutomaticDifferentiation(Of Rev, Rev)

    Public Sub New(lhs As Rev, rhs As Rev)
        MyBase.New(lhs, rhs)
    End Sub

    Public Overrides Sub Differentiation(dx As Double)
        If dx <> 0 Then
            lhs.CalculateDerivative(dx)
            rhs.CalculateDerivative(dx)
        End If
    End Sub
End Class

Public Class AddRevDouble : Inherits AutomaticDifferentiation(Of Rev, Double)

    Public Sub New(lhs As Rev, rhs As Double)
        MyBase.New(lhs, rhs)
    End Sub

    Public Overrides Sub Differentiation(dx As Double)
        If dx <> 0 Then
            lhs.CalculateDerivative(dx)
        End If
    End Sub
End Class

Public Class AddDoubleRev : Inherits AutomaticDifferentiation(Of Double, Rev)

    Public Sub New(lhs As Double, rhs As Rev)
        MyBase.New(lhs, rhs)
    End Sub

    Public Overrides Sub Differentiation(dx As Double)
        If dx <> 0 Then
            rhs.CalculateDerivative(dx)
        End If
    End Sub
End Class