Namespace Generator

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

End Namespace