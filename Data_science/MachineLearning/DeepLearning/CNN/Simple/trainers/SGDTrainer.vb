
Namespace CNN.trainers

    ''' <summary>
    ''' Stochastic gradient descent (often shortened in SGD), also known as incremental gradient descent, is a
    ''' stochastic approximation of the gradient descent optimization method for minimizing an objective function
    ''' that is written as a sum of differentiable functions. In other words, SGD tries to find minimums or
    ''' maximums by iteration.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>

    Public Class SGDTrainer
        Inherits Trainer

        Public Sub New(net As ConvolutionalNN, batch_size As Integer, l2_decay As Single)
            MyBase.New(net, batch_size, l2_decay)
        End Sub

        Public Overrides Sub update(i As Integer, j As Integer, gij As Double, p As Double())
            Dim gsumi = gsum(i)
            ' assume SGD
            If momentum > 0.0 Then
                ' momentum update
                Dim dx = momentum * gsumi(j) - learning_rate * gij ' step
                gsumi(j) = dx ' back this up for next iteration of momentum
                p(j) += dx ' apply corrected gradient
            Else
                ' vanilla sgd
                p(j) += -learning_rate * gij
            End If
        End Sub
    End Class

End Namespace
