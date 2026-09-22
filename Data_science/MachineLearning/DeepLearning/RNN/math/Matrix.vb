#Region "Microsoft.VisualBasic::a0007061efa18dcad48292152512fe10, Data_science\MachineLearning\DeepLearning\RNN\math\Matrix.vb"

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

    '   Total Lines: 569
    '    Code Lines: 304 (53.43%)
    ' Comment Lines: 198 (34.80%)
    '    - Xml Docs: 91.92%
    ' 
    '   Blank Lines: 67 (11.78%)
    '     File Size: 22.49 KB


    '     Class Matrix
    ' 
    '         Properties: M, N, Vector
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) add, apply, (+3 Overloads) at, clip, (+2 Overloads) div
    '                   dot, exp, fromFlat, fromRaw, getk
    '                   (+2 Overloads) mul, neg, oneHot, oneHotIndex, (+2 Overloads) ones
    '                   onesLike, prod, raw, sum, T
    '                   tanh, unravel, (+2 Overloads) zeros, zerosLike
    ' 
    '         Sub: (+3 Overloads) setAt
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace RNN

    ''' <summary>
    ''' A dense M x N matrix of doubles used by the character level RNN implementation.
    ''' </summary>
    ''' <remarks>
    ''' When M = 1 or N = 1 the matrix is treated as a k-vector <c>v</c>. In matrix multiplication and element-wise
    ''' operations such a vector behaves like <c>v.T()</c>, so the row and column vector forms can be mixed freely.
    ''' </remarks>
    <Serializable>
    Public Class Matrix

        Private data As Double()() ' MxN

        ' Create 

        ''' <summary>
        ''' Creates a copy of another matrix.
        ''' </summary>
        ''' <param name="other">The matrix to copy; it must not be <c>Nothing</c>.</param>
        Public Sub New(other As Matrix)
            M = other.M
            N = other.N
            data = Utils.deepCopyOf(other.data)
        End Sub

        ''' <summary>
        ''' Creates a matrix from a rectangular jagged array.
        ''' </summary>
        ''' <param name="data">The M x N array; both dimensions must be greater than zero.</param>
        Private Sub New(data As Double()())
            M = Utils.arrayRows(data)
            N = Utils.arrayCols(data)

            If M = 0 OrElse N = 0 Then ' Don't accept 0 as a size.
                Throw New ArgumentException("One of the array dimensions is 0.")
            End If

            Me.data = data
        End Sub

        ''' <summary>
        ''' Creates a matrix from a row-major flat array.
        ''' </summary>
        ''' <param name="M">The row count; it must be greater than zero.</param>
        ''' <param name="data">The row-major flat data.</param>
        Private Sub New(M As Integer, data As Double())
            Me.M = M
            Me.N = data.Length Mod Me.M

            For i = 0 To Me.M - 1
                For j = 0 To N - 1
                    Me.data(i)(j) = data(M * i + j)
                Next
            Next
        End Sub

        ''' <summary>
        ''' Creates a matrix from a rectangular jagged array, copying the data.
        ''' </summary>
        ''' <param name="data">The M x N array; both dimensions must be greater than zero.</param>
        ''' <returns>The new matrix.</returns>
        Public Shared Function fromRaw(data As Double()()) As Matrix
            Return New Matrix(Utils.deepCopyOf(data))
        End Function

        ''' <summary>
        ''' Creates a matrix from a row-major flat array.
        ''' </summary>
        ''' <param name="M">The row count; it must be greater than zero.</param>
        ''' <param name="data">The row-major flat data.</param>
        ''' <returns>The new matrix.</returns>
        Public Shared Function fromFlat(M As Integer, data As Double()) As Matrix
            Return New Matrix(M, data)
        End Function

        ''' <summary>
        ''' Creates a matrix of all zeros.
        ''' </summary>
        ''' <param name="M">Row count; must be greater than zero.</param>
        ''' <param name="N">Column count; must be greater than zero.</param>
        ''' <returns>A zero filled matrix.</returns>
        Public Shared Function zeros(M As Integer, N As Integer) As Matrix
            Return New Matrix(RectangularArray.Matrix(Of Double)(M, N))
        End Function

        ''' <summary>
        ''' Creates a k dimensional zero vector.
        ''' </summary>
        ''' <param name="k">Vector length; must be greater than zero.</param>
        ''' <returns>A zero filled vector.</returns>
        Public Shared Function zeros(k As Integer) As Matrix
            Return zeros(1, k)
        End Function

        ''' <summary>
        ''' Creates a zero matrix shaped like another one.
        ''' </summary>
        ''' <param name="other">The template matrix; it must not be <c>Nothing</c>.</param>
        ''' <returns>A zero filled matrix with the same shape.</returns>
        Public Shared Function zerosLike(other As Matrix) As Matrix
            Return zeros(other.M, other.N)
        End Function

        ''' <summary>
        ''' Creates a matrix of all ones.
        ''' </summary>
        ''' <param name="M">Row count; must be greater than zero.</param>
        ''' <param name="N">Column count; must be greater than zero.</param>
        ''' <returns>A matrix filled with ones.</returns>
        Public Shared Function ones(M As Integer, N As Integer) As Matrix
            Return zeros(M, N).add(1.0)
        End Function

        ''' <summary>
        ''' Creates a k dimensional vector of ones.
        ''' </summary>
        ''' <param name="k">Vector length; must be greater than zero.</param>
        ''' <returns>A vector filled with ones.</returns>
        Public Shared Function ones(k As Integer) As Matrix
            Return ones(1, k)
        End Function

        ''' <summary>
        ''' Creates a matrix of ones shaped like another one.
        ''' </summary>
        ''' <param name="other">The template matrix.</param>
        ''' <returns>A matrix filled with ones and the same shape.</returns>
        Public Shared Function onesLike(other As Matrix) As Matrix
            Return ones(other.M, other.N)
        End Function

        ''' <summary>
        ''' Creates a one-hot vector, in which <c>v(i) = 1</c> and every other element is zero.
        ''' </summary>
        ''' <param name="k">Vector length.</param>
        ''' <param name="i">Index of the single one; requires <c>0 &lt;= i &lt; k</c>.</param>
        ''' <returns>The one-hot vector.</returns>
        Public Shared Function oneHot(k As Integer, i As Integer) As Matrix
            Dim v = zeros(k)
            v.setAt(i, 1.0)
            Return v
        End Function

        ''' <summary>
        ''' Computes the matrix product <c>a x b</c>, transposing <paramref name="b"/> when that makes the dimensions
        ''' compatible.
        ''' </summary>
        ''' <param name="a">The left matrix.</param>
        ''' <param name="b">The right matrix.</param>
        ''' <returns>The product matrix.</returns>
        ''' <exception cref="Exception">Thrown when the dimensions are incompatible.</exception>
        Public Shared Function dot(a As Matrix, b As Matrix) As Matrix
            ' System.out.println("a: " + a.M + "x" + a.N + "b:" + b.M + "x" + b.N);
            If a.N <> b.M Then ' if dimensions are not compatible
                If a.N = 1 AndAlso b.N = 1 Then ' if both are vectors
                    ' b^T is compatible (an outer product)
                    b = b.T()
                ElseIf b.M = 1 AndAlso a.N = b.N Then ' and the other dimension matches
                    ' b^T is compatible
                    b = b.T()
                Else
                    Throw New Exception("Incompatible dimensions for matrix multiplication.")
                End If
            End If

            Dim c = RectangularArray.Matrix(Of Double)(a.M, b.N)
            For i = 0 To a.M - 1
                For j = 0 To b.N - 1
                    For k = 0 To a.N - 1
                        c(i)(j) += a.data(i)(k) * b.data(k)(j)
                    Next
                Next
            Next

            Return New Matrix(c)
        End Function

        ''' <summary>Returns the transpose of this matrix.</summary>
        ''' <returns>A new matrix with rows and columns swapped.</returns>
        Public Overridable Function T() As Matrix
            Dim new_data = RectangularArray.Matrix(Of Double)(N, M)

            For i = 0 To M - 1
                For j = 0 To N - 1
                    new_data(j)(i) = data(i)(j)
                Next
            Next

            Return New Matrix(new_data)
        End Function

        ' Operators 

        ''' <summary>
        ''' Adds a scalar to every element, in place.
        ''' </summary>
        ''' <param name="x">The value to add.</param>
        ''' <returns>This matrix.</returns>
        Public Overridable Function add(x As Double) As Matrix
            For Each row In data
                For j = 0 To N - 1
                    row(j) += x
                Next
            Next
            Return Me
        End Function

        ''' <summary>
        ''' Adds another matrix element wise, in place. Row and column vectors of matching length are broadcast together.
        ''' </summary>
        ''' <param name="other">The matrix or vector to add.</param>
        ''' <returns>This matrix.</returns>
        ''' <exception cref="Exception">Thrown when the shapes are incompatible.</exception>
        Public Overridable Function add(other As Matrix) As Matrix
            If M = other.M AndAlso N = other.N Then ' compatible matrices
                For i = 0 To M - 1
                    For j = 0 To N - 1
                        data(i)(j) += other.data(i)(j)
                    Next
                Next
            ElseIf M = other.N AndAlso N = 1 AndAlso other.M = 1 Then
                ' this is a row vector and other is a column vector
                For i = 0 To M - 1
                    data(i)(0) += other.data(0)(i)
                Next
            ElseIf N = other.M AndAlso M = 1 AndAlso other.N = 1 Then
                ' this is a column vector and other is a row vector
                For i = 0 To N - 1
                    data(0)(i) += other.data(i)(0)
                Next
            Else
                Throw New Exception("Matrices/vectors incompatible for element-wise addition.")
            End If

            Return Me
        End Function

        ''' <summary>
        ''' Multiplies every element by a scalar, in place.
        ''' </summary>
        ''' <param name="x">The scale factor.</param>
        ''' <returns>This matrix.</returns>
        Public Overridable Function mul(x As Double) As Matrix
            For Each row In data
                For j = 0 To N - 1
                    row(j) *= x
                Next
            Next
            Return Me
        End Function

        ''' <summary>
        ''' Multiplies another matrix element wise, in place. Row and column vectors of matching length are broadcast together.
        ''' </summary>
        ''' <param name="other">The matrix or vector to multiply with.</param>
        ''' <returns>This matrix.</returns>
        ''' <exception cref="Exception">Thrown when the shapes are incompatible.</exception>
        Public Overridable Function mul(other As Matrix) As Matrix
            If M = other.M AndAlso N = other.N Then ' compatible matrices
                For i = 0 To M - 1
                    For j = 0 To N - 1
                        data(i)(j) *= other.data(i)(j)
                    Next
                Next
            ElseIf M = other.N AndAlso N = 1 AndAlso other.M = 1 Then
                ' this is a row vector and other is a column vector
                For i = 0 To M - 1
                    data(i)(0) *= other.data(0)(i)
                Next
            ElseIf N = other.M AndAlso M = 1 AndAlso other.N = 1 Then
                ' this is a column vector and other is a row vector
                For i = 0 To N - 1
                    data(0)(i) *= other.data(i)(0)
                Next
            Else
                Throw New Exception("Matrices/vectors incompatible for element-wise multiplication.")
            End If

            Return Me
        End Function

        ''' <summary>Negates every element, in place.</summary>
        ''' <returns>This matrix.</returns>
        Public Overridable Function neg() As Matrix
            Return mul(-1.0)
        End Function

        ''' <summary>
        ''' Divides every element by a scalar, in place.
        ''' </summary>
        ''' <param name="x">The divisor.</param>
        ''' <returns>This matrix.</returns>
        Public Overridable Function div(x As Double) As Matrix
            For Each row In data
                For j = 0 To N - 1
                    row(j) /= x
                Next
            Next
            Return Me
        End Function

        ''' <summary>
        ''' Divides by another matrix element wise, in place. Row and column vectors of matching length are broadcast together.
        ''' </summary>
        ''' <param name="other">The matrix or vector divisor.</param>
        ''' <returns>This matrix.</returns>
        ''' <exception cref="Exception">Thrown when the shapes are incompatible.</exception>
        Public Overridable Function div(other As Matrix) As Matrix
            If M = other.M AndAlso N = other.N Then ' compatible matrices
                For i = 0 To M - 1
                    For j = 0 To N - 1
                        data(i)(j) /= other.data(i)(j)
                    Next
                Next
            ElseIf M = other.N AndAlso N = 1 AndAlso other.M = 1 Then
                ' this is a row vector and other is a column vector
                For i = 0 To M - 1
                    data(i)(0) /= other.data(0)(i)
                Next
            ElseIf N = other.M AndAlso M = 1 AndAlso other.N = 1 Then
                ' this is a column vector and other is a row vector
                For i = 0 To N - 1
                    data(0)(i) /= other.data(i)(0)
                Next
            Else
                Throw New Exception("Matrices/vectors incompatible for element-wise division.")
            End If

            Return Me
        End Function

        ''' <summary>Applies <c>e^x</c> element wise, in place.</summary>
        ''' <returns>This matrix.</returns>
        Public Overridable Function exp() As Matrix
            For Each row In data
                For j = 0 To N - 1
                    row(j) = System.Math.Exp(row(j))
                Next
            Next
            Return Me
        End Function

        ''' <summary>Applies <c>tanh(x)</c> element wise, in place.</summary>
        ''' <returns>This matrix.</returns>
        Public Overridable Function tanh() As Matrix
            For Each row In data
                For j = 0 To N - 1
                    row(j) = System.Math.Tanh(row(j))
                Next
            Next
            Return Me
        End Function

        ''' <summary>
        ''' Clips every element into the interval <c>[x_a, x_b]</c>, in place.
        ''' </summary>
        ''' <param name="x_a">Lower bound; it must be smaller than <paramref name="x_b"/>.</param>
        ''' <param name="x_b">Upper bound.</param>
        ''' <returns>This matrix.</returns>
        Public Overridable Function clip(x_a As Double, x_b As Double) As Matrix
            For Each row In data
                For j = 0 To N - 1
                    If row(j) < x_a Then
                        row(j) = x_a
                    ElseIf x_b < row(j) Then
                        row(j) = x_b
                    End If
                Next
            Next

            Return Me
        End Function

        ''' <summary>
        ''' Applies a function to every element, in place.
        ''' </summary>
        ''' <param name="f">The function to apply.</param>
        ''' <returns>This matrix.</returns>
        Public Overridable Function apply(f As Func(Of Double, Double)) As Matrix
            For Each row In data
                For j = 0 To N - 1
                    row(j) = f(row(j))
                Next
            Next
            Return Me
        End Function

        ' Other of all elements 

        ''' <summary>Returns the sum of all elements.</summary>
        ''' <returns>The total sum.</returns>
        Public Overridable Function sum() As Double
            Dim lSum = 0.0
            For Each row In data
                For j = 0 To N - 1
                    lSum += row(j)
                Next
            Next

            Return lSum
        End Function

        ''' <summary>Returns the product of all elements.</summary>
        ''' <returns>The total product.</returns>
        Public Overridable Function prod() As Double
            Dim lProd = 0.0
            For Each row In data
                For j = 0 To N - 1
                    lProd *= row(j)
                Next
            Next

            Return lProd
        End Function

        ''' <summary>Returns a deep copy of the underlying M x N array.</summary>
        ''' <returns>The copied jagged array.</returns>
        Public Overridable Function raw() As Double()()
            Return Utils.deepCopyOf(data)
        End Function

        ''' <summary>Returns the matrix flattened in row-major order.</summary>
        ''' <returns>A flat copy of the matrix data.</returns>
        Public Overridable Function unravel() As Double()
            Dim result = New Double(M * N - 1) {}

            For i = 0 To M - 1
                For j = 0 To N - 1
                    result(N * i + j) = data(i)(j)
                Next
            Next

            Return result
        End Function

        ' State 

        ''' <summary>Gets a value indicating whether the matrix is a vector (a single row or column).</summary>
        Public Overridable ReadOnly Property Vector As Boolean
            Get
                Return M = 1 OrElse N = 1
            End Get
        End Property

        ''' <summary>
        ''' Returns the index of the single element equal to one.
        ''' </summary>
        ''' <returns>The index of the one.</returns>
        ''' <exception cref="Exception">Thrown when the matrix is not a valid one-hot vector.</exception>
        Public Overridable Function oneHotIndex() As Integer
            Dim one_already_encountered = False
            Dim one_hot_index = 0
            For i = 0 To M - 1
                For j = 0 To N - 1
                    ' continue
                    If Math.close(data(i)(j), 0.0) Then ' ignore zeros
                    ElseIf Math.close(data(i)(j), 1.0) Then ' allow a single one
                        If one_already_encountered Then
                            Throw New Exception("A one-hot vector can't have multiple ones.")
                        End If

                        one_already_encountered = True
                        one_hot_index = If(M = 1, j, i)
                    Else
                        Throw New Exception("A one-hot vector can't have elements other than 0 or 1.")
                    End If
                Next
            Next

            If Not one_already_encountered Then
                Throw New Exception("One-hot vector can't be all zeros.")
            End If

            Return one_hot_index
        End Function

        ' Dimensions 

        ''' <summary>Gets the row count.</summary>
        Public Overridable ReadOnly Property M As Integer

        ''' <summary>Gets the column count.</summary>
        Public Overridable ReadOnly Property N As Integer

        ''' <summary>
        ''' Returns the vector length of this matrix.
        ''' </summary>
        ''' <returns>The length of the vector.</returns>
        ''' <exception cref="Exception">Thrown when the matrix is not a vector.</exception>
        Public Overridable Function getk() As Integer
            If M = 1 Then
                Return N
            ElseIf N = 1 Then
                Return M
            Else
                Throw New Exception("The matrix is not a vector.")
            End If
        End Function

        ' Element access 

        ''' <summary>
        ''' Gets a vector element. Requires the matrix to be a vector.
        ''' </summary>
        ''' <param name="i">The element index; requires <c>i &lt; k</c>.</param>
        ''' <returns>The value at <paramref name="i"/>.</returns>
        Public Overridable Function at(i As Integer) As Double
            If M = 1 Then
                Return data(0)(i) ' row vector
            End If
            Return data(i)(0) ' column vector
        End Function

        ''' <summary>
        ''' Gets a matrix element.
        ''' </summary>
        ''' <param name="i">Row index; requires <c>i &lt; M</c>.</param>
        ''' <param name="j">Column index; requires <c>j &lt; N</c>.</param>
        ''' <returns>The value at (<paramref name="i"/>, <paramref name="j"/>).</returns>
        Public Overridable Function at(i As Integer, j As Integer) As Double
            Return data(i)(j)
        End Function

        ''' <summary>
        ''' Gets the vector element addressed by a one-hot index vector.
        ''' </summary>
        ''' <param name="index">A one-hot vector whose single one selects the element.</param>
        ''' <returns>The selected value.</returns>
        Public Overridable Function at(index As Matrix) As Double
            Return at(index.oneHotIndex())
        End Function

        ''' <summary>
        ''' Sets a vector element.
        ''' </summary>
        ''' <param name="i">The element index; requires <c>0 &lt;= i &lt; k</c>.</param>
        ''' <param name="x">The value to store.</param>
        Public Overridable Sub setAt(i As Integer, x As Double)
            If M = 1 Then
                data(0)(i) = x ' row vector
            End If
            If N = 1 Then
                data(i)(0) = x ' column vector
            End If
        End Sub

        ''' <summary>
        ''' Sets a matrix element.
        ''' </summary>
        ''' <param name="i">Row index; requires <c>0 &lt;= i &lt; M</c>.</param>
        ''' <param name="j">Column index; requires <c>0 &lt;= j &lt; N</c>.</param>
        ''' <param name="x">The value to store.</param>
        Public Overridable Sub setAt(i As Integer, j As Integer, x As Double)
            data(i)(j) = x
        End Sub

        ''' <summary>
        ''' Sets the vector element addressed by a one-hot index vector.
        ''' </summary>
        ''' <param name="m">A one-hot vector whose single one selects the element.</param>
        ''' <param name="x">The value to store.</param>
        Public Overridable Sub setAt(m As Matrix, x As Double)
            setAt(m.oneHotIndex(), x)
        End Sub
    End Class
End Namespace
