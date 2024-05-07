#Region "Microsoft.VisualBasic::9e76e47818c645eb46c8c532918f4fd3, G:/GCModeller/src/runtime/sciBASIC#/Microsoft.VisualBasic.Core/src//ComponentModel/System.Collections.Generic/IndexOf.vb"

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

    '   Total Lines: 506
    '    Code Lines: 270
    ' Comment Lines: 179
    '   Blank Lines: 57
    '     File Size: 18.64 KB


    '     Class Index
    ' 
    '         Properties: Count, Map, Objects
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) Add, EnumerateMapKeys, GetEnumerator, GetOrdinal, GetSynonymOrdinal
    '                   IEnumerable_GetEnumerator, indexing, Indexing, (+2 Overloads) Intersect, NotExists
    '                   ToString
    ' 
    '         Sub: [Set], Add, AddList, Clear, (+2 Overloads) Delete
    ' 
    '         Operators: (+2 Overloads) -, (+2 Overloads) +, <, <>, =
    '                    >, (+2 Overloads) IsFalse, (+2 Overloads) IsTrue, (+2 Overloads) Like
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Collection

    ''' <summary>
    ''' Mappings of ``key As String -> index As Integer``
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks>
    ''' ###### 2017-12-10
    ''' 
    ''' 经过测试，字典对象是完全可以容纳UniProt数据库之中的物种的数量上限的
    ''' </remarks>
    Public Class Index(Of T) : Implements IEnumerable(Of SeqValue(Of T))

        Dim maps As New Dictionary(Of T, Integer)
        ''' <summary>
        ''' list value will be set to nothing on <see cref="Delete(T)"/>
        ''' </summary>
        Dim index As HashList(Of SeqValue(Of T))
        ''' <summary>
        ''' the index offset value ,zero by default
        ''' </summary>
        Dim base%

        ''' <summary>
        ''' 获取包含在<see cref="System.Collections.Generic.Dictionary"/>中的键/值对的数目。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Count As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return maps.Count
            End Get
        End Property

        ''' <summary>
        ''' Gets the input object keys that using for the construction of this index.
        ''' </summary>
        ''' <returns>an array of value copy of the map keys</returns>
        Public ReadOnly Property Objects As T()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return maps.Keys.ToArray
            End Get
        End Property

        ''' <summary>
        ''' 请注意，这里的数据源请尽量使用Distinct的，否则对于重复的数据，只会记录下第一个位置
        ''' </summary>
        ''' <param name="source"></param>
        Sub New(source As IEnumerable(Of T), Optional base% = 0)
            If source Is Nothing Then
                source = {}
            End If

            For Each x As SeqValue(Of T) In source.SeqIterator
                If Not maps.ContainsKey(x.value) Then
                    Call maps.Add(x.value, x.i + base)
                End If
            Next

            Me.base = base
            Me.index = indexing()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function indexing() As HashList(Of SeqValue(Of T))
            Return maps _
                .Select(Function(s)
                            Return New SeqValue(Of T) With {
                                .i = s.Value,
                                .value = s.Key
                            }
                        End Function) _
                .AsHashList
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(ParamArray vector As T())
            Call Me.New(source:=DirectCast(vector, IEnumerable(Of T)))
        End Sub

        ''' <summary>
        ''' 默认是从0开始的
        ''' </summary>
        ''' <param name="base"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(base As Integer)
            Call Me.New({}, base:=base)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="maps">如果是json加载，可能会出现空值的字典</param>
        ''' <param name="base%"></param>
        Sub New(maps As IDictionary(Of T, Integer), Optional base% = 0)
            Static emptyIndex As [Default](Of IDictionary(Of String, Integer)) = New Dictionary(Of String, Integer)

            Me.base = base
            Me.maps = New Dictionary(Of T, Integer)(dictionary:=maps Or emptyIndex)
            Me.index = indexing()
        End Sub

        ''' <summary>
        ''' 获取目标对象在本索引之中的位置编号，不存在则返回-1
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns>
        ''' this function always returns -1(ignores of the <see cref="base"/> offset value) 
        ''' if the target element is not found.
        ''' </returns>
        Default Public ReadOnly Property IndexOf(x As T) As Integer
            Get
                If maps.ContainsKey(x) Then
                    Return maps(x)
                Else
                    Return -1
                End If
            End Get
        End Property

        Default Public ReadOnly Property IndexOf([set] As IEnumerable(Of T)) As Integer()
            Get
                Return (From i As T
                        In [set]
                        Select Me.IndexOf(i)).ToArray
            End Get
        End Property

        ''' <summary>
        ''' 直接通过索引获取目标对象的值，请注意，如果<typeparamref name="T"/>泛型类型是<see cref="Integer"/>，
        ''' 则如果需要查找index的话，则必须要显式的指定参数名为``x:=``，否则调用的是当前的这个索引方法，
        ''' 得到错误的结果
        ''' </summary>
        ''' <param name="index%"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property IndexOf(index As Integer) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.index(index).value
            End Get
        End Property

        ''' <summary>
        ''' 与<see cref="Objects"/>只读属性的功能相似，只不过这个函数是Linq枚举器模式
        ''' </summary>
        ''' <returns></returns>
        Public Function EnumerateMapKeys() As IEnumerable(Of String)
            Return maps.Keys.AsEnumerable
        End Function

        ''' <summary>
        ''' get element index
        ''' </summary>
        ''' <param name="items"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetOrdinal(items As IEnumerable(Of T)) As Integer()
            Return items.Select(Function(element) Me(element)).ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="synonym"></param>
        ''' <returns>this function returns -1 if synonym value is not found inside the index</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetSynonymOrdinal(ParamArray synonym As T()) As Integer
            Return GetOrdinal(synonym) _
                .Where(Function(i) i > -1) _
                .DefaultFirst(-1)
        End Function

        Public Sub [Set](index As Integer, val As T)
            Me.maps(val) = index
            Me.index.Item(index) = New SeqValue(Of T) With {
                .i = index,
                .value = val
            }
        End Sub

        ''' <summary>
        ''' just removes the key, the index ordinal offset will not make any changes
        ''' </summary>
        ''' <param name="index"></param>
        Public Sub Delete(index As T)
            Dim i = Me.IndexOf(index)

            If i > -1 Then
                Me.maps.Remove(index)
                Me.index(i) = Nothing
            End If
        End Sub

        ''' <summary>
        ''' just removes the key, the index ordinal offset will not make any changes
        ''' </summary>
        ''' <param name="index"></param>
        Public Sub Delete(ParamArray index As T())
            For Each item As T In index
                Call Delete(item)
            Next
        End Sub

        Public Iterator Function Intersect(collection As IEnumerable(Of T)) As IEnumerable(Of T)
            For Each x As T In collection
                If IndexOf(x) > -1 Then
                    Yield x
                End If
            Next
        End Function

        Public Iterator Function Intersect(compares As Index(Of T)) As IEnumerable(Of T)
            For Each x As T In compares.maps.Keys
                If IndexOf(x) > -1 Then
                    Yield x
                End If
            Next
        End Function

        ''' <summary>
        ''' For Linq ``where``
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function NotExists(x As T) As Boolean
            Return IndexOf(x) = -1
        End Function

        ''' <summary>
        ''' 这个函数是线程不安全的
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns>
        ''' 这个函数所返回来的值是所添加的<paramref name="x"/>的index值
        ''' </returns>
        ''' <remarks>
        ''' this function will ignores of the existed duplicated item
        ''' </remarks>
        Public Function Add(x As T) As Integer
            If Not maps.ContainsKey(x) Then
                Call maps.Add(x, maps.Count + base)
                Call index.Add(
                    x:=New SeqValue(Of T) With {
                        .i = maps(x),
                        .value = x
                    })
            End If

            Return maps(x)
        End Function

        ''' <summary>
        ''' insert the specific element into the specific location of the index
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="index"></param>
        Public Sub Add(x As T, index As Integer)
            Call Me.maps.Add(x, index)
            Call Me.index.Add(New SeqValue(Of T) With {
                .i = index,
                .value = x
            })
        End Sub

        Public Sub AddList(ParamArray x As T())
            For Each xi As T In x
                Call Add(xi)
            Next
        End Sub

        ''' <summary>
        ''' add a collection of the unique items into current index object
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Iterator Function Add(x As T()) As IEnumerable(Of Integer)
            For Each xi As T In x
                Yield Add(xi)
            Next
        End Function

        ''' <summary>
        ''' clear mapping and reset the index offset
        ''' </summary>
        Public Sub Clear()
            Call maps.Clear()
            Call index.Clear()
        End Sub

        ''' <summary>
        ''' Display the input source sequence.
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return maps.Keys _
                .Select(Function(x) x.ToString) _
                .ToArray _
                .GetJson
        End Function

        ''' <summary>
        ''' Returns the ``obj => index`` mapping table.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Map As Dictionary(Of T, Integer)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me
            End Get
        End Property

        ''' <summary>
        ''' cast index to a dictionary mapping of object element value to its index value
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(index As Index(Of T)) As Dictionary(Of T, Integer)
            Return New Dictionary(Of T, Integer)(index.maps)
        End Operator

        ''' <summary>
        ''' Create a index of target element array with index base zero
        ''' </summary>
        ''' <param name="objs"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(objs As T()) As Index(Of T)
            Return New Index(Of T)(source:=objs)
        End Operator

        ''' <summary>
        ''' Create a index of target element array with index base zero
        ''' </summary>
        ''' <param name="list"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(list As List(Of T)) As Index(Of T)
            Return New Index(Of T)(source:=list)
        End Operator

        ''' <summary>
        ''' Add a new key to this index object.
        ''' </summary>
        ''' <param name="index">Element key index object.</param>
        ''' <param name="element"></param>
        ''' <returns></returns>
        Public Shared Operator +(index As Index(Of T), element As T) As Index(Of T)
            Call index.Add(element)
            Return index
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(list As List(Of T), index As Index(Of T)) As List(Of T)
            Return list + index.Objects
        End Operator

        ''' <summary>
        ''' Delete items from source <paramref name="index"/> and then returns the new modify index collection
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="list"></param>
        ''' <returns></returns>
        Public Shared Operator -(index As Index(Of T), list As List(Of T)) As Index(Of T)
            ' make a data copy
            Dim table As New Dictionary(Of T, Integer)(index.maps)

            For Each item As T In list
                If table.ContainsKey(item) Then
                    Call table.Remove(item)
                End If
            Next

            Return New Index(Of T)(table.Keys)
        End Operator

        ''' <summary>
        ''' <paramref name="item"/> is one of the element in <paramref name="indexr"/>
        ''' </summary>
        ''' <param name="item"></param>
        ''' <param name="indexr"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Like(item As T, indexr As Index(Of T)) As Boolean
            If item Is Nothing OrElse indexr Is Nothing Then
                Return False
            Else
                Return indexr(x:=item) > -1
            End If
        End Operator

        Public Iterator Function GetEnumerator() As IEnumerator(Of SeqValue(Of T)) Implements IEnumerable(Of SeqValue(Of T)).GetEnumerator
            For Each o As SeqValue(Of T) In index
                ' 20231227
                ' handling of the delete operation result
                If o.value Is Nothing AndAlso o.i = 0 Then
                    Continue For
                End If

                Yield o
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        ''' <summary>
        ''' The element numbers in current index object is equals to given count number value?
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="count%"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(index As Index(Of T), count%) As Boolean
            Return index.Count = count
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(index As Index(Of T), count%) As Boolean
            Return Not index = count
        End Operator

        ''' <summary>
        ''' The element count inside current index object greater than the given <paramref name="count"/>?
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="count%"></param>
        ''' <returns></returns>
        Public Shared Operator >(index As Index(Of T), count%) As Boolean
            Return index.Count > count
        End Operator

        ''' <summary>
        ''' The element count inside current index object smaller than the given <paramref name="count"/>?
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="count%"></param>
        ''' <returns></returns>
        Public Shared Operator <(index As Index(Of T), count%) As Boolean
            Return index.Count < count
        End Operator

        Public Shared Operator IsTrue(index As Index(Of T)) As Boolean
            If index Is Nothing Then
                Return False
            ElseIf index.Count = 0 Then
                Return False
            ElseIf index.Count = 1 AndAlso Len(CObj(index.Objects(Scan0))) = 0 Then
                Return False
            Else
                Return True
            End If
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator IsFalse(index As Index(Of T)) As Boolean
            Return Not op_True(index)
        End Operator

        ''' <summary>
        ''' removes elements from <paramref name="list"/> based on a 
        ''' given <paramref name="filter"/> element list.
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="filter"></param>
        ''' <returns>a new sequence that not have any elements 
        ''' inside the <paramref name="filter"/> list.
        ''' </returns>
        Public Shared Operator -(list As T(), filter As Index(Of T)) As T()
            Return list.Where(Function(i) Not i Like filter).ToArray
        End Operator

        ''' <summary>
        ''' create a zero-based object index
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        Public Shared Function Indexing(data As IEnumerable(Of T)) As Dictionary(Of T, Integer)
            Dim i As Integer = Scan0
            Dim index As New Dictionary(Of T, Integer)

            For Each item As T In data
                If Not index.ContainsKey(item) Then
                    index.Add(item, i)
                End If

                i += 1
            Next

            Return index
        End Function
    End Class
End Namespace
