Imports System.Globalization
Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Serialization.JSON.Formatter

Namespace ApplicationServices.Debugging

    ''' <summary>
    ''' 调试审计事件(<see cref="DebugAuditEvent"/>)的类别枚举
    ''' </summary>
    Public Enum DebugAuditEventKind As Integer

        ''' <summary>
        ''' 变量快照：由一次 <see cref="DebugAudit.Snapshot"/> 调用产生
        ''' </summary>
        Capture = 0
        ''' <summary>
        ''' 条件检查或者断言
        ''' </summary>
        Assertion = 1
        ''' <summary>
        ''' 由 <see cref="DebugAudit.Watch"/> 创建的计时作用域
        ''' </summary>
        Scope = 2
        ''' <summary>
        ''' 由 <see cref="DebugAudit.Mark"/> 写入的手动标记点
        ''' </summary>
        Mark = 3
    End Enum

    ''' <summary>
    ''' 一条结构化的调试审计事件：快照、断言、计时作用域以及标记点统一使用这个模型进行记录。
    ''' </summary>
    ''' <remarks>
    ''' 这个类型的所有属性都是可读写的公开属性，因此可以直接被 JSON 序列化工具以及表格打印工具所使用。
    ''' </remarks>
    Public Class DebugAuditEvent

        ''' <summary>
        ''' 事件发生的时间点(本地时间)
        ''' </summary>
        ''' <returns></returns>
        Public Property Time As Date = DateTime.Now
        ''' <summary>
        ''' 事件类别
        ''' </summary>
        ''' <returns></returns>
        Public Property Kind As DebugAuditEventKind = DebugAuditEventKind.Mark
        ''' <summary>
        ''' 日志等级标签：``warn`` / ``info`` / ``debug``
        ''' </summary>
        ''' <returns></returns>
        Public Property Level As MSG_TYPES
        ''' <summary>
        ''' 触发事件的表达式原文，这个属性值来自于编译器提供的 CallerArgumentExpression 特性
        ''' </summary>
        ''' <returns></returns>
        Public Property Expression As String
        ''' <summary>
        ''' 事件的描述信息，对于断言失败而言就是输出到终端上面的那一行文本
        ''' </summary>
        ''' <returns></returns>
        Public Property Message As String
        ''' <summary>
        ''' 触发事件的源代码文件路径，这个属性值来自于编译器提供的 CallerFilePath 特性
        ''' </summary>
        ''' <returns></returns>
        Public Property SourceFile As String
        ''' <summary>
        ''' 触发事件的源代码行号，未知的时候为-1
        ''' </summary>
        ''' <returns></returns>
        Public Property Line As Integer = -1
        ''' <summary>
        ''' 触发事件的函数名称，这个属性值来自于编译器提供的 CallerMemberName 特性
        ''' </summary>
        ''' <returns></returns>
        Public Property Member As String
        ''' <summary>
        ''' 触发事件所在的线程编号
        ''' </summary>
        ''' <returns></returns>
        Public Property ThreadId As Integer = Thread.CurrentThread.ManagedThreadId
        ''' <summary>
        ''' 计时作用域所花费的时间，单位为毫秒；对于非作用域事件，这个属性值为0
        ''' </summary>
        ''' <returns></returns>
        Public Property ElapsedMs As Double
        ''' <summary>
        ''' 这个事件所描述的操作是否成功？对于断言而言，条件成立的时候为<c>True</c>
        ''' </summary>
        ''' <returns></returns>
        Public Property Succeeded As Boolean = True

        ''' <summary>
        ''' 形如 ``Member (D:\a.vb:12)`` 的代码位置描述文本，包含完整的源代码文件路径
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Location As String
            Get
                Return Compose(SourceFile)
            End Get
        End Property

        ''' <summary>
        ''' 与 <see cref="Location"/> 相同，但是源代码文件只保留文件名称，用于表格输出这类需要控制列宽的场景
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ShortLocation As String
            Get
                If String.IsNullOrEmpty(SourceFile) Then
                    Return Compose(SourceFile)
                End If

                Dim fileName As String

                Try
                    fileName = System.IO.Path.GetFileName(SourceFile)
                Catch ex As Exception
                    fileName = SourceFile
                End Try

                Return Compose(If(String.IsNullOrEmpty(fileName), SourceFile, fileName))
            End Get
        End Property

        Private Function Compose(fileName As String) As String
            Dim caller As String = If(String.IsNullOrEmpty(Member), "<unknown>", Member)

            If String.IsNullOrEmpty(fileName) Then
                Return caller
            ElseIf Line <= 0 Then
                Return $"{caller} ({fileName})"
            Else
                Return $"{caller} ({fileName}:{CStr(Line)})"
            End If
        End Function

        ''' <summary>
        ''' 将当前的这个审计事件格式化为一行文本(不包含时间戳以及事件类别的前缀)
        ''' </summary>
        ''' <returns></returns>
        Public Function Describe() As String
            Dim body As String = If(String.IsNullOrEmpty(Message), Kind.ToString(), Message)
            Dim text As String = body

            If Kind = DebugAuditEventKind.Scope Then
                text = $"{text} -> {ElapsedMs.ToString("0.###", CultureInfo.InvariantCulture)}ms"
            End If

            Return $"{text} at {Location}"
        End Function

        ''' <summary>
        ''' 将当前的这个审计事件格式化为一行文本
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim timestamp As String = Time.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)

            Return $"[{timestamp}] [{Kind.ToString().ToLower()}] {Describe()}"
        End Function

        ''' <summary>
        ''' 将当前的这个审计事件序列化为一个合法的JSON对象的文本片段
        ''' </summary>
        ''' <param name="indent">当前对象所在层级的缩进字符串</param>
        ''' <returns></returns>
        Public Function ToJsonText(Optional indent As String = "") As String
            Dim inner As String = indent & "  "
            Dim sb As New StringBuilder()

            Call sb.Append(indent).AppendLine("{")
            Call sb.Append(inner).Append("""time"": ").Append(Time.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture).JsonText).AppendLine(",")
            Call sb.Append(inner).Append("""kind"": ").Append(Kind.ToString.JsonText).AppendLine(",")
            Call sb.Append(inner).Append("""level"": ").Append(Level.Description.ToLower.JsonText).AppendLine(",")
            Call sb.Append(inner).Append("""expression"": ").Append(Expression.JsonText).AppendLine(",")
            Call sb.Append(inner).Append("""message"": ").Append(Message.JsonText).AppendLine(",")
            Call sb.Append(inner).Append("""sourceFile"": ").Append(SourceFile.JsonText).AppendLine(",")
            Call sb.Append(inner).Append("""line"": ").Append(Line.ToString(CultureInfo.InvariantCulture)).AppendLine(",")
            Call sb.Append(inner).Append("""member"": ").Append(Member.JsonText).AppendLine(",")
            Call sb.Append(inner).Append("""threadId"": ").Append(ThreadId.ToString(CultureInfo.InvariantCulture)).AppendLine(",")
            Call sb.Append(inner).Append("""elapsedMs"": ").Append(ElapsedMs.ToString("0.###", CultureInfo.InvariantCulture)).AppendLine(",")
            Call sb.Append(inner).Append("""succeeded"": ").Append(If(Succeeded, "true", "false")).AppendLine()
            Call sb.Append(indent).Append("}")

            Return sb.ToString()
        End Function
    End Class
End Namespace
