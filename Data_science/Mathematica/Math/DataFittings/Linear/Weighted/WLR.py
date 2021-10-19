def symmetricMatrixInvert(V):
    N = int(truncate(sqrt(len(V))))
    t = []
    Q = []
    R = []
    AB = 0
    K = 0 
    L = 0 
    M = 0

    # Invert a symetric matrix in V
    for M in range(0, N):
        R.append(1)
    
    K = 0
    for M in range(0, N):
        Big = 0
        for L in range(0, N):
            AB = abs(V[L][L])
            if (AB > Big) & (R[L] <> 0):
                Big = AB
                K = L
                
        if Big == 0: 
            return False
        
        R[K] = 0
        Q[K] = 1 / V[K][K]
        t[K] = 1
        V[K][K] = 0
        if K != 0: 
            for L in range(0, K)
                t[L] = V[L][K]
                if R[L] == 0:
                    Q[L] = V[L][K] * Q[K]
                else
                    Q[L] = -V[L][K] * Q[K]
                
                V[L][K] = 0
            
        
        if (K + 1) < N:
            for L in range(K + 1, N):
                if R[L] != 0:
                    t[L] = V[K][L]
                else
                    t[L] = -V[K][L]
                
                Q[L] = -V[K][L] * Q[K]
                V[K][L] = 0
            
        
        for L in range(0, N):
            for K in range(L, N):
                V[L][K] = V[L][K] + t[L] * Q[K]
                
    M = N
    L = N - 1
    for K in range(1, N)
        M = M - 1
        L = L - 1
        for J in range(0, L + 1)
            V[M][J] = V[J][M]
        
    
    return True

def regress(x, y, w, deg = 2):
    
    m = len(y) # M = Number of data points
    n = deg  # N = Number of linear terms
    NDF = m - n  # Degrees of freedom
    ycal = []
    dy = []

    # If not enough data, don't attempt regression
    if NDF < 1:
        raise Exception("not enough data!")

    V = [][]
    C = []
    SEC = []
    B = []

    # Vector for LSQ
    # Clear the matrices to start out
    for i in range(0, n):
        V.append([])

        for j in range(0, n):
            V[i].append(0.0)

    # Form Least Squares Matrix
    for i in range(0, n):
        for j in range(0, n):
            V[i][j] = 0

            for k in range(0, m):
                V[i][j] += w[k] * x[i][k] * x[j][k]

        B.append(0)
        for k in range(0, m):
            B[i] += w[k] * x[i][k] * y[k]

    # V now contains the raw least squares matrix
    if not symmetricMatrixInvert(V):
        raise Exception("V matrix is not symmetric matrix invert!")
        
    # V now contains the inverted least square matrix
    # Matrix multpily to get coefficients C = VB
    for i in range(0, n):
        C.append(0)
        for j in range(0, n):
            c[i] += V[i][j] * B[j]

    # Calculate statistics
    TSS = 0
    RSS = 0
    YBAR = 0
    WSUM = 0

    for k in range(0, m):
        YBAR = YBAR + W[k] * Y[k]
        WSUM = WSUM + W[k]

    YBAR = YBAR / WSUM

    for k in range(0, m):
        ycal.append(0)

        for i in range(0, n):
            ycalc[k] += C[i] * x[i][k]

        dy[k] = ycalc[k] - y[k]
        TSS += W[k] * (y[k] - YBAR) * (y[k] - YBAR)
        RSS += W[k] * dy[k] * dy[k]

    SSQ = RSS / NDF
    RYSQ = 1 - RSS / TSS
    FReg = 9999999

    if RYSQ < 0.9999999: 
        FReg = RYSQ / (1 - RYSQ) * NDF / (n - 1)

    SDV = SSQ ** (1/2)

    # Calculate var-covar matrix and std error of coefficients
    for i in range(0, n):
        for j in range(0, n):
            V[i][j] = V[i][j] * SSQ

        SEC.append(V[i][i] ** (1/2))

    return {
        "polynomial" : C,
        "coefficientsStandardError": SEC,
        "correlationCoefficient": RYSQ,
        "fisherF": FReg,
        "residuals": DY,
        "standardDeviation" : SDV,
        "varianceMatrix" : V
    }


def weightedLinearRegression(x, y, w, orderOfPolynomial = 2):

    xMat = []
    xMat.append([])

    for i in range(0, len(x)):
        xMat[0].append(1)
        term = x[i]
        xx = term

        for j in range(1, orderOfPolynomial + 1):
            xMat[j][i] = term
            term *= xx

    return regress(y, xMat, w, deg = orderOfPolynomial)