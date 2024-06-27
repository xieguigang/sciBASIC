#Region "Microsoft.VisualBasic::6e614aae571ab76f79b778dd339a1b03, Microsoft.VisualBasic.Core\src\Extensions\Collection\KeyValuePair.vb"

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

    '   Total Lines: 928
    '    Code Lines: 536 (57.76%)
    ' Comment Lines: 289 (31.14%)
    '    - Xml Docs: 94.12%
    ' 
    '   Blank Lines: 103 (11.10%)
    '     File Size: 39.55 KB


    '     Module KeyValuePairExtensions
    ' 
    '         Function: (+2 Overloads) [Select], (+2 Overloads) Add, AsEnumerable, AsNamedValueTuples, AsTable
    '                   ComputeIfAbsent, (+3 Overloads) ContainsKey, (+2 Overloads) DescendingMap, DictionaryData, (+2 Overloads) EnumerateTuples
    '                   EnumParser, FlatTable, (+2 Overloads) GetByKey, GetValueOrDefault, GroupByKey
    '                   HaveData, IterateNameCollections, IterateNameValues, IteratesAll, Join
    '                   KeyItem, (+3 Overloads) Keys, (+2 Overloads) NamedValues, (+3 Overloads) NameValueCollection, ParserDictionary
    '                   Popout, RemoveAndGet, ReverseMaps, (+2 Overloads) Selects, SetOfKeyValuePairs
    '                   (+2 Overloads) Subset, tableInternal, (+2 Overloads) Takes, (+3 Overloads) ToDictionary, ToLower
    '                   ToUpper, Tsv, Tuple, TupleTable, (+2 Overloads) Values
    '                   XMLModel
    ' 
    '         Sub: Add, SortByKey, SortByValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Specialized
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports r = System.Text.RegularExpressions.Regex

Namespace ComponentModel.Collection

    ''' <summary>
    ''' KeyValue pair data related extensions API.
    ''' </summary>
    ''' 
    <HideModuleName>
    Public Module KeyValuePairExtensions

        <Extension>
        Public Iterator Function Values(Of T)(pool As IEnumerable(Of SeqValue(Of T))) As IEnumerable(Of T)
            For Each item As SeqValue(Of T) In pool.SafeQuery
                Yield item.value
            Next
        End Function

        ''' <summary>
        ''' 从目标字典中按照给定的键名称获取值然后删除给定的键名称对应的数据
        ''' </summary>
        ''' <typeparam name="K"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="table"></param>
        ''' <param name="key"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Popout(Of K, V)(table As Dictionary(Of K, V), key As K) As V
            If table.ContainsKey(key) Then
                Dim value As V = table(key)
                table.Remove(key)
                Return value
            Else
                Return Nothing
            End If
        End Function

        <Extension>
        Public Function TupleTable(tuple As (String(), String())) As Dictionary(Of String, String)
            Dim table As New Dictionary(Of String, String)

            For i As Integer = 0 To tuple.Item1.Length - 1
                Call table.Add(tuple.Item1(i), tuple.Item2(i))
            Next

            Return table
        End Function

        ''' <summary>
        ''' transform the hash key string to lower case characters
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="table"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToLower(Of T)(table As Dictionary(Of String, T)) As Dictionary(Of String, T)
            Return table.ToDictionary(Function(k) k.Key.ToLower, Function(k) k.Value)
        End Function

        ''' <summary>
        ''' transform the hash key string to upper case characters
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="table"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToUpper(Of T)(table As Dictionary(Of String, T)) As Dictionary(Of String, T)
            Return table.ToDictionary(Function(k) k.Key.ToUpper, Function(k) k.Value)
        End Function

        ''' <summary>
        ''' 这个拓展函数主要是针对值得构建比较耗时的操作，主要应用于数据缓存场景
        ''' </summary>
        ''' <typeparam name="K"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="table"></param>
        ''' <param name="key"></param>
        ''' <param name="lazyValue"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' target <paramref name="table"/> has been SyncLock at here
        ''' </remarks>
        <Extension>
        Public Function ComputeIfAbsent(Of K, V)(table As IDictionary(Of K, V), key As K, lazyValue As Func(Of K, V)) As V
            SyncLock table
                If Not table.ContainsKey(key) OrElse table(key:=key) Is Nothing Then
                    table(key:=key) = lazyValue(key)
                End If
            End SyncLock

            Return table(key:=key)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetValueOrDefault(Of K, V)(key As K, table As Dictionary(Of K, V), Optional [default] As V = Nothing) As V
            Return table.TryGetValue(key, [default]:=[default])
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Selects(Of K, V)(table As Dictionary(Of K, V), ParamArray keys As K()) As IEnumerable(Of V)
            Return From key As K In keys Select table(key)
        End Function

        <Extension>
        Public Iterator Function AsEnumerable(keys As NameObjectCollectionBase.KeysCollection) As IEnumerable(Of String)
            If Not keys Is Nothing AndAlso keys.Count > 0 Then
                For i As Integer = 0 To keys.Count - 1
                    Yield keys.Item(i)
                Next
            End If
        End Function

#Region "HashMapHelper"

        '	Copyright ©  - 2017 Tangible Software Solutions Inc.
        '	This class can be used by anyone provided that the copyright notice remains intact.
        '
        '	This class is used to replace calls to some Java HashMap or Hashtable methods.

        <Extension>
        Public Function SetOfKeyValuePairs(Of TKey, TValue)(dictionary As IDictionary(Of TKey, TValue)) As HashSet(Of KeyValuePair(Of TKey, TValue))
            Dim entries As New HashSet(Of KeyValuePair(Of TKey, TValue))()

            For Each keyValuePair As KeyValuePair(Of TKey, TValue) In dictionary
                Call entries.Add(keyValuePair)
            Next

            Return entries
        End Function
#End Region

        <Extension>
        Public Function DescendingMap(Of T As INamedValue)(data As IEnumerable(Of T)) As Dictionary(Of String, T)
            Return data.OrderByDescending(Function(a) a.Key).ToDictionary(Function(a) a.Key)
        End Function

        <Extension>
        Public Function DescendingMap(Of Key As IComparable(Of Key), T)(data As IEnumerable(Of KeyValuePair(Of Key, T))) As Dictionary(Of Key, T)
            Dim map As New Dictionary(Of Key, T)

            For Each item In data.OrderByDescending(Function(a) a.Key)
                Call map.Add(item.Key, item.Value)
            Next

            Return map
        End Function

#If NET_48 Or netcore5 = 1 Then

        ''' <summary>
        ''' Create a tuple for two elements
        ''' </summary>
        ''' <typeparam name="T1"></typeparam>
        ''' <typeparam name="T2"></typeparam>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Tuple(Of T1, T2)(a As T1, b As T2) As (T1, T2)
            Return (a, b)
        End Function

#End If

        ''' <summary>
        ''' 将目标键值对集合保存为一个``Tsv``文件
        ''' </summary>
        ''' <param name="table"></param>
        ''' <param name="saveTo$"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Tsv(table As Dictionary(Of String, String),
                            saveTo$,
                            Optional encoding As Encodings = Encodings.UTF8,
                            Optional reversed As Boolean = False) As Boolean

            Dim file$() = $"key{ASCII.TAB}map" +
                table _
                .Select(Function(map)
                            If reversed Then
                                Return $"{map.Value}{ASCII.TAB}{map.Key}"
                            Else
                                Return $"{map.Key}{ASCII.TAB}{map.Value}"
                            End If
                        End Function) _
                .AsList
            Return file.SaveTo(saveTo, encoding.CodePage)
        End Function

#If NET_48 Or netcore5 = 1 Then

        ''' <summary>
        ''' tuple set to dictionary table
        ''' </summary>
        ''' <typeparam name="K"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="tuples"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsTable(Of K, V)(tuples As IEnumerable(Of (K, V))) As Dictionary(Of K, V)
            Return tuples.ToDictionary(Function(t) t.Item1, Function(t) t.Item2)
        End Function

#End If

        ''' <summary>
        ''' Item selector by directly text equals match.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="key"></param>
        ''' <returns>
        ''' nothing will be return if the <paramref name="key"/> is not exists
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function KeyItem(Of T As INamedValue)(source As IEnumerable(Of T), key$) As T
            Return source _
                .Where(Function(i) i.Key = key) _
                .FirstOrDefault
        End Function

        ''' <summary>
        ''' Item selector by using regex expression.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="pattern">The regular expression pattern.</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        <Extension>
        Public Function Takes(Of T As INamedValue)(source As IEnumerable(Of T), pattern$) As IEnumerable(Of T)
            Return source _
                .Where(Function(i)
                           Return r.Match(i.Key, pattern, RegexICSng).Success
                       End Function)
        End Function

        ''' <summary>
        ''' 根据所给定的一个主键列表批量化取出目标字典之中的一部分数据构成一个新的字典子集对象
        ''' 函数会根据<see cref="keys"/>参数来做排序
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="table"></param>
        ''' <param name="keys$"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Subset(Of T)(table As Dictionary(Of String, T), keys$(), Optional ignoreMissing As Boolean = False) As Dictionary(Of String, T)
            If ignoreMissing Then
                Return keys _
                    .Where(AddressOf table.ContainsKey) _
                    .ToDictionary(Function(key) key,
                                  Function(key)
                                      Return table(key)
                                  End Function)
            Else
                Return keys.ToDictionary(Function(key) key,
                                         Function(key)
                                             Return table(key)
                                         End Function)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Subset(Of K, V)(table As Dictionary(Of K, V), assert As Func(Of K, V, Boolean)) As Dictionary(Of K, V)
            Return table _
                .Where(Function(map) assert(map.Key, map.Value)) _
                .ToDictionary(Function(map) map.Key,
                              Function(map) map.Value)
        End Function

        ''' <summary>
        ''' 按照给定的键名列表<paramref name="keys"/>将字典之中的对应的元素按照键名的顺序取出来
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="table"></param>
        ''' <param name="keys"></param>
        ''' <param name="default">
        ''' Use this as default value is key is not exists
        ''' </param>
        ''' <returns>
        ''' <paramref name="default"/> value will be returned if the 
        ''' specific key is not exists in the given <paramref name="table"/>
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Iterator Function Takes(Of T)(table As IDictionary(Of String, T),
                                             keys As IEnumerable(Of String),
                                             Optional [default] As T = Nothing) As IEnumerable(Of T)
            For Each key As String In keys
                If table.ContainsKey(key) Then
                    Yield table(key)
                Else
                    Yield [default]
                End If
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function XMLModel(data As IEnumerable(Of NamedValue(Of String))) As NamedValue()
            Return data _
                .Select(Function(n) New NamedValue With {.name = n.Name, .text = n.Value}) _
                .ToArray
        End Function

        <Extension>
        Public Iterator Function EnumerateTuples(Of T)(table As Dictionary(Of String, T)) As IEnumerable(Of (name As String, obj As T))
            For Each entry In table
                Yield (entry.Key, entry.Value)
            Next
        End Function

        <Extension>
        Public Iterator Function EnumerateTuples(Of K, V)(table As IEnumerable(Of KeyValuePair(Of K, V))) As IEnumerable(Of (tag As K, obj As V))
            For Each entry In table
                Yield (entry.Key, entry.Value)
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsNamedValueTuples(Of T)(tuples As IEnumerable(Of KeyValuePair(Of String, T))) As IEnumerable(Of NamedValue(Of T))
            Return tuples.Select(Function(p) New NamedValue(Of T)(p.Key, p.Value))
        End Function

        ''' <summary>
        ''' Removes the target key in the dictionary table, and then gets the removed value.
        ''' (删除字典之中的指定的键值对，然后返回被删除的数据值)
        ''' </summary>
        ''' <typeparam name="K"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="table"></param>
        ''' <param name="key"></param>
        ''' <returns>The value of the removed <paramref name="key"/></returns>
        <Extension>
        Public Function RemoveAndGet(Of K, V)(table As Dictionary(Of K, V), key As K) As V
            Dim item As V = table(key)
            Call table.Remove(key)
            Return item
        End Function

        ''' <summary>
        ''' Iterates all of the values in the <paramref name="source"/> collection data.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Function IteratesAll(Of T As INamedValue)(source As IEnumerable(Of NamedCollection(Of T))) As T()
            Return source.Select(Function(c) c.value).IteratesALL.ToArray
        End Function

        <Extension>
        Public Sub Add(Of T)(ByRef list As System.Collections.Generic.List(Of NamedCollection(Of T)), name As String, data As IEnumerable(Of T))
            Call list.Add(New NamedCollection(Of T)(name, data))
        End Sub

        ''' <summary>
        ''' Groups source by <see cref="INamedValue.Key"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GroupByKey(Of T As INamedValue)(source As IEnumerable(Of T)) As NamedCollection(Of T)()
            Return source _
                .GroupBy(Function(o) o.Key) _
                .Select(Function(g)
                            Return New NamedCollection(Of T) With {
                                 .name = g.Key,
                                 .value = g.ToArray
                             }
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' Retrieve all items' value data.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Values(Of T)(source As IEnumerable(Of NamedValue(Of T))) As T()
            Return source _
                .SafeQuery _
                .Select(Function(x) x.Value) _
                .ToArray
        End Function

        ''' <summary>
        ''' get values from the key-value paires
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="V">the tuple value type</typeparam>
        ''' <param name="src"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Values(Of T, V)(src As IEnumerable(Of KeyValuePair(Of T, V))) As V()
            Return src.SafeQuery.Select(Function(x) x.Value).ToArray
        End Function

        ''' <summary>
        ''' gets all <see cref="INamedValue.Key"/> values.
        ''' (按照顺序得到输入的序列之中的<see cref="INamedValue.Key"/>属性值)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="distinct">是否还进行去重操作？默认不做去重</param>
        ''' <returns>
        ''' A collection list of the index key name values which is extract from the <paramref name="source"/>,
        ''' the label key orders keeps the same with the elements inside the input <paramref name="source"/>
        ''' sequence.
        ''' </returns>
        <Extension>
        Public Function Keys(Of T As INamedValue)(source As IEnumerable(Of T), Optional distinct As Boolean = False) As List(Of String)
            Dim list As IEnumerable(Of String) = source.Select(Function(o) o.Key)
            If distinct Then
                list = list.Distinct
            End If
            Return list.AsList
        End Function

        <Extension>
        Public Function Keys(Of T As INamedValue)(source As Enumeration(Of T), Optional distinct As Boolean = False) As List(Of String)
            Return source.AsEnumerable.Keys
        End Function

        ''' <summary>
        ''' get group keys
        ''' </summary>
        ''' <typeparam name="K"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Keys(Of K, V)(source As IEnumerable(Of IGrouping(Of K, V))) As K()
            Return source _
                .Select(Function(x) x.Key) _
                .ToArray
        End Function

        ''' <summary>
        ''' Get a specific item value from the target collction data using its UniqueID property，
        ''' (请注意，请尽量不要使用本方法，因为这个方法的效率有些低，对于获取<see cref="INamedValue">
        ''' </see>类型的集合之中的某一个对象，请尽量先转换为字典对象，在使用该字典对象进行查找以提高代码效率，使用本方法的优点是可以选择忽略<paramref name="uid">
        ''' </paramref>参数之中的大小写，以及对集合之中的存在相同的Key的这种情况的容忍)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="uid"></param>
        ''' <param name="IgnoreCase"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ExportAPI("Get.Item")>
        <Extension> Public Function GetByKey(Of T As INamedValue)(
                                           source As IEnumerable(Of T),
                                              uid As String,
                              Optional ignoreCase As StringComparison = StringComparison.Ordinal) _
                                                  As T

            Dim find As T = LinqAPI.DefaultFirst(Of T) _
 _
                () <= From x As T
                      In source
                      Where String.Equals(uid, x.Key, ignoreCase)
                      Select x

            Return find
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function GetByKey(Of T As INamedValue)(
                                           source As IEnumerable(Of T),
                                              uid As String,
                              Optional ignoreCase As Boolean = False) _
                                                  As T
            Return source.GetByKey(uid, If(ignoreCase, StringComparison.OrdinalIgnoreCase, StringComparison.Ordinal))
        End Function

        ''' <summary>
        ''' Dictionary object contains the specific <see cref="NamedValue(Of T).Name"/>?
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="table"></param>
        ''' <param name="k"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ContainsKey(Of T As INamedValue)(table As Dictionary(Of T), k As NamedValue(Of T)) As Boolean
            Return table.ContainsKey(k.Name)
        End Function

        ''' <summary>
        ''' Dictionary object contains the specific <see cref="NamedValue(Of T).Name"/>?
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="table"></param>
        ''' <param name="k"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ContainsKey(Of T)(table As Dictionary(Of String, T), k As NamedValue(Of T)) As Boolean
            Return table.ContainsKey(k.Name)
        End Function

        ''' <summary>
        ''' Converts the interface object into a Dictionary object.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function DictionaryData(Of T, V)(source As IReadOnlyDictionary(Of T, V)) As Dictionary(Of T, V)
            Return source.ToDictionary(Function(x) x.Key, Function(x) x.Value)
        End Function

        ''' <summary>
        ''' Creates the dictionary for string converts to enum value.
        ''' (接受的泛型类型必须是枚举类型)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="lcaseKey"></param>
        ''' <param name="usingDescription"></param>
        ''' <returns></returns>
        Public Function EnumParser(Of T As Structure)(Optional lcaseKey As Boolean = True, Optional usingDescription As Boolean = False) As Dictionary(Of String, T)
            Dim values As [Enum]() = Enums(Of T)().Select(Function(e) DirectCast(CType(e, Object), [Enum])).ToArray
            Dim [case] = If(lcaseKey, Function(key$) LCase(key), Function(key$) key)

            If usingDescription Then
                Return values.ToDictionary(
                    Function(e) [case](key:=e.Description),
                    Function(e) DirectCast(CType(e, Object), T))
            Else
                Return values.ToDictionary(
                    Function(e) [case](key:=e.ToString),
                    Function(e) DirectCast(CType(e, Object), T))
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function NamedValues(maps As IEnumerable(Of IDMap)) As NamedValue(Of String)()
            Return maps _
                .Select(Function(m) New NamedValue(Of String)(m.Key, m.Maps)) _
                .ToArray
        End Function

        ''' <summary>
        ''' Convert the dictionary table as the <see cref="NamedValue(Of T)"/> collection.
        ''' (将目标字典之中的键值对转换为被命名为的变量值)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="table"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 如果需要调用Linq模式的方法，可以使用<see cref="IterateNameValues"/>
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function NamedValues(Of T)(table As Dictionary(Of String, T)) As NamedValue(Of T)()
            Return table _
                .Select(Function(k)
                            Return New NamedValue(Of T) With {
                                .Name = k.Key,
                                .Value = k.Value
                            }
                        End Function) _
                .ToArray
        End Function

        <Extension>
        Public Iterator Function IterateNameValues(Of T)(table As Dictionary(Of String, T)) As IEnumerable(Of NamedValue(Of T))
            For Each map As KeyValuePair(Of String, T) In table
                Yield New NamedValue(Of T) With {
                    .Name = map.Key,
                    .Value = map.Value
                }
            Next
        End Function

        <Extension>
        Public Iterator Function IterateNameCollections(Of T)(table As Dictionary(Of String, T())) As IEnumerable(Of NamedCollection(Of T))
            For Each map As KeyValuePair(Of String, T()) In table
                Yield New NamedCollection(Of T)(map.Key, map.Value)
            Next
        End Function

        <Extension>
        Public Function NameValueCollection(maps As IEnumerable(Of IDMap)) As NameValueCollection
            Dim nc As New NameValueCollection

            For Each m As IDMap In maps
                Call nc.Add(m.Key, m.Maps)
            Next

            Return nc
        End Function

        <Extension>
        Public Function NameValueCollection(maps As IEnumerable(Of KeyValuePair(Of String, String))) As NameValueCollection
            Dim nv As New NameValueCollection

            For Each tuple As KeyValuePair(Of String, String) In maps
                Call nv.Add(tuple.Key, tuple.Value)
            Next

            Return nv
        End Function

        ''' <summary>
        ''' 获取得到的集合对象是一个安全的集合对象，不存在的键名会直接返回空值
        ''' </summary>
        ''' <param name="maps">
        ''' the duplicated keys in this tuple list can be existed
        ''' </param>
        ''' <returns>
        ''' the duplicated key can be existed
        ''' </returns>
        <Extension>
        Public Function NameValueCollection(maps As IEnumerable(Of NamedValue(Of String))) As NameValueCollection
            Dim nc As New NameValueCollection

            For Each m As NamedValue(Of String) In maps
                ' 20221031 for the tuple value with duplicated name
                ' that will add to the exists array list to
                ' create a data array
                Call nc.Add(m.Name, m.Value)
            Next

            Return nc
        End Function

        <Extension> Public Sub SortByValue(Of V, T)(ByRef table As Dictionary(Of V, T), Optional desc As Boolean = False)
            Dim orders As KeyValuePair(Of V, T)()
            Dim out As New Dictionary(Of V, T)

            If Not desc Then
                orders = table.OrderBy(Function(p) p.Value).ToArray
            Else
                orders = table _
                    .OrderByDescending(Function(p) p.Value) _
                    .ToArray
            End If

            For Each k As KeyValuePair(Of V, T) In orders
                Call out.Add(k.Key, k.Value)
            Next

            table = out
        End Sub

        ''' <summary>
        ''' 按照键名对字典进行重新排序
        ''' </summary>
        ''' <typeparam name="V"></typeparam>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="table"></param>
        ''' <param name="desc">默认为从小到大的升序排序</param>
        <Extension> Public Sub SortByKey(Of V, T)(ByRef table As Dictionary(Of V, T), Optional desc As Boolean = False)
            Dim orders As V()
            Dim out As New Dictionary(Of V, T)

            If Not desc Then
                orders = table.Keys.OrderBy(Function(k) k).ToArray
            Else
                orders = table.Keys _
                    .OrderByDescending(Function(k) k) _
                    .ToArray
            End If

            For Each k As V In orders
                Call out.Add(k, table(k))
            Next

            table = out
        End Sub

        ''' <summary>
        ''' Determines whether the <see cref="NameValueCollection"/> contains the specified key.
        ''' </summary>
        ''' <param name="d"></param>
        ''' <param name="key$">The key to locate in the <see cref="NameValueCollection"/></param>
        ''' <returns>true if the System.Collections.Generic.Dictionary`2 contains an element with
        ''' the specified key; otherwise, false.</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ContainsKey(d As NameValueCollection, key$) As Boolean
            Return Not String.IsNullOrEmpty(d(key))
        End Function

        <Extension>
        Public Function Join(Of T, V)(d As Dictionary(Of T, V), name As T, value As V) As Dictionary(Of T, V)
            d(name) = value
            Return d
        End Function

        ''' <summary>
        ''' 请注意，这里的类型约束只允许枚举类型
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ParserDictionary(Of T As Structure)() As Dictionary(Of String, T)
            Return Enums(Of T).ToDictionary(Function(x) DirectCast(CType(x, Object), [Enum]).Description)
        End Function

        ''' <summary>
        ''' Data exists and not nothing
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="d"></param>
        ''' <param name="key"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function HaveData(Of T)(d As Dictionary(Of T, String), key As T) As Boolean
            Return d.ContainsKey(key) AndAlso Not String.IsNullOrEmpty(d(key))
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="nc"></param>
        ''' <param name="allStrings">get all of the string values for each 
        ''' parameter name? if this parameter value is TRUE, then the value 
        ''' of the output dictionary will be a string array.
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function ToDictionary(nc As NameValueCollection, Optional allStrings As Boolean = False) As [Variant](Of Dictionary(Of String, String), Dictionary(Of String, String()))
            If allStrings Then
                Dim table As New Dictionary(Of String, String())

                For Each key As String In nc.AllKeys
                    table(key) = nc.GetValues(key)
                Next

                Return table
            Else
                Dim table As New Dictionary(Of String, String)

                For Each key As String In nc.AllKeys
                    table(key) = nc(key)
                Next

                Return table
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToDictionary(Of K, V, KOut, VOut)(input As IEnumerable(Of KeyValuePair(Of K, V)), key As Func(Of K, V, KOut), value As Func(Of K, V, VOut)) As Dictionary(Of KOut, VOut)
            Return input.ToDictionary(Function(tuple) key(tuple.Key, tuple.Value),
                                      Function(tuple)
                                          Return value(tuple.Key, tuple.Value)
                                      End Function)
        End Function

        Const sourceEmpty$ = "Source is nothing, returns empty dictionary table!"

        ''' <summary>
        ''' Creates a <see cref="System.Collections.Generic.Dictionary"/>`2 from an <see cref="System.Collections.Generic.IEnumerable"/>`1
        ''' according to a specified key selector function.
        ''' </summary>
        ''' <typeparam name="T">Unique identifier provider <see cref="INamedValue.Key"/></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function ToDictionary(Of T As INamedValue)(source As IEnumerable(Of T), Optional replaceOnDuplicate As Boolean = False) As Dictionary(Of T)
            If source Is Nothing Then
#If DEBUG Then
                Call sourceEmpty.Warning
#End If
                Return New Dictionary(Of T)
            Else
                Dim currentKey As String = "NA"
                Dim keys As New List(Of String)

                Try
                    Return source.tableInternal(currentKey, keys, replaceOnDuplicate)
                Catch ex As Exception
                    ex = New Exception(currentKey, ex)
                    ex = New Exception(keys.GetJson, ex)

                    Throw ex
                End Try
            End If
        End Function

        <Extension>
        Private Function tableInternal(Of T As INamedValue)(source As IEnumerable(Of T),
                                                            ByRef currentKey$,
                                                            ByRef keys As List(Of String),
                                                            replaceOnDuplicate As Boolean) As Dictionary(Of T)
            With New Dictionary(Of T)
                If replaceOnDuplicate Then
                    For Each item As T In source
                        .Item(item.Key) = item
                    Next
                Else
                    For Each item As T In source
                        currentKey = item.Key

                        Call .Add(currentKey, item)
                        Call keys.Add(currentKey)
                    Next
                End If

                Return .ByRef
            End With
        End Function

        ''' <summary>
        ''' 将命名变量对象进行降维，名字作为键名，值作为键值，生成字典
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="table"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function FlatTable(Of T)(table As Dictionary(Of NamedValue(Of T))) As Dictionary(Of String, T)
            Return table.ToDictionary(Function(item) item.Key,
                                      Function(item) item.Value.Value)
        End Function

        <Extension> Public Function Add(Of TKey, TValue)(ByRef list As List(Of KeyValuePair(Of TKey, TValue)), key As TKey, value As TValue) As List(Of KeyValuePair(Of TKey, TValue))
            If list Is Nothing Then
                list = New List(Of KeyValuePair(Of TKey, TValue))
            End If
            list += New KeyValuePair(Of TKey, TValue)(key, value)
            Return list
        End Function

        ''' <summary>
        ''' Adds an object to the end of the List`1.
        ''' </summary>
        ''' <typeparam name="TKey"></typeparam>
        ''' <typeparam name="TValue"></typeparam>
        ''' <param name="list"></param>
        ''' <param name="key"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        <Extension> Public Function Add(Of TKey, TValue)(ByRef list As List(Of KeyValuePairObject(Of TKey, TValue)), key As TKey, value As TValue) As List(Of KeyValuePairObject(Of TKey, TValue))
            If list Is Nothing Then
                list = New List(Of KeyValuePairObject(Of TKey, TValue))
            End If
            list += New KeyValuePairObject(Of TKey, TValue)(key, value)
            Return list
        End Function

        ''' <summary>
        ''' 使用这个函数应该要确保value是没有重复的，假若<paramref name="removeDuplicated"/>是默认值的话.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="d"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ReverseMaps(Of T, V)(d As Dictionary(Of T, V), Optional removeDuplicated As Boolean = False) As Dictionary(Of V, T)
            If removeDuplicated Then
                Dim groupsData = From x In d Select x Group x By x.Value Into Group
                Return groupsData.ToDictionary(
                    Function(g) g.Value,
                    Function(g) g.Group.First.Key)
            Else
                Return d.ToDictionary(
                    Function(x) x.Value,
                    Function(x) x.Key)
            End If
        End Function

        ''' <summary>
        ''' 一次性地使用一个键名的集合从字典之中选出一组数据
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="d"></param>
        ''' <param name="keys"></param>
        ''' <param name="skipNonExist"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Selects(Of T, V)(d As Dictionary(Of T, V), keys As IEnumerable(Of T), Optional skipNonExist As Boolean = False) As V()
            If skipNonExist Then
                Return keys _
                    .Where(AddressOf d.ContainsKey) _
                    .Select(Function(k) d(k)) _
                    .ToArray
            Else
                Return keys.Select(Function(k) d(k)).ToArray
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function [Select](Of K, V, T)(source As IEnumerable(Of KeyValuePair(Of K, V)), project As Func(Of K, V, T)) As IEnumerable(Of T)
            Return source.Select(Function(value) project(value.Key, value.Value))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function [Select](Of V, T)(source As IEnumerable(Of KeyValuePair(Of String, V)), project As Func(Of String, V, T)) As IEnumerable(Of T)
            Return source.Select(Function(value) project(value.Key, value.Value))
        End Function
    End Module

End Namespace
