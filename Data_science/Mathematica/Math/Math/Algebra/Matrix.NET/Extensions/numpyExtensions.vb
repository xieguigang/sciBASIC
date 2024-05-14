#Region "Microsoft.VisualBasic::6d29d79c8ffa2462f688988a9f3629c9, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Extensions\numpyExtensions.vb"

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

    '   Total Lines: 309
    '    Code Lines: 200
    ' Comment Lines: 75
    '   Blank Lines: 34
    '     File Size: 12.69 KB


    '     Enum ApplyOnAxis
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class Numpy
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: argsort, array, column_stack, eye, power
    '                   Sort, (+2 Overloads) Sum, (+2 Overloads) Where
    ' 
    '     Module NumpyExtensions
    ' 
    '         Function: Apply, flatten, Mean, r, shape
    '                   Sort, Std, sum, Sum, t
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace LinearAlgebra.Matrix

    Public Enum ApplyOnAxis As Integer
        Any = -1
        Column = 0
        Row = 1
    End Enum

    Public NotInheritable Class Numpy

        Private Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function array(v As IEnumerable(Of Double)) As Vector
            Return New Vector(data:=v)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function power(v As IEnumerable(Of Double), pow As Double) As Vector
            Return New Vector(SIMD.Exponent.f64_op_exponent_f64_scalar(v.ToArray, pow))
        End Function

        Public Shared Function eye(n As Integer) As Double()()
            Dim rows As Double()() = New Double(n - 1)() {}

            For i As Integer = 0 To n - 1
                rows(i) = New Double(n - 1) {}
                rows(i)(i) = 1
            Next

            Return rows
        End Function

        ''' <summary>
        ''' Perform an indirect sort along the given axis using the algorithm specified
            ''' by the `kind` keyword. It returns an array Of indices Of the same shape As
            ''' `a` that index data along the given axis in sorted order.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns>Returns the indices that would sort an array.</returns>
        ''' <example>
        ''' x = np.array([3, 1, 2])
        ''' np.argsort(x)
        ''' array([1, 2, 0])
        ''' </example>
        ''' 
        Public Shared Function argsort(data As IEnumerable(Of Double)) As Integer()
            Dim sort = From x In data.SeqIterator Select x Order By x.value
            Dim index = sort.Select(Function(x) x.i).ToArray

            Return index
        End Function

        ''' <summary>
        ''' Sorting or Ordering Vectors
        ''' Sort (or order) a vector or factor (partially) into ascending or descending order. For ordering along more than one variable, e.g., for sorting data frames, see order.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Sort(x As IEnumerable(Of Double), Optional decreasing As Boolean = False) As Vector
            If decreasing Then
                Return New Vector(x.OrderByDescending(Function(n) n))
            Else
                Return New Vector(x.OrderBy(Function(n) n))
            End If
        End Function

        Public Shared Iterator Function column_stack(ParamArray vectors As Vector()) As IEnumerable(Of Double())
            Dim maxL = vectors.Max(Function(vec) vec.Length)

#Disable Warning
            For i As Integer = 0 To maxL - 1
                Yield vectors _
                    .Select(Function(vec) vec.ElementAtOrDefault(i)) _
                    .ToArray
            Next
#Enable Warning
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="condition">
        ''' 当conditon的某个位置的为true时，输出x的对应位置的元素，否则选择y对应位置的元素；
        ''' </param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Shared Function Where(condition As IEnumerable(Of Boolean), x As Vector, y As Vector) As Vector
            Return Iterator Function() As IEnumerable(Of Double)
                       For Each index As SeqValue(Of Boolean) In condition.SeqIterator
                           If index.value Then
                               Yield x(index)
                           Else
                               Yield y(index)
                           End If
                       Next
                   End Function().AsVector
        End Function

        Public Shared Function Where(condition As IEnumerable(Of Boolean), x As Double, y As Double) As Vector
            Return Iterator Function() As IEnumerable(Of Double)
                       For Each index As SeqValue(Of Boolean) In condition.SeqIterator
                           If index.value Then
                               Yield x
                           Else
                               Yield y
                           End If
                       Next
                   End Function().AsVector
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Sum(x As IEnumerable(Of Double), Optional NaRM As Boolean = False) As Vector
            Return New Vector({x.Sum})
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Sum(x As IEnumerable(Of Boolean), Optional NaRM As Boolean = False) As Vector
            Dim data = (From b As Boolean In x Select If(b, 1, 0)).ToArray
            Return New Vector(integers:={data.Sum})
        End Function
    End Class

    <HideModuleName> Public Module NumpyExtensions

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
