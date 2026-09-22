#Region "Microsoft.VisualBasic::12273d7357e4c1a6a29e1ef80191c2a1, llm\Agent\AgentLoop.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 364
    '    Code Lines: 199 (54.67%)
    ' Comment Lines: 76 (20.88%)
    '    - Xml Docs: 47.37%
    ' 
    '   Blank Lines: 89 (24.45%)
    '     File Size: 16.20 KB


    '     Class AgentLoop
    ' 
    '         Properties: Model, Registry
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: BuildInitialPrompt, GenerateFree, Run, SuffixAfterSharedPrefix
    ' 
    '         Sub: CollectTrace, FeedBack
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
Imports Microsoft.VisualBasic.DeepLearning.LLM.Agent.ToolCalls
Imports Microsoft.VisualBasic.DeepLearning.LLM.Text
Imports Diagnostics = System.Diagnostics
Imports Microsoft.VisualBasic.DeepLearning.LLM.Sampler

Namespace Agent

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
        Public Function Run(systemPrompt As String, userMessage As String,
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
            Dim sampler As New LLMSampler(opt.Sampling)

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
        Private Function GenerateFree(stream As TokenStream, sampler As LLMSampler,
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

