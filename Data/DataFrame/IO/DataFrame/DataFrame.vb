#Region "Microsoft.VisualBasic::c6945b994655d9e70180e4a9bab72d88, Data\DataFrame\IO\DataFrame\DataFrame.vb"

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

    '   Total Lines: 776
    '    Code Lines: 500 (64.43%)
    ' Comment Lines: 162 (20.88%)
    '    - Xml Docs: 82.10%
    ' 
    '   Blank Lines: 114 (14.69%)
    '     File Size: 31.42 KB


    '     Class DataFrame
    ' 
    '         Properties: Depth, FieldCount, Headers, HeadTitles, IDataRecord_Item
    '                     IsClosed, Item, Nrows, RecordsAffected, Rows
    '                     SchemaOridinal
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: __createTableVector, [Select], AddAttribute, AppendLine, ColumnRows
    '                   CreateDataSource, (+2 Overloads) CreateObject, createObjectInternal, csv, EnumerateData
    '                   EnumerateRowObjects, GetBoolean, GetByte, GetBytes, GetChar
    '                   GetChars, getColumnList, GetColumnVectors, GetData, GetDataTypeName
    '                   GetDateTime, GetDecimal, GetDouble, GetEnumerator, GetEnumerator2
    '                   GetFieldType, GetFloat, GetGuid, GetInt16, GetInt32
    '                   GetInt64, GetName, (+2 Overloads) GetOrdinal, GetOrdinalSchema, GetSchemaTable
    '                   GetString, GetValue, GetValueLambda, GetValues, IDataRecord_GetValue
    '                   IsDBNull, (+2 Overloads) Load, LoadDataSet, MeasureTypeSchema, Parse
    '                   Read, Slice, ToString
    ' 
    '         Sub: ChangeMapping, Close, CopyFrom, (+2 Overloads) Dispose, Initialize
    '              Reset
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Serialization.JSON
Imports ASCII = Microsoft.VisualBasic.Text.ASCII

Namespace IO

    ''' <summary>
    ''' The dynamics data frame object which its first line is not contains the but using for the title property.
    ''' </summary>
    ''' <remarks>
    ''' (第一行总是没有的，即本对象类型适用于第一行为列标题行的数据)
    ''' </remarks>
    Public Class DataFrame
        Implements ISchema
        Implements IDataReader
        Implements IDisposable
        Implements IEnumerable(Of DynamicObjectLoader)

        ''' <summary>
        ''' <see cref="current"></see>在<see cref="table"></see>之中的位置
        ''' </summary>
        ''' <remarks></remarks>
        Dim p% = -1
        Dim current As RowObject

        ''' <summary>
        ''' table data that contains no header row
        ''' </summary>
        Friend table As ICollection(Of RowObject)

        ''' <summary>
        ''' Using the first line of the csv row as the column headers in this csv file.
        ''' </summary>
        ''' <remarks></remarks>
        Protected columnList As HeaderSchema
        Protected typeSchema As Type()

        Public ReadOnly Property SchemaOridinal As Dictionary(Of String, Integer) Implements ISchema.SchemaOridinal
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return columnList.SchemaOridinal
            End Get
        End Property

        Public ReadOnly Property Rows As IEnumerable(Of RowObject)
            Get
                Return table
            End Get
        End Property

        Default Public ReadOnly Property Column(name As String) As String()
            Get
                Dim offset As Integer = GetOrdinal(name)

                If offset < 0 Then
                    Return Nothing
                Else
                    Return table.GetColumn(offset)
                End If
            End Get
        End Property

        Default Public ReadOnly Property Column(offset As Integer) As String()
            Get
                Return table.GetColumn(offset)
            End Get
        End Property

        ''' <summary>
        ''' get number of the row data
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Nrows As Integer
            Get
                Return table.Count
            End Get
        End Property

        Const FieldExists$ = "Required change column name mapping from `{0}` to `{1}`, but the column ``{1}`` is already exists in your file data!"

        ''' <summary>
        ''' ``Csv.Field -> <see cref="PropertyInfo.Name"/>``
        ''' </summary>
        ''' <param name="mapping">``{oldFieldName, newFieldName}``</param>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub ChangeMapping(mapping As Dictionary(Of String, String))
            Call columnList.ChangeMapping(mapping)
        End Sub

        ''' <summary>
        ''' 这个函数会返回目标列在schema之中的下标序列号
        ''' 如果已经存在了，则会直接返回下标号
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function AddAttribute(Name As String) As Integer
            Return columnList.AddAttribute(Name)
        End Function

        Public Function MeasureTypeSchema() As DataFrame
            Dim types As New List(Of Type)

            For Each col As String() In table.GetColumns
                types.Add(DataImports.SampleForType(col))
            Next

            typeSchema = types.ToArray

            Return Me
        End Function

        ''' <summary>
        ''' Get the lines data for the convinent data operation.(为了保持一致的顺序，这个函数是非并行化的)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateDataSource() As DynamicObjectLoader()
            Dim rowNumbers = table.Count.Sequence
            Dim LQuery As DynamicObjectLoader() = LinqAPI.Exec(Of DynamicObjectLoader) _
                                                                                       _
            () <=
                 _
                From i As Integer
                In rowNumbers.AsParallel
                Let line As RowObject = table(i)
                Select row = New DynamicObjectLoader With {
                    .lineNumber = i,
                    .RowData = line,
                    .Schema = Me.SchemaOridinal,
                    .dataFrame = Me
                }
                Order By row.lineNumber Ascending

            Return LQuery
        End Function

        Public Iterator Function EnumerateData() As IEnumerable(Of Dictionary(Of String, String))
            For Each row As RowObject In table
                Dim out As New Dictionary(Of String, String)

                For Each key In SchemaOridinal
                    Call out.Add(key.Key, row(key.Value))
                Next

                Yield out
            Next
        End Function

        Public Iterator Function EnumerateRowObjects() As IEnumerable(Of Object())
            For Each row As RowObject In table
                Yield row _
                    .Select(Function(str, i)
                                Return Scripting.CTypeDynamic(str, typeSchema(i))
                            End Function) _
                    .ToArray
            Next
        End Function

        ''' <summary>
        ''' The column headers in the csv file first row.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HeadTitles As String()
            Get
                Return columnList.Headers
            End Get
        End Property

        ''' <summary>
        ''' The column headers in the csv file first row.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Headers As RowObject
            Get
                Return New RowObject(columnList.Headers)
            End Get
        End Property

        Private ReadOnly Property Depth As Integer Implements IDataReader.Depth
            Get
                Return 0
            End Get
        End Property

        Private ReadOnly Property IsClosed As Boolean Implements IDataReader.IsClosed
            Get
                Return False
            End Get
        End Property

        Private ReadOnly Property RecordsAffected As Integer Implements IDataReader.RecordsAffected
            Get
                Return 0
            End Get
        End Property

        Private ReadOnly Property FieldCount As Integer Implements IDataRecord.FieldCount
            Get
                Return columnList.Count
            End Get
        End Property

        Private ReadOnly Property IDataRecord_Item(i As Integer) As Object Implements IDataRecord.Item
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return IDataRecord_GetValue(i)
            End Get
        End Property

        Public Overloads ReadOnly Property Item(name As String) As Object Implements IDataRecord.Item
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return IDataRecord_GetValue(GetOrdinal(name))
            End Get
        End Property

        Public Function Slice(index As IEnumerable(Of Integer)) As DataFrame
            Dim subset = Rows.ToArray.CopyOf(index.ToArray)
            Dim df As New DataFrame With {
                .columnList = columnList,
                .current = Nothing,
                .p = Nothing,
                .table = subset,
                .typeSchema = typeSchema
            }

            Return df
        End Function

        ''' <summary>
        ''' Convert this dataframe object as a csv document object
        ''' </summary>
        ''' <returns></returns>
        Public Function csv() As File
            Dim file As New File
            file += New RowObject(columnList.Headers)
            file += DirectCast(table, IEnumerable(Of RowObject))
            Return file
        End Function

        Protected Friend Sub New()
        End Sub

        Sub New(header As IEnumerable(Of String))
            columnList = New HeaderSchema(header)
            table = New List(Of RowObject)
        End Sub

        ''' <summary>
        ''' Create a new dataframe with column value assigned
        ''' </summary>
        ''' <param name="columns">
        ''' 只支持基础类型,不支持复杂类型,因为csv文件的单元格不适用于复杂数据类型的数据文本的存储
        ''' </param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(ParamArray columns As ArgumentReference())
            Call Initialize(ColumnRows(columns).AsList, Me)
        End Sub

        Public Function GetColumnVectors() As IEnumerable(Of String())
            Return table.GetColumns
        End Function

        Public Function AppendLine(row As IEnumerable(Of String)) As DataFrame
            Call table.Add(New RowObject(row))
            Return Me
        End Function

        ''' <summary>
        ''' transpose the columns vector as rows collection
        ''' </summary>
        ''' <param name="columns"></param>
        ''' <returns></returns>
        Private Shared Iterator Function ColumnRows(columns As ArgumentReference()) As IEnumerable(Of RowObject)
            Dim collectionType As Type() = {GetType(Array), GetType(IEnumerable), GetType(IList)}
            Dim matrix As Object()() = columns _
                .Select(Function(c)
                            Dim type As Type = c.ValueType

                            If collectionType.Any(Function(base)
                                                      Return type.IsInheritsFrom(base)
                                                  End Function) Then
                                ' 是一个值的集合
                                Return DirectCast(c.Value, IEnumerable).ToVector
                            Else
                                ' 是一个单个的值,转换为值的集合
                                Return New Object() {c.Value}
                            End If
                        End Function) _
                .ToArray
            Dim maxLen = Aggregate c In matrix Into Max(c.Length)
            Dim row As IEnumerable(Of String)

            ' yield title row
            Yield New RowObject(columns.Select(Function(c) c.Name))

            For i As Integer = 0 To maxLen - 1
#Disable Warning
                row = matrix _
                    .Select(Function(v)
                                Return v.ElementAtOrNull(i)
                            End Function) _
                    .Select(AddressOf Scripting.ToString)
#Enable Warning
                Yield New RowObject(row)
            Next
        End Function

        ''' <summary>
        ''' Try loading a excel csv data file as a dynamics data frame object.
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="encoding"></param>
        ''' <param name="simpleRowIterators">
        ''' set this parameter to False will enable the multiple line row parser feature.
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>(尝试加载一个Csv文件为数据框对象，请注意，第一行必须要为标题行)</remarks>
        Public Overloads Shared Function Load(path As String,
                                              Optional encoding As Encoding = Nothing,
                                              Optional fast As Boolean = False,
                                              Optional skipWhile As NamedValue(Of Func(Of String, Boolean)) = Nothing,
                                              Optional simpleRowIterators As Boolean = True) As DataFrame
            Dim file As File

            If fast Then
                file = FileLoader.FastLoad(path, True, encoding, skipWhile)
            Else
                file = File.Load(path, encoding, , skipWhile, simpleRowIterator:=simpleRowIterators)
            End If

            Return CreateObject(file)
        End Function

        Public Overloads Shared Function Load(stream As Stream,
                                              Optional encoding As Encoding = Nothing,
                                              Optional isTsv As Boolean = False) As DataFrame
            Dim file As New File With {
                ._innerTable = File.loads(
                    file:=stream,
                    encoding:=If(encoding, Encoding.Default),
                    trimBlanks:=False,
                    skipWhile:=Nothing,
                    isTsv:=isTsv
                )
            }
            Dim table As DataFrame = CreateObject(file)

            Return table
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadDataSet(path$, Optional encoding As Encoding = Nothing) As IEnumerable(Of DataSet)
            Return DataSet.LoadDataSet(path, encoding:=encoding)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Function Parse(Of T As Class)(content As String) As IEnumerable(Of T)
            Return File.Parse(content).AsDataSource(Of T)
        End Function

        ''' <summary>
        ''' use the first row as the column names
        ''' </summary>
        ''' <param name="table"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' just check for the table header and give the warning message
        ''' </remarks>
        Private Shared Function getColumnList(table As IEnumerable(Of RowObject)) As List(Of String)
            Dim empty_ordinal As New List(Of Integer)
            Dim whitespace_padding As New List(Of (String, Integer))
            Dim colnames As List(Of String) = table.FirstOrDefault.AsList

            If colnames.IsNullOrEmpty Then
                Call "empty table data!".Warning
            Else
                For i As Integer = 0 To colnames.Count - 1
                    If colnames(i).StringEmpty(whitespaceAsEmpty:=True) Then
                        Call empty_ordinal.Add(i)
                    Else
                        Dim lc As Char = colnames(i).First
                        Dim rc As Char = colnames(i).Last

                        If lc = " "c OrElse lc = ASCII.TAB OrElse rc = " "c OrElse rc = ASCII.TAB Then
                            Call whitespace_padding.Add((colnames(i), i))
                        End If
                    End If
                Next

                ' 20240430
                ' 这里不能够使用Trim函数，因为Column也可能是故意定义了空格在其实或者结束的位置的，
                ' 使用Trim函数之后，反而可能会导致GetOrder函数执行失败。故而在这里只给出警告信息即可
                If empty_ordinal.Any OrElse whitespace_padding.Any Then
                    Dim warnings As New List(Of String)

                    If empty_ordinal.Any Then
                        warnings.Add($"there are empty column header in your table data in columns: {empty_ordinal.ToArray.GetJson}.")
                    End If
                    If whitespace_padding.Any Then
                        If warnings.Any Then
                            warnings.Add("and also ")
                        End If
                        warnings.Add($"there are column headers that padding with whitespace in left or right: {whitespace_padding.Select(Function(c) $"[{c.Item2}] '{c.Item1}'").JoinBy(", ")}. these may caused the ``GetOrder()`` function execute failure!")
                    End If

                    Call warnings.JoinBy("").Warning
                End If
            End If

            Return colnames
        End Function

        ''' <summary>
        ''' Creates the data frame object from the csv docs.
        ''' </summary>
        ''' <param name="file"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Function CreateObject(file As File) As DataFrame
            Return createObjectInternal(file)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="headers">the header text data of each columns</param>
        ''' <param name="rows">the table data that exlcudes the first header row.</param>
        ''' <returns></returns>
        Public Overloads Shared Function CreateObject(headers As IEnumerable(Of String), rows As IEnumerable(Of RowObject)) As DataFrame
            Dim df As New DataFrame With {
                .table = rows.AsList,
                .columnList = New HeaderSchema(headers)
            }
            Return df
        End Function

        ''' <summary>
        ''' just needs to assign the table value and the column names
        ''' </summary>
        ''' <param name="table"></param>
        ''' <param name="dataframe"></param>
        Private Shared Sub Initialize(table As List(Of RowObject), ByRef dataframe As DataFrame)
            dataframe.table = table.Skip(1).AsList
            dataframe.columnList = New HeaderSchema(getColumnList(table))
        End Sub

        Private Shared Function createObjectInternal(file As File) As DataFrame
            Dim dataframe As New DataFrame
            Call Initialize(file._innerTable, dataframe)
            Return dataframe
        End Function

        Protected Friend Function __createTableVector() As RowObject()
            Dim readBuffer As New List(Of RowObject)({CType(Me.columnList.Headers, RowObject)})
            Call readBuffer.AddRange(table)
            Return readBuffer.ToArray
        End Function

        ''' <summary>
        ''' Function return -1 when column not found. 
        ''' </summary>
        ''' <param name="Column"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetOrdinal(Column As String) As Integer Implements IDataRecord.GetOrdinal, ISchema.GetOrdinal
            Return columnList.GetOrdinal(Column)
        End Function

        Public Function GetOrdinal(ParamArray [alias] As String()) As Integer
            For Each name As String In [alias]
                Dim i As Integer = GetOrdinal(Column:=name)

                If i > -1 Then
                    Return i
                End If
            Next

            Return -1
        End Function

        ''' <summary>
        ''' Gets the order list of the specific column list, -1 value will be returned when it is not exists in the table.
        ''' (获取列集合的位置列表，不存在的列则返回-1)
        ''' </summary>
        ''' <param name="columns"></param>
        ''' <returns></returns>
        ''' <remarks>由于存在一一对应关系，这里不会再使用并行拓展</remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetOrdinalSchema(columns As String()) As Integer()
            Return columns _
                .Select(AddressOf columnList.GetOrdinal) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetValue(ordinal As Integer) As String
#If DEBUG Then
            If ordinal > Me.current.Count - 1 Then
                Return ""
            End If
#End If
            Return current.Column(ordinal)
        End Function

        Public Function GetValueLambda(columnName$, Optional caseSensitive As Boolean = True) As Func(Of RowObject, String)
            Dim index As Integer = GetOrdinal(Column:=columnName)

            If index = -1 AndAlso caseSensitive Then
                Return Function(r) Nothing
            ElseIf index = -1 Then
                With which(columnList.Headers.Select(Function(c) c.TextEquals(columnName))).ToArray
                    If .IsNullOrEmpty Then
                        Return Function(r) Nothing
                    Else
                        index = .ByRef(Scan0)
                    End If
                End With
            End If

            Return Function(r) r(index)
        End Function

        ''' <summary>
        ''' The data frame object start to reading the data in this table, if the current pointer is reach 
        ''' the top of the lines then this function will returns FALSE to stop the reading loop.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Read() As Boolean Implements IDataReader.Read, IDataReader.NextResult
            If p = table.Count - 1 Then
                Return False
            Else
                p += 1
                current = table(p)

                Return True
            End If
        End Function

        ''' <summary>
        ''' Reset the reading position in the data frame object.
        ''' </summary>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Reset()
            p = -1
        End Sub

        ''' <summary>
        ''' 这个方法会清除当前对象之中的原有数据
        ''' </summary>
        ''' <param name="source"></param>
        ''' <remarks></remarks>
        Public Sub CopyFrom(source As File)
            table = source._innerTable.Skip(1).AsList
            columnList = New HeaderSchema(source._innerTable.First)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return table(p).ToString
        End Function

        ''' <summary>
        ''' Subset of the dataframe object by given column field names
        ''' </summary>
        ''' <param name="columnList"></param>
        ''' <returns></returns>
        Public Function [Select](columnList As String()) As DataFrame
            Dim newTable As New List(Of RowObject)
            ' Location pointer to the column
            Dim pList As Integer() = GetOrdinalSchema(columnList)

            Call Me.Reset()

            Do While Me.Read
                newTable += New RowObject(pList.Select(Function(i) current.Column(i)))
            Loop

            Return New DataFrame With {
                .columnList = New HeaderSchema(columnList),
                .table = newTable
            }
        End Function

        Public Iterator Function GetEnumerator2() As IEnumerator(Of DynamicObjectLoader) Implements IEnumerable(Of DynamicObjectLoader).GetEnumerator
            Dim schema As Dictionary(Of String, Integer) = columnList.SchemaOridinal

            For Each l As DynamicObjectLoader In From i As SeqValue(Of RowObject)
                                                 In table.SeqIterator
                                                 Let line As RowObject = i.value
                                                 Let loader = New DynamicObjectLoader With {
                                                     .lineNumber = i.i,
                                                     .RowData = line,
                                                     .Schema = schema
                                                 }
                                                 Select loader
                Yield l
            Next
        End Function

        ''' <summary>
        ''' Closes the <see cref="System.Data.IDataReader"/>:<see cref="DataFrame"/> Object.  
        ''' </summary>
        Private Sub Close() Implements IDataReader.Close
            ' Do Nothing
        End Sub

        ''' <summary>
        ''' Returns a System.Data.DataTable that describes the column metadata of the System.Data.IDataReader.
        ''' </summary>
        ''' <returns>A System.Data.DataTable that describes the column metadata.</returns>
        Private Function GetSchemaTable() As DataTable Implements IDataReader.GetSchemaTable
            Throw New NotImplementedException()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetName(i As Integer) As String Implements IDataRecord.GetName
            Return columnList.GetName(i)
        End Function

        Public Function GetDataTypeName(i As Integer) As String Implements IDataRecord.GetDataTypeName
            Return typeSchema(i).FullName
        End Function

        Public Function GetFieldType(i As Integer) As Type Implements IDataRecord.GetFieldType
            Return typeSchema(i)
        End Function

        Private Function IDataRecord_GetValue(i As Integer) As Object Implements IDataRecord.GetValue
            Return current.Column(i)
        End Function

        Public Function GetValues(values() As Object) As Integer Implements IDataRecord.GetValues
            If values.IsNullOrEmpty Then
                Return 0
            Else
                For i As Integer = 0 To values.Length - 1
                    values(i) = current.Column(i)
                Next

                Return values.Length
            End If
        End Function

        Public Function GetBoolean(i As Integer) As Boolean Implements IDataRecord.GetBoolean
            Dim value As String = current.Column(i)
            Return Scripting.CTypeDynamic(Of Boolean)(value)
        End Function

        Public Function GetByte(i As Integer) As Byte Implements IDataRecord.GetByte
            Dim value As String = current.Column(i)
            Return Scripting.CTypeDynamic(Of Byte)(value)
        End Function

        Public Function GetBytes(i As Integer, fieldOffset As Long, buffer() As Byte, bufferoffset As Integer, length As Integer) As Long Implements IDataRecord.GetBytes
            Throw New NotImplementedException()
        End Function

        Public Function GetChar(i As Integer) As Char Implements IDataRecord.GetChar
            Dim value As String = current.Column(i)
            Return Scripting.CTypeDynamic(Of Char)(value)
        End Function

        Public Function GetChars(i As Integer, fieldoffset As Long, buffer() As Char, bufferoffset As Integer, length As Integer) As Long Implements IDataRecord.GetChars
            Throw New NotImplementedException()
        End Function

        Public Function GetGuid(i As Integer) As Guid Implements IDataRecord.GetGuid
            Dim value As String = current.Column(i)
            Return Scripting.CTypeDynamic(Of Guid)(value)
        End Function

        Public Function GetInt16(i As Integer) As Short Implements IDataRecord.GetInt16
            Dim value As String = current.Column(i)
            Return Scripting.CTypeDynamic(Of Short)(value)
        End Function

        Public Function GetInt32(i As Integer) As Integer Implements IDataRecord.GetInt32
            Dim value As String = current.Column(i)
            Return Scripting.CTypeDynamic(Of Integer)(value)
        End Function

        Public Function GetInt64(i As Integer) As Long Implements IDataRecord.GetInt64
            Dim value As String = current.Column(i)
            Return Scripting.CTypeDynamic(Of Long)(value)
        End Function

        Public Function GetFloat(i As Integer) As Single Implements IDataRecord.GetFloat
            Dim value As String = current.Column(i)
            Return Scripting.CTypeDynamic(Of Single)(value)
        End Function

        Public Function GetDouble(i As Integer) As Double Implements IDataRecord.GetDouble
            Dim value As String = current.Column(i)
            Return Scripting.CTypeDynamic(Of Double)(value)
        End Function

        Public Function GetString(i As Integer) As String Implements IDataRecord.GetString
            Dim value As String = current.Column(i)
            Return value
        End Function

        Public Function GetDecimal(i As Integer) As Decimal Implements IDataRecord.GetDecimal
            Dim value As String = current.Column(i)
            Return Scripting.CTypeDynamic(Of Decimal)(value)
        End Function

        Public Function GetDateTime(i As Integer) As Date Implements IDataRecord.GetDateTime
            Dim value As String = current.Column(i)
            Return Scripting.CTypeDynamic(Of Date)(value)
        End Function

        Public Function GetData(i As Integer) As IDataReader Implements IDataRecord.GetData
            Return Me
        End Function

        Public Function IsDBNull(i As Integer) As Boolean Implements IDataRecord.IsDBNull
            Return String.IsNullOrEmpty(current.Column(i))
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator2()
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub

#End Region
    End Class
End Namespace
