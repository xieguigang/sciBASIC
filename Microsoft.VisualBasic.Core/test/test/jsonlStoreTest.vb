#Region "Microsoft.VisualBasic::jsonlStoreTest, Microsoft.VisualBasic.Core\test\test\jsonlStoreTest.vb"

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

    ' Module jsonlStoreTest
    ' 
    '     Function: SameSeq
    ' 
    '     Sub: Check, CheckAppendAndVirtualRead, CheckMergeAppendFastPath
    '          CheckMergeFullRewrite, CheckSpliceOperations, CheckTornTailRecovery
    '          CheckWalModuleRoundTrip, CheckWalReplayAfterCrash, Run
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.Data.Repository

''' <summary>
''' the demo checks of the jsonl store engine (Data.Repository.TextStore):
''' <see cref="JsonlStore"/> (虚拟片段表 + 行索引 + 显式合并) and <see cref="WAL"/> (写前日志)。
''' </summary>
''' <remarks>
''' The test project of this solution is a console application, so that the
''' verification is written as a set of the assertions instead of the MSTest
''' [TestMethod] attributes.
''' </remarks>
Module jsonlStoreTest

    Dim passed As Integer
    Dim failed As Integer

    ''' <summary>
    ''' runs all of the jsonl store / WAL demo checks.
    ''' </summary>
    Sub Run()
        passed = 0
        failed = 0

        Dim root As String = Path.Combine(Path.GetTempPath(), "jsonlstore-demo-" & Guid.NewGuid().ToString("N"))
        Call Directory.CreateDirectory(root)

        Console.WriteLine("==== jsonl store / WAL demo checks ====")
        Console.WriteLine($"work dir: {root}")
        Console.WriteLine()

        Try
            Call CheckAppendAndVirtualRead(root)
            Call CheckSpliceOperations(root)
            Call CheckWalReplayAfterCrash(root)
            Call CheckTornTailRecovery(root)
            Call CheckMergeAppendFastPath(root)
            Call CheckMergeFullRewrite(root)
            Call CheckWalModuleRoundTrip(root)
        Finally
            Try : Directory.Delete(root, recursive:=True) : Catch : End Try
        End Try

        Console.WriteLine()
        Console.WriteLine($"  passed: {passed}, failed: {failed}")
        Console.WriteLine()
    End Sub

    ' ------------------------------------------------------------------------
    ' 1) 追加写入只进入 WAL / 内存挂起层，顺序读与随机读自动挂载虚拟内容
    ' ------------------------------------------------------------------------
    Private Sub CheckAppendAndVirtualRead(root As String)
        Console.WriteLine("-- append + virtual read --")

        Dim dbPath As String = Path.Combine(root, "append.jsonl")
        Dim expected As String() = {"{""id"":1}", "{""id"":2}", "{""id"":3}"}

        Using store As New JsonlStore(dbPath)
            Call store.Open()
            Call store.AppendLine("{""id"":1}")
            Call store.AppendLines({"{""id"":2}", "{""id"":3}"})

            Check("total lines = 3", store.TotalLines = 3, $"actual={store.TotalLines}")
            Check("base lines = 0", store.BaseLineCount = 0, $"actual={store.BaseLineCount}")
            Check("has pending changes", store.HasPendingChanges)
            Check("pending operation count = 2", store.PendingOperationCount = 2, $"actual={store.PendingOperationCount}")
            Check("pending buffered lines = 3", store.PendingBufferedLineCount = 3, $"actual={store.PendingBufferedLineCount}")

            Check("sequential read mounts pending lines", SameSeq(store.ReadLines(), expected))
            Check("random read line 2", store.ReadLine(2) = "{""id"":2}", $"actual={store.ReadLine(2)}")
            Check("range read 2..3", SameSeq(store.ReadLines(2, 2), {"{""id"":2}", "{""id"":3}"}))
        End Using

        ' 未调用 Merge()：源文件仍为空，修改全部停留在 WAL 中
        Check("source file not modified before merge", FileLength(dbPath) = 0)
    End Sub

    ' ------------------------------------------------------------------------
    ' 2) 插入 / 替换 / 删除归一为同一个 splice 原语，虚拟视图保持一致
    ' ------------------------------------------------------------------------
    Private Sub CheckSpliceOperations(root As String)
        Console.WriteLine("-- insert / replace / delete --")

        Dim dbPath As String = Path.Combine(root, "splice.jsonl")

        Using store As New JsonlStore(dbPath)
            Call store.Open()
            Call store.AppendLines({"a", "b", "c", "d"})

            Call store.InsertLines(2, {"x", "y"})            ' a x y b c d
            Call store.ReplaceLine(1, "A")                   ' A x y b c d
            Call store.ReplaceLines(4, 2, {"B", "C"})        ' A x y B C d
            Call store.DeleteLine(6)                         ' A x y B C

            Dim expected As String() = {"A", "x", "y", "B", "C"}
            Check("virtual total = 5", store.TotalLines = 5, $"actual={store.TotalLines}")
            Check("splice operations applied", SameSeq(store.ReadLines(), expected))
        End Using
    End Sub

    ' ------------------------------------------------------------------------
    ' 3) 崩溃恢复：不合并直接退出，重开时从 WAL 重放挂起修改
    ' ------------------------------------------------------------------------
    Private Sub CheckWalReplayAfterCrash(root As String)
        Console.WriteLine("-- WAL replay after crash --")

        Dim dbPath As String = Path.Combine(root, "replay.jsonl")
        Dim expected As String() = {"1", "2", "3"}

        Using store As New JsonlStore(dbPath)
            Call store.Open()
            Call store.AppendLines(expected)
            ' 故意不调用 Merge()，模拟程序被终止：修改只存在于 WAL
        End Using

        Check("data file still empty after crash", FileLength(dbPath) = 0)

        Using store As New JsonlStore(dbPath)
            Call store.Open()

            Check("replayed total = 3", store.TotalLines = 3, $"actual={store.TotalLines}")
            Check("replayed content", SameSeq(store.ReadLines(), expected))
            Check("log still holds pending records", store.HasPendingChanges)

            Call store.Merge()
            Check("log cleared after merge", Not store.HasPendingChanges AndAlso store.PendingOperationCount = 0)
        End Using

        Check("merged file content", SameSeq(File.ReadAllLines(dbPath), expected))
    End Sub

    ' ------------------------------------------------------------------------
    ' 4) WAL 撕裂尾（未以换行终止的残缺记录）必须被丢弃且不使打开失败
    ' ------------------------------------------------------------------------
    Private Sub CheckTornTailRecovery(root As String)
        Console.WriteLine("-- WAL torn tail recovery --")

        Dim dbPath As String = Path.Combine(root, "torn.jsonl")
        Dim expected As String() = {"alpha", "beta"}

        Using store As New JsonlStore(dbPath)
            Call store.Open()
            Call store.AppendLines(expected)
        End Using

        ' 在 WAL 末尾追加一条未换行终止的残缺记录，模拟断电撕裂写
        Call File.AppendAllText(dbPath & ".wal", "{""op"":""sp"",""pos"":1,""del"":0,""l"":[")

        Dim messages As New List(Of String)
        Using store As New JsonlStore(dbPath)
            AddHandler store.Info, Sub(m) messages.Add(m)
            Call store.Open()

            Check("torn tail dropped, total = 2", store.TotalLines = 2, $"actual={store.TotalLines}")
            Check("valid records replayed", SameSeq(store.ReadLines(), expected))
        End Using

        Check("torn tail reported via Info", messages.Exists(Function(m) m.Contains("不完整记录")))

        ' 撕裂尾已被截断，再次打开应恢复正常
        Using store As New JsonlStore(dbPath)
            Call store.Open()
            Check("reopen after torn tail ok", store.TotalLines = 2, $"actual={store.TotalLines}")
        End Using
    End Sub

    ' ------------------------------------------------------------------------
    ' 5) 合并快路径：纯追加型布局只写新增字节
    ' ------------------------------------------------------------------------
    Private Sub CheckMergeAppendFastPath(root As String)
        Console.WriteLine("-- merge fast append path --")

        Dim dbPath As String = Path.Combine(root, "merge-append.jsonl")
        Call File.WriteAllText(dbPath, "base1" & vbLf & "base2" & vbLf)

        Using store As New JsonlStore(dbPath)
            Call store.Open()

            Check("base lines = 2", store.BaseLineCount = 2, $"actual={store.BaseLineCount}")

            Call store.AppendLine("new1")
            Call store.AppendLine("new2")

            Check("virtual total = 4", store.TotalLines = 4, $"actual={store.TotalLines}")

            Call store.Merge()

            Check("merged total = 4", store.TotalLines = 4, $"actual={store.TotalLines}")
            Check("merged base = 4", store.BaseLineCount = 4, $"actual={store.BaseLineCount}")
        End Using

        Check("fast append file content", SameSeq(File.ReadAllLines(dbPath), {"base1", "base2", "new1", "new2"}))
    End Sub

    ' ------------------------------------------------------------------------
    ' 6) 合并全量重写路径：非追加型布局走临时文件 + 原子替换
    ' ------------------------------------------------------------------------
    Private Sub CheckMergeFullRewrite(root As String)
        Console.WriteLine("-- merge full rewrite path --")

        Dim dbPath As String = Path.Combine(root, "merge-full.jsonl")
        Call File.WriteAllText(dbPath, "a" & vbLf & "b" & vbLf & "c" & vbLf)

        Dim expected As String() = {"head", "a", "B", "c"}

        Using store As New JsonlStore(dbPath)
            Call store.Open()

            Call store.InsertLines(1, {"head"})      ' head a b c
            Call store.ReplaceLine(3, "B")           ' head a B c

            Check("virtual content before merge", SameSeq(store.ReadLines(), expected))

            Call store.Merge()

            Check("merged total = 4", store.TotalLines = 4, $"actual={store.TotalLines}")
        End Using

        Check("full rewrite file content", SameSeq(File.ReadAllLines(dbPath), expected))
        Check("index file written", File.Exists(dbPath & ".idx"))
        Check("merge temp/backup cleaned", Not File.Exists(dbPath & ".merge.tmp") AndAlso Not File.Exists(dbPath & ".bak"))
    End Sub

    ' ------------------------------------------------------------------------
    ' 7) WAL 模块直连：序列化 / 解析往返、JSON 转义、撕裂尾丢弃
    ' ------------------------------------------------------------------------
    Private Sub CheckWalModuleRoundTrip(root As String)
        Console.WriteLine("-- WAL module round trip --")

        Dim logPath As String = Path.Combine(root, "direct.wal")
        Dim opt As New JsonlStoreOptions() With {.FsyncEachWrite = False}

        ' 含引号、反斜杠、制表符、换行符的行内容，验证 JSON 转义能无损往返
        Dim tricky As String = "quote:"" back:\ tab:" & vbTab & " nl:" & vbLf & " end"

        Dim wal As New WAL(logPath, opt)
        Call wal.Open()
        Call wal.AppendSplice(1, 0, New List(Of String) From {"hello", "world"})   ' 0
        Call wal.AppendSplice(2, 1, Nothing)                                       ' 1  纯删除
        Call wal.AppendSplice(3, 0, New List(Of String) From {tricky})             ' 2
        Call wal.AppendMergeBegin(isFullRewrite:=False, oldLen:=0, newLen:=10)     ' 3
        Call wal.AppendMergeDone()                                                 ' 4
        Call wal.Flush()
        Call wal.Dispose()   ' 写句柄关闭后才能再以只读方式打开日志

        Console.WriteLine("RAW-WAL>>>")
        Console.WriteLine(File.ReadAllText(logPath).Replace(vbLf, "<LF>" & vbLf).Replace(vbTab, "<TAB>"))
        Console.WriteLine("<<<RAW-WAL")

        Dim logLen As Long = New FileInfo(logPath).Length

        Dim reader As New WAL(logPath, opt)
        Dim read As WAL.ReadResult = reader.ReadRecords()

        Check("record count = 5", read.Records.Count = 5, $"actual={read.Records.Count}")
        Check("good length = log length", read.GoodLength = logLen, $"good={read.GoodLength}, len={logLen}")
        Check("record[0] is splice", read.Records(0).Kind = WAL.RecordKind.Splice)
        Check("record[0] pos/del", read.Records(0).Pos = 1 AndAlso read.Records(0).Del = 0)
        Check("record[0] lines", SameSeq(read.Records(0).Lines, {"hello", "world"}))
        Check("record[1] pure delete", read.Records(1).Kind = WAL.RecordKind.Splice AndAlso read.Records(1).Del = 1)
        Check("record[2] escaped line round trip", read.Records(2).Lines(0) = tricky)
        Check("record[3] merge append marker",
              read.Records(3).Kind = WAL.RecordKind.MergeAppend AndAlso read.Records(3).OldLen = 0 AndAlso read.Records(3).NewLen = 10)
        Check("record[4] merge done", read.Records(4).Kind = WAL.RecordKind.MergeDone)
        Check("start offsets ascending",
              read.Records(1).StartOffset > read.Records(0).StartOffset AndAlso read.Records(4).StartOffset > read.Records(3).StartOffset)

        Call reader.Dispose()

        ' 追加一条未终结的残缺记录，验证 ReadRecords 丢弃撕裂尾
        Call File.AppendAllText(logPath, "{""op"":""sp"",""pos"":")
        Dim reader2 As New WAL(logPath, opt)
        Dim messages As New List(Of String)
        AddHandler reader2.Info, Sub(m) messages.Add(m)

        Dim read2 As WAL.ReadResult = reader2.ReadRecords()
        Check("torn tail record dropped", read2.Records.Count = 5, $"actual={read2.Records.Count}")
        Check("good length unchanged after torn tail", read2.GoodLength = read.GoodLength)
        Check("torn tail reported via Info", messages.Exists(Function(m) m.Contains("不完整记录")))
        Call reader2.Dispose()
    End Sub

    ' ------------------------------------------------------------------------
    ' helpers
    ' ------------------------------------------------------------------------

    Private Function FileLength(filePath As String) As Long
        If Not File.Exists(filePath) Then Return 0
        Return New FileInfo(filePath).Length
    End Function

    Private Function SameSeq(actual As IEnumerable(Of String), expected As String()) As Boolean
        Dim a As String() = actual.ToArray()

        If a.Length <> expected.Length Then Return False

        For i As Integer = 0 To a.Length - 1
            If a(i) <> expected(i) Then Return False
        Next

        Return True
    End Function

    Private Sub Check(name As String, condition As Boolean, Optional detail As String = "")
        If condition Then
            passed += 1
            Console.WriteLine($"  [PASS] {name}")
        Else
            failed += 1
            Console.WriteLine($"  [FAIL] {name}")

            If Not String.IsNullOrEmpty(detail) Then
                Console.WriteLine($"         {detail}")
            End If
        End If
    End Sub

End Module
