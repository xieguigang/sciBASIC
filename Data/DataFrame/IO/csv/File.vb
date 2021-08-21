#Region "Microsoft.VisualBasic::8930cc182cbb31161106d856d41ff8b5, Data\DataFrame\IO\csv\File.vb"

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

    '     Class File
    ' 
    '         Properties: Cell, EstimatedFileSize, Headers, Rows, Width
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: __createTableVector, AppendLine, AppendLines, AppendRange, FindAll
    '                   FindAtColumn, Generate, GenerateDocument, GetAllStringTokens, GetByLine
    '                   InsertEmptyColumnBefore, Project, Remove, (+2 Overloads) Save, (+2 Overloads) ToArray
    '                   TokenCounts, ToString, Transpose, Trim
    ' 
    '         Sub: __setColumn, Append, (+2 Overloads) AppendLine, DeleteCell, RemoveRange
    ' 
    '         Operators: (+2 Overloads) +
    '         Delegate Function
    ' 
    '             Properties: IsReadOnly, RowNumbers
    ' 
    '             Function: __LINQ_LOAD, AsMatrix, Contains, Distinct, GetEnumerator
    '                       GetEnumerator1, IndexOf, IsNullOrEmpty, (+2 Overloads) Join, Load
    '                       loads, (+2 Overloads) LoadTsv, Parse, ReadHeaderRow, Remove
    '                       RemoveSubRow, Save
    ' 
    '             Sub: (+3 Overloads) Add, Clear, CopyTo, Insert, InsertAt
    '                  RemoveAt, Save
    ' 
    '             Operators: <, <=, >, >=
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace IO

    ''' <summary>
    ''' A comma character seperate table file that can be read and write in the EXCEL.
    ''' (一个能够被Excel程序所读取的表格文件)
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    <ActiveViews(File.ActiveViews)>
    Public Class File
        Implements IEnumerable(Of RowObject)
        Implements IList(Of RowObject)
        Implements ISaveHandle

        Friend Const ActiveViews =
"header1,header2,header3,...
A11,A12,A13,...
B21,B22,B23,...
......"

        ''' <summary>
        ''' The first row in the table was using as the headers
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property Headers As RowObject
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _innerTable?.FirstOrDefault
            End Get
        End Property

        ''' <summary>
        ''' Get all rows in current table object
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Rows As RowObject()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _innerTable.ToArray
            End Get
        End Property

        ''' <summary>
        ''' First line in the table is the column name definition line.
        ''' </summary>
        ''' <remarks>
        ''' 已经去掉了首行标题行了的
        ''' </remarks>
        Protected Friend _innerTable As New List(Of RowObject)

        ''' <summary>
        ''' Creates an empty csv docs object.
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Creates csv file object from the rows data.
        ''' </summary>
        ''' <param name="data"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(data As IEnumerable(Of RowObject))
            _innerTable = data.AsList
        End Sub

        ''' <summary>
        ''' Load document from path
        ''' </summary>
        ''' <param name="path"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(path As String,
                Optional encoding As Encodings = Encodings.Default,
                Optional trimBlanks As Boolean = False,
                Optional skipWhile As NamedValue(Of Func(Of String, Boolean)) = Nothing)

            _innerTable = loads(path, encoding.CodePage, trimBlanks, skipWhile)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(source As IEnumerable(Of RowObject), path As String)
            Call Me.New(source)
        End Sub

        ''' <summary>
        ''' Gets or sets the specific cell's data
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(x%, y%) As String
            Get
                Dim row As RowObject = Me(x)
                Return row(y)
            End Get
            Set(value As String)
                Dim row As RowObject = Me(x)
                row(y) = value
            End Set
        End Property

        ''' <summary>
        ''' Get column values by column name.
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <returns></returns>
        Default Public Overloads ReadOnly Property Item(name$) As String()
            Get
                Dim match$ = Headers _
                    .Where(Function(c) c.TextEquals(name)) _
                    .DefaultFirst(Nothing)
                Dim index% = Headers.IndexOf(match)

                Return Column(index).ToArray
            End Get
        End Property

        ''' <summary>
        ''' Get the max width number of the rows in the table.(返回表中的元素最多的一列的列数目)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Width As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _innerTable _
                    .Select(Function(row) row.NumbersOfColumn) _
                    .Max
            End Get
        End Property

        ''' <summary>
        ''' Get all data of a column of a specific column number.(获取文件中的某一列中的所有数据)
        ''' </summary>
        ''' <param name="Index"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Iterator Property Column(Index As Integer) As IEnumerable(Of String)
            Get
                For Each row As RowObject In _innerTable
                    Yield row.Column(Index)
                Next
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As IEnumerable(Of String))
                Call __setColumn(value.ToArray, Index)
            End Set
        End Property

        Private Sub __setColumn(value As String(), index As Integer)
            If _innerTable.Count < value.Length Then
                Dim d As Integer = value.Length - _innerTable.Count

                For i As Integer = 0 To d - 1
                    Call _innerTable.Add(New RowObject)
                Next
            End If

            For i As Integer = 0 To value.Length - 1
                _innerTable(i).Column(index) = value(i)
            Next
        End Sub

        ''' <summary>
        ''' 将本文件之中的所有列取出来，假若有任意一个列的元素的数目不够的话，则会在相应的位置之上使用空白来替换
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 由于是使用<see cref="_innerTable"/>来作为数据源的，所以对于csv对象而言，是含有header数据的，即含有第一行数据
        ''' 对于<see cref="DataFrame"/>类型而言，由于在创建对象的时候，第一行数据由于需要被用作为header，所以这个内部表对象之中是不包含有header行的，即这个属性所输出的结果只中是不包含有header行的
        ''' </remarks>
        Public ReadOnly Iterator Property Columns As IEnumerable(Of String())
            Get
                If _innerTable.Count = 0 Then
                    Return
                End If
                For Each column As IEnumerable(Of String) In
                    From col As Integer
                    In Width.Sequence
                    Select Me.Column(col)

                    Yield column.ToArray
                Next
            End Get
        End Property

        Public ReadOnly Iterator Property Comments(Optional prefix$ = "#") As IEnumerable(Of String)
            Get
                For Each row As RowObject In _innerTable
                    If InStr(row.First, prefix) = 1 Then
                        Yield row.AsLine(" "c)
                    End If
                Next
            End Get
        End Property

        ''' <summary>
        ''' 删除空白的列
        ''' </summary>
        ''' <returns></returns>
        Public Function Trim() As File
            Dim columns$()() = Me _
                .Columns _
                .Where(Function(c) Not c.IsEmptyStringVector) _
                .ToArray
            Dim df As File = columns.JoinColumns
            Return df
        End Function

        ''' <summary>
        ''' 按照列进行投影操作, 这个函数仅适用于小型数据
        ''' </summary>
        ''' <param name="fieldNames"></param>
        ''' <returns></returns>
        Public Function Project(fieldNames As IEnumerable(Of String)) As File
            Dim columns = fieldNames.Select(Function(name) Me(name)).ToArray
            Dim newTable = columns.JoinColumns

            Return newTable
        End Function

        Public ReadOnly Property EstimatedFileSize As Double
            Get
                Dim LQuery = (From row As RowObject
                              In Me.AsParallel
                              Select (From col As String
                                      In row
                                      Select CDbl(Len(col))).Sum).Sum
                Return LQuery * 8
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function AppendLine(Row As RowObject) As File
            Call _innerTable.Add(Row)
            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function AppendLines(rows As IEnumerable(Of RowObject)) As File
            Call _innerTable.AddRange(rows)
            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub AppendLine(row As IEnumerable(Of String))
            Call _innerTable.Add(New RowObject(row))
        End Sub

        ''' <summary>
        ''' 添加一个空白行
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub AppendLine()
            Call _innerTable.Add(New String() {""})
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Append(dataframe As File)
            Call _innerTable.AddRange(dataframe._innerTable)
        End Sub

        ''' <summary>
        ''' Add a data row collection into this Csv file object instance and then return the total row number after the add operation.
        ''' (向CSV文件之中批量添加行记录，之后返回当前所打开的文件在添加纪录之后的总行数)
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function AppendRange(source As IEnumerable(Of RowObject)) As Long
            Call _innerTable.AddRange(source)
            Return _innerTable.Count
        End Function

        ''' <summary>
        ''' Get a data row in the specific row number, when a row is not exists in current csv file then the function will return a empty row.
        ''' (当目标对象不存在的时候，会返回一个空行对象)
        ''' </summary>
        ''' <param name="line"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetByLine(line As Integer) As RowObject
            If line > _innerTable.Count - 1 Then
                Return New RowObject
            Else
                Return _innerTable(line)
            End If
        End Function

        ''' <summary>
        ''' 使用迭代器返回包含有目标关键词的行
        ''' </summary>
        ''' <param name="KeyWord"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Iterator Function FindAll(KeyWord As String) As IEnumerable(Of RowObject)
            For Each row As RowObject In _innerTable
                If row.LocateKeyWord(KeyWord) > -1 Then
                    Yield row
                End If
            Next
        End Function

        ''' <summary>
        ''' Using the content in a specific column as the target for search using a specific keyword, and then return all of the rows that have the query keyword.
        ''' (以指定的列中的内容搜索关键词，并返回检索成功的行的集合)
        ''' </summary>
        ''' <param name="KeyWord"></param>
        ''' <param name="Column"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function FindAtColumn(KeyWord As String, Column As Integer) As RowObject()
            Return LinqAPI.Exec(Of RowObject) <= From row As RowObject
                                                 In _innerTable.AsParallel
                                                 Let strCell As String = row.Column(Column)
                                                 Where InStr(strCell, KeyWord, CompareMethod.Text) > 0 OrElse
                                                     String.Equals(strCell, KeyWord, StringComparison.OrdinalIgnoreCase)
                                                 Select row
        End Function

        ''' <summary>
        ''' Get and set the string content in a specific table cell.(设置或者获取某一个指定的单元格中的字符串内容)
        ''' </summary>
        ''' <param name="X"></param>
        ''' <param name="Y"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Cell(X As Integer, Y As Integer) As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _innerTable(X).Column(Y)
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As String)
                _innerTable(X).Column(Y) = value
            End Set
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return Headers.GetJson
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToArray() As RowObject()
            Return _innerTable.ToArray
        End Function

        Public Function ToArray(Of T)([ctype] As Func(Of RowObject, T)) As T()
            Dim array As T() = _innerTable.Select([ctype]).ToArray
            Return array
        End Function

        ''' <summary>
        ''' 对当前的csv矩阵进行转置之后返回新的文件
        ''' </summary>
        ''' <returns></returns>
        Public Function Transpose() As File
            Dim buf As String()() = Me.Columns.MatrixTranspose.ToArray
            Dim tableRows = LinqAPI.MakeList(Of RowObject) <=
                        From i As Integer
                        In buf.First.Sequence
                        Select New RowObject(From line As String() In buf Select line(i))

            Return New File With {._innerTable = tableRows}
        End Function

        ''' <summary>
        ''' Delete all of the row that meet the delete condition.(将所有满足条件的行进行删除)
        ''' </summary>
        ''' <param name="condition"></param>
        ''' <remarks></remarks>
        Public Function Remove(condition As Func(Of RowObject, Boolean)) As RowObject()
            Dim LQuery As RowObject() =
                LinqAPI.Exec(Of RowObject) <= From row As RowObject
                                              In Me._innerTable
                                              Where True = condition(row)
                                              Select row
            Call RemoveRange(LQuery)
            Return LQuery
        End Function

        ''' <summary>
        ''' 删除目标列中的单元格中符合条件的内容
        ''' </summary>
        ''' <param name="Condition">条件测试</param>
        ''' <param name="index">列标号</param>
        ''' <remarks></remarks>
        Public Sub DeleteCell(Condition As Func(Of String, Boolean), index As Integer)
            For i As Integer = 0 To _innerTable.Count - 1
                Dim row = _innerTable(i)
                If Condition(row(index)) = True Then
                    row(index) = ""
                End If
            Next
        End Sub

        ''' <summary>
        ''' Remove the item in a specific row collection.
        ''' </summary>
        ''' <param name="source"></param>
        ''' <remarks></remarks>
        Public Sub RemoveRange(source As IEnumerable(Of RowObject))
            For Each row As RowObject In source
                Call _innerTable.Remove(row)
            Next
        End Sub

        ''' <summary>
        ''' Generate the csv data file document using the table data.(将表格对象转换为文本文件以进行保存)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function Generate() As String
            Dim sb As New StringBuilder(2048)
            For Each Row As RowObject In _innerTable
                Call sb.AppendLine(Row.AsLine)
            Next

            Return sb.ToString
        End Function

        Public Function GetAllStringTokens() As String()
            Return LinqAPI.Exec(Of String) <= From row As RowObject
                                              In Me._innerTable
                                              Select row.ToArray
        End Function

        ''' <summary>
        ''' 将表对象转换为文本文件之中的文本内容
        ''' </summary>
        ''' <param name="Parallel">假若是不需要顺序的大文件，请设置为True</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GenerateDocument(Parallel As Boolean) As String()
            If Parallel Then
                Return (From row As RowObject In _innerTable.AsParallel Select row.AsLine).ToArray
            Else
                Return (From row As RowObject In _innerTable Select row.AsLine).ToArray
            End If
        End Function

        ''' <summary>
        ''' Insert a new empty line of row data before the specific row number.(在指定列标号的列之前插入一行空列)
        ''' </summary>
        ''' <param name="column"></param>
        ''' <remarks></remarks>
        Public Function InsertEmptyColumnBefore(column As Integer) As Integer
            Dim LQuery = From row As RowObject
                         In _innerTable.AsParallel
                         Select row.InsertAt("", column) '
            Return LQuery.ToArray.Count
        End Function

        ''' <summary>
        ''' 统计某一个指定的列之中的各个项目的出现次数
        ''' </summary>
        ''' <param name="ColumnIndex"></param>
        ''' <param name="FirstLineTitle"></param>
        ''' <param name="IgnoreBlanks"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function TokenCounts(ColumnIndex As Integer,
                                    Optional FirstLineTitle As Boolean = True,
                                    Optional IgnoreBlanks As Boolean = True,
                                    Optional ignoreCase As Boolean = False) As File

            Dim source = If(FirstLineTitle, _innerTable.Skip(1), _innerTable)
            Dim Values As String() = LinqAPI.Exec(Of String) <=
                From row As RowObject
                In source.AsParallel
                Select row.Column(ColumnIndex) ' 选择出该特定的列对象之中的数据

            Dim gData = (From str As String In Values Select str, lower = Strings.LCase(str)).ToArray

            If IgnoreBlanks Then
                gData = gData.Where(Function(s) Not String.IsNullOrEmpty(s.str)).ToArray
            End If

            Dim tokensGroup As NamedValue(Of Integer)()

            If ignoreCase Then
                Dim counts = From x In gData
                             Select x
                             Group x By x.lower Into Group
                tokensGroup = LinqAPI.Exec(Of NamedValue(Of Integer)) <=
                    From x
                    In counts
                    Select New NamedValue(Of Integer) With {
                        .Name = x.Group.First.str,
                        .Value = x.Group.Count
                    }
            Else
                Dim counts = From x In gData
                             Select x
                             Group x By x.str Into Group
                tokensGroup = LinqAPI.Exec(Of NamedValue(Of Integer)) <=
                    From x
                    In counts
                    Select New NamedValue(Of Integer) With {
                        .Name = x.str,
                        .Value = x.Group.Count
                    }
            End If

            Dim stats As New File
            stats += New RowObject({If(FirstLineTitle, $"Item values for '{First.Column(ColumnIndex)}'", "Item values"), "Counts"})
            stats += From token As NamedValue(Of Integer)
                     In tokensGroup
                     Select New RowObject({token.Name, CStr(token.Value)})
            Return stats
        End Function

#Region "List Operations"

        ''' <summary>
        ''' Add a row collection
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Public Shared Operator +(file As File, source As IEnumerable(Of RowObject)) As File
            Call file.AppendRange(source)
            Return file
        End Operator

        ''' <summary>
        ''' Add a row
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="row"></param>
        ''' <returns></returns>
        Public Shared Operator +(file As File, row As IEnumerable(Of String)) As File
            Call file.Add(New RowObject(row))
            Return file
        End Operator
#End Region

        ''' <summary>
        ''' Save this csv document into a specific file location <paramref name="path"/>.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <remarks>当目标保存路径不存在的时候，会自动创建文件夹</remarks>
        Public Function Save(path$, Encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Return StreamIO.SaveDataFrame(Me, path, Encoding)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Save(path$, encoding As Encodings, Optional tsv As Boolean = False, Optional silent As Boolean = True) As Boolean
            Return StreamIO.SaveDataFrame(Me, path, encoding.CodePage, tsv:=tsv, silent:=silent)
        End Function

        ''' <summary>
        ''' 这个方法是保存<see cref="Csv.DataFrame"></see>对象之中的数据所需要的
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Friend Overridable Function __createTableVector() As RowObject()
            Return Me._innerTable.ToArray
        End Function

#Region "CSV file data loading methods"

        ''' <summary>
        ''' Read a Csv file, default encoding is utf8
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Widening Operator CType(path$) As File
            If Not path.FileExists Then
                Call "Target data table is not exists on your file system!".Warning
                Return New File
            End If
            Return File.Load(path)
        End Operator

        Public Overloads Shared Widening Operator CType(Lines As String()) As File
            'Dim LoadMethod As LoadMethod =
            '    [If](Of LoadMethod)(Lines.Length > 1024, AddressOf __PBS_LOAD, AddressOf __LINQ_LOAD)
            Dim sw As Stopwatch = Stopwatch.StartNew
            Dim CSV As New File(__LINQ_LOAD(data:=Lines))

            Call $"Csv load {Lines.Length} lines data in {sw.ElapsedMilliseconds}ms...".__DEBUG_ECHO ' //{LoadMethod.ToString}".__DEBUG_ECHO

            Return CSV
        End Operator

        Private Delegate Function LoadMethod(data As String()) As File

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks>为了提高数据的加载效率，先使用LINQ预加载数据，之后使用Parallel LINQ进行数据的解析操作</remarks>
        Friend Shared Function __LINQ_LOAD(data As String()) As IEnumerable(Of RowObject)
            Return From line As String
                   In data.AsParallel
                   Let row As RowObject = CType(line, RowObject)
                   Select row
        End Function

        Public Overloads Shared Widening Operator CType(rows As RowObject()) As File
            Return New File With {
                ._innerTable = rows.AsList
            }
        End Operator

        Public Overloads Shared Widening Operator CType(rows As List(Of RowObject)) As File
            Return New File With {
                ._innerTable = rows
            }
        End Operator

        ''' <summary>
        ''' Load the csv data document from a given path.(从指定的文件路径之中加载一个CSV格式的数据文件)
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Load(path$,
                                    Optional encoding As Encoding = Nothing,
                                    Optional trimBlanks As Boolean = False,
                                    Optional skipWhile As NamedValue(Of Func(Of String, Boolean)) = Nothing) As File

            Dim buf As List(Of RowObject) = loads(path, encoding Or TextEncodings.DefaultEncoding, trimBlanks, skipWhile)
            Dim csv As New File With {
                ._innerTable = buf
            }

            Return csv
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadTsv(path$, Optional encoding As Encodings = Encodings.UTF8) As File
            Return csv.Imports(path, ASCII.TAB, encoding.CodePage)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadTsv(path$, Optional encoding As Encoding = Nothing) As File
            Return csv.Imports(path, ASCII.TAB, encoding)
        End Function


        Public Shared Function ReadHeaderRow(path$, Optional encoding As Encodings = Encodings.UTF8, Optional tsv As Boolean = False) As RowObject
            Dim firstLine$ = path.ReadFirstLine(encoding.CodePage)

            If tsv Then
                Return New RowObject(firstLine.Split(ASCII.TAB))
            Else
                Return New RowObject(IO.Tokenizer.CharsParser(firstLine))
            End If
        End Function

        ''' <summary>
        ''' 同时兼容本地文件和网络文件的
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function loads(path As String, encoding As Encoding, trimBlanks As Boolean, skipWhile As NamedValue(Of Func(Of String, Boolean))) As List(Of RowObject)
            Return FileLoader.Load(path.MapNetFile.ReadAllLines(encoding), trimBlanks, skipWhile)
        End Function

        ''' <summary>
        ''' 对目标文本内容字符串进行解析，得到csv文件对象数据模型
        ''' </summary>
        ''' <param name="content">这个参数是文本内容，而非是文件路径</param>
        ''' <param name="trimBlanks"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Parse(content$, Optional trimBlanks As Boolean = True, Optional skipWhile As NamedValue(Of Func(Of String, Boolean)) = Nothing) As File
            Return New File(FileLoader.Load(content.LineTokens, trimBlanks, skipWhile))
        End Function
#End Region

        Public Shared Function Join(ParamArray list As File()) As File
            Dim csv As New File

            list = LinqAPI.Exec(Of File) <=
                From file As File
                In list
                Select file
                Order By csv.Count Descending

            For RowId As Integer = 0 To list.First.Count - 1
                Dim Row As RowObject = New RowObject
                Row.AddRange(list.First()(RowId))
                For i As Integer = 1 To list.Count - 1
                    If RowId < list(i).Count Then
                        Row.AddRange(list(i)(RowId))
                    End If
                Next

                csv._innerTable.Add(Row)
            Next

            Return csv
        End Function

        ''' <summary>
        ''' cbind a column
        ''' </summary>
        ''' <param name="column">the column data</param>
        ''' <returns></returns>
        Public Function Join(column As IEnumerable(Of String)) As File
            Call __setColumn(column.ToArray, Headers.Count)
            Return Me
        End Function

        ''' <summary>
        ''' 去除Csv文件之中的重复记录
        ''' </summary>
        ''' <param name="OrderBy">当为本参数指定一个非负数值的时候，程序会按照指定的列值进行排序</param>
        ''' <param name="Asc">当进行排序操作的时候，是否按照升序进行排序，否则按照降序排序</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Distinct(csv As File, Optional OrderBy As Integer = -1, Optional Asc As Boolean = True) As File
            Dim LQuery As IEnumerable(Of String) =
                From row As RowObject
                In csv
                Let Line As String = row.AsLine
                Select Line
                Distinct '
            Dim dRows = LQuery.Select(AddressOf RowObject.TryParse)

            If OrderBy >= 0 Then
                If Asc Then
                    dRows = dRows.OrderBy(Function(r) r.Column(OrderBy))
                Else
                    dRows = dRows.OrderByDescending(Function(r) r.Column(OrderBy))
                End If
            End If

            Return New File With {
                ._innerTable = New List(Of RowObject)(dRows)
            }
        End Function

        Public Shared Function RemoveSubRow(df As File) As File
            Dim innerTable = LinqAPI.MakeList(Of RowObject) <=
                From row As RowObject
                In df
                Select row
                Order By row.GetALLNonEmptys.Count Descending '

            For Each mrow As SeqValue(Of RowObject) In innerTable.ToArray.SeqIterator
                Dim LQuery As IEnumerable(Of RowObject) =
                    From row As RowObject
                    In innerTable.Skip(mrow.i + 1).AsParallel
                    Where mrow.value.Contains(row)
                    Select row '

                For Each x In LQuery
                    Call innerTable.Remove(x)
                Next
            Next

            Return New File(innerTable)
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of RowObject) Implements IEnumerable(Of RowObject).GetEnumerator
            For i As Integer = 0 To _innerTable.Count - 1
                Yield _innerTable(i)
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function AsMatrix() As IEnumerable(Of IEnumerable(Of String))
            Return _innerTable.Select(Function(r) r.AsEnumerable)
        End Function

        ''' <summary>
        ''' 判断目标数据文件是否为空
        ''' </summary>
        ''' <param name="df"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function IsNullOrEmpty(df As IEnumerable(Of RowObject)) As Boolean
            Return df Is Nothing OrElse Not df.Any
        End Function

        Public Shared Operator <=(df As File, type As Type) As Object()
            Return Reflector.LoadDataToObject(df.DataFrame, type, False).ToArray
        End Operator

        Public Shared Operator >=(df As File, type As Type) As Object()
            Return df <= type
        End Operator

        Public Shared Operator >(File As File, path As String) As Boolean
            Return File.Save(path, Encoding.ASCII)
        End Operator

        Public Shared Operator <(file As File, path As String) As Boolean
            Throw New NotSupportedException
        End Operator

#Region "Implements of Generic.IList(Of DocumentFormat.Csv.DocumentStream.File.Row) interface"

        Public Sub Add(item As RowObject) Implements ICollection(Of RowObject).Add
            Call _innerTable.Add(item)
        End Sub

        Public Sub Add(ParamArray row As Object())
            Call Add(row.Select(Function(c) Scripting.ToString(c)))
        End Sub

        Public Sub Add(ParamArray row As String())
            Call _innerTable.Add(New RowObject(row))
        End Sub

        Public Sub Clear() Implements ICollection(Of RowObject).Clear
            Call _innerTable.Clear()
        End Sub

        Public Function Contains(item As RowObject) As Boolean Implements ICollection(Of RowObject).Contains
            Return _innerTable.Contains(item)
        End Function

        Public Overloads Sub CopyTo(array() As RowObject, arrayIndex As Integer) Implements ICollection(Of RowObject).CopyTo
            Call _innerTable.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' Row Counts
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RowNumbers As Integer Implements ICollection(Of RowObject).Count
            Get
                Return _innerTable.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of RowObject).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(item As RowObject) As Boolean Implements ICollection(Of RowObject).Remove
            Return _innerTable.Remove(item)
        End Function

        Public Function IndexOf(item As RowObject) As Integer Implements IList(Of RowObject).IndexOf
            Return _innerTable.IndexOf(item)
        End Function

        ''' <summary>
        ''' 在所指定的行号下面插入一行数据
        ''' </summary>
        ''' <param name="rowId"></param>
        ''' <param name="Row"></param>
        ''' <remarks></remarks>
        Public Sub Insert(rowId As Integer, Row As RowObject)
            Call _innerTable.Insert(rowId + 1, Row)
        End Sub

        ''' <summary>
        ''' 使用IList接口本身的Insert方法来执行插入
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="item"></param>
        ''' <remarks></remarks>
        Public Sub InsertAt(index As Integer, item As RowObject) Implements IList(Of RowObject).Insert
            Call _innerTable.Insert(index, item)
        End Sub

        ''' <summary>
        ''' Gets a row in the document stream object.
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(index As Integer) As RowObject Implements IList(Of RowObject).Item
            Get
                Return _innerTable(index)
            End Get
            Set(value As RowObject)
                _innerTable(index) = value
            End Set
        End Property

        Public Sub RemoveAt(index As Integer) Implements IList(Of RowObject).RemoveAt
            Call _innerTable.RemoveAt(index)
        End Sub

        Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(path, encoding.CodePage)
        End Function

        Public Sub Save(output As StreamWriter, Optional isTsv As Boolean = False)
            Dim delimiter As Char = ","c Or ASCII.TAB.When(isTsv)

            For Each row As RowObject In Me
                Call output.WriteLine(row.AsLine(delimiter))
            Next
        End Sub
#End Region
    End Class
End Namespace
