Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
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

        For i As Integer = 0 To MAX_ITERATIONS - 1
            Dim lJ = CalcJacobian(data, beta)
            Dim JT As NumericMatrix = lJ.Transpose()
            Dim bigJ = JT * lJ

            bigJ = bigJ.Inverse(success, unsafe:=False)

            If Not success Then
                Exit For
            Else
                bigJ *= JT
                beta -= bigJ * rB
            End If

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
End Class
