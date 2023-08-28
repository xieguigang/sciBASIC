Imports std = System.Math

Namespace CNN.trainers


    ''' <summary>
    ''' The adaptive gradient trainer will over time sum up the square of
    ''' the gradient and use it to change the weights.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class AdaGradTrainer
        Inherits Trainer

        Public Sub New(net As ConvolutionalNN, batch_size As Integer, l2_decay As Single)
            MyBase.New(net, batch_size, l2_decay)
        End Sub

        Public Overrides Sub update(i As Integer, j As Integer, gij As Double, p As Double())
            Dim gsumi = gsum(i)
            ' adagrad update
            gsumi(j) = gsumi(j) + gij * gij
            Dim dx = -learning_rate / std.Sqrt(gsumi(j) + eps) * gij
            p(j) += dx
        End Sub
    End Class
End Namespace
