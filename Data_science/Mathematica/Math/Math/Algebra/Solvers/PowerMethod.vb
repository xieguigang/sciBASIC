#Region "Microsoft.VisualBasic::42db24606205dc81dd7e9a8fdc15b37d, Data_science\Mathematica\Math\Math\Algebra\Solvers\PowerMethod.vb"

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

    '   Total Lines: 126
    '    Code Lines: 105 (83.33%)
    ' Comment Lines: 6 (4.76%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (11.90%)
    '     File Size: 4.07 KB


    '     Class PowerMethod
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: difference, minus, (+2 Overloads) multiply, vectorsMultiply
    ' 
    '         Sub: newCurLambda, newCurX, newPrevLambda, newPrevX, powerMethod
    '              printVectorDiscrepancy
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports std = System.Math

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
        Private ReadOnly epsilon As Double = 0 '.00001
        Private count As Integer = 0


        Sub New(matrA As Double()())
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
