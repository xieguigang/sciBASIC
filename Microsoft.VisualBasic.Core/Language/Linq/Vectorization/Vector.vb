#Region "Microsoft.VisualBasic::61feeb113201015edb940e96d5da63e3, Microsoft.VisualBasic.Core\Language\Linq\Vectorization\Vector.vb"

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

    '     Class Vector
    ' 
    '         Properties: Array, First, IsSingle, Last, Length
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetEnumerator, IEnumerable_GetEnumerator, Subset, ToString, Which
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Dynamic
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports CollectionSet = Microsoft.VisualBasic.ComponentModel.DataStructures.Set

Namespace Language.Vectorization

    ''' <summary>
    ''' VB.NET object collection
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Vector(Of T) : Inherits DynamicObject
        Implements IEnumerable(Of T)

        ''' <summary>
        ''' Array that hold the .NET object in this collection
        ''' </summary>
        Protected buffer As T()

        ''' <summary>
        ''' Gets the element counts in this vector collection
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property Length As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer.Length
            End Get
        End Property

        Public ReadOnly Property IsSingle As Boolean
            Get
                Return Length = 1
            End Get
        End Property

        ''' <summary>
        ''' 请注意，这个属性是直接返回内部数组的引用，所以对这个属性的数组内的元素的修改将会直接修改这个向量的值
        ''' 如果不希望将内部引用进行修改，请使用迭代器或者<see cref="Enumerable.ToArray"/> Linq拓展
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property Array As T()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer
            End Get
        End Property

#Region ""
        ''' <summary>
        ''' The last elements in the collection <see cref="List(Of T)"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property Last As T
            Get
                If Length = 0 Then
                    Return Nothing
                Else
                    Return buffer(Length - 1)
                End If
            End Get
            Set(value As T)
                If Length = 0 Then
                    Throw New IndexOutOfRangeException
                Else
                    buffer(Length - 1) = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' The first elements in the collection <see cref="List(Of T)"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property First As T
            Get
                If Length = 0 Then
                    Return Nothing
                Else
                    Return buffer(0)
                End If
            End Get
            Set(value As T)
                If Length = 0 Then
                    Throw New IndexOutOfRangeException
                Else
                    buffer(Scan0) = value
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
                Return buffer(address.Address)
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As T)
                buffer(address.Address) = value
            End Set
        End Property

#Region "2017-7-22 -1索引好像对向量的意义不大，而且会降低代码性能，所以在这里去除了这个索引属性"

        '''' <summary>
        '''' Can accept negative number as the index value, negative value means ``<see cref="Count"/> - n``, 
        '''' example as ``list(-1)``: means the last element in this list: ``list(list.Count -1)``
        '''' </summary>
        '''' <param name="index%"></param>
        '''' <returns></returns>
        'Default Public Overloads Property Item(index%) As T
        '    Get
        '        If index < 0 Then
        '            index = Count + index  ' -1 -> count -1
        '        End If
        '        Return buffer(index)
        '    End Get
        '    Set(value As T)
        '        If index < 0 Then
        '            index = Count + index  ' -1 -> count -1
        '        End If

        '        buffer(index) = value
        '    End Set
        'End Property

        ''' <summary>
        ''' Direct get the element in the array by its index.
        ''' </summary>
        ''' <param name="index%"></param>
        ''' <returns></returns>
        Default Public Overridable Overloads Property Item(index%) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            <DebuggerStepThrough>
            Get
                Return buffer(index)
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            <DebuggerStepThrough>
            Set(value As T)
                buffer(index) = value
            End Set
        End Property
#End Region

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
                    list += buffer(i)
                Next

                Return list
            End Get
            Set(value As List(Of T))
                Dim index%() = exp.TranslateIndex

                For Each i As SeqValue(Of Integer) In index.SeqIterator
                    buffer(i.value) = value(i.i)
                Next
            End Set
        End Property

        ''' <summary>
        ''' Get subset of the collection by using a continues index
        ''' </summary>
        ''' <param name="range"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(range As IntRange) As List(Of T)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New List(Of T)(Me.Skip(range.Min).Take(range.Length))
            End Get
            Set(value As List(Of T))
                Dim indices As Integer() = range.ToArray

                For i As Integer = 0 To indices.Length - 1
                    buffer(indices(i)) = value(i)
                Next
            End Set
        End Property

#If NET_48 Then
        Default Public Overloads Property Item(range As (start%, ends%)) As List(Of T)
            Get
                Return New List(Of T)(Me.Skip(range.start).Take(count:=range.ends - range.start))
            End Get
            Set(value As List(Of T))
                Me(New IntRange(range.start, range.ends)) = value
            End Set
        End Property
#End If

        ''' <summary>
        ''' Gets subset of the collection by using a discontinues index
        ''' </summary>
        ''' <param name="indices"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(indices As IEnumerable(Of Integer)) As List(Of T)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New List(Of T)(indices.Select(Function(i) buffer(i)))
            End Get
            Set(value As List(Of T))
                For Each i As SeqValue(Of Integer) In indices.SeqIterator
                    buffer(+i) = value(i.i)
                Next
            End Set
        End Property

        ''' <summary>
        ''' 从当前的向量序列之中进行向量子集的截取
        ''' </summary>
        ''' <param name="booleans"></param>
        ''' <returns></returns>
        Public Iterator Function Subset(booleans As IEnumerable(Of Boolean)) As IEnumerable(Of T)
            For Each index In booleans.SeqIterator
                If index.value = True Then
                    Yield buffer(index.i)
                End If
            Next
        End Function

        ''' <summary>
        ''' Select all of the elements from this list collection is any of them match the condition expression: <paramref name="where"/>
        ''' </summary>
        ''' <param name="[where]"></param>
        ''' <returns></returns>
        Default Public Overloads ReadOnly Property Item([where] As Predicate(Of T)) As T()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return buffer.Where(Function(o) where(o)).ToArray
            End Get
        End Property

        ''' <summary>
        ''' Select elements by logical condiction result.
        ''' </summary>
        ''' <param name="booleans"></param>
        ''' <returns></returns>
        Default Public Overridable Overloads Property Item(booleans As IEnumerable(Of Boolean)) As Vector(Of T)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New Vector(Of T)(Me(indices:=Linq.Which(booleans)))
            End Get
            Set(value As Vector(Of T))
                For Each i In booleans.SeqIterator
                    If i.value Then
                        buffer(i) = value(i)
                    End If
                Next
            End Set
        End Property
#End Region

#Region "Constructor"

        <DebuggerStepThrough>
        Public Sub New()
        End Sub

        <DebuggerStepThrough>
        Sub New(capacity%)
            buffer = New T(capacity - 1) {}
        End Sub

        ''' <summary>
        ''' 构建一个新的向量对象，这个向量对象只提供基本的数据存储和访问模型，并没有提供高级的动态处理和模式解析的操作
        ''' </summary>
        ''' <param name="data"></param>
        ''' 
        <DebuggerStepThrough>
        Sub New(data As IEnumerable(Of T))
            buffer = data.ToArray
        End Sub
#End Region

        <DebuggerStepThrough>
        Public Overrides Function ToString() As String
            Return $"{buffer.Length} @ {GetType(T).FullName}"
        End Function

        <DebuggerStepThrough>
        Public Overridable Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each element As T In buffer
                Yield element
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Which(assert As Func(Of T, Boolean)) As Integer()
            Return Linq.Which(Me.Select(assert))
        End Function

        ''' <summary>
        ''' 没用？？？
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Public Overloads Shared Narrowing Operator CType(v As Vector(Of T)) As T()
            Return v.buffer.ToArray
        End Operator

        Public Overloads Shared Narrowing Operator CType(v As Vector(Of T)) As List(Of T)
            Return v.buffer.AsList
        End Operator

        ''' <summary>
        ''' Append the elements in vector <paramref name="a"/> with all of the elements in vector <paramref name="b"/> directly.
        ''' Union two collection directly without <see cref="Enumerable.Distinct"/> operation.
        ''' (请注意，使用<see cref="CollectionSet"/>集合对象的Union功能会去除重复，而这个操作符则是直接进行合并取``并集``而不去重)
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator &(a As Vector(Of T), b As Vector(Of T)) As Vector(Of T)
            Return New Vector(Of T)(a.buffer.AsList + b.buffer)
        End Operator
    End Class
End Namespace
