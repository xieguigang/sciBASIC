#Region "Microsoft.VisualBasic::3569dcb78a2ae1e811fdf4b54f36f84d, G:/GCModeller/src/runtime/sciBASIC#/Microsoft.VisualBasic.Core/src//Language/Linq/List.vb"

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

    '   Total Lines: 684
    '    Code Lines: 367
    ' Comment Lines: 253
    '   Blank Lines: 64
    '     File Size: 26.34 KB


    '     Class List
    ' 
    '         Properties: First, Last
    ' 
    '         Constructor: (+5 Overloads) Sub New
    '         Function: [Default], Poll, Pop, PopAll, ReverseIterator
    '                   ValuesEnumerator
    '         Operators: (+5 Overloads) -, *, ^, (+9 Overloads) +, <
    '                    <=, (+2 Overloads) <>, (+2 Overloads) =, >, >=
    '                    >>
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.UnixBash.FileSystem
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Expressions

Namespace Language

    ''' <summary>
    ''' Represents a strongly typed list of objects that can be accessed by index. Provides
    ''' methods to search, sort, and manipulate lists.To browse the .NET Framework source
    ''' code for this type, see the Reference Source.
    ''' (加强版的<see cref="System.Collections.Generic.List(Of T)"/>)
    ''' </summary>
    ''' <typeparam name="T">The type of elements in the list.</typeparam>
    Public Class List(Of T) : Inherits System.Collections.Generic.List(Of T)

#Region "Improvements Index"

        ''' <summary>
        ''' The last elements in the collection <see cref="List(Of T)"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property Last As T
            Get
                If Count() = 0 Then
                    Return Nothing
                Else
                    Return MyBase.Item(Count() - 1)
                End If
            End Get
            Set(value As T)
                If Count() = 0 Then
                    Call Add(value)
                Else
                    MyBase.Item(Count() - 1) = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' The first elements in the collection <see cref="List(Of T)"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property First As T
            Get
                If Count() = 0 Then
                    Return Nothing
                Else
                    Return MyBase.Item(0)
                End If
            End Get
            Set(value As T)
                If Count() = 0 Then
                    Call Add(value)
                Else
                    MyBase.Item(Scan0) = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="args">同时支持boolean和integer</param>
        ''' <returns></returns>
        Default Public Overloads Property Item(args As Object) As List(Of T)
            Get
                Dim index = Indexer.Indexing(args)
                Return Me(index)
            End Get
            Set
                Dim index = Indexer.Indexing(args)
                Me(index) = Value
            End Set
        End Property

        ''' <summary>
        ''' This indexer property is using for the ODEs-system computing.
        ''' (这个是为了ODEs计算模块所准备的一个数据接口)
        ''' </summary>
        ''' <param name="address"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(address As IAddress(Of Integer)) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Item(address.Address)
            End Get
            Set(value As T)
                Item(address.Address) = value
            End Set
        End Property

        ''' <summary>
        ''' Can accept negative number as the index value, negative value means ``<see cref="Count"/> - n``, 
        ''' example as ``list(-1)``: means the last element in this list: ``list(list.Count -1)``
        ''' </summary>
        ''' <param name="index%"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(index%) As T
            Get
                If index < 0 Then
                    index = Count() + index  ' -1 -> count -1
                End If
                Return MyBase.Item(index)
            End Get
            Set(value As T)
                If index < 0 Then
                    index = Count() + index  ' -1 -> count -1
                End If
                MyBase.Item(index) = value
            End Set
        End Property

        Default Public Overloads Property Item(index As Integer?) As T
            Get
                Return Item(index.Value)
            End Get
            Set(value As T)
                Item(index.Value) = value
            End Set
        End Property

        ''' <summary>
        ''' Can accept negative number as the index value, negative value means ``<see cref="Count"/> - n``, 
        ''' example as ``list(-1)``: means the last element in this list: ``list(list.Count -1)``
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(index As i32) As T
            Get
                Return Item(index.Value)
            End Get
            Set(value As T)
                Item(index.Value) = value
            End Set
        End Property

        ''' <summary>
        ''' Using a index vector expression to select/update many elements from this list collection.
        ''' </summary>
        ''' <param name="exp$">
        ''' + ``1``, index=1
        ''' + ``1:8``, index=1, count=8
        ''' + ``1->8``, index from 1 to 8
        ''' + ``8->1``, index from 8 to 1
        ''' + ``1,2,3,4``, index=1 or  2 or 3 or 4
        ''' </param>
        ''' <returns></returns>
        Default Public Overloads Property Item(exp$) As List(Of T)
            Get
                Dim list As New List(Of T)

                For Each i% In exp.TranslateIndex
                    list += Item(index:=i)
                Next

                Return list
            End Get
            Set(value As List(Of T))
                For Each i As SeqValue(Of Integer) In exp.TranslateIndex.SeqIterator
                    Item(index:=+i) = value(i.i)
                Next
            End Set
        End Property

        Default Public Overloads Property Item(range As IntRange) As List(Of T)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New List(Of T)(Me.Skip(range.Min).Take(range.Interval))
            End Get
            Set(value As List(Of T))
                Dim indices As Integer() = range.ToArray

                For i As Integer = 0 To indices.Length - 1
                    Item(index:=indices(i)) = value(i)
                Next
            End Set
        End Property

        Default Public Overloads Property Item(indices As IEnumerable(Of Integer)) As List(Of T)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New List(Of T)(indices.Select(Function(i) Item(index:=i)))
            End Get
            Set(value As List(Of T))
                For Each i As SeqValue(Of Integer) In indices.SeqIterator
                    Item(index:=+i) = value(i.i)
                Next
            End Set
        End Property

        ''' <summary>
        ''' Select all of the elements from this list collection is any of them match the condition expression: <paramref name="where"/>
        ''' </summary>
        ''' <param name="[where]"></param>
        ''' <returns></returns>
        Default Public Overloads ReadOnly Property Item([where] As Predicate(Of T)) As T()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return MyBase.Where(Function(o) where(o)).ToArray
            End Get
        End Property

        Default Public Overloads Property Item(booleans As IEnumerable(Of Boolean)) As T()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me(which(booleans))
            End Get
            Set(value As T())
                For Each i In booleans.SeqIterator
                    If i.value Then
                        MyBase.Item(i) = value(i)
                    End If
                Next
            End Set
        End Property
#End Region

        ''' <summary>
        ''' Initializes a new instance of the <see cref="List(Of T)"/> class that
        ''' contains elements copied from the specified collection and has sufficient capacity
        ''' to accommodate the number of elements copied.
        ''' </summary>
        ''' <param name="source">The collection whose elements are copied to the new list.</param>
        ''' <remarks>
        ''' (这是一个安全的构造函数，假若输入的参数为空值，则只会创建一个空的列表，而不会抛出错误)
        ''' </remarks>
        <DebuggerStepThrough>
        Sub New(source As IEnumerable(Of T))
            Call MyBase.New(If(source Is Nothing, {}, source.ToArray))
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="List(Of T)"/> class that
        ''' contains elements copied from the specified collection and has sufficient capacity
        ''' to accommodate the number of elements copied.
        ''' </summary>
        ''' <param name="x">The collection whose elements are copied to the new list.</param>
        ''' 
        <DebuggerStepThrough>
        Sub New(ParamArray x As T())
            Call MyBase.New(If(x Is Nothing, {}, x))
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the Microsoft VisualBasic language <see cref="List(Of T)"/> class 
        ''' that is empty and has the default initial capacity.
        ''' </summary>
        ''' 
        <DebuggerStepThrough>
        Public Sub New()
            Call MyBase.New
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="List(Of T)"/> class that
        ''' is empty and has the specified initial capacity.
        ''' </summary>
        ''' <param name="capacity">The number of elements that the new list can initially store.</param>
        ''' 
        <DebuggerStepThrough>
        Public Sub New(capacity As Integer)
            Call MyBase.New(capacity)
        End Sub

        <DebuggerStepThrough>
        Public Sub New(size As Integer, fill As T)
            For i As Integer = 0 To size - 1
                Call Add(fill)
            Next
        End Sub

        ' 这个Add方法会导致一些隐式转换的类型匹配失败，所以删除掉这个方法
        'Public Overloads Sub Add(data As IEnumerable(Of T))
        '    Call MyBase.AddRange(data.SafeQuery)
        'End Sub

        ''' <summary>
        ''' Pop all of the elements value in to array from the list object 
        ''' and then clear all of the <see cref="List(Of T)"/> data.
        ''' </summary>
        ''' <returns></returns>
        Public Function PopAll() As T()
            Dim array As T() = ToArray()
            Call Clear()
            Return array
        End Function

        ''' <summary>
        ''' Adds an object to the end of the <see cref="List(Of T)"/>.
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="x">
        ''' The object to be added to the end of the <see cref="List(Of T)"/>. 
        ''' The value can be null for reference types.
        ''' </param>
        ''' <returns></returns>
        Public Shared Operator +(list As List(Of T), x As T) As List(Of T)
            If list Is Nothing Then
                Return New List(Of T) From {x}
            Else
                Call list.Add(x)
                Return list
            End If
        End Operator

        ''' <summary>
        ''' Adds an object to the end of the <see cref="List(Of T)"/>.
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="x">
        ''' The object to be added to the end of the <see cref="List(Of T)"/>. 
        ''' The value can be null for reference types.
        ''' </param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(list As List(Of T), x As Value(Of T)) As List(Of T)
            Return list + x.Value
        End Operator

        ''' <summary>
        ''' Adds an object to the begin of the <see cref="List(Of T)"/>.
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="x">The object to be added to the end of the <see cref="List(Of T)"/>. The
        ''' value can be null for reference types.</param>
        ''' <returns></returns>
        Public Shared Operator +(x As T, list As List(Of T)) As List(Of T)
            If list Is Nothing Then
                Return New List(Of T) From {x}
            Else
                Call list.Insert(Scan0, x)
                Return list
            End If
        End Operator

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="n">
        ''' If parameter <paramref name="n"/> equals to ZERO, then just 
        ''' <see cref="Clear()"/> the list contents and keeps the object 
        ''' reference.
        ''' </param>
        ''' <returns></returns>
        Public Shared Operator *(list As List(Of T), n%) As List(Of T)
            Select Case n
                Case < 0
                    Return New List(Of T)
                Case 0
                    Call list.Clear()
                    Return list
                Case 1
                    Return New List(Of T)(list)
                Case > 1
                    Dim out As New List(Of T)

                    For i As Integer = 1 To n
                        out.AddRange(list)
                    Next

                    Return out
                Case Else
                    Throw New NotImplementedException
            End Select
        End Operator

        ''' <summary>
        ''' Removes the first occurrence of a specific object from the <see cref="List(Of T)"/>.
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="x">The object to remove from the <see cref="List(Of T)"/>. The value can
        ''' be null for reference types.</param>
        ''' <returns></returns>
        Public Shared Operator -(list As List(Of T), x As T) As List(Of T)
            Call list.Remove(x)
            Return list
        End Operator

        ''' <summary>
        ''' Adds the elements of the specified collection to the end of the <see cref="List(Of T)"/>.
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="vals"></param>
        ''' <returns></returns>
        Public Shared Operator +(list As List(Of T), vals As IEnumerable(Of T)) As List(Of T)
            If Not vals Is Nothing Then
                Call list.AddRange(vals.ToArray)
            End If
            Return list
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(list As List(Of T), [iterator] As Func(Of IEnumerable(Of T))) As List(Of T)
            Return list + iterator()
        End Operator

        ' 下面的操作符会导致重载失败
        '<MethodImpl(MethodImplOptions.AggressiveInlining)>
        'Public Overloads Shared Operator +(list As List(Of T), index As Index(Of T)) As List(Of T)
        '    Call list.AddRange(index.Objects)
        '    Return list
        'End Operator

        ''' <summary>
        ''' Append <paramref name="list2"/> to the end of <paramref name="list1"/>
        ''' </summary>
        ''' <param name="list1"></param>
        ''' <param name="list2"></param>
        ''' <returns></returns>
        Public Shared Operator +(list1 As List(Of T), list2 As List(Of T)) As List(Of T)
            If Not list2 Is Nothing Then
                list1.AddRange(list2.ToArray)
            End If
            Return list1
        End Operator

        ''' <summary>
        ''' Adds the elements of the specified collection to the end of the <see cref="List(Of T)"/>.
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="vals"></param>
        ''' <returns></returns>
        Public Shared Operator +(list As List(Of T), vals As IEnumerable(Of IEnumerable(Of T))) As List(Of T)
            If Not vals Is Nothing Then
                Call list.AddRange(vals.IteratesALL)
            End If
            Return list
        End Operator

        ''' <summary>
        ''' Adds the elements of the specified collection to the begining of the <paramref name="list"/> <see cref="List(Of T)"/>.
        ''' (output = <paramref name="vals"/> contract <paramref name="list"/>)
        ''' (这个操作符并不会修改所输入的两个原始序列的内容)
        ''' </summary>
        ''' <param name="vals"></param>
        ''' <param name="list"></param>
        ''' <returns></returns>
        Public Shared Operator +(vals As IEnumerable(Of T), list As List(Of T)) As List(Of T)
            Dim all As List(Of T) = vals.AsList
            Call all.AddRange(list)
            Return all
        End Operator

        ' 请注意，由于下面的代码是和Csv文件操作模块有冲突的，所以代码在这里被注释掉了
        'Public Shared Operator +(vals As IEnumerable(Of IEnumerable(Of T)), list As List(Of T)) As List(Of T)
        '    Call list.AddRange(vals.MatrixAsIterator)
        '    Return list
        'End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(vals As T(), list As List(Of T)) As List(Of T)
            Return New List(Of T)(vals) + list.AsEnumerable
        End Operator

        ''' <summary>
        ''' 批量的从目标列表之中移除<paramref name="removes"/>集合之中的对象
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="removes"></param>
        ''' <returns></returns>
        Public Shared Operator -(list As List(Of T), removes As IEnumerable(Of T)) As List(Of T)
            If Not removes Is Nothing Then
                For Each x As T In removes
                    Call list.Remove(x)
                Next
            End If
            Return list
        End Operator

        Public Overloads Shared Operator -(list As List(Of T), all As Func(Of T, Boolean)) As List(Of T)
            Call list.RemoveAll(Function(x) all(x))
            Return list
        End Operator

        ''' <summary>
        ''' <see cref="List(Of T).RemoveAt(Integer)"/>
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Public Shared Operator -(list As List(Of T), index%) As List(Of T)
            Call list.RemoveAt(index)
            Return list
        End Operator

        ''' <summary>
        ''' 从输入的向量数组之中移除掉列表之中的指定元素，然后返回<paramref name="vector"/>的剩余元素
        ''' </summary>
        ''' <param name="vector"></param>
        ''' <param name="list"></param>
        ''' <returns></returns>
        Public Shared Operator -(vector As T(), list As List(Of T)) As List(Of T)
            Return vector.AsList - DirectCast(list, IEnumerable(Of T))
        End Operator

        ''' <summary>
        ''' 将这个列表对象隐式转换为向量数组
        ''' </summary>
        ''' <param name="list"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(list As List(Of T)) As T()
            If list Is Nothing Then
                Return {}
            Else
                Return list.ToArray
            End If
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType([single] As T) As List(Of T)
            Return New List(Of T) From {[single]}
        End Operator

        ' 因为这个隐式会使得数组被默认转换为本List对象，会导致 + 运算符重载失败，所以在这里将这个隐式转换取消掉
        'Public Shared Widening Operator CType(array As T()) As List(Of T)
        '    Return New List(Of T)(array)
        'End Operator

        ''' <summary>
        ''' Find a item in the <see cref="List(Of T)"/>
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="find"></param>
        ''' <returns></returns>
        Public Shared Operator ^(list As List(Of T), find As Func(Of T, Boolean)) As T
            Dim LQuery = LinqAPI.DefaultFirst(Of T) _
                                                    _
                () <= From x As T
                      In list.AsParallel
                      Where True = find(x)
                      Select x

            Return LQuery
        End Operator

        ''' <summary>
        ''' Elements count not equals to a specific number?
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="count%"></param>
        ''' <returns></returns>
        Public Shared Operator <>(list As List(Of T), count%) As Boolean
            If list Is Nothing Then
                Return True
            End If
            Return list.Count <> count
        End Operator

        ''' <summary>
        ''' Elements count is greater than or equals to a specific number?
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="count%"></param>
        ''' <returns></returns>
        Public Shared Operator >=(list As List(Of T), count%) As Boolean
            If list Is Nothing Then
                Return 0 >= count
            Else
                Return list.Count >= count
            End If
        End Operator

        ''' <summary>
        ''' Elements count is less than or equals to a specific number?
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="count%"></param>
        ''' <returns></returns>
        Public Shared Operator <=(list As List(Of T), count%) As Boolean
            If list Is Nothing Then
                Return 0 <= count
            Else
                Return list.Count <= count
            End If
        End Operator

        ''' <summary>
        ''' Assert that the element counts of this list object is equals to a specifc number?
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="count%"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(list As List(Of T), count%) As Boolean
            Return Not (list <> count)
        End Operator

        ''' <summary>
        ''' <see cref="Enumerable.SequenceEqual(Of T)"/>
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="collection"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(list As List(Of T), collection As IEnumerable(Of T)) As Boolean
            Return list.SequenceEqual(collection)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(list As List(Of T), collection As IEnumerable(Of T)) As Boolean
            Return Not list.SequenceEqual(collection)
        End Operator

        ''' <summary>
        ''' <see cref="Count"/> of <paramref name="list"/> &gt; <paramref name="n"/>
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="n%"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator >(list As List(Of T), n%) As Boolean
            Return list.Count > n
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <(list As List(Of T), n%) As Boolean
            Return Not list > n
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator >>(source As List(Of T), path As Integer) As Boolean
            Dim file As FileHandle = My.File.GetHandle(path)
            Return source > file.FileName
        End Operator

        ''' <summary>
        ''' 反向的枚举出当前列表之中的所有元素
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function ReverseIterator() As IEnumerable(Of T)
            For i As Integer = Count - 1 To 0 Step -1
                Yield MyBase.Item(i)
            Next
        End Function

        ''' <summary>
        ''' Enums all of the elements in this collection list object by return a value reference type
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function ValuesEnumerator() As IEnumerable(Of Value(Of T))
            Dim o As New Value(Of T)

            For Each x As T In Me
                o.Value = x
                Yield o
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function [Default]() As [Default](Of List(Of T))
            Return New List(Of T)
        End Function

        ''' <summary>
        ''' Get the <see cref="Last"/> element value and then removes the last element.
        ''' </summary>
        ''' <returns>
        ''' 
        ''' </returns>
        Public Function Pop(Optional strict As Boolean = True) As T
            If Count = 0 AndAlso Not strict Then
                Return Nothing
            Else
                Dim out As T = Last
                Call Me.RemoveLast
                Return out
            End If
        End Function

        Public Function Poll() As T
            Dim out = First
            Call Me.RemoveAt(Scan0)
            Return out
        End Function
    End Class
End Namespace
