' ---------------------------------------------------------------------------
' TextGenerator —— 自回归生成循环
'
' 生成的本质就是"前向 → 取最后一个位置的 logits → 采样 → 拼回输入 → 再来一遍"，
' 直到 EOS 或达到长度上限。本类负责把这个循环与 KV Cache、采样器、以及
' "外部 logits 处理器"三件事组合起来。
'
' 关于"外部 logits 处理器"（<see cref="GenerationOptions.LogitsProcessor"/>）：
'
' 它是 function calling 里<b>约束解码</b>的挂载点。约束解码并不会改动模型，
' 只是在每一步采样之前，把不符合 JSON Schema 语法状态机的 token 的 logits
' 置为 -∞ —— 于是"非法 token 的概率为 0"，模型物理上无法生成结构非法的参数。
' 因为有这个挂载点，本类不需要知道任何关于工具调用的知识。
'
' 关于 UseCache：
'
' 同一个 prompt 既可以用"prefill + 增量解码"（O(t) / 步）生成，也可以用
' "每步重算全部前缀"（O(t²) / 步）生成。两条路径在数学上完全一致，因此
' 把 UseCache 关掉就得到了一个天然的<b>正确性对照组</b>：如果两条路径逐 token
' 输出不同，那一定是 KV Cache 的实现有 bug。
' ---------------------------------------------------------------------------

Imports std = System.Math
Imports Diagnostics = System.Diagnostics

Namespace LLM

    ''' <summary>生成超参。</summary>
    Public Class GenerationOptions

        ''' <summary>最多新生成的 token 个数。</summary>
        Public Property MaxNewTokens As Integer = 32

        ''' <summary>是否启用 KV Cache（关闭即退化为每步重算全部前缀）。</summary>
        Public Property UseCache As Boolean = True

        ''' <summary>遇到这些 token 就停止生成（通常是 EOS / 工具调用结束标记）。</summary>
        Public Property StopTokenIds As Integer()

        ''' <summary>
        ''' 采样前的 logits 处理器：<c>(logits, stepIndex) → logits</c>。
        ''' </summary>
        ''' <remarks>
        ''' 约束解码在这里把非法 token 的 logits 置为 <c>-∞</c>。
        ''' 传入的数组会被原地修改，返回同一个引用即可。
        ''' </remarks>
        Public Property LogitsProcessor As Func(Of Double(), Integer, Double())

        ''' <summary>是否记录每一步的耗时明细。</summary>
        Public Property TrackTiming As Boolean = True

    End Class

    ''' <summary>一次生成的结果（含性能与路径信息）。</summary>
    Public Class GenerationResult

        ''' <summary>prompt 的 token 序列。</summary>
        Public Property PromptTokens As Integer()

        ''' <summary>新生成的 token 序列（不含停止符之后的内容）。</summary>
        Public Property GeneratedTokens As New List(Of Integer)

        ''' <summary>完整上下文（prompt + 新生成）。</summary>
        Public Property Context As New List(Of Integer)

        ''' <summary>prefill（或首次全序列前向）耗时，毫秒。</summary>
        Public Property PreludeMilliseconds As Double

        ''' <summary>逐步解码的耗时明细，毫秒。</summary>
        Public Property StepMilliseconds As New List(Of Double)

        ''' <summary>KV Cache 占用的字节数（未启用时为 0）。</summary>
        Public Property CacheBytes As Long

        ''' <summary>本次生成使用的计算路径描述。</summary>
        Public Property ComputePath As String

        ''' <summary>是否命中停止符。</summary>
        Public Property StoppedByStopToken As Boolean

        ''' <summary>新生成的 token 个数。</summary>
        Public ReadOnly Property NewTokens As Integer
            Get
                Return GeneratedTokens.Count
            End Get
        End Property

        ''' <summary>增量解码的总耗时，毫秒。</summary>
        Public ReadOnly Property DecodeMilliseconds As Double
            Get
                Dim total As Double = 0.0

                For Each ms In StepMilliseconds
                    total += ms
                Next

                Return total
            End Get
        End Property

        ''' <summary>单步平均耗时，毫秒。</summary>
        Public ReadOnly Property AverageStepMilliseconds As Double
            Get
                If StepMilliseconds.Count = 0 Then Return 0.0
                Return DecodeMilliseconds / StepMilliseconds.Count
            End Get
        End Property

    End Class

    ''' <summary>把模型与采样器组合成"给定 prompt 生成后续 token"的循环。</summary>
    Public Class TextGenerator

        Private ReadOnly _model As LLMModel

        ''' <summary>被驱动的模型。</summary>
        Public ReadOnly Property Model As LLMModel
            Get
                Return _model
            End Get
        End Property

        ''' <summary>采样器。</summary>
        Public Property Sampler As Sampler

        ''' <summary>
        ''' Creates a text generator.
        ''' </summary>
        ''' <param name="model">The language model used for generation.</param>
        ''' <param name="sampler">Optional sampler; a default sampler is created when omitted.</param>
        Public Sub New(model As LLMModel, Optional sampler As Sampler = Nothing)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))

            _model = model
            Me.Sampler = If(sampler, New Sampler())
        End Sub

        ''' <summary>自回归生成。</summary>
        ''' <param name="promptIds">prompt 的 token 序列（至少 1 个 token）</param>
        ''' <param name="options">生成超参；传 <see langword="Nothing"/> 使用默认值</param>
        Public Function Generate(promptIds As Integer(), Optional options As GenerationOptions = Nothing) As GenerationResult
            If promptIds Is Nothing OrElse promptIds.Length = 0 Then
                Throw New ArgumentException("prompt 不能为空")
            End If

            Dim opt = If(options, New GenerationOptions())
            Dim result As New GenerationResult With {
                .PromptTokens = CType(promptIds.Clone(), Integer()),
                .ComputePath = If(opt.UseCache, "KV Cache（prefill + 增量解码，单步 O(t)）", "无缓存（每步重算全部前缀，单步 O(t²)）")
            }

            Dim context As New List(Of Integer)(promptIds)
            Dim caches As KVCache()() = Nothing

            ' ---- 1. 预填充 ----
            Dim watch = Diagnostics.Stopwatch.StartNew()
            Dim logits As Double()

            If opt.UseCache Then
                caches = _model.CreateCaches(_model.Config.MaxSeqLen, 1)
                logits = _model.Prefill(promptIds, caches(0))
                result.CacheBytes = LLMModel.CacheBytes(caches)
            Else
                logits = _model.ForwardWithoutCache(promptIds)
            End If

            watch.Stop()
            result.PreludeMilliseconds = watch.Elapsed.TotalMilliseconds

            ' ---- 2. 逐步解码 ----
            For [step] As Integer = 1 To opt.MaxNewTokens
                If opt.LogitsProcessor IsNot Nothing Then
                    logits = opt.LogitsProcessor(logits, [step])
                End If

                Dim token = Sampler.Sample(logits, context)

                result.GeneratedTokens.Add(token)
                context.Add(token)

                If opt.StopTokenIds IsNot Nothing AndAlso Array.IndexOf(opt.StopTokenIds, token) >= 0 Then
                    result.StoppedByStopToken = True
                    Exit For
                End If

                If [step] = opt.MaxNewTokens Then Exit For

                Dim stepWatch = Diagnostics.Stopwatch.StartNew()

                If opt.UseCache Then
                    logits = _model.DecodeStep(token, caches(0))
                Else
                    logits = _model.ForwardWithoutCache(context.ToArray())
                End If

                stepWatch.Stop()

                If opt.TrackTiming Then
                    result.StepMilliseconds.Add(stepWatch.Elapsed.TotalMilliseconds)
                End If
            Next

            result.Context = context

            ' 缓存占用要按<b>生成结束</b>时的长度统计 —— 在 prefill 之后立刻读只会得到
            ' prompt 那么长的缓存，看不出"随序列线性增长"这件事。
            If caches IsNot Nothing Then
                result.CacheBytes = LLMModel.CacheBytes(caches)
            End If

            Return result
        End Function

        ''' <summary>生成并解码为文本（需要外部提供 detokenize 函数）。</summary>
        Public Function GenerateText(promptIds As Integer(),
                                     detokenize As Func(Of IEnumerable(Of Integer), String),
                                     Optional options As GenerationOptions = Nothing) As String

            Dim result = Generate(promptIds, options)

            Return detokenize(result.GeneratedTokens)
        End Function

    End Class

End Namespace
