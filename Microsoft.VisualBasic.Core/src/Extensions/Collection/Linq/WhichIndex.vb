#Region "Microsoft.VisualBasic::8010d2a75b0505934abab58b936643f3, Microsoft.VisualBasic.Core\src\Extensions\Collection\Linq\WhichIndex.vb"

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

    '   Total Lines: 229
    '    Code Lines: 109 (47.60%)
    ' Comment Lines: 94 (41.05%)
    '    - Xml Docs: 91.49%
    ' 
    '   Blank Lines: 26 (11.35%)
    '     File Size: 9.17 KB


    '     Module WhichSymbol
    ' 
    ' 
    ' 
    '     Class WhichIndex
    ' 
    '         Properties: Symbol
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: [True], GetMinIndex, Index, (+2 Overloads) IsFalse, (+2 Overloads) IsGreaterThan
    '                   (+3 Overloads) IsTrue, (+2 Overloads) Max, Min, Top
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Linq

    <HideModuleName>
    Public Module WhichSymbol

        ''' <summary>
        ''' # Which indices are TRUE?
        ''' 
        ''' Give the TRUE indices of a logical object, allowing for array indices.
        ''' </summary>

        Public ReadOnly which As New WhichIndex

    End Module

    ''' <summary>
    ''' Where is the Min() or Max() or first TRUE or FALSE ?
    ''' (这个模块之中的函数返回来的都是集合之中的符合条件的对象元素的index坐标)
    ''' </summary>
    Public NotInheritable Class WhichIndex

        Public Shared ReadOnly Property Symbol As WhichIndex
            Get
                Return which
            End Get
        End Property

        ''' <summary>
        ''' Give the TRUE indices of a logical object, allowing for array indices.
        ''' </summary>
        ''' <param name="booleans">
        ''' a logical vector or array. NAs are allowed and omitted (treated as if FALSE).
        ''' </param>
        ''' <returns>
        ''' Basically, the result is (1:length(x))[x] in typical cases; 
        ''' more generally, including when x has NA's, which(x) is 
        ''' seq_along(x)[!is.na(x) &amp; x] plus names when x has.
        ''' </returns>
        ''' <remarks>
        ''' Unlike most other base R functions this does not coerce x to logical: 
        ''' only arguments with typeof logical are accepted and others give an 
        ''' error.
        ''' </remarks>
        Default Public ReadOnly Property arrayInd(booleans As IEnumerable(Of Boolean)) As IEnumerable(Of Integer)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.IsTrue(booleans)
            End Get
        End Property

        ''' <summary>
        ''' Returns the collection element its index where the test expression <paramref name="predicate"/> result is TRUE
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="predicate"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Index(Of T)(source As IEnumerable(Of T), predicate As Func(Of T, Boolean)) As IEnumerable(Of Integer)
            Return source _
                .SeqIterator _
                .Where(Function(i) predicate(i.value)) _
                .Select(Function(o) o.i)
        End Function

        Public Function GetMinIndex(values As List(Of Double)) As Integer
            Dim min As Double = Double.MaxValue
            Dim minIndex As Integer = 0
            For i As Integer = 0 To values.Count - 1
                If values(i) < min Then
                    min = values(i)
                    minIndex = i
                End If
            Next

            Return minIndex
        End Function

        ''' <summary>
        ''' 在这里不适用Module类型，要不然会和其他的Max拓展函数产生冲突的。。
        ''' </summary>
        Friend Sub New()
        End Sub

        ''' <summary>
        ''' Determines the location, i.e., index of the (first) minimum or maximum of a numeric (or logical) vector.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x">
        ''' numeric (logical, integer or double) vector or an R object for which the internal coercion to 
        ''' double works whose min or max is searched for.
        ''' </param>
        ''' <returns>
        ''' returns -1 means empty collection
        ''' </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Max(Of T As IComparable)(x As IEnumerable(Of T)) As Integer
            Return x.MaxIndex
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Max(Of T, K As IComparable)(x As IEnumerable(Of T), compareProject As Func(Of T, K)) As Integer
            Return x.Select(compareProject).MaxIndex
        End Function

        ''' <summary>
        ''' Determines the location, i.e., index of the (first) minimum or maximum of a numeric (or logical) vector.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x">
        ''' numeric (logical, integer or double) vector or an R object for which the internal coercion to 
        ''' double works whose min or max is searched for.
        ''' </param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Min(Of T As IComparable)(x As IEnumerable(Of T)) As Integer
            Return x.MinIndex
        End Function

        ''' <summary>
        ''' Return all of the indices which is True
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns>
        ''' the result index is zero-based by default
        ''' </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsTrue(v As IEnumerable(Of Boolean), Optional offset% = 0) As Integer()
            Return v _
                .SeqIterator _
                .Where(Function(b) True = +b) _
                .Select(Function(i) CInt(i) + offset) _
                .ToArray
        End Function

        ''' <summary>
        ''' Syntax helper for <see cref="VectorShadows(Of T)(IEnumerable(Of T))"/>
        ''' </summary>
        ''' <param name="list"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsTrue(list As Object) As Integer()
            Return IsTrue(DirectCast(list, IEnumerable).Cast(Of Object).Select(Function(o) CBool(o)))
        End Function

        ''' <summary>
        ''' Returns all of the indices which is False
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function IsFalse(v As IEnumerable(Of Boolean)) As Integer()
            Return v _
                .SeqIterator _
                .Where(Function(b) False = +b) _
                .Select(Function(i) CInt(i)) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsTrue([operator] As Func(Of Boolean())) As Integer()
            Return IsTrue([operator]())
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsFalse([operator] As Func(Of Boolean())) As Integer()
            Return WhichIndex.IsFalse([operator]())
        End Function

        Public Function Top(Of T As IComparable(Of T))(seq As IEnumerable(Of T), n As Integer) As Integer()
            Return seq.SeqIterator _
                .OrderByDescending(Function(x) x.value) _
                .Take(n) _
                .Ordinals _
                .ToArray
        End Function

        ''' <summary>
        ''' 查找出列表之中符合条件的所有的索引编号
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="array"></param>
        ''' <param name="condi"></param>
        ''' <returns></returns>
        Public Iterator Function [True](Of T)(array As IEnumerable(Of T), condi As Predicate(Of T)) As IEnumerable(Of Integer)
            Dim i%

            For Each x As T In array
                If True = condi(x) Then
                    Yield i
                End If

                i += 1
            Next
        End Function

        ''' <summary>
        ''' 枚举出所有大于目标的顶点编号
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="compareTo"></param>
        ''' <returns></returns>
        ''' <remarks>因为这个返回的是一个迭代器，所以可以和First结合产生FirstGreaterThan表达式</remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsGreaterThan(Of T As IComparable)(source As IEnumerable(Of T), compareTo As T) As IEnumerable(Of Integer)
            Return source _
                .SeqIterator _
                .Where(Function(o) Language.GreaterThan(o.value, compareTo)) _
                .Select(Function(i) i.i) ' 因为返回的是linq表达式，所以这里就不加ToArray了
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IsGreaterThan(Of T, C As IComparable)(source As IEnumerable(Of T), getValue As Func(Of T, C), compareTo As C) As IEnumerable(Of Integer)
            Return which.IsGreaterThan(source.Select(getValue), compareTo)
        End Function
    End Class
End Namespace
