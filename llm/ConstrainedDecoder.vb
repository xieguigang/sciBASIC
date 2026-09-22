' ---------------------------------------------------------------------------
' ConstrainedDecoder —— 约束解码（Constrained / Grammar-based Decoding）
'
' 要解决的问题：模型是概率模型，它的输出只在"训练分布的意义上"合法。
' 在 function calling 里这意味着它可能编造 schema 里不存在的参数、把整数写成字符串、
' 或者把枚举值写成 <c>"very hot"</c>。仅仅靠 prompt 里的说明文本是拦不住这些的。
'
' 约束解码的思路（readme 里描述的 FSM 做法）：
'
'   1. 把参数的 JSON Schema 编译成一个<b>有限状态机</b>；
'   2. 自回归的每一步，状态机根据已生成的前缀算出"当前合法的下一个字符集合"；
'   3. 把这些字符反过来映射成"合法的 token 集合"，对 logits 做掩码
'      —— 非法 token 的概率被置为 -∞，再采样。
'
' 于是 schema 声明 units ∈ {celsius, fahrenheit} 之后，解码器就<b>物理上无法</b>
' 生成 "very hot"：那几个 token 的概率恒为 0，采样永远取不到。字符串字段该闭合引号时，
' 其他 token 同样不可达。
'
' 一个重要边界：约束解码只能保证<b>语法</b>合法，不保证<b>语义</b>正确。
' 例如 city 字段是自由字符串，"火星" 在语法上完全合法 —— 参数值是否真实合理
' 仍然是模型能力问题，框架只能靠工具名/取值校验去兜底。
'
' 本实现的状态机覆盖一个 JSON <b>对象</b>（工具的参数）。对象之外的部分
' （<c>function&lt;｜tool▁sep｜&gt;{name}</c> 头部与代码围栏）由 Agent 循环负责，
' 因为"调用哪个工具"是模型该有的决策自由度，不属于约束的范围。
'
' 键的约束方式：必须按 schema 中声明的顺序出现，且必需键一个都不能少。
' 这是 OpenAI "strict mode" 一类的做法 —— 把分支搜索变成确定的状态推进，
' 状态机足够简单，也使得"生成的 JSON 一定符合 schema"成为可证明的性质。
' ---------------------------------------------------------------------------

Imports System.Text

Namespace LLM

    ''' <summary>
    ''' 按 JSON Schema 约束 logits 的解码器：保证生成的参数对象在语法上<b>必然</b>合法。
    ''' </summary>
    Public Class ConstrainedDecoder

        ''' <summary>状态机的内部状态。</summary>
        Private Enum JsonState
            ''' <summary>期望对象的左花括号</summary>
            ExpectObjectStart
            ''' <summary>期望下一个键的引号，或（在允许时）对象的右花括号</summary>
            ExpectKeyOrEnd
            ''' <summary>正在写键名</summary>
            InKey
            ''' <summary>期望冒号</summary>
            ExpectColon
            ''' <summary>期望值的起始字符</summary>
            ExpectValue
            ''' <summary>正在写枚举字面量（含首尾引号）</summary>
            InEnumValue
            ''' <summary>正在写数字</summary>
            InNumber
            ''' <summary>正在写 true / false</summary>
            InBoolean
            ''' <summary>正在写字符串值（引号内部）</summary>
            InStringValue
            ''' <summary>值已完整，期望逗号或右花括号</summary>
            AfterValue
            ''' <summary>对象已闭合，约束结束</summary>
            Done
        End Enum

        ''' <summary>状态机的可回滚快照 —— token 合法性是"试探性推进"，失败必须能还原。</summary>
        Private Structure Snapshot
            Public State As JsonState
            Public KeyIndex As Integer
            Public Buffer As String
            Public Escaped As Boolean
        End Structure

        Private ReadOnly _schema As JsonSchema
        Private ReadOnly _vocabulary As TokenizerVocabulary

        Private _state As JsonState
        Private _keyIndex As Integer
        Private _buffer As String
        Private _escaped As Boolean
        Private _text As New StringBuilder()

        Private _lastLegalTokenCount As Integer
        Private _lastMaskedTokenCount As Integer
        Private _lastLegalCharacters As Char()

        ''' <summary>最近一次 <see cref="ApplyMask"/> 放行的 token 个数。</summary>
        Public Property LastLegalTokenCount As Integer
            Get
                Return _lastLegalTokenCount
            End Get
            Private Set(value As Integer)
                _lastLegalTokenCount = value
            End Set
        End Property

        ''' <summary>最近一次 <see cref="ApplyMask"/> 时状态机允许的首字符集合（诊断用）。</summary>
        Public Property LastLegalCharacters As Char()
            Get
                Return _lastLegalCharacters
            End Get
            Private Set(value As Char())
                _lastLegalCharacters = value
            End Set
        End Property

        ''' <summary>被掩码掉的 token 个数。</summary>
        Public Property LastMaskedTokenCount As Integer
            Get
                Return _lastMaskedTokenCount
            End Get
            Private Set(value As Integer)
                _lastMaskedTokenCount = value
            End Set
        End Property

        ''' <summary>约束所依据的 schema。</summary>
        Public ReadOnly Property Schema As JsonSchema
            Get
                Return _schema
            End Get
        End Property

        ''' <summary>
        ''' 逐 token 的约束轨迹，形如
        ''' <c>#3 token=1234 '&quot;city&quot;' state=in key name legal=812 masked=99999</c>。
        ''' </summary>
        ''' <remarks>
        ''' 它存在的意义是"让约束变得可见"：读这条轨迹可以直接看到每一步有多少 token
        ''' 因为不合 schema 而被物理屏蔽掉了。
        ''' </remarks>
        Public ReadOnly Property Trace As New List(Of String)

        ''' <summary>轨迹最多记录多少条（超出后只保留统计，避免刷屏）。</summary>
        Public Property TraceLimit As Integer = 64

        ''' <summary>已经生成（并被接受）的 JSON 文本。</summary>
        Public ReadOnly Property GeneratedText As String
            Get
                Return _text.ToString()
            End Get
        End Property

        ''' <summary>对象是否已经闭合（约束达成，可以停止生成）。</summary>
        Public ReadOnly Property IsComplete As Boolean
            Get
                Return _state = JsonState.Done
            End Get
        End Property

        ''' <summary>当前状态的可读名称（用于逐 token 打印约束过程）。</summary>
        Public ReadOnly Property StateName As String
            Get
                Select Case _state
                    Case JsonState.ExpectObjectStart : Return "expect '{'"
                    Case JsonState.ExpectKeyOrEnd : Return "expect key or '}'"
                    Case JsonState.InKey : Return "in key name"
                    Case JsonState.ExpectColon : Return "expect ':'"
                    Case JsonState.ExpectValue : Return "expect value"
                    Case JsonState.InEnumValue : Return "in enum literal"
                    Case JsonState.InNumber : Return "in number"
                    Case JsonState.InBoolean : Return "in boolean"
                    Case JsonState.InStringValue : Return "in string"
                    Case JsonState.AfterValue : Return "expect ',' or '}'"
                    Case JsonState.Done : Return "done"
                    Case Else : Return _state.ToString()
                End Select
            End Get
        End Property

        ''' <summary>
        ''' 当前状态下还需要补齐的键名（用于诊断"约束把模型逼到了哪一步"）。
        ''' </summary>
        Public ReadOnly Property PendingKey As String
            Get
                If _state = JsonState.InKey OrElse _state = JsonState.ExpectKeyOrEnd OrElse _state = JsonState.ExpectColon Then
                    If _keyIndex >= 0 AndAlso _keyIndex < _schema.Properties.Count Then
                        Return _schema.Properties(_keyIndex).Name
                    End If
                End If

                Return Nothing
            End Get
        End Property

        ''' <summary>
        ''' Creates a constrained decoder.
        ''' </summary>
        ''' <param name="schema">The JSON schema that the generated arguments must satisfy.</param>
        ''' <param name="vocabulary">The tokenizer vocabulary used to mask out invalid tokens.</param>
        Public Sub New(schema As JsonSchema, vocabulary As TokenizerVocabulary)
            If schema Is Nothing Then Throw New ArgumentNullException(NameOf(schema))
            If vocabulary Is Nothing Then Throw New ArgumentNullException(NameOf(vocabulary))

            _schema = schema
            _vocabulary = vocabulary

            Call Reset()
        End Sub

        ''' <summary>把状态机复位到"期望对象起始"。</summary>
        Public Sub Reset()
            _state = JsonState.ExpectObjectStart
            _keyIndex = 0
            _buffer = String.Empty
            _escaped = False
            Call _text.Clear()
            Call Trace.Clear()

            LastLegalTokenCount = 0
            LastMaskedTokenCount = 0
            LastLegalCharacters = New Char() {}
        End Sub

#Region "状态机"

        ''' <summary>取快照（试探性推进前的现场保护）。</summary>
        Private Function Capture() As Snapshot
            Return New Snapshot With {
                .State = _state,
                .KeyIndex = _keyIndex,
                .Buffer = _buffer,
                .Escaped = _escaped
            }
        End Function

        Private Sub Restore(s As Snapshot)
            _state = s.State
            _keyIndex = s.KeyIndex
            _buffer = s.Buffer
            _escaped = s.Escaped
        End Sub

        ''' <summary>原地推进状态机（不负责回滚）。</summary>
        Private Function ConsumeInPlace(c As Char) As Boolean
            Select Case _state

                Case JsonState.ExpectObjectStart
                    If c <> "{"c Then Return False
                    _state = JsonState.ExpectKeyOrEnd
                    Return True

                Case JsonState.ExpectKeyOrEnd
                    If _keyIndex >= _schema.Properties.Count Then
                        ' 全部键都写完了，只允许闭合
                        If c = "}"c Then
                            _state = JsonState.Done
                            Return True
                        End If

                        Return False
                    End If

                    If c = """"c Then
                        _buffer = String.Empty
                        _state = JsonState.InKey
                        Return True
                    End If

                    If c = "}"c AndAlso Not HasRequiredFrom(_keyIndex) Then
                        _state = JsonState.Done
                        Return True
                    End If

                    Return False

                Case JsonState.InKey
                    Dim expected = _schema.Properties(_keyIndex).Name

                    If _buffer.Length < expected.Length AndAlso c = expected(_buffer.Length) Then
                        _buffer &= c
                        Return True
                    End If

                    If _buffer.Length = expected.Length AndAlso c = """"c Then
                        _state = JsonState.ExpectColon
                        Return True
                    End If

                    Return False

                Case JsonState.ExpectColon
                    If c <> ":"c Then Return False
                    _buffer = String.Empty
                    _state = JsonState.ExpectValue
                    Return True

                Case JsonState.ExpectValue
                    Return ConsumeValueStart(c)

                Case JsonState.InEnumValue
                    Dim extended = _buffer & c
                    Dim literals = _schema.Properties(_keyIndex).EnumLiterals()
                    Dim matched As Boolean = False

                    For Each lit In literals
                        If lit.StartsWith(extended, StringComparison.Ordinal) Then
                            matched = True
                            Exit For
                        End If
                    Next

                    If Not matched Then Return False

                    _buffer = extended

                    For Each lit In literals
                        If lit = _buffer Then
                            _state = JsonState.AfterValue
                            Exit For
                        End If
                    Next

                    Return True

                Case JsonState.InNumber
                    Return ConsumeNumber(c)

                Case JsonState.InBoolean
                    Return ConsumeBoolean(c)

                Case JsonState.InStringValue
                    If _escaped Then
                        ' 只允许 JSON 定义的合法转义字符：\" \\ \/ \b \f \n \r \t \uXXXX
                        Dim escapes As String = "\" & "/bfnrtu" & Chr(34)

                        If escapes.IndexOf(c) < 0 Then Return False
                        _escaped = False
                        Return True
                    End If

                    If c = "\"c Then
                        _escaped = True
                        Return True
                    End If

                    If c = """"c Then
                        _state = JsonState.AfterValue
                        Return True
                    End If

                    ' 未转义的控制字符必须被禁止，否则生成的就不是合法 JSON
                    If Char.IsControl(c) Then Return False

                    Return True

                Case JsonState.AfterValue
                    ' 逗号只有在"后面确实还有键"时才合法 —— 否则会生成尾随逗号这种非法 JSON
                    If c = ","c AndAlso HasNextProperty() Then
                        _keyIndex += 1
                        _state = JsonState.ExpectKeyOrEnd
                        _buffer = String.Empty
                        Return True
                    End If

                    If c = "}"c AndAlso Not HasRequiredFrom(_keyIndex + 1) Then
                        _state = JsonState.Done
                        Return True
                    End If

                    Return False

                Case JsonState.Done
                    Return False

            End Select

            Return False
        End Function

        Private Function ConsumeValueStart(c As Char) As Boolean
            Dim prop = _schema.Properties(_keyIndex)

            ' 枚举优先：所有枚举字面量都带引号，因此首字符只能是引号
            If prop.EnumValues IsNot Nothing AndAlso prop.EnumValues.Length > 0 Then
                If c <> """"c Then Return False
                _buffer = c.ToString()
                _state = JsonState.InEnumValue
                Return True
            End If

            Select Case prop.Type

                Case "boolean"
                    If c = "t"c OrElse c = "f"c Then
                        _buffer = c.ToString()
                        _state = JsonState.InBoolean
                        Return True
                    End If

                    Return False

                Case "integer", "number"
                    If c = "-"c OrElse Char.IsDigit(c) Then
                        _buffer = c.ToString()
                        _state = JsonState.InNumber
                        Return True
                    End If

                    Return False

                Case Else
                    If c <> """"c Then Return False
                    _escaped = False
                    _state = JsonState.InStringValue
                    Return True

            End Select
        End Function

        Private Function ConsumeNumber(c As Char) As Boolean
            Dim type = _schema.Properties(_keyIndex).Type

            If c = ","c Then
                If Not HasNextProperty() Then Return False
                If Not IsCompleteNumber(_buffer, type) Then Return False

                _buffer = String.Empty
                _keyIndex += 1
                _state = JsonState.ExpectKeyOrEnd
                Return True
            End If

            If c = "}"c Then
                If Not IsCompleteNumber(_buffer, type) Then Return False

                _buffer = String.Empty
                _state = JsonState.Done
                Return True
            End If

            Dim extended = _buffer & c

            If IsNumberPrefix(extended, type) Then
                _buffer = extended
                Return True
            End If

            Return False
        End Function

        Private Function ConsumeBoolean(c As Char) As Boolean
            If c = ","c OrElse c = "}"c Then
                If _buffer <> "true" AndAlso _buffer <> "false" Then Return False
                If c = ","c AndAlso Not HasNextProperty() Then Return False

                _buffer = String.Empty

                If c = "}"c Then
                    _state = JsonState.Done
                Else
                    _keyIndex += 1
                    _state = JsonState.ExpectKeyOrEnd
                End If

                Return True
            End If

            Dim extended = _buffer & c

            If "true".StartsWith(extended, StringComparison.Ordinal) OrElse
               "false".StartsWith(extended, StringComparison.Ordinal) Then
                _buffer = extended
                Return True
            End If

            Return False
        End Function

        ''' <summary>当前键之后是否还有下一个键（决定逗号是否合法）。</summary>
        Private Function HasNextProperty() As Boolean
            Return _keyIndex + 1 < _schema.Properties.Count
        End Function

        ''' <summary>[start, Count) 区间内是否还有必填键。</summary>
        Private Function HasRequiredFrom(start As Integer) As Boolean
            For i As Integer = System.Math.Max(0, start) To _schema.Properties.Count - 1
                If _schema.Properties(i).Required Then Return True
            Next

            Return False
        End Function

#End Region

#Region "数字字面量的前缀 / 完整判定"

        ''' <summary>判断 <paramref name="s"/> 是否可能是某个合法数字的前缀。</summary>
        Private Shared Function IsNumberPrefix(s As String, type As String) As Boolean
            Dim i As Integer = 0
            Dim n = s.Length

            If n = 0 Then Return False

            If s(0) = "-"c Then i = 1

            Dim intDigits As Integer = 0

            While i < n AndAlso Char.IsDigit(s(i))
                intDigits += 1
                i += 1
            End While

            If i < n AndAlso s(i) = "."c Then
                If type = "integer" Then Return False
                i += 1

                While i < n AndAlso Char.IsDigit(s(i))
                    i += 1
                End While
            End If

            If i < n AndAlso (s(i) = "e"c OrElse s(i) = "E"c) Then
                If type = "integer" Then Return False
                i += 1

                If i < n AndAlso (s(i) = "+"c OrElse s(i) = "-"c) Then i += 1

                While i < n AndAlso Char.IsDigit(s(i))
                    i += 1
                End While
            End If

            Return i = n
        End Function

        ''' <summary>判断 <paramref name="s"/> 是否已经是一个完整的合法数字。</summary>
        Private Shared Function IsCompleteNumber(s As String, type As String) As Boolean
            If String.IsNullOrEmpty(s) Then Return False

            Dim i As Integer = 0
            Dim n = s.Length

            If s(0) = "-"c Then i = 1

            Dim intDigits As Integer = 0

            While i < n AndAlso Char.IsDigit(s(i))
                intDigits += 1
                i += 1
            End While

            If intDigits = 0 Then Return False

            If i < n AndAlso s(i) = "."c Then
                If type = "integer" Then Return False
                i += 1

                Dim fracDigits As Integer = 0

                While i < n AndAlso Char.IsDigit(s(i))
                    fracDigits += 1
                    i += 1
                End While

                If fracDigits = 0 Then Return False
            End If

            If i < n AndAlso (s(i) = "e"c OrElse s(i) = "E"c) Then
                If type = "integer" Then Return False
                i += 1

                If i < n AndAlso (s(i) = "+"c OrElse s(i) = "-"c) Then i += 1

                Dim expDigits As Integer = 0

                While i < n AndAlso Char.IsDigit(s(i))
                    expDigits += 1
                    i += 1
                End While

                If expDigits = 0 Then Return False
            End If

            Return i = n
        End Function

#End Region

#Region "token 合法性与 logits 掩码"

        ''' <summary>
        ''' 判断一个 token 贡献的文本能否被当前状态机完整消费。
        ''' </summary>
        ''' <remarks>
        ''' 自由字符串状态（<see cref="JsonState.InStringValue"/> 且不在转义中）允许任意
        ''' 非引号/非反斜杠字符，因此这里给它一条 O(len) 的快路径 —— 否则 10 万词表里
        ''' 的绝大多数 token 都要走完整的状态机遍历，单步代价会高出一个数量级。
        ''' </remarks>
        Private Function IsTokenLegal(text As String) As Boolean
            If String.IsNullOrEmpty(text) Then Return False
            If _state = JsonState.Done Then Return False

            If _state = JsonState.InStringValue AndAlso Not _escaped Then
                For i As Integer = 0 To text.Length - 1
                    Dim c = text(i)

                    If c = """"c OrElse c = "\"c OrElse Char.IsControl(c) Then Return False
                Next

                Return True
            End If

            Dim snapshot = Capture()

            For i As Integer = 0 To text.Length - 1
                If Not ConsumeInPlace(text(i)) Then
                    Call Restore(snapshot)
                    Return False
                End If
            Next

            Call Restore(snapshot)

            Return True
        End Function

        ''' <summary>
        ''' 判断某个字符是否可能作为"下一个 token 的首字符"。
        ''' </summary>
        ''' <remarks>
        ''' 这是纯粹的预过滤：它只保证"以该字符开头的 token 中可能有合法的"，
        ''' 真正的合法性仍由 <see cref="IsTokenLegal"/> 逐字符验证。
        ''' 预过滤的价值在于把绝大多数首字符桶整体跳过。
        ''' </remarks>
        Private Function IsFirstCharacterCandidate(c As Char) As Boolean
            If _state = JsonState.Done Then Return False

            If _state = JsonState.InStringValue AndAlso Not _escaped Then
                Return c <> """"c AndAlso c <> "\"c AndAlso Not Char.IsControl(c)
            End If

            ' 这是"试探"，无论成功还是失败都必须把状态机还原 ——
            ' 只回滚失败分支会让成功的试探永久推进状态，表现为"还没生成任何字符，
            ' 状态机却已经跑到后面去了"。
            Dim snapshot = Capture()
            Dim accepted = ConsumeInPlace(c)

            Call Restore(snapshot)

            Return accepted
        End Function

        ''' <summary>
        ''' 就地对 <paramref name="logits"/> 施加掩码：非法的 token 位置被置为 -∞。
        ''' </summary>
        ''' <returns>放行的合法 token 个数</returns>
        Public Function ApplyMask(logits As Double()) As Integer
            If logits Is Nothing Then Throw New ArgumentNullException(NameOf(logits))

            Dim size = logits.Length
            Dim allowed(size - 1) As Boolean
            Dim legalCharacters As New List(Of Char)
            Dim legal As Integer = 0

            For Each c In _vocabulary.FirstCharacters
                If Not IsFirstCharacterCandidate(c) Then Continue For

                Call legalCharacters.Add(c)

                For Each id In _vocabulary.BucketOf(c)
                    If id >= size Then Continue For
                    If allowed(id) Then Continue For
                    If Not IsTokenLegal(_vocabulary.TextOf(id)) Then Continue For

                    allowed(id) = True
                    legal += 1
                Next
            Next

            Dim masked As Integer = 0

            For id As Integer = 0 To size - 1
                If allowed(id) Then Continue For

                ' 已经是 -∞ 的（例如约束外被外部屏蔽的）不计入统计
                If Not Double.IsNegativeInfinity(logits(id)) Then masked += 1

                logits(id) = Double.NegativeInfinity
            Next

            LastLegalTokenCount = legal
            LastMaskedTokenCount = masked
            LastLegalCharacters = legalCharacters.ToArray()

            Return legal
        End Function

        ''' <summary>
        ''' 构造一个可以直接挂到 <see cref="GenerationOptions.LogitsProcessor"/> 上的处理器。
        ''' </summary>
        Public Function CreateProcessor() As Func(Of Double(), Integer, Double())
            Return Function(logits, stepIndex)
                       Call ApplyMask(logits)
                       Return logits
                   End Function
        End Function

        ''' <summary>
        ''' 把一段<b>已经生成</b>的文本提交给状态机（例如被外部强行预置的片段）。
        ''' </summary>
        ''' <exception cref="InvalidOperationException">文本无法被状态机接受时抛出。</exception>
        Public Sub Commit(text As String)
            If String.IsNullOrEmpty(text) Then Return

            For i As Integer = 0 To text.Length - 1
                If Not ConsumeInPlace(text(i)) Then
                    Throw New InvalidOperationException(
                        $"文本 '{text}' 的第 {i} 个字符 '{text(i)}' 违反了约束（当前状态：{StateName}）")
                End If
            Next

            Call _text.Append(text)
        End Sub

        ''' <summary>
        ''' 在给定的 token 流上按约束生成一个 JSON 对象，直到对象闭合或达到长度上限。
        ''' </summary>
        ''' <param name="stream">已完成 prefill 的 token 流</param>
        ''' <param name="sampler">采样器</param>
        ''' <param name="maxTokens">最多生成的 token 数</param>
        ''' <returns>本次新生成的 token 序列（已追加到 <paramref name="stream"/> 的上下文）</returns>
        Public Function Generate(stream As TokenStream, sampler As Sampler, maxTokens As Integer) As List(Of Integer)
            If stream Is Nothing Then Throw New ArgumentNullException(NameOf(stream))
            If sampler Is Nothing Then Throw New ArgumentNullException(NameOf(sampler))

            Dim produced As New List(Of Integer)()

            For i As Integer = 1 To maxTokens
                If IsComplete Then Exit For

                Dim logits = stream.CurrentLogits

                If logits Is Nothing Then
                    Throw New InvalidOperationException("token 流尚未完成 prefill，没有可用的 logits")
                End If

                Dim legal = ApplyMask(logits)

                If legal = 0 Then
                    Throw New InvalidOperationException(
                        $"约束解码在第 {i} 步找不到任何合法 token（状态：{StateName}，已生成：{GeneratedText}）")
                End If

                Dim token = sampler.Sample(logits, stream.Context)

                produced.Add(token)
                Call stream.Accept(token)

                Dim text = _vocabulary.TextOf(token)

                If String.IsNullOrEmpty(text) Then
                    Throw New InvalidOperationException($"采样到了无法在约束下使用的 token {token}")
                End If

                ' 推进状态机。ApplyMask 只做"试探"并回滚，真正的推进发生在这里 ——
                ' 少了这一步，状态机永远停在初始状态，会无限重复生成同一个合法 token。
                For k As Integer = 0 To text.Length - 1
                    If Not ConsumeInPlace(text(k)) Then
                        Throw New InvalidOperationException(
                            $"内部不一致：被 ApplyMask 判定为合法的 token {token}（文本 '{text}'）" &
                            $"却在字符 '{text(k)}' 处无法推进状态机")
                    End If
                Next

                Call _text.Append(text)

                If Trace.Count < TraceLimit Then
                    Call Trace.Add($"#{i} token={token} +'{Escape(text)}' -> {StateName} " &
                                   $"(legal={LastLegalTokenCount}, masked={LastMaskedTokenCount})")
                End If
            Next

            Return produced
        End Function

        ''' <summary>把控制字符转义成可打印形式（轨迹输出用）。</summary>
        Private Shared Function Escape(text As String) As String
            Return text.Replace(vbLf, "\n").Replace(vbCr, "\r").Replace(vbTab, "\t")
        End Function

#End Region

    End Class

End Namespace
