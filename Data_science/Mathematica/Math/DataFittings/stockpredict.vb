#Region "Microsoft.VisualBasic::56c2e9759fefbfc143fb7790ca12c59a, Data_science\Mathematica\Math\DataFittings\stockpredict.vb"

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

    '   Total Lines: 101
    '    Code Lines: 60
    ' Comment Lines: 11
    '   Blank Lines: 30
    '     File Size: 2.85 KB


    ' Class StockPredict
    ' 
    '     Properties: predicted, variance
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: regress
    ' 
    ' /********************************************************************************/

#End Region

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
