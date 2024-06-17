Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Public Module FastICA
    ' 
    ' 	 * AlgorithmFunctions.c
    ' 	 *
    ' 	 *  Created on: 14 apr. 2016
    ' 	 *      Author: dharrison
    ' 	 

    Const RAND_MAX As Integer = Integer.MaxValue

    Public Function PreprocessingCentering(Xobs As Double()(), N As Integer, M As Integer) As Double()()
        ' 
        ' 		 * This function performs the centering operation on a matrix X.
        ' 		 * For more details refer to the reference literature (ICA: Algorithms and applications).
        ' 		 

        Dim i As Integer
        Dim j As Integer
        Dim X As Double()()
        Dim meanVector As Double()

        X = RectangularArray.Matrix(Of Double)(N, M)
        meanVector = New Double(N - 1) {}

        ' Calculating mean vector of observation matrix
        i = 0

        While i < N
            j = 0

            While j < M
                meanVector(i) += Xobs(i)(j)
                Threading.Interlocked.Increment(j)
            End While

            Threading.Interlocked.Increment(i)
        End While

        i = 0

        While i < N
            meanVector(i) /= M
            Threading.Interlocked.Increment(i)
        End While

        ' Centering observation matrix Xobs
        i = 0

        While i < N
            j = 0

            While j < M
                X(i)(j) = Xobs(i)(j) - meanVector(i)
                Threading.Interlocked.Increment(j)
            End While

            Threading.Interlocked.Increment(i)
        End While

        Return X
    End Function

    Public Function PreprocessingWhitening(X As Double()(), N As Integer, M As Integer) As Double()()
        ' 
        ' 		 * This function performs the whitening operation on a matrix X.
        ' 		 * For more details refer to the reference literature (ICA: Algorithms and applications)
        ' 		 * Note: the number of iterations for used for eigen decomposition of matrix ExxT is by default 100
        ' 		 * The default value can be changed by changing iterationsED to desired value.
        ' 		 

        Dim i As Integer
        Dim j As Integer
        Dim k As Integer
        Dim EigValues As Double()
        Dim EigVectors As Double()()
        Dim EigVectorsT As Double()()
        Dim Dnegroot As Double()
        Dim ExxT As Double()()
        Dim Z As Double()()
        Dim Dummy1 As Double()()
        Dim Dummy2 As Double()()
        Dim Drootmat As Double()()

        Dim iterationsED = 100

        ExxT = RectangularArray.Matrix(Of Double)(N, N)
        EigValues = New Double(N - 1) {}
        Dnegroot = New Double(N - 1) {}
        Drootmat = RectangularArray.Matrix(Of Double)(N, N)
        Dummy1 = RectangularArray.Matrix(Of Double)(N, N)
        Dummy2 = RectangularArray.Matrix(Of Double)(N, N)
        EigVectors = RectangularArray.Matrix(Of Double)(N, N)
        EigVectorsT = RectangularArray.Matrix(Of Double)(N, N)
        Z = RectangularArray.Matrix(Of Double)(N, M)

        ' Calculating covariance
        i = 0

        While i < N
            j = 0

            While j < N
                k = 0

                While k < M
                    ExxT(i)(j) += X(i)(k) * X(j)(k) / (M - 1)
                    Threading.Interlocked.Increment(k)
                End While

                Threading.Interlocked.Increment(j)
            End While

            Threading.Interlocked.Increment(i)
        End While

        ' Eigen Decomposition of (N x N real symmetric) covariance matrix ExxT of X
        EigenDecomposition(ExxT, N, EigVectors, EigValues, iterationsED)

        ' Building matrix D^-1/2, containing the negative roots of the eigenvalues of covariance matrix of X,
        ' 		 * the centered data of Xobs
        i = 0

        While i < N
            Dnegroot(i) = 1 / Math.Sqrt(EigValues(i))
            Threading.Interlocked.Increment(i)
        End While

        i = 0

        While i < N
            j = 0

            While j < N
                If i = j Then
                    Drootmat(i)(j) = Dnegroot(i)
                Else
                    Drootmat(i)(j) = 0.0
                End If

                Threading.Interlocked.Increment(j)
            End While

            Threading.Interlocked.Increment(i)
        End While

        ' Whitening matrix X. Z = E*1/sqrt(D)*E'*X 
        ' Dummy1 = E*1/sqrt(D)
        Dummy1 = MatMult(EigVectors, N, N, Drootmat, N, N)

        ' Tranpose of E
        EigVectorsT = MatTranspose(EigVectors, N, N)

        ' Dummy2 = Dummy1*E'
        Dummy2 = MatMult(Dummy1, N, N, EigVectorsT, N, N)

        ' Whitened matrix Z
        Z = MatMult(Dummy2, N, N, X, N, M)

        Return Z
    End Function

    Public Function SolveFastICA(Z As Double()(), N As Integer, M As Integer, Optional iterations As Integer = 1000) As Double()()
        ' 
        ' 		 * This function performs the FastICA algorithm.
        ' 		 * For more details refer to the reference literature (ICA: Algorithms and applications).
        ' 		 

        Dim p As Integer
        Dim i As Integer
        Dim j As Integer
        Dim k As Integer
        Dim it As Integer
        Dim G As Double()
        Dim Gder As Double()
        Dim ZGt As Double()
        Dim dumsum As Double()
        Dim W As Double()()
        Dim wp As Double()
        Dim GderOnes As Double
        Dim f As Double

        G = New Double(M - 1) {}
        Gder = New Double(M - 1) {}
        dumsum = New Double(N - 1) {}
        W = RectangularArray.Matrix(Of Double)(N, N)
        wp = New Double(N - 1) {}

        i = 0

        While i < N
            j = 0

            While j < N
                W(i)(j) = CDbl(randf.NextNumber()) / RAND_MAX
                Threading.Interlocked.Increment(j)
            End While

            Threading.Interlocked.Increment(i)
        End While

        ' FastICA algorithm
        p = 0

        While p < N
            i = 0

            While i < N
                wp(i) = CDbl(randf.NextNumber()) / RAND_MAX
                Threading.Interlocked.Increment(i)
            End While

            VectorNormalization(wp, N)

            ' FastICA iterations
            it = 0

            While it < iterations
                G = VecMatMult(wp, N, Z, M)

                i = 0

                While i < M
                    Gder(i) = 1 - Math.Tanh(G(i)) * Math.Tanh(G(i))
                    Threading.Interlocked.Increment(i)
                End While

                i = 0

                While i < M
                    G(i) = Math.Tanh(G(i))
                    Threading.Interlocked.Increment(i)
                End While

                ' wp = 1/M*Z*G' - 1/M*Gder*ones(M,1)*wp 
                ZGt = MatVecMult(Z, N, M, G)

                i = 0

                While i < N
                    ZGt(i) /= M
                    Threading.Interlocked.Increment(i)
                End While

                GderOnes = 0.0
                i = 0

                While i < M
                    GderOnes += Gder(i) / M
                    Threading.Interlocked.Increment(i)
                End While

                i = 0

                While i < N
                    wp(i) = ZGt(i) - GderOnes * wp(i)
                    Threading.Interlocked.Increment(i)
                End While

                ' Gram-Schmidt decorrelation
                i = 0

                While i < N
                    dumsum(i) = 0.0
                    Threading.Interlocked.Increment(i)
                End While

                i = 0

                While i < N
                    j = 0

                    While j < p
                        f = 0.0
                        k = 0

                        While k < N
                            f += wp(k) * W(k)(j)
                            Threading.Interlocked.Increment(k)
                        End While

                        dumsum(i) += f * W(i)(j)
                        Threading.Interlocked.Increment(j)
                    End While

                    Threading.Interlocked.Increment(i)
                End While

                i = 0

                While i < N
                    wp(i) -= dumsum(i)
                    Threading.Interlocked.Increment(i)
                End While

                VectorNormalization(wp, N)
                Threading.Interlocked.Increment(it)
            End While

            ' Storing estimated rows of the inverse of the mixing matrix as columns in W
            i = 0

            While i < N
                W(i)(p) = wp(i)
                Threading.Interlocked.Increment(i)
            End While

            Threading.Interlocked.Increment(p)
        End While

        ' Normalizing estimated inverse of mixing matrix A
        i = 0

        While i < N
            j = 0

            While j < N
                W(i)(j) /= Math.Sqrt(2.0)
                Threading.Interlocked.Increment(j)
            End While

            Threading.Interlocked.Increment(i)
        End While

        Return W
    End Function

    ' 
    ' 	 * ExportingData.c
    ' 	 *
    ' 	 *  Created on: 15 apr. 2016
    ' 	 *      Author: dharrison
    ' 	 


    Public Sub ExportingData()
        ' 
        ' 		 * This function exports the data obtained from the FastICA algorithm
        ' 		 * to a textfile 'SourcesEstimation.txt'.
        ' 		 * The data in the textfile is visualized in Matlab using the Matlab script
        ' 		 * ReadingData.m
        ' 		 

        'int i;
        'int j;

        ''' *Exporting data*/
        'FILE pfile;

        'pfile = fopen("SourcesEstimation.txt", "w");
        'if (pfile != null)
        '{
        '    fprintf(pfile, "%d\n%d\n", N, M);
        '    for (i = 0; i < M; ++i)
        '    {
        '        for (j = 0; j < N; ++j)
        '        {
        '            fprintf(pfile, "%.2f\t", Sest[j][i]);
        '        }
        '        fprintf(pfile, "\n");
        '    }

        '    for (i = 0; i < M; ++i)
        '    {
        '        fprintf(pfile, "%.2f\n", timeVector[i]);
        '    }

        '    fclose(pfile);
        '}
        'else
        '{
        '    Console.Write("Could not open file");
        '}
        'Console.Write("\n\n");
    End Sub

    ' 
    ' 	 * Functions.c
    ' 	 *
    ' 	 *  Created on: 14 apr. 2016
    ' 	 *      Author: dharrison
    ' 	 

    ' 
    ' 	 * Functions.h
    ' 	 *
    ' 	 *  Created on: 14 apr. 2016
    ' 	 *      Author: dharrison
    ' 	 

    Public Sub Initialize()
        ' 
        ' 		 * This function generates the mixing matrix Amix, used to
        ' 		 * generate the observation matrix Xobs, containing the M observed samples of
        ' 		 * N sources.
        ' 		 

        Dim i As Integer
        Dim j As Integer
        ' Generating mixing matrix Amix
        i = 0

        While i < N
            j = 0

            While j < N
                Amix(i)(j) = CDbl(randf.NextNumber()) / RAND_MAX
                Threading.Interlocked.Increment(j)
            End While

            Threading.Interlocked.Increment(i)
        End While
    End Sub

    Public Sub SetUpSources()
        ' 
        ' 		 * This function generated the source matrix S, containing the sources used
        ' 		 * to generate data for the observation matrix.
        ' 		 

        Dim i As Integer
        i = 0

        While i < M
            timeVector(i) = (finalTime - initialTime) / (M - 1) * i
            Threading.Interlocked.Increment(i)
        End While

        ' Source 1
        i = 0

        While i < M
            S(0)(i) = funcSource1(timeVector(i))
            Threading.Interlocked.Increment(i)
        End While

        ' Source 2
        i = 0

        While i < M
            S(1)(i) = funcSource2(timeVector(i))
            Threading.Interlocked.Increment(i)
        End While

        ' Source 3
        i = 0

        While i < M
            S(2)(i) = funcSource3(timeVector(i))
            Threading.Interlocked.Increment(i)
        End While

        ' Source 4
        i = 0

        While i < M
            S(3)(i) = funcSource4(timeVector(i))
            Threading.Interlocked.Increment(i)
        End While

        ' Source 5
        avgsource5 = 0.0
        i = 0

        While i < M
            S(4)(i) = funcSource5(timeVector(i))
            avgsource5 += S(4)(i)
            Threading.Interlocked.Increment(i)
        End While

        ' Averaging Source 5
        avgsource5 /= M
        i = 0

        While i < M
            S(4)(i) -= avgsource5
            Threading.Interlocked.Increment(i)
        End While

        ' Source 6
        avgsource6 = 0.0
        i = 0

        While i < M
            S(5)(i) = funcSource6(timeVector(i))
            avgsource6 += S(5)(i)
            Threading.Interlocked.Increment(i)
        End While

        ' Averaging Source 6
        avgsource6 /= M
        i = 0

        While i < M
            S(5)(i) -= avgsource6
            Threading.Interlocked.Increment(i)
        End While
    End Sub

    Public Function XobsGen(Amix As Double()(), S As Double()(), N As Integer, M As Integer) As Double()()
        ' 
        ' 		 * This function generates the observation matrix Xobs
        ' 		 * consisting of M observed mixture samples of size N.
        ' 		 

        Dim i As Integer
        Dim j As Integer
        Dim k As Integer
        Dim Xobs = RectangularArray.Matrix(Of Double)(N, M)

        ' Generating observation matrix Xobs
        i = 0

        While i < N
            j = 0

            While j < M
                k = 0

                While k < N
                    Xobs(i)(j) += Amix(i)(k) * S(k)(j)
                    Threading.Interlocked.Increment(k)
                End While

                Threading.Interlocked.Increment(j)
            End While

            Threading.Interlocked.Increment(i)
        End While

        Return Xobs
    End Function

    Public Sub FreeMemory()
        ' 
        ' 		 * This function frees memory allocated for execution of the
        ' 		 * algorithm.
        ' 		 

        Amix = Nothing
        W = Nothing
        WT = Nothing
        timeVector = Nothing
        S = Nothing
        Sest = Nothing
        Xobs = Nothing
        X = Nothing
        Z = Nothing
    End Sub


    ' 
    ' 	 * MatrixOps.c
    ' 	 *
    ' 	 *  Created on: 15 apr. 2016
    ' 	 *      Author: dharrison
    ' 	 

    Public Function MatMult(A As Double()(), rows1 As Integer, columns1 As Integer, B As Double()(), rows2 As Integer, columns2 As Integer) As Double()()
        ' 
        ' 		 * This function performs matrix multiplication of two matrices A and B (A*B)
        ' 		 * with at least two rows and columns.
        ' 		 * The result Sp = A*B is returned by the function.
        ' 		 

        Dim i As Integer
        Dim j As Integer
        Dim k As Integer
        Dim Sp As Double()()

        Sp = RectangularArray.Matrix(Of Double)(rows1, columns2)

        i = 0

        While i < rows1
            j = 0

            While j < columns2
                k = 0

                While k < columns1
                    Sp(i)(j) += A(i)(k) * B(k)(j)
                    Threading.Interlocked.Increment(k)
                End While

                Threading.Interlocked.Increment(j)
            End While

            Threading.Interlocked.Increment(i)
        End While

        Return Sp
    End Function

    Public Function VecMatMult(ByRef V As Double(), SizeVec As Integer, B As Double()(), columns As Integer) As Double()
        ' 
        ' 		 * This function performs matrix multiplication of vector V and matrix B (V*B)
        ' 		 * with the matrix containing at least two rows and columns
        ' 		 * The result Sp = V*B is returned by the function.
        ' 		 

        Dim i As Integer
        Dim k As Integer
        Dim Sp As Double()

        Sp = New Double(columns - 1) {}

        i = 0

        While i < columns
            k = 0

            While k < SizeVec
                Sp(i) += V(k) * B(k)(i)
                Threading.Interlocked.Increment(k)
            End While

            Threading.Interlocked.Increment(i)
        End While

        Return Sp
    End Function

    Public Function MatVecMult(B As Double()(), rows As Integer, columns As Integer, ByRef V As Double()) As Double()
        ' 
        ' 		 * This function performs matrix multiplication of matrix B and vector V (B*V)
        ' 		 * with the matrix containing at least two rows and columns.
        ' 		 * The result Sp = B*V is returned by the function.
        ' 		 

        Dim i As Integer
        Dim k As Integer
        Dim Sp As Double()

        Sp = New Double(rows - 1) {}

        i = 0

        While i < rows
            k = 0

            While k < columns
                Sp(i) += B(i)(k) * V(k)
                Threading.Interlocked.Increment(k)
            End While

            Threading.Interlocked.Increment(i)
        End While

        Return Sp
    End Function

    Public Function MatTranspose(A As Double()(), rows As Integer, columns As Integer) As Double()()
        ' 
        ' 		 * This function computes the transpose of matrix A
        ' 		 * The tranpose Sp = A' (matlab notation) is returned by the function.
        ' 		 

        Dim i As Integer
        Dim j As Integer
        Dim Sp = RectangularArray.Matrix(Of Double)(columns, rows)

        i = 0

        While i < columns
            j = 0

            While j < rows
                Sp(i)(j) = A(j)(i)
                Threading.Interlocked.Increment(j)
            End While

            Threading.Interlocked.Increment(i)
        End While

        Return Sp
    End Function


    Public Sub VectorNormalization(ByRef wp As Double(), sizeVec As Integer)
        ' 
        ' 		 * This function normalizes a vector wp and returns
        ' 		 * the normalized vector wp.
        ' 		 

        Dim i As Integer
        Dim sqrtwpwp = 0.0

        i = 0

        While i < sizeVec
            sqrtwpwp += wp(i) * wp(i)
            Threading.Interlocked.Increment(i)
        End While

        i = 0

        While i < sizeVec
            wp(i) = wp(i) / Math.Sqrt(sqrtwpwp)
            Threading.Interlocked.Increment(i)
        End While
    End Sub

    Public Sub EigenDecomposition(ExxT As Double()(), N As Integer, EigVectors As Double()(), ByRef EigValues As Double(), iterations As Integer)
        ' 
        ' 		 * This function computes the eigenvalues and eigenvectors
        ' 		 * of a real, symmetric, N x N matrix.
        ' 		 * The eigenvalues are stored in the function argument EigValues and the eigenvectors are
        ' 		 * stored in the function argument EigVectors.
        ' 		 

        Dim i As Integer
        Dim j As Integer
        Dim k As Integer
        Dim p As Integer
        Dim it As Integer
        Dim EigVecs As Double()()
        Dim Q As Double()()
        Dim EigVals As Double()()
        Dim wp As Double()
        Dim dumsum As Double()
        Dim R As Double()()
        Dim QT As Double()()
        Dim f As Double

        EigVecs = RectangularArray.Matrix(Of Double)(N, N)
        Q = RectangularArray.Matrix(Of Double)(N, N)
        EigVals = RectangularArray.Matrix(Of Double)(N, N)
        wp = New Double(N - 1) {}
        dumsum = New Double(N - 1) {}
        R = RectangularArray.Matrix(Of Double)(N, N)
        QT = RectangularArray.Matrix(Of Double)(N, N)

        ' Initializing Ait, matrix containing eigenvalues as the diagonal
        i = 0

        While i < N
            j = 0

            While j < N
                EigVals(i)(j) = ExxT(i)(j)
                Threading.Interlocked.Increment(j)
            End While

            Threading.Interlocked.Increment(i)
        End While

        ' Initializing Q and E for computation of eigenvectors
        i = 0

        While i < N
            Q(i)(i) = 1.0
            EigVecs(i)(i) = 1.0
            Threading.Interlocked.Increment(i)
        End While

        ' Eigen decomposition iterations
        it = 0

        While it < iterations
            ' Gram-Schmidt decorrelation
            p = 0

            While p < N
                i = 0

                While i < N
                    wp(i) = EigVals(i)(p)
                    Threading.Interlocked.Increment(i)
                End While

                VectorNormalization(wp, N)

                i = 0

                While i < N
                    dumsum(i) = 0.0
                    Threading.Interlocked.Increment(i)
                End While

                i = 0

                While i < N
                    j = 0

                    While j < p
                        f = 0.0
                        k = 0

                        While k < N
                            f += wp(k) * Q(k)(j)
                            Threading.Interlocked.Increment(k)
                        End While

                        dumsum(i) += f * Q(i)(j)
                        Threading.Interlocked.Increment(j)
                    End While

                    Threading.Interlocked.Increment(i)
                End While

                i = 0

                While i < N
                    wp(i) -= dumsum(i)
                    Threading.Interlocked.Increment(i)
                End While

                VectorNormalization(wp, N)

                ' Storing estimated rows of the inverse of the mixing matrix as columns in W
                i = 0

                While i < N
                    Q(i)(p) = wp(i)
                    Threading.Interlocked.Increment(i)
                End While

                Threading.Interlocked.Increment(p)
            End While

            QT = MatTranspose(Q, N, N)

            R = MatMult(QT, N, N, EigVals, N, N)

            EigVals = MatMult(R, N, N, Q, N, N)

            EigVecs = MatMult(EigVecs, N, N, Q, N, N)
            Threading.Interlocked.Increment(it)
        End While

        EigVecs = MatMult(EigVecs, N, N, Q, N, N)

        i = 0

        While i < N
            EigValues(i) = EigVals(i)(i)
            Threading.Interlocked.Increment(i)
        End While

        i = 0

        While i < N
            j = 0

            While j < N
                EigVectors(i)(j) = EigVecs(i)(j)
                Threading.Interlocked.Increment(j)
            End While

            Threading.Interlocked.Increment(i)
        End While
    End Sub

    ' 
    ' 	 * Memory.c
    ' 	 *
    ' 	 *  Created on: 14 apr. 2016
    ' 	 *      Author: dharrison
    ' 	 




    ' 
    ' 	 * Setup.c
    ' 	 *
    ' 	 *  Created on: 14 apr. 2016
    ' 	 *      Author: dharrison
    ' 	 



    Public Sub setupVars()
        ' 
        ' 		 * This function sets up various parameters used by the algorithm
        ' 		 

        periodSource5 = (finalTime - initialTime) / na
        periodSource6 = (finalTime - initialTime) / ns / 2

        timeVector = New Double(M - 1) {}

        Amix = RectangularArray.Matrix(Of Double)(N, N)
        W = RectangularArray.Matrix(Of Double)(N, N)
        WT = RectangularArray.Matrix(Of Double)(N, N)

        ' Generating Data for ICA

        S = RectangularArray.Matrix(Of Double)(N, M)
        Sest = RectangularArray.Matrix(Of Double)(N, M)
        Xobs = RectangularArray.Matrix(Of Double)(N, M)
        X = RectangularArray.Matrix(Of Double)(N, M)
        Z = RectangularArray.Matrix(Of Double)(N, M)
    End Sub

    ' 
    ' 	 * ICAMain.c
    ' 	 *
    ' 	 *  Created on: 14 apr. 2016
    ' 	 *      Author: Derek W. Harrison
    ' 	 *
    ' 	 *  This code is a C implementation of the FastICA method for recovering independent
    ' 	 *  components in component mixtures
    ' 	 *
    ' 	 *  For more information refer to the paper 'ICA: Algorithms and applications' which is found
    ' 	 *  in the same directory as the source files of this code.
    ' 	 



    Private Function Main() As Integer
        begin = Date.Now

        ' User input parameter data
        ParameterInput()

        ' Setting up variables and generating Data
        setupVars()

        ' Initializing mixing matrix
        Initialize()

        ' Setting up source signals
        SetUpSources()

        ' Generating observed sample data
        Xobs = XobsGen(Amix, S, N, M)

        ' FastICA algorithm
        X = PreprocessingCentering(Xobs, N, M)

        Z = PreprocessingWhitening(X, N, M)

        W = SolveFastICA(Z, N, M, iterations)

        ' Outputting results of FastICA algorithm
        WT = MatTranspose(W, N, N)

        Sest = MatMult(WT, N, N, Z, N, M)


        ' Exporting estimated source data to .txt for visualization in Matlab
        ExportingData()

        ' Cleaning up
        FreeMemory()

        [end] = Date.Now
        ' time_spent = (double)(end - begin) / CLOCKS_PER_SEC;

        Console.Write(vbLf & "timespent: {0:f}" & vbLf, time_spent)

        Return 0
    End Function

    ' 
    ' 	 * ParameterInput.c
    ' 	 *
    ' 	 *  Created on: 14 apr. 2016
    ' 	 *      Author: dharrison
    ' 	 



    Public Sub ParameterInput()
        ' 
        ' 		 * The user parameters are entered here. This is the only section of the code
        ' 		 * Which should be changed by the user. Note that 6 sources are available. If more sources
        ' 		 * are required or desired these need to be entered in the corresponding function SetUpSources()
        ' 		 * in Functions.c. The extra sources need to be added in Sources.c
        ' 		 * If less are required or desired, the excess sources need to be commented or removed
        ' 		 * from the SetUpSources() function.
        ' 		 

        N = 6 'The number of sources. (It is preferable not to change this value)
        C = N 'The number of observations (sample size). This value should be equal to N and should not be changed!
        M = 10000 'The number of observation samples

        K = 0.1F 'The slope of the zig-zag source
        na = 8F 'The amount of zig-zag source periods (amount of peaks)
        ns = 5F 'The amount of alternating step-function periods

        finalTime = 40.0F * 3.1415926535F 'Final time (s)
        initialTime = 0.0F 'Initial time (s)

        iterations = 100 'Number of FastICA iterations
    End Sub

    ' 
    ' 	 * Sources.c
    ' 	 *
    ' 	 *  Created on: 14 apr. 2016
    ' 	 *      Author: dharrison
    ' 	 



    Public Function funcSource1(x As Double) As Double
        Return Math.Sin(1.1 * x)
    End Function


    Public Function funcSource2(x As Double) As Double
        Return Math.Cos(0.25 * x)
    End Function


    Public Function funcSource3(x As Double) As Double
        Return Math.Sin(0.1 * x)
    End Function


    Public Function funcSource4(x As Double) As Double
        Return Math.Cos(0.7 * x)
    End Function


    Public Function funcSource5(x As Double) As Double
        Return K * x - Math.Floor(x / periodSource5) * K * periodSource5
    End Function


    Public Function funcSource6(x As Double) As Double
        If CInt(Math.Floor(x / periodSource6)) Mod 2 = 0 Then
            Return 1
        Else
            Return -1
        End If
    End Function

    ' 
    ' 	 * AlgoFunctions.h
    ' 	 *
    ' 	 *  Created on: 14 apr. 2016
    ' 	 *      Author: dharrison
    ' 	 



    ' 
    ' 	 * MatrixOps.h
    ' 	 *
    ' 	 *  Created on: 15 apr. 2016
    ' 	 *      Author: dharrison
    ' 	 




    ' 
    ' 	 * Memory.h
    ' 	 *
    ' 	 *  Created on: 15 apr. 2016
    ' 	 *      Author: dharrison
    ' 	 



    ' 
    ' 	 * Parameter.h
    ' 	 *
    ' 	 *  Created on: 14 apr. 2016
    ' 	 *      Author: dharrison
    ' 	 


    ' 
    ' 	 * ParameterDefinition.h
    ' 	 *
    ' 	 *  Created on: 14 apr. 2016
    ' 	 *      Author: dharrison
    ' 	 



    Public iterations As Integer
    Public N As Integer
    Public C As Integer
    Public M As Integer
    Public p As Integer
    Public finalTime As Single
    Public initialTime As Single
    Public K As Single
    Public na As Single
    Public ns As Single

    Public Amix As Double()()
    Public W As Double()()
    Public WT As Double()()
    Public timeVector As Double()
    Public S As Double()()
    Public Sest As Double()()
    Public Xobs As Double()()
    Public X As Double()()
    Public Z As Double()()
    Public periodSource5 As Double
    Public periodSource6 As Double
    Public avgsource5 As Double
    Public avgsource6 As Double
    Public time_spent As Double
    Public begin As Date = Date.Now
    Public [end] As Date

End Module
