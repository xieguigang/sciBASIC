#Region "Microsoft.VisualBasic::ac358be28c17855cb9628c52ed1361d9, Data\BinaryData\SQLite3\Writer\Sqlite3TableWriter.vb"

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

    '   Total Lines: 331
    '    Code Lines: 229 (69.18%)
    ' Comment Lines: 29 (8.76%)
    '    - Xml Docs: 93.10%
    ' 
    '   Blank Lines: 73 (22.05%)
    '     File Size: 11.91 KB


    '     Class Sqlite3TableWriter
    ' 
    '         Properties: Columns, PrimaryKeyOrdinal, RowCount, RowIdAliasOrdinal, Sql
    '                     TableName
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) AddRow, BuildSnapshot, DeleteRow, DeleteRowByPrimaryKey, EnumerateRows
    '                   GenerateSql, GetOrdinal, GetRow, IsIntegerValue, NormalizeValues
    '                   ToString, (+2 Overloads) UpdateRow, UpdateRowByPrimaryKey, ValuesEqual
    ' 
    '         Sub: Clear, LoadRow, SetOriginalSql
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Linq

Namespace Writer

    ''' <summary>
    ''' 单张数据表的可写模型, 提供链式的行级增删改查(CRUD)。所有修改在 <see cref="Sqlite3Writer.Commit"/> 时落盘。
    ''' </summary>
    Public Class Sqlite3TableWriter

        Private ReadOnly _writer As Sqlite3Writer
        Private ReadOnly _columns As New List(Of Sqlite3Column)()
        Private ReadOnly _rows As New SortedDictionary(Of Long, Object())()

        Private _originalSql As String
        Private _nextRowId As Long = 1

        Friend Sub New(writer As Sqlite3Writer, name As String, columns As IEnumerable(Of Sqlite3Column))
            Me._writer = writer
            Me.TableName = name

            For Each c As Sqlite3Column In columns
                _columns.Add(c)
            Next
        End Sub

        ''' <summary>表名</summary>
        Public ReadOnly Property TableName As String

        ''' <summary>列定义</summary>
        Public ReadOnly Property Columns As Sqlite3Column()
            Get
                Return _columns.ToArray
            End Get
        End Property

        ''' <summary>当前内存之中的数据行数</summary>
        Public ReadOnly Property RowCount As Integer
            Get
                Return _rows.Count
            End Get
        End Property

        ''' <summary>rowid 别名列(INTEGER PRIMARY KEY)的序号, 不存在时为 -1</summary>
        Public ReadOnly Property RowIdAliasOrdinal As Integer
            Get
                For i As Integer = 0 To _columns.Count - 1
                    If _columns(i).IsRowIdAlias() Then
                        Return i
                    End If
                Next

                Return -1
            End Get
        End Property

        ''' <summary>主键列序号, 不存在时为 -1</summary>
        Public ReadOnly Property PrimaryKeyOrdinal As Integer
            Get
                For i As Integer = 0 To _columns.Count - 1
                    If _columns(i).PrimaryKey Then
                        Return i
                    End If
                Next

                Return -1
            End Get
        End Property

        ''' <summary>
        ''' 写入磁盘时使用的 CREATE TABLE 语句。由本模块创建的表为自动生成; 打开已有库时原样保留。
        ''' </summary>
        Public ReadOnly Property Sql As String
            Get
                If Not String.IsNullOrEmpty(_originalSql) Then
                    Return _originalSql
                End If

                Return GenerateSql()
            End Get
        End Property

        Friend Sub SetOriginalSql(sql As String)
            _originalSql = sql
        End Sub

        ''' <summary>按列名获取列序号(忽略大小写), 找不到返回 -1</summary>
        Public Function GetOrdinal(name As String) As Integer
            For i As Integer = 0 To _columns.Count - 1
                If String.Equals(_columns(i).Name, name, StringComparison.OrdinalIgnoreCase) Then
                    Return i
                End If
            Next

            Return -1
        End Function

        ''' <summary>
        ''' 追加一行数据。当表存在 INTEGER PRIMARY KEY 列时, 若该列提供了整数值则作为 rowid, 否则自增分配。
        ''' </summary>
        ''' <returns>该行的 rowid</returns>
        Public Function AddRow(values As Object()) As Long
            Dim row As Object() = NormalizeValues(values)
            Dim rowId As Long

            If RowIdAliasOrdinal >= 0 Then
                Dim key As Object = row(RowIdAliasOrdinal)

                If IsIntegerValue(key) Then
                    rowId = Convert.ToInt64(key)
                ElseIf key Is Nothing Then
                    rowId = _nextRowId
                Else
                    Throw New ArgumentException($"表 [{TableName}] 的 INTEGER PRIMARY KEY 列必须为整数或 Nothing")
                End If
            Else
                rowId = _nextRowId
            End If

            Return AddRow(rowId, row)
        End Function

        ''' <summary>以指定 rowid 追加一行数据</summary>
        Public Function AddRow(rowId As Long, values As Object()) As Long
            If _rows.ContainsKey(rowId) Then
                Throw New ArgumentException($"rowid {rowId} 已经存在于表 [{TableName}] 之中")
            End If

            Dim row As Object() = NormalizeValues(values)

            If RowIdAliasOrdinal >= 0 Then
                row(RowIdAliasOrdinal) = rowId
            End If

            _rows(rowId) = row

            If rowId >= _nextRowId Then
                _nextRowId = rowId + 1
            End If

            Call _writer.MarkDirty()
            Return rowId
        End Function

        ''' <summary>按 rowid 整体更新一行</summary>
        Public Function UpdateRow(rowId As Long, values As Object()) As Boolean
            If Not _rows.ContainsKey(rowId) Then
                Return False
            End If

            Dim row As Object() = NormalizeValues(values)

            If RowIdAliasOrdinal >= 0 Then
                row(RowIdAliasOrdinal) = rowId
            End If

            _rows(rowId) = row
            Call _writer.MarkDirty()
            Return True
        End Function

        ''' <summary>按 rowid 更新指定列</summary>
        Public Function UpdateRow(rowId As Long, columnName As String, value As Object) As Boolean
            If Not _rows.ContainsKey(rowId) Then
                Return False
            End If

            Dim ordinal As Integer = GetOrdinal(columnName)

            If ordinal < 0 Then
                Throw New ArgumentException($"表 [{TableName}] 之中不存在列 [{columnName}]")
            End If

            ' INTEGER PRIMARY KEY 是 rowid 的别名, 不允许通过列更新改变 rowid
            _rows(rowId)(ordinal) = If(ordinal = RowIdAliasOrdinal, CObj(rowId), value)
            Call _writer.MarkDirty()
            Return True
        End Function

        ''' <summary>按主键值更新一行</summary>
        Public Function UpdateRowByPrimaryKey(key As Object, values As Object()) As Boolean
            Dim pk As Integer = PrimaryKeyOrdinal

            If pk < 0 Then
                Throw New InvalidOperationException($"表 [{TableName}] 没有定义主键")
            End If

            For Each kv As KeyValuePair(Of Long, Object()) In _rows
                If ValuesEqual(kv.Value(pk), key) Then
                    Return UpdateRow(kv.Key, values)
                End If
            Next

            Return False
        End Function

        ''' <summary>按 rowid 删除一行</summary>
        Public Function DeleteRow(rowId As Long) As Boolean
            If Not _rows.Remove(rowId) Then
                Return False
            End If

            Call _writer.MarkDirty()
            Return True
        End Function

        ''' <summary>按主键值删除一行</summary>
        Public Function DeleteRowByPrimaryKey(key As Object) As Boolean
            Dim pk As Integer = PrimaryKeyOrdinal

            If pk < 0 Then
                Throw New InvalidOperationException($"表 [{TableName}] 没有定义主键")
            End If

            For Each kv As KeyValuePair(Of Long, Object()) In _rows
                If ValuesEqual(kv.Value(pk), key) Then
                    Return DeleteRow(kv.Key)
                End If
            Next

            Return False
        End Function

        ''' <summary>清空全部数据行</summary>
        Public Sub Clear()
            If _rows.Count > 0 Then
                _rows.Clear()
                Call _writer.MarkDirty()
            End If
        End Sub

        ''' <summary>按 rowid 读取一行</summary>
        Public Function GetRow(rowId As Long) As Sqlite3DataRow
            Dim values As Object() = Nothing

            If Not _rows.TryGetValue(rowId, values) Then
                Return Nothing
            End If

            Return New Sqlite3DataRow(Me, rowId, values)
        End Function

        ''' <summary>按 rowid 升序枚举全部数据行</summary>
        Public Iterator Function EnumerateRows() As IEnumerable(Of Sqlite3DataRow)
            For Each kv As KeyValuePair(Of Long, Object()) In _rows
                Yield New Sqlite3DataRow(Me, kv.Key, kv.Value)
            Next
        End Function

        ''' <summary>构建提交快照(记录体编码)</summary>
        Friend Function BuildSnapshot() As Internal.WriterTableSnapshot
            Dim entries As New List(Of Internal.LeafEntry)()
            Dim aliasOrdinal As Integer = RowIdAliasOrdinal

            For Each kv As KeyValuePair(Of Long, Object()) In _rows
                Dim record As Byte() = Internal.RecordEncoder.EncodeRecord(kv.Value, aliasOrdinal)
                entries.Add(New Internal.LeafEntry With {.RowId = kv.Key, .Record = record})
            Next

            Return New Internal.WriterTableSnapshot With {
                .Name = TableName,
                .Sql = Sql,
                .Entries = entries
            }
        End Function

        ''' <summary>载入一行已有数据(打开已有库时使用)</summary>
        Friend Sub LoadRow(rowId As Long, values As Object())
            _rows(rowId) = values

            If rowId >= _nextRowId Then
                _nextRowId = rowId + 1
            End If
        End Sub

        Private Function NormalizeValues(values As Object()) As Object()
            Dim result(_columns.Count - 1) As Object

            If values IsNot Nothing Then
                If values.Length > _columns.Count Then
                    Throw New ArgumentException($"行数据的列数({values.Length})多于表 [{TableName}] 的列数({_columns.Count})")
                End If

                Call Array.Copy(values, result, values.Length)
            End If

            Return result
        End Function

        Private Function GenerateSql() As String
            Dim parts As New List(Of String)()

            For Each c As Sqlite3Column In _columns
                parts.Add(c.ToSql())
            Next

            Dim primaryKeys As Sqlite3Column() = _columns.Where(Function(c) c.PrimaryKey).ToArray()

            ' 单个 INTEGER PRIMARY KEY 已在列定义之中表达(rowid 别名), 不需要额外的表级约束
            Dim expressedInline As Boolean = primaryKeys.Length = 1 AndAlso primaryKeys(0).IsRowIdAlias()

            If primaryKeys.Length > 0 AndAlso Not expressedInline Then
                parts.Add("PRIMARY KEY (" & String.Join(", ", primaryKeys.Select(Function(c) Sqlite3Column.EscapeName(c.Name))) & ")")
            End If

            Return "CREATE TABLE " & Sqlite3Column.EscapeName(TableName) & " (" & String.Join(", ", parts) & ")"
        End Function

        Private Shared Function IsIntegerValue(value As Object) As Boolean
            Return TypeOf value Is Long OrElse TypeOf value Is Integer OrElse TypeOf value Is Short OrElse TypeOf value Is Byte
        End Function

        Private Shared Function ValuesEqual(a As Object, b As Object) As Boolean
            If a Is Nothing AndAlso b Is Nothing Then
                Return True
            End If

            If a Is Nothing OrElse b Is Nothing Then
                Return False
            End If

            Return Equals(a, b)
        End Function

        Public Overrides Function ToString() As String
            Return $"[{TableName}] {RowCount} rows"
        End Function

    End Class

End Namespace

