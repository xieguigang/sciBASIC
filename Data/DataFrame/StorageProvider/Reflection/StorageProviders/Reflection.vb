#Region "Microsoft.VisualBasic::be062f63f1405e77c3491c5cc2b65267, Data\DataFrame\StorageProvider\Reflection\StorageProviders\Reflection.vb"

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

    '   Total Lines: 343
    '    Code Lines: 224 (65.31%)
    ' Comment Lines: 81 (23.62%)
    '    - Xml Docs: 86.42%
    ' 
    '   Blank Lines: 38 (11.08%)
    '     File Size: 16.51 KB


    '     Module Reflector
    ' 
    '         Function: Convert, CreateRowBuilder, doSave, ExportAsPropertyAttributes, GetDataFrameworkTypeSchema
    '                   GetsRowData, Load, LoadDataToObject, Save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON
Imports TableSchema = Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.SchemaProvider

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
            Dim Schema As TableSchema = TableSchema.CreateObjectInternal(type, Explicit).CopyReadDataFromObject
            Dim cols = LinqAPI.Exec(Of NamedValue(Of Type)) _
 _
                () <= From columAttr As Column
                      In Schema.Columns
                      Let ptype As Type = columAttr.BindProperty.PropertyType
                      Select New NamedValue(Of Type) With {
                          .Name = columAttr.Name,
                          .Value = ptype
                      }

            Dim array = From columnItem As CollectionColumn
                        In Schema.CollectionColumns
                        Let ptype As Type = columnItem.BindProperty.PropertyType
                        Select New NamedValue(Of Type) With {
                            .Name = columnItem.Name,
                            .Value = ptype
                        }

            Dim table As Dictionary(Of String, Type) = cols _
                .Join(array) _
                .ToDictionary(Function(col) col.Name,
                              Function(col)
                                  Return col.Value
                              End Function)
            Return table
        End Function
#End If

        Public Function CreateRowBuilder(Of T)(Optional strict As Boolean = False) As RowBuilder
            Dim type As Type = GetType(T)
            Dim schema As TableSchema = TableSchema.CreateObjectInternal(type, strict).CopyWriteDataToObject
            Dim rowBuilder As New RowBuilder(schema)

            Return rowBuilder
        End Function

        ''' <summary>
        ''' 将Csv文件加载至一个目标集合之中以完成数据从文件之中的读取操作
        ''' </summary>
        ''' <param name="csv"></param>
        ''' <param name="type"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <Extension>
        Public Function LoadDataToObject(csv As DataFrame, type As Type,
                                         Optional strict As Boolean = False,
                                         Optional metaBlank As String = "",
                                         Optional parallel As Boolean = True,
                                         Optional silent As Boolean = False) As IEnumerable(Of Object)

            Dim schema As TableSchema = TableSchema.CreateObjectInternal(type, strict).CopyWriteDataToObject
            Dim rowBuilder As New RowBuilder(schema)

#If DEBUG Then
            parallel = False
#End If

            Dim sequence = csv.table _
                .SeqIterator _
                .Populate(parallel)
            Dim buf = From line As SeqValue(Of RowObject)
                      In sequence
                      Select lineNumber = line.i,
                          filledObject = Activator.CreateInstance(type),
                          row = line.value

            Call rowBuilder.IndexOf(csv)
            Call rowBuilder.SolveReadOnlyMetaConflicts(silent)

            ' 顺序需要一一对应，所以在最后这里进行了一下排序操作
            Dim LQuery = From item
                         In buf.Populate(parallel)
                         Select item.lineNumber,
                             item.row,
                             data = rowBuilder.FillData(item.row, item.filledObject, metaBlank)
                         Order By lineNumber Ascending

            Return LQuery.Select(Function(x) x.data)
        End Function

        ''' <summary>
        ''' 从文件之中读取数据并转换为对象数据
        ''' </summary>
        ''' <typeparam name="TClass"></typeparam>
        ''' <param name="df"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        ''' <remarks>在这里查找所有具有写属性的属性对象即可</remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Convert(Of TClass As Class)(df As DataFrame,
                                                    Optional strict As Boolean = True,
                                                    Optional metaBlank$ = "",
                                                    Optional silent As Boolean = False) As IEnumerable(Of TClass)

            Return df.LoadDataToObject(GetType(TClass), strict, metaBlank, silent:=silent).As(Of TClass)
        End Function

        ''' <summary>
        ''' Method for load a csv data file into a specific type of object collection.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="Explicit">
        ''' 当本参数值为False的时候，所有的简单属性值都将被解析出来，而忽略掉其是否带有
        ''' <see cref="Csv.StorageProvider.Reflection.ColumnAttribute"></see>自定义属性
        ''' </param>
        ''' <param name="path"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Load(Of T As Class)(path$,
                                            Optional Explicit As Boolean = True,
                                            Optional encoding As Encoding = Nothing,
                                            Optional fast As Boolean = False,
                                            Optional maps As Dictionary(Of String, String) = Nothing,
                                            Optional mute As Boolean = False,
                                            Optional metaBlank As String = "",
                                            Optional skipWhile As NamedValue(Of Func(Of String, Boolean)) = Nothing,
                                            Optional simpleRowIterators As Boolean = True,
                                            Optional tsv As Boolean = False) As IEnumerable(Of T)
            If Not path.FileExists Then
                ' 空文件
                Call $"Csv file ""{path.ToFileURL}"" is empty!".Warning
                Return {}
            Else
                Call "Load data from filestream....".__DEBUG_ECHO(mute:=mute)
            End If

            Dim buffer As IEnumerable(Of T)
            ' read csv data
            Dim reader As DataFrame = IO.DataFrame.Load(path, encoding, fast, skipWhile,
                                                        simpleRowIterators:=simpleRowIterators,
                                                        tsv:=tsv)

            If Not maps Is Nothing Then
                ' 改变列的名称映射以方便进行反序列化数据加载
                Call reader.ChangeMapping(maps)
            End If

            Call $"Reflector load data into type {GetType(T).FullName}".__DEBUG_ECHO(mute:=mute)
            buffer = Reflector.Convert(Of T)(reader, Explicit, metaBlank, silent:=mute)
            Call "[Job Done!]".__DEBUG_ECHO(mute:=mute)

            Return buffer
        End Function

        ''' <summary>
        ''' Save the specifc type object collection into the csv data file.(将目标对象数据的集合转换为Csv文件已进行数据保存操作)
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        ''' <remarks>查找所有具备读属性的属性值</remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Iterator Function GetsRowData(source As IEnumerable(Of Object), type As Type,
                        Optional strict As Boolean = True,
                        Optional maps As Dictionary(Of String, String) = Nothing,
                        Optional parallel As Boolean = True,
                        Optional metaBlank As String = "",
                        Optional reorderKeys As Integer = 0,
                        Optional layout As Dictionary(Of String, Integer) = Nothing) As IEnumerable(Of RowObject)

            For Each row As RowObject In doSave(source, type, strict, Nothing, metaBlank,
                                                maps:=maps,
                                                parallel:=parallel,
                                                reorderKeys:=reorderKeys,
                                                layout:=layout)
                Yield row
            Next
        End Function

        ''' <summary>
        ''' Save the specifc type object collection into the csv data file.(将目标对象数据的集合转换为Csv文件已进行数据保存操作)
        ''' </summary>
        ''' <param name="objSource"></param>
        ''' <param name="strict"></param>
        ''' <param name="schemaOut">请注意，Key是Csv文件之中的标题，不是属性名称了</param>
        ''' <returns></returns>
        ''' <remarks>查找所有具备读属性的属性值</remarks>
        Public Iterator Function doSave(objSource As IEnumerable,
                                          typeDef As Type,
                                         strict As Boolean,
                                        schemaOut As Dictionary(Of String, Type),
                               Optional metaBlank As String = "",
                               Optional maps As Dictionary(Of String, String) = Nothing,
                               Optional parallel As Boolean = True,
                               Optional reorderKeys As Integer = 0,
                               Optional layout As Dictionary(Of String, Integer) = Nothing,
                               Optional numFormat$ = Nothing) As IEnumerable(Of RowObject)

            ' 结束迭代器，防止Linq表达式重新计算
            Dim source As Object() = objSource.ToVector
            Dim schema As TableSchema = TableSchema.CreateObjectInternal(typeDef, strict).CopyReadDataFromObject
            Dim rowWriter As RowWriter = New RowWriter(schema, metaBlank, layout) _
                .CacheIndex(source, reorderKeys)

            schemaOut = rowWriter _
                .columns _
                .ToDictionary(Function(x) x.Name,
                              Function(x)
                                  Return x.BindProperty.PropertyType
                              End Function)

            Dim title As RowObject = rowWriter.GetRowNames(maps).Join(rowWriter.GetMetaTitles)

            Yield title

            If Not rowWriter.metaRow Is Nothing Then
                ' 只读属性会和字典属性产生冲突
                Dim valueType As Type = rowWriter.metaRow _
                                                 .Dictionary _
                                                 .GenericTypeArguments _
                                                 .Last
                Dim key$ = Nothing

                Try
                    For Each key$ In rowWriter.GetMetaTitles
                        Call schemaOut.Add(key, valueType)
                    Next
                Catch ex As Exception
                    Dim msg = $"key:='{key}', keys:={schemaOut.Keys.AsEnumerable.GetJson}, metaKeys:={rowWriter.GetMetaTitles.GetJson}"
                    ex = New Exception(msg, ex)
                    Throw ex
                End Try
            End If

            ' 为了保持对象之间的顺序的一致性，在这里不能够使用并行查询
            Dim LQuery As IEnumerable(Of RowObject) =
                From row As Object
                In source
                Let createdRow As RowObject = If(
                    row Is Nothing,
                    New RowObject,
                    rowWriter.ToRow(row, numFormat))
                Select createdRow

            If parallel Then
                For Each row As RowObject In LQuery.AsParallel
                    Yield row
                Next
            Else
                For Each row As RowObject In LQuery
                    Yield row
                Next
            End If
        End Function

        ''' <summary>
        ''' Save the specifc type object collection into the csv data file.
        ''' (将目标对象数据的集合转换为Csv文件已进行数据保存操作，非并行化的以保持数据原有的顺序)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="strict"></param>
        ''' <param name="schemaOut">``ByRef``反向输出的Schema参数</param>
        ''' <returns></returns>
        ''' <remarks>查找所有具备读属性的属性值</remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Save(Of T)(source As IEnumerable(Of T),
                                   Optional strict As Boolean = True,
                                   Optional metaBlank As String = "",
                                   Optional maps As Dictionary(Of String, String) = Nothing,
                                   Optional parallel As Boolean = True,
                                   Optional ByRef schemaOut As Dictionary(Of String, Type) = Nothing,
                                   Optional reorderKeys As Integer = 0,
                                   Optional numFormat$ = Nothing) As File

            Return Reflector.doSave(
                source, GetType(T), strict,
                schemaOut,
                metaBlank,
                maps,
                parallel,
                reorderKeys:=reorderKeys,
                numFormat:=numFormat
            ).DataFrame
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

            Dim df As File = Save(source, Explicit)
            Dim title As RowObject = df.First
            Dim __pCache As Integer() = title.Sequence.ToArray

            Dim buf As List(Of Dictionary(Of String, String)) =
                LinqAPI.MakeList(Of Dictionary(Of String, String)) <=
                    From rowL As IO.RowObject
                    In df.Skip(1).AsParallel
                    Select (From p As Integer
                            In __pCache
                            Select key = title(CInt(p)),
                                value = rowL(CInt(p))) _
                                  .ToDictionary(Function(x) x.key,
                                                Function(x)
                                                    Return x.value
                                                End Function)
            Return buf
        End Function
    End Module
End Namespace
