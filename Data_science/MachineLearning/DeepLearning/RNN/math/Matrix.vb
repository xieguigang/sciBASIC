#Region "Microsoft.VisualBasic::04c4431efe585be74de6824ba373ce45, Data_science\MachineLearning\DeepLearning\RNN\math\Matrix.vb"

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

    '   Total Lines: 443
    '    Code Lines: 304 (68.62%)
    ' Comment Lines: 72 (16.25%)
    '    - Xml Docs: 11.11%
    ' 
    '   Blank Lines: 67 (15.12%)
    '     File Size: 15.60 KB


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

    ' MxN Matrix
    ' If M=1 or N=1, the matrix is treated as a k-vector v.
    ' v is equivalent to v.T() in scenarios like matrix multiplication,
    ' or element-wise addition.
    <Serializable>
    Public Class Matrix

        Private data As Double()() ' MxN

        ' Create 

        ' Constructs by copying other. Requires other != null.
        Public Sub New(other As Matrix)
            M = other.M
            N = other.N
            data = Utils.deepCopyOf(other.data)
        End Sub

        ' Constructs using an MxN array. Requires M, N > 0.
        Private Sub New(data As Double()())
            M = Utils.arrayRows(data)
            N = Utils.arrayCols(data)

            If M = 0 OrElse N = 0 Then ' Don't accept 0 as a size.
                Throw New ArgumentException("One of the array dimensions is 0.")
            End If

            Me.data = data
        End Sub

        ' Constructs using an M*N row-major array. Requires M > 0.
        Private Sub New(M As Integer, data As Double())
            Me.M = M
            Me.N = data.Length Mod Me.M

            For i = 0 To Me.M - 1
                For j = 0 To N - 1
                    Me.data(i)(j) = data(M * i + j)
                Next
            Next
        End Sub

        ' Returns a matrix constructed using an MxN array. Requires M, N > 0.
        Public Shared Function fromRaw(data As Double()()) As Matrix
            Return New Matrix(Utils.deepCopyOf(data))
        End Function

        ' Constructs using an M*N row-major array. Requires M > 0.
        Public Shared Function fromFlat(M As Integer, data As Double()) As Matrix
            Return New Matrix(M, data)
        End Function

        ' Returns a matrix with all zeros, M rows, N cols. Requires M, N > 0.
        Public Shared Function zeros(M As Integer, N As Integer) As Matrix
            Return New Matrix(RectangularArray.Matrix(Of Double)(M, N))
        End Function

        ' Returns a k-dimensional vector with all zeros. Requires k > 0.
        Public Shared Function zeros(k As Integer) As Matrix
            Return zeros(1, k)
        End Function

        ' Returns a matrix shaped like other with all zeros. Requires other !=
        ' null.
        Public Shared Function zerosLike(other As Matrix) As Matrix
            Return zeros(other.M, other.N)
        End Function

        ' Returns a matrix with all ones, M rows, N cols. Requires M, N > 0.
        Public Shared Function ones(M As Integer, N As Integer) As Matrix
            Return zeros(M, N).add(1.0)
        End Function

        ' Returns a k-dimensional vector with all ones. Requires k > 0.
        Public Shared Function ones(k As Integer) As Matrix
            Return ones(1, k)
        End Function

        ' Returns a matrix shaped like other with all ones.
        Public Shared Function onesLike(other As Matrix) As Matrix
            Return ones(other.M, other.N)
        End Function

        ' Returns a one-hot vector (v.at(i) = 1, v.at(j) = 0 ; j != i).
        ' Requires 0 <= i < k.
        Public Shared Function oneHot(k As Integer, i As Integer) As Matrix
            Dim v = zeros(k)
            v.setAt(i, 1.0)
            Return v
        End Function

        ' Returns the matrix product (a x b)
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

        ' Returns the transpose of this matrix.
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

        ' Adds x to all elements.
        Public Overridable Function add(x As Double) As Matrix
            For Each row In data
                For j = 0 To N - 1
                    row(j) += x
                Next
            Next
            Return Me
        End Function

        ' Adds element-wise. Requires the matrices to have the same
        ' dimensions.
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

        ' Multiplies all elements by x.
        Public Overridable Function mul(x As Double) As Matrix
            For Each row In data
                For j = 0 To N - 1
                    row(j) *= x
                Next
            Next
            Return Me
        End Function

        ' Multiplies element-wise.
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

        ' Multiplies all elements by -1.0.
        Public Overridable Function neg() As Matrix
            Return mul(-1.0)
        End Function

        ' Divides all elements by x.
        Public Overridable Function div(x As Double) As Matrix
            For Each row In data
                For j = 0 To N - 1
                    row(j) /= x
                Next
            Next
            Return Me
        End Function

        ' Divides element-wise.
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

        ' Applies e^x element-wise.
        Public Overridable Function exp() As Matrix
            For Each row In data
                For j = 0 To N - 1
                    row(j) = System.Math.Exp(row(j))
                Next
            Next
            Return Me
        End Function

        ' Applies tanh(x) element-wise.
        Public Overridable Function tanh() As Matrix
            For Each row In data
                For j = 0 To N - 1
                    row(j) = System.Math.Tanh(row(j))
                Next
            Next
            Return Me
        End Function

        ' Clips all elements to the interval [x_a, x_b]. Requires that x_a < x_b
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

        ' Calls f for each element.
        Public Overridable Function apply(f As Func(Of Double, Double)) As Matrix
            For Each row In data
                For j = 0 To N - 1
                    row(j) = f(row(j))
                Next
            Next
            Return Me
        End Function

        ' Other of all elements 

        ' Returns the sum of elements.
        Public Overridable Function sum() As Double
            Dim lSum = 0.0
            For Each row In data
                For j = 0 To N - 1
                    lSum += row(j)
                Next
            Next

            Return lSum
        End Function

        ' Returns the product of elements.
        Public Overridable Function prod() As Double
            Dim lProd = 0.0
            For Each row In data
                For j = 0 To N - 1
                    lProd *= row(j)
                Next
            Next

            Return lProd
        End Function

        ' Returns a copy of the MxN array.
        Public Overridable Function raw() As Double()()
            Return Utils.deepCopyOf(data)
        End Function

        ' Returns a row-major flattened array.
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

        ' Returns true, if the matrix is a vector.
        Public Overridable ReadOnly Property Vector As Boolean
            Get
                Return M = 1 OrElse N = 1
            End Get
        End Property

        ' Returns the index with the value 1.0.
        ' Requires the matrix to be a one-hot vector.
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

        ''' <summary>
        ''' Returns the row count.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property M As Integer

        ''' <summary>
        ''' Returns the column count.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property N As Integer

        ' Returns the vector length. Requires that the matrix is a vector.
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

        ' Returns the vector element at i. Requires i < k
        Public Overridable Function at(i As Integer) As Double
            If M = 1 Then
                Return data(0)(i) ' row vector
            End If
            Return data(i)(0) ' column vector
        End Function

        ' Returns the matrix element at i,j. Requires i < M, j < N.
        Public Overridable Function at(i As Integer, j As Integer) As Double
            Return data(i)(j)
        End Function

        ' Returns the vector element at m.oneHotIndex(). Requires index to be a
        ' one-hot vector.
        Public Overridable Function at(index As Matrix) As Double
            Return at(index.oneHotIndex())
        End Function

        ' Sets the vector element at i to x. Requires 0 <= i < k.
        Public Overridable Sub setAt(i As Integer, x As Double)
            If M = 1 Then
                data(0)(i) = x ' row vector
            End If
            If N = 1 Then
                data(i)(0) = x ' column vector
            End If
        End Sub

        ' Sets the matrix element at i,j to x. Requires 0 <= i < M and 0 <= j < N.
        Public Overridable Sub setAt(i As Integer, j As Integer, x As Double)
            data(i)(j) = x
        End Sub

        ' Sets the vector element at m.oneHotIndex() to x.
        ' Requires index to be a one-hot vector, and its index i < k.
        Public Overridable Sub setAt(m As Matrix, x As Double)
            setAt(m.oneHotIndex(), x)
        End Sub
    End Class
End Namespace
