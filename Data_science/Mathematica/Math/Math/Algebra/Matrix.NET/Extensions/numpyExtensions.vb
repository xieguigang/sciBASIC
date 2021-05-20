#Region "Microsoft.VisualBasic::b3304f087d232b224603136591e899a2, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Extensions\numpyExtensions.vb"

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

    '     Enum ApplyOnAxis
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module NumpyExtensions
    ' 
    '         Function: Apply, Mean, Sort, Std, Sum
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

    <HideModuleName> Public Module NumpyExtensions

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
