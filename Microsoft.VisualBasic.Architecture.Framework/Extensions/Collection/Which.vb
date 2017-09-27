#Region "Microsoft.VisualBasic::345f4e30f824de65a69591cdb80d8b23, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Collection\Which.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Linq

    ''' <summary>
    ''' Where is the Min() or Max() or first TRUE or FALSE ?
    ''' (这个模块之中的函数返回来的都是集合之中的符合条件的对象元素的index坐标)
    ''' </summary>
    Public NotInheritable Class Which

        ''' <summary>
        ''' Returns the collection element its index where the test expression <paramref name="predicate"/> result is TRUE
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="predicate"></param>
        ''' <returns></returns>
        Public Shared Function Index(Of T)(source As IEnumerable(Of T), predicate As Func(Of T, Boolean)) As IEnumerable(Of Integer)
            Return source _
                .SeqIterator _
                .Where(Function(i) predicate(i.value)) _
                .Select(Function(o) o.i)
        End Function

        Public Shared Function GetMinIndex(values As List(Of Double)) As Integer
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
        Private Sub New()
        End Sub

        ''' <summary>
        ''' Determines the location, i.e., index of the (first) minimum or maximum of a numeric (or logical) vector.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x">
        ''' numeric (logical, integer or double) vector or an R object for which the internal coercion to 
        ''' double works whose min or max is searched for.
        ''' </param>
        ''' <returns></returns>
        Public Shared Function Max(Of T As IComparable)(x As IEnumerable(Of T)) As Integer
            Return x.MaxIndex
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
        Public Shared Function Min(Of T As IComparable)(x As IEnumerable(Of T)) As Integer
            Return x.MinIndex
        End Function

        ''' <summary>
        ''' Return all of the indices which is True
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Public Shared Function IsTrue(v As IEnumerable(Of Boolean)) As Integer()
            Return v _
                .SeqIterator _
                .Where(Function(b) True = +b) _
                .Select(Function(i) CInt(i)) _
                .ToArray
        End Function

        ''' <summary>
        ''' Syntax helper for <see cref="VectorShadows(Of T)(IEnumerable(Of T))"/>
        ''' </summary>
        ''' <param name="list"></param>
        ''' <returns></returns>
        Public Shared Function IsTrue(list As Object) As Integer()
            Return IsTrue(DirectCast(list, IEnumerable).Cast(Of Object).Select(Function(o) CBool(o)))
        End Function

        ''' <summary>
        ''' Returns all of the indices which is False
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Public Shared Function IsFalse(v As IEnumerable(Of Boolean)) As Integer()
            Return v _
                .SeqIterator _
                .Where(Function(b) False = +b) _
                .Select(Function(i) CInt(i)) _
                .ToArray
        End Function

        Public Shared Function IsTrue([operator] As Func(Of Boolean())) As Integer()
            Return IsTrue([operator]())
        End Function

        Public Shared Function IsFalse([operator] As Func(Of Boolean())) As Integer()
            Return Which.IsFalse([operator]())
        End Function

        ''' <summary>
        ''' 查找出列表之中符合条件的所有的索引编号
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="array"></param>
        ''' <param name="condi"></param>
        ''' <returns></returns>
        Public Shared Iterator Function [True](Of T)(array As IEnumerable(Of T), condi As Assert(Of T)) As IEnumerable(Of Integer)
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
        Public Shared Function IsGreaterThan(Of T As IComparable)(source As IEnumerable(Of T), compareTo As T) As IEnumerable(Of Integer)
            Return source _
                .SeqIterator _
                .Where(Function(o) Language.GreaterThan(o.value, compareTo)) _
                .Select(Function(i) i.i) ' 因为返回的是linq表达式，所以这里就不加ToArray了
        End Function

        Public Shared Function IsGreaterThan(Of T, C As IComparable)(source As IEnumerable(Of T), getValue As Func(Of T, C), compareTo As C) As IEnumerable(Of Integer)
            Return Which.IsGreaterThan(source.Select(getValue), compareTo)
        End Function
    End Class
End Namespace
