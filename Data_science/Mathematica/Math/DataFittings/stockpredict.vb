Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

''' <summary>
''' Bayesian Curve Fitting
''' </summary>
Public Class StockPredict

    Private n As Integer
    Private M As Integer = 5
    Private alpha As Double = 0.005
    Private beta As Double = 11.1
    Private t() As Double

    Public ReadOnly Property predicted As Double
    Public ReadOnly Property variance As Double

    Public Sub New(t As Double(), Optional n As Integer = 10)
        Me.t = t
        Me.n = n

        Call regress(predicted, variance)
    End Sub

    Private Sub regress(ByRef predicted As Double, ByRef variance As Double)
        Dim x(n) As Double
        Dim a()() As Double = RectangularArray.Matrix(Of Double)(M + 1, 1)
        Dim b()() As Double = {New Double(M) {}}
        Dim s()() As Double
        Dim lt()() As Double = RectangularArray.Matrix(Of Double)(M + 1, 1)
        Dim predictprice()() As Double


        '---------------initialize the training data---------------

        For i = 0 To n
            x(i) = i + 1
        Next i


        '--------------calculate SUM-φ(xn)-------------------

        For i = 0 To n - 1
            For j = 0 To M
                a(j)(0) += x(i) ^ j
            Next j
        Next i

        Dim A_Conflict As New NumericMatrix(a)

        '-----------------initialize φ(x)T------------------

        For i = 0 To M
            b(0)(i) = x(n) ^ i
        Next i

        Dim B_Conflict As New NumericMatrix(b)

        '------------calculate the matrix S-------------

        Dim S_Conflict As NumericMatrix = DirectCast(A_Conflict.DotProduct(B_Conflict), NumericMatrix) * beta
        s = S_Conflict.Array

        For i = 0 To M
            For j = 0 To M
                If i = j Then
                    s(i)(j) += alpha ' + Alpha * I
                End If
            Next j
        Next i

        '------------calculate the inversion of matrix S-------------

        S_Conflict = S_Conflict.Inverse()

        '-------------last two parts multiply------------

        For i = 0 To n - 1
            For j = 0 To M
                lt(j)(0) += (x(i) ^ j) * t(i)
            Next j
        Next i


        Dim LT_Conflict As New NumericMatrix(lt)

        '-------------first two parts matrix multiply------------

        Dim FT As NumericMatrix = B_Conflict * S_Conflict

        '----------combine together--------------

        Dim PP As NumericMatrix = FT * LT_Conflict * beta
        predictprice = PP.Array

        variance = 1 / beta + (B_Conflict * S_Conflict * B_Conflict.Transpose())(0, 0)
        variance = std.Sqrt(variance)
        predicted = predictprice(0)(0)
    End Sub
End Class