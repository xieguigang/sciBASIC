Imports std = System.Math

Namespace CNN.trainers

    ''' <summary>
    ''' This is AdaGrad but with a moving window weighted average
    ''' so the gradient is not accumulated over the entire history of the run.
    ''' it's also referred to as Idea #1 in Zeiler paper on AdaDelta.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>

    Public Class WindowGradTrainer
        Inherits Trainer
        Private ReadOnly ro As Double = 0.95

        Public Sub New(net As ConvolutionalNN, batch_size As Integer, l2_decay As Single)
            MyBase.New(net, batch_size, l2_decay)
        End Sub

        Public Overrides Sub update(i As Integer, j As Integer, gij As Double, p As Double())
            Dim gsumi = gsum(i)

            ' this is adagrad but with a moving window weighted average
            ' so the gradient is not accumulated over the entire history of the run.
            ' it's also referred to as Idea #1 in Zeiler paper on Adadelta. Seems reasonable to me!
            gsumi(j) = ro * gsumi(j) + (1 - ro) * gij * gij
            Dim dx = -learning_rate / std.Sqrt(gsumi(j) + eps) * gij ' eps added for better conditioning
            p(j) += dx
        End Sub
    End Class

End Namespace
