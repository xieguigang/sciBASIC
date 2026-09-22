#Region "Microsoft.VisualBasic::8c7bd4615490f6ae4defe0c02af374ba, llm\Agent\AgentRunResult.vb"

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

    '   Total Lines: 32
    '    Code Lines: 17 (53.12%)
    ' Comment Lines: 10 (31.25%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (15.62%)
    '     File Size: 1.48 KB


    '     Class AgentRunResult
    ' 
    '         Properties: CacheBytes, ElapsedMilliseconds, FinalText, HitRoundLimit, Rounds
    '                     SystemPrompt, ToolCallCount, TotalContextTokens, UserMessage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
