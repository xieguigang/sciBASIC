#Region "Microsoft.VisualBasic::ee3f0586ee30cf9c7c073f9bf8832c35, Data_science\Mathematica\Math\DataFittings\Linear\Weighted\WeightedLinearRegression.vb"

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

    '   Total Lines: 238
    '    Code Lines: 191
    ' Comment Lines: 29
    '   Blank Lines: 18
    '     File Size: 7.76 KB


    ' Module WeightedLinearRegression
    ' 
    '     Function: (+3 Overloads) Regress, SymmetricMatrixInvert, XVector
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports stdNum = System.Math

''' <summary>
''' ## An Algorithm for Weighted Linear Regression
''' 
''' > https://www.codeproject.com/Articles/25335/An-Algorithm-for-Weighted-Linear-Regression
''' </summary>
Public Module WeightedLinearRegression

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Regress(X As Vector, Y As Vector, W As Vector, Optional orderOfPolynomial% = 1) As WeightedFit
        Return Regress(X.ToArray, Y.ToArray, W.ToArray, orderOfPolynomial)
    End Function

    Public Function Regress(X#(), Y#(), W#(), Optional orderOfPolynomial% = 2) As WeightedFit
        Dim Xmatrix#(,) = New Double(orderOfPolynomial, X.Length - 1) {}
        Dim term#
        Dim xx#

        For i As Integer = 0 To X.Length - 1
            Xmatrix(0, i) = 1
            term = X(i)
            xx = term

            For j As Integer = 1 To orderOfPolynomial
                Xmatrix(j, i) = term
                term *= xx
            Next
        Next

        Return Regress(Y, Xmatrix, W)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="M">
    ''' i=1: X vector
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Private Function XVector(M As Double(,)) As Double()
        Dim i% = 1
        Dim X As New List(Of Double)

        For j As Integer = 0 To M.GetUpperBound(1)
            X.Add(M(i, j))
        Next

        Return X.ToArray
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Y">Y[j]   = j-th observed data point</param>
    ''' <param name="X">X[i,j] = j-th value of the i-th independent varialble</param>
    ''' <param name="W">W[j]   = j-th weight value</param>
    ''' <returns></returns>
    Public Function Regress(Y#(), X#(,), W#()) As WeightedFit
        Dim M As Integer = Y.Length     ' M = Number of data points
        Dim N As Integer = X.Length \ M ' N = Number of linear terms
        Dim NDF As Integer = M - N      ' Degrees of freedom
        Dim Ycalc = New Double(M - 1) {}
        Dim DY = New Double(M - 1) {}

        ' If not enough data, don't attempt regression
        If NDF < 1 Then
            Return Nothing
        End If
        
        Dim V = New Double(N - 1, N - 1) {}
        Dim C = New Double(N - 1) {}
        Dim SEC = New Double(N - 1) {}
        Dim B As Double() = New Double(N - 1) {}
        ' Vector for LSQ
        ' Clear the matrices to start out
        For i As Integer = 0 To N - 1
            For j As Integer = 0 To N - 1
                V(i, j) = 0
            Next
        Next

        ' Form Least Squares Matrix
        For i As Integer = 0 To N - 1
            For j As Integer = 0 To N - 1
                V(i, j) = 0
                For k As Integer = 0 To M - 1
                    V(i, j) = V(i, j) + W(k) * X(i, k) * X(j, k)
                Next
            Next
            B(i) = 0
            For k As Integer = 0 To M - 1
                B(i) = B(i) + W(k) * X(i, k) * Y(k)
            Next
        Next
        ' V now contains the raw least squares matrix
        If Not SymmetricMatrixInvert(V) Then
            Return Nothing
        End If
        ' V now contains the inverted least square matrix
        ' Matrix multpily to get coefficients C = VB
        For i As Integer = 0 To N - 1
            C(i) = 0
            For j As Integer = 0 To N - 1
                C(i) = C(i) + V(i, j) * B(j)
            Next
        Next

        ' Calculate statistics
        Dim TSS As Double = 0
        Dim RSS As Double = 0
        Dim YBAR As Double = 0
        Dim WSUM As Double = 0
        For k As Integer = 0 To M - 1
            YBAR = YBAR + W(k) * Y(k)
            WSUM = WSUM + W(k)
        Next
        YBAR = YBAR / WSUM
        For k As Integer = 0 To M - 1
            Ycalc(k) = 0
            For i As Integer = 0 To N - 1
                Ycalc(k) = Ycalc(k) + C(i) * X(i, k)
            Next
            DY(k) = Ycalc(k) - Y(k)
            TSS = TSS + W(k) * (Y(k) - YBAR) * (Y(k) - YBAR)
            RSS = RSS + W(k) * DY(k) * DY(k)
        Next
        Dim SSQ As Double = RSS / NDF
        Dim RYSQ# = 1 - RSS / TSS
        Dim FReg# = 9999999
        If RYSQ < 0.9999999 Then
            FReg = RYSQ / (1 - RYSQ) * NDF / (N - 1)
        End If
        Dim SDV = stdNum.Sqrt(SSQ)

        ' Calculate var-covar matrix and std error of coefficients
        For i As Integer = 0 To N - 1
            For j As Integer = 0 To N - 1
                V(i, j) = V(i, j) * SSQ
            Next
            SEC(i) = stdNum.Sqrt(V(i, i))
        Next

        Return New WeightedFit With {
            .ErrorTest = X.XVector _
                .Select(Function(xi, i)
                            Return New TestPoint With {
                                .X = xi,
                                .Y = Y(i),
                                .Yfit = Ycalc(i)
                            }
                        End Function) _
                .Select(Function(p) DirectCast(p, IFitError)) _
                .ToArray,
            .Polynomial = New Polynomial With {
                .Factors = C
            },
            .CoefficientsStandardError = SEC,
            .CorrelationCoefficient = RYSQ,
            .FisherF = FReg,
            .Residuals = DY,
            .StandardDeviation = SDV,
            .VarianceMatrix = V
        }
    End Function

    Public Function SymmetricMatrixInvert(V As Double(,)) As Boolean
        Dim N As Integer = CInt(stdNum.Truncate(stdNum.Sqrt(V.Length)))
        Dim t As Double() = New Double(N - 1) {}
        Dim Q As Double() = New Double(N - 1) {}
        Dim R As Double() = New Double(N - 1) {}
        Dim AB As Double
        Dim K As Integer, L As Integer, M As Integer

        ' Invert a symetric matrix in V
        For M = 0 To N - 1
            R(M) = 1
        Next
        K = 0
        For M = 0 To N - 1
            Dim Big As Double = 0
            For L = 0 To N - 1
                AB = stdNum.Abs(V(L, L))
                If (AB > Big) AndAlso (R(L) <> 0) Then
                    Big = AB
                    K = L
                End If
            Next
            If Big = 0 Then
                Return False
            End If
            R(K) = 0
            Q(K) = 1 / V(K, K)
            t(K) = 1
            V(K, K) = 0
            If K <> 0 Then
                For L = 0 To K - 1
                    t(L) = V(L, K)
                    If R(L) = 0 Then
                        Q(L) = V(L, K) * Q(K)
                    Else
                        Q(L) = -V(L, K) * Q(K)
                    End If
                    V(L, K) = 0
                Next
            End If
            If (K + 1) < N Then
                For L = K + 1 To N - 1
                    If R(L) <> 0 Then
                        t(L) = V(K, L)
                    Else
                        t(L) = -V(K, L)
                    End If
                    Q(L) = -V(K, L) * Q(K)
                    V(K, L) = 0
                Next
            End If
            For L = 0 To N - 1
                For K = L To N - 1
                    V(L, K) = V(L, K) + t(L) * Q(K)
                Next
            Next
        Next
        M = N
        L = N - 1
        For K = 1 To N - 1
            M = M - 1
            L = L - 1
            For J As Integer = 0 To L
                V(M, J) = V(J, M)
            Next
        Next
        Return True
    End Function
End Module
