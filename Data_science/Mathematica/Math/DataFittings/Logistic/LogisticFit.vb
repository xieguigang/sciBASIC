#Region "Microsoft.VisualBasic::d16d718703edc78a2cd0651f423fe6a1, Data_science\Mathematica\Math\DataFittings\Logistic\LogisticFit.vb"

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

    '   Total Lines: 43
    '    Code Lines: 36 (83.72%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (16.28%)
    '     File Size: 1.50 KB


    ' Class LogisticFit
    ' 
    '     Properties: ErrorTest, Polynomial, R2
    ' 
    '     Function: CreateFit, GetY
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Bootstrapping.Multivariate
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Logistic

    Public Class LogisticFit : Implements IFitted

        Public ReadOnly Property R2 As Double Implements IFitted.R2
            Get
                Return 1
            End Get
        End Property

        Public Property Polynomial As Formula Implements IFitted.Polynomial
        Public Property ErrorTest As IFitError() Implements IFitted.ErrorTest

        Public Function GetY(ParamArray x() As Double) As Double Implements IFitted.GetY
            Dim logit As Double = Polynomial.Factors _
                .Select(Function(wi, i) wi * x(i)) _
                .Sum
            Dim log As Double = Logistic.sigmoid(logit)

            Return log
        End Function

        Friend Shared Function CreateFit(log As Logistic, matrix As Instance()) As LogisticFit
            Dim weights As New Polynomial With {.Factors = log.theta.ToArray}
            Dim test As IFitError() = matrix _
                .Select(Function(i)
                            Return New [Error] With {
                                .X = i.x.AsVector,
                                .Y = i.label,
                                .Yfit = log.predict(i.x)
                            }
                        End Function) _
                .Select(Function(pi) DirectCast(pi, IFitError)) _
                .ToArray

            Return New LogisticFit With {
                .ErrorTest = test,
                .Polynomial = weights
            }
        End Function
    End Class
End Namespace