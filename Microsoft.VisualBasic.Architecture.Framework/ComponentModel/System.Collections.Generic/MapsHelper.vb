#Region "Microsoft.VisualBasic::40695c422fa6278690cddead5d410403, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\System.Collections.Generic\MapsHelper.vb"

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
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace ComponentModel

    ''' <summary>
    ''' 其实这个对象就是字典查询的一个简化操作而已
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure MapsHelper(Of T)

        ReadOnly __default As T
        ReadOnly __maps As IReadOnlyDictionary(Of String, T)

        Default Public ReadOnly Property Value(key$) As T
            Get
                Return GetValue(key)
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

        Public Overrides Function ToString() As String
            Return __maps.DictionaryData.GetJson
        End Function
    End Structure

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

        ''' <summary>
        ''' 与<see cref="IKeyValuePairObject(Of TKey, TValue)"/>相比，这个类型更加倾向于特定化的描述两个对象之间的一一对应关系
        ''' </summary>
        Public Interface IMap
            Property Key As T1
            Property Maps As V
        End Interface
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

        Public Shared Widening Operator CType(map As Map(Of String, String)) As IDMap
            Return New IDMap With {
                .Key = map.Key,
                .Maps = map.Maps
            }
        End Operator

        Public Shared Widening Operator CType(map As IDMap) As Map(Of String, String)
            Return New Map(Of String, String) With {
                .Key = map.Key,
                .Maps = map.Maps
            }
        End Operator
    End Structure
End Namespace
