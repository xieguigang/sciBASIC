#Region "Microsoft.VisualBasic::b54c692043e62979349116607e5f7192, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\BitMap\HashList.vb"

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

    '   Total Lines: 200
    '    Code Lines: 134 (67.00%)
    ' Comment Lines: 36 (18.00%)
    '    - Xml Docs: 94.44%
    ' 
    '   Blank Lines: 30 (15.00%)
    '     File Size: 7.61 KB


    '     Class HashList
    ' 
    '         Properties: Count, EmptySlots
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) Contains, GetAvailablePos, GetEnumerator, GetEnumerator1
    ' 
    '         Sub: (+2 Overloads) Add, Clear, (+2 Overloads) Remove
    ' 
    '         Operators: *, +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel

    ''' <summary>
    ''' 指针位置应该是只读的，因为在这个列表之中，元素的读取时直接和位置以及指针值相关的
    ''' </summary>
    ''' <typeparam name="T">
    ''' Class object that can be dispose by the system automatically and the class object that should 
    ''' have a handle property to specific its position in this list class. 
    ''' (能够被系统所自动销毁的对象类型，并且该类型的对象必须含有一个Handle属性来指明其在本列表中的位置)
    ''' </typeparam>
    ''' <remarks>
    ''' 创建这个列表类型的初衷是能够将数据对象和其所在的位置绑定在一起：
    ''' 
    ''' 当目标对象添加进入这个列表之后，列表会自动寻找空余位置，然后将新的元素添加进入空余位置，之后将位置索引值写入对象
    ''' 所以在这个列表进行添加方法之后，元素可能不是按照顺序排列的
    ''' </remarks>
    Public Class HashList(Of T As IAddressOf) : Implements IEnumerable(Of T)

        ''' <summary>
        ''' Object instances data physical storage position, element may be null after 
        ''' remove a specify object handle. 
        ''' (列表中的元素对象实例的实际存储位置，当对象元素从列表之中被移除了之后，其将会被销毁)
        ''' </summary>
        ''' <remarks>
        ''' 即与只读属性'ListData'相比，这个字段的列表中可能含有空引用的元素对象.
        ''' </remarks>
        Dim list As New List(Of T)
        Dim isNothing As Predicate(Of T) = Function(x) x Is Nothing

        ''' <summary>
        ''' 返回所有不为空的元素的数量，因为本列表的存储特性的关系，为空的位置实际上是没有值的，所以不会返回这些为空的值的统计数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Count As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return list.Where(Function(x) Not isNothing(x)).Count
            End Get
        End Property

        Public ReadOnly Property EmptySlots As Integer()
            Get
                Return list _
                    .SeqIterator _
                    .Where(Function(x) isNothing(x.value)) _
                    .Select(Function(x) x.i) _
                    .ToArray
            End Get
        End Property

        Public Function GetAvailablePos() As Integer
            For i As Integer = 0 To list.Count - 1
                If isNothing(list(i)) Then
                    Return i
                End If
            Next

            If list.Count = 0 OrElse isNothing(list(0)) Then
                Return 0
            Else
                ' append to last
                Return list.Count
            End If
        End Function

        ''' <summary>
        ''' 与迭代器<see cref="GetEnumerator()"/>函数所不同的是，迭代器函数返回的都是非空元素，而这个读写属性则是可以直接接触到内部的
        ''' </summary>
        ''' <param name="index%"></param>
        ''' <returns></returns>
        Default Public Property Item(index%) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If index > list.Count - 1 Then
                    Return Nothing
                End If

                If index < 0 Then
                    index = list.Count + index
                End If

                Return list(index)
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As T)
                If index < 0 Then
                    index = list.Count + index
                End If

                list(index) = value
            End Set
        End Property

        Sub New(Optional isNothing As Predicate(Of T) = Nothing)
            If Not isNothing Is Nothing Then
                Me.isNothing = isNothing
            End If
        End Sub

        Sub New(capacity%, Optional isNothing As Predicate(Of T) = Nothing)
            Call Me.New(isNothing)

            For i As Integer = 0 To capacity - 1
                Call list.Add(Nothing)
            Next
        End Sub

        Sub New(source As IEnumerable(Of T), Optional isNothing As Predicate(Of T) = Nothing)
            Call Me.New(isNothing)
            Call Add(source)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Contains(x As T) As Boolean
            Return Not Me(x.Address) Is Nothing
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Contains(index As Integer) As Boolean
            Return Not Me(index) Is Nothing
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Clear()
            Call list.Clear()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Remove(x As T)
            list(x.Address) = Nothing
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Remove(index%)
            list(index) = Nothing
        End Sub

        Public Sub Add(source As IEnumerable(Of T))
            For Each x As T In source.SafeQuery
                Call Add(x)
            Next
        End Sub

        ''' <summary>
        ''' 将要被添加的元素对象<paramref name="x"/>在这个列表之中的位置应该是提前设置好了的
        ''' </summary>
        ''' <param name="x"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(x As T)
            If list.Count <= x.Address Then
                For i As Integer = 0 To x.Address - list.Count
                    list.Add(Nothing)
                Next
            End If

            list(x.Address) = x
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(src As HashList(Of T)) As T()
            Return src.ToArray
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(array As T()) As HashList(Of T)
            Return New HashList(Of T)(array)
        End Operator

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each x As T In list.Where(Function(o) Not isNothing(o))
                Yield x
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Shared Operator +(list As HashList(Of T), element As T) As HashList(Of T)
            Call list.Add(element)
            Return list
        End Operator

        Public Shared Operator *(list As HashList(Of T), n%) As HashList(Of T)
            If n = 0 Then
                Call list.Clear()
            ElseIf n < 0 Then
                Throw New NotImplementedException
            Else
                Throw New NotImplementedException
            End If

            Return list
        End Operator
    End Class
End Namespace
