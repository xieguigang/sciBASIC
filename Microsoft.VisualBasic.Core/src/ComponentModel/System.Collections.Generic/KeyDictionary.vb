#Region "Microsoft.VisualBasic::4ffc81a523eea8551656486a0f0747d2, Microsoft.VisualBasic.Core\src\ComponentModel\System.Collections.Generic\KeyDictionary.vb"

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

    '   Total Lines: 358
    '    Code Lines: 207
    ' Comment Lines: 107
    '   Blank Lines: 44
    '     File Size: 13.37 KB


    '     Class HashTable
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class Dictionary
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: Find, GetValueList, Have, Remove, SafeGetValue
    '                   (+2 Overloads) TryGetValue
    ' 
    '         Sub: Add, AddRange, InsertOrUpdate
    ' 
    '         Operators: (+2 Overloads) -, ^, +, <=, >=
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Collection

    Public Class HashTable(Of T) : Inherits Dictionary(Of String, T)

        ReadOnly assert As Predicate(Of Object)

        Default Public Overloads Property Item(key As String) As [Default](Of T)
            Get
                Dim value As T = If(ContainsKey(key), MyBase.Item(key), Nothing)
                Return New [Default](Of T)(value, assert)
            End Get
            Set(value As [Default](Of T))
                MyBase.Item(key) = value.DefaultValue
            End Set
        End Property

        Sub New(copy As Dictionary(Of String, T), Optional assert As Predicate(Of Object) = Nothing)
            Call MyBase.New(copy)

            Me.assert = assert
        End Sub
    End Class

    ''' <summary>
    ''' Represents a collection of keys and values.To browse the .NET Framework source
    ''' code for this type, see the Reference Source.
    ''' </summary>
    ''' <typeparam name="V"></typeparam>
    Public Class Dictionary(Of V As INamedValue) : Inherits SortedDictionary(Of String, V)
        ' Implements IEnumerable(Of V)

        Default Public Overloads Property Item(o As V) As V
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return MyBase.Item(o.Key)
            End Get
            Set(value As V)
                MyBase.Item(o.Key) = value
            End Set
        End Property

        ''' <summary>
        ''' 不存在的键名或者空值的键名都会返回``Nothing``
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(key As String) As V
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If Not key Is Nothing AndAlso ContainsKey(key) Then
                    Return MyBase.Item(key)
                Else
                    Return Nothing
                End If
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As V)
                MyBase.Item(key) = value
            End Set
        End Property

        ''' <summary>
        ''' The <paramref name="keys"/> element counts should equals to the value length when invoke property set.
        ''' </summary>
        ''' <param name="keys"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(keys As IEnumerable(Of String)) As V()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return keys _
                    .Select(Function(key) MyBase.Item(key)) _
                    .ToArray
            End Get
            Set(value As V())
                For Each key As SeqValue(Of String) In keys.SeqIterator
                    MyBase.Item(key.value) = value(key)
                Next
            End Set
        End Property

        ''' <summary>
        ''' create a new empty <see cref="SortedDictionary(Of String, V)"/> 
        ''' </summary>
        <DebuggerStepThrough>
        Sub New()
            Call MyBase.New
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the System.Collections.Generic.SortedDictionary`2
        ''' class that contains elements copied from the specified System.Collections.Generic.IDictionary`2
        ''' and uses the default System.Collections.Generic.IComparer`1 implementation for
        ''' the key type.
        ''' </summary>
        ''' <param name="source">
        ''' The System.Collections.Generic.IDictionary`2 whose elements are copied to the
        ''' new System.Collections.Generic.SortedDictionary`2.
        ''' </param>
        ''' 
        <DebuggerStepThrough>
        Sub New(source As Dictionary(Of String, V))
            Call MyBase.New(source)
        End Sub

        <DebuggerStepThrough>
        Sub New(source As IEnumerable(Of V), Optional overridesDuplicateds As Boolean = False)
            Call Me.New

            If overridesDuplicateds Then
                For Each x As V In source
                    Me(x.Key) = x
                Next
            Else
                For Each x As V In source
                    Call Add(x)
                Next
            End If
        End Sub

        <DebuggerStepThrough>
        Public Function GetValueList() As List(Of V)
            Return Values.AsList
        End Function

        ''' <summary>
        ''' Adds an element with the specified key and value into the System.Collections.Generic.SortedDictionary`2.
        ''' </summary>
        ''' <param name="item"></param>
        ''' 
        <DebuggerStepThrough>
        Public Overloads Sub Add(item As V)
            Call MyBase.Add(item.Key, item)
        End Sub

        <DebuggerStepThrough>
        Public Sub AddRange(source As IEnumerable(Of V))
            For Each x As V In source
                Call MyBase.Add(x.Key, x)
            Next
        End Sub

        Public Sub InsertOrUpdate(x As V)
            If Me.ContainsKey(x.Key) Then
                Me(x.Key) = x
            Else
                Call MyBase.Add(x.Key, x)
            End If
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="name">不区分大小写的</param>
        ''' <returns></returns>
        Public Function Find(name As String) As V
            If MyBase.ContainsKey(name) Then
                Return Me(name)
            Else
                Dim key As New Value(Of String)

                If Me.ContainsKey(key = name.ToLower) Then
                    Return Me(name)
                ElseIf Me.ContainsKey(key = name.ToUpper) Then
                    Return Me(name)
                Else
                    Return Nothing
                End If
            End If
        End Function

        ''' <summary>
        ''' Inline method alias of function <see cref="ContainsKey(String)"/> in parent class
        ''' </summary>
        ''' <param name="item"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function Have(item As V) As Boolean
            Return MyBase.ContainsKey(item.Key)
        End Function

        ''' <summary>
        ''' If the value is not found in the hash directionary, then the default value will be returns, and the default value is nothing.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="[default]"></param>
        ''' <param name="success">可能value本身就是空值，所以在这里使用这个参数来判断是否存在</param>
        ''' <returns></returns>
        Public Function SafeGetValue(name As String,
                                     Optional ByRef [default] As V = Nothing,
                                     Optional ByRef success As Boolean = False) As V
            Dim x As V = Nothing

            success = MyBase.TryGetValue(name, x)

            If success Then
                Return x
            Else
                Return [default]
            End If
        End Function

        ''' <summary>
        ''' Gets the value associated with the specified key.(假若找不到键值，就会返回Nothing)
        ''' </summary>
        ''' <param name="name">The key of the value to get.</param>
        ''' <param name="success">true if the System.Collections.Generic.SortedDictionary`2 contains an element
        ''' with the specified key; otherwise, false.</param>
        ''' <returns>When this method returns, the value associated with the specified key, if the
        ''' key is found; otherwise, the default value for the type of the value parameter.</returns>
        Public Overloads Function TryGetValue(name As String, Optional ByRef success As Boolean = True) As V
            Dim value As V = Nothing
            success = MyBase.TryGetValue(name, value)
            Return value
        End Function

        Public Overloads Function TryGetValue(name$, [default] As V) As V
            If MyBase.ContainsKey(name) Then
                Return MyBase.Item(name)
            Else
                Return [default]
            End If
        End Function

        ''' <summary>
        ''' 假若目标元素不存在于本字典之中，则会返回False
        ''' </summary>
        ''' <param name="vec"></param>
        ''' <returns></returns>
        Public Overloads Function Remove(vec As V) As Boolean
            If Me.ContainsKey(vec.Key) Then
                Return Me.Remove(vec.Key)
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Adds an element with the specified key and value into the System.Collections.Generic.SortedDictionary`2.
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="item"></param>
        ''' <returns></returns>
        Public Shared Operator +(list As Dictionary(Of V), item As V) As Dictionary(Of V)
            If Not list.ContainsKey(item.Key) Then
                list.Add(item)
            Else
                Throw New DuplicateNameException(item.Key)
            End If

            Return list
        End Operator

        ''' <summary>
        ''' Find a variable in the hash table
        ''' </summary>
        ''' <param name="table"></param>
        ''' <param name="uid"></param>
        ''' <returns></returns>
        Public Shared Operator ^(table As Dictionary(Of V), uid As String) As V
            If table.ContainsKey(uid) Then
                Return table(uid)
            Else
                Return Nothing
            End If
        End Operator

        Public Shared Operator -(hash As Dictionary(Of V), id As String) As Dictionary(Of V)
            Call hash.Remove(id)
            Return hash
        End Operator

        ''' <summary>
        ''' 批量移除字典之中的键值对
        ''' </summary>
        ''' <param name="hash"></param>
        ''' <param name="keys">需要移除的键名的列表</param>
        ''' <returns></returns>
        Public Shared Operator -(hash As Dictionary(Of V), keys As IEnumerable(Of String)) As Dictionary(Of V)
            For Each k As String In keys
                Call hash.Remove(k)
            Next

            Return hash
        End Operator

        Public Shared Widening Operator CType(source As System.Collections.Generic.List(Of V)) As Dictionary(Of V)
            Return source.ToDictionary
        End Operator

        Public Shared Widening Operator CType(table As Dictionary(Of String, V)) As Dictionary(Of V)
            Return New Dictionary(Of V)(table)
        End Operator

        Public Shared Widening Operator CType(source As V()) As Dictionary(Of V)
            Return source.ToDictionary
        End Operator

        Public Shared Narrowing Operator CType(source As Dictionary(Of V)) As List(Of V)
            Return New List(Of V)(source.Values)
        End Operator

        Public Shared Narrowing Operator CType(table As Dictionary(Of V)) As Dictionary(Of String, V)
            Return New Dictionary(Of String, V)(table)
        End Operator

        ''' <summary>
        ''' Get value by key.
        ''' </summary>
        ''' <param name="hash"></param>
        ''' <param name="key"></param>
        ''' <returns></returns>
        Public Shared Operator <=(hash As Dictionary(Of V), key As String) As V
            Return hash(key)
        End Operator

        Public Shared Operator >=(hash As Dictionary(Of V), null As String) As V
            Throw New NotSupportedException
        End Operator

        ''' <summary>
        ''' <see cref="ContainsKey(String)"/>
        ''' </summary>
        ''' <param name="hash"></param>
        ''' <param name="null"></param>
        ''' <returns></returns>
        Public Shared Operator &(hash As Dictionary(Of V), null As String) As Boolean
            Return hash.ContainsKey(null)
        End Operator

        ''' <summary>
        ''' <see cref="ContainsKey(String)"/>
        ''' </summary>
        ''' <param name="table"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Operator &(table As Dictionary(Of V), x As V) As Boolean
            Return table & x.Key
        End Operator

        Public Shared Narrowing Operator CType(map As Dictionary(Of V)) As V()
            Return map.Values.ToArray
        End Operator

        ' 实现这个集合接口会和字典的集合接口出现冲突
        'Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator(Of V) Implements IEnumerable(Of V).GetEnumerator
        '    For Each x In MyBase.Values
        '        Yield x
        '    Next
        'End Function
    End Class
End Namespace
