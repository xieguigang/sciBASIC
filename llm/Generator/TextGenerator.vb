#Region "Microsoft.VisualBasic::3610a999c8967536ebfd79a58b131d2c, llm\Generator\TextGenerator.vb"

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

    '   Total Lines: 143
    '    Code Lines: 75 (52.45%)
    ' Comment Lines: 37 (25.87%)
    '    - Xml Docs: 32.43%
    ' 
    '   Blank Lines: 31 (21.68%)
    '     File Size: 6.23 KB


    '     Class TextGenerator
    ' 
    '         Properties: Model, Sampler
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Generate, GenerateText
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

Imports Microsoft.VisualBasic.DeepLearning.LLM.Sampler
Imports Diagnostics = System.Diagnostics

Namespace Generator

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
        Public Property Sampler As LLMSampler

        ''' <summary>
        ''' Creates a text generator.
        ''' </summary>
        ''' <param name="model">The language model used for generation.</param>
        ''' <param name="sampler">Optional sampler; a default sampler is created when omitted.</param>
        Public Sub New(model As LLMModel, Optional sampler As LLMSampler = Nothing)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))

            _model = model
            Me.Sampler = If(sampler, New LLMSampler())
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

