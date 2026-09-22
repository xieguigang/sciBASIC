' ---------------------------------------------------------------------------
' ToolCallProtocol —— DeepSeek 风格的"工具调用" token 协议
'
' 核心事实（readme 里那句话的落地）：<b>LLM 从来不会真正调用函数</b>。
' 所谓 function calling，本质上是三件事的工程化协作：
'
'   1. 结构化提示工程 —— 工具定义经 chat template 序列化进上下文；
'   2. 生成特定格式的 token 序列 —— 模型吐出下面这套特殊 token 包裹的片段；
'   3. 外部代码执行解析 —— 框架认出这套 token，暂停生成、解析参数、执行真实函数。
'
' 这些"特殊 token"不是魔法，而是<b>被加进词表的保留 token</b>：模型经过训练学会在
' 需要调用工具时输出它们，推理引擎则用它们做状态切换的信号灯。本文件里的常量就是
' DeepSeek 分词器中真实存在的保留 token（可在 tokenizer.json 的 added_tokens 中查到）。
'
' 协议文本形态（与 tokenizer_config.json 里的 chat_template 逐字一致）：
'
'     <｜Assistant｜><｜tool▁calls▁begin｜>
'       <｜tool▁call▁begin｜>function<｜tool▁sep｜>{name}
'       ```json
'       {arguments}
'       ```
'       <｜tool▁call▁end｜>
'     <｜tool▁calls▁end｜><｜end▁of▁sentence｜>
'
' 工具结果的回填形态：
'
'     <｜tool▁outputs▁begin｜><｜tool▁output▁begin｜>{content}<｜tool▁output▁end｜><｜tool▁outputs▁end｜>
'
' 同一轮里可以有多个调用，它们之间用换行分隔（"并行工具调用"）。
' ---------------------------------------------------------------------------

Imports System.Text

Namespace Agent.ToolCalls

    ''' <summary>工具调用协议的标记常量与编解码工具。</summary>
    Public Module ToolCallProtocol

#Region "保留 token 的字面形式"

        ''' <summary>用户角色标记。</summary>
        Public Const UserMarker As String = "<｜User｜>"

        ''' <summary>助手角色标记。生成时它就是"该轮到模型说话了"的信号。</summary>
        Public Const AssistantMarker As String = "<｜Assistant｜>"

        ''' <summary>一轮工具调用的开始。</summary>
        Public Const CallsBeginMarker As String = "<｜tool▁calls▁begin｜>"

        ''' <summary>一轮工具调用的结束。</summary>
        Public Const CallsEndMarker As String = "<｜tool▁calls▁end｜>"

        ''' <summary>单个工具调用的开始。</summary>
        Public Const CallBeginMarker As String = "<｜tool▁call▁begin｜>"

        ''' <summary>单个工具调用的结束。</summary>
        Public Const CallEndMarker As String = "<｜tool▁call▁end｜>"

        ''' <summary>调用类型与工具名之间的分隔符。</summary>
        Public Const SepMarker As String = "<｜tool▁sep｜>"

        ''' <summary>工具结果集合的开始。</summary>
        Public Const OutputsBeginMarker As String = "<｜tool▁outputs▁begin｜>"

        ''' <summary>工具结果集合的结束。</summary>
        Public Const OutputsEndMarker As String = "<｜tool▁outputs▁end｜>"

        ''' <summary>单条工具结果的开始。</summary>
        Public Const OutputBeginMarker As String = "<｜tool▁output▁begin｜>"

        ''' <summary>单条工具结果的结束。</summary>
        Public Const OutputEndMarker As String = "<｜tool▁output▁end｜>"

        ''' <summary>句首标记（BOS）。</summary>
        Public Const BeginOfSentenceMarker As String = "<｜begin▁of▁sentence｜>"

        ''' <summary>句尾标记（EOS），同时也是填充标记。</summary>
        Public Const EndOfSentenceMarker As String = "<｜end▁of▁sentence｜>"

        ''' <summary>JSON 代码围栏的开始（含换行）。</summary>
        Public Const JsonFenceOpen As String = "```json" & vbLf

        ''' <summary>JSON 代码围栏的结束（含换行）。</summary>
        Public Const JsonFenceClose As String = vbLf & "```"

        ''' <summary>调用类型的固定取值。</summary>
        Public Const CallTypeFunction As String = "function"

        ''' <summary>协议中出现的全部标记，便于提示信息与校验。</summary>
        Public ReadOnly Property AllMarkers As String()
            Get
                Return New String() {
                    UserMarker, AssistantMarker,
                    CallsBeginMarker, CallsEndMarker,
                    CallBeginMarker, CallEndMarker, SepMarker,
                    OutputsBeginMarker, OutputsEndMarker,
                    OutputBeginMarker, OutputEndMarker,
                    EndOfSentenceMarker
                }
            End Get
        End Property

#End Region

#Region "格式化"

        ''' <summary>把"已经决定要调用某个工具"这件事写成调用片段的头部。</summary>
        ''' <param name="name">工具名</param>
        ''' <remarks>
        ''' 产出 <c>function&lt;｜tool▁sep｜&gt;{name}\n```json\n</c>。它不含
        ''' <see cref="CallsBeginMarker"/>：那个标记应当由模型自己生成，作为"我决定调用工具"
        ''' 的信号灯。
        ''' </remarks>
        Public Function FormatCallHeader(name As String) As String
            Return CallTypeFunction & SepMarker & name & vbLf & JsonFenceOpen
        End Function

        ''' <summary>调用片段的尾部：关闭代码围栏并收束本轮调用。</summary>
        Public Function FormatCallTail(Optional terminate As Boolean = True) As String
            Dim tail = JsonFenceClose & CallEndMarker & CallsEndMarker

            Return If(terminate, tail & EndOfSentenceMarker, tail)
        End Function

        ''' <summary>把一段工具结果包装成回填到上下文里的形态。</summary>
        Public Function FormatResult(content As String) As String
            Return OutputsBeginMarker & OutputBeginMarker & content & OutputEndMarker & OutputsEndMarker
        End Function

        ''' <summary>把若干条工具结果包装成一轮结果（并行调用的场景）。</summary>
        Public Function FormatResults(contents As IEnumerable(Of String)) As String
            Dim text As New StringBuilder()
            Dim first As Boolean = True

            Call text.Append(OutputsBeginMarker)

            For Each content In contents
                Call text.Append(OutputBeginMarker)
                Call text.Append(content)
                Call text.Append(OutputEndMarker)
                first = False
            Next

            If first Then
                ' 没有任何结果时也给出一个合法的空结果块，避免上下文里出现悬空的开始标记
                Call text.Append(OutputBeginMarker)
                Call text.Append(OutputEndMarker)
            End If

            Call text.Append(OutputsEndMarker)

            Return text.ToString()
        End Function

#End Region

#Region "检测与解析"

        ''' <summary>文本里是否出现了"开始调用工具"的信号灯。</summary>
        Public Function HasToolCallSignal(text As String) As Boolean
            If String.IsNullOrEmpty(text) Then Return False
            Return text.Contains(CallsBeginMarker) OrElse text.Contains(CallBeginMarker)
        End Function

        ''' <summary>文本里是否已经出现了"本轮调用结束"的标记。</summary>
        Public Function HasCallTerminator(text As String) As Boolean
            If String.IsNullOrEmpty(text) Then Return False
            Return text.Contains(CallsEndMarker)
        End Function

        ''' <summary>
        ''' 从文本中抽出"模型决定调用哪个工具"。
        ''' </summary>
        ''' <param name="text">模型生成的文本</param>
        ''' <param name="name">解析出的工具名</param>
        ''' <param name="afterMarker">
        ''' 调用开始标记之后的全部文本；调用方据此判断"模型自己写了多少"，只补写剩余部分。
        ''' </param>
        ''' <returns>是否成功解析出工具名</returns>
        ''' <remarks>
        ''' 工具名<b>不在</b>约束解码的范围内 —— "调用哪个工具"是模型该有的决策自由度，
        ''' 框架只负责在拿到名字之后校验它是否存在（对应"幻觉工具名"这一失败模式）。
        ''' </remarks>
        Public Function TryExtractToolName(text As String, ByRef name As String,
                                           ByRef afterMarker As String) As Boolean

            name = Nothing
            afterMarker = Nothing

            If String.IsNullOrEmpty(text) Then Return False

            Dim idx = text.IndexOf(CallsBeginMarker, StringComparison.Ordinal)

            If idx >= 0 Then
                Dim tail = text.Substring(idx + CallsBeginMarker.Length)
                Dim callBegin = tail.IndexOf(CallBeginMarker, StringComparison.Ordinal)
                afterMarker = If(callBegin >= 0, tail.Substring(callBegin + CallBeginMarker.Length), tail)
            Else
                idx = text.IndexOf(CallBeginMarker, StringComparison.Ordinal)
                If idx < 0 Then Return False

                afterMarker = text.Substring(idx + CallBeginMarker.Length)
            End If

            Dim sepIdx = afterMarker.IndexOf(SepMarker, StringComparison.Ordinal)

            If sepIdx < 0 Then Return False

            Dim afterSep = afterMarker.Substring(sepIdx + SepMarker.Length)
            Dim nl = afterSep.IndexOf(vbLf)
            Dim raw = If(nl >= 0, afterSep.Substring(0, nl), afterSep).Trim()

            If raw.Length = 0 Then Return False

            name = raw

            Return True
        End Function

        ''' <summary>
        ''' 解析文本中的全部工具调用。
        ''' </summary>
        ''' <param name="text">模型生成的文本（或上下文片段）</param>
        ''' <param name="calls">解析结果</param>
        ''' <param name="failure">失败原因（<c>error</c> 是 VB 保留字，因此换个名字）</param>
        ''' <returns>是否至少解析出一个调用</returns>
        ''' <remarks>
        ''' 解析是"宽容"的：即使末尾的结束标记还没生成出来（流式场景下参数是逐 token
        ''' 碎片到达的），只要工具名与 JSON 代码块已经完整，也能解析成功。
        ''' </remarks>
        Public Function TryParseCalls(text As String, ByRef calls As List(Of ToolCall),
                                      ByRef failure As String) As Boolean

            calls = New List(Of ToolCall)()
            failure = Nothing

            If String.IsNullOrEmpty(text) Then
                failure = "空文本"
                Return False
            End If

            Dim cursor = 0

            While True
                Dim beginIdx = text.IndexOf(CallBeginMarker, cursor, StringComparison.Ordinal)

                If beginIdx < 0 Then Exit While

                Dim afterBegin = beginIdx + CallBeginMarker.Length

                ' 1. 逐字读取到分隔符，得到调用类型（固定为 function）
                Dim sepIdx = text.IndexOf(SepMarker, afterBegin, StringComparison.Ordinal)

                If sepIdx < 0 Then
                    failure = $"第 {calls.Count} 个调用的类型分隔符 {SepMarker} 缺失"
                    Exit While
                End If

                Dim callType = text.Substring(afterBegin, sepIdx - afterBegin).Trim()

                If Not String.Equals(callType, CallTypeFunction, StringComparison.OrdinalIgnoreCase) Then
                    failure = $"不支持的调用类型 '{callType}'"
                    Exit While
                End If

                ' 2. 工具名：从分隔符之后到换行之前
                Dim afterSep = sepIdx + SepMarker.Length
                Dim nlIdx = text.IndexOf(vbLf, afterSep)

                If nlIdx < 0 Then
                    failure = $"第 {calls.Count} 个调用缺少换行，工具名不完整"
                    Exit While
                End If

                Dim name = text.Substring(afterSep, nlIdx - afterSep).Trim()

                If String.IsNullOrEmpty(name) Then
                    failure = $"第 {calls.Count} 个调用的工具名为空"
                    Exit While
                End If

                ' 3. 跳过可能的 ```json 围栏
                Dim jsonStart = nlIdx + 1

                If jsonStart + JsonFenceOpen.Length <= text.Length AndAlso
                   text.Substring(jsonStart, JsonFenceOpen.Length) = JsonFenceOpen Then
                    jsonStart += JsonFenceOpen.Length
                End If

                ' 4. 参数 JSON：到 ``` 或调用结束标记为止
                Dim jsonEnd = text.IndexOf(JsonFenceClose, jsonStart, StringComparison.Ordinal)
                Dim endIdx = text.IndexOf(CallEndMarker, jsonStart, StringComparison.Ordinal)

                If jsonEnd < 0 Then jsonEnd = If(endIdx >= 0, endIdx, text.Length)
                If endIdx < 0 Then endIdx = text.Length

                Dim jsonText = text.Substring(jsonStart, System.Math.Max(0, jsonEnd - jsonStart)).Trim()

                Dim toolCall As New ToolCall With {
                    .Index = calls.Count,
                    .Name = name,
                    .ArgumentsJson = jsonText
                }

                Call ParseArguments(jsonText, toolCall)

                calls.Add(toolCall)

                cursor = If(endIdx < text.Length, endIdx + CallEndMarker.Length, text.Length)
            End While

            If calls.Count = 0 AndAlso failure Is Nothing Then
                failure = "未找到任何工具调用片段"
            End If

            Return calls.Count > 0
        End Function

        ''' <summary>
        ''' 解析参数 JSON 文本为键值对。
        ''' </summary>
        ''' <remarks>
        ''' 刻意不用第三方 JSON 解析器：这里的 JSON 是<b>受约束生成</b>出来的、结构已知的
        ''' 扁平对象，一个几十行的手写扫描器就够，而且能把"解析失败"直接当作
        ''' 约束解码失效的信号暴露出来。
        ''' </remarks>
        Public Function ParseArgumentObject(jsonText As String) As Dictionary(Of String, String)
            Dim toolCall As New ToolCall With {.ArgumentsJson = jsonText}

            Call ParseArguments(jsonText, toolCall)

            Return toolCall.Arguments
        End Function

        Private Sub ParseArguments(jsonText As String, target As ToolCall)
            If String.IsNullOrEmpty(jsonText) Then Return

            Dim i = 0
            Dim n = jsonText.Length

            ' 跳过开头的 '{'
            While i < n AndAlso Char.IsWhiteSpace(jsonText(i))
                i += 1
            End While

            If i < n AndAlso jsonText(i) = "{"c Then i += 1

            While i < n
                While i < n AndAlso (Char.IsWhiteSpace(jsonText(i)) OrElse jsonText(i) = ","c)
                    i += 1
                End While

                If i >= n OrElse jsonText(i) = "}"c Then Exit While
                If jsonText(i) <> """"c Then Exit While

                i += 1

                Dim key As New StringBuilder()

                While i < n AndAlso jsonText(i) <> """"c
                    If jsonText(i) = "\"c AndAlso i + 1 < n Then
                        Call key.Append(jsonText(i + 1))
                        i += 2
                    Else
                        Call key.Append(jsonText(i))
                        i += 1
                    End If
                End While

                If i < n Then i += 1

                While i < n AndAlso Char.IsWhiteSpace(jsonText(i))
                    i += 1
                End While

                If i >= n OrElse jsonText(i) <> ":"c Then Exit While

                i += 1

                While i < n AndAlso Char.IsWhiteSpace(jsonText(i))
                    i += 1
                End While

                Dim value As New StringBuilder()

                If i < n AndAlso jsonText(i) = """"c Then
                    ' 字符串值：还原转义
                    i += 1

                    While i < n AndAlso jsonText(i) <> """"c
                        If jsonText(i) = "\"c AndAlso i + 1 < n Then
                            Call value.Append(Unescape(jsonText(i + 1)))
                            i += 2
                        Else
                            Call value.Append(jsonText(i))
                            i += 1
                        End If
                    End While

                    If i < n Then i += 1
                Else
                    ' 数字 / 布尔 / null / 数组 / 嵌套对象：按"到逗号或右花括号为止"截取
                    Dim depth As Integer = 0

                    While i < n
                        Dim c = jsonText(i)

                        If c = "{"c OrElse c = "["c Then
                            depth += 1
                        ElseIf c = "}"c OrElse c = "]"c Then
                            If depth = 0 Then Exit While
                            depth -= 1
                        ElseIf c = ","c AndAlso depth = 0 Then
                            Exit While
                        End If

                        Call value.Append(c)
                        i += 1
                    End While
                End If

                Dim keyText = key.ToString().Trim()

                If keyText.Length > 0 Then
                    target.Arguments(keyText) = value.ToString().Trim()
                End If
            End While
        End Sub

        Private Function Unescape(c As Char) As String
            Select Case c
                Case "n"c : Return vbLf
                Case "r"c : Return vbCr
                Case "t"c : Return vbTab
                Case Else : Return c.ToString()
            End Select
        End Function

#End Region

    End Module

End Namespace
