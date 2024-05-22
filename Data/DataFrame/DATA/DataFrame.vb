#Region "Microsoft.VisualBasic::9cb52eef12222618ddfc34ef63606cca, Data\DataFrame\DATA\DataFrame.vb"

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

    '   Total Lines: 191
    '    Code Lines: 125 (65.45%)
    ' Comment Lines: 44 (23.04%)
    '    - Xml Docs: 90.91%
    ' 
    '   Blank Lines: 22 (11.52%)
    '     File Size: 7.68 KB


    '     Class DataFrame
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: [As], Append, Cbind, GetEnumerator, IEnumerable_GetEnumerator
    '                   Load, SaveTable, ToString
    ' 
    '         Sub: TagFieldName
    ' 
    '         Operators: +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace DATA

    ''' <summary>
    ''' 提供了类似于R语言之中的``cbind``操作类似的按照列进行数据框合并的方法
    ''' </summary>
    Public Class DataFrame : Implements IEnumerable(Of EntityObject)

        ''' <summary>
        ''' Row data in the csv table
        ''' </summary>
        Dim entityList As Dictionary(Of EntityObject)

        Default Public Property Item(id$, property$) As String
            Get
                Return entityList(id)([property])
            End Get
            Set(value As String)
                entityList(id)([property]) = value
            End Set
        End Property

        Default Public Property Item([property] As String) As String()
            Get
                Return entityList _
                    .Values _
                    .Select(Function(d) d([property])) _
                    .ToArray
            End Get
            Set(value As String())
                For Each i As SeqValue(Of EntityObject) In entityList.Values.SeqIterator
                    i.value([property]) = value.ElementAtOrDefault(i)
                Next
            End Set
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(list As IEnumerable(Of EntityObject), Optional doUnique As Boolean = False)
            entityList = list.ToDictionary(replaceOnDuplicate:=doUnique)
        End Sub

        Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub TagFieldName(tag$, fieldName$)
            Call MappingsHelper _
                .TagFieldName(entityList.Values, tag, fieldName) _
                .ToArray
        End Sub

        Public Function Cbind(data As EntityObject, Optional transpose As Boolean = False) As DataFrame
            If Not transpose Then
                Return Me + {data}
            Else
                Return Me + data.Properties _
                    .Select(Function(r)
                                Return New EntityObject With {
                                    .ID = r.Key,
                                    .Properties = New Dictionary(Of String, String) From {
                                        {data.ID, r.Value}
                                    }
                                }
                            End Function) _
                    .ToArray
            End If
        End Function

        ''' <summary>
        ''' Convert row object as target .NET object
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [As](Of T As Class)() As IEnumerable(Of T)
            Return entityList.Values _
                .ToCsvDoc _
                .AsDataSource(Of T)
        End Function

        ''' <summary>
        ''' Get all entity keys
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return entityList.Keys.ToArray.GetJson
        End Function

        ''' <summary>
        ''' Save as csv file.
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SaveTable(path$, Optional encoding As Encodings = Encodings.UTF8) As Boolean
            Return entityList.Values.SaveTo(path, encoding:=encoding.CodePage, strict:=False)
        End Function

        ''' <summary>
        ''' Load from a csv file
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Load(path$,
                                    Optional encoding As Encodings = Encodings.Default,
                                    Optional uidMap$ = Nothing,
                                    Optional doUnique As Boolean = False) As DataFrame

            Return New DataFrame(EntityObject.LoadDataSet(path, uidMap:=uidMap), doUnique)
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of EntityObject) Implements IEnumerable(Of EntityObject).GetEnumerator
            For Each x In entityList.Values
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        ''' <summary>
        ''' ``cbind`` operation.
        ''' （通过这个操作符进行两个数据集的合并，不会出现数据遗漏）
        ''' </summary>
        ''' <param name="data">unique</param>
        ''' <param name="appends">multiple</param>
        ''' <returns></returns>
        Public Shared Operator +(data As DataFrame, appends As IEnumerable(Of EntityObject)) As DataFrame
            For Each x As EntityObject In appends
                ' 如果对象列表之中存在append，则进行属性合并
                If data.entityList.ContainsKey(x.ID) Then
                    With data.entityList(x.ID)
                        For Each [property] In x.Properties
                            .ItemValue([property].Key) = [property].Value
                        Next
                    End With
                Else
                    ' 当对象不存在的时候，则直接进行追加
                    data.entityList += x
                End If
            Next

            Return data
        End Operator

        ''' <summary>
        ''' 这是一个可伸缩的Linq方法，可能会出现数据遗漏，即<paramref name="unique"/>数据集之中的数据可能会在合并之后出现缺失
        ''' </summary>
        ''' <param name="multiple"></param>
        ''' <param name="unique"></param>
        ''' <returns></returns>
        Public Shared Function Append(multiple As IEnumerable(Of EntityObject),
                                      unique As DataFrame,
                                      Optional allowNothing As Boolean = False) As IEnumerable(Of EntityObject)
            Return multiple _
                .Select(Function(query)
                            Dim id As String

                            If allowNothing Then
                                id = query.ID Or EmptyString
                            Else
                                id = query.ID
                            End If

                            If Not unique.entityList.ContainsKey(id) Then
                                Return query
                            Else
                                With unique.entityList(id)
                                    For Each [property] In .Properties
                                        query.Properties([property].Key) = [property].Value
                                    Next
                                End With

                                Return query
                            End If
                        End Function)
        End Function
    End Class
End Namespace
