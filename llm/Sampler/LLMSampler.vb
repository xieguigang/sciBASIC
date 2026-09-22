#Region "Microsoft.VisualBasic::9e9b4f68fb0e2f403a4e05631a5d1b04, llm\Sampler\LLMSampler.vb"

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

    '   Total Lines: 236
    '    Code Lines: 135 (57.20%)
    ' Comment Lines: 51 (21.61%)
    '    - Xml Docs: 43.14%
    ' 
    '   Blank Lines: 50 (21.19%)
    '     File Size: 8.70 KB


    '     Class LLMSampler
    ' 
    '         Properties: Config, LastCandidateCount, LastEntropy
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: BuildProbabilities, Sample
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' Sampler —— 从 logits 到下一个 token 的采样策略
'
' logits 变成概率再选出 token 的方式，共同决定了生成风格里"创造性 vs 精确性"的平衡：
'
'   温度 τ    先对 logits 除以 τ 再 softmax。
'             τ → 0 退化为贪心（只取最大值）；τ = 1 保持模型原始分布；
'             τ > 1 分布被压平、更随机（也更容易走题）。
'
'   Top-k     只在概率最高的 k 个 token 里采样，直接砍掉长尾。k 固定，不随分布形状自适应。
'
'   Top-p     只在"累计概率达到 p 的最小 token 集合"里采样（核采样）。
'             分布尖锐时集合小、分布平坦时集合大，因此通常比固定 k 更自然。
'
'   重复惩罚  对已经出现过的 token 打折，抑制"复读机"式退化。
'
' 采样顺序遵循主流实现的约定：
'   重复惩罚 → 贪心短路 → 温度 → Top-k → softmax → Top-p → 重新归一化 → 采样
' ---------------------------------------------------------------------------

Imports std = System.Math

Namespace Sampler

    ''' <summary>
    ''' 采样器：把 <c>logits</c> 变成概率分布，再按配置选出一个 token。
    ''' </summary>
    Public Class LLMSampler

        Private ReadOnly _random As Random

        ''' <summary>采样超参（可在运行时替换，<see cref="_random"/> 不受影响）。</summary>
        Public Property Config As SamplingConfig

        Private _lastCandidateCount As Integer
        Private _lastEntropy As Double

        ''' <summary>最近一次采样保留的候选 token 个数（Top-k / Top-p 截断之后）。</summary>
        Public Property LastCandidateCount As Integer
            Get
                Return _lastCandidateCount
            End Get
            Private Set(value As Integer)
                _lastCandidateCount = value
            End Set
        End Property

        ''' <summary>最近一次采样所依据分布的熵（nats），可用于观测"确定性程度"。</summary>
        Public Property LastEntropy As Double
            Get
                Return _lastEntropy
            End Get
            Private Set(value As Double)
                _lastEntropy = value
            End Set
        End Property

        ''' <summary>
        ''' Creates a sampler.
        ''' </summary>
        ''' <param name="config">Optional sampling configuration; default values are used when omitted.</param>
        Public Sub New(Optional config As SamplingConfig = Nothing)
            Me.Config = If(config, New SamplingConfig())
            _random = If(Me.Config.Seed.HasValue, New Random(Me.Config.Seed.Value), New Random())
        End Sub

        ''' <summary>
        ''' 从 <paramref name="logits"/> 采样一个 token。
        ''' </summary>
        ''' <param name="logits">长度 = 词表大小的 logits</param>
        ''' <param name="history">已生成的 token 序列，用于重复惩罚；可为 <see langword="Nothing"/></param>
        Public Function Sample(logits As Double(), Optional history As IReadOnlyList(Of Integer) = Nothing) As Integer
            If logits Is Nothing OrElse logits.Length = 0 Then
                Throw New ArgumentException("logits 不能为空")
            End If

            If Config.Greedy Then
                Dim best = 0
                Dim bestVal = Double.NegativeInfinity

                For i As Integer = 0 To logits.Length - 1
                    If logits(i) > bestVal Then
                        bestVal = logits(i)
                        best = i
                    End If
                Next

                LastCandidateCount = 1
                LastEntropy = 0.0

                Return best
            End If

            Dim probs = BuildProbabilities(logits, history)
            Dim r = _random.NextDouble()
            Dim acc As Double = 0.0

            For i As Integer = 0 To probs.Length - 1
                acc += probs(i)

                If r < acc Then Return i
            Next

            ' 浮点累加可能差一点点到 1，兜底返回概率最大的那个
            Dim fallback = 0
            Dim fallbackVal = 0.0

            For i As Integer = 0 To probs.Length - 1
                If probs(i) > fallbackVal Then
                    fallbackVal = probs(i)
                    fallback = i
                End If
            Next

            Return fallback
        End Function

        ''' <summary>
        ''' 构造完整的词表概率分布（未入选的 token 概率为 0）。
        ''' </summary>
        ''' <remarks>
        ''' 单独暴露出来是为了观测：测试程序可以用它打印"截断之后还剩多少个候选"、
        ''' 分布熵是多少，从而把温度 / Top-k / Top-p 的作用可视化。
        ''' </remarks>
        Public Function BuildProbabilities(logits As Double(), Optional history As IReadOnlyList(Of Integer) = Nothing) As Double()
            Dim n = logits.Length
            Dim work(n - 1) As Double

            Call Array.Copy(logits, work, n)

            ' ---- 1. 重复惩罚 ----
            Dim penalty = Config.RepetitionPenalty

            If penalty > 0 AndAlso penalty <> 1.0 AndAlso history IsNot Nothing Then
                For Each t In history
                    If t >= 0 AndAlso t < n Then
                        ' 正 logits 除以惩罚、负 logits 乘以惩罚 —— 两种情况下都会让该 token 更不被选中
                        If work(t) > 0 Then
                            work(t) /= penalty
                        Else
                            work(t) *= penalty
                        End If
                    End If
                Next
            End If

            ' ---- 2. 温度 ----
            Dim temperature = If(Config.Temperature > 0, Config.Temperature, 1.0)

            ' ---- 3. 按 logits 降序排序（对取负后的值做升序排序即可）----
            Dim keys(n - 1) As Double
            Dim order(n - 1) As Integer

            For i As Integer = 0 To n - 1
                ' +∞（原值为 -∞，常见于约束解码的非法 token）会被排到最后，正好符合预期
                keys(i) = -work(i)
                order(i) = i
            Next

            Call Array.Sort(keys, order)

            Dim keep = n

            If Config.TopK > 0 AndAlso Config.TopK < n Then keep = Config.TopK

            ' ---- 4. 在保留下来的候选上做 softmax ----
            Dim probs(n - 1) As Double
            Dim maxVal = work(order(0))
            Dim sum As Double = 0.0

            For i As Integer = 0 To keep - 1
                Dim e = std.Exp((work(order(i)) - maxVal) / temperature)
                probs(order(i)) = e
                sum += e
            Next

            If sum <= 0 Then sum = 1.0

            For i As Integer = 0 To keep - 1
                probs(order(i)) /= sum
            Next

            ' ---- 5. Top-p（核采样）----
            Dim topP = Config.TopP

            If topP > 0 AndAlso topP < 1.0 Then
                Dim cumulative As Double = 0.0
                Dim cut = keep - 1

                For i As Integer = 0 To keep - 1
                    cumulative += probs(order(i))

                    If cumulative >= topP Then
                        cut = i
                        Exit For
                    End If
                Next

                ' 至少保留 1 个候选
                keep = cut + 1

                For i As Integer = cut + 1 To order.Length - 1
                    probs(order(i)) = 0.0
                Next

                Dim renorm As Double = 0.0

                For i As Integer = 0 To cut
                    renorm += probs(order(i))
                Next

                If renorm > 0 Then
                    For i As Integer = 0 To cut
                        probs(order(i)) /= renorm
                    Next
                End If
            End If

            ' ---- 6. 观测指标 ----
            Dim entropy As Double = 0.0

            For i As Integer = 0 To keep - 1
                Dim p = probs(order(i))

                If p > 0 Then entropy -= p * std.Log(p)
            Next

            LastCandidateCount = keep
            LastEntropy = entropy

            Return probs
        End Function

    End Class

End Namespace

