Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.Flags
Imports Microsoft.VisualBasic.Serialization.JSON.Formatter

Namespace ApplicationServices.Debugging

    ''' <summary>
    ''' 运行时的调试审计模块：提供变量快照、条件检查/断言、计时作用域以及标记点这四类调试审计能力，
    ''' 并且把所有的审计记录统一记录到有长度上限的事件缓冲区之中，可以导出为文本表格或者JSON文本。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 断言失败的时候默认只是记录一条 <see cref="DebugAuditEventKind.Assertion"/> 事件并且通过
    ''' <see cref="VBDebugger"/> 的日志通道打印一条警告级别的信息(受命令行参数 ``echo`` 以及 ``/mute`` 控制)，
    ''' 不会打断程序的执行；如果需要 fail-fast 的效果，可以打开 <see cref="ThrowOnFailure"/> 开关，
    ''' 或者直接调用 <see cref="Require"/> 方法。
    ''' </para>
    ''' <para>
    ''' 条件成立的成功路径上面不会分配内存、不会加锁，也不会产生任何日志输出，
    ''' 因此在调试版本的程序之中可以放心地在热路径上面使用。
    ''' </para>
    ''' </remarks>
    Public Module DebugAudit

        ''' <summary>
        ''' 审计事件缓冲区的默认长度上限
        ''' </summary>
        Private Const DefaultMaxEvents As Integer = 256
        ''' <summary>
        ''' 变量值在输出为文本的时候的最大长度，超出的部分会被截断
        ''' </summary>
        Private Const MaxValueTextLength As Integer = 512

        Private ReadOnly sync As New Object()
        ''' <summary>
        ''' 全局的变量快照，<see cref="Snapshot"/> 会整体替换掉这个引用
        ''' </summary>
        Private captures As New Dictionary(Of String, Object)
        Private m_records As New List(Of DebugAuditEvent)
        Private m_maxEvents As Integer = DefaultMaxEvents
        Private m_throwOnFailure As Boolean = False
        Private m_failureCount As Integer = 0
        Private m_lastFailure As DebugAuditEvent = Nothing

        ''' <summary>
        ''' 审计事件缓冲区的长度上限，默认是256条事件。超出上限的时候最旧的事件会被丢弃。
        ''' </summary>
        ''' <returns></returns>
        Public Property MaxEvents As Integer
            Get
                Return m_maxEvents
            End Get
            Set(value As Integer)
                SyncLock sync
                    m_maxEvents = If(value < 1, 1, value)
                    Call TrimRecords()
                End SyncLock
            End Set
        End Property

        ''' <summary>
        ''' 断言失败的时候是否直接抛出 <see cref="DebugAssertionException"/> 异常(fail-fast)。
        ''' 默认值为<c>False</c>，即只记录失败并且打印警告信息，不会打断当前程序的执行过程。
        ''' </summary>
        ''' <returns></returns>
        Public Property ThrowOnFailure As Boolean
            Get
                Return m_throwOnFailure
            End Get
            Set(value As Boolean)
                m_throwOnFailure = value
            End Set
        End Property

        ''' <summary>
        ''' 从程序启动到目前为止所发生的断言失败的总次数
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property FailureCount As Integer
            Get
                SyncLock sync
                    Return m_failureCount
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' 最近一次断言失败所对应的审计事件，没有发生过失败的时候这个属性值为<c>Nothing</c>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LastFailure As DebugAuditEvent
            Get
                SyncLock sync
                    Return m_lastFailure
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' 获取当前的审计事件缓冲区的一个副本(按照事件发生的时间顺序排列)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Records As DebugAuditEvent()
            Get
                SyncLock sync
                    Return m_records.ToArray()
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' 获取当前的变量快照的一个副本。
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 返回的是副本，调用者对返回值的任何修改都不会影响到模块内部的快照数据。
        ''' </remarks>
        Public Function GetCaptures() As Dictionary(Of String, Object)
            SyncLock sync
                Return New Dictionary(Of String, Object)(captures)
            End SyncLock
        End Function

        ''' <summary>
        ''' 以增量的方式记录一个变量的值到全局的变量快照之中(与 <see cref="Snapshot"/> 的整体替换行为相反)
        ''' </summary>
        ''' <param name="name">变量的名称，为空的名字将会被忽略</param>
        ''' <param name="value">变量的值</param>
        ''' <example>
        ''' DebugAudit.Capture(Someone.Talk)
        ''' ' will generates the debug info
        ''' ' callerargumentexpression name = 'blablabla' value of Someone.Talk
        ''' ' captures["Someone.Talk"] = "blablabla"
        ''' </example>
        <Conditional("DEBUG")>
        Public Sub Capture(value As Object, <CallerArgumentExpression("value")> Optional name As String = Nothing)
            If String.IsNullOrEmpty(name) Then
                Return
            End If

            SyncLock sync
                captures(name) = value
            End SyncLock
        End Sub

        ''' <summary>
        ''' 清空当前的变量快照
        ''' </summary>
        Public Sub ClearCaptures()
            SyncLock sync
                captures = New Dictionary(Of String, Object)
            End SyncLock
        End Sub

        ''' <summary>
        ''' 清空所有的审计数据：变量快照、审计事件缓冲区以及失败计数
        ''' </summary>
        Public Sub Reset()
            SyncLock sync
                captures = New Dictionary(Of String, Object)

                m_records.Clear()
                m_failureCount = 0
                m_lastFailure = Nothing
            End SyncLock
        End Sub

        ''' <summary>
        ''' 整体的替换掉当前的变量快照，并且记录一条 <see cref="DebugAuditEventKind.Capture"/> 类型的事件。
        ''' </summary>
        ''' <param name="vars">需要记录的一组变量，名字为空的变量会被跳过</param>
        ''' <example>
        ''' <code>
        ''' Call DebugAudit.Snapshot(
        '''     DebugAudit.V(user.Profile.Age),
        '''     DebugAudit.V(user.Profile.Name),
        '''     DebugAudit.V(orders.Count))
        ''' </code>
        ''' </example>
        <Conditional("DEBUG")>
        Public Sub Snapshot(ParamArray vars As (name As String, value As Object)())
            Dim snapshot As New Dictionary(Of String, Object)
            Dim names As New List(Of String)

            If vars IsNot Nothing Then
                For Each item As (name As String, value As Object) In vars
                    If String.IsNullOrEmpty(item.name) Then
                        Continue For
                    End If

                    snapshot(item.name) = item.value
                    Call names.Add(item.name)
                Next
            End If

            SyncLock sync
                captures = snapshot
            End SyncLock

            Call AddRecord(New DebugAuditEvent With {
                .Kind = DebugAuditEventKind.Capture,
                .Level = MSG_TYPES.DEBUG,
                .Message = $"captured {CStr(names.Count)} variable(s): {String.Join(", ", names)}",
                .Succeeded = True
            })
        End Sub

        ''' <summary>
        ''' 检查一个条件是否成立，条件不成立的时候记录一条断言失败的审计事件，并且打印警告信息
        ''' (如果 <see cref="ThrowOnFailure"/> 开关被打开，则会抛出 <see cref="DebugAssertionException"/> 异常)。
        ''' </summary>
        ''' <param name="condition">需要检查的条件表达式</param>
        ''' <param name="msg">自定义的失败信息，为空的时候使用表达式原文</param>
        ''' <param name="expr">条件表达式的原文，由编译器自动传入</param>
        ''' <param name="sourceFile">源代码文件路径，由编译器自动传入</param>
        ''' <param name="line">源代码行号，由编译器自动传入</param>
        ''' <param name="callee">函数名称，由编译器自动传入</param>
        <Conditional("DEBUG")>
        Public Sub Check(condition As Boolean,
                         Optional msg As String = Nothing,
                         <CallerArgumentExpression("condition")> Optional expr As String = Nothing,
                         <CallerFilePath> Optional sourceFile As String = Nothing,
                         <CallerLineNumber> Optional line As Integer = -1,
                         <CallerMemberName> Optional callee As String = Nothing)

            If condition Then
                Return
            End If

            Dim failure As DebugAssertionException = RecordFailure(
                FailureOf(expr, msg, sourceFile, line, callee), throwNow:=False)

            If failure IsNot Nothing Then
                Throw failure
            End If
        End Sub

        ''' <summary>
        ''' 检查一个条件是否成立，并且把检查结果作为函数的返回值返回(条件不成立的时候返回<c>False</c>)。
        ''' 与 <see cref="Check"/> 的区别在于这个方法在发布版本之中不会被编译器消除，因此可以用于
        ''' 需要在运行时(包括Release版本)动态决定是否执行后续逻辑的场合。
        ''' </summary>
        ''' <param name="condition">需要检查的条件表达式</param>
        ''' <param name="msg">自定义的失败信息</param>
        ''' <param name="expr">条件表达式的原文，由编译器自动传入</param>
        ''' <param name="sourceFile">源代码文件路径，由编译器自动传入</param>
        ''' <param name="line">源代码行号，由编译器自动传入</param>
        ''' <param name="callee">函数名称，由编译器自动传入</param>
        ''' <returns></returns>
        Public Function Test(condition As Boolean,
                             Optional msg As String = Nothing,
                             <CallerArgumentExpression("condition")> Optional expr As String = Nothing,
                             <CallerFilePath> Optional sourceFile As String = Nothing,
                             <CallerLineNumber> Optional line As Integer = -1,
                             <CallerMemberName> Optional callee As String = Nothing) As Boolean

            If condition Then
                Return True
            End If

            Dim failure As DebugAssertionException = RecordFailure(
                FailureOf(expr, msg, sourceFile, line, callee), throwNow:=False)

            If failure IsNot Nothing Then
                Throw failure
            End If

            Return False
        End Function

        ''' <summary>
        ''' 强制性的要求一个条件成立，条件不成立的时候会立即抛出 <see cref="DebugAssertionException"/> 异常。
        ''' </summary>
        ''' <param name="condition">需要检查的条件表达式</param>
        ''' <param name="msg">自定义的失败信息</param>
        ''' <param name="expr">条件表达式的原文，由编译器自动传入</param>
        ''' <param name="sourceFile">源代码文件路径，由编译器自动传入</param>
        ''' <param name="line">源代码行号，由编译器自动传入</param>
        ''' <param name="callee">函数名称，由编译器自动传入</param>
        ''' <returns>条件成立的时候返回<c>True</c></returns>
        Public Function Require(condition As Boolean,
                                Optional msg As String = Nothing,
                                <CallerArgumentExpression("condition")> Optional expr As String = Nothing,
                                <CallerFilePath> Optional sourceFile As String = Nothing,
                                <CallerLineNumber> Optional line As Integer = -1,
                                <CallerMemberName> Optional callee As String = Nothing) As Boolean

            If condition Then
                Return True
            End If

            Dim failure As DebugAssertionException = RecordFailure(
                FailureOf(expr, msg, sourceFile, line, callee), throwNow:=True)

            If failure IsNot Nothing Then
                Throw failure
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' 检查两个值是否相等，不相等的时候记录一条断言失败事件。
        ''' </summary>
        ''' <typeparam name="T">需要比较的值类型</typeparam>
        ''' <param name="expected">期望的值</param>
        ''' <param name="actual">实际的值</param>
        ''' <param name="msg">自定义的失败信息</param>
        ''' <param name="expr">实际值表达式的原文，由编译器自动传入</param>
        ''' <param name="sourceFile">源代码文件路径，由编译器自动传入</param>
        ''' <param name="line">源代码行号，由编译器自动传入</param>
        ''' <param name="callee">函数名称，由编译器自动传入</param>
        ''' <returns></returns>
        Public Function AreEqual(Of T)(expected As T,
                                       actual As T,
                                       Optional msg As String = Nothing,
                                       <CallerArgumentExpression("actual")> Optional expr As String = Nothing,
                                       <CallerFilePath> Optional sourceFile As String = Nothing,
                                       <CallerLineNumber> Optional line As Integer = -1,
                                       <CallerMemberName> Optional callee As String = Nothing) As Boolean

            If EqualityComparer(Of T).Default.Equals(expected, actual) Then
                Return True
            End If

            Dim details As String =
                $"expected '{ValueText(expected)}' but actually is '{ValueText(actual)}'"
            Dim failure As DebugAssertionException = RecordFailure(
                FailureOf(expr, If(String.IsNullOrEmpty(msg), details, msg & " -> " & details), sourceFile, line, callee),
                throwNow:=False)

            If failure IsNot Nothing Then
                Throw failure
            End If

            Return False
        End Function

        ''' <summary>
        ''' 检查一段代码是否抛出了指定类型的异常，如果没有抛出异常或者抛出的异常类型不匹配，都会记录一条断言失败事件。
        ''' </summary>
        ''' <typeparam name="TException">期望抛出的异常类型</typeparam>
        ''' <param name="code">需要执行的代码段</param>
        ''' <param name="msg">自定义的失败信息</param>
        ''' <param name="expr">代码段表达式的原文，由编译器自动传入</param>
        ''' <param name="sourceFile">源代码文件路径，由编译器自动传入</param>
        ''' <param name="line">源代码行号，由编译器自动传入</param>
        ''' <param name="callee">函数名称，由编译器自动传入</param>
        ''' <returns></returns>
        Public Function CheckThrows(Of TException As Exception)(code As Action,
                                                                Optional msg As String = Nothing,
                                                                <CallerArgumentExpression("code")> Optional expr As String = Nothing,
                                                                <CallerFilePath> Optional sourceFile As String = Nothing,
                                                                <CallerLineNumber> Optional line As Integer = -1,
                                                                <CallerMemberName> Optional callee As String = Nothing) As Boolean

            If code Is Nothing Then
                Return DebugAudit.Test(False, If(msg, "CheckThrows: nothing to test"), expr, sourceFile, line, callee)
            End If

            Dim failure As Exception = Nothing

            Try
                Call code()
            Catch ex As Exception
                failure = ex
            End Try

            If failure Is Nothing Then
                Return DebugAudit.Test(False,
                                       If(msg, $"no exception was thrown, but {GetType(TException).Name} was expected"),
                                       expr, sourceFile, line, callee)
            ElseIf TypeOf failure Is TException Then
                Return DebugAudit.Test(True, msg, expr, sourceFile, line, callee)
            Else
                Return DebugAudit.Test(False,
                                       If(msg, $"{failure.GetType.Name} was thrown, but {GetType(TException).Name} was expected"),
                                       expr, sourceFile, line, callee)
            End If
        End Function

        ''' <summary>
        ''' 创建一个计时作用域对象，配合<c>Using</c>语句使用可以在代码段执行完毕的时候自动记录耗时信息。
        ''' </summary>
        ''' <param name="name">作用域的名称</param>
        ''' <param name="logOnExit">退出作用域的时候是否将耗时信息打印到调试终端</param>
        ''' <param name="sourceFile">源代码文件路径，由编译器自动传入</param>
        ''' <param name="line">源代码行号，由编译器自动传入</param>
        ''' <param name="callee">函数名称，由编译器自动传入</param>
        ''' <returns></returns>
        Public Function Watch(name As String,
                              Optional logOnExit As Boolean = True,
                              <CallerFilePath> Optional sourceFile As String = Nothing,
                              <CallerLineNumber> Optional line As Integer = -1,
                              <CallerMemberName> Optional callee As String = Nothing) As DebugAuditScope

            Return New DebugAuditScope(name, logOnExit, sourceFile, line, callee)
        End Function

        ''' <summary>
        ''' 记录一个手动的标记点。与 <see cref="Capture"/> 的区别在于这里还会额外写入一条
        ''' <see cref="DebugAuditEventKind.Mark"/> 类型的审计事件。
        ''' </summary>
        ''' <param name="name">标记的名称</param>
        ''' <param name="value">可选的相关变量值，不为空的时候会同时写入变量快照</param>
        ''' <param name="sourceFile">源代码文件路径，由编译器自动传入</param>
        ''' <param name="line">源代码行号，由编译器自动传入</param>
        ''' <param name="callee">函数名称，由编译器自动传入</param>
        Public Sub Mark(name As String,
                        Optional value As Object = Nothing,
                        <CallerFilePath> Optional sourceFile As String = Nothing,
                        <CallerLineNumber> Optional line As Integer = -1,
                        <CallerMemberName> Optional callee As String = Nothing)

            Dim label As String = If(String.IsNullOrEmpty(name), "<mark>", name)

            If Not String.IsNullOrEmpty(name) AndAlso value IsNot Nothing Then
                SyncLock sync
                    captures(name) = value
                End SyncLock
            End If

            Dim record As New DebugAuditEvent With {
                .Kind = DebugAuditEventKind.Mark,
                .Level = MSG_TYPES.INF,
                .Message = If(value Is Nothing, label, $"{label} = {ValueText(value)}"),
                .SourceFile = sourceFile,
                .Line = line,
                .Member = callee,
                .Succeeded = True
            }

            Call AddRecord(record)
            Call VBDebugger.log(record.ToString(), "audit", AnsiColor.BrightBlack, MSG_TYPES.INF, False, "")
        End Sub

        ''' <summary>
        ''' 将当前的变量快照以及审计事件缓冲区渲染为一个对齐的文本表格。
        ''' </summary>
        ''' <param name="includeRecords">是否输出审计事件列表</param>
        ''' <returns></returns>
        Public Function Dump(Optional includeRecords As Boolean = True) As String
            Dim sb As New StringBuilder()
            Dim snapshot As Dictionary(Of String, Object) = GetCaptures()
            Dim history As DebugAuditEvent() = Records

            Call sb.AppendLine(
                $"DebugAudit: {CStr(snapshot.Count)} capture(s), {CStr(FailureCount)} failure(s), {CStr(history.Length)} record(s)")

            If snapshot.Count = 0 Then
                Call sb.AppendLine()
                Call sb.AppendLine("<no captures>")
            Else
                Dim rows As New List(Of Object())

                For Each key As String In snapshot.Keys
                    Call rows.Add(New Object() {key, ValueText(snapshot(key))})
                Next

                Call sb.AppendLine()
                Call sb.AppendLine(PrintTable({"Name", "Value"}, rows))
            End If

            If includeRecords Then
                If history.Length = 0 Then
                    Call sb.AppendLine()
                    Call sb.AppendLine("<no records>")
                Else
                    Dim eventRows As New List(Of Object())

                    For Each record As DebugAuditEvent In history
                        Call eventRows.Add(New Object() {
                            record.Time.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture),
                            record.Kind.ToString(),
                            record.Level.Description,
                            If(record.Message, ""),
                            record.ShortLocation,
                            record.ElapsedMs.ToString("0.###", CultureInfo.InvariantCulture),
                            If(record.Succeeded, "ok", "failed")
                        })
                    Next

                    Call sb.AppendLine()
                    Call sb.AppendLine(PrintTable({"Time", "Kind", "Level", "Message", "Location", "Elapsed(ms)", "Status"}, eventRows))
                End If
            End If

            Return sb.ToString().TrimEnd()
        End Function

        ''' <summary>
        ''' 将当前的变量快照以及审计事件缓冲区序列化为一段合法的JSON文本
        ''' </summary>
        ''' <param name="indent">是否输出带缩进的美化格式</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这里的JSON文本是手写生成的，不依赖任何序列化框架，因此只要数据本身可以转换为文本就不会失败。
        ''' </remarks>
        Public Function ToJson(Optional indent As Boolean = True) As String
            Dim pad As String = If(indent, "  ", "")
            Dim inner As String = pad & pad
            Dim snapshot As Dictionary(Of String, Object) = GetCaptures()
            Dim history As DebugAuditEvent() = Records
            Dim keys As String() = snapshot.Keys.ToArray()
            Dim sb As New StringBuilder()

            Call sb.AppendLine("{")
            Call sb.Append(pad).Append("""captures"": {").AppendLine()

            For i As Integer = 0 To keys.Length - 1
                Dim comma As String = If(i < keys.Length - 1, ",", "")

                Call sb.Append(inner) _
                    .Append(keys(i).JsonText) _
                    .Append(": ") _
                    .Append(ValueText(snapshot(keys(i))).JsonText) _
                    .AppendLine(comma)
            Next

            Call sb.Append(pad).AppendLine("},")
            Call sb.Append(pad).Append("""failureCount"": ") _
                .Append(FailureCount.ToString(CultureInfo.InvariantCulture)).AppendLine(",")
            Call sb.Append(pad).Append("""records"": [").AppendLine()

            For i As Integer = 0 To history.Length - 1
                Dim comma As String = If(i < history.Length - 1, ",", "")

                Call sb.Append(history(i).ToJsonText(inner)).AppendLine(comma)
            Next

            Call sb.Append(pad).AppendLine("]")
            Call sb.AppendLine("}")

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' 将当前的调试审计报告以JSON格式保存到指定的文件之中
        ''' </summary>
        ''' <param name="path">目标文件的路径，父文件夹不存在的时候会自动创建</param>
        Public Sub SaveReport(path As String)
            If String.IsNullOrEmpty(path) Then
                Return
            End If

            Try
                Dim folder As String = System.IO.Path.GetDirectoryName(path)

                If Not String.IsNullOrEmpty(folder) AndAlso Not System.IO.Directory.Exists(folder) Then
                    Call System.IO.Directory.CreateDirectory(folder)
                End If

                Call System.IO.File.WriteAllText(path, ToJson(indent:=True), Encoding.UTF8)
            Catch ex As Exception
                ' 调试报告写盘失败不应该影响到业务代码的执行
                Call VBDebugger.log($"DebugAudit.SaveReport({path}) -> {ex.Message}",
                                    "audit", AnsiColor.BrightRed, MSG_TYPES.ERR, False, "")
            End Try
        End Sub

        ''' <summary>
        ''' 获取一个对象的文本表示，用于调试信息的输出。过长的文本会被截断，<c>Nothing</c>输出为<c>Nothing</c>。
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function ValueText(value As Object) As String
            If value Is Nothing Then
                Return "Nothing"
            End If

            Dim text As String

            Try
                text = value.ToString()
            Catch ex As Exception
                text = $"<{value.GetType.FullName} -> {ex.Message}>"
            End Try

            If text Is Nothing Then
                Return "Nothing"
            ElseIf text.Length > MaxValueTextLength Then
                Return text.Substring(0, MaxValueTextLength) & "..."
            Else
                Return text
            End If
        End Function

        ''' <summary>
        ''' 计时作用域结束的时候由 <see cref="DebugAuditScope"/> 对象进行回调
        ''' </summary>
        ''' <param name="scope"></param>
        Friend Sub EndScope(scope As DebugAuditScope)
            If scope Is Nothing Then
                Return
            End If

            Dim elapsed As Double = scope.ElapsedMilliseconds
            Dim values As Dictionary(Of String, Object) = scope.Values

            SyncLock sync
                For Each key As String In values.Keys
                    captures(key) = values(key)
                Next
            End SyncLock

            Dim record As New DebugAuditEvent With {
                .Kind = DebugAuditEventKind.Scope,
                .Level = MSG_TYPES.DEBUG,
                .Message = scope.Name,
                .SourceFile = scope.SourceFile,
                .Line = scope.Line,
                .Member = scope.Caller,
                .ElapsedMs = elapsed,
                .Succeeded = True
            }

            Call AddRecord(record)

            If scope.LogOnExit Then
                Call VBDebugger.log(record.ToString(), "audit", AnsiColor.BrightBlack, MSG_TYPES.DEBUG, False, "")
            End If
        End Sub

        Private Sub AddRecord(record As DebugAuditEvent)
            If record Is Nothing Then
                Return
            End If

            SyncLock sync
                Call m_records.Add(record)
                Call TrimRecords()
            End SyncLock
        End Sub

        ''' <remarks>这个函数只能够在<see cref="sync"/>锁的内部被调用</remarks>
        Private Sub TrimRecords()
            If m_records.Count <= m_maxEvents Then
                Return
            End If

            Call m_records.RemoveRange(0, m_records.Count - m_maxEvents)
        End Sub

        ''' <summary>
        ''' 记录一次断言失败：写入事件缓冲区、打印警告信息，并且在需要的时候返回一个待抛出的异常对象
        ''' </summary>
        ''' <param name="failure">失败的审计事件</param>
        ''' <param name="throwNow">是否无条件抛出异常</param>
        ''' <returns>需要抛出异常的时候返回非<c>Nothing</c>的异常对象</returns>
        Private Function RecordFailure(failure As DebugAuditEvent, throwNow As Boolean) As DebugAssertionException
            If failure Is Nothing Then
                Return Nothing
            End If

            SyncLock sync
                Call m_records.Add(failure)
                m_failureCount += 1
                m_lastFailure = failure
                Call TrimRecords()
            End SyncLock

            Call VBDebugger.log(failure.ToString(), "audit", AnsiColor.BrightYellow, MSG_TYPES.WRN, False, "")

            If throwNow OrElse m_throwOnFailure Then
                Return DebugAssertionException.Create(failure)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' 根据调用位置信息构建一条断言失败的审计事件
        ''' </summary>
        ''' <param name="expr">表达式原文</param>
        ''' <param name="msg">自定义消息</param>
        ''' <param name="sourceFile">源代码文件</param>
        ''' <param name="line">行号</param>
        ''' <param name="callee">函数名</param>
        ''' <returns></returns>
        Private Function FailureOf(expr As String, msg As String, sourceFile As String, line As Integer, callee As String) As DebugAuditEvent
            Dim expression As String = If(String.IsNullOrEmpty(expr), "<unknown>", expr)
            Dim text As String = If(String.IsNullOrEmpty(msg),
                                    $"Assertion failed: {expression}",
                                    $"{msg} ({expression})")

            Return New DebugAuditEvent With {
                .Kind = DebugAuditEventKind.Assertion,
                .Level = MSG_TYPES.WRN,
                .Expression = expression,
                .Message = text,
                .SourceFile = sourceFile,
                .Line = line,
                .Member = callee,
                .Succeeded = False
            }
        End Function

        ''' <summary>
        ''' 使用<see cref="ConsoleTableBuilder"/>将一组行数据渲染为文本表格，渲染失败的时候退化为简单的文本拼接
        ''' </summary>
        ''' <param name="columns">表头的列名称</param>
        ''' <param name="rows">表格的数据行</param>
        ''' <returns></returns>
        Private Function PrintTable(columns As String(), rows As List(Of Object())) As String
            Try
                Return ConsoleTableBuilder.From(rows).WithColumn(columns).Export().ToString().TrimEnd()
            Catch ex As Exception
                Dim sb As New StringBuilder()

                Call sb.AppendLine(String.Join(" | ", columns))

                For Each row As Object() In rows
                    Call sb.AppendLine(String.Join(" | ", row.Select(Function(v As Object) If(v Is Nothing, "", v.ToString())).ToArray()))
                Next

                Return sb.ToString().TrimEnd()
            End Try
        End Function

        ''' <summary>
        ''' 将一个表达式的原文与它的值作为一个元组返回，用于 <see cref="Snapshot"/> 的参数。
        ''' </summary>
        ''' <param name="value">表达式的值</param>
        ''' <param name="name">表达式的原文，由编译器自动传入</param>
        ''' <returns></returns>
        ''' <example>
        ''' DebugAudit.V(user.Profile.Age &gt; 18)
        ''' ' will generates tuple output
        ''' ' ( name,                  value)
        ''' ' ("user.Profile.Age &gt; 18", True)
        ''' </example>
        Public Function V(value As Object, <CallerArgumentExpression("value")> Optional name As String = Nothing) As (name As String, value As Object)
            Return (If(name, "<unknown>"), value)
        End Function
    End Module
End Namespace
