Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Tables
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Writer.Internal

Namespace Writer

    ''' <summary>
    ''' SQLite3 数据库写入器。
    ''' 
    ''' 采用"内存模型 + 提交时整文件重建"的策略: 全部增删改在内存之中完成,
    ''' 调用 <see cref="Commit"/> 时一次性序列化重建整个数据库文件, 并以临时文件原子替换目标文件,
    ''' 保证写入失败不会损坏原库。构建之后可以被本模块的读取器重新打开。
    ''' </summary>
    Public Class Sqlite3Writer : Implements IDisposable

        Private ReadOnly _tables As New List(Of Sqlite3TableWriter)()

        Private _path As String
        Private _pageSize As Integer = 4096
        Private _reservedSpace As Integer = 0
        Private _changeCounter As Long = 1
        Private _dirty As Boolean
        Private _disposed As Boolean

        Private Sub New()
        End Sub

        ''' <summary>目标数据库文件路径</summary>
        Public ReadOnly Property FilePath As String
            Get
                Return _path
            End Get
        End Property

        ''' <summary>页大小(字节)</summary>
        Public ReadOnly Property PageSize As Integer
            Get
                Return _pageSize
            End Get
        End Property

        ''' <summary>Dispose 时是否自动提交(默认 True)</summary>
        Public Property AutoCommitOnDispose As Boolean = True

        ''' <summary>
        ''' 从零创建一个新的数据库文件(默认页大小 4096), 并立即写入一个合法的空库。
        ''' </summary>
        Public Shared Function CreateFile(path As String, Optional pageSize As Integer = 4096) As Sqlite3Writer
            If String.IsNullOrWhiteSpace(path) Then
                Throw New ArgumentException("数据库文件路径不能为空", NameOf(path))
            End If

            Dim writer As New Sqlite3Writer()
            writer._path = path
            writer._pageSize = If(pageSize = 65536, 65536, pageSize)
            writer._reservedSpace = 0
            writer._changeCounter = 1
            writer.Commit()

            Return writer
        End Function

        ''' <summary>
        ''' 打开一个已有的数据库文件, 把它加载为内存模型后即可执行增删改查。
        ''' </summary>
        Public Shared Function OpenFile(path As String) As Sqlite3Writer
            If Not File.Exists(path) Then
                Throw New FileNotFoundException("数据库文件不存在: " & path, path)
            End If

            Dim writer As New Sqlite3Writer()
            writer._path = path

            Using db As Sqlite3Database = Sqlite3Database.OpenFile(path, New Sqlite3Settings())
                writer._pageSize = db.Header.PageSize
                writer._reservedSpace = db.Header.ReservedSpaceAtEndOfPage
                writer._changeCounter = CLng(db.Header.ChangeCounter)

                For Each schemaRow As Sqlite3SchemaRow In db.GetTables
                    If Not String.Equals(schemaRow.type, "table", StringComparison.OrdinalIgnoreCase) Then
                        Continue For
                    End If

                    Dim table As Sqlite3Table = db.GetTable(schemaRow.tableName)
                    Dim primaryKeys As String() = table.schema.PrimaryKeys
                    Dim columns As New List(Of Sqlite3Column)()

                    For Each column As NamedValue(Of String) In table.schema.columns
                        Dim name As String = column.Name
                        Dim declared As String = column.Value
                        Dim isPrimaryKey As Boolean =
                            primaryKeys.Any(Function(p) String.Equals(p, name, StringComparison.OrdinalIgnoreCase))

                        columns.Add(New Sqlite3Column With {
                            .Name = name,
                            .Type = declared,
                            .PrimaryKey = isPrimaryKey
                        })
                    Next

                    Dim tableWriter As New Sqlite3TableWriter(writer, schemaRow.tableName, columns)
                    Call tableWriter.SetOriginalSql(schemaRow.Sql)

                    For Each row As Sqlite3Row In table.EnumerateRows()
                        Dim values As Object() = DirectCast(row.ColumnData.Clone(), Object())
                        Call tableWriter.LoadRow(row.RowId, values)
                    Next

                    writer._tables.Add(tableWriter)
                Next
            End Using

            Return writer
        End Function

        ''' <summary>创建一个新的数据表并返回其可写模型</summary>
        Public Function CreateTable(name As String, columns As IEnumerable(Of Sqlite3Column)) As Sqlite3TableWriter
            If String.IsNullOrWhiteSpace(name) Then
                Throw New ArgumentException("表名不能为空", NameOf(name))
            End If

            If GetTableNames().Any(Function(x) String.Equals(x, name, StringComparison.OrdinalIgnoreCase)) Then
                Throw New InvalidOperationException("数据表已经存在: " & name)
            End If

            Dim columnList As Sqlite3Column() = columns.ToArray()

            If columnList.Length = 0 Then
                Throw New ArgumentException("数据表至少需要定义一列", NameOf(columns))
            End If

            Dim tableWriter As New Sqlite3TableWriter(Me, name, columnList)
            _tables.Add(tableWriter)
            _dirty = True

            Return tableWriter
        End Function

        ''' <summary>获取指定名称的表</summary>
        Public Function GetTable(name As String) As Sqlite3TableWriter
            For Each table As Sqlite3TableWriter In _tables
                If String.Equals(table.TableName, name, StringComparison.OrdinalIgnoreCase) Then
                    Return table
                End If
            Next

            Throw New InvalidOperationException("找不到数据表: " & name)
        End Function

        ''' <summary>按创建顺序返回全部表名</summary>
        Public Function GetTableNames() As IEnumerable(Of String)
            Return _tables.Select(Function(t) t.TableName).ToArray()
        End Function

        ''' <summary>删除指定名称的表</summary>
        Public Function DropTable(name As String) As Boolean
            For i As Integer = 0 To _tables.Count - 1
                If String.Equals(_tables(i).TableName, name, StringComparison.OrdinalIgnoreCase) Then
                    _tables.RemoveAt(i)
                    _dirty = True
                    Return True
                End If
            Next

            Return False
        End Function

        ''' <summary>
        ''' 把当前内存模型整体提交到磁盘。可重复调用, 结果一致。
        ''' </summary>
        Public Sub Commit()
            If _disposed Then
                Throw New ObjectDisposedException(NameOf(Sqlite3Writer))
            End If

            Dim snapshots As New List(Of WriterTableSnapshot)()

            For Each table As Sqlite3TableWriter In _tables
                snapshots.Add(table.BuildSnapshot())
            Next

            _changeCounter += 1

            Dim serializer As New SqliteFileSerializer(_pageSize, _reservedSpace, _changeCounter)
            Dim bytes As Byte() = serializer.Serialize(snapshots)

            Call WriteAtomic(_path, bytes)
            _dirty = False
        End Sub

        ''' <summary>标记存在未提交的修改(供表模型回调)</summary>
        Friend Sub MarkDirty()
            _dirty = True
        End Sub

        ''' <summary>是否存在未提交的修改</summary>
        Public ReadOnly Property IsDirty As Boolean
            Get
                Return _dirty
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then
                Return
            End If

            _disposed = True

            If AutoCommitOnDispose AndAlso _dirty Then
                Call Commit()
            End If
        End Sub

        Private Shared Sub WriteAtomic(path As String, bytes As Byte())
            Dim directory As String = System.IO.Path.GetDirectoryName(path)

            If Not String.IsNullOrEmpty(directory) Then
                Call System.IO.Directory.CreateDirectory(directory)
            End If

            Dim tempPath As String = path & ".tmp"

            Try
                File.WriteAllBytes(tempPath, bytes)

                If File.Exists(path) Then
                    File.Replace(tempPath, path, Nothing)
                Else
                    File.Move(tempPath, path)
                End If
            Catch
                If File.Exists(tempPath) Then
                    Try
                        File.Delete(tempPath)
                    Catch
                        ' 忽略清理失败
                    End Try
                End If

                Throw
            End Try
        End Sub

    End Class

End Namespace
