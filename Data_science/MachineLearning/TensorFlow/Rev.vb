#Region "Microsoft.VisualBasic::aa2efa81624081b34a6426f473b15af6, Data_science\MachineLearning\TensorFlow\Rev.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 145
    '    Code Lines: 118 (81.38%)
    ' Comment Lines: 3 (2.07%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 24 (16.55%)
    '     File Size: 6.47 KB


    ' Class Rev
    ' 
    '     Properties: Zero
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: Exp, Log, Pow, ToString
    '     Operators: (+4 Overloads) -, (+3 Overloads) *, (+3 Overloads) /, (+3 Overloads) +
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

''' <summary>
''' Data type for automatic differentiation with reverse mode accumulation.
''' </summary>
Public Class Rev

    Public Magnitude As Double
    Public Derivative As Double

    Public CalculateDerivative As Differentiation

    Public Shared ReadOnly Property Zero As Rev
        Get
            Return New Rev(0.0)
        End Get
    End Property

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

