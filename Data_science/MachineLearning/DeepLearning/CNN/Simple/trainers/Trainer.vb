Imports Microsoft.VisualBasic.Language.Java
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
    Public MustInherit Class Trainer
        Private net As ConvolutionalNN
        Protected Friend learning_rate, l1_decay, l2_decay As Double
        Protected Friend batch_size, k As Integer
        Protected Friend momentum, eps As Double
        Protected Friend gsum, xsum As IList(Of Double())

        Public Sub New(net As ConvolutionalNN, batch_size As Integer, l2_decay As Single)
            Me.net = net

            learning_rate = 0.01
            l1_decay = 0.001
            Me.l2_decay = l2_decay
            Me.batch_size = batch_size
            momentum = 0.9
            eps = 0.00000001

            gsum = New List(Of Double())()
            xsum = New List(Of Double())()

            k = 0 ' iteration counter
        End Sub

        Public Overridable Function train(x As DataBlock, y As Integer) As TrainResult
            net.forward(x, True) ' also set the flag that lets the net know we're just training

            Dim cost_loss = net.backward(y)
            Dim l2_decay_loss = 0.0
            Dim l1_decay_loss = 0.0

            k += 1
            If k Mod batch_size = 0 Then

                Dim pglist = net.BackPropagationResult.ToArray

                ' initialize lists for accumulators. Will only be done once on first iteration
                If gsum.Count = 0 AndAlso momentum > 0.0 Then
                    For i = 0 To pglist.Length - 1
                        Dim newGsumArr = New Double(pglist(i).Weights.Length - 1) {}
                        newGsumArr.fill(0)
                        gsum.Add(newGsumArr)
                        initTrainData(pglist(i))
                    Next
                End If

                ' perform an update for all sets of weights
                For i = 0 To pglist.Length - 1
                    Dim pg = pglist(i) ' param, gradient, other options in future (custom learning rate etc)
                    Dim p = pg.Weights
                    Dim g = pg.Gradients

                    ' learning rate for some parameters.
                    Dim l2_decay_mul = pg.L2DecayMul
                    Dim l1_decay_mul = pg.L1DecayMul
                    Dim l2_decay = Me.l2_decay * l2_decay_mul
                    Dim l1_decay = Me.l1_decay * l1_decay_mul

                    Dim plen = p.Length
                    For j = 0 To plen - 1
                        l2_decay_loss += l2_decay * p(j) * p(j) / 2 ' accumulate weight decay loss
                        l1_decay_loss += l1_decay * std.Abs(p(j))
                        Dim l1grad = l1_decay * If(p(j) > 0, 1, -1)
                        Dim l2grad = l2_decay * p(j)

                        Dim gij = (l2grad + l1grad + g(j)) / batch_size ' raw batch gradient

                        update(i, j, gij, p)

                        g(j) = 0.0 ' zero out gradient so that we can begin accumulating anew
                    Next
                Next
            End If

            ' appending softmax_loss for backwards compatibility, but from now on we will always use cost_loss
            ' in future, TODO: have to completely redo the way loss is done around the network as currently
            ' loss is a bit of a hack. Ideally, user should specify arbitrary number of loss functions on any layer
            ' and it should all be computed correctly and automatically.
            Return New TrainResult(0, 0, l1_decay_loss, l2_decay_loss, cost_loss, cost_loss, cost_loss + l1_decay_loss + l2_decay_loss)
        End Function

        Public MustOverride Sub update(i As Integer, j As Integer, gij As Double, p As Double())

        Public Overridable Sub initTrainData(bpr As BackPropResult)
        End Sub
    End Class

End Namespace
