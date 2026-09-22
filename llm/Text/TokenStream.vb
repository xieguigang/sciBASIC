' ---------------------------------------------------------------------------
' TokenStream —— 自回归生成的"下一个 token"流
'
' 把"上下文 → 下一个位置的 logits"这件事封装成一个流对象，从而让上层（文本生成器、
' 约束解码器、Agent 循环）不必关心底层的两条计算路径：
'
'   * 带缓存：prefill 一次，之后每次 Accept 只处理 1 个 token（单步 O(t)）；
'   * 无缓存：每次 Accept 都把整段上下文重新前向一遍（单步 O(t²)）。
'
' 两条路径给出的 logits 在数学上完全一致，因此它同时也是 KV Cache 的正确性对照组。
'
' 之所以把它独立成类而不是塞在生成循环里：约束解码需要在每一步采样之前对 logits
' 做掩码，Agent 循环还需要在"工具结果回填"时把一批 token 一次性灌回流中 ——
' 这两件事都要求"采样"与"喂回"是解耦的两步。
' ---------------------------------------------------------------------------

''' <summary>
''' 自回归 token 流：持有上下文与缓存，按需给出下一个位置的 logits。
''' </summary>
Public Class TokenStream

    Private ReadOnly _model As LLMModel
    Private ReadOnly _useCache As Boolean
    Private ReadOnly _caches As KVCache()()
    Private ReadOnly _context As New List(Of Integer)

    Private _logits As Double()

    ''' <summary>被驱动的模型。</summary>
    Public ReadOnly Property Model As LLMModel
        Get
            Return _model
        End Get
    End Property

    ''' <summary>完整上下文（prompt + 已接受的 token）。</summary>
    Public ReadOnly Property Context As IList(Of Integer)
        Get
            Return _context
        End Get
    End Property

    ''' <summary>是否走 KV Cache 路径。</summary>
    Public ReadOnly Property IsCached As Boolean
        Get
            Return _useCache
        End Get
    End Property

    ''' <summary>KV Cache 当前占用的字节数（未启用时为 0）。</summary>
    Public ReadOnly Property CacheBytes As Long
        Get
            If Not _useCache Then Return 0
            Return LLMModel.CacheBytes(_caches)
        End Get
    End Property

    ''' <summary>
    ''' 当前上下文之下、<b>下一个位置</b>的 logits（长度 = 词表大小）。
    ''' </summary>
    ''' <remarks>
    ''' 返回的是内部数组本身：调用方（如约束解码）需要就地掩码它以省掉一次 10 万级
    ''' 元素的拷贝。不要长期持有该引用 —— 下一次 <see cref="Accept"/> 会替换它。
    ''' </remarks>
    Public ReadOnly Property CurrentLogits As Double()
        Get
            Return _logits
        End Get
    End Property

    Private Sub New(model As LLMModel, promptIds As Integer(), useCache As Boolean)
        _model = model
        _useCache = useCache

        If promptIds IsNot Nothing AndAlso promptIds.Length > 0 Then
            _context.AddRange(promptIds)
        End If

        If useCache Then
            _caches = model.CreateCaches(model.Config.MaxSeqLen, 1)

            If _context.Count > 0 Then
                _logits = model.Prefill(_context.ToArray(), _caches(0))
            End If
        Else
            If _context.Count > 0 Then
                _logits = model.ForwardWithoutCache(_context.ToArray())
            End If
        End If
    End Sub

    ''' <summary>
    ''' 创建一个 token 流，并把 <paramref name="promptIds"/> 作为初始上下文（含 prefill）。
    ''' </summary>
    ''' <param name="model">语言模型</param>
    ''' <param name="promptIds">初始上下文；可为空</param>
    ''' <param name="useCache">是否启用 KV Cache</param>
    Public Shared Function Create(model As LLMModel, promptIds As Integer(),
                                  Optional useCache As Boolean = True) As TokenStream
        Return New TokenStream(model, promptIds, useCache)
    End Function

    ''' <summary>
    ''' 接受一个 token：把它追加到上下文，并推进到下一个位置的 logits。
    ''' </summary>
    Public Sub Accept(token As Integer)
        _context.Add(token)

        If _useCache Then
            _logits = _model.DecodeStep(token, _caches(0))
        Else
            _logits = _model.ForwardWithoutCache(_context.ToArray())
        End If
    End Sub

    ''' <summary>
    ''' 一次性灌入一批已知 token（工具结果回填的入口）。
    ''' </summary>
    ''' <remarks>
    ''' 与逐个 <see cref="Accept"/> 的差别只在效率：批量灌入时带缓存的路径只需要一次
    ''' prefill（把整批 token 的 K/V 一次写进缓存），而不是每个 token 走一次增量解码。
    ''' 这正是 readme 里"工具结果回填后 KV cache 可以复用前缀、只需 prefill 新增 token"
    ''' 那句话的实现。
    ''' </remarks>
    Public Sub Fill(tokens As IEnumerable(Of Integer))
        Dim batch = tokens.ToArray()

        If batch.Length = 0 Then Return

        _context.AddRange(batch)

        If _useCache Then
            _logits = _model.Prefill(batch, _caches(0))
        Else
            _logits = _model.ForwardWithoutCache(_context.ToArray())
        End If
    End Sub

    ''' <summary>把已有的 logits 原样用作"下一步"的 logits（用于预置好的续写）。</summary>
    Friend Sub OverrideLogits(logits As Double())
        _logits = logits
    End Sub

End Class


