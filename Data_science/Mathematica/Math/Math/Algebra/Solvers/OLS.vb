Namespace LinearAlgebra.Solvers

    Public Module OLS

        ''' <summary>
        ''' OLS 最小二乘法
        ''' </summary>
        Public Function Solve(X As Double(,), y As Double(), nS As Integer, nP As Integer) As Double()
            ' X'X
            Dim XtX As Double(,) = New Double(nP - 1, nP - 1) {}
            For i = 0 To nP - 1
                For j = 0 To nP - 1
                    Dim sum As Double = 0
                    For k = 0 To nS - 1
                        sum += X(k, i) * X(k, j)
                    Next
                    XtX(i, j) = sum
                Next
            Next

            ' X'y
            Dim Xty As Double() = New Double(nP - 1) {}
            For i = 0 To nP - 1
                Dim sum As Double = 0
                For k = 0 To nS - 1
                    sum += X(k, i) * y(k)
                Next
                Xty(i) = sum
            Next

            ' 求逆
            Dim invXtX As Double(,) = StructureLearning.BnStructureLearner.MatrixInverse(XtX, nP)
            If invXtX Is Nothing Then
                Dim result As Double() = New Double(nP - 1) {}
                result(0) = y.Average()
                Return result
            End If

            ' β = (X'X)^(-1) X'y
            Dim beta As Double() = New Double(nP - 1) {}
            For i = 0 To nP - 1
                Dim sum As Double = 0
                For j = 0 To nP - 1
                    sum += invXtX(i, j) * Xty(j)
                Next
                beta(i) = sum
            Next

            Return beta
        End Function
    End Module
End Namespace