Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.KeyValuePair

<Extension>
Public Module IEnumerations

    <Extension> Public Function Differ(Of T As ComponentModel.Collection.Generic.sIdEnumerable, T2)(
                                          source As System.Collections.Generic.IEnumerable(Of T),
                                          ToDiffer As System.Collections.Generic.IEnumerable(Of T2),
                                          getId As Func(Of T2, String)) As String()

        Dim TargetIndex As String() = (From item As T In source Select item.Identifier).ToArray
        Dim LQuery = (From item As T2 In ToDiffer
                      Let strId As String = getId(item)
                      Where Array.IndexOf(TargetIndex, strId) = -1
                      Select strId).ToArray
        Return LQuery
    End Function

    <Extension> Public Function Differ(Of T As ComponentModel.Collection.Generic.sIdEnumerable,
                                         T2 As ComponentModel.Collection.Generic.sIdEnumerable)(
                                         source As System.Collections.Generic.IEnumerable(Of T),
                                         ToDiffer As System.Collections.Generic.IEnumerable(Of T2)) As String()

        Dim TargetIndex As String() = (From item In source Select item.Identifier).ToArray
        Dim LQuery = (From item As T2 In ToDiffer
                      Where Array.IndexOf(TargetIndex, item.Identifier) = -1
                      Select item.Identifier).ToArray
        Return LQuery
    End Function

    <Extension>
    Public Function GetItem(Of T As sIdEnumerable)(Id As String, source As IEnumerable(Of T)) As T
        Return source.GetItem(Id)
    End Function

    <Extension> Public Function GetItems(Of T As ComponentModel.Collection.Generic.sIdEnumerable)(source As IEnumerable(Of T), Id As String) As T()
        Dim LQuery = (From ItemObj As T In source Where String.Equals(Id, ItemObj.Identifier) Select ItemObj).ToArray
        Return LQuery
    End Function

    ''' <summary>
    ''' 将目标集合对象转换为一个字典对象
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function CreateDictionary(Of T As sIdEnumerable)(Collection As IEnumerable(Of T)) As Dictionary(Of String, T)
        Dim Dictionary As Dictionary(Of String, T) = New Dictionary(Of String, T)
        For Each obj In Collection
            Call Dictionary.Add(obj.Identifier, obj)
        Next

        Return Dictionary
    End Function

    <Extension> Public Function FindByItemKey(source As IEnumerable(Of ComponentModel.KeyValuePair), Key As String, Optional Explicit As Boolean = True) As ComponentModel.KeyValuePair()
        Dim Method = If(Explicit, System.StringComparison.Ordinal, System.StringComparison.OrdinalIgnoreCase)
        Dim LQuery = (From item In source Where String.Equals(item.Key, Key, Method) Select item).ToArray
        Return LQuery
    End Function

    <Extension> Public Function FindByItemKey(Of PairItemType As IKeyValuePair)(source As IEnumerable(Of PairItemType), Key As String, Optional Explicit As Boolean = True) As PairItemType()
        Dim Method = If(Explicit, System.StringComparison.Ordinal, System.StringComparison.OrdinalIgnoreCase)
        Dim LQuery = (From item In source Where String.Equals(item.locusId, Key, Method) Select item).ToArray
        Return LQuery
    End Function

    <Extension> Public Function FindByItemValue(Of PairItemType As IKeyValuePair)(source As IEnumerable(Of PairItemType), Value As String, Optional strict As Boolean = True) As PairItemType()
        Dim Method = If(strict, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase)
        Dim LQuery = (From item In source Where String.Equals(item.locusId, Value, Method) Select item).ToArray
        Return LQuery
    End Function

    ''' <summary>
    ''' use the overload method <see cref="ComponentModel.Collection.Generic.pairitem(Of TItem1, TItem2).Equals"></see> of the type 
    ''' <see cref="ComponentModel.Collection.Generic.PairItem(Of TItem1, TItem2)"></see>
    ''' </summary>
    ''' <typeparam name="TItem1"></typeparam>
    ''' <typeparam name="TItem2"></typeparam>
    ''' <typeparam name="pairItem"></typeparam>
    ''' <param name="entry"></param>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetItem(Of TItem1, TItem2, pairItem As PairItem(Of TItem1, TItem2))(entry As pairItem, source As IEnumerable(Of pairItem)) As pairItem()
        Dim LQuery As pairItem() = (From obj As pairItem In source Where entry.Equals(obj) Select obj).ToArray
        Return LQuery
    End Function

    <Extension> Public Function GetItems(Of T As sIdEnumerable)(source As IEnumerable(Of T), uniqueId As String, Optional Explicit As Boolean = True) As T()
        If source.IsNullOrEmpty Then Return New T() {}

        Dim method As StringComparison = If(Explicit, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase)
        Dim value = (From x As T In source Where String.Equals(x.Identifier, uniqueId, method) Select x).ToArray

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
    <Extension> Public Function Takes(Of T As sIdEnumerable)(lstId As IEnumerable(Of String), source As IEnumerable(Of T)) As T()
        Dim Dict As Dictionary(Of String, T) = source.ToDictionary
        Dim LQuery As T() = (From sId As String In lstId Where Dict.ContainsKey(sId) Select Dict(sId)).ToArray
        Return LQuery
    End Function

    <Extension> Public Function GetItem(Of T As sIdEnumerable)(source As IEnumerable(Of T), uniqueId As String) As T
        Dim LQuery = (From itemObj As T In source Where String.Equals(uniqueId, itemObj.Identifier) Select itemObj).FirstOrDefault
        Return LQuery
    End Function

    <Extension> Public Function ToEntryDictionary(Of T As IReadOnlyId)(source As IEnumerable(Of T)) As Dictionary(Of String, T)
        Return source.ToDictionary(Function(item As T) item.locusId)
    End Function

    <Extension> Public Function GetItem(Of T As IReadOnlyId)(source As IEnumerable(Of T), uniqueId As String, Optional caseSensitive As Boolean = True) As T
        Dim method As StringComparison = If(caseSensitive, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase)
        Dim LQuery = (From itemObj As T
                      In source
                      Where String.Equals(itemObj.locusId, uniqueId, method)
                      Select itemObj).FirstOrDefault
        Return LQuery
    End Function

    ''' <summary>
    ''' Converts the source collection into a dictionary object.(将目标集合对象转换为一个字典对象)
    ''' </summary>
    ''' <typeparam name="T">Unique identifier provider</typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ToDictionary(Of T As sIdEnumerable)(source As IEnumerable(Of T)) As Dictionary(Of String, T)
        Dim Dict As Dictionary(Of String, T) = New Dictionary(Of String, T)
        Dim i As Integer = 0
        Try
            For Each item As T In source
                Call Dict.Add(item.Identifier, item)
                i += 1
            Next
        Catch ex As Exception
            ex = New Exception(source(i).Identifier, ex)
            Throw ex
        End Try

        Return Dict
    End Function

    <Extension> Public Function ToDictionary(Of T As sIdEnumerable)(source As IEnumerable(Of T), distinct As Boolean) As Dictionary(Of String, T)
        If Not distinct Then Return source.ToDictionary

        Dim Dict As Dictionary(Of String, T) = New Dictionary(Of String, T)
        For Each item As T In source
            If Not Dict.ContainsKey(item.Identifier) Then
                Call Dict.Add(item.Identifier, item)
            Else
                Call Console.WriteLine(item.Identifier & " is dulplicated......")
            End If
        Next

        Return Dict
    End Function
End Module