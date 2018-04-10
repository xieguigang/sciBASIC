''' <summary>
''' ## An Algorithm for Weighted Linear Regression
''' 
''' > https://www.codeproject.com/Articles/25335/An-Algorithm-for-Weighted-Linear-Regression
''' </summary>
Public Module WeightedLinearRegression

    Public Function Regress(Y As Double(), X As Double(,), W As Double()) As WeightedFit
        Dim M As Integer = Y.Length
        ' M = Number of data points
        Dim N As Integer = X.Length \ M
        ' N = Number of linear terms
        Dim NDF As Integer = M - N
        ' Degrees of freedom
        Dim Ycalc = New Double(M - 1) {}
        Dim DY = New Double(M - 1) {}
        ' If not enough data, don't attempt regression
        If NDF < 1 Then
            Return Nothing
        End If
        Dim V = New Double(N - 1, N - 1) {}
        Dim C = New Double(N - 1) {}
        Dim SEC = New Double(N - 1) {}
        Dim B As Double() = New Double(N - 1) {}
        ' Vector for LSQ
        ' Clear the matrices to start out
        For i As Integer = 0 To N - 1
            For j As Integer = 0 To N - 1
                V(i, j) = 0
            Next
        Next

        ' Form Least Squares Matrix
        For i As Integer = 0 To N - 1
            For j As Integer = 0 To N - 1
                V(i, j) = 0
                For k As Integer = 0 To M - 1
                    V(i, j) = V(i, j) + W(k) * X(i, k) * X(j, k)
                Next
            Next
            B(i) = 0
            For k As Integer = 0 To M - 1
                B(i) = B(i) + W(k) * X(i, k) * Y(k)
            Next
        Next
        ' V now contains the raw least squares matrix
        If Not SymmetricMatrixInvert(V) Then
            Return Nothing
        End If
        ' V now contains the inverted least square matrix
        ' Matrix multpily to get coefficients C = VB
        For i As Integer = 0 To N - 1
            C(i) = 0
            For j As Integer = 0 To N - 1
                C(i) = C(i) + V(i, j) * B(j)
            Next
        Next

        ' Calculate statistics
        Dim TSS As Double = 0
        Dim RSS As Double = 0
        Dim YBAR As Double = 0
        Dim WSUM As Double = 0
        For k As Integer = 0 To M - 1
            YBAR = YBAR + W(k) * Y(k)
            WSUM = WSUM + W(k)
        Next
        YBAR = YBAR / WSUM
        For k As Integer = 0 To M - 1
            Ycalc(k) = 0
            For i As Integer = 0 To N - 1
                Ycalc(k) = Ycalc(k) + C(i) * X(i, k)
            Next
            DY(k) = Ycalc(k) - Y(k)
            TSS = TSS + W(k) * (Y(k) - YBAR) * (Y(k) - YBAR)
            RSS = RSS + W(k) * DY(k) * DY(k)
        Next
        Dim SSQ As Double = RSS / NDF
        Dim RYSQ = 1 - RSS / TSS
        Dim FReg = 9999999
        If RYSQ < 0.9999999 Then
            FReg = RYSQ / (1 - RYSQ) * NDF / (N - 1)
        End If
        Dim SDV = Math.Sqrt(SSQ)

        ' Calculate var-covar matrix and std error of coefficients
        For i As Integer = 0 To N - 1
            For j As Integer = 0 To N - 1
                V(i, j) = V(i, j) * SSQ
            Next
            SEC(i) = Math.Sqrt(V(i, i))
        Next

        Return New WeightedFit With {
            .CalculatedValues = Ycalc,
            .Coefficients = C,
            .CoefficientsStandardError = SEC,
            .CorrelationCoefficient = RYSQ,
            .FisherF = FReg,
            .Residuals = DY,
            .StandardDeviation = SDV,
            .VarianceMatrix = V
        }
    End Function

    Public Function SymmetricMatrixInvert(V As Double(,)) As Boolean
        Dim N As Integer = CInt(Math.Truncate(Math.Sqrt(V.Length)))
        Dim t As Double() = New Double(N - 1) {}
        Dim Q As Double() = New Double(N - 1) {}
        Dim R As Double() = New Double(N - 1) {}
        Dim AB As Double
        Dim K As Integer, L As Integer, M As Integer

        ' Invert a symetric matrix in V
        For M = 0 To N - 1
            R(M) = 1
        Next
        K = 0
        For M = 0 To N - 1
            Dim Big As Double = 0
            For L = 0 To N - 1
                AB = Math.Abs(V(L, L))
                If (AB > Big) AndAlso (R(L) <> 0) Then
                    Big = AB
                    K = L
                End If
            Next
            If Big = 0 Then
                Return False
            End If
            R(K) = 0
            Q(K) = 1 / V(K, K)
            t(K) = 1
            V(K, K) = 0
            If K <> 0 Then
                For L = 0 To K - 1
                    t(L) = V(L, K)
                    If R(L) = 0 Then
                        Q(L) = V(L, K) * Q(K)
                    Else
                        Q(L) = -V(L, K) * Q(K)
                    End If
                    V(L, K) = 0
                Next
            End If
            If (K + 1) < N Then
                For L = K + 1 To N - 1
                    If R(L) <> 0 Then
                        t(L) = V(K, L)
                    Else
                        t(L) = -V(K, L)
                    End If
                    Q(L) = -V(K, L) * Q(K)
                    V(K, L) = 0
                Next
            End If
            For L = 0 To N - 1
                For K = L To N - 1
                    V(L, K) = V(L, K) + t(L) * Q(K)
                Next
            Next
        Next
        M = N
        L = N - 1
        For K = 1 To N - 1
            M = M - 1
            L = L - 1
            For J As Integer = 0 To L
                V(M, J) = V(J, M)
            Next
        Next
        Return True
    End Function
End Module
