#Region "Microsoft.VisualBasic::11da719da20d5fb1b9f463d41de3bf1e, ..\sciBASIC#\Data\DataFrame\IO\DataFrame.vb"

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
Imports System.Text
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Terminal

Namespace IO

    ''' <summary>
    ''' The dynamics data frame object which its first line is not contains the but using for the title property.
    ''' (第一行总是没有的，即本对象类型适用于第一行为列标题行的数据)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class DataFrame : Inherits File
        Implements ISchema
        Implements System.Data.IDataReader
        Implements System.IDisposable
        Implements IEnumerable(Of DynamicObjectLoader)

        ''' <summary>
        ''' <see cref="__currentLine"></see>在<see cref="_innerTable"></see>之中的位置
        ''' </summary>
        ''' <remarks></remarks>
        Dim __current% = -1
        Dim __currentLine As RowObject

        ''' <summary>
        ''' Using the first line of the csv row as the column headers in this csv file.
        ''' </summary>
        ''' <remarks></remarks>
        Protected __columnList As List(Of String)
        Public ReadOnly Property SchemaOridinal As Dictionary(Of String, Integer) Implements ISchema.SchemaOridinal

        ''' <summary>
        ''' ``Csv.Field -> <see cref="PropertyInfo.Name"/>``
        ''' </summary>
        ''' <param name="MappingData">{oldFieldName, newFieldName}</param>
        ''' <remarks></remarks>
        Public Sub ChangeMapping(MappingData As Dictionary(Of String, String))
            For Each ColumnName As KeyValuePair(Of String, String) In MappingData
                Dim p As Integer = __columnList.IndexOf(ColumnName.Key)

                If Not p = -1 Then ' 由于只是改变映射的名称，并不要添加新的列，所以在这里忽略掉不存在的列
                    __columnList(p) = ColumnName.Value
                    _SchemaOridinal.Remove(ColumnName.Key)
                    _SchemaOridinal.Add(ColumnName.Value, p)
                End If
            Next
        End Sub

        Public Function AddAttribute(Name As String) As Integer
            If SchemaOridinal.ContainsKey(Name) Then
                Return SchemaOridinal(Name)
            Else
                Dim p As Integer = __columnList.Count
                Call __columnList.Add(Name)
                Call _SchemaOridinal.Add(Name, p)
                Return p
            End If
        End Function

        ''' <summary>
        ''' There is an duplicated key exists in your csv table, please delete the duplicated key and try load again!
        ''' </summary>
        Const DuplicatedKeys As String = "There is an duplicated key exists in your csv table, please delete the duplicated key and try load again!"

        ''' <summary>
        ''' Indexing the column headers
        ''' </summary>
        ''' <param name="df"></param>
        ''' <returns></returns>
        Private Shared Function __createSchemaOridinal(df As DataFrame) As Dictionary(Of String, Integer)
            Dim arrayCache As String() = df.__columnList.ToArray

            Try
                Return arrayCache _
                    .SeqIterator _
                    .ToDictionary(Function(i) i.value, Function(i) i.i)

            Catch ex As Exception
                Dim sb As New StringBuilder(DuplicatedKeys)

                Call sb.AppendLine("Here is the column header keys in you data: ")
                Call sb.AppendLine()
                Call sb.AppendLine("   " & arrayCache.GetJson)

                Throw New DataException(sb.ToString, ex)
            End Try
        End Function

        ''' <summary>
        ''' Get the lines data for the convinent data operation.(为了保持一致的顺序，这个函数是非并行化的)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateDataSource() As DynamicObjectLoader()
            Dim LQuery As DynamicObjectLoader() = LinqAPI.Exec(Of DynamicObjectLoader) <=
 _
                From i As Integer
                In RowNumbers.Sequence.AsParallel
                Let line As RowObject = _innerTable(i)  ' 已经去掉了首行标题行了的
                Select row = New DynamicObjectLoader With {
                    .LineNumber = i,
                    .RowData = line,
                    .Schema = Me.SchemaOridinal,
                    ._innerDataFrame = Me
                }
                Order By row.LineNumber Ascending

            Return LQuery
        End Function

        Public Iterator Function EnumerateData() As IEnumerable(Of Dictionary(Of String, String))
            For Each row In _innerTable
                Dim out As New Dictionary(Of String, String)

                For Each key In SchemaOridinal
                    out.Add(key.Key, row(key.Value))
                Next

                Yield out
            Next
        End Function

        ''' <summary>
        ''' The column headers in the csv file first row.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HeadTitles As String()
            Get
                Return __columnList.ToArray
            End Get
        End Property

        ''' <summary>
        ''' The column headers in the csv file first row.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property Headers As RowObject
            Get
                Return New RowObject(__columnList)
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

        Public ReadOnly Property RecordsAffected As Integer Implements IDataReader.RecordsAffected
            Get
                Return 0
            End Get
        End Property

        Public ReadOnly Property FieldCount As Integer Implements IDataRecord.FieldCount
            Get
                Return __columnList.Count
            End Get
        End Property

        Private ReadOnly Property IDataRecord_Item(i As Integer) As Object Implements IDataRecord.Item
            Get
                Return IDataRecord_GetValue(i)
            End Get
        End Property

        Public Overloads ReadOnly Property Item(name As String) As Object Implements IDataRecord.Item
            Get
                Return IDataRecord_GetValue(GetOrdinal(name))
            End Get
        End Property

        ''' <summary>
        ''' Convert this dataframe object as a csv document object
        ''' </summary>
        ''' <returns></returns>
        Public Function csv() As File
            Dim File As New File
            File += __columnList.ToCsvRow
            File += DirectCast(_innerTable, IEnumerable(Of RowObject))
            Return File
        End Function

        Protected Friend Sub New()
        End Sub

        ''' <summary>
        ''' Try loading a excel csv data file as a dynamics data frame object.(尝试加载一个Csv文件为数据框对象，请注意，第一行必须要为标题行)
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Load(path As String, encoding As Encoding, Optional fast As Boolean = False) As DataFrame
            Dim File As File = If(fast, File.FastLoad(path, True, encoding), File.Load(path, encoding))
            Return CreateObject(File)
        End Function

        Private Shared Function __getColumnList(table As IEnumerable(Of RowObject)) As List(Of String)
            Return LinqAPI.MakeList(Of String) <= From strValue As String
                                                  In table.First
                                                  Select __reviewColumnHeader(strValue)
        End Function

        ''' <summary>
        ''' ``[CSV::Reflector::Warnning] There are empty column header in your data!``
        ''' </summary>
        Const EmptyWarning$ = "[CSV::Reflector::Warnning] There are empty column header in your data!"

        ''' <summary>
        ''' 这里不能够使用Trim函数，因为Column也可能是故意定义了空格在其实或者结束的位置的，使用Trim函数之后，反而会导致GetOrder函数执行失败。故而在这里只给出警告信息即可
        ''' </summary>
        ''' <param name="strValue"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function __reviewColumnHeader(strValue As String) As String
            If String.IsNullOrEmpty(strValue) Then
                Call EmptyWarning.Warning
                Return ""
            End If

            Dim ch As Char = strValue.First

            If ch = " "c OrElse ch = vbTab Then
                Call xConsole.WriteLine($"^y{String.Format(FailureWarning, strValue)}^!")
            End If
            ch = strValue.Last
            If ch = " "c OrElse ch = vbTab Then
                Call xConsole.WriteLine($"^y{String.Format(FailureWarning, strValue)}^!")
            End If

            Return strValue '这里不能够使用Trim函数，因为Column也可能是故意定义了空格在其实或者结束的位置的，使用Trim函数之后，反而会导致GetOrder函数执行失败。故而在这里只给出警告信息即可
        End Function

        Const FailureWarning As String =
            "[CSV::Reflector::Warning] The Column header ""{0}"" end with the space character value, this may caused the ``GetOrder()`` function execute failure!"

        ''' <summary>
        ''' Creates the data frame object from the csv docs.
        ''' </summary>
        ''' <param name="file"></param>
        ''' <returns></returns>
        Public Overloads Shared Function CreateObject(file As File) As DataFrame
            Try
                Return __createObject(file)
            Catch ex As Exception
                Call $"Error during read file from handle {file.FilePath.ToFileURL}".__DEBUG_ECHO
                Call ex.PrintException
                Throw
            End Try
        End Function

        Private Shared Function __createObject(file As File) As DataFrame
            Dim df As New DataFrame With {
                ._innerTable = file._innerTable.Skip(1).ToList,
                .FilePath = file.FilePath
            }
            df.__columnList = __getColumnList(file._innerTable)
            df._SchemaOridinal = __createSchemaOridinal(df)

            Return df
        End Function

        Protected Friend Overrides Function __createTableVector() As RowObject()
            Dim readBuffer As New List(Of RowObject)({CType(Me.__columnList, RowObject)})
            Call readBuffer.AddRange(_innerTable)
            Return readBuffer.ToArray
        End Function

        Public Overrides Function Generate() As String
            Dim sb As StringBuilder = New StringBuilder(1024)
            Dim head As String =
                New RowObject(__columnList).AsLine

            Call sb.AppendLine(head)

            For Each row As RowObject In _innerTable
                Call sb.AppendLine(row.AsLine)
            Next

            Return sb.ToString
        End Function

        ''' <summary>
        ''' Function return -1 when column not found. 
        ''' </summary>
        ''' <param name="Column"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetOrdinal(Column As String) As Integer Implements IDataRecord.GetOrdinal, ISchema.GetOrdinal
            Return __columnList.IndexOf(Column)
        End Function

        ''' <summary>
        ''' Gets the order list of the specific column list, -1 value will be returned when it is not exists in the table.
        ''' (获取列集合的位置列表，不存在的列则返回-1)
        ''' </summary>
        ''' <param name="columns"></param>
        ''' <returns></returns>
        ''' <remarks>由于存在一一对应关系，这里不会再使用并行拓展</remarks>
        Public Function GetOrdinalSchema(columns As String()) As Integer()
            Return columns.ToArray(
                [ctype]:=AddressOf __columnList.IndexOf,
                parallel:=False)
        End Function

        Public Function GetValue(ordinal As Integer) As String
#If DEBUG Then
            If ordinal > Me.__currentLine.Count - 1 Then
                Return ""
            End If
#End If
            Return __currentLine.Column(ordinal)
        End Function

        ''' <summary>
        ''' The data frame object start to reading the data in this table, if the current pointer is reach 
        ''' the top of the lines then this function will returns FALSE to stop the reading loop.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Read() As Boolean Implements IDataReader.Read, IDataReader.NextResult
            If __current = _innerTable.Count - 1 Then
                Return False
            Else
                __current += 1
                __currentLine = _innerTable(__current)

                Return True
            End If
        End Function

        ''' <summary>
        ''' Reset the reading position in the data frame object.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Reset()
            __current = -1
        End Sub

        ''' <summary>
        ''' 这个方法会清除当前对象之中的原有数据
        ''' </summary>
        ''' <param name="source"></param>
        ''' <remarks></remarks>
        Public Sub CopyFrom(source As File)
            _innerTable = source._innerTable.Skip(1).ToList
            FilePath = source.FilePath
            __columnList = source._innerTable.First.ToList
        End Sub

        Public Overrides Function ToString() As String
            Return FilePath.ToFileURL & "  // " & _innerTable(__current).ToString
        End Function

        Public Function [Select](columnList As String()) As DataFrame
            Dim newTable As New List(Of RowObject)
            Dim pList As Integer() =
                GetOrdinalSchema(columnList)   'Location pointer to the column

            Call Me.Reset()

            Do While Me.Read
                newTable += New RowObject(
                    pList.Select(
                    Function(i) __currentLine.Column(i)))
            Loop

            Return New DataFrame With {
                .__columnList = columnList.ToList,
                .FilePath = FilePath,
                ._innerTable = newTable
            }
        End Function

        Public Iterator Function GetEnumerator2() As IEnumerator(Of DynamicObjectLoader) Implements IEnumerable(Of DynamicObjectLoader).GetEnumerator
            Dim schema As Dictionary(Of String, Integer) =
                __columnList _
                .SeqIterator _
                .ToDictionary(Function(x) x.value,
                              Function(x) x.i)

            For Each l As DynamicObjectLoader In From i As SeqValue(Of RowObject)
                                                 In Me._innerTable.SeqIterator
                                                 Let line As RowObject = i.value
                                                 Let loader = New DynamicObjectLoader With {
                                                     .LineNumber = i.i,
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

        Public Function GetName(i As Integer) As String Implements IDataRecord.GetName
            Return __columnList(i)
        End Function

        Public Function GetDataTypeName(i As Integer) As String Implements IDataRecord.GetDataTypeName
            Dim value As String = GetValue(i)
            If IsNumeric(value) Then
                Return "System.Double"
            ElseIf InStr(value, ", ") > 0 OrElse InStr(value, "; ") > 0 Then
                Return "System.String()"
            Else
                Return "System.String"
            End If
        End Function

        Public Function GetFieldType(i As Integer) As Type Implements IDataRecord.GetFieldType
            Dim typeName As String = GetDataTypeName(i)
            Return Scripting.InputHandler.GetType(typeName, True)
        End Function

        Private Function IDataRecord_GetValue(i As Integer) As Object Implements IDataRecord.GetValue
            Return __currentLine.Column(i)
        End Function

        Public Function GetValues(values() As Object) As Integer Implements IDataRecord.GetValues
            If values.IsNullOrEmpty Then
                Return 0
            Else
                For i As Integer = 0 To values.Length - 1
                    values(i) = __currentLine.Column(i)
                Next

                Return values.Length
            End If
        End Function

        Public Function GetBoolean(i As Integer) As Boolean Implements IDataRecord.GetBoolean
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Boolean)(value)
        End Function

        Public Function GetByte(i As Integer) As Byte Implements IDataRecord.GetByte
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Byte)(value)
        End Function

        Public Function GetBytes(i As Integer, fieldOffset As Long, buffer() As Byte, bufferoffset As Integer, length As Integer) As Long Implements IDataRecord.GetBytes
            Throw New NotImplementedException()
        End Function

        Public Function GetChar(i As Integer) As Char Implements IDataRecord.GetChar
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Char)(value)
        End Function

        Public Function GetChars(i As Integer, fieldoffset As Long, buffer() As Char, bufferoffset As Integer, length As Integer) As Long Implements IDataRecord.GetChars
            Throw New NotImplementedException()
        End Function

        Public Function GetGuid(i As Integer) As Guid Implements IDataRecord.GetGuid
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Guid)(value)
        End Function

        Public Function GetInt16(i As Integer) As Short Implements IDataRecord.GetInt16
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Short)(value)
        End Function

        Public Function GetInt32(i As Integer) As Integer Implements IDataRecord.GetInt32
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Integer)(value)
        End Function

        Public Function GetInt64(i As Integer) As Long Implements IDataRecord.GetInt64
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Long)(value)
        End Function

        Public Function GetFloat(i As Integer) As Single Implements IDataRecord.GetFloat
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Single)(value)
        End Function

        Public Function GetDouble(i As Integer) As Double Implements IDataRecord.GetDouble
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Double)(value)
        End Function

        Public Function GetString(i As Integer) As String Implements IDataRecord.GetString
            Dim value As String = __currentLine.Column(i)
            Return value
        End Function

        Public Function GetDecimal(i As Integer) As Decimal Implements IDataRecord.GetDecimal
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Decimal)(value)
        End Function

        Public Function GetDateTime(i As Integer) As Date Implements IDataRecord.GetDateTime
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Date)(value)
        End Function

        Public Function GetData(i As Integer) As IDataReader Implements IDataRecord.GetData
            Return Me
        End Function

        Public Function IsDBNull(i As Integer) As Boolean Implements IDataRecord.IsDBNull
            Return String.IsNullOrEmpty(__currentLine.Column(i))
        End Function
    End Class
End Namespace
