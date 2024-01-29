
Namespace CNN.trainers

    ''' <summary>
    ''' Another extension of gradient descent is due to Yurii Nesterov from 1983,[7] and has been subsequently generalized
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>

    Public Class NesterovTrainer : Inherits TrainerAlgorithm

        Public Sub New(batch_size As Integer, l2_decay As Single)
            MyBase.New(batch_size, l2_decay)
        End Sub

        Public Overrides Sub update(i As Integer, j As Integer, gij As Double, p As Double())
            Dim gsumi = gsum(i)
            Dim dx = gsumi(j)
            gsumi(j) = gsumi(j) * momentum + learning_rate * gij
            dx = momentum * dx - (1.0 + momentum) * gsumi(j)
            p(j) += dx
        End Sub
    End Class

End Namespace
