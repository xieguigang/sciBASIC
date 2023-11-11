Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports std = System.Math

' Dim matrA = New Double()() {
'     New Double() {0.5757, -0.0758, 0.0152, 0.0303, 0.1061},
'     New Double() {0.0788, 0.9014, 0.0000, -0.0606, 0.0606},
'     New Double() {0.0455, 0.0000, 0.7242, -0.2121, 0.1212},
'     New Double() {-0.0909, 0.1909, 0.0000, 0.7121, -0.0303},
'     New Double() {0.3788, 0.0000, 0.1364, 0.0152, 0.8484}
' }

' Dim pm As PowerMethod = New PowerMethod(matrA)
' pm.powerMethod()
' pm.printDiscrepancy()
' pm.printVectorDiscrepancy()

Namespace LinearAlgebra.Solvers

    ''' <summary>
    ''' Method for finding eigenvectors
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/ValentinDutin/PowerIterationMethod
    ''' </remarks>
    Public Class PowerMethod
        Private matrA As Double()()
        Private curLambda As Double = 1
        Private prevLambda As Double = 0
        Private curX As Double()
        Private prevX As Double()
        Private n As Integer
        Private ReadOnly epsilon As Double = 0.00001
        Private count As Integer = 0
        Private polinom As Double() = New Double() {1, -3.1966884499998454, 3.7968475734968248, -2.0678062361747767, 0.50824834130184049, -0.044096040836169914}


        Friend Sub New(matrA As Double()())
            n = matrA.Length
            prevX = New Double(n - 1) {}
            curX = New Double(n - 1) {}

            Me.matrA = RectangularArray.Matrix(Of Double)(n, n)
            For i = 0 To n - 1
                For j = 0 To n - 1
                    Me.matrA(i)(j) = 0
                    For k = 0 To n - 1
                        Me.matrA(i)(j) += matrA(k)(i) * matrA(k)(j)
                    Next
                Next
                If i = 0 Then
                    prevX(i) = 1
                Else
                    prevX(i) = 0
                End If
            Next
        End Sub
        Private Sub newCurX()
            curX = multiply(matrA, prevX)
        End Sub

        Private Sub newCurLambda()
            curLambda = vectorsMultiply(curX, prevX) / vectorsMultiply(prevX, prevX)
        End Sub

        Private Sub newPrevX()
            For i = 0 To n - 1
                prevX(i) = curX(i)
            Next
        End Sub
        Private Sub newPrevLambda()
            prevLambda = curLambda
        End Sub

        Private Function difference() As Double
            Return std.Abs(curLambda - prevLambda)
        End Function

        Public Overridable Sub powerMethod()
            While difference() > epsilon
                If count > 0 Then
                    newPrevLambda()
                End If
                newCurX()
                newCurLambda()
                newPrevX()
                count += 1
            End While
            Console.WriteLine("count = " & count.ToString())
            Console.WriteLine("Lambda = " & curLambda.ToString())
            Console.WriteLine("vector X:")
            For Each item In curX
                Console.WriteLine(item)
            Next
        End Sub


        Public Overridable Sub printDiscrepancy()
            Dim discrepancy As Double = 0
            For j = 0 To n - 1
                discrepancy += std.Pow(curLambda, n - j) * polinom(j)
            Next
            discrepancy += polinom(n)
            Console.WriteLine("discrepancy for lambda = " & curLambda.ToString())
            Console.Write("{0,25}", discrepancy.ToString() & vbLf)
        End Sub


        Private Function multiply(matr As Double()(), vector As Double()) As Double()
            Dim result = New Double(n - 1) {}
            For i = 0 To n - 1
                result(i) = 0
                For j = 0 To n - 1
                    result(i) += matr(i)(j) * vector(j)
                Next
            Next
            Return result
        End Function


        Private Function multiply(vector As Double(), lambda As Double) As Double()
            Dim result = New Double(n - 1) {}
            For i = 0 To n - 1
                result(i) = vector(i) * lambda
            Next
            Return result
        End Function
        Private Function minus(vectorA As Double(), vectorB As Double()) As Double()
            Dim result = New Double(n - 1) {}
            For i = 0 To n - 1
                result(i) = vectorA(i) - vectorB(i)
            Next
            Return result
        End Function

        Public Overridable Sub printVectorDiscrepancy()
            Console.WriteLine(vbLf & "Eigen vectors discrepancy" & vbLf)
            For Each item In minus(multiply(matrA, curX), multiply(curX, curLambda))
                Console.WriteLine(item)
            Next
        End Sub



        Private Function vectorsMultiply(first As Double(), second As Double()) As Double
            Dim res As Double = 0
            For i = 0 To n - 1
                res += first(i) * second(i)
                i += 1
            Next
            Return res
        End Function
    End Class



End Namespace