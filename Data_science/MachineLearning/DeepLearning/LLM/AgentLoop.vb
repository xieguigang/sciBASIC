' ---------------------------------------------------------------------------
' AgentLoop —— function calling 的完整闭环
'
' 一次"决策 → 执行 → 回填"为一个 step，循环执行直到模型不再输出工具调用。
' 这正是 ReAct 框架的最小内核（Thought–Action–Observation 三元组），
' 也是所有 Agent 框架的公共骨架：
'
'     用户消息 + tools schema
'        → chat template 序列化进 prompt
'        → 模型自回归生成（可能输出 tool_call 特殊 token）
'        → 约束解码保证参数 JSON 合法
'        → 推理框架解析出 name + arguments
'        → 应用代码执行真实函数
'        → 结果以工具结果块注入上下文          ← 此处的 KV Cache 前缀被完整复用
'        → 回到生成
'        → 无工具 token 时输出纯文本，finish_reason: stop
'
' 两个工程细节在本实现中是被刻意展示出来的：
'
'   * <b>KV Cache 前缀复用</b>：工具结果回填时用 <see cref="TokenStream.Fill"/> 一次
'     prefill 把新增的 token 灌进缓存，而不是从头重算 —— 这是多轮工具调用的关键延迟优化；
'
'   * <b>约束解码只作用于参数对象</b>：调用哪个工具是模型该有的决策自由度，
'     因此工具名不入约束；参数的结构才需要被约束住。
' ---------------------------------------------------------------------------

Imports System.Text
Imports Diagnostics = System.Diagnostics

Namespace LLM

    ''' <summary>Agent 循环的超参。</summary>
    Public Class AgentLoopOptions

        ''' <summary>最大工具轮次（防循环调用的兜底）。</summary>
        Public Property MaxToolRounds As Integer = 4

        ''' <summary>每一轮"自由生成"阶段最多生成多少 token（用于发现工具调用信号灯）。</summary>
        Public Property MaxTokensPerReply As Integer = 24

        ''' <summary>约束解码阶段最多生成多少 token 来补全参数对象。</summary>
        Public Property MaxArgumentTokens As Integer = 96

        ''' <summary>是否启用 KV Cache。</summary>
        Public Property UseCache As Boolean = True

        ''' <summary>采样配置。</summary>
        Public Property Sampling As New SamplingConfig()

        ''' <summary>
        ''' 强制以该工具开始本轮调用。
        ''' </summary>
        ''' <remarks>
        ''' 小模型在训练不充分时未必能自己吐出工具调用信号灯。该选项让演示流程
        ''' 确定性地走到"约束解码"这一段，从而把算法本身讲清楚；置空则完全依赖模型的自主决策。
        ''' 无论哪种方式，参数对象都<b>必然</b>由约束解码产出。
        ''' </remarks>
        Public Property ForcedToolName As String

        ''' <summary>是否把过程打印到控制台。</summary>
        Public Property Verbose As Boolean = True

        ''' <summary>约束解码逐 token 轨迹的最大记录长度。</summary>
        Public Property TraceSteps As Integer = 16

    End Class

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
        public Property ReusedPrefixTokens As Integer

    End Class

    ''' <summary>一次完整 Agent 会话的结果。</summary>
    Public Class AgentRunResult

        ''' <summary>The original user message that started the run.</summary>
        Public Property UserMessage As String
        ''' <summary>The system prompt used for the run.</summary>
        Public Property SystemPrompt As String
        ''' <summary>All rounds executed by the loop, in order.</summary>
        Public Property Rounds As New List(Of AgentRound)
        ''' <summary>The last text produced by the model.</summary>
        Public Property FinalText As String
        ''' <summary>Total number of context tokens consumed by the run.</summary>
        Public Property TotalContextTokens As Integer
        ''' <summary>Size, in bytes, of the key/value cache held during the run.</summary>
        Public Property CacheBytes As Long
        ''' <summary>Wall clock duration of the run, in milliseconds.</summary>
        Public Property ElapsedMilliseconds As Double
        ''' <summary>Indicates whether the loop stopped because the round limit was reached.</summary>
        Public Property HitRoundLimit As Boolean

        ''' <summary>Number of rounds in which a tool call was issued.</summary>
        Public ReadOnly Property ToolCallCount As Integer
            Get
                Return Rounds.Where(Function(r) r.HasToolCall).Count()
            End Get
        End Property

    End Class

    ''' <summary>
    ''' 多轮工具调用循环：模型负责"想"，框架负责"做"。
    ''' </summary>
    Public Class AgentLoop

        Private ReadOnly _model As LLMModel
        Private ReadOnly _codec As ITextCodec
        Private ReadOnly _registry As ToolRegistry

        ''' <summary>被驱动的模型。</summary>
        Public ReadOnly Property Model As LLMModel
            Get
                Return _model
            End Get
        End Property

        ''' <summary>工具注册表。</summary>
        Public ReadOnly Property Registry As ToolRegistry
            Get
                Return _registry
            End Get
        End Property

        ''' <summary>
        ''' Creates the agent loop.
        ''' </summary>
        ''' <param name="model">The language model that generates text and tool calls.</param>
        ''' <param name="codec">The text codec used to encode prompts and decode outputs.</param>
        ''' <param name="registry">The registry that resolves and executes the tools requested by the model.</param>
        Public Sub New(model As LLMModel, codec As ITextCodec, registry As ToolRegistry)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If codec Is Nothing Then Throw New ArgumentNullException(NameOf(codec))
            If registry Is Nothing Then Throw New ArgumentNullException(NameOf(registry))

            _model = model
            _codec = codec
            _registry = registry
        End Sub

#Region "prompt 组装"

        ''' <summary>
        ''' 组装首轮 prompt：<c>{BOS}{system(含工具清单)}{User}{用户消息}{Assistant}</c>。
        ''' </summary>
        ''' <remarks>
        ''' 这里的形态与 tokenizer_config.json 中 chat_template 的语义保持一致：
        ''' system 内容直接跟在 BOS 之后（DeepSeek 模板不额外加角色标记），
        ''' 用户与助手各自用保留 token 标出边界。
        ''' </remarks>
        Public Function BuildInitialPrompt(systemPrompt As String, userMessage As String) As String
            Dim text As New StringBuilder()

            Call text.Append(ToolCallProtocol.BeginOfSentenceMarker)

            If Not String.IsNullOrEmpty(systemPrompt) Then
                Call text.Append(systemPrompt)
                Call text.Append(vbLf & vbLf)
            End If

            Call text.Append(ToolCallProtocol.UserMarker)
            Call text.Append(userMessage)
            Call text.Append(ToolCallProtocol.AssistantMarker)

            Return text.ToString()
        End Function

#End Region

#Region "主循环"

        ''' <summary>
        ''' 跑一轮完整的"决策 → 执行 → 回填"循环，直到模型给出纯文本回复或达到轮次上限。
        ''' </summary>
        ''' <param name="systemPrompt">系统提示（通常由 <see cref="ToolRegistry.RenderToolCatalog"/> 生成）</param>
        ''' <param name="userMessage">用户消息</param>
        ''' <param name="options">循环超参</param>
        public Function Run(systemPrompt As String, userMessage As String,
                            Optional options As AgentLoopOptions = Nothing) As AgentRunResult

            Dim opt = If(options, New AgentLoopOptions())
            Dim watch = Diagnostics.Stopwatch.StartNew()

            Dim promptText = BuildInitialPrompt(systemPrompt, userMessage)
            Dim promptIds = _codec.Encode(promptText)

            Dim result As New AgentRunResult With {
                .UserMessage = userMessage,
                .SystemPrompt = systemPrompt
            }

            Dim stream = TokenStream.Create(_model, promptIds, opt.UseCache)
            Dim sampler As New Sampler(opt.Sampling)

            If opt.Verbose Then
                Call Console.WriteLine($"[agent] prompt tokens = {promptIds.Length}, path = {If(opt.UseCache, "KV Cache", "recompute")}")
            End If

            Dim previousCallSignature As String = Nothing
            Dim finalText As String = ""

            For round As Integer = 1 To opt.MaxToolRounds
                Dim record As New AgentRound With {
                    .Index = round,
                    .ContextTokensBefore = stream.Context.Count
                }

                ' ---- 1. 自由生成：让模型自己决定"直接回答"还是"调用工具" ----
                Dim freeTokens = GenerateFree(stream, sampler, opt, record)

                ' ---- 2. 判断是否出现工具调用信号灯 ----
                Dim toolName As String = Nothing
                Dim textAfterMarker As String = Nothing

                If Not ToolCallProtocol.TryExtractToolName(record.AssistantText, toolName, textAfterMarker) Then
                    ' 只有在"演示模式 + 首轮"时才无条件发起调用，保证演示能走到约束解码那一段；
                    ' 其余情况一律尊重模型自己的选择：没有信号灯就是纯文本回复。
                    Dim forcedThisRound = round = 1 AndAlso Not String.IsNullOrEmpty(opt.ForcedToolName)

                    If Not forcedThisRound Then
                        finalText = record.AssistantText
                        result.Rounds.Add(record)
                        Exit For
                    End If

                    toolName = opt.ForcedToolName
                    textAfterMarker = Nothing
                    record.ToolDecidedByModel = False
                End If

                record.HasToolCall = True
                record.ToolName = toolName

                Dim tool = _registry.Find(toolName)

                If tool Is Nothing Then
                    ' 失败模式 1：幻觉工具名。把可用清单回填给模型，让它自我修正。
                    record.ToolResult = $"{{""error"": ""tool '{toolName}' does not exist; available: {String.Join(", ", _registry.Names)}""}}"
                    record.ToolCallText = record.AssistantText

                    If opt.Verbose Then Call Console.WriteLine($"[agent] round {round}: unknown tool '{toolName}' -> 回填错误让模型自我修正")

                    Call FeedBack(stream, record, _codec, opt)
                    result.Rounds.Add(record)

                    If round = opt.MaxToolRounds Then Exit For
                    Continue For
                End If

                ' ---- 3. 补齐调用头部（把模型没写全的部分补上）----
                Dim canonicalHeader = ToolCallProtocol.FormatCallHeader(toolName)
                Dim missing = SuffixAfterSharedPrefix(textAfterMarker, canonicalHeader)

                If missing.Length > 0 Then Call stream.Fill(_codec.Encode(missing))

                ' ---- 4. 约束解码生成参数对象 ----
                Dim decoder As New ConstrainedDecoder(tool.Schema, _codec.Vocabulary)

                record.UsedConstrainedDecoding = True

                Dim argumentTokens = decoder.Generate(stream, sampler, opt.MaxArgumentTokens)

                record.ArgumentsJson = decoder.GeneratedText
                Call CollectTrace(record, decoder, opt)

                If opt.Verbose Then
                    Call Console.WriteLine($"[agent] round {round}: 约束解码生成 {argumentTokens.Count} 个 token, " &
                                           $"最终状态 = {decoder.StateName}, 参数 = {decoder.GeneratedText}")
                End If

                ' ---- 5. 补齐调用尾部 ----
                Dim tail = ToolCallProtocol.FormatCallTail(terminate:=False)
                Call stream.Fill(_codec.Encode(tail))

                record.ToolCallText = ToolCallProtocol.CallsBeginMarker & canonicalHeader & decoder.GeneratedText & tail

                ' ---- 6. 解析参数并执行真实函数 ----
                record.Arguments = ToolCallProtocol.ParseArgumentObject(decoder.GeneratedText)

                Dim invocation As New ToolCall With {
                    .Name = toolName,
                    .ArgumentsJson = decoder.GeneratedText,
                    .Arguments = record.Arguments
                }

                record.ToolResult = _registry.Invoke(invocation)

                ' ---- 7. 把工具结果回填进上下文（复用 KV Cache 前缀）----
                Call FeedBack(stream, record, _codec, opt)

                If opt.Verbose Then
                    Call Console.WriteLine($"[agent] round {round}: {toolName}({String.Join(", ", record.Arguments.Select(Function(kv) kv.Key & "=" & kv.Value))}) -> {record.ToolResult}")
                End If

                ' 失败模式 3：重复调用检测
                Dim signature = $"{toolName}|{decoder.GeneratedText}"

                If Equals(signature, previousCallSignature) Then
                    Call stream.Fill(_codec.Encode(
                        "note: the same tool call was just repeated; answer the user directly using the observation above."))
                End If

                previousCallSignature = signature

                record.ContextTokensAfter = stream.Context.Count
                result.Rounds.Add(record)

                If round = opt.MaxToolRounds Then
                    result.HitRoundLimit = True
                    Exit For
                End If
            Next

            ' ---- 收尾：如果最后一轮仍是工具调用，再让模型给出一段文本回复 ----
            If String.IsNullOrEmpty(finalText) AndAlso result.Rounds.Count > 0 AndAlso
               result.Rounds.Last().HasToolCall Then

                Dim closing As New AgentRound With {
                    .Index = result.Rounds.Count + 1,
                    .ContextTokensBefore = stream.Context.Count
                }

                Call GenerateFree(stream, sampler, opt, closing)

                finalText = closing.AssistantText
                closing.ContextTokensAfter = stream.Context.Count
                result.Rounds.Add(closing)
            End If

            watch.Stop()

            result.FinalText = finalText
            result.TotalContextTokens = stream.Context.Count
            result.CacheBytes = stream.CacheBytes
            result.ElapsedMilliseconds = watch.Elapsed.TotalMilliseconds

            Return result
        End Function

#End Region

#Region "内部步骤"

        ''' <summary>
        ''' 自由生成一段文本（不加约束），遇到 EOS 或长度上限即停。
        ''' </summary>
        ''' <remarks>
        ''' 生成过程中每步都把"本轮已生成的 token"整体解码一次再检查信号灯 —— 之所以不做
        ''' 更聪明的增量解码，是因为保留标记（如 <c>&lt;｜tool▁calls▁begin｜&gt;</c>）在
        ''' 分词上可能被切成多个 token，逐 token 解码无法保证标记的完整性。
        ''' </remarks>
        Private Function GenerateFree(stream As TokenStream, sampler As Sampler,
                                      opt As AgentLoopOptions, record As AgentRound) As List(Of Integer)

            Dim produced As New List(Of Integer)()
            Dim eos = _codec.TokenIdOf(ToolCallProtocol.EndOfSentenceMarker)

            Dim detectedName As String = Nothing
            Dim detectedTail As String = Nothing

            For i As Integer = 1 To opt.MaxTokensPerReply
                Dim token = sampler.Sample(stream.CurrentLogits, stream.Context)

                produced.Add(token)
                Call stream.Accept(token)

                If token = eos Then Exit For

                Dim text = _codec.Decode(produced)

                If ToolCallProtocol.TryExtractToolName(text, detectedName, detectedTail) Then Exit For

                ' 演示模式下只要看到信号灯就立刻收手，避免模型在头部继续自由发挥、
                ' 把多余的 token 写进上下文
                If Not String.IsNullOrEmpty(opt.ForcedToolName) AndAlso
                   ToolCallProtocol.HasToolCallSignal(text) Then
                    Exit For
                End If
            Next

            record.AssistantText = _codec.Decode(produced)

            Return produced
        End Function

        ''' <summary>把工具结果（连同"轮到助手发言"的信号）一次性灌回上下文。</summary>
        Private Shared Sub FeedBack(stream As TokenStream, record As AgentRound, codec As ITextCodec,
                                    opt As AgentLoopOptions)

            record.ReusedPrefixTokens = stream.Context.Count

            Dim text = ToolCallProtocol.FormatResult(record.ToolResult) & ToolCallProtocol.AssistantMarker

            Call stream.Fill(codec.Encode(text))

            record.ContextTokensAfter = stream.Context.Count
        End Sub

        ''' <summary>把约束解码的逐 token 轨迹抄进本轮记录（便于在控制台展示"约束如何生效"）。</summary>
        Private Shared Sub CollectTrace(record As AgentRound, decoder As ConstrainedDecoder, opt As AgentLoopOptions)
            Dim limit = System.Math.Max(0, opt.TraceSteps)

            For i As Integer = 0 To System.Math.Min(limit, decoder.Trace.Count) - 1
                Call record.ConstrainedTrace.Add(decoder.Trace(i))
            Next

            If decoder.Trace.Count > limit Then
                Call record.ConstrainedTrace.Add($"... （共 {decoder.Trace.Count} 步，此处省略 {decoder.Trace.Count - limit} 步）")
            End If
        End Sub

        ''' <summary>返回 <paramref name="target"/> 中"未被 <paramref name="existing"/> 覆盖"的后缀。</summary>
        Private Shared Function SuffixAfterSharedPrefix(existing As String, target As String) As String
            If String.IsNullOrEmpty(existing) Then Return target

            Dim max = System.Math.Min(existing.Length, target.Length)
            Dim common As Integer = 0

            While common < max AndAlso existing(common) = target(common)
                common += 1
            End While

            ' 只补写"模型自己尚未写出"的那一段
            Return target.Substring(common)
        End Function

#End Region

    End Class

End Namespace
