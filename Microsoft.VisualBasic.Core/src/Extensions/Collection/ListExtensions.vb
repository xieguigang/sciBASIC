#Region "Microsoft.VisualBasic::4117d4995955d06f42f2f581850d0eb5, Microsoft.VisualBasic.Core\src\Extensions\Collection\ListExtensions.vb"

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

    '   Total Lines: 380
    '    Code Lines: 198 (52.11%)
    ' Comment Lines: 144 (37.89%)
    '    - Xml Docs: 86.81%
    ' 
    '   Blank Lines: 38 (10.00%)
    '     File Size: 14.00 KB


    ' Module ListExtensions
    ' 
    '     Function: AppendAfter, AsHashList, AsHashSet, AsList, AsLoop
    '               Count, HasKey, Indexing, Pop, PopAt
    '               PopFirst, Random, ReorderByKeys, (+2 Overloads) ToList, TopMostFrequent
    ' 
    '     Sub: AddFirst, DoEach, (+2 Overloads) ForEach, RemoveAll, Swap
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

''' <summary>
''' Initializes a new instance of the <see cref="List"/>`1 class that
''' contains elements copied from the specified collection and has sufficient capacity
''' to accommodate the number of elements copied.
''' </summary>
Public Module ListExtensions

    ''' <summary>
    ''' append the <paramref name="list"/> after the collection <paramref name="first"/>.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="first"></param>
    ''' <param name="list"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function AppendAfter(Of T)(first As IEnumerable(Of T), ParamArray list As T()) As IEnumerable(Of T)
        For Each xi As T In first.SafeQuery
            Yield xi
        Next
        For Each xi As T In list
            Yield xi
        Next
    End Function

    <Extension>
    Public Function Count(Of T As IEquatable(Of T))(list As IEnumerable(Of T), item As T) As Integer
        Dim i As Integer = 0

        For Each obj In list
            If obj.Equals(item) Then
                i += 1
            End If
        Next

        Return i
    End Function

    ' this count method has already been defined in the .netcore base framework
    '''' <summary>
    '''' Get the element count which matched with the given <paramref name="predicate"/> expression
    '''' </summary>
    '''' <typeparam name="T"></typeparam>
    '''' <param name="list"></param>
    '''' <param name="predicate"></param>
    '''' <returns></returns>
    '<Extension>
    'Public Function Count(Of T)(list As IEnumerable(Of T), predicate As Predicate(Of T)) As Integer
    '    Return list.Where(AddressOf predicate.Invoke).Count
    'End Function

    '<Extension>
    'Public Function Count(Of T As IComparable(Of T))(list As IEnumerable(Of T), item As T) As Integer

    'End Function

    ''' <summary>
    ''' 查找出序列之中最频繁出现的对象(这个函数会自动跳过空值)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list"></param>
    ''' <returns>
    ''' this function returns the item with top frequency in the given list sequence; 
    ''' and nothing will be returns if the given collection is empty.
    ''' </returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function TopMostFrequent(Of T)(list As IEnumerable(Of T), Optional equals As IEqualityComparer(Of T) = Nothing) As T
        ' 因为可能会碰到list是空的情况，所以在这里需要使用FirstOrDefault
        If equals Is Nothing Then
            Return list _
                .SafeQuery _
                .Where(Function(x) Not x Is Nothing) _
                .GroupBy(Function(x) x) _
                .OrderByDescending(Function(g) g.Count) _
                .FirstOrDefault _
                .SafeQuery _
                .FirstOrDefault
        Else
            Return list _
                .SafeQuery _
                .Where(Function(x) Not x Is Nothing) _
                .GroupBy(Function(x) x, equals) _
                .OrderByDescending(Function(g) g.Count) _
                .FirstOrDefault _
                .SafeQuery _
                .FirstOrDefault
        End If
    End Function

    ''' <summary>
    ''' ForEach拓展的简化版本
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="collection"></param>
    ''' <param name="[do]"></param>
    <Extension>
    Public Sub DoEach(Of T)(collection As IEnumerable(Of T), [do] As Action(Of T))
        For Each x As T In collection.SafeQuery
            Call [do](x)
        Next
    End Sub

    ''' <summary>
    ''' 返回数组集合之中的一个随机位置的元素
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="v"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Random(Of T)(v As T()) As T
        Dim l% = randf.NextInteger(v.Length)
        Return v(l)
    End Function

    ''' <summary>
    ''' 根据对象的键名来进行重排序，请注意，要确保对象<paramref name="getKey"/>能够从泛型对象之中获取得到唯一的键名
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list"></param>
    ''' <param name="getKey"></param>
    ''' <param name="customOrder">可能会出现大小写不对的情况？</param>
    ''' <returns></returns>
    <Extension>
    Public Function ReorderByKeys(Of T)(list As IEnumerable(Of T), getKey As Func(Of T, String), customOrder$()) As List(Of T)
        Dim ls As List(Of T) = list.AsList
        Dim list2 As New List(Of T)
        Dim internalGet_geneObj =
            Function(id As String)
                ' 假若是对基因组进行排序，可能getkey函数只获取得到的是编号，而customOrder之中还会包含有全称，所以用InStr判断一下？
                Dim query = From element As T
                            In ls.AsParallel
                            Let key As String = getKey(element)
                            Where key.TextEquals(id) OrElse InStr(key, id, CompareMethod.Text) > 0
                            Select element

                Return query.FirstOrDefault
            End Function

        For Each ID As String In customOrder
            Dim selectedItem As T = internalGet_geneObj(ID)

            ' 由于是倒序的，故而将对象移动到最后一个元素即可
            If Not selectedItem Is Nothing Then
                Call list2.Add(selectedItem)
                Call ls.Remove(selectedItem)
            End If
        Next

        ' 添加剩余的没有在customOrder之中找到的数据
        Call list2.AddRange(ls)

        Return list2
    End Function

    ''' <summary>
    ''' 从一个对象集合中创建索引，请注意，传递进入这个函数的参数应该是经过去重操作之后的数据
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <returns>
    ''' returns an empty index collection if the given 
    ''' <paramref name="source"/> id set is empty or 
    ''' nothing.
    ''' </returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Indexing(Of T)(source As IEnumerable(Of T), Optional base As Integer = 0) As Index(Of T)
        Return New Index(Of T)(source, base)
    End Function

    ''' <summary>
    ''' swap the position of two specific element inside a given list collection object
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="l"></param>
    ''' <param name="i%"></param>
    ''' <param name="j%"></param>
    <Extension>
    Public Sub Swap(Of T)(ByRef l As System.Collections.Generic.List(Of T), i%, j%)
        Dim tmp = l(i)
        l(i) = l(j)
        l(j) = tmp
    End Sub

    ''' <summary>
    ''' for each loop
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="action"></param>
    <Extension>
    Public Sub ForEach(Of T)(source As IEnumerable(Of T), action As Action(Of T, Integer))
        For Each x As SeqValue(Of T) In source.SeqIterator
            Call action(x.value, x.i)
        Next
    End Sub

    ''' <summary>
    ''' for each loop
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="action"></param>
    <Extension>
    Public Sub ForEach(Of T)(source As IEnumerable(Of T), action As Action(Of T))
        For Each x As T In source.SafeQuery
            Call action(x)
        Next
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="List"/>`1 class that
    ''' contains elements copied from the specified collection and has sufficient capacity
    ''' to accommodate the number of elements copied.
    ''' </summary>
    ''' <param name="source">The collection whose elements are copied to the new list.</param>
    <Extension>
    Public Function ToList(Of T, TOut)(source As IEnumerable(Of T),
                                       [CType] As Func(Of T, TOut),
                                       Optional parallel As Boolean = False) As List(Of TOut)
        If source Is Nothing Then
            Return New List(Of TOut)
        End If

        Dim result As List(Of TOut)

        If parallel Then
            result = (From x As T In source.AsParallel Select [CType](x)).AsList
        Else
            result = (From x As T In source Select [CType](x)).AsList
        End If

        Return result
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsLoop(Of T)(src As IEnumerable(Of T)) As LoopArray(Of T)
        Return New LoopArray(Of T)(src)
    End Function

    ''' <summary>
    ''' Initializes a new instance of the <see cref="List(Of T)"/> class that
    ''' contains elements copied from the specified collection and has sufficient capacity
    ''' to accommodate the number of elements copied.
    ''' </summary>
    ''' <param name="source">
    ''' The collection whose elements are copied to the new list.
    ''' </param>
    ''' <remarks>
    ''' 如果source集合是空值的话，不会抛错
    ''' </remarks>
    <DebuggerStepThrough>
    <Extension>
    Public Function AsList(Of T)(source As IEnumerable(Of T)) As List(Of T)
        Return New List(Of T)(source)
    End Function

    ''' <summary>
    ''' Function name alias of the function <see cref="Hashtable.ContainsKey(Object)"/>
    ''' </summary>
    ''' <param name="hashtable"></param>
    ''' <param name="key"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function HasKey(hashtable As Hashtable, key As Object) As Boolean
        Return hashtable.ContainsKey(key)
    End Function

    ''' <summary>
    ''' Just using for the element index in a large collection
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="collection">
    ''' If the element in this collection have some duplicated member, then only the first element will be keeped.
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function AsHashSet(Of T)(collection As IEnumerable(Of T)) As Hashtable
        Dim table As New Hashtable

        For Each x As SeqValue(Of T) In collection.SeqIterator
            With x
                If Not table.ContainsKey(.value) Then
                    Call table.Add(.value, .i)
                End If
            End With
        Next

        Return table
    End Function

    ''' <summary>
    ''' <see cref="HashList(Of T)"/>
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsHashList(Of T As IAddressOf)(source As IEnumerable(Of T)) As HashList(Of T)
        Return New HashList(Of T)(source)
    End Function

    ''' <summary>
    ''' Initializes a new instance of the <see cref="List"/> class that
    ''' contains elements copied from the specified collection and has sufficient capacity
    ''' to accommodate the number of elements copied.
    ''' </summary>
    ''' <param name="linq">The collection whose elements are copied to the new list.</param>
    <Extension>
    Public Function ToList(Of T)(linq As ParallelQuery(Of T)) As List(Of T)
        Return New List(Of T)(linq)
    End Function

    <Extension>
    Public Function PopAt(Of T)(list As System.Collections.Generic.List(Of T), index As Integer) As T
        Dim getAt As T = list(index)
        list.RemoveAt(index)
        Return getAt
    End Function

    ''' <summary>
    ''' removes the last element inside the list and then returns it
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Pop(Of T)(list As System.Collections.Generic.List(Of T)) As T
        If list.Count > 0 Then
            Dim last As T = list.Last
            list.RemoveAt(list.Count - 1)
            Return last
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' Remove the first element from the list and then returns this removed element.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list"></param>
    ''' <returns></returns>
    <Extension>
    Public Function PopFirst(Of T)(ByRef list As System.Collections.Generic.List(Of T)) As T
        If list.IsNullOrEmpty Then
            Return Nothing
        Else
            Dim first As T = list(0)
            Call list.RemoveAt(0)
            Return first
        End If
    End Function

    <Extension>
    Public Sub RemoveAll(Of T)(list As System.Collections.Generic.List(Of T), all As IEnumerable(Of T))
        If Not all Is Nothing Then
            For Each item As T In all
                Call list.Remove(item)
            Next
        End If
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Sub AddFirst(Of T)(ByRef list As System.Collections.Generic.List(Of T), x As T)
        Call list.Insert(0, x)
    End Sub
End Module
