#Region "Microsoft.VisualBasic::27ea6b51ece0f925a2c150f827c298ab, ..\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataStructures\HandleList\HandledList.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Namespace ComponentModel

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T">
    ''' Class object that can be dispose by the system automatically and the class object that should 
    ''' have a handle property to specific its position in this list class. 
    ''' (能够被系统所自动销毁的对象类型，并且该类型的对象必须含有一个Handle属性来指明其在本列表中的位置)
    ''' </typeparam>
    ''' <remarks></remarks>
    Public Class HandledList(Of T As IAddressHandle) : Implements Generic.IEnumerable(Of T)

        ''' <summary>
        ''' Object instances data physical storage position, element may be null after 
        ''' remove a specify object handle. 
        ''' (列表中的元素对象实例的实际存储位置，当对象元素从列表之中被移除了之后，其将会被销毁)
        ''' </summary>
        ''' <remarks>
        ''' 即与只读属性'ListData'相比，这个字段的列表中可能含有空引用的元素对象.
        ''' </remarks>
        Dim _ListData As List(Of T)

        ''' <summary>
        ''' Stack list that store the empty pointer
        ''' </summary>
        ''' <remarks></remarks>
        Dim _EmptyListStack As New Stack(Of Long)

        ''' <summary>
        ''' Exists handle that store in this list
        ''' </summary>
        ''' <remarks></remarks>
        Dim _HandleList As New List(Of Long)

        ''' <summary>
        ''' Get the logical list of the data store in this list object instance.
        ''' (获取逻辑形式的列表数据)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ListData As T()
            Get
                Dim Query As Generic.IEnumerable(Of T) = From Handle As Long
                                                         In _HandleList
                                                         Select _ListData(Handle) 'LINQ query to get the object that exists in the list.
                Return Query.ToArray
            End Get
        End Property

        ''' <summary>
        ''' Get the logical list length 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Count As Long
            Get
                Return _HandleList.LongCount
            End Get
        End Property

        ''' <summary>
        ''' Get or set a object instance data that has specify handle value
        ''' </summary>
        ''' <param name="Handle">Target object handle value</param>
        ''' <value>It is not recommend that you use this property to set the data element as the list object's capacity may not cover your handle</value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Default Public Property Item(Handle As Long) As T
            Get
                Return _ListData(Handle)
            End Get
            Set(value As T)
                _ListData(Handle) = value
            End Set
        End Property

        ''' <summary>
        ''' Construct a new list object
        ''' </summary>
        ''' <param name="Capacity">The initialize size of this list object, Optional parameter, default value is 2048</param>
        ''' <remarks></remarks>
        Sub New(Optional Capacity As Integer = 2048)
            _ListData = New List(Of T)(capacity:=Capacity)
        End Sub

        ''' <summary>
        ''' Add a disposable object instance element into this list object and return its object handle value in this list object    
        ''' </summary>
        ''' <param name="e">Object instance that will be store in this list object</param>
        ''' <returns>Object handle in this list object instance</returns>
        ''' <remarks></remarks>
        Public Function Append(ByRef e As T) As Long
            Dim Handle As Long

            If _EmptyListStack.Count = 0 Then
                Call _ListData.Add(e)
                Handle = _ListData.Count - 1
            Else
                Handle = _EmptyListStack.Pop
                _ListData(Handle) = e
            End If

            e.Address = Handle
            Call _HandleList.Add(Handle)

            Return Handle
        End Function

        ''' <summary>
        ''' Append a list of object instance
        ''' </summary>
        ''' <param name="list"></param>
        ''' <remarks></remarks>
        Public Sub AppendRange(ByRef List As Generic.IEnumerable(Of T))
            For i As Integer = 0 To List.Count - 1
                Call Append(List(index:=i))
            Next
        End Sub

        ''' <summary>
        ''' Remove a object instance element in this list object that have a specify handler
        ''' </summary>
        ''' <param name="Handle">Object handle value that specify the target object</param>
        ''' <remarks></remarks>
        Public Sub RemoveAt(Handle As Long)
            Call _ListData(Handle).Dispose()
            Call _EmptyListStack.Push(Handle)
            Call _HandleList.Remove(Handle)
        End Sub

        ''' <summary>
        ''' Know that the specify handle pointe object is null or not? 
        ''' </summary>
        ''' <param name="Handle">Object handle</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Exists(Handle As Long) As Boolean
            Return _HandleList.IndexOf(Handle) <> -1
        End Function

        Public Function Take(startIndex As Long, Optional Count As Long = -1) As T()
            Dim NewList As Generic.IEnumerable(Of T) = From Handle As Long In _HandleList Select _ListData(Handle)

            NewList = NewList.ToArray
            If Count = -1 Then Count = NewList.Count - startIndex

            Return NewList.Skip(startIndex + 1).Take(Count).ToArray
        End Function

        ''' <summary>
        ''' Know that a specify object instance exists in this list object or not? 
        ''' (判断某一个指定的对象实例是否存在于列表对象之中)
        ''' </summary>
        ''' <param name="e">Target object instance(目标要进行查找的对象实例)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Exists(e As T) As Boolean
            Dim LQuery As Generic.IEnumerable(Of Integer) = From Handle As Long In _HandleList Where Handle = e.Address Select 1 'Only one object in the query result if the specific item is exists, when the item 'e' is not exists then the count is ZERO.
            Return LQuery.Count > 0
        End Function

        ''' <summary>
        ''' Remove a specify object in this list object using its hashcode and return its handle value.
        ''' (使用对象的哈希值来查找目标对象并对其进行移除，之后返回其句柄值) 
        ''' </summary>
        ''' <param name="e"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Remove(e As T) As Long
            Dim ListData As Generic.IEnumerable(Of T) = From hdl As Long In _HandleList Select _ListData(hdl)  'Select the object exists in the listdata logically. 
            Dim LINQuery As Generic.IEnumerable(Of T) = From obj As T In ListData Where obj.GetHashCode = e.GetHashCode Select obj 'Query of the equal object finding.
            Dim Handle As Long = LINQuery.First.Address

            Call RemoveAt(Handle:=Handle)
            Return Handle
        End Function

        ''' <summary>
        ''' Clear all of the data in this list object instance.
        ''' (清除本列表对象中的所有数据)
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Flush()
            Call _ListData.Clear()
            Call _EmptyListStack.Clear()
            Call _HandleList.Clear()
        End Sub

        Shared Narrowing Operator CType(e As HandledList(Of T)) As T()
            Return e.ListData
        End Operator

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Dim Array As T() = ListData

            For i As Integer = 0 To Array.Length - 1
                Yield Array(i)
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
