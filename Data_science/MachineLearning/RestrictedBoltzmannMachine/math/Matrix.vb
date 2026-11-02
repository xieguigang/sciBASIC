#Region "Microsoft.VisualBasic::713d015c9a577a7fb41ca1a08822a95a, Data_science\MachineLearning\RestrictedBoltzmannMachine\math\Matrix.vb"

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

    '   Total Lines: 246
    '    Code Lines: 177 (71.95%)
    ' Comment Lines: 10 (4.07%)
    '    - Xml Docs: 70.00%
    ' 
    '   Blank Lines: 59 (23.98%)
    '     File Size: 8.83 KB


    '     Class DenseMatrix
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: [dim], [get], [set], add, addColumns
    '                   (+2 Overloads) apply, cols, columns, concatColumns, (+2 Overloads) concatRows
    '                   copy, data, (+2 Overloads) divide, dot, (+3 Overloads) make
    '                   (+2 Overloads) multiply, pow, random, randomGaussian, row
    '                   (+2 Overloads) rows, splitColumns, subtract, sum, toArray
    '                   ToString, transpose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math.functions
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace math

    ''' <summary>
    ''' Currently this class is half mutable/immutable. The operations that are immutable are defined. This class serves
    ''' to wrap the Parallel Colt library.
    ''' If you need an operation to be immutable just call .copy().
    ''' I am considering making a ImmutableDense,ImmutableSparse,MutableDense,MutableSparse class in the future if needed.
    ''' Created by kenny on 5/24/14.
    ''' </summary>
    Public Class DenseMatrix

        Protected Friend Shared ReadOnly ADDField As DoubleDoubleFunction = New math.functions.doubledouble.Add()
        Protected Friend Shared ReadOnly SUBTRACTField As DoubleDoubleFunction = New math.functions.doubledouble.Subtract()
        Protected Friend Shared ReadOnly MULTIPLYField As DoubleDoubleFunction = New math.functions.doubledouble.Multiply()
        Protected Friend Shared ReadOnly DIVIDEField As DoubleDoubleFunction = New math.functions.doubledouble.Divide()
        Protected Friend Shared ReadOnly RANDOM_GAUSSIAN As DoubleFunction = New RandomGaussian()
        Protected Friend Shared ReadOnly RANDOM_DOUBLE As DoubleFunction = New RandomDouble(1.0)

        Protected Friend m As GeneralMatrix

        Sub New(m As GeneralMatrix)
            Me.m = m
        End Sub

        ' IMMUTABLE OPERATIONS 
        Public Function transpose() As DenseMatrix
            Return New DenseMatrix(m.Transpose)
        End Function

        Public Function copy() As DenseMatrix
            Return New DenseMatrix(New NumericMatrix(m.RowVectors))
        End Function

        Public Function dot(m2 As DenseMatrix) As DenseMatrix
            Return New DenseMatrix(Me.m.Dot(m2.m))
        End Function

        ' MUTABLE OPERATIONS 

        Public Overloads Function apply([function] As DoubleFunction) As DenseMatrix
            Return New DenseMatrix(m.assign([function]))
        End Function

        Public Overloads Function apply(m2 As DenseMatrix, [function] As DoubleDoubleFunction) As DenseMatrix
            Return New DenseMatrix(m.assign(m2.m, [function]))
        End Function

        Public Shared Function make(r As Integer, c As Integer) As DenseMatrix
            Return New DenseMatrix(New NumericMatrix(r, c))
        End Function

        Public Shared Function randomGaussian(r As Integer, c As Integer) As DenseMatrix
            Return New DenseMatrix(NumericMatrix.Gauss(c, r))
        End Function

        ''' <summary>
        ''' 创建一个使用均匀分布<c>[0,1)</c>随机数填充的矩阵。
        ''' </summary>
        ''' <remarks>
        ''' 这里必须使用均匀分布：激活状态的伯努利采样(`ActivationState`)使用的是
        ''' <c>probability &gt;= random</c>的判据，当<c>random</c>服从<c>U[0,1)</c>的时候，
        ''' 单元被激活的概率恰好等于传入的概率值<c>p</c>；如果使用高斯分布，则实际激活概率会变成<c>Φ(p)</c>，
        ''' 从而导致对比散度的负相采样出现系统性的偏差。
        ''' </remarks>
        Public Shared Function random(r As Integer, c As Integer) As DenseMatrix
            Return make(r, c).apply(RANDOM_DOUBLE)
        End Function

        ''' <summary>
        ''' 创建指定尺寸的矩阵，并且使用固定的数值来填充每一个元素。
        ''' </summary>
        Public Shared Function fill(r As Integer, c As Integer, value As Double) As DenseMatrix
            Dim matrix As DenseMatrix = make(r, c)

            For i As Integer = 0 To r - 1
                For j As Integer = 0 To c - 1
                    Call matrix.set(i, j, value)
                Next
            Next

            Return matrix
        End Function

        Public Shared Function make(m As Double()()) As DenseMatrix
            Return New DenseMatrix(New NumericMatrix(m))
        End Function

        Public Shared Function make(m As Vector) As DenseMatrix
            Return New DenseMatrix(New NumericMatrix({m.ToArray}))
        End Function

        Public Overrides Function ToString() As String
            Return m.ToString()
        End Function

        Public Function addColumns(m2 As DenseMatrix) As DenseMatrix
            Dim a = m.RowVectors.ToArray
            Dim b = m2.m.RowVectors.ToArray
            Dim rows As New List(Of Double())
            Dim v As Double()
            Dim len As Integer = m.ColumnDimension + m2.m.ColumnDimension

            For i As Integer = 0 To a.Length - 1
                v = New Double(len - 1) {}
                Array.ConstrainedCopy(a(i).Array, 0, v, 0, m.ColumnDimension)
                Array.ConstrainedCopy(b(i).Array, 0, v, m.ColumnDimension, m2.m.ColumnDimension)
                rows.Add(v)
            Next

            Return New DenseMatrix(New NumericMatrix(rows))
        End Function

        ' MUTABLE OPERATIONS 

        Public Function data() As GeneralMatrix
            Return m
        End Function

        Public Function toArray() As Double()()
            Return m.RowVectors().[Select](Function(v) v.ToArray()).ToArray()
        End Function


        Public Function [set](i As Integer, j As Integer, value As Double) As DenseMatrix
            m(i, j) = value
            Return Me
        End Function


        Public Function row(pRow As Integer) As Vector
            Return m(pRow)
        End Function

        Public Function [dim]() As Integer
            Return rows() * columns()
        End Function

        Public Function rows() As Integer
            Return m.RowDimension
        End Function

        Public Function columns() As Integer
            Return m.ColumnDimension
        End Function


        Public Function [get](i As Integer, j As Integer) As Double
            Return m(i, j)
        End Function

        Public Function add(m2 As DenseMatrix) As DenseMatrix
            Return apply(m2, ADDField)
        End Function

        Public Function subtract(m2 As DenseMatrix) As DenseMatrix
            Return apply(m2, SUBTRACTField)
        End Function

        Public Function multiply(m2 As DenseMatrix) As DenseMatrix
            Return apply(m2, MULTIPLYField)
        End Function

        Public Function multiply(s As Double) As DenseMatrix
            Return apply(New functions.Multiply(s))
        End Function

        Public Function divide(m2 As DenseMatrix) As DenseMatrix
            Return apply(m2, DIVIDEField)
        End Function

        Public Function divide(s As Double) As DenseMatrix
            Return apply(New functions.Divide(s))
        End Function

        Public Function pow(power As Double) As DenseMatrix
            Return apply(New Power(power))
        End Function

        Public Function sum() As Double
            Dim lSum = 0.0
            For i = 0 To rows() - 1
                For j = 0 To columns() - 1
                    lSum += [get](i, j)
                Next
            Next
            Return lSum
        End Function

        ''' <summary>
        ''' 将长度为<c>columns()</c>的向量按照行进行广播相加：<c>result(i, j) = m(i, j) + v(j)</c>
        ''' </summary>
        ''' <remarks>
        ''' RBM的偏置项计算需要这个操作：<c>P(h=1|v) = σ(vW + b)</c>，其中的隐藏层偏置<c>b</c>
        ''' 需要按照样本(行)广播到每一个样本上面。
        ''' </remarks>
        Public Function addRowVector(v As Double()) As DenseMatrix
            If v Is Nothing Then
                Throw New ArgumentNullException(NameOf(v))
            End If
            If v.Length <> columns() Then
                Throw New ArgumentException($"vector size {v.Length} does not match the matrix columns {columns()}")
            End If

            Dim nRows As Integer = rows()
            Dim nCols As Integer = columns()
            Dim data = RectangularArray.Matrix(Of Double)(nRows, nCols)

            For i As Integer = 0 To nRows - 1
                For j As Integer = 0 To nCols - 1
                    data(i)(j) = [get](i, j) + v(j)
                Next
            Next

            Return New DenseMatrix(New NumericMatrix(data))
        End Function

        ''' <summary>
        ''' 按照列进行求和：返回长度为<c>columns()</c>的向量，即所有样本(行)在某一个特征维度上面的累加值。
        ''' </summary>
        ''' <remarks>
        ''' 这个函数用于计算RBM的偏置梯度：<c>∂logP/∂b = E_data[h] - E_model[h]</c>。
        ''' </remarks>
        Public Function columnSum() As Double()
            Dim nCols As Integer = columns()
            Dim sums As Double() = New Double(nCols - 1) {}

            For i As Integer = 0 To rows() - 1
                For j As Integer = 0 To nCols - 1
                    sums(j) += [get](i, j)
                Next
            Next

            Return sums
        End Function

        Public Shared Function splitColumns(m As DenseMatrix, numPieces As Integer) As IList(Of DenseMatrix)

            Dim pieces As IList(Of DenseMatrix) = New List(Of DenseMatrix)(numPieces)

            Dim rows As Integer = m.rows()

            Dim cols As Integer = m.columns() / numPieces ' must be evenly splittable
            For p = 0 To numPieces - 1

                Dim piece = RectangularArray.Matrix(Of Double)(rows, cols)
                For i = 0 To rows - 1
                    For j = 0 To cols - 1
                        piece(i)(j) = m.get(i, j + p * cols)
                    Next
                Next
                pieces.Add(New DenseMatrix(New NumericMatrix(piece)))
            Next
            Return pieces
        End Function

        Public Shared Function concatColumns(ParamArray m As DenseMatrix()) As Double()()
            Dim totalCols = 0
            For i = 0 To m.Length - 1
                totalCols += m(i).columns()
            Next
            Dim rows As Integer = m(0).rows()

            Dim appended = RectangularArray.Matrix(Of Double)(rows, totalCols)
            For k = 0 To m.Length - 1

                Dim cols As Integer = m(k).columns()
                For i = 0 To rows - 1

                    Dim start = k * cols
                    Array.Copy(m(k).row(i).Array, 0, appended(i), start, cols)
                Next
            Next
            Return appended
        End Function


        Public Shared Function concatRows(m As IList(Of DenseMatrix)) As Double()()
            Return concatRows(m.ToArray())
        End Function


        Public Shared Function concatRows(ParamArray m As DenseMatrix()) As Double()()
            Dim totalRows = 0
            For i = 0 To m.Length - 1
                totalRows += m(i).rows()
            Next

            Dim columns As Integer = m(0).columns()

            Dim appended = RectangularArray.Matrix(Of Double)(totalRows, columns)
            Dim row = 0
            For k = 0 To m.Length - 1
                For i = 0 To m(k).rows() - 1
                    Array.Copy(m(k).row(i).Array, 0, appended(row), 0, columns)
                Next
                row += 1
            Next
            Return appended
        End Function

        Public Shared Function rows(m As Double()()) As Integer
            Return m.Length
        End Function

        Public Shared Function cols(m As Double()()) As Integer
            Return m(0).Length
        End Function

    End Class

End Namespace
