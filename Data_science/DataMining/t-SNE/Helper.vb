#Region "Microsoft.VisualBasic::036d324dfaecf8bfb223698502597b78, Data_science\MachineLearning\t-SNE\Helper.vb"

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

    '   Total Lines: 164
    '    Code Lines: 97 (59.15%)
    ' Comment Lines: 36 (21.95%)
    '    - Xml Docs: 63.89%
    ' 
    '   Blank Lines: 31 (18.90%)
    '     File Size: 5.24 KB


    ' Module Helper
    ' 
    '     Function: d2p, L2, xtod, zeros
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Module Helper

    ''' <summary>
    ''' utilitity that creates contiguous vector of zeros of size n
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    Friend Function zeros(n As Integer) As Double()
        Dim arr = New Double(n - 1) {}

        For i = 0 To n - 1
            arr(i) = 0
        Next

        Return arr
    End Function

    ''' <summary>
    ''' compute L2 distance between two vectors
    ''' </summary>
    ''' <param name="x1"></param>
    ''' <param name="x2"></param>
    ''' <returns></returns>
    Private Function L2(x1 As Double(), x2 As Double()) As Double
        Dim N = x1.Length
        Dim d As Double = 0

        For i = 0 To N - 1
            Dim x1i = x1(i)
            Dim x2i = x2(i)
            d += (x1i - x2i) * (x1i - x2i)
        Next

        Return d
    End Function

    ''' <summary>
    ''' compute pairwise distance in all vectors in X
    ''' </summary>
    ''' <param name="X"></param>
    ''' <returns></returns>
    Friend Function xtod(X As Double()()) As Double()
        Dim N As Integer = X.Length
        Dim dist As Double() = zeros(N * N) ' allocate contiguous array

        For i As Integer = 0 To N - 1
            For j As Integer = i + 1 To N - 1
                Dim d = L2(X(i), X(j))

                dist(i * N + j) = d
                dist(j * N + i) = d
            Next
        Next

        Return dist
    End Function

    ''' <summary>
    ''' compute (p_{i|j} + p_{j|i})/(2n)
    ''' </summary>
    ''' <param name="D">distance matrix</param>
    ''' <param name="perplexity"></param>
    ''' <param name="tol"></param>
    ''' <returns></returns>
    Friend Function d2p(D As Double(), perplexity As Double, tol As Double) As Double()
        Dim Nf = std.Sqrt(D.Length) ' this better be an integer
        Dim N As Integer = std.Floor(Nf)
        Dim Htarget = std.Log(perplexity) ' target entropy of distribution
        Dim P = zeros(N * N) ' temporary probability matrix
        Dim prow = zeros(N) ' a temporary storage compartment

        For i = 0 To N - 1
            Dim betamin = Double.NegativeInfinity
            Dim betamax = Double.PositiveInfinity
            Dim beta As Double = 1 ' initial value of precision
            Dim done = False
            Dim maxtries = 50

            ' perform binary search to find a suitable precision beta
            ' so that the entropy of the distribution is appropriate
            Dim num = 0

            While Not done
                'debugger;

                ' compute entropy and kernel row with beta precision
                Dim psum = 0.0

                For j = 0 To N - 1
                    Dim pj = std.Exp(-D(i * N + j) * beta)
                    If i = j Then pj = 0 ' we dont care about diagonals
                    prow(j) = pj
                    psum += pj
                Next

                ' normalize p and compute entropy
                Dim Hhere = 0.0

                For j = 0 To N - 1
                    Dim pj = prow(j) / psum

                    prow(j) = pj

                    If pj > 0.0000001 Then
                        Hhere -= pj * std.Log(pj)
                    End If
                Next

                ' adjust beta based on result
                If Hhere > Htarget Then
                    ' entropy was too high (distribution too diffuse)
                    ' so we need to increase the precision for more peaky distribution
                    betamin = beta ' move up the bounds

                    If betamax = Double.PositiveInfinity Then
                        beta = beta * 2
                    Else
                        beta = (beta + betamax) / 2
                    End If
                Else
                    ' converse case. make distrubtion less peaky
                    betamax = beta

                    If betamin = Double.NegativeInfinity Then
                        beta = beta / 2
                    Else
                        beta = (beta + betamin) / 2
                    End If
                End If

                ' stopping conditions: too many tries or got a good precision
                num += 1

                If std.Abs(Hhere - Htarget) < tol Then
                    done = True
                End If

                If num >= maxtries Then
                    done = True
                End If
            End While

            ' console.log('data point ' + i + ' gets precision ' + beta + ' after ' + num + ' binary search steps.');
            ' copy over the final prow to P at row i
            For j = 0 To N - 1
                P(i * N + j) = prow(j)
            Next
        Next ' end loop over examples i

        ' symmetrize P and normalize it to sum to 1 over all ij
        Dim Pout = zeros(N * N)
        Dim N2 = N * 2

        For i = 0 To N - 1
            For j = 0 To N - 1
                Pout(i * N + j) = std.Max((P(i * N + j) + P(j * N + i)) / N2, 1.0E-100)
            Next
        Next

        Return Pout
    End Function
End Module
