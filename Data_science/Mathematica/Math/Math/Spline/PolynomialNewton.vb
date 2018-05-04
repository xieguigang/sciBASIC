Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Interpolation

    Public Module PolynomialNewton

        ''' <summary>
        ''' 使用牛顿多项式插值计算出给定的X点<paramref name="x1"/>所对应的Y值
        ''' <paramref name="x"/>和<paramref name="y"/>是已知的实验值点
        ''' </summary>
        ''' <param name="x1#"></param>
        ''' <param name="x#"></param>
        ''' <param name="y#"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' <paramref name="x"/> and <paramref name="y"/> should be length equals
        ''' </remarks>
        ''' 
        <Extension>
        Public Function Newton(x1 As Vector, x#(), y#()) As Vector
            Dim result As New Vector(y(0), x1.Length)
            Dim node As New Vector(x1.Dim)
            Dim denominator# = 1.0
            Dim n% = x.Length

            If x.Length <> y.Length OrElse x.Length = 0 Then
                Throw New ArgumentException("Invalid vector length value!")
            End If

            For i As Integer = 1 To n - 1
                node *= 0.0

                For j As Integer = 0 To i

                    ' f[x_0,…,x_n ]
                    denominator = 1.0

                    For k As Integer = 0 To i
                        If k <> j Then
                            denominator *= (x(j) - x(k))
                        End If
                    Next

                    node += y(j) / denominator
                Next

                For k As Integer = 0 To i - 1
                    ' (x-x_0) … (x-x_(i-1) )
                    node *= (x1 - x(k))
                Next

                result += node
            Next

            Return result
        End Function
    End Module
End Namespace