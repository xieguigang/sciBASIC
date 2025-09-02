Imports std = System.Math

''' <summary>
''' Data type for automatic differentiation with reverse mode accumulation.
''' </summary>
Public Class Rev

    Public Magnitude As Double
    Public Derivative As Double

    Public CalculateDerivative As Differentiation

    Public Sub New(y As Double)
        Magnitude = y
        Derivative = 0
        CalculateDerivative = Sub(x) Derivative += x
    End Sub

    Private Sub New(y As Double, dy As Differentiation)
        Magnitude = y
        Derivative = 0
        CalculateDerivative = dy
    End Sub

    Public Overrides Function ToString() As String
        Return $"[magnitude:{Magnitude}, derivative:{Derivative}]"
    End Function

    Public Shared Widening Operator CType(d As Rev) As Double
        Return d.Magnitude
    End Operator

    Public Shared Operator +(lhs As Rev, rhs As Rev) As Rev
        Return New Rev(lhs.Magnitude + rhs.Magnitude, AddressOf New AddRevRev(lhs, rhs).Differentiation)
    End Operator

    Public Shared Operator +(lhs As Rev, rhs As Double) As Rev
        Return New Rev(lhs.Magnitude + rhs, AddressOf New AddRevDouble(lhs, rhs).Differentiation)
    End Operator

    Public Shared Operator +(lhs As Double, rhs As Rev) As Rev
        Return New Rev(lhs + rhs.Magnitude, AddressOf New AddDoubleRev(lhs, rhs).Differentiation)
    End Operator

    Public Shared Operator -(lhs As Rev, rhs As Rev) As Rev
        Return New Rev(lhs.Magnitude - rhs.Magnitude, Sub(dx)
                                                          If dx <> 0 Then
                                                              lhs.CalculateDerivative(dx)
                                                              rhs.CalculateDerivative(-dx)
                                                          End If
                                                      End Sub)
    End Operator

    Public Shared Operator -(lhs As Rev, rhs As Double) As Rev
        Return New Rev(lhs.Magnitude - rhs, Sub(dx)
                                                If dx <> 0 Then
                                                    lhs.CalculateDerivative(dx)
                                                End If
                                            End Sub)
    End Operator

    Public Shared Operator -(lhs As Double, rhs As Rev) As Rev
        Return New Rev(lhs - rhs.Magnitude, Sub(dx)
                                                If dx <> 0 Then
                                                    rhs.CalculateDerivative(-dx)
                                                End If
                                            End Sub)
    End Operator

    Public Shared Operator -(lhs As Rev) As Rev
        Return New Rev(-lhs.Magnitude, Sub(dx)
                                           If dx <> 0 Then
                                               lhs.CalculateDerivative(-dx)
                                           End If
                                       End Sub)
    End Operator

    Public Shared Operator *(lhs As Rev, rhs As Rev) As Rev
        Return New Rev(lhs.Magnitude * rhs.Magnitude, Sub(dx)
                                                          If dx <> 0 Then
                                                              lhs.CalculateDerivative(dx * rhs.Magnitude)
                                                              rhs.CalculateDerivative(dx * lhs.Magnitude)
                                                          End If
                                                      End Sub)
    End Operator

    Public Shared Operator *(lhs As Rev, rhs As Double) As Rev
        Return New Rev(lhs.Magnitude * rhs, Sub(dx)
                                                If dx <> 0 Then
                                                    lhs.CalculateDerivative(dx * rhs)
                                                End If
                                            End Sub)
    End Operator

    Public Shared Operator *(lhs As Double, rhs As Rev) As Rev
        Return New Rev(lhs * rhs.Magnitude, Sub(dx)
                                                If dx <> 0 Then
                                                    rhs.CalculateDerivative(dx * lhs)
                                                End If
                                            End Sub)
    End Operator

    Public Shared Operator /(lhs As Rev, rhs As Rev) As Rev
        Return New Rev(lhs.Magnitude / rhs.Magnitude, Sub(dx)
                                                          If dx <> 0 Then
                                                              lhs.CalculateDerivative(dx / rhs.Magnitude)
                                                              rhs.CalculateDerivative(-dx * lhs.Magnitude / (rhs.Magnitude * rhs.Magnitude))
                                                          End If
                                                      End Sub)
    End Operator

    Public Shared Operator /(lhs As Rev, rhs As Double) As Rev
        Return New Rev(lhs.Magnitude / rhs, Sub(dx)
                                                If dx <> 0 Then
                                                    lhs.CalculateDerivative(dx / rhs)
                                                End If
                                            End Sub)
    End Operator

    Public Shared Operator /(lhs As Double, rhs As Rev) As Rev
        Return New Rev(lhs / rhs.Magnitude, Sub(dx)
                                                If dx <> 0 Then
                                                    rhs.CalculateDerivative(-dx * lhs / (rhs.Magnitude * rhs.Magnitude))
                                                End If
                                            End Sub)
    End Operator

    Public Function Pow(e As Double) As Rev
        Return New Rev(std.Pow(Magnitude, e), Sub(dx) CalculateDerivative(e * std.Pow(Magnitude, e - 1) * dx))
    End Function

    Public Function Exp() As Rev
        Return New Rev(std.Exp(Magnitude), Sub(dx) CalculateDerivative(std.Exp(Magnitude) * dx))
    End Function

    Public Function Log() As Rev
        Return New Rev(std.Log(Magnitude), Sub(dx) CalculateDerivative(1.0 / Magnitude * dx))
    End Function
End Class
