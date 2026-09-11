Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Text

Namespace Data.Repository

    ' ============================================================================
    '  WAL.vb —— JSONL 存储引擎的写前日志（Write-Ahead Log）模块
    '  职责：
    '    · 日志文件（*.wal）的打开 / 截断 / 清理与顺序写入
    '    · 日志记录的序列化与解析（sp / mb / mbf / md）
    '    · 读取时丢弃撕裂尾记录并计算可保留的日志长度
    '    · 内存中待写入行缓冲（挂起层）与挂起操作计数的管理
    '  边界：本模块只管理日志文件与内存挂起层，不持有、不操作数据文件、
    '        .bak、.idx.tmp、.merge.tmp；「合并是否已完成、是否截断数据文件」
    '        等恢复决策由调用方（JsonlStore）负责。
    ' ============================================================================

    ''' <summary>
    ''' JSONL 存储引擎的写前日志。记录格式与磁盘布局固定为 UTF-8 无 BOM 的逐行 JSON。
    ''' </summary>
    Public NotInheritable Class WAL
        Implements IDisposable

        Private Shared ReadOnly Inv As CultureInfo = CultureInfo.InvariantCulture

        ''' <summary>磁盘记录类型（对应令牌 "sp" / "mb" / "mbf" / "md"）。</summary>
        Public Enum RecordKind
            ''' <summary>"sp"：在虚拟位置删除若干行并插入新行。</summary>
            Splice
            ''' <summary>"mb"：快路径（末尾追加式）合并开始标记。</summary>
            MergeAppend
            ''' <summary>"mbf"：全量重写式合并开始标记。</summary>
            MergeFull
            ''' <summary>"md"：合并完成标记。</summary>
            MergeDone
        End Enum

        ''' <summary>一条已解析的 WAL 记录。</summary>
        Public NotInheritable Class Record
            ''' <summary>记录类型。</summary>
            Public Kind As RecordKind
            ''' <summary>Splice：虚拟起始行号（1 基）。</summary>
            Public Pos As Long
            ''' <summary>Splice：删除行数。</summary>
            Public Del As Long
            ''' <summary>MergeAppend / MergeFull：合并前数据文件长度。</summary>
            Public OldLen As Long
            ''' <summary>MergeAppend / MergeFull：合并后数据文件长度。</summary>
            Public NewLen As Long
            ''' <summary>Splice：插入的行内容（可能为空列表，表示纯删除）。</summary>
            Public Lines As List(Of String)
            ''' <summary>该记录在日志文件中的起始字节偏移。</summary>
            Public StartOffset As Long
        End Class

        ''' <summary>ReadRecords 的结果。</summary>
        Public NotInheritable Class ReadResult
            ''' <summary>全部有效记录（撕裂尾已被丢弃）。</summary>
            Public ReadOnly Records As List(Of Record)
            ''' <summary>可安全保留的日志长度（撕裂 / 被丢弃记录之前的字节数）。</summary>
            Public ReadOnly GoodLength As Long

            Public Sub New(records As List(Of Record), goodLength As Long)
                Me.Records = records
                Me.GoodLength = goodLength
            End Sub
        End Class

        Private ReadOnly _logPath As String
        Private ReadOnly _opt As TextStoreOptions
        Private ReadOnly _enc As New UTF8Encoding(False)

        Private _logStream As FileStream
        Private _pending As New List(Of String)   ' 内存挂起行缓冲（只追加）
        Private _opCount As Long
        Private _replaying As Boolean
        Private _disposed As Boolean

        ''' <summary>诊断信息（撕裂尾丢弃、日志重放期间的异常等）。</summary>
        Public Event Info(message As String)

        Public Sub New(logFilePath As String, options As TextStoreOptions)
            If String.IsNullOrWhiteSpace(logFilePath) Then Throw New ArgumentNullException(NameOf(logFilePath))
            If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))
            _logPath = logFilePath
            _opt = options
        End Sub

#Region "公共属性"

        Public ReadOnly Property LogFilePath As String
            Get
                Return _logPath
            End Get
        End Property

        ''' <summary>当前日志文件长度（字节）。未打开时为 0。</summary>
        Public ReadOnly Property Length As Long
            Get
                Return If(_logStream Is Nothing, 0L, _logStream.Length)
            End Get
        End Property

        ''' <summary>挂起操作计数（自上次 Merge 以来写入 / 重放的记录数）。</summary>
        Public ReadOnly Property PendingOperationCount As Long
            Get
                Return _opCount
            End Get
        End Property

        ''' <summary>内存挂起行缓冲的行数。</summary>
        Public ReadOnly Property PendingBufferedLineCount As Long
            Get
                Return CLng(_pending.Count)
            End Get
        End Property

#End Region

#Region "生命周期"

        ''' <summary>打开（或创建）日志文件，并把写入位置定位到末尾。</summary>
        Public Sub Open()
            If _disposed Then Throw New ObjectDisposedException(NameOf(WAL))
            If _logStream IsNot Nothing Then Return
            _logStream = New FileStream(_logPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, _opt.LogBufferBytes)
            _logStream.Position = _logStream.Length
        End Sub

        ''' <summary>把日志截断到指定长度（不会扩容），并把写入位置移到新末尾。</summary>
        Public Sub TruncateTo(length As Long)
            EnsureOpen()
            If _logStream.Length > length Then _logStream.SetLength(length)
            _logStream.Position = _logStream.Length
        End Sub

        ''' <summary>清空日志（合并完成后调用）。</summary>
        Public Sub ClearLog()
            EnsureOpen()
            _logStream.SetLength(0)
            _logStream.Flush(True)
            _logStream.Position = 0
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            If _logStream IsNot Nothing Then
                Try : _logStream.Flush(True) : Catch : End Try
                Try : _logStream.Dispose() : Catch : End Try
                _logStream = Nothing
            End If
            _disposed = True
        End Sub

#End Region

#Region "日志写入"

        ''' <summary>追加一条 Splice 记录（先落盘，含按配置的 fsync）。</summary>
        Public Sub AppendSplice(pos As Long, delCount As Long, lines As List(Of String))
            If _replaying Then Return
            WriteLogBytes("{""op"":""sp"",""pos"":" & pos.ToString(Inv) & ",""del"":" & delCount.ToString(Inv) & ",""l"":[")
            If lines IsNot Nothing Then
                For i = 0 To lines.Count - 1
                    If i > 0 Then WriteLogBytes(",")
                    WriteLogBytes("""")
                    WriteLogBytes(JEscape(lines(i)))
                    WriteLogBytes("""")
                Next
            End If
            WriteLogBytes("]}" & vbLf)
            If _opt.FsyncEachWrite Then _logStream.Flush(True) Else _logStream.Flush()
        End Sub

        ''' <summary>追加一条合并开始标记（mb 或 mbf），必须立即落盘。</summary>
        Public Sub AppendMergeBegin(isFullRewrite As Boolean, oldLen As Long, newLen As Long)
            WriteLogBytes("{""op"":""" & If(isFullRewrite, "mbf", "mb") & """,""old"":" & oldLen.ToString(Inv) & ",""new"":" & newLen.ToString(Inv) & "}" & vbLf)
            _logStream.Flush(True)
        End Sub

        ''' <summary>追加一条合并完成标记（md），必须立即落盘。</summary>
        Public Sub AppendMergeDone()
            WriteLogBytes("{""op"":""md""}" & vbLf)
            _logStream.Flush(True)
        End Sub

        ''' <summary>强制把日志刷到磁盘（FsyncEachWrite=False 时由外部定期调用）。</summary>
        Public Sub Flush()
            EnsureOpen()
            _logStream.Flush(True)
        End Sub

#End Region

#Region "日志读取与恢复"

        ''' <summary>
        ''' 单遍流式读取全部 WAL 记录。<br/>
        ''' · 未以换行终止的尾部记录（撕裂写）会被丢弃并触发 Info；<br/>
        ''' · 已换行终止但无法解析的记录视为损坏，抛 <see cref="InvalidDataException"/>。
        ''' </summary>
        Public Function ReadRecords() As ReadResult
            Dim recs As New List(Of Record)
            Dim goodLen As Long = 0
            If Not File.Exists(_logPath) Then Return New ReadResult(recs, goodLen)

            Using lfs As New FileStream(_logPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Dim lr As New BufferedLineReader(lfs, _enc, _opt.ReadBufferBytes)
                Dim line As String = Nothing
                Dim lastOkTerm As Boolean = True
                Do While lr.ReadLine(line)
                    Dim terminated As Boolean = lr.LastLineWasTerminated
                    Dim rec As Record = Nothing
                    If TryParseRecord(line, rec) Then
                        rec.StartOffset = lr.LastLineStartOffset
                        recs.Add(rec)
                        goodLen = lr.LastLineEndOffset
                        lastOkTerm = terminated
                    Else
                        If terminated Then
                            Throw New InvalidDataException("WAL 在偏移 " & lr.LastLineStartOffset.ToString(Inv) & " 处存在无法解析的记录。")
                        Else
                            RaiseEvent Info("WAL 尾部存在不完整记录（崩溃残留），已丢弃。")
                            Exit Do
                        End If
                    End If
                Loop
                If recs.Count > 0 AndAlso Not lastOkTerm Then
                    goodLen = recs(recs.Count - 1).StartOffset
                    recs.RemoveAt(recs.Count - 1)
                    RaiseEvent Info("WAL 最后一条记录缺少换行终止（撕裂写），已丢弃。")
                End If
            End Using

            Return New ReadResult(recs, goodLen)
        End Function

#End Region

#Region "内存挂起层"

        ''' <summary>
        ''' 把若干行追加进内存挂起缓冲，返回这些行的起始索引。
        ''' 挂起行数超过 <see cref="Integer.MaxValue"/> 时抛异常。
        ''' </summary>
        Public Function AddPendingLines(lines As List(Of String)) As Long
            If lines Is Nothing OrElse lines.Count = 0 Then Return CLng(_pending.Count)
            If CLng(_pending.Count) + lines.Count > Integer.MaxValue Then
                Throw New InvalidOperationException("内存挂起行数过多，请先调用 Merge()。")
            End If
            Dim startIdx As Long = _pending.Count
            For Each l As String In lines
                _pending.Add(l)
            Next
            Return startIdx
        End Function

        ''' <summary>读取内存挂起缓冲中的第 index 行。</summary>
        Public Function PendingLine(index As Integer) As String
            Return _pending(index)
        End Function

        ''' <summary>挂起操作计数 +1。</summary>
        Public Sub IncrementOperationCount()
            _opCount += 1
        End Sub

        ''' <summary>清空内存挂起层（挂起行缓冲与操作计数）。</summary>
        Public Sub ResetPendingState()
            _pending.Clear()
            _opCount = 0
        End Sub

        ''' <summary>进入日志重放态：期间 AppendSplice 不产生磁盘写入。</summary>
        Public Sub BeginReplay()
            _replaying = True
        End Sub

        ''' <summary>退出日志重放态。</summary>
        Public Sub EndReplay()
            _replaying = False
        End Sub

#End Region

#Region "内部：写入辅助"

        Private Sub EnsureOpen()
            If _disposed Then Throw New ObjectDisposedException(NameOf(WAL))
            If _logStream Is Nothing Then Throw New InvalidOperationException("必须先调用 Open()。")
        End Sub

        Private Sub WriteLogBytes(text As String)
            Dim b As Byte() = _enc.GetBytes(text)
            _logStream.Write(b, 0, b.Length)
        End Sub

#End Region

#Region "内部：JSON 序列化 / 解析"

        Private Shared Function JEscape(s As String) As String
            If s Is Nothing Then Return ""
            Dim simple As Boolean = True
            For Each c As Char In s
                If c = """"c OrElse c = "\"c OrElse c < ChrW(32) Then
                    simple = False
                    Exit For
                End If
            Next
            If simple Then Return s
            Dim sb As New StringBuilder(s.Length + 16)
            For Each c As Char In s
                Dim code As Integer = AscW(c)
                Select Case code
                    Case 34 : sb.Append("\"c).Append(""""c)   ' \"
                    Case 92 : sb.Append("\\")         ' \\
                    Case 8 : sb.Append("\b")
                    Case 9 : sb.Append("\t")
                    Case 10 : sb.Append("\n")
                    Case 12 : sb.Append("\f")
                    Case 13 : sb.Append("\r")
                    Case Is < 32 : sb.Append("\u").Append(code.ToString("x4", Inv))
                    Case Else : sb.Append(c)
                End Select
            Next
            Return sb.ToString()
        End Function

        ' 严格解析本类自己生成的 WAL 记录格式
        Private Shared Function TryParseRecord(s As String, ByRef rec As Record) As Boolean
            Dim i As Integer = 0
            If Not ExpectLit(s, i, "{""op"":""") Then Return False
            Dim opStart As Integer = i
            Do While i < s.Length AndAlso s(i) <> """"c
                i += 1
            Loop
            If i >= s.Length Then Return False
            Dim op As String = s.Substring(opStart, i - opStart)
            i += 1
            rec = New Record
            Select Case op
                Case "sp"
                    rec.Kind = RecordKind.Splice
                    If Not ExpectLit(s, i, ",""pos"":") Then Return False
                    If Not ReadLongTok(s, i, rec.Pos) Then Return False
                    If Not ExpectLit(s, i, ",""del"":") Then Return False
                    If Not ReadLongTok(s, i, rec.Del) Then Return False
                    If Not ExpectLit(s, i, ",""l"":[") Then Return False
                    rec.Lines = New List(Of String)
                    If i < s.Length AndAlso s(i) = "]"c Then
                        i += 1
                    Else
                        Do
                            Dim ln As String = ReadJsonStringTok(s, i)
                            If ln Is Nothing Then Return False
                            rec.Lines.Add(ln)
                            If i >= s.Length Then Return False
                            If s(i) = ","c Then
                                i += 1
                            ElseIf s(i) = "]"c Then
                                i += 1
                                Exit Do
                            Else
                                Return False
                            End If
                        Loop
                    End If
                    Return ExpectLit(s, i, "}") AndAlso i = s.Length
                Case "mb", "mbf"
                    rec.Kind = If(op = "mbf", RecordKind.MergeFull, RecordKind.MergeAppend)
                    If Not ExpectLit(s, i, ",""old"":") Then Return False
                    If Not ReadLongTok(s, i, rec.OldLen) Then Return False
                    If Not ExpectLit(s, i, ",""new"":") Then Return False
                    If Not ReadLongTok(s, i, rec.NewLen) Then Return False
                    Return ExpectLit(s, i, "}") AndAlso i = s.Length
                Case "md"
                    rec.Kind = RecordKind.MergeDone
                    Return ExpectLit(s, i, "}") AndAlso i = s.Length
                Case Else
                    Return False
            End Select
        End Function

        Private Shared Function ExpectLit(s As String, ByRef i As Integer, lit As String) As Boolean
            If i + lit.Length > s.Length Then Return False
            For k = 0 To lit.Length - 1
                If s(i + k) <> lit(k) Then Return False
            Next
            i += lit.Length
            Return True
        End Function

        Private Shared Function ReadLongTok(s As String, ByRef i As Integer, ByRef v As Long) As Boolean
            Dim start As Integer = i
            If i < s.Length AndAlso s(i) = "-"c Then i += 1
            Do While i < s.Length AndAlso s(i) >= "0"c AndAlso s(i) <= "9"c
                i += 1
            Loop
            If i = start Then Return False
            v = Long.Parse(s.Substring(start, i - start), Inv)
            Return True
        End Function

        Private Shared Function ReadJsonStringTok(s As String, ByRef i As Integer) As String
            If i >= s.Length OrElse s(i) <> """"c Then Return Nothing
            i += 1
            Dim sb As New StringBuilder()
            Do While i < s.Length
                Dim c As Char = s(i)
                i += 1
                If c = """"c Then Return sb.ToString()
                If c = "\"c Then
                    If i >= s.Length Then Return Nothing
                    Dim e As Char = s(i)
                    i += 1
                    Select Case e
                        Case """"c : sb.Append(""""c)
                        Case "\"c : sb.Append("\"c)
                        Case "/"c : sb.Append("/"c)
                        Case "b"c : sb.Append(ChrW(8))
                        Case "f"c : sb.Append(ChrW(12))
                        Case "n"c : sb.Append(ChrW(10))
                        Case "r"c : sb.Append(ChrW(13))
                        Case "t"c : sb.Append(ChrW(9))
                        Case "u"c
                            If i + 4 > s.Length Then Return Nothing
                            Dim code As Integer
                            If Not Integer.TryParse(s.Substring(i, 4), NumberStyles.HexNumber, Inv, code) Then Return Nothing
                            i += 4
                            sb.Append(ChrW(code))
                        Case Else
                            Return Nothing
                    End Select
                Else
                    sb.Append(c)
                End If
            Loop
            Return Nothing
        End Function

#End Region

    End Class

End Namespace
