#Region "Microsoft.VisualBasic::938c1221b2b40024076eca7303c01923, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\QRDecomposition.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.Serialization

Namespace Matrix

    ''' <summary>QR Decomposition.
    ''' For an m-by-n matrix A with m >= n, the QR decomposition is an m-by-n
    ''' orthogonal matrix Q and an n-by-n upper triangular matrix R so that
    ''' A = Q*R.
    ''' 
    ''' The QR decompostion always exists, even if the matrix does not have
    ''' full rank, so the constructor will never fail.  The primary use of the
    ''' QR decomposition is in the least squares solution of nonsquare systems
    ''' of simultaneous linear equations.  This will fail if IsFullRank()
    ''' returns false.
    ''' </summary>

    <Serializable>
    Public Class QRDecomposition
        Implements ISerializable

#Region "Class variables"

        ''' <summary>Array for internal storage of decomposition.
        ''' @serial internal array storage.
        ''' </summary>
        Private QR As Double()()

        ''' <summary>Row and column dimensions.
        ''' @serial column dimension.
        ''' @serial row dimension.
        ''' </summary>
        Private m As Integer, n As Integer

        ''' <summary>Array for internal storage of diagonal of R.
        ''' @serial diagonal of R.
        ''' </summary>
        Private Rdiag As Double()

#End Region

#Region "Constructor"

        ''' <summary>
        ''' QR Decomposition, computed by Householder reflections. returns Structure to access R and the Householder vectors and compute Q.
        ''' </summary>
        ''' <param name="A">   Rectangular matrix
        ''' </param>
        Public Sub New(A As GeneralMatrix)
            ' Initialize.
            QR = A.ArrayCopy
            m = A.RowDimension
            n = A.ColumnDimension
            Rdiag = New Double(n - 1) {}

            ' Main loop.
            For k As Integer = 0 To n - 1
                ' Compute 2-norm of k-th column without under/overflow.
                Dim nrm As Double = 0
                For i As Integer = k To m - 1
                    nrm = Hypot(nrm, QR(i)(k))
                Next

                If nrm <> 0.0 Then
                    ' Form k-th Householder vector.
                    If QR(k)(k) < 0 Then
                        nrm = -nrm
                    End If
                    For i As Integer = k To m - 1
                        QR(i)(k) /= nrm
                    Next
                    QR(k)(k) += 1.0

                    ' Apply transformation to remaining columns.
                    For j As Integer = k + 1 To n - 1
                        Dim s As Double = 0.0
                        For i As Integer = k To m - 1
                            s += QR(i)(k) * QR(i)(j)
                        Next
                        s = (-s) / QR(k)(k)
                        For i As Integer = k To m - 1
                            QR(i)(j) += s * QR(i)(k)
                        Next
                    Next
                End If
                Rdiag(k) = -nrm
            Next
        End Sub

#End Region

#Region "Public Properties"

        ''' <summary>Is the matrix full rank?</summary>
        ''' <returns>     true if R, and hence A, has full rank.
        ''' </returns>
        Public Overridable ReadOnly Property FullRank() As Boolean
            Get
                For j As Integer = 0 To n - 1
                    If Rdiag(j) = 0 Then
                        Return False
                    End If
                Next
                Return True
            End Get
        End Property

        ''' <summary>Return the Householder vectors</summary>
        ''' <returns>     Lower trapezoidal matrix whose columns define the reflections
        ''' </returns>
        Public Overridable ReadOnly Property H() As GeneralMatrix
            Get
                Dim X As New GeneralMatrix(m, n)
                Dim Ha As Double()() = X.Array
                For i As Integer = 0 To m - 1
                    For j As Integer = 0 To n - 1
                        If i >= j Then
                            Ha(i)(j) = QR(i)(j)
                        Else
                            Ha(i)(j) = 0.0
                        End If
                    Next
                Next
                Return X
            End Get
        End Property


        ''' <summary>Return the upper triangular factor</summary>
        ''' <returns>     R
        ''' </returns>
        Public Overridable ReadOnly Property R() As GeneralMatrix
            Get
                Dim X As New GeneralMatrix(n, n)
                Dim Ra As Double()() = X.Array
                For i As Integer = 0 To n - 1
                    For j As Integer = 0 To n - 1
                        If i < j Then
                            Ra(i)(j) = QR(i)(j)
                        ElseIf i = j Then
                            Ra(i)(j) = Rdiag(i)
                        Else
                            Ra(i)(j) = 0.0
                        End If
                    Next
                Next
                Return X
            End Get
        End Property

        ''' <summary>Generate and return the (economy-sized) orthogonal factor</summary>
        ''' <returns>     Q
        ''' </returns>
        Public Overridable ReadOnly Property Q() As GeneralMatrix
            Get
                Dim X As New GeneralMatrix(m, n)
                Dim Qa As Double()() = X.Array
                For k As Integer = n - 1 To 0 Step -1
                    For i As Integer = 0 To m - 1
                        Qa(i)(k) = 0.0
                    Next
                    Qa(k)(k) = 1.0
                    For j As Integer = k To n - 1
                        If QR(k)(k) <> 0 Then
                            Dim s As Double = 0.0
                            For i As Integer = k To m - 1
                                s += QR(i)(k) * Qa(i)(j)
                            Next
                            s = (-s) / QR(k)(k)
                            For i As Integer = k To m - 1
                                Qa(i)(j) += s * QR(i)(k)
                            Next
                        End If
                    Next
                Next
                Return X
            End Get
        End Property
#End Region

#Region "Public Methods"

        ''' <summary>Least squares solution of A*X = B</summary>
        ''' <param name="B">   A Matrix with as many rows as A and any number of columns.
        ''' </param>
        ''' <returns>     X that minimizes the two norm of Q*R*X-B.
        ''' </returns>
        ''' <exception cref="System.ArgumentException"> Matrix row dimensions must agree.
        ''' </exception>
        ''' <exception cref="System.SystemException"> Matrix is rank deficient.
        ''' </exception>
        Public Overridable Function Solve(B As GeneralMatrix) As GeneralMatrix
            If B.RowDimension <> m Then
                Throw New System.ArgumentException("GeneralMatrix row dimensions must agree.")
            End If
            If Not Me.FullRank Then
                Throw New System.SystemException("Matrix is rank deficient.")
            End If

            ' Copy right hand side
            Dim nx As Integer = B.ColumnDimension
            Dim X As Double()() = B.ArrayCopy

            ' Compute Y = transpose(Q)*B
            For k As Integer = 0 To n - 1
                For j As Integer = 0 To nx - 1
                    Dim s As Double = 0.0
                    For i As Integer = k To m - 1
                        s += QR(i)(k) * X(i)(j)
                    Next
                    s = (-s) / QR(k)(k)
                    For i As Integer = k To m - 1
                        X(i)(j) += s * QR(i)(k)
                    Next
                Next
            Next
            ' Solve R*X = Y;
            For k As Integer = n - 1 To 0 Step -1
                For j As Integer = 0 To nx - 1
                    X(k)(j) /= Rdiag(k)
                Next
                For i As Integer = 0 To k - 1
                    For j As Integer = 0 To nx - 1
                        X(i)(j) -= X(k)(j) * QR(i)(k)
                    Next
                Next
            Next

            Return (New GeneralMatrix(X, n, nx).GetMatrix(0, n - 1, 0, nx - 1))
        End Function

#End Region

        ' A method called when serializing this class.
        Private Sub ISerializable_GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
        End Sub
    End Class
End Namespace
