#Region "Microsoft.VisualBasic::83852c7657703bb4e0af1f7d742f10b9, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Decomposition\CholeskyDecomposition.vb"

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
    '    Code Lines: 85 (56.29%)
    ' Comment Lines: 42 (27.81%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 24 (15.89%)
    '     File Size: 5.46 KB


    '     Class CholeskyDecomposition
    ' 
    '         Properties: SPD
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetL, Solve
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SIMDIntrinsics = Microsoft.VisualBasic.Math.SIMD.SIMDIntrinsics

Namespace LinearAlgebra.Matrix

    ''' <summary>Cholesky Decomposition.
    ''' For a symmetric, positive definite matrix A, the Cholesky decomposition
    ''' is an lower triangular matrix L so that A = L*L'.
    ''' If the matrix is not symmetric or positive definite, the constructor
    ''' returns a partial decomposition and sets an internal flag that may
    ''' be queried by the isSPD() method.
    ''' </summary>
    Public Class CholeskyDecomposition

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

                    ' SIMD 化：L(j) 行尚未写入的元素（下标 >= j）均为 0，因此对
                    ' 两条整行做 FMA 点积与只累加前缀 [0, k) 的结果一致
                    Dim s As Double = SIMDIntrinsics.DotFma(Lrowk, Lrowj)

                    s = (A(j)(k) - s) / L(k)(k)
                    Lrowj(k) = s
                    d = d + s * s
                    isspd = isspd And (A(k)(j) = A(j)(k))
                Next
                d = A(j)(j) - d
                isspd = isspd And (d > 0.0)
                L(j)(j) = System.Math.Sqrt(System.Math.Max(d, 0.0))
                ' 右上角清零：块清零比逐元素赋值少一整轮下标计算
                If j + 1 < n Then
                    Call System.Array.Clear(L(j), j + 1, n - j - 1)
                End If
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

            ' Solve L*Y = B：列向推进的前代替换。
            ' 准确度修正：必须先将 X(k) 除以 L(k)(k) 得到 Y(k)，再把 Y(k) 的
            ' 贡献 AXPY 到后续各行。旧实现先 AXPY 后归一化，会把未除以
            ' L(k,k) 的值传播给后续行，导致 L(0,0) ≠ 1 时结果错误。
            For k As Integer = 0 To n - 1
                Dim rowK As Double() = X(k)
                Dim pivot As Double = L(k)(k)

                For j As Integer = 0 To nx - 1
                    rowK(j) /= pivot
                Next

                For i As Integer = k + 1 To n - 1
                    Call SIMDIntrinsics.AxpyInPlace(-L(i)(k), rowK, X(i))
                Next
            Next

            ' Solve L'*X = Y;
            For k As Integer = n - 1 To 0 Step -1
                Dim rowK As Double() = X(k)
                Dim pivot As Double = L(k)(k)

                For j As Integer = 0 To nx - 1
                    rowK(j) /= pivot
                Next
                For i As Integer = 0 To k - 1
                    Call SIMDIntrinsics.AxpyInPlace(-L(k)(i), rowK, X(i))
                Next
            Next
            Return New NumericMatrix(X, n, nx)
        End Function
#End Region

    End Class
End Namespace
