#Region "Microsoft.VisualBasic::d385009c646d64f02171b058c231143f, Data_science\MachineLearning\DeepLearning\CNN\trainers\TrainerAlgorithm.vb"

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

    '   Total Lines: 196
    '    Code Lines: 100 (51.02%)
    ' Comment Lines: 64 (32.65%)
    '    - Xml Docs: 70.31%
    ' 
    '   Blank Lines: 32 (16.33%)
    '     File Size: 9.34 KB


    '     Class TrainerAlgorithm
    ' 
    '         Properties: batch_size, conv_net, eps, get_output, learning_rate
    '                     momentum
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: SetKernel, train
    ' 
    '         Sub: adjustWeights, initTrainData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.SVM
Imports std = System.Math

Namespace CNN.trainers

    ''' <summary>
    ''' Trainers take the generated output of activations and gradients in
    ''' order to modify the weights in the network to make a better prediction
    ''' the next time the network runs with a data block.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public MustInherit Class TrainerAlgorithm

        Dim net As ConvolutionalNN

        ''' <summary>Learning rate (alpha) applied to the weight update.</summary>
        Public Property learning_rate As Double = 0.01
        ''' <summary>Small constant added for numerical conditioning, avoiding division by zero.</summary>
        Public Property eps As Double = 0.00000001
        ''' <summary>Momentum factor used by the momentum based update rules.</summary>
        Public Property momentum As Double = 0.9

        ''' <summary>L1 and L2 regularization strengths applied during the weight update.</summary>
        Protected Friend l1_decay, l2_decay As Double

        ''' <summary>Iteration counter, incremented on every training step.</summary>
        Protected Friend k As Integer = 0
        ''' <summary>Per parameter accumulators used by the update rules (first and second moment estimates).</summary>
        Protected Friend gsum, xsum As IList(Of Double())

        ''' <summary>Gets the mini batch size; the weights are updated every <see cref="batch_size"/> samples.</summary>
        Public ReadOnly Property batch_size As Integer

        ''' <summary>Gets the convolutional network this trainer updates.</summary>
        Public ReadOnly Property conv_net As ConvolutionalNN
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return net
            End Get
        End Property

        ''' <summary>Gets the output activations produced by the most recent forward pass.</summary>
        Public ReadOnly Property get_output As Double()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return conv_net.output.OutAct.Weights
            End Get
        End Property

        ''' <summary>
        ''' Creates a trainer algorithm with default hyper parameters.
        ''' </summary>
        ''' <param name="batch_size">Number of samples accumulated before the weights are updated.</param>
        ''' <param name="l2_decay">L2 regularization strength; the L1 decay is fixed at 0.001.</param>
        Public Sub New(batch_size As Integer, l2_decay As Single)
            Me.learning_rate = 0.01
            Me.l1_decay = 0.001
            Me.l2_decay = l2_decay
            Me.batch_size = batch_size
            Me.momentum = 0.9
            Me.eps = 0.00000001

            gsum = New List(Of Double())()
            xsum = New List(Of Double())()
        End Sub

        ''' <summary>
        ''' Attaches the network to be trained.
        ''' </summary>
        ''' <param name="cnn">The network whose weights are updated by this trainer.</param>
        ''' <returns>This trainer, so the call can be chained.</returns>
        Public Function SetKernel(cnn As ConvolutionalNN) As TrainerAlgorithm
            Me.net = cnn
            Return Me
        End Function

        ''' <summary>
        ''' Runs one training step: a forward pass, a backward pass and, every <see cref="batch_size"/> samples, a weight
        ''' update.
        ''' </summary>
        ''' <param name="x">The input data block.</param>
        ''' <param name="y">The target output; a single element is treated as a class index, several elements as a
        ''' regression target.</param>
        ''' <param name="checkpoints">Optional performance counter used to time the individual steps.</param>
        ''' <returns>The loss and timing information of this training step.</returns>
        Public Overridable Function train(x As DataBlock, y As Double(), checkpoints As PerformanceCounter) As TrainResult
            Dim cost_loss As Double
            Dim l2_decay_loss = 0.0
            Dim l1_decay_loss = 0.0
            Dim flag As Boolean = Not checkpoints Is Nothing

            ' also set the flag that lets the net know we're just training
            Call net.forward(x, checkpoints)

            If y.Length = 1 Then
                cost_loss = net.backward(CInt(y(0)), checkpoints)
            Else
                cost_loss = net.backward(y, checkpoints).Sum
            End If

            k += 1

            If k Mod batch_size = 0 Then
                Call adjustWeights(l2_decay_loss, l1_decay_loss)

                If flag Then
                    Call checkpoints.Mark("adjust_weights")
                End If
            End If

            ' appending softmax_loss for backwards compatibility, but from now on we will always use cost_loss
            ' in future, TODO: have to completely redo the way loss is done around the network as currently
            ' loss is a bit of a hack. Ideally, user should specify arbitrary number of loss functions on any layer
            ' and it should all be computed correctly and automatically.
            Return New TrainResult(
                0, 0, l1_decay_loss, l2_decay_loss, cost_loss, cost_loss,
                loss:=cost_loss + l1_decay_loss + l2_decay_loss
            )
        End Function

        Private Sub adjustWeights(ByRef l2_decay_loss As Double, ByRef l1_decay_loss As Double)
            Dim pglist As BackPropResult() = net.BackPropagationResult.ToArray

            ' initialize lists for accumulators.
            ' Will only be done once on first iteration.
            ' Note: gsum/xsum must be initialized even when momentum = 0,
            ' because SGDTrainer.update accesses gsum(i) unconditionally.
            If gsum.Count = 0 Then
                For i As Integer = 0 To pglist.Length - 1
                    Dim newGsumArr = New Double(pglist(i).Weights.Length - 1) {}
                    gsum.Add(newGsumArr)
                    initTrainData(pglist(i))
                Next
            End If

            ' perform an update for all sets of weights
            For i As Integer = 0 To pglist.Length - 1
                Dim pg = pglist(i) ' param, gradient, other options in future (custom learning rate etc)
                Dim p = pg.Weights
                Dim g = pg.Gradients

                ' learning rate for some parameters.
                Dim l2_decay_mul = pg.L2DecayMul
                Dim l1_decay_mul = pg.L1DecayMul
                Dim l2_decay = Me.l2_decay * l2_decay_mul
                Dim l1_decay = Me.l1_decay * l1_decay_mul
                Dim plen = p.Length

                For j As Integer = 0 To plen - 1
                    l2_decay_loss += l2_decay * p(j) * p(j) / 2 ' accumulate weight decay loss
                    l1_decay_loss += l1_decay * std.Abs(p(j))

                    Dim l1grad = l1_decay * If(p(j) > 0, 1, -1)
                    Dim l2grad = l2_decay * p(j)
                    Dim gij = (l2grad + l1grad + g(j)) / batch_size ' raw batch gradient

                    Call update(i, j, gij, p)

                    g(j) = 0.0 ' zero out gradient so that we can begin accumulating anew
                Next

                ' 上面的 update 是"绕过 Tensor 直接就地改写权重/梯度数组"的（p(j) / g(j)），
                ' 张量自身无从感知这类写入。GPU 后端会把主机数组缓存到显存并靠张量版本号判断失效，
                ' 因此这里必须显式通知该参数块的宿主（DataBlock）使其缓存副本失效，
                ' 否则下一轮前向会用显存里的旧权重计算，得到静默错误的结果。
                '
                ' 相比全局的 Tensor.InvalidateAllDeviceCaches()，按参数块通知的粒度更细，
                ' 不会把中间激活等其它张量的显存缓存一并冲掉。
                Call pg.NotifyModified()
            Next
        End Sub

        ''' <summary>
        ''' Applies the concrete update rule to one parameter of one parameter block.
        ''' </summary>
        ''' <param name="i">Index of the parameter block inside the network.</param>
        ''' <param name="j">Index of the parameter inside the block.</param>
        ''' <param name="gij">The raw batch gradient of that parameter.</param>
        ''' <param name="p">The parameter vector that is updated in place.</param>
        Public MustOverride Sub update(i As Integer, j As Integer, gij As Double, p As Double())

        ''' <summary>
        ''' Allows an update rule to allocate its additional per parameter accumulator for a parameter block.
        ''' </summary>
        ''' <param name="bpr">The parameter block that is about to be trained for the first time.</param>
        Public Overridable Sub initTrainData(bpr As BackPropResult)
        End Sub

    End Class

End Namespace
