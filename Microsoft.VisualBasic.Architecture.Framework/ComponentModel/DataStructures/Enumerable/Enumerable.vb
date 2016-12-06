#Region "Microsoft.VisualBasic::e005112815fae997297fa4f1b3bcab75, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataStructures\Enumerable\Enumerable.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.KeyValuePair

<Extension>
Public Module IEnumerations

    <Extension> Public Function Differ(Of T As INamedValue,
                                          T2)(
                                     source As IEnumerable(Of T),
                                     ToDiffer As IEnumerable(Of T2),
                                     getId As Func(Of T2, String)) As String()

        Dim TargetIndex As String() = (From item As T In source Select item.Key).ToArray
        Dim LQuery = (From item As T2 In ToDiffer
                      Let strId As String = getId(item)
                      Where Array.IndexOf(TargetIndex, strId) = -1
                      Select strId).ToArray
        Return LQuery
    End Function

    <Extension> Public Function Differ(Of T As INamedValue, T2 As INamedValue)(source As IEnumerable(Of T), ToDiffer As IEnumerable(Of T2)) As String()
        Dim TargetIndex As String() = (From item In source Select item.Key).ToArray
        Dim LQuery = (From item As T2 In ToDiffer
                      Where Array.IndexOf(TargetIndex, item.Key) = -1
                      Select item.Key).ToArray
        Return LQuery
    End Function

    <Extension>
    Public Function GetItem(Of T As INamedValue)(Id As String, source As IEnumerable(Of T)) As T
        Return source.GetItem(Id)
    End Function

    <Extension> Public Function GetItems(Of T As INamedValue)(source As IEnumerable(Of T), Id As String) As T()
        Dim LQuery = (From ItemObj As T In source Where String.Equals(Id, ItemObj.Key) Select ItemObj).ToArray
        Return LQuery
    End Function

    ''' <summary>
    ''' 将目标集合对象转换为一个字典对象
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function CreateDictionary(Of T As INamedValue)(Collection As IEnumerable(Of T)) As Dictionary(Of String, T)
        Dim Dictionary As Dictionary(Of String, T) = New Dictionary(Of String, T)
        For Each obj In Collection
            Call Dictionary.Add(obj.Key, obj)
        Next

        Return Dictionary
    End Function

    <Extension> Public Function FindByItemKey(source As IEnumerable(Of KeyValuePair), Key As String, Optional Explicit As Boolean = True) As ComponentModel.KeyValuePair()
        Dim Method = If(Explicit, System.StringComparison.Ordinal, System.StringComparison.OrdinalIgnoreCase)
        Dim LQuery = (From item In source Where String.Equals(item.Key, Key, Method) Select item).ToArray
        Return LQuery
    End Function

    <Extension> Public Function FindByItemKey(Of PairItemType As IKeyValuePair)(source As IEnumerable(Of PairItemType), Key As String, Optional Explicit As Boolean = True) As PairItemType()
        Dim Method = If(Explicit, System.StringComparison.Ordinal, System.StringComparison.OrdinalIgnoreCase)
        Dim LQuery = (From item In source Where String.Equals(item.Identifier, Key, Method) Select item).ToArray
        Return LQuery
    End Function

    <Extension> Public Function FindByItemValue(Of PairItemType As IKeyValuePair)(source As IEnumerable(Of PairItemType), Value As String, Optional strict As Boolean = True) As PairItemType()
        Dim Method = If(strict, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase)
        Dim LQuery = (From item In source Where String.Equals(item.Identifier, Value, Method) Select item).ToArray
        Return LQuery
    End Function

    ''' <summary>
    ''' use the overload method <see cref="IPairItem(Of TItem1, TItem2).Equals"></see> of the type
    ''' <see cref="IPairItem(Of TItem1, TItem2)"></see>
    ''' </summary>
    ''' <typeparam name="TItem1"></typeparam>
    ''' <typeparam name="TItem2"></typeparam>
    ''' <typeparam name="pairItem"></typeparam>
    ''' <param name="entry"></param>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetItem(Of TItem1, TItem2, pairItem As IPairItem(Of TItem1, TItem2))(entry As pairItem, source As IEnumerable(Of pairItem)) As pairItem()
        Dim LQuery As pairItem() = (From obj As pairItem In source Where entry.Equals(obj) Select obj).ToArray
        Return LQuery
    End Function

    <Extension> Public Function GetItems(Of T As INamedValue)(source As IEnumerable(Of T), uniqueId As String, Optional Explicit As Boolean = True) As T()
        If source.IsNullOrEmpty Then Return New T() {}

        Dim method As StringComparison = If(Explicit, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase)
        Dim value = (From x As T In source Where String.Equals(x.Key, uniqueId, method) Select x).ToArray

        Return value
    End Function

    ''' <summary>
    ''' 按照UniqueId列表来筛选出目标集合
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="lstId"></param>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Takes(Of T As INamedValue)(lstId As IEnumerable(Of String), source As IEnumerable(Of T)) As T()
        Dim Dict As Dictionary(Of T) = source.ToDictionary
        Dim LQuery As T() = (From sId As String In lstId Where Dict.ContainsKey(sId) Select Dict(sId)).ToArray
        Return LQuery
    End Function

    <Extension> Public Function GetItem(Of T As INamedValue)(source As IEnumerable(Of T), uniqueId As String) As T
        Dim LQuery = (From itemObj As T In source Where String.Equals(uniqueId, itemObj.Key) Select itemObj).FirstOrDefault
        Return LQuery
    End Function

    <Extension> Public Function ToEntryDictionary(Of T As IReadOnlyId)(source As IEnumerable(Of T)) As Dictionary(Of String, T)
        Return source.ToDictionary(Function(item As T) item.Identity)
    End Function

    <Extension> Public Function GetItem(Of T As IReadOnlyId)(source As IEnumerable(Of T), uniqueId As String, Optional caseSensitive As Boolean = True) As T
        Dim method As StringComparison = If(caseSensitive, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase)
        Dim LQuery = (From itemObj As T
                      In source
                      Where String.Equals(itemObj.Identity, uniqueId, method)
                      Select itemObj).FirstOrDefault
        Return LQuery
    End Function

    <Extension> Public Function ToDictionary(Of T As INamedValue)(source As IEnumerable(Of T), distinct As Boolean) As Dictionary(Of T)
        If Not distinct Then Return source.ToDictionary

        Dim Thash As Dictionary(Of T) = New Dictionary(Of T)
        For Each item As T In source
            If Not Thash.ContainsKey(item.Key) Then
                Call Thash.Add(item.Key, item)
            Else
                Call Console.WriteLine(item.Key & " is dulplicated......")
            End If
        Next

        Return Thash
    End Function
End Module
