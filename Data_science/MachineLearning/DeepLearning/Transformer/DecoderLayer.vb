#Region "Microsoft.VisualBasic::9b4db32dad598fa40ce70d6e3876ae4a, Data_science\MachineLearning\DeepLearning\Transformer\DecoderLayer.vb"

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

    '   Total Lines: 274
    '    Code Lines: 142 (51.82%)
    ' Comment Lines: 82 (29.93%)
    '    - Xml Docs: 81.71%
    ' 
    '   Blank Lines: 50 (18.25%)
    '     File Size: 14.26 KB


    '     Class DecoderLayer
    ' 
    '         Properties: LastCache
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Backward, Decode
    ' 
    '         Sub: MakeTrainingStep, SetDropoutNodes, ZeroGradients
    '         Class Cache
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' DecoderLayer —— 解码器单层：掩码自注意力 + 交叉注意力 + 前馈网络
'
' 三条子层均为「残差 + LayerNorm」。迁移要点：
'   * 每个子层的 AddNorm 统计量与前向输出都要缓存；
'   * 反向阶段逆序回传，交叉注意力会把梯度同时回传到 encoderOutput；
'   * 由于解码是按词逐步进行的，本层的前向缓存会被覆盖，
'     因此调用方（DecoderStack / TransformerModel）必须保存每一步的缓存副本。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Transformer

    ''' <summary>
    ''' One decoder layer: masked self attention, cross attention and a position wise feed forward network, each wrapped in
    ''' a residual connection and layer normalization.
    ''' </summary>
    ''' <remarks>
    ''' Every sub layer caches its AddNorm statistics and its forward output for the backward pass. Because decoding proceeds
    ''' token by token, the forward cache of this layer is overwritten on every step, so callers
    ''' (<see cref="DecoderStack"/>, <see cref="TransformerModel"/>) must keep a snapshot of the cache of each step.
    ''' </remarks>
    Public Class DecoderLayer

        Private embeddingSize As Integer

        Private mha As MultiHeadAttention
        Private mha_masked As MultiHeadAttention
        ''' <summary>The position wise feed forward sub layer of this decoder layer.</summary>
        Public ff As FeedForwardNetwork

        Private dropoutMask1, dropoutMask2, dropoutMask3 As Boolean()
        Private dropoutRate As Double = 0

        ''' <summary>
        ''' Forward intermediates of one decode step, required by the backward pass.
        ''' </summary>
        Public Class Cache
            ''' <summary>The input of this layer for the step.</summary>
            Public Input As Tensor
            ''' <summary>Output of the masked self attention sub layer.</summary>
            Public MaskedAttention As Tensor
            ''' <summary>Masked self attention output after dropout.</summary>
            Public MaskedAttentionDropped As Tensor
            ''' <summary>Result of the first AddNorm.</summary>
            Public Normalized1 As Tensor
            ''' <summary>Mean used by the first layer normalization.</summary>
            Public Norm1Mean As Double()
            ''' <summary>Inverse standard deviation used by the first layer normalization.</summary>
            Public Norm1InvStd As Double()
            ''' <summary>Output of the cross attention sub layer.</summary>
            Public CrossAttention As Tensor
            ''' <summary>Cross attention output after dropout.</summary>
            Public CrossAttentionDropped As Tensor
            ''' <summary>Result of the second AddNorm.</summary>
            Public Normalized2 As Tensor
            ''' <summary>Mean used by the second layer normalization.</summary>
            Public Norm2Mean As Double()
            ''' <summary>Inverse standard deviation used by the second layer normalization.</summary>
            Public Norm2InvStd As Double()
            ''' <summary>Output of the feed forward sub layer.</summary>
            Public FeedForwardOutput As Tensor
            ''' <summary>Feed forward output after dropout.</summary>
            Public FeedForwardDropped As Tensor
            ''' <summary>Mean used by the third layer normalization.</summary>
            Public Norm3Mean As Double()
            ''' <summary>Inverse standard deviation used by the third layer normalization.</summary>
            Public Norm3InvStd As Double()
            ''' <summary>Indicates whether dropout was applied during this step.</summary>
            Public DropoutApplied As Boolean
            ''' <summary>Forward cache snapshot of the masked self attention sub layer.</summary>
            Public MaskedCache As MultiHeadAttention.Cache
            ''' <summary>Forward cache snapshot of the cross attention sub layer.</summary>
            Public CrossCache As MultiHeadAttention.Cache
            ''' <summary>Forward cache snapshot of the feed forward sub layer.</summary>
            Public FfCache As FeedForwardNetwork.Cache
        End Class

        Private _lastCache As Cache

        ''' <summary>Gets the forward cache of the most recent <see cref="Decode"/> step.</summary>
        Public ReadOnly Property LastCache As Cache
            Get
                Return _lastCache
            End Get
        End Property

        ''' <summary>
        ''' Creates a decoder layer.
        ''' </summary>
        ''' <param name="embeddingSize">Width of the model, used for the residual stream.</param>
        ''' <param name="dk">Dimension of the query and key projections per head.</param>
        ''' <param name="dv">Dimension of the value projection per head.</param>
        ''' <param name="h">Number of attention heads.</param>
        ''' <param name="dff">Hidden width of the feed forward network.</param>
        Public Sub New(embeddingSize As Integer, dk As Integer, dv As Integer, h As Integer, dff As Integer)
            Me.embeddingSize = embeddingSize

            mha = New MultiHeadAttention(dk, dv, h, embeddingSize, False)
            mha_masked = New MultiHeadAttention(dk, dv, h, embeddingSize, True)
            ff = New FeedForwardNetwork(dff, embeddingSize)

            dropoutMask1 = New Boolean(embeddingSize - 1) {}
            dropoutMask2 = New Boolean(embeddingSize - 1) {}
            dropoutMask3 = New Boolean(embeddingSize - 1) {}
        End Sub

        ''' <summary>
        ''' Runs one decoder step: masked self attention, cross attention over the encoder output and a feed forward network.
        ''' </summary>
        ''' <param name="encoderOutput">The output of the encoder stack used by the cross attention sub layer.</param>
        ''' <param name="decoderInput">The embedded decoder input of this step.</param>
        ''' <param name="isTraining">When <c>True</c> dropout is applied where configured.</param>
        ''' <returns>The decoder output of this step.</returns>
        Public Function Decode(encoderOutput As Tensor, decoderInput As Tensor, isTraining As Boolean) As Tensor
            Dim dropoutApplied = isTraining AndAlso dropoutRate > 0

            ' Masked multi headed attention
            Dim maskedAttentionFilteredData = mha_masked.Update(decoderInput)
            Dim maskedAttentionDropped = maskedAttentionFilteredData

            If dropoutApplied Then maskedAttentionDropped = TensorOps.DropoutMask(maskedAttentionDropped, dropoutMask1, dropoutRate)

            Dim mean1 As Double() = Nothing, invStd1 As Double() = Nothing
            Dim normalized1 = TensorOps.AddNormForward(decoderInput, maskedAttentionDropped, mean1, invStd1)

            ' Multi headed attention
            Dim attentionFilteredData = mha.Update(encoderOutput, normalized1)
            Dim attentionDropped = attentionFilteredData

            If dropoutApplied Then attentionDropped = TensorOps.DropoutMask(attentionDropped, dropoutMask2, dropoutRate)

            Dim mean2 As Double() = Nothing, invStd2 As Double() = Nothing
            Dim normalized2 = TensorOps.AddNormForward(normalized1, attentionDropped, mean2, invStd2)

            ' Feed forward neural network
            Dim feedForwardOutput = ff.FeedForward(normalized2)
            Dim feedForwardDropped = feedForwardOutput

            If dropoutApplied Then feedForwardDropped = TensorOps.DropoutMask(feedForwardDropped, dropoutMask3, dropoutRate)

            Dim mean3 As Double() = Nothing, invStd3 As Double() = Nothing
            Dim output = TensorOps.AddNormForward(normalized2, feedForwardDropped, mean3, invStd3)

            _lastCache = New Cache With {
                .Input = decoderInput,
                .MaskedAttention = maskedAttentionFilteredData,
                .MaskedAttentionDropped = maskedAttentionDropped,
                .Normalized1 = normalized1,
                .Norm1Mean = mean1,
                .Norm1InvStd = invStd1,
                .CrossAttention = attentionFilteredData,
                .CrossAttentionDropped = attentionDropped,
                .Normalized2 = normalized2,
                .Norm2Mean = mean2,
                .Norm2InvStd = invStd2,
                .FeedForwardOutput = feedForwardOutput,
                .FeedForwardDropped = feedForwardDropped,
                .Norm3Mean = mean3,
                .Norm3InvStd = invStd3,
                .DropoutApplied = dropoutApplied,
                .MaskedCache = mha_masked.LastCache,
                .CrossCache = mha.LastCache,
                .FfCache = ff.LastCache
            }

            Return output
        End Function

        ''' <summary>
        ''' Backpropagates through the three sub layers and returns the gradient with respect to the decoder input, while
        ''' accumulating the gradient with respect to the encoder output.
        ''' </summary>
        ''' <param name="forwardCache">The forward cache snapshot that belongs to this decode step.</param>
        ''' <param name="dOut">Gradient with respect to the decoder output of this step.</param>
        ''' <param name="dEncoderOutput">Accumulator for the gradient with respect to the encoder output.</param>
        ''' <returns>The gradient with respect to the decoder input.</returns>
        Public Function Backward(forwardCache As Cache, dOut As Tensor, ByRef dEncoderOutput As Tensor) As Tensor
            Dim cache = forwardCache

            If cache Is Nothing Then Throw New InvalidOperationException("必须先执行前向传播才能反向传播")

            ' 第三个 AddNorm：output = AddNorm(normalized2, feedForwardDropped)
            Dim dx3 = TensorOps.AddNormBackward(dOut, cache.Normalized2, cache.FeedForwardDropped, cache.Norm3Mean, cache.Norm3InvStd)
            Dim dFeedForwardDropped = TensorOps.CloneTensor(dx3)
            Dim dNormalized2 = dx3

            Dim dFeedForward = dFeedForwardDropped

            If cache.DropoutApplied Then dFeedForward = TensorOps.DropoutMaskBackward(dFeedForward, dropoutMask3, dropoutRate)

            Call TensorOps.Accumulate(dNormalized2, ff.Backward(cache.FfCache, dFeedForward))

            ' 第二个 AddNorm：normalized2 = AddNorm(normalized1, attentionDropped)
            Dim dx2 = TensorOps.AddNormBackward(dNormalized2, cache.Normalized1, cache.CrossAttentionDropped, cache.Norm2Mean, cache.Norm2InvStd)
            Dim dAttentionDropped = TensorOps.CloneTensor(dx2)
            Dim dNormalized1 = dx2

            Dim dAttention = dAttentionDropped

            If cache.DropoutApplied Then dAttention = TensorOps.DropoutMaskBackward(dAttention, dropoutMask2, dropoutRate)

            Dim dFromEncoder As Tensor = Nothing

            Call TensorOps.Accumulate(dNormalized1, mha.Backward(cache.CrossCache, dAttention, dFromEncoder))

            If dFromEncoder IsNot Nothing Then
                If dEncoderOutput Is Nothing Then
                    dEncoderOutput = dFromEncoder
                Else
                    Call TensorOps.Accumulate(dEncoderOutput, dFromEncoder)
                End If
            End If

            ' 第一个 AddNorm：normalized1 = AddNorm(decoderInput, maskedAttentionDropped)
            Dim dx1 = TensorOps.AddNormBackward(dNormalized1, cache.Input, cache.MaskedAttentionDropped, cache.Norm1Mean, cache.Norm1InvStd)
            Dim dMaskedAttentionDropped = TensorOps.CloneTensor(dx1)
            Dim dDecoderInput = dx1

            Dim dMaskedAttention = dMaskedAttentionDropped

            If cache.DropoutApplied Then dMaskedAttention = TensorOps.DropoutMaskBackward(dMaskedAttention, dropoutMask1, dropoutRate)

            Dim unused As Tensor = Nothing

            Call TensorOps.Accumulate(dDecoderInput, mha_masked.Backward(cache.MaskedCache, dMaskedAttention, unused))

            Return dDecoderInput
        End Function

        ''' <summary>
        ''' Configures dropout and draws new dropout masks for the three sub layers.
        ''' </summary>
        ''' <param name="dropoutRate">Dropout rate in <c>[0, 1)</c>.</param>
        ''' <exception cref="ArgumentException">Thrown when the rate is outside <c>[0, 1)</c>.</exception>
        Public Sub SetDropoutNodes(dropoutRate As Double)
            If dropoutRate < 0 OrElse dropoutRate >= 1 Then Throw New ArgumentException("Error: dropout rate must be >= 0 and < 1")

            Me.dropoutRate = dropoutRate

            For i = 0 To embeddingSize - 1
                dropoutMask1(i) = False
                If randf.NextDouble < dropoutRate Then dropoutMask1(i) = True

                dropoutMask2(i) = False
                If randf.NextDouble < dropoutRate Then dropoutMask2(i) = True

                dropoutMask3(i) = False
                If randf.NextDouble < dropoutRate Then dropoutMask3(i) = True
            Next
        End Sub

        ''' <summary>Clears the gradient accumulators of every parameter of this layer.</summary>
        Public Sub ZeroGradients()
            mha_masked.ZeroGradients()
            mha.ZeroGradients()
            ff.ZeroGradients()
        End Sub

        ''' <summary>
        ''' Applies one optimizer step to every parameter of this layer.
        ''' </summary>
        ''' <param name="learningRate">The learning rate for this step.</param>
        ''' <param name="[step]">The current step index, used by the Adam bias correction.</param>
        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            mha_masked.MakeTrainingStep(learningRate, [step])
            mha.MakeTrainingStep(learningRate, [step])
            ff.MakeTrainingStep(learningRate, [step])
        End Sub

    End Class
End Namespace
