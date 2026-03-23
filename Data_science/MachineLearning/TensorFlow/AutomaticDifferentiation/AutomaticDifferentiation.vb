#Region "Microsoft.VisualBasic::0f50bd6b299aa8f65df4ba96ef74c8f7, Data_science\MachineLearning\TensorFlow\AutomaticDifferentiation\AutomaticDifferentiation.vb"

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

    '   Total Lines: 62
    '    Code Lines: 43 (69.35%)
    ' Comment Lines: 4 (6.45%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (24.19%)
    '     File Size: 1.71 KB


    '     Class AutomaticDifferentiation
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Delegate Sub
    ' 
    ' 
    '     Class AddRevRev
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Differentiation
    ' 
    '     Class AddRevDouble
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Differentiation
    ' 
    '     Class AddDoubleRev
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Differentiation
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace AutomaticDifferentiation

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
End Namespace
