#Region "Microsoft.VisualBasic::fd7a08a46454d5221e358b5b8c20c1fb, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\System.Collections.Generic\Dictionary(Of T, V).vb"

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

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Collection

    ''' <summary>
    ''' Represents a collection of keys and values.To browse the .NET Framework source
    ''' code for this type, see the Reference Source.
    ''' </summary>
    ''' <typeparam name="V"></typeparam>
    Public Class Dictionary(Of V As INamedValue) : Inherits SortedDictionary(Of String, V)
        ' Implements IEnumerable(Of V)

        Default Public Overloads Property Item(o As V) As V
            Get
                Return MyBase.Item(o.Key)
            End Get
            Set(value As V)
                MyBase.Item(o.Key) = value
            End Set
        End Property

        ''' <summary>
        ''' The <paramref name="keys"/> element counts should equals to the value length when invoke property set.
        ''' </summary>
        ''' <param name="keys"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(keys As IEnumerable(Of String)) As V()
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
        Sub New(source As Dictionary(Of String, V))
            Call MyBase.New(source)
        End Sub

        Sub New(source As IEnumerable(Of V))
            Call Me.New

            For Each x As V In source
                Call Add(x)
            Next
        End Sub

        Public Function GetValueList() As List(Of V)
            Return Values.AsList
        End Function

        ''' <summary>
        ''' Adds an element with the specified key and value into the System.Collections.Generic.SortedDictionary`2.
        ''' </summary>
        ''' <param name="item"></param>
        Public Overloads Sub Add(item As V)
            Call MyBase.Add(item.Key, item)
        End Sub

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
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Overloads Function Remove(x As V) As Boolean
            If Me.ContainsKey(x.Key) Then
                Return Me.Remove(x.Key)
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Adds an element with the specified key and value into the System.Collections.Generic.SortedDictionary`2.
        ''' </summary>
        ''' <param name="hash"></param>
        ''' <param name="item"></param>
        ''' <returns></returns>
        Public Shared Operator +(hash As Dictionary(Of V), item As V) As Dictionary(Of V)
            Call hash.Add(item)
            Return hash
        End Operator

        ''' <summary>
        ''' Find a variable in the hash table
        ''' </summary>
        ''' <param name="hash"></param>
        ''' <param name="uid"></param>
        ''' <returns></returns>
        Public Shared Operator ^(hash As Dictionary(Of V), uid As String) As V
            If hash.ContainsKey(uid) Then
                Return hash(uid)
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