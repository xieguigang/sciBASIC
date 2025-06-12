#Region "Microsoft.VisualBasic::099e5a17a356c5c3ac3619bb169086fc, Microsoft.VisualBasic.Core\src\Extensions\Collection\CollectionValueGetter.vb"

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

    '   Total Lines: 404
    '    Code Lines: 222 (54.95%)
    ' Comment Lines: 144 (35.64%)
    '    - Xml Docs: 93.06%
    ' 
    '   Blank Lines: 38 (9.41%)
    '     File Size: 14.76 KB


    ' Module CollectionValueGetter
    ' 
    '     Function: [Get], AsEnumerable, ElementAtOrDefault, ElementAtOrNull, FirstNotEmpty
    '               GetItem, GetValueOrDefault, GetValueOrNull, NotNull, PowerSet
    '               (+3 Overloads) TryGetValue, (+2 Overloads) TryPopOut, Values
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Language
Imports std = System.Math

#If DEBUG Then
Imports Microsoft.VisualBasic.Serialization.JSON
#End If

Public Module CollectionValueGetter

    <Extension>
    Public Iterator Function PowerSet(Of T)([set] As ICollection(Of T)) As IEnumerable(Of T())
        Dim setSize As Integer = [set].Count
        Dim powerSetSize As Integer = std.Pow(2, setSize)

        For i As Integer = 0 To powerSetSize - 1
            Dim subset As New List(Of T)()

            For j As Integer = 0 To setSize - 1
                If (i And (1 << j)) <> 0 Then
                    subset.Add([set](j))
                End If
            Next

            Yield subset.ToArray
        Next
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="value"></param>
    ''' <returns>
    ''' this function returns empty collection if the given <paramref name="value"/> is nothing
    ''' </returns>
    <Extension>
    Public Iterator Function AsEnumerable(Of T)(value As Value(Of T())) As IEnumerable(Of T)
        If value Is Nothing OrElse value.IsNothing Then
            Return
        End If

        For Each item As T In CType(value, T())
            Yield item
        Next
    End Function

    ''' <summary>
    ''' Returns the first not nothing object.
    ''' </summary>
    ''' <typeparam name="T">
    ''' Due to the reason of value type is always not nothing, so that this generic type constrain as Class reference type.
    ''' </typeparam>
    ''' <param name="args"></param>
    ''' <returns></returns>
    Public Function NotNull(Of T As Class)(ParamArray args As T()) As T
        If args.IsNullOrEmpty Then
            Return Nothing
        Else
            For Each x In args
                If Not x Is Nothing Then
                    Return x
                End If
            Next
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Returns the first not null or empty string.
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    Public Function FirstNotEmpty(ParamArray args As String()) As String
        If args.IsNullOrEmpty Then
            Return ""
        Else
            For Each s As String In args
                If Not String.IsNullOrEmpty(s) Then
                    Return s
                End If
            Next
        End If

        Return ""
    End Function

    ''' <summary>
    ''' Safe get the specific index element from the target collection, is the index value invalid, then default value will be return.
    ''' (假若下标越界的话会返回默认值)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="array"></param>
    ''' <param name="index"></param>
    ''' <param name="default">Default value for invalid index is nothing.</param>
    ''' <returns></returns>
    <Extension>
    Public Function [Get](Of T)(array As IEnumerable(Of T), index As Integer, Optional [default] As T = Nothing) As T
        If array Is Nothing Then
            Return [default]
        End If

        If index < 0 OrElse index >= array.Count Then
            Return [default]
        End If

        Dim value As T = array(index)
        Return value
    End Function

    ''' <summary>
    ''' This is a safely method for gets the value in a array, if the index was 
    ''' outside of the boundary or the given <paramref name="array"/> is nothing, 
    ''' then the default value will be return.
    ''' </summary>
    ''' <typeparam name="T">The generic type of current array object</typeparam>
    ''' <param name="array"></param>
    ''' <param name="index">A 32-bit integer that represents the position of the System.Array element to
    ''' get.</param>
    ''' <param name="default">
    ''' Default value for return when the array object is nothing or index outside of the boundary.
    ''' </param>
    ''' <returns>
    ''' The value at the specified position in the one-dimensional System.Array.
    ''' </returns>
    ''' <remarks>
    ''' 假若目标数组是空值或者下标越界的话会返回<paramref name="default"/>默认值
    ''' </remarks> 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ElementAtOrDefault(Of T)(array As T(), index As Integer, Optional [default] As T = Nothing) As T
        If array Is Nothing Then
            Return [default]
        End If
        Return array.GetValueOrDefault(index, [default])
    End Function

    ''' <summary>
    ''' Gets the value at the specified position in the one-dimensional System.Array.
    ''' The index is specified as a 32-bit integer.
    ''' </summary>
    ''' <param name="array"></param>
    ''' <param name="index">A 32-bit integer that represents the position of the System.Array element to
    ''' get.</param>
    ''' <returns>
    ''' The value at the specified position in the one-dimensional System.Array.
    ''' </returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ElementAtOrNull(Of T)(array As T(), index As Integer) As T
        Return array.GetValueOrDefault(index, [default]:=Nothing)
    End Function

    ''' <summary>
    ''' Gets the value at the specified position in the one-dimensional System.Array.
    ''' The index is specified as a 32-bit integer.
    ''' </summary>
    ''' <param name="array"></param>
    ''' <param name="index">A 32-bit integer that represents the position of the System.Array element to
    ''' get.</param>
    ''' <param name="default">
    ''' The default value if index is outside the range of valid indexes for the current System.Array.
    ''' </param>
    ''' <returns>
    ''' The value at the specified position in the one-dimensional System.Array.
    ''' </returns>
    <Extension>
    Public Function GetValueOrDefault(array As Array, index As Integer, Optional [default] As Object = Nothing) As Object
        If array.IsNullOrEmpty Then
            Return [default]
        ElseIf index < 0 Then
            index = array.Length + index
        End If

        ' 负数值直接返回默认值
        If index < 0 OrElse index >= array.Length Then
            Return [default]
        End If

        Dim value As Object = array.GetValue(index)
        Return value
    End Function

    ''' <summary>
    ''' 这个是一个安全的方法，假若下标越界或者目标数据源为空的话，则会返回空值
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="index"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetItem(Of T)(source As IEnumerable(Of T), index As Integer) As T
        If source Is Nothing Then
            Return Nothing
        Else
            Return source.ElementAtOrDefault(index)
        End If
    End Function

    <Extension>
    Public Function GetValueOrNull(Of K, V)(table As IDictionary(Of K, V), key As K) As V
        Dim refOut As V = Nothing
        Call table.TryGetValue(key, value:=refOut)
        Return refOut
    End Function

    ''' <summary>
    ''' get value by key and then removes the target 
    ''' keyed value from the given <paramref name="table"/>.
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="table"></param>
    ''' <param name="key"></param>
    ''' <param name="default"></param>
    ''' <returns>
    ''' the value that associated with the given <paramref name="key"/>,
    ''' which is going to be removed from the specific dictionary 
    ''' <paramref name="table"/> object.
    ''' </returns>
    <Extension>
    Public Function TryPopOut(Of TKey, TValue)(table As Dictionary(Of TKey, TValue),
                                               key As TKey,
                                               Optional [default] As TValue = Nothing) As TValue
        If table Is Nothing Then
            Return [default]
        ElseIf Not table.ContainsKey(key) Then
            Return [default]
        Else
            Dim value As TValue = table(key)
            table.Remove(key)
            Return value
        End If
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="table"></param>
    ''' <param name="synonyms"></param>
    ''' <param name="default"></param>
    ''' <returns>the default value will be returns if all synonym key is missing
    ''' from the given table object.</returns>
    <Extension>
    Public Function TryPopOut(Of TKey, TValue)(table As Dictionary(Of TKey, TValue),
                                               synonyms As IEnumerable(Of TKey),
                                               Optional [default] As TValue = Nothing) As TValue
        If table Is Nothing Then
            Return [default]
        End If

        For Each key As TKey In synonyms
            If table.ContainsKey(key) Then
                Dim val As TValue = table(key)
                table.Remove(key)
                Return val
            End If
        Next

        Return [default]
    End Function

    ''' <summary>
    ''' 假若不存在目标键名，则返回空值，默认值为空值
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="table"></param>
    ''' <param name="keys">
    ''' the synonym keys
    ''' </param>
    ''' <param name="default"></param>
    ''' <returns>returns the key-value which is matched with any input synonym <paramref name="keys"/>.</returns>
    <Extension>
    Public Function TryGetValue(Of TKey, TValue)(table As Dictionary(Of TKey, TValue),
                                                 keys As TKey(),
                                                 Optional [default] As TValue = Nothing,
                                                 Optional mute As Boolean = False,
                                                 <CallerMemberName>
                                                 Optional trace$ = Nothing) As TValue

        ' 表示空的，或者键名是空的，都意味着键名不存在与表之中
        ' 直接返回默认值
        If table Is Nothing Then
#If DEBUG Then
            Call PrintException("Hash_table is nothing!")
#End If
            Return [default]
        ElseIf keys.IsNullOrEmpty Then
#If DEBUG Then
            Call PrintException("Index key is nothing!")
#End If
            Return [default]
        Else
            ' scan the synonym keys
            For Each key As TKey In keys
                If table.ContainsKey(key) Then
                    Return table(key)
                End If
            Next

#If DEBUG Then
            If Not mute Then
                Call PrintException($"missing_index:={keys.Select(AddressOf Scripting.ToString).GetJson}!", trace)
            End If
#End If
            Return [default]
        End If
    End Function

    ''' <summary>
    ''' 假若不存在目标键名，则返回空值，默认值为空值
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="table"></param>
    ''' <param name="index">这个函数会自动处理空键名的情况</param>
    ''' <param name="default"></param>
    ''' <param name="mute">
    ''' mute the verbose debug echo in debug mode?
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' this function is a safe function:
    ''' 
    ''' 1. for input hash table object is nothing, this function will returns the default value
    ''' 2. for key is nothing or key is not found inside the hash table, this function also returns the default value
    ''' </remarks>
    <DebuggerStepThrough>
    <Extension>
    Public Function TryGetValue(Of TKey, TValue)(table As IDictionary(Of TKey, TValue),
                                                 index As TKey,
                                                 Optional [default] As TValue = Nothing,
                                                 Optional mute As Boolean = False,
                                                 <CallerMemberName>
                                                 Optional trace$ = Nothing) As TValue
        ' 表示空的，或者键名是空的，都意味着键名不存在与表之中
        ' 直接返回默认值
        If table Is Nothing Then
#If DEBUG Then
            Call PrintException("Hash_table is nothing!")
#End If
            Return [default]
        ElseIf index Is Nothing Then
#If DEBUG Then
            Call PrintException("Index key is nothing!")
#End If
            Return [default]
        ElseIf Not table.ContainsKey(index) Then
#If DEBUG Then
            If Not mute Then
                Call PrintException($"missing_index:={Scripting.ToString(index)}!", trace)
            End If
#End If
            Return [default]
        End If

        Return table(index)
    End Function

    <Extension>
    Public Function TryGetValue(Of TKey, TValue, TProp)(dict As Dictionary(Of TKey, TValue), index As TKey, prop As String) As TProp
        If dict Is Nothing Then
            Return Nothing
        End If

        If Not dict.ContainsKey(index) Then
            Return Nothing
        End If

        Dim obj As TValue = dict(index)
        Dim propertyInfo As PropertyInfo = obj.GetType.GetProperty(prop)

        If propertyInfo Is Nothing Then
            Return Nothing
        End If

        Dim value As Object = propertyInfo.GetValue(obj, Nothing)
        Return DirectCast(value, TProp)
    End Function

    ''' <summary>
    ''' get value set
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="x">a collection of the value wrapper</param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function Values(Of T)(x As IEnumerable(Of Value(Of T).IValueOf)) As IEnumerable(Of T)
        If x Is Nothing Then
            Return
        End If

        For Each item As Value(Of T).IValueOf In x
            Yield item.Value
        Next
    End Function
End Module
