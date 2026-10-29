Imports System.IO
Imports System.Text.Json
Imports Microsoft.VisualBasic.ApplicationServices.Debugging

''' <summary>
''' <see cref="DebugAudit"/> 运行时调试审计模块的回归测试。
''' 
''' 直接运行：
''' 
''' ```
''' test.exe --debugaudit
''' ```
''' </summary>
Friend Module DebugAuditSelfTest

    ''' <summary>
    ''' 当前失败的断言数量
    ''' </summary>
    Private failedChecks As Integer = 0

    Public Sub Run()
        failedChecks = 0

#If Not DEBUG Then
        ' DebugAudit.Check 与 DebugAudit.Snapshot 都标记了 Conditional("DEBUG")，
        ' 在发布版本之中这些调用会被编译器整体消除掉，因此这个测试只对调试版本有意义。
        Console.WriteLine("DebugAuditSelfTest: Check/Snapshot are removed by the compiler in a Release build.")
        Console.WriteLine("                   please run this self test with a Debug build: dotnet run -c Debug -- --debugaudit")
        Return
#End If

        Dim originMute As Boolean = VBDebugger.Mute
        Dim originThrow As Boolean = DebugAudit.ThrowOnFailure
        Dim originMaxEvents As Integer = DebugAudit.MaxEvents

        Try
            Call DebugAudit.Reset()

            ' 整个测试过程之中静默掉调试输出，最后一个用例会单独验证 Mute 的效果
            VBDebugger.Mute = True
            DebugAudit.ThrowOnFailure = False

            Call TestV()
            Call TestCaptures()
            Call TestSnapshot()
            Call TestCheck()
            Call TestTestAndRequire()
            Call TestThrowOnFailure()
            Call TestAreEqual()
            Call TestCheckThrows()
            Call TestWatch()
            Call TestMark()
            Call TestMaxEvents()
            Call TestReset()
            Call TestDumpAndJson()
            Call TestMuteOutput()
            Call TestConcurrency()
        Finally
            DebugAudit.MaxEvents = originMaxEvents
            DebugAudit.ThrowOnFailure = originThrow
            VBDebugger.Mute = originMute
            Call DebugAudit.Reset()
        End Try

        Console.WriteLine()

        If failedChecks = 0 Then
            Console.WriteLine("DebugAuditSelfTest: all checks passed.")
        Else
            Throw New Exception($"DebugAuditSelfTest: {failedChecks} check(s) failed!")
        End If
    End Sub

    Private Sub AssertTrue(condition As Boolean, message As String)
        If condition Then
            Console.WriteLine($"  [ok]   {message}")
        Else
            failedChecks += 1
            Console.WriteLine($"  [FAIL] {message}")
        End If
    End Sub

    ''' <summary>
    ''' ``V`` 函数需要把表达式的原文以及它的值一起返回
    ''' </summary>
    Private Sub TestV()
        Console.WriteLine("[V] value expression echo")

        Dim x As Integer = 5
        Dim pair = DebugAudit.V(x * 2)

        Call AssertTrue(pair.name = "x * 2", $"表达式原文: '{pair.name}'")
        Call AssertTrue(CInt(pair.value) = 10, "表达式的值")
        Call AssertTrue(DebugAudit.V(x).name = "x", "简单变量的表达式原文")
        Call AssertTrue(DebugAudit.V(Nothing).name = "Nothing", "Nothing 表达式的原文")
    End Sub

    ''' <summary>
    ''' 增量式的变量记录以及快照副本的隔离性
    ''' </summary>
    Private Sub TestCaptures()
        Console.WriteLine("[Capture] incremental captures")

        Call DebugAudit.Reset()

        Call DebugAudit.Capture("a", 1)
        Call DebugAudit.Capture("", 2)
        Call DebugAudit.Capture(Nothing, 3)

        Dim snapshot = DebugAudit.GetCaptures()

        Call AssertTrue(snapshot.Count = 1, $"名字为空的变量被忽略: {snapshot.Count}")
        Call AssertTrue(CInt(snapshot("a")) = 1, "变量值")

        snapshot("injected") = 123

        Call AssertTrue(DebugAudit.GetCaptures().Count = 1, "GetCaptures 返回的是副本")
    End Sub

    ''' <summary>
    ''' 整体替换式的变量快照
    ''' </summary>
    Private Sub TestSnapshot()
        Console.WriteLine("[Snapshot] whole snapshot replacement")

        Call DebugAudit.Reset()
        Call DebugAudit.Capture("stale", 1)

        Call DebugAudit.Snapshot(DebugAudit.V(1 + 1), ("named", "text"), (Nothing, 0))

        Dim snapshot = DebugAudit.GetCaptures()

        Call AssertTrue(snapshot.Count = 2, $"快照整体替换 + 空名字跳过: {snapshot.Count}")
        Call AssertTrue(Not snapshot.ContainsKey("stale"), "旧的变量被整体替换")
        Call AssertTrue(CInt(snapshot("1 + 1")) = 2, "元组参数的变量名与值")
        Call AssertTrue(CStr(snapshot("named")) = "text", "手工指定的变量名")
        Call AssertTrue(DebugAudit.Records.Where(Function(r) r.Kind = DebugAuditEventKind.Capture).Count() = 1,
                        "Snapshot 写入 Capture 事件")
    End Sub

    ''' <summary>
    ''' 兼容原有用法的 <see cref="DebugAudit.Check"/>
    ''' </summary>
    Private Sub TestCheck()
        Console.WriteLine("[Check] assertion failures")

        Call DebugAudit.Reset()

        Dim x As Integer = -1

        Call DebugAudit.Check(1 + 1 = 2)

        Call AssertTrue(DebugAudit.FailureCount = 0, "条件成立时不记录失败")
        Call AssertTrue(DebugAudit.LastFailure Is Nothing, "条件成立时没有最近失败")
        Call AssertTrue(DebugAudit.Records.Length = 0, "条件成立时不写入事件")

        Call DebugAudit.Check(x > 0)

        Dim last = DebugAudit.LastFailure

        Call AssertTrue(DebugAudit.FailureCount = 1, "条件不成立时失败计数加一")
        Call AssertTrue(last IsNot Nothing, "记录了最近失败事件")
        Call AssertTrue(last.Kind = DebugAuditEventKind.Assertion, "失败事件的类别")
        Call AssertTrue(last.Expression = "x > 0", $"失败事件记录了表达式原文: '{last.Expression}'")
        Call AssertTrue(last.Succeeded = False, "失败事件的 Succeeded 标记")
        Call AssertTrue(last.Message.StartsWith("Assertion failed:"), $"失败消息的前缀: '{last.Message}'")
        Call AssertTrue(Not String.IsNullOrEmpty(last.SourceFile) AndAlso last.SourceFile.EndsWith(".vb"), "记录了源代码文件")
        Call AssertTrue(last.Line > 0, $"记录了源代码行号: {last.Line}")
        Call AssertTrue(last.Member = NameOf(TestCheck), $"记录了函数名: '{last.Member}'")
        Call AssertTrue(last.ThreadId > 0, "记录了线程编号")
        Call AssertTrue(last.ToString().Contains(" at "), $"失败文本包含 at: '{last.ToString()}'")
        Call AssertTrue(last.Location.Contains(NameOf(TestCheck)), $"Location 文本: '{last.Location}'")

        Call DebugAudit.Check(False, "custom message")

        Call AssertTrue(DebugAudit.LastFailure.Message.StartsWith("custom message"), "自定义失败消息优先")
        Call AssertTrue(DebugAudit.FailureCount = 2, "第二次失败的计数")
    End Sub

    ''' <summary>
    ''' <see cref="DebugAudit.Test"/> 与 <see cref="DebugAudit.Require"/>
    ''' </summary>
    Private Sub TestTestAndRequire()
        Console.WriteLine("[Test/Require] boolean result and fail-fast")

        Call DebugAudit.Reset()

        Call AssertTrue(DebugAudit.Test(True, "ok"), "Test 成功的时候返回 True")
        Call AssertTrue(DebugAudit.FailureCount = 0, "Test 成功的时候不记录失败")

        Dim x As Integer = -1

        Call AssertTrue(Not DebugAudit.Test(x > 0), "Test 失败的时候返回 False")
        Call AssertTrue(DebugAudit.FailureCount = 1, "Test 失败的时候记录失败")
        Call AssertTrue(DebugAudit.LastFailure.Expression = "x > 0", "Test 记录了表达式原文")

        Dim caught As DebugAssertionException = Nothing

        Try
            Call DebugAudit.Require(False, "must fail")
        Catch ex As DebugAssertionException
            caught = ex
        End Try

        Call AssertTrue(caught IsNot Nothing, "Require 失败的时候抛出 DebugAssertionException")
        Call AssertTrue(caught IsNot Nothing AndAlso caught.Expression = "False", $"异常的表达式原文: '{If(caught Is Nothing, "", caught.Expression)}'")
        Call AssertTrue(caught IsNot Nothing AndAlso caught.Line > 0, "异常携带行号")
        Call AssertTrue(caught IsNot Nothing AndAlso caught.Member = NameOf(TestTestAndRequire), $"异常携带函数名: '{If(caught Is Nothing, "", caught.Member)}'")
        Call AssertTrue(caught IsNot Nothing AndAlso caught.Message.StartsWith("must fail"), $"异常消息: '{If(caught Is Nothing, "", caught.Message)}'")
        Call AssertTrue(DebugAudit.FailureCount = 2, "Require 的失败同样计入失败总数")

        Call AssertTrue(DebugAudit.Require(True), "Require 成功的时候返回 True")
    End Sub

    ''' <summary>
    ''' 全局的 fail-fast 开关
    ''' </summary>
    Private Sub TestThrowOnFailure()
        Console.WriteLine("[ThrowOnFailure] fail-fast switch")

        Call DebugAudit.Reset()

        DebugAudit.ThrowOnFailure = True

        Dim caught As Exception = Nothing

        Try
            Call DebugAudit.Check(1 > 2)
        Catch ex As Exception
            caught = ex
        End Try

        DebugAudit.ThrowOnFailure = False

        Call AssertTrue(TypeOf caught Is DebugAssertionException, "开关打开之后 Check 抛出异常")
        Call AssertTrue(DebugAudit.FailureCount = 1, "抛异常的失败也被记录")

        Dim after As Exception = Nothing

        Try
            Call DebugAudit.Check(1 > 2)
        Catch ex As Exception
            after = ex
        End Try

        Call AssertTrue(after Is Nothing, "开关关闭之后恢复为只记录不抛出")
    End Sub

    ''' <summary>
    ''' 值相等检查
    ''' </summary>
    Private Sub TestAreEqual()
        Console.WriteLine("[AreEqual] value comparison")

        Call DebugAudit.Reset()

        Call AssertTrue(DebugAudit.AreEqual(3, 3), "相等的时候返回 True")
        Call AssertTrue(DebugAudit.FailureCount = 0, "相等的时候不记录失败")

        Call AssertTrue(Not DebugAudit.AreEqual("expected", "actual"), "不相等的时候返回 False")
        Call AssertTrue(DebugAudit.LastFailure.Message.Contains("expected 'expected'"), $"失败消息包含期望值: '{DebugAudit.LastFailure.Message}'")
        Call AssertTrue(DebugAudit.LastFailure.Message.Contains("actual"), "失败消息包含实际值")
    End Sub

    ''' <summary>
    ''' 异常检查
    ''' </summary>
    Private Sub TestCheckThrows()
        Console.WriteLine("[CheckThrows] exception expectation")

        Call DebugAudit.Reset()

        Call AssertTrue(DebugAudit.CheckThrows(Of InvalidOperationException)(
            Sub() Throw New InvalidOperationException("boom")), "抛出期望的异常类型")

        Call AssertTrue(Not DebugAudit.CheckThrows(Of ArgumentException)(
            Sub() Throw New InvalidOperationException("boom")), "异常类型不匹配的时候返回 False")

        Call AssertTrue(Not DebugAudit.CheckThrows(Of Exception)(
            Sub() Call DebugAudit.Capture("noop", 1)), "没有抛出异常的时候返回 False")

        Call AssertTrue(DebugAudit.FailureCount = 2, $"两次失败的异常检查: {DebugAudit.FailureCount}")

        Call AssertTrue(Not DebugAudit.CheckThrows(Of Exception)(Nothing), "空的代码段返回 False")
    End Sub

    ''' <summary>
    ''' 计时作用域
    ''' </summary>
    Private Sub TestWatch()
        Console.WriteLine("[Watch] timing scope")

        Call DebugAudit.Reset()

        Using timer = DebugAudit.Watch("loading", logOnExit:=False)
            Call timer.Capture("size", 42)
            Call timer.Capture("", 0)

            Call AssertTrue(timer.Values.Count = 1, "作用域内的空名字变量被忽略")
            Call AssertTrue(CInt(timer.GetValue("size")) = 42, "作用域内读取变量值")
            Call AssertTrue(timer.GetValue("unknown") Is Nothing, "读取不存在的变量")
            Call AssertTrue(timer.Name = "loading", "作用域名称")
        End Using

        Dim records = DebugAudit.Records.Where(Function(r) r.Kind = DebugAuditEventKind.Scope).ToArray()

        Call AssertTrue(records.Length = 1, $"写入了一条 Scope 事件: {records.Length}")
        Call AssertTrue(records(0).Message = "loading", "Scope 事件的名称")
        Call AssertTrue(records(0).Member = NameOf(TestWatch), $"Scope 事件记录了函数名: '{records(0).Member}'")
        Call AssertTrue(records(0).Line > 0, "Scope 事件记录了行号")
        Call AssertTrue(records(0).ElapsedMs >= 0, "Scope 事件的耗时")
        Call AssertTrue(DebugAudit.GetCaptures().ContainsKey("size"), "作用域内的变量合并到全局快照")
        Call AssertTrue(CInt(DebugAudit.GetCaptures()("size")) = 42, "合并之后的变量值")
    End Sub

    ''' <summary>
    ''' 手动标记点
    ''' </summary>
    Private Sub TestMark()
        Console.WriteLine("[Mark] manual checkpoints")

        Call DebugAudit.Reset()

        Call DebugAudit.Mark("stage-1")
        Call DebugAudit.Mark("items", 10)

        Dim marks = DebugAudit.Records.Where(Function(r) r.Kind = DebugAuditEventKind.Mark).ToArray()

        Call AssertTrue(marks.Length = 2, $"写入两条 Mark 事件: {marks.Length}")
        Call AssertTrue(marks(0).Message = "stage-1", "标记的名称")
        Call AssertTrue(marks(0).Member = NameOf(TestMark), $"标记记录了函数名: '{marks(0).Member}'")
        Call AssertTrue(marks(0).Line > 0, "标记记录了行号")
        Call AssertTrue(marks(1).Message.Contains("10"), $"标记同时记录变量值: '{marks(1).Message}'")
        Call AssertTrue(CInt(DebugAudit.GetCaptures()("items")) = 10, "带值的标记写入变量快照")
        Call AssertTrue(DebugAudit.FailureCount = 0, "标记不会产生失败")
    End Sub

    ''' <summary>
    ''' 事件缓冲区的长度上限
    ''' </summary>
    Private Sub TestMaxEvents()
        Console.WriteLine("[MaxEvents] bounded event buffer")

        Call DebugAudit.Reset()

        DebugAudit.MaxEvents = 4

        For i As Integer = 0 To 9
            Call DebugAudit.Mark($"event-{i}")
        Next

        Call AssertTrue(DebugAudit.Records.Length = 4, $"缓冲区上限生效: {DebugAudit.Records.Length}")
        Call AssertTrue(DebugAudit.Records(3).Message = "event-9", "保留的是最新的事件")

        DebugAudit.MaxEvents = 0

        Call AssertTrue(DebugAudit.MaxEvents = 1, "非法的事件上限被修正为 1")

        DebugAudit.MaxEvents = 256
    End Sub

    ''' <summary>
    ''' 重置
    ''' </summary>
    Private Sub TestReset()
        Console.WriteLine("[Reset] clear all audit data")

        Call DebugAudit.Capture("x", 1)
        Call DebugAudit.Mark("y")
        Call DebugAudit.Check(False)

        Call AssertTrue(DebugAudit.FailureCount > 0, "重置之前存在失败记录")

        Call DebugAudit.Reset()

        Call AssertTrue(DebugAudit.FailureCount = 0, "重置之后失败计数归零")
        Call AssertTrue(DebugAudit.Records.Length = 0, "重置之后事件缓冲区为空")
        Call AssertTrue(DebugAudit.GetCaptures().Count = 0, "重置之后变量快照为空")
        Call AssertTrue(DebugAudit.LastFailure Is Nothing, "重置之后最近失败为空")
    End Sub

    ''' <summary>
    ''' 文本表格以及JSON导出
    ''' </summary>
    Private Sub TestDumpAndJson()
        Console.WriteLine("[Dump/ToJson] report export")

        Call DebugAudit.Reset()

        Call DebugAudit.Capture("alpha", "quote"" inside")
        Call DebugAudit.Capture("beta", 12.5)
        Call DebugAudit.Mark("checkpoint")
        Call DebugAudit.Check(1 = 2)

        Dim text As String = DebugAudit.Dump()

        Console.WriteLine()
        Console.WriteLine(text)
        Console.WriteLine()

        Call AssertTrue(text.Contains("alpha"), "Dump 包含变量名")
        Call AssertTrue(text.Contains("12.5"), "Dump 包含变量值")
        Call AssertTrue(text.Contains("Assertion failed"), "Dump 包含失败信息")
        Call AssertTrue(text.Contains("checkpoint"), "Dump 包含标记点")
        Call AssertTrue(Not DebugAudit.Dump(includeRecords:=False).Contains("Assertion failed"), "可以关闭事件列表")

        Dim json As String = DebugAudit.ToJson()

        Using document = JsonDocument.Parse(json)
            Dim root = document.RootElement

            Call AssertTrue(root.GetProperty("captures").GetProperty("alpha").GetString() = "quote"" inside", "JSON 之中的变量值转义正确")
            Call AssertTrue(root.GetProperty("captures").GetProperty("beta").GetString() = "12.5", "JSON 之中的变量值")
            Call AssertTrue(root.GetProperty("failureCount").GetInt32() = 1, "JSON 之中的失败计数")
            Call AssertTrue(root.GetProperty("records").GetArrayLength() = 2, "JSON 之中的事件数量")
        End Using

        Call DebugAudit.Reset()

        Using document = JsonDocument.Parse(DebugAudit.ToJson(indent:=False))
            Call AssertTrue(document.RootElement.GetProperty("records").GetArrayLength() = 0, "空报告的JSON也是合法的")
            Call AssertTrue(document.RootElement.GetProperty("captures").ValueKind = JsonValueKind.Object, "空报告之中的 captures 是对象")
        End Using

        Dim report As String = Path.Combine(Path.GetTempPath(), $"debugaudit_{Guid.NewGuid().ToString("N")}.json")

        Try
            Call DebugAudit.SaveReport(report)

            Call AssertTrue(File.Exists(report), "SaveReport 写出报告文件")

            Using document = JsonDocument.Parse(File.ReadAllText(report))
                Call AssertTrue(document.RootElement.TryGetProperty("records", Nothing), "写出的报告是合法的JSON")
            End Using
        Finally
            If File.Exists(report) Then
                Call File.Delete(report)
            End If
        End Try

        Call DebugAudit.SaveReport(Nothing)
        Call AssertTrue(True, "空的报告路径不会抛出异常")
    End Sub

    ''' <summary>
    ''' 静默模式之下断言失败不应该输出任何信息
    ''' </summary>
    Private Sub TestMuteOutput()
        Console.WriteLine("[Mute] silent mode")

        Call DebugAudit.Reset()

        Dim origin As TextWriter = Console.Out
        Dim buffer As New StringWriter()

        VBDebugger.Mute = True
        Console.SetOut(buffer)

        Try
            Call DebugAudit.Check(1 = 2)
        Finally
            Console.SetOut(origin)
        End Try

        Call AssertTrue(buffer.ToString().Length = 0, $"静默模式没有输出: '{buffer.ToString()}'")
        Call AssertTrue(DebugAudit.FailureCount = 1, "静默模式依然记录失败")

        Call DebugAudit.Reset()

        Dim loud As New StringWriter()

        VBDebugger.Mute = False
        Console.SetOut(loud)

        Try
            Call DebugAudit.Check(1 = 2)
        Finally
            Console.SetOut(origin)
            VBDebugger.Mute = True
        End Try

        Call AssertTrue(loud.ToString().Contains("Assertion failed"), $"非静默模式会打印断言失败: '{loud.ToString().Trim()}'")
    End Sub

    ''' <summary>
    ''' 多线程并发写入不应该抛出异常或者丢失数据
    ''' </summary>
    Private Sub TestConcurrency()
        Console.WriteLine("[Concurrency] parallel capture and check")

        Call DebugAudit.Reset()

        Dim errors As Integer = 0

        Call System.Threading.Tasks.Parallel.For(0, 500,
            Sub(i As Integer)
                Try
                    Call DebugAudit.Capture($"v{i}", i)
                    Call DebugAudit.Test(i >= 0)
                Catch ex As Exception
                    Call System.Threading.Interlocked.Increment(errors)
                End Try
            End Sub)

        Call AssertTrue(errors = 0, $"并发调用没有抛出异常: {errors}")
        Call AssertTrue(DebugAudit.GetCaptures().Count = 500, $"并发写入的变量完整: {DebugAudit.GetCaptures().Count}")
        Call AssertTrue(DebugAudit.FailureCount = 0, "并发的成功检查没有产生失败记录")
    End Sub
End Module
