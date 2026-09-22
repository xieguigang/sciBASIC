Namespace Generator


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

End Namespace