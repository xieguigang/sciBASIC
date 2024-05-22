#Region "Microsoft.VisualBasic::b14575111acb769e54ce30377d1c624d, Data_science\MachineLearning\t-SNE\CostFunction.vb"

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

    '   Total Lines: 88
    '    Code Lines: 59 (67.05%)
    ' Comment Lines: 8 (9.09%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 21 (23.86%)
    '     File Size: 2.37 KB


    ' Class CostFunction
    ' 
    '     Properties: mN
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: CostGrad
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

Friend Class CostFunction

    ReadOnly tSNE As tSNE

    Public ReadOnly Property mN As Integer
        Get
            Return tSNE.mN
        End Get
    End Property

    Sub New(tSNE As tSNE)
        Me.tSNE = tSNE
    End Sub

    ''' <summary>
    ''' return cost and gradient, given an arrangement
    ''' </summary>
    ''' <param name="Y"></param>
    Public Sub CostGrad(Y As Double()())
        Dim N = mN
        Dim [dim] = tSNE.mDim ' dim of output space
        Dim P = tSNE.mP
        Dim pmul = If(tSNE.mIter < 100, 4, 1) ' trick that helps with local optima

        ' compute current Q distribution, unnormalized first
        Dim lQu = zeros(N * N)
        Dim qsum = 0.0

        For i = 0 To N - 1

            For j = i + 1 To N - 1
                Dim dsum = 0.0

                For d = 0 To [dim] - 1
                    Dim dhere = Y(i)(d) - Y(j)(d)
                    dsum += dhere * dhere
                Next

                ' Student t-distribution
                Dim qu = 1.0 / (1.0 + dsum)
                lQu(i * N + j) = qu
                lQu(j * N + i) = qu
                qsum += 2 * qu
            Next
        Next

        ' normalize Q distribution to sum to 1
        Dim NN = N * N
        Dim lQ = zeros(NN)

        For q = 0 To NN - 1
            lQ(q) = stdNum.Max(lQu(q) / qsum, 1.0E-100)
        Next

        Dim cost = 0.0
        Dim grad As List(Of Double()) = New List(Of Double())()

        For i = 0 To N - 1
            Dim gsum = New Double([dim] - 1) {} ' init grad for point i

            For d = 0 To [dim] - 1
                gsum(d) = 0.0
            Next

            For j = 0 To N - 1
                ' accumulate cost (the non-constant portion at least...)
                cost += -P(i * N + j) * stdNum.Log(lQ(i * N + j))
                Dim premult = 4 * (pmul * P(i * N + j) - lQ(i * N + j)) * lQu(i * N + j)

                For d = 0 To [dim] - 1
                    gsum(d) += premult * (Y(i)(d) - Y(j)(d))
                Next
            Next

            grad.Add(gsum)
        Next

        tSNE.mCost = cost
        tSNE.mGrad = New Double(grad.Count - 1)() {}

        For i = 0 To grad.Count - 1
            tSNE.mGrad(i) = grad(i)
        Next
    End Sub

End Class
