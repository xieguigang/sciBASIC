' ---------------------------------------------------------------------------
' ITextCodec —— 文本 ↔ token 的编解码契约
'
' LLM 算法层（本命名空间）刻意<b>不依赖</b>具体的分词器实现：它只要求外部提供
' "文本 → token id"、"token id → 文本"、以及"标记 → token id"三件事。
'
' 这样做有两个好处：
'   * 算法层与 HuggingFace 分词器解耦，可以直接面向任何词表复现；
'   * 约束解码需要的"词表文本视图"（<see cref="Vocabulary"/>）由分词器一侧一次性
'     构建好后注入，算法层不必知道 BPE / WordPiece / byte-level 的差别。
' ---------------------------------------------------------------------------

Namespace Text

    ''' <summary>文本与 token 之间的编解码接口。</summary>
    Public Interface ITextCodec

        ''' <summary>
        ''' 把文本编码为 token 序列。
        ''' </summary>
        ''' <remarks>
        ''' 实现方<b>不应</b>自动追加 BOS / EOS：prompt 里的角色标记与工具调用标记都是
        ''' 显式写出来的，自动追加会破坏协议。
        ''' </remarks>
        Function Encode(text As String) As Integer()

        ''' <summary>把 token 序列解码为文本。</summary>
        Function Decode(ids As IEnumerable(Of Integer)) As String

        ''' <summary>查询标记对应的 token id；词表中不存在时返回 -1。</summary>
        Function TokenIdOf(marker As String) As Integer

        ''' <summary>词表的文本视图（约束解码使用）。</summary>
        ReadOnly Property Vocabulary As TokenizerVocabulary

    End Interface

End Namespace
