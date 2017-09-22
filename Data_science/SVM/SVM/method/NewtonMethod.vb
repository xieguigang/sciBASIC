#Region "Microsoft.VisualBasic::e1ef259813f61d6a191caaa12873baa9, ..\sciBASIC#\Data_science\SVM\SVM\method\NewtonMethod.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Linq
Imports Microsoft.VisualBasic.DataMining.SVM.Model
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Method

    ''' <summary>
    ''' @author Peter Grube
    ''' @author Ralf Wondratschek
    ''' </summary>
    Public Class NewtonMethod : Inherits Optimizer

        Private Const FIRST_DERIVATE_W As String = "firstDerivateW"
        Private Const FIRST_DERIVATE_B As String = "firstDerivateB"
        Private Const SECOND_DERIVATE_W As String = "secondDerivateW"
        Private Const SECOND_DERIVATE_B As String = "secondDerivateB"
        Private Const SECOND_DERIVATE_W_Product As String = "secondDerivateW1W2"
        Private Const SECOND_DERIVATE_WB As String = "secondDerivateWB"

        Private Const DEFAULT_ITERATIONS As Integer = 100
        Private Const STOP_DIFFERENCE As Double = 0.00000001

        Public Sub New(line As Line, points As IList(Of LabeledPoint))
            Me.New(line, points, DEFAULT_ITERATIONS)
        End Sub

        Public Sub New(line As Line, points As IList(Of LabeledPoint), iterations As Integer)
            MyBase.New(line, points, iterations)
        End Sub

        Protected Friend Overrides Function innerOptimize() As Line
            Dim argument As New SvmArgument(_line.NormalVector, _line.Offset)
            Dim derivation As New Dictionary(Of String, Vector)

            For i As Integer = 0 To _iterations - 1
                If _cancelled Then
                    Return Nothing
                End If

                derivation(FIRST_DERIVATE_W) = firstDerivateW(argument.Offset, C, _points, argument.NormalVector)
                derivation(FIRST_DERIVATE_B) = firstDerivateB(argument.Offset, C, _points, argument.NormalVector)
                derivation(SECOND_DERIVATE_W) = secondDerivateW(argument.Offset, C, _points, argument.NormalVector)
                derivation(SECOND_DERIVATE_B) = secondDerivateB(argument.Offset, C, _points, argument.NormalVector)
                derivation(SECOND_DERIVATE_W_Product) = secondDerivateW2W1(argument.Offset, C, _points, argument.NormalVector)
                derivation(SECOND_DERIVATE_WB) = secondDerivateWB(argument.Offset, C, _points, argument.NormalVector)

                Dim newArg As SvmArgument = newtonMethod(argument, derivation)

                If [stop](argument, newArg) Then
                    Return newArg.ToLine()
                Else
                    argument = newArg
                End If
            Next

            Return argument.ToLine()
        End Function

        Private Shared Function invertHesse(dwup As Vector, dbup2 As Double, dwdb As Vector, dw1dw2 As Double) As Double()()
            'Dim h11 As Vector = dwup * dbup2 - dwdb * dwdb
            'Dim h12 As Vector = dwdb * dwdb - dw1dw2 * dbup2
            'Dim h13 As Vector = dw1dw2 * dwdb - dwdb * dwup
            'Dim h21 As Vector = h12
            'Dim h22 As Double = dw1up2 * dbup2 - dw1db * dw1db
            'Dim h23 As Double = dw1db * dw1dw2 - dw1up2 * dw2db
            'Dim h31 As Double = h13
            'Dim h32 As Double = h23
            'Dim h33 As Double = dw1up2 * dw2up2 - dw1dw2 * dw1dw2
            'Dim invHesse As Double()() = {
            '    {h11, h12, h13},
            '    {h21, h22, h23},
            '    {h31, h32, h33}
            '}.RowIterator _
            ' .ToArray

            'Dim det As Double = dw1up2 * dw2up2 * dbup2 + dw1dw2 * dw2db * dw1db + dw1db * dw1dw2 * dw2db - dw1db * dw2up2 * dw1db - dw2db * dw2db * dw1up2 - dbup2 * dw1dw2 * dw1dw2
            'Dim detHesse As Double = 1 / det
            'Dim ___invertHesse As Double()() = MAT(Of Double)(3, 3)

            'For i As Integer = 0 To 2
            '    For j As Integer = 0 To 2
            '        ___invertHesse(i)(j) = detHesse * invHesse(i)(j)
            '    Next
            'Next

            'Return ___invertHesse
        End Function

        Private Shared Function secondDerTimesfirstDer(matrix As Double()(), vector As Double()) As Double()

            Dim res As Double() = New Double(2) {}

            For i As Integer = 0 To 2
                res(i) = matrix(i)(0) * vector(0) + matrix(i)(1) * vector(1) + matrix(i)(2) * vector(2)
            Next

            Return res
        End Function

        Private Shared Function newtonMethod(normVecOffset As SvmArgument, derivates As Dictionary(Of String, Vector)) As SvmArgument
            Dim vecOffs As SvmArgument = normVecOffset.Clone()

            Dim firstDerivates As Vector = derivates(FIRST_DERIVATE_W).Join(derivates(FIRST_DERIVATE_B)).AsVector
            Dim functionProduct As Double() = secondDerTimesfirstDer(
                invertHesse(derivates(SECOND_DERIVATE_W),
                            derivates(SECOND_DERIVATE_B)(0),
                            derivates(SECOND_DERIVATE_WB),
                            derivates(SECOND_DERIVATE_W_Product)(0)),
                firstDerivates)

            vecOffs.NormalVector = New NormalVector(
                {
                normVecOffset.NormalVector.W1 - functionProduct(0),
                normVecOffset.NormalVector.W2 - functionProduct(1)
            })
            vecOffs.Offset = normVecOffset.Offset - functionProduct(2)

            Return vecOffs
        End Function


        Private Shared Function calculateExponent(y As Double, w As Vector, x As Vector, b As Double) As Double
            Return Math.Exp((-1) * y * ((w * x).Sum + b))
        End Function

        Private Shared Function firstDerivateW(b As Double, c As Double, points As LabeledPoint(), vectorW As NormalVector) As Vector
            Dim sum As New Vector(vectorW.W.Dim)
            Dim zaehler As Vector
            Dim nenner As Vector
            Dim exp As Vector

            For i As Integer = 0 To points.Length - 1
                exp = calculateExponent(points(i).Y, vectorW.W, points(i).X, b)
                zaehler = exp * ((-1) * points(i).Y * points(i).X)
                nenner = 1 + exp
                sum += zaehler / nenner
            Next

            Return vectorW.W + c * sum
        End Function

        Private Shared Function firstDerivateB(b As Double, c As Double, points As LabeledPoint(), vectorW As NormalVector) As Double
            Dim sum As Double = 0.0
            Dim zaehler As Double = 0.0
            Dim nenner As Double = 0.0
            Dim exp As Double = 0.0

            For i As Integer = 0 To points.Length - 1
                exp = calculateExponent(points(i).Y, vectorW.W, points(i).X, b)
                zaehler = exp * ((-1) * points(i).Y)
                nenner = 1 + exp
                sum += zaehler / nenner
            Next

            Return c * sum
        End Function

        Private Shared Function secondDerivateW(b As Double, c As Double, points As LabeledPoint(), vectorW As NormalVector) As Vector
            Dim sum As New Vector(vectorW.W.Dim)
            Dim zaehler As Vector
            Dim nenner As Double = 0.0
            Dim exp As Double = 0.0

            For i As Integer = 0 To points.Length - 1
                exp = calculateExponent(points(i).Y, vectorW.W, points(i).X, b)
                zaehler = Math.Pow(points(i).Y, 2) * (points(i).X ^ 2) * exp
                nenner = Math.Pow(1 + exp, 2)
                sum += zaehler / nenner
            Next

            Return 1.0 + c * sum
        End Function

        Private Shared Function secondDerivateW2W1(b As Double, c As Double, points As LabeledPoint(), vectorW As NormalVector) As Double
            Dim sum As Double = 0.0
            Dim zaehler As Double = 0.0
            Dim nenner As Double = 0.0
            Dim exp As Double = 0.0

            For i As Integer = 0 To points.Length - 1
                exp = calculateExponent(points(i).Y, vectorW.W, points(i).X, b)
                zaehler = Math.Pow(points(i).Y, 2) * points(i).X.Product * exp
                nenner = Math.Pow(1 + exp, 2)
                sum += zaehler / nenner
            Next

            Return c * sum
        End Function

        Private Shared Function secondDerivateWB(b As Double, c As Double, points As LabeledPoint(), vectorW As NormalVector) As Vector
            Dim sum As New Vector(vectorW.W.Dim)
            Dim zaehler As Vector
            Dim nenner As Double = 0.0
            Dim exp As Double = 0.0

            For i As Integer = 0 To points.Length - 1
                exp = calculateExponent(points(i).Y, vectorW.W, points(i).X, b)
                zaehler = Math.Pow(points(i).Y, 2) * points(i).X * exp
                nenner = Math.Pow(1 + exp, 2)
                sum += zaehler / nenner
            Next

            Return c * sum
        End Function

        Private Shared Function secondDerivateB(b As Double, c As Double, points As LabeledPoint(), vectorW As NormalVector) As Double
            Dim sum As Double = 0.0
            Dim zaehler As Double = 0.0
            Dim nenner As Double = 0.0
            Dim exp As Double = 0.0

            For i As Integer = 0 To points.Length - 1
                exp = calculateExponent(points(i).Y, vectorW.W, points(i).X, b)
                zaehler = Math.Pow(points(i).Y, 2) * exp
                nenner = Math.Pow(1 + exp, 2)
                sum += zaehler / nenner
            Next

            Return c * sum
        End Function

        Private Function [stop](before As SvmArgument, after As SvmArgument) As Boolean
            Return Math.Abs(Math.Abs(before.NormalVector.W1) - Math.Abs(after.NormalVector.W1)) < STOP_DIFFERENCE AndAlso
                Math.Abs(Math.Abs(before.NormalVector.W2) - Math.Abs(after.NormalVector.W2)) < STOP_DIFFERENCE AndAlso
                Math.Abs(Math.Abs(before.Offset) - Math.Abs(after.Offset)) < STOP_DIFFERENCE
        End Function
    End Class

End Namespace
