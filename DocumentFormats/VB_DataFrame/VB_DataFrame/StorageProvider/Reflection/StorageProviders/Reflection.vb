#Region "Microsoft.VisualBasic::3a5e02e8bbc63cfa0565902d6f50857e, ..\VB_DataFrame\StorageProvider\Reflection\StorageProviders\Reflection.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DataImports
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.Language

Namespace StorageProvider.Reflection

    ''' <summary>
    ''' The dynamics reflection operations on Csv data source services.
    ''' </summary>
    ''' <remarks></remarks>
    Public Module Reflector

#If NET_40 = 0 Then

        <Extension> Public Function GetDataFrameworkTypeSchema(TypeInfo As Type, Optional Explicit As Boolean = True) As Dictionary(Of String, Type)
            Dim Schema As SchemaProvider = SchemaProvider.CreateObject(TypeInfo, Explicit).CopyReadDataFromObject
            Dim ColumnSchema = (From columAttr As Column
                                In Schema.Columns
                                Select columAttr.Name,
                                    columAttr.BindProperty.PropertyType).ToArray
            Dim ArrayColumnSchema = (From columnItem As CollectionColumn
                                     In Schema.CollectionColumns
                                     Select columnItem.Name,
                                         columnItem.BindProperty.PropertyType).ToArray
            Dim ChunkData = ColumnSchema.Join(ArrayColumnSchema)
            Dim DictData As Dictionary(Of String, Type) =
                ChunkData.ToDictionary(Function(item) item.Name, elementSelector:=Function(item) item.PropertyType)
            Return DictData
        End Function
#End If

        ''' <summary>
        ''' 将Csv文件加载至一个目标集合之中以完成数据从文件之中的读取操作
        ''' </summary>
        ''' <param name="CsvData"></param>
        ''' <param name="TypeInfo"></param>
        ''' <param name="explicit"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LoadDataToObject(CsvData As Csv.DocumentStream.DataFrame, TypeInfo As System.Type, Optional explicit As Boolean = True) As Object()
            'Dim NewTable As New List(Of Object)
            'Dim Ordinal As Integer = 0
            'Dim Schema = Csv.StorageProvider.ComponentModels.SchemaProvider.CreateObject(TypeInfo, explicit).CopyWriteDataToObject
            'Dim ColumnSchema = (From columnItem As ComponentModels.Column In Schema.Columns
            '                    Let p = CsvData.GetOrdinal(columnItem.Name)
            '                    Where Not p < 0
            '                    Select handle = p, columnItem).ToArray
            'Dim ArrayColumnSchema = (From columnItem As Csv.StorageProvider.ComponentModels.CollectionColumn In Schema.CollectionColumns
            '                         Let p = CsvData.GetOrdinal(columnItem.Name)
            '                         Where Not p < 0
            '                         Select handle = p, columnItem).ToArray
            'While CsvData.Read

            '    Dim FilledObject As Object = Activator.CreateInstance(TypeInfo)

            '    For Each Column In ColumnSchema
            '        Ordinal = Column.handle
            '        If Ordinal >= 0 Then
            '            Dim value = CsvData.GetValue(Ordinal)
            '            Dim objectValue = Column.columnItem.ColumnDefine.Convert(value)
            '            Column.columnItem.BindProperty.SetValue(FilledObject, objectValue, Nothing)
            '        End If
            '    Next

            '    For Each Column In ArrayColumnSchema
            '        Ordinal = Column.handle
            '        If Ordinal >= 0 Then
            '            Dim value As String = CsvData.GetValue(Ordinal)
            '            Dim objectValue = Column.columnItem.CollectionColumn.CreateObject(value)
            '            Dim valueArray = objectValue.ToArray(Function(str) Scripting.CTypeDynamic(str, Column.columnItem.CollectionColumn.ReflectedType))

            '            Call Column.columnItem.BindProperty.SetValue(FilledObject, objectValue)
            '        End If
            '    Next

            '    Call NewTable.Add(FilledObject)
            'End While

            'Return NewTable.ToArray
            Throw New NotImplementedException
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
            Dim schema As SchemaProvider =
                SchemaProvider.CreateObject(Of ItemType)(explicit).CopyWriteDataToObject
            Dim rowBuilder As New RowBuilder(schema)
            Dim CreateObjects = (From LineNumber As Integer
                                 In DataFrame._innerTable.Sequence.AsParallel
                                 Select LineNumber,
                                     FilledObject = Activator.CreateInstance(Of ItemType))
            Dim buf = (From line In CreateObjects
                       Select LineNumber = line.LineNumber,
                           row = DataFrame._innerTable(line.LineNumber),
                           line.FilledObject).ToArray

            Call rowBuilder.Indexof(DataFrame)

            Dim LQuery = (From item In buf.AsParallel
                          Select item.LineNumber,
                              item.row,
                              Data = rowBuilder.FillData(Of ItemType)(item.row, item.FilledObject)
                          Order By LineNumber Ascending)
            Dim Table = (From row In LQuery Select row.Data).ToList
            Return Table '顺序需要一一对应
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
            Call "Load data from filestream....".__DEBUG_ECHO
            If Not path.FileExists Then '空文件
                Call $"Csv file ""{path.ToFileURL}"" is empty!".__DEBUG_ECHO
                Return New List(Of ItemType)
            End If

            Dim reader As DataFrame = DocumentStream.DataFrame.Load(path, encoding, fast)  ' read csv data

            If Not maps Is Nothing Then
                Call reader.ChangeMapping(maps)  ' 改变列的名称映射以方便进行反序列化数据加载
            End If

            Call $"Reflector load data into type {GetType(ItemType).FullName}".__DEBUG_ECHO
            Dim ChunkBuffer As List(Of ItemType) = Reflection.Reflector.Convert(Of ItemType)(reader, Explicit)
            Call "[Job Done!]".__DEBUG_ECHO
            Return ChunkBuffer
        End Function

        ''' <summary>
        ''' Save the specifc type object collection into the csv data file.(将目标对象数据的集合转换为Csv文件已进行数据保存操作)
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="Explicit"></param>
        ''' <returns></returns>
        ''' <remarks>查找所有具备读属性的属性值</remarks>
        Public Function Save(source As IEnumerable(Of Object), Optional Explicit As Boolean = True) As DocumentStream.File
            Dim type As System.Type = source.First.GetType
            Return __save(source, type, Explicit)
        End Function

        ''' <summary>
        ''' Save the specifc type object collection into the csv data file.(将目标对象数据的集合转换为Csv文件已进行数据保存操作)
        ''' </summary>
        ''' <param name="___source"></param>
        ''' <param name="Explicit"></param>
        ''' <returns></returns>
        ''' <remarks>查找所有具备读属性的属性值</remarks>
        Public Function __save(___source As IEnumerable,
                               typeDef As Type,
                               Explicit As Boolean,
                               Optional metaBlank As String = "") As DocumentStream.File
            Dim source As Object() = (From x As Object
                                      In ___source
                                      Select x).ToArray
            Dim Schema As SchemaProvider =
                SchemaProvider.CreateObject(typeDef, Explicit).CopyReadDataFromObject
            Dim RowWriter As RowWriter = New RowWriter(Schema, metaBlank).CacheIndex(source)
            Dim LQuery As List(Of RowObject) =
                LinqAPI.MakeList(Of RowObject) <= From itmRow As Object
                                                  In source.AsParallel
                                                  Where Not itmRow Is Nothing
                                                  Let createdRow As RowObject =
                                                      RowWriter.ToRow(itmRow)
                                                  Select createdRow  '为了保持对象之间的顺序的一致性，在这里不能够使用并行查询

            Dim title As RowObject = RowWriter.GetRowNames _
                .Join(RowWriter.GetMetaTitles(source.FirstOrDefault))
            Dim dataFrame As New File(title + LQuery)

            Return dataFrame
        End Function

        ''' <summary>
        ''' Save the specifc type object collection into the csv data file.
        ''' (将目标对象数据的集合转换为Csv文件已进行数据保存操作，非并行化的以保持数据原有的顺序)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="explicit"></param>
        ''' <returns></returns>
        ''' <remarks>查找所有具备读属性的属性值</remarks>
        Public Function Save(Of T)(source As IEnumerable(Of T),
                                   Optional explicit As Boolean = True,
                                   Optional metaBlank As String = "") As DocumentStream.File
            Dim Type As Type = GetType(T)
            Dim doc As DocumentStream.File = __save(source, Type, explicit, metaBlank)
            Return doc
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
