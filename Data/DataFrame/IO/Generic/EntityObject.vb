#Region "Microsoft.VisualBasic::bdc955ebe386bca6a2002917c555c90b, Data\DataFrame\IO\Generic\EntityObject.vb"

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

    '     Class EntityObject
    ' 
    '         Properties: ID
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: Copy, GetIDList, (+4 Overloads) LoadDataSet, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace IO

    ''' <summary>
    ''' The object entity, <see cref="DynamicPropertyBase(Of String)"/>, <see cref="String"/>.
    ''' (有名称属性的表抽象对象)
    ''' </summary>
    Public Class EntityObject : Inherits Table
        Implements INamedValue

        ''' <summary>
        ''' This object identifier
        ''' </summary>
        ''' <returns></returns>
        <Column("ID")>
        Public Overridable Property ID As String Implements INamedValue.Key

        Sub New()
        End Sub

        Sub New(id$)
            Me.ID = id
            Me.Properties = New Dictionary(Of String, String)
        End Sub

        Sub New(id$, props As Dictionary(Of String, String))
            Me.ID = id
            Me.Properties = props
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(x As EntityObject)
            Call Me.New(x.ID, New Dictionary(Of String, String)(x.Properties))
        End Sub

        ''' <summary>
        ''' Copy prop[erty value
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function Copy() As EntityObject
            Return New EntityObject With {
                .ID = ID,
                .Properties = New Dictionary(Of String, String)(Properties)
            }
        End Function

        Public Overrides Function ToString() As String
            Return $"{ID} => ({Properties.Count}) {Properties.Keys.ToArray.GetJson}"
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="path$">
        ''' MetaScripting:
        ''' 
        ''' path: file path or directory scripting
        ''' 
        ''' filepath: file.csv or file.tsv
        ''' scripting: dir/* means all *.csv or *.tsv
        '''            dir/M* means all file match pattern M*.csv or M*.tsv
        ''' </param>
        ''' <param name="uidMap$"></param>
        ''' <param name="tsv"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadDataSet(path$,
                                           Optional ByRef uidMap$ = Nothing,
                                           Optional tsv As Boolean = False,
                                           Optional encoding As Encoding = Nothing) As IEnumerable(Of EntityObject)

            ' 2018-09-04 在原来的代码这里，空的path字符串是返回空集合的
            ' 为了保持和原来的代码的兼容性，在这里使用LastOrDefault来防止抛出错误
            If path.LastOrDefault = "*"c Then
                Dim data As New List(Of EntityObject)
                Dim dir$ = path.Trim("*"c)

                For Each file As String In ls - l - r - ("*.csv" Or "*.tsv".When(tsv)) <= dir
                    data += LoadDataSet(Of EntityObject)(file, uidMap, tsv, encoding:=encoding)
                Next

                Return data
            Else
                Return LoadDataSet(Of EntityObject)(path, uidMap, tsv, encoding:=encoding)
            End If
        End Function

        Public Shared Function GetIDList(path$, Optional uidMap$ = Nothing, Optional tsv As Boolean = False, Optional ignoreMapErrors As Boolean = False) As String()
            Dim table As File = If(tsv, File.LoadTsv(path), File.Load(path))
            Dim getIDsDefault = Function()
                                    Return table.Columns _
                                        .First _
                                        .Skip(1) _
                                        .ToArray
                                End Function

            If uidMap.StringEmpty Then
                ' 第一列的数据就是所需要的编号数据
                Return getIDsDefault()
            Else
                With table.Headers.IndexOf(uidMap)
                    If .ByRef = -1 AndAlso ignoreMapErrors Then
                        Return getIDsDefault()
                    Else
                        ' 当不忽略错误的时候，不存在的uidMap其index位置会出现越界的错误直接在这里报错
                        Return table.Columns(.ByRef) _
                            .Skip(1) _
                            .ToArray
                    End If
                End With
            End If
        End Function

        Public Shared Function LoadDataSet(Of T As EntityObject)(path$,
                                                                 Optional ByRef uidMap$ = Nothing,
                                                                 Optional tsv As Boolean = False,
                                                                 Optional encoding As Encoding = Nothing) As IEnumerable(Of T)
            If Not path.FileExists Then
#If DEBUG Then
                Call $"{path} is missing on your file system!".Warning
#End If
                Return {}
            End If

            If uidMap.StringEmpty Then
                If Not tsv Then
                    Dim first As New RowObject(path.ReadFirstLine)
                    uidMap = first.First
                Else
                    uidMap = path.ReadFirstLine.Split(ASCII.TAB).First
                End If
            End If

            If tsv Then
                Return path.LoadTsv(Of T)(
                    nameMaps:={{uidMap, NameOf(EntityObject.ID)}},
                    encoding:=encoding
                )
            Else
                Return path.LoadCsv(Of T)(
                    explicit:=False,
                    maps:={
                        {uidMap, NameOf(EntityObject.ID)}
                    },
                    encoding:=encoding
                )
            End If
        End Function

        Public Shared Function LoadDataSet(Of T As EntityObject)(stream As File, Optional ByRef uidMap$ = Nothing) As IEnumerable(Of T)
            Dim map As New Dictionary(Of String, String) From {
                {uidMap Or stream(0, 0).AsDefault, NameOf(EntityObject.ID)}
            }
            Return stream.AsDataSource(Of T)(explicit:=False, maps:=map)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadDataSet(stream As File, Optional ByRef uidMap$ = Nothing) As IEnumerable(Of EntityObject)
            Return LoadDataSet(Of EntityObject)(stream, uidMap)
        End Function
    End Class
End Namespace
