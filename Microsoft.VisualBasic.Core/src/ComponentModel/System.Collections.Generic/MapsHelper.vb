#Region "Microsoft.VisualBasic::365e41ebbae12c816e37e68eabfbc3ce, Microsoft.VisualBasic.Core\src\ComponentModel\System.Collections.Generic\MapsHelper.vb"

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

    '   Total Lines: 243
    '    Code Lines: 150 (61.73%)
    ' Comment Lines: 53 (21.81%)
    '    - Xml Docs: 92.45%
    ' 
    '   Blank Lines: 40 (16.46%)
    '     File Size: 8.62 KB


    '     Class MapsHelper
    ' 
    '         Properties: [Default]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetEnumerator, GetValue, IEnumerable_GetEnumerator, ToString
    ' 
    '     Class NameMapping
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Add
    ' 
    '     Structure Map
    ' 
    '         Properties: Key, Maps
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    '         Interface IMap
    ' 
    '             Properties: Key, Maps
    ' 
    ' 
    ' 
    '     Structure IDMap
    ' 
    '         Properties: Key, Maps
    ' 
    '         Function: ParseFromTsv, ParseTsvFile, ToString, TSV
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
Imports Microsoft.VisualBasic.Text

Namespace ComponentModel

    ''' <summary>
    ''' 其实这个对象就是字典查询的一个简化操作而已
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class MapsHelper(Of T) : Implements IEnumerable(Of Map(Of String, T))

        Protected ReadOnly __default As T
        Protected ReadOnly __maps As IDictionary(Of String, T)

        Default Public ReadOnly Property Value(key$) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetValue(key)
            End Get
        End Property

        Public ReadOnly Property [Default] As T
            Get
                Return __default
            End Get
        End Property

        Sub New(map As IReadOnlyDictionary(Of String, T), Optional [default] As T = Nothing)
            __default = [default]
            __maps = map
        End Sub

        Public Function GetValue(key As String) As T
            If __maps.ContainsKey(key) Then
                Return __maps(key)
            Else
                Return __default
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return __maps.ToDictionary.GetJson
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Map(Of String, T)) Implements IEnumerable(Of Map(Of String, T)).GetEnumerator
            For Each tuple As KeyValuePair(Of String, T) In __maps
                Yield New Map(Of String, T)(tuple.Key, tuple.Value)
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class

    ''' <summary>
    ''' 将文件之中的字段名称映射为另一个名称的帮助模块
    ''' </summary>
    Public Class NameMapping : Inherits MapsHelper(Of String)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks>
        ''' ###### 2019-03-08 因为会存在<see cref="Add"/>添加数据的过程,所以在这里应该是使用constructor新构建一个对象
        ''' 否则会因为第二次使用<see cref="NameMapping"/>的时候因为对象引用的原因而出现错误
        ''' </remarks>
        Shared ReadOnly emptyMaps As New [Default](Of Dictionary(Of String, String)) With {
            .constructor = Function() New Dictionary(Of String, String)
        }

        Sub New(Optional dictionary As Dictionary(Of String, String) = Nothing,
                Optional default$ = Nothing)
            Call MyBase.New(dictionary Or emptyMaps, [default])
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(key$, map$)
            Call __maps.Add(key, map)
        End Sub

        Public Shared Widening Operator CType(table$(,)) As NameMapping
            Dim dictionary As New Dictionary(Of String, String)

            For Each map In table.RowIterator
                Call dictionary.Add(map(0), map(1))
            Next

            Return New NameMapping(dictionary, "")
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(maps As NameMapping) As Dictionary(Of String, String)
            If maps Is Nothing Then
                Return Nothing
            Else
                Return New Dictionary(Of String, String)(dictionary:=maps.__maps)
            End If
        End Operator
    End Class

    Public Structure Map(Of T1, V)
        Implements IMap

        ''' <summary>
        ''' The map source
        ''' </summary>
        ''' <returns></returns>
        Public Property Key As T1 Implements IMap.Key
        ''' <summary>
        ''' The mapped target value.
        ''' </summary>
        ''' <returns></returns>
        Public Property Maps As V Implements IMap.Maps

        Sub New(x As T1, y As V)
            Key = x
            Maps = y
        End Sub

        Public Overrides Function ToString() As String
            Return $"map[{Key}, {Maps}]"
        End Function

        ''' <summary>
        ''' 与<see cref="IKeyValuePairObject(Of TKey, TValue)"/>相比，这个类型更加倾向于特定化的描述两个对象之间的一一对应关系
        ''' </summary>
        Public Interface IMap
            Property Key As T1
            Property Maps As V
        End Interface

#If NET_48 Or netcore5 = 1 Then

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(map As Map(Of T1, V)) As (key As T1, mapAs As V)
            Return (map.Key, map.Maps)
        End Operator

#End If

        ''' <summary>
        ''' 因为map主要的作用是获取得到key所配对的value结果，所以在这里是转换为<see cref="Maps"/>结果值的
        ''' </summary>
        ''' <param name="map"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(map As Map(Of T1, V)) As V
            Return map.Maps
        End Operator
    End Structure

    ''' <summary>
    ''' 字符串类型的映射
    ''' </summary>
    Public Structure IDMap : Implements Map(Of String, String).IMap
        Implements INamedValue

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 虽然说这个对象也实现了<see cref="INamedValue"/>接口，但是Key也可能是会出现重复的
        ''' </remarks>
        Public Property Key As String Implements Map(Of String, String).IMap.Key, INamedValue.Key
        Public Property Maps As String Implements Map(Of String, String).IMap.Maps

        ''' <summary>
        ''' 将这个映射转换为tsv文件之中的一行数据
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TSV() As String
            Return Key & vbTab & Maps
        End Function

        Public Shared Function ParseFromTsv(line$) As IDMap
            Dim t = line.Split(ASCII.TAB)

            If t.Length = 0 Then
                Return New IDMap
            ElseIf t.Length = 1 Then
                t.Add("")
            End If

            Return New IDMap With {
                .Key = t(0),
                .Maps = t(1)
            }
        End Function

        ''' <summary>
        ''' 将TSV文件之中的数据行解析为IDMapping结果
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <param name="haveHeader"></param>
        ''' <param name="header"></param>
        ''' <returns></returns>
        Public Shared Function ParseTsvFile(path$, Optional haveHeader As Boolean = False, Optional ByRef header As IDMap = Nothing) As IDMap()
            Dim lines$() = path.ReadAllLines
            Dim out As New List(Of IDMap)

            If haveHeader Then
                header = ParseFromTsv(lines(Scan0))
            End If

            For Each line As String In lines.Skip(If(haveHeader, 1, 0))
                out += IDMap.ParseFromTsv(line)
            Next

            Return out
        End Function

        Public Overrides Function ToString() As String
            Return $"{Key} --> {Maps}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(map As Map(Of String, String)) As IDMap
            Return New IDMap With {
                .Key = map.Key,
                .Maps = map.Maps
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(map As IDMap) As Map(Of String, String)
            Return New Map(Of String, String) With {
                .Key = map.Key,
                .Maps = map.Maps
            }
        End Operator
    End Structure
End Namespace
