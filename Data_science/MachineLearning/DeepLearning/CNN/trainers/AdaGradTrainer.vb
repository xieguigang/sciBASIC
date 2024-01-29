Imports std = System.Math

Namespace CNN.trainers

    ''' <summary>
    ''' The adaptive gradient trainer will over time sum up the square of
    ''' the gradient and use it to change the weights.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class AdaGradTrainer : Inherits TrainerAlgorithm

        Public Sub New(batch_size As Integer, l2_decay As Single)
            MyBase.New(batch_size, l2_decay)
        End Sub

        Public Overrides Sub update(i As Integer, j As Integer, gij As Double, p As Double())
            Dim gsumi As Double() = gsum(i)
            Dim dx As Double
            ' adagrad update
            gsumi(j) = gsumi(j) + gij * gij
            dx = -learning_rate / std.Sqrt(gsumi(j) + eps) * gij
            p(j) += dx
        End Sub

        Public Overrides Function ToString() As String
            Return $"ada_grad(batch_size:{batch_size}, l2_decay:{l2_decay})"
        End Function
    End Class
End Namespace
