#Region "Microsoft.VisualBasic::13aac341ce8feb10e704424af799c9bd, llm\Generator\GenerationResult.vb"

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

    '   Total Lines: 61
    '    Code Lines: 32 (52.46%)
    ' Comment Lines: 12 (19.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 17 (27.87%)
    '     File Size: 2.16 KB


    '     Class GenerationResult
    ' 
    '         Properties: AverageStepMilliseconds, CacheBytes, ComputePath, Context, DecodeMilliseconds
    '                     GeneratedTokens, NewTokens, PreludeMilliseconds, PromptTokens, StepMilliseconds
    '                     StoppedByStopToken
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
