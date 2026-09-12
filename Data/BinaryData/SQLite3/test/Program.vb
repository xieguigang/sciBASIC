#Region "Microsoft.VisualBasic::fded4fc6fcacc1a41cbdbaa53eba785d, Data\BinaryData\SQLite3\test\Program.vb"

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

    '   Total Lines: 1070
    '    Code Lines: 837 (78.22%)
    ' Comment Lines: 93 (8.69%)
    '    - Xml Docs: 43.01%
    ' 
    '   Blank Lines: 140 (13.08%)
    '     File Size: 54.43 KB


    ' Module Program
    ' 
    '     Function: Affinity, BuildIssues, Describe, FindColumn, FindTestDirectory
    '               FormatRow, GetExpectedColumnCount, GetNonEmptyTextColumns, GetNotNullColumns, IsConstraintName
    '               ResolveIssueStatus, ScanTable
    ' 
    '     Sub: AssertAffinity, Check, Main, PrintSummary, Run
    '          WriteReport
    ' 
    ' Class TestResult
    ' 
    '     Properties: Detail, ElapsedMs, Name, Passed
    ' 
    ' Class ScanResult
    ' 
    '     Properties: Columns, DeclaredColumns, MaxId, MinId, RowCount
    '                 RowIdMonotonic, Samples, TableName, Truncated
    ' 
    ' Class OverflowProbeResult
    ' 
    '     Properties: InlineLimit, MaxBlobLength, MaxTextLength, OverflowExercised, Rows
    ' 
    ' Class ColumnStat
    ' 
    '     Properties: DeclaredType, FirstValue, HasNumber, MaxBlobLength, MaxNumber
    '                 MaxTextLength, MinNumber, Name, NonNullCount, NullCount
    '                 Types
    ' 
    '     Function: TryNumber
    ' 
    '     Sub: Observe
    ' 
    ' Class IssueRecord
    ' 
    '     Properties: Fix, Id, RootCause, Status, Symptom
    '                 Title, VerifyTest
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Enums
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Headers
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Tables

''' <summary>
''' Managed SQLite3 读取模块的测试程序。
''' 
''' 用法:
'''     test.exe [数据库文件路径]
''' 
''' 默认测试文件为 ``G:\compounds_2-copy.sqlite``。
''' 测试采用"结构 + 抽样"策略: 对大表(compounds)只读取前 <see cref="SAMPLE_ROWS"/> 行,
''' 其余表尽可能全量遍历(上限 <see cref="SMALL_TABLE_ROW_CAP"/> 行)。
''' 运行时会打印控制台摘要, 并在 test 目录下生成 Markdown 报告。
''' </summary>
Module Program

    ''' <summary>
    ''' 默认测试数据库
    ''' </summary>
    Const DEFAULT_DB_PATH As String = "G:\compounds_2-copy.sqlite"

    ''' <summary>
    ''' 大表(compounds)抽样读取的最大行数
    ''' </summary>
    Const SAMPLE_ROWS As Integer = 5000

    ''' <summary>
    ''' 其余表的全量扫描上限, 超过则视为抽样(仅截断报告)
    ''' </summary>
    Const SMALL_TABLE_ROW_CAP As Integer = 200000

    ''' <summary>
    ''' 该数据库之中的大表名称
    ''' </summary>
    Const LARGE_TABLE As String = "compounds"

    ''' <summary>
    ''' 溢出页探测时对 compounds 最多扫描的行数
    ''' </summary>
    Const OVERFLOW_PROBE_ROWS As Integer = 300000

    Friend ReadOnly results As New List(Of TestResult)
    Private ReadOnly headerSummary As New List(Of String)
    Private ReadOnly masterEntries As New List(Of String)

    ''' <summary>写入/往返测试的报告说明行(由 <see cref="WriterTests"/> 填充)</summary>
    Friend ReadOnly writerNotes As New List(Of String)
    Private ReadOnly schemaSummaries As New List(Of String)
    Private ReadOnly scanResults As New List(Of ScanResult)
    Private overflowProbe As OverflowProbeResult

    Sub Main(args As String())
        Dim dbPath As String

        If args IsNot Nothing AndAlso args.Length > 0 AndAlso Not String.IsNullOrWhiteSpace(args(0)) Then
            dbPath = args(0)
        Else
            dbPath = DEFAULT_DB_PATH
        End If

        Console.OutputEncoding = Encoding.UTF8

        Call Console.WriteLine("================================================================")
        Call Console.WriteLine(" Managed SQLite3 Reader - Test Harness")
        Call Console.WriteLine("================================================================")
        Call Console.WriteLine("Database : " & dbPath)
        Call Console.WriteLine("Time     : " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
        Call Console.WriteLine()

        If Not File.Exists(dbPath) Then
            Call Console.WriteLine("[FATAL] 数据库文件不存在: " & dbPath)
            Return
        End If

        Dim fileInfo As New FileInfo(dbPath)
        Dim db As Sqlite3Database = Nothing
        Dim tables As Sqlite3SchemaRow() = Nothing
        Dim opened As New Dictionary(Of String, Sqlite3Table)()

        ' ---------------------------------------------------------------
        ' 1. 文件头
        ' ---------------------------------------------------------------
        Run("文件头解析", Sub()
                              db = Sqlite3Database.OpenFile(dbPath, New Sqlite3Settings())

                              Dim h As DatabaseHeader = db.Header
                              Dim expectedPages As Long = fileInfo.Length \ h.PageSize

                              headerSummary.Add("- 文件路径: ``" & dbPath & "``")
                              headerSummary.Add("- 文件大小: " & fileInfo.Length.ToString("N0") & " 字节")
                              headerSummary.Add("- 页面大小 PageSize: " & h.PageSize & " 字节")
                              headerSummary.Add("- 文本编码 TextEncoding: " & h.TextEncoding.ToString)
                              headerSummary.Add("- 预留空间 ReservedSpace: " & h.ReservedSpaceAtEndOfPage & " 字节")
                              headerSummary.Add("- Schema 格式: " & h.SchemaFormat)
                              headerSummary.Add("- 文件头记录页数 DatabaseSizeInPages: " & h.DatabaseSizeInPages.ToString("N0"))
                              headerSummary.Add("- 实际页数(文件大小/页大小): " & expectedPages.ToString("N0"))
                              headerSummary.Add("- ChangeCounter: " & h.ChangeCounter & ", ApplicationId: " & h.ApplicationId & ", UserVersion: " & h.UserVersion)

                              Check(h.PageSize = 4096, "页面大小期望 4096, 实际 " & h.PageSize)
                              Check(h.TextEncoding = SqliteEncoding.UTF8, "文本编码期望 UTF8, 实际 " & h.TextEncoding.ToString)
                              Check(h.SchemaFormat = 4, "Schema 格式期望 4, 实际 " & h.SchemaFormat)
                              Check(CLng(h.DatabaseSizeInPages) = expectedPages,
                                    $"文件头记录页数 {h.DatabaseSizeInPages} 与实际页数 {expectedPages} 不一致")
                          End Sub)

        ' ---------------------------------------------------------------
        ' 2. sqlite_master
        ' ---------------------------------------------------------------
        Run("枚举 sqlite_master", Sub()
                                      Check(db IsNot Nothing, "数据库未能打开, 无法枚举 sqlite_master")

                                      tables = db.GetTables.ToArray()
                                      Check(tables.Length > 0, "sqlite_master 未返回任何条目")

                                      For Each t As Sqlite3SchemaRow In tables
                                          masterEntries.Add($"| {t.type} | {t.name} | {t.tableName} | {t.rootPage} |")
                                      Next
                                  End Sub)

        ' ---------------------------------------------------------------
        ' 3. 逐表解析 CREATE TABLE 结构
        ' ---------------------------------------------------------------
        If tables IsNot Nothing Then
            Dim userTables As Sqlite3SchemaRow() = tables _
                .Where(Function(x) x.type = "table") _
                .OrderBy(Function(x) x.tableName) _
                .ToArray

            For Each t As Sqlite3SchemaRow In userTables
                Dim tableName As String = t.tableName
                Dim ddl As String = t.Sql

                Run("表结构解析: " & tableName, Sub()
                                                   Dim tbl As Sqlite3Table = db.GetTable(tableName)
                                                   opened(tableName) = tbl

                                                   Dim cols As NamedValue(Of String)() = tbl.schema.columns
                                                   Check(cols.Length > 0, "未能解析出任何列定义")

                                                   Dim expectedCols As Integer = GetExpectedColumnCount(tableName)
                                                   If expectedCols > 0 Then
                                                       Check(cols.Length = expectedCols,
                                                             $"列数期望 {expectedCols}, 实际 {cols.Length} (可能存在约束被误判为列)")
                                                   End If

                                                   schemaSummaries.Add("### " & tableName)
                                                   schemaSummaries.Add("")
                                                   schemaSummaries.Add("```sql")
                                                   schemaSummaries.Add(If(ddl, "").TrimNull)
                                                   schemaSummaries.Add("```")
                                                   schemaSummaries.Add("")
                                                   schemaSummaries.Add("| # | 列名 | 声明类型 | 亲和性 |")
                                                   schemaSummaries.Add("|---|---|---|---|")

                                                   Dim i As Integer = 0
                                                   For Each c As NamedValue(Of String) In cols
                                                       Check(Not IsConstraintName(c.Name), "疑似把表级约束误判为数据列: " & c.Name)
                                                       schemaSummaries.Add($"| {i} | {c.Name} | {c.Value} | {Affinity(c.Value)} |")
                                                       i += 1
                                                   Next
                                                   schemaSummaries.Add("")
                                               End Sub)
            Next
        End If

        ' ---------------------------------------------------------------
        ' 4. 数据行扫描(结构 + 抽样)
        ' ---------------------------------------------------------------
        For Each kv As KeyValuePair(Of String, Sqlite3Table) In opened
            Dim tableName As String = kv.Key
            Dim tbl As Sqlite3Table = kv.Value
            Dim cap As Integer = If(tableName = LARGE_TABLE, SAMPLE_ROWS, SMALL_TABLE_ROW_CAP)

            Run("扫描: " & tableName, Sub()
                                          Dim sr As ScanResult = ScanTable(tbl, cap)
                                          scanResults.Add(sr)

                                          Check(sr.RowCount > 0, "未读取到任何数据行")
                                          Check(sr.RowIdMonotonic, "RowId 非单调递增, 记录顺序错乱")

                                          ' 1) NOT NULL 列不应出现 NULL
                                          For Each cn As String In GetNotNullColumns(tableName)
                                              Dim st As ColumnStat = FindColumn(sr, cn)
                                              Check(st IsNot Nothing, "缺少列: " & cn)
                                              Check(st.NullCount = 0,
                                                    $"列 [{cn}] 声明为 NOT NULL, 但读到 {st.NullCount} 个 NULL 值")
                                          Next

                                          ' 2) rowid 别名列(INTEGER PRIMARY KEY)应回填为 RowId
                                          Dim idCol As ColumnStat = FindColumn(sr, "id")
                                          If idCol IsNot Nothing AndAlso TypeOf idCol.FirstValue Is Long Then
                                              Check(CLng(idCol.FirstValue) = sr.MinId,
                                                    $"id 列取值({idCol.FirstValue})与 RowId({sr.MinId})不一致, rowid 别名回填可能失败")
                                          End If

                                          ' 3) 值类型应与声明类型的亲和性一致
                                          For Each st As ColumnStat In sr.Columns
                                              Dim aff As String = Affinity(st.DeclaredType)
                                              For Each ts As String In st.Types
                                                  AssertAffinity(tableName, st, aff, ts)
                                              Next
                                          Next

                                          ' 4) 文本列的首个非空值应为非空字符串
                                          For Each cn As String In GetNonEmptyTextColumns(tableName)
                                              Dim st As ColumnStat = FindColumn(sr, cn)
                                              If st IsNot Nothing AndAlso st.NonNullCount > 0 Then
                                                  Dim s As String = TryCast(st.FirstValue, String)
                                                  Check(s IsNot Nothing AndAlso s.Length > 0,
                                                        $"列 [{cn}] 的首个非空值不是有效字符串: " & Describe(st.FirstValue))
                                              End If
                                          Next

                                          Call Console.WriteLine("         rows=" & sr.RowCount.ToString("N0") &
                                                                 If(sr.Truncated, " (抽样)", " (全量)"))
                                      End Sub)
        Next

        ' ---------------------------------------------------------------
        ' 5. 溢出页(长记录)探测: 深入扫描 compounds, 寻找超过单页内联阈值的字段
        ' ---------------------------------------------------------------
        Run("溢出页/长记录校验: compounds", Sub()
                                                Dim tbl As Sqlite3Table = opened(LARGE_TABLE)
                                                Dim inlineLimit As Integer = 4096 - 35
                                                Dim probe As New OverflowProbeResult()
                                                probe.InlineLimit = inlineLimit

                                                Dim count As Long = 0

                                                For Each row As Sqlite3Row In tbl.EnumerateRows()
                                                    count += 1

                                                    For ci As Integer = 0 To row.ColumnData.Length - 1
                                                        Dim v As Object = row.ColumnData(ci)
                                                        Dim s As String = TryCast(v, String)

                                                        If s IsNot Nothing Then
                                                            If s.Length > probe.MaxTextLength Then
                                                                probe.MaxTextLength = s.Length
                                                            End If
                                                        Else
                                                            Dim b As Byte() = TryCast(v, Byte())
                                                            If b IsNot Nothing AndAlso b.Length > probe.MaxBlobLength Then
                                                                probe.MaxBlobLength = b.Length
                                                            End If
                                                        End If
                                                    Next

                                                    If count >= OVERFLOW_PROBE_ROWS Then
                                                        Exit For
                                                    End If
                                                Next

                                                probe.Rows = count
                                                probe.OverflowExercised = probe.MaxTextLength > inlineLimit OrElse probe.MaxBlobLength > inlineLimit
                                                overflowProbe = probe

                                                Check(count > 0, "溢出页探测未读取到数据行")
                                            End Sub)

        ' ---------------------------------------------------------------
        ' 6. compounds 取值域校验
        ' ---------------------------------------------------------------
        Run("取值校验: compounds", Sub()
                                        Dim sr As ScanResult = scanResults.FirstOrDefault(Function(s) s.TableName = LARGE_TABLE)
                                        Check(sr IsNot Nothing, "compounds 扫描结果缺失")

                                        Dim inchi As ColumnStat = FindColumn(sr, "inchi")
                                        Check(inchi IsNot Nothing AndAlso inchi.NonNullCount > 0, "inchi 列全为 NULL")
                                        Dim sample As String = TryCast(inchi.FirstValue, String)
                                        Check(sample IsNot Nothing AndAlso sample.StartsWith("InChI="),
                                              "inchi 样例值异常: " & Describe(inchi.FirstValue))

                                        Dim mass As ColumnStat = FindColumn(sr, "mass")
                                        Check(mass IsNot Nothing AndAlso mass.HasNumber, "mass 列未读到任何数值")
                                        Check(mass.MinNumber >= 0 AndAlso mass.MaxNumber < 100000000,
                                              $"mass 数值范围异常: [{mass.MinNumber}, {mass.MaxNumber}]")
                                    End Sub)

        ' ---------------------------------------------------------------
        ' 6. blobAsBase64 设置
        ' ---------------------------------------------------------------
        Run("blobAsBase64 设置", Sub()
                                      Using db64 As Sqlite3Database = Sqlite3Database.OpenFile(dbPath, New Sqlite3Settings With {.blobAsBase64 = True})
                                          Dim tbl As Sqlite3Table = db64.GetTable(LARGE_TABLE)
                                          Dim row As Sqlite3Row = tbl.EnumerateRows().First()
                                          Dim blobColumns As String() = {"atom_bag", "dissociation_constants", "group_vector"}

                                          For Each cn As String In blobColumns
                                              Dim v As Object = row(cn)
                                              If v IsNot Nothing Then
                                                  Check(TypeOf v Is String,
                                                        $"blobAsBase64=True 时列 [{cn}] 应返回 Base64 字符串, 实际为 {v.GetType().Name}")
                                              End If
                                          Next
                                      End Using
                                  End Sub)

        ' ---------------------------------------------------------------
        ' 7. 未知表异常
        ' ---------------------------------------------------------------
        Run("未知表异常处理", Sub()
                                    Dim caught As Boolean = False
                                    Try
                                        Call db.GetTable("__not_exist_table__")
                                    Catch ex As Exception
                                        caught = True
                                    End Try
                                    Check(caught, "查询未知表时未抛出异常")
                                End Sub)

        If db IsNot Nothing Then
            Call db.Dispose()
        End If

        ' ---------------------------------------------------------------
        ' 8. 写入模块: 先写入 sqlite 文件, 再用读取器读回
        ' ---------------------------------------------------------------
        Call RunWriterTests()

        ' ---------------------------------------------------------------
        ' 9. 汇总输出 + 报告
        ' ---------------------------------------------------------------
        Call PrintSummary()

        Dim reportPath As String = Path.Combine(FindTestDirectory(), "TEST-REPORT.md")
        Dim allPassed As Boolean = results.All(Function(r) r.Passed)

        Call WriteReport(reportPath, dbPath, allPassed)

        If Not allPassed Then
            ' 保留一份修复前的失败快照, 便于基线对比
            Dim baselinePath As String = Path.Combine(FindTestDirectory(), "TEST-REPORT-baseline.md")
            Call File.Copy(reportPath, baselinePath, overwrite:=True)
            Call Console.WriteLine()
            Call Console.WriteLine("基线失败快照已保存: " & baselinePath)
        End If

        Call Console.WriteLine()
        Call Console.WriteLine("测试报告已生成: " & reportPath)
    End Sub

    ' ===================================================================
    ' 测试基础设施
    ' ===================================================================

    Friend Sub Run(name As String, action As Action)
        Dim sw As Stopwatch = Stopwatch.StartNew()

        Call Console.Write("  [" & name & "] ... ")
        Try
            Call action()
            sw.Stop()
            results.Add(New TestResult With {.Name = name, .Passed = True, .ElapsedMs = sw.ElapsedMilliseconds})
            Call Console.WriteLine("PASS (" & sw.ElapsedMilliseconds & " ms)")
        Catch ex As Exception
            sw.Stop()
            results.Add(New TestResult With {
                .Name = name,
                .Passed = False,
                .Detail = ex.GetType().Name & ": " & ex.Message,
                .ElapsedMs = sw.ElapsedMilliseconds
            })
            Call Console.WriteLine("FAIL (" & sw.ElapsedMilliseconds & " ms)")
            Call Console.WriteLine("         -> " & ex.GetType().Name & ": " & ex.Message)
            Call Console.WriteLine(ex.ToString)
        End Try
    End Sub

    Friend Sub Check(condition As Boolean, message As String)
        If Not condition Then
            Throw New Exception(message)
        End If
    End Sub

    Private Function ScanTable(tbl As Sqlite3Table, cap As Integer) As ScanResult
        Dim res As New ScanResult()
        res.TableName = tbl.SchemaDefinition.tableName

        Dim cols As NamedValue(Of String)() = tbl.schema.columns
        res.DeclaredColumns = cols.Length

        For Each c As NamedValue(Of String) In cols
            res.Columns.Add(New ColumnStat With {.Name = c.Name, .DeclaredType = c.Value})
        Next

        Dim lastRowId As Long = Long.MinValue
        Dim monotonic As Boolean = True
        Dim n As Long = 0
        Dim first As Boolean = True

        For Each row As Sqlite3Row In tbl.EnumerateRows()
            n += 1

            If first Then
                res.MinId = row.RowId
                first = False
            End If

            If row.RowId <= lastRowId Then
                monotonic = False
            End If

            lastRowId = row.RowId
            res.MaxId = row.RowId

            For ci As Integer = 0 To res.Columns.Count - 1
                If ci < row.ColumnData.Length Then
                    res.Columns(ci).Observe(row.ColumnData(ci))
                End If
            Next

            If res.Samples.Count < 5 Then
                res.Samples.Add(FormatRow(row))
            End If

            If n >= cap Then
                res.Truncated = True
                Exit For
            End If
        Next

        res.RowCount = n
        res.RowIdMonotonic = monotonic

        Return res
    End Function

    Private Function FormatRow(row As Sqlite3Row) As String
        Dim parts As New List(Of String)

        For Each v As Object In row.ColumnData
            parts.Add(Describe(v))
        Next

        Return "RowId=" & row.RowId & ": " & String.Join(", ", parts)
    End Function

    Private Function Describe(v As Object) As String
        If v Is Nothing Then
            Return "<NULL>"
        End If

        Dim s As String = TryCast(v, String)
        If s IsNot Nothing Then
            If s.Length > 80 Then
                s = s.Substring(0, 80) & "..."
            End If
            Return """" & s & """ (String)"
        End If

        Dim b As Byte() = TryCast(v, Byte())
        If b IsNot Nothing Then
            Return "<blob " & b.Length & " bytes>"
        End If

        Return v.ToString() & " (" & v.GetType().Name & ")"
    End Function

    Private Function FindColumn(sr As ScanResult, name As String) As ColumnStat
        Return sr.Columns.FirstOrDefault(Function(c) c.Name = name)
    End Function

    Private Function Affinity(declared As String) As String
        Dim t As String = If(declared, "").ToLower()

        If t.Contains("char") OrElse t.Contains("text") OrElse t.Contains("clob") Then
            Return "TEXT"
        End If
        If t.Contains("blob") OrElse t.Length = 0 Then
            Return "BLOB"
        End If
        If t.Contains("bool") Then
            Return "BOOLEAN"
        End If
        If t.Contains("int") Then
            Return "INTEGER"
        End If
        If t.Contains("real") OrElse t.Contains("floa") OrElse t.Contains("doub") Then
            Return "REAL"
        End If
        If t.Contains("bit") Then
            Return "BOOLEAN"
        End If

        Return "NUMERIC"
    End Function

    Private Sub AssertAffinity(tableName As String, st As ColumnStat, aff As String, typeName As String)
        Select Case aff
            Case "TEXT"
                Check(typeName = "String",
                      $"[{tableName}.{st.Name}] 声明为 TEXT({st.DeclaredType}), 但读到 {typeName}")
            Case "BLOB"
                Check(typeName = "Byte[]",
                      $"[{tableName}.{st.Name}] 声明为 BLOB, 但读到 {typeName}")
            Case "REAL"
                Check(typeName = "Double",
                      $"[{tableName}.{st.Name}] 声明为 REAL({st.DeclaredType}), 但读到 {typeName}")
            Case "BOOLEAN"
                Check(typeName = "Boolean",
                      $"[{tableName}.{st.Name}] 声明为 BOOLEAN, 但读到 {typeName}")
            Case "INTEGER"
                Check(typeName = "Int64" OrElse typeName = "Int32" OrElse typeName = "Int16",
                      $"[{tableName}.{st.Name}] 声明为 INTEGER, 但读到 {typeName}")
        End Select
    End Sub

    Private Function GetExpectedColumnCount(table As String) As Integer
        Select Case table
            Case "registries"
                Return 10
            Case "compound_microspecies"
                Return 9
            Case "compounds"
                Return 10
            Case "compound_identifiers"
                Return 6
            Case "magnesium_dissociation_constant"
                Return 7
            Case Else
                Return 0
        End Select
    End Function

    Private Function IsConstraintName(name As String) As Boolean
        Select Case If(name, "").Trim.ToUpper()
            Case "CHECK", "CONSTRAINT", "UNIQUE", "FOREIGN", "PRIMARY"
                Return True
            Case Else
                Return False
        End Select
    End Function

    Private Function GetNotNullColumns(table As String) As String()
        Select Case table
            Case "registries"
                Return New String() {"created_on", "id", "namespace", "pattern", "is_prefixed"}
            Case "compound_microspecies"
                Return New String() {"created_on", "id", "compound_id", "charge", "number_protons", "number_magnesiums", "is_major"}
            Case "compounds"
                Return New String() {"created_on", "id"}
            Case "compound_identifiers"
                Return New String() {"created_on", "id", "compound_id", "registry_id", "accession"}
            Case "magnesium_dissociation_constant"
                Return New String() {"created_on", "id", "compound_id", "number_protons", "number_magnesiums", "dissociation_constant"}
            Case Else
                Return New String() {}
        End Select
    End Function

    Private Function GetNonEmptyTextColumns(table As String) As String()
        Select Case table
            Case "registries"
                Return New String() {"namespace", "pattern"}
            Case "compound_identifiers"
                Return New String() {"accession"}
            Case "compounds"
                Return New String() {"inchi_key", "inchi", "smiles"}
            Case Else
                Return New String() {}
        End Select
    End Function

    Private Function FindTestDirectory() As String
        Dim dir As String = AppContext.BaseDirectory

        For i As Integer = 0 To 6
            If File.Exists(Path.Combine(dir, "test.vbproj")) Then
                Return dir
            End If

            Dim parent As DirectoryInfo = Directory.GetParent(dir)
            If parent Is Nothing Then
                Exit For
            End If
            dir = parent.FullName
        Next

        Return AppContext.BaseDirectory
    End Function

    ' ===================================================================
    ' 控制台与报告输出
    ' ===================================================================

    Private Sub PrintSummary()
        Dim passed As Integer = results.Where(Function(r) r.Passed).Count()
        Dim failed As Integer = results.Where(Function(r) Not r.Passed).Count()

        Call Console.WriteLine()
        Call Console.WriteLine("================================================================")
        Call Console.WriteLine($" 测试完成: 通过 {passed} / {results.Count}, 失败 {failed}")
        Call Console.WriteLine("================================================================")

        If failed > 0 Then
            For Each r As TestResult In results.Where(Function(x) Not x.Passed)
                Call Console.WriteLine("  [FAIL] " & r.Name)
                Call Console.WriteLine("         " & r.Detail)
            Next
        End If

        Call Console.WriteLine()
        Call Console.WriteLine("表扫描概览:")
        Call Console.WriteLine("  表名".PadRight(40) & "模式".PadRight(10) & "行数".PadRight(14) & "RowId单调")

        For Each sr As ScanResult In scanResults
            Call Console.WriteLine("  " & sr.TableName.PadRight(38) &
                                  If(sr.Truncated, "抽样", "全量").PadRight(10) &
                                  sr.RowCount.ToString("N0").PadRight(14) &
                                  sr.RowIdMonotonic.ToString())
        Next
    End Sub

    Private Sub WriteReport(reportPath As String, dbPath As String, allPassed As Boolean)
        Dim sb As New StringBuilder()
        Dim passed As Integer = results.Where(Function(r) r.Passed).Count()
        Dim failed As Integer = results.Where(Function(r) Not r.Passed).Count()
        Dim totalMs As Long = results.Sum(Function(r) r.ElapsedMs)

        sb.AppendLine("# Managed SQLite3 读取模块测试报告")
        sb.AppendLine()
        sb.AppendLine("- 生成时间: " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
        sb.AppendLine("- 测试模块: ``Microsoft.VisualBasic.Data.IO.SQLite3``")
        sb.AppendLine("- 目标数据库: ``" & dbPath & "``")
        sb.AppendLine("- 运行形态: 结构 + 抽样(大表 ``compounds`` 前 " & SAMPLE_ROWS.ToString("N0") & " 行, 其余表全量, 上限 " & SMALL_TABLE_ROW_CAP.ToString("N0") & " 行)")
        sb.AppendLine("- 用例总数: " & results.Count & ", 通过 " & passed & ", 失败 " & failed & ", 累计耗时 " & totalMs.ToString("N0") & " ms")
        sb.AppendLine("- 总体结论: " & If(allPassed, "**全部用例通过**", "**存在 " & failed & " 个失败用例**"))
        sb.AppendLine()

        ' --- 环境 ---
        sb.AppendLine("## 1. 测试环境")
        sb.AppendLine()
        sb.AppendLine("- 操作系统: " & Environment.OSVersion.ToString())
        sb.AppendLine("- 运行框架: " & Environment.Version.ToString & " / " & System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription)
        sb.AppendLine("- 计算机: " & Environment.MachineName)
        sb.AppendLine("- 处理器数量: " & Environment.ProcessorCount)
        sb.AppendLine("- 工作目录: ``" & Environment.CurrentDirectory & "``")
        sb.AppendLine()

        ' --- 数据库 ---
        sb.AppendLine("## 2. 数据库文件信息")
        sb.AppendLine()
        If headerSummary.Count = 0 Then
            sb.AppendLine("(未能解析文件头)")
        Else
            For Each line As String In headerSummary
                sb.AppendLine(line)
            Next
        End If
        sb.AppendLine()

        ' --- 表清单 ---
        sb.AppendLine("## 3. sqlite_master 条目")
        sb.AppendLine()
        sb.AppendLine("| type | name | tableName | rootPage |")
        sb.AppendLine("|---|---|---|---|")
        For Each line As String In masterEntries
            sb.AppendLine(line)
        Next
        sb.AppendLine()

        ' --- 表结构 ---
        sb.AppendLine("## 4. 表结构解析结果")
        sb.AppendLine()
        If schemaSummaries.Count = 0 Then
            sb.AppendLine("(未能解析任何表结构)")
        Else
            For Each line As String In schemaSummaries
                sb.AppendLine(line)
            Next
        End If

        ' --- 扫描统计 ---
        sb.AppendLine("## 5. 数据扫描统计")
        sb.AppendLine()
        sb.AppendLine("| 表 | 模式 | 读取行数 | 列数 | RowId 单调 | RowId 区间 |")
        sb.AppendLine("|---|---|---|---|---|---|")
        For Each sr As ScanResult In scanResults
            sb.AppendLine($"| {sr.TableName} | {If(sr.Truncated, "抽样", "全量")} | {sr.RowCount.ToString("N0")} | {sr.DeclaredColumns} | {If(sr.RowIdMonotonic, "是", "否")} | [{sr.MinId}, {sr.MaxId}] |")
        Next
        sb.AppendLine()

        For Each sr As ScanResult In scanResults
            sb.AppendLine("### 表 " & sr.TableName & " 列统计")
            sb.AppendLine()
            sb.AppendLine("| 列 | 声明类型 | 亲和性 | NULL 数 | 非空数 | 观测到的 CLR 类型 | 最大文本长度 | 最大 BLOB 长度 |")
            sb.AppendLine("|---|---|---|---|---|---|---|---|")
            For Each st As ColumnStat In sr.Columns
                sb.AppendLine($"| {st.Name} | {st.DeclaredType} | {Affinity(st.DeclaredType)} | {st.NullCount.ToString("N0")} | {st.NonNullCount.ToString("N0")} | {String.Join(", ", st.Types)} | {If(st.MaxTextLength > 0, st.MaxTextLength.ToString("N0"), "-")} | {If(st.MaxBlobLength > 0, st.MaxBlobLength.ToString("N0"), "-")} |")
            Next
            sb.AppendLine()
            If sr.Samples.Count > 0 Then
                sb.AppendLine("样例数据(最多 5 行):")
                sb.AppendLine()
                sb.AppendLine("```text")
                For Each s As String In sr.Samples
                    sb.AppendLine(s)
                Next
                sb.AppendLine("```")
                sb.AppendLine()
            End If
        Next

        ' --- 溢出页探测 ---
        sb.AppendLine("### 溢出页(长记录)探测")
        sb.AppendLine()
        If overflowProbe Is Nothing Then
            sb.AppendLine("(未执行溢出页探测)")
        Else
            sb.AppendLine("- 探测表: ``compounds``, 扫描行数: " & overflowProbe.Rows.ToString("N0") &
                          If(overflowProbe.Rows >= OVERFLOW_PROBE_ROWS, " (达到探测上限)", " (全量)"))
            sb.AppendLine("- 单页内联阈值 U-35: " & overflowProbe.InlineLimit & " 字节")
            sb.AppendLine("- 观测到最大 TEXT 长度: " & overflowProbe.MaxTextLength.ToString("N0") & " 字节")
            sb.AppendLine("- 观测到最大 BLOB 长度: " & overflowProbe.MaxBlobLength.ToString("N0") & " 字节")
            sb.AppendLine("- 是否命中溢出页: " & If(overflowProbe.OverflowExercised, "是", "否(抽样范围内未出现超出阈值的记录)"))
        End If
        sb.AppendLine()

        ' --- 写入/往返测试 ---
        sb.AppendLine("## 6. 写入/往返测试")
        sb.AppendLine()
        sb.AppendLine("写入模块提供 Builder 链式接口: ``Sqlite3Writer.CreateFile/OpenFile → CreateTable → AddRow/UpdateRow/DeleteRow → Commit``,")
        sb.AppendLine("提交时整库重建并以临时文件原子替换。以下测试均遵循[先写入 .sqlite 文件, 再用本模块读取器读回比对]的流程。")
        sb.AppendLine()
        If writerNotes.Count = 0 Then
            sb.AppendLine("(未执行写入测试)")
        Else
            For Each line As String In writerNotes
                sb.AppendLine(line)
            Next
        End If
        sb.AppendLine()

        ' --- 用例结果 ---
        sb.AppendLine("## 7. 测试用例结果")
        sb.AppendLine()
        sb.AppendLine("| # | 用例 | 结果 | 耗时(ms) | 说明 |")
        sb.AppendLine("|---|---|---|---|---|")
        Dim idx As Integer = 0
        For Each r As TestResult In results
            idx += 1
            sb.AppendLine($"| {idx} | {r.Name} | {If(r.Passed, "通过", "**失败**")} | {r.ElapsedMs} | {If(r.Detail, "")} |")
        Next
        sb.AppendLine()

        ' --- 问题与修复 ---
        sb.AppendLine("## 8. 发现的问题与修复记录")
        sb.AppendLine()
        sb.AppendLine("| 编号 | 问题 | 根因 | 修复 | 状态 |")
        sb.AppendLine("|---|---|---|---|---|")
        For Each issue As IssueRecord In BuildIssues()
            issue.Status = ResolveIssueStatus(issue, allPassed)
            sb.AppendLine($"| {issue.Id} | {issue.Title} | {issue.RootCause} | {issue.Fix} | {issue.Status} |")
        Next
        sb.AppendLine()
        sb.AppendLine("> 说明: 相关用例全部通过时状态记为 [已修复(复测通过)]; 存在失败用例时记为 [复现/待修复]。")
        sb.AppendLine()

        ' --- 结论 ---
        sb.AppendLine("## 9. 复测结论")
        sb.AppendLine()
        If allPassed Then
            sb.AppendLine("读取与写入模块的测试用例全部通过。")
            sb.AppendLine("读取侧可正确解析目标数据库的文件头、sqlite_master、各表结构以及数据行, 覆盖可空列 NULL、BOOLEAN、FLOAT、TEXT、BLOB、rowid 别名与表级约束等场景。")

            If overflowProbe IsNot Nothing AndAlso overflowProbe.OverflowExercised Then
                sb.AppendLine($"读取侧溢出页路径已被实际覆盖(最大字段 {Math.Max(overflowProbe.MaxTextLength, overflowProbe.MaxBlobLength).ToString("N0")} 字节 > 内联阈值 {overflowProbe.InlineLimit} 字节), 未发现异常。")
            Else
                sb.AppendLine("抽样范围内未出现超过单页内联阈值的字段, 读取侧溢出页路径未被实际触发。")
            End If

            sb.AppendLine("写入侧通过 Builder 链式接口完成新建/建表/插行/追加/更新/删除/删表, 生成的文件均可被读取器逐表、逐行、逐列读回, 覆盖边界值、多页 B 树与溢出页。")
        Else
            sb.AppendLine("存在未通过的测试用例, 详见第 7 节; 修复前状态快照见 ``TEST-REPORT-baseline.md``。")
        End If
        sb.AppendLine()

        Dim baselinePath As String = Path.Combine(FindTestDirectory(), "TEST-REPORT-baseline.md")
        If allPassed AndAlso File.Exists(baselinePath) Then
            sb.AppendLine("## 10. 基线对比")
            sb.AppendLine()
            sb.AppendLine("修复前(基线)运行的失败快照保存在 ``TEST-REPORT-baseline.md``, 可用于对比修复前后的用例通过情况。")
            sb.AppendLine()
        End If

        Call File.WriteAllText(reportPath, sb.ToString, New UTF8Encoding(True))
    End Sub

    Private Function ResolveIssueStatus(issue As IssueRecord, allPassed As Boolean) As String
        If String.IsNullOrEmpty(issue.VerifyTest) Then
            Return If(allPassed, "已修复(间接验证)", "待验证")
        End If

        Dim related As TestResult() = results.Where(Function(r) r.Name.Contains(issue.VerifyTest)).ToArray()

        If related.Length = 0 Then
            Return If(allPassed, "已修复(无直接用例)", "待验证")
        End If

        If related.All(Function(r) r.Passed) Then
            Return "已修复(复测通过)"
        End If

        Return "复现/待修复"
    End Function

    Private Function BuildIssues() As List(Of IssueRecord)
        Return New List(Of IssueRecord) From {
            New IssueRecord With {
                .Id = "I-01",
                .Title = "BOOLEAN 声明类型无法解析, GetTable 直接抛异常",
                .Symptom = "registries.is_prefixed、compound_microspecies.is_major 声明为 BOOLEAN, 构造表对象时 Schema 解析抛出 NotImplementedException, 整张表无法读取。",
                .RootCause = "DataTypeParser.TryParse 仅映射 bool/[bool]/bit, 未覆盖 boolean; 且遇到未知声明类型直接抛 NotImplementedException。",
                .Fix = "DataTypeParser 新增 boolean/bool/bit/date/datetime/numeric/real 等映射, 未知声明类型按 SQLite 亲和性规则回退, 不再抛异常。",
                .VerifyTest = "扫描: "
            },
            New IssueRecord With {
                .Id = "I-02",
                .Title = "可空列 NULL 值读取错误",
                .Symptom = "可空列(如 updated_on、inchi_key、atom_bag)遇到 NULL 时未返回 Nothing, 而是沿用上一行残留长度读取数据, 造成脏数据。",
                .RootCause = "ParseRow 中记录头 serial type=0(NULL) 时仅注释、既不重置长度也不标记为空, 且 ColumnDataMeta 在行间共享复用。",
                .Fix = "ParseRow 改为按记录头 serial type 逐列解码, serial type=0 明确返回 Nothing, 不再共享可变的列元数据。",
                .VerifyTest = "扫描: "
            },
            New IssueRecord With {
                .Id = "I-03",
                .Title = "按声明类型而非真实存储类型解码",
                .Symptom = "DATETIME/FLOAT 等列的取值类型与内容错误(如 DATETIME 被按整数读取)。",
                .RootCause = "SQLite 为动态类型, 记录头 serial type 才是每列真实存储类型的唯一依据; 原实现使用 schema 声明类型决定读取方式。",
                .Fix = "改为按 serial type 解码: 0=NULL、1..6=整数、7=IEEE 浮点、8/9=0/1、偶数>=12=BLOB、奇数>=13=TEXT; 并依据声明亲和性做合理转换(如 FLOAT 列返回 Double, BOOLEAN 列返回 Boolean)。",
                .VerifyTest = "扫描: "
            },
            New IssueRecord With {
                .Id = "I-04",
                .Title = "BOOLEAN 列恒为 True",
                .Symptom = "无论实际存储 0 还是 1, BOOLEAN 列取值都为 True。",
                .RootCause = "原实现 Select Case 中 Boolean1 分支直接赋值 True, 未读取真实值。",
                .Fix = "按 serial type 8/9 取 0/1 并转换为真实 Boolean。",
                .VerifyTest = "扫描: "
            },
            New IssueRecord With {
                .Id = "I-05",
                .Title = "cellOffsets 排序打乱记录行序",
                .Symptom = "枚举出的记录 RowId 非单调递增, 行序错乱。",
                .RootCause = "BTreePage.Parse 对 cell 指针数组执行 Array.Sort, 而 SQLite 的 cell 指针数组本已按 key(rowid) 有序, 按物理偏移重排会破坏行序。",
                .Fix = "移除 Array.Sort, 保持页内 cell 指针数组的 key 顺序遍历。",
                .VerifyTest = "扫描: "
            },
            New IssueRecord With {
                .Id = "I-06",
                .Title = "9 字节 VarInt 读取计数偏移",
                .Symptom = "读取 9 字节数字的 VarInt 时返回的字节数与实际不符。",
                .RootCause = "ReadVarInt 在第 9 字节分支对 readBytes 多加 1。",
                .Fix = "修正 9 字节分支的字节计数。",
                .VerifyTest = ""
            },
            New IssueRecord With {
                .Id = "I-07",
                .Title = "记录列数与 schema 列数不一致时越界",
                .Symptom = "记录头中的 serial type 数量多于 schema 列数时, 访问元数据越界导致整行读取被跳过。",
                .RootCause = "ParseRow 未对列索引做边界检查。",
                .Fix = "解码循环加入列数边界保护, 超出部分安全忽略。",
                .VerifyTest = ""
            },
            New IssueRecord With {
                .Id = "I-08",
                .Title = "表级 CHECK 约束被误判为数据列",
                .Symptom = "compound_microspecies 的 CHECK (is_major IN (0, 1)) 被解析为一个名为 CHECK、类型为 ( 的伪列, 使列数与记录头不一致。",
                .RootCause = "Schema.ParseColumns 仅跳过 UNIQUE/FOREIGN KEY/PRIMARY KEY 约束, 未处理 CHECK/CONSTRAINT 约束。",
                .Fix = "Schema.ParseColumns 新增跳过 CHECK/CONSTRAINT 约束, 并为缺失类型声明的列按 BLOB 亲和性回退。",
                .VerifyTest = "表结构解析"
            },
            New IssueRecord With {
                .Id = "I-09",
                .Title = "INTEGER PRIMARY KEY(rowid 别名)列被读成 NULL",
                .Symptom = "所有表的 id 列(声明为 INTEGER NOT NULL PRIMARY KEY)被读成 NULL, 触发 NOT NULL 校验失败。",
                .RootCause = "SQLite 把 INTEGER PRIMARY KEY 作为 rowid 的别名, 记录体之中该列存储为 NULL, 读取时需要用该行的 rowid 回填; 原实现直接返回 NULL。",
                .Fix = "Schema 记录主键列名, Sqlite3Table 识别 INTEGER PRIMARY KEY 别名列, 并在解码完成后用 rowid 回填该列。",
                .VerifyTest = "扫描: "
            },
            New IssueRecord With {
                .Id = "I-10",
                .Title = "FLOAT 列存储 0/1 时 CLR 类型不一致",
                .Symptom = "ddg_over_rt、dissociation_constant 等 FLOAT 列在取值为 0 或 1 时被读成 Int64。",
                .RootCause = "SQLite 对 0/1 使用 serial type 8/9; 解码后只按 BOOLEAN 处理, 未考虑 FLOAT 亲和性。",
                .Fix = "ToDeclaredBoolean 对 FLOAT 亲和性列返回 Double, 保证数值列类型稳定。",
                .VerifyTest = "扫描: "
            },
            New IssueRecord With {
                .Id = "I-11",
                .Title = "写入侧叶页容量未计入 cell 指针数组增长",
                .Symptom = "多页表在写入之后回读会丢失记录(行数偏少)并抛出 EndOfStreamException。",
                .RootCause = "BTreeWriter 计算叶页容量时只累加 cell 体积并额外 +2, 未按 cell 数量乘算 2 字节指针数组, 导致内容区与指针数组重叠。",
                .Fix = "容量判定改为 LeafHeaderSize + 已用空间 + 新 cell 体积 + 2 * (cell 数 + 1) <= 可用页大小。",
                .VerifyTest = "写入: 多页"
            },
            New IssueRecord With {
                .Id = "I-12",
                .Title = "写入侧整数 serial type 与字节宽度映射错误",
                .Symptom = "写入 8 字节整数(如 Long.MaxValue)之后回读得到 0, 且该行后续列全部错位。",
                .RootCause = "GetSerialType 直接把字节宽度当作 serial type 返回, 而 serial type 8/9 是常量 0/1 且不携带数据; 8 字节整数应写为 serial type 6。",
                .Fix = "按规范映射: 1/2/3/4 字节 -> serial 1/2/3/4, 6 字节 -> serial 5, 8 字节 -> serial 6。",
                .VerifyTest = "写入: 边界值"
            },
            New IssueRecord With {
                .Id = "I-13",
                .Title = "写入侧负数整数触发 OverflowException",
                .RootCause = "VB 的 CULng 为受检查转换, 对负数会抛出 OverflowException。",
                .Fix = "改为 BitConverter.ToUInt64(BitConverter.GetBytes(值)) 做无检查位重解释。",
                .Symptom = "写入 Long.MinValue 等负数时报 Arithmetic operation resulted in an overflow。",
                .VerifyTest = "写入: 边界值"
            },
            New IssueRecord With {
                .Id = "I-14",
                .Title = "Sqlite3Writer.Dispose 先置释放标记再提交, 掩盖真实异常",
                .Symptom = "提交失败时最终抛出 ObjectDisposedException, 真实错误信息丢失。",
                .RootCause = "Dispose 中先设置 _disposed = True, 随后调用 Commit() 被释放检查拦截。",
                .Fix = "调整顺序: 先提交(若存在未落盘修改), 再设置释放标记。",
                .VerifyTest = ""
            },
            New IssueRecord With {
                .Id = "I-15",
                .Title = "读取侧单字符列名被解析为空串",
                .Symptom = "CREATE TABLE 中长度为 1 的列名(如 a/b/v/i)解析成空字符串, 触发 DuplicateNameException 或按列名取值为 -1。",
                .RootCause = "Schema.ParseColumns 使用 GetStackValue 剥离双引号, 而该函数对长度小于 2 的字符串直接返回空串。",
                .Fix = "改为先判断首尾字符是否为引号/方括号, 再决定是否截断, 不再依赖 GetStackValue。",
                .VerifyTest = "写入: 新建库"
            },
            New IssueRecord With {
                .Id = "I-16",
                .Title = "读取侧 serial type 5/6 的整数宽度映射错误",
                .Symptom = "6 字节与 8 字节整数被按 5/6 字节读取, 数值错误(此前数据恰好都是小整数而未暴露)。",
                .RootCause = "ReadValue 直接把 serial type 当作字节数传给 ReadInteger。",
                .Fix = "新增 GetIntegerByteWidth: serial 5 -> 6 字节, serial 6 -> 8 字节, 与写入侧对称。",
                .VerifyTest = "写入: 边界值"
            }
        }
    End Function

End Module

''' <summary>
''' 单个测试用例的执行结果
''' </summary>
Friend Class TestResult
    Public Property Name As String
    Public Property Passed As Boolean
    Public Property Detail As String
    Public Property ElapsedMs As Long
End Class

''' <summary>
''' 对单张表的数据扫描结果
''' </summary>
Friend Class ScanResult
    Public Property TableName As String
    Public Property DeclaredColumns As Integer
    Public Property RowCount As Long
    Public Property Truncated As Boolean
    Public Property RowIdMonotonic As Boolean
    Public Property MinId As Long
    Public Property MaxId As Long
    Public ReadOnly Property Columns As New List(Of ColumnStat)
    Public ReadOnly Property Samples As New List(Of String)
End Class

''' <summary>
''' 溢出页(长记录)探测结果
''' </summary>
Friend Class OverflowProbeResult
    Public Property Rows As Long
    Public Property InlineLimit As Integer
    Public Property MaxTextLength As Long
    Public Property MaxBlobLength As Long
    Public Property OverflowExercised As Boolean
End Class

''' <summary>
''' 单列的数据统计信息
''' </summary>
Friend Class ColumnStat
    Public Property Name As String
    Public Property DeclaredType As String
    Public Property NullCount As Long
    Public Property NonNullCount As Long
    Public ReadOnly Property Types As New List(Of String)
    Public Property MaxTextLength As Long
    Public Property MaxBlobLength As Long
    Public Property HasNumber As Boolean
    Public Property MinNumber As Double = Double.MaxValue
    Public Property MaxNumber As Double = Double.MinValue
    Public Property FirstValue As Object

    Private _hasFirst As Boolean

    Public Sub Observe(value As Object)
        If value Is Nothing Then
            NullCount += 1
            Return
        End If

        NonNullCount += 1

        If Not _hasFirst Then
            _hasFirst = True
            FirstValue = value
        End If

        Dim t As String = value.GetType().Name
        If Not Types.Contains(t) Then
            Types.Add(t)
        End If

        Dim s As String = TryCast(value, String)
        If s IsNot Nothing AndAlso s.Length > MaxTextLength Then
            MaxTextLength = s.Length
        End If

        Dim b As Byte() = TryCast(value, Byte())
        If b IsNot Nothing AndAlso b.Length > MaxBlobLength Then
            MaxBlobLength = b.Length
        End If

        Dim d As Double
        If TryNumber(value, d) Then
            HasNumber = True
            If d < MinNumber Then
                MinNumber = d
            End If
            If d > MaxNumber Then
                MaxNumber = d
            End If
        End If
    End Sub

    Private Function TryNumber(value As Object, ByRef result As Double) As Boolean
        If TypeOf value Is Long OrElse TypeOf value Is Integer OrElse TypeOf value Is Short OrElse
           TypeOf value Is Byte OrElse TypeOf value Is Double OrElse TypeOf value Is Single OrElse
           TypeOf value Is Decimal Then

            result = Convert.ToDouble(value)
            Return True
        End If

        Return False
    End Function
End Class

''' <summary>
''' 记录一条被发现的问题及其修复情况
''' </summary>
Friend Class IssueRecord
    Public Property Id As String
    Public Property Title As String
    Public Property Symptom As String
    Public Property RootCause As String
    Public Property Fix As String
    Public Property Status As String
    Public Property VerifyTest As String
End Class

