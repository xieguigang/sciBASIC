' ------------------------------------------------------------------------
' D3D12 调试层消息的纯数据视图
'
' 与 DeviceReport / EnvironmentReport 一样，这里只做数据承载，
' 排版与输出交给调用方（测试程序或日志层），框架本身不依赖 Console。
' ------------------------------------------------------------------------

Namespace Diagnostics

    ''' <summary>
    ''' 一条 D3D12 调试层校验消息。
    ''' </summary>
    Public NotInheritable Class DmlValidationMessage

        ''' <summary>严重级别文本（<c>ERROR</c> / <c>WARNING</c> / <c>INFO</c> …）。</summary>
        Public ReadOnly Property Severity As String

        ''' <summary>分类文本（<c>RESOURCE_MANIPULATION</c> / <c>EXECUTION</c> …）。</summary>
        Public ReadOnly Property Category As String

        ''' <summary>运行时消息 ID。</summary>
        Public ReadOnly Property Id As Integer

        ''' <summary>运行时给出的说明文本。</summary>
        Public ReadOnly Property Description As String

        ''' <summary>消息来自哪一步操作（由捕获方标注，例如 <c>CreateCommittedResource</c>）。</summary>
        Public ReadOnly Property Operation As String

        Public Sub New(severity As String, category As String, id As Integer,
                       description As String, Optional operation As String = Nothing)

            Me.Severity = severity
            Me.Category = category
            Me.Id = id
            Me.Description = description
            Me.Operation = operation
        End Sub

        ''' <summary>是否为会中断执行的严重级别（CORRUPTION / ERROR）。</summary>
        Public ReadOnly Property IsError As Boolean
            Get
                Return String.Equals(Severity, "ERROR", StringComparison.OrdinalIgnoreCase) OrElse
                       String.Equals(Severity, "CORRUPTION", StringComparison.OrdinalIgnoreCase)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Dim where As String = If(String.IsNullOrEmpty(Operation), "", $" [{Operation}]")

            Return $"[{Severity}] {Category}#{Id}{where}: {Description}"
        End Function

    End Class

    ''' <summary>
    ''' 一组校验消息的便捷聚合（便于测试程序一次性排版）。
    ''' </summary>
    Public Module DmlValidationMessageExtensions

        ''' <summary>把消息列表拼成多行文本。</summary>
        Public Function ToText(messages As IEnumerable(Of DmlValidationMessage)) As String
            If messages Is Nothing Then
                Return ""
            End If

            Return String.Join(Environment.NewLine, messages.Select(Function(m) m.ToString()))
        End Function

    End Module

End Namespace
