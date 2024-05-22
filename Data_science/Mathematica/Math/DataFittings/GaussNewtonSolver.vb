#Region "Microsoft.VisualBasic::7c569e8c44602644e35481210f9f323f, Data_science\Mathematica\Math\DataFittings\GaussNewtonSolver.vb"

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

    '   Total Lines: 264
    '    Code Lines: 189 (71.59%)
    ' Comment Lines: 24 (9.09%)
    '    - Xml Docs: 58.33%
    ' 
    '   Blank Lines: 51 (19.32%)
    '     File Size: 8.50 KB


    ' Class GaussNewtonSolver
    ' 
    ' 
    '     Delegate Function
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CalcGradient, CalcJacobian, CalcResiduals, (+3 Overloads) Fit, Invert
    '                   LUPDecompose
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.Scripting
Imports std = System.Math

''' <summary>
''' least squares fitting for general curve functions
''' </summary>
Public Class GaussNewtonSolver

    ''' <summary>
    ''' A general curve function to fit
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="args"></param>
    ''' <returns></returns>
    Public Delegate Function FitFunction(x As Double, args As NumericMatrix) As Double

    ReadOnly m_fitFunction As FitFunction
    ReadOnly rmseTolerance As Double
    ReadOnly iterationTolerance As Double
    ReadOnly [step] As Double = 0.001
    ReadOnly eps As Double = 0.001

    ReadOnly MAX_ITERATIONS As Integer

    Public Sub New(fitFunction As FitFunction,
                   Optional maxIterations As Integer = 1000,
                   Optional rmseTol As Double = 0.00000001,
                   Optional iterTol As Double = 0.000000000000001)

        rmseTolerance = rmseTol
        iterationTolerance = iterTol
        m_fitFunction = fitFunction
        MAX_ITERATIONS = maxIterations
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Fit(data As DataPoint(), argumentSize As Integer) As Double()
        Return Fit(data, Vector.rand(0, 1, argumentSize).ToArray)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Fit(data As DataPoint(), ParamArray args As Double()) As Double()
        Return Fit(data, beta:=New NumericMatrix(args))
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="beta">should be a column vector</param>
    ''' <returns>the function argument values</returns>
    Private Function Fit(data As DataPoint(), beta As NumericMatrix) As Double()
        Dim residuals = CalcResiduals(data, beta)
        Dim rB As New NumericMatrix(residuals)
        Dim rmse = residuals.RMS
        Dim temp_rmse As Double
        Dim success As Boolean = False
        Dim truncate As Double = 100
        Dim truncate2 As Double = 5

        For i As Integer = 0 To MAX_ITERATIONS - 1
            Dim lJ = CalcJacobian(data, beta)
            Dim JT As NumericMatrix = lJ.Transpose()
            Dim bigJ = JT * lJ

            ' the internal LU decomposition has bug
            ' bigJ = bigJ.Inverse()
            ' use the LU decompositon function inside current module
            Invert(bigJ, tol:=0, truncate:=truncate).Set(success, bigJ)

            If Not success Then
                Exit For
            Else
                bigJ *= JT
                beta -= bigJ * rB
            End If

            For offset As Integer = 0 To beta.RowDimension - 1
                If beta(offset, 0) > truncate2 Then
                    beta(offset, 0) = truncate2
                ElseIf beta(offset, 0) < -truncate2 Then
                    beta(offset, 0) = eps
                End If
            Next

            residuals = CalcResiduals(data, beta)

            For j As Integer = 0 To residuals.Length - 1
                rB(j, 0) = residuals(j)
            Next

            temp_rmse = rmse
            rmse = residuals.RMS

            ' early stop, rmse no more changes
            ' can not convergence?
            If std.Abs(temp_rmse - rmse) < iterationTolerance Then
                Exit For
            End If

            ' early stop
            If rmse < rmseTolerance Then
                Exit For
            End If
        Next

        Return beta.IteratesALL.ToArray
    End Function

    Private Function CalcJacobian(data As DataPoint(), beta As NumericMatrix) As NumericMatrix
        Dim betaSize As Size = beta.Dimension
        Dim Jacobian As New NumericMatrix(data.Length, betaSize.Height)
        Dim betaStep As New NumericMatrix(beta)

        For i As Integer = 0 To betaSize.Height - 1
            For j As Integer = 0 To betaSize.Height - 1
                betaStep(j, 0) = beta(j, 0)
                If i = j Then
                    betaStep(j, 0) += [step]
                End If
            Next

            For j As Integer = 0 To data.Length - 1
                Jacobian(j, i) = Me.CalcGradient(beta, betaStep, data(CInt(j)).X)
            Next
        Next

        Return Jacobian
    End Function

    Private Function CalcResiduals(data As DataPoint(), beta As NumericMatrix) As Double()
        Dim ri = New Double(data.Length - 1) {}

        For i As Integer = 0 To data.Length - 1
            ri(i) = data(i).Y - m_fitFunction(data(CInt(i)).X, beta)
        Next

        Return ri
    End Function

    Private Function CalcGradient(beta As NumericMatrix, betaStep As NumericMatrix, x As Double) As Double
        Dim yZero = m_fitFunction(x, beta)
        Dim y = m_fitFunction(x, betaStep)

        Return (yZero - y) / [step]
    End Function

    Public Function LUPDecompose(m As NumericMatrix, Optional tol As Double = 0.0001) As (Boolean, NumericMatrix, Integer(), Integer)
        Dim N = m.RowDimension
        Dim S = 0
        Dim P = New Integer(N - 1) {}

        For i As Integer = 0 To N - 1
            P(i) = i
        Next

        Dim LU As NumericMatrix = New NumericMatrix(m)

        Dim uMax, absA As Double
        Dim iMax As Integer

        For i As Integer = 0 To N - 1
            uMax = 0.0
            iMax = i

            ' find max pivot row
            For k As Integer = i To N - 1
                absA = std.Abs(LU(k, i))
                If absA > uMax Then
                    uMax = absA
                    iMax = k
                End If
            Next

            ' check for degeneracy
            If uMax < tol Then
                Return (False, Nothing, Nothing, -1)
            End If

            ' pivot if necessary
            If iMax <> i Then
                Dim v_temp = LU.Array(i)
                LU.Array(i) = LU.Array(iMax)
                LU.Array(iMax) = v_temp

                Dim temp = P(i)
                P(i) = P(iMax)
                P(iMax) = temp
                S += 1
            End If

            For j As Integer = i + 1 To N - 1
                If LU(j, i) <> 0.0 Then
                    If LU(i, i) <> 0.0 Then
                        LU(j, i) = LU(j, i) / LU(i, i)
                    Else
                        LU(j, i) = LU(j, i) / eps
                    End If
                End If

                For k = i + 1 To N - 1
                    LU(j, k) -= LU(j, i) * LU(i, k)
                Next

            Next
        Next

        Return (True, LU, P, S)
    End Function

    Public Function Invert(m As NumericMatrix, Optional tol As Double = 0.0001, Optional truncate As Double = 10000) As (Boolean, NumericMatrix)
        Dim success As Boolean = Nothing,
            lu As NumericMatrix = Nothing,
            p As Integer() = Nothing,
            S As Integer = Nothing

        LUPDecompose(m, tol).Set(success, lu, p, S)

        If Not success Then
            Return (False, Nothing)
        End If

        Dim N = m.RowDimension
        Dim IA As New NumericMatrix(N, N)

        For j = 0 To N - 1
            For i = 0 To N - 1
                IA(i, j) = If(p(i) = j, 1.0, 0.0)

                For k = 0 To i - 1
                    IA(i, j) -= lu(i, k) * IA(k, j)
                Next
            Next

            For i = N - 1 To 0 Step -1
                For k = i + 1 To N - 1
                    IA(i, j) -= lu(i, k) * IA(k, j)
                Next

                If IA(i, j) <> 0.0 Then
                    If lu(i, i) <> 0.0 Then
                        IA(i, j) /= lu(i, i)
                    Else
                        IA(i, j) /= eps
                    End If

                    If IA(i, j) > truncate Then
                        IA(i, j) = truncate
                    ElseIf IA(i, j) < -truncate Then
                        IA(i, j) = -truncate
                    End If
                End If
            Next
        Next

        Return (True, IA)
    End Function
End Class
