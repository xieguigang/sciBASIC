#Region "Microsoft.VisualBasic::ef42b4d06bf19e5b8e3454794c2eb083, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\NumericMatrix.vb"

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

    '   Total Lines: 2010
    '    Code Lines: 1088 (54.13%)
    ' Comment Lines: 652 (32.44%)
    '    - Xml Docs: 93.56%
    ' 
    '   Blank Lines: 270 (13.43%)
    '     File Size: 73.16 KB


    '     Class NumericMatrix
    ' 
    '         Properties: ColumnDimension, ColumnPackedCopy, DiagonalVector, Dimension, RowDimension
    '                     RowPackedCopy
    ' 
    '         Constructor: (+13 Overloads) Sub New
    ' 
    '         Function: Abs, Add, AddEquals, ArrayLeftDivide, ArrayLeftDivideEquals
    '                   ArrayMultiply, ArrayMultiplyEquals, ArrayPack, ArrayRightDivide, ArrayRightDivideEquals
    '                   Block, chol, Clone, ColWise, Condition
    '                   ConvertJaggedToRectangularFlexible, Copy, (+2 Overloads) Create, Determinant, (+2 Overloads) Diagonal
    '                   DotMultiply, DotProduct, Eigen, Gauss, (+4 Overloads) GetMatrix
    '                   GetRectangularArray, (+2 Overloads) Identity, Inverse, Log, LUD
    '                   max, Max, Min, (+3 Overloads) Multiply, MultiplyEquals
    '                   Norm1, Norm2, NormF, NormInf, Number
    '                   One, Power, QRD, random, Rank
    '                   Resize, RowApply, RowVectors, RowWise, (+2 Overloads) Solve
    '                   SolveTranspose, (+2 Overloads) Subtract, SubtractEquals, SVD, ToString
    '                   Trace, Transpose, Zero
    ' 
    '         Sub: CheckMatrixDimensions, (+2 Overloads) Dispose, Finalize, (+4 Overloads) SetMatrix
    ' 
    '         Operators: (+5 Overloads) -, (+7 Overloads) *, (+3 Overloads) /, (+2 Overloads) ^, (+3 Overloads) +
    '                    <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Runtime.Serialization
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Parallel
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports randf2 = Microsoft.VisualBasic.Math.RandomExtensions

Namespace LinearAlgebra.Matrix

    ''' <summary>
    ''' ### .NET GeneralMatrix class.
    ''' 
    ''' The .NET <see cref="GeneralMatrix"/> Class provides the fundamental operations of numerical
    ''' linear algebra.  Various constructors create Matrices from two dimensional
    ''' arrays of double precision floating point numbers.  Various "gets" and
    ''' "sets" provide access to submatrices and matrix elements.  Several methods 
    ''' implement basic matrix arithmetic, including matrix addition and
    ''' multiplication, matrix norms, and element-by-element array operations.
    ''' Methods for reading and printing matrices are also included.  All the
    ''' operations in this version of the <see cref="GeneralMatrix"/> Class involve real matrices.
    ''' Complex matrices may be handled in a future version.
    ''' 
    ''' Five fundamental matrix decompositions, which consist of pairs or triples
    ''' of matrices, permutation vectors, and the like, produce results in five
    ''' decomposition classes.  These decompositions are accessed by the <see cref="GeneralMatrix"/>
    ''' class to compute solutions of simultaneous linear equations, determinants,
    ''' inverses and other matrix functions.  
    ''' 
    ''' The five decompositions are:
    ''' 
    ''' + <see cref="CholeskyDecomposition"/> of symmetric, positive definite matrices.
    ''' + <see cref="LUDecomposition"/> of rectangular matrices.
    ''' + <see cref="QRDecomposition"/> of rectangular matrices.
    ''' + <see cref="SingularValueDecomposition"/> of rectangular matrices.
    ''' + <see cref="EigenvalueDecomposition"/> of both symmetric and nonsymmetric square matrices.
    ''' 
    ''' Example of use:
    ''' 
    ''' Solve a linear system A x = b and compute the residual norm, ``||b - A x||``.
    ''' 
    ''' ```csharp
    ''' double[][] vals;
    ''' GeneralMatrix A = New NumericMatrix(vals);
    ''' GeneralMatrix b = GeneralMatrix.Random(3,1);
    ''' GeneralMatrix x = A.Solve(b);
    ''' GeneralMatrix r = A.Multiply(x).Subtract(b);
    ''' double rnorm = r.NormInf();
    ''' ```
    ''' </summary>
    ''' <author>  
    ''' The MathWorks, Inc. and the National Institute of Standards and Technology.
    ''' 
    ''' + http://www.codeproject.com/Articles/5835/DotNetMatrix-Simple-Matrix-Library-for-NET
    ''' + https://github.com/fiji/Jama/blob/master/src/main/java/Jama/Matrix.java
    ''' </author>
    ''' <version>  5 August 1998
    ''' </version>
    ''' <remarks>
    ''' Access the internal two-dimensional array.
    ''' Pointer to the two-dimensional array of matrix elements.
    ''' this numeric matrix object consist with a collection of <see cref="Vector"/> as rows.
    ''' </remarks>
    <Serializable>
    Public Class NumericMatrix : Inherits Vector(Of Double())
        Implements ICloneable
        Implements IDisposable
        Implements GeneralMatrix
        Implements INumericMatrix

#Region "Class variables"

        ''' <summary>Row and column dimensions.
        ''' @serial row dimension.
        ''' </summary>
        Dim m As Integer

        ''' <summary>
        ''' Row and column dimensions.
        ''' @serial column dimension.
        ''' </summary>
        Dim n As Integer

#End Region

#Region "Constructors"

        ''' <summary>
        ''' Create a new (column) RealMatrix using {@code v} as the
        ''' data for the unique column of the created matrix.
        ''' The input array Is copied.
        ''' </summary>
        ''' <param name="v">
        ''' Column vector holding data for new matrix.
        ''' </param>
        ''' <remarks>
        ''' 20230815 创建一个只有一列数据的矩阵，矩阵的行数
        ''' 等于<paramref name="v"/>的元素数量
        ''' </remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(v As Double())
            Call Me.New(v.Select(Function(vi) New Double() {vi}))
        End Sub

        ''' <summary>Construct an m-by-n matrix of zeros. m is row number and n is column number</summary>
        ''' <param name="m">Number of rows.</param>
        ''' <param name="n">Number of colums.</param>
        ''' <remarks>
        ''' m is row number and n is column number
        ''' </remarks>
        Public Sub New(m As Integer, n As Integer)
            'Dim A = New Double(m - 1)() {}

            Me.m = m
            Me.n = n

            'For i As Integer = 0 To m - 1
            '    A(i) = New Double(n - 1) {}
            'Next

            'buffer = A
            buffer = RectangularArray.Matrix(Of Double)(m, n)
        End Sub

        ''' <summary>Construct an m-by-n constant matrix.</summary>
        ''' <param name="m">   Number of rows.
        ''' </param>
        ''' <param name="n">   Number of colums.
        ''' </param>
        ''' <param name="s">   Fill the matrix with this scalar value.
        ''' </param>
        Public Sub New(m As Integer, n As Integer, s As Double)
            Me.m = m
            Me.n = n
            'Dim A = New Double(m - 1)() {}

            'For i As Integer = 0 To m - 1
            '    A(i) = New Double(n - 1) {}
            'Next
            'For i As Integer = 0 To m - 1
            '    For j As Integer = 0 To n - 1
            '        A(i)(j) = s
            '    Next
            'Next

            ' buffer = A
            buffer = RectangularArray.Matrix(m, n, s)
        End Sub

        Sub New(A As IEnumerable(Of IEnumerable(Of Double)))
            Call Me.New(A.Select(Function(i) i.ToArray))
        End Sub

        ''' <summary>Construct a matrix from a 2-D array.</summary>
        ''' <param name="A">Two-dimensional array of doubles.
        ''' </param>
        ''' <param name="t">
        ''' the given raw data parameter <paramref name="A"/> is in columns
        ''' required transpose of the matrix.
        ''' </param>
        ''' <exception cref="ArgumentException">All rows must have the same length
        ''' </exception>
        ''' <seealso cref="Create">
        ''' </seealso>
        Public Sub New(A As Double()(), Optional t As Boolean = False)
            If t Then
                A = A.MatrixTranspose.ToArray
            End If

            m = A.Length

            If m = 0 Then
                n = 0
            Else
                n = A(0).Length
            End If

            For i As Integer = 0 To m - 1
                If A(i).Length <> n Then
                    Dim total_len As Long = Aggregate ai As Double()
                                            In A
                                            Into Sum(CLng(ai.Length))

                    Throw New ArgumentException($"All rows must have the same length. total length of matrix vector {total_len} is not matched with the given matrix size: {m}x{n}!")
                End If
            Next

            Me.buffer = A
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(rows As IEnumerable(Of Vector))
            Call Me.New(rows.Select(Function(v) v.ToArray).ToArray)
        End Sub

        ''' <summary>
        ''' create a new numeric matrix based on a given collection of the row data vectors
        ''' </summary>
        ''' <param name="rows"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(rows As IEnumerable(Of Double()))
            Call Me.New(rows.ToArray)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(rows As IEnumerable(Of Single()))
            Call Me.New(rows.Select(Function(s) s.AsDouble))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(M As Double(,))
            Call Me.New(M.RowIterator.ToArray)
        End Sub

        ''' <summary>Construct a matrix quickly without checking arguments.</summary>
        ''' <param name="A">   Two-dimensional array of doubles.
        ''' </param>
        ''' <param name="m">   Number of rows.
        ''' </param>
        ''' <param name="n">   Number of colums.
        ''' </param>
        Public Sub New(A As Double()(), m As Integer, n As Integer)
            Me.buffer = A
            Me.m = m
            Me.n = n
        End Sub

        ''' <summary>
        ''' make the matrix value copy
        ''' </summary>
        ''' <param name="m"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(m As NumericMatrix)
            Call Me.New(m.buffer.Select(Function(r) r.ToArray).ToArray, m.m, m.n)
        End Sub

        ''' <summary>Construct a matrix from a one-dimensional packed array</summary>
        ''' <param name="vals">One-dimensional array of doubles, packed by columns (ala Fortran).
        ''' </param>
        ''' <param name="m">   Number of rows.
        ''' </param>
        ''' <exception cref="ArgumentException">   Array length must be a multiple of m.
        ''' </exception>
        Public Sub New(vals As Double(), m As Integer)
            Me.m = m
            n = (If(m <> 0, vals.Length \ m, 0))
            If m * n <> vals.Length Then
                Throw New ArgumentException("Array length must be a multiple of m.")
            End If
            Dim A = New Double(m - 1)() {}
            For i As Integer = 0 To m - 1
                A(i) = New Double(n - 1) {}
            Next
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    A(i)(j) = vals(i + j * m)
                Next
            Next

            buffer = A
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(m As INumericMatrix)
            Call Me.New(m.ArrayPack(deepcopy:=False))
        End Sub
#End Region

#Region "Public Properties"

        Default Public Overloads Property Item(flags As Boolean()()) As [Variant](Of GeneralMatrix, Double)
            Get
                Dim val As Double()() = New Double(buffer.Length - 1)() {}

                For i As Integer = 0 To val.Length - 1
                    Dim copy As Double() = val(i)
                    Dim bits As Boolean() = flags(i)

                    For idx As Integer = 0 To copy.Length - 1
                        If bits(idx) Then
                            copy(idx) = buffer(i)(idx)
                        End If
                    Next
                Next

                Return New [Variant](Of GeneralMatrix, Double)(New NumericMatrix(val))
            End Get
            Set
                If Value Like GetType(Double) Then
                    Dim dbl As Double = Value

                    For i As Integer = 0 To flags.Length - 1
                        Dim bits As Boolean() = flags(i)
                        Dim ref As Double() = buffer(i)

                        For idx As Integer = 0 To bits.Length - 1
                            If bits(idx) Then
                                ref(idx) = dbl
                            End If
                        Next
                    Next
                Else
                    Dim x As GeneralMatrix = Value

                    For i As Integer = 0 To flags.Length - 1
                        Dim bits As Boolean() = flags(i)
                        Dim ref As Double() = buffer(i)

                        For idx As Integer = 0 To bits.Length - 1
                            If bits(idx) Then
                                ref(idx) = x(i, idx)
                            End If
                        Next
                    Next
                End If
            End Set
        End Property

        ''' <summary>Make a one-dimensional column packed copy of the internal array.</summary>
        ''' <returns>     Matrix elements packed in a one-dimensional array by columns.
        ''' </returns>
        Public Overridable ReadOnly Property ColumnPackedCopy() As Double()
            Get
                Dim vals As Double() = New Double(m * n - 1) {}
                For i As Integer = 0 To m - 1
                    For j As Integer = 0 To n - 1
                        vals(i + j * m) = buffer(i)(j)
                    Next
                Next
                Return vals
            End Get
        End Property


        ''' <summary>Make a one-dimensional row packed copy of the internal array.</summary>
        ''' <returns>     Matrix elements packed in a one-dimensional array by rows.
        ''' </returns>
        Public Overridable ReadOnly Property RowPackedCopy() As Double()
            Get
                Dim vals As Double() = New Double(m * n - 1) {}
                For i As Integer = 0 To m - 1
                    For j As Integer = 0 To n - 1
                        vals(i * n + j) = buffer(i)(j)
                    Next
                Next
                Return vals
            End Get
        End Property

        ''' <summary>Get row dimension.</summary>
        ''' <returns>     m, the number of rows.
        ''' </returns>
        Public Overridable ReadOnly Property RowDimension() As Integer Implements GeneralMatrix.RowDimension
            Get
                Return m
            End Get
        End Property

        ''' <summary>Get column dimension.</summary>
        ''' <returns>     n, the number of columns.
        ''' </returns>
        Public Overridable ReadOnly Property ColumnDimension() As Integer Implements GeneralMatrix.ColumnDimension
            Get
                Return n
            End Get
        End Property

        ''' <summary>
        ''' get [n,m] shape data
        ''' </summary>
        ''' <returns>
        ''' the width is the number of columns(n) and the height is the number of rows(m)
        ''' </returns>
        Public ReadOnly Property Dimension As Size
            Get
                Return New Size(n, m)
            End Get
        End Property

        Public ReadOnly Property DiagonalVector As Vector
            Get
                Dim v As New List(Of Double)

                For i As Integer = 0 To m - 1
                    v += buffer(i)(i)
                Next

                Return New Vector(v)
            End Get
        End Property

#End Region

#Region "Public Methods"

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="i">the start location, col, x</param>
        ''' <param name="j">the start location, row, y</param>
        ''' <param name="p">the size of the block, nrows</param>
        ''' <param name="q">the size of the block, ncols</param>
        ''' <returns></returns>
        Public Function Block(i As Integer, j As Integer, p As Integer, q As Integer) As NumericMatrix
            Dim subrows As New List(Of Double())
            Dim v As Double()

            For row As Integer = j To j + p - 1
                v = New Double(q - 1) {}
                System.Array.ConstrainedCopy(buffer(row), i, v, 0, q)
                subrows.Add(v)
            Next

            Return New NumericMatrix(subrows)
        End Function

        ''' <summary>
        ''' 获取仅包含有一个元素的矩阵对象
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Number() As GeneralMatrix
            Return New NumericMatrix(0, 0)
        End Function

        ''' <summary>Construct a matrix from a copy of a 2-D array.</summary>
        ''' <param name="A">   Two-dimensional array of doubles.
        ''' </param>
        ''' <exception cref="System.ArgumentException">   All rows must have the same length
        ''' </exception>

        Public Shared Function Create(A As Double()()) As GeneralMatrix
            Dim m As Integer = A.Length
            Dim n As Integer = A(0).Length
            Dim X As New NumericMatrix(m, n)
            Dim C As Double()() = X.Array
            For i As Integer = 0 To m - 1
                If A(i).Length <> n Then
                    Throw New System.ArgumentException("All rows must have the same length.")
                End If
                For j As Integer = 0 To n - 1
                    C(i)(j) = A(i)(j)
                Next
            Next
            Return X
        End Function

        Public Shared Function Create(nrow As Integer, ncol As Integer) As NumericMatrix
            Return New NumericMatrix(nrow, ncol)
        End Function

        Public Function Abs() As NumericMatrix
            Dim X As New NumericMatrix(m, n)
            Dim C As Double()() = X.Array

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    C(i)(j) = System.Math.Abs(buffer(i)(j))
                Next
            Next

            Return X
        End Function

        ''' <summary>
        ''' Make a deep copy of a matrix
        ''' </summary>
        ''' <remarks>
        ''' this function will break the array class reference between the matrix instance
        ''' </remarks>
        Public Overridable Overloads Function Copy() As GeneralMatrix
            Dim X As New NumericMatrix(m, n)
            Dim C As Double()() = X.Array

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    C(i)(j) = buffer(i)(j)
                Next
            Next

            Return X
        End Function

        ''' <summary>Get a single element.</summary>
        ''' <param name="i">   Row index.
        ''' </param>
        ''' <param name="j">   Column index.
        ''' </param>
        ''' <returns>     A(i,j)
        ''' </returns>
        ''' <exception cref="System.IndexOutOfRangeException">  
        ''' </exception>

        Default Public Overloads Property Item(i As Integer, j As Integer) As Double Implements GeneralMatrix.X
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer(i)(j)
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As Double)
                buffer(i)(j) = value
            End Set
        End Property

        ''' <summary>Get a single element.</summary>
        ''' <param name="i">   Row index.
        ''' </param>
        ''' <param name="j">   Column index.
        ''' </param>
        ''' <returns>     A(i,j)
        ''' </returns>
        ''' <exception cref="System.IndexOutOfRangeException">  
        ''' </exception>

        Default Public Overloads Property Item(i As UInteger, j As UInteger) As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer(i)(j)
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As Double)
                buffer(i)(j) = value
            End Set
        End Property

        ''' <summary>
        ''' column projection via column index
        ''' </summary>
        ''' <remarks>
        ''' select column values for each row for create a new matrix
        ''' </remarks>
        ''' <param name="indices"></param>
        ''' <returns></returns>
        Default Public Overloads ReadOnly Property Item(indices As IEnumerable(Of Integer)) As GeneralMatrix Implements GeneralMatrix.X
            Get
                Dim index%() = indices.ToArray
                Dim subMAT = buffer _
                    .Select(Function(x) x.Takes(index)) _
                    .ToArray

                Return New NumericMatrix(subMAT)
            End Get
        End Property

        ''' <summary>
        ''' get/set data vector by row or by column
        ''' </summary>
        ''' <param name="i"></param>
        ''' <param name="byRow"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(i As Integer, Optional byRow As Boolean = True) As Vector Implements GeneralMatrix.X
            Get
                If byRow Then
                    Return buffer(i).AsVector
                Else
                    Return buffer.Select(Function(r) r(i)).AsVector
                End If
            End Get
            Set(value As Vector)
                If byRow Then
                    buffer(i) = value.Array
                Else
                    For rId As Integer = 0 To buffer.Length - 1
                        buffer(rId)(i) = value(rId)
                    Next
                End If
            End Set
        End Property

        Default Public Overloads ReadOnly Property Item(rowIdx As BooleanVector) As GeneralMatrix Implements GeneralMatrix.X
            Get
                Dim subMat As New List(Of Double())

                For i As Integer = 0 To buffer.Length - 1
                    If rowIdx(i) Then
                        subMat.Add(buffer(i))
                    End If
                Next

                Return New NumericMatrix(subMat.ToArray)
            End Get
        End Property

        ''' <summary>Get a submatrix.</summary>
        ''' <param name="i0">  Initial row index
        ''' </param>
        ''' <param name="i1">  Final row index
        ''' </param>
        ''' <param name="j0">  Initial column index
        ''' </param>
        ''' <param name="j1">  Final column index
        ''' </param>
        ''' <returns>     A(i0:i1,j0:j1)
        ''' </returns>
        ''' <exception cref="System.IndexOutOfRangeException">   Submatrix indices
        ''' </exception>

        Public Overridable Function GetMatrix(i0 As Integer, i1 As Integer, j0 As Integer, j1 As Integer) As GeneralMatrix
            Dim X As New NumericMatrix(i1 - i0 + 1, j1 - j0 + 1)
            Dim B As Double()() = X.Array
            Try
                For i As Integer = i0 To i1
                    For j As Integer = j0 To j1
                        B(i - i0)(j - j0) = buffer(i)(j)
                    Next
                Next
            Catch e As System.IndexOutOfRangeException
                Throw New System.IndexOutOfRangeException("Submatrix indices", e)
            End Try
            Return X
        End Function

        ''' <summary>Get a submatrix.</summary>
        ''' <param name="r">   Array of row indices.
        ''' </param>
        ''' <param name="c">   Array of column indices.
        ''' </param>
        ''' <returns>     A(r(:),c(:))
        ''' </returns>
        ''' <exception cref="System.IndexOutOfRangeException">   Submatrix indices
        ''' </exception>

        Public Overridable Function GetMatrix(r As Integer(), c As Integer()) As GeneralMatrix
            Dim X As New NumericMatrix(r.Length, c.Length)
            Dim B As Double()() = X.Array
            Try
                For i As Integer = 0 To r.Length - 1
                    For j As Integer = 0 To c.Length - 1
                        B(i)(j) = buffer(r(i))(c(j))
                    Next
                Next
            Catch e As System.IndexOutOfRangeException
                Throw New System.IndexOutOfRangeException("Submatrix indices", e)
            End Try
            Return X
        End Function

        ''' <summary>Get a submatrix.</summary>
        ''' <param name="i0">  Initial row index
        ''' </param>
        ''' <param name="i1">  Final row index
        ''' </param>
        ''' <param name="c">   Array of column indices.
        ''' </param>
        ''' <returns>     A(i0:i1,c(:))
        ''' </returns>
        ''' <exception cref="System.IndexOutOfRangeException">   Submatrix indices
        ''' </exception>

        Public Overridable Function GetMatrix(i0 As Integer, i1 As Integer, c As Integer()) As GeneralMatrix
            Dim X As New NumericMatrix(i1 - i0 + 1, c.Length)
            Dim B As Double()() = X.Array
            Try
                For i As Integer = i0 To i1
                    For j As Integer = 0 To c.Length - 1
                        B(i - i0)(j) = buffer(i)(c(j))
                    Next
                Next
            Catch e As System.IndexOutOfRangeException
                Throw New System.IndexOutOfRangeException("Submatrix indices", e)
            End Try
            Return X
        End Function

        ''' <summary>Get a submatrix.</summary>
        ''' <param name="r">   
        ''' Array of row indices.
        ''' </param>
        ''' <param name="j0">  
        ''' Initial column index
        ''' </param>
        ''' <param name="j1">  
        ''' Final column index
        ''' </param>
        ''' <returns>     
        ''' A(r(:),j0:j1)
        ''' </returns>
        ''' <exception cref="System.IndexOutOfRangeException"> Submatrix indices
        ''' </exception>

        Public Overridable Function GetMatrix(r As Integer(), j0 As Integer, j1 As Integer) As GeneralMatrix Implements GeneralMatrix.GetMatrix
            Dim X As New NumericMatrix(r.Length, j1 - j0 + 1)
            Dim B As Double()() = X.Array
            Try
                For i As Integer = 0 To r.Length - 1
                    For j As Integer = j0 To j1
                        B(i)(j - j0) = buffer(r(i))(j)
                    Next
                Next
            Catch e As System.IndexOutOfRangeException
                Throw New System.IndexOutOfRangeException("Submatrix indices", e)
            End Try
            Return X
        End Function

        ''' <summary>Set a submatrix.</summary>
        ''' <param name="i0">  
        ''' Initial row index
        ''' </param>
        ''' <param name="i1">  
        ''' Final row index
        ''' </param>
        ''' <param name="j0">  
        ''' Initial column index
        ''' </param>
        ''' <param name="j1">  
        ''' Final column index
        ''' </param>
        ''' <param name="X">   
        ''' A(i0:i1,j0:j1)
        ''' </param>
        ''' <exception cref="System.IndexOutOfRangeException">  Submatrix indices
        ''' </exception>

        Public Overridable Sub SetMatrix(i0 As Integer, i1 As Integer, j0 As Integer, j1 As Integer, X As GeneralMatrix)
            Try
                For i As Integer = i0 To i1
                    For j As Integer = j0 To j1
                        buffer(i)(j) = X(i - i0, j - j0)
                    Next
                Next
            Catch e As IndexOutOfRangeException
                Throw New IndexOutOfRangeException("Submatrix indices", e)
            End Try
        End Sub

        ''' <summary>Set a submatrix.</summary>
        ''' <param name="r">   
        ''' Array of row indices.
        ''' </param>
        ''' <param name="c">   
        ''' Array of column indices.
        ''' </param>
        ''' <param name="X">   
        ''' A(r(:),c(:))
        ''' </param>
        ''' <exception cref="IndexOutOfRangeException">  Submatrix indices
        ''' </exception>

        Public Overridable Sub SetMatrix(r As Integer(), c As Integer(), X As GeneralMatrix)
            Try
                For i As Integer = 0 To r.Length - 1
                    For j As Integer = 0 To c.Length - 1
                        buffer(r(i))(c(j)) = X(i, j)
                    Next
                Next
            Catch e As IndexOutOfRangeException
                Throw New IndexOutOfRangeException("Submatrix indices", e)
            End Try
        End Sub

        ''' <summary>Set a submatrix.</summary>
        ''' <param name="r">   
        ''' Array of row indices.
        ''' </param>
        ''' <param name="j0">  
        ''' Initial column index
        ''' </param>
        ''' <param name="j1">  
        ''' Final column index
        ''' </param>
        ''' <param name="X">   
        ''' A(r(:),j0:j1)
        ''' </param>
        ''' <exception cref="System.IndexOutOfRangeException"> Submatrix indices
        ''' </exception>

        Public Overridable Sub SetMatrix(r As Integer(), j0 As Integer, j1 As Integer, X As GeneralMatrix)
            Try
                For i As Integer = 0 To r.Length - 1
                    For j As Integer = j0 To j1
                        buffer(r(i))(j) = X(i, j - j0)
                    Next
                Next
            Catch e As IndexOutOfRangeException
                Throw New IndexOutOfRangeException("Submatrix indices", e)
            End Try
        End Sub

        ''' <summary>Set a submatrix.</summary>
        ''' <param name="i0">  
        ''' Initial row index
        ''' </param>
        ''' <param name="i1">  
        ''' Final row index
        ''' </param>
        ''' <param name="c">   
        ''' Array of column indices.
        ''' </param>
        ''' <param name="X">   
        ''' A(i0:i1,c(:))
        ''' </param>
        ''' <exception cref="System.IndexOutOfRangeException">  Submatrix indices
        ''' </exception>

        Public Overridable Sub SetMatrix(i0 As Integer, i1 As Integer, c As Integer(), X As GeneralMatrix)
            Try
                For i As Integer = i0 To i1
                    For j As Integer = 0 To c.Length - 1
                        buffer(i)(c(j)) = X(i - i0, j)
                    Next
                Next
            Catch e As System.IndexOutOfRangeException
                Throw New System.IndexOutOfRangeException("Submatrix indices", e)
            End Try
        End Sub

        ''' <summary>Matrix transpose.</summary>
        ''' <returns>    
        ''' A'
        ''' </returns>
        ''' <remarks>
        ''' make a value copy of the matrix 
        ''' </remarks>
        Public Overridable Function Transpose() As GeneralMatrix Implements GeneralMatrix.Transpose
            Dim X As New NumericMatrix(n, m)
            Dim C As Double()() = X.Array
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    C(j)(i) = buffer(i)(j)
                Next
            Next
            Return X
        End Function

        ''' <summary>One norm</summary>
        ''' <returns>    
        ''' maximum column sum.
        ''' </returns>

        Public Overridable Function Norm1() As Double
            Dim f As Double = 0
            For j As Integer = 0 To n - 1
                Dim s As Double = 0
                For i As Integer = 0 To m - 1
                    s += System.Math.Abs(buffer(i)(j))
                Next
                f = System.Math.Max(f, s)
            Next
            Return f
        End Function

        ''' <summary>Two norm</summary>
        ''' <returns>    
        ''' maximum singular value.
        ''' </returns>

        Public Overridable Function Norm2() As Double
            Return (New SingularValueDecomposition(Me).Norm2())
        End Function

        ''' <summary>Infinity norm</summary>
        ''' <returns>    
        ''' maximum row sum.
        ''' </returns>

        Public Overridable Function NormInf() As Double
            Dim f As Double = 0

            For i As Integer = 0 To m - 1
                Dim s As Double = 0

                For j As Integer = 0 To n - 1
                    s += System.Math.Abs(buffer(i)(j))
                Next

                f = System.Math.Max(f, s)
            Next
            Return f
        End Function

        ''' <summary>Frobenius norm</summary>
        ''' <returns>    
        ''' sqrt of sum of squares of all elements.
        ''' </returns>

        Public Overridable Function NormF() As Double
            Dim f As Double = 0
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    f = Hypot(f, buffer(i)(j))
                Next
            Next
            Return f
        End Function

        ''' <summary>Unary minus</summary>
        ''' <returns>    -A
        ''' </returns>
        Public Shared Operator -(m As NumericMatrix) As NumericMatrix
            Dim X As New NumericMatrix(m.m, m.n)
            Dim C As Double()() = X.Array
            For i As Integer = 0 To m.m - 1
                For j As Integer = 0 To m.n - 1
                    C(i)(j) = -m.buffer(i)(j)
                Next
            Next
            Return X
        End Operator

        ''' <summary>C = A + B</summary>
        ''' <param name="B">   
        ''' another matrix
        ''' </param>
        ''' <returns>     
        ''' A + B
        ''' </returns>

        Public Overridable Function Add(B As GeneralMatrix) As GeneralMatrix
            CheckMatrixDimensions(B)
            Dim X As New NumericMatrix(m, n)
            Dim C As Double()() = X.Array
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    C(i)(j) = buffer(i)(j) + B(i, j)
                Next
            Next
            Return X
        End Function

        ''' <summary>A = A + B</summary>
        ''' <param name="B">   
        ''' another matrix
        ''' </param>
        ''' <returns>     
        ''' A + B
        ''' </returns>

        Public Overridable Function AddEquals(B As GeneralMatrix) As GeneralMatrix
            CheckMatrixDimensions(B)
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    buffer(i)(j) = buffer(i)(j) + B(i, j)
                Next
            Next
            Return Me
        End Function

        ''' <summary>C = A - B</summary>
        ''' <param name="B">   
        ''' another matrix
        ''' </param>
        ''' <returns>     
        ''' A - B
        ''' </returns>
        Public Overridable Function Subtract(B As GeneralMatrix) As GeneralMatrix
            Call CheckMatrixDimensions(B)

            Dim X As New NumericMatrix(m, n)
            Dim C As Double()() = X.Array

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    C(i)(j) = buffer(i)(j) - B(i, j)
                Next
            Next

            Return X
        End Function

        ''' <summary>C = A - B</summary>
        ''' <param name="B">a numeric value
        ''' </param>
        ''' <returns>     
        ''' A - B
        ''' </returns>
        Public Overridable Function Subtract(B As Double) As GeneralMatrix
            Dim X As New NumericMatrix(m, n)
            Dim C As Double()() = X.Array

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    C(i)(j) = buffer(i)(j) - B
                Next
            Next

            Return X
        End Function

        ''' <summary>C = x ^ y</summary>
        ''' <param name="y">power
        ''' </param>
        ''' <returns>x ^ y
        ''' </returns>
        Public Overridable Function Power(y As Double) As GeneralMatrix
            Dim X As New NumericMatrix(m, n)
            Dim C As Double()() = X.Array

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    C(i)(j) = buffer(i)(j) ^ y
                Next
            Next

            Return X
        End Function

        Public Overridable Function Log(Optional newBase As Double = System.Math.E) As NumericMatrix
            Dim X As New NumericMatrix(m, n)
            Dim C As Double()() = X.Array

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    C(i)(j) = System.Math.Log(buffer(i)(j), newBase)
                Next
            Next

            Return X
        End Function

        ''' <summary>A = A - B</summary>
        ''' <param name="B">   
        ''' another matrix
        ''' </param>
        ''' <returns>     
        ''' A - B
        ''' </returns>

        Public Overridable Function SubtractEquals(B As GeneralMatrix) As GeneralMatrix
            CheckMatrixDimensions(B)
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    buffer(i)(j) = buffer(i)(j) - B(i, j)
                Next
            Next
            Return Me
        End Function

        ''' <summary>Element-by-element multiplication, C = A.*B</summary>
        ''' <param name="B">   
        ''' another matrix
        ''' </param>
        ''' <returns>     
        ''' A.*B
        ''' </returns>

        Public Overridable Function ArrayMultiply(B As GeneralMatrix) As GeneralMatrix
            CheckMatrixDimensions(B)
            Dim X As New NumericMatrix(m, n)
            Dim C As Double()() = X.Array
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    C(i)(j) = buffer(i)(j) * B(i, j)
                Next
            Next
            Return X
        End Function

        ''' <summary>Element-by-element multiplication in place, A = A.*B</summary>
        ''' <param name="B">   
        ''' another matrix
        ''' </param>
        ''' <returns>     
        ''' A.*B
        ''' </returns>

        Public Overridable Function ArrayMultiplyEquals(B As GeneralMatrix) As GeneralMatrix
            CheckMatrixDimensions(B)
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    buffer(i)(j) = buffer(i)(j) * B(i, j)
                Next
            Next
            Return Me
        End Function

        ''' <summary>Element-by-element right division, C = A./B</summary>
        ''' <param name="B">   
        ''' another matrix
        ''' </param>
        ''' <returns>     
        ''' A./B
        ''' </returns>

        Public Overridable Function ArrayRightDivide(B As GeneralMatrix) As GeneralMatrix
            Call CheckMatrixDimensions(B)

            Dim X As New NumericMatrix(m, n)
            Dim C As Double()() = X.Array

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    ' A / B
                    If buffer(i)(j) = 0.0 Then
                        C(i)(j) = 0.0
                    Else
                        C(i)(j) = buffer(i)(j) / B(i, j)
                    End If
                Next
            Next

            Return X
        End Function

        ''' <summary>Element-by-element right division in place, A = A./B</summary>
        ''' <param name="B">   
        ''' another matrix
        ''' </param>
        ''' <returns>     
        ''' A./B
        ''' </returns>

        Public Overridable Function ArrayRightDivideEquals(B As GeneralMatrix) As GeneralMatrix
            CheckMatrixDimensions(B)
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    buffer(i)(j) = buffer(i)(j) / B(i, j)
                Next
            Next
            Return Me
        End Function

        ''' <summary>Element-by-element left division, C = A.\B</summary>
        ''' <param name="B">   
        ''' another matrix
        ''' </param>
        ''' <returns>     
        ''' A.\B
        ''' </returns>

        Public Overridable Function ArrayLeftDivide(B As GeneralMatrix) As GeneralMatrix
            CheckMatrixDimensions(B)
            Dim X As New NumericMatrix(m, n)
            Dim C As Double()() = X.Array
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    C(i)(j) = B(i, j) / buffer(i)(j)
                Next
            Next
            Return X
        End Function

        ''' <summary>Element-by-element left division in place, A = A.\B</summary>
        ''' <param name="B">   
        ''' another matrix
        ''' </param>
        ''' <returns>     
        ''' A.\B
        ''' </returns>

        Public Overridable Function ArrayLeftDivideEquals(B As GeneralMatrix) As GeneralMatrix
            Call CheckMatrixDimensions(B)
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    buffer(i)(j) = B(i, j) / buffer(i)(j)
                Next
            Next
            Return Me
        End Function

        ''' <summary>Multiply a matrix by a scalar, ``C = s*A``</summary>
        ''' <param name="s">   
        ''' scalar
        ''' </param>
        ''' <returns>     
        ''' s*A
        ''' </returns>
        Public Overridable Function Multiply(s As Double) As GeneralMatrix
            Dim X As New NumericMatrix(m, n)
            Dim C As Double()() = X.Array
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    C(i)(j) = s * buffer(i)(j)
                Next
            Next
            Return X
        End Function

        ''' <summary>Multiply a matrix by a scalar, ``C = s*A``</summary>
        ''' <returns>
        ''' s*A
        ''' </returns>
        Public Overridable Function Multiply(v As Vector) As GeneralMatrix
            If RowDimension = v.Dim Then
                Return Me.RowMultiply(v)
            ElseIf ColumnDimension = v.Dim Then
                Return Me.ColumnMultiply(v)
            Else
                Throw New InvalidDataContractException($"the size of the vector(dim={v.Dim}) should be equals to the row dimension({RowDimension}) or column dimension({ColumnDimension}) in your matrix!")
            End If
        End Function

        Public Function DotMultiply(v As Vector) As Vector
            Dim out As Double() = New Double(Me.RowDimension - 1) {}
            Dim i As Integer = 0

            For Each row As Vector In Me.RowVectors
                out(i) = (row * v).Sum
                i += 1
            Next

            Return New Vector(out)
        End Function

        Public Function max(axis As Integer) As Vector
            If axis = 0 Then
                Return Enumerable.Range(0, ColumnDimension) _
                    .Select(Function(ci) Me.ColumnVector(ci).Max) _
                    .AsVector
            Else
                Return buffer.Select(Function(r) r.Max).AsVector
            End If
        End Function

        ''' <summary>
        ''' maxCoeff, get max value in current matrix
        ''' </summary>
        ''' <returns></returns>
        Public Function Max(<Out> Optional ByRef row As Integer = Nothing, <Out> Optional ByRef col As Integer = Nothing) As Double
            Dim maxVal As Double = Double.MinValue

            For i As Integer = 0 To buffer.Length - 1
                Dim r = buffer(i)

                For j As Integer = 0 To r.Length - 1
                    If r(j) > maxVal Then
                        row = i
                        col = j
                        maxVal = r(j)
                    End If
                Next
            Next

            Return maxVal
        End Function

        ''' <summary>
        ''' minCoeff, get min value in current matrix
        ''' </summary>
        ''' <param name="row"></param>
        ''' <param name="col"></param>
        ''' <returns></returns>
        Public Function Min(<Out> Optional ByRef row As Integer = Nothing, <Out> Optional ByRef col As Integer = Nothing) As Double
            Dim minVal As Double = Double.MaxValue

            For i As Integer = 0 To buffer.Length - 1
                Dim r = buffer(i)

                For j As Integer = 0 To r.Length - 1
                    If r(j) < minVal Then
                        row = i
                        col = j
                        minVal = r(j)
                    End If
                Next
            Next

            Return minVal
        End Function

        Public Function RowWise() As WiseOperation
            Return WiseOperation.RowWise(Me)
        End Function

        Public Function ColWise() As WiseOperation
            Return WiseOperation.ColWise(Me)
        End Function

        ''' <summary>Multiply a matrix by a scalar in place, A = s*A</summary>
        ''' <param name="s">   
        ''' scalar
        ''' </param>
        ''' <returns>     
        ''' replace A by s*A
        ''' </returns>

        Public Overridable Function MultiplyEquals(s As Double) As GeneralMatrix
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    buffer(i)(j) = s * buffer(i)(j)
                Next
            Next
            Return Me
        End Function

        ''' <summary>
        ''' Linear algebraic matrix multiplication, ``A * B``
        ''' 
        ''' ``Jama.Matrix.times``
        ''' </summary>
        ''' <param name="B">another matrix</param>
        ''' <returns>Matrix product, A * B</returns>
        ''' <exception cref="System.ArgumentException">Matrix inner dimensions must agree.
        ''' </exception>
        Public Overridable Function Multiply(B As NumericMatrix) As GeneralMatrix
            If B.RowDimension <> n AndAlso (RowDimension <> 1 AndAlso B.RowDimension <> 1) Then
                Throw New ArgumentException("GeneralMatrix inner dimensions must agree.")
            ElseIf B.RowDimension = 1 Then
                Return Multiply(B.RowVectors.First)
            ElseIf RowDimension = 1 Then
                Return B.Multiply(Me.RowVectors.First)
            End If

            Return DotProduct(B)
        End Function

#Region "Operator Overloading"

        ''' <summary>
        '''  Addition of matrices
        ''' </summary>
        ''' <param name="m1"></param>
        ''' <param name="m2"></param>
        ''' <returns></returns>
        Public Shared Operator +(m1 As NumericMatrix, m2 As GeneralMatrix) As GeneralMatrix
            Return m1.Add(m2)
        End Operator

        Public Shared Operator -(m As NumericMatrix, v As Vector) As NumericMatrix
            Return m + (-v)
        End Operator

        Public Shared Operator +(m As NumericMatrix, v As Vector) As NumericMatrix
            If m.ColumnDimension = v.Dim Then
                Return New NumericMatrix(m.RowVectors.Select(Function(ri) ri + v))
            ElseIf m.RowDimension = v.Dim Then
                Dim cols As New List(Of Double())

                For i As Integer = 0 To m.ColumnDimension - 1
                    cols.Add(m.ColumnVector(i) + v)
                Next

                Dim rows As New List(Of Vector)
                Dim idx As Integer

                For i As Integer = 0 To m.RowDimension - 1
                    idx = i
                    rows.Add(cols.Select(Function(j) j(idx)).AsVector)
                Next

                Return New NumericMatrix(rows)
            Else
                Throw New InvalidDataException
            End If
        End Operator

        Public Shared Operator +(x As Double, m1 As NumericMatrix) As NumericMatrix
            Dim y As New NumericMatrix(m1.m, m1.n)
            Dim C As Double()() = y.Array
            For i As Integer = 0 To m1.m - 1
                For j As Integer = 0 To m1.n - 1
                    C(i)(j) = x + m1(i, j)
                Next
            Next
            Return y
        End Operator

        ''' <summary>
        ''' Subtraction of matrices
        ''' </summary>
        ''' <param name="m1"></param>
        ''' <param name="m2"></param>
        ''' <returns></returns>
        Public Shared Operator -(m1 As NumericMatrix, m2 As GeneralMatrix) As NumericMatrix
            If m1.RowDimension <> m2.RowDimension OrElse m1.ColumnDimension <> m2.ColumnDimension Then
                If m1.RowDimension = 1 OrElse m2.RowDimension = 1 Then
                    Throw New NotImplementedException
                ElseIf m1.ColumnDimension = 1 OrElse m2.ColumnDimension = 1 Then
                    If m1.ColumnDimension = 1 Then
                        Dim v As Vector = m1.ColumnVector(Scan0)
                        Dim m As NumericMatrix = Subtraction.RowSubtraction(v, m2)
                        Return m
                    Else
                        Throw New NotImplementedException
                    End If
                Else
                    Throw New InvalidProgramException("the dimension size of two matrix must be agree!")
                End If
            Else
                Return m1.Subtract(m2)
            End If
        End Operator

        Public Shared Operator -(m1 As NumericMatrix, x As Double) As GeneralMatrix
            Return m1.Subtract(x)
        End Operator

        Public Shared Operator ^(m1 As NumericMatrix, y As Double) As GeneralMatrix
            Return m1.Power(y)
        End Operator

        Public Shared Operator ^(x As Double, m1 As NumericMatrix) As NumericMatrix
            Dim exp As New NumericMatrix(m1.m, m1.n)
            Dim C As Double()() = exp.Array

            For i As Integer = 0 To m1.m - 1
                For j As Integer = 0 To m1.n - 1
                    C(i)(j) = x ^ m1.buffer(i)(j)
                Next
            Next

            Return exp
        End Operator

        Public Shared Operator -(x As Double, m As NumericMatrix) As GeneralMatrix
            Dim Xmat As New NumericMatrix(m.RowDimension, m.ColumnDimension)
            Dim C As Double()() = Xmat.Array

            For i As Integer = 0 To m.RowDimension - 1
                For j As Integer = 0 To m.ColumnDimension - 1
                    C(i)(j) = x - m.buffer(i)(j)
                Next
            Next

            Return Xmat
        End Operator

        ''' <summary>
        ''' Element-by-element multiplication of two matrices
        ''' </summary>
        ''' <param name="m1"></param>
        ''' <param name="m2"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(m1 As NumericMatrix, m2 As GeneralMatrix) As GeneralMatrix
            Return m1.ArrayMultiply(B:=m2)
        End Operator

        ''' <summary>
        ''' Element-by-element multiplication of two matrices
        ''' </summary>
        ''' <param name="m1"></param>
        ''' <param name="m2"></param>
        ''' <returns></returns>
        Public Shared Operator *(m1 As GeneralMatrix, m2 As NumericMatrix) As GeneralMatrix
            Return New NumericMatrix(m1.RowVectors).ArrayMultiply(B:=m2)
        End Operator

        ''' <summary>
        ''' Element-by-element multiplication of two matrices
        ''' </summary>
        ''' <param name="m1"></param>
        ''' <param name="m2"></param>
        ''' <returns></returns>
        Public Shared Operator *(m1 As NumericMatrix, m2 As NumericMatrix) As NumericMatrix
            Return m1.ArrayMultiply(m2)
        End Operator

        Public Shared Operator *(m As NumericMatrix, v As Vector) As NumericMatrix
            Dim y As New NumericMatrix(m.RowDimension, m.ColumnDimension)
            Dim x As Double()() = m.Array

            For i As Integer = 0 To x.Length - 1
                Dim factor As Double = v(i)
                Dim newV As Vector = x(i).AsVector * factor
                x(i) = newV
            Next

            Return y
        End Operator

        ''' <summary>
        ''' Element-by-element right division
        ''' </summary>
        ''' <param name="m1"></param>
        ''' <param name="m2"></param>
        ''' <returns></returns>
        Public Shared Operator /(m1 As NumericMatrix, m2 As NumericMatrix) As NumericMatrix
            Return m1.ArrayRightDivide(m2)
        End Operator

        Public Shared Operator /(x As Double, m1 As NumericMatrix) As NumericMatrix
            Dim Xmat As New NumericMatrix(m1.RowDimension, m1.ColumnDimension)
            Dim C As Double()() = Xmat.Array

            For i As Integer = 0 To m1.RowDimension - 1
                For j As Integer = 0 To m1.ColumnDimension - 1
                    C(i)(j) = x / m1.buffer(i)(j)
                Next
            Next

            Return Xmat
        End Operator

        Public Shared Operator /(m1 As NumericMatrix, x As Double) As NumericMatrix
            Dim Xmat As New NumericMatrix(m1.RowDimension, m1.ColumnDimension)
            Dim C As Double()() = Xmat.Array

            For i As Integer = 0 To m1.RowDimension - 1
                For j As Integer = 0 To m1.ColumnDimension - 1
                    C(i)(j) = m1.buffer(i)(j) / x
                Next
            Next

            Return Xmat
        End Operator

        ''' <summary>
        ''' Multiplication of matrices
        ''' </summary>
        ''' <param name="m2"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(x As Double, m2 As NumericMatrix) As GeneralMatrix
            Return m2.Multiply(x)
        End Operator

        ''' <summary>
        ''' Multiplication of matrices
        ''' </summary>
        ''' <param name="m2"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(m2 As NumericMatrix, x As Double) As GeneralMatrix
            Return m2.Multiply(x)
        End Operator

        ''' <summary>
        ''' Multiplication of matrices
        ''' </summary>
        ''' <param name="m2"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(v As Vector, m2 As NumericMatrix) As GeneralMatrix
            Return m2.Multiply(v)
        End Operator

        Public Shared Operator =(w As NumericMatrix, xi As Double) As Boolean()()
            Dim flags As New List(Of Boolean())

            For Each row As Double() In w.buffer
                flags.Add(row.Select(Function(ci) ci = xi).ToArray)
            Next

            Return flags.ToArray
        End Operator

        Public Shared Operator <>(w As NumericMatrix, xi As Double) As Boolean()()
            Dim eq = w = xi

            For Each row As Boolean() In eq
                For i As Integer = 0 To row.Length - 1
                    row(i) = Not row(i)
                Next
            Next

            Return eq
        End Operator
#End Region

        ''' <summary>LU Decomposition</summary>
        ''' <returns>     LUDecomposition
        ''' </returns>
        ''' <seealso cref="LUDecomposition">
        ''' </seealso>

        Public Overridable Function LUD() As LUDecomposition
            Return New LUDecomposition(Me)
        End Function

        ''' <summary>QR Decomposition</summary>
        ''' <returns>     QRDecomposition
        ''' </returns>
        ''' <seealso cref="QRDecomposition">
        ''' </seealso>

        Public Overridable Function QRD() As QRDecomposition
            Return New QRDecomposition(Me)
        End Function

        ''' <summary>Cholesky Decomposition</summary>
        ''' <returns>     CholeskyDecomposition
        ''' </returns>
        ''' <seealso cref="CholeskyDecomposition">
        ''' </seealso>

        Public Overridable Function chol() As CholeskyDecomposition
            Return New CholeskyDecomposition(Me)
        End Function

        ''' <summary>Singular Value Decomposition</summary>
        ''' <returns>     SingularValueDecomposition
        ''' </returns>
        ''' <seealso cref="SingularValueDecomposition">
        ''' </seealso>

        Public Overridable Function SVD() As SingularValueDecomposition
            Return New SingularValueDecomposition(Me)
        End Function

        ''' <summary>Eigenvalue Decomposition</summary>
        ''' <returns>     EigenvalueDecomposition
        ''' </returns>
        ''' <seealso cref="EigenvalueDecomposition">
        ''' </seealso>

        Public Overridable Function Eigen() As EigenvalueDecomposition
            Return New EigenvalueDecomposition(Me)
        End Function

        ''' <summary>Solve A*X = B</summary>
        ''' <param name="B">   right hand side
        ''' </param>
        ''' <returns>     solution if A is square, least squares solution otherwise
        ''' </returns>

        Public Overridable Overloads Function Solve(B As GeneralMatrix) As GeneralMatrix
            Dim decompose As Decomposition

            If m = n Then
                Dim lu As New LUDecomposition(Me)
                decompose = lu
            Else
                Dim qr As New QRDecomposition(Me)
                decompose = qr
            End If

            Return decompose.Solve(B)
        End Function

        Public Overloads Function Solve(b As Double()) As Double()
            Dim colMat As New NumericMatrix(b)
            Dim a = Solve(colMat)
            Return a.ColumnVector(0).ToArray
        End Function

        ''' <summary>Solve X*A = B, which is also A'*X' = B'</summary>
        ''' <param name="B">   right hand side
        ''' </param>
        ''' <returns>     solution if A is square, least squares solution otherwise.
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function SolveTranspose(B As GeneralMatrix) As GeneralMatrix
            Return DirectCast(Transpose(), NumericMatrix).Solve(B.Transpose())
        End Function

        ''' <summary>Matrix inverse or pseudoinverse</summary>
        ''' <returns>     inverse(A) if A is square, pseudoinverse otherwise.
        ''' </returns>
        ''' <remarks>solve identity</remarks>
        Public Overridable Function Inverse() As GeneralMatrix
            Return Solve(Identity(m, m))
        End Function

        ''' <summary>GeneralMatrix determinant</summary>
        ''' <returns>     determinant
        ''' </returns>

        Public Overridable Function Determinant() As Double
            Return New LUDecomposition(Me).Determinant()
        End Function

        ''' <summary>GeneralMatrix rank</summary>
        ''' <returns>     effective numerical rank, obtained from SVD.
        ''' </returns>

        Public Overridable Function Rank() As Integer
            Return New SingularValueDecomposition(Me).Rank()
        End Function

        ''' <summary>Matrix condition (2 norm)</summary>
        ''' <returns>     ratio of largest to smallest singular value.
        ''' </returns>

        Public Overridable Function Condition() As Double
            Return New SingularValueDecomposition(Me).Condition()
        End Function

        ''' <summary>Matrix trace.</summary>
        ''' <returns>     sum of the diagonal elements.
        ''' </returns>

        Public Overridable Function Trace() As Double
            Dim t As Double = 0
            For i As Integer = 0 To System.Math.Min(m, n) - 1
                t += buffer(i)(i)
            Next
            Return t
        End Function

        ''' <summary>Generate identity matrix</summary>
        ''' <param name="m">   Number of rows.
        ''' </param>
        ''' <param name="n">   Number of colums.
        ''' </param>
        ''' <returns>     An m-by-n matrix with ones on the diagonal and zeros elsewhere.
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Identity(m As Integer, n As Integer) As NumericMatrix
            Return Diagonal(m, n, 1.0)
        End Function

        Public Shared Function Diagonal(m As Integer, n As Integer, xi As Double) As NumericMatrix
            Dim A As New NumericMatrix(m, n)
            Dim X As Double()() = A.Array
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    X(i)(j) = (If(i = j, xi, 0.0))
                Next
            Next
            Return A
        End Function

        Public Shared Function Diagonal(m As Integer, n As Integer, v As IEnumerable(Of Double)) As NumericMatrix
            Dim A As New NumericMatrix(m, n)
            Dim X As Double()() = A.Array
            Dim itr = v.GetEnumerator
            Dim getter As Func(Of Double) = Function() As Double
                                                itr.MoveNext()
                                                Return itr.Current
                                            End Function

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    If i = j Then
                        X(i)(j) = getter()
                    Else
                        X(i)(j) = 0.0
                    End If
                Next
            Next

            Return A
        End Function

        ''' <summary>
        ''' Create [m,m] identity matrix
        ''' </summary>
        ''' <param name="m"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Identity(m As Integer) As NumericMatrix
            Return Identity(m, m)
        End Function

#End Region

#Region "Private Methods"

        ''' <summary>Check if size(A) == size(B) *</summary>
        ''' <remarks>
        ''' m == m andalso n == n
        ''' </remarks>
        Private Sub CheckMatrixDimensions(B As GeneralMatrix)
            If B.RowDimension <> m OrElse B.ColumnDimension <> n Then
                Throw New ArgumentException("GeneralMatrix dimensions must agree.")
            End If
        End Sub
#End Region

#Region "Implement IDisposable"
        ''' <summary>
        ''' Do not make this method virtual.
        ''' A derived class should not be able to override this method.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
        End Sub

        ''' <summary>
        ''' Dispose(bool disposing) executes in two distinct scenarios.
        ''' If disposing equals true, the method has been called directly
        ''' or indirectly by a user's code. Managed and unmanaged resources
        ''' can be disposed.
        ''' If disposing equals false, the method has been called by the 
        ''' runtime from inside the finalizer and you should not reference 
        ''' other objects. Only unmanaged resources can be disposed.
        ''' </summary>
        ''' <param name="disposing"></param>
        Private Sub Dispose(disposing As Boolean)
            ' This object will be cleaned up by the Dispose method.
            ' Therefore, you should call GC.SupressFinalize to
            ' take this object off the finalization queue 
            ' and prevent finalization code for this object
            ' from executing a second time.
            If disposing Then
                GC.SuppressFinalize(Me)
            End If
        End Sub

        ''' <summary>
        ''' This destructor will run only if the Dispose method 
        ''' does not get called.
        ''' It gives your base class the opportunity to finalize.
        ''' Do not provide destructors in types derived from this class.
        ''' </summary>
        Protected Overrides Sub Finalize()
            Try
                ' Do not re-create Dispose clean-up code here.
                ' Calling Dispose(false) is optimal in terms of
                ' readability and maintainability.
                Dispose(False)
            Finally
                MyBase.Finalize()
            End Try
        End Sub
#End Region

        ''' <summary>
        ''' 调整矩阵的大小，并保留原有的数据
        ''' </summary>
        ''' <param name="m"></param>
        ''' <param name="n"></param>
        ''' <remarks></remarks>
        Public Function Resize(m As Integer, n As Integer) As GeneralMatrix Implements GeneralMatrix.Resize
            Me.m = m
            Me.n = n

            ReDim Preserve buffer(n - 1)

            For i As Integer = 0 To buffer.Length - 1
                ReDim Preserve buffer(i)(m - 1)
            Next

            Return Me
        End Function

        ''' <summary>
        ''' Clone the GeneralMatrix object.
        ''' </summary>
        Public Function Clone() As Object Implements ICloneable.Clone
            Return Me.Copy()
        End Function

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder($"[Row:{RowDimension}, Column:{ColumnDimension}]")

            If RowDimension * ColumnDimension < 25 Then
                For Each row As Double() In buffer
                    Call sb.AppendLine("[" & row.Select(Function(xi) xi.ToString("G4")).JoinBy(",") & "]")
                    Call sb.AppendLine()
                Next
            End If

            Return sb.ToString
        End Function

        Public Shared Function GetRectangularArray(m As GeneralMatrix) As Double(,)
            Dim x As Double()() = m.ArrayPack(deepcopy:=False)
            Dim a As Double(,) = ConvertJaggedToRectangularFlexible(x)
            Return a
        End Function

        Public Shared Function ConvertJaggedToRectangularFlexible(x As Double()()) As Double(,)
            If x Is Nothing Then Return Nothing
            If x.Length = 0 Then Return New Double(,) {}

            Dim maxColumns As Integer = x.Max(Function(row) row.Length)
            Dim y As Double(,) = New Double(x.Length - 1, maxColumns - 1) {}

            For rowIdx As Integer = 0 To x.Length - 1
                For colIdx As Integer = 0 To x(rowIdx).Length - 1
                    y(rowIdx, colIdx) = x(rowIdx)(colIdx)
                Next
            Next

            Return y
        End Function

        Public Shared Widening Operator CType(data#(,)) As NumericMatrix
            Return New NumericMatrix(data.RowIterator.ToArray)
        End Operator

        Public Shared Widening Operator CType(data#()()) As NumericMatrix
            Return New NumericMatrix(data)
        End Operator

        Public Iterator Function RowApply(Of T)(apply As Func(Of Double(), Integer, T)) As IEnumerable(Of T)
            Dim i As i32 = Scan0

            For Each row As Double() In buffer
                Yield apply(row, ++i)
            Next
        End Function

        Public Iterator Function RowVectors() As IEnumerable(Of Vector) Implements GeneralMatrix.RowVectors
            For Each row As Double() In buffer
                Yield row.AsVector
            Next
        End Function

        ''' <summary>Copy the internal two-dimensional array.</summary>
        ''' <returns>     Two-dimensional array copy of matrix elements.
        ''' </returns>
        Public Function ArrayPack(Optional deepcopy As Boolean = False) As Double()() Implements GeneralMatrix.ArrayPack
            If Not deepcopy Then
                Return buffer
            Else
                Dim makecopy As Double()() = New Double(m - 1)() {}
                For i As Integer = 0 To m - 1
                    makecopy(i) = New Double(n - 1) {}
                Next
                For i As Integer = 0 To m - 1
                    For j As Integer = 0 To n - 1
                        makecopy(i)(j) = buffer(i)(j)
                    Next
                Next
                Return makecopy
            End If
        End Function

        ''' <summary>
        ''' fill numeric value 1 to matrix with dimension size [<paramref name="columnDimension"/> x <paramref name="rowDimension"/>]
        ''' </summary>
        ''' <param name="columnDimension"></param>
        ''' <param name="rowDimension"></param>
        ''' <returns></returns>
        Public Shared Function One(columnDimension As Integer, rowDimension As Integer) As NumericMatrix
            Dim m As New NumericMatrix(rowDimension, columnDimension)
            Dim x = m.Array

            For i As Integer = 0 To rowDimension - 1
                For j As Integer = 0 To columnDimension - 1
                    x(i)(j) = 1
                Next
            Next

            Return m
        End Function

        ''' <summary>
        ''' create a random matrix with dimension size [<paramref name="columnDimension"/> x <paramref name="rowDimension"/>]
        ''' </summary>
        ''' <param name="columnDimension"></param>
        ''' <param name="rowDimension"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' element data range that generats inside the matrix is [-1,1]
        ''' </remarks>
        Public Shared Function Gauss(columnDimension As Integer, rowDimension As Integer) As NumericMatrix
            Dim m As New NumericMatrix(rowDimension, columnDimension)
            Dim x = m.Array

            For i As Integer = 0 To rowDimension - 1
                For j As Integer = 0 To columnDimension - 1
                    x(i)(j) = randf2.NextGaussian(mu:=0, sigma:=1)
                Next
            Next

            Return m
        End Function

        ''' <summary>
        ''' create a random matrix with dimension size [<paramref name="columnDimension"/> x <paramref name="rowDimension"/>]
        ''' </summary>
        ''' <param name="columnDimension"></param>
        ''' <param name="rowDimension"></param>
        ''' <returns></returns>
        Public Shared Function random(columnDimension As Integer, rowDimension As Integer,
                                      Optional min As Double = 0,
                                      Optional max As Double = 1) As NumericMatrix

            Dim m As New NumericMatrix(rowDimension, columnDimension)
            Dim x = m.Array

            For i As Integer = 0 To rowDimension - 1
                For j As Integer = 0 To columnDimension - 1
                    x(i)(j) = randf2.NextDouble(min, max)
                Next
            Next

            Return m
        End Function

        Public Shared Function Zero(columnDimension As Integer, rowDimension As Integer) As NumericMatrix
            Return New NumericMatrix(rowDimension, columnDimension)
        End Function

        ''' <summary>
        ''' 矩阵乘积(matrix product，也叫matmul product)：A 的列数必须和 B 的行数相等
        ''' </summary>
        ''' <param name="B"></param>
        ''' <returns></returns>
        Public Function DotProduct(B As GeneralMatrix) As GeneralMatrix Implements GeneralMatrix.Dot
            Dim c As Double()()

            If ParallelEnvironment.Enable AndAlso VectorTask.n_threads > 1 Then
                c = MatrixDotProduct.Resolve(buffer, B.ArrayPack)
            Else
                Dim Bcolj As Double() = New Double(n - 1) {}

                c = RectangularArray.Matrix(Of Double)(m, B.ColumnDimension)

                For j As Integer = 0 To B.ColumnDimension - 1
                    For k As Integer = 0 To n - 1
                        Bcolj(k) = B(k, j)
                    Next
                    For i As Integer = 0 To m - 1
                        Dim Arowi As Double() = buffer(i)
                        Dim s As Double = 0

                        For k As Integer = 0 To n - 1
                            s += Arowi(k) * Bcolj(k)
                        Next
                        c(i)(j) = s
                    Next
                Next
            End If

            Return New NumericMatrix(c)
        End Function
    End Class
End Namespace
