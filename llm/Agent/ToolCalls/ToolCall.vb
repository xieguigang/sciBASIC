Namespace Agent.ToolCalls

    ''' <summary>一次工具调用的解析结果。</summary>
    Public Class ToolCall

        ''' <summary>被调用的工具名。</summary>
        Public Property Name As String

        ''' <summary>参数的原始 JSON 文本（可能带代码围栏之前的内容）。</summary>
        Public Property ArgumentsJson As String

        ''' <summary>解析后的参数键值对（值一律按字符串保存，由工具自己解释类型）。</summary>
        Public Property Arguments As New Dictionary(Of String, String)

        ''' <summary>第几个调用（0 基）。</summary>
        Public Property Index As Integer

        ''' <summary>Returns the call rendered in the textual protocol form.</summary>
        ''' <returns>A text of the form <c>name(arg=value, ...)</c>.</returns>
        Public Overrides Function ToString() As String
            Dim args = String.Join(", ", Arguments.Select(Function(kv) kv.Key & "=" & kv.Value))

            Return $"{Name}({args})"
        End Function

    End Class
End Namespace