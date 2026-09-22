#Region "Microsoft.VisualBasic::3daa464120eb796ea6f78ed7cfe1466f, Data_science\MachineLearning\DeepLearning\Transformer\MultiHeadAttention.vb"

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

    '   Total Lines: 302
    '    Code Lines: 178 (58.94%)
    ' Comment Lines: 74 (24.50%)
    '    - Xml Docs: 77.03%
    ' 
    '   Blank Lines: 50 (16.56%)
    '     File Size: 14.71 KB


    '     Class MultiHeadAttention
    ' 
    '         Properties: LastCache
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Backward, CalculateScaledMultiHeadedAttention, (+2 Overloads) Update
    ' 
    '         Sub: (+2 Overloads) ApplyLinearInputFilters, InitalizeOptimizers, InitializeLinearFilters, MakeTrainingStep, ZeroGradients
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
' MultiHeadAttention —— 多头缩放点积注意力（迁移到 TensorFlow\Tensor.vb）
'
' 迁移前依赖 AD 张量的 N 维 MatMul/Concat/Softmax/Mask 与自动微分；
' 迁移后所有张量运算都走 TensorOps 中的手写算子与手写反向传播。
' 前向阶段缓存 Qf/Kf/Vf、softmax 概率与拼接结果，反向阶段逆序回传。
' ---------------------------------------------------------------------------

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Namespace Transformer

    ''' <summary>
    ''' Scaled dot product multi head attention, implemented with the hand written operators of <see cref="TensorOps"/> and an
    ''' explicit backward pass.
    ''' </summary>
    ''' <remarks>
    ''' The forward pass caches the projected Q/K/V tensors, the per head softmax probabilities and the concatenated heads; the
    ''' backward pass walks the attention computation in reverse order.
    ''' </remarks>
    Public Class MultiHeadAttention

        Private mask As Boolean
        Private embeddingSize As Integer
        Private nr_heads As Integer       ' Number of attention heads
        Private dk As Integer             ' Key dimension
        Private dv As Integer             ' Value dimension

        ' Learned linear input layers
        Private Qm, Km, Vm As Tensor()

        ' Learned output layer
        Private Wo As Tensor

        Private QmOptimizer, KmOptimizer, VmOptimizer As Optimizer()
        Private WoOptimizer As Optimizer

        ''' <summary>
        ''' Forward intermediates of the attention computation, required by the backward pass.
        ''' </summary>
        Public Class Cache
            ''' <summary>The projected query tensors, one per head.</summary>
            Public Qf As Tensor()
            ''' <summary>The projected key tensors, one per head.</summary>
            Public Kf As Tensor()
            ''' <summary>The projected value tensors, one per head.</summary>
            Public Vf As Tensor()
            ''' <summary>The attention probabilities (softmax output) of every head.</summary>
            Public Probs As Tensor()
            ''' <summary>The heads after concatenation along the last dimension.</summary>
            Public Concat As Tensor
            ''' <summary>The query side input (identical to the K/V input for self attention).</summary>
            Public QueriesInput As Tensor
            ''' <summary>The key side input.</summary>
            Public KInput As Tensor
            ''' <summary>The value side input.</summary>
            Public VInput As Tensor
            ''' <summary>Whether this is cross attention, i.e. queries and keys/values come from different inputs.</summary>
            Public CrossAttention As Boolean
        End Class

        Private _lastCache As Cache

        ''' <summary>Gets the forward cache of the most recent <see cref="Update"/> call.</summary>
        Public ReadOnly Property LastCache As Cache
            Get
                Return _lastCache
            End Get
        End Property

        ''' <summary>
        ''' Creates a multi head attention sub layer.
        ''' </summary>
        ''' <param name="dk">Dimension of the query and key projections per head.</param>
        ''' <param name="dv">Dimension of the value projection per head.</param>
        ''' <param name="nr_heads">Number of attention heads.</param>
        ''' <param name="embeddingSize">Width of the model, used by the output projection.</param>
        ''' <param name="mask">When <c>True</c> the upper triangle of the attention scores is masked (causal attention).</param>
        Public Sub New(dk As Integer, dv As Integer, nr_heads As Integer, embeddingSize As Integer, mask As Boolean)
            Me.dk = dk
            Me.dv = dv
            Me.nr_heads = nr_heads
            Me.embeddingSize = embeddingSize
            Me.mask = mask

            Call InitializeLinearFilters()
            Call InitalizeOptimizers()
        End Sub

        ''' <summary>
        ''' Self attention: queries, keys and values all come from <paramref name="inputData"/>.
        ''' </summary>
        ''' <param name="inputData">The input sequence.</param>
        ''' <returns>The attention output.</returns>
        Public Function Update(inputData As Tensor) As Tensor
            Dim Qf As Tensor() = Nothing, Kf As Tensor() = Nothing, Vf As Tensor() = Nothing
            Call ApplyLinearInputFilters(inputData, Qf, Kf, Vf)
            Return CalculateScaledMultiHeadedAttention(Qf, Kf, Vf, inputData, inputData, inputData, False)
        End Function

        ''' <summary>
        ''' Cross attention: queries come from <paramref name="queries"/>, keys and values from <paramref name="encoderOutput"/>.
        ''' </summary>
        ''' <param name="encoderOutput">The encoder output used as keys and values.</param>
        ''' <param name="queries">The decoder representation used as queries.</param>
        ''' <returns>The attention output.</returns>
        Public Function Update(encoderOutput As Tensor, queries As Tensor) As Tensor
            Dim Qf As Tensor() = Nothing, Kf As Tensor() = Nothing, Vf As Tensor() = Nothing
            Call ApplyLinearInputFilters(encoderOutput, queries, Qf, Kf, Vf)
            Return CalculateScaledMultiHeadedAttention(Qf, Kf, Vf, queries, encoderOutput, encoderOutput, True)
        End Function

        Private Sub ApplyLinearInputFilters(inputData As Tensor, <Out> ByRef Qf As Tensor(), <Out> ByRef Kf As Tensor(), <Out> ByRef Vf As Tensor())
            Qf = New Tensor(nr_heads - 1) {}
            Kf = New Tensor(nr_heads - 1) {}
            Vf = New Tensor(nr_heads - 1) {}

            For h = 0 To nr_heads - 1
                Qf(h) = TensorOps.BatchedMatMul(inputData, Qm(h))
                Kf(h) = TensorOps.BatchedMatMul(inputData, Km(h))
                Vf(h) = TensorOps.BatchedMatMul(inputData, Vm(h))
            Next
        End Sub

        Private Sub ApplyLinearInputFilters(encoderOutput As Tensor, queries As Tensor, <Out> ByRef Qf As Tensor(), <Out> ByRef Kf As Tensor(), <Out> ByRef Vf As Tensor())
            Qf = New Tensor(nr_heads - 1) {}
            Kf = New Tensor(nr_heads - 1) {}
            Vf = New Tensor(nr_heads - 1) {}

            For h = 0 To nr_heads - 1
                Qf(h) = TensorOps.BatchedMatMul(queries, Qm(h))
                Kf(h) = TensorOps.BatchedMatMul(encoderOutput, Km(h))
                Vf(h) = TensorOps.BatchedMatMul(encoderOutput, Vm(h))
            Next
        End Sub

        Private Function CalculateScaledMultiHeadedAttention(Qf As Tensor(), Kf As Tensor(), Vf As Tensor(),
                                                             queriesInput As Tensor, kInput As Tensor, vInput As Tensor,
                                                             crossAttention As Boolean) As Tensor
            Dim AttentionHeads = New Tensor(nr_heads - 1) {}
            Dim probabilities = New Tensor(nr_heads - 1) {}
            Dim scale = 1.0 / std.Sqrt(dk)

            For h = 0 To nr_heads - 1
                Dim AttentionFilter As Tensor = TensorOps.BatchedMatMul(Qf(h), TensorOps.TransposeLastTwo(Kf(h)))
                Dim scaledAttentionFilter = TensorOps.Scale(AttentionFilter, scale)

                If mask Then Call TensorOps.MaskUpperTriangular(scaledAttentionFilter)

                probabilities(h) = TensorOps.SoftmaxLastDim(scaledAttentionFilter)
                AttentionHeads(h) = TensorOps.BatchedMatMul(probabilities(h), Vf(h))
            Next

            ' Apply linear output layer to get the correct output size
            Dim C = TensorOps.ConcatLastDim(AttentionHeads)

            _lastCache = New Cache With {
                .Qf = Qf,
                .Kf = Kf,
                .Vf = Vf,
                .Probs = probabilities,
                .Concat = C,
                .QueriesInput = queriesInput,
                .KInput = kInput,
                .VInput = vInput,
                .CrossAttention = crossAttention
            }

            Return TensorOps.BatchedMatMul(C, Wo)
        End Function

        ''' <summary>
        ''' Backpropagates through the attention computation, returning the gradient with respect to the query input and
        ''' accumulating the parameter gradients of every linear projection.
        ''' </summary>
        ''' <param name="forwardCache">
        ''' The forward cache of this pass. When the decoder runs token by token the <see cref="LastCache"/> is overwritten by
        ''' later steps, so the snapshot of the current step must be passed explicitly.
        ''' </param>
        ''' <param name="dOut">Gradient with respect to the attention output (<c>Concat · Wo</c>).</param>
        ''' <param name="dEncoderOutput">Receives the gradient with respect to the encoder output for cross attention; <c>Nothing</c> for self attention.</param>
        ''' <returns>The gradient with respect to the query input.</returns>
        Public Function Backward(forwardCache As Cache, dOut As Tensor, ByRef dEncoderOutput As Tensor) As Tensor
            Dim cache = forwardCache

            If cache Is Nothing Then Throw New InvalidOperationException("必须先执行前向传播才能反向传播")

            ' 输出投影层
            Dim dConcat As Tensor = Nothing, dWo As Tensor = Nothing
            Call TensorOps.BatchedMatMulBackward(dOut, cache.Concat, Wo, dConcat, dWo)
            Call TensorOps.Accumulate(WoOptimizer.Gradient, dWo)

            Dim dAttentionHeads = TensorOps.SplitLastDim(dConcat, nr_heads)
            Dim scale = 1.0 / std.Sqrt(dk)
            Dim dQueries As Tensor = Nothing
            Dim dEncoder As Tensor = Nothing

            For h = 0 To nr_heads - 1
                ' 注意力输出：head = probs · Vf
                Dim dProbs As Tensor = Nothing, dVf As Tensor = Nothing
                Call TensorOps.BatchedMatMulBackward(dAttentionHeads(h), cache.Probs(h), cache.Vf(h), dProbs, dVf)

                ' V 投影
                Dim dVInput As Tensor = Nothing, dVm As Tensor = Nothing
                Call TensorOps.BatchedMatMulBackward(dVf, cache.VInput, Vm(h), dVInput, dVm)
                Call TensorOps.Accumulate(VmOptimizer(h).Gradient, dVm)

                ' softmax 反向 + 缩放（掩码位置 softmax 输出为 0，其梯度自然为 0）
                Dim dScore = TensorOps.SoftmaxBackward(cache.Probs(h), dProbs)
                Call TensorOps.ScaleInPlace(dScore, scale)

                ' 注意力分数：score = Qf · Kfᵀ
                Dim dQf As Tensor = Nothing, dKfT As Tensor = Nothing
                Call TensorOps.BatchedMatMulBackward(dScore, cache.Qf(h), TensorOps.TransposeLastTwo(cache.Kf(h)), dQf, dKfT)
                Dim dKf = TensorOps.TransposeLastTwo(dKfT)

                ' Q 投影
                Dim dQInput As Tensor = Nothing, dQm As Tensor = Nothing
                Call TensorOps.BatchedMatMulBackward(dQf, cache.QueriesInput, Qm(h), dQInput, dQm)
                Call TensorOps.Accumulate(QmOptimizer(h).Gradient, dQm)

                ' K 投影
                Dim dKInput As Tensor = Nothing, dKm As Tensor = Nothing
                Call TensorOps.BatchedMatMulBackward(dKf, cache.KInput, Km(h), dKInput, dKm)
                Call TensorOps.Accumulate(KmOptimizer(h).Gradient, dKm)

                If dQueries Is Nothing Then dQueries = TensorOps.ZerosLike(dQInput)
                Call TensorOps.Accumulate(dQueries, dQInput)

                If cache.CrossAttention Then
                    If dEncoder Is Nothing Then dEncoder = TensorOps.ZerosLike(dKInput)

                    Call TensorOps.Accumulate(dEncoder, dKInput)
                    Call TensorOps.Accumulate(dEncoder, dVInput)
                Else
                    Call TensorOps.Accumulate(dQueries, dKInput)
                    Call TensorOps.Accumulate(dQueries, dVInput)
                End If
            Next

            dEncoderOutput = dEncoder

            Return dQueries
        End Function

        ''' <summary>Clears the gradient accumulators of every parameter of this layer.</summary>
        Public Sub ZeroGradients()
            For h = 0 To nr_heads - 1
                QmOptimizer(h).ZeroGrad()
                KmOptimizer(h).ZeroGrad()
                VmOptimizer(h).ZeroGrad()
            Next

            WoOptimizer.ZeroGrad()
        End Sub

        ''' <summary>
        ''' Applies one optimizer step to every projection of this layer.
        ''' </summary>
        ''' <param name="learningRate">The learning rate for this step.</param>
        ''' <param name="[step]">The current step index, used by the Adam bias correction.</param>
        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            For h = 0 To nr_heads - 1
                KmOptimizer(h).MakeTrainingStep(learningRate, [step], Km(h))
                QmOptimizer(h).MakeTrainingStep(learningRate, [step], Qm(h))
                VmOptimizer(h).MakeTrainingStep(learningRate, [step], Vm(h))
            Next
            WoOptimizer.MakeTrainingStep(learningRate, [step], Wo)
        End Sub

        Private Sub InitializeLinearFilters()
            Qm = New Tensor(nr_heads - 1) {}
            Km = New Tensor(nr_heads - 1) {}
            Vm = New Tensor(nr_heads - 1) {}

            For h = 0 To nr_heads - 1
                Qm(h) = TensorOps.HeNormalInit(New Integer() {embeddingSize, dk})
                Km(h) = TensorOps.HeNormalInit(New Integer() {embeddingSize, dk})
                Vm(h) = TensorOps.HeNormalInit(New Integer() {embeddingSize, dv})
            Next

            Wo = TensorOps.HeNormalInit(New Integer() {dv * nr_heads, embeddingSize})
        End Sub

        Private Sub InitalizeOptimizers()
            QmOptimizer = New Optimizer(nr_heads - 1) {}
            KmOptimizer = New Optimizer(nr_heads - 1) {}
            VmOptimizer = New Optimizer(nr_heads - 1) {}

            For h = 0 To nr_heads - 1
                QmOptimizer(h) = New Optimizer(Qm(h))
                KmOptimizer(h) = New Optimizer(Km(h))
                VmOptimizer(h) = New Optimizer(Vm(h))
            Next

            WoOptimizer = New Optimizer(Wo)
        End Sub

    End Class
End Namespace
