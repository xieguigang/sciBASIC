#Region "Microsoft.VisualBasic::fd56e72b090563af2e5e862c25a47cd2, Data_science\Mathematica\Math\Math.Statistics\FastICA.vb"

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

    '   Total Lines: 762
    '    Code Lines: 385 (50.52%)
    ' Comment Lines: 232 (30.45%)
    '    - Xml Docs: 81.47%
    ' 
    '   Blank Lines: 145 (19.03%)
    '     File Size: 28.94 KB


    ' Class FastICA
    ' 
    '     Function: funcSource1, funcSource2, funcSource3, funcSource4, funcSource5
    '               funcSource6, Main, MatMult, MatTranspose, MatVecMult
    '               PreprocessingCentering, PreprocessingWhitening, SolveFastICA, VecMatMult, XobsGen
    ' 
    '     Sub: EigenDecomposition, ExportingData, FreeMemory, Initialize, ParameterInput
    '          SetUpSources, setupVars, VectorNormalization
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports rand = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

''' <summary>
''' FastICA (Fast Independent Component Analysis) implementation.
''' This class provides a VB.NET port of the FastICA C reference implementation
''' for recovering independent components from component mixtures.
''' For the underlying theory, refer to the paper "ICA: Algorithms and Applications".
''' </summary>
Public Class FastICA

    ''' <summary>Number of FastICA fixed-point iterations used in <see cref="SolveFastICA"/>.</summary>
    Public iterations As Integer
    ''' <summary>Number of sources (rows of the observation matrix).</summary>
    Public N As Integer
    ''' <summary>Number of observations (sample size); should equal <see cref="N"/>.</summary>
    Public C As Integer
    ''' <summary>Number of observation samples (columns of the observation matrix).</summary>
    Public M As Integer
    ''' <summary>Helper loop index used during source setup.</summary>
    Public p As Integer
    ''' <summary>Final simulation time (seconds).</summary>
    Public finalTime As Single
    ''' <summary>Initial simulation time (seconds).</summary>
    Public initialTime As Single
    ''' <summary>Slope of the zig-zag source (funcSource5).</summary>
    Public K As Single
    ''' <summary>Number of zig-zag source periods (amount of peaks).</summary>
    Public na As Single
    ''' <summary>Number of alternating step-function periods (funcSource6).</summary>
    Public ns As Single

    ''' <summary>The mixing matrix A (N x N) used to generate the observation matrix.</summary>
    Public Amix As Double()()
    ''' <summary>The estimated inverse of the mixing matrix W (N x N).</summary>
    Public W As Double()()
    ''' <summary>Transpose of the estimated inverse mixing matrix W (N x N).</summary>
    Public WT As Double()()
    ''' <summary>Time vector used while generating the source signals.</summary>
    Public timeVector As Double()
    ''' <summary>The generated source matrix S (M x N).</summary>
    Public S As Double()()
    ''' <summary>The estimated source matrix Sest (M x N).</summary>
    Public Sest As Double()()
    ''' <summary>The generated observation matrix Xobs (M x N).</summary>
    Public Xobs As Double()()
    ''' <summary>The centered observation matrix X (M x N).</summary>
    Public X As Double()()
    ''' <summary>The whitened observation matrix Z (M x N).</summary>
    Public Z As Double()()
    ''' <summary>Period of the zig-zag source (funcSource5).</summary>
    Public periodSource5 As Double
    ''' <summary>Period of the step-function source (funcSource6).</summary>
    Public periodSource6 As Double
    ''' <summary>Mean value of the fifth source signal.</summary>
    Public avgsource5 As Double
    ''' <summary>Mean value of the sixth source signal.</summary>
    Public avgsource6 As Double
    ''' <summary>Elapsed computation time of the algorithm.</summary>
    Public time_spent As Double
    ''' <summary>Timestamp recorded at the start of <see cref="Main"/>.</summary>
    Public begin As Date = Date.Now
    ''' <summary>Timestamp recorded at the end of <see cref="Main"/>.</summary>
    Public [end] As Date

    ''' <summary>
    ''' The maximum value returned by the pseudo-random number generator, used to normalize
    ''' generated values into the [0, 1) range. Mirrors the C <c>RAND_MAX</c> constant.
    ''' </summary>
    Const RAND_MAX As Integer = Integer.MaxValue

    ''' <summary>
    ''' Performs the centering operation on the observation matrix <paramref name="Xobs"/>
    ''' by subtracting the per-row mean from each observation.
    ''' </summary>
    ''' <param name="Xobs">The original observation matrix (N x M).</param>
    ''' <param name="N">Number of sources (rows).</param>
    ''' <param name="M">Number of observation samples (columns).</param>
    ''' <returns>The centered matrix X (M x N).</returns>
    Public Function PreprocessingCentering(ByVal Xobs As Double()(), ByVal N As Integer, ByVal M As Integer) As Double()()
        Dim meanVector As Double() = New Double(N - 1) {}
        Dim X As Double()() = RectangularArray.Matrix(Of Double)(M, N)

        ' Calculating mean vector of observation matrix
        For i As Integer = 0 To N - 1
            For j As Integer = 0 To M - 1
                meanVector(i) += Xobs(i)(j)
            Next
        Next

        For i As Integer = 0 To N - 1
            meanVector(i) /= M
        Next

        ' Centering observation matrix Xobs
        For i As Integer = 0 To N - 1
            For j As Integer = 0 To M - 1
                X(i)(j) = Xobs(i)(j) - meanVector(i)
            Next
        Next

        Return X
    End Function

    ''' <summary>
    ''' Performs the whitening operation on the centered matrix <paramref name="X"/>.
    ''' Whitening transforms the data so that its components are uncorrelated and have unit variance,
    ''' which is a required precondition for the FastICA algorithm.
    ''' </summary>
    ''' <param name="X">The centered observation matrix (M x N).</param>
    ''' <param name="N">Number of sources.</param>
    ''' <param name="M">Number of observation samples.</param>
    ''' <returns>The whitened matrix Z (M x N).</returns>
    ''' <remarks>
    ''' The eigen decomposition uses 100 iterations by default. This value can be changed by
    ''' adjusting <c>iterationsED</c> inside this routine.
    ''' </remarks>
    Public Function PreprocessingWhitening(ByVal X As Double()(), ByVal N As Integer, ByVal M As Integer) As Double()()
        Dim EigValues As Double() = New Double(N - 1) {}
        Dim EigVectors As Double()() = RectangularArray.Matrix(Of Double)(N, N)
        Dim EigVectorsT As Double()() = RectangularArray.Matrix(Of Double)(N, N)
        Dim Dnegroot As Double() = New Double(N - 1) {}
        Dim ExxT As Double()() = RectangularArray.Matrix(Of Double)(N, N)
        Dim Z As Double()() = RectangularArray.Matrix(Of Double)(M, N)
        Dim Dummy1 As Double()() = RectangularArray.Matrix(Of Double)(N, N)
        Dim Dummy2 As Double()() = RectangularArray.Matrix(Of Double)(N, N)
        Dim Drootmat As Double()() = RectangularArray.Matrix(Of Double)(N, N)

        Dim iterationsED As Integer = 100

        ' Calculating covariance
        For i As Integer = 0 To N - 1
            For j As Integer = 0 To N - 1
                For k As Integer = 0 To M - 1
                    ExxT(i)(j) += X(i)(k) * X(j)(k) / (M - 1)
                Next
            Next
        Next

        ' Eigen Decomposition of (N x N real symmetric) covariance matrix ExxT of X
        EigenDecomposition(ExxT, N, EigVectors, EigValues, iterationsED)

        ' Building matrix D^-1/2, containing the inverse square roots of the eigenvalues
        For i As Integer = 0 To N - 1
            Dnegroot(i) = 1 / std.Sqrt(EigValues(i))
        Next

        For i As Integer = 0 To N - 1
            For j As Integer = 0 To N - 1
                Drootmat(i)(j) = If(i = j, Dnegroot(i), 0.0)
            Next
        Next

        ' Whitening matrix Z = E * 1/sqrt(D) * E' * X
        ' Dummy1 = E * 1/sqrt(D)
        Dummy1 = MatMult(EigVectors, N, N, Drootmat, N, N)

        ' Transpose of E
        EigVectorsT = MatTranspose(EigVectors, N, N)

        ' Dummy2 = Dummy1 * E'
        Dummy2 = MatMult(Dummy1, N, N, EigVectorsT, N, N)

        ' Whitened matrix Z
        Z = MatMult(Dummy2, N, N, X, N, M)

        Return Z
    End Function

    ''' <summary>
    ''' Performs the FastICA algorithm to estimate the inverse of the mixing matrix.
    ''' </summary>
    ''' <param name="Z">The whitened observation matrix (M x N).</param>
    ''' <param name="N">Number of sources.</param>
    ''' <param name="M">Number of observation samples.</param>
    ''' <param name="iterations">Maximum number of fixed-point iterations per component (default 1000).</param>
    ''' <returns>The estimated inverse of the mixing matrix W (N x N).</returns>
    ''' <remarks>
    ''' Uses the tanh non-linearity and Gram-Schmidt decorrelation between estimated components.
    ''' For the underlying theory, refer to "ICA: Algorithms and Applications".
    ''' </remarks>
    Public Function SolveFastICA(ByVal Z As Double()(), ByVal N As Integer, ByVal M As Integer, Optional ByVal iterations As Integer = 1000) As Double()()
        Dim G As Double() = New Double(M - 1) {}
        Dim Gder As Double() = New Double(M - 1) {}
        Dim dumsum As Double() = New Double(N - 1) {}
        Dim W As Double()() = RectangularArray.Matrix(Of Double)(N, N)
        Dim wp As Double() = New Double(N - 1) {}
        Dim ZGt As Double()
        Dim GderOnes As Double
        Dim f As Double

        ' Random initialization of W
        For i As Integer = 0 To N - 1
            For j As Integer = 0 To N - 1
                W(i)(j) = CDbl(rand.NextNumber()) / RAND_MAX
            Next
        Next

        ' FastICA algorithm
        For p As Integer = 0 To N - 1
            For i As Integer = 0 To N - 1
                wp(i) = CDbl(rand.NextNumber()) / RAND_MAX
            Next

            VectorNormalization(wp, N)

            ' FastICA fixed-point iterations
            For it As Integer = 0 To iterations - 1
                G = VecMatMult(wp, N, Z, M)

                For i As Integer = 0 To M - 1
                    Gder(i) = 1 - std.Tanh(G(i)) * std.Tanh(G(i))
                Next

                For i As Integer = 0 To M - 1
                    G(i) = std.Tanh(G(i))
                Next

                ' wp = 1/M * Z * G' - 1/M * Gder * ones(M,1) * wp
                ZGt = MatVecMult(Z, N, M, G)

                For i As Integer = 0 To N - 1
                    ZGt(i) /= M
                Next

                GderOnes = 0.0

                For i As Integer = 0 To M - 1
                    GderOnes += Gder(i) / M
                Next

                For i As Integer = 0 To N - 1
                    wp(i) = ZGt(i) - GderOnes * wp(i)
                Next

                ' Gram-Schmidt decorrelation
                For i As Integer = 0 To N - 1
                    dumsum(i) = 0.0
                Next

                For i As Integer = 0 To N - 1
                    For j As Integer = 0 To p - 1
                        f = 0.0

                        For k As Integer = 0 To N - 1
                            f += wp(k) * W(k)(j)
                        Next

                        dumsum(i) += f * W(i)(j)
                    Next
                Next

                For i As Integer = 0 To N - 1
                    wp(i) -= dumsum(i)
                Next

                VectorNormalization(wp, N)
            Next

            ' Storing estimated rows of the inverse of the mixing matrix as columns in W
            For i As Integer = 0 To N - 1
                W(i)(p) = wp(i)
            Next
        Next

        ' Normalizing estimated inverse of mixing matrix A
        For i As Integer = 0 To N - 1
            For j As Integer = 0 To N - 1
                W(i)(j) /= std.Sqrt(2.0)
            Next
        Next

        Return W
    End Function

    ''' <summary>
    ''' Exports the estimated source data obtained from the FastICA algorithm.
    ''' </summary>
    ''' <remarks>
    ''' In the original C implementation this routine wrote the estimated sources
    ''' <c>Sest</c> and the <c>timeVector</c> to the text file "SourcesEstimation.txt"
    ''' for visualization in Matlab (ReadingData.m). The VB port is left empty to
    ''' keep the call chain intact without performing file I/O.
    ''' </remarks>
    Public Sub ExportingData()
    End Sub

    ''' <summary>
    ''' Generates the random mixing matrix <c>Amix</c>, used to build the observation
    ''' matrix <c>Xobs</c> containing the M observed mixtures of the N sources.
    ''' </summary>
    Public Sub Initialize()
        ' Generating mixing matrix Amix
        For i As Integer = 0 To N - 1
            For j As Integer = 0 To N - 1
                Amix(i)(j) = CDbl(rand.NextNumber()) / RAND_MAX
            Next
        Next
    End Sub

    ''' <summary>
    ''' Generates the source matrix <c>S</c>, containing the source signals used to
    ''' build the observation matrix data.
    ''' </summary>
    ''' <remarks>
    ''' Sources 5 and 6 are subsequently mean-centered (their average is subtracted)
    ''' to satisfy the zero-mean requirement of ICA.
    ''' </remarks>
    Public Sub SetUpSources()
        For i As Integer = 0 To M - 1
            timeVector(i) = (finalTime - initialTime) / (M - 1) * i
        Next

        ' Source 1
        For i As Integer = 0 To M - 1
            S(0)(i) = funcSource1(timeVector(i))
        Next

        ' Source 2
        For i As Integer = 0 To M - 1
            S(1)(i) = funcSource2(timeVector(i))
        Next

        ' Source 3
        For i As Integer = 0 To M - 1
            S(2)(i) = funcSource3(timeVector(i))
        Next

        ' Source 4
        For i As Integer = 0 To M - 1
            S(3)(i) = funcSource4(timeVector(i))
        Next

        ' Source 5
        avgsource5 = 0.0

        For i As Integer = 0 To M - 1
            S(4)(i) = funcSource5(timeVector(i))
            avgsource5 += S(4)(i)
        Next

        ' Averaging Source 5
        avgsource5 /= M

        For i As Integer = 0 To M - 1
            S(4)(i) -= avgsource5
        Next

        ' Source 6
        avgsource6 = 0.0

        For i As Integer = 0 To M - 1
            S(5)(i) = funcSource6(timeVector(i))
            avgsource6 += S(5)(i)
        Next

        ' Averaging Source 6
        avgsource6 /= M

        For i As Integer = 0 To M - 1
            S(5)(i) -= avgsource6
        Next
    End Sub

    ''' <summary>
    ''' Generates the observation matrix <see ref="Xobs"/>, consisting of M observed
    ''' mixture samples of size N, by multiplying the mixing matrix <paramref name="Amix"/>
    ''' with the source matrix <paramref name="S"/>.
    ''' </summary>
    ''' <param name="Amix">The mixing matrix (N x N).</param>
    ''' <param name="S">The source matrix (M x N).</param>
    ''' <param name="N">Number of sources.</param>
    ''' <param name="M">Number of observation samples.</param>
    ''' <returns>The observation matrix Xobs (M x N).</returns>
    Public Function XobsGen(ByVal Amix As Double()(), ByVal S As Double()(), ByVal N As Integer, ByVal M As Integer) As Double()()
        Dim Xobs = RectangularArray.Matrix(Of Double)(M, N)

        ' Generating observation matrix Xobs
        For i As Integer = 0 To N - 1
            For j As Integer = 0 To M - 1
                For k As Integer = 0 To N - 1
                    Xobs(i)(j) += Amix(i)(k) * S(k)(j)
                Next
            Next
        Next

        Return Xobs
    End Function

    ''' <summary>
    ''' Frees the memory allocated for the execution of the algorithm by erasing all
    ''' working matrices and vectors.
    ''' </summary>
    Public Sub FreeMemory()
        Erase Amix
        Erase W
        Erase WT
        Erase timeVector
        Erase S
        Erase Sest
        Erase Xobs
        Erase X
        Erase Z
    End Sub


    ''' <summary>
    ''' Performs the matrix multiplication of two matrices A and B (A * B).
    ''' </summary>
    ''' <param name="A">The left matrix (rows1 x columns1).</param>
    ''' <param name="rows1">Number of rows of A.</param>
    ''' <param name="columns1">Number of columns of A (must equal rows of B).</param>
    ''' <param name="B">The right matrix (rows2 x columns2).</param>
    ''' <param name="rows2">Number of rows of B.</param>
    ''' <param name="columns2">Number of columns of B.</param>
    ''' <returns>The product matrix Sp = A * B (rows1 x columns2).</returns>
    Public Function MatMult(ByVal A As Double()(), ByVal rows1 As Integer, ByVal columns1 As Integer, ByVal B As Double()(), ByVal rows2 As Integer, ByVal columns2 As Integer) As Double()()
        Dim Sp As Double()() = RectangularArray.Matrix(Of Double)(columns2, rows1)

        For i As Integer = 0 To rows1 - 1
            For j As Integer = 0 To columns2 - 1
                For k As Integer = 0 To columns1 - 1
                    Sp(i)(j) += A(i)(k) * B(k)(j)
                Next
            Next
        Next

        Return Sp
    End Function

    ''' <summary>
    ''' Performs the multiplication of a row vector V by a matrix B (V * B).
    ''' </summary>
    ''' <param name="V">The row vector (passed ByRef).</param>
    ''' <param name="SizeVec">Length of the vector V (must equal rows of B).</param>
    ''' <param name="B">The matrix B (SizeVec x columns).</param>
    ''' <param name="columns">Number of columns of B.</param>
    ''' <returns>The result vector Sp = V * B (length columns).</returns>
    Public Function VecMatMult(ByRef V As Double(), ByVal SizeVec As Integer, ByVal B As Double()(), ByVal columns As Integer) As Double()
        Dim Sp As Double() = New Double(columns - 1) {}

        For i As Integer = 0 To columns - 1
            For k As Integer = 0 To SizeVec - 1
                Sp(i) += V(k) * B(k)(i)
            Next
        Next

        Return Sp
    End Function

    ''' <summary>
    ''' Performs the multiplication of a matrix B by a column vector V (B * V).
    ''' </summary>
    ''' <param name="B">The matrix B (rows x columns).</param>
    ''' <param name="rows">Number of rows of B.</param>
    ''' <param name="columns">Number of columns of B (must equal length of V).</param>
    ''' <param name="V">The column vector (passed ByRef).</param>
    ''' <returns>The result vector Sp = B * V (length rows).</returns>
    Public Function MatVecMult(ByVal B As Double()(), ByVal rows As Integer, ByVal columns As Integer, ByRef V As Double()) As Double()
        Dim Sp As Double() = New Double(rows - 1) {}

        For i As Integer = 0 To rows - 1
            For k As Integer = 0 To columns - 1
                Sp(i) += B(i)(k) * V(k)
            Next
        Next

        Return Sp
    End Function

    ''' <summary>
    ''' Computes the transpose of matrix A. The result Sp = A' (Matlab notation) is returned.
    ''' </summary>
    ''' <param name="A">The input matrix (rows x columns).</param>
    ''' <param name="rows">Number of rows of A.</param>
    ''' <param name="columns">Number of columns of A.</param>
    ''' <returns>The transposed matrix Sp (columns x rows).</returns>
    Public Function MatTranspose(ByVal A As Double()(), ByVal rows As Integer, ByVal columns As Integer) As Double()()
        Dim Sp = RectangularArray.Matrix(Of Double)(rows, columns)

        For i As Integer = 0 To columns - 1
            For j As Integer = 0 To rows - 1
                Sp(i)(j) = A(j)(i)
            Next
        Next

        Return Sp
    End Function


    ''' <summary>
    ''' Normalizes a vector <paramref name="wp"/> to unit length (L2 norm == 1).
    ''' </summary>
    ''' <param name="wp">The vector to normalize (passed ByRef, modified in place).</param>
    ''' <param name="sizeVec">Length of the vector.</param>
    Public Sub VectorNormalization(ByRef wp As Double(), ByVal sizeVec As Integer)
        Dim sqrtwpwp As Double = 0.0

        For i As Integer = 0 To sizeVec - 1
            sqrtwpwp += wp(i) * wp(i)
        Next

        For i As Integer = 0 To sizeVec - 1
            wp(i) = wp(i) / std.Sqrt(sqrtwpwp)
        Next
    End Sub

    ''' <summary>
    ''' Computes the eigenvalues and eigenvectors of a real, symmetric N x N matrix.
    ''' </summary>
    ''' <param name="ExxT">The input symmetric matrix (N x N).</param>
    ''' <param name="N">Matrix dimension.</param>
    ''' <param name="EigVectors">Output eigenvectors matrix (N x N), modified in place.</param>
    ''' <param name="EigValues">Output eigenvalues vector (length N), returned ByRef.</param>
    ''' <param name="iterations">Number of decomposition iterations.</param>
    ''' <remarks>
    ''' Uses a Jacobi-like iterative rotation with Gram-Schmidt orthogonalization
    ''' of the eigenvectors during each iteration.
    ''' </remarks>
    Public Sub EigenDecomposition(ByVal ExxT As Double()(), ByVal N As Integer, ByVal EigVectors As Double()(), ByRef EigValues As Double(), ByVal iterations As Integer)
        Dim EigVecs As Double()() = RectangularArray.Matrix(Of Double)(N, N)
        Dim Q As Double()() = RectangularArray.Matrix(Of Double)(N, N)
        Dim EigVals As Double()() = RectangularArray.Matrix(Of Double)(N, N)
        Dim wp As Double() = New Double(N - 1) {}
        Dim dumsum As Double() = New Double(N - 1) {}
        Dim R As Double()() = RectangularArray.Matrix(Of Double)(N, N)
        Dim QT As Double()() = RectangularArray.Matrix(Of Double)(N, N)
        Dim f As Double

        ' Initializing Ait, matrix containing eigenvalues as the diagonal
        For i As Integer = 0 To N - 1
            For j As Integer = 0 To N - 1
                EigVals(i)(j) = ExxT(i)(j)
            Next
        Next

        ' Initializing Q and E for computation of eigenvectors
        For i As Integer = 0 To N - 1
            Q(i)(i) = 1.0
            EigVecs(i)(i) = 1.0
        Next

        ' Eigen decomposition iterations
        For it As Integer = 0 To iterations - 1
            ' Gram-Schmidt decorrelation
            For p As Integer = 0 To N - 1
                For i As Integer = 0 To N - 1
                    wp(i) = EigVals(i)(p)
                Next

                VectorNormalization(wp, N)

                For i As Integer = 0 To N - 1
                    dumsum(i) = 0.0
                Next

                For i As Integer = 0 To N - 1
                    For j As Integer = 0 To p - 1
                        f = 0.0

                        For k As Integer = 0 To N - 1
                            f += wp(k) * Q(k)(j)
                        Next

                        dumsum(i) += f * Q(i)(j)
                    Next
                Next

                For i As Integer = 0 To N - 1
                    wp(i) -= dumsum(i)
                Next

                VectorNormalization(wp, N)

                ' Storing estimated rows of the inverse of the mixing matrix as columns in W
                For i As Integer = 0 To N - 1
                    Q(i)(p) = wp(i)
                Next
            Next

            QT = MatTranspose(Q, N, N)

            R = MatMult(QT, N, N, EigVals, N, N)

            EigVals = MatMult(R, N, N, Q, N, N)

            EigVecs = MatMult(EigVecs, N, N, Q, N, N)
        Next

        EigVecs = MatMult(EigVecs, N, N, Q, N, N)

        For i As Integer = 0 To N - 1
            EigValues(i) = EigVals(i)(i)
        Next

        For i As Integer = 0 To N - 1
            For j As Integer = 0 To N - 1
                EigVectors(i)(j) = EigVecs(i)(j)
            Next
        Next
    End Sub








    ''' <summary>
    ''' Sets up the various parameters and allocates the working matrices/vectors used by the algorithm.
    ''' </summary>
    Public Sub setupVars()
        periodSource5 = (finalTime - initialTime) / na
        periodSource6 = (finalTime - initialTime) / ns / 2

        timeVector = New Double(M - 1) {}

        Amix = RectangularArray.Matrix(Of Double)(N, N)
        W = RectangularArray.Matrix(Of Double)(N, N)
        WT = RectangularArray.Matrix(Of Double)(N, N)

        ' Generating Data for ICA

        S = RectangularArray.Matrix(Of Double)(M, N)
        Sest = RectangularArray.Matrix(Of Double)(M, N)
        Xobs = RectangularArray.Matrix(Of Double)(M, N)
        X = RectangularArray.Matrix(Of Double)(M, N)
        Z = RectangularArray.Matrix(Of Double)(M, N)
    End Sub




    ''' <summary>
    ''' Main entry point of the FastICA algorithm. Orchestrates parameter input, data generation,
    ''' preprocessing, the FastICA core, estimation output and cleanup, and prints the elapsed time.
    ''' </summary>
    ''' <returns>0 on completion.</returns>
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




    ''' <summary>
    ''' Sets the user-configurable parameters of the algorithm. This is the only section that the
    ''' user is expected to modify.
    ''' </summary>
    ''' <remarks>
    ''' Six sources are available by default. To use more sources they must be added in
    ''' <see cref="SetUpSources"/> and <see cref="funcSource1"/> ... <see cref="funcSource6"/>; to use
    ''' fewer, remove the excess from <see cref="SetUpSources"/>.
    ''' </remarks>
    Public Sub ParameterInput()
        N = 6 'The number of sources. (It is preferable not to change this value)
        C = N 'The number of observations (sample size). This value should be equal to N and should not be changed!
        M = 10000 'The number of observation samples

        K = 0.1F 'The slope of the zig-zag source
        na = 8.0F 'The amount of zig-zag source periods (amount of peaks)
        ns = 5.0F 'The amount of alternating step-function periods

        finalTime = 40.0F * 3.14159274F 'Final time (s)
        initialTime = 0.0F 'Initial time (s)

        iterations = 100 'Number of FastICA iterations
    End Sub




    ''' <summary>First source signal: a sine wave with angular frequency 1.1.</summary>
    ''' <param name="x">The time value.</param>
    ''' <returns>The value of the source signal.</returns>
    Public Function funcSource1(ByVal x As Double) As Double
        Return std.Sin(1.1 * x)
    End Function

    ''' <summary>Second source signal: a cosine wave with angular frequency 0.25.</summary>
    ''' <param name="x">The time value.</param>
    ''' <returns>The value of the source signal.</returns>
    Public Function funcSource2(ByVal x As Double) As Double
        Return std.Cos(0.25 * x)
    End Function

    ''' <summary>Third source signal: a sine wave with angular frequency 0.1.</summary>
    ''' <param name="x">The time value.</param>
    ''' <returns>The value of the source signal.</returns>
    Public Function funcSource3(ByVal x As Double) As Double
        Return std.Sin(0.1 * x)
    End Function

    ''' <summary>Fourth source signal: a cosine wave with angular frequency 0.7.</summary>
    ''' <param name="x">The time value.</param>
    ''' <returns>The value of the source signal.</returns>
    Public Function funcSource4(ByVal x As Double) As Double
        Return std.Cos(0.7 * x)
    End Function

    ''' <summary>Fifth source signal: a saw-tooth (zig-zag) wave defined by the slope K and period <c>periodSource5</c>.</summary>
    ''' <param name="x">The time value.</param>
    ''' <returns>The value of the source signal.</returns>
    Public Function funcSource5(ByVal x As Double) As Double
        Return K * x - std.Floor(x / periodSource5) * K * periodSource5
    End Function

    ''' <summary>Sixth source signal: an alternating step function (+1 / -1) with period <c>periodSource6</c>.</summary>
    ''' <param name="x">The time value.</param>
    ''' <returns>The value of the source signal (+1 or -1).</returns>
    Public Function funcSource6(ByVal x As Double) As Double
        If CInt(std.Floor(x / periodSource6)) Mod 2 = 0 Then
            Return 1
        Else
            Return -1
        End If
    End Function

End Class
