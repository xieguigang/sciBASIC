Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports System.Text

''' <summary>
''' 轻量的测试审计记录器：断言、失败计数与审计报告（文本表格 / JSON）。
''' </summary>
''' <remarks>
''' 这个模块的公开 API（``Reset`` / ``Check`` / ``AreEqual`` / ``CheckThrows`` / ``FailureCount`` /
''' ``Dump`` / ``SaveReport``）与核心库 ``ApplicationServices.Debugging.DebugAudit`` 保持一致，
''' 目的在于：当前工作区内的 ``Microsoft.VisualBasic.Core`` 副本尚未包含 DebugAudit 模块，
''' 因此这里提供一个自包含的等价实现；当核心库补齐之后，删除本文件并切换命名空间即可。
''' </remarks>
Public Module TestAudit

    ''' <summary>
    ''' 一条审计记录
    ''' </summary>
    Public Class AuditRecord

        Public Property Time As Date = Date.Now
        Public Property Kind As String
        Public Property Message As String
        Public Property SourceFile As String
        Public Property Line As Integer = -1
        Public Property Member As String
        Public Property Succeeded As Boolean = True

        ''' <summary>
        ''' 形如 ``Member (file:line)`` 的位置描述
        ''' </summary>
        Public ReadOnly Property Location As String
            Get
                Dim caller As String = If(String.IsNullOrEmpty(Member), "<unknown>", Member)
                Dim file As String = If(String.IsNullOrEmpty(SourceFile), "", IO.Path.GetFileName(SourceFile))

                If String.IsNullOrEmpty(file) Then
                    Return caller
                ElseIf Line <= 0 Then
                    Return $"{caller} ({file})"
                Else
                    Return $"{caller} ({file}:{CStr(Line)})"
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{Time.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)}] [{Kind.ToLower}] " &
                   $"{Message} at {Location} -> {If(Succeeded, "ok", "failed")}"
        End Function
    End Class

    Private ReadOnly sync As New Object()
    Private ReadOnly m_records As New List(Of AuditRecord)
    Private m_failureCount As Integer

    ''' <summary>
    ''' 从开始运行到现在的断言失败总次数
    ''' </summary>
    Public ReadOnly Property FailureCount As Integer
        Get
            SyncLock sync
                Return m_failureCount
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' 当前审计记录缓冲区的一个副本
    ''' </summary>
    Public ReadOnly Property Records As AuditRecord()
        Get
            SyncLock sync
                Return m_records.ToArray()
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' 清空审计记录与失败计数
    ''' </summary>
    Public Sub Reset()
        SyncLock sync
            m_records.Clear()
            m_failureCount = 0
        End SyncLock
    End Sub

    ''' <summary>
    ''' 检查一个条件是否成立，不成立的时候记录一条失败记录并且打印警告信息
    ''' </summary>
    ''' <param name="condition">需要检查的条件</param>
    ''' <param name="msg">自定义的失败信息</param>
    ''' <param name="expr">条件表达式原文，由编译器自动传入</param>
    ''' <param name="sourceFile">源代码文件，由编译器自动传入</param>
    ''' <param name="line">源代码行号，由编译器自动传入</param>
    ''' <param name="callee">函数名称，由编译器自动传入</param>
    ''' <returns>条件成立的时候返回<c>True</c></returns>
    Public Function Check(condition As Boolean,
                          Optional msg As String = Nothing,
                          <CallerArgumentExpression("condition")> Optional expr As String = Nothing,
                          <CallerFilePath> Optional sourceFile As String = Nothing,
                          <CallerLineNumber> Optional line As Integer = -1,
                          <CallerMemberName> Optional callee As String = Nothing) As Boolean

        Return Record(condition, msg, expr, sourceFile, line, callee)
    End Function

    ''' <summary>
    ''' 检查两个值是否相等，不相等的时候记录一条失败记录
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="expected">期望的值</param>
    ''' <param name="actual">实际的值</param>
    ''' <param name="msg">自定义的失败信息</param>
    ''' <param name="expr">实际值表达式原文，由编译器自动传入</param>
    ''' <param name="sourceFile">源代码文件，由编译器自动传入</param>
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
            Return Record(True, msg, expr, sourceFile, line, callee)
        Else
            Dim details As String =
                $"expected '{ValueText(expected)}' but actually is '{ValueText(actual)}'"

            Return Record(False, If(String.IsNullOrEmpty(msg), details, msg & " -> " & details),
                          expr, sourceFile, line, callee)
        End If
    End Function

    ''' <summary>
    ''' 检查一段代码是否抛出了指定类型的异常
    ''' </summary>
    ''' <typeparam name="TException">期望抛出的异常类型</typeparam>
    ''' <param name="code">需要执行的代码段</param>
    ''' <param name="msg">自定义的失败信息</param>
    ''' <param name="expr">代码段原文，由编译器自动传入</param>
    ''' <param name="sourceFile">源代码文件，由编译器自动传入</param>
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
            Return Record(False, If(msg, "CheckThrows: nothing to test"), expr, sourceFile, line, callee)
        End If

        Dim failure As Exception = Nothing

        Try
            Call code()
        Catch ex As Exception
            failure = ex
        End Try

        If failure Is Nothing Then
            Return Record(False, If(msg, $"no exception was thrown, but {GetType(TException).Name} was expected"),
                          expr, sourceFile, line, callee)
        ElseIf TypeOf failure Is TException Then
            Return Record(True, msg, expr, sourceFile, line, callee)
        Else
            Return Record(False, If(msg, $"{failure.GetType.Name} was thrown, but {GetType(TException).Name} was expected"),
                          expr, sourceFile, line, callee)
        End If
    End Function

    ''' <summary>
    ''' 将审计记录渲染为文本表格
    ''' </summary>
    ''' <returns></returns>
    Public Function Dump() As String
        Dim history As AuditRecord() = Records

        If history.Length = 0 Then
            Return "<no audit records>"
        End If

        Dim sb As New StringBuilder()

        Call sb.AppendLine($"TestAudit: {CStr(FailureCount)} failure(s), {CStr(history.Length)} record(s)")
        Call sb.AppendLine()

        For Each record As AuditRecord In history
            Call sb.AppendLine(record.ToString())
        Next

        Return sb.ToString().TrimEnd()
    End Function

    ''' <summary>
    ''' 将审计报告以 JSON 格式写入指定文件，父文件夹不存在的时候会自动创建
    ''' </summary>
    ''' <param name="path"></param>
    Public Sub SaveReport(path As String)
        If String.IsNullOrEmpty(path) Then
            Return
        End If

        Dim folder As String = IO.Path.GetDirectoryName(path)

        If Not String.IsNullOrEmpty(folder) AndAlso Not IO.Directory.Exists(folder) Then
            Call IO.Directory.CreateDirectory(folder)
        End If

        Call IO.File.WriteAllText(path, ToJson(), Encoding.UTF8)
    End Sub

    ''' <summary>
    ''' 将审计报告序列化为 JSON 文本
    ''' </summary>
    ''' <returns></returns>
    Public Function ToJson() As String
        Dim history As AuditRecord() = Records
        Dim sb As New StringBuilder()

        Call sb.AppendLine("{")
        Call sb.Append("  ""failureCount"": ").Append(FailureCount.ToString(CultureInfo.InvariantCulture)).AppendLine(",")
        Call sb.AppendLine("  ""records"": [")

        For i As Integer = 0 To history.Length - 1
            Dim record As AuditRecord = history(i)

            Call sb.AppendLine("    {")
            Call sb.Append("      ""time"": ").Append(record.Time.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture).Json).AppendLine(",")
            Call sb.Append("      ""kind"": ").Append(record.Kind.Json).AppendLine(",")
            Call sb.Append("      ""message"": ").Append(record.Message.Json).AppendLine(",")
            Call sb.Append("      ""sourceFile"": ").Append(record.SourceFile.Json).AppendLine(",")
            Call sb.Append("      ""line"": ").Append(record.Line.ToString(CultureInfo.InvariantCulture)).AppendLine(",")
            Call sb.Append("      ""member"": ").Append(record.Member.Json).AppendLine(",")
            Call sb.Append("      ""succeeded"": ").Append(If(record.Succeeded, "true", "false")).AppendLine()
            Call sb.Append("    }").AppendLine(If(i < history.Length - 1, ",", ""))
        Next

        Call sb.AppendLine("  ]")
        Call sb.AppendLine("}")

        Return sb.ToString()
    End Function

    Private Function Record(succeeded As Boolean,
                            msg As String,
                            expr As String,
                            sourceFile As String,
                            line As Integer,
                            callee As String) As Boolean

        Dim expression As String = If(String.IsNullOrEmpty(expr), "<unknown>", expr)
        Dim text As String = If(String.IsNullOrEmpty(msg), expression, $"{msg} ({expression})")

        Dim entry As New AuditRecord With {
            .Kind = If(succeeded, "check", "failure"),
            .Message = text,
            .SourceFile = sourceFile,
            .Line = line,
            .Member = callee,
            .Succeeded = succeeded
        }

        SyncLock sync
            Call m_records.Add(entry)

            If Not succeeded Then
                m_failureCount += 1
            End If
        End SyncLock

        If Not succeeded Then
            Call Console.Error.WriteLine($"[audit] {entry}")
        End If

        Return succeeded
    End Function

    Private Function ValueText(value As Object) As String
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
        ElseIf text.Length > 512 Then
            Return text.Substring(0, 512) & "..."
        Else
            Return text
        End If
    End Function

    <Runtime.CompilerServices.Extension>
    Private Function Json(text As String) As String
        If text Is Nothing Then
            Return "null"
        End If

        Dim sb As New StringBuilder("""")

        For Each ch As Char In text
            Select Case ch
                Case """"c
                    Call sb.Append("\""")
                Case "\"c
                    Call sb.Append("\\")
                Case vbCr
                    Call sb.Append("\r")
                Case vbLf
                    Call sb.Append("\n")
                Case vbTab
                    Call sb.Append("\t")
                Case Else
                    If AscW(ch) < 32 Then
                        Call sb.Append("\u").Append(AscW(ch).ToString("x4", CultureInfo.InvariantCulture))
                    Else
                        Call sb.Append(ch)
                    End If
            End Select
        Next

        Call sb.Append("""")

        Return sb.ToString()
    End Function
End Module
