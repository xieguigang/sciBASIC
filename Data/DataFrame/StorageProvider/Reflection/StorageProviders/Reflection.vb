#Region "Microsoft.VisualBasic::2fec28dbd49c066ec2c8cef0b7ad50e9, ..\visualbasic_App\DocumentFormats\VB_DataFrame\VB_DataFrame\StorageProvider\Reflection\StorageProviders\Reflection.vb"

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

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.DataImports
Imports Microsoft.VisualBasic.Data.csv.DocumentStream
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace StorageProvider.Reflection

    ''' <summary>
    ''' The dynamics reflection operations on Csv data source services.
    ''' </summary>
    ''' <remarks></remarks>
    Public Module Reflector

#If NET_40 = 0 Then

        ''' <summary>
        ''' Returns the type schema as ``{columnName, type}``, using for the cytoscape software
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="Explicit"></param>
        ''' <returns></returns>
        <Extension> Public Function GetDataFrameworkTypeSchema(type As Type, Optional Explicit As Boolean = True) As Dictionary(Of String, Type)
            Dim Schema As SchemaProvider = SchemaProvider.CreateObject(type, Explicit).CopyReadDataFromObject
            Dim cols = LinqAPI.Exec(Of NamedValue(Of Type)) <=
                From columAttr As Column
                In Schema.Columns
                Select New NamedValue(Of Type) With {
                    .Name = columAttr.Name,
                    .x = columAttr.BindProperty.PropertyType
                }
            Dim array = From columnItem As CollectionColumn
                        In Schema.CollectionColumns
                        Select New NamedValue(Of Type) With {
                            .Name = columnItem.Name,
                            .x = columnItem.BindProperty.PropertyType
                        }
            Dim hash As Dictionary(Of String, Type) =
                cols.Join(array).ToDictionary(Function(x) x.Name,
                                              Function(x) x.x)
            Return hash
        End Function
#End If

        ''' <summary>
        ''' 将Csv文件加载至一个目标集合之中以完成数据从文件之中的读取操作
        ''' </summary>
        ''' <param name="csv"></param>
        ''' <param name="type"></param>
        ''' <param name="explicit"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <Extension>
        Public Function LoadDataToObject(csv As DataFrame, type As Type, Optional explicit As Boolean = False) As IEnumerable(Of Object)
            Dim schema As SchemaProvider = SchemaProvider.CreateObject(type, explicit).CopyWriteDataToObject
            Dim rowBuilder As New RowBuilder(schema)
            Dim buf = From line As SeqValue(Of RowObject)
                      In csv._innerTable.SeqIterator.AsParallel
                      Select LineNumber = line.i,
                          FilledObject = Activator.CreateInstance(type),
                          row = line.obj

            Call rowBuilder.Indexof(csv)

            Dim LQuery = From item
                         In buf.AsParallel
                         Select item.LineNumber,
                             item.row,
                             Data = rowBuilder.FillData(item.row, item.FilledObject)
                         Order By LineNumber Ascending  ' 顺序需要一一对应，所以在最后这里进行了一下排序操作

            Return LQuery.Select(Function(x) x.Data)
        End Function

        ''' <summary>
        ''' 从文件之中读取数据并转换为对象数据
        ''' </summary>
        ''' <typeparam name="ItemType"></typeparam>
        ''' <param name="DataFrame"></param>
        ''' <param name="explicit"></param>
        ''' <returns></returns>
        ''' <remarks>在这里查找所有具有写属性的属性对象即可</remarks>
        Public Function Convert(Of ItemType As Class)(DataFrame As DataFrame, Optional explicit As Boolean = True) As List(Of ItemType)
            Dim type As Type = GetType(ItemType)
            Return DataFrame.LoadDataToObject(type, explicit) _
                .ToList(Function(x) DirectCast(x, ItemType))
        End Function

        ''' <summary>
        ''' Method for load a csv data file into a specific type of object collection.
        ''' </summary>
        ''' <typeparam name="ItemType"></typeparam>
        ''' <param name="Explicit">当本参数值为False的时候，所有的简单属性值都将被解析出来，而忽略掉其是否带有<see cref="Csv.StorageProvider.Reflection.ColumnAttribute"></see>自定义属性</param>
        ''' <param name="path"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Load(Of ItemType As Class)(path As String,
                                                   Optional Explicit As Boolean = True,
                                                   Optional encoding As System.Text.Encoding = Nothing,
                                                   Optional fast As Boolean = False,
                                                   Optional maps As Dictionary(Of String, String) = Nothing) As List(Of ItemType)
            If Not path.FileExists Then '空文件
                Call $"Csv file ""{path.ToFileURL}"" is empty!".__DEBUG_ECHO
                Return New List(Of ItemType)
            Else
                Call "Load data from filestream....".__DEBUG_ECHO
            End If

            Dim reader As DataFrame = DocumentStream.DataFrame.Load(path, encoding, fast)  ' read csv data

            If Not maps Is Nothing Then
                Call reader.ChangeMapping(maps)  ' 改变列的名称映射以方便进行反序列化数据加载
            End If

            Call $"Reflector load data into type {GetType(ItemType).FullName}".__DEBUG_ECHO
            Dim bufs As List(Of ItemType) = Reflector.Convert(Of ItemType)(reader, Explicit)
            Call "[Job Done!]".__DEBUG_ECHO
            Return bufs
        End Function

        ''' <summary>
        ''' Save the specifc type object collection into the csv data file.(将目标对象数据的集合转换为Csv文件已进行数据保存操作)
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="Explicit"></param>
        ''' <returns></returns>
        ''' <remarks>查找所有具备读属性的属性值</remarks>
        Public Function Save(source As IEnumerable(Of Object),
                             Optional Explicit As Boolean = True,
                             Optional maps As Dictionary(Of String, String) = Nothing,
                             Optional parallel As Boolean = True) As DocumentStream.File
            Dim type As Type = source.First.GetType
            Return __save(source, type, Explicit, maps:=maps, parallel:=parallel)
        End Function

        ''' <summary>
        ''' Save the specifc type object collection into the csv data file.(将目标对象数据的集合转换为Csv文件已进行数据保存操作)
        ''' </summary>
        ''' <param name="___source"></param>
        ''' <param name="Explicit"></param>
        ''' <param name="schemaOut">请注意，Key是Csv文件之中的标题，不是属性名称了</param>
        ''' <returns></returns>
        ''' <remarks>查找所有具备读属性的属性值</remarks>
        Public Function __save(___source As IEnumerable,
                               typeDef As Type,
                               Explicit As Boolean,
                               Optional metaBlank As String = "",
                               Optional maps As Dictionary(Of String, String) = Nothing,
                               Optional parallel As Boolean = True,
                               Optional ByRef schemaOut As Dictionary(Of String, Type) = Nothing) As DocumentStream.File

            Dim source As Object() = ___source.ToVector
            Dim Schema As SchemaProvider =
                SchemaProvider.CreateObject(typeDef, Explicit).CopyReadDataFromObject
            Dim RowWriter As RowWriter = New RowWriter(Schema, metaBlank).CacheIndex(source)
            Dim LQuery As List(Of RowObject)
            Dim array As IEnumerable(Of Object) =
                If(parallel,
                DirectCast(source.AsParallel, IEnumerable(Of Object)),
                DirectCast(source, IEnumerable(Of Object)))

            schemaOut = RowWriter.Columns.ToDictionary(
                Function(x) x.Name,
                Function(x) x.BindProperty.PropertyType)
            LQuery = LinqAPI.MakeList(Of RowObject) <=
 _
                From row As Object
                In array
                Where Not row Is Nothing
                Let createdRow As RowObject =
                    RowWriter.ToRow(row)
                Select createdRow  ' 为了保持对象之间的顺序的一致性，在这里不能够使用并行查询

            Dim title As RowObject =
                RowWriter.GetRowNames(maps).Join(RowWriter.GetMetaTitles)
            Dim dataFrame As New File(title + LQuery)

            If Not RowWriter.MetaRow Is Nothing Then
                Dim valueType As Type =
                    RowWriter.MetaRow.Dictionary.GenericTypeArguments.Last
                For Each key As String In RowWriter.GetMetaTitles
                    Call schemaOut.Add(key, valueType)
                Next
            End If

            Return dataFrame
        End Function

        ''' <summary>
        ''' Save the specifc type object collection into the csv data file.
        ''' (将目标对象数据的集合转换为Csv文件已进行数据保存操作，非并行化的以保持数据原有的顺序)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="explicit"></param>
        ''' <param name="schemaOut">``ByRef``反向输出的Schema参数</param>
        ''' <returns></returns>
        ''' <remarks>查找所有具备读属性的属性值</remarks>
        Public Function Save(Of T)(source As IEnumerable(Of T),
                                   Optional explicit As Boolean = True,
                                   Optional metaBlank As String = "",
                                   Optional maps As Dictionary(Of String, String) = Nothing,
                                   Optional parallel As Boolean = True,
                                   Optional ByRef schemaOut As Dictionary(Of String, Type) = Nothing) As DocumentStream.File
            Dim type As Type = GetType(T)
            Dim file As File = __save(source, type, explicit, metaBlank, maps, parallel, schemaOut)
            Return file
        End Function

        ''' <summary>
        ''' 将数据集合导出为键值对，以方便其他操作
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="Explicit"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function ExportAsPropertyAttributes(Of T)(
                                    source As IEnumerable(Of T),
                                    Optional Explicit As Boolean = True) As List(Of Dictionary(Of String, String))

            Dim df As DocumentStream.File = Save(source, Explicit)
            Dim TitleRow As DocumentStream.RowObject = df.First
            Dim __pCache As Integer() = TitleRow.Sequence.ToArray

            Dim buf As List(Of Dictionary(Of String, String)) =
                LinqAPI.MakeList(Of Dictionary(Of String, String)) <=
                    From rowL As DocumentStream.RowObject
                    In df.Skip(1).AsParallel
                    Select (From p As Integer
                            In __pCache
                            Select key = TitleRow(CInt(p)),
                                value = rowL(CInt(p))) _
                                  .ToDictionary(Function(x) x.key,
                                                Function(x) x.value)
            Return buf
        End Function

        Public Enum OperationTypes
            ''' <summary>
            ''' 需要从对象之中读取数据，需要将数据写入文件的时候使用
            ''' </summary>
            ''' <remarks></remarks>
            ReadDataFromObject
            ''' <summary>
            ''' 需要相对象写入数据，从文件之中加载数据的时候使用
            ''' </summary>
            ''' <remarks></remarks>
            WriteDataToObject
        End Enum
    End Module
End Namespace
