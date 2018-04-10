''' <summary>
''' ## An Algorithm for Weighted Linear Regression
''' 
''' > https://www.codeproject.com/Articles/25335/An-Algorithm-for-Weighted-Linear-Regression
''' </summary>
Public Class WeightedLinearRegression
    Private V As Double(,)
    ' Least squares and var/covar matrix
    Public C As Double()
    ' Coefficients
    Public SEC As Double()
    ' Std Error of coefficients
    Private RYSQ As Double
    ' Multiple correlation coefficient
    Private SDV As Double
    ' Standard deviation of errors
    Private FReg As Double
    ' Fisher F statistic for regression
    Private Ycalc As Double()
    ' Calculated values of Y
    Private DY As Double()
    ' Residual values of Y
    Public ReadOnly Property FisherF() As Double
        Get
            Return FReg
        End Get
    End Property

    Public ReadOnly Property CorrelationCoefficient() As Double
        Get
            Return RYSQ
        End Get
    End Property

    Public ReadOnly Property StandardDeviation() As Double
        Get
            Return SDV
        End Get
    End Property

    Public ReadOnly Property CalculatedValues() As Double()
        Get
            Return Ycalc
        End Get
    End Property

    Public ReadOnly Property Residuals() As Double()
        Get
            Return DY
        End Get
    End Property

    Public ReadOnly Property Coefficients() As Double()
        Get
            Return C
        End Get
    End Property

    Public ReadOnly Property CoefficientsStandardError() As Double()
        Get
            Return SEC
        End Get
    End Property

    Public ReadOnly Property VarianceMatrix() As Double(,)
        Get
            Return V
        End Get
    End Property

    Public Function Regress(Y As Double(), X As Double(,), W As Double()) As Boolean
        Dim M As Integer = Y.Length
        ' M = Number of data points
        Dim N As Integer = X.Length \ M
        ' N = Number of linear terms
        Dim NDF As Integer = M - N
        ' Degrees of freedom
        Ycalc = New Double(M - 1) {}
        DY = New Double(M - 1) {}
        ' If not enough data, don't attempt regression
        If NDF < 1 Then
            Return False
        End If
        V = New Double(N - 1, N - 1) {}
        C = New Double(N - 1) {}
        SEC = New Double(N - 1) {}
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
            Return False
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
        RYSQ = 1 - RSS / TSS
        FReg = 9999999
        If RYSQ < 0.9999999 Then
            FReg = RYSQ / (1 - RYSQ) * NDF / (N - 1)
        End If
        SDV = Math.Sqrt(SSQ)

        ' Calculate var-covar matrix and std error of coefficients
        For i As Integer = 0 To N - 1
            For j As Integer = 0 To N - 1
                V(i, j) = V(i, j) * SSQ
            Next
            SEC(i) = Math.Sqrt(V(i, i))
        Next
        Return True
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

    Public Function RunTest(X As Double()) As Double
        Dim NRuns As Integer = 1
        Dim N1 As Integer = 0
        Dim N2 As Integer = 0
        If X(0) > 0 Then
            N1 = 1
        Else
            N2 = 1
        End If

        For k As Integer = 1 To X.Length - 1
            If X(k) > 0 Then
                N1 += 1
            Else
                N2 += 1
            End If
            If X(k) * X(k - 1) < 0 Then
                NRuns += 1
            End If
        Next
        Return 1
    End Function
End Class
