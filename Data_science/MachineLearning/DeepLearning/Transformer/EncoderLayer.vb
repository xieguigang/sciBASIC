#Region "Microsoft.VisualBasic::82e9e12e6d2124ad7b57439666a10299, Data_science\MachineLearning\DeepLearning\Transformer\EncoderLayer.vb"

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

    '   Total Lines: 207
    '    Code Lines: 104 (50.24%)
    ' Comment Lines: 63 (30.43%)
    '    - Xml Docs: 82.54%
    ' 
    '   Blank Lines: 40 (19.32%)
    '     File Size: 10.06 KB


    '     Class EncoderLayer
    ' 
    '         Properties: LastCache
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Backward, Encode
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
' EncoderLayer —— 编码器单层：多头自注意力 + 前馈网络（均带残差 + LayerNorm）
'
' 迁移要点：前向阶段缓存两个 AddNorm 的统计量（均值 / 逆标准差）与两条子层
' 的输出；反向阶段按「第二个 AddNorm → 前馈 → 第一个 AddNorm → 自注意力」
' 的逆序回传。AddNorm 对 A/B 两个分支的梯度相同，因此需要各自持有独立副本。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Transformer

    ''' <summary>
    ''' One encoder layer: multi head self attention followed by a position wise feed forward network, each wrapped in a
    ''' residual connection and layer normalization.
    ''' </summary>
    ''' <remarks>
    ''' The forward pass caches the AddNorm statistics (mean and inverse standard deviation) of both sub layers and their
    ''' outputs; the backward pass walks the layers in reverse order. Because AddNorm yields the same gradient for both
    ''' branches, each branch keeps its own copy.
    ''' </remarks>
    Public Class EncoderLayer

        Private embeddingSize As Integer

        Private mha As MultiHeadAttention
        Private ff As FeedForwardNetwork

        Private dropoutMask1, dropoutMask2 As Boolean()
        Private dropoutRate As Double = 0

        ''' <summary>
        ''' Forward intermediates of one encode pass, required by the backward pass.
        ''' </summary>
        Public Class Cache
            ''' <summary>The input of this layer.</summary>
            Public Input As Tensor
            ''' <summary>Output of the self attention sub layer.</summary>
            Public AttentionOutput As Tensor
            ''' <summary>Self attention output after dropout.</summary>
            Public AttentionDropped As Tensor
            ''' <summary>Result of the first AddNorm.</summary>
            Public Normalized1 As Tensor
            ''' <summary>Mean used by the first layer normalization.</summary>
            Public Norm1Mean As Double()
            ''' <summary>Inverse standard deviation used by the first layer normalization.</summary>
            Public Norm1InvStd As Double()
            ''' <summary>Output of the feed forward sub layer.</summary>
            Public FeedForwardOutput As Tensor
            ''' <summary>Feed forward output after dropout.</summary>
            Public FeedForwardDropped As Tensor
            ''' <summary>Mean used by the second layer normalization.</summary>
            Public Norm2Mean As Double()
            ''' <summary>Inverse standard deviation used by the second layer normalization.</summary>
            Public Norm2InvStd As Double()
            ''' <summary>Indicates whether dropout was applied during this pass.</summary>
            Public DropoutApplied As Boolean
            ''' <summary>Forward cache snapshot of the self attention sub layer.</summary>
            Public MhaCache As MultiHeadAttention.Cache
            ''' <summary>Forward cache snapshot of the feed forward sub layer.</summary>
            Public FfCache As FeedForwardNetwork.Cache
        End Class

        Private _lastCache As Cache

        ''' <summary>Gets the forward cache of the most recent <see cref="Encode"/> pass.</summary>
        Public ReadOnly Property LastCache As Cache
            Get
                Return _lastCache
            End Get
        End Property

        ''' <summary>
        ''' Creates an encoder layer.
        ''' </summary>
        ''' <param name="embeddingSize">Width of the model, used for the residual stream.</param>
        ''' <param name="dk">Dimension of the query and key projections per head.</param>
        ''' <param name="dv">Dimension of the value projection per head.</param>
        ''' <param name="h">Number of attention heads.</param>
        ''' <param name="dff">Hidden width of the feed forward network.</param>
        Public Sub New(embeddingSize As Integer, dk As Integer, dv As Integer, h As Integer, dff As Integer)
            Me.embeddingSize = embeddingSize

            mha = New MultiHeadAttention(dk, dv, h, embeddingSize, False)
            ff = New FeedForwardNetwork(dff, embeddingSize)

            dropoutMask1 = New Boolean(embeddingSize - 1) {}
            dropoutMask2 = New Boolean(embeddingSize - 1) {}
        End Sub

        ''' <summary>
        ''' Runs the encoder input through self attention and the feed forward network.
        ''' </summary>
        ''' <param name="encoderInput">The embedded encoder input.</param>
        ''' <param name="isTraining">When <c>True</c> dropout is applied where configured.</param>
        ''' <returns>The output of this encoder layer.</returns>
        Public Function Encode(encoderInput As Tensor, isTraining As Boolean) As Tensor
            Dim dropoutApplied = isTraining AndAlso dropoutRate > 0

            ' Multi headed attention
            Dim attentionFilteredData = mha.Update(encoderInput)
            Dim attentionDropped = attentionFilteredData

            If dropoutApplied Then attentionDropped = TensorOps.DropoutMask(attentionDropped, dropoutMask1, dropoutRate)

            Dim mean1 As Double() = Nothing, invStd1 As Double() = Nothing
            Dim normalized1 = TensorOps.AddNormForward(encoderInput, attentionDropped, mean1, invStd1)

            ' Feed forward neural network
            Dim feedForwardOutput = ff.FeedForward(normalized1)
            Dim feedForwardDropped = feedForwardOutput

            If dropoutApplied Then feedForwardDropped = TensorOps.DropoutMask(feedForwardDropped, dropoutMask2, dropoutRate)

            Dim mean2 As Double() = Nothing, invStd2 As Double() = Nothing
            Dim output = TensorOps.AddNormForward(normalized1, feedForwardDropped, mean2, invStd2)

            _lastCache = New Cache With {
                .Input = encoderInput,
                .AttentionOutput = attentionFilteredData,
                .AttentionDropped = attentionDropped,
                .Normalized1 = normalized1,
                .Norm1Mean = mean1,
                .Norm1InvStd = invStd1,
                .FeedForwardOutput = feedForwardOutput,
                .FeedForwardDropped = feedForwardDropped,
                .Norm2Mean = mean2,
                .Norm2InvStd = invStd2,
                .DropoutApplied = dropoutApplied,
                .MhaCache = mha.LastCache,
                .FfCache = ff.LastCache
            }

            Return output
        End Function

        ''' <summary>
        ''' Backpropagates through both sub layers.
        ''' </summary>
        ''' <param name="dOut">Gradient with respect to the output of this layer.</param>
        ''' <param name="forwardCache">The forward cache snapshot that belongs to this pass.</param>
        ''' <returns>The gradient with respect to the input of this layer.</returns>
        Public Function Backward(dOut As Tensor, forwardCache As Cache) As Tensor
            Dim cache = forwardCache

            If cache Is Nothing Then Throw New InvalidOperationException("必须先执行前向传播才能反向传播")

            ' 第二个 AddNorm：output = AddNorm(normalized1, feedForwardDropped)
            Dim dx2 = TensorOps.AddNormBackward(dOut, cache.Normalized1, cache.FeedForwardDropped, cache.Norm2Mean, cache.Norm2InvStd)
            Dim dFeedForwardDropped = TensorOps.CloneTensor(dx2)
            Dim dNormalized1 = dx2

            Dim dFeedForward = dFeedForwardDropped

            If cache.DropoutApplied Then dFeedForward = TensorOps.DropoutMaskBackward(dFeedForward, dropoutMask2, dropoutRate)

            Call TensorOps.Accumulate(dNormalized1, ff.Backward(cache.FfCache, dFeedForward))

            ' 第一个 AddNorm：normalized1 = AddNorm(encoderInput, attentionDropped)
            Dim dx1 = TensorOps.AddNormBackward(dNormalized1, cache.Input, cache.AttentionDropped, cache.Norm1Mean, cache.Norm1InvStd)
            Dim dAttentionDropped = TensorOps.CloneTensor(dx1)
            Dim dInput = dx1

            Dim dAttention = dAttentionDropped

            If cache.DropoutApplied Then dAttention = TensorOps.DropoutMaskBackward(dAttention, dropoutMask1, dropoutRate)

            Dim unused As Tensor = Nothing

            Call TensorOps.Accumulate(dInput, mha.Backward(cache.MhaCache, dAttention, unused))

            Return dInput
        End Function

        Public Sub SetDropoutNodes(dropoutRate As Double)
            If dropoutRate < 0 OrElse dropoutRate >= 1 Then Throw New ArgumentException("Error: dropout rate must be >= 0 and < 1")

            Me.dropoutRate = dropoutRate

            For i = 0 To embeddingSize - 1
                dropoutMask1(i) = False
                If randf.NextDouble < dropoutRate Then dropoutMask1(i) = True

                dropoutMask2(i) = False
                If randf.NextDouble < dropoutRate Then dropoutMask2(i) = True
            Next
        End Sub

        ''' <summary>Clears the gradient accumulators of every parameter of this layer.</summary>
        Public Sub ZeroGradients()
            mha.ZeroGradients()
            ff.ZeroGradients()
        End Sub

        ''' <summary>
        ''' Applies one optimizer step to every parameter of this layer.
        ''' </summary>
        ''' <param name="learningRate">The learning rate for this step.</param>
        ''' <param name="[step]">The current step index, used by the Adam bias correction.</param>
        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            mha.MakeTrainingStep(learningRate, [step])
            ff.MakeTrainingStep(learningRate, [step])
        End Sub

    End Class
End Namespace
