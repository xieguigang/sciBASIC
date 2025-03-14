#Region "Microsoft.VisualBasic::643b5ae75071fbec33f76f3546118d70, Microsoft.VisualBasic.Core\src\Extensions\Collection\Linq\Iterator.vb"

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

    '   Total Lines: 145
    '    Code Lines: 94 (64.83%)
    ' Comment Lines: 33 (22.76%)
    '    - Xml Docs: 87.88%
    ' 
    '   Blank Lines: 18 (12.41%)
    '     File Size: 5.64 KB


    '     Module IteratorExtensions
    ' 
    '         Function: [Next], (+2 Overloads) Indices, Ordinals, Previous, (+2 Overloads) SeqIterator
    '                   (+2 Overloads) SeqTuple, Tuples, ValueArray
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports stdNum = System.Math

Namespace Linq

    <HideModuleName>
    Public Module IteratorExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        <DebuggerStepThrough>
        Public Function Tuples(Of T)(seq As IEnumerable(Of SeqValue(Of T))) As IEnumerable(Of (i%, val As T))
            Return seq.Select(Function(i) (i.i, i.value))
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="offset%"></param>
        ''' <returns></returns>
        ''' 
        <DebuggerStepThrough>
        <Extension>
        Public Iterator Function SeqIterator(source As IEnumerable, Optional offset% = 0) As IEnumerable(Of SeqValue(Of Object))
            Dim i As Integer = offset

            For Each o As Object In source
                Yield New SeqValue(Of Object)(i, o)
                i += 1
            Next
        End Function

        ''' <summary>
        ''' Iterates all of the objects in the source sequence with collection index position.
        ''' (``enumerate()`` 函数用于将一个可遍历的数据对象(如列表、元组或字符串)组合为一个索引序列，
        ''' 同时列出数据和数据下标，一般用在 Linq表达式 循环当中。
        ''' 这个拓展函数类似于python之中的 ``enumerate()`` 函数)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source">the source sequence</param>
        ''' <param name="offset">``[start=0]``下标的起始位置</param>
        ''' <returns>
        ''' ``[index, item_value]``
        ''' </returns>
        ''' 
        <DebuggerStepThrough>
        <Extension>
        Public Iterator Function SeqIterator(Of T)(source As IEnumerable(Of T), Optional offset% = 0) As IEnumerable(Of SeqValue(Of T))
            If Not source Is Nothing Then
                Dim idx% = offset

                For Each x As T In source
                    Yield New SeqValue(Of T)(idx, x)
                    idx += 1
                Next
            End If
        End Function

#If NET48_OR_GREATER Or NET8_0_OR_GREATER Then

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        <DebuggerStepThrough>
        Public Function SeqTuple(Of T1, T2)(tuple As (a As IEnumerable(Of T1), b As IEnumerable(Of T2)), Optional offset% = 0) As IEnumerable(Of SeqValue(Of (a As T1, b As T2)))
            Return (tuple.a.ToArray, tuple.b.ToArray).SeqTuple(offset)
        End Function

        <DebuggerStepThrough>
        <Extension>
        Public Iterator Function SeqTuple(Of T1, T2)(tuple As (x As T1(), y As T2()), Optional offset% = 0) As IEnumerable(Of SeqValue(Of (a As T1, b As T2)))
            Dim value As (T1, T2)
            Dim length% = stdNum.Max(tuple.x.Length, tuple.y.Length)

            For i As Integer = 0 To length - 1
                value = (
                    tuple.x.ElementAtOrDefault(i),
                    tuple.y.ElementAtOrDefault(i)
                )
                Yield New SeqValue(Of (T1, T2))(i + offset, value)
            Next
        End Function

#End If

        ''' <summary>
        ''' Move the enumerator pointer to next and get next value, if the pointer is reach the end, then will returns nothing
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Function [Next](Of T)(source As IEnumerator(Of T)) As T
            If source.MoveNext() Then
                Return source.Current
            Else
                Return Nothing
            End If
        End Function

        <Extension>
        Public Function Previous(Of T)(source As IEnumerator(Of T)) As T
            Throw New NotImplementedException
        End Function

        ''' <summary>
        ''' Creates an array from property <see cref="Value(Of T).IValueOf.Value"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        <DebuggerStepThrough>
        Public Function ValueArray(Of T)(source As IEnumerable(Of Value(Of T).IValueOf)) As T()
            Return source.Select(Function(o) o.Value).ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        <DebuggerStepThrough>
        Public Function Indices(Of T)(source As IEnumerable(Of SeqValue(Of T))) As Integer()
            Return source.Ordinals.ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        <DebuggerStepThrough>
        Public Function Ordinals(Of T)(source As IEnumerable(Of SeqValue(Of T))) As IEnumerable(Of Integer)
            Return source.Select(Function(o) o.i)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        <DebuggerStepThrough>
        Public Function Indices(Of T)(source As IEnumerable(Of T), assert As Func(Of T, Boolean)) As Integer()
            Return source _
                .SeqIterator _
                .Where(Function(x) True = assert(x.value)) _
                .Indices
        End Function
    End Module
End Namespace
