Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.Text
Imports std = System.Math

Namespace Data.Repository

    ' ============================================================================
    '  JsonlStore.vb —— 基于单个 JSONL 文本文件的实验性存储引擎
    '  机制：WAL(写前日志) + 内存虚拟覆盖层(片段表) + 稀疏行索引 + 显式合并
    '  行号约定：全部从 1 开始。
    ' ============================================================================

    ''' <summary>
    ''' 基于单个 JSONL 文件的虚拟行存储。
    ''' 写操作（追加/插入/替换/删除）只进入内存片段表 + WAL 日志；
    ''' 顺序读取时自动把挂起修改“虚拟挂载”到输出流；
    ''' 由外部显式调用 Merge() 把挂起修改合并回源文件。
    ''' </summary>
    Public NotInheritable Class JsonlStore
        Implements IDisposable

#Region "常量与嵌套类型"

        Private Const Magic As Integer = &H4A4C5349     ' "JLSI"
        Private Const Version As Integer = 1
        Private Const LF As Byte = 10
        Private Const CR As Byte = 13
        Private Const ReadChunkLines As Integer = 256
        Private Shared ReadOnly BomBytes As Byte() = New Byte() {&HEF, &HBB, &HBF}
        Private Shared ReadOnly Inv As CultureInfo = CultureInfo.InvariantCulture

        Private Enum PieceKind
            OriginalFile   ' 指向源文件：Start = 1 起始行号
            PendingBuffer  ' 指向挂起缓冲：Start = 0 起始索引
        End Enum

        Private NotInheritable Class Piece
            Public Kind As PieceKind
            Public Start As Long
            Public Count As Long
        End Class

#End Region

#Region "字段"

        Private ReadOnly _dataPath, _idxPath, _idxTmpPath, _dataTmpPath, _bakPath, _lockPath As String
        Private ReadOnly _opt As JsonlStoreOptions
        Private ReadOnly _wal As WAL

        Private _enc As Encoding
        Private _nlBytes As Byte() = New Byte() {LF}
        Private _nlText As String = vbLf
        Private _hasBom As Boolean
        Private _dataEndsWithLf As Boolean
        Private _skipTailRepair As Boolean

        Private _fs As FileStream                ' 源文件只读句柄
        Private _reader As BufferedLineReader
        Private _fsNextLine As Long              ' reader 下一行将返回的源文件行号；0 = 未定位

        Private _g As Integer                    ' 索引粒度
        Private _baseLineCount As Long           ' 源文件物理行数
        Private _idxFileLength As Long
        Private _idxEntries As Long() = New Long() {}

        Private _pieces As New List(Of Piece)    ' 虚拟文档片段表
        Private _virtualCount As Long
        Private _cum As Long() = New Long() {}
        Private _cumDirty As Boolean

        Private _lockStream As FileStream
        Private _isOpen As Boolean
        Private _disposed As Boolean
        Private _activeReaders As Integer
        Private ReadOnly _gate As New Object
        Private _rwBuf As Byte()

        ''' <summary>运行期诊断信息（索引重建、撕裂修复、崩溃恢复等）。</summary>
        Public Event Info(message As String)

#End Region

#Region "构造 / 生命周期"

        Public Sub New(dataFilePath As String, Optional options As JsonlStoreOptions = Nothing)
            If String.IsNullOrWhiteSpace(dataFilePath) Then Throw New ArgumentNullException(NameOf(dataFilePath))
            _dataPath = Path.GetFullPath(dataFilePath)
            _opt = If(options, New JsonlStoreOptions())
            If _opt.IndexGranularity < 1 Then Throw New ArgumentOutOfRangeException(NameOf(options), "IndexGranularity 必须 >= 1。")
            If _opt.ReadBufferBytes < 4096 OrElse _opt.MergeBufferBytes < 4096 Then Throw New ArgumentOutOfRangeException(NameOf(options), "缓冲区必须 >= 4096。")
            _idxPath = _dataPath & ".idx"
            _idxTmpPath = _dataPath & ".idx.tmp"
            _dataTmpPath = _dataPath & ".merge.tmp"
            _bakPath = _dataPath & ".bak"
            _lockPath = _dataPath & ".lock"
            _g = _opt.IndexGranularity
            _wal = New WAL(_dataPath & ".wal", _opt)
            AddHandler _wal.Info, AddressOf OnWalInfo
        End Sub

        Private Sub OnWalInfo(message As String)
            RaiseEvent Info(message)
        End Sub

        Public Sub Open()
            If _disposed Then Throw New ObjectDisposedException(NameOf(JsonlStore))
            If _isOpen Then Return

            Dim dir As String = Path.GetDirectoryName(_dataPath)
            If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then Directory.CreateDirectory(dir)

            ' 1) 进程级独占锁：防止多实例同时打开
            Try
                _lockStream = New FileStream(_lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)
            Catch ex As IOException
                Throw New IOException("无法锁定数据库（可能已被另一实例打开）：" & _lockPath, ex)
            End Try

            ' 2) 数据文件（含“合并中途崩溃、数据文件被移走”的补全）
            EnsureDataFileForOpen()

            ' 3) 编码 / 换行检测，打开读句柄
            _enc = If(_opt.Encoding, New UTF8Encoding(False))
            OpenReader()
            DetectBomAndNewLine()

            ' 4) 读 WAL 并做崩溃恢复（合并中断 / 撕裂写 / 撕裂日志尾）
            _skipTailRepair = False
            Dim keepRecs As New List(Of WAL.Record)
            Dim keepLen As Long = 0
            If File.Exists(_wal.LogFilePath) Then keepLen = ReadAndRecoverLog(keepRecs)

            ' 5) 数据文件尾部修复（外部撕裂写入）
            RepairDataTail()

            ' 6) 行索引：加载 / 校验 / 重建
            LoadOrRebuildIndex()

            ' 7) 打开日志，截去被丢弃的尾部
            _wal.Open()
            _wal.TruncateTo(keepLen)

            ' 8) 初始化虚拟层并重放日志
            _pieces = New List(Of Piece)
            _wal.ResetPendingState()
            If _baseLineCount > 0 Then
                _pieces.Add(New Piece With {.Kind = PieceKind.OriginalFile, .Start = 1L, .Count = _baseLineCount})
            End If
            _virtualCount = _baseLineCount
            _wal.BeginReplay()
            Try
                For Each rec As WAL.Record In keepRecs
                    If rec.Pos < 1 OrElse rec.Pos > _virtualCount + 1 OrElse rec.Del < 0 OrElse rec.Pos + rec.Del - 1 > _virtualCount Then
                        Throw New InvalidDataException("WAL 记录行位置越界(pos=" & rec.Pos.ToString(Inv) & ")，日志可能已损坏。")
                    End If
                    ApplySplice(rec.Pos, rec.Del, rec.Lines)
                    _wal.IncrementOperationCount()
                Next
            Finally
                _wal.EndReplay()
            End Try
            _cumDirty = True
            RebuildCum()
            _isOpen = True
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            SyncLock _gate
                _wal.Dispose()
                CloseReader()
                If _lockStream IsNot Nothing Then
                    Try : _lockStream.Dispose() : Catch : End Try
                    _lockStream = Nothing
                End If
                _isOpen = False
            End SyncLock
            _disposed = True
        End Sub

#End Region

#Region "公共属性"

        Public ReadOnly Property DataFilePath As String
            Get
                Return _dataPath
            End Get
        End Property
        Public ReadOnly Property LogFilePath As String
            Get
                Return _wal.LogFilePath
            End Get
        End Property
        Public ReadOnly Property IndexFilePath As String
            Get
                Return _idxPath
            End Get
        End Property
        Public ReadOnly Property IndexGranularity As Integer
            Get
                Return _g
            End Get
        End Property
        Public ReadOnly Property FileNewLine As String
            Get
                Return _nlText
            End Get
        End Property
        ''' <summary>当前虚拟总行数（含挂起的增删）。</summary>
        Public ReadOnly Property TotalLines As Long
            Get
                EnsureOpen()
                SyncLock _gate : Return _virtualCount : End SyncLock
            End Get
        End Property
        Public ReadOnly Property BaseLineCount As Long
            Get
                EnsureOpen()
                SyncLock _gate : Return _baseLineCount : End SyncLock
            End Get
        End Property
        Public ReadOnly Property PendingOperationCount As Long
            Get
                EnsureOpen()
                SyncLock _gate : Return _wal.PendingOperationCount : End SyncLock
            End Get
        End Property
        Public ReadOnly Property PendingBufferedLineCount As Long
            Get
                EnsureOpen()
                SyncLock _gate : Return _wal.PendingBufferedLineCount : End SyncLock
            End Get
        End Property
        Public ReadOnly Property HasPendingChanges As Boolean
            Get
                EnsureOpen()
                SyncLock _gate : Return _wal.PendingOperationCount > 0 OrElse _wal.Length > 0 : End SyncLock
            End Get
        End Property

#End Region

#Region "写操作（进入挂起层 + WAL）"

        ''' <summary>在文件末尾追加一行。</summary>
        Public Sub AppendLine(line As String)
            If line Is Nothing Then Throw New ArgumentNullException(NameOf(line))
            Dim l As New List(Of String)(1)
            l.Add(line)
            SyncLock _gate
                EnsureOpen()
                DoSplice(_virtualCount + 1, 0, l)
            End SyncLock
        End Sub

        ''' <summary>在文件末尾追加多行（一条日志记录 + 一次 fsync，适合批量）。</summary>
        Public Sub AppendLines(lines As IEnumerable(Of String))
            Dim list As List(Of String) = Materialize(lines)
            If list.Count = 0 Then Return
            SyncLock _gate
                EnsureOpen()
                DoSplice(_virtualCount + 1, 0, list)
            End SyncLock
        End Sub

        ''' <summary>
        ''' 从第 startLine 行开始插入多行：新内容占据 startLine..startLine+n-1，
        ''' 原第 startLine 行及其后内容整体后移。startLine = TotalLines+1 时等价于追加。
        ''' </summary>
        Public Sub InsertLines(startLine As Long, lines As IEnumerable(Of String))
            Dim list As List(Of String) = Materialize(lines)
            SyncLock _gate
                EnsureOpen()
                DoSplice(startLine, 0, list)
            End SyncLock
        End Sub

        ''' <summary>把第 lineNumber 行的虚拟内容替换为 newContent。</summary>
        Public Sub ReplaceLine(lineNumber As Long, newContent As String)
            If newContent Is Nothing Then Throw New ArgumentNullException(NameOf(newContent))
            Dim l As New List(Of String)(1)
            l.Add(newContent)
            SyncLock _gate
                EnsureOpen()
                DoSplice(lineNumber, 1, l)
            End SyncLock
        End Sub

        ''' <summary>从 startLine 起删除 deleteCount 行，并插入 newLines（可为 Nothing = 纯删除）。</summary>
        Public Sub ReplaceLines(startLine As Long, deleteCount As Long, newLines As IEnumerable(Of String))
            Dim list As List(Of String) = If(newLines Is Nothing, New List(Of String), Materialize(newLines))
            SyncLock _gate
                EnsureOpen()
                DoSplice(startLine, deleteCount, list)
            End SyncLock
        End Sub

        Public Sub DeleteLine(lineNumber As Long)
            DeleteLines(lineNumber, 1)
        End Sub

        Public Sub DeleteLines(startLine As Long, deleteCount As Long)
            SyncLock _gate
                EnsureOpen()
                DoSplice(startLine, deleteCount, New List(Of String))
            End SyncLock
        End Sub

        ''' <summary>强制把日志刷到磁盘（FsyncEachWrite=False 时由外部定期调用）。</summary>
        Public Sub FlushLog()
            SyncLock _gate
                EnsureOpen()
                _wal.Flush()
            End SyncLock
        End Sub

#End Region

#Region "读操作（虚拟视图）"

        ''' <summary>随机读取虚拟第 lineNumber 行。</summary>
        Public Function ReadLine(lineNumber As Long) As String
            EnsureOpen()
            SyncLock _gate
                If lineNumber < 1 OrElse lineNumber > _virtualCount Then
                    Throw New ArgumentOutOfRangeException(NameOf(lineNumber), "行号必须在 1 ~ " & _virtualCount.ToString(Inv) & " 之间。")
                End If
                Dim pi As Integer, off As Long, before As Long
                FindPiece(lineNumber, pi, off, before)
                Dim p As Piece = _pieces(pi)
                If p.Kind = PieceKind.PendingBuffer Then
                    Return _wal.PendingLine(CInt(p.Start + off))
                End If
                Dim origLine As Long = p.Start + off
                If _fsNextLine <> origLine Then PositionReaderAtLine(origLine)
                Dim s As String = Nothing
                If Not _reader.ReadLine(s) Then Throw New InvalidDataException("源文件在预期行号处结束（索引可能已失效）。")
                _fsNextLine = origLine + 1
                Return s
            End SyncLock
        End Function

        ''' <summary>顺序读取全部虚拟行（挂起修改自动叠加）。单遍流式，不加载整个文件。</summary>
        Public Iterator Function ReadLines() As IEnumerable(Of String)
            EnsureOpen()
            Dim snap As Piece() = SnapshotPieces()
            Interlocked.Increment(_activeReaders)
            Try
                For Each p As Piece In snap
                    If p.Kind = PieceKind.PendingBuffer Then
                        For j As Long = p.Start To p.Start + p.Count - 1
                            Yield _wal.PendingLine(CInt(j))
                        Next
                    Else
                        For Each s As String In ReadOriginalLines(p.Start, p.Count)
                            Yield s
                        Next
                    End If
                Next
            Finally
                Interlocked.Decrement(_activeReaders)
            End Try
        End Function

        ''' <summary>顺序读取虚拟第 startLine 行起的最多 count 行。</summary>
        Public Iterator Function ReadLines(startLine As Long, count As Long) As IEnumerable(Of String)
            EnsureOpen()
            If count <= 0 Then Return
            If startLine < 1 OrElse startLine > _virtualCount Then
                Throw New ArgumentOutOfRangeException(NameOf(startLine), "起始行号必须在 1 ~ " & _virtualCount.ToString(Inv) & " 之间。")
            End If
            Dim snap As Piece() = SnapshotPieces()
            Dim before As Long = 0
            Dim startIdx As Integer = -1
            Dim startOff As Long = 0
            For i = 0 To snap.Length - 1
                If before + snap(i).Count >= startLine Then
                    startIdx = i
                    startOff = startLine - before - 1
                    Exit For
                End If
                before += snap(i).Count
            Next
            If startIdx < 0 Then Return
            Dim emitted As Long = 0
            Interlocked.Increment(_activeReaders)
            Try
                For i = startIdx To snap.Length - 1
                    If emitted >= count Then Exit For
                    Dim p As Piece = snap(i)
                    Dim off As Long = If(i = startIdx, startOff, 0L)
                    Dim avail As Long = p.Count - off
                    If avail <= 0 Then Continue For
                    Dim take As Long = If(count - emitted < avail, count - emitted, avail)
                    If p.Kind = PieceKind.PendingBuffer Then
                        For j As Long = off To off + take - 1
                            Yield _wal.PendingLine(CInt(p.Start + j))
                        Next
                    Else
                        For Each s As String In ReadOriginalLines(p.Start + off, take)
                            Yield s
                        Next
                    End If
                    emitted += take
                Next
            Finally
                Interlocked.Decrement(_activeReaders)
            End Try
        End Function

#End Region

#Region "合并（检查点）"

        ''' <summary>
        ''' 把挂起修改合并回源文件，然后清空日志。
        ''' 纯追加型布局走快路径（只写新增字节）；否则全量重写 + 原子替换。
        ''' 合并过程本身崩溃安全（mb/mbf/md 标记 + .bak 备份）。
        ''' </summary>
        Public Sub Merge()
            SyncLock _gate
                EnsureOpen()
                If _activeReaders > 0 Then
                    Throw New InvalidOperationException("存在未完成的 ReadLines() 枚举，请先完成枚举再合并。")
                End If
                If _wal.PendingOperationCount = 0 AndAlso _wal.Length = 0 Then Return
                CoalesceOriginalPieces()
                If IsAppendOnlyLayout() Then
                    MergeFastAppend()
                Else
                    MergeFullRewrite()
                End If
                _wal.ClearLog()
                _wal.ResetPendingState()
            End SyncLock
        End Sub

        ''' <summary>强制重建并保存行索引（维护用途）。</summary>
        Public Sub RebuildLineIndex()
            SyncLock _gate
                EnsureOpen()
                RebuildIndexCore()
                WriteIndexFile(_idxPath, _baseLineCount, _idxFileLength, _idxEntries)
                RaiseEvent Info("行索引已手动重建。")
            End SyncLock
        End Sub

        Private Function IsAppendOnlyLayout() As Boolean
            If _baseLineCount = 0 Then Return True
            If _pieces.Count = 0 Then Return True
            Dim p0 As Piece = _pieces(0)
            Return p0.Kind = PieceKind.OriginalFile AndAlso p0.Start = 1 AndAlso p0.Count = _baseLineCount
        End Function

        Private Sub CoalesceOriginalPieces()
            Dim i As Integer = 0
            Do While i < _pieces.Count - 1
                Dim a As Piece = _pieces(i)
                Dim b As Piece = _pieces(i + 1)
                If a.Kind = PieceKind.OriginalFile AndAlso b.Kind = PieceKind.OriginalFile AndAlso a.Start + a.Count = b.Start Then
                    a.Count += b.Count
                    _pieces.RemoveAt(i + 1)
                Else
                    i += 1
                End If
            Loop
            _cumDirty = True
        End Sub

        ' —— 快路径：所有修改都发生在文件末尾 → 直接 append，O(新增字节)
        Private Sub MergeFastAppend()
            Dim oldLen As Long = _fs.Length
            Dim needNL As Boolean = oldLen > 0 AndAlso Not _dataEndsWithLf
            Dim newEntries As New List(Of Long)(_idxEntries)
            Dim pos As Long = oldLen + If(needNL, 1L, 0L)
            Dim newTotal As Long = _baseLineCount
            For Each p As Piece In _pieces
                If p.Kind <> PieceKind.PendingBuffer Then Continue For
                For j As Long = p.Start To p.Start + p.Count - 1
                    If newTotal Mod _g = 0 Then newEntries.Add(pos)
                    pos += _enc.GetByteCount(_wal.PendingLine(CInt(j))) + _nlBytes.Length
                    newTotal += 1
                Next
            Next
            Dim newLen As Long = pos

            WriteIndexFile(_idxTmpPath, newTotal, newLen, newEntries)               ' 1) 新索引先落盘
            _wal.AppendMergeBegin(False, oldLen, newLen)                            ' 2) 合并意图

            If newLen > oldLen Then                                                 ' 3) 追加数据
                Using w As New FileStream(_dataPath, FileMode.Append, FileAccess.Write, FileShare.Read Or FileShare.Write, _opt.MergeBufferBytes)
                    If needNL Then w.WriteByte(LF)
                    For Each p As Piece In _pieces
                        If p.Kind <> PieceKind.PendingBuffer Then Continue For
                        For j As Long = p.Start To p.Start + p.Count - 1
                            WriteLineBytes(w, _wal.PendingLine(CInt(j)))
                        Next
                    Next
                    w.Flush(True)
                End Using
            End If

            SwapFile(_idxTmpPath, _idxPath, Nothing)                                ' 4) 原子换索引
            _wal.AppendMergeDone()                                                  ' 5) 完成标记

            _baseLineCount = newTotal
            _idxEntries = newEntries.ToArray()
            _idxFileLength = newLen
            _dataEndsWithLf = True
            _pieces.Clear()
            If newTotal > 0 Then _pieces.Add(New Piece With {.Kind = PieceKind.OriginalFile, .Start = 1L, .Count = newTotal})
            _virtualCount = newTotal
            _cumDirty = True
        End Sub

        ' —— 全量重写路径
        Private Sub MergeFullRewrite()
            Dim oldLen As Long = _fs.Length
            Dim snap As Piece() = SnapshotPieces()
            Dim entries As New List(Of Long)
            Dim total As Long = 0
            Dim pos As Long = 0
            Dim newLen As Long = 0

            Using w As New FileStream(_dataTmpPath, FileMode.Create, FileAccess.Write, FileShare.None, _opt.MergeBufferBytes)
                If snap.Length > 0 AndAlso _hasBom Then
                    w.Write(BomBytes, 0, BomBytes.Length)
                    pos = BomBytes.Length
                End If
                For Each p As Piece In snap
                    If p.Kind = PieceKind.PendingBuffer Then
                        For j As Long = p.Start To p.Start + p.Count - 1
                            If total Mod _g = 0 Then entries.Add(pos)
                            pos += WriteLineBytes(w, _wal.PendingLine(CInt(j))) + _nlBytes.Length
                            total += 1
                        Next
                    Else
                        If _fsNextLine <> p.Start Then PositionReaderAtLine(p.Start)
                        For k As Long = 1 To p.Count
                            Dim s As String = Nothing
                            If Not _reader.ReadLine(s) Then
                                Throw New InvalidDataException("合并时读取源文件第 " & (p.Start + k - 1).ToString(Inv) & " 行失败。")
                            End If
                            _fsNextLine += 1
                            If total Mod _g = 0 Then entries.Add(pos)
                            pos += WriteLineBytes(w, s) + _nlBytes.Length
                            total += 1
                        Next
                    End If
                Next
                w.Flush()
                w.Flush(True)
                newLen = w.Length
            End Using

            WriteIndexFile(_idxTmpPath, total, newLen, entries)                     ' 1) 索引临时文件
            CloseReader()                                                           ' 2) 释放读句柄以便原子替换
            _wal.AppendMergeBegin(True, oldLen, newLen)                             ' 3) 合并意图
            SwapFile(_dataTmpPath, _dataPath, _bakPath)                             ' 4) 原子换数据文件（保留 .bak）
            SwapFile(_idxTmpPath, _idxPath, Nothing)                                ' 5) 原子换索引
            _wal.AppendMergeDone()                                                  ' 6) 完成标记

            If File.Exists(_bakPath) Then
                Try : File.Delete(_bakPath) : Catch : End Try
            End If
            OpenReader()
            _baseLineCount = total
            _idxEntries = entries.ToArray()
            _idxFileLength = newLen
            _dataEndsWithLf = True
            _pieces.Clear()
            If total > 0 Then _pieces.Add(New Piece With {.Kind = PieceKind.OriginalFile, .Start = 1L, .Count = total})
            _virtualCount = total
            _cumDirty = True
        End Sub

        Private Function WriteLineBytes(w As FileStream, line As String) As Integer
            Dim n As Integer = _enc.GetByteCount(line)
            Dim need As Integer = n + _nlBytes.Length
            If _rwBuf Is Nothing OrElse _rwBuf.Length < need Then
                _rwBuf = New Byte(std.Max(need, _opt.MergeBufferBytes) - 1) {}
            End If
            _enc.GetBytes(line, 0, line.Length, _rwBuf, 0)
            w.Write(_rwBuf, 0, n)
            w.Write(_nlBytes, 0, _nlBytes.Length)
            Return n
        End Function

#End Region

#Region "核心内部：片段表 / 日志"

        ' 前置条件：_gate 已持有
        Private Sub DoSplice(pos As Long, delCount As Long, lines As List(Of String))
            If pos < 1 OrElse pos > _virtualCount + 1 Then
                Throw New ArgumentOutOfRangeException("pos", "行位置越界：有效范围 1 ~ " & (_virtualCount + 1).ToString(Inv) & "。")
            End If
            If delCount < 0 Then Throw New ArgumentOutOfRangeException(NameOf(delCount), "删除行数不能为负。")
            If pos + delCount - 1 > _virtualCount Then Throw New ArgumentException("要删除的范围超出当前总行数。")
            If (lines Is Nothing OrElse lines.Count = 0) AndAlso delCount = 0 Then Return
            ' ★ 关键顺序：先写日志（含 fsync），后改内存。
            '   崩溃在两步之间 → 重启后重放日志，状态一致；崩溃在日志写一半 → 撕裂尾被丢弃。
            _wal.AppendSplice(pos, delCount, lines)
            ApplySplice(pos, delCount, lines)
            _wal.IncrementOperationCount()
        End Sub

        ' 统一的“接合”操作：在虚拟位置 pos 删除 delCount 行、插入 newLines。
        ' 追加/插入/替换/删除全部归一为这一个原语，日志重放走同一代码路径。
        Private Sub ApplySplice(pos As Long, delCount As Long, newLines As List(Of String))
            Dim insertIdx As Integer
            If pos > _virtualCount Then
                insertIdx = _pieces.Count                 ' 纯追加（追加到文档末尾）
            Else
                Dim pi As Integer, off As Long, before As Long
                FindPiece(pos, pi, off, before)
                If off > 0 Then
                    SplitPiece(pi, off)
                    pi += 1
                End If
                insertIdx = pi
                Dim remaining As Long = delCount
                Do While remaining > 0
                    Dim p As Piece = _pieces(insertIdx)
                    If p.Count <= remaining Then
                        remaining -= p.Count
                        _pieces.RemoveAt(insertIdx)
                    Else
                        p.Start += remaining
                        p.Count -= remaining
                        remaining = 0
                    End If
                Loop
            End If

            If newLines IsNot Nothing AndAlso newLines.Count > 0 Then
                Dim startIdx As Long = _wal.AddPendingLines(newLines)
                Dim np As New Piece With {.Kind = PieceKind.PendingBuffer, .Start = startIdx, .Count = newLines.Count}
                _pieces.Insert(insertIdx, np)
                TryCoalesce(insertIdx)
            End If

            _virtualCount += If(newLines Is Nothing, 0L, CLng(newLines.Count)) - delCount
            _cumDirty = True
        End Sub

        Private Sub FindPiece(virtualLine As Long, ByRef pi As Integer, ByRef off As Long, ByRef before As Long)
            If _cumDirty Then RebuildCum()
            Dim lo As Integer = 0
            Dim hi As Integer = _cum.Length - 1
            pi = -1
            Do While lo <= hi
                Dim mid As Integer = lo + ((hi - lo) >> 1)
                If _cum(mid) >= virtualLine Then
                    pi = mid
                    hi = mid - 1
                Else
                    lo = mid + 1
                End If
            Loop
            If pi < 0 Then Throw New InvalidDataException("内部错误：虚拟行号超出片段表。")
            before = If(pi = 0, 0L, _cum(pi - 1))
            off = virtualLine - before - 1
        End Sub

        Private Sub RebuildCum()
            If _pieces.Count = 0 Then
                _cum = New Long() {}
            Else
                Dim c(_pieces.Count - 1) As Long
                Dim t As Long = 0
                For i = 0 To _pieces.Count - 1
                    t += _pieces(i).Count
                    c(i) = t
                Next
                _cum = c
            End If
            _cumDirty = False
        End Sub

        Private Sub SplitPiece(pi As Integer, off As Long)
            Dim p As Piece = _pieces(pi)
            Dim rest As New Piece With {.Kind = p.Kind, .Start = p.Start + off, .Count = p.Count - off}
            p.Count = off
            _pieces.Insert(pi + 1, rest)
        End Sub

        Private Sub TryCoalesce(idx As Integer)
            If idx > 0 AndAlso idx < _pieces.Count Then
                Dim prev As Piece = _pieces(idx - 1)
                Dim cur As Piece = _pieces(idx)
                If prev.Kind = PieceKind.PendingBuffer AndAlso cur.Kind = PieceKind.PendingBuffer _
               AndAlso prev.Start + prev.Count = cur.Start Then
                    prev.Count += cur.Count
                    _pieces.RemoveAt(idx)
                    idx -= 1
                End If
            End If
            If idx >= 0 AndAlso idx + 1 < _pieces.Count Then
                Dim cur2 As Piece = _pieces(idx)
                Dim nxt As Piece = _pieces(idx + 1)
                If cur2.Kind = PieceKind.PendingBuffer AndAlso nxt.Kind = PieceKind.PendingBuffer _
               AndAlso cur2.Start + cur2.Count = nxt.Start Then
                    cur2.Count += nxt.Count
                    _pieces.RemoveAt(idx + 1)
                End If
            End If
        End Sub

        Private Function SnapshotPieces() As Piece()
            SyncLock _gate
                Dim arr(_pieces.Count - 1) As Piece
                For i = 0 To _pieces.Count - 1
                    Dim p As Piece = _pieces(i)
                    arr(i) = New Piece With {.Kind = p.Kind, .Start = p.Start, .Count = p.Count}
                Next
                Return arr
            End SyncLock
        End Function

        Private Shared Function Materialize(lines As IEnumerable(Of String)) As List(Of String)
            If lines Is Nothing Then Throw New ArgumentNullException(NameOf(lines))
            Dim result As New List(Of String)
            For Each s As String In lines
                If s Is Nothing Then Throw New ArgumentException("行内容不能为 Nothing。", NameOf(lines))
                result.Add(s)
            Next
            Return result
        End Function

        Private Sub EnsureOpen()
            If _disposed Then Throw New ObjectDisposedException(NameOf(JsonlStore))
            If Not _isOpen Then Throw New InvalidOperationException("必须先调用 Open()。")
        End Sub

#End Region

#Region "核心内部：文件读取 / 索引"

        Private Sub OpenReader()
            _fs = New FileStream(_dataPath, FileMode.Open, FileAccess.Read,
                             FileShare.Read Or FileShare.Write, _opt.ReadBufferBytes, FileOptions.SequentialScan)
            _fsNextLine = 0
            _reader = New BufferedLineReader(_fs, _enc, _opt.ReadBufferBytes)
        End Sub

        Private Sub CloseReader()
            _reader = Nothing
            If _fs IsNot Nothing Then
                _fs.Dispose()
                _fs = Nothing
            End If
            _fsNextLine = 0
        End Sub

        Private Sub DetectBomAndNewLine()
            Dim n As Integer = CInt(std.Min(_fs.Length, 8192L))
            Dim buf(std.Max(n, 1) - 1) As Byte
            _fs.Position = 0
            Dim got As Integer = 0
            Do While got < n
                Dim rr As Integer = _fs.Read(buf, got, n - got)
                If rr <= 0 Then Exit Do
                got += rr
            Loop
            _fs.Position = 0
            If got >= 2 AndAlso ((buf(0) = &HFF AndAlso buf(1) = &HFE) OrElse (buf(0) = &HFE AndAlso buf(1) = &HFF)) Then
                Throw New NotSupportedException("不支持带 UTF-16 BOM 的数据文件：行切分依赖 0x0A 字节。")
            End If
            _hasBom = got >= 3 AndAlso buf(0) = &HEF AndAlso buf(1) = &HBB AndAlso buf(2) = &HBF
            Dim nl As String = _opt.NewLine
            If nl Is Nothing Then
                Dim k As Integer = Array.IndexOf(buf, LF, 0, got)
                nl = If(k > 0 AndAlso buf(k - 1) = CR, vbCrLf, vbLf)
            End If
            _nlText = nl
            _nlBytes = _enc.GetBytes(nl)
        End Sub

        ' 用稀疏索引把 reader 定位到源文件第 n 行
        Private Sub PositionReaderAtLine(n As Long)
            Dim m As Integer = CInt((n - 1) \ _g)
            _reader.SeekTo(_idxEntries(m))
            Dim skip As Integer = CInt((n - 1) - CLng(m) * _g)
            Dim s As String = Nothing
            For i = 1 To skip
                If Not _reader.ReadLine(s) Then Throw New InvalidDataException("源文件行数与索引不一致。")
            Next
            _fsNextLine = n
        End Sub

        ' 分块读取源文件行（每块持锁，块间让出，兼顾并发）
        Private Iterator Function ReadOriginalLines(startLine As Long, count As Long) As IEnumerable(Of String)
            If count <= 0 OrElse startLine < 1 Then Return
            Dim done As Long = 0
            Do While done < count
                Dim take As Integer = CInt(std.Min(count - done, CLng(ReadChunkLines)))
                Dim batch(take - 1) As String
                SyncLock _gate
                    If _fsNextLine <> startLine + done Then PositionReaderAtLine(startLine + done)
                    For i = 0 To take - 1
                        Dim s As String = Nothing
                        If Not _reader.ReadLine(s) Then
                            Throw New InvalidDataException("源文件在预期行号处结束（索引可能已失效）。")
                        End If
                        batch(i) = s
                    Next
                    _fsNextLine = startLine + done + take
                End SyncLock
                For i = 0 To take - 1
                    Yield batch(i)
                Next
                done += take
            Loop
        End Function

        Private Sub LoadOrRebuildIndex()
            If TryLoadIndexFile() Then Return
            RebuildIndexCore()
            WriteIndexFile(_idxPath, _baseLineCount, _idxFileLength, _idxEntries)
            RaiseEvent Info("行索引不存在或已失效，已全量重建（" & _idxFileLength.ToString(Inv) & " 字节 / " & _baseLineCount.ToString(Inv) & " 行）。")
        End Sub

        Private Function TryLoadIndexFile() As Boolean
            If Not File.Exists(_idxPath) Then Return False
            Try
                Using fs As New FileStream(_idxPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                    Using br As New BinaryReader(fs)
                        If br.ReadInt32() <> Magic Then Return False
                        If br.ReadInt32() <> Version Then Return False
                        Dim g As Integer = br.ReadInt32()
                        If g < 1 Then Return False
                        Dim total As Long = br.ReadInt64()
                        Dim flen As Long = br.ReadInt64()
                        Dim cnt As Integer = br.ReadInt32()
                        If total < 0 Then Return False
                        If flen <> _fs.Length Then Return False
                        Dim expected As Long = If(total > 0, (total - 1) \ g + 1, 0)
                        If cnt <> expected Then Return False
                        If fs.Length <> 32L + CLng(cnt) * 8L Then Return False
                        Dim ents(cnt - 1) As Long
                        For i = 0 To cnt - 1
                            ents(i) = br.ReadInt64()
                            If ents(i) < 0 OrElse ents(i) > flen Then Return False
                        Next
                        For i = 1 To cnt - 1
                            If ents(i) <= ents(i - 1) Then Return False
                        Next
                        _g = g
                        _baseLineCount = total
                        _idxFileLength = flen
                        _idxEntries = ents
                        Return True
                    End Using
                End Using
            Catch
                Return False
            End Try
        End Function

        Private Sub RebuildIndexCore()
            _g = _opt.IndexGranularity
            Dim entries As New List(Of Long)
            Dim total As Long = 0
            Dim endsLf As Boolean = True
            Dim fileLen As Long
            ' 用独立句柄扫描，不干扰 _fs/_reader 状态
            Using r As New FileStream(_dataPath, FileMode.Open, FileAccess.Read,
                                  FileShare.Read Or FileShare.Write, _opt.ReadBufferBytes, FileOptions.SequentialScan)
                fileLen = r.Length
                Dim first As Long = If(_hasBom, 3L, 0L)
                If fileLen > first Then
                    total = 1
                    entries.Add(first)
                    Dim buf(_opt.ReadBufferBytes - 1) As Byte
                    Dim basePos As Long = 0
                    Dim lastByte As Integer = -1
                    Do
                        Dim n As Integer = r.Read(buf, 0, buf.Length)
                        If n <= 0 Then Exit Do
                        Dim i As Integer = 0
                        Do
                            Dim k As Integer = Array.IndexOf(buf, LF, i, n - i)
                            If k < 0 Then Exit Do
                            total += 1
                            If (total - 1) Mod _g = 0 Then entries.Add(basePos + k + 1) ' 乐观记录
                            i = k + 1
                        Loop
                        lastByte = buf(n - 1)
                        basePos += n
                    Loop
                    endsLf = (lastByte = LF)
                    If endsLf Then total -= 1   ' 结尾 LF 不开启新行
                End If
            End Using
            Dim expected As Long = If(total > 0, (total - 1) \ _g + 1, 0)
            Do While entries.Count > expected      ' 修剪幻影条目
                entries.RemoveAt(entries.Count - 1)
            Loop
            _baseLineCount = total
            _idxFileLength = fileLen
            _idxEntries = entries.ToArray()
            _dataEndsWithLf = endsLf
        End Sub

        Private Sub WriteIndexFile(path As String, totalLines As Long, fileLength As Long, entries As IList(Of Long))
            Using fs As New FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)
                ' leaveOpen:=True：BinaryWriter 的 Dispose 不能关闭 fs，否则下面的 fs.Flush(True) 会抛 ObjectDisposedException
                Using bw As New BinaryWriter(fs, New UTF8Encoding(False), leaveOpen:=True)
                    bw.Write(Magic)
                    bw.Write(Version)
                    bw.Write(_g)
                    bw.Write(totalLines)
                    bw.Write(fileLength)
                    bw.Write(entries.Count)
                    For i = 0 To entries.Count - 1
                        bw.Write(entries(i))
                    Next
                End Using
                fs.Flush(True)
            End Using
        End Sub

#End Region

#Region "核心内部：打开期崩溃恢复"

        Private Sub EnsureDataFileForOpen()
            If File.Exists(_dataPath) Then Return
            If File.Exists(_bakPath) AndAlso File.Exists(_dataTmpPath) Then
                File.Move(_dataTmpPath, _dataPath) ' 补全“两步重命名”的中断
                RaiseEvent Info("数据文件缺失，已从合并临时文件恢复。")
            ElseIf File.Exists(_bakPath) Then
                Throw New InvalidDataException("数据文件缺失且无法自动恢复（存在 .bak 但缺少合并临时文件）。")
            Else
                File.Create(_dataPath).Dispose()
                RaiseEvent Info("数据文件不存在，已创建空文件。")
            End If
        End Sub

        ' 处理 WAL 解析结果 → 合并中断判定 → 返回需要保留重放的记录与日志保留长度。
        ' 日志读写 / 解析 / 撕裂尾丢弃已由 WAL.ReadRecords 完成，此处只做数据文件相关的恢复决策。
        Private Function ReadAndRecoverLog(keepRecs As List(Of WAL.Record)) As Long
            Dim read As WAL.ReadResult = _wal.ReadRecords()
            Dim recs As List(Of WAL.Record) = read.Records
            Dim goodLen As Long = read.GoodLength

            Dim mbIdx As Integer = -1
            Dim mdIdx As Integer = -1
            For i = 0 To recs.Count - 1
                If recs(i).Kind = WAL.RecordKind.MergeAppend OrElse recs(i).Kind = WAL.RecordKind.MergeFull Then mbIdx = i
                If recs(i).Kind = WAL.RecordKind.MergeDone Then mdIdx = i
            Next

            If mdIdx >= 0 Then
                ' 合并已完成，只是日志还没来得及清空
                If mbIdx < 0 OrElse mbIdx <> mdIdx - 1 OrElse mdIdx <> recs.Count - 1 Then
                    Throw New InvalidDataException("WAL 中的合并标记序列异常。")
                End If
                If _fs.Length <> recs(mbIdx).NewLen Then
                    Throw New InvalidDataException("数据文件长度与已完成合并的记录不符（可能被外部修改）。")
                End If
                FinishMergedState()
                _skipTailRepair = True
                Return 0
            End If

            If mbIdx >= 0 Then
                If mbIdx <> recs.Count - 1 Then Throw New InvalidDataException("WAL 中的合并标记位置异常。")
                Dim mb As WAL.Record = recs(mbIdx)
                Dim curLen As Long = _fs.Length
                Dim mergedOutcome As Boolean

                If mb.Kind = WAL.RecordKind.MergeFull Then
                    ' 全量重写：.bak 的存在 ⇔ 数据交换已发生
                    If File.Exists(_bakPath) Then
                        If curLen <> mb.NewLen Then
                            Throw New InvalidDataException("存在合并备份 .bak，但数据文件长度与合并目标不符。")
                        End If
                        mergedOutcome = True
                    Else
                        If curLen <> mb.OldLen Then
                            Throw New InvalidDataException("数据文件长度与合并前记录不符，无法自动恢复。")
                        End If
                        mergedOutcome = False
                    End If
                Else
                    ' 快速追加：按长度判定
                    If curLen = mb.NewLen Then
                        mergedOutcome = True
                    ElseIf curLen = mb.OldLen Then
                        mergedOutcome = False
                    ElseIf mb.OldLen < curLen AndAlso curLen < mb.NewLen Then
                        TruncateDataTo(mb.OldLen)   ' 撕裂的追加 → 回滚
                        RaiseEvent Info("合并追加中断（撕裂写），已回滚 " & (curLen - mb.OldLen).ToString(Inv) & " 字节。")
                        _skipTailRepair = True
                        mergedOutcome = False
                    Else
                        Throw New InvalidDataException("数据文件长度(" & curLen.ToString(Inv) &
                        ")与合并标记(old=" & mb.OldLen.ToString(Inv) & ", new=" & mb.NewLen.ToString(Inv) & ")不符。")
                    End If
                End If

                If mergedOutcome Then
                    FinishMergedState()
                    _skipTailRepair = True
                    Return 0
                End If
                ' 合并未生效：清理临时文件，重放 mb 之前的 splice
                CleanupTempFiles()
                For i = 0 To mbIdx - 1
                    If recs(i).Kind = WAL.RecordKind.Splice Then keepRecs.Add(recs(i))
                Next
                Return mb.StartOffset
            End If

            ' 无合并标记：正常重放
            CleanupTempFiles()
            For Each rec As WAL.Record In recs
                If rec.Kind = WAL.RecordKind.Splice Then keepRecs.Add(rec)
            Next
            Return goodLen
        End Function

        Private Sub FinishMergedState()
            If File.Exists(_bakPath) Then
                Try : File.Delete(_bakPath) : Catch : End Try
            End If
            If File.Exists(_dataTmpPath) Then
                Try : File.Delete(_dataTmpPath) : Catch : End Try
            End If
            If File.Exists(_idxTmpPath) Then SwapFile(_idxTmpPath, _idxPath, Nothing)
            RaiseEvent Info("检测到一次已完成的合并（上次崩溃遗留），已清理临时文件并丢弃旧日志。")
        End Sub

        Private Sub CleanupTempFiles()
            If File.Exists(_dataTmpPath) Then
                Try : File.Delete(_dataTmpPath) : Catch : End Try
            End If
            If File.Exists(_idxTmpPath) Then
                Try : File.Delete(_idxTmpPath) : Catch : End Try
            End If
        End Sub

        Private Sub RepairDataTail()
            Dim len As Long = _fs.Length
            If len = 0 Then
                _dataEndsWithLf = True
                Return
            End If
            Dim lastLf As Long = FindLastNewlineOffset()
            If lastLf = len - 1 Then
                _dataEndsWithLf = True
                Return
            End If
            If _skipTailRepair OrElse Not _opt.RepairTornTail Then
                _dataEndsWithLf = False
                Return
            End If
            TruncateDataTo(If(lastLf >= 0, lastLf + 1, If(_hasBom, 3L, 0L)))
        End Sub

        Private Function FindLastNewlineOffset() As Long
            Dim chunk As Integer = _opt.ReadBufferBytes
            Dim buf(chunk - 1) As Byte
            Dim pos As Long = _fs.Length
            Using r As New FileStream(_dataPath, FileMode.Open, FileAccess.Read, FileShare.Read Or FileShare.Write)
                Do While pos > 0
                    Dim n As Integer = CInt(std.Min(pos, CLng(chunk)))
                    r.Position = pos - n
                    Dim got As Integer = 0
                    Do While got < n
                        Dim rr As Integer = r.Read(buf, got, n - got)
                        If rr <= 0 Then Exit Do
                        got += rr
                    Loop
                    For i As Integer = got - 1 To 0 Step -1
                        If buf(i) = LF Then Return pos - n + i
                    Next
                    pos -= n
                Loop
            End Using
            Return -1
        End Function

        Private Sub TruncateDataTo(newLen As Long)
            Dim oldLen As Long = _fs.Length
            Using w As New FileStream(_dataPath, FileMode.Open, FileAccess.Write, FileShare.Read Or FileShare.Write)
                w.SetLength(newLen)
                w.Flush(True)
            End Using
            _dataEndsWithLf = True
            RaiseEvent Info("数据文件已截断 " & (oldLen - newLen).ToString(Inv) & " 字节（撕裂写回滚 / 尾部修复）。")
        End Sub

        Private Shared Sub SwapFile(src As String, dst As String, backup As String)
            If File.Exists(dst) Then
                Try
                    File.Replace(src, dst, backup)
                Catch pex As PlatformNotSupportedException
                    ' 非原子平台退化为两步重命名
                    Dim bak As String = backup
                    If bak Is Nothing Then bak = dst & ".oldbak"
                    File.Move(dst, bak)
                    File.Move(src, dst)
                    Try : File.Delete(bak) : Catch : End Try
                End Try
            Else
                File.Move(src, dst)
            End If
        End Sub

#End Region


    End Class

End Namespace