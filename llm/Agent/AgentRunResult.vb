Namespace Agent

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

End Namespace