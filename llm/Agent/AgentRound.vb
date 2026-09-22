Namespace Agent

    ''' <summary>工具循环中的一轮。</summary>
    Public Class AgentRound

        ''' <summary>Zero based index of this round inside the run.</summary>
        Public Property Index As Integer

        ''' <summary>模型在本轮"自由生成"阶段产出的文本。</summary>
        Public Property AssistantText As String

        ''' <summary>本轮是否成功发起了一次工具调用。</summary>
        Public Property HasToolCall As Boolean

        ''' <summary>被调用的工具名。</summary>
        Public Property ToolName As String

        ''' <summary>
        ''' 工具名是否由模型自主决策得出。
        ''' </summary>
        ''' <remarks>
        ''' <see langword="False"/> 表示本轮走了演示模式（<see cref="AgentLoopOptions.ForcedToolName"/>），
        ''' 工具名是外部指定的。演示输出必须把这两种情况区分开，否则会让人误以为
        ''' 模型真的学会了发起调用。
        ''' </remarks>
        Public Property ToolDecidedByModel As Boolean = True

        ''' <summary>完整的调用片段文本（头部 + 参数 + 尾部）。</summary>
        Public Property ToolCallText As String

        ''' <summary>约束解码产出的参数 JSON。</summary>
        Public Property ArgumentsJson As String

        ''' <summary>解析后的参数表。</summary>
        Public Property Arguments As Dictionary(Of String, String)

        ''' <summary>工具的返回值（或结构化错误说明）。</summary>
        Public Property ToolResult As String

        ''' <summary>本轮是否使用了约束解码。</summary>
        Public Property UsedConstrainedDecoding As Boolean

        ''' <summary>约束解码的逐 token 轨迹：<c>状态 / 合法 token 数 / 掩码 token 数 / 已生成文本</c>。</summary>
        Public Property ConstrainedTrace As New List(Of String)

        ''' <summary>本轮开始时上下文的 token 数。</summary>
        Public Property ContextTokensBefore As Integer

        ''' <summary>本轮结束时上下文的 token 数。</summary>
        Public Property ContextTokensAfter As Integer

        ''' <summary>本轮填回工具结果时复用的 KV Cache 前缀长度。</summary>
        Public Property ReusedPrefixTokens As Integer

    End Class

End Namespace