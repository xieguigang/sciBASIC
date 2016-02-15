Imports System.Reflection
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DataImports
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream

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
        Public Function Convert(Of ItemType As Class)(DataFrame As Csv.DocumentStream.DataFrame, Optional explicit As Boolean = True) As List(Of ItemType)
            Dim Type As System.Type = GetType(ItemType)
            Dim Schema = SchemaProvider.CreateObject(Of ItemType)(explicit).CopyWriteDataToObject
            Dim RowBuilder As New RowBuilder(Schema)
            Dim CreateObjects = (From LineNumber As Integer
                                 In DataFrame._innerTable.Count.Sequence.AsParallel
                                 Select LineNumber,
                                     FilledObject = Activator.CreateInstance(Of ItemType))
            Dim ChunkSource = (From line In CreateObjects
                               Select LineNumber = line.LineNumber,
                                   row = DataFrame._innerTable(line.LineNumber),
                                   line.FilledObject).ToArray

            Call RowBuilder.Indexof(DataFrame)

            Dim LQuery = (From item In ChunkSource.AsParallel
                          Select item.LineNumber,
                              item.row,
                              Data = RowBuilder.FillData(Of ItemType)(item.row, item.FilledObject)
                          Order By LineNumber Ascending)
            Dim Table = (From row In LQuery Select row.Data).ToList
            Return Table '顺序需要一一对应
        End Function

        ''' <summary>
        ''' Method for load a csv data file into a specific type of object collection.
        ''' </summary>
        ''' <typeparam name="ItemType"></typeparam>
        ''' <param name="Explicit">当本参数值为False的时候，所有的简单属性值都将被解析出来，而忽略掉其是否带有<see cref="Csv.StorageProvider.Reflection.ColumnAttribute"></see>自定义属性</param>
        ''' <param name="Path"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Load(Of ItemType As Class)(Path As String, Optional Explicit As Boolean = True, Optional encoding As System.Text.Encoding = Nothing, Optional fast As Boolean = False) As List(Of ItemType)
            Call "Load data from filestream....".__DEBUG_ECHO
            If Not Path.FileExists Then '空文件
                Call $"Csv file ""{Path.ToFileURL}"" is empty!".__DEBUG_ECHO
                Return New List(Of ItemType)
            End If

            Dim reader As DataFrame = DocumentStream.DataFrame.Load(Path, encoding, fast)
            Call $"Reflector load data into type {GetType(ItemType).FullName}".__DEBUG_ECHO
            Dim ChunkBuffer As List(Of ItemType) = Reflection.Reflector.Convert(Of ItemType)(reader, Explicit)
            Call "[Job Done!]".__DEBUG_ECHO
            Return ChunkBuffer
        End Function

        'Public ReadOnly Property FillObjectMethods As Dictionary(Of Reflection.ColumnAttribute.Type, System.Action(Of PropertyInfo, Object, String())) =
        '    New Dictionary(Of ColumnAttribute.Type, Action(Of PropertyInfo, Object, String())) From {
        '        {ColumnAttribute.Type.Bool, Sub(bindProperty As System.Reflection.PropertyInfo, FilledObject As Object, strValues As String()) _
        '                                        Call bindProperty.SetValue(FilledObject, (From strData As String In strValues Select CType(strData, Boolean)).ToArray, Nothing)},
        '        {ColumnAttribute.Type.DateTime, Sub(bindProperty As System.Reflection.PropertyInfo, FilledObject As Object, strValues As String()) _
        '                                        Call bindProperty.SetValue(FilledObject, (From strData As String In strValues Select CType(strData, DateTime)).ToArray, Nothing)},
        '        {ColumnAttribute.Type.Double, Sub(bindProperty As System.Reflection.PropertyInfo, FilledObject As Object, strValues As String()) _
        '                                        Call bindProperty.SetValue(FilledObject, (From strData As String In strValues Select Val(strData)).ToArray, Nothing)},
        '        {ColumnAttribute.Type.Integer, Sub(bindProperty As System.Reflection.PropertyInfo, FilledObject As Object, strValues As String()) _
        '                                        Call bindProperty.SetValue(FilledObject, (From strData As String In strValues Select CType(strData, Integer)).ToArray, Nothing)},
        '        {ColumnAttribute.Type.Long, Sub(bindProperty As System.Reflection.PropertyInfo, FilledObject As Object, strValues As String()) _
        '                                        Call bindProperty.SetValue(FilledObject, (From strData As String In strValues Select CType(strData, Long)).ToArray, Nothing)},
        '        {ColumnAttribute.Type.String, Sub(bindProperty As System.Reflection.PropertyInfo, FilledObject As Object, strValues As String()) _
        '                                        Call bindProperty.SetValue(FilledObject, strValues, Nothing)}}

        ''' <summary>
        ''' Save the specifc type object collection into the csv data file.(将目标对象数据的集合转换为Csv文件已进行数据保存操作) 
        ''' </summary>
        ''' <param name="Collection"></param>
        ''' <param name="Explicit"></param>
        ''' <returns></returns>
        ''' <remarks>查找所有具备读属性的属性值</remarks>
        Public Function Save(Collection As Generic.IEnumerable(Of Object), Optional Explicit As Boolean = True) As File
            Dim Type As System.Type = Collection.First.GetType
            Return __save(Collection, Type, Explicit)
        End Function

        ''' <summary>
        ''' Save the specifc type object collection into the csv data file.(将目标对象数据的集合转换为Csv文件已进行数据保存操作) 
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="Explicit"></param>
        ''' <returns></returns>
        ''' <remarks>查找所有具备读属性的属性值</remarks>
        Private Function __save(source As IEnumerable(Of Object), typeDef As Type, Explicit As Boolean) As File
            Dim CsvData As File = New File
            Dim Schema As SchemaProvider = SchemaProvider.CreateObject(typeDef, Explicit).CopyReadDataFromObject
            Dim RowWriter As RowWriter = New RowWriter(Schema).CacheIndex(source)
            Dim LQuery As RowObject() = (From itmRow As Object In source.AsParallel
                                         Where Not itmRow Is Nothing
                                         Let createdRow As RowObject = RowWriter.ToRow(itmRow)
                                         Select createdRow).ToArray '为了保持对象之间的顺序的一致性，在这里不能够使用并行查询

            Call CsvData.Add(RowWriter.GetRowNames.Join(RowWriter.GetMetaTitles(source.FirstOrDefault)))
            Call CsvData.AppendRange(LQuery)

            Return CsvData
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
        Public Function Save(Of T)(source As IEnumerable(Of T), Optional explicit As Boolean = True) As File
            Dim Type As Type = GetType(T)
            Dim doc As File = __save(source, Type, explicit)
            Return doc
        End Function

        ''' <summary>
        ''' 将数据集合导出为键值对，以方便其他操作 
        ''' </summary>
        ''' <typeparam name="ItemType"></typeparam>
        ''' <param name="Collection"></param>
        ''' <param name="Explicit"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function ExportAsPropertyAttributes(Of ItemType)(
                        Collection As Generic.IEnumerable(Of ItemType),
                        Optional Explicit As Boolean = True) As List(Of Dictionary(Of String, String))

            Dim Csv As Csv.DocumentStream.File = Save(Collection, Explicit)
            Dim TitleRow As RowObject = Csv.First
            Dim __pCache As Integer() = TitleRow.Sequence
            Dim ChunkBuffer = (From rowL As RowObject In Csv.Skip(1).AsParallel
                               Select (From p As Integer
                                       In __pCache
                                       Select key = TitleRow(p),
                                           value = rowL(p)).ToDictionary(Function(x) x.key, elementSelector:=Function(x) x.value)).ToList
            Return ChunkBuffer
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