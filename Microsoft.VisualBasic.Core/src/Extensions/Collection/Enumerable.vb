#Region "Microsoft.VisualBasic::6397817a47a85186942ecdb4c3f4f08c, Microsoft.VisualBasic.Core\src\Extensions\Collection\Enumerable.vb"

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

    '   Total Lines: 326
    '    Code Lines: 227 (69.63%)
    ' Comment Lines: 51 (15.64%)
    '    - Xml Docs: 98.04%
    ' 
    '   Blank Lines: 48 (14.72%)
    '     File Size: 13.30 KB


    ' Module IEnumerations
    ' 
    '     Function: [Next], CartesianProduct, ContainsAll, CreateDictionary, (+2 Overloads) Differ
    '               ExceptType, (+2 Overloads) FindByItemKey, FindByItemValue, (+2 Overloads) GetItem, GetItems
    '               OfType, Permutations, Take, (+2 Overloads) Takes, ToDictionary
    '               ToEntryDictionary, Zip
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports Microsoft.VisualBasic.Text.Xml.Models.KeyValuePair

<Extension>
Public Module IEnumerations

    '#If NET48 Then

    '    <Extension>
    '    Public Iterator Function Zip(Of T1, T2)(s1 As IEnumerable(Of T1), s2 As IEnumerable(Of T2)) As IEnumerable(Of (First As T1, Second As T2))
    '        Dim pull1 = s1.GetEnumerator
    '        Dim pull2 = s2.GetEnumerator

    '        Do While pull1.MoveNext AndAlso pull2.MoveNext
    '            Yield (pull1.Current, pull2.Current)
    '        Loop
    '    End Function
    '#End If

    <Extension>
    Public Function OfType(Of A, B, T)(source As IEnumerable(Of [Variant](Of A, B))) As IEnumerable(Of T)
        Return source _
            .Where(Function(element) element Like GetType(T)) _
            .Select(Function(e) DirectCast(e.Value, T))
    End Function

    ''' <summary>
    ''' Get a random element
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="random"></param>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function [Next](Of T)(random As Random, data As T()) As T
        Return data(random.Next(0, data.Length))
    End Function

    <Extension>
    Public Iterator Function ExceptType(Of TIn, T As TIn)(src As IEnumerable(Of TIn)) As IEnumerable(Of TIn)
        For Each element As TIn In src
            If Not TypeOf element Is T Then
                Yield element
            End If
        Next
    End Function

    <Extension>
    Public Function Differ(Of T As INamedValue, T2)(source As IEnumerable(Of T),
                                                    toDiffer As IEnumerable(Of T2),
                                                    getId As Func(Of T2, String)) As String()

        Dim targetIndex As String() = (From item As T In source Select item.Key).ToArray
        Dim LQuery$() = LinqAPI.Exec(Of String) _
                                                _
            () <= From item As T2
                  In toDiffer
                  Let strId As String = getId(item)
                  Where Array.IndexOf(targetIndex, strId) = -1
                  Select strId

        Return LQuery
    End Function

    <Extension> Public Function Differ(Of T As INamedValue, T2 As INamedValue)(source As IEnumerable(Of T), ToDiffer As IEnumerable(Of T2)) As String()
        Dim targetIndex As String() = (From item In source Select item.Key).ToArray
        Dim LQuery = (From item As T2
                      In ToDiffer
                      Where Array.IndexOf(targetIndex, item.Key) = -1
                      Select item.Key).ToArray
        Return LQuery
    End Function

    <Extension>
    Public Function GetItem(Of T As INamedValue)(Id As String, source As IEnumerable(Of T)) As T
        Return source.Take(Id)
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
        Dim Dictionary As New Dictionary(Of String, T)

        For Each obj In Collection
            Call Dictionary.Add(obj.Key, obj)
        Next

        Return Dictionary
    End Function

    ''' <summary>
    ''' Text compare in case sensitive mode
    ''' </summary>
    ReadOnly TextCompareStrict As [Default](Of StringComparison) = StringComparison.Ordinal

    <Extension> Public Function FindByItemKey(source As IEnumerable(Of KeyValuePair), Key As String, Optional strict As Boolean = True) As KeyValuePair()
        Dim method As StringComparison = StringComparison.OrdinalIgnoreCase Or TextCompareStrict.When(strict)
        Dim LQuery = (From item In source Where String.Equals(item.Key, Key, method) Select item).ToArray
        Return LQuery
    End Function

    <Extension> Public Function FindByItemKey(Of PairItemType As IKeyValuePair)(source As IEnumerable(Of PairItemType), Key As String, Optional strict As Boolean = True) As PairItemType()
        Dim method As StringComparison = StringComparison.OrdinalIgnoreCase Or TextCompareStrict.When(strict)
        Dim LQuery = (From item In source Where String.Equals(item.Key, Key, method) Select item).ToArray
        Return LQuery
    End Function

    <Extension> Public Function FindByItemValue(Of PairItemType As IKeyValuePair)(source As IEnumerable(Of PairItemType), Value As String, Optional strict As Boolean = True) As PairItemType()
        Dim method As StringComparison = StringComparison.OrdinalIgnoreCase Or TextCompareStrict.When(strict)
        Dim LQuery = (From item In source Where String.Equals(item.Key, Value, method) Select item).ToArray
        Return LQuery
    End Function

    ''' <summary>
    ''' 这个函数假设参数<paramref name="source"/>之中是有重复的对象，则可以使用uniqueID数据提取出一个集合
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="uniqueId"></param>
    ''' <param name="strict">是否大小写敏感，默认大小写敏感</param>
    ''' <returns></returns>
    <Extension> Public Function Takes(Of T As INamedValue)(source As IEnumerable(Of T), uniqueId As String, Optional strict As Boolean = True) As T()
        If source Is Nothing Then
            Return New T() {}
        End If

        If strict Then
            Dim table As Dictionary(Of String, T()) = source _
                .GroupBy(Function(o) o.Key) _
                .ToDictionary(Function(k) k.Key,
                              Function(g) g.ToArray)

            If table.ContainsKey(uniqueId) Then
                Return table(uniqueId)
            Else
                Return {}
            End If
        Else
            Return LinqAPI.Exec(Of T) _
                                      _
                () <= From x As T
                      In source
                      Where String.Equals(x.Key, uniqueId, StringComparison.OrdinalIgnoreCase)
                      Select x
        End If
    End Function

    ''' <summary>
    ''' 按照uniqueId列表来筛选出目标集合，这个函数是使用字典来进行查询操作的，故而效率会比较高
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list">The list of ID value for <see cref="INamedValue.Key"/></param>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Takes(Of T As INamedValue)(list As IEnumerable(Of String), source As IEnumerable(Of T)) As T()
        Dim table As Dictionary(Of T) = source.ToDictionary
        Dim LQuery As T() = LinqAPI.Exec(Of T) _
                                               _
            () <= From sId As String
                  In list
                  Where table.ContainsKey(sId)
                  Select table(sId)

        Return LQuery
    End Function

    ''' <summary>
    ''' 使用<paramref name="uniqueId"/>唯一标识符从集合之中取出一个目标对象。
    ''' 小集合推荐使用这个函数，但是对于大型集合或者需要查询的次数非常多的话，则推荐使用字典操作来提升性能
    ''' 请注意这个函数会完全匹配字符串的，即大小写敏感
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="uniqueId"></param>
    ''' <returns></returns>
    <Extension> Public Function Take(Of T As INamedValue)(source As IEnumerable(Of T), uniqueId As String, Optional strict As Boolean = True) As T
        Dim level As StringComparison = StringComparison.OrdinalIgnoreCase Or TextCompareStrict.When(strict)
        Dim LQuery As T = LinqAPI.DefaultFirst(Of T) _
                                                     _
            () <= From o As T
                  In source
                  Where String.Equals(uniqueId, o.Key, comparisonType:=level)
                  Select o

        Return LQuery
    End Function

    <Extension> Public Function ToEntryDictionary(Of T As IReadOnlyId)(source As IEnumerable(Of T)) As Dictionary(Of String, T)
        Return source.ToDictionary(Function(item As T) item.Identity)
    End Function

    <Extension> Public Function GetItem(Of T As IReadOnlyId)(source As IEnumerable(Of T), uniqueId As String, Optional caseSensitive As Boolean = True) As T
        Dim method As StringComparison = StringComparison.OrdinalIgnoreCase Or TextCompareStrict.When(caseSensitive)
        Dim LQuery = LinqAPI.DefaultFirst(Of T) _
                                                _
            () <= From itemObj As T
                  In source
                  Where String.Equals(itemObj.Identity, uniqueId, method)
                  Select itemObj

        Return LQuery
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="distinct">
    ''' True: 这个参数会去处重复项
    ''' </param>
    ''' <returns></returns>
    <Extension> Public Function ToDictionary(Of T As INamedValue)(source As IEnumerable(Of T), distinct As Boolean) As Dictionary(Of T)
        If Not distinct Then
            Return source.ToDictionary
        End If

        Dim table As New Dictionary(Of T)
        Dim duplicates As New List(Of String)

        For Each x As T In source
            If Not table.ContainsKey(x.Key) Then
                Call table.Add(x.Key, x)
            Else
                duplicates += x.Key
            End If
        Next

        If duplicates > 0 Then
            Call $"Dictionary table build complete, but there is dulplicated keys: {duplicates.GetJson}...".Warning
        End If

        Return table
    End Function

    Public Function Permutations(Of T)(collection As IReadOnlyList(Of T)) As IEnumerable(Of T())
        Dim n = collection.Count
        Dim used = New Boolean(n - 1) {}
        Dim result = New T(n - 1) {}
        Dim memo = New Dictionary(Of T, Integer)()
        Dim recurse As Func(Of Integer, IEnumerable(Of T())) =
            Iterator Function(m As Integer) As IEnumerable(Of T())
                If m = n Then
                    Yield result.ToArray()
                Else
                    Dim j As Integer = Nothing

                    If (Not memo.TryGetValue(collection(m), j)) Then
                        j = 0
                    End If

                    For i As Integer = j To n - 1
                        If used(i) Then
                            Continue For
                        End If

                        used(i) = True
                        result(i) = collection(m)
                        memo(collection(m)) = i + 1

                        For Each res In recurse(m + 1)
                            Yield res
                        Next

                        used(i) = False
                        memo(collection(m)) = j
                    Next
                End If
            End Function

        Return recurse(0)
    End Function


    Public Function CartesianProduct(Of T)(collections As IReadOnlyList(Of IReadOnlyList(Of T))) As IEnumerable(Of T())
        Dim [set] = New T(collections.Count - 1) {}
        Dim rec As Func(Of Integer, IEnumerable(Of T())) =
            Iterator Function(i As Integer) As IEnumerable(Of T())
                If i = collections.Count Then
                    Yield [set].ToArray()
                Else
                    Dim collection = collections(i)

                    For Each item In collection
                        [set](i) = item
                        For Each res In rec(i + 1)
                            Yield res
                        Next
                    Next
                End If
            End Function

        Return rec(0)
    End Function

    <Extension>
    Public Function ContainsAll(Of T)([set] As ISet(Of T), all As IEnumerable(Of T)) As Boolean
        If Not all Is Nothing Then
            For Each item As T In all
                If Not [set].Contains(item) Then
                    Return False
                End If
            Next
        End If

        Return True
    End Function
End Module
