#Region "Microsoft.VisualBasic::6b6fc4f5a6dcfe027b8f25242d6cc614, llm\LLMBlock.vb"

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

    '   Total Lines: 298
    '    Code Lines: 171 (57.38%)
    ' Comment Lines: 66 (22.15%)
    '    - Xml Docs: 72.73%
    ' 
    '   Blank Lines: 61 (20.47%)
    '     File Size: 11.78 KB


    ' Class LLMBlock
    ' 
    '     Properties: ActiveParametersPerToken, IsMixtureOfExperts, LastCache, TotalParameters
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Align, Backward, Forward, SumLengths
    ' 
    '     Sub: MakeTrainingStep, RegisterParameters, ZeroGradients
    '     Class Cache
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' LLMBlock —— decoder-only Transformer 的一个解码层
'
' 结构（Pre-Norm，与 Llama / Qwen / DeepSeek 一致）：
'
'     x  ←  x + Attention( RMSNorm₁(x) )
'     x  ←  x + FFN( RMSNorm₂(x) )        ← FFN 可以是稠密 SwiGLU，也可以是 MoE
'
' 为什么用 Pre-Norm：归一化放在子层之前、残差绕开归一化直连，等于给网络铺了一条
' 从输入直达输出的"高速通路"，梯度不需要穿过每层的归一化统计量，深层网络因此可训。
'
' 为什么残差里的 FFN 常常换成 MoE：注意力负责"跨位置混合信息"，FFN 才是存储事实
' 知识的地方（参数量占比通常超过 2/3）。把 FFN 稀疏化，就是"在不增加单 token 计算
' 量的前提下把知识容量做大"这一 MoE 动机的直接落点。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 单层解码块：Pre-Norm 的"因果自注意力 + 前馈网络（稠密或 MoE）"。
''' </summary>
Public Class LLMBlock

    Private ReadOnly _dModel As Integer

    ''' <summary>注意力子层前的 RMSNorm。</summary>
    Public ReadOnly Norm1 As RmsNorm

    ''' <summary>前馈子层前的 RMSNorm。</summary>
    Public ReadOnly Norm2 As RmsNorm

    ''' <summary>因果自注意力子层。</summary>
    Public ReadOnly Attn As CausalSelfAttention

    ''' <summary>MoE 前馈子层；为 <see langword="Nothing"/> 时本层使用稠密 SwiGLU。</summary>
    Public ReadOnly MoE As MoELayer

    ''' <summary>稠密 SwiGLU 前馈子层；为 <see langword="Nothing"/> 时本层使用 MoE。</summary>
    Public ReadOnly Dense As SwiGLUFeedForward

    ''' <summary>本层是否使用 MoE。</summary>
    Public ReadOnly Property IsMixtureOfExperts As Boolean
        Get
            Return MoE IsNot Nothing
        End Get
    End Property

    ''' <summary>前向传播的中间量缓存，供反向传播使用。</summary>
    Public Class Cache
        ''' <summary>本层输入</summary>
        Public Input As Tensor
        ''' <summary>第一个 RMSNorm 的缓存</summary>
        Public Norm1Cache As RmsNorm.Cache
        ''' <summary>注意力子层的输出（残差相加之前）</summary>
        Public AttnOut As Tensor
        ''' <summary>注意力子层的前向缓存快照</summary>
        Public AttnCache As CausalSelfAttention.Cache
        ''' <summary>注意力残差相加之后的结果</summary>
        Public AfterAttn As Tensor
        ''' <summary>第二个 RMSNorm 的缓存</summary>
        Public Norm2Cache As RmsNorm.Cache
        ''' <summary>前馈子层的输出（残差相加之前）</summary>
        Public FfOut As Tensor
        ''' <summary>MoE 子层的缓存（稠密层时为 Nothing）</summary>
        Public MoeCache As MoELayer.Cache
        ''' <summary>稠密前馈子层的缓存（MoE 层时为 Nothing）</summary>
        Public DenseCache As SwiGLUFeedForward.Cache
    End Class

    Private _lastCache As Cache

    ''' <summary>最近一次 <see cref="Forward"/> 的中间量缓存。</summary>
    Public ReadOnly Property LastCache As Cache
        Get
            Return _lastCache
        End Get
    End Property

    ''' <param name="dModel">模型宽度</param>
    ''' <param name="nHeads">注意力 Query 头数</param>
    ''' <param name="nKvHeads">注意力 K/V 头数（GQA / MQA）</param>
    ''' <param name="headDim">单头维度</param>
    ''' <param name="moeLayer">本层使用的 MoE 子层；传 <see langword="Nothing"/> 则使用稠密 SwiGLU</param>
    ''' <param name="denseHidden">稠密 SwiGLU 的中间层宽度；&lt;= 0 时按 4·dModel 向上对齐到 64 的倍数</param>
    ''' <remarks>
    ''' 参数刻意命名为 <c>moeLayer</c> 而不是 <c>moe</c>：VB 的标识符大小写不敏感，
    ''' 若形参叫 <c>moe</c>，它会与字段 <see cref="MoE"/> 视为同一名字而被遮蔽，
    ''' 于是 <c>MoE = moe</c> 退化成自赋值、字段永远是 Nothing。
    ''' </remarks>
    Public Sub New(dModel As Integer, nHeads As Integer, nKvHeads As Integer, headDim As Integer,
                   moeLayer As MoELayer,
                   Optional denseHidden As Integer = 0)

        If dModel <= 0 Then Throw New ArgumentException($"dModel 必须为正数，实际 {dModel}")
        If moeLayer Is Nothing AndAlso denseHidden <= 0 Then
            denseHidden = Align(4 * dModel, 64)
        End If

        _dModel = dModel

        Norm1 = New RmsNorm(dModel)
        Norm2 = New RmsNorm(dModel)
        Attn = New CausalSelfAttention(dModel, nHeads, nKvHeads, headDim)

        If moeLayer Is Nothing Then
            Dense = New SwiGLUFeedForward(dModel, denseHidden)
            MoE = Nothing
        Else
            MoE = moeLayer
            Dense = Nothing
        End If
    End Sub

    ''' <summary>把 <paramref name="value"/> 向上对齐到 <paramref name="alignment"/> 的倍数。</summary>
    Private Shared Function Align(value As Integer, alignment As Integer) As Integer
        If alignment <= 1 Then Return value
        Return ((value + alignment - 1) \ alignment) * alignment
    End Function

    ''' <summary>把本层全部子模块的参数登记进参数集。</summary>
    Public Sub RegisterParameters(registry As ParameterSet, prefix As String, Optional weightDecay As Double = 0.0)
        ' RMSNorm 的 γ 不施加权重衰减：它承担的是尺度控制，而不是"知识强度"
        Call Norm1.RegisterParameters(registry, prefix & ".norm1", 0.0)
        Call Norm2.RegisterParameters(registry, prefix & ".norm2", 0.0)
        Call Attn.RegisterParameters(registry, prefix & ".attn", weightDecay)

        If MoE IsNot Nothing Then
            Call MoE.RegisterParameters(registry, prefix & ".moe", weightDecay)
        Else
            Call Dense.RegisterParameters(registry, prefix & ".ffn", weightDecay)
        End If
    End Sub

    ''' <summary>本层总参数（稠密层为实际参数；MoE 层为全部专家的参数之和）。</summary>
    Public ReadOnly Property TotalParameters As Long
        Get
            Dim total As Long = SumLengths(Norm1.Parameters) + SumLengths(Norm2.Parameters) +
                               SumLengths(Attn.Parameters)

            If MoE IsNot Nothing Then
                total += MoE.TotalParameters
            Else
                total += SumLengths(Dense.Parameters)
            End If

            Return total
        End Get
    End Property

    ''' <summary>本层单个 token 实际激活的参数（MoE 层远小于 <see cref="TotalParameters"/>）。</summary>
    Public ReadOnly Property ActiveParametersPerToken As Long
        Get
            Dim total As Long = SumLengths(Norm1.Parameters) + SumLengths(Norm2.Parameters) +
                               SumLengths(Attn.Parameters)

            If MoE IsNot Nothing Then
                total += MoE.ActiveParametersPerToken
            Else
                total += SumLengths(Dense.Parameters)
            End If

            Return total
        End Get
    End Property

    Private Shared Function SumLengths(ts As Tensor()) As Long
        Dim total As Long = 0

        For Each t In ts
            total += t.Length
        Next

        Return total
    End Function

#Region "前向"

    ''' <summary>
    ''' 前向：<c>x ← x + Attn(Norm₁(x))；x ← x + FFN(Norm₂(x))</c>。
    ''' </summary>
    ''' <param name="x">输入 <c>[B, S, dModel]</c></param>
    ''' <param name="positions">长度 S 的绝对位置序列</param>
    ''' <param name="rope">共享的 RoPE 模块</param>
    ''' <param name="caches">长度 B 的 K/V 缓存；<see langword="Nothing"/> 表示训练路径</param>
    Public Function Forward(x As Tensor, positions As Integer(), rope As RotaryEmbedding,
                            Optional caches As KVCache() = Nothing) As Tensor

        Dim norm1Out = Norm1.Forward(x)
        Dim attnOut = Attn.Forward(norm1Out, positions, rope, caches)

        Dim afterAttn = LLMTensorOps.CloneTensor(x)
        Call LLMTensorOps.Accumulate(afterAttn, attnOut)

        Dim norm2Out = Norm2.Forward(afterAttn)

        Dim ffOut As Tensor
        Dim moeCache As MoELayer.Cache = Nothing
        Dim denseCache As SwiGLUFeedForward.Cache = Nothing

        If MoE IsNot Nothing Then
            ffOut = MoE.Forward(norm2Out)
            moeCache = MoE.LastCache
        Else
            ffOut = Dense.Forward(norm2Out)
            denseCache = Dense.LastCache
        End If

        Dim output = LLMTensorOps.CloneTensor(afterAttn)
        Call LLMTensorOps.Accumulate(output, ffOut)

        _lastCache = New Cache With {
            .Input = x,
            .Norm1Cache = Norm1.LastCache,
            .AttnOut = attnOut,
            .AttnCache = Attn.LastCache,
            .AfterAttn = afterAttn,
            .Norm2Cache = Norm2.LastCache,
            .FfOut = ffOut,
            .MoeCache = moeCache,
            .DenseCache = denseCache
        }

        Return output
    End Function

#End Region

#Region "反向"

    ''' <summary>反向传播：返回对输入的梯度。</summary>
    ''' <param name="forwardCache">与该次前向对应的缓存快照</param>
    ''' <param name="dOut">对本层输出的梯度</param>
    ''' <param name="rope">前向使用的同一个 RoPE 模块</param>
    Public Function Backward(forwardCache As Cache, dOut As Tensor, rope As RotaryEmbedding) As Tensor
        Dim cache = forwardCache

        If cache Is Nothing Then Throw New InvalidOperationException("必须先执行前向传播才能反向传播")

        ' x ← x + FFN(Norm₂(x))：加法把梯度原样分给两条支路
        Dim dAfterAttn = LLMTensorOps.CloneTensor(dOut)
        Dim dFf = LLMTensorOps.CloneTensor(dOut)

        Dim dNorm2 As Tensor

        If MoE IsNot Nothing Then
            dNorm2 = MoE.Backward(cache.MoeCache, dFf)
        Else
            dNorm2 = Dense.Backward(cache.DenseCache, dFf)
        End If

        Call LLMTensorOps.Accumulate(dAfterAttn, Norm2.Backward(cache.Norm2Cache, dNorm2))

        ' x ← x + Attn(Norm₁(x))
        Dim dInput = LLMTensorOps.CloneTensor(dAfterAttn)
        Dim dAttn = LLMTensorOps.CloneTensor(dAfterAttn)

        Dim dNorm1 = Attn.Backward(cache.AttnCache, dAttn, rope)

        Call LLMTensorOps.Accumulate(dInput, Norm1.Backward(cache.Norm1Cache, dNorm1))

        Return dInput
    End Function

#End Region

#Region "训练步"

    ''' <summary>清零本层全部子模块的梯度累加器。</summary>
    Public Sub ZeroGradients()
        Norm1.ZeroGradients()
        Norm2.ZeroGradients()
        Attn.ZeroGradients()

        If MoE IsNot Nothing Then
            MoE.ZeroGradients()
        Else
            Dense.ZeroGradients()
        End If
    End Sub

    ''' <summary>按 AdamW 规则更新本层全部子模块。</summary>
    Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
        Norm1.MakeTrainingStep(learningRate, [step])
        Norm2.MakeTrainingStep(learningRate, [step])
        Attn.MakeTrainingStep(learningRate, [step])

        If MoE IsNot Nothing Then
            MoE.MakeTrainingStep(learningRate, [step])
        Else
            Dense.MakeTrainingStep(learningRate, [step])
        End If
    End Sub

#End Region

End Class



