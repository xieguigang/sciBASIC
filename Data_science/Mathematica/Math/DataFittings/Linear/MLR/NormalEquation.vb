Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace Multivariate

    Public Module NormalEquation

        ''' <summary>
        ''' 多元线性回归（最小二乘法）
        ''' β = (X'X)^(-1) X'y
        ''' </summary>
        Public Function LinearRegression(X As Double(,), y As Double(), nS As Integer, nP As Integer) As Double()
            Dim p1 As Integer = nP + 1  ' 含截距

            ' X'X
            Dim XtX As Double(,) = New Double(p1 - 1, p1 - 1) {}
            For i = 0 To p1 - 1
                For j = 0 To p1 - 1
                    Dim sum As Double = 0
                    For k = 0 To nS - 1
                        sum += X(k, i) * X(k, j)
                    Next
                    XtX(i, j) = sum
                Next
            Next

            ' X'y
            Dim Xty As Double() = New Double(p1 - 1) {}
            For i = 0 To p1 - 1
                Dim sum As Double = 0
                For k = 0 To nS - 1
                    sum += X(k, i) * y(k)
                Next
                Xty(i) = sum
            Next

            ' 求解 β = (X'X)^(-1) X'y
            Dim invXtX As Double(,) = MatrixOps.Inverse(XtX, strict:=True, throwSingularity:=False)

            If invXtX Is Nothing Then
                Return Nothing
            End If

            Dim beta As Double() = New Double(p1 - 1) {}
            For i = 0 To p1 - 1
                Dim sum As Double = 0
                For j = 0 To p1 - 1
                    sum += invXtX(i, j) * Xty(j)
                Next
                beta(i) = sum
            Next

            Return beta
        End Function
    End Module
End Namespace