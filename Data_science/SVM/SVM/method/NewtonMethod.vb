Imports System.Linq
Imports Microsoft.VisualBasic.DataMining.SVM.Model

Namespace Method

    ''' <summary>
    ''' @author Peter Grube
    ''' @author Ralf Wondratschek
    ''' </summary>
    Public Class NewtonMethod : Inherits Optimizer

        Private Const FIRST_DERIVATE_W1 As String = "firstDerivateW1"
        Private Const FIRST_DERIVATE_W2 As String = "firstDerivateW2"
        Private Const FIRST_DERIVATE_B As String = "firstDerivateB"
        Private Const SECOND_DERIVATE_W1 As String = "secondDerivateW1"
        Private Const SECOND_DERIVATE_W2 As String = "secondDerivateW2"
        Private Const SECOND_DERIVATE_B As String = "secondDerivateB"
        Private Const SECOND_DERIVATE_W1W2 As String = "secondDerivateW1W2"
        Private Const SECOND_DERIVATE_W1B As String = "secondDerivateW1B"
        Private Const SECOND_DERIVATE_W2B As String = "secondDerivateW2B"

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
            Dim derivation As New Dictionary(Of String, Double)

            For i As Integer = 0 To _iterations - 1
                If _cancelled Then
                    Return Nothing
                End If

                derivation(FIRST_DERIVATE_W1) = firstDerivateW1(argument.Offset, C, _points, argument.NormalVector)
                derivation(FIRST_DERIVATE_W2) = firstDerivateW2(argument.Offset, C, _points, argument.NormalVector)
                derivation(FIRST_DERIVATE_B) = firstDerivateB(argument.Offset, C, _points, argument.NormalVector)
                derivation(SECOND_DERIVATE_W1) = secondDerivateW1(argument.Offset, C, _points, argument.NormalVector)
                derivation(SECOND_DERIVATE_W2) = secondDerivateW2(argument.Offset, C, _points, argument.NormalVector)
                derivation(SECOND_DERIVATE_B) = secondDerivateB(argument.Offset, C, _points, argument.NormalVector)
                derivation(SECOND_DERIVATE_W1W2) = secondDerivateW2W1(argument.Offset, C, _points, argument.NormalVector)
                derivation(SECOND_DERIVATE_W1B) = secondDerivateW1B(argument.Offset, C, _points, argument.NormalVector)
                derivation(SECOND_DERIVATE_W2B) = secondDerivateW2B(argument.Offset, C, _points, argument.NormalVector)

                Dim newArg As SvmArgument = newtonMethod(argument, derivation)

                If [stop](argument, newArg) Then
                    Return newArg.ToLine()
                Else
                    argument = newArg
                End If
            Next

            Return argument.ToLine()
        End Function

        Private Shared Function invertHesse(dw1up2 As Double, dw2up2 As Double, dbup2 As Double, dw2db As Double, dw1db As Double, dw1dw2 As Double) As Double()()
            Dim h11 As Double = dw2up2 * dbup2 - dw2db * dw2db
            Dim h12 As Double = dw1db * dw2db - dw1dw2 * dbup2
            Dim h13 As Double = dw1dw2 * dw2db - dw1db * dw2up2
            Dim h21 As Double = h12
            Dim h22 As Double = dw1up2 * dbup2 - dw1db * dw1db
            Dim h23 As Double = dw1db * dw1dw2 - dw1up2 * dw2db
            Dim h31 As Double = h13
            Dim h32 As Double = h23
            Dim h33 As Double = dw1up2 * dw2up2 - dw1dw2 * dw1dw2
            Dim invHesse As Double()() = {
                {h11, h12, h13},
                {h21, h22, h23},
                {h31, h32, h33}
            }.RowIterator _
             .ToArray

            Dim det As Double = dw1up2 * dw2up2 * dbup2 + dw1dw2 * dw2db * dw1db + dw1db * dw1dw2 * dw2db - dw1db * dw2up2 * dw1db - dw2db * dw2db * dw1up2 - dbup2 * dw1dw2 * dw1dw2
            Dim detHesse As Double = 1 / det
            Dim ___invertHesse As Double()() = MAT(Of Double)(3, 3)

            For i As Integer = 0 To 2
                For j As Integer = 0 To 2
                    ___invertHesse(i)(j) = detHesse * invHesse(i)(j)
                Next
            Next

            Return ___invertHesse
        End Function

        Private Shared Function secondDerTimesfirstDer(matrix As Double()(), vector As Double()) As Double()

            Dim res As Double() = New Double(2) {}

            For i As Integer = 0 To 2
                res(i) = matrix(i)(0) * vector(0) + matrix(i)(1) * vector(1) + matrix(i)(2) * vector(2)
            Next

            Return res
        End Function

        Private Shared Function newtonMethod(normVecOffset As SvmArgument, derivates As Dictionary(Of String, Double)) As SvmArgument
            Dim vecOffs As SvmArgument = normVecOffset.Clone()

            Dim firstDerivates As Double() = {derivates(FIRST_DERIVATE_W1), derivates(FIRST_DERIVATE_W2), derivates(FIRST_DERIVATE_B)}
            Dim functionProduct As Double() = secondDerTimesfirstDer(invertHesse(derivates(SECOND_DERIVATE_W1), derivates(SECOND_DERIVATE_W2), derivates(SECOND_DERIVATE_B), derivates(SECOND_DERIVATE_W2B), derivates(SECOND_DERIVATE_W1B), derivates(SECOND_DERIVATE_W1W2)), firstDerivates)

            vecOffs.NormalVector = New NormalVector(normVecOffset.NormalVector.W1 - functionProduct(0), normVecOffset.NormalVector.W2 - functionProduct(1))
            vecOffs.Offset = normVecOffset.Offset - functionProduct(2)

            Return vecOffs
        End Function


        Private Shared Function calculateExponent(y As Double, w1 As Double, w2 As Double, x1 As Double, x2 As Double, b As Double) As Double
            Return Math.Exp((-1) * y * (w1 * x1 + w2 * x2 + b))
        End Function

        Private Shared Function firstDerivateW1(b As Double, c As Double, points As LabeledPoint(), vectorW As NormalVector) As Double
            Dim sum As Double = 0.0
            Dim zaehler As Double = 0.0
            Dim nenner As Double = 0.0
            Dim exp As Double = 0.0

            For i As Integer = 0 To points.Length - 1
                exp = calculateExponent(points(i).Y, vectorW.W1, vectorW.W2, points(i).X1, points(i).X2, b)
                zaehler = exp * ((-1) * points(i).Y * points(i).X1)
                nenner = 1 + exp
                sum += zaehler / nenner
            Next

            Return vectorW.W1 + c * sum
        End Function

        Private Shared Function firstDerivateW2(b As Double, c As Double, points As LabeledPoint(), vectorW As NormalVector) As Double
            Dim sum As Double = 0.0
            Dim zaehler As Double = 0.0
            Dim nenner As Double = 0.0
            Dim exp As Double = 0.0

            For i As Integer = 0 To points.Length - 1
                exp = calculateExponent(points(i).Y, vectorW.W1, vectorW.W2, points(i).X1, points(i).X2, b)
                zaehler = exp * ((-1) * points(i).Y * points(i).X2)
                nenner = 1 + exp
                sum += zaehler / nenner
            Next

            Return vectorW.W2 + c * sum
        End Function

        Private Shared Function firstDerivateB(b As Double, c As Double, points As LabeledPoint(), vectorW As NormalVector) As Double
            Dim sum As Double = 0.0
            Dim zaehler As Double = 0.0
            Dim nenner As Double = 0.0
            Dim exp As Double = 0.0

            For i As Integer = 0 To points.Length - 1
                exp = calculateExponent(points(i).Y, vectorW.W1, vectorW.W2, points(i).X1, points(i).X2, b)
                zaehler = exp * ((-1) * points(i).Y)
                nenner = 1 + exp
                sum += zaehler / nenner
            Next

            Return c * sum
        End Function

        Private Shared Function secondDerivateW1(b As Double, c As Double, points As LabeledPoint(), vectorW As NormalVector) As Double
            Dim sum As Double = 0.0
            Dim zaehler As Double = 0.0
            Dim nenner As Double = 0.0
            Dim exp As Double = 0.0

            For i As Integer = 0 To points.Length - 1
                exp = calculateExponent(points(i).Y, vectorW.W1, vectorW.W2, points(i).X1, points(i).X2, b)
                zaehler = Math.Pow(points(i).Y, 2) * Math.Pow(points(i).X1, 2) * exp
                nenner = Math.Pow(1 + exp, 2)
                sum += zaehler / nenner
            Next

            Return 1.0 + c * sum
        End Function

        Private Shared Function secondDerivateW2(b As Double, c As Double, points As LabeledPoint(), vectorW As NormalVector) As Double
            Dim sum As Double = 0.0
            Dim zaehler As Double = 0.0
            Dim nenner As Double = 0.0
            Dim exp As Double = 0.0

            For i As Integer = 0 To points.Length - 1
                exp = calculateExponent(points(i).Y, vectorW.W1, vectorW.W2, points(i).X1, points(i).X2, b)
                zaehler = Math.Pow(points(i).Y, 2) * Math.Pow(points(i).X2, 2) * exp
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
                exp = calculateExponent(points(i).Y, vectorW.W1, vectorW.W2, points(i).X1, points(i).X2, b)
                zaehler = Math.Pow(points(i).Y, 2) * points(i).X1 * points(i).X2 * exp
                nenner = Math.Pow(1 + exp, 2)
                sum += zaehler / nenner
            Next

            Return c * sum
        End Function

        Private Shared Function secondDerivateW2B(b As Double, c As Double, points As LabeledPoint(), vectorW As NormalVector) As Double
            Dim sum As Double = 0.0
            Dim zaehler As Double = 0.0
            Dim nenner As Double = 0.0
            Dim exp As Double = 0.0

            For i As Integer = 0 To points.Length - 1
                exp = calculateExponent(points(i).Y, vectorW.W1, vectorW.W2, points(i).X1, points(i).X2, b)
                zaehler = Math.Pow(points(i).Y, 2) * points(i).X2 * exp
                nenner = Math.Pow(1 + exp, 2)
                sum += zaehler / nenner
            Next

            Return c * sum
        End Function

        Private Shared Function secondDerivateW1B(b As Double, c As Double, points As LabeledPoint(), vectorW As NormalVector) As Double
            Dim sum As Double = 0.0
            Dim zaehler As Double = 0.0
            Dim nenner As Double = 0.0
            Dim exp As Double = 0.0

            For i As Integer = 0 To points.Length - 1
                exp = calculateExponent(points(i).Y, vectorW.W1, vectorW.W2, points(i).X1, points(i).X2, b)
                zaehler = Math.Pow(points(i).Y, 2) * points(i).X1 * exp
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
                exp = calculateExponent(points(i).Y, vectorW.W1, vectorW.W2, points(i).X1, points(i).X2, b)
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

