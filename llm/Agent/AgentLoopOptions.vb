#Region "Microsoft.VisualBasic::b7ec76125384d8934eedc05cfc970b48, llm\Agent\AgentLoopOptions.vb"

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

    '   Total Lines: 41
    '    Code Lines: 13 (31.71%)
    ' Comment Lines: 16 (39.02%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (29.27%)
    '     File Size: 1.70 KB


    '     Class AgentLoopOptions
    ' 
    '         Properties: ForcedToolName, MaxArgumentTokens, MaxTokensPerReply, MaxToolRounds, Sampling
    '                     TraceSteps, UseCache, Verbose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DeepLearning.LLM.Sampler

Namespace Agent

    ''' <summary>Agent 循环的超参。</summary>
    Public Class AgentLoopOptions

        ''' <summary>最大工具轮次（防循环调用的兜底）。</summary>
        Public Property MaxToolRounds As Integer = 4

        ''' <summary>每一轮"自由生成"阶段最多生成多少 token（用于发现工具调用信号灯）。</summary>
        Public Property MaxTokensPerReply As Integer = 24

        ''' <summary>约束解码阶段最多生成多少 token 来补全参数对象。</summary>
        Public Property MaxArgumentTokens As Integer = 96

        ''' <summary>是否启用 KV Cache。</summary>
        Public Property UseCache As Boolean = True

        ''' <summary>采样配置。</summary>
        Public Property Sampling As New SamplingConfig()

        ''' <summary>
        ''' 强制以该工具开始本轮调用。
        ''' </summary>
        ''' <remarks>
        ''' 小模型在训练不充分时未必能自己吐出工具调用信号灯。该选项让演示流程
        ''' 确定性地走到"约束解码"这一段，从而把算法本身讲清楚；置空则完全依赖模型的自主决策。
        ''' 无论哪种方式，参数对象都<b>必然</b>由约束解码产出。
        ''' </remarks>
        Public Property ForcedToolName As String

        ''' <summary>是否把过程打印到控制台。</summary>
        Public Property Verbose As Boolean = True

        ''' <summary>约束解码逐 token 轨迹的最大记录长度。</summary>
        Public Property TraceSteps As Integer = 16

    End Class

End Namespace
