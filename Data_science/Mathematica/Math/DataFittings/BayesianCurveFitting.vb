Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Public Class BayesianCurveFitting
    Private ReadOnly M As Integer ' order of curve fitting
    Private ReadOnly N As Integer ' size of data
    Private x As Double()
    Private t As Double()
    Private ReadOnly alpha As Double = 0.005
    Private ReadOnly beta As Double = 11.1
    Private S As NumericMatrix
    Private I As NumericMatrix

    Public Sub New(x As Double(), t As Double(), m As Integer)
        Me.M = m
        N = x.Length
        Me.x = x
        Me.t = t
        I = CType(NumericMatrix.Identity(Me.M + 1, Me.M + 1), NumericMatrix)
        computeS()
    End Sub

    ' compute matrix S
    Private Sub computeS()
        Dim sum As NumericMatrix = New NumericMatrix(M + 1, M + 1)
        For i As Integer = 0 To N - 1
            Dim phi = getPhiX(x(i))
            sum = sum + (phi * phi.Transpose())
        Next
        sum = CType(sum * beta, NumericMatrix)
        S = CType(CType(I * alpha, NumericMatrix) + sum, NumericMatrix)
        S = CType(S.Inverse(), NumericMatrix)
    End Sub

    ' compute matrix phi(x)
    Private Function getPhiX(x As Double) As NumericMatrix
        Dim phiVal = New Double(M + 1 - 1) {}
        For i As Integer = 0 To M
            phiVal(i) = std.Pow(x, i)
        Next
        Dim phi As NumericMatrix = New NumericMatrix(phiVal, M + 1)
        Return phi
    End Function

    ' compute s2(x)
    Public Overridable Function getS2X(x As Double) As Double
        Dim s As NumericMatrix = CType(getPhiX(x).Transpose(), NumericMatrix) * Me.S * getPhiX(x)
        Dim sVal = s(0, 0)
        Dim s2x = 1 / beta + sVal
        Return s2x
    End Function

    ' compute m(x)
    Public Overridable Function getMx(x As Double) As Double
        Dim sum As NumericMatrix = New NumericMatrix(M + 1, 1)
        For i As Integer = 0 To N - 1
            Dim phi = getPhiX(Me.x(i))
            sum = CType(sum + phi * t(i), NumericMatrix)
        Next
        Dim mx As NumericMatrix = CType((CType(getPhiX(x).Transpose(), NumericMatrix) * beta), NumericMatrix)
        mx = mx * S
        mx = mx * sum

        Return mx(0, 0)
    End Function

End Class
