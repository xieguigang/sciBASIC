#Region "Microsoft.VisualBasic::8f9b7db01651fa1311d8bcd009c229cb, Data\BinaryData\SQLite3\test\WriterTests.vb"

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

    '   Total Lines: 284
    '    Code Lines: 219 (77.11%)
    ' Comment Lines: 15 (5.28%)
    '    - Xml Docs: 73.33%
    ' 
    '   Blank Lines: 50 (17.61%)
    '     File Size: 14.89 KB


    ' Module WriterTests
    ' 
    '     Sub: RunWriterTests, TestAppendUpdateDelete, TestCreateAndReadBack, TestEdgeValues, TestFormatSelfCheck
    '          TestMultiPageAndOverflow
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Enums
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Tables
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Writer

''' <summary>
''' 写入模块的"先写库、再读回"往返测试。
''' 
''' 覆盖: 新建库与基础类型、边界值、多页 B 树与溢出页、打开已有文件追加/更新/删除/删表、文件格式自检。
''' 生成的测试文件位于临时目录, 结果并入 <c>TEST-REPORT.md</c>。
''' </summary>
Module WriterTests

    Private Const PageSize As Integer = 4096

    ''' <summary>测试文件所在的临时目录</summary>
    Friend ReadOnly TestRoot As String = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sqlite3-writer-tests")

    Friend Sub RunWriterTests()
        If System.IO.Directory.Exists(TestRoot) Then
            Try
                System.IO.Directory.Delete(TestRoot, True)
            Catch
                ' 忽略无法清理的残留
            End Try
        End If

        Call System.IO.Directory.CreateDirectory(TestRoot)
        Program.writerNotes.Add("- 测试目录: ``" & TestRoot & "``")

        Call TestCreateAndReadBack()
        Call TestEdgeValues()
        Call TestMultiPageAndOverflow()
        Call TestAppendUpdateDelete()
        Call TestFormatSelfCheck()
    End Sub

    ''' <summary>新建库 + 多表 + 基础类型写入, 再用读取器读回逐值比对</summary>
    Private Sub TestCreateAndReadBack()
        Dim path As String = System.IO.Path.Combine(TestRoot, "basic.sqlite")

        Program.Run("写入: 新建库并读回基础类型", Sub()
            Using w As Sqlite3Writer = Sqlite3Writer.CreateFile(path, PageSize)
                Dim demo = w.CreateTable("demo", New Sqlite3Column() {
                    New Sqlite3Column("id", "INTEGER", notNull:=True, primaryKey:=True),
                    New Sqlite3Column("name", "VARCHAR"),
                    New Sqlite3Column("mass", "FLOAT"),
                    New Sqlite3Column("data", "BLOB"),
                    New Sqlite3Column("flag", "BOOLEAN"),
                    New Sqlite3Column("note", "VARCHAR")
                })

                Call demo.AddRow({1, "alpha", 1.5, New Byte() {1, 2, 3}, True, "hello"})
                Call demo.AddRow({2, "beta", 2.5, Nothing, False, Nothing})
                Call demo.AddRow({3, "中文/γ", -3.25, New Byte() {9, 8}, True, Nothing})

                Dim noPk = w.CreateTable("nopk", New Sqlite3Column() {
                    New Sqlite3Column("a", "INTEGER"),
                    New Sqlite3Column("b", "VARCHAR")
                })
                Call noPk.AddRow({10, "ten"})
                Call noPk.AddRow({20, "twenty"})

                w.Commit()
            End Using

            Using db As Sqlite3Database = Sqlite3Database.OpenFile(path, New Sqlite3Settings())
                Dim names As String() = db.GetTables _
                    .Where(Function(x) x.type = "table") _
                    .Select(Function(x) x.tableName) _
                    .ToArray()

                Program.Check(names.Length = 2 AndAlso names.Contains("demo") AndAlso names.Contains("nopk"),
                              "表清单不符合预期: " & String.Join(",", names))

                Dim demo = db.GetTable("demo")
                Program.Check(demo.schema.columns.Length = 6, "demo 列数错误: " & demo.schema.columns.Length)

                Dim rows As Sqlite3Row() = demo.EnumerateRows().ToArray()
                Program.Check(rows.Length = 3, "demo 行数错误: " & rows.Length)
                Program.Check(rows(0).RowId = 1 AndAlso rows(1).RowId = 2 AndAlso rows(2).RowId = 3, "demo rowid 错误")

                ' INTEGER PRIMARY KEY 别名列应回填 rowid
                Program.Check(CLng(rows(0)("id")) = 1, "id 回填错误: " & rows(0)("id"))
                Program.Check(CStr(rows(0)("name")) = "alpha", "name 往返错误")
                Program.Check(CDbl(rows(0)("mass")) = 1.5, "mass 往返错误")
                Program.Check(DirectCast(rows(0)("data"), Byte()).SequenceEqual(New Byte() {1, 2, 3}), "data 往返错误")
                Program.Check(CBool(rows(0)("flag")) = True, "flag 往返错误")
                Program.Check(CStr(rows(0)("note")) = "hello", "note 往返错误")

                Program.Check(rows(1)("data") Is Nothing, "data 应为 NULL")
                Program.Check(rows(1)("note") Is Nothing, "note 应为 NULL")
                Program.Check(CBool(rows(1)("flag")) = False, "flag 应为 False")
                Program.Check(CStr(rows(2)("name")) = "中文/γ", "UTF-8 文本往返错误")

                Dim noPk = db.GetTable("nopk")
                Dim noPkRows As Sqlite3Row() = noPk.EnumerateRows().ToArray()
                Program.Check(noPkRows.Length = 2, "nopk 行数错误")
                Program.Check(noPkRows(0).RowId = 1 AndAlso noPkRows(1).RowId = 2, "nopk 自增 rowid 错误")
                Program.Check(CLng(noPkRows(0)("a")) = 10, "nopk.a 往返错误")
            End Using
        End Sub)
    End Sub

    ''' <summary>NULL / 极值整数 / 浮点 / 空字符串 / 空 BLOB / 长文本</summary>
    Private Sub TestEdgeValues()
        Dim path As String = System.IO.Path.Combine(TestRoot, "edge.sqlite")

        Program.Run("写入: 边界值往返", Sub()
            Using w As Sqlite3Writer = Sqlite3Writer.CreateFile(path, PageSize)
                Dim t = w.CreateTable("edge", New Sqlite3Column() {
                    New Sqlite3Column("i", "INTEGER"),
                    New Sqlite3Column("r", "FLOAT"),
                    New Sqlite3Column("s", "VARCHAR"),
                    New Sqlite3Column("b", "BLOB"),
                    New Sqlite3Column("o", "BOOLEAN")
                })

                Call t.AddRow({Long.MaxValue, 1.7976931348623157E+308, "max", New Byte() {255}, True})
                Call t.AddRow({Long.MinValue, -1.0E-300, "", New Byte() {}, False})
                Call t.AddRow(New Object() {Nothing, Nothing, Nothing, Nothing, Nothing})
                Call t.AddRow({0L, 0.0, New String("x"c, 100), Nothing, True})

                w.Commit()
            End Using

            Using db As Sqlite3Database = Sqlite3Database.OpenFile(path, New Sqlite3Settings())
                Dim rows As Sqlite3Row() = db.GetTable("edge").EnumerateRows().ToArray()
                Program.Check(rows.Length = 4, "行数错误: " & rows.Length)
                Program.Check(CLng(rows(0)("i")) = Long.MaxValue, "Long.MaxValue 往返错误")
                Program.Check(CLng(rows(1)("i")) = Long.MinValue, "Long.MinValue 往返错误")
                Program.Check(CDbl(rows(0)("r")) = 1.7976931348623157E+308, "Double 最大值往返错误")
                Program.Check(CStr(rows(1)("s")) = "", "空字符串往返错误")
                Program.Check(DirectCast(rows(1)("b"), Byte()).Length = 0, "空 BLOB 往返错误")
                Program.Check(rows(2)("i") Is Nothing AndAlso rows(2)("s") Is Nothing AndAlso rows(2)("o") Is Nothing, "NULL 往返错误")
                Program.Check(CStr(rows(3)("s")).Length = 100, "长文本长度错误")
                Program.Check(CBool(rows(3)("o")) = True, "布尔往返错误")
            End Using
        End Sub)
    End Sub

    ''' <summary>插入数千行迫使产生内部页, 并插入超阈值大字段触发溢出页</summary>
    Private Sub TestMultiPageAndOverflow()
        Dim path As String = System.IO.Path.Combine(TestRoot, "multipage.sqlite")
        Dim rowCount As Integer = 3000
        Dim bigText As String = New String("A"c, 9000)
        Dim bigBlob As Byte() = New Byte(19999) {}

        For i As Integer = 0 To bigBlob.Length - 1
            bigBlob(i) = CByte(i And &HFF)
        Next

        Program.Run("写入: 多页 B 树与溢出页", Sub()
            Using w As Sqlite3Writer = Sqlite3Writer.CreateFile(path, PageSize)
                Dim t = w.CreateTable("big", New Sqlite3Column() {
                    New Sqlite3Column("id", "INTEGER", notNull:=True, primaryKey:=True),
                    New Sqlite3Column("txt", "VARCHAR"),
                    New Sqlite3Column("blob", "BLOB")
                })

                For i As Integer = 1 To rowCount
                    Call t.AddRow({CLng(i), New String("n"c, 200 + (i Mod 50)), Nothing})
                Next

                ' 大字段: txt 9000 字节、blob 20000 字节, 均超过单页内联阈值
                Call t.AddRow({CLng(rowCount + 1), bigText, bigBlob})

                w.Commit()
            End Using

            Dim size As Long = New FileInfo(path).Length
            Program.Check(size Mod PageSize = 0, "文件大小不是页大小整数倍")
            Program.Check(size > PageSize * 3, "多页表未产生足够页数: " & size)

            Using db As Sqlite3Database = Sqlite3Database.OpenFile(path, New Sqlite3Settings())
                Dim rows As Sqlite3Row() = db.GetTable("big").EnumerateRows().ToArray()
                Program.Check(rows.Length = rowCount + 1, "行数错误: " & rows.Length)

                Dim last = rows(rows.Length - 1)
                Program.Check(CStr(last("txt")).Length = 9000, "溢出 TEXT 长度错误: " & CStr(last("txt")).Length)
                Program.Check(DirectCast(last("blob"), Byte()).Length = 20000, "溢出 BLOB 长度错误")
                Program.Check(CStr(last("txt")) = bigText, "溢出 TEXT 内容错误")
                Program.Check(DirectCast(last("blob"), Byte()).SequenceEqual(bigBlob), "溢出 BLOB 内容错误")

                Dim monotonic As Boolean = True
                For i As Integer = 1 To rows.Length - 1
                    If rows(i).RowId <= rows(i - 1).RowId Then
                        monotonic = False
                        Exit For
                    End If
                Next
                Program.Check(monotonic, "rowid 非单调递增")

                Program.writerNotes.Add($"- 多页/溢出: 行数 {rowCount + 1:N0}, 文件 {size:N0} 字节 ({size \ PageSize:N0} 页)")
            End Using
        End Sub)
    End Sub

    ''' <summary>打开已有库执行追加 / 更新 / 删除 / 删表, 再读回校验</summary>
    Private Sub TestAppendUpdateDelete()
        Dim path As String = System.IO.Path.Combine(TestRoot, "mutate.sqlite")

        Program.Run("写入: 打开已有库追加/更新/删除", Sub()
            Using w As Sqlite3Writer = Sqlite3Writer.CreateFile(path, PageSize)
                Dim t = w.CreateTable("t", New Sqlite3Column() {
                    New Sqlite3Column("id", "INTEGER", notNull:=True, primaryKey:=True),
                    New Sqlite3Column("v", "VARCHAR")
                })
                Call t.AddRow({1, "one"})
                Call t.AddRow({2, "two"})
                Call t.AddRow({3, "three"})

                Dim tmp = w.CreateTable("temp", New Sqlite3Column() {New Sqlite3Column("x", "INTEGER")})
                Call tmp.AddRow({1})

                w.Commit()
            End Using

            Using w As Sqlite3Writer = Sqlite3Writer.OpenFile(path)
                Dim t = w.GetTable("t")
                Program.Check(t.RowCount = 3, "打开已有库之后行数错误: " & t.RowCount)

                Call t.AddRow({4, "four"})
                Call t.UpdateRow(1, {1, "ONE"})
                Call t.UpdateRow(2, "v", "TWO")
                Call t.DeleteRow(3)

                Program.Check(t.RowCount = 3, "内存之中增删改之后行数错误: " & t.RowCount)
                Program.Check(w.DropTable("temp"), "删表失败")
                w.Commit()
            End Using

            Using db As Sqlite3Database = Sqlite3Database.OpenFile(path, New Sqlite3Settings())
                Dim names As String() = db.GetTables _
                    .Where(Function(x) x.type = "table") _
                    .Select(Function(x) x.tableName) _
                    .ToArray()

                Program.Check(names.Length = 1 AndAlso names(0) = "t", "删表之后表清单错误: " & String.Join(",", names))

                Dim rows As Sqlite3Row() = db.GetTable("t").EnumerateRows().ToArray()
                Program.Check(rows.Length = 3, "增删改之后行数错误: " & rows.Length)
                Program.Check(CStr(rows(0)("v")) = "ONE", "整体更新失败: " & CStr(rows(0)("v")))
                Program.Check(rows(1).RowId = 2 AndAlso CStr(rows(1)("v")) = "TWO", "单列更新失败")
                Program.Check(rows(2).RowId = 4 AndAlso CStr(rows(2)("v")) = "four", "追加失败")
                Program.Check(Not rows.Any(Function(r) r.RowId = 3), "删除 rowid=3 失败")
            End Using
        End Sub)
    End Sub

    ''' <summary>校验生成文件的数据库头字段与页数一致性</summary>
    Private Sub TestFormatSelfCheck()
        Dim path As String = System.IO.Path.Combine(TestRoot, "format.sqlite")

        Program.Run("写入: 文件格式自检", Sub()
            Using w As Sqlite3Writer = Sqlite3Writer.CreateFile(path, PageSize)
                w.CreateTable("a", New Sqlite3Column() {New Sqlite3Column("x", "INTEGER")}).AddRow({1})
                w.CreateTable("b", New Sqlite3Column() {New Sqlite3Column("y", "VARCHAR")}).AddRow({"z"})
                w.Commit()
            End Using

            Using db As Sqlite3Database = Sqlite3Database.OpenFile(path, New Sqlite3Settings())
                Dim h = db.Header
                Dim size As Long = New FileInfo(path).Length

                Program.Check(h.PageSize = PageSize, "页大小错误: " & h.PageSize)
                Program.Check(h.TextEncoding = SqliteEncoding.UTF8, "文本编码错误")
                Program.Check(h.SchemaFormat = 4, "SchemaFormat 错误: " & h.SchemaFormat)
                Program.Check(h.ReservedSpaceAtEndOfPage = 0, "预留空间错误")
                Program.Check(h.FreeListPages = 0, "空闲页数错误: " & h.FreeListPages)
                Program.Check(h.FirstFreelistTrunkPage = 0, "空闲页链表头错误")
                Program.Check(CLng(h.DatabaseSizeInPages) = size \ PageSize, "文件头页数与实际不一致")
                Program.Check(size Mod PageSize = 0, "文件大小不是页大小整数倍")
            End Using

            Program.writerNotes.Add("- 格式自检文件: ``" & path & "`` (" & New FileInfo(path).Length.ToString("N0") & " 字节)")
        End Sub)
    End Sub

End Module

