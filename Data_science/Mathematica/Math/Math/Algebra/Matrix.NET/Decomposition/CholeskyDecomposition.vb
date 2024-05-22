#Region "Microsoft.VisualBasic::f84f019ccd2306b266e36c425fe0aa73, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Decomposition\CholeskyDecomposition.vb"

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

    '   Total Lines: 151
    '    Code Lines: 89 (58.94%)
    ' Comment Lines: 42 (27.81%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 20 (13.25%)
    '     File Size: 5.48 KB


    '     Class CholeskyDecomposition
    ' 
    '         Properties: SPD
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetL, Solve
    ' 
    '         Sub: ISerializable_GetObjectData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.Serialization

Namespace LinearAlgebra.Matrix

    ''' <summary>Cholesky Decomposition.
    ''' For a symmetric, positive definite matrix A, the Cholesky decomposition
    ''' is an lower triangular matrix L so that A = L*L'.
    ''' If the matrix is not symmetric or positive definite, the constructor
    ''' returns a partial decomposition and sets an internal flag that may
    ''' be queried by the isSPD() method.
    ''' </summary>

    <Serializable>
    Public Class CholeskyDecomposition
        Implements ISerializable
#Region "Class variables"

        ''' <summary>Array for internal storage of decomposition.
        ''' @serial internal array storage.
        ''' </summary>
        Private L As Double()()

        ''' <summary>Row and column dimension (square matrix).
        ''' @serial matrix dimension.
        ''' </summary>
        Private n As Integer

        ''' <summary>Symmetric and positive definite flag.
        ''' @serial is symmetric and positive definite flag.
        ''' </summary>
        Private isspd As Boolean

#End Region

#Region "Constructor"

        ''' <summary>
        ''' Cholesky algorithm for symmetric and positive definite matrix. returns Structure to access L and isspd flag.
        ''' </summary>
        ''' <param name="Arg">  Square, symmetric matrix.
        ''' </param>
        Public Sub New(Arg As GeneralMatrix)
            ' Initialize.
            Dim A As Double()() = Arg.ArrayPack
            n = Arg.RowDimension
            L = New Double(n - 1)() {}
            For i As Integer = 0 To n - 1
                L(i) = New Double(n - 1) {}
            Next
            isspd = (Arg.ColumnDimension = n)
            ' Main loop.
            For j As Integer = 0 To n - 1
                Dim Lrowj As Double() = L(j)
                Dim d As Double = 0.0
                For k As Integer = 0 To j - 1
                    Dim Lrowk As Double() = L(k)
                    Dim s As Double = 0.0
                    For i As Integer = 0 To k - 1
                        s += Lrowk(i) * Lrowj(i)
                    Next
                    s = (A(j)(k) - s) / L(k)(k)
                    Lrowj(k) = s
                    d = d + s * s
                    isspd = isspd And (A(k)(j) = A(j)(k))
                Next
                d = A(j)(j) - d
                isspd = isspd And (d > 0.0)
                L(j)(j) = System.Math.Sqrt(System.Math.Max(d, 0.0))
                For k As Integer = j + 1 To n - 1
                    L(j)(k) = 0.0
                Next
            Next
        End Sub

#End Region

#Region "Public Properties"
        ''' <summary>Is the matrix symmetric and positive definite?</summary>
        ''' <returns>     true if A is symmetric and positive definite.
        ''' </returns>
        Public Overridable ReadOnly Property SPD() As Boolean
            Get
                Return isspd
            End Get
        End Property
#End Region

#Region "Public Methods"

        ''' <summary>Return triangular factor.</summary>
        ''' <returns>     L
        ''' </returns>

        Public Overridable Function GetL() As GeneralMatrix
            Return New NumericMatrix(L, n, n)
        End Function

        ''' <summary>Solve A*X = B</summary>
        ''' <param name="B">  A Matrix with as many rows as A and any number of columns.
        ''' </param>
        ''' <returns>     X so that L*L'*X = B
        ''' </returns>
        ''' <exception cref="System.ArgumentException">  Matrix row dimensions must agree.
        ''' </exception>
        ''' <exception cref="System.SystemException"> Matrix is not symmetric positive definite.
        ''' </exception>

        Public Overridable Function Solve(B As GeneralMatrix) As GeneralMatrix
            If B.RowDimension <> n Then
                Throw New System.ArgumentException("Matrix row dimensions must agree.")
            End If
            If Not isspd Then
                Throw New System.SystemException("Matrix is not symmetric positive definite.")
            End If

            ' Copy right hand side.
            Dim X As Double()() = B.ArrayPack(deepcopy:=True)
            Dim nx As Integer = B.ColumnDimension

            ' Solve L*Y = B;
            For k As Integer = 0 To n - 1
                For i As Integer = k + 1 To n - 1
                    For j As Integer = 0 To nx - 1
                        X(i)(j) -= X(k)(j) * L(i)(k)
                    Next
                Next
                For j As Integer = 0 To nx - 1
                    X(k)(j) /= L(k)(k)
                Next
            Next

            ' Solve L'*X = Y;
            For k As Integer = n - 1 To 0 Step -1
                For j As Integer = 0 To nx - 1
                    X(k)(j) /= L(k)(k)
                Next
                For i As Integer = 0 To k - 1
                    For j As Integer = 0 To nx - 1
                        X(i)(j) -= X(k)(j) * L(k)(i)
                    Next
                Next
            Next
            Return New NumericMatrix(X, n, nx)
        End Function
#End Region

        ' A method called when serializing this class.
        Private Sub ISerializable_GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
        End Sub
    End Class
End Namespace
