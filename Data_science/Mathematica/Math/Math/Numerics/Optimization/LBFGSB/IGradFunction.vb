#Region "Microsoft.VisualBasic::8b83c891cc2b84c92170a6dadd1767e3, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\IGradFunction.vb"

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

    '   Total Lines: 54
    '    Code Lines: 38 (70.37%)
    ' Comment Lines: 6 (11.11%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (18.52%)
    '     File Size: 1.68 KB


    '     Class IGradFunction
    ' 
    '         Function: eval, (+2 Overloads) evaluate, in_place_gradient
    ' 
    '         Sub: (+2 Overloads) gradient
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Framework.Optimization.LBFGSB

    Public MustInherit Class IGradFunction

        Public Overridable Function evaluate(x As Double(), grad As Double()) As Double
            Return Double.NaN
        End Function

        Public Overridable Function evaluate(x As Double()) As Double
            Return Double.NaN
        End Function

        Public Sub gradient(x As Double(), grad As Double())
            gradient(x, grad, 0.0001)
        End Sub

        ''' <summary>
        ''' finite difference, symmetrical gradient, stores result in grad[]
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="grad"></param>
        ''' <param name="eps"></param>
        Public Sub gradient(ByRef x As Double(), ByRef grad As Double(), eps As Double)
            Dim n = grad.Length

            For i = 0 To n - 1
                Dim tmp = x(i)
                Dim x1 = tmp - eps
                Dim x2 = tmp + eps
                x(i) = x1
                Dim y1 = evaluate(x)
                x(i) = x2
                Dim y2 = evaluate(x)
                x(i) = tmp ' restore
                grad(i) = (y2 - y1) / (2.0 * eps)
            Next
        End Sub

        Public Overridable Function in_place_gradient() As Boolean
            Return False
        End Function

        Public Function eval(x As Double(), grad As Double()) As Double
            If in_place_gradient() Then
                Return evaluate(x, grad)
            Else
                gradient(x, grad)
                Return evaluate(x)
            End If
        End Function

    End Class

End Namespace
