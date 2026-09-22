#Region "Microsoft.VisualBasic::f64365fdc7ccc74f44ea67285cd76482, Data_science\MachineLearning\DeepLearning\Transformer\OutputLayer.vb"

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

    '   Total Lines: 112
    '    Code Lines: 47 (41.96%)
    ' Comment Lines: 45 (40.18%)
    '    - Xml Docs: 84.44%
    ' 
    '   Blank Lines: 20 (17.86%)
    '     File Size: 5.27 KB


    '     Class OutputLayer
    ' 
    '         Properties: LastCache
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Backward, Output
    ' 
    '         Sub: MakeTrainingStep, ZeroGradients
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
' OutputLayer —— 输出层：把解码器输出投影到词表维度并做 softmax
'
' 迁移要点：前向需要缓存「压平后的输入」（用于计算 Wo 的梯度）与输入原始形状
' （反向时把梯度还原回 [batch, seq, emb]）。softmax 的反向被交叉熵梯度吸收
' （d(logits) = softmax − onehot），因此 Backward 直接接受对 logits 的梯度。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Transformer

    ''' <summary>
    ''' Produce a flat array with the same dimension as the number of words in the dictionary
    ''' </summary>
    Public Class OutputLayer

        ''' <summary>The output projection weight matrix, shaped <c>[embeddingSize * sequenceLength, dictionarySize]</c>.</summary>
        Public Wo As Tensor

        Private WoOptimizer As Optimizer

        ''' <summary>
        ''' Forward intermediates of the output projection, required by the backward pass.
        ''' </summary>
        Public Class Cache
            ''' <summary>The flattened decoder output that was projected.</summary>
            Public FlatInput As Tensor
            ''' <summary>The original shape of the decoder output, used to unflatten the gradient.</summary>
            Public InputShape As Integer()
            ''' <summary>The logits produced before the softmax.</summary>
            Public Logits As Tensor
        End Class

        Private _lastCache As Cache

        ''' <summary>Gets the forward cache of the most recent <see cref="Output"/> call.</summary>
        Public ReadOnly Property LastCache As Cache
            Get
                Return _lastCache
            End Get
        End Property

        ''' <summary>
        ''' Creates the output layer.
        ''' </summary>
        ''' <param name="sequenceLength">Sequence length of the decoder output.</param>
        ''' <param name="embeddingSize">Width of the decoder output.</param>
        ''' <param name="dictionarySize">Size of the target vocabulary.</param>
        Public Sub New(sequenceLength As Integer, embeddingSize As Integer, dictionarySize As Integer)
            Wo = TensorOps.HeNormalInit(New Integer() {embeddingSize * sequenceLength, dictionarySize})

            WoOptimizer = New Optimizer(Wo)
        End Sub

        ''' <summary>
        ''' Projects the decoder output onto the vocabulary and applies a softmax.
        ''' </summary>
        ''' <param name="input">The decoder output.</param>
        ''' <returns>The softmax probabilities shaped <c>[batch, 1, dictionarySize]</c>.</returns>
        Public Function Output(input As Tensor) As Tensor
            Dim flatInput = TensorOps.FlattenLastTwo(input)
            Dim filteredOutput = TensorOps.BatchedMatMul(flatInput, Wo)
            Dim softmaxOutput = TensorOps.SoftmaxLastDim(filteredOutput)

            _lastCache = New Cache With {
                .FlatInput = flatInput,
                .InputShape = CType(input.Shape.Clone(), Integer()),
                .Logits = filteredOutput
            }

            Return softmaxOutput
        End Function

        ''' <summary>
        ''' Backpropagates through the output projection; it accepts the gradient with respect to the logits (before the
        ''' softmax) and returns the gradient with respect to the decoder output.
        ''' </summary>
        ''' <param name="forwardCache">
        ''' The forward cache of this step. When the decoder runs token by token the <see cref="LastCache"/> is overwritten by
        ''' later steps, so the snapshot of the current step must be passed explicitly.
        ''' </param>
        ''' <param name="dLogits">Gradient with respect to the logits.</param>
        ''' <returns>The gradient with respect to the decoder output.</returns>
        Public Function Backward(forwardCache As Cache, dLogits As Tensor) As Tensor
            Dim cache = forwardCache

            If cache Is Nothing Then Throw New InvalidOperationException("必须先执行前向传播才能反向传播")

            Dim dFlat As Tensor = Nothing, dWo As Tensor = Nothing
            Call TensorOps.BatchedMatMulBackward(dLogits, cache.FlatInput, Wo, dFlat, dWo)
            Call TensorOps.Accumulate(WoOptimizer.Gradient, dWo)

            Return TensorOps.UnflattenLastTwo(dFlat, cache.InputShape)
        End Function

        ''' <summary>Clears the gradient accumulator of the output projection.</summary>
        Public Sub ZeroGradients()
            WoOptimizer.ZeroGrad()
        End Sub

        ''' <summary>
        ''' Applies one optimizer step to the output projection.
        ''' </summary>
        ''' <param name="learningRate">The learning rate for this step.</param>
        ''' <param name="[step]">The current step index, used by the Adam bias correction.</param>
        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            WoOptimizer.MakeTrainingStep(learningRate, [step], Wo)
        End Sub

    End Class
End Namespace

