#Region "Microsoft.VisualBasic::924f6211f2482ec41e7de78bc8af3b69, Data_science\MachineLearning\DeepLearning\Transformer\FeedForwardNetwork.vb"

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
    '    Code Lines: 66 (46.15%)
    ' Comment Lines: 52 (36.36%)
    '    - Xml Docs: 73.08%
    ' 
    '   Blank Lines: 25 (17.48%)
    '     File Size: 6.57 KB


    '     Class FeedForwardNetwork
    ' 
    '         Properties: LastCache
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Backward, FeedForward
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
' FeedForwardNetwork —— 位置前馈网络（两层全连接 + ReLU）
'
' 迁移到 TensorFlow\Tensor.vb 后不再有自动微分，因此前向阶段需要缓存
' 预激活值（ReLU 反向所需的掩码来源）与激活值（计算 W2 梯度所需），
' 反向阶段手工累加 W1/W2/b1/b2 的梯度。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Transformer

    ''' <summary>
    ''' Position wise feed forward network: two fully connected layers with a ReLU activation in between.
    ''' </summary>
    ''' <remarks>
    ''' Because the tensor runtime has no automatic differentiation, the forward pass caches the pre-activation (the source of
    ''' the ReLU mask) and the activation (needed for the W2 gradient); the backward pass accumulates the W1, W2, b1 and b2
    ''' gradients by hand.
    ''' </remarks>
    Public Class FeedForwardNetwork

        Private W1, W2 As Tensor
        Private b1, b2 As Tensor

        Private W1Optimizer, W2Optimizer, b1Optimizer, b2Optimizer As Optimizer

        ''' <summary>前向传播的中间量缓存，供反向传播使用。</summary>
        Public Class Cache
            ''' <summary>本层输入 G</summary>
            Public Input As Tensor
            ''' <summary>第一层的预激活值（ReLU 之前）</summary>
            Public PreActivation As Tensor
            ''' <summary>第一层的激活值（ReLU 之后）</summary>
            Public Activation As Tensor
        End Class

        Private _lastCache As Cache

        ''' <summary>Gets the forward cache of the most recent <see cref="FeedForward"/> call.</summary>
        Public ReadOnly Property LastCache As Cache
            Get
                Return _lastCache
            End Get
        End Property

        ''' <summary>
        ''' Creates the feed forward network and initializes its weights with He normal initialization.
        ''' </summary>
        ''' <param name="dff">Hidden width of the inner layer.</param>
        ''' <param name="embeddingSize">Width of the model input and output.</param>
        Public Sub New(dff As Integer, embeddingSize As Integer)
            W1 = TensorOps.HeNormalInit(New Integer() {embeddingSize, dff})
            W2 = TensorOps.HeNormalInit(New Integer() {dff, embeddingSize})
            b1 = New Tensor(dff)
            b2 = New Tensor(embeddingSize)

            W1Optimizer = New Optimizer(W1)
            W2Optimizer = New Optimizer(W2)
            b1Optimizer = New Optimizer(b1)
            b2Optimizer = New Optimizer(b2)
        End Sub

        ''' <summary>
        ''' Runs the two layer transformation and caches its intermediates.
        ''' </summary>
        ''' <param name="G">The input tensor.</param>
        ''' <returns>The output of the second layer.</returns>
        Public Function FeedForward(G As Tensor) As Tensor
            ' First layer
            Dim preActivation = TensorOps.VecAdd(TensorOps.BatchedMatMul(G, W1), b1)
            Dim activation = Tensor.computeKernel.Relu(preActivation)

            ' Second layer
            Dim FFN2 = TensorOps.VecAdd(TensorOps.BatchedMatMul(activation, W2), b2)

            _lastCache = New Cache With {
                .Input = G,
                .PreActivation = preActivation,
                .Activation = activation
            }

            Return FFN2
        End Function

        ''' <summary>
        ''' Backpropagates through the network, accumulating the W1, W2, b1 and b2 gradients.
        ''' </summary>
        ''' <param name="forwardCache">
        ''' The forward cache of this pass. When the decoder runs token by token the <see cref="LastCache"/> of this layer is
        ''' overwritten by later steps, so the snapshot of the current step must be passed explicitly.
        ''' </param>
        ''' <param name="dOut">Gradient with respect to the output of <see cref="FeedForward"/>.</param>
        ''' <returns>The gradient with respect to the input.</returns>
        Public Function Backward(forwardCache As Cache, dOut As Tensor) As Tensor
            Dim cache = forwardCache

            If cache Is Nothing Then Throw New InvalidOperationException("必须先执行前向传播才能反向传播")

            ' 偏置 b2
            Call TensorOps.Accumulate(b2Optimizer.Gradient, TensorOps.VecAddBackward(dOut))

            ' 第二层权重：FFN2 = activation · W2
            Dim dActivation As Tensor = Nothing, dW2 As Tensor = Nothing
            Call TensorOps.BatchedMatMulBackward(dOut, cache.Activation, W2, dActivation, dW2)
            Call TensorOps.Accumulate(W2Optimizer.Gradient, dW2)

            ' ReLU 反向：用预激活值的阶跃掩码门控上游梯度
            Dim dPreActivation = TensorOps.ElementwiseMultiply(dActivation, TensorOps.Heaviside(cache.PreActivation))

            ' 偏置 b1
            Call TensorOps.Accumulate(b1Optimizer.Gradient, TensorOps.VecAddBackward(dPreActivation))

            ' 第一层权重：preActivation = G · W1
            Dim dInput As Tensor = Nothing, dW1 As Tensor = Nothing
            Call TensorOps.BatchedMatMulBackward(dPreActivation, cache.Input, W1, dInput, dW1)
            Call TensorOps.Accumulate(W1Optimizer.Gradient, dW1)

            Return dInput
        End Function

        ''' <summary>Clears the gradient accumulators of every parameter of this layer.</summary>
        Public Sub ZeroGradients()
            W1Optimizer.ZeroGrad()
            W2Optimizer.ZeroGrad()
            b1Optimizer.ZeroGrad()
            b2Optimizer.ZeroGrad()
        End Sub

        ''' <summary>
        ''' Applies one optimizer step to every parameter of this layer.
        ''' </summary>
        ''' <param name="learningRate">The learning rate for this step.</param>
        ''' <param name="[step]">The current step index, used by the Adam bias correction.</param>
        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            W1Optimizer.MakeTrainingStep(learningRate, [step], W1)
            W2Optimizer.MakeTrainingStep(learningRate, [step], W2)
            b1Optimizer.MakeTrainingStep(learningRate, [step], b1)
            b2Optimizer.MakeTrainingStep(learningRate, [step], b2)
        End Sub

    End Class
End Namespace

