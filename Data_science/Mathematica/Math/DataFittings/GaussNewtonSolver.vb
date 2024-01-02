Imports System.Drawing
Imports System.Text
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Public Class GaussNewtonSolver

    Public Delegate Function FitFunction(x As Double, args As NumericMatrix) As Double

    Dim m_fitFunction As FitFunction
    Dim m_trainingInfo As New StringBuilder

    ReadOnly rmseTolerance As Double
    ReadOnly iterationTolerance As Double
    ReadOnly [step] As Double = 0.001
    ReadOnly maxIterations As Integer

    Public ReadOnly Property TrainingInfo As StringBuilder
        Get
            Return m_trainingInfo
        End Get
    End Property

    Public Sub New(fitFunction As FitFunction,
                   Optional maxIterations As Integer = 1000,
                   Optional rmseTol As Double = 0.00000001,
                   Optional iterTol As Double = 0.000000000000001)

        rmseTolerance = rmseTol
        iterationTolerance = iterTol
        m_fitFunction = fitFunction
        Me.maxIterations = maxIterations
    End Sub

    Public Function Fit(data As DataPoint(), initGuesses As NumericMatrix) As NumericMatrix
        Dim beta As NumericMatrix = New NumericMatrix(initGuesses)
        Dim residuals = CalcResiduals(data, beta)
        Dim rB As New NumericMatrix(residuals)
        Dim rmse = residuals.RMS

        For i As Integer = 0 To maxIterations - 1
            Dim lJ = CalcJacobian(data, beta)
            Dim JT As NumericMatrix = lJ.Transpose()
            Dim bigJ = JT * lJ

            bigJ = bigJ.Inverse()
            bigJ *= JT
            beta -= bigJ * rB
            residuals = CalcResiduals(data, beta)

            For j = 0 To residuals.Length - 1
                rB(j, 0) = residuals(j)
            Next

            Dim temp = rmse
            rmse = residuals.RMS

            If std.Abs(temp - rmse) < iterationTolerance Then
                VBDebugger.EchoLine($"Convergence to a solution met, change in RMSE smaller than tolerance.")
                Exit For
            End If

            If rmse < rmseTolerance Then
                VBDebugger.EchoLine($"RMSE tolerance achieved on iteration {i + 1} of {maxIterations}.")
                VBDebugger.EchoLine($"Beta estimation: {beta}")
                Exit For
            End If
        Next

        Return beta
    End Function

    Private Function CalcJacobian(data As DataPoint(), beta As NumericMatrix) As NumericMatrix
        Dim betaSize As Size = beta.Dimension
        Dim Jacobian As New NumericMatrix(data.Length, betaSize.Height)
        Dim betaStep As New NumericMatrix(beta)

        For i = 0 To betaSize.Height - 1
            For j = 0 To betaSize.Height - 1
                betaStep(j, 0) = beta(j, 0)
                If i = j Then
                    betaStep(j, 0) += [step]
                End If
            Next

            For j = 0 To data.Length - 1
                Jacobian(j, i) = Me.CalcGradient(beta, betaStep, data(CInt(j)).X)
            Next
        Next

        Return Jacobian
    End Function

    Private Function CalcResiduals(data As DataPoint(), beta As NumericMatrix) As Double()
        Dim ri = New Double(data.Length - 1) {}

        For i = 0 To data.Length - 1
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
