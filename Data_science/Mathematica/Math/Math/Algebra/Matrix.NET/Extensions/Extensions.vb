#Region "Microsoft.VisualBasic::74ac0e61b6c1583c78147f66dd73f0aa, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Extensions\Extensions.vb"

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

    '   Total Lines: 148
    '    Code Lines: 99 (66.89%)
    ' Comment Lines: 27 (18.24%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 22 (14.86%)
    '     File Size: 5.30 KB


    '     Module Extensions
    ' 
    '         Function: CenterNormalize, ColumnVector, Covariance, eig, ncol
    '                   nrow, rand, size
    ' 
    '         Sub: Print
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Statistics.Linq
Imports Microsoft.VisualBasic.Text
Imports rand2 = Microsoft.VisualBasic.Math.RandomExtensions

Namespace LinearAlgebra.Matrix

    <HideModuleName>
    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function nrow(x As GeneralMatrix) As Integer
            Return x.RowDimension
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ncol(x As GeneralMatrix) As Integer
            Return x.ColumnDimension
        End Function

        <Extension>
        Public Function eig(m As GeneralMatrix) As EigenvalueDecomposition
            Return New EigenvalueDecomposition(m)
        End Function

        ''' <summary>
        ''' get a specific column data as vector
        ''' </summary>
        ''' <param name="matrix"></param>
        ''' <param name="i%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ColumnVector(matrix As GeneralMatrix, i%) As Vector
            Return New Vector(matrix({i}).ArrayPack.Select(Function(r) r(Scan0)))
        End Function

        Public Function size(M As GeneralMatrix, d%) As Integer
            If d = 1 Then
                Return M.RowDimension
            ElseIf d = 2 Then
                Return M.ColumnDimension
            Else
                Throw New NotImplementedException
            End If
        End Function

        ''' <summary>Generate matrix with random elements</summary>
        ''' <param name="m">   Number of rows.
        ''' </param>
        ''' <param name="n">   Number of colums.
        ''' </param>
        ''' <returns>     An m-by-n matrix with uniformly distributed random elements.
        ''' </returns>

        Public Function rand(m%, n%) As GeneralMatrix
            With rand2.seeds
                Dim A As New NumericMatrix(m, n)
                Dim X As Double()() = A.Array

                For i As Integer = 0 To m - 1
                    For j As Integer = 0 To n - 1
                        X(i)(j) = .NextDouble()
                    Next
                Next

                Return A
            End With
        End Function

        ''' <summary>
        ''' Centers each column of the data matrix at its mean.
        ''' Normalizes the input matrix so that each column is centered at 0.
        ''' </summary>
        <Extension>
        Public Function CenterNormalize(m As GeneralMatrix) As GeneralMatrix
            Dim input = m.ArrayPack
            Dim out As Double()() = RectangularArray.Matrix(Of Double)(input.Length, input(0).Length)

            For i As Integer = 0 To input.Length - 1
                Dim meanValue As Double = input(i).Average
                For j As Integer = 0 To input(i).Length - 1
                    out(i)(j) = input(i)(j) - meanValue
                Next
            Next

            Return New NumericMatrix(out)
        End Function

        ''' <summary>
        ''' Constructs the covariance matrix for this data set.
        ''' @return	the covariance matrix of this data set
        ''' </summary>
        <Extension>
        Public Function Covariance(matrix As GeneralMatrix) As GeneralMatrix
            Dim length As Integer = matrix.RowDimension
            Dim out As Double()() = RectangularArray.Matrix(Of Double)(length, length)
            Dim array = matrix.ArrayPack

            For i As Integer = 0 To out.Length - 1
                For j As Integer = 0 To out.Length - 1
                    Dim dataA As Double() = array(i)
                    Dim dataB As Double() = array(j)
                    out(i)(j) = dataA.Covariance(dataB)
                Next
            Next

            Return New NumericMatrix(out)
        End Function

        ''' <summary>
        ''' Print the matrix data onto the console or a specific stream.
        ''' </summary>
        ''' <param name="m"></param>
        ''' <param name="format$"></param>
        ''' <param name="out"></param>
        <Extension>
        Public Sub Print(m As GeneralMatrix, Optional format$ = "F4", Optional out As StreamWriter = Nothing)
            Dim openSTD As Boolean = False
            Dim line$

            If out Is Nothing Then
                out = New StreamWriter(Console.OpenStandardOutput)
                openSTD = True
            End If

            For Each row As Double() In m.ArrayPack
                line = row _
                    .Select(Function(x)
                                If x >= 0 Then
                                    Return " " & x.ToString(format)
                                Else
                                    Return x.ToString(format)
                                End If
                            End Function) _
                    .JoinBy(ASCII.TAB)
                out.WriteLine(line)
            Next

            Call out.Flush()

            If openSTD Then
                out.Dispose()
            End If
        End Sub

        ''' <summary>
        ''' Create column vector matrix
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        <Extension>
        Public Function t(v As IEnumerable(Of Double)) As NumericMatrix
            Dim column As Double()() = v _
                .Select(Function(xi) New Double() {xi}) _
                .ToArray
            Dim cm As New NumericMatrix(column)

            Return cm
        End Function

        ''' <summary>
        ''' Create row vector matrix
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        <Extension>
        Public Function r(v As IEnumerable(Of Double)) As NumericMatrix
            Dim rows As Double()() = New Double()() {v.ToArray}
            Dim rm As New NumericMatrix(rows)

            Return rm
        End Function

        <Extension>
        Public Function flatten(nd As Vector) As Vector
            Return nd
        End Function

        <Extension>
        Public Function shape(m As GeneralMatrix) As Integer()
            Return {m.RowDimension, m.ColumnDimension}
        End Function

        ''' <summary>
        ''' Returns the average of the array elements. The average is taken over the 
        ''' flattened array by default, otherwise over the specified axis. float64 
        ''' intermediate and return values are used for integer inputs.
        ''' </summary>
        ''' <param name="matrix"></param>
        ''' <param name="axis%"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Mean(matrix As IEnumerable(Of Vector), Optional axis% = -1) As Vector
            Return matrix.Apply(Function(x) x.Average, axis:=axis, aggregate:=AddressOf NumericsVector.AsVector)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="Tout"></typeparam>
        ''' <param name="matrix"></param>
        ''' <param name="math"></param>
        ''' <param name="axis">
        ''' + 0 表示按列进行计算
        ''' + 1 表示按行进行计算
        ''' + 小于零则负数则表示所有元素作为一个向量来计算
        ''' </param>
        ''' <param name="aggregate"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Apply(Of T, Tout)(matrix As IEnumerable(Of Vector),
                                      math As Func(Of IEnumerable(Of Double), T),
                                      axis%,
                                      aggregate As Func(Of IEnumerable(Of T), Tout)) As Tout

            ' >>> a = np.array([[1, 2], [3, 4]])
            ' >>> np.mean(a)
            ' 2.5
            ' >>> np.mean(a, axis=0)
            ' Array([ 2., 3.])
            ' >>> np.mean(a, axis=1)
            ' Array([ 1.5, 3.5])
            If axis < 0 Then
                Return aggregate({math(matrix.IteratesALL)})
            ElseIf axis = 0 Then
                Return Iterator Function() As IEnumerable(Of T)
                           Dim data As Vector() = matrix.ToArray
                           Dim columns As Integer = data(Scan0).Length
#Disable Warning
                           For i As Integer = 0 To columns - 1
                               Yield math(data.Select(Function(row) row(i)))
                           Next
#Enable Warning
                       End Function().DoCall(aggregate)
            ElseIf axis = 1 Then
                Return matrix _
                    .SeqIterator _
                    .AsParallel _
                    .Select(Function(r)
                                Return (r.i, math(r.value))
                            End Function) _
                    .OrderBy(Function(a) a.i) _
                    .Select(Function(a) a.Item2) _
                    .DoCall(aggregate)
            Else
                Throw New NotImplementedException
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Std(matrix As IEnumerable(Of Vector), Optional axis% = -1) As Vector
            Return matrix.Apply(Function(x) x.SD, axis:=axis, aggregate:=AddressOf NumericsVector.AsVector)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Sum(matrix As IEnumerable(Of Vector), Optional axis% = -1) As Vector
            Return matrix.Apply(Function(x) x.Sum, axis:=axis, aggregate:=AddressOf NumericsVector.AsVector)
        End Function

        <Extension>
        Public Function sum(matrix As GeneralMatrix, Optional axis% = -1) As Vector
            Return matrix.RowVectors.Sum(axis)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Sort(matrix As IEnumerable(Of Vector), Optional axis%? = -1) As IEnumerable(Of Vector)
            ' >>> a = np.array([[1,4],[3,1]])
            ' >>> np.sort(a)                # sort along the last axis
            ' Array([[1, 4],
            '        [1, 3]])
            ' >>> np.sort(a, axis=None)     # sort the flattened array
            ' Array([1, 1, 3, 4])
            ' >>> np.sort(a, axis=0)        # sort along the first axis
            ' Array([[1, 1],
            '        [3, 4]])

            If axis < 0 Then
                Return matrix _
                .Select(Function(r)
                            Return r _
                                .OrderBy(Function(x) x) _
                                .AsVector
                        End Function)
            ElseIf axis = 0 Then
                Dim reorderMatrix = Iterator Function() As IEnumerable(Of Double())
                                        Dim data As Vector() = matrix.ToArray
                                        Dim columns As Integer = data(Scan0).Length
#Disable Warning
                                        For i As Integer = 0 To columns - 1
                                            Yield data _
                                           .Select(Function(row) row(i)) _
                                           .OrderBy(Function(x) x) _
                                           .ToArray
                                        Next
#Enable Warning
                                    End Function()

                Return Iterator Function() As IEnumerable(Of Vector)
                           Dim columns As Integer = reorderMatrix(Scan0).Length
#Disable Warning
                           For i As Integer = 0 To columns - 1
                               Yield reorderMatrix _
                               .Select(Function(row) row(i)) _
                               .OrderBy(Function(x) x) _
                               .AsVector
                           Next
#Enable Warning
                       End Function()
            ElseIf axis Is Nothing Then
                Return {matrix.IteratesALL.OrderBy(Function(x) x).AsVector}
            Else
                Throw New NotImplementedException
            End If
        End Function
    End Module
End Namespace
