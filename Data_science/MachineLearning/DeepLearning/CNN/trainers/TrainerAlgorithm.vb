#Region "Microsoft.VisualBasic::43cb6bd6b0156e0077b80e6d23e9a2a6, Data_science\MachineLearning\DeepLearning\CNN\trainers\TrainerAlgorithm.vb"

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

    '   Total Lines: 147
    '    Code Lines: 95
    ' Comment Lines: 22
    '   Blank Lines: 30
    '     File Size: 5.62 KB


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

        ''' <summary>
        ''' alpha
        ''' </summary>
        Public Property learning_rate As Double = 0.01
        Public Property eps As Double = 0.00000001
        Public Property momentum As Double = 0.9

        Protected Friend l1_decay, l2_decay As Double

        ''' <summary>
        ''' iteration counter
        ''' </summary>
        Protected Friend k As Integer = 0
        Protected Friend gsum, xsum As IList(Of Double())

        Public ReadOnly Property batch_size As Integer

        Public ReadOnly Property conv_net As ConvolutionalNN
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return net
            End Get
        End Property

        Public ReadOnly Property get_output As Double()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return conv_net.output.OutAct.Weights
            End Get
        End Property

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

        Public Function SetKernel(cnn As ConvolutionalNN) As TrainerAlgorithm
            Me.net = cnn
            Return Me
        End Function

        Public Overridable Function train(x As DataBlock, y As Double(), checkpoints As PerformanceCounter) As TrainResult
            Dim cost_loss As Double
            Dim l2_decay_loss = 0.0
            Dim l1_decay_loss = 0.0

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
                Call checkpoints.Mark("adjust_weights")
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
            ' Will only be done once on first iteration
            If gsum.Count = 0 AndAlso momentum > 0.0 Then
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
            Next
        End Sub

        Public MustOverride Sub update(i As Integer, j As Integer, gij As Double, p As Double())

        Public Overridable Sub initTrainData(bpr As BackPropResult)
        End Sub

    End Class

End Namespace
