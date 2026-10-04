#Region "Microsoft.VisualBasic::505ecc8063b9f781e3d9b0b01f52dde3, Data_science\Mathematica\Math\Math\Algebra\Solvers\PowerMethod.vb"

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

    '   Total Lines: 119
    '    Code Lines: 94 (78.99%)
    ' Comment Lines: 7 (5.88%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 18 (15.13%)
    '     File Size: 3.99 KB


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
Imports SimdEngine = Microsoft.VisualBasic.Math.SIMD.SimdEngine
Imports SIMDIntrinsics = Microsoft.VisualBasic.Math.SIMD.SIMDIntrinsics
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
        ''' <summary>
        ''' 收敛阈值。旧默认值 0 会导致循环永远无法终止（浮点误差使得
        ''' difference() 始终大于 0），必须为一个有效的正值。
        ''' </summary>
        Private ReadOnly epsilon As Double = 0.00001
        ''' <summary>
        ''' 幂迭代次数上限，防止谱分布病态（如 |lambda1| = |lambda2|）时无限循环。
        ''' </summary>
        Private Const maxIterations As Integer = 100000
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
            Call System.Array.Copy(curX, prevX, n)
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

                If count > maxIterations Then
                    Throw New InvalidOperationException(
                        $"Power iteration does not converge after {maxIterations} iterations " &
                        "(possible that |lambda1| = |lambda2| in the spectrum).")
                End If
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

            ' 每一行与向量做 FMA 点积（matr 为 n x n，vector 长度为 n）
            For i = 0 To n - 1
                result(i) = SIMDIntrinsics.DotFma(matr(i), vector)
            Next

            Return result
        End Function


        Private Function multiply(vector As Double(), lambda As Double) As Double()
            Return SimdEngine.MultiplyScalar(Of Double)(lambda, vector)
        End Function

        Private Function minus(vectorA As Double(), vectorB As Double()) As Double()
            Return SimdEngine.Subtract(Of Double)(vectorA, vectorB)
        End Function

        Public Overridable Sub printVectorDiscrepancy()
            Console.WriteLine(vbLf & "Eigen vectors discrepancy" & vbLf)
            For Each item In minus(multiply(matrA, curX), multiply(curX, curLambda))
                Console.WriteLine(item)
            Next
        End Sub

        Private Function vectorsMultiply(first As Double(), second As Double()) As Double
            ' 准确度修正：旧实现在 For 循环体内手动 i += 1，只累加了偶数下标的
            ' 一半元素，点积结果错误。现改用 SIMD FMA 点积内核，一次性处理全部元素。
            Return SIMDIntrinsics.DotFma(first, second)
        End Function
    End Class
End Namespace
