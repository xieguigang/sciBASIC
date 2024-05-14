#Region "Microsoft.VisualBasic::7643f29f48b09ae0824144c0702c9065, Data_science\Mathematica\Math\DataFittings\Linear\MLR\Error.vb"

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

    '   Total Lines: 32
    '    Code Lines: 25
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 1.05 KB


    '     Structure [Error]
    ' 
    '         Properties: X, Y, Yfit
    ' 
    '         Function: RunTest, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports stdNum = System.Math

Namespace Multivariate

    Public Structure [Error] : Implements IFitError

        Public Property X As Vector
        Public Property Y As Double Implements IFitError.Y
        Public Property Yfit As Double Implements IFitError.Yfit

        Public Overrides Function ToString() As String
            Return $"{stdNum.Abs(Y - Yfit)} = |{Y} - {Yfit}|"
        End Function

        Public Shared Iterator Function RunTest(MLR As MLRFit, X As GeneralMatrix, Y As Vector) As IEnumerable(Of [Error])
            For Each xi In X.RowVectors.SeqIterator
                Dim yi = Y.Item(index:=xi)
                Dim yfit = MLR.Fx(xi)

                Yield New [Error] With {
                    .X = xi,
                    .Y = yi,
                    .Yfit = yfit
                }
            Next
        End Function
    End Structure

End Namespace
